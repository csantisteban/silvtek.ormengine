#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.NativeSchema
{
	/// <summary>
	/// Summary description for TableSchema.
	/// </summary>
	public abstract class TableSchema
	{

		public abstract int ID   {get;}
		public abstract string Name {get;}
		public abstract DateTime CreateDate {get;}

	
		public abstract ColumnSchema[] Columns {get;}
		public abstract ColumnSchema GetColumn(string columnName);
		public abstract ColumnSchema GetColumn(int id);
	
	}
}
