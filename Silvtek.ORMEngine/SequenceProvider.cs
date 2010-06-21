using System;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
	[Serializable]
	public abstract class SequenceProvider
	{
		protected SequenceProvider()
		{
		}
		
		public abstract object GetNextNumber(object sender, MapMember info);
	}
}

