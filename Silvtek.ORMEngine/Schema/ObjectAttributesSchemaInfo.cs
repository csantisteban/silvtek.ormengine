#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text;
using Silvtek.ORMEngine.Compression;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Reads the mapping attributes of an object.
    /// </summary>
    [Serializable]
    public class ObjectAttributesSchemaInfo : ObjectSchemaInfo
    {
        private string tableName;
        private readonly MapMemberList memberlist;
       // private readonly AliasMaps aliasMaps;

        private readonly Type type;
        private bool isValid;
        private bool isCacheDisabled;

        private enum MappingType : byte
        {
            Column = 1,
            Query  = 2,
            Undefined
        }

        public override bool IsValid
        {
            get { return isValid; }
        }

        public override MapMemberList MappedMembers
        {
            get { return memberlist; }
        }

        public override bool IsCacheDisabled
        {
            get { return isCacheDisabled;; }
        }


        public override string TableName
        {
            get { return tableName ; }
        }

        public override Type Type
        {
            get { return type; }
        }
        

        internal ObjectAttributesSchemaInfo(Type source)
        {
            Debug.Assert(source != null, "source is null");
            type = source;
            isValid = true;
            memberlist = new MapMemberList();
            isCacheDisabled = false;
          //  aliasMaps = new AliasMaps();
        }

        internal override void Load()
        {
            TableAttribute[] tableAttribute = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true);

            if (tableAttribute.Length == 0)
            {
                isValid = false;
                return;
            }

            tableName = tableAttribute[0].Table;
            isCacheDisabled = tableAttribute[0].IsCachingDisabled;

            if (tableAttribute[0].ConnectionName != null && tableAttribute[0].ConnectionName.Length > 0)
            {
                SetConnectionName(tableAttribute[0].ConnectionName);
            }

            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

            for (int i = 0; i < members.Length; i++)
            {
                
                MemberInfo memberinfo = members[i];

                // Continue when the member is not a property or a field.
                if ((memberinfo.MemberType != MemberTypes.Property) && (memberinfo.MemberType != MemberTypes.Field)) continue;
                

                MappingType memberType = GetMemberType(memberinfo);
                
                if (memberType == MappingType.Undefined) continue;

                if (memberType == MappingType.Column)
                {
                    // validate, properties must have get and set.
                    ValidateMember(memberinfo);
                    MapMemberProperties properties = GetProperties(memberinfo);
                    memberlist.Add(memberinfo, properties, DbTypeFormatter);
                  //  queryNames.Add(memberinfo.Name.ToLower(), memberinfo.Name);
                }
                else if (memberType == MappingType.Query)
                {
                    string mappedToMember = GetQueryColumnValue(memberinfo);
                    memberlist.Alias.Add(memberinfo.Name, mappedToMember);
                }

            }

        }

        private MapMemberProperties GetProperties(MemberInfo memberinfo)
        {
            // Process the column attribute.
            ColumnAttribute[] columnAttribute = (ColumnAttribute[])memberinfo.GetCustomAttributes(typeof(ColumnAttribute), true);

            if (columnAttribute.Length > 1)
            {
                string exMsg = Utility.GetResourceString("Schema:MappedMember:TOO_MANY_COLUMN_ATTR_SAME_MEMBER");
                throw new ORMException(exMsg);
            }

            if (columnAttribute.Length == 0) return new MapMemberProperties(); // do nothing. it is not a valid column.

            AliasAttribute[] aliasAttributes = (AliasAttribute[])memberinfo.GetCustomAttributes(typeof(AliasAttribute), true);

            if (columnAttribute.Length == 0 && aliasAttributes.Length > 0)
            {
                throw new ORMException("Alias Attribute is allowed only for members that has the ColumnAttribute");
            }

            // Process the null attribute
            NullAttribute[] isnullAttributes = (NullAttribute[])memberinfo.GetCustomAttributes(typeof(NullAttribute), true);

            // Process the unique attribute.
            UniqueAttribute[] uniqueAttributes = (UniqueAttribute[])memberinfo.GetCustomAttributes(typeof(UniqueAttribute), true);

            // Process Primary Key attribute.
            PrimaryKeyAttribute[] pkAttributes = (PrimaryKeyAttribute[])memberinfo.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);

            // Process sequence attribute.
            SequenceAttribute[] secAttributes = (SequenceAttribute[])memberinfo.GetCustomAttributes(typeof(SequenceAttribute), true);

            CompressAttribute[] compressAttributes = (CompressAttribute[])memberinfo.GetCustomAttributes(typeof(CompressAttribute), true);
            EncryptAttribute[] encryptAttributes = (EncryptAttribute[])memberinfo.GetCustomAttributes(typeof(EncryptAttribute), true);
            RecordDigestAttribute[] digestAttributes = (RecordDigestAttribute[])memberinfo.GetCustomAttributes(typeof(RecordDigestAttribute), true);
           
            // Process MSSequence Attribute. This attribute marks a member as Microsoft Sql Server sequence.
            DatabaseIdentityAttribute[] databaseIdentityAttributes = (DatabaseIdentityAttribute[])memberinfo.GetCustomAttributes(typeof(DatabaseIdentityAttribute), true);

            ColumnAttribute column = columnAttribute[0];

            MapMemberProperties properties = new MapMemberProperties();
            properties.SetColumnModeType(column.ModeType);

            if (column.DataType == DataType.Object)
            {
                // check if a formatter has been provided;
                ObjectFormatterProviderAttribute[] formatterAttribute = (ObjectFormatterProviderAttribute[])memberinfo.GetCustomAttributes(typeof(ObjectFormatterProviderAttribute), true);
                if (formatterAttribute.Length > 0)
                {
                    IFormatter formatter = formatterAttribute[0].GetFormatter();
                    properties.SetObjectFormatter(formatter);
                }
                else
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    properties.SetObjectFormatter(formatter);
                }
            }

            properties.SetDataType(memberinfo, column.DataType);

            memberlist.Alias.Add(column.Name, memberinfo.Name);
            if (aliasAttributes.Length > 0)
            {
                foreach (AliasAttribute alias in aliasAttributes)
                {
                    memberlist.Alias.AddRange(alias.GetAliases(), memberinfo.Name);
                }
            }
            
