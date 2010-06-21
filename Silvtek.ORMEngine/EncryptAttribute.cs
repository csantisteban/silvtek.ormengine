using System;
using Silvtek.ORMEngine.Encryption;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Tells the ORM engine that the field or property tagged with this attribute be saved in the database compressed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class EncryptAttribute : Attribute
    {
        private Type provider;
        private readonly string password;
        private readonly string salt;

        //public EncryptAttribute()
        //{
        //    provider = null;
        //    password = "***REMOVED***"; // this one has to come from connection.config
        //    salt = "***REMOVED***"; // this has to come from connection.config
        //}

        public EncryptAttribute(string password, string salt)
        {
             provider = null;
            this.password = password;
            this.salt = salt;
        }

        public EncryptAttribute(Type encryptProviderType, string password, string salt) : this(encryptProviderType)
        {
            this.password = password;
            this.salt = salt;

        }

        public EncryptAttribute(Type encryptProviderType) 
        {
            if (encryptProviderType == null)
			{
				throw new ArgumentNullException("encryptProviderType");
			}

            provider = encryptProviderType;

			if (provider.BaseType != typeof(EncryptionProvider))
			{
			    string exMsg = String.Format("Invalid encryption provider. The encryption provider specified in EncryptAttribute must inherit EncryptProvider class");
			    throw new InvalidOperationException(exMsg);
			}
        }

        public EncryptionProvider GetProvider()
        {
            if (provider == null)
            {
                provider = typeof(DefaultEncryptionProvider);
            }

            EncryptionProvider returnValue = (EncryptionProvider)Activator.CreateInstance(provider, true);
            returnValue.Password = password;
            returnValue.Salt = salt;
            return returnValue;
        }

    }
}
