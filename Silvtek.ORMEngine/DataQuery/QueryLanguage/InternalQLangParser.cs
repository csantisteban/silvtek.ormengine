using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Silvtek.ORMEngine.DataQuery;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.QueryLanguage
{
    internal class InternalQLangParser
    {

        private const string _FIND_QUERY_TYPES = @"(?:(?i)(?:from|join)\s+)(\{{1}(?:[\w\d]*)\}{1})(?>(?i)[\s]*as{1}[\s]*){0,1}(?!(?i)left|right|join|where|and|full|outer|as|or|on|select|from|max|group|order|having)(\w+)";
        private const string _FIND_SUBQUERY_ALIAS = @"(?<!\{{1}[\w\d]*\}{1})(?>(?i)as\s){1}(?!(?i)left|right|join|where|and|full|outer|as|or|on|select|from|max|group|order|having)(\w*)\s+for{1}\s+(\{{1}[\w\d]*\}{1})";
        private const string _FIND_MEMBERS = @"\{{1}([\w\d]*):{1}([\w\d]*)\}{1}";
        private const string _FIND_ALIAS_MEMBERS_POINTER = @"([\w\d]*)=>\{{1}([\w\d]*):{1}([\w\d]*)\}{1}";
        private const string _FIND_SELECT_FROM_TYPES = @"^(?i)select\s+((?:\{{1}[\w\d\.]+\}{1},{0,1})*)\s+(?i)from";
        private const string _EXTRACT_TYPE_NAME = @"(?:\{{1}([\w\d]*)\}{1})(?>(?i)[\s]*as{1}[\s]*){0,1}";
        private const string _FIND_VARIABLES = @"@[\w\d]+";

        private StringBuilder afterfrom;
        private StringBuilder beforefrom;


        private ArrayList types;
        private Hashtable aliasList;
        private Hashtable typeTable;
        private ArrayList select;

        private readonly CommandText commandText;

        internal InternalQLangParser(CommandText commandText)
        {
            this.commandText = commandText;

        }

        internal ObjectSchemaInfo[] ObjectSchemaInfos
        {
            get
            {
                ArrayList list = new ArrayList(select.Count);
                foreach (QueryType queryType in select)
                {
                    list.Add(queryType.ObjectSchemaInfo);
                }

                return (ObjectSchemaInfo[])list.ToArray(typeof(ObjectSchemaInfo));
            }
        }


        internal string Parse()
        {
            string originalcommand = commandText.Text;

#if NET20
            int fromIndex = originalcommand.IndexOf("from", 0, StringComparison.OrdinalIgnoreCase);
#endif
#if NET11
            int fromIndex = originalcommand.ToLower().IndexOf("from", 0);  
#endif


            string tmpBeforeFrom = originalcommand.Substring(0, fromIndex - 1);
            string tmpAfterFrom = originalcommand.Substring(fromIndex);

            types = new ArrayList(4);
            select = new ArrayList(4);
            aliasList = new Hashtable(3);
            typeTable = new Hashtable(3);
            afterfrom = new StringBuilder(tmpAfterFrom);
            beforefrom = new StringBuilder(tmpBeforeFrom);

            ParseSubQueryWithAlias();
            ParseTypes();
            ParseMembers();
            ParseSelectObjects();
            SetParametersValue();

            return String.Format("{0} {1}", beforefrom, afterfrom);

        }

        private void ParseTypes()
        {
            Regex regex = new Regex(_FIND_QUERY_TYPES);
            MatchCollection matches = regex.Matches(commandText.Text);
            if (matches.Count == 0) return;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 0)
                {
                    continue;
                }
                string token = match.Groups[1].Value;

                string alias = String.Empty;
                if (match.Groups.Count > 1)
                {
                    alias = match.Groups[2].Value;
                }

                if (token.Length == 0)
                {
                    continue;
                }

                string typeName = QueryLanguageParserUtility.ExtractSingleTypeName(token);

                if (typeName.Length == 0)
                {
                    throw new ORMException("Unable to retrieve the type name for token " + token);
                }

                ObjectSchemaInfo info = commandText.FindSchemaInfo(typeName);

                if (info == null)
                {
                    throw new InvalidOperationException("Failed to parse query command." + typeName+ " is not register with the CommandText instance. Use CommandText.RegisterType(typeof(Sample)) to register types involved in the command");
                }

                QueryType queryType = new QueryType(info, token, alias);

                types.Add(queryType);
                typeTable[queryType.Name] = queryType;
                if (alias.Length > 0)
                {
                    aliasList[alias] = queryType;
                    Debug.WriteLine("Registering alias " + alias);

                }

                afterfrom.Replace(token, queryType.Table);


            }

        }

        private void ParseMembers()
        {
            Regex regex = new Regex(_FIND_MEMBERS);
            MatchCollection matches = regex.Matches(commandText.Text);
            if (matches.Count == 0) return;

            foreach (Match match in matches)
            {

                string token = match.Value;
                if (match.Groups.Count < 2)
                {
                    continue;
                }

                string type = match.Groups[1].Value;
                string member = match.Groups[2].Value;

                if (token.Length == 0)
                {
                    continue;
                }

                if (type.Length == 0)
                {
                    continue;
                }

                // find type.
                QueryType qtype = null;
                if (typeTable.Contains(type))
                {
                    qtype = (QueryType)typeTable[type];

                    if (qtype == null)
                    {
                        continue;
                    }

                    ObjectSchemaInfo schemainfo = qtype.ObjectSchemaInfo;
                    if (schemainfo == null)
                    {
                        throw new OrmMappingException("Failed to load orm schema for " + type);
                    }

                    MapMember mapMember = schemainfo.MappedMembers.Find(member);

                    if (mapMember == null)
                    {
                        throw new OrmMappingException("Failed find mapped member for " + type);
                    }

                    afterfrom.Replace(token, mapMember.ColumnFullName);
                }
                else if (aliasList.Contains(type))
                {
                    qtype = (QueryType)aliasList[type];

                    if (qtype == null)
                    {
                        continue;
                    }

                    ObjectSchemaInfo schemainfo = qtype.ObjectSchemaInfo;
                    if (schemainfo == null)
                    {
                        throw new OrmMappingException("Failed to load orm schema for " + type);
                    }

                    MapMember mapMember = schemainfo.MappedMembers.Find(member);

                    if (mapMember == null)
                    {
                        throw new OrmMappingException("Failed find mapped member for " + type);
                    }

                    string replaceText = String.Format("{0}.{1}", qtype.Alias, mapMember.ColumnName);

                    afterfrom.Replace(token, replaceText);
                }
            }
        }

        private void ParseSubQueryWithAlias()
        {
            Regex regex = new Regex(_FIND_SUBQUERY_ALIAS);
            MatchCollection matches = regex.Matches(commandText.Text);
            if (matches.Count == 0) return;

            foreach (Match match in matches)
            {
                string fullmatch = match.Value;
                if (match.Groups.Count < 2)
                {
                    continue;
                }

                string alias = match.Groups[1].Value;
                string token = match.Groups[2].Value;

                string typeName = QueryLanguageParserUtility.ExtractSingleTypeName(token);

                if (typeName.Length == 0)
                {
                    throw new ORMException("Unable to retrieve the type name for token " + token);
                }

                ObjectSchemaInfo info = commandText.FindSchemaInfo(typeName);

                if (info == null)
                {
                    throw new InvalidOperationException("Failed to parse query command. Unable to find information for " + typeName);
                }

                QueryType queryType = new QueryType(info, token, alias);


                types.Add(queryType);
                typeTable[queryType.Name] = queryType;
                aliasList[alias] = queryType;

                afterfrom.Replace(fullmatch, String.Format("as {0}", alias));

            }
        }

        private void ParseSelectObjects()
        {
            Regex regex = new Regex(_EXTRACT_TYPE_NAME);
            MatchCollection matches = regex.Matches(beforefrom.ToString());
            if (matches.Count == 0) return;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 0)
                {
                    continue;
                }

                string token = match.Value;
                Debug.WriteLine("Parse select value for token "+token);
                if (token.Length == 0)
                {
                    continue;
                }

                string name = String.Empty;
                if (match.Groups.Count > 0)
                {
                    name = match.Groups[1].Value;
                }

                Debug.WriteLine("(select) find type for name "+name);

                QueryType qtype = null;
                if (typeTable.Contains(name))
                {
                    qtype = (QueryType)typeTable[name];

                    if (qtype == null)
                    {
                        continue;
                    }

                    select.Add(qtype);
                    Debug.WriteLine("Found type for name "+name);
                }
                else if (aliasList.Contains(name))
                {
                    qtype = (QueryType)aliasList[name];

                    if (qtype == null)
                    {
                        continue;
                    }

                    Debug.WriteLine("Found type for alias " + name);
                    select.Add(qtype);
                }
            }

            string columns = GetColumnsList();
            beforefrom = new StringBuilder();
            beforefrom.AppendFormat("select {0} ", columns);

        }

        ///<summary>
        /// Returns a select statement.
        ///</summary>
        private string GetColumnsList()
        {

            StringBuilder builder = new StringBuilder(select.Count * 100);

            bool addComma = false;
            for (int i = 0; i < select.Count; i++)
            {
                QueryType qtype = (QueryType)select[i];
                if (qtype == null) continue;

                ObjectSchemaInfo info = qtype.ObjectSchemaInfo;
                if (info == null)
                {
                    throw new OrmMappingException("Failed to load orm schema for " + qtype.Token);
                }

                string tablename;
                if (qtype.Alias != null && qtype.Alias.Length > 0)
                {
                    tablename = qtype.Alias;
                }
                else
                {
                    tablename = info.TableName;
                }

                for (int j = 0; j < info.MappedMembers.Count; j++)
                {
                    string columnName = String.Format("{0}.{1}", tablename, info.MappedMembers[j].ColumnName);
                    if (addComma) builder.Append(",");
                    builder.Append(columnName);
                    addComma = true;
                }
            }

            return builder.ToString();
        }

        private void SetParametersValue()
        {
            CommandParameter[] parameters =  this.commandText.GetParameters();
            
            foreach (CommandParameter parameter in parameters)
            {
                this.beforefrom.Replace(parameter.Name, parameter.GetFormattedValue());
                this.afterfrom.Replace(parameter.Name, parameter.GetFormattedValue());
            }
        }
    }

}
