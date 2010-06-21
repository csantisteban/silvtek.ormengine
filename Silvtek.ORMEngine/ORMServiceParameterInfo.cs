//using System;
//using System.Xml.Serialization;

//namespace Silvtek.ORMEngine
//{
//    [SoapType("Parameter")]
//    public class ORMServiceParameterInfo
//    {
//        private string name;
//        private string value;

//        [SoapElement("Name")]
//        public string Name
//        {
//            get { return name; }
//            set { name = value; }
//        }

//        [SoapElement("Value")]
//        public string Value
//        {
//            get { return value; }
//            set { this.value = value; }
//        }

//        internal ORMServiceParameterInfo()
//        {
//            value = String.Empty;
//            name = String.Empty;
//        }


//        public ORMServiceParameterInfo(string name, string value)
//        {
//            this.name = name;
//            this.value = value;
//        }

//    }
//}
