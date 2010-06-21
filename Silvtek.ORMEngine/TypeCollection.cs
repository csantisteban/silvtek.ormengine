using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

namespace Silvtek.ORMEngine
{
	/// <summary>
	///  Represent a collection of <see cref="Type" />.
	/// </summary>
	public class TypeCollection : CollectionBase
	{
		///<summary>
		/// Gets <see cref="Type" /> instance by its type.
		///</summary>
		internal Type this[int index]
		{
			get {return (Type)InnerList[index];}
		}
		
		///<summary>
		/// Default Constructor. It initialize this class with default values.
		///</summary>
		///<remarks>
		/// It does not contains code inside.
		/// </remarks>
		internal TypeCollection()
		{
        }
		
		
		/// <summary>
		/// Adds an <see cref="ORMObject" /> to this instance.
		/// </summary>
		/// <returns>The index where the object added occupies. </returns>
		/// <param name="obj">The <see cref="ORMObject" /> from which its type
		/// will be added</param>
        public int Add(ORMObject obj)
        {
			return List.Add(obj.GetType());
        }
		
		/// <summary>
		/// Adds an <see cref="ORMObject" /> to this instance.
		/// </summary>
		/// <returns>The index where the object added occupies. </returns>
		/// <param name="type">The <see cref="Type" /> instance added,</param>
        public int Add(Type type)
        {
			return List.Add(type);
        }
		
	}
}
