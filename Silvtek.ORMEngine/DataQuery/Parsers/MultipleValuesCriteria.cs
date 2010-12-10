#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Text;
using System.Collections;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for ConditionMultipleValues.
	/// </summary>
	internal class MultipleValuesCriteria : CriteriaParser
	{
		private const string _TEMPLATE = "({col} {in} ({values}) ) ";
		private readonly ICollection collection;

		internal MultipleValuesCriteria(ObjectSchemaInfo objectInfo, string memberName,  ICollection collection) : base(objectInfo, memberName)
		{
			this.collection = collection;
		}

		internal override void SetComparison(Comparison comparison)
		{
			if (ComparisonHelper.IsCollections(comparison))
			{
				this.InternalSetComparison(comparison);
			}
			else
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>COMPARISON_NOT_FOR_MULTIPLE_VALUES", comparison);
				throw new QueryException(exmsg);
			}
		}

		public override string GetSql()
		{
			string columnName =	 this.GetColumnFullName();
			string comparisonValue = this.GetComparisonString();
			string values = GetCommaDelimitedValues();

			StringBuffer stringBuffer = new StringBuffer(_TEMPLATE);
			stringBuffer.SetTokenValue("col", columnName);
			stringBuffer.SetTokenValue("in", comparisonValue);
			stringBuffer.SetTokenValue("values", values);

			return stringBuffer.ToString();
		}


		private string GetCommaDelimitedValues()
		{
			
			//return empty string if is null or does not contain values.
			if (collection == null || collection.Count == 0)
			{
				string exmsg = Utility.GetResourceString("Querying:EXMSG>EMPTY_COLLECTION");
				throw new QueryException(exmsg);
			}

			int aproxChars = collection.Count * 20;

			StringBuffer valuesTemplate = new StringBuffer("{comma} {value}");

			valuesTemplate.SetTokenValue("comma", String.Empty);
			valuesTemplate.SetTokenValue("value", String.Empty);
			StringBuilder delimitedValues = new StringBuilder(aproxChars);

			int tempCount = 0;

			foreach(object obj in collection)
			{
				string formattedValue = this.GetValue(obj);
				valuesTemplate.SetTokenValue("value", formattedValue);
				delimitedValues.Append(valuesTemplate.ToString());
				if (tempCount == 0)valuesTemplate.SetTokenValue("comma", ",");
			}

			return delimitedValues.ToString();

		}
	}
}
