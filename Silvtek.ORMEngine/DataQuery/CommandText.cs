#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Text.RegularExpressions;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery
{

    /// <summary>
    /// 
    /// </summary>
	public class CommandText
	{
        private const string _FIND_VARIABLES = @"@[\w\d]+";

		private readonly string text;
        private readonly CommandTypes registeredTypes;
        private Hashtable parameters;
        private ArrayList parameterslist;
        private Type mainType;

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
		public string Text
		{
			get {return text;}
		}

        internal CommandParameter[] GetParameters()
        {
            return (CommandParameter[])parameterslist.ToArray(typeof (CommandParameter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandText"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        public CommandText(Type type, string text)
        {
            mainType = type;
            registeredTypes = new CommandTypes();
            parameters = new Hashtable();
            parameterslist = new ArrayList();
            registeredTypes.Add(type);
            this.text = text;
            LoadParameters();
        }


        /// <summary>
        /// Registers the type to run this command. You must register types so they can be instanciated.
        /// </summary>
        /// <param name="type">The type.</param>
        public void RegisterType(Type type)
        {
            registeredTypes.Add(type);
        }

        /// <summary>
        /// Sets the parameter value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="type">The type.</param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        public void SetParameterValue(string parameter, Type type, string member, object value)
        {

            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);
            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter "+parameter+". Parameter not found in command text");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type", "type argument must be an instance of an object");
            }

            if (member == null)
            {
                throw new ArgumentNullException("member", "member argument must be an instance of an object and can not be an empty string");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value", "value argument must be an instance of an object");
            }

            CommandParameter cmdParameter = (CommandParameter) parameters[parameterID];
            cmdParameter.SetValue(type, member, value);
        }


        /// <summary>
        /// Sets the parameter value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="source">The source.</param>
        /// <param name="member">The member.</param>
        public void SetParameterValue(string parameter, object source, string member)
        {
            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);
            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter " + parameter + ". Parameter not found in command text");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source", "source argument must be an instance of an object");
            }

            if (member == null)
            {
                throw new ArgumentNullException("member", "member argument must be an instance of an object and can not be an empty string");
            }

            CommandParameter cmdParameter = (CommandParameter)parameters[parameterID];
            cmdParameter.SetValue(source, member);
        }

        public void SetParameterValue(string parameter, string value)
        {
            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);

            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter " + parameter + ". Parameter not found in command text");
            }

            CommandParameter cmdParameter = (CommandParameter)parameters[parameterID];
            cmdParameter.SetValue(value);
        }

        public void SetParameterValue(string parameter, int value)
        {
            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);

            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter " + parameter + ". Parameter not found in command text");
            }

            CommandParameter cmdParameter = (CommandParameter)parameters[parameterID];
            cmdParameter.SetValue(value);
        }

        public void SetParameterValue(string parameter, DateTime datetime)
        {
            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);

            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter " + parameter + ". Parameter not found in command text");
            }

            CommandParameter cmdParameter = (CommandParameter)parameters[parameterID];
            cmdParameter.SetValue(datetime);
        }

        public void SetParameterValue(string parameter, bool value)
        {
            if (parameter == null || parameter.Length == 0)
            {
                throw new ArgumentNullException("parameter", "parameter argument must be an instance of an object and can not be an empty string");
            }

            string parameterID = CommandParameter.GetParameterID(parameter);

            if (!parameters.Contains(parameterID))
            {
                throw new ORMException("Failed to set value for parameter " + parameter + ". Parameter not found in command text");
            }


            CommandParameter cmdParameter = (CommandParameter)parameters[parameterID];
            cmdParameter.SetValue(value);
        }

 
        private void LoadParameters()
        {
            Regex regex = new Regex(_FIND_VARIABLES);
            MatchCollection matches = regex.Matches(text);
            if (matches.Count == 0) return;

            DbTypeFormatter formatter = ORM.GetObjectInfo(mainType).DbTypeFormatter;
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                string parameter = match.Value;

                string parameterID = CommandParameter.GetParameterID(parameter);
                if (!parameters.Contains(parameterID))
                {

                    CommandParameter commandParameter = new CommandParameter(formatter, parameter);
                    parameters[commandParameter.ParameterID] = commandParameter;
                    parameterslist.Add(commandParameter);
                }
            }
        }

       internal ObjectSchemaInfo FindSchemaInfo(string name)
       {
           return registeredTypes.FindSchemaInfo(name);
       }


       

	}
}
