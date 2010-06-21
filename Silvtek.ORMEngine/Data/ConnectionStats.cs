#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.Data
{
	/// <summary>
	///  Hold the connections stats.
	/// </summary>
	public class ConnectionStats
	{
		
		#region -- member variables --

		private long openConnections;
        private long total;
        private TimeSpan maxConnectionTime;
        private TimeSpan minConnectionTime; 
        private TimeSpan totalConnectionTime;
        private TimeSpan maxExecutionTime; 
        private TimeSpan minExecutionTime; 
        private TimeSpan totalExecutionTime;
        private int forcedToClose; 
        private DateTime lastConnectionTime;
        private Object mutex;
		
        #endregion -- end member variables --


        internal ConnectionStats()
        {
		   openConnections = 0;
           total = 0;
           maxConnectionTime = TimeSpan.Zero;
           minConnectionTime = TimeSpan.Zero;
           totalConnectionTime = TimeSpan.Zero;
           maxExecutionTime = TimeSpan.Zero;
           minExecutionTime = TimeSpan.Zero;
           totalExecutionTime = TimeSpan.Zero;
           forcedToClose = 0;
           lastConnectionTime = DateTime.MinValue;    
           mutex = new object();
        
        }


        ///<summary>
        /// Returns the number of currently open connections.
        ///</summary>
        public long CountOpenConnections
        {
          get {return openConnections;}
        }

        ///<summary>
        /// Returns the total connections opened.
        ///</summary>
        public long Total
        {
          get {return total;}
        }

        ///<summary>
        /// The maximun connection time.
        ///</summary>
        public TimeSpan MaxConnectionTime
        {
          get {return maxConnectionTime;}
        }

        ///<summary>
        /// The minimun connection time.
        ///</summary>
        public TimeSpan MinConnectionTime
        {
          get {return minConnectionTime;}
        }

        ///<summary>
        /// The maximun execution time.
        ///</summary>
        public TimeSpan MaxExecutionTime
        {
          get {return maxExecutionTime;}
        }

        ///<summary>
        /// The minimun execution time.
        ///</summary>
        public TimeSpan MinExecutionTime
        {
          get {return minExecutionTime;}
        }

        ///<summary>
        /// Gets the <see cref="DateTime" /> of the last connection.
        ///</summary>
        public DateTime LastConnectionTime
        {
          get {return lastConnectionTime;}
        }

        ///<summary>
        /// Gets the Total Execution time, since the application started.
        ///</summary>
        public TimeSpan TotalExecutionTime
        {
          get {return totalExecutionTime;}
        }

        ///<summary>
        /// Gets the total connection time, since the application started.
        ///</summary>
        public TimeSpan TotalConnectionTime
        {
          get {return totalConnectionTime;}
        }

        ///<summary>
        /// Gets the number of connection forced to close.
        ///</summary>
        public int CountForcedToClose
        {
          get {return forcedToClose;}
        }

        ///<summary>
        /// Internal. Invoked when a connection is opened.
        ///</summary>
        internal void Open()
        {
       		openConnections++;
      		total++;
        }

        ///<summary>
        /// Internal. Invoked when the connection is closed.
        ///</summary>
        internal void Close(TimeSpan time)
        {
          if (minConnectionTime == new TimeSpan(0))minConnectionTime = time;
          if (time > maxConnectionTime)maxConnectionTime = time;
          if (time < minConnectionTime)minConnectionTime = time;
          totalConnectionTime = totalConnectionTime.Add(time);
          lastConnectionTime = DateTime.Now;
          openConnections--;
        }

        ///<summary>
        /// Sets the execution time for the last connection.
        ///</summary>
        internal void SetExecutionTime(TimeSpan time)
        {
           if (minExecutionTime == new TimeSpan(0))minExecutionTime = time;
           if (time > maxExecutionTime)maxExecutionTime = time;
           if (time < minExecutionTime)minExecutionTime = time;
           totalExecutionTime = totalExecutionTime.Add(time);
        }

        ///<summary>
        /// Invoked when a connection is close by force.
        ///</summary>
        internal void IncrementClosedByForce()
        {
           forcedToClose++;
        }
	}
}
