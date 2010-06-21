using System;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using Silvtek.ORMEngine.Compression;
using Silvtek.ORMEngine.Encryption;
using Silvtek.ORMEngine.Schema;
using System.Reflection;

namespace Silvtek.ORMEngine.XmlMapping
{
    /// <summary>
    /// Map an object from an xml file.
    /// </summary>
    public class ObjectXmlSchemaInfo : ObjectSchemaInfo
    {

        #region -- Private Members --

        private string tableName;
        private readonly MapMemberList memberlist;
       // private readonly AliasMaps aliasMaps;
        private readonly Type type;
        private bool isValid;
        private readonly string sourcePath;
        private bool isCacheDisabled;

        #endregion

        // The type of sequential.
        private enum SequentialType : byte
        {
            Sequential = 1,
            DbIdentity = 2,
            None = 4
        }

        /// <summary>
        /// Whether the current map is valid or not.
        /// </summary>
        public override bool IsValid
        {
            get { return isValid; }
        }

        /// <summary>
        /// Gets the list of mapped members.
        /// </summary>
        public override MapMemberList MappedMembers
        {
            get { return memberlist; }
        }

        public override bool IsCacheDisabled
        {
            get { return isCacheDisabled; }
        }


        /// <summary>
        /// Gets the table name.
        /// </summary>
        public override string TableName
        {
            get { return tableName; }
        }


        /// <summary>
        /// Gets the type.
        /// </summary>
        public override Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="sourcePath">The source path.</param>
        public ObjectXmlSchemaInfo(Type type, string sourcePath)
        {
            this.type = type;
            this.sourcePath = sourcePath;
            this.memberlist = new MapMemberList();
            //aliasMaps = new AliasMaps();
            this.isValid = true;
            this.tableName = String.Empty;
        }


        internal override void Load()
        {
            if (!File.Exists(sourcePath))
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:XML_NOT_FOUND", sourcePath);
                throw new ObjectXmlSchemaException(exMsg);
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(sourcePath);
            XmlNode classNode = GetMappingNode(xmlDocument);
            LoadTableNode(classNode);

        }

        private XmlNode GetMappingNode(XmlNode node)
        {
            string classXPath = String.Format("/dataMapping/class[@fullname='{0}']", type.FullName);
            XmlNode tempNode = node.SelectSingleNode(classXPath);

            if (tempNode == null)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:CLASS_MAP_NOT_FOUND", type.FullName, sourcePath);
                throw new ObjectXmlSchemaException(exMsg); 
            }

            return tempNode;
        }

        private void LoadTableNode(XmlNode node)
        {
            XmlNode tableNode = node.SelectSingleNode("table");

            if (tableNode == null)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:CLASS_TABLE_NOT_DEFINED", type.FullName, sourcePath);
                throw new ObjectXmlSchemaException(exMsg); 
            }

            string tmpIsCacheDisabled = GetAttributeValue(tableNode, "disable-cache");

            if (tmpIsCacheDisabled.Length == 0)
            {
                isCacheDisabled = false;
            }
            else
            {
                try
                {
                    isCacheDisabled = Convert.ToBoolean(tmpIsCacheDisabled);
                }
                catch
                {
                    isCacheDisabled = false;
                }
            }

            tableName = GetAttributeValue(tableNode, "name");
            string connectionName = GetAttributeValue(tableNode, "connection-name");

