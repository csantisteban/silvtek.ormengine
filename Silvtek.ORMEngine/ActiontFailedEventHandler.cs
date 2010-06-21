#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine
{
   /// <summary>
   /// Event handled for failed actions.
   /// </summary>
   /// <param name="sender">Sender</param>
   /// <param name="e">Event Arguments</param>
   public delegate void ActionFailedEventHandler(object sender, ActionFailedEventArgs e);
}
