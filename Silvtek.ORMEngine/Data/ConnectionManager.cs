#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Silvtek.ORMEngine.Configs;

namespace Silvtek.ORMEngine.Data
{
	/// <summary>
	/// Manages ORM Connections.
	/// </summary>
	public class ConnectionManager //: IEnumerable
	{
        
        private static ConnectionManager manager;
        private static object connection_manager_mutex = new object();
	    private static bool startConf = true;
        
        
        #region -- member variables --

        private static readonly Object _mutex = new Object();

        private readonly ConnectionInfoList connectionInformationList;
        private readonly Hashtable openConnections;
        private readonly TimeSpan interval;
        private Timer timer;
        
        #endregion
        
		/// <summary>
		/// Gets the unique <see cref="ConnectionManager"/> for this application.
		/// </summary>
        public static ConnectionManager Current
        {
        	get { return GetSingleton(); }
        }

        ///<summary>
        /// Verify whether a connection name exist in the connection collection.
        ///</summary>
        public bool Contains(string connectionName)
        {
            if (startConf)
            {
                ORMConfigurator.Init();
                startConf = false;
            }

           return connectionInformationList.Contains(connectionName);
        }

		#region -- RegisterConnection Methods --


        /// <summary>
        /// Registers the connection.
        /// </summary>
        /// <param name="settings">The settings.</param>
		public void RegisterConnection(ConnectionSettings settings)
		{
            connectionInformationList.Add(settings);
		}


        /// <summary>
        /// Registers the connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="dbtype">The dbtype.</param>
        /// <param name="timeout">The timeout.</param>
		public void RegisterConnection(string name, string connectionString, ConnectionProvider provider, DbTypeFormatter dbtype, int timeout)
		{
            ConnectionSettings settings = new ConnectionSettings();
            settings.ConnectionName = name;
            settings.ConnectionString = connectionString;
            settings.ConnectionProvider = provider;
            settings.DbTypeFormatter = dbtype;
		    settings.Timeout = timeout;
            RegisterConnection(settings);
		}

        /// <summary>
        /// Register connection information.
        /// </summary>
        /// <param name="name">The connection name.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The database provider</param>
        /// <param name="dbtype">The database type.</param>
        /// <param name="timeout">The timeout for a connection</param>
        /// <param name="prepareStatements">Wheater to prepare statements or not.</param>
        /// <remarks>
        /// <note>If a connection name already exits will be overwritten.
        /// Registering a connection programmatically.
        /// <example>
        ///   <code>
        ///      string connectionString = "[connection-string]";
        ///      MSSqlProvider provider = MSSqlProvider();
        ///      MSSqlType dbType = MSSqlType();
        ///      ConnectionManager.Current.RegisterConnection("Sample", connectionString, , dbType, 40);
        ///   </code>
        ///  </example>
        /// </note>
        /// </remarks>
        public void RegisterConnection(string name, string connectionString, ConnectionProvider provider, DbTypeFormatter dbtype, int timeout, bool prepareStatements)
        {
            ConnectionSettings settings = new ConnectionSettings();
            settings.ConnectionName = name;
            settings.ConnectionString = connectionString;
            settings.ConnectionProvider = provider;
            settings.DbTypeFormatter = dbtype;
            settings.Timeout = timeout;
            settings.PreparedStatements = prepareStatements;
            RegisterConnection(settings);
        }
		
		/// <summary>
		/// Register connection information with no timeout.
		/// </summary>
		/// <param name="name">The connection name.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="provider">The database provider</param>
		/// <param name="dbtype">The database type.</param>
		/// <remarks>
		/// <note>If a connection name already exits will be overwritten.</note>
		/// </remarks>
		public void RegisterConnection(string name, string connectionString, ConnectionProvider provider, DbTypeFormatter dbtype)
		{
            ConnectionSettings settings = new ConnectionSettings();
		    settings.ConnectionName = name;
		    settings.ConnectionString = connectionString;
		    settings.ConnectionProvider = provider;
		    settings.DbTypeFormatter = dbtype;
            RegisterConnection(settings);
		}
				
		#endregion

		
		/// <summary>
		/// Changes the default connection.
		/// </summary>
		/// <param name="name">The new connection name.</param>
		/// <exception cref="ConnectionNameNotFoundException">
		/// When the new default connection name does not exists.
		/// </exception>
        public void ChangeDefaultConnection(string name)
		{
		    connectionInformationList.ChangeDefaultConnection(name);
		}

