#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System.Collections.Specialized;
using System.Diagnostics;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Maps custom alias to a specific instance member
    /// </summary>
    internal class AliasMaps : StringDictionary
    {
        internal AliasMaps()
        {
        }

        internal void AddRange(string[] aliases, string memberName)
        {
            foreach(string str in aliases)
            {
                Add(str, memberName);
            }
        }

        internal new void Add(string queryName, string memberName)
        {
            if (queryName == null || queryName.Length == 0) return;
            if (memberName == null || memberName.Length == 0) return;
           // Debug.Assert((queryName != null && queryName.Length > 0), "queryName is null or empty string");
           // Debug.Assert((memberName != null && memberName.Length > 0), "queryName is null or empty string");

            if (base.ContainsKey(queryName)) return;

            //Debug.WriteLine("Add query alias "+queryName+" for member name: "+memberName);
            base.Add(queryName, memberName);
        }

        internal string GetMemberName(string queryName)
        {
            return base[queryName];
        }
    }
}
