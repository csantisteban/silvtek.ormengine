using System;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Represents default values for each datatype. 
    /// </summary>
    public class ORMDefaults
    {
        public static readonly string STRING = String.Empty;
        public const Int32 INT32 = -1;
        public const Int64 INT64 = -1;
        public const Int16 INT16 = -1;
        public static readonly DateTime DATETIME = DateTime.MinValue;
        public const Double DOUBLE = -1;
        public const float FLOAT = -1f;
        public const float SINGLE = -1f;
        public static readonly Decimal DECIMAL = Decimal.Parse("-1");
        public static readonly Guid GUID = Guid.Empty;

        private ORMDefaults()
        {

        }
    }
}
