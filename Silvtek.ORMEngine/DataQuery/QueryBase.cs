#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Security.Cryptography;
using Silvtek.ORMEngine.Schema;
using Silvtek.ORMEngine.DataQuery.Parsers;
using Silvtek.ORMEngine.Caching;

namespace Silvtek.ORMEngine.DataQuery
{
    //public delegate ResultSet ExecuteCallback(ResultSet resultset, IAsyncResult result)

    public delegate QueryBase QueryExecuteCaller(bool realtime, TimeController controller);

    /// <summary>
    /// Represents the base query class for all queries.
    /// </summary>
    public abstract class QueryBase
    {


        private bool isCaseSensitive;
        private bool distinctKeyword;
        private Criterias criterias;
        private Criterias aggregates;
        private Joins joins;
        private QueriedTypes types;
        private QueriedAggregates queriedAggregates;
        private OrderColumns orderColumns;

        


        /// <summary>
        /// Gets or sets whether the query will be case sensitive.
        /// </summary>
        /// <remarks>This property is only valid on those database that are case sensitive</remarks>
        public bool IsQueryCaseSensitive
        {
            get { return isCaseSensitive; }
            set { isCaseSensitive = value; }
        }


        /// <summary>
        /// Tell the query to whether to add the 'distinct' keyword.
        /// </summary>
        public bool Distinct
        {
            get { return distinctKeyword; }
            set { distinctKeyword = value; }
        }


        #region -- Internal Properties --

        /// <summary>
        /// Gets the criteria collection.
        /// </summary>
        public Criterias Criterias
        {
            get { return GetCriterias(); }
        }

        /// <summary>
        /// Gets the aggregate criteria collection.
        /// </summary>
        public Criterias Aggregates
        {
            get { return GetAggregates(); }
        }

        /// <summary>
        /// Gets the join collection.
        /// </summary>
        internal Joins Joins
        {
            get { return joins; }
        }

        /// <summary>
        /// Gets the query types collection.
        /// </summary>
        internal QueriedTypes Types
        {
            get { return types; }
        }

        /// <summary>
        /// Gets the queries aggregates collection.
        /// </summary>
        internal QueriedAggregates QueriedAggregates
        {
            get
            {
                if (queriedAggregates == null) queriedAggregates = new QueriedAggregates();
                return queriedAggregates;
            }
        }

        /// <summary>
        /// Gets the order by properties names.
        /// </summary>
        internal OrderColumns OrderProperties
        {
            get { return orderColumns; }
        }

        #endregion


        /// <summary>
        /// Creates a new collection of criterias.
        /// </summary>
        /// <returns></returns>
        public static Criterias CreateCriteria()
        {
            return new Criterias(false);
        }

        /// <summary>
        /// Creates a new collection of aggregate criterias.
        /// </summary>
        /// <returns></returns>
        public static Criterias CreateAggregateCriteria()
        {
            return new Criterias(true);
        }

        /// <summary>
        /// Creates a join for a specific <see cref="Type" />.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> to join to.</param>
        /// <param name="member">The property or member name to join to.</param>
        /// <returns><see cref="Join" /> object.</returns>
        public Join CreateJoinFor(Type type, string member)
        {
            ObjectSchemaInfo info = ORM.GetObjectInfo(type);
            return new Join(info, member);
        }

        /// <summary>
        /// Register a ORM type object.
        /// </summary>
        /// <param name="type">The type</param>
        /// <remarks>
        /// By default the query only loads the main type which is the declared in the 
        /// constructor of this object; however there is circustance where you would need
        /// to load more than one type on the same query while join object. 
        /// </remarks>
        /// <returns>It returns the index of the column in which this type
        /// will be located </returns>
        public virtual int RegisterType(Type type)
        {
            if (types == null) types = new QueriedTypes();
            return types.Add(type);
        }

        /// <summary>
        /// Creates a join to the main type registered with this query.
        /// </summary>
        /// <param name="member">The property or member name to join to.</param>
        /// <returns></returns>
        public Join CreateJoinFor(string member)
        {
            return new Join(Types.MainType, member);
        }


        protected QueryBase(Type type)
        {
            types = new QueriedTypes();
            types.Add(type);
        }

        public void AddJoin(Join join)
        {
            if (joins == null) joins = new Joins();
            joins.Add(join);
        }


        public void AddCriteria(Criterias criteria)
        {
            // Uses public property
            Criterias.Add(criteria);
        }

        /// <summary>
        /// Adds the type and member to order by the results.
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="memberName">The member name.</param>
        public void OrderBy(Type type, string memberName, OrderDirection direction)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (memberName == null || memberName.Length == 0)
            {
                throw new ArgumentException("Member cannot be an empty string or null", "memberName");
            }

            if (this.orderColumns == null) this.orderColumns = new OrderColumns();
            orderColumns.Add(type, memberName, direction);
        }

        public void OrderBy(Type type, string memberName)
        {
            OrderBy(type, memberName, OrderDirection.Default);
        }


