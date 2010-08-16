using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// List of static method for serializing ORM object to Xml and binary.
    /// </summary>
    public class ORMSerializer
    {

        /// <summary>
        /// Creates an xml string from an ORMObject.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string CreateXml(ORMObject obj)
        {
            if (obj == null) return String.Empty;
            string text; 
            using (MemoryStream stream = new MemoryStream())
            {
                WriteXml(stream, obj);
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }

                stream.Close();
            }

            return text;
        }

        public static void WriteXml(Stream stream, ORMObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "can't serialize a null object.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("obj", "can't serialize to a null stream.");
            }

            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            

            writer.WriteStartDocument();
            writer.WriteStartElement("ORMObject");

            writer.WriteStartElement("Header");
            writer.WriteAttributeString("schema-version", "1.0");
            string dateTime = String.Format("{0:yyyy-MM-ddThh:mm:ss.fffff}", DateTime.Now);
            writer.WriteAttributeString("datetime", dateTime);

            writer.WriteStartElement("ORMEngine");
            writer.WriteAttributeString("assembly", Assembly.GetExecutingAssembly().GetName().FullName);
            writer.WriteEndElement();

            // close header
            writer.WriteEndElement();

            writer.WriteStartElement("MapInformation");
              writer.WriteStartElement("Object");
              writer.WriteAttributeString("type", obj.SourceObject.GetType().FullName);
              writer.WriteAttributeString("table", obj.ObjectInfo.Schema.TableName);
              writer.WriteEndElement();


              writer.WriteStartElement("MapFields");
              foreach (MapMember map in obj.ObjectInfo.Schema.MappedMembers)
              {
                  writer.WriteStartElement("Field");
                  writer.WriteAttributeString("name", map.MemberName);
                  writer.WriteAttributeString("column", map.ColumnName);
                  writer.WriteAttributeString("data-type", map.Properties.DataType.ToString());
                  writer.WriteAttributeString("primary-key", map.Properties.IsPrimaryKey.ToString());
                  writer.WriteAttributeString("length", map.Properties.Length.ToString());
                  writer.WriteAttributeString("allow-nulls", map.Properties.AcceptNulls.ToString());
                  writer.WriteAttributeString("encrypted", map.Properties.IsEncryptionEnabled.ToString());
                  writer.WriteAttributeString("mode", map.Properties.ModeType.ToString());
                  writer.WriteEndElement();
              }
              writer.WriteEndElement();

              
            // close mapInformation
            writer.WriteEndElement();

            // object state
            writer.WriteStartElement("ObjectState");
            writer.WriteStartElement("Identifier");
            writer.WriteString(obj.GetRecordIdentifier());
            writer.WriteEndElement();
            writer.WriteStartElement("Digest");
            writer.WriteString(obj.GetDigest());
            writer.WriteEndElement();


            obj.FieldList.WriteXml(writer);

            writer.WriteStartElement("Sql");
            writer.WriteString(obj.ObjectInfo.LastSql);
            writer.WriteEndElement();

            writer.WriteStartElement("ActionResult");
            writer.WriteString(obj.Result.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();

          //  byte[] bytes = Serialize(obj.SourceObject);
         //   writer.WriteStartElement("Binary");
         //   writer.WriteAttributeString("type", obj.SourceObject.GetType().FullName);
         //   writer.WriteAttributeString("bytes", bytes.Length.ToString());

#if NET11
        //    string base64 = Convert.ToBase64String(bytes, 0, bytes.Length);
#else
        //    string base64 = Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.InsertLineBreaks);
#endif
        //    writer.WriteRaw(base64);
        //    writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Flush();
        }

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Must be a instance of an object; The current value is null.");
            }

            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter format = new BinaryFormatter();
                format.Serialize(stream, obj);

                bytes = stream.ToArray();

                stream.Close();
            }

            return bytes;
        }

        public static object Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("bytes", "Byte array is empty or null");
            }

            object obj = null;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter format = new BinaryFormatter();
                obj = format.Deserialize(stream);
                stream.Close();
            }

            return obj;
        }
    }
}
