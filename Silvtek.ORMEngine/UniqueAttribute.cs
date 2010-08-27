using System;

namespace Silvtek.ORMEngine
{
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public sealed class UniqueAttribute : Attribute
	{
       // public const bool IsUnique = true;

		public UniqueAttribute()
		{
		}
	}
}
