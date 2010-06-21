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
	internal struct  TimeController
	{

		private DateTime expirationTime;
		private TimeSpan timeSpan;
		private bool slideTime;

        /// <summary>
        /// Gets the expiration date and time.
        /// </summary>
		internal DateTime ExpirationTime
		{
			get {return expirationTime;}
		}

        /// <summary>
        /// Gets the duration
        /// </summary>
		internal TimeSpan Duration
		{
			get {return timeSpan;}
		}

        /// <summary>
        /// Gets whether the time is slide time or absolute.
        /// </summary>
		internal bool HasSlideTime
		{
			get {return slideTime;}
		}

        /// <summary>
        /// Creates a time controller with absolute expiration time.
        /// </summary>
        /// <param name="expirationTime"></param>
		internal TimeController(DateTime expirationTime)
		{
			this.expirationTime = expirationTime;
			timeSpan = TimeSpan.MinValue;
			slideTime = false;
		}

        /// <summary>
        /// Creates a time control with a duration.
        /// </summary>
        /// <param name="ts">Duration time</param>
        /// <param name="slideTime">Whether is slite time or absolute</param>
		internal TimeController(TimeSpan ts, bool slideTime)
		{
			this.expirationTime = DateTime.Now.Add(ts);
			this.timeSpan = ts;
			this.slideTime = slideTime;
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
			if (slideTime && timeSpan > TimeSpan.MinValue)expirationTime = DateTime.Now.Add(timeSpan);
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

		private static TimeController GetDefault()
		{
			TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ORMConfigurator.QueryCacheTimeMs);
			TimeController defaultController= new TimeController(timespan, false);
			return defaultController;	   
		}

	}
}
