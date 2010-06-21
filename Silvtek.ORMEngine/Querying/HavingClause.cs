//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  Represents a SQL Having clause
//    /// </summary>
//    internal class HavingClause
//    {
//        // Holds the template to form the clause
//        private const string TEMPLATE = " {op} ({col} {condition} {value}) ";

//        // Holds the string template to form the between clause.
//        private const string TEMPLATE_2 = " {op} ({col} between {value} and {value2}) ";

//        // Holds whether the current clause is case-sensitive
//        private bool casesensitive;

//        // The StringBuffer that holds the templates.
//        private StringBuffer strbuff;

//        // The SQLAggregate for current clause.
//        private Aggregate aggregate;

//        // Whether current clause will use the distinct keyword
//        private bool distinct;

//        // The boding LogicalOperator.
//        private LogicalOperator op;

//        // Holds the property name.
//        private string membername;

//        // Holds the dataobject
//        private Type main;

//        // Holds the value.
//        private object value;

//        // Holds the value2.
//        private object value2;

//        private Guid instanceSignature;

//        private Comparison sqlcomparison;
        
//        ///<summary>
//        /// Gets whether current clause is case-sensitive.
//        ///</summary>
//        internal bool CaseSensitive
//        {
//           get {return casesensitive;}
//        }

//        ///<summary>
//        /// Gets the <see cref="Comparison" /> for this having clause.
//        ///</summary>
//        public Comparison Comparison
//        {
//          get {return sqlcomparison;}
//        }

//        ///<summary>
//        /// Gets the value to be compared or searched.
//        ///</summary>
//        ///<value>
//        /// The value for which the database will be compared. 
//        ///</value>
//        ///<remarks>
//        /// if <see cref="Comparison" /> is <see cref="Comparison">Comparison.In</see>
//        /// or <see cref="Comparison">Comparison.NotIn</see> the value will be an
//        ///<see cref="ICollection" />.
//        ///</remarks>
//        public object Value
//        {
//          get {return value;}
//        }


//        ///<summary>
//        /// Gets the second value of a <b>Range</b> criteria.
//        ///</summary>
//        ///<value>
//        /// The second value of a <b>Range</b> criteria; 
//        /// if current criteria is not <see cref="Comparison">Comparison.Between</see> or
//        ///<see cref="Comparison">Comparison.NotBetween</see> it will be  <see cref="Null" />.
//        ///</value>
//        public object Value2
//        {
//          get {return value2;}
//        }

//        ///<summary>
//        /// Gets the <see cref="LogicalOperator" /> that binds this having clause to others (if any).
//        ///</summary>
//        internal LogicalOperator LogicalOperator
//        {
//           get {return op;}
//        }


//        #region -- HavingClause constructors --

//        /// <summary>
//        /// Instanciate a new having clause.
//        /// </summary>
//        /// <param name="op">
//        /// The operator for binding to other having clause.
//        /// Use <cref="Operator">Operator.None</cref="Operator"> if there is not previous
//        /// having clause.
//        /// </param>
//        /// <param name="aggregate">
//        ///  The Aggregate operator.
//        /// </param>
//        /// <param name="type">
//        ///  The type object which reference to the table to be query. It must be an <see cref="ORMObject" />.
//        /// </param>
//        /// <param name="memberName">
//        ///	   The member name used for comparison. 
//        /// </param>
//        /// <param name="value">The value to be compared</param>
//        /// <param name="comparison">The comparison operator to compare the <paramref name="memberName" /> to the <paramref name="value" /></param>
//        /// <param name="isCaseSensitive">Whether the comparison is case sensitive or not.</param>
//        /// <param name="distinct">Weather the having clause will use the distinct clause or not.</param>
//        internal HavingClause(LogicalOperator op, Aggregate aggregate, Type type, string memberName, object value,
//                              Comparison comparison, bool isCaseSensitive, bool distinct)
//        {
          
//           this.instanceSignature = Guid.NewGuid();
//           this.distinct = distinct;
//           this.casesensitive = isCaseSensitive;
//           this.aggregate = aggregate;
//           this.op = op;
//           this.membername = memberName;
//           this.main = type;
          
//           strbuff = new StringBuffer(TEMPLATE);
//           strbuff.SetTokenValue("op", QueryHelper.ParseOperator(op));

//           SetComparison(comparison);

//           if (comparison == Comparison.IsNull || comparison == Comparison.IsNotNull)
//           {
//              SetColumn(null);
//           }
//           else
//           {
//              SetColumn(value);
//           }

//        }


