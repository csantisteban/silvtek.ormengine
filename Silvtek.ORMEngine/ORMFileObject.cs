/*** TEMPORARLY DISABLED --- SEE IF IT IS NEED IT FOR NEXT RELEASE -- ***/

//using System;
//using System.Diagnostics;

//using System.IO;
//using Silvtek.ORMEngine.Configs;

//namespace Silvtek.ORMEngine
//{
//    /// <summary>
//    /// Represents a data stream
//    /// </summary>
//    public class ORMStream : Stream, IDisposable
//    {
//        private Stream backbone = null;
//        private readonly string backBoneFile;

//        /// <summary>
//        /// Gets the file path.
//        /// </summary>
//        /// <value>The file path.</value>
//        internal string FilePath
//        {
//            get { return backBoneFile; }
//        }

//        /// <summary>
//        /// Gets the back bone.
//        /// </summary>
//        /// <value>The back bone.</value>
//        private Stream BackBone
//        {
//            get
//            {
//                if (backbone == null)
//                {
//                    backbone = new FileStream(backBoneFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, ORMConfigurator.BufferSize);
//                }
               

//                return backbone;
//            }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="DataStream"/> class.
//        /// </summary>
//        public ORMStream()
//        {
//            string tempFileName = String.Format("{0}.stream", Guid.NewGuid().ToString("N"));
//            backBoneFile = Path.Combine(ORMConfigurator.TemporaryFolder, tempFileName);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="DataStream"/> class.
//        /// </summary>
//        /// <param name="bytes">The bytes.</param>
//        public ORMStream(Byte[] bytes)
//        {
//            string tempFileName = String.Format("{0}.stream", Guid.NewGuid().ToString("N"));
//            backBoneFile = Path.Combine(ORMConfigurator.TemporaryFolder, tempFileName);
//            backbone = new FileStream(backBoneFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, ORMConfigurator.BufferSize);

//            backbone.Write(bytes, 0, bytes.Length);
//            backbone.Flush();

//            // reset.
//            backbone.Position = 0;     
//        }


       

//        /// <summary>
//        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
//        /// </summary>
//        /// <value></value>
//        /// <returns>true if the stream supports reading; otherwise, false.
//        /// </returns>
//        public override bool CanRead
//        {
//            get { return BackBone.CanRead; }
//        }

//        /// <summary>
//        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
//        /// </summary>
//        /// <value></value>
//        /// <returns>true if the stream supports seeking; otherwise, false.
//        /// </returns>
//        public override bool CanSeek
//        {
//            get { return BackBone.CanSeek; }
//        }

//        /// <summary>
//        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
//        /// </summary>
//        /// <value></value>
//        /// <returns>true if the stream supports writing; otherwise, false.
//        /// </returns>
//        public override bool CanWrite
//        {
//            get { return BackBone.CanWrite; }
//        }

//        /// <summary>
//        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
//        /// </summary>
//        /// <exception cref="T:System.IO.IOException">
//        /// An I/O error occurs.
//        /// </exception>
//        public override void Flush()
//        {
//            BackBone.Flush();
//        }

//        /// <summary>
//        /// When overridden in a derived class, gets the length in bytes of the stream.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// A long value representing the length of the stream in bytes.
//        /// </returns>
//        /// <exception cref="T:System.NotSupportedException">
//        /// A class derived from Stream does not support seeking.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override long Length
//        {
//            get { return BackBone.Length; }
//        }

//        /// <summary>
//        /// When overridden in a derived class, gets or sets the position within the current stream.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// The current position within the stream.
//        /// </returns>
//        /// <exception cref="T:System.IO.IOException">
//        /// An I/O error occurs.
//        /// </exception>
//        /// <exception cref="T:System.NotSupportedException">
//        /// The stream does not support seeking.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override long Position
//        {
//            get
//            {
//                return BackBone.Position;
//            }
//            set
//            {
//                BackBone.Position = value;
//            }
//        }

//        /// <summary>
//        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
//        /// </summary>
//        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
//        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
//        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
//        /// <returns>
//        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
//        /// </returns>
//        /// <exception cref="T:System.ArgumentException">
//        /// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
//        /// </exception>
//        /// <exception cref="T:System.ArgumentNullException">
//        /// 	<paramref name="buffer"/> is null.
//        /// </exception>
//        /// <exception cref="T:System.ArgumentOutOfRangeException">
//        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative.
//        /// </exception>
//        /// <exception cref="T:System.IO.IOException">
//        /// An I/O error occurs.
//        /// </exception>
//        /// <exception cref="T:System.NotSupportedException">
//        /// The stream does not support reading.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            return BackBone.Read(buffer, offset, count);
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            return BackBone.Seek(offset, origin);
//        }

//        public override void SetLength(long value)
//        {
//            BackBone.SetLength(value);
//        }

