using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Silvtek.ORMEngine.Configs
{

    /// <summary>
    /// Represents ORM global configurations
    /// </summary>
    public class ORMConfigurator
    {
        // constants
        private const string _XML_CONFIG_FILE = "Connections.Config";
        private static ORMConfigurator singleton;

        private ORMConfigurationBase configuration;
        private static string configuration_file;


        // Returns the unique instance of the Connection Manager.
        internal static ORMConfigurator Current
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
            get { return Current.Configuration.MapFilePath; }
        }

        /// <summary>
        /// Gets the folder where shadow file will be stored for handling load balance and web farms.
        /// </summary>
        public static string BalancerShare
        {
            get { return Current.Configuration.BalancerShare; }
            set { Current.Configuration.BalancerShare = value; }
        }

        /// <summary>
        /// Gets whether the current instance run in an share environment.
        /// </summary>
        public static bool UseBalancerShare
        {
            get { return (Current.Configuration.BalancerShare != null && Current.Configuration.BalancerShare.Length > 0); }
        }
        
        /// <summary>
        /// Gets the temporary folder.
        /// </summary>
        public static string WorkingFolder
        {
            get { return Current.Configuration.WorkingFolder; }
            set { Current.Configuration.WorkingFolder = value; }
        }

        /// <summary>
        /// Gets or sets the temporary folder location.
        /// </summary>
        /// <value>The temporary folder.</value>
        public static string TemporaryFolder
        {
            get { return Current.Configuration.TempFolder; }
            set { Current.Configuration.TempFolder = value; }
        }
  


        /// <summary>
        /// Gets whether the global cache is enabled.
        /// </summary>
        public static bool GlobalCacheEnabled
        {
            get { return Current.Configuration.GlobalCacheEnabled; }
            set { Current.Configuration.GlobalCacheEnabled = value; }
        }

        /// <summary>
        /// Gets or sets whether broadcast updates for update /delete
        /// objects. 
        /// </summary>
        public static bool UpdateBroadCastingEnabled
        {
            get { return Current.Configuration.ChangeBroadcastingEnabled; }
            set { Current.Configuration.ChangeBroadcastingEnabled = value; }
        }

        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        public static int BufferSize
        {
            get { return Current.Configuration.Buffersize; }
        }

        /// <summary>
        /// Gets the query cache time ms.
        /// </summary>
        /// <value>The query cache time ms.</value>
        public static int QueryCacheTimeMs
        {
            get { return Current.Configuration.QueryCacheTimeMs; }
        }

        public static Encoding DefaultEncoding
        {
            get { return Current.Configuration.DefaultEncoding; }
            set { Current.Configuration.DefaultEncoding = value; }
        }

        public ORMConfigurationBase Configuration
        {
            get { return configuration; }
            set { configuration = value; }
        }

        private ORMConfigurator()
        { 
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
                configuration_file = filepath;
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
                    singleton.Configuration.MapFilePath = directory;
                }
            }
        }

        public static void SetBufferSize(int newSize)
        {
            if (newSize == 0)
            {
                throw new ArgumentException("Invalid size value. Size must be greater than 0");
            }

            Current.Configuration.Buffersize = newSize;
        }

        #region -- Private Methods -- 

        // Load the Connection.config xml file.
        // If the file does not exists; it won't be loaded.
        private void LoadConfigurations()
        {
            string directoryName = GetPath();

            // first try base file.
            string xmlDocFile;

            if (configuration_file == null)
            {
                xmlDocFile = directoryName + @"\" + _XML_CONFIG_FILE;
            }
            else
            {
                xmlDocFile = configuration_file;
            }

            // if file does not exists return.
            XmlDocument confXml = null;
            if (!File.Exists(xmlDocFile))
            {
                // try from teh configuration 
                confXml = GetFromApplicationConfigFile();
            }
            else
            {
                confXml = new XmlDocument();
                confXml.Load(xmlDocFile);
            }

            if (confXml != null && confXml.DocumentElement != null) 
            {
                if (confXml.DocumentElement.LocalName == "dataSettings")
                {
                    this.configuration = new LegacyORMConfiguration();
                }
                else if (confXml.DocumentElement.LocalName == "ORMConfiguration")
                {
                    string version = Utility.GetAttributeValue(confXml.DocumentElement, "version");
                    if (version == "1.0")
                    {
                        this.configuration = new ORMConfigurationV1();
                    }
                }


                if (configuration != null)
                {
                    configuration.ReadConfiguration(confXml);
                    configuration.Validate();
                }
                else
                {
                    configuration = new EmptyORMConfiguration();
                }
            }
            else
            {
                configuration = new EmptyORMConfiguration();
            }

          
        }

        private XmlDocument GetFromApplicationConfigFile()
        {
#if NET11
            object obj = ConfigurationSettings.GetConfig("Silvtek.ORMEngine");
#else
            object obj = ConfigurationManager.GetSection("Silvtek.ORMEngine");
#endif

            if (obj == null) return null;

            string xml = Convert.ToString(obj);
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            return document;
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
