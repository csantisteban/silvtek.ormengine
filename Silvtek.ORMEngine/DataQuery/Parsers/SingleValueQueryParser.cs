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
    internal class SingleValueQueryParser : QueryParser	
	{
	    private readonly string membername;

	    /// <summary>
        /// Instanciates a new query parser
        /// </summary>
        /// <param name="query">The query object from which the parser will create the sql commands.</param>
        internal SingleValueQueryParser(QueryBase query, string membername) : base(query)
		{
            this.membername = membername;
		}

        ///<summary>Generates and returns the query sql command.</summary>
        public override string GetSql()
        {
            StringBuffer sqlTemplate = new StringBuffer(SQL_TEMPLATE);
            string distinctKeyword = (Query.Distinct) ? "distinct" : String.Empty;

            // Sets the distinct sql keyword.
            sqlTemplate.SetTokenValue("distinct", distinctKeyword);

            // Sets columns
            if (Query.Types == null) throw new QueryException("Query does not list types. Unable to query with no types");

            MapMember mapMember = Query.Types[0].MappedMembers.Find(membername);

            string columnName = String.Format("{0}.{1}", Query.Types[0].TableName, mapMember.ColumnName);
            sqlTemplate.SetTokenValue("columns", columnName);

            sqlTemplate.SetTokenValue("aggregateColumns", String.Empty);
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
