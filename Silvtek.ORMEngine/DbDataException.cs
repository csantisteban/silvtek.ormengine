using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Thrown when performing a database operation that fails.
	/// </summary>
	[Serializable]
	public class DbDataException : ApplicationException
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="e">Exception</param>
		public DbDataException(Exception e) : base(e.Message, e)
		{}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="msg">MEssage</param>
        public DbDataException(string msg, Exception e) : base (msg, e)
        {}

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="msg">MEssage</param>
        public DbDataException(string msg) : base(msg) {}
	}
}