//        /// <summary>
//        /// Instanciate a new <b>Range</b> having clause
//        /// </summary>
//        /// <param name="op">
//        /// The operator for binding to other having clause.
//        /// Use <cref="Operator">Operator.None</cref="Operator"> if there is not previous
//        /// having clause.
//        /// </param>
//        /// <param name="aggregate">
//        ///  The Aggregate operator.
//        /// </param>
//        /// <param name="type">
//        ///  The type object which reference to the table to be query. It must be an <see cref="ORMObject" />.
//        /// </param>
//        /// <param name="memberName">
//        ///	   The member name used for comparison. 
//        /// </param>
//        /// <param name="value">The first value of the range</param>
//        /// <param name="value1">The second value of the range</param>
//        /// <param name="comparison">The comparison operator to compare the <paramref name="memberName" /> to the <paramref name="value" /></param>
//        /// <param name="isCaseSensitive">Whether the comparison is case sensitive or not.</param>
//        /// <param name="distinct">Weather the having clause will use the distinct clause or not.</param>
//        internal HavingClause(LogicalOperator op, Aggregate aggregate, Type type, string memberName, Comparison comparison,
//                              object value1, object value2, bool isCaseSensitive,  bool distinct )
//        {

//           // check condition.
//          if (comparison != Comparison.Between && comparison != Comparison.NotBetween)
//          {
//              string exMsg = Utility.GetResourceString("QUERYEXCEPTION:NOT_SUPPORTED_WITH_BETWEEN", comparison);
//              throw new QueryException(exMsg);
//          }

//          this.instanceSignature = Guid.NewGuid();
//          this.distinct = distinct;
//          this.casesensitive = isCaseSensitive;
//          this.aggregate = aggregate;
//          this.main = type;
//          this.membername = memberName;

//           strbuff = new StringBuffer(TEMPLATE_2);
//           strbuff.SetTokenValue("op", QueryHelper.ParseOperator(op));
//           SetComparison(comparison);
//           SetColumn(value1, value2);
//        }

//        #endregion

//        public void SetValue(object value)
//        {
//            this.value = value;
//            SetColumn(value);
//        }

//        public void SetValue(object value1, object value2)
//        {
//            this.value = value1;
//            this.value2 = value2;
//            SetColumn(value, value2);
//        }

        
//        public override int GetHashCode()
//        {
//          return instanceSignature.GetHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//           if (obj.GetType() != GetType())return false;
//           HavingClause tmp = (HavingClause)obj;
//           return (tmp.instanceSignature == this.instanceSignature);
//        }

//        public override string ToString()
//        {
//           return strbuff.ToString();
//        }

//        public void SetComparison(Comparison  comparison)
//        {
//           this.sqlcomparison = comparison;
//           strbuff.SetTokenValue("condition", QueryHelper.ParseCondition(comparison));
//        }


//        private void SetColumn(object value)
//        {
//           ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(main);
//          MapMember col = info.MappedMembers.Find(this.membername);

//           if (col == null)
//           {
//                string exMsg = Utility.GetResourceString("EXCEPTION_QUERY_PROP_NOT_FOUND", membername, main.FullName);
//                throw new QueryException(exMsg);
//           }
       
//            string columnName = String.Format("{0}.{1}", info.TableName, col.ColumnName);
//            string parsedAggregate = QueryHelper.ParseAggregate(aggregate, columnName, distinct, col.Formatter, casesensitive);
//            strbuff.SetTokenValue("col", parsedAggregate);
			
//            string formattedValue = String.Empty;
//            formattedValue = QueryHelper.GetValue(col, value, casesensitive);	
//            strbuff.SetTokenValue("value", formattedValue);
           
//        }

//        private void SetColumn(object value1, object value2)
//        {

//            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(main);
//            MapMember col = info.MappedMembers.Find(this.membername);

//            if (col == null)
//            {
//                string exMsg = Utility.GetResourceString("EXCEPTION_QUERY_PROP_NOT_FOUND", membername, main.FullName);
//                throw new QueryException(exMsg);
//            }
       
//            string columnName = String.Format("{0}.{1}", info.TableName, col.ColumnName);
//            string parsedAggregate = QueryHelper.ParseAggregate(aggregate, columnName, distinct, col.Formatter, casesensitive);
//            strbuff.SetTokenValue("col", parsedAggregate);
			
//            string formattedValue1 = QueryHelper.GetValue(col, value1, casesensitive);	
//            strbuff.SetTokenValue("value", formattedValue1);

//            string formattedValue2 = QueryHelper.GetValue(col, value2, casesensitive);	
//            strbuff.SetTokenValue("value2", formattedValue2);
         
//        }



//    }
//}
