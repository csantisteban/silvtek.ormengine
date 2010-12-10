using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
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

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
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

        /// <summary>
        ///   Serialize object to a file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="formatter"></param>
        /// <param name="obj"></param>
        /// <exception cref="SerializationException">
        ///  An error has occurred during serialization, such as if an object in the graph parameter is not marked as serializable.
        /// </exception>
        /// <exception cref="SerializationException">
        ///  When it fails to create a stream for the provide file name.
        /// </exception>
        /// <exception cref="SecurityException">
        ///  The caller does not have the required permission.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///  The file name is null or an empty string.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   The formatter is null.
        /// </exception>
        public static void SerializeToFile(string file, IFormatter formatter, object obj)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("file name cannot be null or empty string", "file");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException("formatter", "formatter must be an instance of an object");
            }


            Stream stream = null;
            try
            {
                stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                formatter.Serialize(stream, obj);
                stream.Flush();
            }
            catch (ArgumentNullException e)
            {
                throw new SerializationException("Failed to create an stream handle for the file '" + file + "'", e);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (stream != null) stream.Close();
            }

        }

 
        /// <summary>
        ///   Serialize object to a file using the BinaryFormatter
        /// </summary>
        /// <param name="file"></param>
        /// <param name="obj"></param>
        /// <exception cref="SerializationException">
        ///  An error has occurred during serialization, such as if an object in the graph parameter is not marked as serializable.
        /// </exception>
        /// <exception cref="SerializationException">
        ///  When it fails to create a stream for the provide file name.
        /// </exception>
        /// <exception cref="SecurityException">
        ///  The caller does not have the required permission.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///  The file name is null or an empty string.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   The formatter is null.
        /// </exception>
        public static void SerializeToFile(string file, object obj)
        {
            BinaryFormatter binary = new BinaryFormatter();
            SerializeToFile(file, binary, obj);
        }

        /// <summary>
        /// Deserialize an object from a file.
        /// </summary>
        /// <param name="file">File full path.</param>
        /// <param name="formatter">The formatter.</param>
        /// <returns>The object.</returns>
        /// <exception cref="SerializationException">
        ///  When it fails to create a stream for the provide file name.
        /// </exception>
        /// <exception cref="SerializationException">
        ///  The serializationStream supports seeking, but its length is 0.
        /// </exception>
        /// <exception cref="SecurityException">
        ///  The caller does not have the required permission.
        /// </exception>
        public static object DeserializeFromFile(string file, IFormatter formatter)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("file name cannot be null or empty string", "file");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException("formatter", "formatter must be an instance of an object");
            }

            if (!File.Exists(file)) return null;

            Stream stream = null;
            object obj = null;
            try
            {
                stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                obj = formatter.Deserialize(stream);

            }
            catch (ArgumentNullException e)
            {
                throw new SerializationException("Failed to create an stream from the file '" + file + "'", e);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (stream != null) stream.Close();
            }

            return obj;
        }

        /// <summary>
        /// Deserialize an object from a file using the <see cref="BinaryFormatter" />.
        /// </summary>
        /// <returns>The object.</returns>
        /// <exception cref="SerializationException">
        ///  When it fails to create a stream for the provide file name.
        /// </exception>
        /// <exception cref="SerializationException">
        ///  The serializationStream supports seeking, but its length is 0.
        /// </exception>
        /// <exception cref="SecurityException">
        ///  The caller does not have the required permission.
        /// </exception>
        public static object DeserializeFromFile(string file)
        {
            BinaryFormatter binary = new BinaryFormatter();
            return DeserializeFromFile(file, binary);
        }

    }
}
