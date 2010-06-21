#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Text;
using Silvtek.ORMEngine.Schema;


namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Summary description for Join.
	/// </summary>
	internal class Joins : CollectionBase, ISqlGenerator
	{
		
		private bool isCaseSensitive;

		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

		internal  Joins()
		{
		}


		internal void Add(Join join)
		{
			InnerList.Add(join);
		}

		public string GetSql()
		{
			StringBuilder builder = new StringBuilder(this.Count * 100);
			foreach(ISqlGenerator sqlGenerator in InnerList)
			{
				builder.Append(sqlGenerator.GetSql());
			}

			return builder.ToString();
		}
		
	}
}
