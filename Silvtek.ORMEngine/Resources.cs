using System;
using System.Reflection;
using System.Resources;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Summary description for Class.
  	/// </summary>
	internal sealed class Resources
	{

        private ResourceManager manager;

        internal const string TRACE_CATEGORY = "ORM Engine";

		/// <summary>
		///	Indexer to get the value of the key.
		/// </summary>
		internal string this [ string key ]
		{
			get { return manager.GetString(key); }
		}

        internal string GetString(string key)
        {
            string val = manager.GetString(key);
            if (val == null)val = String.Format("[Resource '{0}' has been not found]", key);
            return val;
        }

        internal string GetString(string key, params object[] args)
        {
            string text = manager.GetString(key);

            if (text == null)
            {
               text = String.Format("[Resource '{0}' has been not found]", key);
            }
            else
            {
                text = String.Format(text, args);
            }

            return text;
        }


        internal object GetObject(string key)
        {
          return manager.GetObject(key);
        }


		/// <summary>
		///	Constructor to initialize the resource manager
		///	instance variable.
		/// </summary>
		internal Resources()
		{
            manager = new ResourceManager(typeof(Resources).Namespace + ".Resource",  Assembly.GetExecutingAssembly());
		}

	}



}
