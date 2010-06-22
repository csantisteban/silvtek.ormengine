#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using Silvtek.ORMEngine.DataQuery.Parsers;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Represents a single criteria item.
    /// </summary>
    public class CriteriaItem : ISqlGenerator
    {

        private Type type;
        private string memberName;
        private CriteriaParser parser;
        private Aggregate aggregate;
        private bool distinct;
        private bool isCaseSensitive;

        public bool IsQueryCaseSensitive
        {
            get { return isCaseSensitive; }
            set { isCaseSensitive = value; }
        }

        internal bool IsAggregate
        {
            get { return (aggregate != Aggregate.None); }
        }

        public CriteriaItem(Type type, string memberName)
        {
            this.type = type;
            this.memberName = memberName;
            this.parser = null;
            this.aggregate = Aggregate.None;
            this.distinct = false;
        }

        public CriteriaItem(Aggregate aggregate, bool distinct, Type type, string memberName)
        {
            this.type = type;
            this.memberName = memberName;
            this.parser = null;
            this.aggregate = aggregate;
            this.distinct = distinct;
        }

        public void Condition(Comparison comparison, object value)
        {
            parser = CriteriaParser.Create(type, memberName, comparison, value);
        }


        public void EqualTo(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.Equal, value);
        }

        public void NotEqualTo(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.NotEqual, value);
        }

        public void GreaterThan(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.GreaterThan, value);
        }

        public void GreaterEqualThan(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.GreaterEqualThan, value);
        }

        public void LessEqualThan(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.LessEqualThan, value);
        }

        public void LikeTo(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.Like, value);
        }

        public void LessThan(object value)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.LessThan, value);
        }

        public void Between(object value1, object value2)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.Between, value1, value2);
        }

        public void NotBetween(object value1, object value2)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.NotBetween, value1, value2);
        }

        public void Contains(ICollection collection)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.In, collection);
        }

        public void NotContains(ICollection collection)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.NotIn, collection);
        }

        [Obsolete]
        public void ContainIn(QueryBase query)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.In, query);
        }

        [Obsolete]
        public void NotContainIn(QueryBase query)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.NotIn, query);
        }

        public void ContainsIn(QueryBase query)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.In, query);
        }

        public void NotContainsIn(QueryBase query)
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.NotIn, query);
        }

        public void EqualNull()
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.IsNull, String.Empty);
        }

        public void NotEqualNull()
        {
            parser = CriteriaParser.Create(type, memberName, Comparison.IsNotNull, String.Empty);
        }


        #region ISqlGenerator Members

        public string GetSql()
        {
            string retVal = String.Empty;
            if (parser != null)
            {
                if (this.aggregate != Aggregate.None)
                {
                    parser.SetAggregate(this.aggregate, this.distinct);
                }

                parser.IsQueryCaseSensitive = this.IsQueryCaseSensitive;
                retVal = parser.GetSql();
            }

            return retVal;

        }

        #endregion
    }
}
