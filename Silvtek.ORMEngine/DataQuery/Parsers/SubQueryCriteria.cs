#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace  Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for ConditionSubQuery.
	/// </summary>
	internal class SubQueryCriteria : CriteriaParser
	{
		private const string TEMPLATE = "({col} {comparison} ({query}) ) ";
		private readonly QueryBase query;
		private bool isMultiple;

		public bool IsMultiple
		{
			get {return isMultiple;}
		}


		internal SubQueryCriteria(ObjectSchemaInfo objectInfo, string memberName,  QueryBase query) : base(objectInfo, memberName)
		{
			this.query = query;
		}

		internal override void SetComparison(Comparison comparison)
		{
			if (ComparisonHelper.IsNull(comparison) || ComparisonHelper.IsBetween(comparison) )
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>COMPARISON_NOT_FOR_SUBQUERIES", comparison);
				throw new QueryException(exmsg);
			}

		    isMultiple = ComparisonHelper.IsCollections(comparison);
		    
            InternalSetComparison(comparison);
		}

		public override string GetSql()
		{
			string columnName =	 GetColumnFullName();
			string comparisonValue = GetComparisonString();
			string sql = query.GetSql();

			StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
			stringBuffer.SetTokenValue("col", columnName);
			stringBuffer.SetTokenValue("comparison", comparisonValue);
			stringBuffer.SetTokenValue("query", sql);

			return stringBuffer.ToString();
		}
	}
}
