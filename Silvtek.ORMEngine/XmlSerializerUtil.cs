//#region Copyright
/// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
/// See LICENSE in the repository root.
//#endregion

//#if NET20
//using System;
//using System.Collections;
//using System.Collections.ObjectModel;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml;
//using System.Xml.Serialization;
//using System.Reflection;
//using System.Diagnostics;
//using System.IO;
//using Silvtek.ORMEngine.Schema;

//namespace Silvtek.ORMEngine
//{

//    internal class XmlSerializerUtil
//    {

//        internal static void ReadXml(XmlReader reader, Object obj, string prefix)
//        {
//            ReadXml(reader, null, obj, prefix, Action.Undefined);
//        }
           
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="reader"></param>
//        /// <param name="obj"></param>
//        /// <param name="prefix"></param>
//        internal static void ReadXml(XmlReader reader, string objectName, Object obj, string prefix, Action action)
//        {
//            if (obj == null) return;
//            Type type = obj.GetType();
//            //string objectName = element; // type.Name;

//            if (type.GetInterface("IEnumerable") != null && type.GetInterface("IList") != null)
//            {
//                string typeFullName = reader.GetAttribute("contains-type");
//                Type listType = null;
//                if (typeFullName != null && typeFullName.Length > 0)
//                {
//                    listType = Type.GetType(typeFullName, false, true);
//                }

//                if (objectName == null || objectName.Length == 0)
//                {
//                    objectName = type.Name;
//                }

//                ReadIList(reader, obj, prefix, objectName, listType);
//                return; // exist.
//            }

//            FieldInfo[] fieldInfoList = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
//            //Debug.WriteLine("Number of fields :" + fieldInfoList.Length + " for : " + type.FullName);
//            if ((objectName != null && objectName.Length > 0) && reader.LocalName != objectName && reader.Prefix != prefix) return;

//            Dictionary<string, FieldInfo> propertyCollection = new Dictionary<string, FieldInfo>();
//            foreach (FieldInfo field in fieldInfoList)
//            {
//                propertyCollection.Add(field.Name, field);
//                Debug.WriteLine("Field :" + field.Name + " added for type " + type.FullName);
//            }

//            // start reading xml.
//            Hashtable fieldBuffer = new Hashtable();
//            while (reader.Read())
//            {
//                if (reader.LocalName == String.Empty) continue;
//                if (obj is IORMService && reader.LocalName == ((IORMService)obj).ObjectType.Name && reader.NodeType == XmlNodeType.Element && reader.Prefix == prefix)
//                {
//                    // This is a data object node, exit now,
//                    //return;
//                    break;
//                }

//                if (propertyCollection.ContainsKey(reader.LocalName) && reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.EndElement)
//                {
//                    continue;
//                }
//                else if (propertyCollection.ContainsKey(reader.LocalName) && reader.NodeType == XmlNodeType.Element)
//                {

//                    FieldInfo currentField = propertyCollection[reader.LocalName];

//                    if (currentField != null)
//                    {
                     
//                        if (currentField.FieldType.GetInterface("IXmlSerializable") != null)
//                        {
//                            object newObject = Activator.CreateInstance(currentField.FieldType, null);
//                            IXmlSerializable iSer = (IXmlSerializable)newObject;
//                            iSer.ReadXml(reader);
//                            currentField.SetValue(obj, newObject);
//                        }

//                        else if (currentField.FieldType.GetInterface("IList") != null)
//                        {
//                            IList list = (IList)Activator.CreateInstance(currentField.FieldType, null);
//                            string typeFullName = reader.GetAttribute("contains-type");
//                            Type listType = Type.GetType(typeFullName, true, true);
//                            ReadIList(reader, list, prefix, currentField.Name, listType);
                           
//                            currentField.SetValue(obj, list);

//                        }
//                        else if (currentField.FieldType.GetInterface("IEnumerable") != null && currentField.FieldType != typeof(String))
//                        {
//                            ArrayList list = new ArrayList();
//                            string typeFullName = reader.GetAttribute("contains-type");
//                            Type listType = Type.GetType(typeFullName, true, true);
//                            while (reader.Read())
//                            {
//                                if (reader.LocalName == currentField.Name && reader.Prefix == prefix && reader.NodeType == XmlNodeType.EndElement) break;
//                                object newObject = Activator.CreateInstance(listType, null);
//                                IXmlSerializable iSer = (IXmlSerializable)newObject;
//                                iSer.ReadXml(reader);
//                                list.Add(newObject);
//                            }

