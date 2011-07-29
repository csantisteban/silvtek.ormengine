using System.Xml;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Configs
{
    public class LegacyORMConfiguration : ORMConfigurationBase
    {



        #region -- Private Methods --

        // Load the Connection.config xml file.
        // If the file does not exists; it won't be loaded.
        public override void ReadConfiguration(XmlDocument document)
        {

            LoadWorkingFolder(document);
            LoadMapFilesFolder(document);
            LoadBalancerShareFolder(document);
            LoadGlobalCache(document);
            LoadUpdateBroadcastConfiguration(document);
            LoadBufferSize(document);
            LoadQueryCacheDefaultTime(document);
            LoadEncodingType(document);


            // load declared connections.
            XmlNodeList nodelist = document.SelectNodes("/dataSettings/connection");

            if (nodelist != null)
            {
                /* Interact thru XML Doc */
                foreach (XmlNode node in nodelist)
                {
                    ConnectionSettings settings = CreateConnectionSettings(node);
                    if (settings != null)
                    {
                        this.RegisterConnection(settings);
                    }
                }
            }
        }


        private void LoadMapFilesFolder(XmlNode node)
        {
            const string XPATH = "/dataSettings/mapFiles";
            XmlNode mapFileNode = node.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (mapFileNode == null) return;
            MapFilePath = Utility.GetAttributeValue(mapFileNode, "path");
        }

        private void LoadEncodingType(XmlNode node)
        {
            const string XPATH = "/dataSettings/Encoding";
            XmlNode encodingNode = node.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (encodingNode == null) return;

            string encodingString = Utility.GetAttributeValue(encodingNode, "value");

            SetEncoding(encodingString);

        }

        private void LoadWorkingFolder(XmlNode node)
        {
            const string WORKING_FOLDER_XPATH = "/dataSettings/workingFolder";
            XmlNode tempFileNode = node.SelectSingleNode(WORKING_FOLDER_XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (tempFileNode != null)
            {
                WorkingFolder = Utility.GetAttributeValue(tempFileNode, "path");
            }
        }

        private void LoadBalancerShareFolder(XmlNode docNode)
        {
            const string XPATH = "/dataSettings/sharedResources";
            XmlNode node = docNode.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node != null)
            {
                BalancerShare = Utility.GetAttributeValue(node, "path");
            }

        }

        private void LoadBufferSize(XmlNode docNode)
        {
            const string XPATH = "/dataSettings/BufferSize";
            XmlNode node = docNode.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;

            string tmpBufferSize =  Utility.GetAttributeValue(node, "size");
            SetBufferSize(tmpBufferSize);

        }

        private void LoadQueryCacheDefaultTime(XmlNode docNode)
        {
            const string XPATH = "/dataSettings/DefaultQueryCacheTime";
            XmlNode node = docNode.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            string queryCacheTimeMs = Utility.GetAttributeValue(node, "milliseconds");
            SetQueryCacheTime(queryCacheTimeMs);
        }



        private void LoadGlobalCache(XmlNode docNode)
        {
            const string XPATH = "/dataSettings/globalCache";
            XmlNode node = docNode.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            string tempBool = Utility.GetAttributeValue(node, "enabled");
            SetGlobalCacheEnable(tempBool);
        }

        private void LoadUpdateBroadcastConfiguration(XmlNode docNode)
        {
            const string XPATH = "/dataSettings/UpdateBroadcast";
            XmlNode node = docNode.SelectSingleNode(XPATH);

            // if the mapfiles node doe not exists, then return, there is not xml mapping files.
            if (node == null) return;
            string tempBool = Utility.GetAttributeValue(node, "enabled");
            SetBroadCaseEnable(tempBool);

        }


        internal ConnectionSettings CreateConnectionSettings(XmlNode node)
        {
            ConnectionSettings settings = new ConnectionSettings();

            settings.ConnectionName = Utility.GetAttributeValue(node, "name");
            settings.IsDefault = Utility.GetAttributeValueAsBool(node, "default", false);
            settings.Timeout = Utility.GetAttributeValueAsInt32(node, "timeout", false);
            settings.CommandTimeout = Utility.GetAttributeValueAsInt32(node, "commandTimeout", false);
            settings.PreparedStatements = Utility.GetAttributeValueAsBool(node, "preparedStatements", false, false);
            settings.ConnectionString = Utility.GetNodeValue(node, "connectionString");

            XmlNode connectionProviderNode = node.SelectSingleNode("connectionProvider");

            if (connectionProviderNode == null)
            {
                throw new ORMConfigurationException("the connection/provider node is missing in the configuration");
            }


            string connectionProviderAssembly = Utility.GetAttributeValue(connectionProviderNode, "assembly");
            string connectionProviderClass = Utility.GetAttributeValue(connectionProviderNode, "class");

            if (connectionProviderAssembly.Length == 0 || connectionProviderClass.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_PROVIDER_MISSING", settings.ConnectionName);
                throw new ORMException(exMsg);
            }

            settings.ConnectionProvider = base.CreateConnectionProvider(connectionProviderAssembly, connectionProviderClass);


            XmlNode formatterProviderNode = node.SelectSingleNode("dbTypeFormatter");


            if (connectionProviderNode == null)
            {
                throw new ORMConfigurationException("the connection/formatter node is missing in the configuration");
            }

            string formatterProviderAssembly = Utility.GetAttributeValue(formatterProviderNode, "assembly");
            string formatterProviderClass = Utility.GetAttributeValue(formatterProviderNode, "class");

            if (formatterProviderAssembly.Length == 0 || formatterProviderClass.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:FORMATTER_PROVIDER_MISSING", settings.ConnectionName);
                throw new ORMException(exMsg);
            }

            settings.DbTypeFormatter = base.CreateFormatter(formatterProviderAssembly, formatterProviderClass);


            // load retries
            XmlNode retryExceptionsNode = node.SelectSingleNode("retry-exceptions");

            if (retryExceptionsNode != null)
            {
                RetryExceptions retryExceptions = new RetryExceptions();
                retryExceptions.ReadDefaultXmlNode(retryExceptionsNode);
                settings.RetryExceptions = retryExceptions;
            }

            return settings;
        }






        #endregion
    }
}
