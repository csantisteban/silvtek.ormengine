#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.DataQuery;
//using OLDResultSet = Silvtek.ORMEngine.Querying.ResultSet;


namespace Silvtek.ORMEngine.Caching
{
	/// <summary>
	/// Represents a query cache.
	/// </summary>
	internal sealed class QueryCache
	{
		
        // Holds the cache singleton.
		private static QueryCache   cache = null;
        private const int period = 21000; 
		private readonly Timer timer;
		private readonly CacheItemList items;
		private bool isTimerStarted;
		private int lastTotalItemsRemoved;
		
        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
		public int Count
		{
			get {return items.Count;}
		}

        // Instaciate the cache.
		private QueryCache()
		{
			items = new CacheItemList();
			timer = new Timer(new TimerCallback(OnTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
		}

		internal ResultSet GetResultSet(string key)
		{
			 CacheItem item = items.Get(key);
			 if (item == null)return null;
             if (item.HasExpired()) return null;
			 return (ResultSet)item.GetValue();  
		}


        /// <summary>
        /// Returns an object from the cache.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns></returns>
		internal object GetObject(string key)
		{
			CacheItem item = items.Get(key);
			if (item == null)return null;
            if (item.HasExpired()) return null;
			return item.GetValue();  
		}

	

		internal void AddItem(ResultSet resultSet, Type mainType, TimeController timeController)
		{
            if (!ORMConfigurator.GlobalCacheEnabled)
            {
                return;
            }

            // Do not add to the cache if the item has expired already
			if ( timeController.HasExpired() )return;
			StartTimer();
		    CacheItem item = new CacheItem(resultSet.Identifier, resultSet, mainType, timeController);
			items.Add(item);
		}

        //internal void AddItem(OLDResultSet resultSet, Type mainType, TimeController timeController)
        //{
        //    // Do not add to the cache if the item has expired already
        //    if ( timeController.HasExpired() ) return;

        //    StartTimer();
        //    CacheItem item = new CacheItem(resultSet.Identifier, resultSet, mainType, timeController);
        //    items.Add(item);
        //}


		internal void AddItem(string key, object obj, Type mainType, TimeController timeController)
		{
            if (!ORMConfigurator.GlobalCacheEnabled)
            {
                return;
            }

			// Do not add to the cache if the item has expired already
            if ( timeController.HasExpired() )return;
			
            StartTimer();
			CacheItem item = new CacheItem(key, obj, mainType,  timeController);
			items.Add(item);
		}
	
		internal bool Exists(string key)
		{
			return items.Contains(key);
		}

        internal void RemoveCacheForType(Type type)
        {
            items.CollectItems(type);
        }


        #region -- Private Methods --
        private void StartTimer()
		{
			if (!isTimerStarted)
			{
				Debug.WriteLine("Cache Timer Started", Resources.TRACE_CATEGORY);
				isTimerStarted = true;
				timer.Change(0, period);
			}
		}

		private void StopTimer()
		{
			Debug.WriteLine("Cache Timer Stopped", Resources.TRACE_CATEGORY);
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}


		private void OnTimerCallback(object state)
		{
			try
			{
				lastTotalItemsRemoved = items.CollectExpiredItems();

				if (items.Count == 0)
				{
					isTimerStarted = false;
					StopTimer();
				}
			}
			catch(Exception e)
			{
				string traceMsg = String.Format("{0}\n{1}\n{2}\n{3}", e.GetType().FullName, e.Message, e.Source, e.StackTrace);
				Trace.WriteLine(traceMsg, Resources.TRACE_CATEGORY);
			}
		}

		#endregion

        #region -- Static Members --


		internal static QueryCache Current
		{
			get {return GetSingleton();}
		}

	    internal int LastTotalItemsRemoved
	    {
	        get { return lastTotalItemsRemoved; }
	    }


	    private static QueryCache GetSingleton()
		{
			if (cache != null)return cache;
			
			lock(typeof(QueryCache))
			{
				if (cache == null)
				{
					cache = new QueryCache();	
				}
			}
			
			return cache;
		}

        internal static void EmptyCache()
        {
            if (cache != null) cache = null;
        }

        internal static void EmptyCache(Type type)
        {
            Current.RemoveCacheForType(type);
        }

#endregion
		
	}
}
