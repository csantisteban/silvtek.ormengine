#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.QueryLanguage;
using Silvtek.ORMEngine.Schema;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.DataQuery.Parsers;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Represents a SQL Query.
	/// </summary>
	public class Query : QueryBase
	{

        private delegate void Load(string queryIdentifier, string sql, TimeController timeController, AutoResetEvent ev);

		// Variables that controls returned object state.
		private bool bindObjects;				// Holds whether binds query object to the ORM reference manager
		private bool lockObjects;				// Holds all the object returned in the result set.

		// Variables that holds result information.
		private ResultSet resultset;			// Holds the result.
        private bool resultsSetFromCache;
		private Type resultsetTypeKey;			// Holds the class type that one of its property will be use as a key column.
        private string resultsetMemberyKey;	// Holds the property name that is used as a key column.
	    private bool loadInBackground;
	    private int rowNumberBuffer;
	    private int waitForInitialResults;

	    private CommandText commandtext;

	   
        ///<summary>
        /// Event fired every time a <see cref="ResultRow" /> is loaded in
        ///the <see cref="ResultSet" />.
        ///</summary>
        public event RowLoadedEventHandler RowLoaded;


        /// <summary>
        /// Occurs when the execution of the query is completed.
        /// </summary>
        public event EventHandler QueryExecutionCompleted;

		///<summary>
		/// Gets query <see cref="ResultSet" />.
		///</summary>
		public virtual ResultSet ResultSet
		{
			get { return resultset; }
		}

        /// <summary>
        /// Gets a value indicating whether this instance is results set from cache.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is results set from cache; otherwise, <c>false</c>.
        /// </value>
        public bool IsResultsSetFromCache
        {
            get { return resultsSetFromCache; }
            
        }

        /// <summary>
        /// Gets a value indicating whether this instance is results set from cache.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [internal is result set from cache]; otherwise, <c>false</c>.
        /// </value>
        internal protected bool InternalIsResultSetFromCache
        {
            get { return resultsSetFromCache; }
            set { resultsSetFromCache = value; }
        }


		///<summary>
		/// Gets or sets whether the <see cref="ORMObject" /> returned by the <see cref="ResultSet" /> is allow to modify the database.
		///</summary>
		///<value>
		/// <see lang="true" /> the objects; The default value is <see lang="true" />.
		///</value>
		public bool LockObjects
		{
			get {return lockObjects;}
			set {lockObjects = value;}
		}

		///<summary>
		/// Gets or sets whether the object returned by the <see cref="ResultSet" /> are bound which means they are
		/// managed by the <I>ORM Reference manager</I> and they can perform operations with the database.
		///</summary>
		public bool BindObjects
		{
			get {return bindObjects; }
			set {bindObjects = value; }
		}


        /// <summary>
        /// Gets or sets the number of records to be preloaded
        /// </summary>
        /// <value>The row number buffer.</value>
	    public int RowNumberBuffer
	    {
            get { return rowNumberBuffer; }
            set { rowNumberBuffer = value; }
	    }

	    public bool LoadInBackground
	    {
	        get { return loadInBackground; }
	        set { loadInBackground = value; }
	    }

	    public int WaitForInitialResultsInMs
	    {
	        get { return waitForInitialResults; }
	        set { waitForInitialResults = value; }
	    }


	    /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The type of the main object used in the query</param>
		public Query(Type type) : base(type)
		{
            Distinct = false;
            resultsSetFromCache = false;
	        rowNumberBuffer = 1000;
	        WaitForInitialResultsInMs = 15000;
	        loadInBackground = false;
		}


        
        public void SetCommandText(CommandText cmdtext)
        {
            if (cmdtext == null)
            {
                throw new ArgumentNullException("cmdtext");
            }

            this.commandtext = cmdtext;
        }


        /// <summary>
        /// Returns the sql.
        /// </summary>
        public override string GetSql()
        {
            if (commandtext == null)
            {
                QueryParser parser = new QueryParser(this);
                return parser.GetSql();
            }
            else
            {
                InternalQLangParser parser = new InternalQLangParser(commandtext);
                string sql = parser.Parse();
                base.Types.Clear();
                ObjectSchemaInfo[] objectSchemaInfos = parser.ObjectSchemaInfos;
                 
                foreach(ObjectSchemaInfo info in objectSchemaInfos)
                {
                    Types.Add(info);
                }

                return sql;
            }
        }

        /// <summary>
        /// Register with the query the type and member (field or property) used as a key value. The
        /// property or field must be already mapped.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="member"></param>
        public void RegisterKeyValue(Type type, string member)
        {
            resultsetTypeKey = type;
            resultsetMemberyKey = member;
        }


        ///<summary>
        /// Execute query.
        ///</summary>
        internal override void Execute(bool realTime, TimeController controller)
        {
            string sql = GetSql();
            string queryIdentifier = GenerateQueryIdentifier(Types.MainType.Type, sql);
#if DEBUG
			Debug.Write("[Query] Executing sql: "+sql, Resources.TRACE_CATEGORY);
#endif

            if ((!realTime) && QueryCache.Current.Exists(queryIdentifier))
            {
                resultset = QueryCache.Current.GetResultSet(queryIdentifier);
                if (resultset != null)
                {
                    resultsSetFromCache = true;
                    FireQueryExecutionCompleted();
                    return;
                }
            }

            int numberOfColumns = Types.Count;
            resultset = new ResultSet(resultsetTypeKey, resultsetMemberyKey, queryIdentifier, sql, numberOfColumns);
 
            if (!LoadInBackground)
            {
                LoadResults(queryIdentifier, sql, controller, null);
                FireQueryExecutionCompleted();
            }
            else
            {
                AutoResetEvent ev = new AutoResetEvent(false); 
                Load load = new Load(LoadResults);
                load.BeginInvoke(queryIdentifier, sql, controller, ev, new AsyncCallback(LoadCallBack) , this);

                // wait for the buffer to be filled.

#if NET20
                ev.WaitOne(waitForInitialResults);
#endif
#if NET11
                ev.WaitOne();
#endif
            }
        }


        private static void LoadCallBack(IAsyncResult result)
        {
            if (result == null) return;
            AsyncResult asyncResult = (AsyncResult)result;
            Load loadDelegate = (Load)asyncResult.AsyncDelegate;
            loadDelegate.EndInvoke(result);

            Query query = (Query)asyncResult.AsyncState;
            query.FireQueryExecutionCompleted();
        }

     

        #region --  Execute SQL Method --

        ///<summary>
        /// Executes query.
        ///</summary>
        // Comments on this methods are verbose, but worth it.
        private void LoadResults(string queryIdentifier, string sql, TimeController timeController, AutoResetEvent ev)
        {

            QueryCache.Current.AddItem(resultset, Types.MainType.Type, timeController);

            resultsSetFromCache = false;

            // Number of object columns for the result set.
            int numberOfColumns = Types.Count;

            // Creates the result set object.
            
            ResultSet bufferResultSet = null;
            //Get the connection from the main dataobject.
            Connection cnn = Types.MainType.GetConnection();

            if (loadInBackground)
            {
                bufferResultSet = new ResultSet(resultsetTypeKey, resultsetMemberyKey, queryIdentifier, sql, numberOfColumns);
            }

            int counter = 0;
            try
            {
                cnn.Open();
                Command cmd = new Command(sql, cnn);
                DataReader reader = cmd.ExecuteReader();
                

                // Start reading database rows
                while (reader.Read())
                {

                    // initialize database column index to 0 (zero).
                    int columnIndex = 0;

                    // Create result row.
                    ResultRow currentRow = new ResultRow(numberOfColumns);
                    for (int i = 0; i < Types.Count; i++)
                    {
                        // Update the execution date time for making sure it won't time out. (just in case :)).
                        cnn.UpdateExecutionDateTime();

                        ObjectSchemaInfo info = Types[i];
                        MapMemberList columns = info.MappedMembers;

                        // Holds the actual object to be created.
                        object currentObj = null;
                        ORMObject ormObj = null;
                        for (int columnCounter = 0; columnCounter < columns.Count; columnCounter++, columnIndex++)
                        {
                            MapMember mapMember = columns[columnCounter];
                            if (mapMember.Properties.IsPrimaryKey && reader.IsDBNull(columnIndex))
                            {
                                currentObj = null;
                                int columnsLeft = columns.Count - columnCounter;
                                columnIndex += columnsLeft;
                                break;
                            }

                            if (currentObj == null)
                            {
                                cnn.UpdateExecutionDateTime();

                                currentObj = Activator.CreateInstance(info.Type, BindingFlags.NonPublic |
                                                                                 BindingFlags.Public | BindingFlags.Instance,
                                                                      null, null, null);

                                if (currentObj is ORMObject || bindObjects)
                                {
                                    ormObj = ORM.Instanciate(currentObj);
                                }


                                if (ormObj != null) ormObj.InternalFireBeforeLoaded();
                            }

                            if (mapMember.Properties.AcceptNulls && reader.IsDBNull(columnIndex))
                            {
                                mapMember.SetDataSourceValue(currentObj, null);
                                continue;
                            }

                            if (mapMember.Properties.DataType != DataType.Binary)
                            {
                                mapMember.SetDataSourceValue(currentObj, reader.GetValue(columnIndex));
                                if (ormObj != null)
                                {
                                    if (mapMember.Properties.IsRecordDigestEnabled)
                                    {

                                        ormObj.SetRecordDigest(Convert.ToString(reader.GetValue(columnIndex)));
                                    }
                                }

                            }
                            else
                            {
                                if (!reader.IsDBNull(columnCounter))
                                {
                                    DataStream dataStream = new DataStream(mapMember);
                                    Byte[] buffer = new byte[ORMConfigurator.BufferSize];
                                    int startIndex = 0;
                                    long bytesRead;
                                    do
                                    {
                                        bytesRead = reader.GetBytes(columnCounter, startIndex, buffer, 0, buffer.Length);
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
                            if (bindObjects)
                            {
                                ormObj.BindByForce();
                                if (lockObjects) ORM.Lock(ormObj);
                            }
                            else if (lockObjects)
                            {
                                ORM.Lock(ormObj);
                            }

                            ormObj.SetActionResult(ActionResult.Succeed);
                            ormObj.InternalFireAfterLoaded();
                        }

                        currentRow.Add(currentObj);

                    }


                   
                    if (LoadInBackground && bufferResultSet != null)
                    {
                        int index = bufferResultSet.AddRow(currentRow);
                        if (index >= RowNumberBuffer)
                        {
                            resultset.Append(bufferResultSet);
                            bufferResultSet = new ResultSet(resultsetTypeKey, resultsetMemberyKey, queryIdentifier, sql, numberOfColumns);
                            if (ev != null)
                            {
                                ev.Set();
                            }                            
                        }
                    }
                    else
                    {
                        resultset.AddRow(currentRow); 
                    }

                    counter++;
                    cnn.UpdateExecutionDateTime();
                    FireRowLoaded(this, counter, currentRow);
                    cnn.UpdateExecutionDateTime();

                }

                if (LoadInBackground)
                {
                    if (bufferResultSet != null)
                    {
                        resultset.Append(bufferResultSet);
                    }

                    if (ev != null)
                    {
                        ev.Set();
                    }
                }
                reader.Close();
            }
            finally
            {
                cnn.Close();
            }

            
          //  cnn = null;
           // cmd = null;
        }



        protected void FireRowLoaded(object sender, int index, ResultRow row)
        {
            if (RowLoaded != null)
            {
                RowLoaded(sender, new RowLoadedEventArgs(index, row));
            }
        }


        protected void FireQueryExecutionCompleted()
        {
            if (QueryExecutionCompleted != null)
            {
                QueryExecutionCompleted(this, EventArgs.Empty);
            }
        }




        #endregion


        /// <summary>
        /// Finds the specified member name.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="realtime">if set to <c>true</c> [realtime].</param>
        /// <returns></returns>
        public Array Find(string memberName, object searchValue, bool realtime)
        {
            SimpleFindCriteria simpleCriteria = new SimpleFindCriteria();
            simpleCriteria.MemberName = memberName;
            simpleCriteria.Value = Convert.ToString(searchValue);
 
            return Find(simpleCriteria, realtime);

        }

        /// <summary>
        /// Finds the specified member name.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns></returns>
        public Array Find(string memberName, object searchValue)
        {
            return Find(memberName, searchValue, false);
        }

        /// <summary>
        /// Finds the specified simple criteria.
        /// </summary>
        /// <param name="simpleCriteria">The simple criteria.</param>
        /// <returns></returns>
        public Array Find(SimpleFindCriteria simpleCriteria)
        {
            return Find(simpleCriteria, false);
        }

        /// <summary>
        /// Query the database for the primary type for the current Query instance. This is a simple query, which means
        /// that it is filter by one column value. This method is for fast usage of the Query object.
        /// </summary>
        /// <param name="simpleCriteria">An object containing the member name and the value for the query.</param>
        /// <param name="realtime"></param>
        /// <returns></returns>
        public Array Find(SimpleFindCriteria simpleCriteria, bool realtime)
        {
            if (simpleCriteria != null)
            {
                string memberName = simpleCriteria.MemberName;
                string sortBy = simpleCriteria.OrderBy;

                if (memberName.Length != 0 && simpleCriteria.Value.Length != 0)
                {
                    MapMember map = Types.MainType.MappedMembers.Find(memberName);

                    CriteriaItem criteria = new CriteriaItem(Types.MainType.Type, map.MemberName);
                    criteria.EqualTo(simpleCriteria.Value);
                    Criterias.Add(criteria);
                }

                if (sortBy != null && sortBy.Length > 0)
                {
                    OrderBy(Types.MainType.Type, sortBy, simpleCriteria.OrderDirection);
                }
            }

            Execute(realtime);
            return ResultSet.GetColumn(0).ToArray(Types.MainType.Type);
        }

        /// <summary>
        /// Finds the name of the by column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
	    public Array FindByColumnName(string columnName, object value)
        {
            return FindByColumnName(columnName, value, false);
        }

        /// <summary>
        /// Finds the name of the by column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        /// <param name="realtime">if set to <c>true</c> [realtime].</param>
        /// <returns></returns>
        public Array FindByColumnName(string columnName, object value, bool realtime)
        {
            if (value == null) return new object[0];

            ObjectSchemaInfo info = ORM.GetObjectInfo(Types.MainType.Type);
            MapMember member = info.MappedMembers.FindByColumnName(columnName);
            CriteriaItem criteria = new CriteriaItem(Types.MainType.Type, member.MemberName);
            criteria.EqualTo(value);

            Criterias.Add(criteria);

            Execute(realtime);

            return ResultSet.GetColumn(0).ToArray(Types.MainType.Type);
        }



        /// <summary>
        /// Gets all records stored in the database for the type provided
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="realtime">Whether the query should be realtime or it can be taken from the query cache.</param>
        public static Array GetAll(Type type, bool realtime)
        {
            Query query = new Query(type);
            query.Execute();
            return query.ResultSet.GetColumn(0).ToArray(type);
        }

        public static Array Find(Type type, SimpleFindCriteria criteria)
        {
            return Find(type, criteria, false);
        }

        public static Array Find(Type type, SimpleFindCriteria criteria, bool realtime)
        {
            Query query = new Query(type);
            return query.Find(criteria, realtime);
        }

        /// <summary>
        /// Gets all records stored in the database for the type provided
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public Array GetAll(Type type)
        {
            return GetAll(type, false);
        }



    }
}
