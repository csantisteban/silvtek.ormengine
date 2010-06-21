// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
// 
// VERSION CONTROL
//   $Revision: 2 $ 
//   $Author: carlos.santisteban $ 
//   $UTCDate: 2009-01-02 23:53:02 $ $UTCModtime: 2009-01-02 23:56:27 $
//   $Workfile: InternalPool.cs $
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
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Silvtek.ORMEngine.ObjectPooling
{
    internal class InternalPool
    {
        private delegate void ObjectCreation(int numberOfObjs);

        private readonly int minInstances;
        private readonly int maxInstances;
        private bool isReady;
        private readonly Type type;
        private readonly Queue queue;
        private readonly ArrayList newObjects;
        private event EventHandler OnLowCount;

        internal InternalPool(Type type, int minInstances, int maxInstances)
        {
            ValidateType(type);
            isReady = false;
            OnLowCount += new EventHandler(OnLowCountEvent);
            this.type = type;
            this.minInstances = minInstances;
            this.maxInstances = maxInstances;
            newObjects = ArrayList.Synchronized(new ArrayList());
            queue = Queue.Synchronized(new Queue());
            CreateObjects(maxInstances);
        }

        private void OnLowCountEvent(object sender, EventArgs e)
        {
            int diff = maxInstances - queue.Count;
            CreateObjects(diff);
        }

        internal object GetObject()
        {
            // if the pool is not ready or the queue is empty, then create the object on the fly.
            if (!isReady || queue.Count == 0)
            {
                return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
            }

            if (queue.Count <= minInstances)
            {
                // fire event!
                if (OnLowCount != null)OnLowCount(this, EventArgs.Empty);
            }

            return queue.Dequeue();
        }

        private void CreateObjects(int number)
        {
            ObjectCreation objCreation = new ObjectCreation(AsynchCreateObjects);
            objCreation.BeginInvoke(number, ObjectCreationCompleted, null);
        }

        private void AsynchCreateObjects(int numberOfObjs)
        {
            lock (newObjects.SyncRoot)
            {
                int exceptions = 0;
                for (int i = 0; i < numberOfObjs; i++)
                {
                    try
                    {
                        object pooledInstance = Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
                        if (pooledInstance != null)
                        {
                            newObjects.Add(pooledInstance);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions++;
                        if (exceptions > 3)
                        {
                            Trace.TraceError("Failed to create pool of objects for type: " + type.FullName + " error: " + ex.Message);
                  
                            break;
                        }


                        continue;
                    }
                }
            }
        }

        private void ObjectCreationCompleted(IAsyncResult result)
        {
            
            lock(queue.SyncRoot)
            {
                int count = newObjects.Count;
                for(int i = 0; i < count; i++)
                {
                    queue.Enqueue(newObjects[i]);
                }
            }

            lock(newObjects.SyncRoot)
            {
                newObjects.Clear();
            }

            if (!isReady) isReady = true;
        }


        private static void ValidateType(Type type)
        {
            ConstructorInfo info = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new Type[0], null);
            if (info == null || info.IsStatic)
            {
                throw new ObjectPoolingException("Default contructor must be present for pooling an object and cannot be static");
            }

        }
    }
}
#endif