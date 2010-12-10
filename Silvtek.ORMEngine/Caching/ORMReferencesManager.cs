#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Caching
{

	/// <summary>
	///  Maintain all <see cref="WeakReference"/>(s) for a unique
    /// dataobject key.
	/// </summary>
    [Serializable]
	internal class ORMReferencesManager
	{
	
        //private static object _Mutex = new object();
		[NonSerialized]
        private readonly ArrayList instances;
        private readonly string identifier;
		private readonly object syncRoot = new Object();
		private object staticReference;
        private RecordVersion currentVersion;
        private readonly Type type;

       // [NonSerialized]
       // private int seed;

        ///<summary>
        /// Gets the number of instances currently stored.
        ///</summary>
        ///<value>
        /// The number of instances currently stored.
        ///</value>
        internal int Count
        {
          get {return instances.Count;}
        }

        ///<summary>
        /// Gets dataobjects unique key.
        ///</summary>
        ///<value>
        /// The dataobjects unique key.
        ///</value>
        internal string Key
        {
          get {return identifier;}
        }

        /// <summary>
        /// Gets the <see cref="Type"></see> that the Manager holds
        /// </summary>
        /// <value>The type.</value>
	    internal Type Type
	    {
	        get { return type; }
	    }

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
	    internal object SyncRoot
	    {
	        get { return syncRoot; }
	    }

	    /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <returns></returns>
	    internal RecordVersion GetCurrentVersion()
        {
            return currentVersion; 
        }

      
        ///<summary>
        /// Constructor.
        ///</summary>
        internal ORMReferencesManager(string identifier, ORMObject obj)
        {
          
            this.identifier = identifier;
            instances = new ArrayList(255);

            if (obj != null)
            {
                currentVersion = obj.GetRecordVersion();
                SetStaticReference(obj.SourceObject);
                type = obj.GetType();
            }

        }

        private void SetStaticReference(object obj)
        {
            if (obj == null) return;
            Type objectType = obj.GetType();
            object clone = Activator.CreateInstance(objectType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            ORM.Copy(obj, clone);

            staticReference = clone;
        }

	
        ///<summary>
        /// Add a week reference to the collection.
        ///</summary>
        ///<param name="weak">
        /// The reference to add.
        ///</param>
		internal void Add(WeakReference weak)
		{
			//AddStaticReference(weak);
			AddReference(weak);
		}

        private void AddReference(WeakReference weak)
        {
           // Check that the reference is alive.

           if (weak != null && weak.IsAlive)
           {
             object dobj = null;
             
             try
             {
                 dobj = weak.Target;
             }
             catch(InvalidOperationException)
             {
             	 return;
             }
           	
             if (dobj != null)
             {		
				// int newSeed = Interlocked.Increment(ref seed);
				 //dobj.ReferenceID = newSeed;
                 if (dobj is ORMObject)
                 {
                     ((ORMObject)dobj).SetSyncRoot(SyncRoot);
                 }

                 if (staticReference == null)
                 {
                     if (dobj is ORMObject)
                     {
                         SetStaticReference(((ORMObject)dobj).SourceObject);
                     }
                     else
                     {
                         SetStaticReference(dobj);
                     }
                 }

                 instances.Add(weak);
                 //instances.Add(newSeed, weak);           	 
             }
           }
        }

		/// <summary>
		/// Broadcast update to all references.
		/// </summary>
		/// <param name="doSource">The object that triggered the update.</param>
	    internal void BroadcastUpdate(ORMObject doSource)
		{
            if (!ORMConfigurator.UpdateBroadCastingEnabled) return;
            if (staticReference == null) return;

			lock(this)
			{
                currentVersion = doSource.GetRecordVersion();

                try
                {
                    BroadcastUpdateToReferences(doSource.SourceObject);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine("Failed to brocast update to bind object. "+ex.Message);
#if DEBUG
                    throw;
#endif
                }

                try
                {
                    SetStaticReference(doSource.SourceObject);
                }
                catch(Exception ex)
                {
                    staticReference = null;
                    Trace.WriteLine("Failed to broadcast update to static object. " + ex.Message);
#if DEBUG
                    throw;
#endif
                }
			}
			 
		}

		/// <summary>
		/// Broadcast update to all references.
		/// </summary>
		/// <param name="doSource">The object that had trigger the update.</param>
        private void BroadcastUpdateToReferences(object doSource)
		{
		    if (staticReference == null)
		    {
		        Trace.WriteLine("static reference for this record does not exists in the reference manager. Can't broadcast update");
#if DEBUG
		        throw new InvalidOperationException("static reference for this record does not exists in the reference manager. Can't broadcast update");
#endif

		    }

            ArrayList references = new ArrayList(instances);
		    //ArrayList references = new ArrayList(instances.Values);

            for (int i = 0; i < references.Count; i++)
            {
                WeakReference refs = (WeakReference) references[i];

                if (refs == null) continue;
                if (!refs.IsAlive) continue;

                object obj = null;

                // Check that the object in the weak reference is not invalid.
                try
                {
                    obj = refs.Target;
                    if (Object.ReferenceEquals(doSource, obj)) continue;
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine("The weak reference trying to access is invalid", Resources.TRACE_CATEGORY);
                    continue;
                }

                if ((obj != null) && !Object.ReferenceEquals(doSource, obj))
                {

                    // Check if has changes against the previous version;
                    bool isDirty = ORM.AreDifferent(this.staticReference, obj);
                    if (isDirty)
                    {
                        
                        if (obj is ORMObject)
                        {
                            ((ORMObject) obj).ObjectInfo.MarkAsDirty();
                        }

                        // skipt that instance, it is dirty.
                        continue;
                    }

                    // Update the values from the actual object.
                    bool hasDifferentValues = ORM.AreDifferent(doSource, obj);
                    //Debug.WriteLine(" Reference #" + dobj.ReferenceID + " has binding changes = " + dobj.HasBindingChanged);
                    if (hasDifferentValues)
                    {
                        ORM.Copy(doSource, obj);

                        if (obj is ORMObject)
                        {
                            ORMObject tmpOrm = (ORMObject) obj;
                            tmpOrm.FieldList.RefreshObjectSource();
                            tmpOrm.FieldList.CommitChanges();
                        }
                    }

                }

            }

		}

	    /// <summary>
		/// Broadcast that the data has been removed from the database (Deleted)
		/// </summary>
        /// <param name="sourceObject">The object that had removed the data.</param>
        internal void BroadcastRemoval(Object sourceObject)
	    {
            if (!ORMConfigurator.UpdateBroadCastingEnabled) return;
	        staticReference = null;

	        if (ORMConfigurator.UseBalancerShare)
	        {
	            if (currentVersion == null) currentVersion = new RecordVersion(identifier, 0);

                if (sourceObject is ORMObject)
	            {
                    ORMObject tmpORM = (ORMObject)sourceObject;
	                RecordVersion.Delete(tmpORM, currentVersion);
	            }
	            else
	            {
                    ORMObject tmpORM = ORM.Instanciate(sourceObject);
	                RecordVersion.Delete(tmpORM, currentVersion);
	            }

	        }

            //ArrayList references = new ArrayList(instances.Values);
	        ArrayList references = new ArrayList(instances);

	        for (int i = 0; i < references.Count; i++)
	        {
	            WeakReference r = (WeakReference) references[i];
	            if (r == null) continue;
	            if (!r.IsAlive) continue;

	            object storedreference = null;
	            try
	            {
	                storedreference = r.Target;
	            }
	            catch (InvalidOperationException)
	            {
	                continue;
	            }

	            if (storedreference == null) continue;
                if (!(storedreference is ORMObject)) continue;


	            ORMObject tmp = (ORMObject) storedreference;

                if (!object.ReferenceEquals(sourceObject, storedreference))
	            {
	                try
	                {
	                    tmp.MarkAsRemoved();

	                }
	                catch (Exception ex)
	                {
	                    Trace.WriteLine("Failed to broadcast record removal to an ORMObject. "+ex.ToString());

	                }

	            }
	        }

	    }


	    /// <summary>
        /// Gets the orm from the object.
        /// </summary>
        /// <returns></returns>
		internal object Get()
		{
            if (!ORMConfigurator.GlobalCacheEnabled) return null;

            if (ORMConfigurator.UseBalancerShare)
            {
                RecordVersion tempRecordVersion = null;
                if (staticReference != null)
                {
                    tempRecordVersion = RecordVersion.GetCurrentVersionFromSharedCachedFile(staticReference);
                }
                
               bool forceUpdateStaticReference = false;

               if (tempRecordVersion != null)
               {
                   if (currentVersion == null)
                   {
                       currentVersion = tempRecordVersion;
                       forceUpdateStaticReference = true;
                   }
                   else if (tempRecordVersion.VersionNumber > currentVersion.VersionNumber && tempRecordVersion.Type == RecordVersion.ModificationType.Update)
                   {
                       currentVersion = tempRecordVersion;
                       forceUpdateStaticReference = true;

                   }
                   else if (tempRecordVersion.Type == RecordVersion.ModificationType.Delete)
                   {
                       BroadcastRemoval(staticReference);
                       return null;
                   }
               }

               if (forceUpdateStaticReference)
               {
                   if (staticReference is ORMObject)
                   ((ORMObject)staticReference).Refresh();
                   BroadcastUpdateToReferences(staticReference);
               }
            }

            return staticReference;

		}

		internal long CleanCollectedInstances()
		{
			if (instances.Count == 0)return 0;
			int counter = 0;

            int listCount = instances.Count;

			for(int i = 0; i < listCount; i++)
			{
                if (i > instances.Count - 1) break;
                WeakReference reference = (WeakReference)instances[i];

				if ( (reference == null) || (!reference.IsAlive) )
				{
				    instances.RemoveAt(i);
					counter++;
				}
			}

            counter++;
		    return counter;


		}


        ///<summary>
        /// Serves as a hash function for a particular type,
        /// suitable for use in hashing algorithms and data structures
        /// like a hash table
        ///</summary>
        ///<returns>
        /// A hash code for the current <see cref="ORMReferencesManager" />.
        ///</returns>
        public override int GetHashCode()
        {
          return identifier.GetHashCode();
        }


        ///<summary>
        /// Returns a <see cref="String"/> that represents the current
        /// <see cref="ORMReferencesManager"/>.
        ///</summary>
        ///<returns>
        /// A <see cref="String"/> that represents the current
        ///<see cref="ORMReferencesManager"/>.
        ///</returns>
        public override string ToString()
        {
           return identifier;
        }

        ///<summary>
        /// Determines whether two Object instances are equal.
        ///</summary>
        ///<param name="obj">
        /// The Object to compare with the current Object.
        ///</param>
        ///<returns>
        /// true if the specified Object is equal to the current Object;
        /// otherwise, false.
        ///</returns>
        public override bool Equals(object obj)
        {
           if (obj.GetType() != this.GetType())return false;

           return (obj.GetHashCode() == this.GetHashCode());
        }


	}
}

