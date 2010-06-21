using System;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class RecordDigestAttribute : Attribute
    {
        private Type provider;

        public RecordDigestAttribute() //: base(false)
        {
            provider = null;
        }

        public RecordDigestAttribute(Type recordDigestProvider) //: base(false)
        {
            if (recordDigestProvider == null)
			{
                throw new ArgumentNullException("recordDigestProvider");
			}

            provider = recordDigestProvider;

			if (provider.BaseType != typeof(RecordDigestProvider))
			{
			    string exMsg = String.Format("Invalid record digest provider. The encryption provider specified in EncryptAttribute must inherit EncryptProvider class");
			    throw new InvalidOperationException(exMsg);
			}
        }

        public RecordDigestProvider GetProvider()
        {
            if (provider == null)
            {
                provider = typeof(DefaultRecordDigestProvider);
            }

            RecordDigestProvider returnValue = (RecordDigestProvider)Activator.CreateInstance(provider, true);
            return returnValue;
        }
    }
}
