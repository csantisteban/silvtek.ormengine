using System;
using System.Xml.Serialization;

namespace Silvtek.ORMEngine.DataQuery
{
    [SoapType("SimpleFindCriteria", IncludeInSchema=true)]
    public sealed class SimpleFindCriteria
    {

        private string memberName;
        private string value;
        private string sortBy;
        private OrderDirection orderDirection;

        [SoapElement("Field", IsNullable = true)]
        [XmlElement("Field", IsNullable=true)]
        public string MemberName
        {
            get { return memberName; }
            set { memberName = (value == null) ? String.Empty : value; }
        }

        [SoapElement("Value", IsNullable = true)]
        [XmlElement("Value", IsNullable = true)]
        public string Value
        {
            get { return value; }
            set { this.value = (value == null) ? String.Empty : value; }
        }

        [SoapElement("OrderBy", IsNullable = true)]
        [XmlElement("OrderBy", IsNullable = true)]
        public string OrderBy
        {
            get { return sortBy; }
            set { sortBy = (value == null) ? String.Empty : value;  }
        }

        public OrderDirection OrderDirection
        {
            get { return orderDirection; }
            set { orderDirection = value; }
        }

        public SimpleFindCriteria()
        {
            memberName = String.Empty;
            value = String.Empty;
            sortBy = String.Empty;
            orderDirection = OrderDirection.Default;

        }

    }
}