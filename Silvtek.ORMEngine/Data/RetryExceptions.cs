using System;
using System.Collections;
using System.Xml;

namespace Silvtek.ORMEngine.Data
{
    public class RetryExceptions
    {

        private readonly Hashtable exceptionList;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RetryExceptions()
        {
            exceptionList = new Hashtable();
        }

      
        public void ReadDefaultXmlNode(XmlNode node)
        {
            if (node.LocalName != "retry-exceptions") return;

            XmlNodeList exceptionNodeList = node.SelectNodes("exception");
            if (exceptionNodeList == null)
            {
                return;
            }

            foreach (XmlNode entryNode in exceptionNodeList)
            {
                ExceptionEntry exceptionEntryEntry = new ExceptionEntry();
                exceptionEntryEntry.ReadDefaultXmlNode(entryNode);

                foreach (Action action in exceptionEntryEntry.Actions)
                {
                    string key = CreateKey(exceptionEntryEntry.Type, action);
                    exceptionList.Add(key, exceptionEntryEntry);
                }
               
            }

        }

        /// <summary>
        /// Adds programmatically an exception item to ORM configuration.
        /// </summary>
        /// <param name="retry">The retry.</param>
        /// <param name="actions">The actions.</param>
        public void AddRetryException(RetryExceptionMatch retry, Action[] actions)
        {
            ExceptionEntry exceptionEntryEntry = new ExceptionEntry(retry.Type, actions);
            foreach (Action action in actions)
            {
                string key = CreateKey(exceptionEntryEntry.Type, action);
                exceptionList.Add(key, exceptionEntryEntry);
            }
        }


        internal bool ShouldRetry(object instance, Action action, int lastCount)
        {
            Type type = instance.GetType();
            string key = CreateKey(type, action);
            ExceptionEntry entry = (ExceptionEntry)exceptionList[key];

            if (entry == null)
            {
                return false;
            }

            return entry.GetMatch(instance, lastCount);
        }

        private static string CreateKey(Type type, Action action)
        {
            return String.Format("{0}>>{1}", type.AssemblyQualifiedName, action);
        }
    }
}

