#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Reflection;
using Silvtek.ORMEngine.Configs;

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
        private RetryExceptions retryExceptions;

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
            commandTimeout = 30;
            retryExceptions = new RetryExceptions();

        }

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>The name of the connection.</value>
        public  string ConnectionName
        {
            get { return connectionName; }
            set { connectionName = value; }
        }

        /// <summary>
        /// Gets or sets the connection provider.
        /// </summary>
        /// <value>The connection provider.</value>
        public ConnectionProvider ConnectionProvider
        {
            get { return connectionProvider; }
            set { connectionProvider = value; }
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return connectionString; }
            set
            {
                connectionString = (value != null) ? value.Trim() : String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the db type formatter.
        /// </summary>
        /// <value>The db type formatter.</value>
        public DbTypeFormatter DbTypeFormatter
        {
            get { return dbTypeFormatter; }
            set { dbTypeFormatter = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault
        {
            get { return isDefault; }
            set { isDefault = value; }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [prepared statements].
        /// </summary>
        /// <value><c>true</c> if [prepared statements]; otherwise, <c>false</c>.</value>
        public bool PreparedStatements
        {
            get { return preparedStatements; }
            set { preparedStatements = value; }
        }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        /// <value>The command timeout.</value>
        public int CommandTimeout
        {
            get { return commandTimeout; }
            set
            {
                if (value <= 0) return;
                commandTimeout = value;
            }
        }

        /// <summary>
        /// Gets the retry exceptions.
        /// </summary>
        /// <value>The retry exceptions.</value>
        public RetryExceptions RetryExceptions
        {
            get { return retryExceptions; }
            set 
            {
                if (value != null)
                {
                    retryExceptions = value;
                }              
            }
        }


        internal void Validate()
        {
            if (connectionName == null || connectionName.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_NAME_MISSING");
                throw new ORMException(exMsg);
            }

            if (connectionString == null || connectionString.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_STRING_MISSING");
                throw new ORMConfigurationException(exMsg);
            }

            if (connectionProvider == null)
            {
                throw new ORMConfigurationException("connection provider not specified");
            }


            if (dbTypeFormatter == null)
            {
                throw new ORMConfigurationException("connection formatter not specified");
            }
        }

    }
}
