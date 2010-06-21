#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// ORM Error Event argument.
	/// </summary>
	public class ORMErrorEventArgs : EventArgs
	{
		private readonly Exception e;
		private readonly string sql;
		private readonly Action action;

		public Exception Exception
		{
			get {return e;}
		}

		public string Sql
		{
			get {return sql;}
		}

		public Action Action
		{
			get {return action;}
		}

		public ORMErrorEventArgs (Action action, Exception e,  string sql)
		{
			this.e = e;
			this.sql = sql;
			this.action = action;
		}  
	}
}
