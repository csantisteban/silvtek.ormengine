#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion


/*
File:  ColumnAttribute
Author: Carlos Silva-Santisteban.
Version : 1.0
Modified: 3/15/2004
Created:  11/20/2003

Purpose:
 Mark a property as bound or beloging to a column.

History:

*/

using System;

namespace Silvtek.ORMEngine
{

    ///<summary>
    /// Maps a field or property to a database column
    ///</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public class ColumnAttribute : Attribute
	{
       
	#region -- Private Members --

        private readonly string columnName;     // Holds the column name.
        private readonly DataType type;         // Holds the data type.
        private readonly int length;            // Holds the precision in characters.
        private readonly object defaultValue;   // Holds the default value.
        private readonly Type enumType;         // Holds the type if the datatype is an enumeration.
        private readonly EnumConversionType enumConversionType; // Holds the convertion type if the datatype is an enumeration.
        private string fileExtension;
        private ColumnModeType modeType;

     #endregion
     
        ///<summary>
        /// Gets the column name.
        ///</summary>
        ///<value>
        /// The column name. The default value is empty string.
        ///</value>
        public string Name
        {
          get {return columnName; }
        }


        ///<summary>
        /// Gets the column <see cref="DataType" />.
        ///</summary>
        ///<value>
        /// The column <see cref="DataType" />. The default value
        /// is <b>DataType.String</b>.
        ///</value>
        public DataType DataType
        {
          get {return type;}
        }

		/// <summary>
		/// Gets the <see cref="Type" /> of the enumeration.
		/// </summary>
		/// <value>
		/// <b>Null</b> if the <see cref="DataType" /> is not <see cref="DataType" >DataType.Enum</see>.
		/// </value>
		public Type EnumerationType
		{
			get {return enumType;}
		}
			
		/// <summary>
		/// Gets the convertion type for enumerations.
		/// </summary>
		public EnumConversionType EnumerationConversion
		{
			get {return enumConversionType;}
		}

        ///<summary>
        /// Gets the column length
        ///</summary>
        ///<value>
        /// The column precision. The default value is -1.
        ///</value>
        public int Length
        {
            get { return length; }
        }
		 

        ///<summary>
        /// Gets the default value.
        ///</summary>
        ///<value>
        /// The default value.
        ///</value>
        public object Default
        {
            get { return defaultValue; }
        }

        /// <summary>
        /// Gets or sets the file extension for a binary
        /// </summary>
        /// <value>The file extension.</value>
        public string FileExtension
        {
            get { return fileExtension; }
            set { fileExtension = value; }
        }

        public ColumnModeType ModeType
        {
            get { return modeType; }
            set { modeType = value; }
        }


        ///<summary>
        /// Constructor.
        ///</summary>
        ///<param name="name">
        /// The column name.
        ///</param>
        ///<param name="type">
        /// The <see cref="DataType" /> of the column.
        ///</param>
        ///<param name="length">
        /// The number of characters in a <see cref="DataType">DataType.String</see> column.
        ///</param>
        ///<param name="defaultValue" >
        /// The default value for this column.
        ///</param>
        public ColumnAttribute(string name, DataType type, int length, object defaultValue)
        {
            columnName = name;
            enumType  = null;
            this.type = type;
            this.length = length;
            this.defaultValue = defaultValue;
            fileExtension = String.Empty;
            modeType = ColumnModeType.Default;

            if (this.length == -1) this.length = Int32.MaxValue;
			if (type == DataType.Enum)
			{
				throw new ORMException(Utility.GetResourceString("RES_IsEnum"));
			}
        }

      
        ///<summary>
        /// Constructor.
        ///</summary>
        ///<param name="name">
        /// The column name.
        ///</param>
        ///<param name="type">
        /// The <see cref="DataType" /> of the column.
        ///</param>
        public ColumnAttribute(string name, DataType type) : this(name, type, -1, null)  {}


        ///<summary>
        /// constructor.
        ///</summary>
        ///<param name="name">
        /// The name of the column.
        ///</param>
        ///<param name="type">
        /// The <see cref="DataType" />.
        ///</param>
        ///<param name="enumType">
        /// The enumeration type.
        ///</param>
        /// <param name="conversionType">
        /// The type of convertion between enumeration and the value to be stored in the database.
        /// </param>
        ///<remarks>
        /// <b>This constructor is meant to be used with <see cref="ORMEngine.DataType" >DataType.Enum</see>.</b>
        ///</remarks>
        /// <seealso cref="EnumConversionType" />
        /// <seealso cref="DataType" />
		public ColumnAttribute(string name, DataType type, Type enumType, EnumConversionType conversionType)
		{
			if (type != DataType.Enum)
			{
				throw new ORMException(Utility.Resources["RES_NonEnum"]);
			}

			this.type = type;
            this.enumType = enumType;
            length = -1;
			defaultValue = null;
            columnName = name;
            fileExtension = "bin";
			enumConversionType = conversionType;
            modeType = ColumnModeType.Default;
        }

        [Obsolete("Use EnumConversionType instead")]
        public ColumnAttribute(string name, DataType type, Type enumType, EnumConvertionType conversionType)
        {
            if (type != DataType.Enum)
            {
                throw new ORMException(Utility.Resources["RES_NonEnum"]);
            }

            columnName = name;
            length = -1;
            defaultValue = null;
            this.enumType = enumType;
            this.type = type;
            fileExtension = "bin";
            modeType = ColumnModeType.Default;
            enumConversionType = (EnumConversionType)Enum.ToObject(typeof(EnumConversionType), ((int)conversionType)); 
        }

        ///<summary>
        /// Constructor.
        ///</summary>
        ///<param name="name">
        /// The column name.
        ///</param>
        ///<param name="type">
        /// The <see cref="DataType" />.
        ///</param>
        ///<param name="length">
        /// The number of characters in a <see cref="DataType">DataType.String</see> column.
        ///</param>
        public ColumnAttribute(string name, DataType type, int length)  : this(name, type, length, null) {}


        ///<summary>
        /// constructor.
        ///</summary>
        ///<param name="name">
        /// The column name.
        ///</param>
        ///<param name="type">
        /// The <see cref="DataType" />.
        ///</param>
        ///<param name="defaultValue" >
        /// The default value for this column.
        ///</param>
        public ColumnAttribute(string name, DataType type, object defaultValue) : this(name, type, -1, defaultValue){}


        ///<summary>
        /// constructor.
        ///</summary>
        ///<param name="name">
        /// The name of the column.
        ///</param>
        public ColumnAttribute(string name) : this(name, DataType.Undefined, -1, null){}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Column Name</param>
        /// <param name="defaultValue">Default value, if the value for this column is null</param>
        public ColumnAttribute(string name, object defaultValue) : this(name, DataType.Undefined, -1, defaultValue){}

	}
}
