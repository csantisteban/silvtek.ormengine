#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Represents a exception when forming a query thru the
    /// <see cref="Query"/> object.
	/// </summary>
	[Serializable]
	public class QueryException : ApplicationException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="msg"></param>
		public QueryException(string msg) : base(msg)
		{
		}

		public QueryException(string msg, Exception inner) : base(msg, inner) {}
	}
}
