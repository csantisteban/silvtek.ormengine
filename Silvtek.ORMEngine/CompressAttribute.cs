using System;
using Silvtek.ORMEngine.Compression;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Tells the ORM engine that the field or property tagged with this attribute be saved in the database compressed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class CompressAttribute : Attribute
    {
        private Type provider;

#if NET20
        public CompressAttribute()
        {
            provider = null;
        }
#endif

        public CompressAttribute(Type type)
        {
            if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			
			if (provider.BaseType != typeof(CompressionProvider))
			{
			    string exMsg = String.Format("Invalid compresion provider. The compression provider specified in CompressAttribute must inherit CompressionProvider class");
			    throw new InvalidOperationException(exMsg);
			}


            provider = type;
        }

        public CompressionProvider GetProvider()
        {

            if (provider == null)
            {
#if NET20
                provider = typeof (DefaultCompressionProvider);
#endif
#if NET11
        return null;
#endif
            }

            CompressionProvider returnValue = (CompressionProvider)Activator.CreateInstance(provider, true);
            return returnValue;
        }



    }
}
