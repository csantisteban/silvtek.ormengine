using System.Collections;
using System.Diagnostics;


namespace Silvtek.ORMEngine.Data
{
    /// <summary>
    /// Contains a list of <see cref="ConnectionInfo"/> instance.
    /// </summary>
    internal class ConnectionInfoList : DictionaryBase
    {
        private string defaultConnectionName;

        /// <summary>
        /// Gets the <see cref="Silvtek.ORMEngine.Data.ConnectionInfo"/> with the specified name.
        /// </summary>
        /// <value></value>
        internal ConnectionInfo this[string name]
        {
            get
            {
                return GetConnectionInformation(name);
            }
            
        }

        /// <summary>
        /// Determines whether [contains] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified name]; otherwise, <c>false</c>.
        /// </returns>
        internal bool Contains(string name)
        {
            return Dictionary.Contains(name);
        }

        /// <summary>
        /// Gets the connection information.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal ConnectionInfo GetConnectionInformation(string name)
        {
            if (!Dictionary.Contains(name))
            {
               throw new ConnectionNameNotFoundException(name);
            }

            return (ConnectionInfo)Dictionary[name];
        }

        /// <summary>
        /// Adds the specified settings base.
        /// </summary>
        /// <param name="settings">The settings base.</param>
        internal void Add(ConnectionSettingsBase settings)
        {
            ConnectionInfo info = new ConnectionInfo(settings);

            if (settings.IsDefault)
            {
                SetDefaultConnection(settings.ConnectionName);
            }

            if (Dictionary.Contains(settings.ConnectionName))
            {
                string msg = Utility.GetResourceString("CONNECTION_MANAGER:DUPLICATE_CONNECTION_NAME", settings.ConnectionName);
                throw new ORMException(msg);
            }

            Dictionary.Add(settings.ConnectionName, info);


        }

        /// <summary>
        /// Gets the default name of the connection.
        /// </summary>
        /// <value>The default name of the connection.</value>
        internal string DefaultConnectionName
        {
            get { return defaultConnectionName; }
        }

        /// <summary>
        /// Changes the default connection.
        /// </summary>
        /// <param name="newConnectionName">New name of the connection.</param>
        internal void ChangeDefaultConnection(string newConnectionName)
		{

			if (Dictionary.Contains(newConnectionName))
			{
			   string exmsg = Utility.GetResourceString("EXMSG:ConnectionManager:CONNECTION_NAME_NOT_FOUND", newConnectionName);
			   throw new ConnectionNameNotFoundException(exmsg);
			}

			defaultConnectionName = newConnectionName;
		}

        /// <summary>
        /// Adds the specified connectionsettings.
        /// </summary>
        /// <param name="connectionsettings">The connectionsettings.</param>
        internal void Add(ConnectionSettings connectionsettings)
		{
            ConnectionInfo settings = new ConnectionInfo(connectionsettings);

            
            // Write to the trace log if a connection is being overwritten.
           // Trace.WriteLineIf(Dictionary.Contains(connectionsettings.ConnectionName), Utility.GetResourceString("Trace:ConnectionManager:REGISTER_CONNECTION", connectionsettings.ConnectionName), Resources.TRACE_CATEGORY);
			Dictionary[connectionsettings.ConnectionName] = settings;
		}


        private void SetDefaultConnection(string name)
        {
            defaultConnectionName = name;
        }
    }
}
