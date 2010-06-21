using System;
using System.Collections;

namespace Silvtek.ORMEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class AliasAttribute : Attribute
    {

        private ArrayList aliases;
        public AliasAttribute(string alias)
		{
            if (alias == null || alias.Length == 0)
            {
                throw new ORMException("QueryColumnAttribute argument cannot be null or empty string");
            }

            aliases = new ArrayList();
            aliases.Add(alias);
		}

        public AliasAttribute(params string[] aliasParams)
        {
            if (aliasParams == null || aliasParams.Length == 0)
            {
                throw new ORMException("QueryColumnAttribute argument cannot be null or empty string");
            }

            aliases = new ArrayList();
            aliases.AddRange(aliasParams);
        }

        internal string[] GetAliases()
        {
           return (string[])aliases.ToArray(typeof(String));
        }
    }
}
