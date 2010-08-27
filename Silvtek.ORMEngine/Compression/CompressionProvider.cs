#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

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
