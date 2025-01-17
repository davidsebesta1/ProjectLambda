using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A class representing single order of a user.
    /// </summary>
    public class LunchOrder : IDataAccessObject, IEquatable<LunchOrder?>
    {
        /// <inheritdoc/>
        public static string SelectAllQuery => "SELECT * FROM LunchOrder;";

        /// <inheritdoc/>
        public static string InsertQuery => @"UPDATE LunchOrder  SET Lunch_ID = @Lunch_ID, User_ID = @User_ID, Picked = @Picked WHERE ID = @ID;";

        /// <inheritdoc/>
        public static string InsertQueryNoId => @"INSERT INTO LunchOrder (Lunch_ID, User_ID, Picked) VALUES (@Lunch_ID, @User_ID, @Picked);";

        /// <inheritdoc/>
        public static string DeleteQuery => "DELETE FROM LunchOrder WHERE ID = @ID;";

        /// <inheritdoc/>
        public int ID { get; set; }

        /// <summary>
        /// ID of the referenced lunch.
        /// </summary>
        public int Lunch_ID { get; set; }

        /// <summary>
        /// The referenced lunch object based on the <see cref="Lunch_ID"/>.
        /// </summary>
        public Lunch ReferencedLunch
        {
            get
            {
                return DataRetriever<Lunch>.Instance.GetFirstBy(m => m.ID == Lunch_ID).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// ID of the user who placed the lunch order.
        /// </summary>
        public int User_ID { get; set; }

        /// <summary>
        /// The referenced user object based on the <see cref="User_ID"/>.
        /// </summary>
        public User ReferencedUser
        {
            get
            {
                return DataRetriever<User>.Instance.GetFirstBy(m => m.ID == User_ID).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Indicates whether the lunch order was picked by the user. Can be null if not yet picked.
        /// </summary>
        public bool? Picked { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LunchOrder"/> class with the specified details.
        /// </summary>
        /// <param name="id">The unique ID of the lunch order.</param>
        /// <param name="lunchId">The ID of the lunch that is ordered.</param>
        /// <param name="userId">The ID of the user who placed the order.</param>
        /// <param name="picked">Indicates if the lunch order has been picked by the user.</param>
        public LunchOrder(int id, int lunchId, int userId, bool? picked)
        {
            ID = id;
            Lunch_ID = lunchId;
            User_ID = userId;
            Picked = picked;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LunchOrder"/> class with the specified details, without an ID.
        /// </summary>
        /// <param name="lunchId">The ID of the lunch that is ordered.</param>
        /// <param name="userId">The ID of the user who placed the order.</param>
        /// <param name="picked">Indicates if the lunch order has been picked by the user.</param>
        public LunchOrder(int lunchId, int userId, bool? picked) : this(-1, lunchId, userId, picked)
        {
        }

        /// <inheritdoc/>
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new LunchOrder(
                row.Field<int>("ID"),
                row.Field<int>("Lunch_ID"),
                row.Field<int>("User_ID"),
                row.IsNull("Picked") ? null : row.Field<ulong>("Picked") == 1
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
        public MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return
                [
                    new MySqlParameter("@Lunch_ID", Lunch_ID),
                    new MySqlParameter("@User_ID", User_ID),
                    new MySqlParameter("@Picked", Picked ?? (object)DBNull.Value),
                    new MySqlParameter("@ID", ID)
                ];
            }

            return
            [
                new MySqlParameter("@Lunch_ID", Lunch_ID),
                new MySqlParameter("@User_ID", User_ID),
                new MySqlParameter("@Picked", Picked ?? (object)DBNull.Value)
            ];
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Lunch_ID, User_ID, Picked);
        }

        public override string? ToString()
        {
            return $"LunchOrder: Lunch_ID = {Lunch_ID}, User_ID = {User_ID}, Picked = {Picked}";
        }

        public override bool Equals(object? obj)
        {
            return obj is LunchOrder order && Equals(order);
        }

        public bool Equals(LunchOrder? other)
        {
            return ID == other.ID;
        }

        public static bool operator ==(LunchOrder? left, LunchOrder? right)
        {
            return EqualityComparer<LunchOrder>.Default.Equals(left, right);
        }

        public static bool operator !=(LunchOrder? left, LunchOrder? right)
        {
            return !(left == right);
        }
    }
}
