using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Data Object Exception.
	/// </summary>
	[Serializable]
	public class ORMException : ApplicationException
	{
	    private string sql;

	    public string LastSql
	    {
            get { return sql; }
	    }

        public ORMException(string sql, string msg) : base(msg)
        {
            this.sql = sql;
        }

		public ORMException(string msg) : base (msg)
		{
		    sql = String.Empty;

		}

		public ORMException(string msg, Exception innerExp) : base(msg, innerExp)
		{
		    sql = String.Empty;

		}

        public ORMException(string msg, string sql, Exception innerExp)
            : base(msg, innerExp)
        {
            this.sql = sql;

        }
	}
}
