using System;
using System.IO;
using System.Xml;

namespace Silvtek.ORMEngine
{

    /// <summary>
    /// 
    /// </summary>
    public class ORMNull
    {
        /// <summary>
        /// Represents a pseudo null string.
        /// </summary>
        public static readonly string STRING = "\0";


        /// <summary>
        /// Represents a pseudo null value for an Int32.
        /// </summary>
        public const Int32 INT32 = -2147483647;


        public const Int64 INT64 = -9223372036854775807;
        public const Int16 INT16 = -32767;
        public static readonly DateTime DATETIME = DateTime.MinValue.AddTicks(1);
        public const Double DOUBLE = -1.79769313486232e307d;
        public const float FLOAT = 3.402823e37f;
        public const float SINGLE = 3.402823e37f;
        public static readonly Decimal DECIMAL = Decimal.Parse("79228162514264337593543950334");
        public static readonly Guid GUID = Guid.Empty;
        
        public static readonly object OBJECT = new ORMNull();
        public static readonly byte[] BINARY;
        public static readonly XmlDocument XMLDOCUMENT;
        public static readonly Stream STREAM = Stream.Null;

        static ORMNull()
        {
            BINARY = Guid.Empty.ToByteArray();
 
            XMLDOCUMENT = new XmlDocument();
            XMLDOCUMENT.LoadXml("<_0></_0>");
        }
    
        private ORMNull()
        {
            
        }
        

        
    }
}
