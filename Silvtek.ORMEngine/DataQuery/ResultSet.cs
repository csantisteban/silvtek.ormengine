#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Silvtek.ORMEngine.Schema;


namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	///  Represents a Result Set adquired thru <see cref="Query" /> object.
	/// </summary>
	public class ResultSet : ReadOnlyCollectionBase, IEnumerable
	{

		#region -- MemberVariables --
        
		  private readonly string propertyKey;
          private readonly Type typeKey;
          private readonly HybridDictionary table;
		  private ResultColumn[] columns;
		  private readonly string identifier;
		  private readonly string sql;
         
		
		#endregion 

        ///<summary>
        /// Gets a single row in the collection.
        ///</summary>
        ///<param name="index">
        /// The index to retrieve.
        ///</param>
        ///<value>
        /// The <see cref="ResultRow" /> on <paramref name="index">.
        ///</value>
        public ResultRow this [int rowIndex]
        {
          get {return GetRow(rowIndex);}
        }

		public object this [int colIndex, int rowIndex]
		{
			get
			{
				ResultRow row = GetRow(rowIndex);
				return row[colIndex];
			}
		}

		public ResultColumn[] Columns
		{
			get{ return columns;}
		}


        ///<summary>
        /// Returns a <see cref="ResultRow" /> that is on <paramref name="index "/>
        ///</summary>
        ///<param name="index">
        /// The index to retrieve.
        ///</param>
        ///<returns>
        /// <see cref="ResultRow" />.
        ///</returns>
        public ResultRow GetRow(int index)
        {
          return (ResultRow)InnerList[index];
        }

        public ResultRow[] GetRows(int startIndex, int count)
        {
            if (InnerList.Count == 0)return new ResultRow[0];
            if (startIndex > InnerList.Count)return new ResultRow[0];

            if (startIndex < 0) startIndex = 0;
            if (count == -1) count = InnerList.Count;

            int lastIndex = startIndex + count;
            if (lastIndex  > (InnerList.Count - 1))
            {
                count = (InnerList.Count) - startIndex;
                
            }

            ResultRow[] rangeArray = new ResultRow[count];

            InnerList.CopyTo(startIndex, rangeArray, 0, count);

            return rangeArray;
        }

        /// <summary>
        /// Finds and object by a specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><see ref="ResultRow" /></returns>
		public ResultRow Find(object key)
		{
		   return (ResultRow)table[key];
		}

        [Obsolete("Do not use it.")]
        public ICollection GetKeys()
        {
           return table.Keys;
        }

        /// <summary>
        /// Query unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Identifier
        {
          get {return identifier;}
        }

        /// <summary>
        /// Sql executed that generated current result set.
        /// </summary>
        /// <value>The SQL.</value>
		public string Sql
		{
			get {return sql;}
		}

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ResultColumn GetColumn(int index)
        {
            return columns[index];
        }

		/// <summary>
		/// Returns a <see cref="ResultColumn" /> by the object type; If the result set does 
		/// not contain any requested type, it returns an empty typed column.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public ResultColumn GetColumn(Type type)
		{
			for(int i = 0; i < columns.Length; i++)
			{
				if (columns[i].Type == type)return columns[i];
			}

			return new ResultColumn(-1, type);
		}

        public ResultColumn GetColumn()
        {
            if (columns.Length == 0) return new ResultColumn(-1, typeof(object));
            return columns[0];
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Array GetObjects(Type type)
        {
            return GetColumn(type).ToArray(type);
        }

        public Array GetObjects(Type type, int startIndex, int count)
        {
            ResultColumn column = GetColumn(type);
            return column.ToArray(type, startIndex, count);
        }

        /// <summary>
        /// Gets the objects 
        /// </summary>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public Array GetObjects(int columnIndex, int startIndex, int count)
        {
            ResultColumn column = GetColumn(columnIndex);
            return column.ToArray(startIndex, count);
        }

        public Array GetObjects()
        {
            return GetColumn(0).ToArray();
        }

        public Array GetObjects(int startIndex, int count)
        {
            return GetObjects(0, startIndex, count);
        }

	    #region -- Internal Methods --

		internal ResultSet(Type type, string propname, string identifier, string sql, int numOfCols)
		{
			InitializeColumns(numOfCols);
			table = new HybridDictionary();
			typeKey = type;
			propertyKey = propname;
			this.sql = sql;
			this.identifier = identifier;
		}

		internal ResultSet()
		{
			InitializeColumns(0);
			table = new HybridDictionary(0);
			typeKey = null;
			propertyKey = String.Empty;
			sql = String.Empty;
			identifier = String.Empty;

		}

		internal void InitializeColumns(int numOfCols)
		{
			columns = new ResultColumn[numOfCols];
			for(int i = 0; i < columns.Length; i++)
			{
				columns[i] = new ResultColumn(i);
			}
		}	

        internal int AddRow(ResultRow row)
        {
           
			int index = InnerList.Add(row);
			AddToColumns(row);
            if (typeKey != null && propertyKey != null)AddKey(row);
            return index;
        }



        private void AddKey(ResultRow row)
        {
           ORMObject obj = (ORMObject)row.GetObject(typeKey);
           if (obj != null)
           {
             MapMember MappedMember = obj.ObjectInfo.MappedMembers.Find(propertyKey);
             
               if (!MappedMember.Properties.IsPrimaryKey && !MappedMember.Properties.HasSequenceProvider)
             {
				 string exMsg = Utility.GetResourceString("RESULTSET:EXCEPTION:NO_PK_OR_SEQ", typeKey.FullName, propertyKey);
                 throw new QueryException(exMsg);
             }
             
             object key = MappedMember.GetMemberValue(obj);

            // Debug.WriteLine(String.Format("{0}.{1} = {2}",typeKey.FullName, propertyKey, key));
             table.Add(key, row);
           }
        }


		private void AddToColumns(ResultRow row)
		{
			if (row == null)return;

			for(int i = 0; i < row.Count; i++)
			{
				columns[i].Add(row); 
			}
		}

    #endregion
	}
}
