using System;
using System.Collections;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Defines a different alias from which we can reference the current map.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class AliasAttribute : Attribute
    {

        private ArrayList aliases;


        /// <summary>
        /// Initializes a new instance of the <see cref="AliasAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public AliasAttribute(string alias)
		{
            if (alias == null || alias.Length == 0)
            {
                throw new ORMException("QueryColumnAttribute argument cannot be null or empty string");
            }

            aliases = new ArrayList();
            aliases.Add(alias);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasAttribute"/> class.
        /// </summary>
        /// <param name="aliasParams">The alias params.</param>
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
