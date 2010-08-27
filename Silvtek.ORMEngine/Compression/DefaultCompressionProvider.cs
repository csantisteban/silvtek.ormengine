#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

#if NET20
using System;
using System.IO;
using System.IO.Compression;
using Silvtek.ORMEngine.Configs;

namespace Silvtek.ORMEngine.Compression
{

    public class DefaultCompressionProvider : CompressionProvider
    {
        public override Stream Decompress(Stream sourceStream)
        {
            string filename = String.Format("{0:n}.stream", Guid.NewGuid());
            string tmpFile = Path.Combine(ORMConfigurator.TemporaryFolder, filename);

            // now, create stream for compression;

            using (Stream destStream = new FileStream(tmpFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, ORMConfigurator.BufferSize,
                                               FileOptions.SequentialScan))
            {

                using (Stream zipStream = new GZipStream(sourceStream, CompressionMode.Decompress, false))
                {
                    // now, copy source stream to destStream
                    byte[] buffer = new byte[ORMConfigurator.BufferSize];

                    int bytesRead;
                    while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);
                        destStream.Flush();
                    }

                    zipStream.Close();
                }

                destStream.Close();
            }

            return new FileStream(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read, ORMConfigurator.BufferSize,
                                  FileOptions.SequentialScan);

        }

        public override Stream Compress(Stream sourceStream)
        {
            string filename = String.Format("{0:n}.stream", Guid.NewGuid());
            string tmpFile = Path.Combine(ORMConfigurator.TemporaryFolder, filename);

            // now, create stream for compression;

            using (Stream destStream = new FileStream(tmpFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, ORMConfigurator.BufferSize,
                                               FileOptions.SequentialScan))
            {

                using (Stream zipStream = new GZipStream(destStream, CompressionMode.Compress, false))
                {
                    // now, copy source stream to destStream
                    byte[] buffer = new byte[ORMConfigurator.BufferSize];

                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        zipStream.Write(buffer, 0, bytesRead);
                        zipStream.Flush();
                    }
                }
            }

            return new FileStream(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read, ORMConfigurator.BufferSize,
                                  FileOptions.SequentialScan);

        }

        //public static byte[] GetStreamAsByteArray(Stream stream)
        //{
        //    stream.Position = 0;
        //    int streamLength = Convert.ToInt32(stream.Length);
        //    byte[] bytes = new byte[streamLength];

        //    // Read the file into a byte array
        //    stream.Read(bytes, 0, streamLength);
        //    //stream.Close();

        //    return bytes;
        //}

        public override void Dispose()
        {
            //GC.SuppressFinalize(this);
        }
    }

}
#endif