using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Encryption;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    [Serializable]
    internal class PersistentRecord :  ISerializable
    {
        private readonly Type type;
        private readonly string recordIdentifier;
        private readonly string hash;
       
        private readonly ORMObject obj;
        private readonly Hashtable values;
        private string path;

        private readonly string digestValue;

        internal object this[string name]
        {
            get { return values[name]; }
        }

        internal PersistentRecord(ORMObject obj)
        {
            values = new Hashtable();
            this.obj = obj;
    
            recordIdentifier = obj.GetRecordIdentifier();

            hash = obj.GetDigest();
            type = obj.SourceObject.GetType();

            if (obj.ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                foreach (MapMember member in obj.ObjectInfo.MappedMembers)
                {
                    if (member.Properties.HasRecordDigestProvider)
                    {
                        digestValue = Convert.ToString(member.GetMemberValue(obj.SourceObject));
                        break;
                    }
                }
            }
     

        }

        internal void Persist(string path)
        {
            this.path = path;
            ORMSerializer.SerializeToFile(path, this);
        }

        public Type Type
        {
            get { return type; }
        }

        public string RecordIdentifier
        {
            get { return recordIdentifier; }
        }

        public string Hash
        {
            get { return hash; }
        }

        public string DigestValue
        {
            get { return digestValue; }
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArrayList tmpList = new ArrayList();
            foreach (MemberField field in obj.FieldList)
            {
                if (field == null) continue;
                tmpList.Add(field);

            }

            for (int i = 0; i < tmpList.Count; i++)
            {

                NameValueItem item = new NameValueItem();
                MemberField field = (MemberField) tmpList[i];

                if (field.MapMember.Properties.DataType == DataType.Binary || field.MapMember.Properties.DataType == DataType.Object)
                {
                 //   string filename = String.Format(@"{0}\{1}.{2}.stub", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), field.MapMember.MemberName);
                    byte[] bytes = GetBinaryValue(field);
                    item.Name = field.Name;
                    item.Value = bytes;
                    item.Stub = true;
                    continue;
                }

                item.Name = field.Name;
                item.Value = field.ActualValue;

                info.AddValue("record:" + i, item);

            }


            info.AddValue("fieldcount", tmpList.Count);
            info.AddValue("recordhash", Hash);
            info.AddValue("recordIdentifier", RecordIdentifier);
            info.AddValue("type", Type);
            info.AddValue("digestValue", DigestValue);

        }


        public  PersistentRecord(SerializationInfo info, StreamingContext context)
        {
            int fieldCount = info.GetInt32("fieldcount");
            ArrayList tmplist = new ArrayList(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                NameValueItem item = (NameValueItem)info.GetValue("record:" + i, typeof(NameValueItem));
                if (item == null) continue;
                tmplist.Add(item);

            }

            
            hash = info.GetString("recordhash");
            recordIdentifier = info.GetString("recordIdentifier");
            type = (Type)info.GetValue("type", typeof(Type));
            ObjectSchemaInfo schemaInfo = ORM.GetObjectInfo(type);

            digestValue = info.GetString("digestValue");
            values = new Hashtable();

            foreach (NameValueItem item in tmplist)
            {
                if (!item.Stub)
                {
                    values.Add(item.Name, item.Value);
                }
                else
                {
                    MapMember mapMember = schemaInfo.MappedMembers.Find(item.Name);
                    object value = ReadBytes(((byte[])item.Value), mapMember);
                    values.Add(item.Name, value);
                }
            }
        }
        private object ReadBytes(byte[] bytes, MapMember mapMember)
        {
            Stream stream = new DataStream();
            object returnValue = null;

            if (mapMember.Properties.IsEncryptionEnabled)
            {
                Stream encrypted = new MemoryStream(bytes);
                EncryptionProvider provider = mapMember.Properties.GetEncryptionProvider();
                provider.DecryptStream(encrypted, stream);
            }
            else
            {
                if (mapMember.MapMemberType == typeof(Byte[]) && mapMember.Properties.DataType == DataType.Binary)
                {
                    return bytes;
                    // if binary, the membertype byte and it has not encrypted, then return the original bytes.
                }

                stream = new DataStream(bytes);
            }

            if ((mapMember.MapMemberType == typeof(Stream) || mapMember.MapMemberType == typeof(DataStream)) && mapMember.Properties.DataType == DataType.Binary)
            {
                returnValue = stream;
            }

            else if (mapMember.Properties.DataType == DataType.Object)
            {
                IFormatter deserializer = mapMember.Properties.GetObjectFormatter();
                returnValue = deserializer.Deserialize(stream);
            }

            return returnValue;
             
        }

        private object ReadBinaryValue(string path, MapMember mapMember)
        {
            Stream stream = new DataStream();
            object returnValue = null;
            if (mapMember.Properties.IsEncryptionEnabled)
            {
                Stream encrypted = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ORMConfigurator.BufferSize);
                EncryptionProvider provider = mapMember.Properties.GetEncryptionProvider();
                provider.DecryptStream(encrypted, stream);
            }
            else
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ORMConfigurator.BufferSize); 
            }
         
            if (mapMember.MapMemberType == typeof(Byte[]) && mapMember.Properties.DataType == DataType.Binary)
            {
                returnValue = DataStream.RealAllBytes(stream);
            }
            else if ((mapMember.MapMemberType == typeof(Stream) || mapMember.MapMemberType == typeof(DataStream)) && mapMember.Properties.DataType == DataType.Binary)
            {
                returnValue = stream;
            }

            else if (mapMember.Properties.DataType == DataType.Object)
            {
                IFormatter deserializer = mapMember.Properties.GetObjectFormatter();
                returnValue = deserializer.Deserialize(stream);
            }

            return returnValue;
        }

        // get binary value for field (stream, byte[], or custom object).
        private byte[] GetBinaryValue(MemberField field)
        {
            object originalValue = field.MapMember.GetMemberValue(obj.SourceObject);
            Type mapMemberType = field.MapMember.MapMemberType;

            byte[] bytes = null;
            // check if value is a string;

            if (originalValue == null) return new byte[0];
            if (ReferenceEquals(originalValue, Stream.Null)) return new byte[0];

            if (mapMemberType == typeof(Stream) || mapMemberType == typeof(DataStream))
            {


                if (field.MapMember.Properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();

                    DataStream stream = new DataStream((Stream)originalValue);
                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(stream, encryptedStream);
                    bytes = DataStream.RealAllBytes(encryptedStream);
                }
                else
                {
                    bytes = DataStream.RealAllBytes((Stream)originalValue);
                }


            }
            else if (mapMemberType == typeof(Byte[]) && field.MapMember.Properties.DataType == DataType.Binary)
            {

                if (field.MapMember.Properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();

                    Stream originalStream = new DataStream((Byte[])originalValue);
                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(originalStream, encryptedStream);
                    bytes = DataStream.RealAllBytes(encryptedStream);
                }
                else
                {
                    bytes = (Byte[])originalValue;
                }


            }
            else if (field.MapMember.Properties.DataType == DataType.Object)
            {
                using (Stream stream = new MemoryStream())
                {
                    IFormatter serialiazer = field.MapMember.Properties.GetObjectFormatter();
                    serialiazer.Serialize(stream, originalValue);
                    stream.Flush();
                    stream.Position = 0;

                    if (field.MapMember.Properties.IsEncryptionEnabled)
                    {
                        EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();
                        DataStream encryptedStream = new DataStream();
                        provider.EncryptStream(stream, encryptedStream);
                        bytes = DataStream.RealAllBytes(encryptedStream);
                    }
                    else
                    {
                        bytes = DataStream.RealAllBytes(stream);
                    }

                    stream.Close();


                }
            }

            return bytes;
        }


               // get binary value for field (stream, byte[], or custom object).
        private void SaveBinaryValue(MemberField field, string filepath)
        {
            object originalValue = field.MapMember.GetMemberValue(obj.SourceObject);
            Type mapMemberType = field.MapMember.MapMemberType;

            Stream sourceStream = null;
            // check if value is a string;
       
            if (originalValue == null) return;
            if (ReferenceEquals(originalValue, Stream.Null)) return;

            if (mapMemberType == typeof(Stream) || mapMemberType == typeof(DataStream))
            {
                

                if (field.MapMember.Properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();

                    DataStream stream = new DataStream((Stream)originalValue);
                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(stream, encryptedStream);
                    sourceStream = encryptedStream;
                }
                else
                {
                    sourceStream = new DataStream((Stream)originalValue);
                }
                

            }
            else if (mapMemberType == typeof(Byte[]) && field.MapMember.Properties.DataType == DataType.Binary)
            {
                
                if (field.MapMember.Properties.IsEncryptionEnabled)
                {
                    EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();

                    Stream originalStream = new DataStream((Byte[])originalValue);
                    DataStream encryptedStream = new DataStream();
                    provider.EncryptStream(originalStream, encryptedStream);
                    sourceStream = encryptedStream;
                }
                else
                {       
                    sourceStream = new DataStream((Byte[]) originalValue);
                }

   
            }
            else if (field.MapMember.Properties.DataType == DataType.Object)
            {
                using (Stream stream = new MemoryStream())
                {
                    IFormatter serialiazer = field.MapMember.Properties.GetObjectFormatter();
                    serialiazer.Serialize(stream, originalValue);
                    stream.Flush();
                    stream.Position = 0;
                 
                    if (field.MapMember.Properties.IsEncryptionEnabled)
                    {
                        EncryptionProvider provider = field.MapMember.Properties.GetEncryptionProvider();
                        DataStream encryptedStream = new DataStream();
                        provider.EncryptStream(stream, encryptedStream);
                        sourceStream = encryptedStream;
                    }
                    else
                    {
                        sourceStream = stream;   
                    }

                    stream.Close();

                    
                }
            }


            if (sourceStream == null) return;
            using(FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, ORMConfigurator.BufferSize))
            {
                Utility.CopyStream(sourceStream, fs, ORMConfigurator.BufferSize, true);
                fs.Close();
            }
        }

    }
}
