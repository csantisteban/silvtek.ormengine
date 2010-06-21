using System;

namespace Silvtek.ORMEngine
{
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public sealed class NullAttribute : Attribute
	{
		public NullAttribute()
		{
		}
	}
}
