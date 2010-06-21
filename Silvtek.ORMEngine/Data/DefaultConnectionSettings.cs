#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Silvtek.ORMEngine.Data
{
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
        

        public override string ConnectionString
        {
            get { return connectionString; }
        }

        public override string ConnectionName
        {
            get { return connectionName; }
        }

        public override int Timeout
        {
            get { return timeout; }
        }

        public override IFormatter IFormatter
        {
            get { throw new NotImplementedException(); }
        }

        public override string ConnectionProviderAssembly
        {
            get { return connectionProviderAssembly; }
        }

        public override string ConnectionProviderClass
        {
            get { return connectionProviderClass; }
        }

        public override string FormatterProviderAssembly
        {
            get { return formatterProviderAssembly; }
        }

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
            preparedStatements = Utility.GetAttributeValueAsBool(node, "preparedStatements", false, false);
            connectionString = Utility.GetNodeValue(node, "connectionString");

            if (connectionString.Length == 0)
            {
                string exMsg = Utility.GetResourceString("CONNECTION_MANAGER:CONNECTION_STRING_MISSING");
                throw new ORMException(exMsg);
            }

            connectionString = connectionString.Trim();

            XmlNode connectionProviderNode = node.SelectSingleNode("connectionProvider");
            connectionProviderAssembly = Utility.GetAttributeValue(connectionProviderNode, "assembly");
            connectionProviderClass = Utility.GetAttributeValue(connectionProviderNode, "class");

            XmlNode formatterProviderNode = node.SelectSingleNode("dbTypeFormatter");
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
        }


    }
}
