#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for JoinParser.
	/// </summary>
	public class JoinParser	: ISqlGenerator
	{
		private const string TEMPLATE = " {join} join {table} on ({criteria})";

		private Criterias criteria;
		private JoinType joinType;
        private ObjectSchemaInfo objectInfo;
        private bool isCaseSensitive;

		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

        internal JoinParser(JoinType joinType, ObjectSchemaInfo objectInfo)
		{
            this.objectInfo = objectInfo;
            this.joinType = joinType;
			this.criteria = null;
		}
   
		internal void SetCriteria(Criterias criteria)
		{
			this.criteria = criteria;
		}

		public string GetSql()
		{						
			string joinValue = GetJoinValue(joinType);
            string tableValue = objectInfo.TableName;

			criteria.IsQueryCaseSensitive = this.IsQueryCaseSensitive;
			string criteriaValue = criteria.GetSql();

			StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
			stringBuffer.SetTokenValue("join", joinValue);
			stringBuffer.SetTokenValue("table", tableValue);
			stringBuffer.SetTokenValue("criteria", criteriaValue);


			return stringBuffer.ToString();
		}


		private string GetJoinValue(JoinType type)
		{
			
			string val = String.Empty;
			switch(type)
			{
				case JoinType.Inner:
					val = "inner";
					break;
             
				case JoinType.Outer:
					val = "outer";
					break;
             
				case JoinType.Left:
					val = "left";
					break;
             
				case JoinType.Right:
					val = "right";
					break;
				default:
					break;
			}

			return val;
		}

		
	}
}
