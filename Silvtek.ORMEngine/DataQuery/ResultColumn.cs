#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Summary description for ResultRowCollection.
	/// </summary>
	public class ResultColumn : CollectionBase
	{

		private readonly int columnIndex;
		private Type type;
		private bool hasType;


		public int ColumnIndex
		{
			get {return columnIndex;}
		}

		public Type Type
		{
			get {return type;}
		}


		public object this[int index]
		{
			get {return Get(index);}
		}
		
		internal ResultColumn(int columnIndex)
		{
			this.columnIndex = columnIndex;
			type = typeof(Object);
			hasType = false;
		}

		internal ResultColumn(int columnIndex, Type type)
		{
			this.columnIndex = columnIndex;
			this.type = type;
			hasType = true;
		}

		internal void Add(ResultRow row)
		{
			object obj = row[columnIndex];
			InnerList.Add(obj);
			UpdateType(obj);
		}

		internal void UpdateType(object obj)
		{
            // if is null, does not have a type, so exit.

            if (obj == null) return;
			if (hasType)return;

			Type objType = obj.GetType();
			// If the type is object don't do anything else.
			//if (hasType && this.type == typeof(Object))return;

			// If the column does not have a type defined the defined.
			if (!hasType)
			{
				type = objType;
				hasType = true;
				return;
			}

			// If the type is not the same as the first type, then
			// set the type of this column as "object" which will mean that 
			// is a generic column.
//			if (this.type != objType)
//			{
//				type = typeof(Object);
//				hasType = true;
//			}
		}

		public object Get(int index)
		{
            if (InnerList.Count == 0) return null;
            
            if (index >= InnerList.Count)
            {
                return null;    
            }
            
            return InnerList[index];
            
		}

		public Array ToArray(Type type)
		{
			if (InnerList.Count == 0)
			{
				return Array.CreateInstance(type, 0);
			}
			else
			{
				return InnerList.ToArray(type);
			}
		}

		public Array ToArray()
		{
			return ToArray(type);
		}





		

	}
}
