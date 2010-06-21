#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
	/// <summary>
	/// Summary description for Class.
	/// </summary>
	public class RowLoadedEventArgs : EventArgs
	{
       private ResultRow row;
       private int index;


       public int Index
       {
         get {return index;}
       }
       
       public ResultRow Row
       {
         get {return row;}
       }

   	   public RowLoadedEventArgs(int index, ResultRow row)
	   {
          this.index = index;
          this.row = row;
	   }

	}
}
