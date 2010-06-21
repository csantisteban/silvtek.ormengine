//#if !NET11
//using System;
//using System.IO;
//using System.Reflection;
//using System.Xml.Serialization;
//using System.Xml;
//using System.Text;
//using Silvtek.ORMEngine.Data;
//using Silvtek.ORMEngine.Xml;
//using System.Collections;

//namespace Silvtek.ORMEngine
//{
//    public class ORMServiceController
//    {

//        private ORMServiceController()
//        {
//        }

//        public static string CreateXmlService(IORMService service)
//        {
            
//            StringBuilder builder = new StringBuilder(5000);
//            StringWriter stringWriter = new StringWriter(builder);

//            XmlWriterSettings settings = new XmlWriterSettings();
//            settings.CheckCharacters = true;
//            settings.OmitXmlDeclaration = true;
//            settings.ConformanceLevel = ConformanceLevel.Auto;
//            settings.Indent = true;
//            XmlWriter writer = null;
//            try
//            {
//                writer = XmlWriter.Create(stringWriter, settings);
//                XmlSerializer serializer = new XmlSerializer(service.GetType());
//                serializer.Serialize(writer, service);
//            }
//            finally
//            {
//                if (writer != null)
//                {
//                    writer.Flush();
//                    writer.Close();
//                }
//            }

//            return builder.ToString();

//        }

//        //public static void CreateXmlService(XmlWriter writer, IORMService service)
//        //{

//        //    string prefix = ConnectionManager.Current.XmlPrefix;
//        //    string nameSpace = ConnectionManager.Current.XmlNamespace;

//        //    writer.WriteStartElement(prefix, service.GetType().Name);
//        //    writer.WriteAttributeString("namespace", service.GetType().Namespace);

//        //    if (service.Method != null && service.Method.Length > 0)
//        //    {
//        //        writer.WriteStartElement(prefix, "method", nameSpace);
//        //        writer.WriteValue(service.Method);
//        //        writer.WriteEndElement();
//        //        service.Parameters.WriteXml(writer);
//        //    }

//        //    if (service.Action != Action.Undefined)
//        //    {
//        //        writer.WriteStartElement(prefix, "action", nameSpace);
//        //        writer.WriteValue(service.Action);
//        //        writer.WriteEndElement();
//        //        service.Parameters.WriteXml(writer);
//        //    }

//        //    if (service.CurrentObject != null)
//        //    {
//        //        XmlSerializerUtil.WriteXml(writer, service.CurrentObject, prefix, nameSpace);
//        //    }

//        //    writer.WriteEndElement();
//        //}


//        public static IORMService[] CreateServices(string serviceXml, Assembly assembly)
//        {
//            return CreateServices(serviceXml, null, assembly);
//        }

//        /// <summary>
//        /// Instanciates the objects defined inside a service xml node.
//        /// </summary>
//        /// <param name="serviceXml">The service xml node</param>
//        /// <param name="root">The root</param>
//        /// <param name="assembly">The assembly where the service is defined</param>
//        /// <returns></returns>
//        public static IORMService[] CreateServices(string serviceXml, string root, Assembly assembly)
//        {
//            TextReader txtReader = new StringReader(serviceXml);
//            XmlReaderSettings settings = new XmlReaderSettings();
//            settings.CheckCharacters = true;
//            settings.IgnoreComments = true;
//            settings.IgnoreProcessingInstructions = true;
//            settings.IgnoreWhitespace = true;

//            XmlReader reader = XmlReader.Create(txtReader, settings);

//            return CreateServices(reader, root, assembly);
//        }

//        public static IORMService[] CreateServices(XmlReader reader, Assembly assembly)
//        {
//            return CreateServices(reader, null, assembly);
//        }


//        public static IORMService[] CreateServices(XmlReader reader, string root, Assembly assembly)
//        {
//            ArrayList list = new ArrayList();
//            if (reader.IsEmptyElement) return new IORMService[0];

