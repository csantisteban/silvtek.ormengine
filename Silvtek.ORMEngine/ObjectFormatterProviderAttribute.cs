using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Tells the ORM engine that the field or property tagged with this attribute be saved in the database compressed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class ObjectFormatterProviderAttribute : Attribute
    {
        private Type provider;

        internal ObjectFormatterProviderAttribute()
        {
            provider = null;
        }


        public ObjectFormatterProviderAttribute(Type type)
        {
            if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			
            Type typeInterface = provider.GetInterface("System.Runtime.Serialization.IFormatter");
            if (typeInterface == null)
			{
                string exMsg = String.Format("Invalid IFormatter provider. The does not implement the interface: System.Runtime.Serialization.IFormatter");
			    throw new InvalidOperationException(exMsg);
			}


            provider = type;
        }

        public IFormatter GetFormatter()
        {

            if (provider == null)
            {
                provider = typeof (BinaryFormatter);
            }

            IFormatter returnValue = (IFormatter)Activator.CreateInstance(provider, true);
            return returnValue;
        }



    }
}
