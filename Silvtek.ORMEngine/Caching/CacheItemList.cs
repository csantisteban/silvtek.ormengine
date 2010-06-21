#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Silvtek.ORMEngine.Caching
{
	/// <summary>
	/// Contains a collection of cached items.
	/// </summary>
	internal class CacheItemList : NameObjectCollectionBase
	{

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal CacheItemList()
		{
		}

		/// <summary>
		/// Check if the collection contains an specific key.
		/// </summary>
		/// <param name="key">The string to be tested</param>
		/// <returns><see lang="true" /> if it contains; otherwise <see cref=false" /></returns>
		internal bool Contains(string key)
		{
			return (Get(key) != null);
		}

		/// <summary>
		/// Returns an specific item from the cache by its key.
		/// </summary>
		/// <param name="key">The string that represent the key</param>
		/// <returns>A <see cref="CacheItem" /></returns>
		internal CacheItem Get(string key)
		{
			CacheItem item = (CacheItem)base.BaseGet(key);
			if (item == null)return null;
			return ( !item.HasExpired() ) ? item : null;
		}

		/// <summary>
		/// Adds an item to the cache collection.
		/// </summary>
		/// <param name="item">The item to be added</param>
		internal void Add(CacheItem item)
		{
			if (item == null)return;
			CacheItem tempItem = (CacheItem)BaseGet(item.Key);

			if (tempItem != null)
			{
				BaseSet(tempItem.Key, item);
			}

			BaseAdd(item.Key, item);
		}

        internal void CollectItems(Type type)
        {
            int removedObjects = 0;

            string[] keys = base.BaseGetAllKeys();
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                CacheItem item = (CacheItem)base.BaseGet(key);
                if (item == null) continue;
                bool hasExpired = item.HasExpired();

                if (hasExpired || item.Type == type)
                {
                    removedObjects++;
                    base.BaseSet(key, null);
                }
            }

            Trace.WriteLine("Query resultset for type " + type.FullName + " has been removed. Total queries: " + removedObjects);
        }

		/// <summary>
		/// Clean the collection from expired items.
		/// </summary>
		/// <returns>The number of items removed.</returns>
		internal int CollectExpiredItems()
		{
			int removedObjects = 0;

			lock(this)
			{
				object[] values = base.BaseGetAllValues();
                for (int i = (values.Length - 1); i >= 0; i--)
				{
                    if (values[i] == null)
                    {
                        if (i >= base.Count) continue;
                        if (i < 0) break;
                        base.BaseRemoveAt(i);
                        continue;
                    }

					CacheItem item = (CacheItem)values[i];
					bool hasExpired = item.HasExpired();
					//bool isUnderUsed = item.IsUnderUsed();

					//Debug.WriteLineIf(isUnderUsed, "Underused! Ratio: "+item.GetUsageRatio());
					//if (hasExpired || isUnderUsed)
                  
                    if (hasExpired)
					{
						removedObjects++;
						base.BaseRemove(item.Key);
					}
				}
			}

		    return removedObjects;

		}
				
	}
}
