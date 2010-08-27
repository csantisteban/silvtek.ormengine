// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
//  NOTES 
//   i.e (#n Note)
//  


#if NET20

using System;

namespace Silvtek.ORMEngine.ObjectPooling
{
    public class ObjectPoolingException : Exception
    {
        public ObjectPoolingException(string msg) : base(msg) { }
    }
}

#endif