#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace  Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for CriteriaConditionSingleValue.
	/// </summary>
	internal class SingleValueCriteria : CriteriaParser
	{
		private const string TEMPLATE = " ({col} {comparison} {value}) ";
		private object			value;	

		internal SingleValueCriteria(ObjectSchemaInfo objectInfo, string memberName, Comparison comparison,  object value)	: 
				 base(objectInfo, memberName)
		{
			this.value = value;
            InternalSetComparison(comparison);
		}

		internal override void SetComparison(Comparison comparison)
		{
			if (ComparisonHelper.IsSingle(comparison))
			{
				this.InternalSetComparison(comparison);
			}
			else
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>COMPARISON_NOT_FOR_SINGLE_VALUE", comparison);
				throw new QueryException(exmsg);
			}
		}


		public override string GetSql()
		{
			string columnName =	 this.GetColumnFullName();
			string comparisonValue = this.GetComparisonString();
			string formattedValue = String.Empty;
			
			if (!ComparisonHelper.IsNull(Comparison))
			{
				formattedValue = this.GetValue(value);	
			}
			
			StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
			stringBuffer.SetTokenValue("col", columnName);
		   	stringBuffer.SetTokenValue("comparison", comparisonValue);
			stringBuffer.SetTokenValue("value", formattedValue);

			return stringBuffer.ToString();
		}
	}
}
