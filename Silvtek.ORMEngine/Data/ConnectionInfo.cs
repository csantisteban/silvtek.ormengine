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
        private int commandTimeout;
        private readonly RetryExceptions retryExceptions;

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

        /// <summary>
        /// Gets or sets a value indicating whether [prepared statement].
        /// </summary>
        /// <value><c>true</c> if [prepared statement]; otherwise, <c>false</c>.</value>
        public bool PreparedStatement
        {
            get { return preparedStatement; }
            set { preparedStatement = value; }
        }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>The command timeout.</value>
        public int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }

        /// <summary>
        /// Gets the retry exceptions.
        /// </summary>
        /// <value>The retry exceptions.</value>
        public RetryExceptions RetryExceptions
        {
            get { return retryExceptions; }
        }


        internal ConnectionInfo(ConnectionSettings settings)
        {
            this.name = settings.ConnectionName;
            cnnString = settings.ConnectionString;
            this.timeout = settings.Timeout;
            this.dbtype = settings.DbTypeFormatter;
            this.provider = settings.ConnectionProvider;
            preparedStatement = false;
            this.commandTimeout = settings.CommandTimeout;
            this.retryExceptions = settings.RetryExceptions;
            stats = new ConnectionStats();

        }

        internal ConnectionInfo(ConnectionSettingsBase settings)
        {
 
            provider = settings.GetConnectionProvider();
            dbtype = settings.GetDbTypeFormatter();
            timeout = settings.Timeout;
            cnnString = settings.ConnectionString;
            name = settings.ConnectionName;
            preparedStatement = settings.PreparedStatements;
            commandTimeout = settings.CommandTimeout;
            this.retryExceptions = settings.RetryExceptions;
            stats = new ConnectionStats();
        }


	}
}
