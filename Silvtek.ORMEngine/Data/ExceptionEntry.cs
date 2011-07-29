using System;
using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Xml;

namespace Silvtek.ORMEngine.Data
{
    /// <summary>
    /// Represents a retry entry
    /// </summary>
    public class ExceptionEntry
    {

        private string typeName;
        private ArrayList matches;
        private Action[] actions;
        private Type type;


     
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntry"/> class.
        /// </summary>
        public ExceptionEntry()
        {
            typeName = String.Empty;
            matches = new ArrayList();
            actions = new Action[0];
            type = null;
        }

        public ExceptionEntry(Type type, Action[] actions)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type is null");
            }

            this.type = type;

            this.actions = actions;
            typeName = type.Name;

            matches = new ArrayList();
        }


        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName
        {
            get { return typeName; }
        }


        /// <summary>
        /// Gets the actions.
        /// </summary>
        /// <value>The actions.</value>
        public Action[] Actions
        {
            get { return actions; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type
        {
            get { return type; }
        }



        internal bool MatchAction(Action action)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                Action tmp = actions[i];
                if (tmp == action) return true;
            }

            return false;
        }

        // read an retry entry
        // <exception type="SqlException" actions="Create,Save">
        //  <match property="" regex="" retries="" />
        //  <match property="" regex="" retries="" />
        // </exception>
        internal void ReadDefaultXmlNode(XmlNode node)
        {
            if (node.LocalName != "exception") return;

            typeName = Utility.GetAttributeValue(node, "type");
            type = Type.GetType(typeName, true, true);

            XmlNode actionsNode = node.SelectSingleNode("actions");
            actions = ReadActions(actionsNode);

            XmlNodeList matchNodeList = node.SelectNodes("match");
            if (matchNodeList != null)
            {

                foreach (XmlNode tmpNode in matchNodeList)
                {
                    string property = Utility.GetAttributeValue(tmpNode, "property");
                    string match = Utility.GetAttributeValue(tmpNode, "regex");
                    int retries = Utility.GetAttributeValueAsInt32(tmpNode, "retries", false);

                    RetryExceptionMatch retryExceptionMatch = new RetryExceptionMatch(this.type, property, match, retries);
                    matches.Add(retryExceptionMatch);
                }
                     
            }
             
        }

        /// <summary>
        /// Return a <see cref="RetryExceptionMatch"/> if the value of the instance matches to the value configured.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        internal bool GetMatch(object instance, int counter)
        {
            foreach (RetryExceptionMatch tmpMatch in matches)
            {
                bool success = tmpMatch.ShouldRetry(instance, counter);
                if (success)
                {
                    return true;
                }
            }

            return false;
        }


        private Action[] ReadActions(XmlNode actionsNode)
        {
            Action[] tmpActions; 
            if (actionsNode != null)
            {
                string actionsRaw = Utility.GetNodeValue(actionsNode);
                if (actionsRaw.Length > 0 && actionsRaw != "All")
                {
                    string[] actionList = actionsRaw.Split(',');
                    ArrayList tmpList = new ArrayList(actionList.Length);
                    foreach (string str in actionList)
                    {
                        if (str == null || str.Length == 0) continue;
                        Action tmpAction = (Action) Enum.Parse(typeof (Action), str, true);
                        tmpList.Add(tmpAction);
                    }

                    tmpActions = (Action[])tmpList.ToArray(typeof(Action));
                }
                else
                {
                    tmpActions = new Action[4]
                                  {
                                      Action.Create, Action.Save, Action.Remove, Action.Load
                                  };
                }
            }
            else
            {
                tmpActions = new Action[4]
                              {
                                  Action.Create, Action.Save, Action.Remove, Action.Load
                              };
            }

            return tmpActions;
        }


    }
}
