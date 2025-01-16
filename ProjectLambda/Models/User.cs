using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A user that can login onto the 
    /// </summary>
    public class User : IDataAccessObject, IEquatable<User?>
    {
        public static string SelectAllQuery => "SELECT * FROM User;";

        public static string InsertQuery => @"UPDATE User SET FirstName = @FirstName, LastName = @LastName, Username = @Username, Credit = @Credit, PasswordHashed = @PasswordHashed WHERE ID = @ID;";

        public static string InsertQueryNoId => @"INSERT INTO User (FirstName, LastName, Username, PasswordHashed, Credit) VALUES (@FirstName, @LastName, @Username, @PasswordHashed, @Credit);";

        public static string DeleteQuery => "DELETE FROM User WHERE ID = @ID;";

        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string PasswordHashed { get; set; }

        public decimal Credit { get; set; }

        public User(int id, string firstName, string lastName, string username, string hashedPass, decimal credit)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            PasswordHashed = hashedPass;
            Credit = credit;
        }

        public User(string firstName, string lastName, string username, string hashedPass, decimal credit) : this(-1, firstName, lastName, username, hashedPass, credit)
        {

        }

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

        public bool CheckPassword(string plaintext)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(plaintext, PasswordHashed);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User? other)
        {
            return other is not null && ID == other.ID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, FirstName, LastName, Credit);
        }

        public override string? ToString()
        {
            return $"User: {FirstName} {LastName}, Credit: {Credit:F2}";
        }
    }
}
