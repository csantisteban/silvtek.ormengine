using System;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class UniqueConstraintViolatedException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueConstraintViolatedException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public UniqueConstraintViolatedException(string msg) : base(msg)
        {
            
        }
    }
}