#if NET20
            //properties.ColumnName = column.Name;
            //properties.EnumConversionType = column.EnumerationConversion;
            //properties.EnumerationType = column.EnumerationType;
            //properties.DefaultValue = column.Default;
            //properties.Length = column.Length;
            //properties.AcceptNulls = (isnullAttributes.Length > 0);
            //properties.IsUnique = (uniqueAttributes.Length > 0);
            //properties.IsPrimaryKey = (pkAttributes.Length > 0);
            //properties.UseDatabaseIdentity = (databaseIdentityAttributes.Length > 0);
            //properties.Table = tableName;
            //properties.FileExtension = column.FileExtension;
#endif
            if (column is TimeStampAttribute)
            {
                TimeStampAttribute timeStampAttribute = (TimeStampAttribute) column;
                properties.SetIsTimeStamp(true, timeStampAttribute.Syncfield);
            }

            properties.SetEnumConvertionType(column.EnumerationConversion);
            properties.SetEnumerationType(column.EnumerationType);
            properties.SetDefaultValue(column.Default);
            properties.SetLength(column.Length);
            properties.SetAcceptNulls(isnullAttributes.Length > 0);
            properties.SetIsUnique(uniqueAttributes.Length > 0);
            properties.SetIsPrimaryKey(pkAttributes.Length > 0);
            properties.SetUseDatabaseIdentity(databaseIdentityAttributes.Length > 0);
            properties.SetTable(tableName);
            properties.SetColumnName(column.Name);
            properties.SetFileExtension(column.FileExtension);

            if (column.Encoding == null || column.Encoding == Encoding.Default)
            {
                properties.SetEncoding(ORMConfigurator.DefaultEncoding);
            }
            else
            {
                properties.SetEncoding(column.Encoding);
            }

            if (secAttributes.Length > 0)
            {
                SequenceProvider provider = secAttributes[0].GetProvider();
                properties.SetSequenceProvider(provider);
            }

            if (compressAttributes.Length > 0)
            {
                CompressionProvider compressionProvider = compressAttributes[0].GetProvider();
                if (compressionProvider != null)
                {
                    properties.SetCompressionProvider(compressionProvider);
                }
            }


            if (encryptAttributes.Length > 0)
            {
                EncryptionProvider encryptionProvider = encryptAttributes[0].GetProvider();
                if (encryptionProvider != null)
                {
                    properties.SetEncryptionProvider(encryptionProvider);
                }
            }

            //
            if (digestAttributes.Length > 0)
            {
                RecordDigestProvider recordDigestProvider = digestAttributes[0].GetProvider();
                if (recordDigestProvider != null)
                {
                    properties.SetRecordDigestProvider(recordDigestProvider);
                }
            }
            return properties;
        }

        private string GetQueryColumnValue(MemberInfo memberInfo)
        {

            QueryColumnAttribute[] queryColumnAttributes = (QueryColumnAttribute[])memberInfo.GetCustomAttributes(typeof(QueryColumnAttribute), true);
            Debug.Assert(queryColumnAttributes.Length > 0, "DEV - Member does not have QueryColumn Attribute.");

            return queryColumnAttributes[0].MemberName;
        }


        private MappingType GetMemberType(MemberInfo memberInfo)
        {
            QueryColumnAttribute[] queryColumnAttributes = (QueryColumnAttribute[])memberInfo.GetCustomAttributes(typeof(QueryColumnAttribute), true);
            ColumnAttribute[] columnAttribute = (ColumnAttribute[])memberInfo.GetCustomAttributes(typeof(ColumnAttribute), true);

            MappingType resultValue = MappingType.Undefined;
            bool isColumn = columnAttribute.Length > 0;
            bool isQuery = queryColumnAttributes.Length > 0;

            if (isColumn && isQuery)
            {
                string exmsg = Utility.GetResourceString("RULES-EXCEPTION:QUERYATTRIBUTE_WITH_COLUMNATTRIBUTE", type.FullName, memberInfo.Name);
                throw new OrmMappingException(exmsg);
            }

            if (isColumn)
            {
                resultValue = MappingType.Column;
            }
            else if (isQuery)
            {
                resultValue = MappingType.Query;
            }

            return resultValue;
        }

        private void ValidateMember(MemberInfo memberinfo)
        {

            if (memberinfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyinfo = (PropertyInfo)memberinfo;

                if (!propertyinfo.CanRead || !propertyinfo.CanWrite)
                {
                    string exMsg = Utility.GetResourceString("Schema:PROPERTY_MUST_HAVE_GET_SET", propertyinfo.Name, propertyinfo.ReflectedType);
                    throw new OrmMappingException(exMsg);
                }
            }
        }
    }
}
