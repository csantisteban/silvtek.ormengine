using System;

namespace Silvtek.ORMEngine
{
    ///<summary>
    /// Defines the vendor provide type.
    ///</summary>
	public enum ProviderType
	{
		ODBC= 0,
		Oracle = 1,
		MySQL = 2,
		MSSql = 3,
		DB2 = 4,
	    PostGreSQL = 5,
        Undefined = 99
	}
}
