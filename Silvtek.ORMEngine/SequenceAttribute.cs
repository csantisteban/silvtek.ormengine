using System;
using System.Reflection;

namespace Silvtek.ORMEngine
{
   ///<summary>
   /// Marks a field or property as a sequence attribute. 
   ///</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,   Inherited = true)]
	public sealed class SequenceAttribute : Attribute
	{
		
		private readonly Type sequenceProvider;
		
		/// <summary>
		/// Gets the sequence provider.
		/// </summary>
		public Type ProviderType
		{
			get {return sequenceProvider;}
		}
		
		public SequenceAttribute(Type sequenceProviderType)
		{
			if (sequenceProviderType == null)
			{
				throw new ArgumentNullException("type");
			}
			
			if (sequenceProviderType.BaseType != typeof(SequenceProvider))
			{
				string exMsg = Utility.GetResourceString("EXMSG:SequenceAttribute:INVALID_TYPE", sequenceProviderType.FullName);
			    throw new InvalidOperationException(exMsg);
			}


			sequenceProvider = sequenceProviderType;
	 }

		
		public SequenceProvider GetProvider()
		{		
			SequenceProvider returnValue = (SequenceProvider)Activator.CreateInstance(sequenceProvider, true);	
			return returnValue;				
		}
	}
	
}
