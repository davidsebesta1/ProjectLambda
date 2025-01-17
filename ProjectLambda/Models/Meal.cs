using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A class representing a single meal as part of lunch.
    /// </summary>
    public class Meal : IDataAccessObject, IEquatable<Meal?>
    {
        /// <inheritdoc/>
        public static string SelectAllQuery => "SELECT * FROM Meal;";

        /// <inheritdoc/>
        public static string InsertQuery => @"UPDATE Meal SET MealType_ID = @MealType_ID, Name = @Name WHERE ID = @ID;";

        /// <inheritdoc/>
        public static string InsertQueryNoId => @"INSERT INTO Meal (MealType_ID, Name) VALUES (@MealType_ID, @Name);";

        /// <inheritdoc/>
        public static string DeleteQuery => "DELETE FROM Meal WHERE ID = @ID;";

        /// <inheritdoc/>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the meal type that this meal belongs to.
        /// </summary>
        public int MealType_ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the meal.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Meal"/> class with the specified ID, meal type ID, and meal name.
        /// </summary>
        /// <param name="id">The unique identifier of the meal.</param>
        /// <param name="mealTypeId">The ID of the meal type.</param>
        /// <param name="name">The name of the meal.</param>
        public Meal(int id, int mealTypeId, string name)
        {
            ID = id;
            MealType_ID = mealTypeId;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Meal"/> class without an ID, using the specified meal type ID and meal name.
        /// The ID is automatically set to -1 for a new meal.
        /// </summary>
        /// <param name="mealTypeId">The ID of the meal type.</param>
        /// <param name="name">The name of the meal.</param>
        public Meal(int mealTypeId, string name) : this(-1, mealTypeId, name)
        {
        }

        /// <inheritdoc/>
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new Meal(
                row.Field<int>("ID"),
                row.Field<int>("MealType_ID"),
                row.Field<string>("Name")
            );
        }

        /// <inheritdoc/>
        public async Task<int> SaveAsync()
        {
            try
            {
                if (ID == -1)
                {
                    ID = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync($"{InsertQueryNoId} SELECT LAST_INSERT_ID();", GetMySqlParameters(false));
                    return ID;
                }
                else
                {
                    await MySqlDatabaseConnection.Instance.ExecuteNonQueryAsync(InsertQuery, GetMySqlParameters(true));
                    return ID;
                }
            }
            catch (Exception ex)
            {
                await Logging.Logger.LogError(ex);
            }

            return -1;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, MealType_ID, Name);
        }

        public override string? ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Meal order && Equals(order);
        }

        public bool Equals(Meal? other)
        {
            return ID == other.ID;
        }

        public static bool operator ==(Meal? left, Meal? right)
        {
            return EqualityComparer<Meal>.Default.Equals(left, right);
        }

        public static bool operator !=(Meal? left, Meal? right)
        {
            return !(left == right);
        }
    }
}
