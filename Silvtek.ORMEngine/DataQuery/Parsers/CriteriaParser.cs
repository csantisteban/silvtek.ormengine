#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Collections;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	
	public abstract class CriteriaParser   : ISqlGenerator
	{
        private readonly ObjectSchemaInfo objectInfo;
        private readonly string memberName;
        private readonly MapMember column;
	    private bool isQueryCaseSensitive;
	    private Comparison comparison;
	    private Aggregate aggregate; // The aggregate function if any.
	    private bool distinct; // if true, adds the keyword distinct to the aggreagate function.
 
		#region -- Abstract members --

		internal abstract void SetComparison(Comparison comparison);
		public abstract string GetSql();

		#endregion

		protected Comparison Comparison
		{
			get {return comparison;}
		}

		/// <summary>
		/// Gets whether the criteria is case sensitive.
		/// </summary>
		public bool IsQueryCaseSensitive
		{
			get {return isQueryCaseSensitive;}
			set {isQueryCaseSensitive = value;}
		}

		internal static CriteriaParser Create(Type type, string memberName, Comparison comparison, object value)
		{
			ObjectSchemaInfo info = ORM.GetObjectInfo(type);
            CriteriaParser criteria;
            if (value is QueryBase)
            {
                criteria = new SubQueryCriteria(info, memberName, (QueryBase)value);
                criteria.SetComparison(comparison);
            }
            else if (value is SqlText)
            {
                criteria = new SqlTextCriteria(info, memberName, (SqlText)value);
                criteria.SetComparison(comparison);
            }
            else
            {
                Debug.WriteLine("IS OBJECT");
                criteria = new SingleValueCriteria(info, memberName, comparison, value);
            }
              return criteria;
		}

		internal static CriteriaParser Create(Type type, string memberName, Comparison comparison, object value1, object value2)
		{
			ObjectSchemaInfo info = ORM.GetObjectInfo(type);
			CriteriaParser criteria = new TwoValuesCriteria(info, memberName, value1, value2);
            criteria.comparison = comparison;
            return criteria;
		}

		internal static CriteriaParser Create(Type type, string memberName, Comparison comparison, ICollection collection)
		{
			ObjectSchemaInfo info = ORM.GetObjectInfo(type);
			CriteriaParser criteria = new MultipleValuesCriteria(info, memberName, collection);
		    criteria.comparison = comparison;
			return criteria;
		}

        //internal static CriteriaParser Create(Type type, string memberName, Comparison comparison, Query query)
        //{
        //    ObjectSchemaInfo info = ORM.GetObjectInfo(type);
        //    CriteriaParser criteria = new SubQueryCriteria(info, memberName, query);
        //    return criteria;
        //}

        //internal static CriteriaParser Create(Type type, string memberName, Comparison comparison, SqlText text)
        //{
        //    ObjectSchemaInfo info = ORM.GetObjectInfo(type);
        //    CriteriaParser criteria = new SqlTextCriteria(info, memberName, text);
        //    return criteria;
        //}

		/// <summary>
		/// Gets the <see cref="MappedMember" /> instance to which this condition is bound it.
		/// </summary>
		internal MapMember MappedMember
		{
			get {return column;}
		}

		internal CriteriaParser(ObjectSchemaInfo objectInfo, string memberName)
		{
			this.objectInfo = objectInfo;
			this.memberName = memberName;
			isQueryCaseSensitive = false;
			aggregate = Aggregate.None;
			distinct = false;
			column = objectInfo.MappedMembers.Find(memberName);


            if (column == null)
            {
                string exMsg = Utility.GetResourceString("QUERYING:EXMSG>MEMBER_NOT_FOUND", memberName,
                                                         objectInfo.Type.FullName);
                throw new QueryException(exMsg);

            }
		}

		internal void SetAggregate(Aggregate aggregate, bool addDistinctKeyword)
		{
			this.aggregate = aggregate;
			distinct = addDistinctKeyword;
		}

		

		internal void InternalSetComparison(Comparison comparison)
		{
			this.comparison = comparison;
		}


		/// <summary>
		/// Returns the string that represents the comparison.
		/// </summary>
		protected string GetComparisonString()
		{
			string retval = String.Empty;
			switch(comparison)
			{
				case Comparison.Equal:
					retval = "=";
					break;
				case Comparison.NotEqual:
					retval = "<>";
					break;
				case Comparison.GreaterThan:
					retval = ">";
					break;
				case Comparison.GreaterEqualThan:
					retval = ">=";
					break;
				case Comparison.LessThan:
					retval  = "<";
					break;
				case Comparison.LessEqualThan:
					retval = "<=";
					break;
				case Comparison.Like:
					retval  = "like";
					break;
				case Comparison.IsNull:
					retval =  "is null";
					break;
				case Comparison.IsNotNull:
					retval  = "is not null";
					break;
				case Comparison.In:
					retval = "in";
					break;
				case Comparison.NotIn:
					retval = "not in";
					break;
				case Comparison.Between:
					retval = "between";
					break;
				case Comparison.NotBetween:
					retval = "not between";
					break;
				default:
					string exmsg = Utility.GetResourceString("QUERYING:EXMSG>NOT_SUPPORTED_COMPARISON", comparison);
					throw new QueryException(exmsg);
			}

			return  retval;

		}

		private string ParseAggregate(Aggregate aggregate, bool distinct, string columnName)
		{
			StringBuffer buffer = new StringBuffer("{cmd}({distinct}{col})");

			string distinctKeyword = (distinct) ? "distinct " : String.Empty;
			buffer.SetTokenValue("distinct", distinctKeyword);
			buffer.SetTokenValue("col", columnName);
          
			switch(aggregate)
			{
				case Aggregate.Avg:
					buffer.SetTokenValue("cmd", "avg");
					break;
				case Aggregate.Count:
					buffer.SetTokenValue("cmd", "count");
					break;
				case Aggregate.Max:
					buffer.SetTokenValue("cmd", "max");
					break;
				case Aggregate.Min:
					buffer.SetTokenValue("cmd", "min");
					break;
				case Aggregate.Sum:
					buffer.SetTokenValue("cmd", "sum");
					break;
				case Aggregate.CountAll:
					buffer.SetTokenValue("cmd", "count");
					buffer.SetTokenValue("distinct", String.Empty);
					buffer.SetTokenValue("col", "*");
					break;
				case Aggregate.None:
					buffer = new StringBuffer(String.Empty);
					break;
				default:
					buffer = new StringBuffer(String.Empty);
					break;
			}

			return buffer.ToString();
		}

		protected string GetValue(object value)
		{
			string returnValue;

            if ((column.Formatter.IsCaseSensitive) && (column.Properties.DataType == DataType.String) && (!isQueryCaseSensitive))
			{
				if (value is String)value = ((string)value).ToLower();
			}

            if (column.Properties.DataType == DataType.Binary || column.Properties.DataType == DataType.Object)
            {
                throw new NotSupportedException("Query database with a binary or object data type is not supported at this moment.");
            }

			returnValue = MapUtility.FormatValueAsString(column, value, false);
			
			return returnValue;
		}


		protected string GetColumnFullName()
		{
			return GetColumnFullName(objectInfo, memberName);
		}

		protected  string GetColumnFullName(ObjectSchemaInfo info, string memberName)
		{
			MapMember column = info.MappedMembers.Find(memberName);

			string columnName = String.Format("{0}.{1}", objectInfo.TableName, column.ColumnName);
			
				
			if ( (objectInfo.DbTypeFormatter.IsCaseSensitive) && (column.Properties.DataType == DataType.String) && (!isQueryCaseSensitive) )
			{
                columnName = objectInfo.DbTypeFormatter.LCase(columnName);
			}
			
			if (aggregate != Aggregate.None)
			{
				columnName = ParseAggregate(aggregate, distinct, columnName);
			}

			return columnName;

		}

//		protected string GetValue(object value)
//		{
//			string returnValue = String.Empty;
//			
//			if ( (column.Formatter.IsCaseSensitive) && (column.Properties.DataType == DataType.String) && (!isCriteriaCaseSensitive))
//			{
//				if (value is String)value = ((string)value).ToLower();
//			}
//            
//			if (value is Query)
//			{
//				returnValue = GetValue(value as Query);
//			}
//			else
//			{
//				returnValue = MappedMember.FormatValueAsString(column, value);
//			}
//
//			return returnValue;
//		}
//
//		private string GetValue(Query query)
//		{
//			string returnValue = String.Empty;
//			
//			if (query.IsAggregate)
//			{
//				returnValue = String.Format(" ({0}) ", query.GetSql());	
//			}
//			else
//			{
//				if (query.SingleMember.Count == 0 || query.SingleMember.Count > 1)
//				{
//					// fix error.
//					throw new QueryException("SubQueries must be an aggregate query or contain only single members");
//				}
//
//				query.LoadObjects = false;
//				
//				returnValue = String.Format(" ({0}) ", query.GetSql());
//			}
//
//			return returnValue;
//		}

	}
}
