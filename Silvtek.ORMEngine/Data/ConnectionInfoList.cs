using System.Collections;
using System.Diagnostics;


namespace Silvtek.ORMEngine.Data
{
    internal class ConnectionInfoList : DictionaryBase
    {
        private string defaultConnectionName;

        internal ConnectionInfo this[string name]
        {
            get
            {
                return GetConnectionInformation(name);
            }
            
        }

        internal bool Contains(string name)
        {
            return Dictionary.Contains(name);
        }

        internal ConnectionInfo GetConnectionInformation(string name)
        {
            if (!Dictionary.Contains(name))
            {
               throw new ConnectionNameNotFoundException(name);
            }

            return (ConnectionInfo)Dictionary[name];
        }

        internal void Add(ConnectionSettingsBase settingsBase)
        {
            ConnectionInfo info = new ConnectionInfo(settingsBase);

            if (settingsBase.IsDefault)
            {
                SetDefaultConnection(settingsBase.ConnectionName);
            }

            if (Dictionary.Contains(settingsBase.ConnectionName))
            {
                string msg = Utility.GetResourceString("CONNECTION_MANAGER:DUPLICATE_CONNECTION_NAME", settingsBase.ConnectionName);
                throw new ORMException(msg);
            }

            Dictionary.Add(settingsBase.ConnectionName, info);


        }

        internal string DefaultConnectionName
        {
            get { return defaultConnectionName; }
        }

        internal void ChangeDefaultConnection(string newConnectionName)
		{

			if (Dictionary.Contains(newConnectionName))
			{
			   string exmsg = Utility.GetResourceString("EXMSG:ConnectionManager:CONNECTION_NAME_NOT_FOUND", newConnectionName);
			   throw new ConnectionNameNotFoundException(exmsg);
			}

			defaultConnectionName = newConnectionName;
		}

        internal void Add(string name, string connectionString, ConnectionProvider provider, DbTypeFormatter dbtype, int timeout, bool prepareStatement)
		{
			ConnectionInfo settings = new ConnectionInfo(name, connectionString, timeout, provider, dbtype);
            settings.PreparedStatement = prepareStatement;

            // Write to the trace log if a connection is being overwritten.
			Trace.WriteLineIf(Dictionary.Contains(name), Utility.GetResourceString("Trace:ConnectionManager:REGISTER_CONNECTION", name), Resources.TRACE_CATEGORY);
			Dictionary[name] = settings;
		}

        private void SetDefaultConnection(string name)
        {
            defaultConnectionName = name;
        }
    }
}
