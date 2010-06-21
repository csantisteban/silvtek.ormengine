//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//using System;
//using System.Collections;
//using System.Text;
//using System.Diagnostics;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    ///  Generates the criteria SQL string.
//    /// </summary>
//    ///<remarks>
//    /// The instance of this class generates all the criterial sql commands
//    /// for the <see cref="Query" />.
//    ///</remarks>
//    internal class Criteria : ICriteria
//    {
		 
//        #region -- Sql Generator Constant Variables ---
		
//        private const string OPERATOR_TOKEN = "op";
//        private const string COLUMN_TOKEN = "col";
//        private const string CONDITION_TOKEN = "condition";
//        private const string VALUE_TOKEN = "value";
//        private const string VALUE2_TOKEN = "value2";
       
//        // Holds the default criteria template.
//        private const string TEMPLATE = " {op} ({col} {condition} {value}) ";

//        // Holds the criteria template when using the between keyword.
//        private const string TEMPLATE_2 = " {op} ({col} {condition} {value} and {value2}) ";

//        // Holds the criteria template when using the in keyword.
//        private const string TEMPLATE_3 = " {op} ({col} {condition} ({value})) ";

//        #endregion
		
//        private bool casesensitive;			// Holds whether the query is case sensitive.
//        private StringBuffer strbuff;		// Holds the string buffert that holds the current command template.
//        private LogicalOperator op;			// Holds the prefix sql LogicalOperator.
//        private object value;				// Holds the value to be compare.
//        private object value2;				// Holds the value to be compare. Null when comparition is not "BETWEEN" or "NOT BETWEEN"
//       // private string instance_signature;	// Holds the unique string signature for hashing.
//        private Guid objectID;
//        private Type maintype;				// Holds the query's type.
//        private MapMember columnproperty;	// Holds the column info.
//        private string propertyname;		// Holds the property or member name.
//        private Comparison comparison;		// Holds the comparison.

//        ///<summary>
//        /// Gets whether the criteria is case sensitive.
//        ///</summary>
//        ///<remarks>
//        /// Case sensitive will work only with database is query are case
//        /// sensitive like Oracle.
//        ///</remarks>
//        internal bool CaseSensitive
//        {
//           get {return casesensitive;}
//        }


//        ///<summary>
//        /// Gets the <see cref="SQLOperator" /> that current criteria
//        /// uses to bound to the previous criteria.
//        ///</summary>
//        ///<value>
//        /// The <see cref="SQLOperator" />.
//        ///</values>
//        public LogicalOperator LogicalOperator
//        {
//            get {return op;}
//            set {op = value;}
//        }

//        ///<summary>
//        /// Gets the <see cref="Comparison" /> for this instance.
//        ///</summary>
//        public Comparison Comparison
//        {
//          get {return comparison;}
//        }


//        public object Value
//        {
//          get {return value;}
//        }



//        public object Value2
//        {
//          get {return value2;}
//        }


//        #region -- Constructors --

//        internal Criteria(LogicalOperator op, Type type, string memberName, Comparison comparison, Type type2,
//                          string memberName2, bool isCaseSensitive)
//        {
//           _Init(TEMPLATE, op, type, memberName, comparison, isCaseSensitive);

//           SetColumn(type2, memberName2);
//        }

//        ///<summary>
//        /// Construct a criteria the has multiple values to test.
//        ///</summary>
//        ///<param name="op">
//        /// The prefix LogicalOperator. If current criteria is the first
//        /// use <see cref="SQLOperator.None" />.
//        ///</param>
//        ///<param name="obj">
//        /// The <see cref="ORMObject"> that current criteria
//        /// uses to identify the <paramref name="propname" />.
//        ///</param>
//        ///<param name="values">
//        /// A collection of values.
//        ///<param>
//        ///<param name="condition">
//        /// Accepts only <see cref="SQLCondition.In" /> or
//        /// <see cref="SQLCondition.NotIn" />. Any other condition
//        /// on this constructor will throw an <see cref="QueryException" />.
//        ///</param>
//        ///<param name="isCaseSensitive">
//        /// Whether current criteria is case-sensitive.
//        ///</param>
//        internal Criteria(LogicalOperator op, Type type, string propname, Comparison comparison, ICollection values, bool isCaseSensitive)
//        {
//            // check condition.
//            if (comparison != Comparison.In && comparison != Comparison.NotIn)
//            {
//                string exMsg =  Utility.GetResourceString("QUERYEXCEPTION:NOT_SUPPORTED_WITH_COLLECTIONS", comparison);
//                throw new QueryException(exMsg);
//            }

//            _Init(TEMPLATE_3, op, type, propname, comparison, isCaseSensitive);
//            SetColumn(values);


//        }


//        internal Criteria(LogicalOperator op, Type type, string propname, object value, Comparison comparison, bool isCaseSensitive)
//        {
//           _Init(TEMPLATE, op, type, propname, comparison, isCaseSensitive);

//           if (comparison == Comparison.IsNull || comparison == Comparison.IsNotNull)
//           {
//              SetColumn(null);
//           }
//           else
//           {
//               SetColumn(value);
//           }


//        }


//        internal Criteria(LogicalOperator op, Type type, string propname, Comparison comparison, object value1, object value2, bool isCaseSensitive)
//        {

