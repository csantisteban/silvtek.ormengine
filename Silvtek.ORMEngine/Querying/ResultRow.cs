//using System;
//using System.Reflection;
//using System.Collections;
//using System.Collections.Specialized;
//using System.Diagnostics;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Summary description for Class.
//    /// </summary>
//    public sealed class ResultRow : IEnumerable	
//    {
//        private object[] objects;
//        private HybridDictionary objectTypes;		// Allows us to retrieve object by its type.
//        private int index;
//        private string hashcodeString;


//        public int Count
//        {
//          get {return objects.Length;}
//        }

//        public object this[int columnIndex]
//        {
//           get {return GetObject(columnIndex);}
//        }

//        public object this[Type type]
//        {
//            get {return GetObject(type);}
//        }

//        ///<summary>
//        /// Constructor.
//        ///</summary>
//        ///<param name="number">
//        /// The number of objects.
//        ///</param>
//        internal ResultRow(int number)
//        {
//            objects = new object[number];
//            objectTypes = new HybridDictionary(number);
//            index = 0;
//            hashcodeString = String.Empty;
//        }

//        internal void Add(ORMObject obj)
//        {
//            objects[index] = obj;
//            index++;
//            if (obj != null)
//            {
//                hashcodeString += obj.GetRecordIdentifier();
//                objectTypes.Add(obj.GetType(), obj);
//            }

//        }

//        internal void Add(object obj)
//        {
//            objects[index] = obj;
//            index++;

//            if (obj != null)
//            {
//                hashcodeString += obj.ToString();
//                objectTypes.Add(obj.GetType(), obj);
//            }
//        }
        

//        public ORMObject GetORMObject(int index)
//        {
//           return (ORMObject)objects[index];
//        }

//        public ORMObject GetORMObject(Type type)
//        {
//            return (ORMObject)objectTypes[type];
//        }

//        public object GetObject(int index)
//        {
//          return objects[index];
//        }

//        public object GetObject(Type type)
//        {
//            return objectTypes[type];
//        }

//        public override string ToString()
//        {
//          return  hashcodeString;
//        }

//        public override int GetHashCode()
//        {
//          return hashcodeString.GetHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//           if (obj.GetType() != GetType())return false;
//           ResultRow row = (ResultRow)obj;
//           return (hashcodeString == row.hashcodeString);
//        }


//        #region IEnumerable Members

//        public IEnumerator GetEnumerator()
//        {
//            if (objects == null)objects = new object[0];
//            return objects.GetEnumerator();
//        }

//        #endregion
//    }
//}

