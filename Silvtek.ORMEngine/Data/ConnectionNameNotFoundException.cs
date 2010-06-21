#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.Data
{
	public class ConnectionNameNotFoundException: ApplicationException
	{
        private string msg = String.Empty;

        public override string Message
        {
          get
          {
            return msg;
          }
        }

		public ConnectionNameNotFoundException(string name)
		{
           // todo: it must come from resource file.
           msg = String.Format("The connection {0} does not exists. Connection must be configurated at connection.config file. ", name);

		}
	}
}
