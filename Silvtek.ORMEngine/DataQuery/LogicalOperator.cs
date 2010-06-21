#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
   ///<summary>
   /// Defines a logical SQL Operator
   ///</summary>
   internal enum LogicalOperator : int
   {
	   /// <summary>
	   /// Defiens the <c>SQL</c> "OR" logical operator.
	   /// </summary>
      Or = 1,

      /// <summary>
      /// Defiens the <c>SQL</c> "AND" logical operator.
      /// </summary>
      And = 2,

      /// <summary>
      /// Defines the <c>SQL</c> "NOT" logical operator.
      /// </summary>
      [Obsolete("DO NOT USE. It is not supported")]
	  Not = 3,

      ///<summary>None.</summary>
      None = 0
   }
}
