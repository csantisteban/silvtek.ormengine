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
		bool IsQueryCaseSensitive {get; set;}
		string GetSql();
	}
}
