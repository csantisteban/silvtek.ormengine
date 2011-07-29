#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using Silvtek.ORMEngine;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.NativeSchema;
using Silvtek.ORMEngine.Caching;


namespace Silvtek.ORMEngine.MSSql
{
	/// <summary>
	/// Summary description for Class.
	/// </summary>
	public class MSSqlProvider : ConnectionProvider
	{
        ///Constructor.
		public MSSqlProvider()
		{
		}



        ///<summary>Gets the assembly name for Microsoft.Data.SqlClient </summary>
        ///<value>The assembly name for Microsoft.Data.SqlClient</value>
        ///<remarks>The value is <b>Microsoft.Data.SqlClient</b></remarks>
	    public override string ProviderAssembly
	    {
          get  {return "Microsoft.Data.SqlClient";}
	    }

        ///<summary>
        /// Gets the <see cref="ProviderType" /> for a ODBC Connection.
        ///</summary>
        ///<value> The <see cref="ProviderType" /> for a ODBC Connection.</value>
        ///<remarks>The value is <see cref="ProviderType.ODBC" />.</remarks>
	    public override ProviderType Type
	    {
            get
            {
	    	  return ProviderType.MSSql;
            }
	    }

        ///<summary>
        /// Provides a ODBC Connection.
        ///</summary>
        ///<returns>
        /// a <see cref="IDbConnection" /> for a Sql Connection
        ///</returns>
		public override IDbConnection GetConnection()
		{
		   SqlConnection cnn = new SqlConnection();
		   return (IDbConnection)cnn;
		}

        ///<summary>
        /// Provides a <see cref="IDbCommand" /> for a ODBC Connection.
        ///</summary>
        ///<returns>
        /// a <see cref="IDbCommand" /> for a ODBC Connection.
        ///</returns>
	    public override IDbCommand GetCommand()
	    {
	    	 SqlCommand cmd = new SqlCommand();
	    	 return (IDbCommand)cmd;
	    }

		public override TableSchema GetTableSchema(string tableName, string connectionName)
		{
			string key = String.Format("6BDD4784:{0}:{1}", tableName, connectionName);


			if (QueryCache.Current.Exists(key))
			{
				return (TableSchema)QueryCache.Current.GetObject(key);
			}
			else
			{
				TableSchema schema = MSSqlTableSchema.GetSchema(tableName, connectionName);
				
				if (schema != null)
				{
                    TimeController controller = new TimeController(new TimeSpan(0, 0, 20, 0, 0), true);
					QueryCache.Current.AddItem(key, schema, typeof(TableSchema), controller);
				}

				return schema;
			}

			
		}

	}
}
