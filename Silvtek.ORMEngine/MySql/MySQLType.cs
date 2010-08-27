#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

namespace Silvtek.ORMEngine.MySql
{
	using System;

    ///<summary>
    /// Represent an <see cref="DbTypeFormatter"/> for MySQL databases.
    ///</summary>
	public class MySQLType : DbTypeFormatter
	{

		public MySQLType()
		{
		}
        

		public override bool IsCaseSensitive
		{
			get {return false;}
		}

        public override object DBNullValue
        {
            get { return DBNull.Value; }
        }

		public override string ConcatExpr
		{
           get {return "+";}
		}

		public override string Now
		{
          get  {return "'"+DateTime.Now.ToString("u")+"'";}
		}

 		public override string  SingleQuote
		{
           get {return "\\\'";}
		}

        
		public override string Today
		{
          get
          {
		    return DateTime.Today.ToString("u");
          }
		}

		public override string FormatDateTime(DateTime adatetime)
		{
			return adatetime.ToString("u");
		}

		public override string FormatDate(System.DateTime adate)
		{
			string strDate = adate.ToString("yyyy-MM-dd");
			return strDate;
		}


        public override string FormatBoolean(bool value)
        {
           return (value)? "1" : "0";
        }

        public override bool ToBoolean(string str)
        {
            return (str == "1");
        }
        
        public override string FormatGuid(Guid guid)
        {
           return String.Format("{0:N}", guid);
        }
        
		public override string FormatString(string sql)
		{
          if(sql == null)return String.Empty;
		  return sql.Replace("'", SingleQuote);
		}

		public override string UCase(string str)
		{
		    return String.Format("UPPER({0})", str);
		}

		public override string LCase(string str)
		{
		    return String.Format("LOWER({0})", str);
		}

		public override string Trim(string str)
		{
		    return String.Format("TRIM({0})", str);
		}

        public override string GetIdentityCommand()
        {
            return "select LAST_INSERT_ID()";   
        }

        public override string GetNowCommand()
        {
            return "Now()";
        }

        public override string GetServerDateTimeCommand()
        {
            return "select Now()";
        }

        public override string ParameterizedPrefix
        {
            get { return "@"; }
        }

        public override string BeginTransactionCommand()
        {
            return "START TRANSACTION;";
        }

        public override string CommitTransactionCommand()
        {
            return "COMMIT;";
        }

        public override string RollbackTranscationCommand()
        {
            return "ROLLBACK;";
        }
    }
}

