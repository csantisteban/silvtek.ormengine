using System;
using System.Collections;
using System.Data;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Execute an stored procedure against a database.
    /// </summary>
    public class SPQuery
    {
        private readonly string storedprocedure;
        private readonly Hashtable table;
        private readonly ArrayList parameterIdentifiers;
        private readonly string connectionName;


        /// <summary>
        /// Initializes a new instance of the <see cref="SPQuery"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="storedprocedure">The storedprocedure.</param>
        public SPQuery(string storedprocedure, string connectionName)
        {
            this.storedprocedure = storedprocedure;
            this.connectionName = connectionName;
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
            foreach (IDbDataParameter parameter in parameters)
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
        /// Executes stored procedure agains database and returns all records in a <see cref="DataTable"/> object.
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteDataTable()
        {
            /// string[] parameterskeys = (String[])parameterIdentifiers.ToArray(typeof(String));
            // string list = String.Join("|", parameterskeys);
            // string pseudoSql = String.Format("{0}>{1}", storedprocedure, list);
            // string queryIdentifier = QueryBase.GenerateQueryIdentifier(typeof(DataSet), pseudoSql);

            DataTable datatable = new DataTable();
            DataReader reader = null;
            try
            {
                reader = ExecuteReader();
                while(reader.Read())
                {
                    DataRow row = datatable.NewRow();
                    
                    for(int i=0; i <= reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        row[columnName] = reader.GetValue(i);
          
                    }

                    datatable.Rows.Add(row);
                }
                
                //datatable.Load(reader, LoadOption.OverwriteChanges);
            }
            finally
            {
                if (reader != null) reader.Close();
            }


            return datatable;
           
        }

        /// <summary>
        /// Executes stored procedure and returns a data reader.
        /// </summary>
        /// <returns></returns>
        public DataReader ExecuteReader()
        {

            Connection cnn = ConnectionManager.Current.GetConnection(connectionName);
            cnn.Open();
            Command cmd = new Command(storedprocedure, cnn);
            cmd.CommandType = CommandType.StoredProcedure;

            // append parameters.
            foreach (IDbDataParameter parameter in table.Values)
            {
                if (parameter == null) continue;
                cmd.Parameters.Add(parameter);
            }

            return cmd.ExecuteReader(CommandBehavior.CloseConnection);

        }

        /// <summary>
        /// Executes an stored procedure statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {

            Connection cnn = null;

            int recordsAffected = 0;
            try
            {
                cnn = ConnectionManager.Current.GetConnection(connectionName);
                cnn.Open();

                Command cmd = new Command(storedprocedure, cnn);
                cmd.CommandType = CommandType.StoredProcedure;

                // append parameters.
                foreach (IDbDataParameter parameter in table.Values)
                {
                    if (parameter == null) continue;
                    cmd.Parameters.Add(parameter);
                }

                recordsAffected = cmd.ExecuteNonQuery();

              

            }
            finally
            {
                if (cnn != null) cnn.Close();
            }

            return recordsAffected;
        }

        /// <summary>
        /// Executes the object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Array ExecuteObject(Type type)
        {
            SPObjectQuery query = new SPObjectQuery(type, this.storedprocedure);
            foreach (IDbDataParameter parameter in table.Values)
            {
                if (parameter == null) continue;
                query.AppendParameter(parameter);
            }

            query.Execute(true);

            return query.ResultSet.GetColumn(type).ToArray(type);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar()
        {

            Connection cnn = null;

            object result = null;
            try
            {
                cnn = ConnectionManager.Current.GetConnection(connectionName);
                cnn.Open();

                Command cmd = new Command(storedprocedure, cnn);
                cmd.CommandType = CommandType.StoredProcedure;

                // append parameters.
                foreach (IDbDataParameter parameter in table.Values)
                {
                    if (parameter == null) continue;
                    cmd.Parameters.Add(parameter);
                }

                result = cmd.ExecuteScalar();



            }
            finally
            {
                if (cnn != null) cnn.Close();
            }

            return result;
        }
    }
}
