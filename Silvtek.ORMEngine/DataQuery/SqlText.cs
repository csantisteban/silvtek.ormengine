#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
	
	public class SqlText
	{
		private string text;
		private bool isAggregate;

		public string Text
		{
			get {return text;}
		}

		public bool IsAggregate
		{
			get {return isAggregate;}
		}

		public SqlText(string text, bool isAggregate)
		{
			this.isAggregate = isAggregate;
			this.text = text;
		}

		public static SqlText Create(string text, params object[] args)
		{
			string cmd = String.Format(text, args);
			return new SqlText(text, false);
		}

		public static SqlText CreateAggregate(string text, params object[] args)
		{
			string cmd = String.Format(text, args);
			return new SqlText(text, true);
		}
	}
}
