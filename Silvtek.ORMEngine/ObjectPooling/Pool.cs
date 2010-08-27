// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
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