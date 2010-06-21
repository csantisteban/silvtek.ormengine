//using System.Xml.Serialization;

//namespace Silvtek.ORMEngine
//{
//    /// <summary>
//    /// Represents the result for an action utilizing the ORM.
//    /// </summary>
//    [SoapType("ORMServiceResult", IncludeInSchema = true)]
//    public class ORMServiceResult
//    {
//        private bool isSuccess;
//        private ActionResult actionResult;
//        private ObjectSource objectSource;
//        private object obj;
//        private string message;
//        private string version;
//        private string build;

//        public ORMServiceResult()
//        {
//            isSuccess = true;
//            actionResult = ActionResult.Undefined;
//            obj = null;
//            objectSource = ObjectSource.Undefined;
//        }

//        public bool IsSuccess
//        {
//            get { return isSuccess; }
//            set { isSuccess = value; }
//        }

//        public ActionResult ActionResult
//        {
//            get { return actionResult; }
//            set { actionResult = value; }
//        }

//        [SoapElement("Data", IsNullable = true)]
//        public object Data
//        {
//            get { return obj; }
//            set { obj = value; }
//        }

//        [SoapElement("Message", IsNullable = true)]
//        public string Message
//        {
//            get { return message; }
//            set { message = value; }
//        }

//        [SoapElement("Version", IsNullable = true)]
//        public string Version
//        {
//            get { return version; }
//            set { version = value; }
//        }

//        public string Build
//        {
//            get { return build; }
//            set { build = value; }
//        }

//        public ObjectSource ObjectSource
//        {
//            get { return objectSource; }
//            set { objectSource = value; }
//        }

//        public static ORMServiceResult Create(bool isSuccess, ActionResult result, ObjectSource source, object obj)
//        {
//            ORMServiceResult tmp = new ORMServiceResult();
//            tmp.isSuccess = isSuccess;
//            tmp.actionResult = result;
//            tmp.obj = obj;
//            tmp.objectSource = source;
//            return tmp;
//        }
//    }
//}
