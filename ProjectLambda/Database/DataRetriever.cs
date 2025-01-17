using ProjectLambda.Models.Interfaces;
using System.Data;

namespace ProjectLambda.Database
{
    /// <summary>
    /// A class for retrieving instances from databases.
    /// </summary>
    /// <typeparam name="T">Type of the retrieved.</typeparam>
    public class DataRetriever<T> where T : IDataAccessObject
    {
        private static readonly Dictionary<Type, DataRetriever<T>> _cachersByType = new Dictionary<Type, DataRetriever<T>>();

        /// <summary>
        /// Event invoked on every save or update.s
        /// </summary>
        public event EventHandler<DataServiceOnSaveOrUpdateArgs<T>> OnSaveOrUpdate;

        /// <summary>
        /// Event invoked when object is deleted.
        /// </summary>
        public event EventHandler<DataServiceOnDeleteArgs<T>> OnDelete;

        /// <summary>
        /// Gets the instance of the data cacher 
        /// </summary>
        public static DataRetriever<T> Instance
        {
            get
            {
                if (_cachersByType.TryGetValue(typeof(T), out DataRetriever<T> cacher))
                {
                    return cacher;
                }

                DataRetriever<T> cacher2 = new();
                _cachersByType[typeof(T)] = cacher2;
                return cacher2;
            }
        }


        private DataRetriever()
        {

        }

        /// <summary>
        /// Gets all new instances of this type in a new list.
        /// </summary>
        /// <returns>A list of a new instances.</returns>
        public async Task<List<T>> GetAll()
        {
            List<T> list = new List<T>();
            DataTable? table = await MySqlDatabaseConnection.Instance.ExecuteQueryAsync(T.SelectAllQuery);

            if (table == null)
                return list;

            foreach (DataRow row in table.Rows)
            {
                T obj = (T)T.LoadFromRow(row);
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Adds or updates the specified object in the database.
        /// </summary>
        /// <param name="item"></param>
        public void AddOrUpdate(T item)
        {
            OnSaveOrUpdate?.Invoke(this, new DataServiceOnSaveOrUpdateArgs<T>(item, false));
        }

        /// <summary>
        /// Removes the specified object from the database.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            OnDelete?.Invoke(this, new DataServiceOnDeleteArgs<T>(item));
        }

        /// <summary>
        /// Gets all items by predicate condiction.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <returns>A new list of all items that matches the condition.</returns>
        public async Task<List<T>> GetAllBy(Predicate<T> predicate)
        {
            return new List<T>((await GetAll()).Where(n => predicate(n)));
        }

        /// <summary>
        /// Gets the first instance of an item that matches the predicate.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <returns>Item or null if not found.</returns>
        public async Task<T?> GetFirstBy(Predicate<T> predicate)
        {
            List<T> list = await GetAll();
            return list.FirstOrDefault(n => predicate(n));
        }
    }

    /// <summary>
    /// Provides event arguments for delete operations in a data service, containing information about the deleted item.
    /// </summary>
    /// <typeparam name="T">The type of the item that was deleted.</typeparam>
    public class DataServiceOnDeleteArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the type of the item that was deleted.
        /// </summary>
        public Type Type;

        /// <summary>
        /// Gets the item that was deleted.
        /// </summary>
        public T ItemDeleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataServiceOnDeleteArgs{T}"/> class with the specified deleted item.
        /// </summary>
        /// <param name="itemDeleted">The item that was deleted.</param>
        public DataServiceOnDeleteArgs(T itemDeleted) : base()
        {
            Type = itemDeleted.GetType();
            ItemDeleted = itemDeleted;
        }
    }

    /// <summary>
    /// Provides event arguments for save or update operations in a data service, containing information about the saved or updated item.
    /// </summary>
    /// <typeparam name="T">The type of the item that was saved or updated.</typeparam>
    public class DataServiceOnSaveOrUpdateArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the type of the item that was saved or updated.
        /// </summary>
        public Type Type;

        /// <summary>
        /// Gets the item that was saved or updated.
        /// </summary>
        public T ItemSaved;

        /// <summary>
        /// Indicates whether the item was a new item (true) or an updated item (false).
        /// </summary>
        public bool SavedNewItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataServiceOnSaveOrUpdateArgs{T}"/> class with the specified saved or updated item and whether it's a new item.
        /// </summary>
        /// <param name="itemSaved">The item that was saved or updated.</param>
        /// <param name="isNewItem">Indicates whether the item is a new item.</param>
        public DataServiceOnSaveOrUpdateArgs(T itemSaved, bool isNewItem) : base()
        {
            Type = itemSaved.GetType();
            ItemSaved = itemSaved;
            SavedNewItem = isNewItem;
        }
    }
}
