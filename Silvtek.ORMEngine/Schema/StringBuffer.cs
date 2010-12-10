#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

namespace Silvtek.ORMEngine.Schema
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections;


    internal sealed class StringBuffer
    {

        private readonly string template;
        private readonly int initSize;
        private readonly Hashtable table;
        private readonly char tokenStartMarker;
        private readonly char tokenEndMarker;
        private string defaultstring;
        // ReSharper disable InconsistentNaming
        private static readonly char[] SPECIAL_CHARS = {'{', '}', '(', ')', '$', '^', '&', '|',
                                        '[', ']', '?', '+', '*', '~', '%'};
        // ReSharper restore InconsistentNaming


        private const string PATTERN = @"({0}[\w\d]+{1})";


        private Regex regex;


        ///<summary>
        /// Gets or sets the default string.
        ///</summary>
        internal string DefaultString
        {
            get { return defaultstring; }
            set { defaultstring = value; }
        }


        internal StringBuffer(string str)
            : this(str, '{', '}', String.Empty)
        {
            this.initSize = 0;
        }


        internal StringBuffer(string template, string defaultValue) :
            this(template, '{', '}', defaultValue)
        {
            this.initSize = 0;
        }


        internal StringBuffer(string template, int initSize) :
            this(template, '{', '}', String.Empty)
        {
            this.initSize = initSize;
        }

        internal StringBuffer(string template, char tokenMarker) :
            this(template, tokenMarker, tokenMarker, String.Empty)
        {
            this.initSize = 0;
        }


        internal StringBuffer(string template, char tokenStartMarker, char tokenEndMarker, string defaultValue)
        {
            this.initSize = 0;
            this.tokenStartMarker = tokenStartMarker;
            this.tokenEndMarker = tokenEndMarker;
            this.template = template;
            defaultstring = defaultValue;

            string exp = CreateExpression();
            regex = new Regex(exp);

            MatchCollection matches = regex.Matches(template);

            table = Hashtable.Synchronized(new Hashtable(matches.Count + 1));

            foreach (Match match in matches)
            {
                if (!table.Contains(match.Value))
                {
                    table.Add(match.Value, defaultstring);
                }
            }

        }



        internal void SetTokenValue(string token, object val)
        {
            string tempToken = String.Format("{0}{1}{2}", tokenStartMarker,
                                  token, tokenEndMarker);

            if (table.Contains(tempToken))
            {
                table[tempToken] = val;
            }
            else
            {
                throw new StringBufferException("Trying to set a value to " +
                                                "non-existance token. Token name: " + tempToken);
            }

        }

        internal void SetTokenValue(int token, object val)
        {
            SetTokenValue(Convert.ToString(token), val);
        }



        internal void ResetTo(string val)
        {
            lock (table.SyncRoot)
            {
                string[] aKeys = new string[table.Keys.Count];

                table.Keys.CopyTo(aKeys, 0);

                for (int i = 0; i < aKeys.Length; i++)
                {
                    string key = aKeys[i];
                    table[key] = val;
                }
            }
        }

        internal void Reset()
        {
            ResetTo(defaultstring);
        }



        internal StringBuilder BuildString()
        {
            StringBuilder builder;

            if (initSize <= 0)
            {
                int size = Convert.ToInt32(template.Length + (template.Length * 0.5));
                builder = new StringBuilder(template, size);
            }
            else
            {
                builder = new StringBuilder(template, initSize);
            }


            lock (this)
            {
                int count = table.Keys.Count;
                ArrayList keylist = new ArrayList(table.Keys);
                for (int i = 0; i <= count - 1; i++)
                {
                    string key = (string)keylist[i];
                    builder.Replace(key, (table[key] != null) ? Convert.ToString(table[key]) : String.Empty);
                }

            }

            return builder;
        }



        internal string CreateExpression()
        {
            string tmpStartToken = tokenStartMarker.ToString();
            string tmpEndToken = tokenEndMarker.ToString();

            for (int i = 0; i < SPECIAL_CHARS.Length; i++)
            {
                if (tokenStartMarker == SPECIAL_CHARS[i])
                {
                    tmpStartToken = @"\" + tokenStartMarker;
                }

                if (tokenEndMarker == SPECIAL_CHARS[i])
                {
                    tmpEndToken = @"\" + tokenEndMarker;
                }

            }

            return String.Format(PATTERN, tmpStartToken, tmpEndToken);

        }


        public override string ToString()
        {
            StringBuilder builder = BuildString();
            return builder.ToString();
        }
    }
}
