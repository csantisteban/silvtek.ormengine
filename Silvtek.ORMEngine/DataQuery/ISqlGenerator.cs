#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Summary description for ISqlGenerator.
	/// </summary>
	public interface ISqlGenerator
	{
        /// <summary>
        /// Gets or sets a value indicating whether this instance is query case sensitive.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is query case sensitive; otherwise, <c>false</c>.
        /// </value>
		bool IsQueryCaseSensitive {get; set;}

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <returns></returns>
		string GetSql();
	}
}
