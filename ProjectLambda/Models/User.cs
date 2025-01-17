using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Helpers;
using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A user that can login onto.
    /// </summary>
    public class User : IDataAccessObject, IEquatable<User?>
    {
        /// <inheritdoc/>
        public static string SelectAllQuery => "SELECT * FROM User;";

        /// <inheritdoc/>
        public static string InsertQuery => @"UPDATE User SET FirstName = @FirstName, LastName = @LastName, Username = @Username, Credit = @Credit, PasswordHashed = @PasswordHashed WHERE ID = @ID;";

        /// <inheritdoc/>
        public static string InsertQueryNoId => @"INSERT INTO User (FirstName, LastName, Username, PasswordHashed, Credit) VALUES (@FirstName, @LastName, @Username, @PasswordHashed, @Credit);";

        /// <inheritdoc/>
        public static string DeleteQuery => "DELETE FROM User WHERE ID = @ID;";

        /// <summary>
        /// Query that adds credit to the user.
        /// </summary>
        public static string AddCreditQuery => "UPDATE User SET Credit = (SELECT Credit) + @Delta WHERE ID = @ID;";

        /// <inheritdoc/>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string PasswordHashed { get; set; }

        /// <summary>
        /// Gets or sets the credit balance of the user.
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with the specified ID, first name, last name, username, hashed password, and credit.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="firstName">The first name of the user.</param>
        /// <param name="lastName">The last name of the user.</param>
        /// <param name="username">The username of the user.</param>
        /// <param name="hashedPass">The hashed password of the user.</param>
        /// <param name="credit">The credit balance of the user.</param>
        public User(int id, string firstName, string lastName, string username, string hashedPass, decimal credit)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            PasswordHashed = hashedPass;
            Credit = credit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class without an ID, using the specified first name, last name, username, hashed password, and credit.
        /// The ID is automatically set to -1 for a new user.
        /// </summary>
        /// <param name="firstName">The first name of the user.</param>
        /// <param name="lastName">The last name of the user.</param>
        /// <param name="username">The username of the user.</param>
        /// <param name="hashedPass">The hashed password of the user.</param>
        /// <param name="credit">The credit balance of the user.</param>
        public User(string firstName, string lastName, string username, string hashedPass, decimal credit) : this(-1, firstName, lastName, username, hashedPass, credit)
        {
        }

        /// <inheritdoc/>
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new User(
                row.Field<int>("ID"),
                row.Field<string>("FirstName"),
                row.Field<string>("LastName"),
                row.Field<string>("Username"),
                row.Field<string>("PasswordHashed"),
                row.Field<decimal>("Credit")
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

        /// <summary>
        /// Generates a new monthly report for this user.
        /// </summary>
        /// <param name="month">Month number between 1 and 12.</param>
        /// <returns>A new monthly report.</returns>
        public async Task<UserMonthlyReport> GenerateMonthlyReportAsync(int year, int month, List<Lunch> lunches = null)
        {
            lunches ??= await DataRetriever<Lunch>.Instance.GetAll();

            List<LunchOrder> orders = await DataRetriever<LunchOrder>.Instance.GetAllBy(n => n.User_ID == ID && n.ReferencedLunch.Date.Month == month && n.ReferencedLunch.Date.Year == year);

            return new UserMonthlyReport(this, orders, lunches.Count);
        }

        /// <inheritdoc/>
        private MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return
                [
                    new MySqlParameter("@FirstName", FirstName),
                    new MySqlParameter("@LastName", LastName),
                    new MySqlParameter("@Username", LastName),
                    new MySqlParameter("@Credit", Credit),
                    new MySqlParameter("@PasswordHashed", PasswordHashed),
                    new MySqlParameter("@ID", ID)
                ];
            }

            return
            [
                new MySqlParameter("@FirstName", FirstName),
                new MySqlParameter("@LastName", LastName),
                new MySqlParameter("@Username", LastName),
                new MySqlParameter("@Credit", Credit),
                new MySqlParameter("@PasswordHashed", PasswordHashed),
            ];
        }

        /// <summary>
        /// Gets params for the <see cref="AddCreditQuery"/>.
        /// </summary>
        /// <param name="delta">Credits to be added to the current value.</param>
        /// <returns>Params for the query.</returns>
        public MySqlParameter[] GetMySqlParametersEditCredit(double delta)
        {
            return [new MySqlParameter("@ID", ID), new MySqlParameter("@Delta", delta)];
        }

        /// <summary>
        /// Checks whether the password matches the hash.
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public bool CheckPassword(string plaintext)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(plaintext, PasswordHashed);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, FirstName, LastName, Credit);
        }

        public override string? ToString()
        {
            return $"User: {FirstName} {LastName}, Credit: {Credit:F2}";
        }

        public override bool Equals(object? obj)
        {
            return obj is User order && Equals(order);
        }

        public bool Equals(User? other)
        {
            return ID == other.ID;
        }

        public static bool operator ==(User? left, User? right)
        {
            return EqualityComparer<User>.Default.Equals(left, right);
        }

        public static bool operator !=(User? left, User? right)
        {
            return !(left == right);
        }
    }
}
