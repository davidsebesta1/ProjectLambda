using MySql.Data.MySqlClient;
using ProjectLambda.Database;
using ProjectLambda.Models.Interfaces;
using System.Data;
using System.Globalization;

namespace ProjectLambda.Models
{
    /// <summary>
    /// A class representing a single lunch.
    /// </summary>
    public class Lunch : IDataAccessObject, IEquatable<Lunch?>
    {
        /// <inheritdoc/>
        public static string SelectAllQuery => "SELECT * FROM Lunch;";

        /// <inheritdoc/>
        public static string InsertQuery => @"UPDATE Lunch SET Soup_ID = @Soup_ID, MainMeal_ID = @MainMeal_ID, Dessert_ID = @Dessert_ID, Price = @Price, Date = @Date, MaxOrderTime = @MaxOrderTime WHERE ID = @ID;";

        /// <inheritdoc/>
        public static string InsertQueryNoId => @"INSERT INTO Lunch (Soup_ID, MainMeal_ID, Dessert_ID, Price, Date, MaxOrderTime) VALUES (@Soup_ID, @MainMeal_ID, @Dessert_ID, @Price, @Date, @MaxOrderTime);";

        /// <inheritdoc/>
        public static string DeleteQuery => "DELETE FROM Lunch WHERE ID = @ID;";

        /// <summary>
        /// Csv header row file text.
        /// </summary>
        public static string CsvRowHeader => "SoupName,MainCourseName,DessertName,Date,MaxOrderDate,Price";

        /// <inheritdoc/>
        public int ID { get; set; }

        /// <summary>
        /// ID of referenced soup.
        /// </summary>
        public int Soup_ID { get; set; }

