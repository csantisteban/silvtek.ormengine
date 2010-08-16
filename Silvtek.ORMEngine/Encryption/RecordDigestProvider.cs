using System;

namespace Silvtek.ORMEngine.Encryption
{
    /// <summary>
    /// Provided for creating the record digest.
    /// </summary>
    [Serializable]
    public abstract class RecordDigestProvider
    {
        /// <summary>
        /// Generates the digest.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public abstract string GenerateDigest(object[] values);

        /// <summary>
        /// Validates the digest.
        /// </summary>
        /// <param name="digest">The digest.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public abstract DigestComparisonResult ValidateDigest(string digest, object[] values);
    }
}
