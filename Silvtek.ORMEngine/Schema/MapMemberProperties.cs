#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Silvtek.ORMEngine.Compression;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Represents the properties for a mapped member (Field or property)
    /// </summary>
    public sealed class MapMemberProperties
    {
        #region ::- Private Variables ::-

        private bool acceptNulls; // tell us if the column accept null.
        private string columnName;
        private DataType dataType;
        private object defaultValue;
        private bool defaultDefined;
        private EnumConversionType enumConversionType;
        private Type enumType;
        private bool isPrimaryKey; // tell us if the column is a primary key

        private bool isRulesApplied;
        private bool isUnique; // tell us if the column must be unique (No Implemented yet)
        private int length;
        
        private string table;
        private bool useDatabaseIdentity;
        private string fileExtension;
        private CompressionProvider compressionProvider;
        private EncryptionProvider encryptionProvider;
        private SequenceProvider sequenceProvider; // tells if the column is sequential
        private RecordDigestProvider recordDigestProvider;
        private IFormatter objectFormatter;
        private ColumnModeType modeType;

        private MemberAttributes attributes;
        

        #endregion

        #region ::- Properties -::

        internal ColumnModeType ModeType
        {
            get { return modeType; }
            set { modeType = value; }
        }

        public string ColumnName
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return columnName;
            }
#if NET20
            internal set
            {
            //    Debug.Assert((value != null && value.Length > 0), "Setting a empty string as column name");
                columnName = (value == null || value.Length == 0) ? String.Empty : value;
            }
#endif
        }

        public string Table
        {
            get { return table; }
#if NET20
            internal set { table = (value == null || value.Length == 0) ? String.Empty : value; }
#endif
        }

        /// <summary>
        /// Gets whether nulls are allowed or not.
        /// </summary>
        public bool AcceptNulls
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return acceptNulls;
            }
#if NET20
            internal set
            {
                SetAcceptNulls(value);
            }
#endif
        }

        ///<summary>
        /// Gets whether the column check for value uniquess.
        ///</summary>
        public bool IsUnique
        {
            get
            {
                VerifyRulesHasBeenApplied();

                return isUnique;
            }
#if NET20
            internal set { SetIsUnique(value); }
#endif
        }

        internal string FileExtension
        {
           get { return fileExtension;}
#if NET20
           set { fileExtension = value ?? "bin"; }
#endif
        }


        internal bool ContainsAttribute(MemberAttributes attribute)
        {
            return ((MapMemberAttributes & attribute) == attribute);
        }


        ///<summary>
        /// Gets whether the current member is a primary key.
        ///</summary>
        public bool IsPrimaryKey
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return isPrimaryKey;
            }
#if NET20
            internal set { SetIsPrimaryKey(value); }
#endif
        }

        [Obsolete("Use GetSequenceProvider instead")]
        internal SequenceProvider SequenceProvider
        {
            get { return sequenceProvider; }
        }

        internal SequenceProvider GetSequenceProvider()
        {
            return sequenceProvider;
        }

        internal CompressionProvider GetCompressionProvider()
        {
            return compressionProvider;
        }

        internal EncryptionProvider GetEncryptionProvider()
        {
            return encryptionProvider;
        }

        internal RecordDigestProvider GetRecordDigestProvider()
        {
            return recordDigestProvider;
        }

        internal IFormatter GetObjectFormatter()
        {
             return objectFormatter; 
        }

        /// <summary>
        /// Gets the datatype of the current member.
        /// </summary>
        public DataType DataType
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return dataType;
            }
        }

        ///<summary>
        /// Gets the column length.
        ///</summary>
        public int Length
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return length;
            }
#if NET20
            internal set { length = value; }
#endif
        }

        /// <summary>
        /// Gets whether whether this column is a SqlServerSequential (Identity).
        /// </summary>
        public bool UseDatabaseIdentity
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return useDatabaseIdentity;
            }
#if NET20
            internal set { SetUseDatabaseIdentity(value); }
#endif
        }

        /// <summary>
        /// Gets the default value for current member.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                VerifyRulesHasBeenApplied();

                return defaultValue;
            }
//#if NET20
//            internal set { defaultValue = value; }
//#endif
        }

        ///<summary>
        /// Gets the enumeration <see cref="Type" /> that the value of the current
        /// member will be converted.
        ///</summary>
        public Type EnumerationType
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return enumType;
            }
#if NET20
            internal set { enumType = value; }
#endif
        }

        /// <summary>
        /// Gets the convertion type for the enumeration value.
        /// </summary>
        public EnumConversionType EnumConversionType
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return enumConversionType;
            }
#if NET20
            internal set { enumConversionType = value; }
