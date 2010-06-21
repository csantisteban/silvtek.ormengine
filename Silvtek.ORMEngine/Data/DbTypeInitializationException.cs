#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.Data
{
	/// <summary>
	/// Description of DbTypeInitializationException.
	/// </summary>
    [Serializable]
	public sealed class DbTypeInitializationException : ApplicationException
	{
		private string message;
		private string typeName;
		
		public override string Message
		{
			get {return message; }
		}

		public string TypeName
		{
			get {return typeName;}
		}
			
		public DbTypeInitializationException(string msg, string fullTypeName, Exception e) : base(msg, e)
		{
			this.message = msg;
			this.typeName = fullTypeName;
		}
	}
}
