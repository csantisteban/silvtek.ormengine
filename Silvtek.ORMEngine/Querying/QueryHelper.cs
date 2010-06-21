//using System;
//using System.Text;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;
//using System.Security.Cryptography;
//using System.Data;
//using Silvtek.ORMEngine.Schema;
//using Silvtek.ORMEngine.Data;
//using Silvtek.ORMEngine.Caching;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Summary description for QueryHelper.
//    /// </summary>
//    internal sealed class QueryHelper
//    {
//        private QueryHelper()
//        {
//        }

//        internal static string GetDataObjectColumn(Type type, string memberName, out MapMember columnprop, bool isCaseSensitive)
//        {
//            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//            bool colfound = false;
//            string column = String.Empty;
//            columnprop = null;

//            MapMember col = info.MappedMembers.Find(memberName);
           
//            if (col != null)
//            {
//                colfound = true;
//                columnprop = col;
//                string column_name = String.Format("{0}.{1}", info.TableName, col.ColumnName);

//                if ( (col.Properties.DataType != DataType.String) ||
//                    (col.Properties.DataType == DataType.String && isCaseSensitive) )
//                {
//                    column = column_name;
//                }
//                else
//                {
//                    column = info.DbTypeFormatter.LCase(column_name);
//                }
//            }
        

//            if (!colfound)
//            {
//                string exMsg = Utility.GetResourceString("EXCEPTION_QUERY_PROP_NOT_FOUND", memberName, type.FullName);
//                throw new QueryException(exMsg);
//            }

//            return column;

//        }


//        internal static string ParseAggregate(Aggregate aggregate, string columnName, bool distinct, DbTypeFormatter dbType, bool isCaseSensitive)
//        {
//            StringBuffer buffer = new StringBuffer("{cmd}({distinct} {col})");

//            if ( (!isCaseSensitive && dbType.IsCaseSensitive))
//            {
//                columnName = dbType.LCase(columnName);
//            }

//            if (distinct)
//            {
//                buffer.SetTokenValue("distinct", "distinct");
//            }
//            else
//            {
//                buffer.SetTokenValue("distinct", String.Empty);
//            }

//            buffer.SetTokenValue("col", columnName);
          
//            switch(aggregate)
//            {
//                case Aggregate.Avg:
//                    buffer.SetTokenValue("cmd", "avg");
//                    break;
//                case Aggregate.Count:
//                    buffer.SetTokenValue("cmd", "count");
//                    break;
//                case Aggregate.Max:
//                    buffer.SetTokenValue("cmd", "max");
//                    break;
//                case Aggregate.Min:
//                    buffer.SetTokenValue("cmd", "min");
//                    break;
//                case Aggregate.Sum:
//                    buffer.SetTokenValue("cmd", "sum");
//                    break;
//                case Aggregate.CountAll:
//                    buffer.SetTokenValue("cmd", "count");
//                    buffer.SetTokenValue("distinct", String.Empty);
//                    buffer.SetTokenValue("col", "*");
//                    break;
//                case Aggregate.None:
//                    buffer = new StringBuffer(String.Empty);
//                    break;
//                default:
//                    buffer = new StringBuffer(String.Empty);
//                    break;
//            }

//            return buffer.ToString();
//        }

