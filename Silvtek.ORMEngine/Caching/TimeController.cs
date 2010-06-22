#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.Configs;

namespace Silvtek.ORMEngine.Caching
{
	/// <summary>
	/// REpresents a cache time controller
	/// </summary>
	public struct  TimeController
	{

		private DateTime expirationTime;
		private readonly TimeSpan time;
		private readonly bool isSlideTime;

        /// <summary>
        /// Gets the expiration date and time.
        /// </summary>
        public DateTime ExpirationTime
		{
			get {return expirationTime;}
		}

        /// <summary>
        /// Gets the duration
        /// </summary>
        public TimeSpan Duration
		{
			get {return time;}
		}

        /// <summary>
        /// Gets whether the time is slide time or absolute.
        /// </summary>
        public bool HasSlideTime
		{
			get {return isSlideTime;}
		}

        /// <summary>
        /// Creates a time controller with absolute expiration time.
        /// </summary>
        /// <param name="expirationTime"></param>
        public TimeController(DateTime expirationTime)
		{
			this.expirationTime = expirationTime;
			time = TimeSpan.MinValue;
			isSlideTime = false;
		}

        /// <summary>
        /// Creates a time control with a duration.
        /// </summary>
        /// <param name="ts">Duration time</param>
        /// <param name="slideTime">Whether is slite time or absolute</param>
        public TimeController(TimeSpan ts, bool slideTime)
		{
			this.expirationTime = DateTime.Now.Add(ts);
			this.time = ts;
			this.isSlideTime = slideTime;
		}

        /// <summary>
        /// Return whether the time has expired.
        /// </summary>
        /// <returns></returns>
		internal bool HasExpired()
		{
			return (DateTime.Now > expirationTime);
		}


		internal void SlideTime()
		{
			if (isSlideTime && time > TimeSpan.MinValue)expirationTime = DateTime.Now.Add(time);
		}


		internal static TimeController Default
		{
			get {return GetDefault();}
		}


		private static TimeController GetInactive()
		{
			TimeController inactiveController = new TimeController(DateTime.MinValue);
			return inactiveController;
		}

        public static TimeController GetDefault()
		{
			TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ORMConfigurator.QueryCacheTimeMs);
			TimeController defaultController= new TimeController(timespan, false);
			return defaultController;	   
		}

	}
}
