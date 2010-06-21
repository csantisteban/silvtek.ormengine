//#if !NET11
//using System;
//using System.Collections.ObjectModel;
//using System.Xml.Serialization;

//namespace Silvtek.ORMEngine
//{
//    [SoapType("Parameters")]
//    public class ORMServiceParameterInfoCollection : Collection<ORMServiceParameterInfo>
//    {
//        public ORMServiceParameterInfoCollection() { }

//        internal object FindParameter(string name)
//        {
//            object value = null;
//            foreach (ORMServiceParameterInfo info in Items)
//            {
//                if (String.Compare(info.Name, name, true) == 0)
//                {
//                    value = info.Value;
//                }
//            }

//            return value;

//        }


//        //#region IXmlSerializable Members

//        //public System.Xml.Schema.XmlSchema GetSchema()
//        //{
//        //    return
//        //}

//        //public void ReadXml(XmlReader reader)
//        //{
//        //    if (reader.LocalName == XML_ELEMENT_NAME && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.IsEmptyElement) return;
//        //    if (reader.LocalName == "parameters" && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.EndElement)
//        //    {
//        //        return;
//        //    }

//        //        do
//        //        {

//        //            if (reader.LocalName == XML_ELEMENT_NAME && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.Element)
//        //            {
//        //                XmlSerializer serializer = new XmlSerializer(typeof (ORMServiceParameterInfo));
//        //                ORMServiceParameterInfo parameter = (ORMServiceParameterInfo)serializer.Deserialize(reader);
//        //                Add(parameter);
//        //            }

//        //            if (reader.LocalName == XML_ELEMENT_NAME && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.EndElement)
//        //            {
//        //                return;
//        //            }


//        //        } while (reader.Read());
    
            
//        //}

//        //public void WriteXml(XmlWriter writer)
//        //{
//        //    if (this.Count > 0)
//        //    {
//        //        writer.WriteStartElement(ConnectionManager.Current.XmlPrefix, "parameters", ConnectionManager.Current.XmlNamespace);
//        //        foreach (ORMServiceParameterInfo parameter in this.Items)
//        //        {
//        //            writer.WriteStartElement(ConnectionManager.Current.XmlPrefix, "parameter", ConnectionManager.Current.XmlNamespace);
//        //            parameter.WriteXml(writer);
//        //            writer.WriteEndElement();
//        //        }
//        //        writer.WriteEndElement();
//        //    }
//        //    else
//        //    {
//        //        writer.WriteStartElement(ConnectionManager.Current.XmlPrefix, "parameters", ConnectionManager.Current.XmlNamespace);
//        //        writer.WriteEndElement();
//        //    }
//        //}

//        //#endregion
//    }
//}
//#endif