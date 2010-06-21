//using System;
//using System.Text;
//using System.Collections;
//using System.Diagnostics;
//using System.Reflection;
//using System.Security.Cryptography;
//using System.Data;
//using Silvtek.ORMEngine.Schema;
//using Silvtek.ORMEngine.Data;
//using Silvtek.ORMEngine.Caching;


//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Performs query against database.
//    /// </summary>
//    public class Query
//    {

//        #region -- String Constants that defines SQL templates --

//        // SQL STATEMENT TEMPLATE
//        private const string SQL_TEMPLATE =  "select {distinct} {columns} from {table} {joins} {where} {groupby} {having} {order_by} ";

//        // WHERE CRITERIA TEMPLATE
//        private const string WHERE_TEMPLATE = "where {criteria}";

//        // HAVING CRITERIA TEMPLATE
//        private const string HAVING_TEMPLATE = "having {criteria}";

//        #endregion 

//        #region -- Member Variables --
      
//        // Variables that holds the main type and its schema.
//        private Type  maintype;					// Holds the query main databoject for this query 
//        private ObjectSchemaInfo maininfo;			// Holds the schema information of main type to be queried.

//        // Variables that holds query information
//        private CriteriaList criteria;			// Holds a list of criterias.
//        private JoinClauseList joins;			// Holds a list of joints
//        private OrderByList orderbys;			// Holds the order by statement. 
//        private TypeCollection objects;			// Holds a collection of type that will be returned in the resultset.
//        private HavingClauseList having;		// Hold havings criterias
//        private SingleMemberList singlecolumns;	// Holds single columns ,
//        private bool isCaseSensitive;			// Holds whether the criteria should be case sensitive. Valid only if the database is case sensitive.
//        private bool distinct;					// Holds whether to add the 'distinct' keyword to the generated sql.
//        private Aggregate aggregate;			// Holds the agregates type for this query.                                            		
//        private string aggregatePropertyName;   // Holds the column name of the aggregate query.
//        private object aggregateResult;	        // Holds the result of an aggregate search/
		
//        // Variables that controls returned object state.
//        private bool loadObjects;				// Holds whether to load Objects
//        private bool bindObjects;				// Holds whether binds query object to the ORM reference manager
//        private bool lockObjects;				// Holds all the object returned in the result set.
		
//        // Variables that holds result information.
//        private ResultSet resultset;			// Holds the result.
//        private Type resultsetTypeKey;			// Holds the class type that one of its property will be use as a key column.
//        private string resultsetPropertyKey;	// Holds the property name that is used as a key column.

//       ///<summary>
//       /// Event fired every time a <see cref="ResultRow" /> is loaded in
//       ///the <see cref="ResultSet" />.
//       ///</summary>
//       public event RowLoadedEventHandler RowLoaded;

//        #endregion

//       ///<summary>
//       /// Set or gets whether the query will contains the SQL keyword <c>distinct</c>
//       ///</summary>
//       ///<value>
//       /// <b>True</b> if contains the SQL keyword <c>distinct</c>; otherwise
//       /// <b>False</b>
//       ///</value>
//       public bool Distinct
//       {
//         get {return distinct;}
//         set {distinct = value;}
//       }


//       ///<summary>
//       /// Gets query <see cref="ResultSet" />.
//       ///</summary>
//       public ResultSet ResultSet
//       {
//         get { return resultset; }
//       }

//       ///<summary>
//       /// Gets or sets whether the <see cref="ORMObject" /> returned by the <see cref="ResultSet" /> can interact with 
//       /// the database. <i>For this property to have effect <see cref="BindResult" /> must be true.</i>
//       ///</summary>
//       ///<value>
//       /// <see lang="true" /> the objects; The default value is <see lang="true" />.
//       ///</value>
//       ///<remarks>
//       public bool LockObjects
//       {
//         get {return lockObjects;}
//         set {lockObjects = value;}
//       }

//       ///<summary>
//       /// Gets or sets whether the object returned by the <see cref="ResultSet" /> are bound which means they are
//       /// managed by the <I>ORM Reference manager</I> and they can perform operations with the database.
//       ///</summary>
//       ///<value>
//       /// <see lang="true"/> to bind the object in the <see cref="ResultSet"/>; otherwise <see lang="false" />. 
//       /// The default value is <see lang="false" />.
//       ///<remarks>
//       /// <note type="Warning">
//       ///  Binding queries that returns large result set could affect performance. 
//       ///</remarks>
//       public bool BindResults
//       {
//         get {return bindObjects; }
//         set {bindObjects = value; }
//       }