//                            IEnumerable ienumerable = (IEnumerable)list;
//                            currentField.SetValue(obj, ienumerable);
//                        }
//                        else if (currentField.FieldType.BaseType == typeof(XmlNode))
//                        {
//                            string value = reader.ReadInnerXml();
//                            XmlDocument document = new XmlDocument();
//                            if (value != null && value.Length > 0)
//                            {
//                                document.LoadXml(value);
//                            }

//                            fieldBuffer.Add(((MemberInfo)currentField), value);
//                        }
//                        else
//                        {
//                            object tempValue = reader.ReadString();
//                            fieldBuffer.Add(((MemberInfo)currentField), tempValue);
//                            //SetValue(currentField, obj, tempValue);
//                        }
//                    }

//                    if (action == Action.Save)
//                    {
//                        // load temporary object
//                        //object tempSourceObject = Activator.CreateInstance(obj.GetType(), null);
//                        foreach (DictionaryEntry entry in fieldBuffer)
//                        {
//                            SetValue(((FieldInfo)entry.Key), obj, entry.Value);
//                        }

//                        ORMObject tempObj = ORM.Instanciate(obj);
//                        tempObj.Load();


//                        //MapMember[] pks = tempObj.ObjectInfo.MappedMembers.GetAllPrimaryKeyMembers();


//                        //foreach (MapMember member in pks)
//                        //{
//                        //    object readValue = fieldBuffer[member.Member];
//                        //    member.SetValue(obj, readValue);
//                        //}

                        
//                    }

//                    foreach (DictionaryEntry entry in fieldBuffer)
//                    {
//                        SetValue(((FieldInfo)entry.Key), obj, entry.Value);
//                    }

//                }
//                else if (!propertyCollection.ContainsKey(reader.LocalName) && reader.NodeType == XmlNodeType.EndElement)
//                {
//                    break;
//                }

      
//            }

//        }

//        private static void ReadIList(XmlReader reader, object listObj, string prefix, string objectName, Type listType)
//        {
//            if (listObj == null) return;
//            while (reader.Read())
//            {
//                if (reader.LocalName == objectName && reader.Prefix == prefix && reader.NodeType == XmlNodeType.EndElement) break;
//                if (reader.NodeType == XmlNodeType.EndElement) continue;

//                object newObject = null;
//                if (listType != null && listType.GetInterface("IXmlSerializable", true) != null)
//                {
//                    newObject = Activator.CreateInstance(listType, null);
//                    IXmlSerializable iSer = (IXmlSerializable)newObject;
//                    iSer.ReadXml(reader);
//                }
//                else
//                {
//                    newObject = ReadNativeObject(reader, listType);
//                }

//                IList list = listObj as IList;
//                list.Add(newObject);

//            }
//        }


//        internal static void WriteXml(XmlWriter writer, object obj, string prefix, string ns)
//        {
//            if (obj == null) return;
//            Type type = obj.GetType();
//            string typeName = type.Name;

//            #region -- Write XML for IEnumerable Types -- 
            
//            if (obj.GetType().GetInterface("IEnumerable") != null)
//            {
//                IEnumerable collection = obj as IEnumerable;

//                foreach (object objTemp in collection)
//                {
//                    if (objTemp is IXmlSerializable)
//                    {
//                        IXmlSerializable iSer = (IXmlSerializable)objTemp;
//                        iSer.WriteXml(writer);
//                    }
//                    else
//                    {

//                        WriteNativeObject(writer, objTemp, ns, prefix);
//                    }
//                }

//                return;// exit...
//            }
            
//#endregion


//            FieldInfo[] fieldList = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);


//            Dictionary<string, FieldInfo> fieldDictionary = new Dictionary<string, FieldInfo>();
//            foreach (FieldInfo currentFieldInfo in fieldList)
//            {
//                // Do not serialize if has a NonSerialized attribute.
//                NonSerializedAttribute[] nonSerializedAttributes = (NonSerializedAttribute[])currentFieldInfo.GetCustomAttributes(typeof(NonSerializedAttribute), true);
//                if (nonSerializedAttributes.Length > 0) continue;
                
