#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{

	/// <summary>
	/// Create SQL command to query database.
	/// </summary>
	internal class QueryParser : ISqlGenerator	
	{
		// SQL STATEMENT TEMPLATE
		internal const string SQL_TEMPLATE =  "select {distinct} {columns} {aggregateColumns} from {table} {joins} {where} {groupby} {having} {orderby} ";

		// WHERE CRITERIA TEMPLATE
        internal const string WHERE_TEMPLATE = "where {criteria}";

		// HAVING CRITERIA TEMPLATE
        internal const string HAVING_TEMPLATE = "having {criteria}";
		
		#region -- Member variables --

        //private Joins joins;
        //private Criterias criterias;
        //private Criterias havings;
        //private QueriedTypes types;
        //private QueriedAggregates aggregates;
        //private OrderColumns orderColumns;
        //private bool isCaseSensitive;
        //private bool isDistinct;
        private readonly QueryBase query;

		#endregion

        internal QueryBase Query
        {
            get { return query; }
        }

		///<summary>Gets whether the query is case sensitive.</summary>
        ///<remarks>Only valid on database that are case sensitive.</remarks>
		public bool IsQueryCaseSensitive
		{
			get {return query.IsQueryCaseSensitive;}
            set { query.IsQueryCaseSensitive = value; }
		}
		
        /// <summary>
        /// Instanciates a new query parser
        /// </summary>
        /// <param name="query">The query object from which the parser will create the sql commands.</param>
		protected internal QueryParser(QueryBase query)
		{
            this.query = query;
            //this.aggregates = query.QueriedAggregates;
            //this.joins = query.Joins;
            //this.criterias = query.Criterias;
            //this.havings = query.Aggregates;
            //this.types = query.Types;
            //this.orderColumns = query.OrderProperties;
            //this.isCaseSensitive = query.IsQueryCaseSensitive;
            //this.isDistinct = query.Distinct;
		}
		
		
		///<summary>Generates and returns the query sql command.</summary>
        public virtual string GetSql()
		{
			StringBuffer sqlTemplate = new StringBuffer(SQL_TEMPLATE);
			string distinctKeyword = (query.Distinct) ? "distinct" : String.Empty;
			
			// Sets the distinct sql keyword.
			sqlTemplate.SetTokenValue("distinct", distinctKeyword);
			
			// Sets columns
			if (query.Types == null)throw new QueryException("Query does not list types. Unable to query with no types");
			string columns = query.Types.GetSql();
			sqlTemplate.SetTokenValue("columns", columns );

            if (query.QueriedAggregates != null && query.QueriedAggregates.Count > 0)
			{
                String aggregatesList = String.Format(", {0}", query.QueriedAggregates.GetSql());
                sqlTemplate.SetTokenValue("aggregateColumns", aggregatesList);
				sqlTemplate.SetTokenValue("groupby", columns);				
			}
			else
			{
				sqlTemplate.SetTokenValue("aggregateColumns", String.Empty);
				sqlTemplate.SetTokenValue("groupby", String.Empty);
			}

            ObjectSchemaInfo mainTypeInfo = query.Types[0];
			sqlTemplate.SetTokenValue("table", mainTypeInfo.TableName);
			
			string joinClause = (query.Joins != null) ? query.Joins.GetSql() : String.Empty;
            sqlTemplate.SetTokenValue("joins", joinClause);

			string whereClause = GetWhereClause();
			sqlTemplate.SetTokenValue("where", whereClause);

            string havingClause = GetHavingClause();
			sqlTemplate.SetTokenValue("having", havingClause);

            // Create the order clause
            string orderClause = (query.OrderProperties != null) ? query.OrderProperties.GetSql() : String.Empty;
			sqlTemplate.SetTokenValue("orderby", orderClause);
			return sqlTemplate.ToString();
		}

		protected string GetWhereClause()
		{
            string result = String.Empty;

            if (query.Criterias != null && query.Criterias.Count > 0)
            {
                query.Criterias.IsQueryCaseSensitive = query.IsQueryCaseSensitive;
                StringBuffer whereTemplate = new StringBuffer(WHERE_TEMPLATE);
                whereTemplate.SetTokenValue("criteria", query.Criterias.GetSql());
                result =  whereTemplate.ToString();
            }

            return result;
		}

        protected string GetHavingClause()
		{
            string result = String.Empty;

            if (query.Aggregates != null && query.Aggregates.Count > 0)
            {
                query.Aggregates.IsQueryCaseSensitive = IsQueryCaseSensitive;
                StringBuffer whereTemplate = new StringBuffer(HAVING_TEMPLATE);
                whereTemplate.SetTokenValue("criteria", query.Aggregates.GetSql());
                result = whereTemplate.ToString();
            }

            return result;
		}
		
	}
}
