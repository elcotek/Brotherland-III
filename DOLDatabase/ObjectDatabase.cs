/*
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
using DOL.Database.Handlers;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DOL.Database
{
    /// <summary>
    /// Default Object Database Base Implementation
    /// </summary>
    public abstract class ObjectDatabase : IObjectDatabase
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Number Format Info to Use for Database
        /// </summary>
        protected static readonly NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;

        /// <summary>
        /// Data Table Handlers for this Database Handler
        /// </summary>
        protected readonly Dictionary<string, DataTableHandler> TableDatasets = new Dictionary<string, DataTableHandler>();

        /// <summary>
        /// Connection String for this Database
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        /// Creates a new Instance of <see cref="ObjectDatabase"/>
        /// </summary>
        /// <param name="ConnectionString">Database Connection String</param>
        protected ObjectDatabase(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        /// <summary>
        /// Helper to Retrieve Table Handler from Object Type
        /// Return Real Table Handler for Modifications Queries
        /// </summary>
        /// <param name="objectType">Object Type</param>
        /// <returns>DataTableHandler for this Object Type or null.</returns>
        protected DataTableHandler GetTableHandler(Type objectType)
        {
            var tableName = AttributesUtils.GetTableName(objectType);
            DataTableHandler handler;
            return TableDatasets.TryGetValue(tableName, out handler) ? handler : null;
        }

        /// <summary>
        /// Helper to Retrieve Table or View Handler from Object Type
        /// Return View or Table for Select Queries
        /// </summary>
        /// <param name="objectType">Object Type</param>
        /// <returns>DataTableHandler for this Object Type or null.</returns>
        protected DataTableHandler GetTableOrViewHandler(Type objectType)
        {
            var tableName = AttributesUtils.GetTableOrViewName(objectType);
            DataTableHandler handler;
            return TableDatasets.TryGetValue(tableName, out handler) ? handler : null;
        }

        #region Public Add Objects Implementation
        /// <summary>
        /// Insert a new DataObject into the database and save it
        /// </summary>
        /// <param name="dataObject">DataObject to Add into database</param>
        /// <returns>True if the DataObject was added.</returns>
        public bool AddObject(DataObject dataObject)
        {
            return AddObject(new[] { dataObject });
        }

        /// <summary>
        /// Insert new DataObjects into the database and save them
        /// </summary>
        /// <param name="dataObjects">DataObjects to Add into database</param>
        /// <returns>True if All DataObjects were added.</returns>
        public bool AddObject(IEnumerable<DataObject> dataObjects)
        {
            var success = true;

            // Verwenden von ToLookup, um die Datenobjekte einmalig zu gruppieren
            var groupedDataObjects = dataObjects.ToLookup(obj => obj.GetType());

            Parallel.ForEach(groupedDataObjects, grp =>
            {
                var tableHandler = GetTableHandler(grp.Key);

                if (tableHandler == null)
                {
                    if (Log.IsErrorEnabled)
                        Log.ErrorFormat("AddObject: DataObject Type ({0}) not registered !", grp.Key.FullName);
                    success = false;
                    return;
                }

                // Filtern der erlaubten Objekte
                var allowedGroup = grp.Where(item => item.AllowAdd).ToList();
                if (allowedGroup.Count > 0)
                {
                    var results = AddObjectImpl(tableHandler, allowedGroup.ToArray());

                    foreach (var (result, index) in results.Select((result, index) => (result, index)))
                    {
                        var obj = allowedGroup[index];
                        if (result)
                        {
                            // Erfolg, Handling der Pre-Caching-Logik
                            if (tableHandler.UsesPreCaching)
                            {
                                var primary = tableHandler.PrimaryKey;
                                if (primary != null)
                                    tableHandler.SetPreCachedObject(primary.GetValue(obj), obj);
                            }

                            // Speichern der Beziehungen, falls vorhanden
                            if (tableHandler.HasRelations)
                                success &= SaveObjectRelations(tableHandler, new[] { obj });
                        }
                        else if (Log.IsErrorEnabled)
                        {
                            Log.ErrorFormat("AddObjects: DataObject ({0}) could not be inserted into database...", obj);
                            success = false;
                        }
                    }
                }
                else
                {
                    if (Log.IsWarnEnabled)
                    {
                        foreach (var obj in grp)
                            Log.WarnFormat("AddObject: DataObject ({0}) not allowed to be added to Database", obj);
                    }
                    success = false;
                }
            });

            return success;
        }


        #endregion
        #region Public Save Objects Implementation
        /// <summary>
        /// Saves a DataObject to database if saving is allowed and object is dirty
        /// </summary>
        /// <param name="dataObject">DataObject to Save in database</param>
        /// <returns>True is the DataObject was saved.</returns>
        public bool SaveObject(DataObject dataObject)
        {
            return SaveObject(new[] { dataObject });
        }

        /// <summary>
        /// Save DataObjects to database if saving is allowed and object is dirty
        /// </summary>
        /// <param name="dataObjects">DataObjects to Save in database</param>
        /// <returns>True if All DataObjects were saved.</returns>
        public bool SaveObject(IEnumerable<DataObject> dataObjects)
        {
            var success = true;
            var groupedData = dataObjects.GroupBy(obj => obj.GetType()).ToList();

            Parallel.ForEach(groupedData, grp =>
            {
                var tableHandler = GetTableHandler(grp.Key);
                if (tableHandler == null)
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.ErrorFormat("SaveObject: DataObject Type ({0}) not registered!", grp.Key.FullName);
                    }
                    success = false;
                    return;
                }

                var objs = grp.Where(obj => obj.Dirty).ToList();
                if (!objs.Any()) return; // �berspringen, wenn keine "dirty" Objekte vorhanden sind

                var results = SaveObjectImpl(tableHandler, objs.ToArray());
                var resultsByObjs = results.Select((result, index) => new { Success = result, DataObject = objs[index] })
                    .GroupBy(obj => obj.Success);

                foreach (var resultGrp in resultsByObjs)
                {
                    if (resultGrp.Key)
                    {
                        if (tableHandler.UsesPreCaching)
                        {
                            var primary = tableHandler.PrimaryKey;
                            if (primary != null)
                            {
                                foreach (var successObj in resultGrp.Select(obj => obj.DataObject))
                                {
                                    tableHandler.SetPreCachedObject(primary.GetValue(successObj), successObj);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Log.IsErrorEnabled)
                        {
                            foreach (var obj in resultGrp)
                            {
                                Log.ErrorFormat("SaveObject: DataObject ({0}) could not be saved into database...", obj.DataObject);
                            }
                        }
                        success = false;
                    }
                }

                if (tableHandler.HasRelations)
                {
                    success &= SaveObjectRelations(tableHandler, grp);
                }
            });

            return success;
        }



        #endregion
        #region Public Delete Objects Implementation
        /// <summary>
        /// Delete a DataObject from database if deletion is allowed
        /// </summary>
        /// <param name="dataObject">DataObject to Delete from database</param>
        /// <returns>True if the DataObject was deleted.</returns>
        public bool DeleteObject(DataObject dataObject)
        {
            return DeleteObject(new[] { dataObject });
        }

        /// <summary>
        /// Delete DataObjects from database if deletion is allowed
        /// </summary>
        /// <param name="dataObjects">DataObjects to Delete from database</param>
        /// <returns>True if All DataObjects were deleted.</returns>
        public bool DeleteObject(IEnumerable<DataObject> dataObjects)
        {
            var success = true;

            // Vorab eine Liste f�r das Logging von Fehlerobjekten
            var errorLogList = new List<DataObject>();

            foreach (var grp in dataObjects.GroupBy(obj => obj.GetType()))
            {
                var tableHandler = GetTableHandler(grp.Key);

                if (tableHandler == null)
                {
                    if (Log.IsErrorEnabled)
                        Log.ErrorFormat("DeleteObject: DataObject Type ({0}) not registered!", grp.Key.FullName);
                    success = false;
                    continue;
                }

                foreach (var allowed in grp.GroupBy(item => item.AllowDelete))
                {
                    if (allowed.Key)
                    {
                        var objs = allowed.ToArray();
                        var results = DeleteObjectImpl(tableHandler, objs);

                        // Verwenden Sie eine einfache Schleife anstelle von LINQ
                        var successGroups = new Dictionary<bool, List<DataObject>>();

                        var resultsCount = results.Count();
                        for (int i = 0; i < resultsCount; i++)
                        {
                            if (!successGroups.ContainsKey(results.ElementAt(i))) // Verwenden von ElementAt anstelle des direkten Zugriffs
                            {
                                successGroups[results.ElementAt(i)] = new List<DataObject>();
                            }
                            successGroups[results.ElementAt(i)].Add(objs[i]);
                        }

                        foreach (var resultGrp in successGroups)
                        {
                            if (resultGrp.Key)
                            {
                                // Delete in Precache if tablehandler use it
                                if (tableHandler.UsesPreCaching)
                                {
                                    var primary = tableHandler.PrimaryKey;
                                    if (primary != null)
                                    {
                                        foreach (var successObj in resultGrp.Value)
                                            tableHandler.DeletePreCachedObject(primary.GetValue(successObj));
                                    }
                                }

                                // Success Objects Need to check Relations that should be deleted
                                if (tableHandler.HasRelations)
                                    success &= DeleteObjectRelations(tableHandler, resultGrp.Value);
                            }
                            else
                            {
                                // F�gen Sie gescheiterte Objekte zur Fehlerprotokollierung hinzu
                                errorLogList.AddRange(resultGrp.Value);
                                success = false;
                            }
                        }
                    }
                    else
                    {
                        if (Log.IsWarnEnabled)
                        {
                            foreach (var obj in allowed)
                                Log.WarnFormat("DeleteObject: DataObject ({0}) not allowed to be deleted from Database", obj);
                        }
                        success = false;
                    }
                }
            }

            // Loggen Sie alle Fehlerobjekte auf einmal
            if (Log.IsErrorEnabled && errorLogList.Count > 0)
            {
                Log.ErrorFormat("DeleteObject: DataObjects ({0}) could not be deleted from database...", string.Join(", ", errorLogList));
            }

            return success;
        }

        #endregion
        #region Relation Update Handling
        /// <summary>
        /// Save Relations Objects attached to DataObjects
        /// </summary>
        /// <param name="tableHandler">TableHandler for Source DataObjects Relation</param>
        /// <param name="dataObjects">DataObjects to parse</param>
        /// <returns>True if all Relations were saved</returns>
        protected bool SaveObjectRelations(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
        {
            var success = true;
            foreach (var relation in tableHandler.ElementBindings.Where(bind => bind.Relation != null))
            {
                // Relation Check
                var remoteHandler = GetTableHandler(relation.ValueType);
                if (remoteHandler == null)
                {
                    if (Log.IsErrorEnabled)
                        Log.ErrorFormat("SaveObjectRelations: Remote Table for Type ({0}) is not registered !", relation.ValueType.FullName);
                    success = false;
                    continue;
                }

                // Caching the value type to avoid multiple lookups
                var hasElementType = relation.ValueType.HasElementType;

                // Check For Array Type
                var groups = new Dictionary<bool, List<(DataObject Local, DataObject Remote)>>();

                Parallel.ForEach(dataObjects, obj =>
                {
                    if (hasElementType)
                    {
                        var enumerable = (IEnumerable<DataObject>)relation.GetValue(obj);
                        if (enumerable != null)
                        {
                            foreach (var rel in enumerable)
                            {
                                if (rel != null)
                                {
                                    var isPersisted = rel.IsPersisted;
                                    if (!groups.TryGetValue(isPersisted, out var list))
                                    {
                                        list = new List<(DataObject, DataObject)>();
                                        groups[isPersisted] = list;
                                    }
                                    list.Add((obj, rel));
                                }
                            }
                        }
                    }
                    else
                    {
                        var remote = (DataObject)relation.GetValue(obj);
                        if (remote != null)
                        {
                            var isPersisted = remote.IsPersisted;
                            if (!groups.TryGetValue(isPersisted, out var list))
                            {
                                list = new List<(DataObject, DataObject)>();
                                groups[isPersisted] = list;
                            }
                            list.Add((obj, remote));
                        }
                    }
                });

                foreach (var grp in groups)
                {
                    // Group by object that can be added or saved
                    var canAddOrSave = grp.Key; // true if persisted, false if not
                    var allowedGroups = grp.Value.GroupBy(item => canAddOrSave ? item.Remote.Dirty : item.Remote.AllowAdd);

                    foreach (var allowed in allowedGroups)
                    {
                        if (allowed.Key)
                        {
                            var objs = allowed.ToArray();
                            var results = canAddOrSave ? SaveObjectImpl(remoteHandler, objs.Select(item => item.Remote)) :
                                                          AddObjectImpl(remoteHandler, objs.Select(item => item.Remote));

                            foreach (var result in results.Select((result, index) => new { Success = result, RelObject = objs[index] }))
                            {
                                if (result.Success)
                                {
                                    // Update in Precache if tablehandler use it
                                    if (remoteHandler.UsesPreCaching)
                                    {
                                        var primary = remoteHandler.PrimaryKey;
                                        if (primary != null)
                                            remoteHandler.SetPreCachedObject(primary.GetValue(result.RelObject.Remote), result.RelObject.Remote);
                                    }
                                }
                                else
                                {
                                    if (Log.IsErrorEnabled)
                                        Log.ErrorFormat("SaveObjectRelations: {0} Relation ({1}) of DataObject ({2}) failed for Object ({3})",
                                                        canAddOrSave ? "Saving" : "Adding", relation.ValueType, result.RelObject.Local, result.RelObject.Remote);
                                    success = false;
                                }
                            }
                        }
                        else
                        {
                            // Objects that could not be added can lead to failure
                            if (!canAddOrSave)
                            {
                                if (Log.IsWarnEnabled)
                                {
                                    foreach (var obj in allowed)
                                        Log.WarnFormat("SaveObjectRelations: DataObject ({0}) not allowed to be added to Database", obj);
                                }
                                success = false;
                            }
                        }
                    }
                }
            }
            return success;

        }


        /// <summary>
        /// Delete Relations Objects attached to DataObjects
        /// </summary>
        /// <param name="tableHandler">TableHandler for Source DataObjects Relation</param>
        /// <param name="dataObjects">DataObjects to parse</param>
        /// <returns>True if all Relations were deleted</returns>
        public bool DeleteObjectRelations(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
        {
            var success = true;
            foreach (var relation in tableHandler.ElementBindings.Where(bind => bind.Relation != null && bind.Relation.AutoDelete))
            {
                var remoteHandler = GetTableHandler(relation.ValueType);
                if (remoteHandler == null)
                {
                    if (Log.IsErrorEnabled)
                        Log.ErrorFormat("DeleteObjectRelations: Remote Table for Type ({0}) is not registered !", relation.ValueType.FullName);
                    success = false;
                    continue;
                }

                var remoteType = relation.ValueType;
                var isArrayType = remoteType.HasElementType;

                // Umwandlung in eine Liste nach dem Filter
                var groups = isArrayType
                    ? dataObjects
                        .Select(obj => new { obj, Enumerable = (IEnumerable<DataObject>)relation.GetValue(obj) })
                        .Where(obj => obj.Enumerable != null)
                        .SelectMany(obj => obj.Enumerable.Select(rel => new { Local = obj.obj, Remote = rel }))
                        .Where(obj => obj.Remote != null && obj.Remote.IsPersisted)
                        .ToList()
                    : dataObjects
                        .Select(obj => new { Local = obj, Remote = (DataObject)relation.GetValue(obj) })
                        .Where(obj => obj.Remote != null && obj.Remote.IsPersisted)
                        .ToList();

                // Caching des Primary Keys f�r den Remote Handler
                var primary = remoteHandler.PrimaryKey;

                foreach (var grp in groups.GroupBy(obj => obj.Remote.AllowDelete))
                {
                    if (grp.Key)
                    {
                        var objs = grp.ToArray();
                        var results = DeleteObjectImpl(remoteHandler, objs.Select(obj => obj.Remote)).ToArray();

                        var resultsByObjs = results
                            .Select((result, index) => new { Success = result, RelObject = objs[index] })
                            .GroupBy(obj => obj.Success);

                        foreach (var resultGrp in resultsByObjs)
                        {
                            if (resultGrp.Key)
                            {
                                // Delete in Precache if tablehandler use it
                                if (remoteHandler.UsesPreCaching && primary != null)
                                {
                                    foreach (var successObj in resultGrp.Select(obj => obj.RelObject.Remote))
                                        remoteHandler.DeletePreCachedObject(primary.GetValue(successObj));
                                }
                            }
                            else
                            {
                                if (Log.IsErrorEnabled)
                                {
                                    foreach (var result in resultGrp)
                                        Log.ErrorFormat("DeleteObjectRelations: Deleting Relation ({0}) of DataObject ({1}) failed for Object ({2})",
                                                        relation.ValueType, result.RelObject.Local, result.RelObject.Remote);
                                }
                                success = false;
                            }
                        }
                    }
                    else
                    {
                        // Logging f�r nicht l�schbare Objekte
                        if (Log.IsWarnEnabled)
                        {
                            foreach (var obj in grp)
                                Log.WarnFormat("DeleteObjectRelations: DataObject ({0}) not allowed to be deleted from Database", obj);
                        }
                        success = false;
                    }
                }
            }

            return success;

        }
        #endregion
        #region Relation Select/Fill Handling
        /// <summary>
        /// Populate or Refresh Objects Relations
        /// </summary>
        /// <param name="dataObjects">Objects to Populate</param>
        public void FillObjectRelations(IEnumerable<DataObject> dataObjects)
        {
            // Interface Call, force Refresh
            FillObjectRelations(dataObjects, true);
        }

        /// <summary>
        /// Populate or Refresh Object Relations
        /// </summary>
        /// <param name="dataObject">Object to Populate</param>
        public void FillObjectRelations(DataObject dataObject)
        {
            // Interface Call, force Refresh
            FillObjectRelations(new[] { dataObject }, true);
        }

        /// <summary>
        /// Populate or Refresh Objects Relations
        /// </summary>
        /// <param name="dataObjects">Objects to Populate</param>
        /// <param name="force">Force Refresh even if Autoload is False</param>
        protected virtual void FillObjectRelations(IEnumerable<DataObject> dataObjects, bool force)
        {
            var groups = dataObjects.GroupBy(obj => obj.GetType()).AsParallel();

            foreach (var grp in groups)
            {
                var dataType = grp.Key;
                var tableName = AttributesUtils.GetTableOrViewName(dataType);
                try
                {
                    DataTableHandler tableHandler;
                    if (!TableDatasets.TryGetValue(tableName, out tableHandler))
                        throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", tableName));

                    if (!tableHandler.HasRelations)
                        continue;

                    var relations = tableHandler.ElementBindings.Where(bind => bind.Relation != null).ToList();
                    foreach (var relation in relations)
                    {
                        // Check if Loading is needed
                        if (!(relation.Relation.AutoLoad || force))
                            continue;

                        var remoteName = AttributesUtils.GetTableOrViewName(relation.ValueType);
                        try
                        {
                            DataTableHandler remoteHandler;
                            if (!TableDatasets.TryGetValue(remoteName, out remoteHandler))
                                throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", remoteName));

                            // Select Object On Relation Constraint
                            var localBind = tableHandler.FieldElementBindings.Single(bind => bind.ColumnName.Equals(relation.Relation.LocalField, StringComparison.OrdinalIgnoreCase));
                            var remoteBind = remoteHandler.FieldElementBindings.Single(bind => bind.ColumnName.Equals(relation.Relation.RemoteField, StringComparison.OrdinalIgnoreCase));

                            FillObjectRelationsImpl(relation, localBind, remoteBind, remoteHandler, grp);
                        }
                        catch (Exception re)
                        {
                            if (Log.IsErrorEnabled)
                            {
                                var sb = new StringBuilder();
                                sb.AppendFormat("Could not Retrieve Objects from Relation (Table {0}, Local {1}, Remote Table {2}, Remote {3})\n", tableName, relation.Relation.LocalField, AttributesUtils.GetTableOrViewName(relation.ValueType), relation.Relation.RemoteField);
                                sb.Append(re);
                                Log.Error(sb.ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Log.IsErrorEnabled)
                    {
                        var sb = new StringBuilder();
                        sb.AppendFormat("Could not Resolve Relations for Table {0}\n", tableName);
                        sb.Append(e);
                        Log.Error(sb.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// Populate or Refresh Object Relation Implementation
        /// </summary>
        /// <param name="relationBind">Element Binding for Relation Field</param>
        /// <param name="localBind">Local Binding for Value Match</param>
        /// <param name="remoteBind">Remote Binding for Column Match</param>
        /// <param name="remoteHandler">Remote Table Handler for Cache Retrieving</param>
        /// <param name="dataObjects">DataObjects to Populate</param>
        protected virtual void FillObjectRelationsImpl(ElementBinding relationBind, ElementBinding localBind, ElementBinding remoteBind, DataTableHandler remoteHandler, IEnumerable<DataObject> dataObjects)
        {
            var type = relationBind.ValueType;

            var isElementType = false;
            if (type.HasElementType)
            {
                type = type.GetElementType();
                isElementType = true;
            }

            // Initialisiert eine Liste, um die Ergebnisse zu speichern
            var objects = dataObjects.ToArray(); // Beachten Sie, dass dies nur einmalig ben�tigt wird
            var objsResults = new List<IEnumerable<DataObject>>(objects.Length);

            // Handle Cache Search if relevant or use a Select Query
            if (remoteHandler.UsesPreCaching)
            {
                if (remoteHandler.PrimaryKeys.All(pk => pk.ColumnName.Equals(remoteBind.ColumnName, StringComparison.OrdinalIgnoreCase)))
                {
                    // Parallelisieren der Verarbeitung
                    objsResults = objects.AsParallel().Select(obj =>
                    {
                        var local = localBind.GetValue(obj);
                        if (local == null) return Enumerable.Empty<DataObject>();

                        var retrieve = remoteHandler.GetPreCachedObject(local);
                        return retrieve != null ? new[] { retrieve } : Enumerable.Empty<DataObject>();
                    }).ToList(); // Verwendung von ToList um die aufgerufenene IEnumerable zu binden
                }
                else
                {
                    objsResults = objects.AsParallel()
                        .Select(obj => remoteHandler.SearchPreCachedObjects(rem =>
                        {
                            var local = localBind.GetValue(obj);
                            var remote = remoteBind.GetValue(rem);
                            return local != null && remote != null &&
                                   (localBind.ValueType == typeof(string)
                                        ? remote.ToString().Equals(local.ToString(), StringComparison.OrdinalIgnoreCase)
                                        : remote.Equals(local));
                        })).ToList(); // Verwendung von ToList um die aufgerufenene IEnumerable zu binden
                }
            }
            else
            {
                var whereClause = $"`{remoteBind.ColumnName}` = @{remoteBind.ColumnName}";
                var parameters = objects.Select(obj => new[] { new QueryParameter($"@{remoteBind.ColumnName}", localBind.GetValue(obj), localBind.ValueType) }).ToList();

                var list = SelectObjectsImpl(remoteHandler, whereClause, parameters, Transaction.IsolationLevel.DEFAULT);
                objsResults = list.Select(innerList => innerList.Cast<DOL.Database.DataObject>()).ToList();
            }

            var resultByObjs = objsResults.Select((obj, index) => new { DataObject = objects[index], Results = obj }).ToArray();

            // Store Relations
            MethodInfo castMethod = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(type);
            MethodInfo methodToArray = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(type);

            foreach (var result in resultByObjs)
            {
                if (isElementType)
                {
                    if (result.Results.Any())
                    {
                        relationBind.SetValue(result.DataObject, methodToArray.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { result.Results }) }));
                    }
                    else
                    {
                        relationBind.SetValue(result.DataObject, null);
                    }
                }
                else
                {
                    relationBind.SetValue(result.DataObject, result.Results.SingleOrDefault());
                }
            }

            // Fill Sub Relations
            FillObjectRelations(resultByObjs.SelectMany(result => result.Results), false);
        }

        #endregion
        #region Public Object Select with Key API
        /// <summary>
        /// Retrieve a DataObject from database based on its primary key value. 
        /// </summary>
        /// <param name="key">Primary Key Value</param>
        /// <returns>Object found or null if not found</returns>
        public TObject FindObjectByKey<TObject>(object key)
            where TObject : DataObject
        {
            return FindObjectsByKey<TObject>(new[] { key }).FirstOrDefault();
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on their primary key values
        /// </summary>
        /// <param name="keys">Collection of Primary Key Values</param>
        /// <returns>Collection of DataObject with primary key matching values</returns>
        public virtual IList<TObject> FindObjectsByKey<TObject>(IEnumerable<object> keys)
        where TObject : DataObject
        {
            var objectTypeName = typeof(TObject).FullName;
            var tableHandler = GetTableOrViewHandler(typeof(TObject));

            if (tableHandler == null)
            {
                Log.Error($"FindObjectByKey: DataObject Type ({objectTypeName}) not registered !");
                throw new DatabaseException($"Table {objectTypeName} is not registered for Database Connection...");
            }

            if (tableHandler.UsesPreCaching)
            {
                return keys
                    .AsParallel() // Parallel verarbeiten
                    .Select(key => tableHandler.GetPreCachedObject(key))
                    .Where(obj => obj != null)
                    .Cast<TObject>()
                    .ToList();
            }

            var objs = FindObjectByKeyImpl(tableHandler, keys)
                .Where(obj => obj != null)
                .Cast<TObject>()
                .ToList();

            FillObjectRelations(objs, false);

            return objs;
        }



        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on their primary key values
        /// </summary>
        /// <param name="tableHandler">Table Handler for the DataObjects to Retrieve</param>
        /// <param name="keys">Collection of Primary Key Values</param>
        /// <returns>Collection of DataObject with primary key matching values</returns>
        protected abstract IEnumerable<DataObject> FindObjectByKeyImpl(DataTableHandler tableHandler, IEnumerable<object> keys);
        #endregion

        #region Public Object Select API With Parameters
        /// <summary>
        /// Retrieve a Single DataObject from the database based on the Where Expression and Parameters Collection
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="parameters"></param>
        /// <returns>DataObject or null</returns>
        public TObject SelectObject<TObject>(string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters)
            where TObject : DataObject
        {
            IList<TObject> list = SelectObjects<TObject>(whereExpression, parameters).First();
            if (list != null)
                return list.First();
            return null;
        }

        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression and Parameters
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public TObject SelectObject<TObject>(string whereExpression, IEnumerable<QueryParameter> parameter)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, parameter).First();
        }

        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression and Parameter
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public TObject SelectObject<TObject>(string whereExpression, QueryParameter param)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, param).First();
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameters Collection
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameters">Collection of Parameters</param>
        /// <returns>Collection of Objects Sets for each matching Parametrized Query</returns>
        public IList<IList<TObject>> SelectObjects<TObject>(string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters)
    where TObject : DataObject
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(whereExpression))
            {
                throw new ArgumentException("Die Where-Bedingung darf nicht leer sein.", nameof(whereExpression));
            }

            var tableHandler = GetTableOrViewHandler(typeof(TObject));
            if (tableHandler == null)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.ErrorFormat("SelectObjects: DataObject Type ({0}) not registered!", typeof(TObject).FullName);
                }

                throw new DatabaseException($"Tabelle {typeof(TObject).FullName} ist nicht f�r die Datenbankverbindung registriert.");
            }

            var rawObjects = SelectObjectsImpl(tableHandler, whereExpression, parameters, Transaction.IsolationLevel.DEFAULT);

            var objs = new ConcurrentBag<IList<TObject>>();

            Parallel.ForEach(rawObjects, res =>
            {
                var tempList = res.OfType<TObject>().ToList();
                objs.Add(tempList);
            });

            FillObjectRelations(objs.SelectMany(obj => obj), false);
           
            return objs.ToList();
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameter Collection
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameter">Collection of Parameter</param>
        /// <returns>Collection of Objects matching Parametrized Query</returns>
        public IList<TObject> SelectObjects<TObject>(string whereExpression, IEnumerable<QueryParameter> parameter)
            where TObject : DataObject
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            return SelectObjects<TObject>(whereExpression, new[] { parameter }).First();
        }
        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on the Where Expression and Parameter
        /// </summary>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="param">Single Parameter</param>
        /// <returns>Collection of Objects matching Parametrized Query</returns>
        public IList<TObject> SelectObjects<TObject>(string whereExpression, QueryParameter param)
            where TObject : DataObject
        {
            if (param == null)
                throw new ArgumentNullException("param");

            return SelectObjects<TObject>(whereExpression, new[] { new[] { param } }).First();
        }
        #endregion

        #region Public Object Select API Without Parameters
        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <returns>Single Object or First Object if multiple matches</returns>
        public TObject SelectObject<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return SelectObject<TObject>(whereExpression, Transaction.IsolationLevel.DEFAULT);
        }

        /// <summary>
        /// Retrieve a Single DataObject from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Single Object or First Object if multiple matches</returns>
        public TObject SelectObject<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, isolation).FirstOrDefault();
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <returns>Collection of DataObjects matching filter</returns>
        public IList<TObject> SelectObjects<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, Transaction.IsolationLevel.DEFAULT);
        }

        /// <summary>
        /// Retrieve a Collection of DataObjects from database based on Where Expression
        /// </summary>
        /// <param name="whereExpression">Where Expression Filter</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of DataObjects matching filter</returns>
        public IList<TObject> SelectObjects<TObject>(string whereExpression, Transaction.IsolationLevel isolation)
            where TObject : DataObject
        {
            return SelectObjects<TObject>(whereExpression, new[] { new QueryParameter[] { } }).First().ToArray();
        }
        #endregion

        #region Public Object Select All API
        /// <summary>
        /// Select all Objects From Table holding TObject Type
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Select</typeparam>
        /// <returns>Collection of all DataObject for this Type</returns>
        public IList<TObject> SelectAllObjects<TObject>()
            where TObject : DataObject
        {
            return SelectAllObjects<TObject>(Transaction.IsolationLevel.DEFAULT);
        }
        /// <summary>
        /// Select all Objects From Table holding TObject Type
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Select</typeparam>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of all DataObject for this Type</returns>
        public IList<TObject> SelectAllObjects<TObject>(Transaction.IsolationLevel isolation)
    where TObject : DataObject
        {
            var tableHandler = GetTableOrViewHandler(typeof(TObject));
            if (tableHandler == null)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("SelectAllObjects: DataObject Type ({0}) not registered!", typeof(TObject).FullName);

                throw new DatabaseException($"Table {typeof(TObject).FullName} is not registered for Database Connection...");
            }

            // Cache verwenden, pr�fen, ob der Cache aktiv ist
            if (tableHandler.UsesPreCaching)
            {
                return tableHandler.SearchPreCachedObjects(obj => obj != null).Cast<TObject>().ToArray();
            }

            // Datenobjekte abrufen
            var dataObjects = SelectObjectsImpl(tableHandler, null, new[] { new QueryParameter[] { } }, isolation).Single();

            // Parallelisierung der Verarbeitung der Datenobjekte
            var parallelDataObjects = dataObjects.AsParallel().ToArray();

            // F�llen der Objektbeziehungen nur bei Bedarf
            FillObjectRelations(parallelDataObjects, false);

            return parallelDataObjects.Cast<TObject>().ToArray();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Gets the number of objects in a given table in the database.
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <returns>a positive integer representing the number of objects; zero if no object exists</returns>
        public int GetObjectCount<TObject>()
            where TObject : DataObject
        {
            return GetObjectCount<TObject>(string.Empty);
        }

        /// <summary>
        /// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <param name="whereExpression">the where clause to filter object count on</param>
        /// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
        public int GetObjectCount<TObject>(string whereExpression)
            where TObject : DataObject
        {
            return GetObjectCountImpl<TObject>(whereExpression);
        }

        /// <summary>
        /// Register Data Object Type if not already Registered
        /// </summary>
        /// <param name="dataObjectType">DataObject Type</param>
        public virtual void RegisterDataObject(Type dataObjectType)
        {
            var tableName = AttributesUtils.GetTableOrViewName(dataObjectType);
            if (TableDatasets.ContainsKey(tableName))
                return;

            var dataTableHandler = new DataTableHandler(dataObjectType);
            TableDatasets.Add(tableName, dataTableHandler);
        }

        /// <summary>
        /// escape the strange character from string
        /// </summary>
        /// <param name="rawInput">the string</param>
        /// <returns>the string with escaped character</returns>
        public abstract string Escape(string rawInput);

        /// <summary>
        /// Execute a Raw Non-Query on the Database
        /// </summary>
        /// <param name="rawQuery">Raw Command</param>
        /// <returns>True if the Command succeeded</returns>
        public virtual bool ExecuteNonQuery(string rawQuery)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation
        /// <summary>
        /// Adds new DataObjects to the database.
        /// </summary>
        /// <param name="dataObjects">DataObjects to add to the database</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were added successfully; false otherwise</returns>
        protected abstract IEnumerable<bool> AddObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Saves Persisted DataObjects into Database
        /// </summary>
        /// <param name="dataObjects">DataObjects to Save</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were saved successfully; false otherwise</returns>
        protected abstract IEnumerable<bool> SaveObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Deletes DataObjects from the database.
        /// </summary>
        /// <param name="dataObjects">DataObjects to delete from the database</param>
        /// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
        /// <returns>True if objects were deleted successfully; false otherwise</returns>
        protected abstract IEnumerable<bool> DeleteObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Retrieve a Collection of DataObjects Sets from database filtered by Parametrized Where Expression
        /// </summary>
        /// <param name="tableHandler">Table Handler for these DataObjects</param>
        /// <param name="whereExpression">Parametrized Where Expression</param>
        /// <param name="parameters">Parameters for filtering</param>
        /// <param name="isolation">Isolation Level</param>
        /// <returns>Collection of DataObjects Sets matching Parametrized Where Expression</returns>
        protected abstract IList<IList<DataObject>> SelectObjectsImpl(DataTableHandler tableHandler, string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters, Transaction.IsolationLevel isolation);

        /// <summary>
        /// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
        /// </summary>
        /// <typeparam name="TObject">the type of objects to retrieve</typeparam>
        /// <param name="whereExpression">the where clause to filter object count on</param>
        /// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
        protected abstract int GetObjectCountImpl<TObject>(string whereExpression)
            where TObject : DataObject;
        #endregion

        #region Cache
        /// <summary>
        /// Selects object from the database and updates or adds entry in the pre-cache.
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Query</typeparam>
        /// <param name="key">Key to Update</param>
        /// <returns>True if Object was found with given key</returns>
        public bool UpdateInCache<TObject>(object key)
            where TObject : DataObject
        {
            return UpdateObjsInCache<TObject>(new[] { key });
        }

        /// <summary>
        /// Selects objects from the database and updates or adds entries in the pre-cache.
        /// </summary>
        /// <typeparam name="TObject">DataObject Type to Query</typeparam>
        /// <param name="keys">Key Collection to Update</param>
        /// <returns>True if All Objects were found with given keys</returns>
        public bool UpdateObjsInCache<TObject>(IEnumerable<object> keys)
        where TObject : DataObject
        {
            var tableHandler = GetTableOrViewHandler(typeof(TObject));
            if (tableHandler == null)
            {
                if (Log.IsErrorEnabled)
                    Log.ErrorFormat("UpdateInCache: DataObject Type ({0}) not registered !", typeof(TObject).FullName);

                throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", typeof(TObject).FullName));
            }

            var keysArray = keys.ToArray();
            var objs = FindObjectByKeyImpl(tableHandler, keysArray);

            var success = true;
            var objsList = objs.ToList(); // Konvertiere IEnumerable in eine Liste
            Parallel.For(0, objsList.Count, i =>
            {
                if (objsList[i] != null)
                    tableHandler.SetPreCachedObject(keysArray[i], objsList[i]);
                else
                    success = false; // achten Sie darauf, dies ordnungsgem�� zu synchronisieren
            });

            return success;
        }


        #endregion

        #region Factory

        public static IObjectDatabase GetObjectDatabase(ConnectionType connectionType, string connectionString)
        {
            if (connectionType == ConnectionType.DATABASE_MYSQL)
                return new MySQLObjectDatabase(connectionString);
            if (connectionType == ConnectionType.DATABASE_SQLITE)
                return new SQLiteObjectDatabase(connectionString);

            return null;
        }

        #endregion
    }
}
