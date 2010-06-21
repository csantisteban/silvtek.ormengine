 using System;
using System.Data;
using MySql.Data.MySqlClient;
using Silvtek.ORMEngine;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.NativeSchema;
using Silvtek.ORMEngine.Caching;


namespace Silvtek.ORMEngine.MySql
{
	/// <summary>
	/// Summary description for Class.
	/// </summary>
	public class MySqlProvider : ConnectionProvider
	{
        ///Constructor.
        public MySqlProvider()
		{
		}



        ///<summary>Gets the assembly name for Microsoft.Data.SqlClient </summary>
        ///<value>The assembly name for Microsoft.Data.SqlClient</value>
        ///<remarks>The value is <b>Microsoft.Data.SqlClient</b></remarks>
	    public override string ProviderAssembly
	    {
          get  {return "MySql.Data";}
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
                return ProviderType.MySQL;
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
            MySqlConnection cnn = new MySqlConnection();
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
	    	 MySqlCommand cmd = new MySqlCommand();
	    	 return (IDbCommand)cmd;
	    }

		public override TableSchema GetTableSchema(string tableName, string connectionName)
		{
            throw new NotImplementedException("MySql GetTableSchema has not been implemented for MySql");
		}

	
	}
}
