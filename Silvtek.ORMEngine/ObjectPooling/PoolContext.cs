// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
//  NOTES 
//   i.e (#n Note)
//  

#if NET20
using System;

namespace Silvtek.ORMEngine.ObjectPooling
{
    internal class PoolContext
    {
        private static PoolContext context;

        private readonly PoolManager manager;

        private PoolContext()
        {
            manager = new PoolManager();
        }

        internal void RegisterType(Type type, int minObjects, int maxObjects)
        {
            manager.Register(type, minObjects, maxObjects);
        }

        internal void RegisterType(Type type)
        {
            manager.Register(type);
        }

        internal object GetObject(Type type)
        {
            return manager.GetObject(type);
        }

        internal void RemoveType(Type type)
        {
            manager.Remove(type);
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        internal static void SetDefaultObjectNumbers(int minObjects, int maxObjects)
        {
            PoolManager.DEFAULT_MAX_OBJECTS = minObjects;
            PoolManager.DEFAULT_MAX_OBJECTS = maxObjects;
        }

        internal static PoolContext Current
        {
            get
            {
                if (context != null) return context;
                lock(typeof(PoolContext))
                {
                    if (context == null)
                    {
                        context = new PoolContext();
                    }
                }

                return context;
            }
        }
    }
}
#endif