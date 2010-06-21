using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	///  Bound a class to a database table
	/// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class TableAttribute : Attribute
	{

        private readonly string tablename;
        private readonly string connectionName;
	    private readonly bool isCachingDisabled;

        ///<summary>
        /// Gets the connection name.
        ///</summary>
        public string ConnectionName
        {
          get {return connectionName;}
        }

        ///<summary>
        /// The table name.
        ///</summary>
        public string Table
        {
          get {return tablename;}
        }

	    public bool IsCachingDisabled
	    {
            get { return isCachingDisabled; }

	    }
        ///<summary>
        /// Constructor.
        ///</summary>
        ///<param name="tableName">
        /// The table name.
        ///</param>
        ///<param name="connectionName">
        /// The connection name for this dataobject.
        ///</param>
		public TableAttribute(string tableName, string connectionName)
		{
            tablename = tableName;
            this.connectionName = connectionName;
		}

        public TableAttribute(string tableName, string connectionName, bool noCaching)
        {
            tablename = tableName;
            this.connectionName = connectionName;
            isCachingDisabled = noCaching;
        }

        ///<summary>
        /// Constructor.
        ///</summary>
        ///<param name="tableName">
        /// The table name.
        ///</param>
		public TableAttribute(string tableName)
		{
            tablename = tableName;
            connectionName = String.Empty;
		}
	}
}
