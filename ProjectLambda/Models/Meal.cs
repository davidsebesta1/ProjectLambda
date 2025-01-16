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
    public class Meal : IDataAccessObject, IEquatable<Meal?>
    {
        public static string SelectAllQuery => "SELECT * FROM Meal;";
        public static string InsertQuery => @"UPDATE Meal SET MealType_ID = @MealType_ID, Name = @Name WHERE ID = @ID;";
        public static string InsertQueryNoId => @"INSERT INTO Meal (MealType_ID, Name) VALUES (@MealType_ID, @Name);";
        public static string DeleteQuery => "DELETE FROM Meal WHERE ID = @ID;";

        public int ID { get; set; }
        public int MealType_ID { get; set; }
        public string Name { get; set; }

        // Constructor for a complete Meal object
        public Meal(int id, int mealTypeId, string name)
        {
            ID = id;
            MealType_ID = mealTypeId;
            Name = name;
        }

        // Constructor for a new Meal (without an ID yet)
        public Meal(int mealTypeId, string name) : this(-1, mealTypeId, name)
        {
        }

        // Load a Meal object from a DataRow
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new Meal(
                row.Field<int>("ID"),
                row.Field<int>("MealType_ID"),
                row.Field<string>("Name")
            );
        }

        // Save the Meal object (Insert or Update)
        public async Task<int> SaveAsync()
        {
            try
            {
                if (ID == -1)
                {
                    ID = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync($"{InsertQueryNoId} SELECT LAST_INSERT_ID();",GetMySqlParameters(false));
                    return ID;
                }
                else
                {
                    await MySqlDatabaseConnection.Instance.ExecuteNonQueryAsync(InsertQuery,GetMySqlParameters(true));
                    return ID;
                }
            }
            catch (Exception ex)
            {
                await Logging.Logger.LogError(ex);
            }

            return -1;
        }

        // Delete the Meal object
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
        public MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return
                [
                    new MySqlParameter("@MealType_ID", MealType_ID),
                    new MySqlParameter("@Name", Name),
                    new MySqlParameter("@ID", ID)
                ];
            }

            return
            [
                new MySqlParameter("@MealType_ID", MealType_ID),
                new MySqlParameter("@Name", Name)
            ];
        }

        // Equality and HashCode implementations
        public override bool Equals(object? obj)
        {
            return Equals(obj as Meal);
        }

        public bool Equals(Meal? other)
        {
            return other is not null && ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, MealType_ID, Name);
        }

        public override string? ToString()
        {
            return $"Meal: {Name}, MealType_ID: {MealType_ID}";
        }
    }
}
