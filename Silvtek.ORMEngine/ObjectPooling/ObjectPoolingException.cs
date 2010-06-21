// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
// 
// VERSION CONTROL
//   $Revision: 2 $ 
//   $Author: carlos.santisteban $ 
//   $UTCDate: 2009-01-02 23:53:02 $ $UTCModtime: 2009-01-02 23:56:35 $
//   $Workfile: ObjectPoolingException.cs $
// 
//  DEVELOPER COMMENTS
//   i.e  (01/01/0001  Initials    Comment)
//   
//  
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