using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Summary description for Utility.
    /// </summary>
    internal sealed class Utility
    {
        private static readonly Resources res = new Resources();
        private Utility()
        {

        }

        internal static Resources Resources
        {
            get { return new Resources(); }
        }

        /// <summary>
        /// Gets the resource string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        internal static string GetResourceString(string key, params object[] args)
        {
            return res.GetString(key, args);
        }

        /// <summary>
        /// Gets the resource string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal static string GetResourceString(string key)
        {

            return res.GetString(key);
        }

        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="errMsg">The err MSG.</param>
        /// <returns></returns>
        internal static string GetNodeValue(XmlNode sourceNode, string xpath, bool isRequired, string errMsg)
        {
            XmlNode node = sourceNode.SelectSingleNode(xpath);
            string retValue = String.Empty;

            if (node == null && !isRequired)
            {
                retValue = String.Empty;
            }
            else if (node == null && isRequired)
            {
                throw new XmlException(errMsg);
            }
            else
            {
                retValue = node.InnerText;
            }

            return retValue;
        }


        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        internal static string GetNodeValue(XmlNode sourceNode, string xpath)
        {
            return GetNodeValue(sourceNode, xpath, false, String.Empty);
        }

        /// <summary>
        /// Gets the attribute value as int32.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <returns></returns>
        internal static int GetAttributeValueAsInt32(XmlNode node, string attributeName, bool isRequired)
        {
            string tempValue = GetAttributeValue(node, attributeName);

            //return false if the value is not required and it has not been defined.
            if ((tempValue.Length == 0) && (!isRequired)) return 0;

            if (tempValue.Length == 0 && isRequired)
            {
                string exMsg = Utility.GetResourceString("UTILITY:REQUIRED_VALUE", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            int returnValue = 0;
            try
            {
                returnValue = Convert.ToInt32(tempValue);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("UTILITY:INVALID_TO_PARSE_TO_INT32", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the attribute value as bool.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="boolDefault">if set to <c>true</c> [bool default].</param>
        /// <returns></returns>
        internal static bool GetAttributeValueAsBool(XmlNode node, string attributeName, bool isRequired, bool boolDefault)
        {
            string tempValue = GetAttributeValue(node, attributeName);

            //return false if the value is not required and it has not been defined.
            if ((tempValue.Length == 0) && (!isRequired)) return boolDefault;

            if (tempValue.Length == 0 && isRequired)
            {
                string exMsg = Utility.GetResourceString("UTILITY:REQUIRED_VALUE", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            bool returnValue = false;
            try
            {
                returnValue = Convert.ToBoolean(tempValue);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("UTILITY:INVALID_TO_PARSE_TO_BOOL", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the attribute value as bool.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <returns></returns>
        internal static bool GetAttributeValueAsBool(XmlNode node, string attributeName, bool isRequired)
        {
            string tempValue = GetAttributeValue(node, attributeName);

            //return false if the value is not required and it has not been defined.
            if ((tempValue.Length == 0) && (!isRequired)) return false;

            if (tempValue.Length == 0 && isRequired)
            {
                string exMsg = Utility.GetResourceString("UTILITY:REQUIRED_VALUE", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            bool returnValue = false;
            try
            {
                returnValue = Convert.ToBoolean(tempValue);
            }
            catch (Exception)
            {
                string exMsg = Utility.GetResourceString("UTILITY:INVALID_TO_PARSE_TO_BOOL", attributeName, node.Name);
                throw new ORMException(exMsg);
            }

            return returnValue;
        }


        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        internal static string GetAttributeValue(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if (attribute == null) return String.Empty;

            string tempValue = attribute.Value;
            if (tempValue == null) tempValue = String.Empty;

            return tempValue;
        }

        /// <summary>
        /// Byteses to hex.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        internal static string BytesToHex(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString.Append(bytes[i].ToString("x2"));
            }

            return hexString.ToString();
        }

        /// <summary>
        /// Gets the hash from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        internal static string GetHashFromStream(Stream stream)
        {

            if (!stream.CanRead || stream.CanSeek)
            {
                throw new InvalidOperationException("Underlying stream can't be read or seek");
            }

            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] byteResult = sha.ComputeHash(stream);

            stream.Position = 0;

            return BytesToHex(byteResult);
        }

        /// <summary>
        /// Converts the hex to bytes.
        /// </summary>
        /// <param name="hex">The hex.</param>
        /// <returns></returns>
        internal static byte[] ConvertHexToBytes(String hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Converts to bytes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal static byte[] ConvertToBytes(object value)
        {
          //  SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] sourceBytes = new byte[0];

            if (value == null) return new byte[0];

            if (value.GetType() == typeof (String))
            {
                string text = Convert.ToString(value);
                sourceBytes = Encoding.UTF8.GetBytes(text);
            }
            else if (value.GetType() == typeof (bool))
            {
                sourceBytes = BitConverter.GetBytes((bool) value);
            }
            else if (value.GetType() == typeof (DateTime))
            {
                long num = ((DateTime) value).Ticks;
                sourceBytes = BitConverter.GetBytes(num);
            }
            else if (value.GetType().GetInterface("IConvertible", true) != null)
            {
                string text = Convert.ToString(value);
                sourceBytes = Encoding.UTF8.GetBytes(text);
            }
            else if (value.GetType() == typeof (XmlDocument))
            {
                string xml = ((XmlDocument) value).OuterXml;
                sourceBytes = Encoding.UTF8.GetBytes(xml);
            }
            else if (value.GetType() == typeof (byte[]))
            {
                sourceBytes = (byte[]) value;
            }
            else
            {
                SerializableAttribute[] serializableAttribute = (SerializableAttribute[])value.GetType().GetCustomAttributes(typeof (SerializableAttribute), false);
                if (serializableAttribute.Length > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, value);
                        stream.Flush();

                        stream.Position = 0;
                        sourceBytes = stream.ToArray();
                        stream.Close();
                    }
                }
            }

            return sourceBytes;
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
        internal static void CopyStream(Stream source, Stream dest, int bufferSize, bool autoFlush)
        {

            if (source == null)
            {
                throw new ArgumentNullException("source", "Source stream must be a instance of an object");
            }

            if (dest == null)
            {
                throw new ArgumentNullException("dest", "Destination stream must be a instance of an object");
            }

            byte[] buffer = new byte[bufferSize];

            int bytesRead = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, bytesRead);
                if (autoFlush) dest.Flush();

            }
        }


        
    }
}