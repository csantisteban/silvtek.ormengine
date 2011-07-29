#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Runtime.Serialization;
using System.Xml;
using Silvtek.ORMEngine.Configs;

namespace Silvtek.ORMEngine.Data
{
    /// <summary>
    /// Represents the default connection settings. This connection setting are loaded from connection.config.
    /// </summary>
    internal class DefaultConnectionSettings : ConnectionSettingsBase
    {
        private readonly string connectionName;
        private readonly string connectionProviderAssembly;
        private readonly string connectionProviderClass;
        private readonly string connectionString;
        private readonly string formatterProviderAssembly;
        private readonly string formatterProviderClass;
        private readonly bool isDefault;
        private readonly int timeout;
        private readonly bool preparedStatements;
        private readonly int commandTimeout;
        private readonly RetryExceptions retryExceptions;


        public DefaultConnectionSettings()
        {
            retryExceptions = new RetryExceptions();

            connectionName = String.Empty;
            connectionProviderAssembly = String.Empty;
            connectionProviderClass = String.Empty;
            connectionString = String.Empty;
            formatterProviderAssembly = String.Empty;
            formatterProviderClass = String.Empty;
            isDefault = false;
            timeout = 90;
            preparedStatements = false;
            commandTimeout = 90;
            
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value></value>
        public override string ConnectionString
        {
            get { return connectionString; }
        }

        /// <summary>
        /// Gets the connection name.
        /// </summary>
        /// <value></value>
        public override string ConnectionName
        {
            get { return connectionName; }
        }

        /// <summary>
        /// Gets the time out.
        /// </summary>
        /// <value></value>
        public override int Timeout
        {
            get { return timeout; }
        }

        /// <summary>
        /// Gets the serialization formatter.
        /// </summary>
        /// <value>The serialization formatter.</value>
        public override IFormatter SerializationFormatter
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the connection provider assembly.
        /// </summary>
        /// <value></value>
        public override string ConnectionProviderAssembly
        {
            get { return connectionProviderAssembly; }
        }

        /// <summary>
        /// Gets the Connection provider full class name.
        /// </summary>
        /// <value></value>
        public override string ConnectionProviderClass
        {
            get { return connectionProviderClass; }
        }

        /// <summary>
        /// Gets the formatter provider assembly.
        /// </summary>
        /// <value>The formatter provider assembly.</value>
        public override string FormatterProviderAssembly
        {
            get { return formatterProviderAssembly; }
        }

        /// <summary>
        /// Gets the formatter provider class.
        /// </summary>
        /// <value>The formatter provider class.</value>
        public override string FormatterProviderClass
        {
            get { return formatterProviderClass; }
        }


        /// <summary>
        /// Gets whether this instance is the default Connection Settings.
        /// </summary>
        public override bool IsDefault
        {
            get { return isDefault; }
        }

        /// <summary>
        /// Gets whether the ORM will create prepared sql commands.
        /// </summary>
        public override bool PreparedStatements
        {
            get { return preparedStatements; }
        }

        /// <summary>
        /// Gets the command timeout.
        /// </summary>
        /// <value>The command timeout.</value>
        public override int CommandTimeout
        {
            get { return commandTimeout; }
        }

        public override RetryExceptions RetryExceptions
        {
            get { return retryExceptions; }
        }


        internal DefaultConnectionSettings(XmlNode node)
        {
            connectionName = Utility.GetAttributeValue(node, "name");
            if (connectionName.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_NAME_MISSING");
                throw new ORMException(exMsg);
            }


            isDefault = Utility.GetAttributeValueAsBool(node, "default", false);
            timeout = Utility.GetAttributeValueAsInt32(node, "timeout", false);
            commandTimeout = Utility.GetAttributeValueAsInt32(node, "command-timeout", false);

            if (commandTimeout == 0)
            {
                commandTimeout = Utility.GetAttributeValueAsInt32(node, "commandTimeout", false);
            }

            if (commandTimeout == 0) commandTimeout = 90;

            preparedStatements = Utility.GetAttributeValueAsBool(node, "prepared-statements", false, false);
            if (!preparedStatements)
            {
                preparedStatements = Utility.GetAttributeValueAsBool(node, "preparedStatements", false, false);
            }
                
             connectionString = Utility.GetNodeValue(node, "connection-string");

            if (connectionString.Length == 0)
            {
                connectionString = Utility.GetNodeValue(node, "connectionString");
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_STRING_MISSING");
                throw new ORMConfigurationException(exMsg);
            }

            connectionString = connectionString.Trim();

            XmlNode connectionProviderNode = node.SelectSingleNode("connectionProvider");
            if (connectionProviderNode == null)
            {
                connectionProviderNode = node.SelectSingleNode("connection-provider");
            }

            if (connectionProviderNode == null)
            {
                throw new ORMConfigurationException("the connection/provider node is missing in the configuration");
            }


            connectionProviderAssembly = Utility.GetAttributeValue(connectionProviderNode, "assembly");
            connectionProviderClass = Utility.GetAttributeValue(connectionProviderNode, "class");



            XmlNode formatterProviderNode = node.SelectSingleNode("dbTypeFormatter");

            if (formatterProviderNode == null)
            {
                formatterProviderNode = node.SelectSingleNode("formatter");
            }

            if (connectionProviderNode == null)
            {
                throw new ORMConfigurationException("the connection/formatter node is missing in the configuration");
            }

            formatterProviderAssembly = Utility.GetAttributeValue(formatterProviderNode, "assembly");
            formatterProviderClass = Utility.GetAttributeValue(formatterProviderNode, "class");

            if (connectionProviderAssembly.Length == 0 || connectionProviderClass.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_PROVIDER_MISSING", connectionName);
                throw new ORMException(exMsg);
            }

            if (formatterProviderAssembly.Length == 0 || formatterProviderClass.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:FORMATTER_PROVIDER_MISSING", connectionName);
                throw new ORMException(exMsg);
            }

            // load retries
            XmlNode retryExceptionsNode = connectionProviderNode.SelectSingleNode("retry-exceptions");
            if (retryExceptionsNode != null)
            {
                this.retryExceptions.ReadDefaultXmlNode(retryExceptionsNode);
            }
        }




    }
}
