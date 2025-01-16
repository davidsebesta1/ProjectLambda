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
    public class MealType : IDataAccessObject, IEquatable<MealType?>
    {
        public static string SelectAllQuery => "SELECT * FROM MealType;";
        public static string InsertQuery => @"UPDATE MealType SET Name = @Name WHERE ID = @ID;";
        public static string InsertQueryNoId => @"INSERT INTO MealType (Name) VALUES (@Name);";
        public static string DeleteQuery => "DELETE FROM MealType WHERE ID = @ID;";

        public int ID { get; set; }
        public string Name { get; set; }

        // Constructor for a complete MealType object
        public MealType(int id, string name)
        {
            ID = id;
            Name = name;
        }

        // Constructor for a new MealType (without an ID yet)
        public MealType(string name) : this(-1, name)
        {
        }

        // Load a MealType object from a DataRow
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new MealType(
                row.Field<int>("ID"),
                row.Field<string>("Name")
            );
        }

        // Save the MealType object (Insert or Update)
        public async Task<int> SaveAsync()
        {
            try
            {
                if (ID == -1)
                {
                    // Insert new MealType and get the last inserted ID
                    ID = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync(
                        $"{InsertQueryNoId} SELECT LAST_INSERT_ID();",
                        GetMySqlParameters(false)
                    );
                    return ID;
                }
                else
                {
                    // Update the existing MealType
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

        // Delete the MealType object
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
                new MySqlParameter("@Name", Name),
                new MySqlParameter("@ID", ID)
            };
            }

            return new[]
            {
            new MySqlParameter("@Name", Name)
        };
        }

        // Equality and HashCode implementations
        public override bool Equals(object? obj)
        {
            return Equals(obj as MealType);
        }

        public bool Equals(MealType? other)
        {
            return other is not null && ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name);
        }

        public override string? ToString()
        {
            return $"MealType: {Name}";
        }
    }
}
