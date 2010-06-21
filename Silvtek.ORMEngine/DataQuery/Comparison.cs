#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{

   ///<summary>
   /// Defines a SQL comparison.
   ///</summary>
   public enum Comparison : int
   {
      ///<summary>
      /// Defines the SQL keywork <c>like</c> to be used in the criteria.
      ///</summary>
      Like = 101,

	  NotLike = 102,
      ///<summary>
      /// Defines the SQL operator <c>=</c> to be used in the criteria.
      ///</summary>
      Equal = 110,

      ///<summary>
      /// Defines the SQL operator <c> &gt;&lt </c> to be used in the criteria.
      ///</summary>
      NotEqual = 111,

      ///<summary>
      /// Defines the sql operator <c> &gt;= </c> to be used in the criteria.
      ///</summary>
      GreaterEqualThan = 120,

      ///<summary>
      /// Defines the sql operator <c> &lt;= </c> to be used in the criteria.
      ///</summary>
      LessEqualThan = 130,
      
      ///<summary>
      /// Defines the sql operator <c> &gt; </c> to be used in the criteria.
      ///</summary>
      GreaterThan = 140,

      ///<summary>
      /// Defines the sql operator <c> &lt; </c> to be used in the criteria.
      ///</summary>
      LessThan = 150,

      ///<summary>
      /// Defines the SQL keyworkd <c> is null </c> to be used in the criteria.
      ///</summary>
      IsNull = 1001,

      ///<summary>
      ///Defines the SQL keyword <c> is not null </c> to be used in the criteria.
      ///</summary>
      IsNotNull = 1002,

      ///<summary>
      /// Defines the SQL keyworkd  <c> between </c> to be used in the criteria.
      ///</summary>
      Between = 2001,

      ///<summary>
      /// Defines the sql keyword <c> not between </c> to be used in the criteria
      ///</summary>
      NotBetween = 2002,

      ///<summary>
      /// Defines the SQL keyword <c> in </c> to be used in the criteria.
      ///</summary>
      In = 2101,

      ///<summary>
      /// Defines the SQL keyword <c> not in </c> to be used in the criteria.
      ///</summary>
      NotIn = 2102
   }
}
