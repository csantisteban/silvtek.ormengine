
#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Text;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Description of QueriedAggregates.
	/// </summary>
	internal class QueriedAggregates :  CollectionBase, ISqlGenerator
	{
	
		private const string TEMPLATE = "{cmd}({distinct} {col})";
		
		private bool isCaseSensitive;

		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

		internal QueriedAggregates()
		{
			
		}
		
		internal void Add(Aggregate aggregate, Type type, string member, bool isDistinct)
		{
			string valueToAdd = String.Empty;
			if (aggregate == Aggregate.CountAll || aggregate == Aggregate.None)
			{
				valueToAdd = ParseAggregate(aggregate);
				
			}
			else
			{
				StringBuffer buffer = new StringBuffer(TEMPLATE);
			
				ObjectSchemaInfo info = ORM.GetObjectInfo(type);
				MapMember MappedMember = info.MappedMembers.Find(member);
				if (MappedMember == null)
				{
					throw new MemberNotFoundException(type, member, String.Empty);
				}
			
				string distinctKeyword = (isDistinct) ? "distinct" : String.Empty;
				string columnName = String.Format("{0}.{1}", info.TableName, MappedMember.ColumnName);
				string cmdAggregate = ParseAggregate(aggregate);
				
				buffer.SetTokenValue("cmd", cmdAggregate);
				buffer.SetTokenValue("distinct", distinctKeyword);
				buffer.SetTokenValue("col", columnName);
				
				valueToAdd = buffer.ToString();
			}

			InnerList.Add(valueToAdd);
		}
		
		
		private string ParseAggregate(Aggregate aggregate)
		{
			string retValue = String.Empty;
			switch(aggregate)
			{
				case Aggregate.Avg:
					retValue = "avg";
					break;
				case Aggregate.Count:
					retValue = "count";
					break;
				case Aggregate.Max:
					retValue = "max";
					break;
				case Aggregate.Min:
					retValue = "min";
					break;
				case Aggregate.Sum:
					retValue = "sum";
					break;
				case Aggregate.CountAll:
					retValue = "count(*)";
					break;
				case Aggregate.None:
					retValue = String.Empty;
					break;
				default:
					retValue = String.Empty;
					break;
			}

			return retValue;
		}
		
		public string GetSql()
		{
            if (InnerList.Count == 0) return String.Empty;
            string[] tempSqls = (String[])InnerList.ToArray(typeof(String));
            return String.Join(",", tempSqls);
        }
	}
}
