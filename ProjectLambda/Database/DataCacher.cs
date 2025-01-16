using ProjectLambda.Models.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;

namespace ProjectLambda.Database
{
    public class DataCacher<T> where T : IDataAccessObject
    {
        private static readonly Dictionary<Type, DataCacher<T>> _cachersByType = new Dictionary<Type, DataCacher<T>>();

        private readonly ConcurrentDictionary<int, T> _cachedPerId = new ConcurrentDictionary<int, T>();
        private readonly List<T> _objects = new List<T>();

        public event EventHandler<DataServiceOnSaveOrUpdateArgs<T>> OnSaveOrUpdate;
        public event EventHandler<DataServiceOnDeleteArgs<T>> OnDelete;

        /// <summary>
        /// Gets the instance of the data cacher 
        /// </summary>
        public static DataCacher<T> Instance
        {
            get
            {
                if (_cachersByType.TryGetValue(typeof(T), out DataCacher<T> cacher))
                {
                    return cacher;
                }

                DataCacher<T> cacher2 = new();
                _cachersByType[typeof(T)] = cacher2;
                return cacher2;
            }
        }


        private DataCacher()
        {

        }

        public async Task<List<T>> GetAll()
        {
            if (_cachedPerId.IsEmpty)
            {
                DataTable? table = await MySqlDatabaseConnection.Instance.ExecuteQueryAsync(T.SelectAllQuery);

                if (table == null) return _objects;

                foreach (DataRow row in table.Rows)
                {
                    T obj = (T)T.LoadFromRow(row);
                    _cachedPerId.TryAdd(obj.ID, obj);
                    _objects.Add(obj);
                }
            }
            return _objects;
        }

        public bool AddOrUpdate(T item)
        {
            if (_cachedPerId.ContainsKey(item.ID))
            {
                OnSaveOrUpdate?.Invoke(this, new DataServiceOnSaveOrUpdateArgs<T>(item, true));
                return false;
            }


            _cachedPerId.TryAdd(item.ID, item);
            _objects.Add(item);
            OnSaveOrUpdate?.Invoke(this, new DataServiceOnSaveOrUpdateArgs<T>(item, false));
            return true;
        }

        public bool Remove(T item)
        {
            bool res = _cachedPerId.Remove(item.ID, out var val) && _objects.Remove(item);
            OnDelete?.Invoke(this, new DataServiceOnDeleteArgs<T>(item));
            return res;
        }

        public async Task<List<T>> GetAllBy(Predicate<T> predicate)
        {
            return new List<T>((await GetAll()).Where(n => predicate(n)));
        }

        public async Task<T?> GetFirstBy(Predicate<T> predicate)
        {
            if (!_objects.Any()) await GetAll();
            return _objects.FirstOrDefault(n => predicate(n));
        }
    }

    public class DataServiceOnDeleteArgs<T> : EventArgs
    {
        public Type Type;
        public T ItemDeleted;

        public DataServiceOnDeleteArgs(T itemDeleted) : base()
        {
            Type = itemDeleted.GetType();
            ItemDeleted = itemDeleted;
        }
    }

    public class DataServiceOnSaveOrUpdateArgs<T> : EventArgs
    {
        public Type Type;
        public T ItemSaved;
        public bool SavedNewItem;

        public DataServiceOnSaveOrUpdateArgs(T itemSaved, bool isNewItem) : base()
        {
            Type = itemSaved.GetType();
            ItemSaved = itemSaved;
            SavedNewItem = isNewItem;
        }
    }
}
