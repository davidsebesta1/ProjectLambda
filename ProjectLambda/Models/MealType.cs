using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A class represnting meal type.
    /// </summary>
    public class MealType : IDataAccessObject, IEquatable<MealType?>
    {
        /// <inheritdoc/>
        public static string SelectAllQuery => "SELECT * FROM MealType;";

        /// <inheritdoc/>
        public static string InsertQuery => @"UPDATE MealType SET Name = @Name WHERE ID = @ID;";

        /// <inheritdoc/>
        public static string InsertQueryNoId => @"INSERT INTO MealType (Name) VALUES (@Name);";

        /// <inheritdoc/>
        public static string DeleteQuery => "DELETE FROM MealType WHERE ID = @ID;";

        /// <inheritdoc/>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the meal type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MealType"/> class with the specified ID and name.
        /// </summary>
        /// <param name="id">The unique identifier of the meal type.</param>
        /// <param name="name">The name of the meal type.</param>
        public MealType(int id, string name)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MealType"/> class without an ID, using the specified name.
        /// The ID is automatically set to -1 for a new meal type.
        /// </summary>
        /// <param name="name">The name of the meal type.</param>
        public MealType(string name) : this(-1, name)
        {
        }

        /// <inheritdoc/>
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new MealType(
                row.Field<int>("ID"),
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
                return await MySqlDatabaseConnection.Instance.ExecuteNonQueryAsync(DeleteQuery, new MySqlParameter("@ID", ID)) == 1;
            }
            catch (Exception ex)
            {
                await Logging.Logger.LogError(ex);
            }

            return false;
        }

        /// <inheritdoc/>
        private MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return
                [
                    new MySqlParameter("@Name", Name),
                    new MySqlParameter("@ID", ID)
                ];
            }

            return
            [
                new MySqlParameter("@Name", Name)
            ];
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name);
        }

        public override string? ToString()
        {
            return $"MealType: {Name}";
        }

        public override bool Equals(object? obj)
        {
            return obj is MealType order && Equals(order);
        }

        public bool Equals(MealType? other)
        {
            return ID == other.ID;
        }

        public static bool operator ==(MealType? left, MealType? right)
        {
            return EqualityComparer<MealType>.Default.Equals(left, right);
        }

        public static bool operator !=(MealType? left, MealType? right)
        {
            return !(left == right);
        }
    }
}
