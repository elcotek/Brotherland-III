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

using DOL.Database.Connection;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using IsolationLevel = DOL.Database.Transaction.IsolationLevel;

namespace DOL.Database.Handlers
{




    public class MySQLObjectDatabase : SQLObjectDatabase
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool DebugOFF = true;




        /// <summary>
        /// Create a new instance of <see cref="MySQLObjectDatabase"/>
        /// </summary>
        /// <param name="ConnectionString">Database Connection String</param>
        public MySQLObjectDatabase(string ConnectionString)
            : base(ConnectionString)
        {
            // Options of MySQL connection string
            if (!this.ConnectionString.Contains("Treat Tiny As Boolean") && !this.ConnectionString.Contains("TreatTinyAsBoolean"))
            {
                this.ConnectionString += ";Treat Tiny As Boolean=False";
            }

            if (!this.ConnectionString.Contains("Ignore Prepare") && !this.ConnectionString.Contains("Ignore Prepare"))
            {
                this.ConnectionString += ";Ignore Prepare=False";
            }

            if (!this.ConnectionString.Contains("Allow User Variables") && !this.ConnectionString.Contains("AllowUserVariables"))
            {
                this.ConnectionString += ";Allow User Variables=True";
            }

            if (!this.ConnectionString.Contains("Convert Zero Datetime") && !this.ConnectionString.Contains("ConvertZeroDateTime"))
            {
                this.ConnectionString += ";Convert Zero Datetime=True";
            }
        }

