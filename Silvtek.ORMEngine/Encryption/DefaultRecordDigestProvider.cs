using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Silvtek.ORMEngine.Encryption
{
    /// <summary>
    /// Default record digest provided.
    /// </summary>
    public class DefaultRecordDigestProvider : RecordDigestProvider
    {
        public override string GenerateDigest(object[] values)
        {
            string digest = String.Empty;

            try
            {
                digest = CreateDigestAsString(values);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to create digest. " + e.Message);
                digest = "ERR";
            }

            return digest;
            
        }

        /// <summary>
        /// Validates the digest.
        /// </summary>
        /// <param name="digest">The digest.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public override DigestComparisonResult ValidateDigest(string digest, object[] values)
        {
            if (digest == null || digest.Length == 0 || digest == "ERR")
            {
                return DigestComparisonResult.Inconclusive;
            }

            byte[] currentDigestBytes = CreateDigest(values);

            if (currentDigestBytes == null || currentDigestBytes.Length == 0)
            {
                return DigestComparisonResult.Inconclusive;
            }

            string currentDigest = Utility.BytesToHex(currentDigestBytes);
            if (String.CompareOrdinal(currentDigest, digest) != 0)
            {
                //double check.
                byte[] digestbytes = Utility.ConvertHexToBytes(digest);
                for (int i = 0; i < currentDigestBytes.Length; i++)
                {
                    if (currentDigestBytes[i] != digestbytes[i])
                    {
                        return DigestComparisonResult.Different;
                    }
                }
            }

            return DigestComparisonResult.Equal;

        }

        private static byte[] CreateDigest(object[] values)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] sourceBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (object value in values)
                {
                    if (value == null) continue;

                    if (value.GetType() == typeof(String))
                    {
                        string text = Convert.ToString(value);
                        byte[] bytes = Encoding.UTF8.GetBytes(text);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (value.GetType() == typeof(bool))
                    {
                        byte[] bytes = BitConverter.GetBytes((bool)value);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (value.GetType() == typeof(DateTime))
                    {
                        long num = ((DateTime)value).Ticks;
                        byte[] bytes = BitConverter.GetBytes(num);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (value.GetType().GetInterface("IConvertible", true) != null)
                    {
                        string text = Convert.ToString(value);
                        byte[] bytes = Encoding.UTF8.GetBytes(text);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (value.GetType() == typeof(XmlDocument))
                    {
                        string xml = ((XmlDocument)value).OuterXml;
                        byte[] bytes = Encoding.UTF8.GetBytes(xml);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else if (value.GetType() == typeof(byte[]))
                    {
                        byte[] bytes = (byte[])value;
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        SerializableAttribute[] serializableAttribute =
                            (SerializableAttribute[])
                            value.GetType().GetCustomAttributes(typeof(SerializableAttribute), false);
                        if (serializableAttribute.Length > 0)
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(stream, value);
                        }
                    }
                }

                stream.Flush();

                stream.Position = 0;
                sourceBytes = stream.ToArray();
                stream.Close();
            }

            return sha1.ComputeHash(sourceBytes);       
        }

        private static string CreateDigestAsString(object[] values)
        {
            byte[] result = CreateDigest(values);
            return Utility.BytesToHex(result);
        }

     
    }
}
