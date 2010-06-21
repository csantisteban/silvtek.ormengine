#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace  Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for FilterBase.
	/// </summary>
	public class CriteriaItemParser : ISqlGenerator
	{
	    private const string TEMPLATE = " {operator}({criteria})"  ;
		private LogicalOperator logicalOperator;
		private ISqlGenerator criteria;
		private Guid objectID;
		private int index;


		public bool IsQueryCaseSensitive
		{
			get {return criteria.IsQueryCaseSensitive;}
			set {criteria.IsQueryCaseSensitive = value;}
		}

		internal CriteriaItemParser(LogicalOperator logicalOperator, ISqlGenerator criteria)
		{
			this.objectID = Guid.NewGuid();
			this.logicalOperator = logicalOperator;
			this.criteria = criteria; 
		}



		internal void SetIndex(int index)
		{
			this.index = index;
		}


		public string GetSql()
		{
			 StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
			 string oper = String.Empty;
			 if (index > 0)
			 {
				 LogicalOperator tempOperator = (logicalOperator == LogicalOperator.None) ? LogicalOperator.And : logicalOperator;
				 oper = ParseOperator(tempOperator);
			 }

			 string criteriaSql = criteria.GetSql();
			 stringBuffer.SetTokenValue("operator", oper);
			 stringBuffer.SetTokenValue("criteria", criteriaSql);
			 return stringBuffer.ToString();
		}

		/// <summary>
		/// Parse and return the <c>SQL</c> string for the LogicalOperator.
		/// </summary>
		/// <param name="op">The <see cref="LogicalOperator" /> to be parsed</param>
		/// <returns>Returns the string representing the logical operator.</returns>
		private string ParseOperator(LogicalOperator op)
		{
			string optor = String.Empty;
			switch (op)
			{
				case LogicalOperator.None:
					optor = String.Empty;
					break;
				case LogicalOperator.And:
					optor = "and";
					break;
				case LogicalOperator.Or:
					optor = "or";
					break;
				default:
					string exMsg = Utility.GetResourceString("Query:Ex:NOT_SUPPORTED_LOGICAL_OPERATOR", op);
					throw new NotImplementedException(exMsg);

			}

			return optor;
		}

	}
}
