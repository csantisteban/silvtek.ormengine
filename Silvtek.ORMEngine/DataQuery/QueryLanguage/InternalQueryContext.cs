//using System;
//using System.Collections;
//using Silvtek.ORMEngine.Schema;


//namespace Silvtek.ORMEngine.QueryLanguage
//{
//    internal class InternalQueryContext : IQueryContext
//    {
//        private readonly Hashtable typeByFullNameList;
//        private readonly Hashtable typeByNameList;
//        private string command;
//       // private readonly Hashtable values;

//        internal InternalQueryContext()
//        {
//            typeByFullNameList = new Hashtable();
//            typeByNameList = new Hashtable();
//        }

//        internal string Command
//        {
//            get { return command; }
//        }

//        internal void SetCommand(string text)
//        {
//            this.command = text;
//        }

//        public Type GetType(string typeName)
//        {
//            ObjectSchemaInfo info = null;
//            if (typeByFullNameList.Contains(typeName))
//            {
//                info = (ObjectSchemaInfo) typeByFullNameList[typeName];
//            }
//            else if (typeByNameList.Contains(typeName))
//            {
//                info = (ObjectSchemaInfo)typeByNameList[typeName];
//            }

//            if (info != null)
//            {
//                return info.Type;
//            }

//            return null;
//        }

//        public void RegisterType(Type type)
//        {
//            if (type == null) return;

//            ObjectSchemaInfo  info = ORM.GetObjectInfo(type);
//            if (info == null)
//            {
//                throw new InvalidOperationException("Type "+type.FullName+" is not a valid ORM object");
//            }

//            if (type.FullName != null)
//            {
//                typeByFullNameList[type.FullName] = info;
//            }

//            typeByNameList[type.Name] = info;
//        }
//    }
//}
