#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Reflection;
using System.Diagnostics;
using Silvtek.ORMEngine.Schema;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.DataQuery.Parsers;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Represents an Aggregate query
    /// </summary>
    public class AggregateQuery : QueryBase
    {

        private object[] result;
       
        /// <summary>
        /// Gets the result value array.
        /// </summary>
        public object[] Result
        {
            get { return result; }
        }

        public AggregateQuery(Aggregate aggregate, Type type, string member, bool isDistinct) : base(type)
        {
            this.Distinct = false;
            QueriedAggregates.Add(aggregate, type, member, isDistinct);
        }

        public void AddAggregate(Aggregate aggregate, Type type, string member, bool isDistinct)
        {
            QueriedAggregates.Add(aggregate, type, member, isDistinct);
        }

        /// <summary>
        /// Returns the result as <cref lang="Int32" /> 
        /// </summary>
        /// <returns></returns>
        //public int GetInt32()
        //{
        //    if (result == null) return 0;
        //    return Convert.ToInt32(result);
        //}

        //public string GetString()
        //{
        //    if (result == null) return String.Empty;
        //    return Convert.ToString(result);
        //}

        //public DateTime GetDateTime()
        //{
        //    if (result == null) return DateTime.MinValue;
        //    return Convert.ToDateTime(result);
        //}

        public override string GetSql()
        {
            AggregateQueryParser parser = new AggregateQueryParser(this); 
            string sql = parser.GetSql();
            //Trace.WriteLine("Aggregate SQL:" + sql);
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

                    QueryCache.Current.AddItem(queryIdentifier, result, this.Types.MainType.Type, timeController);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", sql, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
                throw new QueryException(e.Message, e);;
            }
            finally
            {
                cnn.Close();
            }

            cmd = null;
            cnn = null;
        }
    }
}
