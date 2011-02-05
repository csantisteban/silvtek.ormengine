#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Diagnostics;


namespace Silvtek.ORMEngine.Schema
{
	/// <summary>
	///  Mantain a list of <see cref="ObjectSchemaInfo" /> instances.
    /// </summary>
    /// <remarks>
    /// This is a singleton.
    ///</remarks>
	internal sealed class ObjectSchemaContainer
	{
        private static object object_schema_container_mutex = new object(); 
        private static ObjectSchemaContainer container;     // Hold the unique instance of this class.
        private readonly Hashtable listholder;               // hold the ObjectSchemaInfo

        private readonly object mutex;


        ///<summary>
        /// Gets the number of <see cref="ObjectSchemaInfo" /> that this
        /// instance is holding.
        ///</summary>
        ///<value>
        /// The number of <see cref="ObjectSchemaInfo" /> that this instance
        /// is holding.
        ///</value>
        internal int Count
        {
            get {return listholder.Count;}
        }

            
 


        ///<summary>
        /// Returns a <see cref="ObjectSchemaInfo" /> from the pool.
        ///</summary>
        ///<param name="type">
        ///  The <see cref="Type"/> of the object for which the ObjectSchemaInfo will be returned.
        ///</param>
        ///<returns>
        /// A <see cref="ObjectSchemaInfo" />
        ///</returns>
        ///<seealso cref="Retrieve" />
        internal ObjectSchemaInfo Retrieve(Type type)
        {
            if (type == null)
            {
                return null;
            }

            string typeFullName = type.FullName;
            if (typeFullName == null) return null;

            ObjectSchemaInfo info = null;
            try
            {
                if (listholder.Contains(typeFullName))
                {
                    info = (ObjectSchemaInfo) listholder[typeFullName];
                }
                else
                {

                    lock (mutex)
                    {
                        info = ObjectSchemaInfoLoader.GetLoader(type);

                        if (!listholder.Contains(typeFullName))
                        {
                            // doubl check, if another thread has not added this instanceName
                            listholder.Add(typeFullName, info);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // RECHECK
                string traceMsg = Utility.GetResourceString("EXCEPTION_INFOPOOL", typeFullName, e.Message, e.StackTrace, DateTime.Now);
                Trace.WriteLine(traceMsg, Resources.TRACE_CATEGORY);
                throw new OrmMappingException(traceMsg);
            }

            return info;
        }

	    /// <summary>
		/// Returns the unique instance of this object.
		/// </summary>
        internal static ObjectSchemaContainer Current
		{
			get {return GetInstance();}
		}


        internal static ObjectSchemaInfo GetObjectSchema(Type type)
        {
            return Current.Retrieve(type);
        }

        /* constructor */
        private ObjectSchemaContainer()
        {
           listholder = new Hashtable(150);
           mutex = new object();
        }

		///<summary>
        /// Returns the unique <see cref="ObjectSchemaContainer" /> instance.
		///</summary>
		///<returns>
        /// The unique <see cref="ObjectSchemaContainer" /> instance.
		///</returns>
		private static ObjectSchemaContainer GetInstance()
		{
			if (container != null)return container;

            lock (object_schema_container_mutex)
			{
				if (container == null)
				{
					container = new ObjectSchemaContainer();
				}
			}

			return container;
		}



    }
}
