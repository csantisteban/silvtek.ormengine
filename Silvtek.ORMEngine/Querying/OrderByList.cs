//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  A collection of order bys.
//    /// </summary>
//    public class OrderByList : CollectionBase
//    {

//        private Type main;
//        private Direction direction;
		
//        internal string this[int index]
//        {
//            get {return (string)InnerList[index];}
//        }
		
//        ///<summary>
//        ///  Defines the direction for the <see cref="Query" /> class.
//        ///</summary>
//        internal enum Direction : int
//        {
//            Ascending		= 1000,
//            Descending		= 2000
//        }

//        ///<summary>
//        /// Constructor.
//        /// </summary>
//       internal OrderByList(Type type)
//       {
//          this.main = type;
//       }

//       internal Direction InternalDirection
//       {
//         get {return direction;}
//       }

//       ///<summary>
//        /// Sets the <c>ascending</c> order the current <see cref="Query" />
//        /// </summary>
//        public void Ascending()
//       {
//           direction = Direction.Ascending;
//       }

//        ///<summary>
//        /// Sets the <c>Decending</c> order of the current <see cref="Query" />.
//       public void Descending()
//       {
//           direction = Direction.Descending;
//       }


		
//        /// <summary>
//        /// Adds an <see cref="type" /> to the collection.
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="propertyName">A  string</param>
//       public void Add(Type type, string propertyName)
//       {

//           if (type == null)type = main;
           
//           ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//           string columnFullName = String.Empty;

//           MapMember column = info.MappedMembers.Find(propertyName);

//           if (column != null)
//           {
//              columnFullName = String.Format("{0}.{1}", info.TableName, column.ColumnName);
//           }
//           else
//           {
//               string exMsg = Utility.GetResourceString("QUERY:ORDER_BY_EXCEPTION", propertyName, type.FullName);
//               throw new QueryException(exMsg);
//           }

//           List.Add(columnFullName);

//       }


//       public void Add(string propertyName)
//       {
//          Add(null, propertyName);
//       }

//       public override string ToString()
//       {
//          string orderby_str = String.Empty;
//          StringBuffer tmp_col = new StringBuffer("{comma} {col}");
//          tmp_col.SetTokenValue("comma", String.Empty);

//          foreach(string col in List)
//          {
//             tmp_col.SetTokenValue("col", col);
//             orderby_str += tmp_col.ToString();
//             tmp_col.SetTokenValue("comma", ",");
//          }

//          return orderby_str;
//       }

//       internal static string ParseDirection(Direction dir)
//       {
//          string rv = String.Empty;
//          switch(dir)
//          {
//            case Direction.Ascending:
//            rv = "asc";
//            break;
//            case Direction.Descending:
//            rv = "desc";
//            break;
//          }

//          return rv;
//       }


//    }
//}
