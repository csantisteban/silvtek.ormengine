//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;


//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  Represents a collection of SQL Join statements.
//    /// </summary>
//    public sealed class JoinClauseList : CollectionBase
//    {
//        // Holds the main dataobject type.
//        private Type main;
//        private bool isCaseSensitive;
//        private JoinClause currentSQLJoin;
//        private int currentindex;


//        ///<summary>
//        /// Gets the current <see cref="SQLJoin" /> instance.
//        ///</summary>
////        public JoinClause Last
////        {
////          get {return currentSQLJoin;}
////        }

//        ///<summary>
//        /// Gets a <see cref="SQLJoin" /> stored in the collection.
//        ///</summary>
//        ///<param name="index">
//        /// The index.
//        ///</summary>
////        public JoinClause this[int index]
////        {
////          get {return GetSQLJoin(index);}
////        }

//        ///<summary>
//        /// Gets the current index.
//        ///</summary>
////        public int CurrentIndex
////        {
////          get {return currentindex;}
////        }

//        ///<summary>
//        /// Constructor.
//        ///</summary>
//        ///<param name="type">
//        /// The main <see cref="Type" />.
//        ///</param>
//        ///<param name="isCaseSensitive">
//        /// Whether the query is case sensitive.
//        ///</param>
//        internal JoinClauseList(Type type, bool isCaseSensitive)
//        {
//          this.main = type;
//          this.isCaseSensitive = isCaseSensitive;
//          currentindex = -1;
//        }

//        ///<summary>
//        /// Returns a <see cref="SQLJoin" /> stored in the collection.
//        ///</summary>
//        ///<param name="i" />
//        /// The index.
//        ///</param>
////        internal JoinClause GetSQLJoin(int i)
////        {
////          return (JoinClause)List[i];
////        }


//        public void Add(JoinType jointype, Type type1, string property1, Comparison comparison, Type type2,	 string property2)
//        {
//           if (type1 == null)throw new ArgumentNullException(
//                                    "Argument type cannot be null !", "type1");


//           if (type2 == null)throw new ArgumentNullException(
//                                    "Argument type cannot be null !", "type2");


//           JoinClause join = new JoinClause(jointype, type1, property1, comparison,
//                                      type2, property2, isCaseSensitive);
//           currentindex =  List.Add(join);
//           currentSQLJoin = join;

//        }

//        public void Add(JoinType jointype, string property1, Comparison comparison, Type type, string property2)
//        {
//            if (type == null)
//            {
//               throw new ArgumentNullException("Cannot be null !", "type");
//            }

//            JoinClause join = new JoinClause(jointype, main, property1, comparison,
//                                       type, property2, isCaseSensitive);

//            currentindex = List.Add(join);
//            currentSQLJoin = join;
//        }
        
//        #region -- Inner Join --

//        /// <summary>
//        ///	  Adds an inner join.
//        /// </summary>
//        /// <param name="type1">The type of the first object to be inner joined</param>
//        /// <param name="property1">The property or member of the first object to be inner joined.</param>
//        /// <param name="comparison">The type of comparison between the first object and the second object.</param>
//        /// <param name="type2">The type of the second object to be inner joined.</param>
//        /// <param name="property2">The property of the second object to inner join.</param>
//        public void Inner(Type type1, string property1, Comparison comparison, Type type2, string property2)
//        {
//            Add(JoinType.Inner, type1, property1, comparison, type2, property2);
//        }

//        /// <summary>
//        ///	  Adds an inner join.
//        /// </summary>
//        /// <param name="property1">The property or member of main object declared in the <see cref="Query"/> object to be inner joined.</param>
//        /// <param name="comparison">The type of comparison between the first object and the second object.</param>
//        /// <param name="type2">The type of the second object to be inner joined.</param>
//        /// <param name="property2">The property of the second object to inner join.</param>
//        public void Inner(string property1, Comparison comparison, Type type2, string property2)
//        {
//            Add(JoinType.Inner, property1, comparison, type2, property2);
//        }

//        #endregion

//        #region -- Left Join --
//        /// <summary>
//        ///	  Adds a left join
//        /// </summary>
//        /// <param name="type1">The type of the first object to be left joined</param>
//        /// <param name="propertyName1">The property or member of the first object to be left joined.</param>
//        /// <param name="comparison">The type of comparison between the first object and the second object.</param>
//        /// <param name="type2">The type of the second object to be left joined.</param>
//        /// <param name="propertyName2">The property of the second object to left join.</param>
//        public void Left(Type type1, string propertyName1, Comparison comparison, Type type2, string propertyName2)
//        {
//            Add(JoinType.Left, type1, propertyName1, comparison, type2, propertyName2);
//        }

//        /// <summary>
//        ///	  Adds a left join
//        /// </summary>
//        /// <param name="propertyName1">The property or member of the first object to be left joined which is the object declared on the <see cref="Query" /></param>
//        /// <param name="comparison">The type of comparison between the first object and the second object.</param>
//        /// <param name="type2">The type of the second object to be left joined.</param>
//        /// <param name="propertyName2">The property of the second object to left join.</param>
//        public void Left(string propertyName1, Comparison comparison, Type type2, string propertyName2)
//        {
//            Add(JoinType.Left, propertyName1, comparison, type2, propertyName2);
//        }

//        #endregion

//        public void Right(Type type1, string prop1, Comparison comparison, Type type2, string prop2)
//        {
//           Add(JoinType.Right, type1, prop1, comparison, type2, prop2);
//        }

//        public void Right(string prop1, Comparison comparison, Type type2, string prop2)
//        {
//           Add(JoinType.Right, prop1, comparison, type2, prop2);
//        }

//        public void Outer(Type type1, string prop1, Comparison comparison, Type type2, string prop2)
//        {
//           Add(JoinType.Outer, type1, prop1, comparison, type2, prop2);
//        }

//        public void Outer(string prop1, Comparison comparison, Type type2, string prop2)
//        {
//           Add(JoinType.Outer, prop1, comparison, type2, prop2);
//        }


//        public override string ToString()
//        {
//            string join_str = String.Empty;
//            foreach(JoinClause joinObj in List)
//            {
//                join_str += joinObj.ToString();
//            }

//           return join_str;
//        }
//    }

//}
