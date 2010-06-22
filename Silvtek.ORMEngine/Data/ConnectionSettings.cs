#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.Data
{
    /// <summary>
    /// Connection settings.
    /// </summary>
    public class ConnectionSettings 
    {
        private  string connectionName;
        private string connectionString;
        private  bool isDefault;
        private  int timeout;
        private  bool preparedStatements;
        private int commandTimeout;
        private ConnectionProvider connectionProvider;
        private DbTypeFormatter dbTypeFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSettings"/> class.
        /// </summary>
        public ConnectionSettings()
        {
            connectionName = String.Empty;
            connectionString = String.Empty;
            connectionProvider = null;
            dbTypeFormatter = null;
            isDefault = false;
            timeout = 45000;
            preparedStatements = false;
            commandTimeout = 90;

        }

        public  string ConnectionName
        {
            get { return connectionName; }
            set { connectionName = value; }
        }



        public ConnectionProvider ConnectionProvider
        {
            get { return connectionProvider; }
            set { connectionProvider = value; }
        }

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public DbTypeFormatter DbTypeFormatter
        {
            get { return dbTypeFormatter; }
            set { dbTypeFormatter = value; }
        }

        public bool IsDefault
        {
            get { return isDefault; }
            set { isDefault = value; }
        }

        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public bool PreparedStatements
        {
            get { return preparedStatements; }
            set { preparedStatements = value; }
        }

        public int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }
    }
}
