//using System;
//using Silvtek.ORMEngine.DataQuery;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine
//{
//    public sealed class QueryProxy<T>
//    {
//        private ResultSource lastResultSource;

//        public ResultSource LastResultSource
//        {
//            get { return lastResultSource; }
//        }

//        public T[] FindOrdered(string memberName, object searchValue, string orderByMemberName, OrderDirection order)
//        {
//            Query query = CreateQueryForFind(memberName, searchValue);
//            query.OrderBy(typeof(T), memberName, order);
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));
//        }

//        public T[] Find(string memberName, object searchValue)
//        {
//            return Find(memberName, searchValue, false);
//        }

//        public virtual T[] Find(string memberName, object searchValue, TimeSpan cacheTime, bool slideTime)
//        {
//            if (searchValue == null) return new T[0];

//            Query query = CreateQueryForFind(memberName, searchValue);
//            query.Execute(cacheTime, slideTime);
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));
//        }

//        public T[] Find(string memberName, object searchValue, bool realtime)
//        {
//            if (searchValue == null) return new T[0];

//            Query query = CreateQueryForFind(memberName, searchValue);
//            query.Execute(realtime);
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));

//        }

//        private Query CreateQueryForFind(string memberName, object value)
//        {
//            Query query = new Query(typeof(T));

//            CriteriaItem criteria = new CriteriaItem(typeof(T), memberName);
//            criteria.EqualTo(value);
//            query.Criterias.Add(criteria);
//            return query;
//        }

//        public T[] FindByColumnName(string columnName, object value)
//        {
//            return FindByColumnName(columnName, value, false);
//        }

//        public T[] FindByColumnName(string columnName, object value, bool realtime)
//        {
//            if (value == null) return new T[0];
//            ObjectSchemaInfo info = ORM.GetObjectInfo(typeof(T));
//            MapMember member = info.MappedMembers.FindByColumnName(columnName);
//            Query query = new Query(typeof(T));

//            CriteriaItem criteria = new CriteriaItem(typeof(T), member.MemberName);
//            criteria.EqualTo(value);

//            query.Criterias.Add(criteria);

//            query.Execute(realtime);
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));
//        }

//        /// <summary>
//        /// Returns all the records for this type of services
//        /// </summary>
//        /// <returns></returns>
//        public T[] GetAll(bool realtime)
//        {
//            Query query = new Query(typeof(T));
//            query.Execute();
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));
//        }

//        public T[] GetAll()
//        {
//            return GetAll(false);
//        }

//        public T[] GetAllOrdered(string orderByMember, OrderDirection direction)
//        {
//            return GetAllOrdered(orderByMember, direction, false);
//        }

//        public T[] GetAllOrdered(string orderByMember, OrderDirection direction, bool realtime)
//        {
//            Query query = new Query(typeof(T));
//            query.OrderBy(typeof(T), orderByMember, direction);
//            query.Execute(realtime);
//            lastResultSource = (query.IsResultsSetFromCache) ? ResultSource.Cache : ResultSource.Database;
//            return (T[])query.ResultSet.GetColumn(0).ToArray(typeof(T));
//        }

//    }
//}
