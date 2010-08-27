#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

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
        private readonly AliasMaps aliasMaps;
        private bool hasUniqueMember;
        private MapMember[] primaryKeysMembers;
        private MapMember[] sequentialMembers;
        private MapMember identityMember;
        private MapMember[] uniqueMembers;


        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>The alias.</value>
        internal AliasMaps Alias
        {
            get { return aliasMaps; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has time stamp.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has time stamp; otherwise, <c>false</c>.
        /// </value>
        internal bool HasTimeStamp
        {
            get { return hasTimestamp; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has sequential member.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has sequential member; otherwise, <c>false</c>.
        /// </value>
        internal bool HasSequentialMember
        {
            get { return hasSequentialMember; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has digest record member.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has digest record member; otherwise, <c>false</c>.
        /// </value>
        internal bool HasDigestRecordMember
        {
            get { return hasDigestRecordMember; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has db identity member.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has db identity member; otherwise, <c>false</c>.
        /// </value>
        internal bool HasDbIdentityMember
        {
            get { return hasDbIdentityMember; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has primary keys.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has primary keys; otherwise, <c>false</c>.
        /// </value>
        internal bool HasPrimaryKeys
        {
            get { return hasPrimaryKeys; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has binary member.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has binary member; otherwise, <c>false</c>.
        /// </value>
        public bool HasBinaryMember
        {
            get { return hasBinaryMember; }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has unique member.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has unique member; otherwise, <c>false</c>.
        /// </value>
        public bool HasUniqueMember
        {
            get { return hasUniqueMember; }
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
            primaryKeysMembers = null;
            sequentialMembers = null;
            identityMember = null;
            uniqueMembers = null;
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
            if (!hasUniqueMember) hasUniqueMember = properties.IsUnique;


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
            if (sequentialMembers == null)
            {
                ArrayList list = new ArrayList();
                int size = Count;
                for (int i = 0; i < size; i++)
                {
                    MapMember tempMember = (MapMember) BaseGet(i);
                    if (tempMember.Properties.HasSequenceProvider)
                    {
                        list.Add(tempMember);
                    }
                }

                sequentialMembers =(MapMember[]) list.ToArray(typeof (MapMember));
            }

            return sequentialMembers;
        }

        internal MapMember[] GetAllPrimaryKeyMembers()
        {
            if (primaryKeysMembers == null)
            {
                ArrayList list = new ArrayList();
                int size = this.Count;
                for (int i = 0; i < size; i++)
                {
                    MapMember tempMember = (MapMember) BaseGet(i);
                    if (tempMember.Properties.IsPrimaryKey)
                    {
                        list.Add(tempMember);
                    }
                }

                primaryKeysMembers = (MapMember[]) list.ToArray(typeof (MapMember));
            }

            return primaryKeysMembers;
        }

        /// <summary>
        /// Gets all unique members.
        /// </summary>
        /// <returns></returns>
        internal MapMember[] GetAllUniqueMembers()
        {
            if (uniqueMembers == null)
            {
                ArrayList list = new ArrayList();
                int size = this.Count;
                for (int i = 0; i < size; i++)
                {
                    MapMember tempMember = (MapMember) BaseGet(i);
                    if (tempMember.Properties.IsUnique)
                    {
                        list.Add(tempMember);
                    }
                }
                uniqueMembers = (MapMember[])list.ToArray(typeof(MapMember));
            }

            return uniqueMembers;
           
        }

        /// <summary>
        /// Gets the db identity member.
        /// </summary>
        /// <returns></returns>
        internal MapMember GetDbIdentityMember()
        {
            if (identityMember == null)
            {
                int size = this.Count;
                for (int i = 0; i < size; i++)
                {
                    MapMember tempMember = (MapMember) BaseGet(i);
                    if (tempMember.Properties.UseDatabaseIdentity)
                    {
                        identityMember = tempMember;
                        break;
                    }
                }

            }

            return identityMember;
        }


    }
}