//       /// <summary>
//       /// Gets the criteria collection.
//       /// </summary>
//       public CriteriaList Criteria
//       {
//         get {return criteria;}
//       }


//       /// <summary>
//       /// Gets the having collection.
//       /// </summary>
//       public HavingClauseList Having
//       {
//         get {return having;}
//       }


//        public bool IsAggregate
//        {
//            get {return aggregate != Aggregate.None;}
//        }
	  
//        public JoinClauseList Join
//        {
//            get {return joins;}
//        }

//        /// <summary>
//        /// Gets the order by list.
//        /// </summary>
//       public OrderByList OrderBy
//       {
//         get {return orderbys;}
//       }

//        /// <summary>
//        /// Gets the aggregate result if the query is an aggregate query; otherwise returns null.
//        /// </summary>
//       public object AggregateResult
//       {
//         get {return aggregateResult;}
//       }


//       public SingleMemberList SingleMember
//       {
//         get {return singlecolumns;}
//       }

//       public bool LoadObjects
//       {
//          get {return loadObjects;}
//          set {loadObjects = value;}
//       }

//        /// <summary>
//        /// Gets or sets whether the query is case sensitive.
//        /// </summary>
//       public bool CaseSensitive
//       {
//         get {return isCaseSensitive;}
//       }


//    #region --    Constructors    --


//       ///<summary>
//       /// Instance an aggregate query.
//       ///</summary>
//       ///<param name="type">
//       /// The <see cref="Type" /> of an <see cref="ORMObject" />.
//       ///</param>
//       ///<param name="aggregate">
//       /// Type of <see cref="Aggregate" />.
//       ///</param>
//       ///<param name="memberName">
//       /// The property of <paramref name="dataobject" /> to perform
//       /// the aggregate function.
//       ///</param>
//       public Query(Type type, Aggregate aggregate, string memberName) : this(type, aggregate, memberName, true)
//       {}



//       ///<summary>
//       /// Instanciates a aggregate query.
//       ///</summary>
//       ///<param name="type">
//       /// The main <see cref="Type" /> of the query.
//       ///</param>
//       ///<param name="aggregate">
//       /// The <see cref="Aggregate" /> type.
//       ///</param>
//       ///<param name="memberName">
//       /// The member name to be queried.
//       ///</param>
//       ///<param name="caseSensitive">
//       ///  Whether the query will be case sensitive or not. (only applies to case sensitive databases).
//       ///</param>
//       public Query(Type type, Aggregate aggregate, string memberName, bool caseSensitive) : this(type, caseSensitive)
//       {
//          this.aggregate = aggregate;
//          this.aggregatePropertyName = memberName;
//          aggregateResult = 0;

//       }


//       ///<summary>
//       /// Instance a regular query.
//       ///</summary>
//       ///<param name="dataobject">
//       /// The <see cref="Type" /> of an <see cref="ORMObject" />.
//       ///</param>
//       public Query(Type type) : this(type, true)
//       {}

//       ///<summary>
//       /// Instance a regular query.
//       ///</summary>
//       ///<param name="dataobject">
//       /// The <see cref="Type" /> of an <see cref="ORMObject" />.
//       ///</param>
//       ///<param name="caseSensitive">
//       ///  Whether the criteria results is case sensitive or not.
//       ///</param>
//       public Query(Type type, bool caseSensitive)
//       {

//         if (type == null)
//         {
//           throw new ArgumentNullException("Argument cannot be null !", "type");
//         }

//         this.maintype = type;
//         this.maininfo = ObjectSchemaContainer.GetObjectSchema(maintype);
//         this.bindObjects = false;
//         this.lockObjects = true;
//         this.aggregate = Aggregate.None;
//         this.isCaseSensitive = caseSensitive;
//         this.criteria = new CriteriaList(maintype, isCaseSensitive);
//         this.joins = new JoinClauseList(maintype, isCaseSensitive);
//         this.orderbys = new OrderByList(maintype);
//         this.having = new HavingClauseList(maintype, isCaseSensitive);
//         this.singlecolumns = new SingleMemberList();
//         this.objects = new TypeCollection();
//         this.objects.Add(maintype);
//         this.resultset = new ResultSet();
//         this.aggregateResult = 0;
//         this.loadObjects = true;
//       }

//    #endregion

//        public GroupedCriteria CreateGroupedCriteria(LogicalOperator op)
//        {
//            GroupedCriteria grp_criteria = new GroupedCriteria(op, maintype, isCaseSensitive);
//            criteria.Add(grp_criteria);
//            return grp_criteria;
//        }

