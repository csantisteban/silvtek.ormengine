//using System;
//using System.Collections;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using Silvtek.ORMEngine.Schema;


//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  Represents a Result Set adquired thru <see cref="Query" /> object.
//    /// </summary>
//    public class ResultSet : ReadOnlyCollectionBase, IEnumerable
//    {

//        #region -- MemberVariables --
        
//          private string propertyKey;
//          private Type typeKey;
//          private HybridDictionary table;
//          private ResultColumn[] columns;
//          private string identifier;
//          private string sql;
         
		
//        #endregion 

//        ///<summary>
//        /// Gets a single row in the collection.
//        ///</summary>
//        ///<param name="index">
//        /// The index to retrieve.
//        ///</param>
//        ///<value>
//        /// The <see cref="ResultRow" /> on <paramref name="index">.
//        ///</value>
//        public ResultRow this [int rowIndex]
//        {
//          get {return GetRow(rowIndex);}
//        }

//        public object this [int colIndex, int rowIndex]
//        {
//            get
//            {
//                ResultRow row = GetRow(rowIndex);
//                return row[colIndex];
//            }
//        }

//        public ResultColumn[] Columns
//        {
//            get{ return columns;}
//        }


//        ///<summary>
//        /// Returns a <see cref="ResultRow" /> that is on <paramref name="index "/>
//        ///</summary>
//        ///<param name="index">
//        /// The index to retrieve.
//        ///</param>
//        ///<returns>
//        /// <see cref="ResultRow" />.
//        ///</returns>
//        public ResultRow GetRow(int index)
//        {
//          return (ResultRow)InnerList[index];
//        }

//        public ResultRow Find(object key)
//        {
//           return (ResultRow)table[key];
//        }


//        public ICollection GetKeys()
//        {
//           return table.Keys;
//        }

//        public string Identifier
//        {
//          get {return identifier;}
//        }
        
//        public string Sql
//        {
//            get {return sql;}
//        }

//        public ResultColumn GetColumn(int index)
//        {
//            return columns[index];
//        }

//        /// <summary>
//        /// Returns a <see cref="ResultColumn" /> by the object type; If the result set does 
//        /// not contain any requested type, it returns an empty typed column.
//        /// </summary>
//        /// <param name="type">The type.</param>
//        /// <returns></returns>
//        public ResultColumn GetColumn(Type type)
//        {
//            for(int i = 0; i < columns.Length; i++)
//            {
//                if (columns[i].Type == type)return columns[i];
//            }

//            return new ResultColumn(-1, type);
//        }

//    #region -- Internal Methods --

//        internal ResultSet(Type type, string propname, string identifier, string sql, int numOfCols)
//        {
//            InitializeColumns(numOfCols);
//            this.table = new HybridDictionary();
//            this.typeKey = type;
//            this.propertyKey = propname;
//            this.sql = sql;
//            this.identifier = identifier;
//        }

//        internal ResultSet()
//        {
//            InitializeColumns(0);
//            this.table = new HybridDictionary(0);
//            this.typeKey = null;
//            this.propertyKey = String.Empty;
//            this.sql = String.Empty;
//            this.identifier = String.Empty;

//        }

//        internal void InitializeColumns(int numOfCols)
//        {
//            this.columns = new ResultColumn[numOfCols];
//            for(int i = 0; i < columns.Length; i++)
//            {
//                columns[i] = new ResultColumn(i);
//            }
//        }	

//        internal int AddRow(ResultRow row)
//        {
           
//            int index = InnerList.Add(row);
//            AddToColumns(row);
//            if (typeKey != null && propertyKey != null)AddKey(row);
//            return index;
//        }



//        private void AddKey(ResultRow row)
//        {
//           ORMObject obj = row.GetORMObject(typeKey);
//           if (obj != null)
//           {
//             MapMember colprop;
//             QueryHelper.GetDataObjectColumn(obj.GetType(), propertyKey, out colprop, false);

//             if (!colprop.Properties.IsPrimaryKey && !colprop.Properties.HasSequenceProvider)
//             {
//                 string exMsg = Utility.GetResourceString("RESULTSET:EXCEPTION:NO_PK_OR_SEQ", typeKey.FullName, propertyKey);
//                 throw new QueryException(exMsg);
//             }
             
//             object key = colprop.GetValue(obj);

//             Debug.WriteLine(String.Format("{0}.{1} = {2}",typeKey.FullName, propertyKey, key));

//             table.Add(key, row);
//           }
//        }


//        private void AddToColumns(ResultRow row)
//        {
//            if (row == null)return;

//            for(int i = 0; i < row.Count; i++)
//            {
//                columns[i].Add(row); 
//            }
//        }

//    #endregion
//    }
//}
