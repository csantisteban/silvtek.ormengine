using System;

namespace Silvtek.ORMEngine.Encryption
{
    [Serializable]
    public abstract class RecordDigestProvider
    {
        public abstract string GenerateDigest(object[] values);
        public abstract DigestComparisonResult ValidateDigest(string digest, object[] values);
    }
}
