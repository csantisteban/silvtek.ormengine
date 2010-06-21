#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
//using Silvtek.ORMEngine.Querying;

namespace Silvtek.ORMEngine.Caching
{
	/// <summary>
	/// Represents an cached item.
	/// </summary>
    internal sealed class CacheItem
    {

        private readonly object objvalue;
        private readonly string key;
        private readonly DateTime beginTime;
        private TimeController timeController;
        private int hits;
        private readonly Type mainType;

        /// <summary>
        /// Gets the key for this item.
        /// </summary>
        internal string Key
        {
            get { return key; }
        }
        

        /// <summary>
        /// Gets the type of the item cached.
        /// </summary>
        internal Type Type
        {
            get { return mainType; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The string that represents the key for this item</param>
        /// <param name="value">The object to be cached.</param>
        /// <param name="timeController">The object that control its live in the cache.</param>
        /// <param name="mainType">The type to add to the cache.</param>
        internal CacheItem(string key, object value, Type mainType, TimeController timeController)
        {
            this.timeController = timeController;
            hits = 1;
            beginTime = DateTime.Now;
            this.key = key;
            objvalue = value;
            this.mainType = mainType;
        }

        /// <summary>
        /// Returns the item age.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetAge()
        {
            return (DateTime.Now - beginTime);
        }

        /// <summary>
        /// Returns the usage ration
        /// </summary>
        /// <returns></returns>
        public float GetUsageRatio()
        {
            TimeSpan age = GetAge();
            float fHits = Convert.ToSingle(hits);
            float ftime = (float)age.TotalMilliseconds;
            return (hits / ftime) * 1000f;
        }


        /// <summary>
        /// Returns whether is an underused item or not.
        /// </summary>
        /// <returns><see lang="true" /> if is underused; otherwise <see lang="false" /></returns>
        public bool IsUnderUsed()
        {
            float ratio = GetUsageRatio();
            return (ratio < 0.0082f);
        }


        /// <summary>
        /// Returns whether the item has expired or not.
        /// </summary>
        /// <returns><see lang="true" /> if this item has expired; otherwise <see lang="false" /></returns>
        public bool HasExpired()
        {
            return timeController.HasExpired();
        }


        /// <summary>
        /// Returns the object that this instance represents.
        /// </summary>
        public object GetValue()
        {
            hits++;	 // Increment hits
            timeController.SlideTime();
            return objvalue;
        }

    }
}
