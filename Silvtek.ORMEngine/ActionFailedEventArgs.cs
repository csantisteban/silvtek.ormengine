#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// <see cref="ORMObject" /> operation failure event arguments.
	/// </summary>
	public class ActionFailedEventArgs : EventArgs
	{
       private readonly Action action;
       private readonly string reason;

       /// <summary>
       /// Gets the <see cref="Action" /> that failed.
       /// </summary>
       public Action Action
       {
         get {return action;}
       }

       ///<summary>Gets the failure reason.</summary>
       public string Reason
       {
         get {return reason;}
       }

	   ///<summary>
	   /// Constructor.
	   ///</summary>
	   ///<param name="action">
	   /// The <see cref="Action" /> that failed. 
	   ///</param>
       ///<param name="reason">
       ///The failure reason. 
       ///</param>
	   internal ActionFailedEventArgs (Action action,  string reason)
	   {
          this.action = action;
          this.reason = reason;
	   }

	}
}