#endif
        }

        /// <summary>
        /// Gets whether a sequence provider object is attached to current member
        /// </summary>
        public bool HasSequenceProvider
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return (sequenceProvider != null);
            }
        }

        internal bool HasRecordDigestProvider
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return (recordDigestProvider != null);
            }
        }

        public bool IsCompressionEnabled
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return (compressionProvider != null);               
            }

        }

        public bool IsEncryptionEnabled
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return (encryptionProvider != null);
            }

        }

        public bool IsRecordDigestEnabled
        {
            get
            {
                VerifyRulesHasBeenApplied();
                return (recordDigestProvider != null);
            }

        }

        internal MemberAttributes MapMemberAttributes
        {
            get { return attributes; }
        }

        public bool DefaultDefined
        {
            get { return defaultDefined; }
        }

        #endregion

        #region ::- Internal Methods -::

        /// <summary>
        /// Sets a sequence provider.
        /// </summary>
        /// <param name="sequence"></param>
        internal void SetSequenceProvider(SequenceProvider sequence)
        {
            sequenceProvider = sequence;
        }

        internal void SetCompressionProvider(CompressionProvider provider)
        {
            compressionProvider = provider;
        }

        internal void SetEncryptionProvider(EncryptionProvider provider)
        {
            encryptionProvider = provider;
        }

        internal void SetObjectFormatter(IFormatter formatter)
        {
            objectFormatter = formatter;
        }

        internal void SetRecordDigestProvider(RecordDigestProvider provider)
        {
            recordDigestProvider = provider;
        }

        internal void SetDataType(MemberInfo info, DataType dataType)
        {
            if (dataType != DataType.Undefined)
            {
                ValidateDataType(info, dataType);
                this.dataType = dataType;
            }
            else
            {
                this.dataType = GetDataTypeFromMember(info);
            }
        }

        #endregion

        /// <summary>
        /// Constructor. Initialize object to its default value.
        /// </summary>
        internal MapMemberProperties()
        {
            acceptNulls = true;
            isPrimaryKey = false;
            isUnique = false;
            useDatabaseIdentity = false;
            length = -1;
            dataType = DataType.Undefined;
            defaultValue = String.Empty;
            enumConversionType = EnumConversionType.ByName;
            enumType = null;
            isRulesApplied = false;
            columnName = String.Empty;
            fileExtension = "bin"; // default extension;
            attributes = MemberAttributes.None;
            defaultDefined = false;
        }

        /// <summary>
        /// Applys mapping rules.
        /// </summary>
        internal void ApplyRules()
        {
            if (isRulesApplied) return; // Do not applied the rules twice.

            if (isPrimaryKey && acceptNulls)
            {
                string exMsg = Utility.GetResourceString("RULES-EXCEPTION:PRIMARY_KEY_NO_ACCEPT_NULLS");
                throw new OrmMappingException(exMsg);
            }

            if (sequenceProvider != null && useDatabaseIdentity)
            {
                string exMsg = Utility.GetResourceString("RULES-EXCEPTION:IDENTITY_OR_SEQUENTIAL");
                throw new ORMException(exMsg);
            }

            if (isUnique && acceptNulls)
            {
                string exMsg = Utility.GetResourceString("RULES-EXCEPTION:UNIQUE_NO_ACCEPT_NULLS");
                throw new ORMException(exMsg);
            }

            if (compressionProvider != null && dataType != DataType.Binary)
            {
                string exmMsg = String.Format("Compression can only apply to DataType.Binary.");
                throw new ORMException(exmMsg);
            }

            

            // set the default value to null, if the default value is empty string.
            if (dataType == DataType.Xml && Convert.ToString(defaultValue).Length == 0)
            {
                defaultValue = null;
            }

            isRulesApplied = true;
        }

        [Conditional("DEBUG")]
        private void VerifyRulesHasBeenApplied()
        {
            if (!isRulesApplied)
            {
                string exMsg = Utility.GetResourceString("DEV-EXCEPTION:RULES_NOT_APPLIED");
                Debug.WriteLine(exMsg);
                throw new InvalidOperationException(exMsg);
            }
        }

        #region ::- Internal Setters Methods for NET 1.1 Framework. -::

//#if NET11

        internal void SetAcceptNulls(bool value)
        {
            if (value)
            {
                attributes = MapMemberAttributes | MemberAttributes.AllowNulls;
            }

            acceptNulls = value;
        }

        internal void SetIsUnique(bool value)
        {
            if (value)
            {
                attributes = MapMemberAttributes | MemberAttributes.Unique;
            }

            isUnique = value;
        }

        internal void SetIsPrimaryKey(bool value)
        {
            if (value)
            {
                attributes = MapMemberAttributes | MemberAttributes.PrimaryKey;
            }

            isPrimaryKey = value;
        }
       
        internal void SetUseDatabaseIdentity(bool value)
        {
            if (value)
            {
                attributes = MapMemberAttributes | MemberAttributes.DatabaseIdentity;
            }

            useDatabaseIdentity = value;
        }

        //internal void SetDataType(DataType value)
        //{
        //    datatype = value;
        //}

        internal void SetDefaultValue(object value)
        {
            if (value != null)
            {
                defaultDefined = true;
                defaultValue = value;
            }
            else
            {
                defaultDefined = false;
                defaultValue = MapUtility.GetMappedDefaultValue(dataType);
            }
            
        }

        internal void SetLength(int value)
        {
            length = value;
        }

        internal void SetFileExtension(string value)
        {
            fileExtension = value;
        }

        internal void SetEnumerationType(Type value)
        {
            enumType = value;  
        }

        internal void SetEnumConvertionType(EnumConversionType value)
        {
            enumConversionType = value;
        }

        internal void SetIsTimeStamp(bool value, bool syncField )
        {
            attributes = MapMemberAttributes | MemberAttributes.Timestamp;
            if (syncField)
            {
                attributes = MapMemberAttributes | MemberAttributes.SyncField;
            }
        }


        internal void SetColumnName(string value)
        {
            columnName = (value == null || value.Length == 0) ? String.Empty : value;
        }

        internal void SetTable(string value)
        {
            table = (value == null || value.Length == 0) ? String.Empty : value;
        }

