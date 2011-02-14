#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using Silvtek.ORMEngine.ObjectPooling;

namespace Silvtek.ORMEngine.Data
{

    ///<summary>
    /// Represents an open connection to a data source.
    ///</summary>
    ///<remarks>
    ///<b> This is a generic connection </b>, it wraps specific
    /// connection like <i>SqlConnection, OracleConnection, OdbcConnection</i>
    ///<br />
    /// For more detail use see <see cref="IDbConnection" />
    ///</remarks>
	public class Connection : IDbConnection
	{
#if NET20
        private static readonly Pool<Connection> connectionPool = new Pool<Connection>(45, 2500);
#endif

        #region -- Member variables --
        // Hold a IDbConnection object
		private IDbConnection connection;

        // Hold the connection type.
		private DbTypeFormatter dbtype;

        // Hold the connection provider (OdbcProvider, OracleProvider, etc).
		private ProviderType providerType;

         // Hold the connection properties.
        private ConnectionInfo connectionInfo;

        // Hold the time that this connection is open, and
        // ready to execute.
        private DateTime openTime;

        // Hold the connection name.
        //private string name;

        // Hold the connection open timeout.
        private int timeout;

        // Hold the connection unique id.
        private readonly string id;

        // Hold the last time the connection has executed against the database.
        private DateTime lastExecutionDateTime;

        private int commandTimeout;

    #endregion
    
		 #region -- Events --
    
        /// <summary>
        /// Fired when the connection is opened.
        /// </summary>
        public EventHandler ConnectionOpened;
        
        
        /// Fired when the conneciton is closed.
        public EventHandler ConnectionClosed;
        
    #endregion
        
        ///<summary>
        /// Gets the <see cref="ConnectionSettingsBase" /> for this connection.
        ///</summary>
        public ConnectionInfo Settings
        {
          get { return connectionInfo; }
        }

        ///<summary>
        /// Gets the time this connection has been open.
        ///</summary>
        ///<value>
        /// The <see cref="TimeSpan" /> that the connection has been open. If
        /// the connection is currently, the value is equal to the span of
        /// the last open-close or 0 if never has been open.
        ///</value>
        public TimeSpan OpenTime
        {
          get
          {
            return (openTime > DateTime.MinValue) ? DateTime.Now - openTime : TimeSpan.Zero;
          }
        }
        
        
        ///<summary>
        /// Gets or sets the time in milliseconds that the connection can be open.
        ///</summary>
        ///<value>
        /// The time in milliseconds that the connection can be open.
        ///</value>
        public int Timeout
        {
          set { timeout = value; }
          get { return timeout; }
        }


        ///<summary>Gets the connection unique id.</summary>
        ///<value>The connection unique id</value>
        public string ID
        {
            get { return id; }
        }

        ///<summary>
        /// Gets or sets the string used to open a database
        ///</summary>
        ///<value>
        /// A string containing connection settings
        ///</value>
        ///<remarks>
        /// The <b>ConnectionString</b> property can be set
        /// only while the connection is closed.
        ///</remarks>
		public virtual string ConnectionString
		{
			get
			{
			  return connection.ConnectionString;
			}
			set
			{
			   connection.ConnectionString = value;
			}
		}

        ///<summary>
        /// Gets the time to wait while trying to establish a connection before
        /// terminating the attempt and generating an error
        ///</summary>
        ///<value>
        /// The time (in seconds) to wait for a connection to open.
        /// The default value is 15 seconds.
        ///</value>
        ///<remarks>
        /// A value of 0 indicates no limit to the wait time,
        /// rather than no wait time, and should be avoided
        /// in a <see cref="ConnectionString" /> because it would cause
        /// the connection attempt to wait indefinitely.
        ///</remarks>
		public virtual int ConnectionTimeout
		{
			get
			{
				return connection.ConnectionTimeout;
			}
		}

        ///<summary>
        /// Gets the name of the current database or the database to be used
        /// after a connection is opened.
        ///</summary>
        ///<value>
        /// The name of the current database or the name of the database to be
        /// used once a connection is open. The default value is an empty string.
        ///</value>
        ///<remarks>
        /// The Database property can be dynamically updated by using the
        /// <see cref="ChangeDatabase" /> method or by passing an SQL statement
        /// using one of the Execute methods. (Initially the database is set by
        /// means of the connection string.) If you change the current database
        /// using an SQL statement or the ChangeDatabase method,
        /// an informational message is sent and then the property is updated.
        ///</remarks>
		public virtual string Database
		{
			get
			{
				return connection.Database;
			}
		}

        ///<summary>
        /// Gets the current state of the connection.
        ///</summary>
        ///<value>
        ///One of the <see cref="ConnectionState" /> values.
        ///</value>
		public virtual ConnectionState State
		{
			get
			{
				return connection.State;
			}
		}


        ///<summary>
        /// Gets the <see cref="DbTypeFormatter" /> of the connection.
        ///</summary>
        ///<value>
        /// The <see cref="DbTypeFormatter" /> of the connection.
        ///</value>
        public DbTypeFormatter DbTypeFormatter
        {
        	get {return dbtype;}
        }

