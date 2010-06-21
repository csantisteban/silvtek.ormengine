#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

namespace Silvtek.ORMEngine.Data
{
    ///<summary>
    /// Defines a connection property.
    ///</summary>
    ///<remarks>
    /// This class is mainly for stats.
    ///</remarks>
	public sealed class ConnectionInfo
	{
        private readonly string cnnString;
        private readonly DbTypeFormatter dbtype;
        private readonly string name;
        private readonly ConnectionProvider provider;
        private readonly ConnectionStats stats;
        private int timeout;
        private bool preparedStatement;

        /// <summary>
        /// Gets the connections stadistics.
        /// </summary>
        public ConnectionStats Stats
        {
            get { return stats; }
        }

        /// <summary>
        /// Gets the number of seconds on which
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { if (value >= 0)timeout = value; }
        }

        /// <summary>
        /// Gets the name of the connection.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return cnnString; }
        }

        /// <summary>
        /// Gets the connection provider.
        /// </summary>
        public ConnectionProvider Provider
        {
            get { return provider; }
        }

        /// <summary>
        /// Gets the database type.
        /// </summary>
        public DbTypeFormatter DbTypeFormatter
        {
            get { return dbtype; }
        }

        public bool PreparedStatement
        {
            get { return preparedStatement; }
            set { preparedStatement = value; }
        }


        internal ConnectionInfo(string name, string connectionString, int timeout, ConnectionProvider provider, DbTypeFormatter dbtype)
        {
            this.name = name;
            cnnString = connectionString;
            this.timeout = timeout;
            this.dbtype = dbtype;
            this.provider = provider;
            preparedStatement = false;
            stats = new ConnectionStats();

        }

        internal ConnectionInfo(ConnectionSettingsBase settingsBase)
        {
 
            provider = settingsBase.GetConnectionProvider();
            dbtype = settingsBase.GetDbTypeFormatter();
            timeout = settingsBase.Timeout;
            cnnString = settingsBase.ConnectionString;
            name = settingsBase.ConnectionName;
            preparedStatement = settingsBase.PreparedStatements;
            stats = new ConnectionStats();
        }


	}
}
