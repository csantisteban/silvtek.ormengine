using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Specialized;
using Silvtek.ORMEngine.Caching;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Represents the collection of map object members in an single Type.
    /// </summary>
    public class MapMemberList : NameObjectCollectionBase
    {
        private bool hasSequentialMember;
        private bool hasDbIdentityMember;
        private bool hasPrimaryKeys;
        private bool hasDigestRecordMember;
        private bool hasTimestamp;
        private bool hasBinaryMember;
        private readonly Hashtable columnMapping;
        private readonly StringDictionary insensitiveCaseMemberMapping;
        private AliasMaps aliasMaps;


        internal AliasMaps Alias
        {
            get { return aliasMaps; }
        }

        internal bool HasTimeStamp
        {
            get { return hasTimestamp; }
        }

        internal bool HasSequentialMember
        {
            get { return hasSequentialMember; }
        }

        internal bool HasDigestRecordMember
        {
            get { return hasDigestRecordMember; }
        }

        internal bool HasDbIdentityMember
        {
            get { return hasDbIdentityMember; }
        }

        internal bool HasPrimaryKeys
        {
            get { return hasPrimaryKeys; }
        }

        public bool HasBinaryMember
        {
            get { return hasBinaryMember; }
        }

        internal MapMember this[int index]
        {
            get 
            {  
                return (MapMember)BaseGet(index); 
            }
        }

        internal MapMemberList()
        {
            columnMapping = new Hashtable();
            insensitiveCaseMemberMapping = new StringDictionary();
            aliasMaps = new AliasMaps(); 
        }

        public new IEnumerator GetEnumerator()
        {
            MapMember[] members = GetAllMemberInfos();
            return members.GetEnumerator();
        }

        public MapMember[] GetAllMemberInfos()
        {
            return (MapMember[])BaseGetAllValues(typeof(MapMember));
        }

        internal void Add(MemberInfo member, MapMemberProperties properties, DbTypeFormatter formatter)
        {
            MapMember mappedMember = new MapMember(member, properties, formatter);
            Debug.Assert(properties.ColumnName != null && properties.ColumnName.Length > 0, "System is trying to add an no-mapped member");

            if (!hasPrimaryKeys) hasPrimaryKeys = (properties.IsPrimaryKey);
            if (!hasSequentialMember) hasSequentialMember = (properties.HasSequenceProvider);
            if (!hasDbIdentityMember) hasDbIdentityMember = (properties.UseDatabaseIdentity);
            if (!hasDigestRecordMember) hasDigestRecordMember = (properties.HasRecordDigestProvider);
            if (!hasTimestamp) hasTimestamp = (properties.ContainsAttribute(MemberAttributes.Timestamp));
            if (!hasBinaryMember) hasBinaryMember = (properties.DataType == DataType.Binary || properties.DataType == DataType.Object);

            BaseAdd(member.Name, mappedMember);
            insensitiveCaseMemberMapping.Add(member.Name.ToLower(), member.Name);

            if (columnMapping.Contains(mappedMember.ColumnName))
            {
                MapMember tempMappedMember = FindByColumnName(mappedMember.ColumnName);

                string exMsg = Utility.GetResourceString("EXMSG>>ORM_OBJECT_SCHEMA:DUPLICATED_COLUMN", mappedMember.ColumnName, member.Name, tempMappedMember.MemberName);
                throw new ORMException(exMsg);
            }

            columnMapping.Add(mappedMember.ColumnName, mappedMember);
        }

        
       

        /// <summary>
        /// Finds a <see cref="MapMember" /> by the name. This method makes a case sensitive search.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns></returns>
        public MapMember Find(string memberName)
        {
            if (memberName == null) return null;
            MapMember map = (MapMember)BaseGet(memberName);
            if (map == null)
            {
                map = FindCaseInsensitive(memberName);

                if (map == null)
                {
                    string mappedMember = aliasMaps.GetMemberName(memberName);
                    map = (MapMember)BaseGet(mappedMember);
                }
            }


            return map;
        }

        /// <summary>
        /// Finds a <see cref="MapMember" /> by the name. This method makes a case insensitive search.
        /// </summary>
        /// <param name="memberName">Member name.</param>
        /// <returns></returns>
        public MapMember FindCaseInsensitive(string memberName)
        {
            if (memberName == null) return null;
            string tmpMember = insensitiveCaseMemberMapping[memberName.ToLower()];
            if (tmpMember == null || tmpMember.Length == 0) return null;
            return Find(tmpMember);
        }

        internal MapMember FindByColumnName(string columnName)
        {
            return (MapMember)columnMapping[columnName];
        }

        internal MapMember[] GetAllSequentialMembers()
        {
            ArrayList list = new ArrayList();
            int size = Count;
            for (int i = 0; i < size; i++)
            {
                MapMember tempMember = (MapMember)BaseGet(i);
                if (tempMember.Properties.HasSequenceProvider)
                {
                    list.Add(tempMember);
                }
            }

            return (MapMember[])list.ToArray(typeof(MapMember));
        }

        internal MapMember[] GetAllPrimaryKeyMembers()
        {
            ArrayList list = new ArrayList();
            int size = this.Count;
            for (int i = 0; i < size; i++)
            {
                MapMember tempMember = (MapMember)BaseGet(i);
                if (tempMember.Properties.IsPrimaryKey)
                {
                    list.Add(tempMember);
                }
            }

            return (MapMember[])list.ToArray(typeof(MapMember));
        }

        internal MapMember GetDbIdentityMember()
        {
            MapMember retValue = null;
            int size = this.Count;
            for (int i = 0; i < size; i++)
            {
                MapMember tempMember = (MapMember)BaseGet(i);
                if (tempMember.Properties.UseDatabaseIdentity)
                {
                    retValue = tempMember;
                    break;
                }
            }

            return retValue;
        }
    }
}