        /// <summary>
        /// The referenced soup.
        /// </summary>
        public Meal ReferencedSoup
        {
            get
            {
                return DataRetriever<Meal>.Instance.GetFirstBy(m => m.ID == Soup_ID).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// ID of the referenced mail course.
        /// </summary>
        public int MainMeal_ID { get; set; }

        /// <summary>
        /// The referenced main course.
        /// </summary>
        public Meal ReferencedMainCourse
        {
            get
            {
                return DataRetriever<Meal>.Instance.GetFirstBy(m => m.ID == MainMeal_ID).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// ID of referenced dessert.
        /// </summary>
        public int Dessert_ID { get; set; }

        public Meal ReferencedDessert
        {
            get
            {
                return DataRetriever<Meal>.Instance.GetFirstBy(m => m.ID == Dessert_ID).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// The price of the lunch.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// The date of the lunch.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The maximum time by which an order can be placed for the lunch.
        /// </summary>
        public DateTime MaxOrderTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lunch"/> class with the specified details.
        /// </summary>
        /// <param name="id">The unique ID of the lunch.</param>
        /// <param name="soupId">The ID of the soup.</param>
        /// <param name="mainMealId">The ID of the main course.</param>
        /// <param name="dessertId">The ID of the dessert.</param>
        /// <param name="price">The price of the lunch.</param>
        /// <param name="date">The date of the lunch.</param>
        /// <param name="maxOrderTime">The maximum order time for the lunch.</param>
        public Lunch(int id, int soupId, int mainMealId, int dessertId, double price, DateTime date, DateTime maxOrderTime)
        {
            ID = id;
            Soup_ID = soupId;
            MainMeal_ID = mainMealId;
            Dessert_ID = dessertId;
            Price = price;
            Date = date;
            MaxOrderTime = maxOrderTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lunch"/> class with the specified details, with an automatic generation of ID.
        /// </summary>
        /// <param name="soupId">The ID of the soup.</param>
        /// <param name="mainMealId">The ID of the main course.</param>
        /// <param name="dessertId">The ID of the dessert.</param>
        /// <param name="price">The price of the lunch.</param>
        /// <param name="date">The date of the lunch.</param>
        /// <param name="maxOrderTime">The maximum order time for the lunch.</param>
        public Lunch(int soupId, int mainMealId, int dessertId, double price, DateTime date, DateTime maxOrderTime) : this(-1, soupId, mainMealId, dessertId, price, date, maxOrderTime)
        {
        }

        /// <summary>
        /// Creates a list of lunches from CSV file rows. Ensures new meals are created but not saved yet.
        /// </summary>
        /// <param name="rows">The rows from the CSV file.</param>
        /// <returns>A list of Lunch objects.</returns>
        public static async Task<bool> LoadFromCsvRow(List<string> rows)
        {
            List<Lunch> lunches = new List<Lunch>();

            foreach (string row in rows)
            {
                string[] args = row.Split(',');

                if (args.Length < 6)
                {
                    throw new ArgumentException("Row does not contain all required fields.");
                }

                string soupName = args[0].Trim();
                string mainCourseName = args[1].Trim();
                string dessertName = args[2].Trim();

                if (!DateTime.TryParseExact(args[3].Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime dateTime))
                    return false;

                if (!DateTime.TryParseExact(args[4].Trim(), "dd/MM/yyyy HH:mm", null, DateTimeStyles.None, out DateTime dateTimeMax))
                    return false;

                if (!double.TryParse(args[5].Trim(), CultureInfo.InvariantCulture, out double price))
                    return false;


                Meal? soup = await DataRetriever<Meal>.Instance.GetFirstBy(m => m.Name == soupName);
                if (soup == null)
                {
                    soup = new Meal(await GetMealTypeIdAsync("Soup"), soupName);
                    await soup.SaveAsync();
                }

                Meal? mainMeal = await DataRetriever<Meal>.Instance.GetFirstBy(m => m.Name == mainCourseName);
                if (mainMeal == null)
                {
                    mainMeal = new Meal(await GetMealTypeIdAsync("MainCourse"), mainCourseName);
                    await mainMeal.SaveAsync();
                }

                Meal? dessert = await DataRetriever<Meal>.Instance.GetFirstBy(m => m.Name == dessertName);
                if (dessert == null)
                {
                    dessert = new Meal(await GetMealTypeIdAsync("Dessert"), dessertName);
                    await dessert.SaveAsync();
                }

                lunches.Add(new Lunch(soup.ID, mainMeal.ID, dessert.ID, price, dateTime, dateTimeMax));
            }


            List<(string, MySqlParameter[])> list = new List<(string, MySqlParameter[])>(lunches.Count);

            foreach (Lunch lunch in lunches)
            {
                list.Add((Lunch.InsertQueryNoId, lunch.GetMySqlParameters(false)));
            }

            return await MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(list);
        }

        /// <summary>
        /// Gets the MealType ID for a given meal type name.
        /// </summary>
        private static async Task<int> GetMealTypeIdAsync(string mealTypeName)
        {
            MealType? mealType = await DataRetriever<MealType>.Instance.GetFirstBy(mt => mt.Name == mealTypeName);
            if (mealType == null)
            {
                throw new InvalidOperationException($"Meal type '{mealTypeName}' not found.");
            }

            return mealType.ID;
        }

        /// <inheritdoc/>
        public static IDataAccessObject LoadFromRow(DataRow row)
        {
            return new Lunch(
                row.Field<int>("ID"),
                row.Field<int>("Soup_ID"),
                row.Field<int>("MainMeal_ID"),
                row.Field<int>("Dessert_ID"),
                (double)row.Field<decimal>("Price"),
                row.Field<DateTime>("Date"),
                row.Field<DateTime>("MaxOrderTime")
            );
        }

        /// <inheritdoc/>
        public async Task<int> SaveAsync()
        {
            try
            {
                if (ID == -1)
                {
                    // Insert new Lunch and get the last inserted ID
                    ID = await MySqlDatabaseConnection.Instance.ExecuteScalarIntAsync(
                        $"{InsertQueryNoId} SELECT LAST_INSERT_ID();",
                        GetMySqlParameters(false)
                    );
                    return ID;
                }
                else
                {
                    // Update the existing Lunch
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
        private MySqlParameter[] GetMySqlParameters(bool includeID)
        {
            if (includeID)
            {
                return
                [
                    new MySqlParameter("@Soup_ID", Soup_ID),
                    new MySqlParameter("@MainMeal_ID", MainMeal_ID),
                    new MySqlParameter("@Dessert_ID", Dessert_ID),
                    new MySqlParameter("@Price", Price),
                    new MySqlParameter("@Date", Date),
                    new MySqlParameter("@MaxOrderTime", MaxOrderTime),
                    new MySqlParameter("@ID", ID)
                ];
            }

            return
            [
                new MySqlParameter("@Soup_ID", Soup_ID),
                new MySqlParameter("@MainMeal_ID", MainMeal_ID),
                new MySqlParameter("@Dessert_ID", Dessert_ID),
                new MySqlParameter("@Price", Price),
                new MySqlParameter("@Date", Date),
                new MySqlParameter("@MaxOrderTime", MaxOrderTime)
            ];
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Soup_ID, MainMeal_ID, Dessert_ID, Price, Date, MaxOrderTime);
        }

        public override string? ToString()
        {
            return $"Lunch: Soup_ID = {Soup_ID}, MainMeal_ID = {MainMeal_ID}, Dessert_ID = {Dessert_ID}, " +
                   $"Price = {Price:F2}, Date = {Date:yyyy-MM-dd}, MaxOrderTime = {MaxOrderTime:yyyy-MM-dd HH:mm:ss}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Lunch order && Equals(order);
        }

        public bool Equals(Lunch? other)
        {
            return ID == other.ID;
        }

        public static bool operator ==(Lunch? left, Lunch? right)
        {
            return EqualityComparer<Lunch>.Default.Equals(left, right);
        }

        public static bool operator !=(Lunch? left, Lunch? right)
        {
            return !(left == right);
        }
    }
}