//            // check condition.
//            if (comparison != Comparison.Between && comparison != Comparison.NotBetween)
//            {
//                string exMsg = Utility.GetResourceString( "QUERYEXCEPTION:NOT_SUPPORTED_WITH_BETWEEN", comparison);
//                throw new QueryException(exMsg);
//            }

//            _Init(TEMPLATE_2, op, type, propname, comparison, isCaseSensitive);
//            SetColumn(value1, value2);

//        }

//        #endregion

//        #region -- Init Method --

//        // Private method that initialize this class.
//        // this method should be invoked from the constructor only!.
//        private void _Init(string template, LogicalOperator op, Type type, string propname, Comparison comparison, bool isCaseSensitive)
//        {
//            this.objectID = Guid.NewGuid();
//            strbuff = new StringBuffer(template);
//            this.casesensitive = isCaseSensitive;
//            this.maintype = type;
//            this.propertyname = propname;
//            this.value = null;
//            this.value2 = null;
//            this.LogicalOperator = op;
//            SetCondition(comparison);
			
//            strbuff.SetTokenValue(Criteria.COLUMN_TOKEN, QueryHelper.GetDataObjectColumn(maintype, propname, out columnproperty, isCaseSensitive));
//        }

//        #endregion

//        ///<summary>
//        /// Set the value to be search or compared
//        ///</summary>
//        public void SetValue(object value)
//        {
//           this.value = value;
//           SetColumn(value);
//        }

//        ///<summary>
//        /// Sets the values to be compared between.
//        ///</summar>
//        public void SetValue(object value, object value2)
//        {
//           this.value = value;
//           this.value2 = value;
//           SetColumn(value, value2);
//        }

//        ///<summary>
//        /// Sets the values to be compared or searched.
//        ///</summary>
//        public void SetValue(ICollection col)
//        {
//           this.value = col;
//           SetColumn(value);
//        }

//        public void SetOperator(LogicalOperator op)
//        {
//          this.op = op;
//          strbuff.SetTokenValue(Criteria.OPERATOR_TOKEN, QueryHelper.ParseOperator(op));
//        }

//        public void SetCondition(Comparison  comparison)
//        {
//           this.comparison = comparison;
//           strbuff.SetTokenValue(Criteria.CONDITION_TOKEN, QueryHelper.ParseCondition(comparison));
//        }


//        public override int GetHashCode()
//        {
//          return objectID.GetHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//           if (obj.GetType() != GetType())return false;
//           Criteria tmp = (Criteria)obj;
//           return (tmp.objectID == this.objectID);
//        }


//        public override string ToString()
//        {
//            strbuff.SetTokenValue(Criteria.OPERATOR_TOKEN, QueryHelper.ParseOperator(op));

//           return strbuff.ToString();
//        }


//        private void SetColumn(object value, object value2)
//        {
//           string formattedValue = QueryHelper.GetValue(columnproperty, value, casesensitive);
//           strbuff.SetTokenValue(Criteria.VALUE_TOKEN, formattedValue);

//           if (value2 != null)
//           {
//               string formattedValue2 = QueryHelper.GetValue(columnproperty, value2, casesensitive);
//               strbuff.SetTokenValue(Criteria.VALUE2_TOKEN, formattedValue2);
//           }
//       }

//       private void SetColumn(object value)
//       {
//           SetColumn(value, null);
//       }

//       private void SetColumn(ICollection values)
//       {
//         //ObjectSchemaInfo info = ORMInfoPool.GetORMInfo(maintype);
//         string delimited_value = GetCommaDelimitedValues(columnproperty, values);

//         strbuff.SetTokenValue(Criteria.VALUE_TOKEN, delimited_value);

//       }

//       private void SetColumn(Type type, string propname)
//       {
//         MapMember colprop;
//         string column = QueryHelper.GetDataObjectColumn(type, propname, out colprop, casesensitive);

//         Trace.WriteLineIf((colprop.Properties.DataType != columnproperty.Properties.DataType),
//                            Utility.GetResourceString("TRACE:SQLCRITERIA:NO_SAME_TYPE", propname, type, propertyname, maintype) );

//           strbuff.SetTokenValue(Criteria.VALUE_TOKEN, column);
//       }


//        /* convert an ICollection instance in a string of comma delimited
//           values */
//        private string GetCommaDelimitedValues(MapMember column, ICollection values)
//        {
//           //return empty string if is null or does not contain values.
//           if (values == null || values.Count == 0)return String.Empty;

//           int aprox_chars = values.Count * 20;
//           StringBuffer values_template = new StringBuffer("{comma} {value}");

//           values_template.SetTokenValue("comma", String.Empty);
//           values_template.SetTokenValue("value", String.Empty);
//           StringBuilder delimited_values = new StringBuilder(aprox_chars);

//           int temp_count = 0;

//           foreach(object obj in values)
//           {
//               string formattedValue = QueryHelper.GetValue(column, obj, casesensitive);
//               values_template.SetTokenValue(Criteria.VALUE_TOKEN, formattedValue);
//               delimited_values.Append(values_template.ToString());
//               if (temp_count == 0)values_template.SetTokenValue("comma", ",");
//           }

//           return delimited_values.ToString();

//        }


//    }
//}
