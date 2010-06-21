#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine.DataQuery
{
        ///<summary>
        /// Defines the join type.
        ///</summary>
        ///<remarks>
		public enum JoinType
        {
           ///<summary>
           /// Defines query as a inner join.
           ///</summary>
           Inner = 1,

           ///<summary>
           /// Defines query as a outer join.
           ///</summary>
           Outer = 2,

           ///<summary>
           /// Defines query a left join.
           ///</summary>
           Left = 3,

           ///<summary>
           /// Defines query as a right join.
           ///</summary>
           Right = 4
        }

}