//#endif

        #endregion

        #region ::- Private Methods -::

        // Maps the member type to a defined ORM data type.
        private DataType GetDataTypeFromMember(MemberInfo member)
        {
            Type type = GetMemberDeclaredType(member);

            if (type == typeof (String)) return DataType.String;
            if (type.IsEnum) return DataType.Enum;
            if (type == typeof (DateTime)) return DataType.DateTime;
            if (type == typeof (Boolean)) return DataType.Boolean;
            if (type == typeof (Int16)) return DataType.Int16;
            if (type == typeof (Int32)) return DataType.Int32;
            if (type == typeof (Int64)) return DataType.Int64;
            if (type == typeof (Single)) return DataType.Single;
            if (type == typeof (Double)) return DataType.Double;
            if (type == typeof (Byte)) return DataType.Byte;
            if (type == typeof (Decimal)) return DataType.Decimal;
            if (type == typeof (Object)) return DataType.Object;
            if (type == typeof (XmlDocument)) return DataType.Xml;
            if (type == typeof (Guid)) return DataType.Guid;
            if (type == typeof (Byte[])) return DataType.Binary;
            if (type == typeof (Stream)) return DataType.Binary;
            return DataType.Undefined;
        }

        private void ValidateDataType(MemberInfo member, DataType datatype)
        {
            bool isValid = false;

            Type type = GetMemberDeclaredType(member);
            switch (datatype)
            {
                case DataType.String:
                    {
                        isValid = type == typeof (String);
                        break;
                    }
                case DataType.Enum:
                    {
                        isValid = type.IsEnum;
                        break;
                    }
                case DataType.DateTime:
                    {
                        isValid = (type == typeof (DateTime));
                        break;
                    }
                case DataType.Int16:
                    {
                        isValid = (type == typeof (Int16));
                        break;
                    }
                case DataType.Int32:
                    {
                        isValid = (type == typeof (Int32));
                        break;
                    }
                case DataType.Int64:
                    {
                        isValid = (type == typeof (Int64));
                        break;
                    }
                case DataType.Double:
                    {
                        isValid = (type == typeof (Double));
                        break;
                    }
                case DataType.Byte:
                    {
                        isValid = (type == typeof (Byte));
                        break;
                    }
                case DataType.Boolean:
                    {
                        isValid = (type == typeof (Boolean));
                        break;
                    }
                case DataType.Decimal:
                    {
                        isValid = (type == typeof (Decimal));
                        break;
                    }
                case DataType.Single:
                    {
                        isValid = (type == typeof (Single));
                        break;
                    }
                case DataType.Object:
                    {
                       Type typeInterface = type.GetInterface("System.Runtime.Serialization.ISerializable");
                        isValid = (typeInterface != null);
                        break;
                    }
                case DataType.Xml:
                    {
                        isValid = (type == typeof (XmlDocument));
                        break;
                    }
                case DataType.Guid:
                    {
                        isValid = (type == typeof (Guid));
                        break;
                    }
                case DataType.Binary:
                    {
                        isValid = (type == typeof(byte[]) || type == typeof(Stream) || type ==  typeof(string));
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("DataType '" + datatype + "' is not supported");
                    }
            }

            if (!isValid)
            {
                string exMsg = Utility.GetResourceString("MAPPING:DATATYPE_NOT_EQUAL_TO_MEMBERTYPE", datatype, member.Name, member.ReflectedType);
                throw new OrmMappingException(exMsg);
            }
        }

        private Type GetMemberDeclaredType(MemberInfo member)
        {
            Type type = null;
            if (member is FieldInfo)
            {
                FieldInfo field = (FieldInfo) member;
                type = field.FieldType;
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo propertyInfo = (PropertyInfo) member;
                type = propertyInfo.PropertyType;
            }
            else
            {
                throw new InvalidOperationException("the argument 'member' cannot be converted as FieldInfor or PropertyInfo");
            }

            return type;
        }

        #endregion
    }
}