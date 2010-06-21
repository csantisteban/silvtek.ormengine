#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Description of MemberNotFoundException.
	/// </summary>
	public class MemberNotFoundException : ApplicationException
	{
		private string message;
		public override string Message
		{
			get {return message;}
		}
		
		
		public MemberNotFoundException(Type type, string member, string msg)
		{
			this.message = String.Format("The ORM field or property '{0}' has not been found for type {1} :: {2}", member, type.FullName, msg);	
		}
		
		public MemberNotFoundException(Type type, string member) : this(type, member, String.Empty){}
	}
}
