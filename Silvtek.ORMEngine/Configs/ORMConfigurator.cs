using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Configs
{

    /// <summary>
    /// Represents ORM global configurations
    /// </summary>
    public class ORMConfigurator
    {
        private static ORMConfigurator singleton;
        private const string XML_CONFIG_FILE = "Connections.Config";

        private static string configurationFile;

        private string mapFilePath;
        private string balancerShare;
        private string workingFolder;
        private string tempFolder;
        private bool globalCacheEnabled;
        private bool changeBroadcastingEnabled;
        private Encoding defaultEncoding;
       
        private int buffersize;
        private int queryCacheTimeMs;

        // Returns the unique instance of the Connection Manager.
        internal static ORMConfigurator Singleton
        {
            get
            {
                if (singleton != null) return singleton;

                lock (typeof(ORMConfigurator))
                {
                    if (singleton == null)
                    {
                        singleton = new ORMConfigurator();
                    }
                }

                return singleton;
            }
        }

        /// <summary>
        /// Gets or sets the map files root folder.
        /// </summary>
        public static string MapFiles
        {
            get { return Singleton.mapFilePath; }
        }

        /// <summary>
        /// Gets the folder where shadow file will be stored for handling load balance and web farms.
        /// </summary>
        public static string BalancerShare
        {
            get { return Singleton.balancerShare; }
            set { Singleton.balancerShare = value; }
        }

        /// <summary>
        /// Gets whether the current instance run in an share environment.
        /// </summary>
        public static bool UseBalancerShare
        {
            get { return (Singleton.balancerShare != null && Singleton.balancerShare.Length > 0); }
        }
        
        /// <summary>
        /// Gets the temporary folder.
        /// </summary>
        public static string WorkingFolder
        {
            get { return Singleton.workingFolder; }
            set { Singleton.workingFolder = value; }
        }

        /// <summary>
        /// Gets or sets the temporary folder location.
        /// </summary>
        /// <value>The temporary folder.</value>
        public static string TemporaryFolder
        {
            get { return Singleton.tempFolder; }
            set { Singleton.tempFolder = value; }
        }
  


        /// <summary>
        /// Gets whether the global cache is enabled.
        /// </summary>
        public static bool GlobalCacheEnabled
        {
            get { return Singleton.globalCacheEnabled; }
            set { Singleton.globalCacheEnabled = value; }
        }

        /// <summary>
        /// Gets or sets whether broadcast updates for update /delete
        /// objects. 
        /// </summary>
        public static bool UpdateBroadCastingEnabled
        {
            get { return Singleton.changeBroadcastingEnabled; }
            set { Singleton.changeBroadcastingEnabled = value; }
        }

        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        public static int BufferSize
        {
            get { return Singleton.buffersize; }
        }

        /// <summary>
        /// Gets the query cache time ms.
        /// </summary>
        /// <value>The query cache time ms.</value>
        public static int QueryCacheTimeMs
        {
            get { return Singleton.queryCacheTimeMs; }
        }

        public static Encoding DefaultEncoding
        {
            get { return Singleton.defaultEncoding; }
            set { Singleton.defaultEncoding = value; }
        }

        private ORMConfigurator()
        {
            buffersize = 5120;
            changeBroadcastingEnabled = true;
            globalCacheEnabled = true;
            this.balancerShare = String.Empty;
            this.mapFilePath = String.Empty;
            this.defaultEncoding = Encoding.ASCII;
            this.queryCacheTimeMs = 800;
            this.workingFolder = String.Empty;
            
            LoadConfigurations();
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        public static void Init()
        {
            if (singleton != null) return;
            
            lock (typeof(ORMConfigurator))
            {
                if (singleton == null)
                {
                    singleton = new ORMConfigurator();
                }
            }
        }

        /// <summary>
        /// Sets the path for the configuration file.
        /// </summary>
        /// <param name="filepath">The file path</param>
        /// <exception cref="FileNotFoundException">When the file specified do not exists.</exception>
        public static void SetConfigurationFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                string exMsg = String.Format("The ORM configuration file '{0}' do not exists", filepath);
                throw new FileNotFoundException(exMsg);
            }

            lock (typeof(ORMConfigurator))
            {
                configurationFile = filepath;
            }
        }

        /// <summary>
        /// Reload the configuration file.
        /// </summary>
        public static void ReloadConfigurationFile()
        {
            lock (typeof(ORMConfigurator))
            {
                singleton = new ORMConfigurator();
            }
        }

        /// <summary>
        /// Sets programatically the Map files directory. This method will reload the configurations.
        /// </summary>
        /// <param name="directory">The new map files directory.</param>
        public static void SetMapFileDirectory(string directory)
        {
            if (directory == null || directory.Length == 0)
            {
                throw new ArgumentException("Invalid map file directory path");
            }

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException("Map file directory: "+directory+" not found!");
            }

            lock (typeof(ORMConfigurator))
            {
                if (singleton == null)
                {
                    singleton = new ORMConfigurator();
                    singleton.mapFilePath = directory;
                }
            }
        }

        public static void SetBufferSize(int newSize)
        {
            if (newSize == 0)
            {
                throw new ArgumentException("Invalid size value. Size must be greater than 0");
            }

            Singleton.buffersize = newSize;
        }

        #region -- Private Methods -- 

        // Load the Connection.config xml file.
        // If the file does not exists; it won't be loaded.
        private void LoadConfigurations()
        {
            string directoryName = GetPath();

            string xmlDocFile;

            if (configurationFile == null)
            {
                xmlDocFile = directoryName + @"\" + XML_CONFIG_FILE;
            }
            else
            {
                xmlDocFile = configurationFile;
            }

            // if file does not exists return.
            if (!File.Exists(xmlDocFile)) return;

            XmlDocument confXml = new XmlDocument();
            confXml.Load(xmlDocFile);
            LoadWorkingFolder(confXml);
            LoadMapFilesFolder(confXml);
            LoadBalancerShareFolder(confXml);
            LoadGlobalCache(confXml);
            LoadUpdateBroadcastConfiguration(confXml);
            LoadBufferSize(confXml);
            LoadQueryCacheDefaultTime(confXml);
            LoadEncodingType(confXml);
            

            // load declared connections.
            XmlNodeList nodelist = confXml.SelectNodes("/dataSettings/connection");

            if (nodelist != null)
            {
                /* Interact thru XML Doc */
                foreach (XmlNode node in nodelist)
                {
                    DefaultConnectionSettings settings = new DefaultConnectionSettings(node);
                    ConnectionManager.Current.RegisterConnection(settings);
                }
            }
        }


        private void LoadMapFilesFolder(XmlNode node)
        {
            const string mapFileXPath = "/dataSettings/mapFiles";
            XmlNode mapFileNode = node.SelectSingleNode(mapFileXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (mapFileNode == null) return;
            mapFilePath = Utility.GetAttributeValue(mapFileNode, "path");

            if (mapFilePath == null || mapFilePath.Length == 0) return;

            if (!Directory.Exists(mapFilePath))
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:MAP_FILE_PATH_NOT_FOUND", mapFilePath);
                throw new ORMException(exMsg);
            }

        }

        private void LoadEncodingType(XmlNode node)
        {
            const string encodingXpath = "/dataSettings/Encoding";
            XmlNode mapFileNode = node.SelectSingleNode(encodingXpath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (mapFileNode == null)
            {

                defaultEncoding = Encoding.Default;
                return;

            }

            string encodingString = Utility.GetAttributeValue(mapFileNode, "value");
           
            

            if (encodingString == null || encodingString.Length == 0) return;

            Encoding tmpEncoding = Encoding.GetEncoding(encodingString);

            defaultEncoding = tmpEncoding;

        }

        private void LoadWorkingFolder(XmlNode node)
        {
            const string WORKING_FOLDER_XPATH = "/dataSettings/workingFolder";
            XmlNode tempFileNode = node.SelectSingleNode(WORKING_FOLDER_XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (tempFileNode != null)
            {
                workingFolder = Utility.GetAttributeValue(tempFileNode, "path");
            }

// ReSharper disable ReplaceWithStringIsNullOrEmpty
            if (workingFolder == null || workingFolder.Length == 0)
// ReSharper restore ReplaceWithStringIsNullOrEmpty
            {
                workingFolder = Path.GetTempPath();
                tempFolder = workingFolder;
                // try to access the temporary path, by creating a test file.
                string filename = String.Format("ormtest-{0:N}", Guid.NewGuid());
                string testFile = Path.Combine(workingFolder, filename);
                try
                {
                    using (StreamWriter writer = new StreamWriter(testFile))
                    {
                        writer.Write("orm test");
                        writer.Close();
                        File.Delete(testFile);
                    }

                    
                }
                catch(Exception ex)
                {
                    throw new ORMException(ex.Message+ " Unable to access system temporary folder: " + workingFolder+ " check permissions. As alternative, you can also configure your temporary folder in connections.config");
                }

                return;
            }

            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            tempFolder = String.Format(@"{0}\tmp\", workingFolder);
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
                Directory.CreateDirectory(tempFolder);
            }
            else
            {
                Directory.CreateDirectory(tempFolder);
            }
           
            
 
        }

        private void LoadBalancerShareFolder(XmlNode docNode)
        {
            const string balanceShareXPath = "/dataSettings/sharedResources";
            XmlNode node = docNode.SelectSingleNode(balanceShareXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null)
            {
                balancerShare = String.Empty;
                return;
            }

            balancerShare = Utility.GetAttributeValue(node, "path");

            if (balancerShare == null || balancerShare.Length == 0) return;

            if (!Directory.Exists(balancerShare))
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:BALANCER_PATH_NOT_FOUND", balancerShare);
                throw new ORMException(exMsg);
            }
        }

        private void LoadBufferSize(XmlNode docNode)
        {
            const string balanceShareXPath = "/dataSettings/BufferSize";
            XmlNode node = docNode.SelectSingleNode(balanceShareXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            buffersize = Utility.GetAttributeValueAsInt32(node, "size", false);

            if (buffersize == 0)
            {
                buffersize = 5120;
            }
        }

        private void LoadQueryCacheDefaultTime(XmlNode docNode)
        {
            const string balanceShareXPath = "/dataSettings/DefaultQueryCacheTime";
            XmlNode node = docNode.SelectSingleNode(balanceShareXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            queryCacheTimeMs = Utility.GetAttributeValueAsInt32(node, "milliseconds", false);

            if (queryCacheTimeMs == 0)
            {
                queryCacheTimeMs = 750;
            }
        }



        private void LoadGlobalCache(XmlNode docNode)
        {
            const string balanceShareXPath = "/dataSettings/globalCache";
            XmlNode node = docNode.SelectSingleNode(balanceShareXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            string tempBool = Utility.GetAttributeValue(node, "enabled");
            if (tempBool == null || tempBool.Length == 0)
            {
                globalCacheEnabled = true;
                return;
            }

            try
            {
                globalCacheEnabled = Convert.ToBoolean(tempBool);
            }
            catch (Exception)
            {
                throw new ORMException("Invalid value set in the attribute 'enabled' of the globalCache element");
            }
        }

        private void LoadUpdateBroadcastConfiguration(XmlNode docNode)
        {
            const string balanceShareXPath = "/dataSettings/UpdateBroadcast";
            XmlNode node = docNode.SelectSingleNode(balanceShareXPath);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            string tempBool = Utility.GetAttributeValue(node, "enabled");
            if (tempBool == null || tempBool.Length == 0)
            {
                this.changeBroadcastingEnabled = true;
                return;
            }

            try
            {
                changeBroadcastingEnabled = Convert.ToBoolean(tempBool);
            }
            catch (Exception)
            {
                throw new ORMException("Invalid value set in the attribute 'enabled' of the UpdateBroadcast element");
            }
        }

        // Returns the path of the current assembly.
        private static string GetPath()
        {

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string codebase = currentAssembly.CodeBase;
            string directoryName = Path.GetDirectoryName(codebase.Replace(@"file:///", ""));
            return directoryName;

        }



        #endregion
    }
}