//                if (currentFieldInfo.FieldType.GetInterface("IXmlSerializable") == null)
//                {
//                    if (currentFieldInfo.FieldType.IsPrimitive || currentFieldInfo.FieldType == typeof(String))
//                    {
//                        object value = currentFieldInfo.GetValue(obj);
//                        if (value == null) continue;

//                        string convertedValue = Convert.ToString(value);
//                        if (convertedValue.Length == 0) continue;

//                        writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                        writer.WriteValue(convertedValue);
//                        writer.WriteEndElement();
//                    }
//                    else if (currentFieldInfo.FieldType.IsEnum)
//                    {
//                        object value = currentFieldInfo.GetValue(obj);
//                        if (value == null) continue;

//                        string name = Enum.GetName(currentFieldInfo.FieldType, value);
//                        writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                        writer.WriteValue(Convert.ToString(name));
//                        writer.WriteEndElement();
//                    }
//                    else if (currentFieldInfo.FieldType == typeof(DateTime))
//                    {
//                        object value = currentFieldInfo.GetValue(obj);
//                        DateTime tmpDT = Convert.ToDateTime(value);
//                        if (tmpDT == DateTime.MinValue || tmpDT == DateTime.MaxValue) continue;
//                        string dateFormatted = String.Format("{0:yyyy-MM-ddThh:mm:ss.fffzz}", tmpDT);
//                        writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                        writer.WriteValue(dateFormatted);
//                        writer.WriteEndElement();
//                    }
//                    else if (currentFieldInfo.FieldType.BaseType == typeof(XmlNode))
//                    {
//                        XmlNode value = (XmlNode)currentFieldInfo.GetValue(obj);
                        
//                        writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                        if (value != null)
//                        {
//                            writer.WriteCData(value.OuterXml);
//                        }
//                        writer.WriteEndElement();
//                    }
//                    else if (currentFieldInfo.FieldType.GetInterface("IEnumerable") != null)
//                    {
//                        Debug.WriteLine("collection object found. field name: " + currentFieldInfo.Name + " type: " + typeName);
//                        XmlElementAttribute[] xmlElementAttrArray = (XmlElementAttribute[])currentFieldInfo.GetCustomAttributes(typeof(XmlElementAttribute), false);
//                        if (xmlElementAttrArray == null)
//                        {
//                            xmlElementAttrArray = (XmlElementAttribute[])currentFieldInfo.FieldType.GetCustomAttributes(typeof(XmlElementAttribute), false);
//                        }

//                        if (xmlElementAttrArray != null && xmlElementAttrArray.Length > 0)
//                        {
//                            XmlElementAttribute xmlElementAttr = xmlElementAttrArray[0];
//                            object value = currentFieldInfo.GetValue(obj);
//                            if (value == null) continue;
//                            if (value is IEnumerable)
//                            {
//                                IEnumerable collection = value as IEnumerable;

//                                writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                                writer.WriteAttributeString("contains-type", xmlElementAttr.Type.AssemblyQualifiedName);
//                                foreach (object objTemp in collection)
//                                {
//                                    if (objTemp is IXmlSerializable)
//                                    {
//                                        IXmlSerializable iSer = (IXmlSerializable)objTemp;
//                                        iSer.WriteXml(writer);
//                                    }
//                                    else
//                                    {
//                                        WriteNativeObject(writer, objTemp, ns, prefix);
//                                    }
//                                }
//                                writer.WriteEndElement();
//                            }
//                        }
//                    }
//                    else
//                    {
//                        Debug.WriteLine("IXmlSerializable object found. field name: " + currentFieldInfo.Name + " type: " + typeName);
//                        object value = currentFieldInfo.GetValue(obj);
//                        if (value == null) continue;
//                        writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                        // writer.WriteAttributeString("type", currentFieldInfo.DeclaringType.FullName);
//                        writer.WriteValue(Convert.ToString(value));
//                        writer.WriteEndElement();
//                    }
//                }
//                else
//                {
//                    writer.WriteStartElement(prefix, currentFieldInfo.Name, ns);
//                   // writer.WriteAttributeString("type", currentFieldInfo.DeclaringType.FullName);
//                    IXmlSerializable iSer = (IXmlSerializable)currentFieldInfo.GetValue(obj);
//                    if (iSer != null)
//                    {
//                        iSer.WriteXml(writer);
//                    }
//                    writer.WriteEndElement();
//                }
//            }

// //           writer.WriteEndElement();

//        }

