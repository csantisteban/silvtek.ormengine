using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Thrown when a locked dataobject trys to update the datasource.
	/// </summary>
	[Serializable]
	public class LockORMException : ApplicationException
	{

		public LockORMException(string msg) : base(msg)
		{
		}
	}
}
