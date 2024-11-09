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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MySql.Data.MySqlClient;

namespace DOL.Database.Connection
{
	/// <summary>
	/// Called after mysql query.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public delegate void QueryCallback(MySqlDataReader reader);

	/// <summary>
	/// Class for Handling the Connection to the ADO.Net Layer of the Databases.
	/// Funktions for loading and storing the complete Dataset are in there.
	/// </summary>
	public class DataConnection
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Queue<MySqlConnection> m_connectionPool = new Queue<MySqlConnection>();

		private string connString;
		private ConnectionType connType;

		/// <summary>
		/// Constructor to set up a Database
		/// </summary>
		/// <param name="connType">Connection-Type the Database should use</param>
		/// <param name="connString">Connection-String to indicate the Parameters of the Datasource.
		///     XML = Directory where the XML-Files should be stored
		///     MYSQL = ADO.NET ConnectionString 
		///     MSSQL = ADO.NET ConnectionString 
		///     OLEDB = ADO.NET ConnectionString 
		///     ODBC = ADO.NET ConnectionString 
		/// </param>
		public DataConnection(ConnectionType connType, string connString)
		{
			this.connType = connType;

			//if Directory has no trailing \ than append it ;-)
			if (connType == ConnectionType.DATABASE_XML)
			{
				if (connString[connString.Length - 1] != Path.DirectorySeparatorChar)
					this.connString = connString + Path.DirectorySeparatorChar;

				if (!Directory.Exists(connString))
				{
					try
					{
						Directory.CreateDirectory(connString);
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				// Options of MySQL connection string
				if (!connString.Contains("Treat Tiny As Boolean"))
				{
					connString += ";Treat Tiny As Boolean=False";
				}

				this.connString = connString;
			}
		}

		/// <summary>
		/// Check if SQL connection
		/// </summary>
		public bool IsSQLConnection
		{
			get { return connType == ConnectionType.DATABASE_MYSQL; }
		}

		/// <summary>
		/// The connection type to DB (xml, mysql,...)
		/// </summary>
		public ConnectionType ConnectionType
		{
			get { return connType; }
		}

        /// <summary>
        /// escape the strange character from string
        /// </summary>
        /// <param name="s">the string</param>
        /// <returns>the string with escaped character</returns>
        public string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var sb = new StringBuilder(s.Length);

            if (!IsSQLConnection)
            {
                foreach (char c in s)
                {
                    if (c == '\'')
                        sb.Append("''");
                    else
                        sb.Append(c);
                }
            }
            else
            {
                foreach (char c in s)
                {
                    switch (c)
                    {
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '\"':
                            sb.Append("\\\"");
                            break;
                        case '\'':
                            sb.Append("\\'");
                            break;
                        case '’':
                            sb.Append("\\’");
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Gets connection from connection pool.
        /// </summary>
        /// <param name="isNewConnection">Set to <code>true</code> if new connection is created.</param>
        /// <returns>Connection.</returns>
        private MySqlConnection GetMySqlConnection(out bool isNewConnection)
        {
            MySqlConnection conn = null;
            // Überprüfen, ob eine Verbindung aus dem Pool verfügbar ist
            if (m_connectionPool.Count > 0)
            {
                conn = m_connectionPool.Dequeue();
                isNewConnection = false;
            }
            else
            {
                isNewConnection = true;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                conn = new MySqlConnection(connString);
                conn.Open();
                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Gaining SQL connection took " + stopwatch.ElapsedMilliseconds + "ms");
                }

                log.Info("New DB connection created");
            }

            return conn;
        }



        /// <summary>
        /// Releases the connection to connection pool.
        /// </summary>
        /// <param name="conn">The connection to relase.</param>
        private void ReleaseConnection(MySqlConnection conn)
		{
            lock (((ICollection)m_connectionPool).SyncRoot)
			{
				m_connectionPool.Enqueue(conn);
			}
		}
        /// <summary>
        /// Execute a non query like update or delete
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <returns>number of rows affected</returns>
        public async Task<int> ExecuteNonQueryAsync(string sqlcommand)
        {
            if (connType == ConnectionType.DATABASE_MYSQL)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("SQL: " + sqlcommand);
                }

                int affected = 0;

                using (var conn = new MySqlConnection(connString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = sqlcommand;
                            await conn.OpenAsync();
                            long start = (DateTime.UtcNow.Ticks / 10000);
                            affected = await cmd.ExecuteNonQueryAsync();

                            if (log.IsDebugEnabled)
                                log.Debug("SQL NonQuery exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
                            else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
                                log.Warn("SQL NonQuery took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
                        }
                        catch (Exception e)
                        {
                            if (!HandleException(e))
                            {
                                throw;
                            }
                        }
                    }
                }

                return affected;
            }

            if (log.IsWarnEnabled)
                log.Warn("SQL NonQuery's not supported for this connection type");

            return 0;
        }

       
        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns><code>true</code> if operation should be repeated, <code>false</code> otherwise.</returns>
        private static bool HandleException(Exception e)
        {
            bool ret = false;
            var innerException = e.InnerException;

            if (innerException != null)
            {
                SocketException socketException = innerException.InnerException as SocketException ?? innerException as SocketException;

                if (socketException != null)
                {
                    // Handle socket exception. Error codes:
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

                    log.WarnFormat("Socket exception: ({0}) {1}; repeat: {2}", socketException.ErrorCode, socketException.Message, ret);
                }
            }

            return ret;
        }


        /// <summary>
        /// Execute select on sql database
        /// Close returned datareader when done or use using(reader) { ... }
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <param name="callback"></param>
        public async Task ExecuteSelectAsync(string sqlcommand, QueryCallback callback, Transaction.IsolationLevel isolation)
        {
            if (connType == ConnectionType.DATABASE_MYSQL)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("SQL: " + sqlcommand);
                }

                bool repeat = false;

                do
                {
                    try
                    {
                        using (var conn = new MySqlConnection(connString))
                        {
                            await conn.OpenAsync().ConfigureAwait(false);
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = sqlcommand;
                                long start = DateTime.UtcNow.Ticks / 10000;

                                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                                {
                                    callback((MySqlDataReader)reader);
                                }

                                long executionTime = (DateTime.UtcNow.Ticks / 10000) - start;
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug("SQL Select (" + isolation + ") exec time " + executionTime + "ms");
                                }
                                else if (executionTime > 500 && log.IsWarnEnabled)
                                {
                                    log.Warn("SQL Select (" + isolation + ") took " + executionTime + "ms!\n" + sqlcommand);
                                }
                            }
                        }

                        repeat = false;
                    }
                    catch (Exception e)
                    {
                        if (!HandleException(e))
                        {
                            if (log.IsErrorEnabled)
                                log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
                            throw;
                        }

                        repeat = true;
                    }
                } while (repeat);

                return;
            }