        #region MySQL Implementation
        /// <summary>
        /// Convert a Table ElementBinding to Database Type string (Upper)
        /// </summary>
        /// <param name="bind">ElementBindind to Convert</param>
        /// <param name="table">DataTableHandler for Special cases</param>
        /// <returns>Database Type string ToUpper</returns>
        protected virtual string GetDatabaseType(ElementBinding bind, DataTableHandler table)
        {
            if (bind == null)
            {
                throw new ArgumentNullException(nameof(bind));
            }

            string type = null;
            // Check Value Type
            if (bind.ValueType == typeof(char))
            {
                type = "SMALLINT(5) UNSIGNED";
            }
            else if (bind.ValueType == typeof(DateTime))
            {
                type = "DATETIME";
            }
            else if (bind.ValueType == typeof(sbyte))
            {
                type = "TINYINT(3)";
            }
            else if (bind.ValueType == typeof(short))
            {
                type = "SMALLINT(6)";
            }
            else if (bind.ValueType == typeof(int))
            {
                type = "INT(11)";
            }
            else if (bind.ValueType == typeof(long))
            {
                type = "BIGINT(20)";
            }
            else if (bind.ValueType == typeof(byte))
            {
                type = "TINYINT(3) UNSIGNED";
            }
            else if (bind.ValueType == typeof(ushort))
            {
                type = "SMALLINT(5) UNSIGNED";
            }
            else if (bind.ValueType == typeof(uint))
            {
                type = "INT(10) UNSIGNED";
            }
            else if (bind.ValueType == typeof(ulong))
            {
                type = "BIGINT(20) UNSIGNED";
            }
            else if (bind.ValueType == typeof(float))
            {
                // Float Value have less precision than C# Single.
                type = "DOUBLE";
            }
            else if (bind.ValueType == typeof(double))
            {
                type = "DOUBLE";
            }
            else if (bind.ValueType == typeof(bool))
            {
                type = "TINYINT(1)";
            }
            else if (bind.ValueType == typeof(string))
            {
                if (bind.DataElement != null && bind.DataElement.Varchar > 0)
                {
                    type = string.Format("VARCHAR({0})", bind.DataElement.Varchar);
                }
                else if (table.Table.PrimaryKey.Any(key => key.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase))
                         || table.Table.Constraints.OfType<UniqueConstraint>().Any(cstrnt => cstrnt.Columns.Any(col => col.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase)))
                         || (table.Table.ExtendedProperties["INDEXES"] != null && (table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>)
                             .Any(kv => kv.Value.Any(col => col.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase)))))
                {
                    // If is in Primary Key Constraint or Unique Constraint or Index row, cast to Varchar.
                    type = "VARCHAR(255)";
                }
                else
                {
                    type = "TEXT";
                }
            }
            else
            {
                type = "BLOB";
            }

            if (bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement)
            {
                if (bind.ValueType == typeof(ulong) || bind.ValueType == typeof(long))
                    type = "BIGINT(20)";
                else
                    type = "INT(11)";
            }

            return type;
        }

        /// <summary>
        /// Get Database Column Definition for ElementBinding
        /// </summary>
        /// <param name="bind">ElementBinding for Column Definition</param>
        /// <param name="table">DataTableHanlder for Special cases</param>
        /// <returns>Column Definitnion string.</returns>
        protected virtual string GetColumnDefinition(ElementBinding bind, DataTableHandler table)
        {
            string type = GetDatabaseType(bind, table);
            string defaultDef = null;

            // Check for Default Value depending on Constraints and Type
            if (bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement)
            {
                defaultDef = "NOT NULL AUTO_INCREMENT";
            }
            else if (bind.DataElement != null && bind.DataElement.AllowDbNull)
            {
                defaultDef = "DEFAULT NULL";
            }
            else if (bind.ValueType == typeof(DateTime))
            {
                defaultDef = "NOT NULL DEFAULT '2000-01-01 00:00:00'";
            }
            else
            {
                defaultDef = "NOT NULL";
            }

            return string.Format("`{0}` {1} {2}", bind.ColumnName, type, defaultDef);
        }
        #endregion

        #region Create / Alter Table
        /// <summary>
        /// Check for Table Existence, Create or Alter accordingly
        /// </summary>
        /// <param name="table">Table Handler</param>
        public override void CheckOrCreateTableImpl(DataTableHandler table)
        {
            var currentTableColumns = new List<TableRowBindind>();
            try
            {
                ExecuteSelectImpl(string.Format("DESCRIBE `{0}`", table.TableName),
                                  reader =>
                                  {
                                      while (reader.Read())
                                      {
                                          var column = reader.GetString(0);
                                          var colType = reader.GetString(1);
                                          var allowNull = reader.GetString(2).ToLower() == "yes";
                                          var primary = reader.GetString(3).ToLower() == "pri";
                                          currentTableColumns.Add(new TableRowBindind(column, colType, allowNull, primary));
                                          if (log.IsDebugEnabled && DebugOFF == false)
                                              log.DebugFormat("CheckOrCreateTable: Found Column {0} in existing table {1}", column, table.TableName);
                                      }
                                      if (log.IsDebugEnabled && DebugOFF == false)
                                          log.DebugFormat("CheckOrCreateTable: {0} columns existing in table {1}", currentTableColumns.Count, table.TableName);
                                  }, IsolationLevel.DEFAULT);
            }
            catch (Exception e)
            {
                if (log.IsDebugEnabled && DebugOFF == false)
                    log.Debug("CheckOrCreateTable: ", e);
            }

            // Create Table or Alter Table
            if (currentTableColumns.Any())
            {
                AlterTable(currentTableColumns, table);
            }
            else
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("Table {0} doesn't exist, creating it...", table.TableName);

                CreateTable(table);
            }
        }

        /// <summary>
        /// Create a New Table from DataTableHandler Definition
        /// </summary>
        /// <param name="table">DataTableHandler Definition to Create in Database</param>
        protected void CreateTable(DataTableHandler table)
        {
            var columnDef = table.FieldElementBindings
                .Select(bind => GetColumnDefinition(bind, table));

            var primaryFields = string.Format("PRIMARY KEY ({0})",
                                              string.Join(", ", table.Table.PrimaryKey.Select(pk => string.Format("`{0}`", pk.ColumnName))));

            var uniqueFields = table.Table.Constraints.OfType<UniqueConstraint>().Where(cstrnt => !cstrnt.IsPrimaryKey)
                .Select(cstrnt => string.Format("UNIQUE KEY `{0}` ({1})", cstrnt.ConstraintName,
                                                string.Join(", ", cstrnt.Columns.Select(col => string.Format("`{0}`", col.ColumnName)))));

            var indexes = table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>;

            var indexesFields = indexes == null ? new string[] { }
                : indexes.Select(index => string.Format("KEY `{0}` ({1})", index.Key,
                                                    string.Join(", ", index.Value.Select(col => string.Format("`{0}`", col.ColumnName)))));

            var command = string.Format("CREATE TABLE IF NOT EXISTS `{0}` ({1})", table.TableName,
                                        string.Join(", \n", columnDef.Concat(new[] { primaryFields }).Concat(uniqueFields).Concat(indexesFields)));

            ExecuteNonQueryImpl(command);
        }

        /// <summary>
        /// Alter an Existing Table to Match DataTableHandler Definition
        /// </summary>
        /// <param name="currentColumns">Current Existing Columns</param>
        /// <param name="table">DataTableHandler to Implement</param>
        protected void AlterTable(IEnumerable<TableRowBindind> currentColumns, DataTableHandler table)
        {
            var columnDefs = new List<string>();
            var alteredColumn = new List<string>();
            // Check for Missing Column or Wrong Type
            foreach (ElementBinding binding in table.FieldElementBindings)
            {
                var column = currentColumns.FirstOrDefault(col => col.ColumnName.Equals(binding.ColumnName, StringComparison.OrdinalIgnoreCase));

                if (column != null)
                {

                    // Check Null && Type
                    if ((binding.DataElement != null && binding.DataElement.AllowDbNull) != column.AllowDbNull
                        || !GetDatabaseType(binding, table).Equals(column.ColumnType, StringComparison.OrdinalIgnoreCase))
                    {
                        columnDefs.Add(string.Format("CHANGE `{1}` {0}", GetColumnDefinition(binding, table), binding.ColumnName));
                        alteredColumn.Add(binding.ColumnName);
                    }

                    continue;
                }

                columnDefs.Add(string.Format("ADD {0}", GetColumnDefinition(binding, table)));
            }

            // Check for Indexes
            var indexes = new List<Tuple<bool, string, string>>();
            try
            {
                ExecuteSelectImpl(string.Format("SHOW INDEX FROM `{0}`", table.TableName),
                                  reader =>
                                  {
                                      while (reader.Read())
                                      {
                                          var unique = reader.GetInt64(1) < 1;
                                          var indexname = reader.GetString(2);
                                          var column = reader.GetString(4);
                                          indexes.Add(new Tuple<bool, string, string>(unique, indexname, column));
                                          if (log.IsDebugEnabled && DebugOFF == false)
                                              log.DebugFormat("AlterTable: Found Index `{0}` (Unique:{1}) on `{2}` in existing table {3}", indexname, unique, column, table.TableName);
                                      }
                                      if (log.IsDebugEnabled && DebugOFF == false)
                                          log.DebugFormat("AlterTable: {0} Indexes existing in table {1}", indexes.Count, table.TableName);
                                  }, IsolationLevel.DEFAULT);
            }
            catch (Exception e)
            {
                if (log.IsDebugEnabled && DebugOFF == false)
                    log.Debug("AlterTable: ", e);
            }

            // Sort Indexes
            var existingIndexes = indexes.GroupBy(ind => new { KeyName = ind.Item2, Unique = ind.Item1 })
                .Select(grp => new { grp.Key.KeyName, grp.Key.Unique, Columns = grp.Select(i => i.Item3).ToArray() }).ToArray();

            var havePrimaryIndex = existingIndexes.FirstOrDefault(ind => ind.KeyName.Equals("PRIMARY"));
            var currentPrimaryColumn = new string[] { };
            if (havePrimaryIndex != null)
                currentPrimaryColumn = havePrimaryIndex.Columns;

            // Check for Any Difference in Primary Keys
            if (table.Table.PrimaryKey.Length != currentPrimaryColumn.Length
               || table.Table.PrimaryKey.Any(pk =>
               {
                   var column = currentPrimaryColumn.FirstOrDefault(col => col.Equals(pk.ColumnName, StringComparison.OrdinalIgnoreCase));

                   return column == null;
               }))
            {
                // Allow to edit Auto increment key if not previously modified
                foreach (var oldkeys in currentColumns.Where(col => col.Primary && !alteredColumn.Any(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
                    columnDefs.Add(string.Format("MODIFY `{0}` {1} {2}", oldkeys.ColumnName, oldkeys.ColumnType, oldkeys.AllowDbNull ? "DEFAULT NULL" : "NOT NULL"));

                if (currentPrimaryColumn.Any())
                    columnDefs.Add("DROP PRIMARY KEY");

                columnDefs.Add(string.Format("ADD PRIMARY KEY ({0})", string.Join(", ", table.Table.PrimaryKey.Select(pk => string.Format("`{0}`", pk.ColumnName)))));
            }


            var tableIndexes = table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>;

            // Check for Index Removal
            foreach (var existing in existingIndexes.Where(ind => !ind.KeyName.Equals("PRIMARY")))
            {
                if (tableIndexes.TryGetValue(existing.KeyName, out DataColumn[] realindex))
                {
                    // Check for index modifications
                    if (realindex.Length != existing.Columns.Length
                        || !realindex.All(col => existing.Columns.Any(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
                    {
                        columnDefs.Add(string.Format("DROP KEY `{0}`", existing.KeyName));
                        columnDefs.Add(string.Format("ADD KEY `{0}` ({1})", existing.KeyName, string.Join(", ", realindex.Select(col => string.Format("`{0}`", col)))));
                    }
                }
                else
                {
                    // Check for Unique
                    var realunique = table.Table.Constraints.OfType<UniqueConstraint>().FirstOrDefault(cstrnt => !cstrnt.IsPrimaryKey && cstrnt.ConstraintName.Equals(existing.KeyName, StringComparison.OrdinalIgnoreCase));
                    if (realunique == null)
                        columnDefs.Add(string.Format("DROP KEY `{0}`", existing.KeyName));
                    else if (realunique.Columns.Length != existing.Columns.Length
                             || !realunique.Columns.All(col => existing.Columns.Any(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
                    {
                        columnDefs.Add(string.Format("DROP KEY `{0}`", existing.KeyName));
                        columnDefs.Add(string.Format("ADD UNIQUE KEY `{0}` ({1})", existing.KeyName, string.Join(", ", realunique.Columns.Select(col => string.Format("`{0}`", col)))));
                    }
                }
            }

            // Missing Indexes
            foreach (var missing in tableIndexes.Where(kv => existingIndexes.All(c => !c.KeyName.Equals(kv.Key, StringComparison.OrdinalIgnoreCase))))
                columnDefs.Add(string.Format("ADD KEY `{0}` ({1})", missing.Key, string.Join(", ", missing.Value.Select(col => string.Format("`{0}`", col)))));

            foreach (var missing in table.Table.Constraints.OfType<UniqueConstraint>().Where(cstrnt => !cstrnt.IsPrimaryKey && existingIndexes.All(c => !c.KeyName.Equals(cstrnt.ConstraintName, StringComparison.OrdinalIgnoreCase))))
                columnDefs.Add(string.Format("ADD UNIQUE KEY `{0}` ({1})", missing.ConstraintName, string.Join(", ", missing.Columns.Select(col => string.Format("`{0}`", col)))));


            if (!columnDefs.Any())
                return;

            if (log.IsInfoEnabled)
                log.InfoFormat("Altering Table {0} this could take a few minutes...", table.TableName);

            using (var conn = new MySqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    using (var tran = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        try
                        {
                            cmd.Transaction = tran;

                            // Update Null Values
                            var nullToNonNull = table.FieldElementBindings.Join(currentColumns, bind => bind.ColumnName, col => col.ColumnName, (bind, col) => new { bind, col }, StringComparer.OrdinalIgnoreCase)
                                .Where(match => match.bind.DataElement != null && match.bind.DataElement.AllowDbNull == false && match.col.AllowDbNull == true)
                                .Select(match => match.bind)
                                .ToArray();

                            if (nullToNonNull.Any())
                            {
                                cmd.CommandText = string.Format("UPDATE `{0}` SET {1} WHERE {2}"
                                                                , table.TableName
                                                                , string.Join(", ", nullToNonNull.Select(bind => string.Format("`{0}` = IFNULL(`{0}`, {1})", bind.ColumnName, bind.ValueType == typeof(DateTime)
                                                                                                                               ? "'2000-01-01 00:00:00'"
                                                                                                                               : bind.ValueType == typeof(string)
                                                                                                                               ? "''"
                                                                                                                               : "0")))
                                                                , string.Join(" OR ", nullToNonNull.Select(bind => string.Format("`{0}` IS NULL", bind.ColumnName)))
                                                           );
                                cmd.ExecuteNonQuery();
                            }

                            // Alter Table
                            cmd.CommandText = string.Format("ALTER TABLE `{0}` {1}", table.TableName, string.Join(", \n", columnDefs));
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            if (log.IsDebugEnabled)
                                log.Debug("AlterTableImpl: ", e);

                            if (log.IsWarnEnabled)
                                log.WarnFormat("AlterTableImpl: Error While Altering Table {0}, rollback...\n{1}", table.TableName, e);

                            throw;
                        }
                    }
                }
            }
        }
        #endregion

        #region Property implementation
        /// <summary>
        /// The connection type to DB (xml, mysql,...)
        /// </summary>
        public override ConnectionType ConnectionType { get { return ConnectionType.DATABASE_MYSQL; } }
        #endregion

        #region SQLObject Implementation
        /// <summary>
        /// Raw SQL Select Implementation with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Command for reading</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
        /// <param name="Reader">Reader Method</param>
        /// <param name="Isolation">Transaction Isolation</param>
        protected override void ExecuteSelectImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, Action<IDataReader> Reader, IsolationLevel Isolation)
        {
            if (log.IsDebugEnabled && DebugOFF == false)
                log.DebugFormat("ExecuteSelectImpl: {0}", SQLCommand);

            var repeat = false;
            var current = 0;
            do
            {
                repeat = false;

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = SQLCommand;
                            conn.Open();
                            long start = (DateTime.UtcNow.Ticks / 10000);

                            // Register Parameters
                            foreach (var param in parameters.First().Select(kv => new { kv.Name, Type = GuessQueryParameterDbType(kv) }))
                                cmd.Parameters.Add(param.Name, param.Type);
                            cmd.Prepare();

                            foreach (var parameter in parameters.Skip(current))
                            {
                                FillSQLParameter(parameter, cmd.Parameters);

                                using (var reader = cmd.ExecuteReader())
                                {
                                    try
                                    {
                                        Reader(reader);
                                    }
                                    catch (Exception es)
                                    {
                                        if (log.IsWarnEnabled)
                                            log.WarnFormat("ExecuteSelectImpl: Exception in Select Callback : {2}{0}{2}{1}", es, Environment.StackTrace, Environment.NewLine);
                                    }
                                    finally
                                    {
                                        reader.Close();
                                    }
                                }
                                current++;
                            }

                            if (log.IsDebugEnabled && DebugOFF == false)
                                log.DebugFormat("ExecuteSelectImpl: SQL Select ({0}) exec time {1}ms", Isolation, ((DateTime.UtcNow.Ticks / 10000) - start));
                            else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
                                log.WarnFormat("ExecuteSelectImpl: SQL Select ({0}) took {1}ms!\n{2}", Isolation, ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);

                        }
                        catch (Exception e)
                        {
                            if (!HandleException(e))
                            {
                                if (log.IsErrorEnabled)
                                    log.ErrorFormat("ExecuteSelectImpl: UnHandled Exception in Select Query \"{0}\"\n{1}", SQLCommand, e);

                                throw;
                            }
                            repeat = true;
                        }
                    }
                }
            }
            while (repeat);
        }

        /// <summary>
        /// Implementation of Raw Non-Query with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Raw Command</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
        /// <returns>True if the Command succeeded</returns>
        protected override IEnumerable<int> ExecuteNonQueryImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters)
        {
            if (log.IsDebugEnabled && DebugOFF == false)
                log.DebugFormat("ExecuteNonQueryImpl: {0}", SQLCommand);

            var affected = new List<int>();
            var repeat = false;
            var current = 0;
            do
            {
                repeat = false;

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = SQLCommand;
                            conn.Open();
                            long start = (DateTime.UtcNow.Ticks / 10000);

                            // Register Parameters
                            foreach (var param in parameters.First().Select(kv => new { kv.Name, Type = GuessQueryParameterDbType(kv) }))
                                cmd.Parameters.Add(param.Name, param.Type);
                            cmd.Prepare();

                            foreach (var parameter in parameters.Skip(current))
                            {
                                FillSQLParameter(parameter, cmd.Parameters);

                                var result = -1;
                                try
                                {
                                    result = cmd.ExecuteNonQuery();
                                    affected.Add(result);
                                }
                                catch (MySqlException sqle)
                                {
                                    if (HandleSQLException(sqle))
                                    {
                                        affected.Add(result);
                                        if (log.IsErrorEnabled)
                                            log.ErrorFormat("ExecuteNonQueryImpl: Constraint Violation for raw query \"{0}\"\n{1}\n{2}", SQLCommand, sqle, Environment.StackTrace);
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                                current++;

                                if (log.IsDebugEnabled && DebugOFF == false && result < 1)
                                    log.DebugFormat("ExecuteNonQueryImpl: No Change for raw query \"{0}\"", SQLCommand);
                            }

                            if (log.IsDebugEnabled && DebugOFF == false)
                                log.DebugFormat("ExecuteNonQueryImpl: SQL NonQuery exec time {0}ms", ((DateTime.UtcNow.Ticks / 10000) - start));
                            else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
                                log.WarnFormat("ExecuteNonQueryImpl: SQL NonQuery took {0}ms!\n{1}", ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);
                        }
                        catch (Exception e)
                        {
                            if (!HandleException(e))
                            {
                                if (log.IsErrorEnabled)
                                    log.ErrorFormat("ExecuteNonQueryImpl: UnHandled Exception for raw query \"{0}\"\n{1}", SQLCommand, e);

                                throw;
                            }
                            repeat = true;
                        }
                    }
                }
            }
            while (repeat);

            return affected;
        }

        /// <summary>
        /// Implementation of Scalar Query with Parameters for Prepared Query
        /// </summary>
        /// <param name="SQLCommand">Scalar Command</param>
        /// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
        /// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
        /// <returns>Objects Returned by Scalar</returns>
        protected override object[] ExecuteScalarImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, bool retrieveLastInsertID)
        {
            if (log.IsDebugEnabled && DebugOFF == false)
                log.DebugFormat("ExecuteScalarImpl: {0}", SQLCommand);

            var obj = new List<object>();
            var repeat = false;
            var current = 0;
            do
            {
                repeat = false;

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = SQLCommand;
                            conn.Open();
                            long start = (DateTime.UtcNow.Ticks / 10000);

                            // Register Parameters
                            foreach (var param in parameters.First().Select(kv => new { kv.Name, Type = GuessQueryParameterDbType(kv) }))
                                cmd.Parameters.Add(param.Name, param.Type);
                            cmd.Prepare();

                            foreach (var parameter in parameters.Skip(current))
                            {
                                FillSQLParameter(parameter, cmd.Parameters);

                                if (retrieveLastInsertID)
                                {
                                    using (var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                                    {
                                        try
                                        {
                                            cmd.Transaction = tran;
                                            try
                                            {
                                                cmd.ExecuteNonQuery();
                                                obj.Add(cmd.LastInsertedId);
                                            }
                                            catch (MySqlException sqle)
                                            {
                                                if (HandleSQLException(sqle))
                                                {
                                                    obj.Add(-1);
                                                    if (log.IsErrorEnabled)
                                                        log.ErrorFormat("ExecuteScalarImpl: Constraint Violation for command \"{0}\"\n{1}\n{2}", SQLCommand, sqle, Environment.StackTrace);
                                                }
                                                else
                                                {
                                                    throw;
                                                }
                                            }
                                            tran.Commit();
                                        }
                                        catch (Exception te)
                                        {
                                            tran.Rollback();
                                            if (log.IsErrorEnabled)
                                                log.ErrorFormat("ExecuteScalarImpl: Error in Transaction (Rollback) for command : {0}\n{1}", SQLCommand, te);
                                        }

                                    }
                                }
                                else
                                {
                                    var result = cmd.ExecuteScalar();
                                    obj.Add(result);
                                }
                                current++;
                            }

                            if (log.IsDebugEnabled && DebugOFF == false)
                                log.DebugFormat("ExecuteScalar: SQL ScalarQuery exec time {0}ms", ((DateTime.UtcNow.Ticks / 10000) - start));
                            else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
                                log.WarnFormat("ExecuteScalar: SQL ScalarQuery took {0}ms!\n{1}", ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);
                        }
                        catch (Exception e)
                        {
                            if (!HandleException(e))
                            {
                                if (log.IsErrorEnabled)
                                    log.ErrorFormat("ExecuteScalar: UnHandled Exception for command \"{0}\"\n{1}", SQLCommand, e);

                                throw;
                            }
                            repeat = true;
                        }
                    }
                }
            }
            while (repeat);

            return obj.ToArray();
        }
        #endregion

        /// <summary>
		/// Retrieve Query Parameter DB Type from Table Type or Runtime Type
		/// </summary>
		/// <param name="param">Query Parameter</param>
		/// <returns>MySqlDbType for this Query Paramter, "Blob" as default.</returns>
        protected static MySqlDbType GuessQueryParameterDbType(QueryParameter param)
        {
            var type = param.ValueType ?? (param.Value?.GetType());

            if (typeof(string).IsAssignableFrom(type))
                return MySqlDbType.Text;
            if (typeof(DateTime).IsAssignableFrom(type))
                return MySqlDbType.DateTime;
            if (typeof(bool).IsAssignableFrom(type))
                return MySqlDbType.Bit;
            if (typeof(char).IsAssignableFrom(type))
                return MySqlDbType.UInt16;
            if (typeof(byte).IsAssignableFrom(type))
                return MySqlDbType.UByte;
            if (typeof(sbyte).IsAssignableFrom(type))
                return MySqlDbType.Byte;
            if (typeof(ushort).IsAssignableFrom(type))
                return MySqlDbType.UInt16;
            if (typeof(short).IsAssignableFrom(type))
                return MySqlDbType.Int16;
            if (typeof(uint).IsAssignableFrom(type))
                return MySqlDbType.UInt32;
            if (typeof(int).IsAssignableFrom(type))
                return MySqlDbType.Int32;
            if (typeof(ulong).IsAssignableFrom(type))
                return MySqlDbType.UInt64;
            if (typeof(long).IsAssignableFrom(type))
                return MySqlDbType.Int64;
            if (typeof(float).IsAssignableFrom(type))
                return MySqlDbType.Double;
            if (typeof(double).IsAssignableFrom(type))
                return MySqlDbType.Double;

            return MySqlDbType.Blob;
        }
        /// <summary>
        /// Handle Non Fatal SQL Query Exception
        /// </summary>
        /// <param name="sqle">SQL Excepiton</param>
        /// <returns>True if handled, False otherwise</returns>
        protected static bool HandleSQLException(MySqlException sqle)
        {
            switch ((MySqlErrorCode)sqle.Number)
            {
                case MySqlErrorCode.DuplicateUnique:
                case MySqlErrorCode.DuplicateKeyEntry:
                    return true;
                default:
                    return false;
            }
        }
    }
}