//            if (root != null && root.Length > 0)
//            {
//                do
//                {
//                    if (reader.LocalName == root && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.Element) break;
//                    if (reader.LocalName == root && reader.Prefix != ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.Element)
//                    {
//                        throw new InvalidOperationException("The specified root does not match the request root. Check your root name and prefix.");
//                    }
              
//                } while (reader.Read());

//            }

//            //if ( (root == null || root.Length == 0)|| (reader.LocalName == root && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.Element) )

//            // {
//            // Process Services.
//            do
//            {
//                if (reader.LocalName == root && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.Element) continue;
//                if ((root != null && root.Length > 0) && (reader.LocalName == root && reader.Prefix == ConnectionManager.Current.XmlPrefix && reader.NodeType == XmlNodeType.EndElement)) break;
//                if (reader.IsEmptyElement) continue;

//                // All nodes on this level must be a services.
//                if (reader.NodeType == XmlNodeType.Element && reader.Prefix == ConnectionManager.Current.XmlPrefix)
//                {
//                    string currentNode = reader.LocalName;
//                    string nameSpace = reader["NameSpace"];
//                    string name;
//                    if (nameSpace != null && nameSpace.Length > 0)
//                    {
//                        name = String.Format("{0}.{1},{2}", nameSpace, currentNode, assembly.GetName().Name);
//                    }
//                    else
//                    {
//                        name = String.Format("{0},{1}", currentNode, assembly.GetName().Name);
//                    }

//                    Type type = Type.GetType(name, true, false);
//                    //IORMService service = (IORMService)Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
//                    //service.ReadXml(reader);
//                    XmlSerializer serializer = new XmlSerializer(type);
//                    IORMService service = (IORMService)serializer.Deserialize(reader);
//                    list.Add(service);
//                }
//            } while (reader.Read());


//            return (IORMService[])list.ToArray(typeof(IORMService));
//        }


//        /// <summary>
//        /// Run Xml objects
//        /// </summary>
//        /// <param name="reader">Xml Reader from where we gather the information</param>
//        /// <param name="root">The root node</param>
//        /// <param name="assembly">The assembly where the "Services" clases are located</param>
//        /// <param name="responseWriter">The response writer.</param>
//        /// <remarks>
//        ///  RunXml expects Services classes that inherits from ORMService object to be implemented.
//        /// </remarks>
//        public static ORMServicesResults RunXmlServices(XmlReader reader, string root, Assembly assembly)
//        {


//            IORMService[] services = CreateServices(reader, root, assembly);   
//            ORMServicesResults results = new ORMServicesResults();
//            foreach(IORMService service in services)
//            {
 
//                //if (service.Method != null && service.Method.Length > 0)
//                //{
//                object actionResult = service.RunAction();                   
//                //}

//                Xml.ORMServiceResult serviceResult = new Xml.ORMServiceResult(service, actionResult, service.Method);
//                results.Add(serviceResult);
//                service.Dispose();
//            }

//            return results;

            


//        }

//        /// <summary>
//        ///  /// Run Xml ORM objects
//        /// </summary>
//        /// <param name="xml">The xml containing the objects</param>
//        /// <param name="assembly">The assembly where the services are located.</param>
//        /// <returns>The response xml</returns>
//        public static ORMServicesResults RunXmlServices(string xml, Assembly assembly)
//        {
//            // READER
//            TextReader textReader = new StringReader(xml);
//            XmlReaderSettings settingsReader = new XmlReaderSettings();
//            settingsReader.CheckCharacters = true;
//            settingsReader.IgnoreComments = true;
//            settingsReader.IgnoreProcessingInstructions = true;
//            settingsReader.IgnoreWhitespace = true;

//            ORMServicesResults results = new ORMServicesResults();
//            using (XmlReader reader = XmlReader.Create(textReader, settingsReader))
//            {
//                results = RunXmlServices(reader, Constants.DATA_OBJECT_ROOT, assembly);
//                reader.Close();
//            }

//            return results;
//        }



//        // close class
//    }


//}
//#endif
