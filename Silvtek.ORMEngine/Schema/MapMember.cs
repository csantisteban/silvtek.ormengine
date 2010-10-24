#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Silvtek.ORMEngine.Compression;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine.Schema
{

    /// <summary>
    /// Represents a mapped member.
    /// </summary>
    public class MapMember
    {
        #region -- private members --

        private readonly MemberInfo member;
        private readonly Type mapMemberType;
        private readonly MapMemberProperties properties;
        private readonly DbTypeFormatter formatter;
        private readonly DbType dbType;

        #endregion


        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string MemberName
        {
            get { return member.Name; }
        }

        /// <summary>
        /// Gets the column name to which this member is mapped to.
        /// </summary>
        public string ColumnName
        {
            get { return properties.ColumnName; }
        }

        /// <summary>
        /// Gets the column full name
        /// </summary>
        public string ColumnFullName
        {
            get { return String.Format("{0}.{1}", Properties.Table, properties.ColumnName); }
        }

        /// <summary>
        /// Gets the instance of <see cref="DbTypeFormatter" />.
        /// </summary>
        public DbTypeFormatter Formatter
        {
            get { return formatter; }
        }

        /// <summary>
        /// Gets the <see cref="MemberInfo" /> instance for this member.
        /// </summary>
        internal MemberInfo Member
        {
            get { return member; }
        }


        /// <summary>
        /// Gets the properties.
        /// </summary>
        public MapMemberProperties Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets the type of the map member.
        /// </summary>
        /// <value>The type of the map member.</value>
        public Type MapMemberType
        {
            get { return mapMemberType; }
        }

        /// <summary>
        /// Gets the type of the db.
        /// </summary>
        /// <value>The type of the db.</value>
        public DbType DbType
        {
            get { return dbType; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapMember"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="formatter">The formatter.</param>
        internal MapMember(MemberInfo member, MapMemberProperties properties, DbTypeFormatter formatter)
        {
            Debug.Assert((member != null), "member is null ");
            Debug.Assert(properties != null, "properties argument is null");

            this.member = member;
            this.properties = properties;
            this.formatter = formatter;

            if (member is FieldInfo)
            {
                FieldInfo fieldInfo = (FieldInfo) member;
                mapMemberType = fieldInfo.FieldType;
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo propertyInfo = (PropertyInfo)member;
                mapMemberType = propertyInfo.PropertyType; 
            }
            else
            {
                mapMemberType = typeof (Object);
            }

           
            properties.ApplyRules();

            this.dbType = MapUtility.DbTypeMapping(properties, properties.DataType);

        }


        private void SetBinaryValue(object source, Stream sourceStream)
        {
            Stream stream = sourceStream;
            if ((!properties.AcceptNulls) && (sourceStream == null))
            {
                string exMsg = Utility.GetResourceString("RES_NullException", member.Name, source.GetType().FullName);
                throw new ORMException(exMsg);
            }

            if (Properties.DataType != DataType.Binary && Properties.DataType != DataType.Object)
            {
                throw new ORMException("Unable set binary value to an non-binary MapMember. DataType: "+Properties.DataType);
            }

            if (properties.IsEncryptionEnabled)
            {
                EncryptionProvider provider = properties.GetEncryptionProvider();

                DataStream encryptedStream = new DataStream();
                provider.DecryptStream(stream, encryptedStream);
            }

            if (properties.IsCompressionEnabled)
            {
                CompressionProvider compressionProvider = properties.GetCompressionProvider();
                stream = compressionProvider.Decompress(sourceStream);
            }

            object convertedValue = null;

            if (mapMemberType == typeof(Byte[]) && Properties.DataType == DataType.Binary)
            {
               convertedValue = DataStream.RealAllBytes(stream);
            }
            else if ((mapMemberType == typeof(Stream) || MapMemberType == typeof(DataStream)) && Properties.DataType == DataType.Binary)
            {
                convertedValue = stream;
            }
            else if (mapMemberType == typeof(string) && Properties.DataType == DataType.Binary)
            {
                string path = String.Format(@"{0}\{1:N}.{2}", ORMConfigurator.TemporaryFolder, Guid.NewGuid(),
                                               Properties.FileExtension);
                using (Stream destStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    // now, copy source stream to destStream
                    byte[] buffer = new byte[ORMConfigurator.BufferSize];

                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);
                        destStream.Flush();
                    }

                    destStream.Close();
                    convertedValue = path;
                }
                
            }
            else if (Properties.DataType == DataType.Object)
            {
                IFormatter deserializer = Properties.GetObjectFormatter();
                if (sourceStream != null)
                {
           
                    convertedValue = deserializer.Deserialize(sourceStream);
                }
            }
            else
            {
                throw new ORMException("Unable set binary value to a field that is not Byte[] or Stream. Field Type" + mapMemberType.FullName + " Object:" + source.GetType().FullName);
            }

            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldinfo = (FieldInfo)member;
                fieldinfo.SetValue(source, convertedValue);
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyinfo = (PropertyInfo)member;
                propertyinfo.SetValue(source, convertedValue, null);
            }
            else
            {
                throw new NotSupportedException("The memberinfo type '" + member.MemberType + "' is not supported");
            } 
          
        }

        // get binary value for field (stream, byte[], or custom object).
        private byte[] GetBinaryValue(object source)
        {
            object originalValue = GetMemberValue(source);

            // check if value is a string;
            byte[] returnValue = null;

            if (originalValue == null) return null;
            if (ReferenceEquals(originalValue, Stream.Null)) return null;
            
            if (MapMemberType == typeof(Stream) || MapMemberType == typeof(DataStream))
            {
                DataStream stream;
                if (properties.IsCompressionEnabled)
                {
                    CompressionProvider compressionProvider = properties.GetCompressionProvider();
                    Stream compressedStream = compressionProvider.Compress((Stream) originalValue);
                    stream = new DataStream(compressedStream);
                }
                else
                {
                    stream = new DataStream((Stream) originalValue);
                }


                if (properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = properties.GetEncryptionProvider();

                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(stream, encryptedStream);
                    returnValue = encryptedStream.ReadAllBytes();
                }
                else
                {
                    returnValue = stream.ReadAllBytes();
                }
                

            }
            else if (MapMemberType == typeof(Byte[]) && Properties.DataType == DataType.Binary)
            {
                DataStream stream;
                if (properties.IsCompressionEnabled)
                {
                    CompressionProvider compressionProvider = properties.GetCompressionProvider();
                    DataStream tempStream = new DataStream((Byte[])originalValue);
                    Stream compressedStream = compressionProvider.Compress(tempStream);
                    stream = new DataStream(compressedStream);
                    returnValue = stream.ReadAllBytes();
                }
                else
                {
                    returnValue = (Byte[]) originalValue;
                }

                if (properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = properties.GetEncryptionProvider();

                    Stream originalStream = new DataStream(returnValue);
                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(originalStream, encryptedStream);
                    returnValue = encryptedStream.ReadAllBytes();
                }

   
            }
            else if (MapMemberType == typeof(String) && Properties.DataType == DataType.Binary)
            {
                // no check if it is a value path.
                string path = Convert.ToString(originalValue);
#if NET20
                Uri uri;
                bool isUri = Uri.TryCreate(path, UriKind.Absolute, out uri);
#endif
#if NET11
                bool isUri = false;
                try
                {
                    Uri uri = new Uri(path, false);
                    isUri = true;
                }
                catch(Exception ex)
                {
                    isUri = false;
                }
#endif
                if (isUri)
                {
                    // the path is valid.
                    DataStream stream;
                    if (properties.IsCompressionEnabled)
                    {
                        DataStream fileStream = new DataStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        CompressionProvider compressionProvider = properties.GetCompressionProvider();
                        Stream compressedStream = compressionProvider.Compress(fileStream);
                        stream = new DataStream(compressedStream);
                    }
                    else
                    {
                        stream = new DataStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }


                    if (properties.IsEncryptionEnabled)
                    {
                        EncryptionProvider provider = properties.GetEncryptionProvider();

                        DataStream encryptedStream = new DataStream();
                        provider.EncryptStream(stream, encryptedStream);
                        returnValue = encryptedStream.ReadAllBytes();
                    }
                    else
                    {
                        returnValue = stream.ReadAllBytes();
                    }

                }
                
            
            }
            else if (Properties.DataType == DataType.Object)
            {
                using (Stream stream = new MemoryStream())
                {
                    IFormatter serialiazer = Properties.GetObjectFormatter();
                    serialiazer.Serialize(stream, originalValue);
                    stream.Flush();
                    stream.Position = 0;
                    if (properties.IsCompressionEnabled)
                    {
                        CompressionProvider compressionProvider = properties.GetCompressionProvider();
                        Stream compressedStream = compressionProvider.Compress(stream);

                        returnValue = new DataStream(compressedStream).ReadAllBytes();
                    }               
                    else
                    {
                        returnValue = new DataStream(stream).ReadAllBytes();
                    }

                    stream.Close();

                    if (properties.IsEncryptionEnabled)
                    {
                        EncryptionProvider provider = properties.GetEncryptionProvider();

                        Stream originalStream = new DataStream(returnValue);
                        DataStream encryptedStream = new DataStream();
                        provider.EncryptStream(originalStream, encryptedStream);
                        returnValue = encryptedStream.ReadAllBytes();
                    }

                    
                }
            }

            return returnValue;

        }


        /// <summary>
        /// Sets the data source value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        internal void SetDataSourceValue(object source, object value)
        {

            if ((!properties.AcceptNulls) && (value == null))
            {
                string exMsg = Utility.GetResourceString("RES_NullException", member.Name, source.GetType().FullName);
                throw new ORMException(exMsg);
            }

            if (Properties.DataType == DataType.Binary)
            {
                if (!(value is DataStream))
                {
                    if (value is Byte[]) value = new DataStream((byte[])value);
                    if (value is Stream) value = new DataStream((Stream)value);
                }

                SetBinaryValue(source, (DataStream) value);
                return;
            }

            if (Properties.DataType == DataType.Object)
            {
                if (!(value is DataStream))
                {
                    if (value is Byte[]) value = new DataStream((byte[])value);
                    if (value is Stream) value = new DataStream((Stream)value);
                }

                SetBinaryValue(source, (DataStream)value);
                return;
            }

            object convertedValue = MapUtility.ConvertTo(this, value);

            
            if (Properties.IsEncryptionEnabled && Properties.DataType == DataType.String)
            {
                EncryptionProvider provider = Properties.GetEncryptionProvider();
                convertedValue = provider.Decrypt(Convert.ToString(convertedValue));
            }

            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldinfo = (FieldInfo)member;
                fieldinfo.SetValue(source, convertedValue);
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyinfo = (PropertyInfo)member;
                propertyinfo.SetValue(source, convertedValue, null);
            }
            else
            {
                throw new NotSupportedException("The memberinfo type '" + member.MemberType + "' is not supported");
            }
        }

        /// <summary>
        /// Gets the data source value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal object GetDataSourceValue(object source)
        {
            object originalValue;
            if (Properties.DataType == DataType.Binary || Properties.DataType == DataType.Object)
            {
                originalValue = GetBinaryValue(source);
            }
            else
            {
                originalValue = GetMemberValue(source);

                // if encryption is enabled, then encrypt. DataType must be string. 
                if (Properties.IsEncryptionEnabled && Properties.DataType == DataType.String)
                {
                    EncryptionProvider provider = Properties.GetEncryptionProvider();
                    originalValue = provider.Encrypt(Convert.ToString(originalValue));
                }
            }

            if (Properties.IsPrimaryKey && originalValue == null)
            {
                string exMsg = Utility.GetResourceString("RES_PKIsNullOnInsert", MemberName);
                throw new ORMException(exMsg);
            }

            if ((!Properties.AcceptNulls && originalValue == null) ||
                (!Properties.AcceptNulls && originalValue != null && Properties.DataType == DataType.String && ((string)originalValue).Length == 0))
            {
                if (!Properties.DefaultDefined)
                {
                    string typeFullName = source.GetType().FullName;
                    string propertyName = MemberName;
                    string excMsg = Utility.GetResourceString("EXCEPTION:PROP_IS_NULL", propertyName, typeFullName, originalValue);

                    throw new ORMException(excMsg);
                }

                originalValue = Properties.DefaultValue;
            }

            // Here we set the values.
            if (Properties.DataType == DataType.String && originalValue != null)
            {
                string tempValue = Convert.ToString(originalValue);

                // Throw an error if the string excced the capacity.
                if (tempValue.Length > Properties.Length)
                {
                    string exMsg = Utility.GetResourceString("SQL:VALUE_SET_EXCEEDS_CAPACITY", source.GetType().FullName, MemberName, Properties.Length, tempValue);
                    throw new ORMException(exMsg);
                }
            }

            return originalValue;
        }


        internal void SetMemberValue(object source, object value)
        {
            if ((!properties.AcceptNulls) && (value == null))
            {
                string exMsg = Utility.GetResourceString("RES_NullException", member.Name, source.GetType().FullName);
                throw new ORMException(exMsg);
            }

            object convertedValue = MapUtility.ConvertTo(this, value);

            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldinfo = (FieldInfo)member;
                try
                {
                    fieldinfo.SetValue(source, convertedValue);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyinfo = (PropertyInfo)member;
                try
                {
                    propertyinfo.SetValue(source, convertedValue, null);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }
            else
            {
                throw new NotSupportedException("The memberinfo type '" + member.MemberType + "' is not supported");
            }
        }

        /// <summary>
        /// Gets the current value of a mapped member.
        /// </summary>
        /// <param name="source">the object in which the mapped member (property/field) will be retrieved.</param>
        internal object GetMemberValue(object source)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldinfo = (FieldInfo)member;
                try
                {
                    return fieldinfo.GetValue(source);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }

            if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyinfo = (PropertyInfo)member;
                try
                {
                    return propertyinfo.GetValue(source, null);
                }
                catch(TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }

                    throw;
                }
            }

            throw new NotSupportedException("The memberinfo type '" + member.MemberType + "' is not supported");
        }

        /// <summary>
        /// Returns the next sequence number for the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal object GetSequence(object obj)
        {
            if (Properties.GetSequenceProvider() == null) return -1;
            return Properties.GetSequenceProvider().GetNextNumber(obj, this);
        }
    }
}
