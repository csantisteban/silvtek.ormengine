#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion


using System;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Data;


namespace Silvtek.ORMEngine.Caching
{
	/// <summary>
	/// Manages ORM objects accross the application.
	/// </summary>
	internal class GlobalReferencesManager
	{

		private const int DEFAULT_TIMER_FREQUENCY = 60000;
        private static GlobalReferencesManager globalmanager;
        private static int timerFrequency = DEFAULT_TIMER_FREQUENCY;
        private readonly DateTime recordVersionFileCleanedDateTime;

       #region -- Member variables --
      
        private long instanceCounter;
        private bool isCleaning;
		private  bool isTimerStarted;
        private readonly Hashtable container;
	    private readonly Hashtable mutexCollection;

        private static readonly Object mutex  = new object();
		private readonly Timer cleaningTimer;

       #endregion

		/// <summary>
		/// Gets the number of instances being managed.
		/// </summary>
		internal long InstanceCounter
		{
			get {return instanceCounter;}
		}
		
		/// <summary>
		/// Gets the unique instance of this class.
		/// </summary>
		internal static GlobalReferencesManager Current
		{
			get {return GetSingleton();}
		}

      	/// <summary>
      	/// Adds a new object to the reference manager.
      	/// </summary>
      	/// <param name="orm">The object to be added. It must be an <see cref="ORMObject" /></param>
        // Note, if it is a new reference, then it should have come directly from the database.
		internal void Add(ORMObject orm)
		{
			if (orm == null)return;
            if (!ORMConfigurator.GlobalCacheEnabled && !ORMConfigurator.UpdateBroadCastingEnabled) return;
            if (orm.ObjectInfo.IsIsolated) return;

            // never cache a member that has binary data on it.
            if (orm.ObjectInfo.MappedMembers.HasBinaryMember) return;
           
            string key = orm.GetRecordIdentifier();

            // Do not add if the object does not have an identifier.
            if (key == null || key.Length == 0) return;


			// Debug.Assert(key != null && key.Length > 0, "Key passed to the global reference manager is invalid: ["+key+"]");
			

			ORMReferencesManager recordManager = (ORMReferencesManager)container[key];

            if (recordManager == null)
            {
                try
                {
                    object keymutex = mutexCollection[key];

                    if (keymutex == null)
                    {
                        keymutex = new object();
                        mutexCollection[key] = keymutex;
                    }

                    lock (keymutex)
                    {
                        recordManager = new ORMReferencesManager(key, orm);
                        container[key] = recordManager;
                    }
                }
                catch (Exception ex)
                {
                    //Trace.WriteLine(ex.Message);
                    return;
                }

            }

      	    WeakReference reference = new WeakReference(orm.SourceObject);	
		

			recordManager.Add(reference);
			Interlocked.Increment(ref instanceCounter);
			StartTimer();
		}

        /// <summary>
        /// Broadcast an update through all instances that represents the same record.
        /// </summary>
        /// <param name="obj"></param>
		internal void BroadcastUpdate(ORMObject obj)
		{
            if (!ORMConfigurator.UpdateBroadCastingEnabled) return;
			string  key = obj.GetRecordIdentifier();
			if (container.Contains(key))
			{
				ORMReferencesManager recordManager = (ORMReferencesManager)container[key];
				if (recordManager != null)
				{
                    try
                    {
                        lock (recordManager.SyncRoot)
                        {
                            recordManager.BroadcastUpdate(obj);
                        }
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine("Failed to broadcast update. "+ex.Message);

#if DEBUG
                    throw;
#endif
                    }
				}
			}

		
		}