//        /// <summary>
//        /// Returns <c>SQL</c> string LogicalOperator.
//        /// </summary>
//        /// <param name="comparison">The comparison operator to be parsed</param>
//        /// <returns></returns>
//        internal static string ParseCondition(Comparison comparison)
//        {
//            string str_condition = String.Empty;
//            switch(comparison)
//            {
//                case Comparison.Equal:
//                    str_condition = "=";
//                    break;
//                case Comparison.NotEqual:
//                    str_condition = "<>";
//                    break;
//                case Comparison.GreaterThan:
//                    str_condition = ">";
//                    break;
//                case Comparison.GreaterEqualThan:
//                    str_condition = ">=";
//                    break;
//                case Comparison.LessThan:
//                    str_condition  = "<";
//                    break;
//                case Comparison.LessEqualThan:
//                    str_condition = "<=";
//                    break;
//                case Comparison.Like:
//                    str_condition  = "like";
//                    break;
//                case Comparison.IsNull:
//                    str_condition =  "is null";
//                    break;
//                case Comparison.IsNotNull:
//                    str_condition  = "is not null";
//                    break;
//                case Comparison.In:
//                    str_condition = "in";
//                    break;
//                case Comparison.NotIn:
//                    str_condition = "not in";
//                    break;
//                case Comparison.Between:
//                    str_condition = "between";
//                    break;
//                case Comparison.NotBetween:
//                    str_condition = "not between";
//                    break;
//                default:
//                    str_condition  = String.Empty;
//                    break;
//            }

//            return  str_condition;

//        }

//        /// <summary>
//        /// Parse and return the <c>SQL</c> string for the LogicalOperator.
//        /// </summary>
//        /// <param name="op">The <see cref="LogicalOperator" /> to be parsed</param>
//        /// <returns>Returns the string representing the logical operator.</returns>
//        internal static string ParseOperator(LogicalOperator op)
//        {
//            string optor = String.Empty;
//            switch (op)
//            {
//                case LogicalOperator.None:
//                    optor = String.Empty;
//                    break;

//                case LogicalOperator.And:
//                    optor = "and";
//                    break;
//                case LogicalOperator.Or:
//                    optor = "or";
//                    break;

//                case LogicalOperator.Not:
//                    optor = "not";
//                    break;

//                default:
//                    string exMsg = Utility.GetResourceString("Query:Ex:NOT_SUPPORTED_LOGICAL_OPERATOR", op);
//                    throw new NotImplementedException(exMsg);

//            }

//            return optor;
//        }

//        internal static string GetColumnFullName(Type type, string memberName)
//        {
                   
//            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//            bool colfound = false;
//            string column_name = String.Empty;
            
//            MapMember col = info.MappedMembers.Find(memberName);
//            if (col != null)
//            {
//                colfound = true;
//                column_name = String.Format("{0}.{1}", info.TableName, col.ColumnName);
//            }
            
//            if (!colfound)
//            {
//                string exMsg = Utility.GetResourceString("EXCEPTION_QUERY_PROP_NOT_FOUND", memberName, type.FullName);
//                throw new QueryException(exMsg);
//            }

//            return column_name;
//        }


//        internal static string GetValue(MapMember column, object value, bool isCaseSensitive)
//        {
//            string returnValue = String.Empty;
			
//            if ( (column.Properties.DataType == DataType.String) && (!isCaseSensitive) && (column.Formatter.IsCaseSensitive))
//            {
//                if (value is String)value = ((string)value).ToLower();
//            }
            
//            if (value is Query)
//            {
//                returnValue = GetValue(value as Query);
//            }
//            else if (value is SqlText)
//            {
//                SqlText sqlText = value as SqlText;
//                if (sqlText == null)return String.Empty;
//                returnValue = sqlText.Text;
//            }
//            else
//            {
//                returnValue = MapUtility.FormatValueAsString(column, value, false);
//            }

//            return returnValue;
//        }

//        private static string GetValue(Query query)
//        {
//            string returnValue = String.Empty;
			
//            if (query.IsAggregate)
//            {
//                returnValue = String.Format(" ({0}) ", query.GetSql());	
//            }
//            else
//            {
//                if (query.SingleMember.Count == 0 || query.SingleMember.Count > 1)
//                {
//                    // fix error.
//                    throw new QueryException("SubQueries must be an aggregate query or contain only single members");
//                }

//                query.LoadObjects = false;
				
//                returnValue = String.Format(" ({0}) ", query.GetSql());
//            }

//            return returnValue;
//        }



		



//    }
//}
