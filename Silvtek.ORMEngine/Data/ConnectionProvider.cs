#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System.Data;
using Silvtek.ORMEngine.NativeSchema;

namespace Silvtek.ORMEngine.Data
{
    
	public abstract class ConnectionProvider
	{

        public abstract ProviderType Type
        {
            get;
        }

        public abstract string ProviderAssembly
        {
            get;
        }

		/// <summary>
		/// Returns a connection from the provider.
		/// </summary>
	    public abstract IDbConnection GetConnection();
	    
		/// <summary>
		/// Returns a command from the provider.
		/// </summary>
		/// <returns></returns>
		public abstract IDbCommand GetCommand();

		/// <summary>
		/// Return the table schema as defined nativaly in the database.
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="connectionName">The connection name</param>
		/// <returns></returns>
		public abstract TableSchema GetTableSchema(string tableName, string connectionName);
	}
}
