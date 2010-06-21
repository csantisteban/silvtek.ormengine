// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
// 
// VERSION CONTROL
//   $Revision: 2 $ 
//   $Author: carlos.santisteban $ 
//   $UTCDate: 2009-01-02 23:53:02 $ $UTCModtime: 2009-01-02 23:56:41 $
//   $Workfile: Pool.cs $
// 
//  DEVELOPER COMMENTS
//   i.e  (01/01/0001  Initials    Comment)
//   
//  
//  NOTES 
//   i.e (#n Note)
//  

namespace Silvtek.ORMEngine.ObjectPooling
{
#if NET20
    public class Pool<T>
    {
        public Pool()
        {
            PoolContext.Current.RegisterType(typeof(T));
        }

        public Pool(int minObjects, int maxObjects)
        {
            PoolContext.Current.RegisterType(typeof(T), minObjects, maxObjects);
        }

        public T GetObject()
        {
            return (T)PoolContext.Current.GetObject(typeof(T));            
        }

    }
#endif
}