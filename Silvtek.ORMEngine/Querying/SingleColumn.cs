//using System;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Summary description for Class.
//    /// </summary>
//    public class SingleMember
//    {
//        private Type type;
//        private string property;
//        private Aggregate aggregate;
//        private bool aggregateDistinct;
//        private string value;

//        internal Aggregate Aggregate
//        {
//          get {return aggregate;}
//        }

//        internal string Value
//        {
//          get {return value;}
//        }


//        internal SingleMember(Aggregate aggregate, Type type, string property, bool aggr_distinct)
//        {

//            this.aggregate = aggregate;
//            this.type = type;
//            this.property = property;
//            this.aggregateDistinct = aggr_distinct;

//            value = QueryHelper.GetColumnFullName(type, property);

//            ObjectSchemaInfo info = ORM.GetObjectInfo(type);

//            if (aggregate != Aggregate.None)
//            {
//                value = QueryHelper.ParseAggregate(aggregate, value, aggregateDistinct, info.DbTypeFormatter, false);
//            }
//        }

//        internal SingleMember(Type type, string property, bool aggr_distinct) :
//                         this (Aggregate.None, type, property, aggr_distinct)
//        {    }


//    }
//}
