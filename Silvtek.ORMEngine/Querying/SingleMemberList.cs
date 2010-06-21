//using System;
//using System.Collections;

//namespace Silvtek.ORMEngine.Querying
//{
//    /// <summary>
//    /// Summary description for Class.
//    /// </summary>
//    public class SingleMemberList : CollectionBase
//    {
        
//        private bool hasAggregate;

//        ///<summary>
//        /// Gets whether this column list contain aggregates.
//        /// </summary>
//        public bool HasAggregate
//        {
//          get {return hasAggregate;}
//        }

//        ///<summary>
//        /// Gets a <see cref="SingleColumn" /> by its index.
//        ///</summary>
//        internal SingleMember this[int index]
//        {
//            get {return (SingleMember)InnerList[index];}
//        }
        
//        ///<summary>
//        /// Constructor
//        /// </summary>
//        internal SingleMemberList()
//        {
//          hasAggregate = false;
//        }

//        /// <summary>
//        ///  Creates and adds an <see cref="SingleColumn" /> to the collection.
//        /// </summary>
//        /// <returns>The index on which the <see cref="SingleColumn" /> is stored.</returns>
//        /// <param name="aggregate">Specify the type of aggregate.</param>
//        /// <param name="type">Specify the object type from which the column will be retrieved</param>
//        /// <param name="property">The property or member that contains the column information.</param>
//        /// <param name="distinctKeyword">Whether to add the 'distinct' sql keyword or not.</param>
//        public int Add(Aggregate aggregate, Type type, string property,	 bool distinctKeyword)
//        {
//          hasAggregate = (aggregate != Aggregate.None);
//          return List.Add(new SingleMember(aggregate, type, property, distinctKeyword));
//        }

//        /// <summary>
//        ///  Creates and adds an <see cref="SingleColumn" /> to the collection.
//        /// </summary>
//        /// <returns>The index on which the <see cref="SingleColumn" /> is stored.</returns>
//        /// <param name="type">Specify the object type from which the column will be retrieved</param>
//        /// <param name="property">The property or member that contains the column information.</param>
//      public int Add(Type type, string property)
//        {
//          return List.Add(new SingleMember(type, property, false));
//        }

//    }
//}
