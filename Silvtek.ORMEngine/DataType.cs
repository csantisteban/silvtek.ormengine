#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

namespace Silvtek.ORMEngine
{
   ///<summary>
   /// Defines the available data types.
   ///</summary>
   public enum DataType
   {
       ///<summary>
       /// Defines the column as a binary datatype.
       ///</summary>
       ///<remarks>
       /// This datatype is not yet implemented.
       ///</remarks>
       Binary,

       ///<summary>
       /// Defines the column as a boolean datatype.
       ///</summary>
       Boolean,

       ///<summary>
       /// Defines the column as a byte datatype.
       ///</summary>
       Byte,

       ///<summary>
       /// Defines the column as a DataTime datatype.
       ///</summary>
       DateTime,

       ///<summary>
       /// Defines the column as a Decimal datatype.
       ///</summary>
       Decimal,

       ///<summary>
       /// Defines the column as a Double datatype.
       ///</summary>
       Double,

       ///<summary>
       /// Defines the column as a Guid datatype.
       ///</summary>
       Guid,

       ///<summary>
       /// Defines the column as a Int16 datatype.
       ///</summary>
       Int16,

       ///<summary>
       /// Defines the column as a Int32 datatype.
       ///</summary>
       Int32,

       ///<summary>
       /// Defines the column as a Int64 datatype.
       ///</summary>
       Int64,

       ///<summary>
       /// Defines the column as a Enum datatype.
       ///</summary>
       Enum,

       ///<summary>
       /// Defines the column as a Object datatype.
       ///</summary>
       ///<remarks>
       /// This datatype is not yet implemented.
       ///</remarks>
       Object,

       ///<summary>
       /// Defines the column as a String datatype.
       ///</summary>
       String,


	   /// <summary>
	   /// Defines the column as a <see cref="Single" /> datatype.
	   /// </summary>
	   Single,

       /// <summary>
       /// Defines the column as containing XML
       /// </summary>
       Xml,

       Undefined,

   }
}
