﻿/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */

using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.UniqueID;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOL.Database
{
    /// <summary>
    /// Abstract Base Class for SQL based Database Connector
    /// </summary>
    public abstract class SQLObjectDatabase : ObjectDatabase
    {
        private static readonly object Lock = new object();

        /// <summary>
        /// Create a new instance of <see cref="SQLObjectDatabase"/>
        /// </summary>
        /// <param name="ConnectionString">Database Connection String</param>
        protected SQLObjectDatabase(string ConnectionString)
            : base(ConnectionString)
        {

        }

        #region ObjectDatabase Base Implementation for SQL
        /// <summary>
        /// Register Data Object Type if not already Registered
        /// </summary>
        /// <param name="dataObjectType">DataObject Type</param>
        public override void RegisterDataObject(Type dataObjectType)
        {
            var tableName = AttributesUtils.GetTableOrViewName(dataObjectType);
            var isView = AttributesUtils.GetViewName(dataObjectType) != null;
            var viewAs = AttributesUtils.GetViewAs(dataObjectType);

            if (TableDatasets.TryGetValue(tableName, out DataTableHandler existingHandler))
            {
                if (dataObjectType != existingHandler.ObjectType)
                    throw new DatabaseException($"Table Handler Duplicate for Type: {dataObjectType}, Table Name '{tableName}' Already Registered with Type : {existingHandler.ObjectType}");

                return;
            }

            var dataTableHandler = new DataTableHandler(dataObjectType);

            try
            {
                if (isView)
                {
                    if (!string.IsNullOrEmpty(viewAs))
                    {
                        ExecuteNonQueryImpl($"DROP VIEW IF EXISTS `{tableName}`");
                        ExecuteNonQueryImpl($"CREATE VIEW `{tableName}` AS {string.Format(viewAs, $"`{AttributesUtils.GetTableName(dataObjectType)}`")}");
                    }
                }
                else
                {
                    CheckOrCreateTableImpl(dataTableHandler);
                }

                lock (Lock)
                {
                    TableDatasets.Add(tableName, dataTableHandler);
                }

                // Init PreCache
                if (dataTableHandler.UsesPreCaching)
                {
                    var primary = dataTableHandler.PrimaryKeys.Single();
                    var objects = SelectObjectsImpl(dataTableHandler, string.Empty, new[] { new QueryParameter[] { } }, Transaction.IsolationLevel.DEFAULT).First();

                    // Parallelisierung der Herzspeicherung
                    Parallel.ForEach(objects, obj =>
                    {
                        dataTableHandler.SetPreCachedObject(primary.GetValue(obj), obj);
                    });
                }
            }
            catch (Exception e)
            {
                // Optional: eine zentrale Fehlerbehandlung implementieren oder debuggen/loggen in separaten Methoden auslagern, um den Overhead zu reduzieren
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat($"RegisterDataObject: Error While Registering Table \"{tableName}\"\n{e}");
            }
        }


        /// <summary>
        /// Escape wrong characters from string for Database Insertion
        /// </summary>
        /// <param name="rawInput">String to Escape</param>
        /// <returns>Escaped String</returns>
        public override string Escape(string rawInput)
        {
            rawInput = rawInput.Replace("\\", "\\\\");
            rawInput = rawInput.Replace("\"", "\\\"");
            rawInput = rawInput.Replace("'", "\\'");
            return rawInput.Replace("’", "\\’");
        }
        #endregion

        #region ObjectDatabase Objects Implementations
        /// <summary>
        /// Adds new DataObjects to the database.
        /// </summary>
        /// <param name="dataObjects">DataObjects to add to the database</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were added successfully; false otherwise</returns>
        protected override IEnumerable<bool> AddObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
        {
            var success = new List<bool>();
            var objs = dataObjects.ToList();  // Punkt 1: Verwenden von List<T>

            if (!objs.Any())
                return success;

            try
            {
                // Check Primary Keys
                var fieldBindings = tableHandler.FieldElementBindings.ToList();
                var usePrimaryAutoInc = fieldBindings.Any(bind => bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement);

                // Columns
                var columns = fieldBindings.Where(bind => bind.PrimaryKey == null || !bind.PrimaryKey.AutoIncrement)
                    .Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToArray();

                // Prepare SQL Query using StringBuilder for better performance
                var commandBuilder = new StringBuilder();
                commandBuilder.AppendFormat("INSERT INTO `{0}` ({1}) VALUES({2})",
                    tableHandler.TableName,
                    string.Join(", ", columns.Select(col => col.ColumnName)),
                    string.Join(", ", columns.Select(col => col.ParamName)));

                var command = commandBuilder.ToString();

                // Init Object Id GUID
                foreach (var obj in objs.Where(obj => obj.ObjectId == null))
                    obj.ObjectId = IDGenerator.GenerateID();

                // Build Parameters
                var parameters = objs.Select(obj => columns.Select(col => new QueryParameter(col.ParamName, col.Binding.GetValue(obj), col.Binding.ValueType))).ToList();

                // Primary Key Auto Inc Handler
                if (usePrimaryAutoInc)
                {
                    var lastId = ExecuteScalarImpl(command, parameters, true);
                    var binding = fieldBindings.First(bind => bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement);
                    var resultByObjects = lastId.Select((result, index) => new { Result = Convert.ToInt64(result), DataObject = objs[index] }).ToList();

                    foreach (var result in resultByObjects)
                    {
                        if (result.Result > 0)
                        {
                            DatabaseSetValue(result.DataObject, binding, result.Result);
                            result.DataObject.ObjectId = result.Result.ToString();
                            result.DataObject.Dirty = false;
                            result.DataObject.IsPersisted = true;
                            result.DataObject.IsDeleted = false;
                            success.Add(true);
                        }
                        else
                        {
                            if (Log.IsErrorEnabled)
                            {
                                Log.ErrorFormat("Error adding data object into {0} Object = {1}, UsePrimaryAutoInc, Query = {2}",
                                                tableHandler.TableName, result.DataObject, command);
                            }
                            success.Add(false);
                        }
                    }
                }
                else
                {
                    var affected = ExecuteNonQueryImpl(command, parameters);
                    var resultByObjects = affected.Select((result, index) => new { Result = result, DataObject = objs[index] }).ToList();

                    foreach (var result in resultByObjects)
                    {
                        if (result.Result > 0)
                        {
                            result.DataObject.Dirty = false;
                            result.DataObject.IsPersisted = true;
                            result.DataObject.IsDeleted = false;
                            success.Add(true);
                        }
                        else
                        {
                            if (Log.IsErrorEnabled)
                            {
                                Log.ErrorFormat("Error adding data object into {0} Object = {1} Query = {2}",
                                                tableHandler.TableName, result.DataObject, command);
                            }
                            success.Add(false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("Error while adding data objects in table: {0}\n{1}", tableHandler.TableName, e);
            }

            return success;
        }



        /// <summary>
        /// Saves Persisted DataObjects into Database
        /// </summary>
        /// <param name="dataObjects">DataObjects to Save</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were saved successfully; false otherwise</returns>
        protected override IEnumerable<bool> SaveObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
        {
            var success = new List<bool>();
            if (!dataObjects.Any())
                return success;
           
            try
            {
                // Caching der FieldElementBindings
                var bindings = tableHandler.FieldElementBindings.ToList();

                // Columns Filtering out ReadOnly
                var columns = bindings.Where(bind => bind.PrimaryKey == null && bind.ReadOnly == null)
                    .Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToList();

                // Primary Key
                var primary = bindings.Where(bind => bind.PrimaryKey != null)
                    .Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToList();

                if (!primary.Any())
                    throw new DatabaseException(string.Format("Table {0} has no primary key for saving...", tableHandler.TableName));

                var command = new StringBuilder();
                command.AppendFormat("UPDATE `{0}` SET ", tableHandler.TableName);
                command.Append(string.Join(", ", columns.Select(col => string.Format("{0} = {1}", col.ColumnName, col.ParamName))));
                command.Append(" WHERE ");
                command.Append(string.Join(" AND ", primary.Select(col => string.Format("{0} = {1}", col.ColumnName, col.ParamName))));

                var parameters = dataObjects.Select(obj => columns.Concat(primary)
                    .Select(col => new QueryParameter(col.ParamName, col.Binding.GetValue(obj), col.Binding.ValueType))).ToArray();

                var affected = ExecuteNonQueryImpl(command.ToString(), parameters);
                var resultByObjects = affected.Select((result, index) => new { Result = result, DataObject = dataObjects.ElementAt(index) });

                foreach (var result in resultByObjects)
                {
                    if (result.Result > 0)
                    {
                        result.DataObject.Dirty = false;
                        result.DataObject.IsPersisted = true;
                        success.Add(true);
                    }
                    else
                    {
                        if (Log.IsErrorEnabled)
                        {
                            if (result.Result < 0)
                                Log.ErrorFormat("Error saving data object in table {0} Object = {1} --- constraint failed? {2}", tableHandler.TableName, result.DataObject, command);
                            else
                                Log.ErrorFormat("Error saving data object in table {0} Object = {1} --- keyvalue changed? {2}\n{3}", tableHandler.TableName, result.DataObject, command, Environment.StackTrace);
                        }
                        success.Add(false);
                    }
                }
            }

            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("Error while saving data object in table: {0}\n{1}", tableHandler.TableName, e);
            }

            return success;
        }

        /// <summary>
        /// Deletes DataObjects from the database.
        /// </summary>
        /// <param name="dataObjects">DataObjects to delete from the database</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were deleted successfully; false otherwise</returns>
        protected override IEnumerable<bool> DeleteObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
        {
            var success = new List<bool>();
            if (!dataObjects.Any())
                return success;

            try
            {
                var primary = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey != null).ToList();
                if (!primary.Any())
                    throw new DatabaseException(string.Format("Table {0} has no primary key for deletion...", tableHandler.TableName));

                var commandTemplate = string.Format("DELETE FROM `{0}` WHERE {1}", tableHandler.TableName,
                                string.Join(" AND ", primary.Select(col => string.Format("`{0}` = @{0}", col.ColumnName))));

                // Verwende eine Liste für Parameter
                var affectedResults = new List<int>();
                foreach (var obj in dataObjects)
                {
                    var parameters = primary.Select(pk => new QueryParameter(string.Format("@{0}", pk.ColumnName), pk.GetValue(obj), pk.ValueType));

                    // Ausführung des Löschbefehls
                    var affected = ExecuteNonQueryImpl(commandTemplate, parameters);
                    affectedResults.Add(affected);
                }

                for (int index = 0; index < affectedResults.Count; index++)
                {
                    if (affectedResults[index] > 0)
                    {
                        var obj = dataObjects.ElementAt(index);
                        obj.IsPersisted = false;
                        obj.IsDeleted = true;
                        success.Add(true);
                    }
                    else
                    {
                        if (Log.IsErrorEnabled)
                            Log.ErrorFormat("Error deleting data object from table {0} Object = {1} --- keyvalue changed? {2}\n{3}", tableHandler.TableName, dataObjects.ElementAt(index), commandTemplate, Environment.StackTrace);
                        success.Add(false);
                    }
                }
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("Error while deleting data object in table: {0}\n{1}", tableHandler.TableName, e);
            }

            return success;
        }

        #endregion

        #region ObjectDatabase Select Implementation
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on their primary key values
        /// </summary>
        /// <param name="tableHandler">Table Handler for the DataObjects to Retrieve</param>
        /// <param name="keys">Collection of Primary Key Values</param>
        /// <returns>Collection of DataObject with primary key matching values</returns>
        protected override IEnumerable<DataObject> FindObjectByKeyImpl(DataTableHandler tableHandler, IEnumerable<object> keys)
        {
            var primary = tableHandler.FieldElementBindings
                .Where(bind => bind.PrimaryKey != null)
                .Select(bind => new
                {
                    ColumnName = $"`{bind.ColumnName}`",
                    ParamName = $"@{bind.ColumnName}",
                    ParamType = bind.ValueType
                }).ToArray();

            if (!primary.Any())
                throw new DatabaseException($"Table {tableHandler.TableName} has no primary key for finding by key...");

            var whereClause = string.Join(" AND ", primary.Select(col => $"{col.ColumnName} = {col.ParamName}"));

            var keysArray = keys.ToList();  // Verwenden von ToList() um Mehrfachzugriffe zu vermeiden
            var parameters = keysArray.Select(key => primary.Select(col => new QueryParameter(col.ParamName, key, col.ParamType))).ToArray();

            var objs = SelectObjectsImpl(tableHandler, whereClause, parameters, Transaction.IsolationLevel.DEFAULT);

            return objs.Select((results, index) => results.FirstOrDefault()).ToArray();  // Verwendung von FirstOrDefault
        }


        /// <summary>
        /// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <param name="whereExpression">the where clause to filter object count on</param>
        /// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
        protected override int GetObjectCountImpl<TObject>(string whereExpression)
        {
            string tableName = AttributesUtils.GetTableOrViewName(typeof(TObject));
            if (!TableDatasets.TryGetValue(tableName, out DataTableHandler tableHandler))
                throw new DatabaseException($"Table {tableName} is not registered for Database Connection...");

            string command = string.IsNullOrEmpty(whereExpression)
                ? $"SELECT COUNT(*) FROM `{tableName}`"
                : $"SELECT COUNT(*) FROM `{tableName}` WHERE {whereExpression}";

            var count = ExecuteScalarImpl(command);

            return Convert.ToInt32(count);
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects Sets from database filtered by Parametrized Where Expression
        /// </summary>
        /// <param name="tableHandler">Table Handler for these DataObjects</param>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameters">Parameters for filtering</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of DataObjects Sets matching Parametrized Where Expression</returns>
        protected override IList<IList<DataObject>> SelectObjectsImpl(DataTableHandler tableHandler, string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters, Transaction.IsolationLevel isolation)
        {
            var columns = tableHandler.FieldElementBindings.ToArray();

            // Verwendung von StringBuilder für die SQL-Abfrage
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(string.Join(", ", columns.Select(col => $"`{col.ColumnName}`")));
            sb.Append($" FROM `{tableHandler.TableName}`");

            if (!string.IsNullOrEmpty(whereExpression))
            {
                sb.Append($" WHERE {whereExpression}");
            }

            string command = sb.ToString();
            var primary = columns.FirstOrDefault(col => col.PrimaryKey != null);
            var dataObjects = new List<IList<DataObject>>();

            ExecuteSelectImpl(command, parameters, reader => {
                var list = new List<DataObject>();
                var data = new object[reader.FieldCount];

                while (reader.Read())
                {
                    reader.GetValues(data);
                    // Verwendung einer Factory-Methode zur Erstellung von DataObject
                    var obj = CreateDataObject(tableHandler.ObjectType);

                    // Füllen des Objekts
                    for (var current = 0; current < columns.Length; current++)
                    {
                        DatabaseSetValue(obj, columns[current], data[current]);
                    }

                    // Setzen des Primärschlüssels
                    if (primary != null)
                        obj.ObjectId = primary.GetValue(obj).ToString();

                    list.Add(obj);
                    obj.Dirty = false;
                    obj.IsPersisted = true;
                }
                dataObjects.Add(list.ToArray());
            }, isolation);

            return dataObjects.ToArray();
        }

        // Beispiel einer Factory-Methode
        private DataObject CreateDataObject(Type objectType)
        {
            return Activator.CreateInstance(objectType) as DataObject;
        }

        /// <summary>
        /// Set Value to DataObject Field according to ElementBinding
        /// </summary>
        /// <param name="obj">DataObject to Fill</param>
        /// <param name="bind">ElementBinding for the targeted Member</param>
        /// <param name="value">Object Value to Fill</param>
        protected virtual void DatabaseSetValue(DataObject obj, ElementBinding bind, object value)
        {
            if (value == null || value.GetType().IsInstanceOfType(DBNull.Value))
                return;

            try
            {
                if (bind.ValueType == typeof(bool))
                    bind.SetValue(obj, Convert.ToBoolean(value));
                else if (bind.ValueType == typeof(char))
                    bind.SetValue(obj, Convert.ToChar(value));
                else if (bind.ValueType == typeof(sbyte))
                    bind.SetValue(obj, Convert.ToSByte(value));
                else if (bind.ValueType == typeof(short))
                    bind.SetValue(obj, Convert.ToInt16(value));
                else if (bind.ValueType == typeof(int))
                    bind.SetValue(obj, Convert.ToInt32(value));
                else if (bind.ValueType == typeof(long))
                    bind.SetValue(obj, Convert.ToInt64(value));
                else if (bind.ValueType == typeof(byte))
                    bind.SetValue(obj, Convert.ToByte(value));
                else if (bind.ValueType == typeof(ushort))
                    bind.SetValue(obj, Convert.ToUInt16(value));
                else if (bind.ValueType == typeof(uint))
                    bind.SetValue(obj, Convert.ToUInt32(value));
                else if (bind.ValueType == typeof(ulong))
                    bind.SetValue(obj, Convert.ToUInt64(value));
                else if (bind.ValueType == typeof(DateTime))
                    bind.SetValue(obj, Convert.ToDateTime(value));
                else if (bind.ValueType == typeof(float))
                    bind.SetValue(obj, Convert.ToSingle(value));
                else if (bind.ValueType == typeof(double))
                    bind.SetValue(obj, Convert.ToDouble(value));
                else if (bind.ValueType == typeof(string))
                    bind.SetValue(obj, Convert.ToString(value));
                else
                    bind.SetValue(obj, value);
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("{0}: {1} = {2} doesnt fit to {3}\n{4}", obj.TableName, bind.ColumnName, value.GetType().FullName, bind.ValueType, e);
            }
        }

        /// <summary>
        /// Fill SQL Command Parameter with Converted Values.
        /// </summary>
        /// <param name="parameter">Parameter collection for this Command</param>
        /// <param name="dbParams">DbParameter Object to Fill</param>
        protected virtual void FillSQLParameter(IEnumerable<QueryParameter> parameter, DbParameterCollection dbParams)
        {
            // Specififc Handling for Char Cast from DB Integer
            foreach (var param in parameter.Where(param => param.Name != null))
            {
                if (param.Value is char)
                    dbParams[param.Name].Value = Convert.ToUInt16(param.Value);
                else
                    dbParams[param.Name].Value = param.Value;
            }
        }
        #endregion

        #region Abstract Properties		
        /// <summary>
        /// The connection type to DB (xml, mysql,...)
        /// </summary>
        public abstract ConnectionType ConnectionType { get; }
        #endregion

        #region Table Implementation
        /// <summary>
        /// Check for Table Existence, Create or Alter accordingly
        /// </summary>
        /// <param name="table">Table Handler</param>
        public abstract void CheckOrCreateTableImpl(DataTableHandler table);
        #endregion

        #region Select Implementation
        /// <summary>
        /// Raw SQL Select Implementation
        /// </summary>
        /// <param name="SQLCommand">Command for reading</param>
        /// <param name="Reader">Reader Method</param>
        /// <param name="Isolation">Transaction Isolation</param>
        protected void ExecuteSelectImpl(string SQLCommand, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
        {
            ExecuteSelectImpl(SQLCommand, new[] { new QueryParameter[] { } }, Reader, Isolation);
        }

        /// <summary>
        /// Raw SQL Select Implementation with Single Parameter for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Command for reading</param>
        /// <param name="param">Parameter for Single Read</param>
        /// <param name="Reader">Reader Method</param>
        /// <param name="Isolation">Transaction Isolation</param>
        protected void ExecuteSelectImpl(string SQLCommand, QueryParameter param, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
        {
            ExecuteSelectImpl(SQLCommand, new[] { new[] { param } }, Reader, Isolation);
        }

        /// <summary>
        /// Raw SQL Select Implementation with Parameters for Single Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Command for reading</param>
        /// <param name="parameter">Collection of Parameters for Single Read</param>
        /// <param name="Reader">Reader Method</param>
        /// <param name="Isolation">Transaction Isolation</param>
        protected void ExecuteSelectImpl(string SQLCommand, IEnumerable<QueryParameter> parameter, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
        {
            ExecuteSelectImpl(SQLCommand, new[] { parameter }, Reader, Isolation);
        }

        /// <summary>
        /// Raw SQL Select Implementation with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Command for reading</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
        /// <param name="Reader">Reader Method</param>
        /// <param name="Isolation">Transaction Isolation</param>
        protected abstract void ExecuteSelectImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation);
        #endregion

        #region Non Query Implementation
        /// <summary>
        /// Execute a Raw Non-Query on the Database
        /// </summary>
        /// <param name="rawQuery">Raw Command</param>
        /// <returns>True if the Command succeeded</returns>
        public override bool ExecuteNonQuery(string rawQuery)
        {
            try
            {
                return ExecuteNonQueryImpl(rawQuery) > 0;
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("Error while executing raw query \"{0}\"\n{1}", rawQuery, e);
            }

            return false;
        }

        /// <summary>
        /// Implementation of Raw Non-Query
        /// </summary>
        /// <param name="SQLCommand">Raw Command</param>
        protected int ExecuteNonQueryImpl(string SQLCommand)
        {
            return ExecuteNonQueryImpl(SQLCommand, new[] { new QueryParameter[] { } }).First();
        }

        /// <summary>
        /// Raw Non-Query Implementation with Single Parameter for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Raw Command</param>
        /// <param name="param">Parameter for Single Command</param>
        protected int ExecuteNonQueryImpl(string SQLCommand, QueryParameter param)
        {
            return ExecuteNonQueryImpl(SQLCommand, new[] { new[] { param } }).First();
        }

        /// <summary>
        /// Raw Non-Query Implementation with Parameters for Single Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Raw Command</param>
        /// <param name="parameter">Collection of Parameters for Single Command</param>
        protected int ExecuteNonQueryImpl(string SQLCommand, IEnumerable<QueryParameter> parameter)
        {
            return ExecuteNonQueryImpl(SQLCommand, new[] { parameter }).First();
        }

        /// <summary>
        /// Implementation of Raw Non-Query with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Raw Command</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Command</param>
        /// <returns>True foreach Command that succeeded</returns>
        protected abstract IEnumerable<int> ExecuteNonQueryImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters);
        #endregion

        #region Scalar Implementation
        /// <summary>
        /// Implementation of Scalar Query
        /// </summary>
        /// <param name="SQLCommand">Scalar Command</param>
        /// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
        /// <returns>Object Returned by Scalar</returns>
        protected object ExecuteScalarImpl(string SQLCommand, bool retrieveLastInsertID = false)
        {
            return ExecuteScalarImpl(SQLCommand, new[] { new QueryParameter[] { } }, retrieveLastInsertID).First();
        }

        /// <summary>
        /// Implementation of Scalar Query with Single Parameter for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Scalar Command</param>
        /// <param name="param">Parameter for Single Command</param>
        /// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
        /// <returns>Object Returned by Scalar</returns>
        protected object ExecuteScalarImpl(string SQLCommand, QueryParameter param, bool retrieveLastInsertID = false)
        {
            return ExecuteScalarImpl(SQLCommand, new[] { new[] { param } }, retrieveLastInsertID).First();
        }

        /// <summary>
        /// Implementation of Scalar Query with Parameters for Single Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Scalar Command</param>
        /// <param name="parameter">Collection of Parameters for Single Command</param>
        /// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
        /// <returns>Object Returned by Scalar</returns>
        protected object ExecuteScalarImpl(string SQLCommand, IEnumerable<QueryParameter> parameter, bool retrieveLastInsertID = false)
        {
            return ExecuteScalarImpl(SQLCommand, new[] { parameter }, retrieveLastInsertID).First();
        }

        /// <summary>
        /// Implementation of Scalar Query with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Scalar Command</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
        /// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
        /// <returns>Objects Returned by Scalar</returns>
        protected abstract object[] ExecuteScalarImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, bool retrieveLastInsertID);
        #endregion

        #region Specific
        protected virtual bool HandleException(Exception e)
        {
            bool ret = false;
            var socketException = e.InnerException == null
                ? null
                : e.InnerException.InnerException as System.Net.Sockets.SocketException;

            if (socketException == null)
                socketException = e.InnerException as System.Net.Sockets.SocketException;

            if (socketException != null)
            {
                // Handle socket exception. Error codes:
                // http://msdn2.microsoft.com/en-us/library/ms740668.aspx
                // 10052 = Network dropped connection on reset.
                // 10053 = Software caused connection abort.
                // 10054 = Connection reset by peer.
                // 10057 = Socket is not connected.
                // 10058 = Cannot send after socket shutdown.
                switch (socketException.ErrorCode)
                {
                    case 10052:
                    case 10053:
                    case 10054:
                    case 10057:
                    case 10058:
                        ret = true;
                        break;
                }

                if (Log.IsWarnEnabled)
                    Log.WarnFormat("Socket exception: ({0}) {1}; repeat: {2}", socketException.ErrorCode, socketException.Message, ret);
            }

            return ret;
        }
        #endregion
    }
}
