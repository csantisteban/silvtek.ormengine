//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{

//    /// <summary>
//    ///  Represent a grouped criteria.
//    /// </summary>
//    public class GroupedCriteria : ICriteria
//    {
//        // WHERE CRITERIA TEMPLATE
//       private const string GROUPED_CRITERIA = "{op} ({criteria})";


//       private CriteriaList criterias;
//       private LogicalOperator op;
//       private Type main;
//       private bool isCaseSensitive;

//       public string SqlClause
//       {
//         get {return GetParsedCriteria();}
//       }

//       public LogicalOperator LogicalOperator
//       {
//         get {return op;}
//       }

//       ///<summary>
//       /// Gets the <see cref="SQLCriteriaCollection" />.
//       ///</summary>
//       ///<example>
//       ///<para>To add a criteria uses the following code.</para>
//       ///<code>
//       /// [GroupedCriteria Instance].Criteria.Add(.......);
//       ///</code>
//       ///</example>
//       public CriteriaList Criteria
//       {
//         get {return criterias;}
//       }

//       internal GroupedCriteria(LogicalOperator op, Type type, bool isCaseSensitive)
//       {
//           this.criterias = new CriteriaList(type, isCaseSensitive);
//           this.op = op;
//           this.isCaseSensitive = isCaseSensitive;
//           this.main = type;
//       }


//        private string GetParsedCriteria()
//        {
//            string rv = String.Empty;
//            if (criterias.Count > 0)
//            {
//                StringBuffer criteria = new StringBuffer(GROUPED_CRITERIA);
//                criteria.SetTokenValue("criteria", criterias.ToString());
//                criteria.SetTokenValue("op", QueryHelper.ParseOperator(op));
//                rv = criteria.ToString();
//            }

//            return rv;
//        }

//        public void SetOperator(LogicalOperator op)
//        {
//           this.op = op;
//        }


//        public override int GetHashCode()
//        {
//           return criterias.GetHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//           if (obj.GetType() != GetType())return false;
//           GroupedCriteria tmp = (GroupedCriteria)obj;
//           return (tmp.criterias.GetHashCode() == this.criterias.GetHashCode());
//        }

//        public override string ToString()
//        {
//           return GetParsedCriteria();
//        }
//    }
//}
