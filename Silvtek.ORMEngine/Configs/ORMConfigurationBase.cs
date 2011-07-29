using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Configs
{
    public abstract class ORMConfigurationBase 
    {

        private string mapFilePath;
        private string balancerShare;
        private string workingFolder;
        private string tempFolder;
        private bool globalCacheEnabled;
        private bool changeBroadcastingEnabled;
        private Encoding defaultEncoding;
        private int buffersize;
        private int queryCacheTimeMs;


        protected ORMConfigurationBase()
        {
            mapFilePath = String.Empty;
            balancerShare = String.Empty;
            workingFolder = String.Empty;
            tempFolder = String.Empty;
            globalCacheEnabled = true;
            changeBroadcastingEnabled = true;
            defaultEncoding = Encoding.UTF8;
            buffersize = 5120;
            queryCacheTimeMs = 750;

        }
        public string MapFilePath
        {
            get { return mapFilePath; }
            set { mapFilePath = value; }
        }

        public string BalancerShare
        {
            get { return balancerShare; }
            set { balancerShare = value; }
        }

        public string WorkingFolder
        {
            get { return workingFolder; }
            set { workingFolder = value; }
        }

        public string TempFolder
        {
            get { return tempFolder; }
            set { tempFolder = value; }
        }

        public bool GlobalCacheEnabled
        {
            get { return globalCacheEnabled; }
            set { globalCacheEnabled = value; }
        }

        public bool ChangeBroadcastingEnabled
        {
            get { return changeBroadcastingEnabled; }
            set { changeBroadcastingEnabled = value; }
        }

        public Encoding DefaultEncoding
        {
            get { return defaultEncoding; }
            set { defaultEncoding = value; }
        }

        public int Buffersize
        {
            get { return buffersize; }
            set { buffersize = value; }
        }

        public int QueryCacheTimeMs
        {
            get { return queryCacheTimeMs; }
            set { queryCacheTimeMs = value; }
        }

        public void RegisterConnection(ConnectionSettings settings)
        {
            settings.Validate();
            ConnectionManager.Current.RegisterConnection(settings);
        }

        public abstract void ReadConfiguration(XmlDocument document);
        /// <summary>
        /// Gets the connection provider.
        /// </summary>
        /// <returns></returns>
        public ConnectionProvider CreateConnectionProvider(string assemblyName, string classname)
        {

            ConnectionProvider provider = null;
            try
            {
                // Initializing Provider
                Assembly tmpProviderAssembly = Assembly.Load(assemblyName);
                Type providerTypeClass = tmpProviderAssembly.GetType(classname);
                provider = (ConnectionProvider)Activator.CreateInstance(providerTypeClass);
            }
            catch (TypeInitializationException e)
            {
                string exMsg = Utility.GetResourceString("ConnectionInfo:ProviderInitializationException", classname, assemblyName);
                throw new ProviderInitializationException(exMsg, classname, e);
            }

       
            return provider;

        }

        /// <summary>
        /// Gets the db type formatter.
        /// </summary>
        /// <returns></returns>
        public DbTypeFormatter CreateFormatter(string assemblyName, string classname)
        {

            DbTypeFormatter dbtype = null;
            try
            {
                Assembly tmpTypeAssembly = Assembly.Load(assemblyName);
                Type dbTypeType = tmpTypeAssembly.GetType(classname);
                dbtype = (DbTypeFormatter)Activator.CreateInstance(dbTypeType);
            }
            catch (TypeInitializationException e)
            {
                string exMsg = Utility.GetResourceString("ConnectionInfo:DbTypeInitializationException", classname, assemblyName);
                throw new DbTypeInitializationException(exMsg, classname, e);
            }

            return dbtype;
        }

        protected void SetEncoding(string encoding)
        {
            if (encoding == null || encoding.Length == 0) return;

            defaultEncoding = Encoding.GetEncoding(encoding);
        }

        protected void SetBufferSize(string value)
        {
            if (value == null || value.Length == 0) return;

            try
            {
                Buffersize = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                throw new ORMConfigurationException("Buffer size defined in the configuration is not an integer");
            }
        }

        protected void SetQueryCacheTime(string value)
        {
            if (value == null || value.Length == 0) return;

            try
            {
                QueryCacheTimeMs = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                throw new ORMConfigurationException("QueryCacheTime defined in the configuration is not an integer");
            }
        }

        protected void SetGlobalCacheEnable(string value)
        {
            if (value == null || value.Length == 0) return;

            try
            {
                GlobalCacheEnabled = Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                throw new ORMConfigurationException("GlobalCacheEnabled defined in the configuration is not True or False");
            }
        }

        protected void SetBroadCaseEnable(string value)
        {
            if (value == null || value.Length == 0) return;

            try
            {
                ChangeBroadcastingEnabled = Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                throw new ORMConfigurationException("Change broadcase enabled defined in the configuration is not True or False");
            }
        }


        internal void Validate()
        {
            if (mapFilePath != null && mapFilePath.Length > 0)
            {
                if (!Directory.Exists(MapFilePath))
                {
                    string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:MAP_FILE_PATH_NOT_FOUND", MapFilePath);
                    throw new ORMException(exMsg);
                }
            }

            if (balancerShare != null && balancerShare.Length > 0)
            {
                if (!Directory.Exists(BalancerShare))
                {
                    string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:BALANCER_PATH_NOT_FOUND", balancerShare);
                    throw new ORMException(exMsg);
                }
            }

            ValidateWorkingFolder();

            if (Buffersize == 0)
            {
                Buffersize = 5120;
            }

            if (QueryCacheTimeMs == 0)
            {
                QueryCacheTimeMs = 750;
            }
        }

        private void ValidateWorkingFolder()
        {
            // ReSharper disable ReplaceWithStringIsNullOrEmpty
            if (WorkingFolder == null || WorkingFolder.Length == 0)
                // ReSharper restore ReplaceWithStringIsNullOrEmpty
            {
                WorkingFolder = Path.GetTempPath();
                TempFolder = WorkingFolder;
                // try to access the temporary path, by creating a test file.
                string filename = String.Format("ormtest-{0:N}", Guid.NewGuid());
                string testFile = Path.Combine(WorkingFolder, filename);
                try
                {
                    using (StreamWriter writer = new StreamWriter(testFile))
                    {
                        writer.Write("orm test");
                        writer.Close();
                        File.Delete(testFile);
                    }
                }
                catch (Exception ex)
                {
                    throw new ORMException(ex.Message + " Unable to access system temporary folder: " + workingFolder + " check permissions. As alternative, you can also configure your temporary folder in connections.config");
                }

                return;
            }

            if (!Directory.Exists(WorkingFolder))
            {
                Directory.CreateDirectory(WorkingFolder);
            }

            TempFolder = String.Format(@"{0}\tmp\", WorkingFolder);
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                Directory.CreateDirectory(TempFolder);
            }
        }
    }
}
