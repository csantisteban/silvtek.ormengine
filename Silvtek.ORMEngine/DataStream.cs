using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    public class DataStream : Stream, IDisposable 
    {
        private Stream backbone = null;
        private readonly string backBoneFile;
        private readonly MapMember mapMember;

        internal string FilePath
        {
            get { return backBoneFile; }
        }

        private Stream BackBone
        {
            get
            {
                if ( (backbone == null && mapMember != null) || ( (backbone == null) && (backBoneFile != null && backBoneFile.Length > 0) ))
                {
                    backbone = new FileStream(backBoneFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, ORMConfigurator.BufferSize);
                }
             

                return backbone;
            }
        }


        public DataStream(Encoding encoding, string text)
        {

            byte[] bytes = encoding.GetBytes(text);
            backbone = new MemoryStream(bytes, 0, bytes.Length, false, true);
            backbone.Position = 0;
        }

        public DataStream(string filepath, FileMode fileMode, FileAccess fileAccess, FileShare fileshare)
        {
            backBoneFile = filepath;
            backbone = new FileStream(filepath, fileMode, fileAccess, fileshare, ORMConfigurator.BufferSize);
        }

        public DataStream(Byte[] bytes)
        {
            backbone = new MemoryStream(bytes, 0, bytes.Length, false, true);
            backbone.Position = 0; 
            backbone.Flush();
        }

        public DataStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "Cannot instanciate DataStream with a null argument.");
            }

            backbone = stream;
        }

        internal DataStream(MapMember mapMember)
        {
            this.mapMember = mapMember;
            string tempFileName = String.Format("{0}.{1}~{2}.bin", mapMember.Member.DeclaringType.Name, mapMember.MemberName, Guid.NewGuid().ToString("N"));
            backBoneFile = Path.Combine(ORMConfigurator.TemporaryFolder, tempFileName); 
          
        }

       
        public override bool CanRead
        {
            get { return BackBone.CanRead; }
        }

        public override bool CanSeek
        {
            get { return BackBone.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return BackBone.CanWrite; }
        }

        public override void Flush()
        {
            BackBone.Flush();
        }

        public override long Length
        {
            get { return BackBone.Length; }
        }

        public override long Position
        {
            get
            {
                return BackBone.Position;
            }
            set
            {
                BackBone.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BackBone.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BackBone.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BackBone.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BackBone.Write(buffer, offset, count);
        }

        
        public override void Close()
        {
            if (backbone != null)base.Close();
           
            //if (backBoneFile != null && backBoneFile.Length > 0)
            //{
            //    backbone = null;
            //}
        }

        public override void WriteByte(byte value)
        {
            BackBone.WriteByte(value);
        }

        public override int ReadByte()
        {
           return  BackBone.ReadByte();
        }

        public byte[] ReadAllBytes()
        {
            byte[] buffer = new byte[BackBone.Length];
            BackBone.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public void CopyTo(Stream stream)
        {
            Copy(this, stream, ORMConfigurator.BufferSize, true);
        }

        public static void Copy(Stream stream, Stream destination)
        {
            Copy(stream, destination, ORMConfigurator.BufferSize, true);
        }

        /// <summary>
        /// Copy one stream to another
        /// </summary>
        /// <param name="source">source stream</param>
        /// <param name="dest">destination stream</param>
        /// <param name="bufferSize">The buffer site</param>
        /// <remarks>
        ///  <p>This method does not handled exception and it does not close the streams.</p>
        ///  <p>
        ///  This method should be wrapped between try and finally method to ensure that
        ///  all handled are disposed if stream fails.
        ///  </p>
        /// </remarks>
        public static void Copy(Stream source, Stream destination, int bufferSize, bool autoFlush)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source", "Source stream must be a instance of an object");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("dest", "Destination stream must be a instance of an object");
            }

            byte[] buffer = new byte[bufferSize];

            int bytesRead = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                if (autoFlush) destination.Flush();

            }
        }


        public static byte[] RealAllBytes(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public new void Dispose()
        {
            if (BackBone != null)
            {
                BackBone.Close();
#if NET20
                BackBone.Dispose();
#endif
            }

            if (backBoneFile != null || backBoneFile.Length > 0)
            {
                try
                {
                    if (File.Exists(backBoneFile))
                    {
                        File.Delete(backBoneFile);
                    }
                }
                catch(Exception ex)
                {
                    // if error, do not throw;
                    Trace.Write("failed to remove file for DataStream. (Dispose Method). "+ex.Message);
                }
            }
        }
    }
}