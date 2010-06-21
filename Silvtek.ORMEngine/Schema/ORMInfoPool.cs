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
        public int Count
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
        public ObjectSchemaInfo Retrieve(Type type)
        {
            string typeFullName = type.FullName;

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
		public static ObjectSchemaContainer Current
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
		/// Returns the unique <see cref="ORMInfoPool" /> instance.
		///</summary>
		///<returns>
		/// The unique <see cref="ORMInfoPool" /> instance.
		///</returns>
		private static ObjectSchemaContainer GetInstance()
		{
			if (container != null)return container;

			lock(typeof(ObjectSchemaContainer))
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
