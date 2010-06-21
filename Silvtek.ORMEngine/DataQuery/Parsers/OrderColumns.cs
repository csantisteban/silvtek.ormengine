#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Text;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery.Parsers
{
	/// <summary>
	/// Collection of columns from which the query is ordered.
	/// </summary>
	internal class OrderColumns  : CollectionBase, ISqlGenerator	
	{
		private bool isQueryCaseSensitive;
	
        /// <summary>
        /// Gets or sets whether the query is case sensitive.
        /// </summary>
		public bool IsQueryCaseSensitive
		{
			get {return isQueryCaseSensitive;}
			set {isQueryCaseSensitive = value;}
		}
		
        /// <summary>
        /// Default constructor.
        /// </summary>
		internal OrderColumns()
		{
		}
		
        /// <summary>
        /// Adds a column to which the query is ordered
        /// </summary>
        /// <param name="objectInfo">The object info.</param>
        /// <param name="member">The member name.</param>
        /// <param name="direction">The direction.</param>
		internal void Add(ObjectSchemaInfo objectInfo, string member, OrderDirection direction)
		{
			MapMember column = objectInfo.MappedMembers.Find(member);
			
			if (column == null)
			{
				throw new MemberNotFoundException(objectInfo.Type, member);
			}

            OrderItem item = new OrderItem(column, direction);

            InnerList.Add(item);
		}

        /// <summary>
        /// Adds a column to which the query is ordered
        /// </summary>
        /// <param name="member">The member name.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="type">The object type.</param>
        internal void Add(Type type, string member, OrderDirection direction)
        {
            ObjectSchemaInfo info = ORM.GetObjectInfo(type);
            Add(info, member, direction);
        }
		
		/// <summary>
		/// Returns the sql order command.
		/// </summary>
		/// <returns></returns>
		public string GetSql()
		{
            if (this.Count == 0) return String.Empty;

            StringBuilder builder = new StringBuilder(this.Count * 14);
            builder.Append("order by ");
			bool addComma = false;
			for(int j = 0; j < this.Count; j++)
			{
                OrderItem item = (OrderItem)InnerList[j];
                if (addComma)builder.Append(",");
                builder.AppendFormat("{0}", item.GetOrderColumn());
				addComma = true;		
			}
					
			return builder.ToString();
		}
		

        /// <summary>
        /// Represents a single order item.
        /// </summary>
        /// <remarks>Private class</remarks>
        private sealed class OrderItem 
        {
            private const string SQLTEMPLATE = "{column}{direction}";
            private MapMember MappedMember;
            private OrderDirection direction;

            internal OrderItem(MapMember MappedMember, OrderDirection direction)
            {
                this.MappedMember = MappedMember;
                this.direction = direction;
            }

            private string GetOrderCommand()
            {
                string orderValue = String.Empty;

                switch (this.direction)
                {
                    case OrderDirection.Ascending:
                        orderValue = " asc";
                        break;
                    case OrderDirection.Descending:
                        orderValue = " desc";
                        break;
                    case OrderDirection.Default:
                        orderValue = String.Empty;
                        break;
                    default:
                        throw new NotSupportedException("Not supported order type");
                }

                return orderValue;
            }

            public string GetOrderColumn()
            {
                if (MappedMember == null) return String.Empty;
                StringBuffer buffer = new StringBuffer(SQLTEMPLATE);
                string directionCommand = GetOrderCommand();
                buffer.SetTokenValue("column", MappedMember.ColumnFullName);
                buffer.SetTokenValue("direction", directionCommand);
                return buffer.ToString();
            }
        }
	}
}
