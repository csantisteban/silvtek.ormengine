// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
//  NOTES 
//   i.e (#n Note)
//  

#if NET20
using System;
using System.Collections;
using System.Reflection;
using Silvtek.ORMEngine.ObjectPooling;

namespace Silvtek.ORMEngine.ObjectPooling
{

    internal class PoolManager
    {
        internal static int DEFAULT_MIN_OBJECTS = 3;
        internal static int DEFAULT_MAX_OBJECTS = 25;

        internal Hashtable types;

        internal PoolManager()
        {
            types = new Hashtable();
        }

        internal void Register(Type type)
        {
            Register(type, DEFAULT_MIN_OBJECTS, DEFAULT_MAX_OBJECTS);
        }

        internal void Register(Type type, int minObjects, int maxObjects)
        {
            InternalPool pool = new InternalPool(type, minObjects, maxObjects);
            if (types.Contains(type))
            {
                types.Remove(type);
            }

            types.Add(type, pool);
        }

        internal void Remove(Type type)
        {
            types.Remove(type);
        }

        internal object GetObject(Type type)
        {
            InternalPool pool = (InternalPool) types[type];
            if (pool == null)
            {
                Register(type, DEFAULT_MIN_OBJECTS, DEFAULT_MAX_OBJECTS);
                return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
            }

            return pool.GetObject();
        }


    }
}
#endif