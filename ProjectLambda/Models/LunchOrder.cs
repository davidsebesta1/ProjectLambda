using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLambda.Models
{
    public class LunchOrder : IDataAccessObject, IEquatable<LunchOrder?>
    {
        public static string SelectAllQuery => "SELECT * FROM LunchOrder;";
        public static string InsertQuery => @"UPDATE LunchOrder 
                                          SET Lunch_ID = @Lunch_ID, User_ID = @User_ID, Picked = @Picked 
                                          WHERE ID = @ID;";
        public static string InsertQueryNoId => @"INSERT INTO LunchOrder (Lunch_ID, User_ID, Picked) 
                                              VALUES (@Lunch_ID, @User_ID, @Picked);";
        public static string DeleteQuery => "DELETE FROM LunchOrder WHERE ID = @ID;";

        public int ID { get; set; }
        public int Lunch_ID { get; set; }
        public int User_ID { get; set; }
        public bool? Picked { get; set; }

        // Constructor for a complete LunchOrder object
        public LunchOrder(int id, int lunchId, int userId, bool? picked)
        {
            ID = id;
            Lunch_ID = lunchId;
            User_ID = userId;
            Picked = picked;
        }

        // Constructor for a new LunchOrder (without an ID yet)
        public LunchOrder(int lunchId, int userId, bool? picked)
            : this(-1, lunchId, userId, picked)
        {
        }

        // Load a LunchOrder object from a DataRow
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new LunchOrder(
                row.Field<int>("ID"),
                row.Field<int>("Lunch_ID"),
                row.Field<int>("User_ID"),
                row.IsNull("Picked") ? null : row.Field<bool>("Picked")
            );
        }

        // Save the LunchOrder object (Insert or Update)
        public async Task<int> SaveAsync()
        {
            try
            {
                if (ID == -1)
                {
                    // Insert new LunchOrder and get the last inserted ID
                    ID = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync(
                        $"{InsertQueryNoId} SELECT LAST_INSERT_ID();",
                        GetMySqlParameters(false)
                    );
                    return ID;
                }
                else
                {
                    // Update the existing LunchOrder
                    await MySqlDatabaseConnection.Instance.ExecuteNonQueryAsync(
                        InsertQuery,
                        GetMySqlParameters(true)
                    );
                    return ID;
                }
            }
            catch (Exception ex)
            {
                await Logging.Logger.LogError(ex);
            }

            return -1; // Indicating failure to save
        }

        // Delete the LunchOrder object
        public async Task<bool> DeleteAsync()
        {
            try
            {
                return await MySqlDatabaseConnection.Instance.ExecuteNonQueryAsync(
                    DeleteQuery,
                    new MySqlParameter("@ID", ID)
                ) == 1;
            }
            catch (Exception ex)
            {
                await Logging.Logger.LogError(ex);
            }

            return false;
        }

        // Generate MySQL parameters for the queries
        private MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return new[]
                {
                new MySqlParameter("@Lunch_ID", Lunch_ID),
                new MySqlParameter("@User_ID", User_ID),
                new MySqlParameter("@Picked", Picked ?? (object)DBNull.Value),
                new MySqlParameter("@ID", ID)
            };
            }

            return new[]
            {
            new MySqlParameter("@Lunch_ID", Lunch_ID),
            new MySqlParameter("@User_ID", User_ID),
            new MySqlParameter("@Picked", Picked ?? (object)DBNull.Value)
        };
        }

        // Equality and HashCode implementations
        public override bool Equals(object? obj)
        {
            return Equals(obj as LunchOrder);
        }

        public bool Equals(LunchOrder? other)
        {
            return other is not null && ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Lunch_ID, User_ID, Picked);
        }

        public override string? ToString()
        {
            return $"LunchOrder: Lunch_ID = {Lunch_ID}, User_ID = {User_ID}, Picked = {Picked}";
        }
    }
}
