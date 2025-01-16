using System.Data;

namespace ProjectLambda.Models.Interfaces
{
    /// <summary>
    /// Base interface providing features for Data Access Object pattern.
    /// </summary>
    public interface IDataAccessObject
    {
        /// <summary>
        /// Primary key for any DAO object.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Asynchronous method for saving the object in database.
        /// </summary>
        /// <returns>The ID of the object.</returns>
        public abstract Task<int> SaveAsync();

        /// <summary>
        /// Asynchronous method for deleting the object from database.
        /// </summary>
        /// <returns>Whether the deletion was successful.</returns>
        public abstract Task<bool> DeleteAsync();

        /// <summary>
        /// Query for returning all object of this type.
        /// </summary>
        public static abstract string SelectAllQuery { get; }

        /// <summary>
        /// Query for updating already existing object into database.
        /// </summary>
        public static abstract string InsertQuery { get; }

        /// <summary>
        /// Query for inserting a new object into database. Returns object's internal database ID.
        /// </summary>
        public static abstract string InsertQueryNoId { get; }

        /// <summary>
        /// Query for deleting object from database.
        /// </summary>
        public static abstract string DeleteQuery { get; }

        /// <summary>
        /// Method for loading this object from a standard <see cref="DataRow"/> object.
        /// </summary>
        /// <param name="row">Data row from database.</param>
        /// <returns>A new object casted to this interface.</returns>
        public static abstract IDataAccessObject LoadFromRow(DataRow row);
    }
}
