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
    internal class AggregateQueryParser : QueryParser
    {

        internal protected AggregateQueryParser(QueryBase query) : base(query)
        {
            if (query.QueriedAggregates.Count == 0)
            {
                throw new QueryException("Invalid Aggregate Query. An Aggregate query must have atleast one aggregate function");
            }
        }

        ///<summary>Generates and returns the query sql command.</summary>
        public override string GetSql()
        {
            StringBuffer sqlTemplate = new StringBuffer(QueryParser.SQL_TEMPLATE);
            string distinctKeyword = (Query.Distinct) ? "distinct" : String.Empty;

            // Sets the distinct sql keyword.
            sqlTemplate.SetTokenValue("distinct", distinctKeyword);

            // Sets columns
            if (Query.Types == null) throw new QueryException("Query does not list types. Unable to query with no types");

            sqlTemplate.SetTokenValue("columns", String.Empty);

            sqlTemplate.SetTokenValue("aggregateColumns", Query.QueriedAggregates.GetSql());
            sqlTemplate.SetTokenValue("groupby", String.Empty);

            ObjectSchemaInfo mainTypeInfo = Query.Types[0];
            sqlTemplate.SetTokenValue("table", mainTypeInfo.TableName);

            string joinClause = (Query.Joins != null) ? Query.Joins.GetSql() : String.Empty;
            sqlTemplate.SetTokenValue("joins", joinClause);

            string whereClause = GetWhereClause();
            sqlTemplate.SetTokenValue("where", whereClause);

            string havingClause = GetHavingClause();
            sqlTemplate.SetTokenValue("having", havingClause);

            string orderClause = (Query.OrderProperties != null) ? Query.OrderProperties.GetSql() : String.Empty;
            sqlTemplate.SetTokenValue("orderby", orderClause);
            return sqlTemplate.ToString();
        }


    }
}