//        /// <summary>
//        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
//        /// </summary>
//        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
//        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
//        /// <param name="count">The number of bytes to be written to the current stream.</param>
//        /// <exception cref="T:System.ArgumentException">
//        /// The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
//        /// </exception>
//        /// <exception cref="T:System.ArgumentNullException">
//        /// 	<paramref name="buffer"/> is null.
//        /// </exception>
//        /// <exception cref="T:System.ArgumentOutOfRangeException">
//        /// 	<paramref name="offset"/> or <paramref name="count"/> is negative.
//        /// </exception>
//        /// <exception cref="T:System.IO.IOException">
//        /// An I/O error occurs.
//        /// </exception>
//        /// <exception cref="T:System.NotSupportedException">
//        /// The stream does not support writing.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            BackBone.Write(buffer, offset, count);
//        }


//        /// <summary>
//        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
//        /// </summary>
//        public override void Close()
//        {
//            if (backbone != null) base.Close();

//            //if (backBoneFile != null && backBoneFile.Length > 0)
//            //{
//            //    backbone = null;
//            //}
//        }

//        /// <summary>
//        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
//        /// </summary>
//        /// <param name="value">The byte to write to the stream.</param>
//        /// <exception cref="T:System.IO.IOException">
//        /// An I/O error occurs.
//        /// </exception>
//        /// <exception cref="T:System.NotSupportedException">
//        /// The stream does not support writing, or the stream is already closed.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override void WriteByte(byte value)
//        {
//            BackBone.WriteByte(value);
//        }

//        /// <summary>
//        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
//        /// </summary>
//        /// <returns>
//        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
//        /// </returns>
//        /// <exception cref="T:System.NotSupportedException">
//        /// The stream does not support reading.
//        /// </exception>
//        /// <exception cref="T:System.ObjectDisposedException">
//        /// Methods were called after the stream was closed.
//        /// </exception>
//        public override int ReadByte()
//        {
//            return BackBone.ReadByte();
//        }

//        /// <summary>
//        /// Reads all bytes.
//        /// </summary>
//        /// <returns></returns>
//        public byte[] ReadAllBytes()
//        {
//            byte[] buffer = new byte[BackBone.Length];
//            BackBone.Read(buffer, 0, buffer.Length);
//            return buffer;
//        }

//        /// <summary>
//        /// Copies to.
//        /// </summary>
//        /// <param name="stream">The stream.</param>
//        public void CopyTo(Stream stream)
//        {
//            Copy(this, stream, ORMConfigurator.BufferSize, true);
//        }

//        /// <summary>
//        /// Copies the specified stream.
//        /// </summary>
//        /// <param name="stream">The stream.</param>
//        /// <param name="destination">The destination.</param>
//        public static void Copy(Stream stream, Stream destination)
//        {
//            Copy(stream, destination, ORMConfigurator.BufferSize, true);
//        }

//        /// <summary>
//        /// Copy one stream to another
//        /// </summary>
//        /// <param name="source">source stream</param>
//        /// <param name="destination"></param>
//        /// <param name="bufferSize">The buffer site</param>
//        /// <param name="autoFlush"></param>
//        /// <remarks>
//        ///  <p>This method does not handled exception and it does not close the streams.</p>
//        ///  <p>
//        ///  This method should be wrapped between try and finally method to ensure that
//        ///  all handled are disposed if stream fails.
//        ///  </p>
//        /// </remarks>
//        public static void Copy(Stream source, Stream destination, int bufferSize, bool autoFlush)
//        {

//            if (source == null)
//            {
//                throw new ArgumentNullException("source", "Source stream must be a instance of an object");
//            }

//            if (destination == null)
//            {
//                throw new ArgumentNullException("dest", "Destination stream must be a instance of an object");
//            }

//            byte[] buffer = new byte[bufferSize];

//            int bytesRead = 0;
//            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
//            {
//                destination.Write(buffer, 0, bytesRead);
//                if (autoFlush) destination.Flush();

//            }
//        }



//        /// <summary>
//        /// Reals all bytes.
//        /// </summary>
//        /// <param name="stream">The stream.</param>
//        /// <returns></returns>
//        public static byte[] RealAllBytes(Stream stream)
//        {
//            byte[] buffer = new byte[stream.Length];
//            stream.Read(buffer, 0, buffer.Length);
//            return buffer;
//        }

//        public new void Dispose()
//        {
//            if (BackBone != null)
//            {
//                BackBone.Close();
//#if NET20
//                BackBone.Dispose();
//#endif
//            }

//            if (backBoneFile != null || backBoneFile.Length > 0)
//            {
//                try
//                {
//                    if (File.Exists(backBoneFile))
//                    {
//                        File.Delete(backBoneFile);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    // if error, do not throw;
//                    Trace.Write("failed to remove file for DataStream. (Dispose Method). " + ex.Message);
//                }
//            }
//        }
//    }
//}