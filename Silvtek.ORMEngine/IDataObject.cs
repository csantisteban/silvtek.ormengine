namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Marks an object as a data object and defines CRUD methods
    /// </summary>
    public interface IDataObject
    {
        ///<summary>
        /// Created a new record in the database.
        ///</summary>
        void Create();

        ///<summary>
        /// Load current instance from the datasource.
        ///</summary>
        void Load();

        ///<summary>
        /// Removes record from datasource.
        ///</summary>
        void Remove();

        ///<summary>
        /// Saves changes to the database.
        ///</summary>
        void Save();

        /// <summary>
        /// Gets the digest.
        /// </summary>
        /// <returns></returns>
        string GetDigest();
    }
}
