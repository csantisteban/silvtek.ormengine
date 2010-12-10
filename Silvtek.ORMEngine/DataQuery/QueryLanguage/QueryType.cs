using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.QueryLanguage
{
    /// <summary>
    /// Represents a type specified in the command text. 
    /// </summary>
    internal class QueryType 
    {
        private Type type;
        private string token;
        private string alias;
        private string name;
        private string table;
        private ObjectSchemaInfo schemaInfo;

        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>The alias.</value>
        internal string Alias
        {
            get { return alias; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        internal Type Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        internal string Token
        {
            get { return token; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        internal string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <value>The table.</value>
        internal string Table
        {
            get { return table; }
        }

        public ObjectSchemaInfo ObjectSchemaInfo
        {
            get { return schemaInfo; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryType"/> class.
        /// </summary>
        /// <param name="schemaInfo">The schema info.</param>
        /// <param name="token">The token.</param>
        internal QueryType(ObjectSchemaInfo schemaInfo, string token) : this(schemaInfo, token, String.Empty)
        {
            this.name = schemaInfo.Type.Name;
            this.table = schemaInfo.TableName;
            this.token = token;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryType"/> class.
        /// </summary>
        /// <param name="schemaInfo">The schema info.</param>
        /// <param name="token">The token.</param>
        /// <param name="alias">The alias.</param>
        internal QueryType(ObjectSchemaInfo schemaInfo, string token, string alias)
        {
            if (schemaInfo == null)
            {
                throw new ArgumentNullException("schemaInfo", "Failed to instanciate a QueryType object. The schema information is not an instance of an object");
            }

            if (token == null || token.Length == 0)
            {
                throw new ArgumentException("token is null or empty stirng", "token");
            }

            this.name = schemaInfo.Type.Name;
            this.table = schemaInfo.TableName;
            this.schemaInfo = schemaInfo;
            this.token = token;
            this.alias = alias;
        }

       
        //public bool ParseToken()
        //{
        //    this.token = token;
        //    this.alias = alias;

        //    this.name = QueryLanguageParserUtility.ExtractSingleTypeName(token);
        //    if (name.Length == 0)
        //    {
        //        throw new ORMException("Unable to get type name from token " + token);
        //    }

        //    // token for a type must be the type name or the full type name.
        //    type = context.GetType(name);

        //    if (type == null)
        //    {
        //        throw new ORMException("Unable to get type of type name "+name);
        //    }

        //    schemaInfo = ObjectSchemaContainer.Current.Retrieve(type);

        //    if (ObjectSchemaInfo != null)
        //    {
        //        table = ObjectSchemaInfo.TableName;
        //    }

        //    return true;

        //}

    }
}
