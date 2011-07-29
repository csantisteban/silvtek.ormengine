using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Silvtek.ORMEngine.Data
{
    /// <summary>
    /// Represents a single match
    /// </summary>
    public class RetryExceptionMatch
    {
        private readonly string property;
        private string match;
        private int tries;
        private readonly PropertyInfo propertyInfo;
        private Type type;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RetryExceptionMatch(Type type, string property, string match, int tries)
        {
            this.type = type;
            this.property = property;
            this.match = (match == null) ? String.Empty : match;
            this.property = property;
            this.tries = tries;
            propertyInfo = FindPropertyInfo(type, property);
        }


        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>The property.</value>
        public string Property
        {
            get { return property; }
        }

        /// <summary>
        /// Gets or sets the match.
        /// </summary>
        /// <value>The match.</value>
        public string MatchValue
        {
            get { return match; }
        }

        /// <summary>
        /// Gets or sets the tries.
        /// </summary>
        /// <value>The tries.</value>
        public int Tries
        {
            get { return tries; }
            set { tries = value; }
        }

        public Type Type
        {
            get { return type; }
        }

        private PropertyInfo FindPropertyInfo(Type type, string property)
        {
            PropertyInfo info = type.GetProperty(property, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (info == null)
            {
                throw new MissingMemberException(property + " missing from class " + type.FullName);
            }

            return info;
        }

        internal bool ShouldRetry(object instance, int counter)
        {
            string value = GetValue(instance);
            Regex regex = new Regex(match);
            Match regmatch = regex.Match(value);
            bool success = regmatch.Success;
            if (success)
            {
                
                // bail out if the number of tries is set hight than 10.
                if (counter > this.tries || counter > 10)
                {
                    return false;
                }
            }

            return success;
        }

        private string GetValue(object instance)
        {
            object obj =  propertyInfo.GetValue(instance, null);
            if (obj == null) return String.Empty;

            return Convert.ToString(obj);
        }
    }
}
