#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
	/// <summary>
	/// Collection of utility methods for using the ORM engine.
	/// </summary>
	public sealed class ORM
	{

		private ORM()
		{
		}

        /// <summary>
        /// Defines the type of behavior for a single Field.
        /// </summary>
        //public enum FieldBehavior : byte
        //{
        //     <summary>
        //     Defines that when the field is empty, use the default value specified in the ORM mapping.
        //     </summary>
        //    WhenEmptyUseDefault,

        //     <summary>
        //     Defines that when the field is empty, skip.
        //     </summary>
        //    WhenEmptySkip,

        //     <summary>
        //     Ensure to use all fields.
        //     </summary>
        //    UseAll
        //}

		/// <summary>
		/// Gets the number of types loaded into the ORM Cache.
		/// </summary>
		public static int NumberTypes
		{
			get {return ObjectSchemaContainer.Current.Count;}
		}

		/// <summary>
		///	 Gets the number of instances loaded into memory.
		/// </summary>
		public static long NumberOfInstances
		{
			get { return GlobalReferencesManager.Current.InstanceCounter; }
		}

		/// <summary>
		/// Locks an <see cref="ORMObject" />.
		/// </summary>
		/// <param name="objInstance">The instance to be locked.</param>
		public static void Lock(ORMObject objInstance)
		{
            objInstance.ObjectInfo.Locked();
		}

		/// <summary>
		/// Sets the connection name for a specific type
		/// </summary>
		/// <param name="type">The type to which the connection name will be linked. The type used must be mapped.</param>
		/// <param name="connectionName">The connection name.</param>
		public static void SetConnectionName(Type type, string connectionName)
		{
			ObjectSchemaInfo info = ObjectSchemaContainer.Current.Retrieve(type);
			if (info == null)
			{
				string msg = Utility.GetResourceString("EXMSG:ORM:TYPE_NOT_FOUND", type);
			    throw new InvalidOperationException(msg);
			}

			info.ConnectionName = connectionName;
		}

        /// <summary>
        /// Instanciates an <see cref="ORMObject"/>. This method binds the <paramref name="source">source</paramref> instance
        /// to the database, and returns the bound object, which is an instance of <see cref="ORMObject"/>
        /// </summary>
        /// <param name="source">The object to be bound to the database</param>
        public static ORMObject Instanciate(object source)
        {
            if (source == null) return null;
            if (source is ORMObject)
            {
                return (ORMObject)source;
            }


            return new ORMObject(source);

        }

        public static ORMObject Create(object source)
        {
            return Create(source, false);
        }

        /// <summary>
        /// Creates a record in the database for the mapped instance passed in the argument.
        /// </summary>
        /// <param name="source">The object to be created in the database</param>
        /// <param name="synchronized"></param>
        public static ORMObject Create(object source, bool synchronized)
        {
            ORMObject orm = Instanciate(source);

            if (synchronized)
            {
                lock (orm.SyncRoot)
                {
                    orm.Create();
                }
            }
            else
            {
                orm.Create();
            }


            return orm;
        }

        public static ORMObject Save(object source)
        {
            return Save(source, false);
        }


        public static DateTime GetServerDateTime(string connectionName)
        {
            Connection cnn = ConnectionManager.Current.GetConnection(connectionName);

            Command cmd = new Command(cnn);
            cmd.CommandText = cnn.DbTypeFormatter.GetServerDateTimeCommand();
            cmd.CommandType = CommandType.Text;
            DateTime result = DateTime.MinValue;
            try
            {
                cnn.Open();
                object tmp = cmd.ExecuteScalar();
                if (tmp != null)
                {
                    result = Convert.ToDateTime(tmp);
                }

            }
            finally
            {
                if (cnn != null)cnn.Close();
            }

            return result;
        }

        public static DateTime GetServerDateTime(Type type)
        {
            ObjectSchemaInfo objectInfo = ORM.GetObjectInfo(type);
            Connection cnn = objectInfo.GetConnection();

            Command cmd = new Command(cnn);
            cmd.CommandText = cnn.DbTypeFormatter.GetServerDateTimeCommand();
            cmd.CommandType = CommandType.Text;
            DateTime result = DateTime.MinValue;
            try
            {
                cnn.Open();
                object tmp = cmd.ExecuteScalar();
                if (tmp != null)
                {
                    result = Convert.ToDateTime(tmp);
                }

            }
            finally
            {
                if (cnn != null) cnn.Close();
            }

            return result;  
        }

        public static DigestComparisonResult CompareRecordDigest(object obj)
        {
            ORMObject orm = Instanciate(obj);
            bool load = (orm.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (!load)
            {
                orm.Load();
            }

            return orm.CompareRecordDigest();
        }


        public static ORMObject Save(object source, bool synchronized)
        {

            ORMObject orm = Instanciate(source);

            bool load = (orm.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (synchronized)
            {

                lock (orm.SyncRoot)
                {
                    orm.Save(load);
                    Trace.WriteLineIf(orm.Result != ActionResult.Succeed, "record not updated " + source.GetType().FullName + ". result: " + orm.Result);
                }
            }
            else
            {
                orm.Save(load);
                Trace.WriteLineIf(orm.Result != ActionResult.Succeed, "record not updated " + source.GetType().FullName + ". result: " + orm.Result);

            }

            return orm;
      
        }

        public object[] Instanciate(Type type, DataTable dt)
        {
            ArrayList objects = new ArrayList();

            // Start reading database rows
            foreach (DataRow row in dt.Rows)
            {
             
                ObjectSchemaInfo info = ORM.GetObjectInfo(type);
                MapMemberList columns = info.MappedMembers;

                // Holds the actual object to be created.
                object currentObj = null;
                ORMObject ormObj = null;
                for (int columnCounter = 0; columnCounter < columns.Count; columnCounter++)
                {

                    if (columns[columnCounter].Properties.IsPrimaryKey && Convert.IsDBNull(row[columns[0].ColumnName]))
                    {
                        currentObj = null;
                        break;
                    }

                    if (currentObj == null)
                    {

                        currentObj = Activator.CreateInstance(info.Type, BindingFlags.NonPublic |
                                                                         BindingFlags.Public | BindingFlags.Instance,
                                                              null, null, null);

                        if (currentObj is ORMObject)
                        {
                            ormObj = ORM.Instanciate(currentObj);
                        }


                        if (ormObj != null) ormObj.InternalFireBeforeLoaded();
                    }

                    if (columns[columnCounter].Properties.AcceptNulls && Convert.IsDBNull(row[columns[0].ColumnName]))
                        continue;

                    if (columns[columnCounter].Properties.DataType != DataType.Binary)
                    {
                        columns[columnCounter].SetDataSourceValue(currentObj, row[columns[0].ColumnName]);
                    }
                    else
                    {
                        if (!Convert.IsDBNull(row[columns[0].ColumnName]))
                        {
                            DataStream dataStream = null;
                            if (row[columns[0].ColumnName] is byte[])
                            {
                                dataStream = new DataStream((byte[]) row[columns[0].ColumnName]);
                                dataStream.Position = 0;
                                columns[columnCounter].SetDataSourceValue(currentObj, dataStream);

                            }
                        }
                        else
                        {
                            columns[columnCounter].SetDataSourceValue(currentObj, null);
                        }
                    }
                }

                if (ormObj != null)
                {
                    ormObj.BindByForce();
                    ormObj.InternalFireAfterLoaded();
                }

                objects.Add(currentObj);

            }

            return (object[]) objects.ToArray(type);
        }


	    ///// <summary>
        ///// Express Update
        ///// </summary>
        ///// <param name="source">The object to be updated</param>
        ///// <remarks>Use ORMObject.Store for better manipulation of data.</remarks>
        ///// <returns></returns>
        //public static ActionResult Store(object source)
        //{
        //    return Store(source, FieldBehavior.UseAll);
        //}

        ///// <summary>
        ///// Express Update
        ///// </summary>
        ///// <param name="source">The object to be updated</param>
        ///// <param name="allowNullFields">Whether to allow to update mpty or null fields contained in
        /////  the source object</param>
        ///// <remarks>Use ORMObject.Store for better manipulation of data.</remarks>
        ///// <returns></returns>
        //public static ActionResult Store(object source, FieldBehavior  behaviour)
        //{
            
        //    object copy = Activator.CreateInstance(source.GetType(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            
        //    // Instanciate ORM Object for source and copy.
        //    ORMObject ormObjectCopy = Instanciate(copy);
        //    ORMObject ormObject = Instanciate(source);
            
        //    ObjectSchemaInfo objectInfo = GetObjectInfo(source.GetType());
        //    MapMember[] pks = objectInfo.MappedMembers.GetAllPrimaryKeyMembers();

        //    foreach (MapMember pk in pks)
        //    {
        //        object originalValue = pk.GetValue(source);
        //        pk.SetValue(copy, originalValue);
        //    }

        //    // now load the copy.
        //    ormObjectCopy.Load();

        //    if (ormObjectCopy.Result != ActionResult.Succeed) return ormObjectCopy.Result;
          
        //    // now copy the rest of the members to make an update.

        //    foreach(MapMember mappedMember in objectInfo.MappedMembers)
        //    {
        //        if (mappedMember.Properties.IsPrimaryKey)continue;

        //        object originalValue = mappedMember.GetValue(source);
                
        //        // check if the value is empty.
        //        bool isEmpty = (originalValue == null || Convert.ToString(originalValue).Length == 0);
        //        if (isEmpty && behaviour == FieldBehavior.WhenEmptySkip) continue;
        //        if (isEmpty && behaviour == FieldBehavior.WhenEmptyUseDefault)
        //        {
        //            originalValue = mappedMember.Properties.DefaultValue;
        //        }

        //        object copyValue = mappedMember.GetValue(copy);
        //        if (originalValue != copyValue)
        //        {
        //            if ((!mappedMember.Properties.AcceptNulls) && originalValue == null)
        //            {
        //                string exMsg = Utility.GetResourceString("ORM_STORE_PARTIAL_INITIALIZED_OBJ",source.GetType().FullName, mappedMember.MemberName);
        //                throw new ORMException(exMsg);
        //            }

        //            mappedMember.SetValue(copy, originalValue);
        //        }
        //    }

        //    ormObjectCopy.Save();

        //    return ormObjectCopy.Result;
            

        //}

        public static ORMObject Load(object source)
        {
            ORMObject ormObject = Instanciate(source);
            ormObject.Load();
            return ormObject;
        }

        public static ORMObject Refresh(object source)
        {
            ORMObject ormObject = Instanciate(source);
            ormObject.Refresh();
            return ormObject;
        }

        /// <summary>
        /// Removes a record from the database for the object passed. The object must have their primary key fields populated.
        /// </summary>
        /// <param name="source">The mapped object representing a record in the database to be removed.</param>
        /// <returns></returns>
        public static ActionResult Remove(object source)
        {
            return Remove(source, false);
        }

        /// <summary>
        /// Removes a record from the database for the object passed. The object must have their primary key fields populated.
        /// </summary>
        /// <param name="source">The mapped object representing a record in the database to be removed.</param>
        /// <param name="synchronized">True, for thread safety.</param>
        /// <returns></returns>
        public static ActionResult Remove(object source, bool synchronized)
        {
            ORMObject orm = Instanciate(source);

            if (synchronized)
            {
                lock (orm.SyncRoot)
                {
                    orm.Remove();
                }
            }
            else
            {
                orm.Remove();
            }

            return orm.Result; 
        }

        /// <summary>
        /// Force the ORM engine to release all cache from memory. 
        /// For extra clean up, invoke GC.Collect after this method.
        /// </summary>
        public static void ClearCaches()
        {
            QueryCache.EmptyCache();
            GlobalReferencesManager.ResetCache();
        }

        /// <summary>
        /// Clears all items from the cache.
        /// </summary>
        public static void ClearQueryCache()
        {
            QueryCache.EmptyCache();
        }

        public static void ClearCacheForType(Type type)
        {
            QueryCache.EmptyCache(type);
        }

		/// <summary>
		/// Returns the object schema.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ObjectSchemaInfo GetObjectInfo(Type type)
		{
			ObjectSchemaInfo info = ObjectSchemaContainer.Current.Retrieve(type);
			if (info == null)
			{
				string msg = Utility.GetResourceString("EXMSG:ORM:TYPE_NOT_FOUND", type);
				throw new InvalidOperationException(msg);
			}

			return info;
		}

        
        internal static void Copy(object source, object dest)
        {
            if (source.GetType() != dest.GetType())
            {
                throw new InvalidOperationException("Can't copy from type "+source.GetType().FullName+" to type "+dest.GetType().FullName);
            }

            ObjectSchemaInfo info = ORM.GetObjectInfo(source.GetType());
            foreach(MapMember member in info.MappedMembers)
            {
                object value = member.GetDataSourceValue(source);
                member.SetDataSourceValue(dest, value);
            }         
        }

        internal static bool HasDifferentValues(object source, object dest)
        {
            if (source == null || dest == null) return true;

            if (source.GetType() != dest.GetType())
            {
                throw new InvalidOperationException("Can't copy from type " + source.GetType().FullName + " to type " + dest.GetType().FullName);
            }

            ObjectSchemaInfo info = ORM.GetObjectInfo(source.GetType());
            foreach (MapMember member in info.MappedMembers)
            {
                if (member.Properties.IsRecordDigestEnabled) continue;
                if (member.Properties.ContainsAttribute(MemberAttributes.Timestamp)) continue;

                if  ((  member.Properties.ModeType == ColumnModeType.ExcludeFromDigest 
                     || member.Properties.ModeType == (ColumnModeType.ExcludeFromDigest | ColumnModeType.IgnoreChanges)
                     || member.Properties.ModeType == ColumnModeType.IgnoreChanges)
                    )
                    
                {
                    continue;
                }


                object value = member.GetMemberValue(source);
                object destValue = member.GetMemberValue(dest);

                if (!Object.Equals(value, destValue))
                {
                    return true;
                }

            }

            return false;
        }


	}
}