            if (log.IsWarnEnabled)
                log.Warn("SQL Selects not supported for this connection type");
        }


        /// <summary>
        /// Execute scalar on sql database
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sqlcommand)
        {
            if (connType == ConnectionType.DATABASE_MYSQL)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("SQL: " + sqlcommand);
                }

                object obj = null;

                try
                {
                    using (var conn = new MySqlConnection(connString))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            cmd.CommandText = sqlcommand;
                            long start = DateTime.UtcNow.Ticks / 10000;
                            obj = cmd.ExecuteScalar();
                            long executionTime = (DateTime.UtcNow.Ticks / 10000) - start;

                            if (log.IsDebugEnabled)
                                log.Debug("SQL Select exec time " + executionTime + "ms");
                            else if (executionTime > 500 && log.IsWarnEnabled)
                                log.Warn("SQL Select took " + executionTime + "ms!\n" + sqlcommand);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!HandleException(e))
                    {
                        if (log.IsErrorEnabled)
                            log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
                        throw;
                    }
                }

                return obj;
            }

            if (log.IsWarnEnabled)
                log.Warn("SQL Scalar not supported for this connection type");

            return null;
        }


        /// <summary>
        /// Create the table in mysql
        /// </summary>
        /// <param name="table">the table to create</param>
        public async Task CheckOrCreateTable(DataTable table)
        {
            if (connType == ConnectionType.DATABASE_MYSQL)
            {
                var currentTableColumns = new List<string>();
                try
                {
                    await ExecuteSelectAsync("DESCRIBE `" + table.TableName + "`", delegate (MySqlDataReader reader)
                    {
                        while (reader.Read())
                        {
                            currentTableColumns.Add(reader.GetString(0).ToLower());
                            log.Debug(reader.GetString(0).ToLower());
                        }
                        if (log.IsDebugEnabled)
                            log.Debug(currentTableColumns.Count + " in table");
                    }, Transaction.IsolationLevel.DEFAULT);
                }
                catch (Exception e)
                {
                    if (log.IsDebugEnabled)
                        log.Debug(e.ToString());

                    if (log.IsWarnEnabled)
                        log.Warn("Table " + table.TableName + " doesn't exist, creating it...");
                }

                var sb = new StringBuilder();
                var primaryKeys = new Dictionary<string, DataColumn>();
                foreach (var key in table.PrimaryKey)
                {
                    primaryKeys[key.ColumnName] = key;
                }

                var columnDefs = new List<string>();
                var alterAddColumnDefs = new List<string>();
                var sqlTypes = new Dictionary<Type, string>
        {
            { typeof(char), "SMALLINT UNSIGNED" },
            { typeof(DateTime), "DATETIME DEFAULT '2000-01-01'" },
            { typeof(sbyte), "TINYINT" },
            { typeof(short), "SMALLINT" },
            { typeof(int), "INT" },
            { typeof(long), "BIGINT" },
            { typeof(byte), "TINYINT UNSIGNED" },
            { typeof(ushort), "SMALLINT UNSIGNED" },
            { typeof(uint), "INT UNSIGNED" },
            { typeof(ulong), "BIGINT UNSIGNED" },
            { typeof(float), "FLOAT" },
            { typeof(double), "DOUBLE" },
            { typeof(bool), "TINYINT(1)" }
        };

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Type systype = table.Columns[i].DataType;

                    string column = "`" + table.Columns[i].ColumnName + "` ";
                    column += sqlTypes.ContainsKey(systype) ? sqlTypes[systype] : "BLOB";

                    if (!table.Columns[i].AllowDBNull)
                    {
                        column += " NOT NULL";
                    }
                    if (table.Columns[i].AutoIncrement)
                    {
                        column += " AUTO_INCREMENT";
                    }

                    columnDefs.Add(column);

                    // If the column doesn't exist but the table does, then alter table
                    if (currentTableColumns.Count > 0 && !currentTableColumns.Contains(table.Columns[i].ColumnName.ToLower()))
                    {
                        log.Debug("added for alteration " + table.Columns[i].ColumnName.ToLower());
                        alterAddColumnDefs.Add(column);
                    }
                }

                string columndef = string.Join(", ", columnDefs);

                // Create primary keys
                if (table.PrimaryKey.Length > 0)
                {
                    columndef += ", PRIMARY KEY (";
                    columndef += string.Join(", ", table.PrimaryKey.Select(pk => "`" + pk.ColumnName + "`"));
                    columndef += ")";
                }

                // Unique indexes
                foreach (var column in table.Columns.Cast<DataColumn>())
                {
                    if (column.Unique && !primaryKeys.ContainsKey(column.ColumnName))
                    {
                        columndef += ", UNIQUE INDEX (`" + column.ColumnName + "`)";
                    }
                }

                // Indexes
                foreach (var column in table.Columns.Cast<DataColumn>())
                {
                    if (column.ExtendedProperties.ContainsKey("INDEX")
                        && !primaryKeys.ContainsKey(column.ColumnName)
                        && !column.Unique)
                    {
                        columndef += ", INDEX (`" + column.ColumnName + "`";

                        if (column.ExtendedProperties.ContainsKey("INDEXCOLUMNS"))
                        {
                            columndef += ", " + column.ExtendedProperties["INDEXCOLUMNS"];
                        }

                        columndef += ")";
                    }
                }

                sb.Append("CREATE TABLE IF NOT EXISTS `" + table.TableName + "` (" + columndef + ")");

                try
                {
                    if (log.IsDebugEnabled)
                        log.Debug(sb.ToString());

                    await ExecuteNonQueryAsync(sb.ToString());
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Error while creating table " + table.TableName, e);
                }

                // Alter table if needed
                if (alterAddColumnDefs.Count > 0)
                {
                    columndef = string.Join(", ", alterAddColumnDefs);
                    string alterTable = "ALTER TABLE `" + table.TableName + "` ADD (" + columndef + ")";
                    try
                    {
                        log.Warn("Altering table " + table.TableName);
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(alterTable);
                        }
                        await ExecuteNonQueryAsync(alterTable);
                    }
                    catch (Exception e)
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Error while altering table " + table.TableName, e);
                    }
                }
            }
        }


        /// <summary>
        /// Gets the format for date times
        /// </summary>
        /// <returns></returns>
        public string GetDBDateFormat()
		{
			switch (connType)
			{
				case ConnectionType.DATABASE_MYSQL:
					return "yyyy-MM-dd HH:mm:ss";
			}

			return "yyyy-MM-dd HH:mm:ss";
		}

        /// <summary>
        /// Load an Dataset with the a Table
        /// </summary>
        /// <param name="tableName">Name of the Table to Load in the DataSet</param>
        /// <param name="dataSet">DataSet that sould be filled</param>
        /// <exception cref="DatabaseException"></exception>
        public void LoadDataSet(string tableName, DataSet dataSet)
        {
            dataSet.Clear();
            try
            {
                switch (connType)
                {
                    case ConnectionType.DATABASE_MSSQL:
                        using (var conn = new SqlConnection(connString))
                        using (var adapter = new SqlDataAdapter("SELECT * from " + tableName, conn))
                        {
                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        break;

                    case ConnectionType.DATABASE_ODBC:
                        using (var conn = new OdbcConnection(connString))
                        using (var adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn))
                        {
                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        break;

                    case ConnectionType.DATABASE_OLEDB:
                        using (var conn = new OleDbConnection(connString))
                        using (var adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn))
                        {
                            adapter.Fill(dataSet.Tables[tableName]);
                        }
                        break;

                    case ConnectionType.DATABASE_XML:
                        dataSet.Tables[tableName].ReadXml(connString + tableName + ".xml");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Could not load the Database-Table", ex);
            }
        }


        /// <summary>
        /// Writes all Changes in a Dataset to the Table
        /// </summary>
        /// <param name="tableName">Name of the Table to update</param>
        /// <param name="dataSet">DataSet set contains the Changes that sould be written</param>
        /// <exception cref="DatabaseException"></exception>
        public void SaveDataSet(string tableName, DataSet dataSet)
        {
            if (!dataSet.HasChanges())
                return;

            switch (connType)
            {
                case ConnectionType.DATABASE_XML:
                    {
                        try
                        {
                            dataSet.WriteXml($"{connString}{tableName}.xml");
                            dataSet.AcceptChanges();
                            dataSet.WriteXmlSchema($"{connString}{tableName}.xsd");
                        }
                        catch (Exception e)
                        {
                            throw new DatabaseException("Could not save Databases in XML-Files!", e);
                        }
                        break;
                    }
                case ConnectionType.DATABASE_MSSQL:
                    {
                        try
                        {
                            using (var conn = new SqlConnection(connString))
                            {
                                var adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", conn);
                                var builder = new SqlCommandBuilder(adapter);

                                adapter.DeleteCommand = builder.GetDeleteCommand();
                                adapter.UpdateCommand = builder.GetUpdateCommand();
                                adapter.InsertCommand = builder.GetInsertCommand();

                                DataSet changes;
                                adapter.ContinueUpdateOnError = true;

                                // Lock dataset only if there are concurrent accesses.
                                if (Monitor.TryEnter(dataSet))
                                {
                                    try
                                    {
                                        changes = dataSet.GetChanges();
                                        adapter.Update(changes, tableName);
                                        PrintDatasetErrors(changes);
                                        dataSet.AcceptChanges();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(dataSet);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Could not save the Table " + tableName, ex);
                        }
                        break;
                    }
                case ConnectionType.DATABASE_ODBC:
                    {
                        try
                        {
                            using (var conn = new OdbcConnection(connString))
                            {
                                var adapter = new OdbcDataAdapter($"SELECT * FROM {tableName}", conn);
                                var builder = new OdbcCommandBuilder(adapter);

                                adapter.DeleteCommand = builder.GetDeleteCommand();
                                adapter.UpdateCommand = builder.GetUpdateCommand();
                                adapter.InsertCommand = builder.GetInsertCommand();

                                DataSet changes;
                                adapter.ContinueUpdateOnError = true;

                                if (Monitor.TryEnter(dataSet))
                                {
                                    try
                                    {
                                        changes = dataSet.GetChanges();
                                        adapter.Update(changes, tableName);
                                        dataSet.AcceptChanges();
                                        PrintDatasetErrors(changes);
                                    }
                                    finally
                                    {
                                        Monitor.Exit(dataSet);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Could not save the Database-Table", ex);
                        }
                        break;
                    }
                case ConnectionType.DATABASE_MYSQL:
                    {
                        return; // not needed anymore
                    }
                case ConnectionType.DATABASE_OLEDB:
                    {
                        try
                        {
                            using (var conn = new OleDbConnection(connString))
                            {
                                var adapter = new OleDbDataAdapter($"SELECT * FROM {tableName}", conn);
                                var builder = new OleDbCommandBuilder(adapter);

                                adapter.DeleteCommand = builder.GetDeleteCommand();
                                adapter.UpdateCommand = builder.GetUpdateCommand();
                                adapter.InsertCommand = builder.GetInsertCommand();

                                DataSet changes;
                                adapter.ContinueUpdateOnError = true;

                                if (Monitor.TryEnter(dataSet))
                                {
                                    try
                                    {
                                        changes = dataSet.GetChanges();
                                        adapter.Update(changes, tableName);
                                        PrintDatasetErrors(changes);
                                        dataSet.AcceptChanges();
                                    }
                                    finally
                                    {
                                        Monitor.Exit(dataSet);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new DatabaseException("Could not save the Database-Table", ex);
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// Print the dataset error
        /// </summary>
        /// <param name="dataset">the dataset to check</param>
        public void PrintDatasetErrors(DataSet dataset)
        {
            if (!dataset.HasErrors) return;

            foreach (DataTable table in dataset.Tables)
            {
                if (!table.HasErrors) continue;

                foreach (DataRow row in table.Rows)
                {
                    if (!row.HasErrors) continue;

                    var sb = new StringBuilder();
                    string errorMessage;

                    if (row.RowState == DataRowState.Deleted)
                    {
                        if (log.IsErrorEnabled)
                        {
                            errorMessage = $"Error deleting row in table {table.TableName}: {row.RowError}";
                            sb.Append(errorMessage);
                            foreach (DataColumn col in table.Columns)
                            {
                                sb.Append($"{col.ColumnName}={row[col, DataRowVersion.Original]} ");
                            }
                            log.Error(sb.ToString());
                        }
                    }
                    else
                    {
                        if (log.IsErrorEnabled)
                        {
                            errorMessage = $"Error updating table {table.TableName}: {row.RowError} {row.GetColumnsInError()}";
                            sb.Append(errorMessage);
                            foreach (DataColumn col in table.Columns)
                            {
                                sb.Append($"{col.ColumnName}={row[col]} ");
                            }
                            log.Error(sb.ToString());

                            sb.Clear();
                            sb.Append("Affected columns: ");
                            foreach (DataColumn col in row.GetColumnsInError())
                            {
                                sb.Append($"{col.ColumnName} ");
                            }
                            log.Error(sb.ToString());
                        }
                    }
                }
            }
        }

    }
}