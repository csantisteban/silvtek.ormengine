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
		private readonly bool isTimerStarted;
        private readonly Hashtable ormContainer;
        private readonly Object mutex;
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


			Debug.Assert(key != null && key.Length > 0, "Key passed to the global reference manager is invalid: ["+key+"]");
			
			ORMReferencesManager recordManager = (ORMReferencesManager)ormContainer[key];
			if (recordManager == null)
			{
				lock(mutex)
				{
					recordManager =	new ORMReferencesManager(key, orm);
					ormContainer.Add(key, recordManager);
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
			if (ormContainer.Contains(key))
			{
				ORMReferencesManager doController = (ORMReferencesManager)ormContainer[key];
				if (doController != null)
				{
					Debug.WriteLine("About to broadcast update for: "+key);
					doController.BroadcastUpdate(obj);
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
			if (ormContainer.Contains(key))
			{
				ORMReferencesManager doController = (ORMReferencesManager)ormContainer[key];
				if (doController != null)
				{
					doController.BroadcastRemoval(obj);
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
          
			object dobj = null;
          
			if (ormContainer.Contains(ormKey))
			{
				ORMReferencesManager doRM = (ORMReferencesManager)ormContainer[ormKey];
				dobj = doRM.Get();
			}
           
			return dobj;
		}

		/// <summary>
		/// Returns the manager for a specific record.
		/// </summary>
        /// <param name="ormKey">The record key.</param>
		/// <returns></returns>
		internal ORMReferencesManager GetManagerInstance(string ormKey)
		{
			ORMReferencesManager dom = (ORMReferencesManager)ormContainer[ormKey];
			if (dom == null)
			{
                lock (ormContainer.SyncRoot)
                {
                    dom = new ORMReferencesManager(ormKey, null);
                    ormContainer[ormKey] = dom;
                }
            }

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

			ormContainer = new Hashtable(256);
			mutex = new object();

			TimerCallback callback = new TimerCallback(CleanCollectedObjects);
			cleaningTimer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);
		}

		private void StartTimer()
		{
			if(!isTimerStarted)
			{
				cleaningTimer.Change(timerFrequency, timerFrequency);
			}
		}

		private void StopTimer()
		{
		    cleaningTimer.Change(Timeout.Infinite, Timeout.Infinite);
			timerFrequency = DEFAULT_TIMER_FREQUENCY;
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
				//IncreaseFrequency();
				return;
			}

			isCleaning = true;
			long removedObjects = 0;
             
			Trace.WriteLine(Utility.GetResourceString("TRACE:START_REF_CLEANING", ormContainer.Count, instanceCounter, DateTime.Now),
				            Resources.TRACE_CATEGORY);

			int recordCount = ormContainer.Count;
			string[] keys = new string[recordCount];
			
			int counter = 0;
           
			ArrayList keyList = new ArrayList(ormContainer.Keys);
			
			for (int i = 0; i < recordCount; i++)
			{
				// Get Key.
				string key = (string)keyList[i];

				ORMReferencesManager refManager = (ORMReferencesManager)ormContainer[key];

#if DEBUG
				//Save Number of ref (For Tracing only)
				int numberOfReferences = refManager.Count;
#endif
				long removedCollectedInstances = refManager.CleanCollectedInstances();

				removedObjects = removedObjects + removedCollectedInstances;
#if DEBUG
				Debug.WriteLine(Utility.GetResourceString("TRACE:NUMBER_OF_REF_PER_DATAOBJECT", key, numberOfReferences, removedCollectedInstances,
					           (numberOfReferences - removedCollectedInstances), DateTime.Now), Resources.TRACE_CATEGORY);
#endif

				if (refManager.Count == 0)keys[counter] = key;
				counter++;
			}

			lock(mutex)
			{
				for(int j = 0; j < keys.Length; j++)
				{
					if (keys[j] == null)continue;
					ormContainer.Remove(keys[j]);
				}

				if (ormContainer.Count == 0)
				{
					StopTimer();
				}
			}

			Trace.WriteLine(Utility.GetResourceString("TRACE_END_CLEANING_REF", ormContainer.Count, instanceCounter,
				removedObjects, DateTime.Now), Resources.TRACE_CATEGORY);

			//TODO: Use some kind of synchronization mechanims for the "instanceCounter Variable"
			instanceCounter = instanceCounter - removedObjects;

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

			isCleaning = false;
			
			 
		}

     }



}
