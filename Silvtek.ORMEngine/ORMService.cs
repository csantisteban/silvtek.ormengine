//#if !NET11
//using System;
//using System.Reflection;
//using System.Xml.Serialization;
//using Silvtek.ORMEngine.DataQuery;

//namespace Silvtek.ORMEngine
//{
//    ///<summary>
//    /// Base Data Service class. It provides methods for interaction with the data source, in 
//    /// this particular case using the Silvtek.ORMEngine.
//    ///</summary>
//    public class ORMService<T> : CRUDService<T>, IORMService 
//    {

//        /// <summary>
//        /// Only for deserialization, do no use it directly.
//        /// </summary>
//        private Action action;

//        /// <summary>
//        /// Only for deserialization, do no use it directly.
//        /// </summary>
//        private string method;

//        private ORMServiceParameterInfoCollection parameters;

        
//        [NonSerialized]
//        private ObjectSource objectSource;
       

//        [SoapElement("Action")]
//        public Action Action
//        {
//            get { return action; }
//            set { action = value; }
//        }

//        [SoapElement("Method")]
//        public string Method
//        {
//            get { return method; }
//            set { method = value; }
//        }

//        [SoapElement("ResultSource")]
//        [XmlElement("ResultSource")]
//        public ObjectSource ObjectSource
//        {
//            get { return objectSource; }
//            set { objectSource = value; }
//        }
        


//        [SoapElement("Parameters")]
//        [XmlElement("Parameters", IsNullable = true)]
//        public ORMServiceParameterInfoCollection Parameters
//        {
//            get 
//            {
//                if (parameters == null) parameters = new ORMServiceParameterInfoCollection();
//                return parameters;     
//            }
//        }
    
//        [SoapAttribute("NameSpace")]
//        [XmlAttribute("NameSpace")]
//        public string NameSpace
//        {
//            get { return typeof(T).Namespace; }
//        }

//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        protected ORMService()
//        {
//            action = Action.None;
//            parameters = null;
//            ObjectSource = ObjectSource.Undefined;
//        }

//        public virtual ORMServiceResult Find(string memberName, object searchValue)
//        {
//            Query query = new Query(typeof(T));
           
//            T[] result = (T[])query.Find(memberName, searchValue);
            
//            ObjectSource source = (query.IsResultsSetFromCache) ? ObjectSource.Cache : ObjectSource.Database;
//            ORMServiceResult serviceResult = ORMServiceResult.Create(true, ActionResult.Succeed, source, result);
//            return serviceResult;
         
//        }

//        public ORMServiceResult RunAction()
//        {
//            ORMServiceResult result = null;
//            switch (action)
//            {
//                case Action.Create:
//                    result = Create(CurrentObject);
//                    method = "Create";
//                    return result;
                    
//                case Action.Load:
//                    result = Load(CurrentObject);
//                    method = "Load";
//                    return result;
                   
//                case Action.Remove:
//                    result = Remove(CurrentObject);
//                    method = "Remove";
//                    return result;
                   
//                case Action.Save:
//                    result = Save(CurrentObject);
//                    method = "Save";
//                    return result;
                    
//                case Action.RunMethod:
//                    return RunMethod();

//                case Action.None:
//                case Action.Undefined:
//                    return null;
//                default:
//                    throw new NotSupportedException("The action " + action + " is not supported");
//            }

//        }

//        private ORMServiceResult RunMethod()
//        {
//            if (method == null || method.Length == 0) return null;
//            object result;
//            MethodInfo methodInfo = GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
//            if (methodInfo == null)
//            {
//                throw new MissingMethodException("Method '"+method+"' was not implemented for service of type "+GetType().FullName);
//            }

//            ParameterInfo[] parameterInfoList = methodInfo.GetParameters();
//            if (parameters != null && parameters.Count > 0 && parameterInfoList.Length > 0)
//            {
//                object[] actualValueParameters = new object[parameterInfoList.Length];

//                for (int i = 0; i < parameters.Count; i++)
//                {
//                    ParameterInfo paraInfo = parameterInfoList[i];
//                    object value = parameters.FindParameter(paraInfo.Name);
//                    object convertedValue = ConvertToMethodArgumentType(paraInfo, value) ?? String.Empty;
//                    actualValueParameters[i] = convertedValue;
//                }

//                result = methodInfo.Invoke(this, actualValueParameters);
//            }
//            else
//            {
//               result = methodInfo.Invoke(this , null);
//            }

//            if (result is ORMServiceResult)
//            {
//                return (ORMServiceResult)result;
//            }
//            else
//            {
//                return ORMServiceResult.Create(true, ActionResult.Succeed, ObjectSource.NoApplicable, result);
//            }
//        }

//        protected void AddMethodCallParameterInfo(ORMServiceParameterInfo parameter)
//        {
//            if (parameters == null) parameters = new ORMServiceParameterInfoCollection();
//            parameters.Add(parameter);
//        }

//       private object ConvertToMethodArgumentType(ParameterInfo fieldInfo, object value)
//        {
//            object convertedValue = null;
//            if (value != null)
//            {

//                if (Nullable.GetUnderlyingType(fieldInfo.ParameterType) != null)
//                {
//                    Type type = Nullable.GetUnderlyingType(fieldInfo.ParameterType);
//                    convertedValue = Convert.ChangeType(value, type);
//                }
//                else if (fieldInfo.ParameterType.IsEnum)
//                {
//                    string tempValue = Convert.ToString(value);
//                    convertedValue = Enum.Parse(fieldInfo.ParameterType, tempValue, true);
//                }
//                else if (fieldInfo.ParameterType.IsPrimitive || fieldInfo.ParameterType == typeof(String) || fieldInfo.ParameterType == typeof(DateTime))
//                {
//                    convertedValue = Convert.ChangeType(value, fieldInfo.ParameterType);
//                }
//                else if (fieldInfo.ParameterType == typeof(Guid))
//                {
//                    string tmpGuid = Convert.ToString(value);
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

//                    convertedValue = value;
//                    // do nothing.
//                    // left it blank.
//                }
//            }

//            return convertedValue;
//        }



//    }
//}
//#endif