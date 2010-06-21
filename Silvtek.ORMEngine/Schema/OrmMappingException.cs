#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Text;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Represents an exception while mapping object. 
    /// </summary>
    [Serializable]
    public class OrmMappingException : ApplicationException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public OrmMappingException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrmMappingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