            if (connectionName.Length == 0)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:CONNECTION-NAME_ATTRIBUTE_MISSING", type.FullName, sourcePath);
                throw new ObjectXmlSchemaException(exMsg);
            }

            SetConnectionName(connectionName);

            if (tableName.Length == 0)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:NAME_ATTRIBUTE_MISSING", type.FullName, sourcePath);
                throw new ObjectXmlSchemaException(exMsg);
            }

            LoadMappedMembers(node);
            
        }



        // Load and map ORM members.
        private void LoadMappedMembers(XmlNode node)
        {
            // xpath to load map
            const string xpath = "map";
            XmlNodeList mapNodeList = node.SelectNodes(xpath);
            if (mapNodeList == null)
            {
                isValid = false;
                return;
            }

            // iterate thru all mapped members.
            foreach (XmlNode currentNode in mapNodeList)
            {
                string memberName = GetAttributeValue(currentNode, "member-name");

                // Find member.
                MemberInfo[] memberInfoList = type.GetMember(memberName, MemberTypes.Property | MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (memberInfoList.Length == 0)
                {
                    string exMsg = Utility.GetResourceString("XMLSCHEMA:NOT_FOUND_MEMBER", memberName, type.FullName);
                    throw new OrmMappingException(exMsg);
                }

                MemberInfo memberInfo = memberInfoList[0];
                ValidateMember(memberInfo);

                MapMemberProperties mappedMemberProperties = new MapMemberProperties();

                mappedMemberProperties.SetDataType(memberInfo, GetAttributeDataType(currentNode) );
                
                int length = GetAttributeValueAsInt32(currentNode, "length");
                string tmpDefaultValue = GetAttributeValue(currentNode, "default-value");
                object defaultValue = (tmpDefaultValue.Length > 0) ? tmpDefaultValue : null;
                bool acceptNulls = GetAttributeValueAsBool(currentNode, "accepts-null");
                bool isPk = GetAttributeValueAsBool(currentNode, "is-key");
                string columnName = GetAttributeValue(currentNode, "column-name");
                
#if NET20

                mappedMemberProperties.SetDefaultValue(defaultValue);
                mappedMemberProperties.Length = length;  
                mappedMemberProperties.AcceptNulls = acceptNulls;
                mappedMemberProperties.IsPrimaryKey = isPk;
                mappedMemberProperties.ColumnName = columnName;
                mappedMemberProperties.Table = tableName;
#endif

#if NET11
                mappedMemberProperties.SetLength(length);
                mappedMemberProperties.SetDefaultValue(defaultValue);
                mappedMemberProperties.SetAcceptNulls(acceptNulls);
                mappedMemberProperties.SetIsPrimaryKey(isPk);
                mappedMemberProperties.SetColumnName(columnName);
                mappedMemberProperties.SetTable(tableName);

#endif
                SequentialType sequentialType = GetAttributeValueAsSequentialType(currentNode, "sequential-type");

                if (sequentialType == SequentialType.Sequential)
                {
                    XmlNode sequenceProviderNode = currentNode.SelectSingleNode("sequenceProvider");
                    if (sequenceProviderNode != null)
                    {
                        string providerClassName = GetAttributeValue(sequenceProviderNode, "class");
                        string providerAssemblyName = GetAttributeValue(sequenceProviderNode, "assembly");

                        Assembly providerAssembly = Assembly.Load(providerAssemblyName);
                        Type providerType = providerAssembly.GetType(providerClassName);
                        SequenceProvider provider = (SequenceProvider)Activator.CreateInstance(providerType);
                        mappedMemberProperties.SetSequenceProvider(provider);

                    }
                }
                else if (sequentialType == SequentialType.DbIdentity)
                {
#if NET20
                    mappedMemberProperties.UseDatabaseIdentity = true;
#endif
#if NET11
                    mappedMemberProperties.SetUseDatabaseIdentity(true);
#endif
                }


                XmlNode enumInfoNode = currentNode.SelectSingleNode("enumeration");

                if (enumInfoNode != null)
                {
#if NET20
                    mappedMemberProperties.EnumerationType = GetAttributeValueAsType(enumInfoNode, "type");
                    mappedMemberProperties.EnumConversionType = GetAttributeValueAsEnumerationConvertionType(enumInfoNode, "convert-by");
#endif
#if NET11
                    mappedMemberProperties.SetEnumerationType( GetAttributeValueAsType(enumInfoNode, "type") );
                    mappedMemberProperties.SetEnumConvertionType ( GetAttributeValueAsEnumerationConvertionType(enumInfoNode, "convert-by") );
#endif

                }

#if NET20
                XmlNode compressionNode = currentNode.SelectSingleNode("compressProvider");
                if (compressionNode != null)
                {

                    string providerClassName = GetAttributeValue(compressionNode, "class");
                    string providerAssemblyName = GetAttributeValue(compressionNode, "assembly");

                    if (providerClassName.Length > 0)
                    {
                        Assembly providerAssembly = Assembly.Load(providerAssemblyName);
                        Type providerType = providerAssembly.GetType(providerClassName);
                        CompressionProvider provider = (CompressionProvider) Activator.CreateInstance(providerType);
                        mappedMemberProperties.SetCompressionProvider(provider);
                    }
                    else
                    {
                        DefaultCompressionProvider defaultProvider = new DefaultCompressionProvider();
                        mappedMemberProperties.SetCompressionProvider(defaultProvider);
                    }

                }
#endif

                XmlNode encryptNode = currentNode.SelectSingleNode("encryptionProvider");
                if (encryptNode != null)
                {

                    string providerClassName = GetAttributeValue(encryptNode, "class");
                    string providerAssemblyName = GetAttributeValue(encryptNode, "assembly");
                    string password = GetAttributeValue(encryptNode, "password");
                    string salt = GetAttributeValue(encryptNode, "salt");

                    if (providerClassName.Length > 0)
                    {
                        Assembly providerAssembly = Assembly.Load(providerAssemblyName);
                        Type providerType = providerAssembly.GetType(providerClassName);
                        EncryptionProvider provider = (EncryptionProvider) Activator.CreateInstance(providerType);
                        provider.Password = password;
                        provider.Salt = salt;
                        mappedMemberProperties.SetEncryptionProvider(provider);
                    }
                    else
                    {
                        DefaultEncryptionProvider defaultProvider = new DefaultEncryptionProvider();
                        defaultProvider.Password = password;
                        defaultProvider.Salt = salt;
                        mappedMemberProperties.SetEncryptionProvider(defaultProvider);
                    }

                }

                XmlNodeList queryNameNodeList = currentNode.SelectNodes("queryAlias");

                StringCollection queryNamesList = new StringCollection();
                if (queryNameNodeList != null && queryNameNodeList.Count > 0)
                {
                    foreach (XmlNode queryNameNode in queryNameNodeList)
                    {
                        string queryNameValue = GetAttributeValue(queryNameNode, "name");
                        queryNamesList.Add(queryNameValue);
                    }
                }

                memberlist.Add(memberInfo, mappedMemberProperties, DbTypeFormatter);
            }
        }

        private DataType GetAttributeDataType(XmlNode currentNode)
        {
            DataType datatype;
            string datatypeTemp = String.Empty;
            try
            {
                datatypeTemp = GetAttributeValue(currentNode, "datatype");

                if (datatypeTemp.Length == 0) return DataType.Undefined;

                datatype = (DataType)Enum.Parse(typeof(DataType), datatypeTemp, true);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:INVALID_DATATYPE", datatypeTemp, type.FullName);
                throw new OrmMappingException(exMsg);
            }

            return datatype;
        }

        private static void ValidateMember(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                if (!propertyInfo.CanRead && !propertyInfo.CanWrite)
                {
                    string exMsg = Utility.GetResourceString("Schema:PROPERTY_MUST_HAVE_GET_SET", propertyInfo.Name, propertyInfo.ReflectedType);
                    throw new OrmMappingException(exMsg);
                }
            }
        }


        private static string GetAttributeValue(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if (attribute == null) return String.Empty;

            string tempValue = attribute.Value;
            if (tempValue == null) tempValue = String.Empty;

            return tempValue;
        }

        private static int GetAttributeValueAsInt32(XmlNode node, string attributeName)
        {
            string value = GetAttributeValue(node, attributeName);
            if (value.Length == 0) return 0;

            int retValue = 0;

            try
            {
                retValue = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:INVALID_VALUE_INT32", attributeName, value);
                throw new ObjectXmlSchemaException(exMsg);
            }

            return retValue;
        }

        private static bool GetAttributeValueAsBool(XmlNode node, string attributeName)
        {
            string value = GetAttributeValue(node, attributeName);
            if (value.Length == 0) return false;

            bool retValue = false;
            try
            {
                retValue = Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:INVALID_VALUE_BOOLEAN", attributeName, value, node.OuterXml);
                throw new ObjectXmlSchemaException(exMsg);
            }

            return retValue;
        }

        private static SequentialType GetAttributeValueAsSequentialType(XmlNode node, string attributeName)
        {
            string value = GetAttributeValue(node, attributeName);
            if (value.Length == 0) return SequentialType.None;

            SequentialType retValue = SequentialType.None;

            try
            {
                retValue = (SequentialType)Enum.Parse(typeof(SequentialType), value);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA:INVALID_SEQUENTIAL_TYPE", value);
                throw new ObjectXmlSchemaException(exMsg);
            }

            return retValue;
        }

        private static Type GetAttributeValueAsType(XmlNode node, string attributeName)
        {
            string value = GetAttributeValue(node, attributeName);
            if (value.Length == 0) return null;

            Type type = null;

            try
            {
                type = Type.GetType(value);    
            }
            catch (Exception e)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA_FAILED_TO_LOAD_TYPE", value, e.Message);
                throw new ObjectXmlSchemaException(exMsg);
            }

            if (type == null)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA_UNABLE_TO_LOAD_ENUM", value);
                throw new ObjectXmlSchemaException(exMsg);
            }

            if (!type.IsEnum)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA_INVALID_ENUMERATION_TYPE", value);
                throw new ObjectXmlSchemaException(exMsg);
            }
            
            return type;
        }

        private EnumConversionType GetAttributeValueAsEnumerationConvertionType(XmlNode node, string attributeName)
        {
            string value = GetAttributeValue(node, attributeName);
            if (value.Length == 0) return EnumConversionType.ByName;

            EnumConversionType retValue = EnumConversionType.ByName;

            try
            {
                retValue = (EnumConversionType)Enum.Parse(typeof(EnumConversionType), value);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("XMLSCHEMA_INVALID_ENUMERATION_CONVERTION_TYPE", value);
                throw new ObjectXmlSchemaException(exMsg);
            }

            return retValue;
        }
    }
}
