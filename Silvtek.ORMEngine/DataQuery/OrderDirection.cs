
#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System.Xml.Serialization;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Defines the order direction of a query.
    /// </summary>
    public enum OrderDirection : byte
    {
        /// <summary>
        /// Order descending.
        /// </summary>
        Descending = 1,

        /// <summary>
        /// Order ascending.
        /// </summary>
        Ascending = 8,

        /// <summary>
        /// Uses Database default.
        /// </summary>
        Default = 16
    }

}
