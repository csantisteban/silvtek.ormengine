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
        private static Resources res = new Resources();
        private Utility()
        {

        }

        internal static Resources Resources
        {
            get { return new Resources(); }
        }

        //internal static object GetResourceValue(string key)
        //{
        //    Resources res = new Resources();
        //    return res.GetObject(key);
        //}

        internal static string GetResourceString(string key, params object[] args)
        {
            return res.GetString(key, args);
        }

        internal static string GetResourceString(string key)
        {

            return res.GetString(key);
        }

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


        internal static string GetNodeValue(XmlNode sourceNode, string xpath)
        {
            return GetNodeValue(sourceNode, xpath, false, String.Empty);
        }

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


        internal static string GetAttributeValue(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = node.Attributes[attributeName];
            if (attribute == null) return String.Empty;

            string tempValue = attribute.Value;
            if (tempValue == null) tempValue = String.Empty;

            return tempValue;
        }

        internal static string BytesToHex(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString.Append(bytes[i].ToString("X2"));
            }

            return hexString.ToString();
        }

        internal static byte[] ConverToBytes(object value)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
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
    }
}