//        internal static string SerializeToString(Object obj, string prefix, string ns)
//        {
//            StringBuilder builder = new StringBuilder(5000);
//            StringWriter writer = new StringWriter(builder);

//            Serialize(writer, obj, prefix, ns);
//            return builder.ToString();
//        }

//        internal static string SerializeToString(Object obj, Type rootType, string prefix, string ns)
//        {
//            return SerializeToString(obj, rootType, null, prefix, ns);
//        }

//        internal static string SerializeToString(Object obj, Type rootType, string root, string prefix, string ns)
//        {
//            StringBuilder builder = new StringBuilder(5000);
//            StringWriter writer = new StringWriter(builder);

//            Serialize(writer, obj, rootType, root, prefix, ns);
//            return builder.ToString();
//        }

//        internal static void Serialize(string filepath, object obj, string prefix, string ns)
//        {
//            using (StreamWriter stream = File.CreateText(filepath))
//            {
//                Serialize(stream, obj, prefix, ns);
//            }
//        }

//        internal static void Serialize(TextWriter textWriter, object obj, string prefix, string ns)
//        {
//            Serialize(textWriter, obj, null, null, prefix, ns);
//        }

        
//        internal static void Serialize(TextWriter textWriter, object obj, Type typeRoot, string root, string prefix, string ns)
//        {

//            XmlWriterSettings settings = new XmlWriterSettings();
//            settings.CheckCharacters = true;
//            settings.OmitXmlDeclaration = true;
//            settings.ConformanceLevel = ConformanceLevel.Auto;
//            settings.Indent = true;


//            XmlWriter writer = XmlWriter.Create(textWriter, settings);
//            writer.WriteStartDocument();

//            if (typeRoot != null)
//            {
//                if (root != null && root.Length > 0)writer.WriteStartElement(prefix, root, ns);
//                writer.WriteAttributeString("type", typeRoot.AssemblyQualifiedName);
//            }

//            XmlSerializerUtil.WriteXml(writer, obj, prefix, ns);

//            if (obj is ORMObject)
//            {
//                ORMObject tempObj = (ORMObject)obj;
//                writer.WriteStartElement(prefix, "actionResult", ns);
//                writer.WriteValue(tempObj.Result);
//                writer.WriteEndElement();
//            }

//            if (typeRoot != null)
//            {
//                writer.WriteEndElement();
//            }

//            writer.WriteEndDocument();
//            writer.Flush();
//            writer.Close();
//        }

//        internal  static object Deserialize(string xml, Type type, string root, string prefix)
//        {
//            TextReader reader = new StringReader(xml);

//            return Deserialize(reader, type, root, prefix);
//        }


//        internal static object Deserialize(string xml, Type type, string prefix)
//        {
//            return Deserialize(xml, type, null, prefix);
//        }


//        internal static object Deserialize(TextReader textReader, Type type, string prefix)
//        {
//            return Deserialize(textReader, type, null, prefix);
//        }

//        internal static object Deserialize(XmlReader reader, Type type, string root, string prefix, Action action)
//        {
//            string name = (root != null && root.Length > 0) ? root : type.Name;
//            object obj = Activator.CreateInstance(type);
//            if (root != null && root.Length > 0)
//            {
//                do
//                {
//                    if (reader.LocalName == name && reader.Prefix == prefix && reader.NodeType == XmlNodeType.Element)
//                    {
//                        XmlSerializerUtil.ReadXml(reader, name, obj, prefix, action);
//                    }

//                    if (reader.LocalName == name && reader.Prefix == prefix && reader.NodeType == XmlNodeType.EndElement) break;
//                } while (reader.Read()) ;
//            }
//            else
//            {
//                XmlSerializerUtil.ReadXml(reader, obj, prefix);
//            }

//            return obj;
//        }

//        internal static object Deserialize(TextReader textReader, Type type, string root, string prefix)
//        {
            
//            XmlReaderSettings settings = new XmlReaderSettings();
//            settings.CheckCharacters = true;
//            settings.IgnoreComments = true;
//            settings.IgnoreProcessingInstructions = true;
//            settings.IgnoreWhitespace = true;
            
//            string name = (root != null && root.Length > 0)? root : type.Name;
//            XmlReader reader = XmlReader.Create(textReader, settings);