//        public GroupedCriteria CreateGroupedCriteria()
//        {
//           return CreateGroupedCriteria(LogicalOperator.None);
//        }

//        /// <summary>
//        /// Register an type to be loaded. 
//        /// </summary>
//        /// <param name="ormType">The type to be loaded.</param>
//        public void RegisterType(Type ormType)
//        {
//            objects.Add(ormType);
//        }
     
//        ///<summary>
//        /// Sets the <see cref="Type" /> and <b>Property</b> that will
//        /// be used a key for the collections. This will allow rapid
//        /// access in the <see cref="ResultSet" />.
//        ///</summary>
//        ///<param name="type" >
//        /// Sets the <see cref="ORMObject" /> that its property is used
//        /// as key for rapid access in the <see cref="ResultSet" />.
//        ///</param>
//        ///<param name="property">
//        /// The property of <paramref name="type" /> that is used
//        /// as key for rapid access iin the <see cref="ResultSet" />.
//        ///</param>
//        public void SetKeyMember(Type type, string property)
//        {
//            this.resultsetTypeKey = type;
//            this.resultsetPropertyKey = property;
//        }

//        /// <summary>
//        ///  Execute query and sets the duration that it can be cached.
//        /// </summary>
//        /// <param name="span">The duration.</param>
//        /// <param name="slideTime">Whether the duration will be a slide time or fixed.</param>
//        public void Execute(TimeSpan span, bool slideTime)
//        {
//            TimeController controller = new TimeController(span, slideTime);
//            Execute(false, controller);
//        }

//        /// <summary>
//        /// Execute query and sets a absolute expiration time.
//        /// </summary>
//        /// <param name="expirationTime">The date and time the result will expired from the cached.</param>
//        public void Execute(DateTime expirationTime)
//        {
//            TimeController controller = new TimeController(expirationTime);
//            Execute(false, controller);
//        }

//        /// <summary>
//        /// Execute Query.
//        /// </summary>
//        /// <param name="realtime">if <see lang="true" /> queries the database each time, even when 
//        /// the result could be on the cached; if <see lang="false" /> gets from the cache if exists in there.</param>
//        public void Execute(bool realtime)
//        {
//            Execute(realtime, TimeController.Default);
//        }

//        /// <summary>
//        /// Execute Query. It will return the query from the cache if exists in there.
//        /// </summary>
//        public void Execute()
//        {
//            Execute(false);
//        }

//        ///<summary>
//        /// Execute query.
//        ///</summary>
//        private void Execute(bool realTime, TimeController controller)
//        {
//            string sql = GetSql();
//            string queryIdentifier = Query.GenerateQueryIdentifier(this.maintype, sql);
//#if DEBUG
//            Debug.Write("[Query] Executing sql: "+sql, Resources.TRACE_CATEGORY);
//#endif
			
//            if (aggregate == Aggregate.None)
//            {
//                LoadResults(queryIdentifier, sql, realTime, controller);
//            }
//            else
//            {
//                LoadAggregateResults(queryIdentifier, sql, realTime, controller);
//            }
//        }

//        private void LoadAggregateResults(string queryIdentifier, string sql, bool realtime, TimeController timeController)
//        {
//            //sql = GetSQL();
//            resultset = new ResultSet();

//            if ((!realtime) && QueryCache.Current.Exists(queryIdentifier))
//            {
//                this.aggregateResult = QueryCache.Current.GetObject(queryIdentifier);
//                if (this.aggregateResult != null)return;
//            }

//            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(maintype);
//            Connection cnn = info.GetConnection();
//            cnn.Open();
//            Command cmd = new Command(sql, cnn);

//            try
//            {
//                DataReader reader = cmd.ExecuteReader();
//                if (reader.Read())
//                {
//                    if (!reader.IsDBNull(0))
//                    {
//                        aggregateResult = reader.GetValue(0);
//                        QueryCache.Current.AddItem(queryIdentifier, aggregateResult, this.maintype, timeController);
//                    }
//                    else
//                    {
//                        aggregateResult = null;
//                    }
//                }

//                reader.Close();
//            }
//            catch (Exception e)
//            {
//                Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", sql, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
//                throw;
//            }
//            finally
//            {
//                cnn.Close();
//            }

//            cmd = null;
//            cnn = null;
//        }

//      #region --  Execute SQL Method --

//        ///<summary>
//        /// Executes query.
//        ///</summary>
//        private void LoadResults(string queryIdentifier, string sql, bool realtime, TimeController timeController)
//        {
//             //get sql.
//             //sql = GetSQL();
            
