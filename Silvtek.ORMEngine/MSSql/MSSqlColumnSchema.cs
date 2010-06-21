#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.NativeSchema;

namespace Silvtek.ORMEngine.MSSql
{
	/// <summary>
	/// Describe a microsoft column schema.
	/// </summary>
	public class MSSqlColumnSchema   : ColumnSchema
	{
		private int tableID;
		private int columnID;
		private string name;
		private string type;
		private bool acceptNull;
		private int length;

		public override bool AcceptsNull
		{
			get {return acceptNull;}
		}

		public override int ID
		{
			get
			{
				return columnID;
			}
		}

		/// <summary>
		/// Gets whether the column is a primary key.
		/// </summary>
		/// <value>Always return false for this implementation.</value>
		public override bool IsPrimaryKey
		{
			get {return false;	}
		}

		public override int TableID
		{
			get {return tableID;}
		}

		public override string NativeType
		{
			get { return type;}
		}

		public override string Name
		{
			get { return name;}
		}

		public override int Length
		{
			get	{return length;	 }
		}


		internal MSSqlColumnSchema(int tableID, int columnID, string name, int length, string nativeType, bool isNull)
		{
		    this.acceptNull = isNull;
			this.tableID = tableID;
			this.columnID = columnID;
			this.length = length;
			this.name = name;
			this.type = nativeType;
		}

		internal MSSqlColumnSchema()
		{
			this.acceptNull = true;
			this.tableID = -1;
			this.columnID = -1;
			this.length = 0;
			this.name = String.Empty;
			this.type = String.Empty;;
		}

			
	
	}
}
