#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Represents a collection of instances retrieved from the database. These instance
	/// must be ORM Objects.
	/// </summary>
	public class ResultColumn : CollectionBase
	{

		private readonly int columnIndex;
		private Type type;
		private bool hasType;


        /// <summary>
        /// Gets the index of the column.
        /// </summary>
        /// <value>The index of the column.</value>
		public int ColumnIndex
		{
			get {return columnIndex;}
		}

        /// <summary>
        /// Gets the type that contains.
        /// </summary>
        /// <value>The type.</value>
		public Type Type
		{
			get {return type;}
		}


        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <value></value>
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

        /// <summary>
        /// Results an array for the type provided.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public Array ToArray(Type type, int startIndex, int count)
        {
            if (InnerList.Count == 0) return Array.CreateInstance(type, 0);
            if (startIndex > InnerList.Count) return Array.CreateInstance(type, 0);

            if (startIndex < 0) startIndex = 0;
            if (count == -1) count = InnerList.Count;

            int lastIndex = startIndex + count;
            if (lastIndex > (InnerList.Count - 1))
            {
                count = (InnerList.Count) - startIndex;

            }

            Array rangeArray = Array.CreateInstance(type, count);
            InnerList.CopyTo(startIndex, rangeArray, 0, count);

            return rangeArray;
        }

        /// <summary>
        /// Results an array for the type provided.
        /// </summary>
		public Array ToArray(Type type)
		{
		    if (InnerList.Count == 0)
			{
				return Array.CreateInstance(type, 0);
			}


		    return InnerList.ToArray(type);
		}

        /// <summary>
        /// Results an array the type assigned to this instance.
        /// </summary>
	    public Array ToArray()
		{
			return ToArray(type);
		}

        /// <summary>
        /// Results the array.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public Array ToArray(int startIndex, int count)
        {
            return ToArray(type, startIndex, count);
        }





		

	}
}
