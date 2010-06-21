using System;
using System.IO;


namespace Silvtek.ORMEngine.Compression
{
    [Serializable]
    public abstract class CompressionProvider : IDisposable
    {
        protected CompressionProvider()
        {
        }

        public abstract Stream Decompress(Stream stream);
        public abstract Stream Compress(Stream sourceStream);
        public abstract void Dispose();
    }
}
