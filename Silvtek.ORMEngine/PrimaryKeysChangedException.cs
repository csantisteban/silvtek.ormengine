using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Thrown when a primary key is modified.
	/// </summary>
	public class PrimaryKeysChangedException : DbDataException
	{

		public PrimaryKeysChangedException(string msg) : base(msg)
		{
		}
	}
}
