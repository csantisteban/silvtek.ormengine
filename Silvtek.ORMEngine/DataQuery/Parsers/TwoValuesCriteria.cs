#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace  Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for ConditionTwoValues.
	/// </summary>
	internal class TwoValuesCriteria	: CriteriaParser
	{
		
		private const string TEMPLATE = " ({col} {between} {value1} and {value2}) ";

		private object value1;
		private object value2;


		public TwoValuesCriteria(ObjectSchemaInfo objectInfo, string memberName,  object value1, object value2) : base(objectInfo, memberName)
		{
			this.value1 = value1;
			this.value2 = value2;
		}

		internal override void SetComparison(Comparison comparison)
		{
			if (ComparisonHelper.IsBetween(comparison))
			{
				this.InternalSetComparison(comparison);
			}
			else
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>COMPARISON_NOT_FOR_TWO_VALUES", comparison);
				throw new QueryException(exmsg);
			}
		}

		public override string GetSql()
		{
			string columnName =	 this.GetColumnFullName();
			string comparisonValue = this.GetComparisonString();
			string formattedValue1 = this.GetValue(value1);
			string formattedValue2 = this.GetValue(value2);

			StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
			stringBuffer.SetTokenValue("col", columnName);
			stringBuffer.SetTokenValue("between", comparisonValue);
			stringBuffer.SetTokenValue("value1", formattedValue1);
			stringBuffer.SetTokenValue("value2", formattedValue2);

			return stringBuffer.ToString();
		}
	}
}
