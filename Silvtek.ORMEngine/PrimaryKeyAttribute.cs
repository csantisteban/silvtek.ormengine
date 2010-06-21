using System;

namespace Silvtek.ORMEngine
{
   ///<summary>
   /// Primary Key Attribute.
   ///</summary>
   [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public sealed class PrimaryKeyAttribute : Attribute
	{
		public PrimaryKeyAttribute()
		{
		}
	}
}
