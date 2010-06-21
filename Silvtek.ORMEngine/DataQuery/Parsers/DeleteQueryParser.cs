using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
    internal sealed class DeleteQueryParser : ISqlGenerator	
    {
        // SQL STATEMENT TEMPLATE
        internal const string SQL_TEMPLATE = "delete from {table} {where}";

        // WHERE CRITERIA TEMPLATE
        internal const string WHERE_TEMPLATE = "where {criteria}";

        private readonly DeleteQuery query;

        ///<summary>Gets whether the query is case sensitive.</summary>
        ///<remarks>Only valid on database that are case sensitive.</remarks>
        public bool IsQueryCaseSensitive
        {
            get { return query.IsQueryCaseSensitive; }
            set { query.IsQueryCaseSensitive = value; }
        }

        internal DeleteQueryParser(DeleteQuery query)
        {
            this.query = query;
        }

        ///<summary>Generates and returns the query sql command.</summary>
        public string GetSql()
        {
            StringBuffer sqlTemplate = new StringBuffer(SQL_TEMPLATE);


            // Sets columns
            if (query.Types == null) throw new QueryException("Query does not list types. Unable to query with no types");
            

            ObjectSchemaInfo mainTypeInfo = query.Types[0];
            sqlTemplate.SetTokenValue("table", mainTypeInfo.TableName);

            string whereClause = GetWhereClause();
            sqlTemplate.SetTokenValue("where", whereClause);

            return sqlTemplate.ToString();
        }

        private string GetWhereClause()
        {
            string result = String.Empty;

            if (query.Criterias != null && query.Criterias.Count > 0)
            {
                query.Criterias.IsQueryCaseSensitive = query.IsQueryCaseSensitive;
                StringBuffer whereTemplate = new StringBuffer(WHERE_TEMPLATE);
                whereTemplate.SetTokenValue("criteria", query.Criterias.GetSql());
                result = whereTemplate.ToString();
            }

            return result;
        }

    }
}
