using System;
using System.Diagnostics;
using System.Text;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.DataQuery.Parsers;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery
{
    public class SingleValueQuery : QueryBase
    {
        private object[] result;
        private readonly string membername;

        /// <summary>
        /// Gets the result value array.
        /// </summary>
        public object[] Result
        {
            get { return result; }
        }

        public SingleValueQuery(Type type, string member) : base(type)
		{
            Distinct = false;
            this.membername = member;
        }

        public override string GetSql()
        {
            SingleValueQueryParser parser = new SingleValueQueryParser(this, membername);
            string sql = parser.GetSql();
            Trace.WriteLine("sql" + sql);
            return sql;
        }

        ///<summary>
        /// Execute query.
        ///</summary>
        internal override void Execute(bool realTime, TimeController controller)
        {
            string sql = GetSql();
            string queryIdentifier = Query.GenerateQueryIdentifier(Types.MainType.Type, sql);
#if DEBUG
            Debug.Write("[Query] Executing sql: " + sql, Resources.TRACE_CATEGORY);
#endif

            LoadResult(queryIdentifier, sql, realTime, controller);
        }

        private void LoadResult(string queryIdentifier, string sql, bool realtime, TimeController timeController)
        {

            if ((!realtime) && QueryCache.Current.Exists(queryIdentifier))
            {
                result = (object[])QueryCache.Current.GetObject(queryIdentifier);
                if (result != null) return;
            }

            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(Types.MainType.Type);
            Connection cnn = info.GetConnection();
            cnn.Open();
            Command cmd = new Command(sql, cnn);

            try
            {
                DataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    result = new object[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!reader.IsDBNull(i))
                        {
                            result[i] = reader.GetValue(i);
                        }
                        else
                        {
                            result[i] = null;
                        }
                    }

                    QueryCache.Current.AddItem(queryIdentifier, result, Types.MainType.Type, timeController);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", sql, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
                throw new QueryException(e.Message, e); 
            }
            finally
            {
                cnn.Close();
            }
        }

    }
}
