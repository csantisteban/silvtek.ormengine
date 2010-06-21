#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

namespace Silvtek.ORMEngine.MSSql
{
	using System;

    ///<summary>
    /// Represent an <see cref="DbTypeFormatter"/> for SqlServer databases.
    ///</summary>
    [Obsolete("Use MSSqlFormatter instead.")]
    public class MSSqlType : DbTypeFormatter
    {

        public MSSqlType()
        {
        }

        public override bool IsCaseSensitive
        {
            get { return false; }
        }

        public override object DBNullValue
        {
            get { return DBNull.Value; }
        }

        public override string ConcatExpr
        {
            get { return "+"; }
        }

        public override string Now
        {
            //get  {return "'"+DateTime.Now.ToString("u")+"'";}
            get { return FormatDateTime(DateTime.Now); }
        }

        public override string SingleQuote
        {
            get { return "''"; }
        }


        public override string Today
        {
            get
            {
                return GetDate(DateTime.Today);
            }
        }

        public override string ParameterizedPrefix
        {
            get { return "@"; }
        }

        public override string FormatDateTime(DateTime dt)
        {
            return GetDate(dt);
        }

        //{ d 'yyyy-mm-dd'}
        public override string FormatDate(DateTime dt)
        {
            return String.Format("{0} d '{1:yyyy-MM-dd} ' {2}", "{", dt, "}");
        }


        public override string FormatBoolean(bool value)
        {
            return (value) ? "1" : "0";
        }

        public override bool ToBoolean(string str)
        {
            if (str == null || str.Length == 0) return false;
            str = str.ToLower();
            return (str == "1" || str == "true");
        }


        public override string FormatGuid(Guid guid)
        {
            return String.Format("{0:N}", guid);
        }

        public override string FormatString(string sql)
        {
            if (sql == null) return String.Empty;
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

        //{ ts 'yyyy-mm-dd hh:mm:ss[.fff] '} 
        private string GetDate(DateTime dt)
        {
            //return String.Format("{0} ts '{1:s} '{2}", "{", dt, "}");
            return String.Format("{0:yyyy-MM-ddTHH:mm:ss.fff}", dt);
        }

        public override string GetIdentityCommand()
        {
            return "select @@identity";
        }

        public override string GetNowCommand()
        {
            return "GetDate()";
        }

        public override string GetServerDateTimeCommand()
        {
            return "select GetDate()";
        }
    }


}
