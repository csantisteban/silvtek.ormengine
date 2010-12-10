#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Summary description for SqlTextCriteria.
	/// </summary>
    public class SqlTextCriteria : CriteriaParser
    {
        private const string TEMPLATE = " ({col} {comparison} {value}) ";
        private object value;

        internal SqlTextCriteria(ObjectSchemaInfo objectInfo, string memberName, CommandText text) : base(objectInfo, memberName)
        {
            this.value = text.Text;
            InternalSetComparison(Comparison.Equal);
        }


        internal override void SetComparison(Comparison comparison)
        {
            this.InternalSetComparison(comparison);
        }

        public override string GetSql()
        {
            string columnName = this.GetColumnFullName();
            string comparisonValue = this.GetComparisonString();
            string formattedValue = String.Empty;

            StringBuffer stringBuffer = new StringBuffer(TEMPLATE);
            stringBuffer.SetTokenValue("col", columnName);
            stringBuffer.SetTokenValue("comparison", comparisonValue);
            stringBuffer.SetTokenValue("value", value);

            return stringBuffer.ToString();
        }

    }
}
