using System;

namespace Silvtek.ORMEngine
{

    ///<summary>
    /// Represent that database type.
    ///</summary>
	public abstract class DbTypeFormatter
	{

		 public abstract bool IsCaseSensitive {get;}

         ///<summary>
         /// Gets the database specific single quote escape character.
         ///</summary>
		 ///<value>The database specific single quote escape character.</value>
		 ///<remarks>
         /// <example>
         ///   In MySql single quote escape is <b>\'</b>,
         ///   this property must return return <b>\\\'</b> (C# format)
         /// </example>
         /// </remarks>
		 public abstract string SingleQuote {get;}

         public abstract string ConcatExpr {get;}

       
         ///<summary>
         /// Gets database specific formatted string for the current
         /// date and time
         ///</summary>
         ///<value>
         /// The database specific formatted string for the current
         /// date and time.
         ///</value>
        public abstract string Now { get; }

         ///<summary>
         /// Gets prefix specific formatted string for the current
         /// date.
         ///</summary>
         ///<value>
         /// The database specific formatted string for the current
         /// date.
         ///</value>
        public abstract string Today {get;}

        ///<summary>
        /// Gets prefix specific for the current database.
        ///</summary>
        ///<value>
        /// The prefix specific string for the current database.
        ///</value>
        public abstract string ParameterizedPrefix { get; }

        public abstract object DBNullValue { get;}

		public abstract string FormatDate(DateTime adate);

		public abstract string FormatDateTime(DateTime adatetime);

        public abstract string FormatGuid(Guid guid);

        public abstract string FormatBoolean(bool value);

        public abstract bool ToBoolean(string str);
        
        public abstract string FormatString(string str);

		public abstract string UCase(string str);

		public abstract string LCase(string str);

		public abstract string Trim(string str);

        public abstract string GetIdentityCommand();

        public abstract string GetNowCommand();

        public abstract string GetServerDateTimeCommand();

	}
}