        public void AddAggregateCriteria(Criterias criteria)
        {
            // Uses public property
            Aggregates.Add(criteria);
        }



        // Generates an unique queryID depending of the sql and main type.
        internal static string GenerateQueryIdentifier(Type type, string sql)
        {
            if ((sql == null) || (sql.Length == 0)) return String.Empty;

           
            byte[] bytes = Encoding.UTF8.GetBytes(sql);
            SHA1Managed sha1 = new SHA1Managed();
            byte[] result = sha1.ComputeHash(bytes);
            string identifier = Utility.BytesToHex(result);

            return identifier;
        }

        /// <summary>
        /// Gets the query unique identifier. This unique identifier is generated
        /// depending of sql. 
        /// </summary>
        /// <returns></returns>
        public string GetIdentifier()
        {
            string sql = GetSql();
            if (types.Count == 0) return String.Empty;
            if (types[0] == null) return String.Empty;
            Type type = this.types[0].Type;
            return GenerateQueryIdentifier(type, sql);
        }

        /// <summary>
        /// Clear query cache.
        /// </summary>
        public static void ClearCache()
        {
            QueryCache.EmptyCache();
        }

        private QueryBase InternalExecuteAsync(bool realtime, TimeController controller)
        {
            Execute(realtime, controller);
            return this;
        }

        /// <summary>
        /// Begins the execute a query asynchronously
        /// </summary>
        /// <param name="span">The time span that this query can be cached..</param>
        /// <param name="slideTime">if set to <c>true</c> if the time slides with every hit</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IAsyncResult BeginExecute(TimeSpan span, bool slideTime, AsyncCallback callback, object state)
        {
            QueryExecuteCaller ormActionDelegate = new QueryExecuteCaller(InternalExecuteAsync);
            TimeController controller = new TimeController(span, slideTime);
            return ormActionDelegate.BeginInvoke(false, controller, callback, state);
        }

        /// <summary>
        /// Begins the execute a query asynchronously
        /// </summary>
        /// <param name="expirationTime">The expiration date time for of this query. It will be removed from the cached..</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IAsyncResult BeginExecute(DateTime expirationTime, AsyncCallback callback, object state)
        {
            QueryExecuteCaller ormActionDelegate = new QueryExecuteCaller(InternalExecuteAsync);
            TimeController controller = new TimeController(expirationTime);
            return ormActionDelegate.BeginInvoke(false, controller, callback, state);
        }

        /// <summary>
        /// Begins the execute a query asynchronously
        /// </summary>
        /// <param name="realtime">if set to <c>true</c> it will get records directly from database skipping cache..</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IAsyncResult BeginExecute(bool realtime, AsyncCallback callback, object state)
        {
            QueryExecuteCaller ormActionDelegate = new QueryExecuteCaller(InternalExecuteAsync);
            TimeController controller = TimeController.Default;
            return ormActionDelegate.BeginInvoke(realtime, controller, callback, state);
        }

        /// <summary>
        /// Begins the execute a query asynchronously.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            QueryExecuteCaller ormActionDelegate = new QueryExecuteCaller(InternalExecuteAsync);
            TimeController controller = TimeController.Default;
            return ormActionDelegate.BeginInvoke(false, controller, callback, state);
        }

        public QueryBase EndExecute(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            QueryExecuteCaller ormActionDelegate = (QueryExecuteCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        #region -- Execute Methods --

        

        /// <summary>
        ///  Execute query and sets the duration that it can be cached.
        /// </summary>
        /// <param name="span">The duration.</param>
        /// <param name="slideTime">Whether the duration will be a slide time or fixed.</param>
        public void Execute(TimeSpan span, bool slideTime)
        {
            TimeController controller = new TimeController(span, slideTime);
            Execute(false, controller);
        }

        /// <summary>
        /// Execute query and sets a absolute expiration time.
        /// </summary>
        /// <param name="expirationTime">The date and time the result will expired from the cached.</param>
        public void Execute(DateTime expirationTime)
        {
            TimeController controller = new TimeController(expirationTime);
            Execute(false, controller);
        }

        /// <summary>
        /// Execute Query.
        /// </summary>
        /// <param name="realtime">if <see lang="true" /> queries the database each time, even when 
        /// the result could be on the cached; if <see lang="false" /> gets from the cache if exists in there.</param>
        public void Execute(bool realtime)
        {
            Execute(realtime, TimeController.Default);
        }

        /// <summary>
        /// Execute Query. It will return the query from the cache if exists in there.
        /// </summary>
        public void Execute()
        {
            Execute(false);
        }



        #endregion


        internal abstract void Execute(bool realtime, TimeController controller);

        public abstract string GetSql();


        private Criterias GetCriterias()
        {
            if (criterias == null) criterias = new Criterias(false);
            return criterias;
        }

        private Criterias GetAggregates()
        {
            if (aggregates == null) aggregates = new Criterias(true);
            return aggregates;
        }


    }

}
