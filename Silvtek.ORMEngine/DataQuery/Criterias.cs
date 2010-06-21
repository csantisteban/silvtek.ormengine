#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Silvtek.ORMEngine.DataQuery.Parsers;

namespace  Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Represents a collection of criterias
	/// </summary>
	public class Criterias : CollectionBase, ISqlGenerator
	{

		private bool isAggregateCollection;
		private bool isCaseSensitive;

        /// <summary>
        /// Gets or sets whether the query should be case sensitive;
        /// </summary>
 		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

		internal bool IsAggregateCriteria
		{
			get {return isAggregateCollection;}
		}


		internal Criterias(bool isAggregateCollection)
		{
			this.isAggregateCollection = isAggregateCollection;
		}

		private void Add(LogicalOperator logical, ISqlGenerator criteria)
		{
			 
			if (this.Count > 1 && logical == LogicalOperator.None)
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>LOGICAL_OPERATOR_NOT_SET");
				throw new InvalidOperationException(exmsg);
			}

            CriteriaItemParser item = new CriteriaItemParser(logical, criteria);
			int index = InnerList.Add(item);
			item.SetIndex(index);
		}

		public void Add(CriteriaItem criteria)
		{
			ValidateEntries(criteria);
		    Add(LogicalOperator.None, criteria);
		}

		public void Add(Criterias criteria)
		{
			ValidateEntries(criteria);
			Add(LogicalOperator.None, criteria);
		}

		public void And(CriteriaItem criteria)
		{
			ValidateEntries(criteria);
		    Add(LogicalOperator.And, criteria);
		}

		public void And(Criterias criteria)
		{
			ValidateEntries(criteria);
			Add(LogicalOperator.And, criteria);
		}

		public void Or(CriteriaItem criteria)
		{
			ValidateEntries(criteria);
			Add(LogicalOperator.Or, criteria);
		}

		public void Or(Criterias criteria)
		{
			ValidateEntries(criteria);
			Add(LogicalOperator.Or, criteria);
		}

		public string GetSql()
		{
			StringBuilder builder = new StringBuilder(this.Count * 100);
			foreach(ISqlGenerator sqlGenerator in InnerList)
			{
				sqlGenerator.IsQueryCaseSensitive = this.isCaseSensitive;
				builder.Append(sqlGenerator.GetSql());
			}

			return builder.ToString();
		}

		private void ValidateEntries(CriteriaItem criteria)
		{
			if (this.isAggregateCollection && (!criteria.IsAggregate))
			{
				string exMsg = Utility.GetResourceString("Querying:EXMSG>INVALID_AGGREGATE_CRITERIA");
			}
		}

		private void ValidateEntries(Criterias criteria)
		{
			if (this.isAggregateCollection && (!criteria.IsAggregateCriteria))
			{
				string exMsg = Utility.GetResourceString("Querying:EXMSG>INVALID_AGGREGATE_CRITERIA");
			}
		}


	}
}
