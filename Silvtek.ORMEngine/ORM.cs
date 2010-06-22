#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
		/// <param name="ormObject">The instance to be locked.</param>
		public static void Lock(ORMObject ormObject)
		{
            ormObject.ObjectInfo.Locked();
		}

        /// <summary>
        /// Gets the bound object.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static object GetBoundObject(ORMObject obj)
        {
            return obj.SourceObject;
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

   

        #region -- CreateAsync Methods --


        /// <summary>
        /// Creates a new record in the database from the object provided. The object provided 
        /// in the parameter must be a mapped to a table.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public static IAsyncResult BeginCreate(object source, AsyncCallback callback, object state)
        {
            ORMObject orm = Instanciate(source);
            return orm.BeginCreate(callback, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static ORMObject EndCreate(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMCreateCaller ormActionDelegate = (ORMCreateCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        #endregion 


        /// <summary>
        /// Creates the specified orm object in the database. This method
        /// will create a insert statement.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Saves object to the database asynchrously.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public static IAsyncResult BeginSave(object source, AsyncCallback callback, object state)
        {
            return BeginSave(source, false, callback, state);
        }

        /// <summary>
        /// Saves object to the database asynchrously.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="synchronized">Synch this object with other instances representing same record.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public static IAsyncResult BeginSave(object source, bool synchronized, AsyncCallback callback, object state)
        {
            ORMObject orm = Instanciate(source);

            bool load = (orm.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (synchronized)
            {
                lock (orm.SyncRoot)
                {
                    return orm.BeginSave(load, callback, state);
                }
            }
            else
            {
                return orm.BeginSave(load, callback, state);
            }
        }

        public static ORMObject EndSave(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMSaveCaller ormActionDelegate = (ORMSaveCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        /// <summary>
        /// Saves object to the database
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ORMObject Save(object source)
        {
            return Save(source, false);
        }

        /// <summary>
        /// Saves the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="synchronized">Synch this object with other instances representing same record.</param>
        /// <returns></returns>
        public static ORMObject Save(object source, bool synchronized)
        {
            ORMObject orm = Instanciate(source);

            bool load = (orm.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (synchronized)
            {

                lock (orm.SyncRoot)
                {
                    orm.Save(load);
                }
            }
            else
            {
                orm.Save(load);
            }

            return orm;
      
        }

        /// <summary>
        /// Instanciates the specified type from a datatable. 
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static object[] Instanciate(Type type, DataTable dt)
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
                                                                         BindingFlags.Public    | 
                                                                         BindingFlags.Instance,
                                                                                               null, null, null);

                        if (currentObj is ORMObject)
                        {
                            ormObj = Instanciate(currentObj);
                        }


                        if (ormObj != null) ormObj.InternalFireBeforeLoaded();
                    }

                    if (columns[columnCounter].Properties.AcceptNulls && Convert.IsDBNull(row[columns[0].ColumnName]))
                    {
                        continue;
                    }

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


        /// <summary>
        /// Gets the database server date time.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns></returns>
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
                cnn.Close();
            }

            return result;
        }

        /// <summary>
        /// Gets the server date time.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
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
                cnn.Close();
            }

            return result;
        }

        /// <summary>
        /// Compares the record digest of the specified object againts the digest stored
        /// in the database. Object must have a record digest column specified.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
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



        /// <summary>
        /// Loads a object asynchronously
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public static IAsyncResult BeginLoad(object source, AsyncCallback callback, object state)
        {
            ORMObject ormObject = Instanciate(source);
            return ormObject.BeginLoad(callback, state);
        }

        public static ORMObject EndLoad(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMLoadCaller ormActionDelegate = (ORMLoadCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        /// <summary>
        /// Loads the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ORMObject Load(object source)
        {
            ORMObject ormObject = Instanciate(source);
            ormObject.Load();
            return ormObject;
        }

        /// <summary>
        /// Refresh current object directly from database. This method will bypass any cached object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public static IAsyncResult BeginRefresh(object source, AsyncCallback callback, object state)
        {
            ORMObject ormObject = Instanciate(source);
            return ormObject.BeginRefresh(callback, state);
        }

        public static ORMObject EndRefresh(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMLoadCaller ormActionDelegate = (ORMLoadCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }


        /// <summary>
        /// Refreshes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ORMObject Refresh(object source)
        {
            ORMObject ormObject = Instanciate(source);
            ormObject.Refresh();
            return ormObject;
        }


        public static IAsyncResult BeginRemove(object source, AsyncCallback callback, object state)
        {
            ORMObject orm = Instanciate(source);
            return orm.BeginRemove(callback, state);
        }

        public static ActionResult EndRemove(IAsyncResult result)
        {
            if (result == null) return ActionResult.Undefined;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMRemoveCaller ormActionDelegate = (ORMRemoveCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
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


        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
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

        /// <summary>
        /// Test whether two object of the same type are different database instances
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        internal static bool AreDifferent(object source, object dest)
        {
            if (source == null || dest == null) return false;

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
