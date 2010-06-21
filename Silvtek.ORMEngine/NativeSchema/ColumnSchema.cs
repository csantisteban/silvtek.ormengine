#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.NativeSchema
{
	/// <summary>
	/// Summary description for ColumnSchema.
	/// </summary>
	public abstract class ColumnSchema
	{

		public abstract int TableID
		{
			get; 
		}

		public abstract int ID
		{
			get; 
		}

		public abstract string Name
		{
			get; 
		}

		public abstract bool IsPrimaryKey
		{
			get; 
		}

		public abstract bool AcceptsNull
		{
			get; 
		}

		public abstract int Length
		{
			get; 
		}

		public abstract string NativeType
		{
			get; 
		}


		protected ColumnSchema()
		{

		}
	}
}
