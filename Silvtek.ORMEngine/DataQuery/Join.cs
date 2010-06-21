#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using Silvtek.ORMEngine.Schema;
using Silvtek.ORMEngine.DataQuery.Parsers;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Represents a join between to ORM objects.
	/// </summary>
	public class Join	: ISqlGenerator
	{

		private ObjectSchemaInfo firstObject;      // First object.
        private ObjectSchemaInfo secondObject;     // Second object.
		private string memberName;              // The member name of the first object that will map the second object.
		private Criterias criteria;             // The collection of criterias for the join.
        private JoinType joinType;              // The join type.
		private bool isCaseSensitive;           // Whether the join is case sensitive or not.

		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

		internal Join(ObjectSchemaInfo objectInfo, string memberName)
		{
			this.firstObject = objectInfo;
			this.memberName = memberName;
			this.criteria = new Criterias(false);
		}


		public Criterias InnerJoin(Comparison comparison, Type type, string member)
		{
            return CreateJoin(JoinType.Inner, comparison, type, member);
		}

        public Criterias LeftJoin(Comparison comparison, Type type, string member)
        {
            return CreateJoin(JoinType.Left, comparison, type, member);
        }

        public Criterias OuterJoin(Comparison comparison, Type type, string member)
        {
            return CreateJoin(JoinType.Outer, comparison, type, member);
        }

        public Criterias RightJoin(Comparison comparison, Type type, string member)
        {
            return CreateJoin(JoinType.Right, comparison, type, member);
        }


        private Criterias CreateJoin(JoinType joinType, Comparison comparison, Type type, string member)
        {
            this.joinType = joinType;
            CriteriaItem criteriaItem = new CriteriaItem(firstObject.Type, memberName);
            secondObject = ORM.GetObjectInfo(type);

            string text = secondObject.MappedMembers.Find(member).ColumnFullName;
            SqlText sqlCriteria = new SqlText(text, false);
            criteriaItem.Condition(comparison, sqlCriteria);
            criteria.Add(criteriaItem);
            return criteria;
        }

		public string GetSql()
		{
            JoinParser parser = new JoinParser(joinType, secondObject);
			parser.SetCriteria(criteria);

			return parser.GetSql();
		}
	}
}
