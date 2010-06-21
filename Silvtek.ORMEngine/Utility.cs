using System;
using System.Xml;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	internal sealed class Utility
	{

		private Utility()
		{

		}

		internal static Resources Resources
		{
			get {return new Resources();}
		}

		internal static object GetResourceValue(string key)
		{
			Resources res = new Resources();
			return res.GetObject(key);
		}

		internal static string GetResourceString(string key, params object[] args)
		{
			Resources res = new Resources();
			return res.GetString(key, args);
		}

		internal static string GetResourceString(string key)
		{
			Resources res = new Resources();
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
            if ((tempValue.Length == 0) && (!isRequired))return false;

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
    }
}
