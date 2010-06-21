#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Data;

namespace Silvtek.ORMEngine.Data
{
	/// <summary>
	/// Represents a parameter to a <see cref="Command"/>Command, and optionally, its mapping to <see cref="DataSet" />
	/// columns. This class cannot be inherited.
	/// </summary>

	public sealed class DataParameter : IDataParameter, IDbDataParameter
	{

		#region -- member variables --

		private ParameterDirection direction;
		private DbType dbtype;
		private string parameterName;
		private object paramValue;
		private string sourceColumn;
		private DataRowVersion dataRowVersion;
		private byte precision;
		private byte scale;
		private int size;
		private bool isNullable;
		
		#endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="DataParameter"/> class.
        /// </summary>
		public DataParameter()
		{
			this.parameterName = String.Empty;
			this.direction = ParameterDirection.Input;
			this.DbType = DbType.String;
			this.paramValue = null;
			this.isNullable = true;
			this.sourceColumn = String.Empty;
			this.scale = 0;
			this.dataRowVersion = DataRowVersion.Current;
			this.precision = 0;
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="DataParameter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
		public DataParameter(string name, object value)
		{
		    this.parameterName = name;
		    this.paramValue = value;

			this.direction = ParameterDirection.Input;
			this.DbType = DbType.String;
			this.isNullable = true;
			this.sourceColumn = String.Empty;
			this.scale = 0;
			this.dataRowVersion = DataRowVersion.Current;
			this.precision = 0;

		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DataParameter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType">Type of the db.</param>
		public DataParameter(string name, object value, DbType dbType)
		{
			this.parameterName = name;
			this.paramValue = value;
			this.dbtype = dbType;

			this.direction = ParameterDirection.Input;
			this.isNullable = true;
			this.sourceColumn = String.Empty;
			this.scale = 0;
			this.dataRowVersion = DataRowVersion.Current;
			this.precision = 0;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DataParameter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType">Type of the db.</param>
        /// <param name="direction">The direction.</param>
		public DataParameter(string name, object value, DbType dbType, ParameterDirection direction)
		{
			this.parameterName = name;
			this.paramValue = value;
			this.dbtype = dbType;
			this.direction = direction;

			this.isNullable = true;
			this.sourceColumn = String.Empty;
			this.scale = 0;
			this.dataRowVersion = DataRowVersion.Current;
			this.precision = 0;
		}


		#region IDataParameter Members

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Data.ParameterDirection"/> values. The default is Input.</returns>
        /// <exception cref="T:System.ArgumentException">The property was not set to one of the valid <see cref="T:System.Data.ParameterDirection"/> values. </exception>
		public ParameterDirection Direction
		{
			get	{ return direction; }
			set { direction = value; }
		}

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DbType"/> of the parameter.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Data.DbType"/> values. The default is <see cref="F:System.Data.DbType.String"/>.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property was not set to a valid <see cref="T:System.Data.DbType"/>. </exception>
		public DbType DbType
		{
			get {return dbtype;}
			set	{ dbtype = value;}
		}

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Object"/> that is the value of the parameter. The default value is null.</returns>
		public object Value
		{
			get	{return paramValue;}
			set {paramValue = value;}
		}

        /// <summary>
        /// Gets a value indicating whether the parameter accepts null values.
        /// </summary>
        /// <value></value>
        /// <returns>true if null values are accepted; otherwise, false. The default is false.</returns>
		public bool IsNullable
		{
			get {return isNullable;}
		}

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DataRowVersion"/> to use when loading <see cref="P:System.Data.IDataParameter.Value"/>.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Data.DataRowVersion"/> values. The default is Current.</returns>
        /// <exception cref="T:System.ArgumentException">The property was not set one of the <see cref="T:System.Data.DataRowVersion"/> values. </exception>
		public System.Data.DataRowVersion SourceVersion
		{
			get  { return dataRowVersion;}
			set  { dataRowVersion = value;}
		}

        /// <summary>
        /// Gets or sets the name of the <see cref="T:System.Data.IDataParameter"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the <see cref="T:System.Data.IDataParameter"/>. The default is an empty string.</returns>
		public string ParameterName
		{
			get	{return parameterName;}
			set {parameterName = value;}
		}

        /// <summary>
        /// Gets or sets the name of the source column that is mapped to the <see cref="T:System.Data.DataSet"/> and used for loading or returning the <see cref="P:System.Data.IDataParameter.Value"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the source column that is mapped to the <see cref="T:System.Data.DataSet"/>. The default is an empty string.</returns>
		public string SourceColumn
		{
			get	{return sourceColumn;}
			set {sourceColumn = value;}
		}

		#endregion

		#region IDbDataParameter Members

        /// <summary>
        /// Indicates the precision of numeric parameters.
        /// </summary>
        /// <value></value>
        /// <returns>The maximum number of digits used to represent the Value property of a data provider Parameter object. The default value is 0, which indicates that a data provider sets the precision for Value.</returns>
		public byte Precision
		{
			get	{return precision;}
			set {precision = value;}
		}

        /// <summary>
        /// Indicates the scale of numeric parameters.
        /// </summary>
        /// <value></value>
        /// <returns>The number of decimal places to which <see cref="T:System.Data.OleDb.OleDbParameter.Value"/> is resolved. The default is 0.</returns>
		public byte Scale
		{
			get { return scale; }
			set	{ scale = value; }
		}

        /// <summary>
        /// The size of the parameter.
        /// </summary>
        /// <value></value>
        /// <returns>The maximum size, in bytes, of the data within the column. The default value is inferred from the the parameter value.</returns>
		public int Size
		{
			get { return size; }
			set { size = value;}
		}

		#endregion
	}
}
