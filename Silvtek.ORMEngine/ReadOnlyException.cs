using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Thrown when a read only dataobject is intented to update database.
	/// </summary>
	public class ReadOnlyException : DbDataException
	{

		public ReadOnlyException(string msg) : base(msg)
		{
		}
	}
}