        ///<summary>
        /// Gets the <see cref="ProviderType" /> of the connection.
        ///</summary>
        ///<value>
        /// The <see cref="ProviderType" /> of the connection.
        ///</value>
        public ProviderType ProviderType
        {
        	  get {	return providerType; }
        }

		/// <summary>
		/// Gets the connection last execution date and time.
		/// </summary>
        internal DateTime LastExecutionDateTime
        {
           get {  return lastExecutionDateTime; }
        }

        public int CommandTimeout
        {
            get { return commandTimeout; }
        }


        private Connection()
        {
            id = Guid.NewGuid().ToString("N");
            connectionInfo = null;
            dbtype = null;
            providerType = ProviderType.Undefined;
            timeout = 0;
            commandTimeout = 90;
        }

        private void SetConnectionInformation(ConnectionInfo cnnInfo)
        {
            connectionInfo = cnnInfo;
            connection = connectionInfo.Provider.GetConnection();
            connection.ConnectionString = connectionInfo.ConnectionString;
            dbtype = connectionInfo.DbTypeFormatter;
            providerType = connectionInfo.Provider.Type;
            timeout = connectionInfo.Timeout;
            commandTimeout = connectionInfo.CommandTimeout;
        }

        internal static Connection CreateConnection(ConnectionInfo cnnInfo)
        {
#if NET20
            Connection cnn = connectionPool.GetObject();
            cnn.SetConnectionInformation(cnnInfo);
            return cnn;
#endif
#if NET11
            Connection cnn = new Connection();
            cnn.SetConnectionInformation(cnnInfo);
            return cnn;
#endif
        }


        ///<summary>
        /// Opens a database connection with the settings specified by the
        /// <see cref="ConnectionString" /> property of the provider-specific
        /// Connection object.
        ///</summary>
        public virtual void Open()
        {
            if (connectionInfo == null)
            {
                throw new InvalidOperationException("Unable to open database connection. There is not settings for this connection instance. Please report this problem, it is a implementation defect (bug)");
            }

           
            connection.Open();
           

            ConnectionManager.Current.AddOpenConnection(this);
            openTime = DateTime.Now;
            connectionInfo.Stats.Open();

            // Fire Event
            if (ConnectionOpened != null) ConnectionOpened(this, EventArgs.Empty);
        }

        /// <summary>
		/// Creates and returns a Command object associated with the connection.
		/// </summary>
		/// <returns>An <B>IDbCommand</B> object</returns>
		public virtual IDbCommand CreateCommand()
		{
            return connection.CreateCommand();
		}


		/// <summary>
		/// Changes the current database for an open Connection object.
		/// </summary>
		/// <param name="databaseName"></param>
		public virtual void ChangeDatabase(string databaseName)
		{
            
			  connection.ChangeDatabase(databaseName);
           
		}

		/// <summary>
		/// Closes the connection to the database.
		/// </summary>
		public virtual void Close()
		{
			connection.Close();
            connectionInfo.Stats.Close(OpenTime);
            openTime = DateTime.MinValue;
            ConnectionManager.Current.RemoveOpenConnection(this);
            if (ConnectionClosed != null)ConnectionClosed(this, EventArgs.Empty);
		}

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <param name="il">One of the <see cref="IsolationLevel" /> values</param>
		/// <returns>An object representing the new transaction.</returns>
		/// <remarks>Once the transaction has completed, you must explicitly commit or roll back 
		/// the transaction by using the Commit or Rollback methods.</remarks>
		public virtual IDbTransaction BeginTransaction(IsolationLevel il)
		{
			IDbTransaction trans;
			
            
             trans = connection.BeginTransaction(il);
           
            return trans;
		}

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <returns>An object representing the new transaction.</returns>
		/// <remarks>Once the transaction has completed, you must explicitly commit or roll back 
		/// the transaction by using the Commit or Rollback methods.</remarks>
		public virtual IDbTransaction BeginTransaction()
		{
			IDbTransaction trans;
			
            
              trans = connection.BeginTransaction();
          

            return trans;
		}

		/// <summary>
		/// Dispose resources.
		/// </summary>
		public virtual void Dispose()
		{
			connection.Dispose();
		}

		/// <summary>
		/// Serves as a hash function for a particular type, 
		/// suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current Object.</returns>
        public override int GetHashCode()
        {
           return id.GetHashCode();
        }

        /// <summary>
        /// Determines whether two Object instances are equal.
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object. </param>
        /// <returns><see lang="true"/> if the specified Object is equal to the current Object; otherwise, <see lang="false"/>.</returns>
        public override bool Equals(object obj)
        {
           if (obj.GetType() != this.GetType())return false;
           Connection tmp_conn = (Connection)obj;
           return (id == tmp_conn.ID);
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
           return ConnectionString;
        }

        /// <summary>
        /// Updates the execution time to right now.
        /// </summary>
        internal void UpdateExecutionDateTime()
        {
           lastExecutionDateTime = DateTime.Now;
        }

	}
}



