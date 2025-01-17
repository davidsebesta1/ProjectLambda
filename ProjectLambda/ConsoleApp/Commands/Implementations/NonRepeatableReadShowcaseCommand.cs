using MySql.Data.MySqlClient;
using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Logging;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class NonRepeatableReadShowcaseCommand : ICommand
    {
        public string Command => "NonRepShowcase";

        public string Description => "Showcases the phantom read on a report example.";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            Logger.Log("Showcasing non-repeatable read:");
            Logger.Log($"Isolation level of the transactions is set to {Program.Config.IsolationLevel}");

            ShowcaseNonRepeatableRead().Wait();
            response = "Non-repeatable read showcase completed. Check logs for details.";
            return true;
        }

        private async Task ShowcaseNonRepeatableRead()
        {
            int userId = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync(
                "SELECT ID FROM User WHERE Username = @Username;",
                new MySqlParameter("@Username", "admin")
            );

            decimal initialCredit;

            Logger.Log("Transaction A: Starting...");
            await using (var connectionA = new MySqlConnection(MySqlDatabaseConnection.Instance.ConnectionString))
            {
                await connectionA.OpenAsync();

                await using (MySqlTransaction transactionA = await connectionA.BeginTransactionAsync())
                {
                    await MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(new List<(string, MySqlParameter[])>(), transactionA);

                    Logger.Log("Transaction A: Reading initial credit...");

                    initialCredit = await MySqlDatabaseConnection.Instance.ExecuteScalarDecimalAsync("SELECT Credit FROM User WHERE ID = @UserID;", new MySqlParameter("@UserID", userId));

                    Logger.Log($"Transaction A: Initial credit read: {initialCredit}");

                    Logger.Log("Transaction A: Sleeping for 5 seconds to simulate processing...");
                    await MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(new List<(string, MySqlParameter[])>
                    {
                        ("DO SLEEP(5);", new MySqlParameter[] { })
                    }, transactionA);

                    Logger.Log("Transaction B: Starting...");
                    await using (MySqlConnection connectionB = new MySqlConnection(MySqlDatabaseConnection.Instance.ConnectionString))
                    {
                        await connectionB.OpenAsync();

                        await using (MySqlTransaction transactionB = await connectionB.BeginTransactionAsync())
                        {
                            Logger.Log("Transaction B: Updating credit...");
                            await MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(new List<(string, MySqlParameter[])>
                            {
                                ("UPDATE User SET Credit = @NewCredit WHERE ID = @UserID;",
                                    new MySqlParameter[]
                                    {
                                        new MySqlParameter("@NewCredit", 500.00m),
                                        new MySqlParameter("@UserID", userId)
                                    })
                                }, transactionB);

                            await transactionB.CommitAsync();
                            Logger.Log("Transaction B: Credit updated.");
                        }
                    }

                    Logger.Log("Transaction A: Re-reading credit after Transaction B...");
                    decimal finalCredit = await MySqlDatabaseConnection.Instance.ExecuteScalarDecimalAsync("SELECT Credit FROM User WHERE ID = @UserID;", new MySqlParameter("@UserID", userId));

                    Logger.Log($"Transaction A: Final credit read: {finalCredit}");

                    if (initialCredit != finalCredit)
                    {
                        Logger.Log("Non-repeatable read observed: Credit changed during Transaction A.");
                    }
                    else
                    {
                        Logger.Log("No non-repeatable read: Credit remained the same.");
                    }

                    await transactionA.CommitAsync();
                }
            }

            Logger.Log("Resetting credit to original value...");

            MySqlTransaction resetTransaction = null;
            using (MySqlConnection connection = new MySqlConnection(MySqlDatabaseConnection.Instance.ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    resetTransaction = await connection.BeginTransactionAsync();

                    List<(string, MySqlParameter[])> resetQueries = new List<(string, MySqlParameter[])>
                    {
                        ("UPDATE User SET Credit = @NewCredit WHERE ID = @UserID;",
                        new MySqlParameter[]
                        {
                            new MySqlParameter("@NewCredit", initialCredit),
                            new MySqlParameter("@UserID", userId)
                        })
                    };

                    await MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(resetQueries, resetTransaction);

                    await resetTransaction.CommitAsync();
                    Logger.Log("Credit reset successfully.");
                }
                catch (Exception ex)
                {
                    await Logger.LogError(ex);
                    if (resetTransaction != null)
                    {
                        await resetTransaction.RollbackAsync();
                        Logger.Log("Transaction rolled back.");
                    }
                }
                finally
                {
                    if (resetTransaction != null)
                    {
                        await resetTransaction.DisposeAsync();
                    }
                }
            }

            Logger.Log("Credit reset to original value.");
        }
    }
}
