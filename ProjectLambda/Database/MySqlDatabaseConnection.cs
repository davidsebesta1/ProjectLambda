using MySql.Data.MySqlClient;
using ProjectLambda.Logging;
using System.Data;
using System.Data.Common;

namespace ProjectLambda.Database
{
    /// <summary>
    /// A class to connect o to MySql DB.
    /// </summary>
    public class MySqlDatabaseConnection
    {
        public string ConnectionString { get; private set; }

        private static MySqlDatabaseConnection _instance;

        public static string IsolationLevelSetQuery => "SET TRANSACTION ISOLATION LEVEL ";

        /// <summary>
        /// Gets the singleton instance. Initializes a new if it isn't already.
        /// </summary>
        public static MySqlDatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MySqlDatabaseConnection(Program.Config.DatabaseIP, Program.Config.InitialDatabase, Program.Config.LoginUser, Program.Config.LoginPassword);
                    InitializeSetup();
                }

                return _instance;
            }
        }

        private MySqlDatabaseConnection(string ip, string databaseName, string UID, string pw)
        {
            ConnectionString = $"server='{ip}';database='{databaseName}';user='{UID}';password='{pw}';Allow User Variables=true;";
        }

        private static async void InitializeSetup()
        {
            try
            {
                if (_instance == null)
                {
                    throw new Exception("Instance for database connection is null");
                }
            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
            }
        }

        #region Queries Methods

        /// <summary>
        /// Executes the query that doesn't return a table asynchronously.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">Parameters of the query.</param>
        /// <returns>Result from the RDBMS.</returns>
        public async Task<int> ExecuteNonQueryAsync(string query, params MySqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(query))
                return 0;

            try
            {
                await using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    await using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        };

                        return await command.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
            }

            return 0;
        }

        /// <summary>
        /// Executes the query that returns a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">Parameters of the query.</param>
        /// <returns>Result from the RDBMS.</returns>
        public async Task<DataTable?> ExecuteQueryAsync(string query, params MySqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(query))
                return null;

            try
            {
                await using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    await using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        DbDataReader reader = await command.ExecuteReaderAsync();
                        DataTable table = new DataTable();
                        table.Load(reader);

                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Executes the query that return a number.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">Parameters of the query.</param>
        /// <returns>Result from the RDBMS.</returns>
        public async Task<int> ExecuteScalarIntAsync(string query, params MySqlParameter[] parameters)
        {
            await Console.Out.WriteLineAsync(query);
            if (string.IsNullOrEmpty(query))
                return 0;

            try
            {
                await using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    await using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        object? result = await command.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int intValue))
                        {
                            return intValue;
                        }

                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
            }

            return 0;
        }

        /// <summary>
        /// Executes the query that returns a decimal value.
        /// </summary>
        /// <param name="query">The SQL query.</param>
        /// <param name="parameters">Parameters of the query.</param>
        /// <returns>Result from the RDBMS as a decimal.</returns>
        public async Task<decimal> ExecuteScalarDecimalAsync(string query, params MySqlParameter[] parameters)
        {
            await Console.Out.WriteLineAsync(query);
            if (string.IsNullOrEmpty(query))
                return 0;

            try
            {
                await using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    await using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        object? result = await command.ExecuteScalarAsync();

                        if (result != null && decimal.TryParse(result.ToString(), out decimal decimalValue))
                        {
                            return decimalValue;
                        }

                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
            }

            return 0;
        }

        /// <summary>
        /// Executes the transaction multiple queries.
        /// </summary>
        /// <param name="queries">Query and its params.</param>
        /// <param name="level">Transaction isolation level to be applied.</param>
        /// <returns>Whether the transaction has been commited or rollback.</returns>
        public async Task<bool> ExecuteTransactionAsync(List<(string, MySqlParameter[])> queries, MySqlTransaction transaction)
        {
            MySqlConnection connection = null;
            if (transaction != null)
            {
                connection = transaction.Connection;
            }

            connection ??= new MySqlConnection(ConnectionString);

            try
            {
                foreach (var kvp in queries)
                {
                    await using (MySqlCommand command = new MySqlCommand(kvp.Item1, connection))
                    {
                        if (kvp.Item2 != null)
                        {
                            command.Parameters.AddRange(kvp.Item2);
                        };

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                await Logger.LogError(ex);
                await transaction.RollbackAsync();
            }


            await transaction.DisposeAsync();

            return false;
        }



        /// <summary>
        /// Executes the transaction multiple queries.
        /// </summary>
        /// <param name="queries">Query and its params.</param>
        /// <param name="level">Transaction isolation level to be applied.</param>
        /// <returns>Whether the transaction has been commited or rollback.</returns>
        public async Task<bool> ExecuteTransactionAsync(List<(string, MySqlParameter[])> queries, IsolationLevels level = IsolationLevels.REPEATABLE_READ)
        {
            MySqlTransaction transaction = null;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    await using (MySqlCommand command = new MySqlCommand(IsolationLevelSetQuery + level.ToString().Replace('_', ' '), connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    transaction = connection.BeginTransaction();

                    foreach (var kvp in queries)
                    {
                        await using (MySqlCommand command = new MySqlCommand(kvp.Item1, connection))
                        {
                            if (kvp.Item2 != null)
                            {
                                command.Parameters.AddRange(kvp.Item2);
                            };

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return true;

                }
                catch (Exception ex)
                {
                    await Logger.LogError(ex);
                    await transaction.RollbackAsync();
                }
            }

            await transaction.DisposeAsync();

            return false;
        }

        #endregion
    }
}