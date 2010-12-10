using System;
using System.Collections;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Description of QueriedTypes.
    /// </summary>
    internal class CommandTypes : CollectionBase
    {
        private string connectionName;
        private readonly Hashtable typeByFullNameList;
        private readonly Hashtable typeByNameList;

        internal CommandTypes()
        {
            connectionName = String.Empty;
            typeByFullNameList = new Hashtable();
            typeByNameList = new Hashtable();
        }

        internal ObjectSchemaInfo this[int index]
        {
            get { return (ObjectSchemaInfo)InnerList[index]; }
        }

        internal int Add(ObjectSchemaInfo info)
        {
            if (InnerList.Count == 0)
            {
                connectionName = info.ConnectionName;
                return InternalAdd(info);
            }
            else
            {
                // Checks that the type to be added do not already exits.
                // If exists, it will exit the method without inserting again a existing type.
                for (int i = 0; i < InnerList.Count; i++)
                {
                    if (((ObjectSchemaInfo)InnerList[i]).Type == info.Type)
                    {
                        return i;
                    }

                    if (connectionName != info.ConnectionName)
                    {
                        //TODO: The error description must come from the resource file.
                        string exMsg = Utility.GetResourceString("QueryData::EXMSG:DIFFERENT_CONNECTIONS", "" , connectionName, info.Type, info.ConnectionName);
                        throw new QueryException(exMsg);
                    }
                }

                return InternalAdd(info);
            }
        }

        internal ObjectSchemaInfo FindSchemaInfo(string name)
        {
            ObjectSchemaInfo info = null;
            if (typeByFullNameList.Contains(name))
            {
                info = (ObjectSchemaInfo)typeByFullNameList[name];
            }
            else if (typeByNameList.Contains(name))
            {
                info = (ObjectSchemaInfo)typeByNameList[name];
            }

            return info;
        }


        private int InternalAdd(ObjectSchemaInfo info)
        {

            if (info.Type.FullName != null)
            {
                typeByFullNameList[info.Type.FullName] = info;
            }

            typeByNameList[info.Type.Name] = info;

            return InnerList.Add(info);
        }

        internal int Add(Type type)
        {
            ObjectSchemaInfo objectInfo = ORM.GetObjectInfo(type);
            return Add(objectInfo);
        }

    }
}