	    /// <summary>
		/// Returns a connection.
		/// </summary>
		/// <param name="name">The name of the connection to be returned.</param>
		/// <returns>A <see cref="Connection"/> instance. The connection needs to be open in order to be used.</returns>
        public Connection GetConnection(string name)
        {
            if (startConf)
            {
                ORMConfigurator.Init();
                startConf = false;
            }

	        ConnectionInfo cnnInfo = connectionInformationList.GetConnectionInformation(name);
            return Connection.CreateConnection(cnnInfo);
        }

		/// <summary>
		/// Returns the connection settings by its name.
		/// </summary>
		/// <param name="name">Connection name</param>
		/// <returns>The <see cref="ConnectionSettingsBase" /> instance;
		/// if the connection name is not found, it returns <b>Null.</b>
		/// </returns>
        public ConnectionInfo GetConnectionSettings(string name)
		{
		    if (startConf)
		    {
		        ORMConfigurator.Init();
		        startConf = false;
		    }
		    return connectionInformationList.GetConnectionInformation(name);
		}

	    public void RegisterConnection(ConnectionSettingsBase settings)
        {
            if (settings == null) return;
            connectionInformationList.Add(settings);
        }
 
        /// <summary>
        /// Adds an open connection to be controlled by this instance.
        /// </summary>
        /// <param name="cnn">The connection</param>
        internal void AddOpenConnection(Connection cnn)
        {
        	if (cnn == null)return;
     
        	WeakReference wref = new WeakReference(cnn);
        	
        	lock(_mutex)
        	{
                
        	   openConnections.Add(cnn.ID, wref);
        	}
        	
        }

		#region -- RemoveOpenConnection overloaded methods --
		/// <summary>
		/// Remove an open connection from this instance.
		/// </summary>
		/// <param name="cnn">The connection to be removed.</param>
        internal void RemoveOpenConnection(Connection cnn)
        {
        	if (cnn == null)return;
        	RemoveOpenConnection(cnn.ID);
        }


		/// <summary>
		/// Remove an open connection from this instance.
		/// </summary>
		/// <param name="cnnID">The conneectionID</param>
        internal void RemoveOpenConnection(string cnnID)
        {
        	lock(_mutex)
        	{
        	   openConnections.Remove(cnnID);
        	}
        }

		#endregion

         
        // Constructor.
        private ConnectionManager()
        {
            
            connectionInformationList = new ConnectionInfoList();
            openConnections = new Hashtable(20);
            interval = new TimeSpan(0, 0, 45);


            // Initialize timer.
            InitTime();
        }

	    // Returns the unique instance of the Connection Manager.
		private static ConnectionManager GetSingleton()
		{
			if (manager != null)return manager;

            lock (connection_manager_mutex)
			{
				if (manager == null)
				{
					manager = new ConnectionManager();
				}
			}
			
			return manager;
		}
		


        private void InitTime()
        {
            TimerCallback timerDelegate = new TimerCallback(CheckConnections);
            timer = new Timer(timerDelegate, null, new TimeSpan(0, 1, 0), interval);
        }


	    private void CheckConnections(object obj)
        {
// ReSharper disable RedundantAssignment
			obj = null;// Argument 'obj' Not used.
// ReSharper restore RedundantAssignment

            if (openConnections.Count == 0)return;

            lock (_mutex)
            {

                ArrayList list = new ArrayList(openConnections.Keys);
                int numOpenCnn = list.Count;
                //DateTime now = DateTime.Now;

                for (int i = 0; i < numOpenCnn; i++)
                {

                    WeakReference wref = (WeakReference)openConnections[list[i]];
                    if (!wref.IsAlive)
                    {
                        openConnections.Remove(list[i]);
                        continue;
                    }

                    Connection con = (Connection)wref.Target;

                    if (con.Timeout < 10) continue;

                    if (con.OpenTime.TotalMilliseconds > con.Timeout)
                    {
                        //Before closing check if the connection is executing.
                        TimeSpan lastExec = DateTime.Now - con.LastExecutionDateTime;

                        //if last execution in this connection is 36000 Milliseconds
                        //idle without  execution
                        if (lastExec.TotalMilliseconds > 36000)
                        {
                            con.Settings.Stats.IncrementClosedByForce();

                            //string traceMsg = Utility.GetResourceString("TRACE:CON_FORCED_TO_CLOSE", con.ID, con.OpenTime, lastExec);
                            //Trace.WriteLine(traceMsg, Resources.TRACE_CATEGORY);
                            
                            //todo: call windows event.
                            con.Close();
                        }
                    }
                }
            }
		}

	}
}

