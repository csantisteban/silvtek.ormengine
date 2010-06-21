using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Summary description for QueryColumnAttribute.
	/// </summary>
	/// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,  Inherited = true)]
	public sealed class QueryColumnAttribute : Attribute
	{
        private readonly string memberName;

        public string MemberName
        {
            get {return memberName;}
        }

		public QueryColumnAttribute(string memberName)
		{
            if (memberName == null || memberName.Length == 0)
            {
                throw new ORMException("QueryColumnAttribute argument cannot be null or empty string");
            }

            this.memberName = memberName;
		}
	}
}
