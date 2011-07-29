#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine.Schema
{
// ReSharper disable ConvertToStaticClass
    internal sealed class MapUtility
// ReSharper restore ConvertToStaticClass
    {
        private const string _VERSION_REGEX = @"(?<=\[{1})[\d]+(?<!\]{1})";
        private const string _DIGEST_REGEX = @"(?<=\[{1}[\d]+\]{1}).*";

        private MapUtility() { }

        internal static object FormatValue(MapMember column, object value, bool forceNulls)
        {
            if (IsNullValue(column, value))
            {
                if (column.Properties.DefaultDefined)
                {
                    value = column.Properties.DefaultValue;
                }
                else
                {
                    if (column.Properties.AcceptNulls && column.Properties.DataType != DataType.Binary && column.Properties.DataType != DataType.Object)
                    {
                        return column.Formatter.DBNullValue;
                    }
                    
                    if (column.Properties.AcceptNulls && (column.Properties.DataType == DataType.Binary || column.Properties.DataType == DataType.Object))
                    {
                        return new byte[0];
                    }

                    if (forceNulls)
                    {
                        string exMsg = Utility.GetResourceString("RES_NullException", column.MemberName, column.Member.DeclaringType.FullName);
                        throw new ORMException(exMsg);
                    }

                    return column.Formatter.DBNullValue;
                }
            }

            // return value;
            object rv;

            switch (column.Properties.DataType)
            {
                case DataType.String:
                    {
                        string strVal = Convert.ToString(value);

                        if (strVal.Length == 0 && !column.Properties.AcceptNulls && forceNulls)
                        {
                            string exMsg = Utility.GetResourceString("RES_NullException", column.MemberName,
                                                                     column.Member.DeclaringType.FullName);
                            throw new ORMException(exMsg);
                        }

                        rv = Convert.ToString(value);
                        break;
                    }
                case DataType.Int16:
                case DataType.Double:
                case DataType.Int32:
                case DataType.Byte:
                case DataType.Int64:
                case DataType.Single:
                case DataType.Decimal:
                    {
                        //DbType dbtype = column.Formatter;
                        rv = value;
                        break;
                    }
                case DataType.DateTime:
                    {

                        // DbTypeFormatter dbtype = column.Formatter;
                        DateTime dt = Convert.ToDateTime(value);

                        if (dt == DateTime.MinValue)
                        {
                            if (!column.Properties.AcceptNulls && forceNulls)
                            {
                                string exMsg1 = Utility.GetResourceString("RES_NullException", column.MemberName,
                                                                          column.Member.DeclaringType.FullName);
                                throw new ORMException(exMsg1);
                            }

                            rv = column.Formatter.DBNullValue;
                        }
                        else
                        {
                            rv = dt;
                        }

                        break;
                    }
                case DataType.Boolean:
                    {
                       // DbTypeFormatter dbtype = column.Formatter;
                        string tmpValue = Convert.ToString(value);
                        if (String.Compare(tmpValue, "true", true) == 0 || String.Compare(tmpValue, "false", true) == 0)
                        {
                            rv = Convert.ToBoolean(value);
                        }
                        else if (tmpValue == "1")
                        {
                            rv = Convert.ToBoolean(tmpValue);
                        }
                        else if (tmpValue == "0")
                        {
                            rv = Convert.ToBoolean(tmpValue);
                        }
                        else
                        {
                            string exMsg = String.Format("Invalid boolean value set for member {0} of type {1}",
                                                         column.MemberName, column.Member.DeclaringType.FullName);
                            throw new ORMException(exMsg);
                        }

                        break;
                    }
                case DataType.Guid:
                    {
                        DbTypeFormatter dbtype = column.Formatter;
                        if (Guid.Empty.Equals(value))
                        {
                            if (!column.Properties.AcceptNulls && forceNulls)
                            {
                                string exMsg2 = Utility.GetResourceString("RES_NullException", column.MemberName,
                                                                          column.Member.DeclaringType.FullName);
                                throw new ORMException(exMsg2);
                            }
                            rv = column.Formatter.DBNullValue;
                        }
                        else
                        {
                            rv = String.Format("{0}", dbtype.FormatGuid((Guid) value));
                        }
                        break;
                    }
                case DataType.Enum:
                    {
                        rv = GetEnumerationValue(column, value);
                        break;
                    }
                case DataType.Xml:
                    {
                        XmlDocument document = (XmlDocument) value;
                        rv = document != null
                                 ? document.OuterXml
                                 : String.Empty;
                        break;
                    }
                case DataType.Binary:
                    {
                        return value;
                    }
                case DataType.Object:
                    {
                        return value;
                    }
                default:
                    {
                        throw new NotSupportedException(Enum.GetName(typeof (DataType), column.Properties.DataType) +
                                                        "Is not supported!");
                    }

            }
            return rv;
        }

        internal static object GetMappedDefaultValue(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.String:
                    {
                        return null;
                    }
                case DataType.Int16:
                    {
                        return ORMDefaults.INT16;
                    }
                case DataType.Double:
                    {
                        return ORMDefaults.DOUBLE;
                    }
                case DataType.Int32:
                    {
                        return ORMDefaults.INT32;
                    }
                case DataType.Byte:
                    return false;
                case DataType.Int64:
                    {
                        return ORMDefaults.INT64;
                    }
                case DataType.Single:
                    {
                        return ORMDefaults.FLOAT;
                    }
                case DataType.Decimal:
                    {
                        return ORMDefaults.DECIMAL;
                    }
                case DataType.DateTime:
                    {
                        return ORMDefaults.DATETIME;
                    }
                case DataType.Boolean:
                    {
                        return false;
                    }
                case DataType.Guid:
                    {
                        return ORMDefaults.GUID;
                    }
                case DataType.Enum:
                    {
                        return false;

                    }

                case DataType.Xml:
                    {
                        return null;
                    }

                case DataType.Binary:
                    {
                        return null;
                    }
                case DataType.Object:
                    {
                        return null;
                    }
                default:
                    {
                        throw new NotSupportedException(Enum.GetName(typeof(DataType), dataType) + "Not Identified !");
                    }

            }


        }

        private static bool IsNullValue(MapMember column, object value)
        {
            if (value == null) return true;

            switch (column.Properties.DataType)
            {
                case DataType.String:
                    {

                        string strVal = Convert.ToString(value);
                        if (String.Compare(strVal, ORMNull.STRING) == 0) return true;

                        return false;
                    }
                case DataType.Int16:
                    {
                        return ORMNull.INT16.Equals(value);
                    }
                case DataType.Double:
                    {
                        return ORMNull.DOUBLE.Equals(value);
                    }
                case DataType.Int32:
                    {
                        return ORMNull.INT32.Equals(value);
                    }
                case DataType.Byte:
                    return false;
                case DataType.Int64:
                    {
                        return ORMNull.INT64.Equals(value);
                    }
                case DataType.Single:
                    {
                        return ORMNull.FLOAT.Equals(value);
                    }
                case DataType.Decimal:
                        {
                            return ORMNull.DECIMAL.Equals(value);
                        }
                    case DataType.DateTime:
                    {


                        return ORMNull.DATETIME.Equals(value);



                    }
                case DataType.Boolean:
                    {
                        return false;
                    }
                case DataType.Guid:
                    {
                        return ORMNull.GUID.Equals(value);
                    }
                case DataType.Enum:
                    {
                        return false;

                    }

                case DataType.Xml:
                    {
                        XmlDocument document = (XmlDocument)value;
                        return document.InnerXml == ORMNull.XMLDOCUMENT.InnerXml;
                    }

                case DataType.Binary:
                    {
                        if (value is Byte[])
                        {
                            byte[] bytes = (byte[]) value;
                            if (bytes.Length != ORMNull.BINARY.Length) return false;

                            // now, compare byte by byte to see if match.
                            for (int i = 0; i < bytes.Length; i++)
                            {
                                if (bytes[i] != ORMNull.BINARY[i]) return false;
                            }

                            return true;
                        }
                        if(value is Stream)
                        {
                            if (ReferenceEquals(value, Stream.Null)) return true;
                            return false;
                        }
                        if (value is DataStream)
                        {
                            if (ReferenceEquals(value, Stream.Null)) return true;
                            return false;
                        }

                        return false;
                    }
                case DataType.Object:
                    {
                        if (value is ORMNull) return true;
                        return false;
                    }
                default:
                    {
                        throw new NotSupportedException(Enum.GetName(typeof(DataType), column.Properties.DataType) + "Not Identified !");
                    }

            }
           
      
        }

       

#region :: static method: FormatValue ::
        
        /* Get a database specific value depending of the type. */
        internal static string FormatValueAsString(MapMember column, object value, bool forceNulls)
        {
            if (IsNullValue(column, value))
            {
                if (column.Properties.DefaultDefined)
                {
                    value = column.Properties.DefaultValue;
                }
                else
                {
                    if (column.Properties.AcceptNulls)
                    {
                        return "null";
                    }
                    
                    if (forceNulls)
                    {
                        string exMsg = Utility.GetResourceString("RES_NullException", column.MemberName, column.Member.DeclaringType.FullName);
                        throw new ORMException(exMsg);
                    }
                    
                    
                    return "null";
                }
            }

            // return value;
            string rv;

            switch (column.Properties.DataType)
            {
                case DataType.String:
                    {
                        string strVal = Convert.ToString(value);

                        if (strVal.Length == 0 && !column.Properties.AcceptNulls && forceNulls)
                        {
                            string exMsg = Utility.GetResourceString("RES_NullException", column.MemberName, column.Member.DeclaringType.FullName);
                            throw new ORMException(exMsg);
                        }

                        rv = String.Format("'{0}'", column.Formatter.FormatString(Convert.ToString(value)));
                        break;
                    }
                case DataType.Int16:
                case DataType.Double:
                case DataType.Int32:
                case DataType.Byte:
                case DataType.Int64:
                case DataType.Single:
                case DataType.Decimal:
                    {
                        //DbType dbtype = column.Formatter;
                        rv = String.Format("{0}", Convert.ToString(value));
                        break;
                    }
                case DataType.DateTime:
                    {

                        DbTypeFormatter dbtype = column.Formatter;
                        DateTime dt = Convert.ToDateTime(value);

                        if (dt == DateTime.MinValue || dt == ORMNull.DATETIME)
                        {
                            if (!column.Properties.AcceptNulls && forceNulls)
                            {
                                string exMsg1 = Utility.GetResourceString("RES_NullException", column.MemberName, column.Member.DeclaringType.FullName);
                                throw new ORMException(exMsg1);
                            }

                            rv = "null";
                        }
                        else
                        {
                            rv = String.Format("'{0}'", dbtype.FormatDateTime(dt));
                        }

                        break;
                    }
                case DataType.Boolean:
                    {
                        DbTypeFormatter dbtype = column.Formatter;
                        string tmpValue = Convert.ToString(value);
                        if (String.Compare(tmpValue, "true", true) == 0 || String.Compare(tmpValue, "false", true) == 0)
                        {
                            rv = String.Format("'{0}'", dbtype.FormatBoolean(Convert.ToBoolean(value)));
                        }
                        else if (tmpValue == "1")
                        {
                            rv = String.Format("'{0}'", dbtype.FormatBoolean(true));
                        }
                        else if (tmpValue == "0")
                        {
                            rv = String.Format("'{0}'", dbtype.FormatBoolean(false));
                        }
                        else
                        {
                            string exMsg = String.Format("Invalid boolean value set for member {0} of type {1}", column.MemberName, column.Member.DeclaringType.FullName);
                            throw new ORMException(exMsg);
                        }

                        break;
                    }
                case DataType.Guid:
                    {
                        DbTypeFormatter dbtype = column.Formatter;
                        if (Guid.Empty.Equals(value) || ORMNull.GUID.Equals(value))
                        {
                            if (!column.Properties.AcceptNulls && forceNulls)
                            {
                                string exMsg2 = Utility.GetResourceString("RES_NullException", column.MemberName, column.Member.DeclaringType.FullName);
                                throw new ORMException(exMsg2);
                            }
                            rv = "null";
                        }
                        else
                        {
                            rv = String.Format("'{0}'", dbtype.FormatGuid((Guid)value));
                        }
                        break;
                    }
                case DataType.Enum:
                    {
                        rv = GetEnumerationValueAsString(column, value);
                        break;
                    }
                
                case DataType.Xml:
                    {
                        XmlDocument document = (XmlDocument)value;
                        if (document != null)
                        {
                            string xml = column.Formatter.FormatString(document.OuterXml);
                            if (xml == ORMNull.XMLDOCUMENT.OuterXml)
                            {
                                return "null";
                            }

                            return String.Format("'{0}'", xml);
                        }

                        return "null";
                    }

                case DataType.Binary:
                case DataType.Object:
                default:
                    {
                        throw new NotSupportedException(Enum.GetName(typeof(DataType), column.Properties.DataType) + "Not Identified !");
                    }

            }
            return rv;

        }

        #endregion


        //internal static object ConvertNonMappedValue(object value)
        //{
        //    if (value is String)
        //    {
        //        string strVal = Convert.ToString(value);
        //        if (String.Compare(strVal, ORMNull.STRING) == 0) return null;
        //    }

        //    if (value is XmlDocument)
        //    {
        //        XmlDocument document = (XmlDocument)value;
        //        if (document.InnerXml == ORMNull.XMLDOCUMENT.InnerXml) return null;
        //    }

        //    if (value is Byte[])
        //    {
        //        byte[] bytes = (byte[])value;
        //        if (bytes.Length != ORMNull.BINARY.Length) return value;

        //        // now, compare byte by byte to see if match.
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            if (bytes[i] != ORMNull.BINARY[i]) return value;
        //        }

        //        return null;
        //    }

        //    return value;
        //}

        //internal static bool CanUpdateMember(object value)
        //{
        //    if (value == null) return false;
        //    if (value is DBNull) return false;
        
            
        //    if (value is String)
        //    {
        //        string strVal = Convert.ToString(value);
        //        if (String.Compare(strVal, ORMNull.STRING) == 0) return true;
        //    }
        //    if (value is Int16)
        //    {
        //        return ORMNull.INT16.Equals(value);
        //    }
            
        //    if (value is Double)
        //    {
        //        return ORMNull.DOUBLE.Equals(value);
        //    }

        //    if (value is Int32)
        //    {
        //        return ORMNull.INT32.Equals(value);
        //    }

        //    if (value is Int64)
        //    {
        //        return ORMNull.INT64.Equals(value);
        //    }

        //    if (value is Single)
        //    {
        //        return ORMNull.FLOAT.Equals(value);
        //    }

        //    if (value is DateTime)
        //    {
        //         return ORMNull.DATETIME.Equals(value);
        //    }

        //    if (value is Guid)
        //    {
        //        return ORMNull.GUID.Equals(value);
        //    }
                
        //    if (value is ORMNull || value is DBNull)
        //    {
        //        return true;
        //    }

        //    if (value is XmlDocument)
        //    {
        //        XmlDocument document = (XmlDocument)value;
        //        return document.InnerXml == ORMNull.XMLDOCUMENT.InnerXml;  
        //    }

        //    if (value is Stream)
        //    {
        //        return ReferenceEquals(ORMNull.STREAM, value) ;
        //    }

        //    if (value is Byte[])
        //    {
        //        byte[] bytes = (byte[]) value;
        //        if (bytes.Length != ORMNull.BINARY.Length) return false;

        //        // now, compare byte by byte to see if match.
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            if (bytes[i] != ORMNull.BINARY[i]) return false;
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        internal static bool CanUpdateMember(MapMember member,  object value)
        {
            Debug.Assert(member != null);

            //return null if value is null.
            if (!member.Properties.AcceptNulls)
            {
                if (value == null) return false;
                if (value is DBNull) return false;
            }
            else if (IsNullValue(member, value))
            {
                return true;
            }
 
            // Start convertion.
            switch (member.Properties.DataType)
            {

                case DataType.String:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        break;
                    }
                case DataType.Enum:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0 || String.Compare(str, "Undefined", true) == 0) return false;
                        break;
                    }
                case DataType.DateTime:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            DateTime datetime = Convert.ToDateTime(value);
                            if (datetime == DateTime.MinValue) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to DateTime while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Int16:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            Int16 i16 = Convert.ToInt16(value);
                            if (i16 == Int16.MinValue) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to Int16 while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Int32:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            Int32 n = Convert.ToInt32(value);
                            if (n == Int32.MinValue) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to Int32 while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Int64:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            Int64 n = Convert.ToInt64(value);
                            if (n == Int64.MinValue) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to Int64 while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Double:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            double n = Convert.ToDouble(value);
                            if (n == Double.NaN) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to Double while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Byte:
                    {
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            byte n = Convert.ToByte(value);
                            if (n == Byte.MinValue) return false;
                            break;
                        }
                        catch (FormatException)
                        {
                            //TODO: Message must come from the resource file.
                            string exMsg = String.Format("Unable to convert '{0}' to Byte while loading column name '{1}'", value, member.ColumnFullName);
                            throw new ORMException(exMsg);
                        }
                    }
                case DataType.Boolean:
                    {
                       
                        try
                        {
                            String str = Convert.ToString(value);
                            if (str.Length == 0) return false;
                            break;
                        }
                        catch (Exception e)
                        {
                            throw new ORMException(e.Message);
                        }

                    }
                case DataType.Decimal:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        decimal n = Convert.ToDecimal(value);
                        if (n == Decimal.MinValue) return false;
                    
                        break;
                    }
                case DataType.Single:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        float n = Convert.ToSingle(value);
                        if (n == Single.NaN) return false;
                    
                        break;
                    }
                case DataType.Guid:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        Guid guid;
                        if (value is Guid)
                        {
                            guid = (Guid)value;
                            if (guid == Guid.Empty) return false;
                        }

                        break;
                    }
                case DataType.Xml:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        break;
                    }
                case DataType.Object:
                    {
                        return true;
                    }
                case DataType.Binary:
                    {
                        return true;
                    }
                default:
                    {
                        String str = Convert.ToString(value);
                        if (str.Length == 0) return false;
                        break;
                    }

            }
                    return true;
            

        }

        #region :: static method: ConvertTo ::

        ///<summary>
        /// Verify <paramref name="member" /> <see cref="DataType" />
        /// and convert <paramref name="value" /> to the column's type.
        ///</summary>
        ///<param name="member">
        /// The <see cref="MapMember" />
        ///</param>
        ///<param name="value">
        /// The value to be converted.
        ///</param>
        internal static object ConvertTo(MapMember member, object value)
        {
            Debug.Assert(member != null);

            //return null if value is null.
            if (value is DBNull) value = null;
           

            // Start convertion.
            switch (member.Properties.DataType)
            {

                case DataType.String:
                {
                    String str = Convert.ToString(value);
                    if (str.Length == 0 || String.Compare(str, ORMNull.STRING) == 0) str = String.Empty;
                    return str;
                }
                case DataType.Enum:
                {
                    return ParseEnumeration(member, value);
                }
                case DataType.DateTime:
                {
                    try
                    {
                        return value != null ? Convert.ToDateTime(value) : DateTime.MinValue;
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to DateTime while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Int16:
                {
                    try
                    {
                        return value != null ? Convert.ToInt16(value) : (Int16)0;
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to Int16 while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Int32:
                {
                    try
                    {
                        return value != null ? Convert.ToInt32(value) : 0;
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to Int32 while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Int64:
                {
                    try
                    {
                        return value != null ? Convert.ToInt64(value) : 0;
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to Int64 while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Double:
                {
                    try
                    {
                        return value != null ? Convert.ToDouble(value) : 0d;
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to Double while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Byte:
                {
                    try
                    {
                         return value != null ? Convert.ToByte(value) : Convert.ToByte(0);
                    }
                    catch (FormatException)
                    {
                        //TODO: Message must come from the resource file.
                        string exMsg = String.Format("Unable to convert '{0}' to Byte while loading column name '{1}'", value, member.ColumnFullName);
                        throw new ORMException(exMsg);
                    }
                }
                case DataType.Boolean:
                {
                    bool bol;
                    try
                    {
                        if (value == null) return false;

                        string tmpValue = Convert.ToString(value);
                        bol = member.Formatter.ToBoolean(tmpValue);
                    }
                    catch (Exception e)
                    {
                        throw new ORMException(e.Message);
                    }

                    return bol;
                }
                case DataType.Decimal:
                {
                    return value != null ? Convert.ToDecimal(value) : Decimal.Zero;
                }
                case DataType.Single:
                {
                    return value != null ? Convert.ToSingle(value) : 0f;
                }
            case DataType.Guid:
                {
                    if (value == null) return Guid.Empty;

                    Guid guid = Guid.Empty;
                    if (value is Guid)
                    {
                        guid = (Guid)value;
                     
                    }
                    else
                    {
                        String str = Convert.ToString(value);
                        if (str.Length > 0)
                        {
                            guid = new Guid(str);
                        }
                    }

                    return guid;
                }
                case DataType.Xml:
                {
                    if (value == null) return null;
                    XmlDocument document = null;
                    if (value is XmlDocument)
                    {
                        document = (XmlDocument)value;
                    }
                    else
                    {
                        String str = Convert.ToString(value);
                        if (str.Length > 0)
                        {
                            document = new XmlDocument();
                            document.LoadXml(str);
                        }
                    }

                    if (document != null && document.InnerXml == ORMNull.XMLDOCUMENT.InnerXml) return null;
                    
                    return document;
                }
                case DataType.Object:
                {
                    if (value == null || value is ORMNull || value is DBNull) return null;
                    return value;
                }
                default:
                {
                    return value;
                }
            }

        }

        #endregion

        #region :: static method: GetEnumerationValue ::


        internal static object GetEnumerationValue(MapMember member, object value)
        {
            // Check that the value is actually an enumeration.
            if (!value.GetType().IsEnum)
            {
                string exMsg = Utility.GetResourceString("DataObjectSQLBuilder:GetEnumerationValue:InvalidEnum", member.ColumnName,
                                                         value.GetType().FullName);
                throw new ORMException(exMsg);
            }

            MapMemberProperties properties = member.Properties;
            object returnValue;

            if (properties.EnumConversionType == EnumConversionType.ByName)
            {
                returnValue = String.Format("{0}", Enum.GetName(value.GetType(), value));
            }
            else if (properties.EnumConversionType == EnumConversionType.ByValue)
            {
                returnValue = String.Format("{0:d}", value);
            }
            else
            {
                string exMsg = Utility.GetResourceString("ConvertionType:NOT_IMPLEMENTED", properties.EnumConversionType);
                throw new NotSupportedException(exMsg);
            }

            return returnValue;
        }


        // Returns the value of an enumeration depending of the convertion type
        internal static string GetEnumerationValueAsString(MapMember member, object value)
        {
            // Check that the value is actually an enumeration.
            if (!value.GetType().IsEnum)
            {
                string exMsg = Utility.GetResourceString("DataObjectSQLBuilder:GetEnumerationValue:InvalidEnum", member.ColumnName,
                                                         value.GetType().FullName);
                throw new ORMException(exMsg);
            }

            MapMemberProperties properties = member.Properties;
            string returnValue = String.Empty;

            if (properties.EnumConversionType == EnumConversionType.ByName)
            {
                returnValue = String.Format("'{0}'", Enum.GetName(value.GetType(), value));
            }
            else if (properties.EnumConversionType == EnumConversionType.ByValue)
            {
                returnValue = String.Format("{0:d}", value);
            }
            else
            {
                string exMsg = Utility.GetResourceString("ConvertionType:NOT_IMPLEMENTED", properties.EnumConversionType);
                throw new NotSupportedException(exMsg);
            }

            return returnValue;
        }

        #endregion

        #region :: static method: ParseEnumeration ::

        // Parse an enumeration.
        private static object ParseEnumeration(MapMember mappedMember, object value)
        {
            
            if (value == null)
            {
                if (Enum.IsDefined(mappedMember.Properties.EnumerationType, "Undefined"))
                {
                    return Enum.Parse(mappedMember.Properties.EnumerationType, "Undefined", true);
                }
               
                if (Enum.IsDefined(mappedMember.Properties.EnumerationType, 0))
                {
                    return Enum.ToObject(mappedMember.Properties.EnumerationType, 0); 
                }
                
            }

            object parsedEnum = null;
            //Trace.Assert(mappedMember.Properties.EnumerationType != null, "Enumeration type has not been defined");

            if (mappedMember.Properties.EnumConversionType == EnumConversionType.ByName)
            {
              
                try
                {
                    parsedEnum = Enum.Parse(mappedMember.Properties.EnumerationType, Convert.ToString(value), true);
                }
                catch (Exception e)
                {
                    string message = Utility.GetResourceString("MappedMember:ParseEnumeration:PARSE_FAILED", value, mappedMember.Properties.EnumerationType, mappedMember.Properties.ColumnName, mappedMember.Member.DeclaringType, mappedMember.Member.Name);
                    throw new ORMException(message, e);
                }
            }
            else if (mappedMember.Properties.EnumConversionType == EnumConversionType.ByValue)
            {
                try
                {
                    parsedEnum = Enum.ToObject(mappedMember.Properties.EnumerationType, value);
                }
                catch (ArgumentException e)
                {
                    string message = Utility.GetResourceString("MappedMember:ParseEnumeration:PARSE_FAILED_INVALID_TYPE", value.GetType().FullName, mappedMember.Properties.EnumerationType,
                                                               mappedMember.ColumnName, mappedMember.Member.DeclaringType.FullName, mappedMember.Member.Name);
                    throw new ORMException(message, e);
                }
                catch (Exception e)
                {
                    string message = Utility.GetResourceString("MappedMember:ParseEnumeration:PARSE_FAILED", value, mappedMember.Properties.EnumerationType, mappedMember.Properties.ColumnName, mappedMember.Member.DeclaringType, mappedMember.MemberName);
                    throw new ORMException(message, e);
                }

            }
            else
            {
                string message = Utility.GetResourceString("ConvertionType:NOT_IMPLEMENTED", mappedMember.Properties.EnumConversionType);
                throw new NotSupportedException(message);
            }

            return parsedEnum;
        }

        #endregion

        /// <summary>
        /// Gets the full name of the column.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        public static string GetFullColumnName(ObjectSchemaInfo schema, MapMember member)
        {
            return String.Format("{0}.{1}", schema.TableName, member.ColumnName); 
        }

        /// <summary>
        /// Creates the record digest.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        internal static string CreateRecordDigest(MapMemberList members, object obj, int version)
        {
            ArrayList list = new ArrayList();
            MapMember digestMemberField = null;
            foreach (MapMember member in members)
            {
                if (member.Properties.HasRecordDigestProvider)
                {
                    digestMemberField = member;
                    continue;
                }

                if (member.Properties.ContainsAttribute(MemberAttributes.IgnoreChanges) || member.Properties.ContainsAttribute(MemberAttributes.ExcludeFromDigest))
                {
                    continue;
                }

                list.Add(member.GetMemberValue(obj));
            }

            if (digestMemberField == null) return String.Empty;
            RecordDigestProvider provider = digestMemberField.Properties.GetRecordDigestProvider();
            object[] objs = (object[])list.ToArray(typeof (object));

            return String.Format("[{1:0000}]{0}", provider.GenerateDigest(objs), version);

        }

        /// <summary>
        /// Generates the standard record digest.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        internal static string GenerateStandardRecordDigest(MapMemberList members, object obj)
        {
            ArrayList list = new ArrayList();
            foreach (MapMember member in members)
            {
                if (member.Properties.HasRecordDigestProvider || member.Properties.ContainsAttribute(MemberAttributes.IgnoreChanges) || member.Properties.ContainsAttribute(MemberAttributes.ExcludeFromDigest))
                {
                    continue;
                }

                list.Add(member.GetMemberValue(obj));
            }

            if (list.Count == 0) return String.Empty;
            RecordDigestProvider provider = new DefaultRecordDigestProvider();
            object[] objs = (object[]) list.ToArray(typeof (object));

            return provider.GenerateDigest(objs);
        }

        /// <summary>
        /// Gets the record version.
        /// </summary>
        /// <param name="fulldigestText">The fulldigest text.</param>
        /// <returns></returns>
        internal static int GetRecordVersion(string fulldigestText)
        {
            if (fulldigestText == null || fulldigestText.Length == 0) return 0;

            int version = 0;
            Match match = Regex.Match(fulldigestText, _VERSION_REGEX);

            if (match.Success)
            {
                try
                {
                    version = Convert.ToInt32(match.Value);
                }
                catch (Exception)
                {
                    version = 0;
                }
            }

            return version;
           
        }

        /// <summary>
        /// Compares the record digest.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        internal static DigestComparisonResult CompareRecordDigest(MapMemberList members, object obj)
        {
            ArrayList list = new ArrayList();
           // MapMember digestMemberField = null;
            string digest = String.Empty;
            RecordDigestProvider provider = null;
            foreach (MapMember member in members)
            {
                if (member.Properties.HasRecordDigestProvider)
                {
                    string fulldigestText = Convert.ToString(member.GetMemberValue(obj));
                    if (fulldigestText == null || fulldigestText.Length == 0)
                    {
                        return DigestComparisonResult.EmptyDigest;
                    }

                    provider = member.Properties.GetRecordDigestProvider();
                    Match match = Regex.Match(fulldigestText, _DIGEST_REGEX);
                    
                    if (match.Success)
                    {
                        digest = match.Value;
                    }

                    continue;
                }
                if ((member.Properties.ModeType == ColumnModeType.ExcludeFromDigest 
                    || member.Properties.ModeType == (ColumnModeType.ExcludeFromDigest | ColumnModeType.IgnoreChanges))
                    || member.Properties.ModeType == ColumnModeType.IgnoreChanges
                   )

                {
                    continue;
                }

                list.Add(member.GetMemberValue(obj));
            }

            if (provider == null) return DigestComparisonResult.NoDigestMemberExist;
            
            object[] objs = (object[]) list.ToArray(typeof (object));
            return provider.ValidateDigest(digest, objs);
        }

        /// <summary>
        /// Gets the digest.
        /// </summary>
        /// <param name="fulldigestvalue">The fulldigestvalue.</param>
        /// <returns></returns>
        internal static string GetDigest(string fulldigestvalue)
        {
            Match match = Regex.Match(fulldigestvalue, _DIGEST_REGEX);
            string digest = String.Empty;
            if (match.Success)
            {
                digest = match.Value;
            }

            return digest;
        }

        /// <summary>
        /// Sets the record digest.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        internal static void SetRecordDigest(MapMemberList members, object obj, int version)
        {
            ArrayList list = new ArrayList();
            MapMember digestMemberField = null;
            foreach (MapMember member in members)
            {
                if (member.Properties.HasRecordDigestProvider)
                {
                    digestMemberField = member;
                    continue;
                }

                list.Add(member.GetMemberValue(obj));
            }

            if (digestMemberField == null) return;
            RecordDigestProvider provider = digestMemberField.Properties.GetRecordDigestProvider();
            object[] objs = (object[])list.ToArray(typeof(object));
            string digest = String.Format("[{1:0000}]{0}", provider.GenerateDigest(objs), version);
            
            digestMemberField.SetMemberValue(obj, digest);
            Debug.WriteLine("Set digest value "+digest);
        }

        /// <summary>
        /// Dbs the type mapping.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns></returns>
        internal static DbType DbTypeMapping(MapMemberProperties properties, DataType dataType)
        {
            switch (dataType)
            {

                case DataType.String:
                {
                    if (properties.Encoding == Encoding.ASCII)
                    {
                        return DbType.AnsiString;
                    }
                    else
                    {
                        return DbType.String;
                    }
                }
                case DataType.Enum:
                    {
                        return DbType.AnsiString;
                    }
                case DataType.DateTime:
                    {

                        return DbType.DateTime;

                    }
                case DataType.Int16:

                    {
                        return DbType.Int16;
                    }
                case DataType.Int32:
                    {
                        return DbType.Int32;
                    }
                case DataType.Int64:
                    {
                        return DbType.Int64;
                    }
                case DataType.Double:
                    {
                        return DbType.Double;
                    }
                case DataType.Byte:
                    {
                        return DbType.Byte;
                    }
                case DataType.Boolean:
                    {
                        return DbType.Boolean;
                    }
                case DataType.Decimal:
                    {
                        return DbType.Decimal;
                    }
                case DataType.Single:
                    {
                        return DbType.Single;
                    }
                case DataType.Guid:
                    {
                        return DbType.Guid;
                    }
                case DataType.Xml:
                    {
                        return DbType.String;
                    }
                case DataType.Object:
                    {
                        return DbType.Binary;
                    }
                case DataType.Binary:
                    {
                        return DbType.Binary;
                    }
                default:
                    {
                        return DbType.String;
                    }

            }
        }
    }
}