//            object obj = Activator.CreateInstance(type);
//            if (root != null && root.Length > 0)
//            {
//                while (reader.Read())
//                {
//                    if (reader.LocalName == name && reader.Prefix == prefix && reader.NodeType == XmlNodeType.Element)
//                    {
//                        XmlSerializerUtil.ReadXml(reader, name, obj, prefix, Action.Undefined);
//                    }
//                }
//            }
//            else
//            {
//                XmlSerializerUtil.ReadXml(reader, obj, prefix);
//            }


//            return obj;
//        }

//        internal static void SetValue(FieldInfo fieldInfo, object obj, object newValue)
//        {

//            object convertedValue = null;
//            if (newValue != null)
//            {
//                // if (( !fieldInfo.FieldType.IsEnum) &&  Nullable.GetUnderlyingType(fieldInfo.FieldType) == null && fieldInfo.FieldType != typeof(Guid))
//                // {
//                //     convertedValue = Convert.ChangeType(newValue, fieldInfo.FieldType);
//                // }
//                if (Nullable.GetUnderlyingType(fieldInfo.FieldType) != null)
//                {
//                    if (newValue is string && Convert.ToString(newValue).Length == 0)
//                    {
//                        newValue = null;
//                    }

//                    Type type = Nullable.GetUnderlyingType(fieldInfo.FieldType);
//                    convertedValue = Convert.ChangeType(newValue, type);
//                }
//                else if (fieldInfo.FieldType.IsEnum)
//                {
//                    string tempValue = Convert.ToString(newValue);
//                    convertedValue = Enum.Parse(fieldInfo.FieldType, tempValue, true);
//                }
//                else if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType == typeof(String) || fieldInfo.FieldType == typeof(DateTime))
//                {
//                    convertedValue = Convert.ChangeType(newValue, fieldInfo.FieldType);
//                }
//                else if (fieldInfo.FieldType == typeof(Guid))
//                {
//                    string tmpGuid = Convert.ToString(newValue);
//                    if (tmpGuid.Length > 0)
//                    {
//                        convertedValue = new Guid(tmpGuid);
//                    }
//                    else
//                    {
//                        convertedValue = Guid.Empty;
//                    }
//                }

//                else
//                {
//#if DEBUG
//                    string debugMsg = String.Format("The field name {0} has not been set with the value {1}. Type: {2}", fieldInfo.Name, newValue, obj.GetType().FullName);
//                    Debug.WriteLine(debugMsg, "DEBUGGiNG");
//#endif
//                    convertedValue = newValue;
//                    // do nothing.
//                    // left it blank.
//                }

//                fieldInfo.SetValue(obj, convertedValue);
//            }


//        }

//        private static  object ReadNativeObject(XmlReader reader, Type type)
//        {
//            string name = reader.LocalName;
//            string val = reader.ReadString();
//            //if (type == null)
//            //{
//            //    throw new ArgumentNullException("type", "type arg is null, The collection does not specified the contain-type attribute");
//            //}

//            if (type == null)
//            {
//                string qualifedTypeName = reader.GetAttribute("type");
//                if (qualifedTypeName != null && qualifedTypeName.Length > 0)
//                {
//                    type = Type.GetType(qualifedTypeName);
//                }
//            }

//            if (String.Compare(name, "Guid", true) == 0 || type == typeof(Guid))
//            {
//                return new Guid(val);
//            }
//            else if (type != null)
//            {
//                return Convert.ChangeType(val, type);
//            }
//            else
//            {
//                return (object)val;
//            }
//        }

//        private static void WriteNativeObject(XmlWriter writer, object obj, string ns, string prefix)
//        {
//            if (obj == null) return;
//            Type type = obj.GetType();

//            if (type.IsPrimitive || type.IsEnum || type == typeof(String) || type == typeof(DateTime) || type == typeof(Guid))
//            {
//                string name = type.Name;
//                string value = Convert.ToString(obj);

//                writer.WriteStartElement(prefix, name, ns);
//                writer.WriteAttributeString("type", obj.GetType().AssemblyQualifiedName);
//                if (type == typeof(DateTime))
//                {
//                    value = String.Format("{0:yyyy-MM-ddThh:mm:ss.fffzz}", obj);
//                }
//                else if (type == typeof(Guid))
//                {
//                    value = String.Format("{0:N}", obj);
//                }
//                writer.WriteValue(obj);
//                writer.WriteEndElement();

//            }
//        }

//    }

//}
//#endif