		/// <summary>
		/// Broadcast record removal through all instance that reprensents the record being removed.
		/// </summary>
		/// <param name="obj"></param>
		internal void BroadcastRemoval(ORMObject obj)
		{
			string key = obj.GetRecordIdentifier();
			if (container.Contains(key))
			{
				ORMReferencesManager recordManager = (ORMReferencesManager)container[key];
				if (recordManager != null)
				{
                    try
                    {
                        lock (recordManager.SyncRoot)
                        {
                            recordManager.BroadcastRemoval(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Failed to record deletion . " + ex.Message);
#if DEBUG
                    throw;
#endif
                    }
				}
			}
			
		}

		/// <summary>
		/// Returns the database object by its unique reference manager key.
		/// </summary>
        /// <param name="ormKey">he object reference manager key.</param>
		/// <returns>The object being retrieved.</returns>
		internal object GetDataObjectFrom(string ormKey)
		{
            if (!ORMConfigurator.GlobalCacheEnabled) return null;
          
			object ormObject = null;
          
			if (container.Contains(ormKey))
			{
				ORMReferencesManager doRM = (ORMReferencesManager)container[ormKey];
				ormObject = doRM.Get();
			}
           
			return ormObject;
		}

		/// <summary>
		/// Returns the manager for a specific record.
		/// </summary>
        /// <param name="ormKey">The record key.</param>
		/// <returns></returns>
        internal ORMReferencesManager GetManagerInstance(string ormKey)
		{
		    ORMReferencesManager dom = (ORMReferencesManager) container[ormKey];
		
		    return dom;
		}


	    // returns the unique instance of this class.
		private static GlobalReferencesManager GetSingleton()
		{
			if (globalmanager != null)return globalmanager;

			lock(typeof(GlobalReferencesManager))
			{
				if (globalmanager == null)
				{
					globalmanager = new GlobalReferencesManager();
				}
			}

			return globalmanager;
		}

        internal static void ResetCache()
        {
            if (globalmanager != null)
            {
                lock (typeof(GlobalReferencesManager))
                {
                    globalmanager = new GlobalReferencesManager();
                }
            }
        }

		// Constructor.
		private GlobalReferencesManager()
		{
			isCleaning = false;
			isTimerStarted = false;
            recordVersionFileCleanedDateTime = DateTime.MinValue;

			container = new Hashtable(256);
            mutexCollection = new Hashtable(256);

			TimerCallback callback = new TimerCallback(CleanCollectedObjects);
			cleaningTimer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);
		}

		private void StartTimer()
		{
			if(!isTimerStarted)
			{
				cleaningTimer.Change(timerFrequency, timerFrequency);
                isTimerStarted = true;
			}
		}

		private void StopTimer()
		{
		    cleaningTimer.Change(Timeout.Infinite, Timeout.Infinite);
			timerFrequency = DEFAULT_TIMER_FREQUENCY;
		    isTimerStarted = false;
		}

        //private void IncreaseFrequency()
        //{
        //    timerFrequency = (int)(timerFrequency * 1.5);
        //    cleaningTimer.Change(timerFrequency, timerFrequency);
        //}

		// Currently not used.
		//private void DecreaseFrequency()
		//{
		//	timerFrequency = (int)(timerFrequency * 0.75);
		//	cleaningTimer.Change(timerFrequency, timerFrequency);
		//}



        private void CleanCollectedObjects(object state)
        {

            if (isCleaning)
            {
                return;
            }

            try
            {
                isCleaning = true;
                long removedObjects = 0;

                Debug.WriteLine(Utility.GetResourceString("TRACE:START_REF_CLEANING", container.Count, instanceCounter, DateTime.Now),
                               Resources.TRACE_CATEGORY);

                int recordCount = container.Count;
                string[] keys = new string[recordCount];

                int counter = 0;

                ArrayList keyList = new ArrayList(container.Keys);

                for (int i = 0; i < recordCount; i++)
                {
                    // Get Key.
                    string key = (string) keyList[i];

                    ORMReferencesManager refManager = (ORMReferencesManager) container[key];

                    if (refManager == null) continue;

#if DEBUG
                    //Save Number of ref (For Tracing only)
                    int numberOfReferences = refManager.Count;
#endif
              
                    lock (refManager.SyncRoot)
                    {
                       long removedCollectedInstances = refManager.CleanCollectedInstances();
                       removedObjects = removedObjects + removedCollectedInstances;
                       if (refManager.Count == 0) keys[counter] = key;

#if DEBUG
                       Debug.WriteLine(Utility.GetResourceString("TRACE:NUMBER_OF_REF_PER_DATAOBJECT", key, numberOfReferences, removedCollectedInstances,
                                                                 (numberOfReferences - removedCollectedInstances), DateTime.Now), Resources.TRACE_CATEGORY);
#endif  
                    }

                    counter++;
                    

                }


                for (int j = 0; j < keys.Length; j++)
                {
                    if (keys[j] == null) continue;
                    container.Remove(keys[j]);
                    mutexCollection.Remove(keys[j]);
                }

                if (container.Count == 0)
                {
                    StopTimer();
                }


                //Trace.WriteLine(Utility.GetResourceString("TRACE_END_CLEANING_REF", ormContainer.Count, instanceCounter,
                //                                          removedObjects, DateTime.Now), Resources.TRACE_CATEGORY);

#if NET20
                Interlocked.Add(ref instanceCounter, (removedObjects * -1));
                if (instanceCounter < 0) Interlocked.Exchange(ref instanceCounter, 0);
#endif
#if NET11
                //long tmpInstanceCounter = instanceCounter - removedObjects;
                //Interlocked.Exchange(ref instanceCounter, tmpInstanceCounter);
                instanceCounter = instanceCounter - removedObjects;
                if (instanceCounter < 0)instanceCounter = 0;
#endif


                if (ORMConfigurator.UseBalancerShare)
                {
                    TimeSpan diff = DateTime.Now - recordVersionFileCleanedDateTime;
                    if (diff.Hours > 23)
                    {
                        string[] files = Directory.GetFiles(ORMConfigurator.BalancerShare, "*." + GlobalConstants.ORM_VERSION_CONTROL_EXTENSION);
                        foreach (string fileName in files)
                        {
                            FileInfo fileInfo = new FileInfo(fileName);
                            fileInfo.Refresh();
                            if (fileInfo.LastAccessTime < DateTime.Now.AddHours(-23))
                            {
                                fileInfo.Delete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error while cleaning dead instances. " +ex.Message);
            }
            finally
            {
                isCleaning = false;
            }

            


        }

	}



}