//            if ((!realtime) && QueryCache.Current.Exists(queryIdentifier))
//            {
//               this.resultset = QueryCache.Current.GetOLDResultSet(queryIdentifier);
//               if (this.resultset != null)return;
//            }

//             // Number of columns for the result set.
//             int numberOfColumns = (objects.Count + singlecolumns.Count);
            
//             //Instance resultset collection.
//             resultset = new ResultSet(resultsetTypeKey, resultsetPropertyKey, queryIdentifier, sql, numberOfColumns);
		
//             //Get the ObjectSchemaInfo instance from the main dataobject.
//             ObjectSchemaInfo maininfo = ObjectSchemaContainer.GetObjectSchema(maintype);

//             //Get the connection from the main dataobject.
//             Connection cnn = maininfo.GetConnection();
//             cnn.Open();
//             Command cmd = new Command(sql, cnn);

//             try
//             {
//                DataReader reader = cmd.ExecuteReader();

//                // Start reading rows.
//                while(reader.Read())
//                {

//                  int col_index = 0;   //Column index.

//                  ResultRow currentRow = new ResultRow(numberOfColumns);

//                  if (loadObjects)
//                  {

//                    foreach(Type type in objects)
//                    {
//                        cnn.UpdateExecutionDateTime();

//                        ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//                        MapMemberList columns = info.MappedMembers;

//                        ORMObject currentObj = null;
//                        for(int col_counter = 0 ; col_counter < columns.Count;  col_counter++, col_index++)
//                        {

//                            if (columns[col_counter].Properties.IsPrimaryKey && reader.IsDBNull(col_index))
//                            {
//                                currentObj = null;
//                                int col_left = columns.Count - col_counter;
//                                col_index += col_left;
//                                break;
//                            }

//                            if (currentObj == null)
//                            {
//                                cnn.UpdateExecutionDateTime();

//                                currentObj = (ORMObject)Activator.CreateInstance(type,
//                                                                                 BindingFlags.NonPublic |
//                                                                                 BindingFlags.Public |
//                                                                                 BindingFlags.Instance,
//                                                                                 null, null, null);

//                                currentObj.InternalFireBeforeLoaded();
//                            }

//                            if (columns[col_counter].Properties.AcceptNulls && reader.IsDBNull(col_index))continue;

//                            columns[col_counter].SetValue(currentObj, reader.GetValue(col_index));
	                          
//                        }

//                        if (currentObj != null)
//                        {
//                            if (bindObjects)
//                            {
//                                currentObj.BindByForce();
//                                if (lockObjects)ORM.Lock(currentObj);
//                            }
//                            else
//                            {
//                                ORM.Lock(currentObj);
//                            }

//                            currentObj.InternalFireAfterLoaded();
//                        }

//                        currentRow.Add(currentObj);

//                    }

//                  } // close if condition.

//                    for(int k = 0; k <= singlecolumns.Count - 1; k++)
//                    {
//                      cnn.UpdateExecutionDateTime();
//                      //check if the column is null, pass null, if the value
//                      //is DBNullValue.
//                      object val = (reader.IsDBNull(col_index)) ? null:	 reader.GetValue(col_index);

//                      //add the value in the collection.
//                      currentRow.Add(val);


//                      col_index++;
//                    }

//                    int index = resultset.AddRow(currentRow);

//                    if (RowLoaded != null)
//                    {
//                      cnn.UpdateExecutionDateTime();
//                      RowLoaded(resultset, new RowLoadedEventArgs(index, currentRow));
//                      cnn.UpdateExecutionDateTime();
//                    }

//                 }

//                 reader.Close();
//             }
//             catch (Exception e)
//             {
//                 Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", sql, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
//                 throw;
//             }
//             finally
//             {
//                cnn.Close();
//             }

//             QueryCache.Current.AddItem(resultset, this.maintype, timeController);
//             cnn = null;
//             cmd = null;
//        }

//   #endregion

//      #region -- Generate SQL --

//        ///<summary>
//        /// This method will generate the sql statement to run the query.
//        ///</summary>
//        internal string GetSql()
//        {
//          // Hold sql string template.
//          StringBuffer buffer = new StringBuffer(SQL_TEMPLATE);

//          // Set template default values.
//          buffer.SetTokenValue("table", maininfo.TableName);
//          buffer.SetTokenValue("joins", String.Empty);
//          buffer.SetTokenValue("where", String.Empty);
//          buffer.SetTokenValue("order_by", String.Empty);
//          buffer.SetTokenValue("having", String.Empty);
//          buffer.SetTokenValue("distinct", String.Empty);
//          buffer.SetTokenValue("groupby", String.Empty);

