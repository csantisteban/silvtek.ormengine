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
//    /// Summary description for Class.
//    /// </summary>
//    public class HavingClauseList : CollectionBase
//    {
//        private Type main;
//        private bool isCaseSensitive;


//        internal HavingClause this[int index]
//        {
//            get {return (HavingClause)InnerList[index];}
//        }
		
		
//        internal HavingClauseList(Type type, bool isCaseSensitive)
//        {
//           this.main = type;
//           this.isCaseSensitive = isCaseSensitive;
//        }


//        public void Add(Aggregate aggregate, string property, Comparison comparison, object value)
//        {
//            Add(LogicalOperator.None, aggregate, main, property, comparison, value, false);
//        }

//        public void Add(Aggregate aggregate, string property, Comparison comparison, object value, bool distinct)
//        {
//            Add(LogicalOperator.None, aggregate, main, property, comparison, value, distinct);
//        }
	
//        public void Add(LogicalOperator prefixOp, Aggregate aggregate, Type type, string property, Comparison comparison, 
//                        object value, bool distinct)
//        {

//          bool isIn		 =	(comparison == Comparison.In);
//          bool isNotIn	 =	(comparison == Comparison.NotIn);
//          bool isNull	 =	(comparison == Comparison.IsNull);
//          bool isNotNull =	(comparison == Comparison.IsNotNull);

//          if ( isIn || isNotIn || isNull || isNotNull)
//          {
//              string exMsg = String.Format("Comparison {0} not supported in a having clause", comparison);
//              throw new QueryException(exMsg);
//          }

//          if (type == null)type = main;
          

//          if (List.Count == 0 && prefixOp != LogicalOperator.None)
//          {
//            prefixOp = LogicalOperator.None;
//          }

//          else if(List.Count > 0 && prefixOp == LogicalOperator.None)
//          {
//            prefixOp = LogicalOperator.And;
//          }


//          HavingClause tmpHaving = null;
//          if (comparison == Comparison.Between || comparison == Comparison.NotBetween)
//          {
//             tmpHaving = new HavingClause(prefixOp, aggregate, type, property, comparison, value, value, isCaseSensitive, distinct);
//          }
//          else
//          {
//             tmpHaving = new HavingClause(prefixOp, aggregate, type, property, value, comparison, isCaseSensitive, distinct);
//          }

//          List.Add(tmpHaving);
//        }


//        public void AddRange(LogicalOperator op, Aggregate aggregate, Type type, string propertyname, Comparison comp,
//                             object value1, object value2, bool distinct)
//        {
//          if (type == null)type = main;

//          if (List.Count == 0 && op != LogicalOperator.None)
//          {
//            op = LogicalOperator.None;
//          }
//          else if(List.Count > 0 && op == LogicalOperator.None)
//          {
//            op = LogicalOperator.And;
//          }

//          HavingClause tmpHaving = new HavingClause(op, aggregate, type, propertyname,
//                                                   comp, value1, value2, isCaseSensitive,
//                                                   distinct);
//          List.Add(tmpHaving);
          
//        }

//        public override string ToString()
//        {
//            string having_str = String.Empty;
//            foreach(HavingClause havingObj in List)
//            {
//               having_str += havingObj.ToString();
//            }

//            return having_str;
//        }




//    }
//}
