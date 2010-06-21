#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine
{
    public delegate  void ORMEventHandler(object sender, ORMEventArgs args);

    /// <summary>
    /// Represents a ORM Event argument.
    /// </summary>
    public class ORMEventArgs : EventArgs
    {
        private readonly object obj;

        public ORMEventArgs(object obj)
        {
           this.obj = obj;
        }

        public object DataObject
        {
            get { return obj; }
        }
    }

    
}
