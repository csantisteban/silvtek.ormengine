#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// This class generates all SQL requested by the <see cref="ORMObject" />
    /// it belongs.
    /// </summary>
    [Serializable]
    internal sealed class SqlBuilder
    {
        private const string DELETE_SQL = "delete from {table} " +
                                          "where {criteria}";

        private const string INSERT_SQL = "insert into {table} ({columns}) values ({values})";
        private const string SELECT_SQL = "select {columns} from {table} where {criteria} ";

        private const string UPDATE_SQL = "update {table} " +
                                          "set {columnvalue} " +
                                          "where {criteria} ";

       // private readonly string insertColumns;
        private readonly MapMemberList mappedMembers;
        private readonly string selectColumns;
        private readonly string tableName;

        // Holds the index for the sql server identity column, the default is 0;
        private int databaseIdentityColumnIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema"><see cref="ObjectSchemaInfo" /> instance for current ORM object</param>
        internal SqlBuilder(ObjectSchemaInfo schema)
        {
            tableName = schema.TableName;
            mappedMembers = schema.MappedMembers;

            // this value must be always defaulted to -1, due that the columns index start at 0.
            databaseIdentityColumnIndex = -1;

           // StringBuilder tmpColumnsListInsert = new StringBuilder(1000);
            StringBuilder tmpColumnsListSelect = new StringBuilder(1000);

            int size = mappedMembers.Count;
            for (int i = 0; i < size; i++)
            {
                string columnFullName = mappedMembers[i].ColumnFullName;
                tmpColumnsListSelect.Append(columnFullName);

                if (i < size - 1) tmpColumnsListSelect.Append(", ");

                //if (!mappedMembers[i].Properties.UseDatabaseIdentity)
                //{
                //    tmpColumnsListInsert.Append(columnFullName);
                //    if (i < size - 1) tmpColumnsListInsert.Append(", ");
                //}
            }

            //insertColumns = tmpColumnsListInsert.ToString();
            selectColumns = tmpColumnsListSelect.ToString();
        }

        internal bool HasSqlServerSequential
        {
            get { return (databaseIdentityColumnIndex > -1); }
        }

        ///<summary>
        /// Returns a select statement.
        ///</summary>
        internal string GetSelectStatement(object source)
        {
            StringBuffer select = new StringBuffer(SELECT_SQL, 1000);
            select.SetTokenValue("table", tableName);
            select.SetTokenValue("columns", selectColumns);
            select.SetTokenValue("criteria", GetPKValueList(source));
            return select.ToString();
        }

        //internal string GetInsertStatement(object source)
        //{
        //    StringBuilder tmpColumnValues = new StringBuilder(1000);
        //    int size = mappedMembers.Count;
        //    for (int i = 0; i < size; i++)
        //    {
        //        // is is SQLServer sequential do not add to the insert statement;
        //        if (mappedMembers[i].Properties.UseDatabaseIdentity)
        //        {
        //            databaseIdentityColumnIndex = i;
        //            continue;
        //        }

        //        // Get value from the source object.    
        //        object val = mappedMembers[i].GetValue(source);

        //        // Check if the primary key member has a value, if not throw error.
        //        if (mappedMembers[i].Properties.IsPrimaryKey && val == null)
        //        {
        //            string exMsg = Utility.GetResourceString("RES_PKIsNullOnInsert", mappedMembers[i].MemberName);
        //            throw new ORMException(exMsg);
        //        }
        //        else if ((!mappedMembers[i].Properties.AcceptNulls && val == null) ||
        //                 (!mappedMembers[i].Properties.AcceptNulls && val != null && mappedMembers[i].Properties.DataType == DataType.String && ((string) val).Length == 0)
        //            )
        //        {
        //            if (mappedMembers[i].Properties.DefaultValue == null)
        //            {
        //                string typeFullName = source.GetType().FullName;
        //                string propertyName = mappedMembers[i].MemberName;
        //                string excMsg = Utility.GetResourceString("EXCEPTION:PROP_IS_NULL", propertyName, typeFullName, val);

        //                throw new ORMException(excMsg);
        //            }
        //            else
        //            {
        //                val = mappedMembers[i].Properties.DefaultValue;
        //            }
        //        }

        //        // Here we set the values.
        //        if (mappedMembers[i].Properties.DataType == DataType.String && val != null)
        //        {
        //            string tmp_val = Convert.ToString(val);

        //            // Throw an error if the string excced the capacity.
        //            if (tmp_val.Length > mappedMembers[i].Properties.Length)
        //            {
        //                string exMsg = Utility.GetResourceString("SQL:VALUE_SET_EXCEEDS_CAPACITY", source.GetType().FullName, mappedMembers[i].MemberName, mappedMembers[i].Properties.Length, tmp_val);
        //                throw new ORMException(exMsg);
        //            }
        //        }

        //        string tmpValue = MapUtility.FormatValueAsString(mappedMembers[i], val, true);
        //        tmpColumnValues.Append(tmpValue);

        //        if (i < size - 1) tmpColumnValues.Append(", ");
        //    }

        //    StringBuffer insert = new StringBuffer(INSERT_SQL, 1000);
        //    insert.SetTokenValue("table", tableName);
        //    insert.SetTokenValue("columns", insertColumns);
        //    insert.SetTokenValue("values", tmpColumnValues.ToString());
        //    return insert.ToString();
        //}

        //internal string GetUpdateStatement(MemberFieldList fields, object source)
        //{
        //    const string criteriaTemplate = "{comma} {column} = {value}";
        //    int columnsToUpdate = 0;

        //    StringBuffer criteriaBuff = new StringBuffer(criteriaTemplate);
        //    StringBuilder criteria = new StringBuilder();
        //    criteriaBuff.SetTokenValue("comma", "");

        //    // Count in how many column the value has been changed.
        //    for (int i = 0; i < mappedMembers.Count; i++)
        //    {
        //        MapMember col = mappedMembers[i];
        //        //bool hasFieldChanged = true;
        //        if (col.Properties.IsPrimaryKey) continue;

        //        MemberField field = fields[col.MemberName];
        //        if (!field.Changed) continue;

        //        criteriaBuff.SetTokenValue("column", col.ColumnFullName);
        //        object val = col.GetValue(source);

        //        if ((!col.Properties.AcceptNulls && val == null) ||
        //            (!col.Properties.AcceptNulls && val != null && col.Properties.DataType == DataType.String && ((string) val).Length == 0)
        //            )
        //        {
        //            if (col.Properties.DefaultValue == null)
        //            {
        //                string typeFullName = source.GetType().FullName;
        //                string propertyName = col.MemberName;
        //                string excMsg = Utility.GetResourceString("EXCEPTION:PROP_IS_NULL", propertyName, typeFullName, val);

        //                throw new ORMException(excMsg);
        //            }
        //            else
        //            {
        //                val = col.Properties.DefaultValue;
        //            }
        //        }

        //        if (col.Properties.DataType == DataType.String && val != null)
        //        {
        //            string tmpValue = Convert.ToString(val);

        //            // Truncate value if is too long.
        //            if (tmpValue.Length > col.Properties.Length)
        //            {
        //                val = tmpValue.Substring(0, col.Properties.Length);
        //                col.SetValue(source, val);
        //            }
        //        }

        //        columnsToUpdate++;

        //        string memberValue = MapUtility.FormatValueAsString(col, val, true);
        //        criteriaBuff.SetTokenValue("value", memberValue);

        //        criteria.Append(criteriaBuff.ToString());


        //        criteriaBuff.SetTokenValue("comma", ",");
        //    }

        //    StringBuffer update = new StringBuffer(UPDATE_SQL, 1000);
        //    update.SetTokenValue("table", tableName);
        //    update.SetTokenValue("columnvalue", criteria.ToString());
        //    update.SetTokenValue("criteria", GetPKValueList(source));

        //    string updateStmt = (columnsToUpdate > 0) ? update.ToString() : String.Empty;
        //    return updateStmt;
        //}

        internal string GetDeleteStatement(object source)
        {
            StringBuffer delete = new StringBuffer(DELETE_SQL, 1000);
            delete.SetTokenValue("table", tableName);
            delete.SetTokenValue("criteria", GetPKValueList(source));
            return delete.ToString();
        }

        /// <summary>
        /// Create and returns a <see cref="Command" /> instance for current object 
        /// </summary>
        /// <param name="cnn"><see cref="Connection" /> instance</param>
        /// <param name="source">The orm object</param>
        /// <returns></returns>
        internal Command GetInsertCommand(Connection cnn, object source)
        {

            Command cmd = new Command(cnn);
            StringBuilder tmpColumnValues = new StringBuilder(1000);
            StringBuilder tmpColumnNames = new StringBuilder(1000);

            int size = mappedMembers.Count;

            for (int i = 0; i < size; i++)
            {

                // is is SQLServer sequential do not add to the insert statement;
                if (mappedMembers[i].Properties.UseDatabaseIdentity)
                {
                    databaseIdentityColumnIndex = i;

                    continue;
                }

                bool isTimestamp = mappedMembers[i].Properties.ContainsAttribute(MemberAttributes.Timestamp);
                object originalValue = mappedMembers[i].GetDataSourceValue(source);

               
                if (originalValue == null && mappedMembers[i].Properties.AcceptNulls) continue;
                    
               

                if (i > 0 && (databaseIdentityColumnIndex + 1 != i || databaseIdentityColumnIndex > 0))
                {
                    tmpColumnNames.Append(", ");
                    tmpColumnValues.Append(", ");
                }

                if (isTimestamp)
                {
                    if (mappedMembers[i].Properties.ContainsAttribute(MemberAttributes.SyncField))
                    {
                        originalValue = ORM.GetServerDateTime(source.GetType());
                        mappedMembers[i].SetDataSourceValue(source, originalValue);
                    }
                    else
                    {
                        tmpColumnValues.Append(mappedMembers[i].Formatter.GetNowCommand());
                        tmpColumnNames.Append(mappedMembers[i].ColumnName);
                        continue;
                    }
                }



                object formattedValue = MapUtility.FormatValue(mappedMembers[i], originalValue, true);
                //if (formattedValue == null || String.Compare(Convert.ToString(formattedValue), "null", false) == 0) continue;

                if (formattedValue == null || String.Compare(Convert.ToString(formattedValue), "null", false) == 0)
                {
                    formattedValue = Convert.DBNull;
                }

                string tmpValue = cnn.DbTypeFormatter.ParameterizedPrefix + mappedMembers[i].ColumnName;
                IDbDataParameter parameter = cmd.CreateParameter();
                parameter.ParameterName = tmpValue;
                parameter.Value = formattedValue;
                cmd.Parameters.Add(parameter);
                tmpColumnValues.Append(tmpValue);
                tmpColumnNames.Append(mappedMembers[i].ColumnName);


            }

            StringBuffer insert = new StringBuffer(INSERT_SQL, 1000);
            insert.SetTokenValue("table", tableName);
            insert.SetTokenValue("columns", tmpColumnNames.ToString());
            insert.SetTokenValue("values", tmpColumnValues.ToString());
            //return insert.ToString();

            cmd.CommandText = insert.ToString();
            return cmd;
        }

        internal Command GetUpdateCommand(Connection cnn, MemberFieldList fields, object source)
        {
            
            const string CRITERIA_TEMPLATE = "{comma} {column} = {value}";
            int columnsToUpdate = 0;

            Command cmd = new Command(cnn);

            StringBuffer criteriaBuffer = new StringBuffer(CRITERIA_TEMPLATE);
            StringBuilder criteria = new StringBuilder();
            criteriaBuffer.SetTokenValue("comma", "");

            // Count in how many column the value has been changed.
            for (int i = 0; i < mappedMembers.Count; i++)
            {

                MapMember mapMember = mappedMembers[i];


                if (mapMember.Properties.IsPrimaryKey) continue;


                MemberField field = fields[mapMember.MemberName];
                bool changed = (mapMember.Properties.DataType == DataType.Binary ||
                                mapMember.Properties.DataType == DataType.Object);

                if (!changed)
                {
                    changed = field.HasChanged || field.IncludeInUpdates;
                }

                if (!changed)
                {
#if DEBUG
                    string msgdbg0 = String.Format("Field {0} not changed. Field Skipped. Value: {0}", field.ActualValue);
                    Debug.WriteLine(msgdbg0);
#endif
                    continue;
                }


                columnsToUpdate++;
                object originalValue = mappedMembers[i].GetDataSourceValue(source);

                if (mappedMembers[i].Properties.ContainsAttribute(MemberAttributes.Timestamp))
                {
                    if (mappedMembers[i].Properties.ContainsAttribute(MemberAttributes.SyncField))
                    {
                        originalValue = ORM.GetServerDateTime(source.GetType());
                        mappedMembers[i].SetDataSourceValue(source, originalValue);
                    }
                    else 
                    {
                        criteriaBuffer.SetTokenValue("value", mappedMembers[i].Formatter.GetNowCommand());
                        criteriaBuffer.SetTokenValue("column", mappedMembers[i].ColumnName);
                        criteria.Append(criteriaBuffer.ToString());
                        criteriaBuffer.SetTokenValue("comma", ",");
                        continue;
                    }
                }

                criteriaBuffer.SetTokenValue("column", mapMember.ColumnFullName);

                //string memberValue = MapUtility.FormatValueAsString(col, val, true);
                string memberValue = cnn.DbTypeFormatter.ParameterizedPrefix + mapMember.ColumnName;
                criteriaBuffer.SetTokenValue("value", memberValue);

                criteria.Append(criteriaBuffer.ToString());

                IDbDataParameter parameter = cmd.CreateParameter();
                parameter.ParameterName = memberValue;
                parameter.Value = MapUtility.FormatValue(mapMember, originalValue, true);
#if DEBUG
                String dbgmsg = String.Format("[Update] Set {0} with value {1}", memberValue, parameter.Value);
                Debug.WriteLine(dbgmsg);
#endif
                cmd.Parameters.Add(parameter);

                criteriaBuffer.SetTokenValue("comma", ",");
            }

            StringBuffer update = new StringBuffer(UPDATE_SQL, 1000);
            update.SetTokenValue("table", tableName);
            update.SetTokenValue("columnvalue", criteria.ToString());

            string primaryKeyCriteria = GeneratedParameterizedPrimaryKeysCriteria(source, cmd);
            update.SetTokenValue("criteria", primaryKeyCriteria);

            //string updateStmt = String.Empty;
            if (columnsToUpdate > 0)
            {
                cmd.CommandText = update.ToString();
            }
            else
            {
                return null;
            }

            //cmd.CommandText = updateStmt;
            return cmd;
        }

        private string GeneratedParameterizedPrimaryKeysCriteria(object source, IDbCommand cmd)
        {
            MapMember[] pks = mappedMembers.GetAllPrimaryKeyMembers();

            if (pks.Length == 0)
            {
                throw new ORMException(Utility.GetResourceString("RES_PK_NotFound"));
            }

            const string criteriaTemplate = "{and} {column} = {value} ";

            StringBuffer criteriaBuff = new StringBuffer(criteriaTemplate);
            StringBuilder criteria = new StringBuilder();
            criteriaBuff.SetTokenValue("and", String.Empty);

            foreach (MapMember mappedMember in pks)
            {
                object val = mappedMember.GetMemberValue(source);

                // Set the column name.
                
                criteriaBuff.SetTokenValue("column", mappedMember.ColumnFullName);

                // Format value and set.
                string parameterName = mappedMember.Formatter.ParameterizedPrefix + mappedMember.ColumnName;
                criteriaBuff.SetTokenValue("value", parameterName);
                criteria.Append(criteriaBuff.ToString());

                // add the 'and' keyword to be use on the subsequent primary key.
                criteriaBuff.SetTokenValue("and", "and");

                IDbDataParameter parameter = cmd.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = MapUtility.FormatValue(mappedMember, val, true);
                cmd.Parameters.Add(parameter);
            }

            return criteria.ToString();
        }

        private string GetPKValueList(object source)
        {
            MapMember[] pks = mappedMembers.GetAllPrimaryKeyMembers();

            if (pks.Length == 0)
            {
                throw new ORMException(Utility.GetResourceString("RES_PK_NotFound"));
            }

            const string criteriaTemplate = "{and} {column} = {value} ";

            StringBuffer criteriaBuff = new StringBuffer(criteriaTemplate);
            StringBuilder criteria = new StringBuilder();
            criteriaBuff.SetTokenValue("and", String.Empty);

            foreach (MapMember mappedMember in pks)
            {
                object val = mappedMember.GetMemberValue(source);

                // Set the column name.
                criteriaBuff.SetTokenValue("column", mappedMember.ColumnFullName);

                // Format value and set.
                string tmpValue = MapUtility.FormatValueAsString(mappedMember, val, true);
                criteriaBuff.SetTokenValue("value", tmpValue);
                criteria.Append(criteriaBuff.ToString());

                // add the 'and' keyword to be use on the subsequent primary key.
                criteriaBuff.SetTokenValue("and", "and");
            }


            return criteria.ToString();
        }

        // FIX - This method sould return the value; object should not be update from here.
        internal void SetIdentityValue(Connection cnn, ObjectSchemaInfo schema, object source)
        {
            string sql = cnn.DbTypeFormatter.GetIdentityCommand();

            Command cmd = null;
            int sqlreturnvalue = 0;
            try
            {
                cmd = new Command(sql, cnn);
                cmd.CommandText = sql;
                sqlreturnvalue = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw new ORMException("Failed to retrieve Database Identity. Command Sent: " + sql + " Datasource message: " + e.Message);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }

            // Sets the incremental value to mapped property.
            schema.MappedMembers[databaseIdentityColumnIndex].SetMemberValue(source, sqlreturnvalue);
        }
    }
}