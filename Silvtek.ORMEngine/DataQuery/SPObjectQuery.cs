using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Loads an object using a stored procedure. Stored procedure must return all columns that 
    /// belong to the object being loaded. 
    /// </summary>
    public class SPObjectQuery : Query
    {
        private readonly string storedprocedure;
        private readonly Hashtable table;
        private ResultSet resultset;
        private readonly ArrayList parameterIdentifiers;


        /// <summary>
        /// Initializes a new instance of the <see cref="SPQuery"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="storedprocedure">The storedprocedure.</param>
        public SPObjectQuery(Type type, string storedprocedure) : base(type)
        {
            this.storedprocedure = storedprocedure;   
            this.table = new Hashtable();
            parameterIdentifiers = new ArrayList();
        }

       
        /// <summary>
        /// Appends the parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AppendParameter(string name, object value)
        {
            parameterIdentifiers.Add(String.Format("{0}:{1}", name, value));
            IDbDataParameter parameter = new DataParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            if (!table.Contains(name))
            {
                table.Add(name, parameter);
            }
            else
            {
                table[name] = parameter;
            }
        }

        /// <summary>
        /// Appends the parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbtype">The dbtype.</param>
        public void AppendParameter(string name, object value, DbType dbtype)
        {
            parameterIdentifiers.Add(String.Format("{0}:{1}", name, value));
            IDbDataParameter parameter = new DataParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.DbType = dbtype;

            if (!table.Contains(name))
            {
                table.Add(name, parameter);
            }
            else
            {
                table[name] = parameter;
            }
        }

        /// <summary>
        /// Appends the parameter.
        /// </summary>
        /// <param name="idbparameter">The idbparameter.</param>
        public void AppendParameter(IDbDataParameter idbparameter)
        {

            if (!table.Contains(idbparameter.ParameterName))
            {
                table.Add(idbparameter.ParameterName, idbparameter);
            }
            else
            {
                table[idbparameter.ParameterName] = idbparameter;
            }

        }

        /// <summary>
        /// Appends the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void AppendParameters(IDbDataParameter[] parameters)
        {
            foreach(IDbDataParameter parameter in parameters)
            {
                if (!table.Contains(parameter.ParameterName))
                {
                    table.Add(parameter.ParameterName, parameter);
                }
                else
                {
                    table[parameter.ParameterName] = parameter;
                }
            }
        }
        
        /// <summary>
        /// Executes the specified realtime.
        /// </summary>
        /// <param name="realtime">if set to <c>true</c> [realtime].</param>
        /// <param name="controller">The controller.</param>
        internal override void Execute(bool realtime, TimeController controller)
        {
            if (Types.Count > 1)
            {
                throw new NotSupportedException("SPQuery does not support executing a stored procedure for loading multiple object types.");
            }

            string[] parameterskeys = (String[]) parameterIdentifiers.ToArray(typeof (String));
            string list = String.Join("|", parameterskeys);
            string pseudoSql = String.Format("{0}>{1}", storedprocedure, list);
            string queryIdentifier = GenerateQueryIdentifier(Types.MainType.Type, pseudoSql);

            if ((!realtime) && QueryCache.Current.Exists(queryIdentifier))
            {
                resultset = QueryCache.Current.GetResultSet(queryIdentifier);
                if (resultset != null)
                {
                    InternalIsResultSetFromCache = true;
                    return;
                }
            }

            // Creates the result set object.
            resultset = new ResultSet(null, null, queryIdentifier, storedprocedure, 1);
            Connection cnn = null;
            try
            {
                cnn = Types.MainType.GetConnection();
                cnn.Open();
                Command cmd = new Command(storedprocedure, cnn);
                cmd.CommandType = CommandType.StoredProcedure;

                // append parameters.
                foreach(IDbDataParameter parameter in table.Values)
                {
                    if (parameter == null) continue;
                    cmd.Parameters.Add(parameter);
                }


                DataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    // Create result row.
                    ResultRow currentRow = new ResultRow(1);

                    // Update the execution date time for making sure it won't time out. (just in case :)).
                    cnn.UpdateExecutionDateTime();

                    ObjectSchemaInfo info = Types.MainType;
                    MapMemberList mapMemberList = info.MappedMembers;

                    // Holds the actual object to be created.
                    object currentObj = null;
                    ORMObject ormObj = null;


                    for (int memberIndex = 0; memberIndex < mapMemberList.Count; memberIndex++)
                    {
                        MapMember mapMember = mapMemberList[memberIndex];
                        string columnName = mapMember.ColumnName;

                        if (mapMember.Properties.IsPrimaryKey && Convert.IsDBNull(reader[columnName]))
                        {
                            currentObj = null;
                            break;
                        }

                        if (currentObj == null)
                        {
                            cnn.UpdateExecutionDateTime();

                            currentObj = Activator.CreateInstance(info.Type, BindingFlags.NonPublic |
                                                                             BindingFlags.Public | BindingFlags.Instance,
                                                                  null, null, null);

                            if (currentObj is ORMObject || BindObjects)
                            {
                                ormObj = ORM.Instanciate(currentObj);
                            }


                            if (ormObj != null) ormObj.InternalFireBeforeLoaded();
                        }

                        if (mapMember.Properties.AcceptNulls && Convert.IsDBNull(reader[columnName]))
                        {
                            mapMember.SetDataSourceValue(currentObj, null);
                            continue;
                        }

                        if (mapMember.Properties.DataType != DataType.Binary)
                        {
                            mapMember.SetDataSourceValue(currentObj, reader[columnName]);
                            if (ormObj != null)
                            {
                                if (mapMember.Properties.IsRecordDigestEnabled)
                                {

                                    ormObj.SetRecordDigest(Convert.ToString(reader[columnName]));
                                }
                            }

                        }
                        else
                        {
                            
                            if (!Convert.IsDBNull(reader[columnName]))
                            {
                                DataStream dataStream = new DataStream(mapMember);
                                Byte[] buffer = new byte[ORMConfigurator.BufferSize];
                                int startIndex = 0;
                                long bytesRead;
                                do
                                {
                                    int columnIndex = reader.GetOrdinal(columnName);
                                    bytesRead = reader.GetBytes(columnIndex, startIndex, buffer, 0, buffer.Length);
                                    if (bytesRead == 0) break;
                                    dataStream.Write(buffer, 0, (int) bytesRead);
                                    dataStream.Flush();
                                    startIndex += buffer.Length;
                                } while (buffer.Length == bytesRead);

                                dataStream.Position = 0;
                                mapMember.SetDataSourceValue(currentObj, dataStream);
                            }
                            else
                            {
                                mapMember.SetDataSourceValue(currentObj, null);
                            }

                        }

                    }

                    if (ormObj != null)
                    {
                        if (BindObjects)
                        {
                            ormObj.BindByForce();
                            if (LockObjects) ORM.Lock(ormObj);
                        }
                        else
                        {
                            ORM.Lock(ormObj);
                        }

                        ormObj.InternalFireAfterLoaded();
                    }

                    currentRow.Add(currentObj);

                    int index = resultset.AddRow(currentRow);

                    cnn.UpdateExecutionDateTime();
                    FireRowLoaded(this, index, currentRow);
                    cnn.UpdateExecutionDateTime();

                }

                reader.Close();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", this.storedprocedure, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
                throw;
            }
            finally
            {
                if (cnn != null)cnn.Close();
            }

            QueryCache.Current.AddItem(resultset, Types.MainType.Type, controller);
        }

        public override string GetSql()
        {
            return this.storedprocedure;
        }



      
    }
}
