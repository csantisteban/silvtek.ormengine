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
	/// Description of QueriedTypes.
	/// </summary>
	internal class QueriedTypes :  CollectionBase, ISqlGenerator
	{
		
		private bool isCaseSensitive;
        private string connectionName;
		
		internal QueriedTypes()
		{
            connectionName = String.Empty;
		}
		
		///<summary>Gets whether the query is case sensitive.</summary>
		public bool IsQueryCaseSensitive
		{
			get {return isCaseSensitive;}
			set {isCaseSensitive = value;}
		}

        internal ObjectSchemaInfo this[int index]
		{
            get { return (ObjectSchemaInfo)InnerList[index]; }
		}

        internal ObjectSchemaInfo MainType
        {
            get { return this[0]; }
        }

		internal int Add(ObjectSchemaInfo info)
		{
			if (InnerList.Count == 0)
			{
                connectionName = info.ConnectionName;
                return InnerList.Add(info);
			}
			else
			{
				// Checks that the type to be added do not already exits.
				// If exists, it will exit the method without inserting again a existing type.
				for(int i = 0; i < InnerList.Count; i++)
				{
                    if (((ObjectSchemaInfo)InnerList[i]).Type == info.Type)
                    {
                        return i;
                    } 

                    if (connectionName != info.ConnectionName)
                    {
                        //TODO: The error description must come from the resource file.
                        string exMsg = Utility.GetResourceString("QueryData::EXMSG:DIFFERENT_CONNECTIONS", MainType.Type, connectionName, info.Type, info.ConnectionName);
                        throw new QueryException(exMsg);
                    }
				}
				
				return InnerList.Add(info);
			}
		}

        internal int Add(Type type)
        {
            ObjectSchemaInfo objectInfo = ORM.GetObjectInfo(type);
            return Add(objectInfo);
        }

		public string GetSql()
		{
			
			StringBuilder builder = new StringBuilder(Count * 100);
					
			bool addComma = false;
			for(int i = 0; i < InnerList.Count; i++)
			{
				ObjectSchemaInfo info = (ObjectSchemaInfo)InnerList[i];
				
				for(int j = 0; j < info.MappedMembers.Count; j++)
				{
					string columnName = String.Format("{0}.{1}", info.TableName, info.MappedMembers[j].ColumnName);
					if (addComma)builder.Append(",");
					builder.Append(columnName);
					addComma = true;		
				}
			}
			
			return builder.ToString();
		}
	}
}