//          // check if the current query is an aggregate query.
//          if (distinct && aggregate == Aggregate.None)
//          {
//             buffer.SetTokenValue("distinct", "distinct");
//          }

//          if (aggregate == Aggregate.None)
//          {
//             buffer.SetTokenValue("columns", GetColumns(false));
//          }
//          else
//          {

//            if (aggregate == Aggregate.CountAll)
//            {
//                // count(*) statement
//                string parsedAggregate = QueryHelper.ParseAggregate(aggregate, null, false, maininfo.DbTypeFormatter, isCaseSensitive);
//                buffer.SetTokenValue("columns", parsedAggregate );
//            }
//            else
//            {
//                 MapMember col = maininfo.MappedMembers.Find(aggregatePropertyName);
//                 string parsedAggregate = QueryHelper.ParseAggregate(aggregate, col.ColumnName, distinct, maininfo.DbTypeFormatter, isCaseSensitive);
//                 buffer.SetTokenValue("columns", parsedAggregate);
//            }
//          }


//          //Add Joins.
//          if (joins.Count > 0)buffer.SetTokenValue("joins", joins.ToString());

//          //Add criterias.
//          if (criteria.Count > 0)
//          {
//            StringBuffer where = new StringBuffer(WHERE_TEMPLATE);
//            where.SetTokenValue("criteria", criteria.ToString());
//            buffer.SetTokenValue("where", where.ToString());
//          }

//          //Add Order bys
//          if (orderbys.Count > 0)
//          {
//            string tmpOrderBy = String.Format("order by {0} {1} ",
//                                              orderbys.ToString(),
//                                              OrderByList.ParseDirection(orderbys.InternalDirection));

//            buffer.SetTokenValue("order_by", tmpOrderBy);
//          }

//          //Add Havings
//          if (having.Count > 0)
//          {

//            StringBuffer havingBuffer = new StringBuffer(HAVING_TEMPLATE);
//            havingBuffer.SetTokenValue("criteria", having.ToString());
//            buffer.SetTokenValue("having", havingBuffer.ToString());
//          }

//          if (singlecolumns.HasAggregate || (aggregate == Aggregate.None && having.Count > 0 ) )
//          {
//            buffer.SetTokenValue("groupby", String.Format("group by {0}", GetColumns(true)));
//          }

//          //return sql.
//          return buffer.ToString();
//        }
//    #endregion


//        private string GetColumns(bool forOrderBy)
//        {
//           StringBuffer columns = new StringBuffer("{comma} {columnlist}");
//           columns.SetTokenValue("comma", String.Empty);
//           string columnlist = String.Empty;


//         /* condition add on 7/8 */
//         if (loadObjects || ( (!loadObjects) && singlecolumns.Count == 0))
//         {
//           foreach(Type type in objects)
//           {
//              string list = GetColumnsList(type);
//              columns.SetTokenValue("columnlist", list);
//              columnlist += columns.ToString();
//              columns.SetTokenValue("comma", ",");
//           }
//         }

//          /*add on 7/8 */
//          foreach(SingleMember single in singlecolumns)
//          {
//            if (single.Aggregate != Aggregate.None && forOrderBy)continue;
//            columns.SetTokenValue("columnlist", single.Value);
//            columnlist += columns.ToString();
//            columns.SetTokenValue("comma", ",");
//          }


//           return columnlist;
//        }

//        private string GetColumnsList(Type type)
//        {
//            ObjectSchemaInfo info = ObjectSchemaContainer.GetObjectSchema(type);
//            string tablename = info.TableName;
//            int size = info.MappedMembers.Count;
//            StringBuilder tmpColumnsList = new StringBuilder(250);
//            for (int i = 0; i < size; i++)
//            {
//                tmpColumnsList.Append(String.Format("{0}.{1}",  tablename, info.MappedMembers[i].ColumnName));

//                if (i < size - 1)tmpColumnsList.Append(", ");
//            }

//            return tmpColumnsList.ToString();
//        }

//        // Generates an unique queryID depending of the sql and main type.
//        internal static string GenerateQueryIdentifier(Type type, string sql)
//        {
//            if ( (sql == null) || (sql.Length == 0) )return String.Empty;

//            byte[] bytes = Encoding.UTF8.GetBytes(sql);
//            SHA1Managed sha1 = new SHA1Managed();
//            byte[] result = sha1.ComputeHash(bytes);

//            long resultAsNumber = BitConverter.ToInt64(result, 0);
//            string suffix = Convert.ToString(resultAsNumber, 16);
					
//            return String.Format("{0}:{1}:OLD", type.FullName, suffix); 
//        }


//    }
//}
