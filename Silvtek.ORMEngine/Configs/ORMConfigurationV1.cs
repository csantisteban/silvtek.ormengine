using System;
using System.Xml;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Configs
{
    internal class ORMConfigurationV1 : ORMConfigurationBase
    {
        

        public override void ReadConfiguration(XmlDocument document)
        {
            if (document.DocumentElement == null || document.DocumentElement.LocalName != "Silvtek.ORMEngine")
            {
                throw new ORMConfigurationException("Silvtek.ORMEngine parent is missing");
            }

            string version = Utility.GetAttributeValue(document.DocumentElement, "version");
            if (version != "1.0")
            {
                throw new InvalidOperationException("Wrong ORMConfiguration reader laoded for configuration schema version 1.0. This is a defect in the API");
            }

            // read global settings

            MapFilePath = Utility.GetAttributeValue(document.DocumentElement, "global/map-file", "path");
            BalancerShare = Utility.GetAttributeValue(document.DocumentElement, "global/shared", "path");
            WorkingFolder = Utility.GetAttributeValue(document.DocumentElement, "global/working-folder", "path");
            TempFolder = Utility.GetAttributeValue(document.DocumentElement, "global/temporary-folder", "path");

            string tmpGlobalCache = Utility.GetAttributeValue(document.DocumentElement, "global/cache", "enabled");
            string tmpBroadcastingEnabled = Utility.GetAttributeValue(document.DocumentElement, "global/broadcast-changes", "enabled");
            string defaultQueryCacheTime = Utility.GetAttributeValue(document.DocumentElement, "global/query", "cache-limit-ms");
            string bufferSize = Utility.GetAttributeValue(document.DocumentElement, "global/buffer", "size");
            string tempEncoding = Utility.GetAttributeValue(document.DocumentElement, "global/encoding", "value");


            SetGlobalCacheEnable(tmpGlobalCache);
            SetBroadCaseEnable(tmpBroadcastingEnabled);
            SetQueryCacheTime(defaultQueryCacheTime);
            SetBufferSize(bufferSize);
            SetEncoding(tempEncoding);

            // now, read connections

            // load declared connections.
            XmlNodeList connectionNodes = document.DocumentElement.SelectNodes("connections/connection");

            if (connectionNodes != null)
            {
                /* Interact thru XML Doc */
                foreach (XmlNode node in connectionNodes)
                {
                    ConnectionSettings settings = CreateConnectionSettings(node);
                    if (settings != null)
                    {
                        this.RegisterConnection(settings);
                    }
                }
            }

        }

        internal ConnectionSettings CreateConnectionSettings(XmlNode node)
        {
            ConnectionSettings settings = new ConnectionSettings();

            settings.ConnectionName = Utility.GetAttributeValue(node, "name");
            settings.IsDefault = Utility.GetAttributeValueAsBool(node, "default", false);
            settings.Timeout = Utility.GetAttributeValueAsInt32(node, "timeout", false);
            settings.CommandTimeout = Utility.GetAttributeValueAsInt32(node, "command-timeout", false);
            settings.PreparedStatements = Utility.GetAttributeValueAsBool(node, "prepared-statement", false, false);
            settings.ConnectionString = Utility.GetNodeValue(node, "connection-string");

            XmlNode connectionProviderNode = node.SelectSingleNode("provider");

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


            XmlNode formatterProviderNode = node.SelectSingleNode("formatter");


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
    }
}
