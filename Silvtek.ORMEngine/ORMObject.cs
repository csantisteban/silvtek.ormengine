#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Reflection;
using Silvtek.ORMEngine.Configs;
using Silvtek.ORMEngine.DataQuery;
using Silvtek.ORMEngine.Schema;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine
{

    /// <summary>
    /// ORM async caller delegate
    /// </summary>
    public delegate ORMObject ORMLoadCaller();

    public delegate ORMObject ORMCreateCaller();
    public delegate ActionResult ORMRemoveCaller();
    public delegate ORMObject ORMSaveCaller(bool load);

	/// <summary>
	/// Represents an object mapped to a database table.
	/// </summary>
    [Serializable]
    public class ORMObject : ICloneable, ISerializable, IDataObject, IORMObject
	{
        // Holds the unique identifier string for this object.
        private string identifierstring;

	    private long bindtimestamp;

        // tells wheather this object was loaded from the database 
        // for save.
	    private bool loadedForSave;

        private readonly ORMObjectInfo objectInfo;
        private readonly object sourceBound;
	    private string digest;
        private RecordVersion recordVersion; 
        private Hashtable bag;
	    

        // Hold the operation result.
        private ActionResult result;

        // Holds the hashcode string.
        private int hashcode;

         // Holds whether currently belongs to an transaction.
        [NonSerialized]
        private bool belongsTransaction;

        // Hold the dataobjectfields objects.
        private MemberFieldList memberFieldList;

        [NonSerialized]
        private OperationNotifierDelegate operationNotifier;

		private object syncRoot;

		/// <summary>
		/// Used by the <see cref="Transaction"/> class
		/// </summary>
        protected internal delegate void AfterTransactionEventHandler(ORMObject obj, Action op);

        ///<summary>
        /// Fired when the DataObject is changed in the datasource.
        ///</summary>
        public event ORMEventHandler Saved;

        ///<summary>
        /// Fired the record associated with current instance
        /// is removed from the datasource (Database).
        ///</summary>
        public event ORMEventHandler Removed;

        ///<summary>
        /// Fired hen the dataobject is created (stored) in the
        /// datasource (Database).
        ///</summary>
        public event ORMEventHandler Created;

		#region -- Public Events --

        ///<summary>
        /// Fired just before the record is created in the database.
        ///</summary>
        public event ORMEventHandler BeforeCreated;

        ///<summary>
        /// Fired after this record is created in the database.
        ///</summary>
        public event ORMEventHandler AfterCreated;

        ///<summary>
        /// Fired just before this record changes is stored in the database.
        ///</summary>
        public event ORMEventHandler BeforeSaved;

        ///<summary>
        /// Fired after this record changes is stored in the database.
        ///</summary>
        public event ORMEventHandler AfterSaved;

        ///<summary>
        /// Fired just before loading this record from the database.
        ///</summary>
        public event ORMEventHandler BeforeLoaded;

        ///<summary>
        /// Fired just after this record is loaded from the database.
        ///</summary>
        public event ORMEventHandler AfterLoaded;


        ///<summary>
        /// Fired just before this record is removed from the database.
        ///</summary>
        public event ORMEventHandler BeforeRemoved;

        ///<summary>
        /// Fired after this record is removed from the database.
        ///</summary>
        public event ORMEventHandler AfterRemoved;


        ///<summary>
        /// Fired when this object is binding to the database. <br/>
        /// This happen when is being loaded or created.
        ///</summary>
        public event ORMEventHandler DataBinding;

        ///<summary>
        /// Fired when this record is not found in the database.
        ///</summary>
        public event ORMEventHandler RecordNotFound;

        ///<summary>
        /// Fired when failed to perform an database operation.
        ///</summary>
        public event ActionFailedEventHandler Failed;

	    public event ORMEventHandler Init;

        ///<summary>
        /// Fired when an Exception is encounter trying to perform
        /// an operation with the database.
        ///</summary>
        public event ORMErrorEventHandler Error;

		#endregion

        ///<summary>
        /// TODO: DOCUMENT THIS.
        ///</summary>
        [NonSerialized]
        internal protected AfterTransactionEventHandler AfterTransaction;

	    private bool isLoaded;

	    /// <summary>
        /// Gets the bag value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal object GetBagValue(string key)
        {
            if (bag == null) return null;
            return bag[key];
        }

        /// <summary>
        /// Adds the bag value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void AddBagValue(string key, object value)
        {
            if (bag == null) bag = new Hashtable();
            bag.Add(key, value);
        }

        /// <summary>
        /// Gets the instance of <see cref="ORMObjectInfo" /> for this class.
        /// </summary>
        /// <value>The object info.</value>
        public ORMObjectInfo ObjectInfo
        {
            get { return objectInfo; }
        }

        /// <summary>
        /// Gets the an syncronized object for this instance.
        /// </summary>
		public Object SyncRoot
		{
			get {return syncRoot;}
		}

        ///<summary>
        /// Gets the result of the last transaction against the database.
        ///</summary>
        ///<value>
        /// The <see cref="ActionResult" /> of the last transaction
        /// performed against the database.
        ///</value>
        public ActionResult Result
        {
          get {return result; }
        }

        /// <summary>
        /// Gets the age since this instance has been bind to the database. Which means, since the last
        /// time is a record was created using this object, or last time this record has loaded, or updated.
        /// </summary>
	    public long Age
	    {
            get { return (bindtimestamp == 0) ? 0 : (DateTime.Now.Ticks - bindtimestamp); }
	    }

		///<summary>
		/// Tells whether any fields of this object has changed from
		/// its original value.
		///</summary>
		internal bool HasChanged
		{
			get { return FieldList.HasChanges; }
		}

		///<summary>
		/// Gets the field list for the current instances.
		///</summary>
		///<value>
		/// The <see cref="MemberFieldList" /> for the current instance.
		///</value>
		internal protected MemberFieldList FieldList
		{
			get { return memberFieldList; }
		}

#if !NET11
//#pragma warning disable 1587
        /// <summary>
        /// Gets the source object.
        /// </summary>
        /// <value>The source object.</value>
//#pragma warning restore 1587

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
#endif
        internal object SourceObject
        {
            get { return (sourceBound == null ? this : sourceBound ); }
        }

        /// <summary>
        /// Gets the record version number since its was created. 
        /// When a record is created, its version is 0; This property only works
        /// if the ORM object has a "RecordDigestAttribute" applied to one of columns fields.
        /// </summary>
	    public int RecordVersionNumber
	    {
	        get
	        {
                if (recordVersion == null) return 0;
	            return recordVersion.VersionNumber;
	        }
	    }

		#region -- Constructors --

        ///<summary>
        /// Constructor
        ///</summary>
        protected ORMObject()
        {
            objectInfo = new ORMObjectInfo(GetType());
            result = ActionResult.TransactionNotPerformed;
            belongsTransaction = false;
            sourceBound = null;
            // recordDigest = String.Empty;
            syncRoot = new Object();

            memberFieldList = new MemberFieldList(objectInfo, SourceObject);

            FireOnInit();
        }


	    /// <summary>
        /// Private Constructor.
        /// </summary>
         /// <param name="source"></param>
		internal ORMObject(object source)
		{
            objectInfo = new ORMObjectInfo(source.GetType());
			result = ActionResult.TransactionNotPerformed;	
			belongsTransaction = false;
			syncRoot = new Object();
            sourceBound = source;
         //   recordDigest = String.Empty;

            memberFieldList = new MemberFieldList(objectInfo, SourceObject);
            FireOnInit();

		}

		#endregion


        internal string GetCreateStatement()
        {
            //Fire event.
            FireBeforeCreated();

            // Do not perform any transaction if this object
            // is already bounded to the database, meaning that
            // has a  relation with a row in the database.
            if (objectInfo.BindingStatus == BindingStatus.Bound)
            {
                FireFailed(Action.Create, Utility.Resources["FAILED_REASON_CREATE"]);
                return String.Empty;
            }

            CheckReadOnlyState(Action.Create);
            CheckLockedState(Action.Create);

            if (ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                try
                {
                    string fulldigestValue = MapUtility.SetRecordDigest(ObjectInfo.MappedMembers, SourceObject, 1);
                    //recordDigest = MapUtility.GetDigest(fulldigestValue);
                    recordVersion = new RecordVersion(GetRecordIdentifier(), 1);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to set record digest. " + e.Message);
                }
            }

            return objectInfo.GetInsertStatement(SourceObject);
        }

	    #region -- Create Method --


        ///<summary>
        /// Created a new record in the database.
        ///</summary>
        public virtual void Create()
        {
            //Fire event.
            FireBeforeCreated();

            // Do not perform any transaction if this object
            // is already bounded to the database, meaning that
            // has a  relation with a row in the database.
            if (objectInfo.BindingStatus == BindingStatus.Bound)
            {
                FireFailed(Action.Create, Utility.Resources["FAILED_REASON_CREATE"]);
                return;
            }

            CheckReadOnlyState(Action.Create);
            CheckLockedState(Action.Create);

            

            if (ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                try
                {
                    MapUtility.SetRecordDigest(ObjectInfo.MappedMembers, SourceObject , 1);
                    //recordDigest = MapUtility.GetDigest(fulldigestValue);
                    recordVersion = new RecordVersion(GetRecordIdentifier(), 1);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Failed to set record digest. "+e.Message);
                }
            }

            if (ORMConfigurator.UseBalancerShare)
            {
                if (recordVersion == null) recordVersion = new RecordVersion(GetRecordIdentifier(), 0);
                RecordVersion.Create(this, recordVersion);
            }

            this.memberFieldList.RefreshObjectSource();
            CheckUniqueValues(Action.Create);
           
            int rv;
            string sqlText = String.Empty;
            Connection cnn = objectInfo.Schema.GetConnection();
            try
            {
                Command cmd = objectInfo.GetInsertCommand(cnn, SourceObject);
                cnn.Open();

                sqlText = cmd.CommandText;
                rv = cmd.ExecuteNonQuery();

                // FIX HERE -- Test if uses db identity should come from schema.
                // FIX HERE -- SetIndentity method should return the value. not set
                if (objectInfo.Schema.SqlBuilder.HasIdentityColumn) objectInfo.Schema.SqlBuilder.SetIdentityValue(cnn, objectInfo.Schema, SourceObject);
            }
            catch (Exception e)
            {
                //Fire Events.
                FireFailed(Action.Create, Utility.Resources["FAILED_REASON_EXCEPTION"]);
                FireError(Action.Create, e, String.Empty);

                // retrow exception.
                //string exMsg = Utility.GetResourceString("RES_DataObjectSQLException", e.Message, sql);
                string exMsg = Utility.GetResourceString("RES_DataObjectSQLException", e.Message, sqlText);
                throw new ORMException(exMsg, sqlText, e);
            }
            finally
            {
                //Closing connection.
                cnn.Close();
            }

            result = (rv == 1) ? ActionResult.Succeed : ActionResult.Failed;

            if (result == ActionResult.Succeed)
            {
               //InternalAfterOperationDelegate afterOp = new InternalAfterOperationDelegate(AfterCreateSucceed);
               //afterOp.BeginInvoke(null, null);
               AfterCreateSucceed();
            }
            else
            {
                FireFailed(Action.Create, Utility.Resources["FAILED_REASON_UNABLETOSUCCESS"]);
            }
        }

	    private void CheckUniqueValues(Action action)
	    {
	        if (ObjectInfo.MappedMembers.HasUniqueMember)
	        {

	            MapMember[] members = ObjectInfo.MappedMembers.GetAllUniqueMembers();
	            Type type = this.SourceObject.GetType();
	            foreach(MapMember member in members)
	            {
                    if (action == Action.Save)
                    {
                        if (!this.FieldList[member.MemberName].HasChanged)
                        {
                            continue;
                        }
                    }

	                AggregateQuery query = new AggregateQuery(Aggregate.Count, type, member.MemberName, false);
	                CriteriaItem item = new CriteriaItem(type, member.MemberName);
	                item.EqualTo(member.GetMemberValue(SourceObject));
	                query.Criterias.Add(item);

                    if (action == Action.Save)
                    {
                        MapMember[] pks = ObjectInfo.MappedMembers.GetAllPrimaryKeyMembers();
                        foreach (MapMember pk in pks)
                        {
                            CriteriaItem tmp = new CriteriaItem(type, pk.MemberName);
                            tmp.NotEqualTo(pk.GetMemberValue(SourceObject));
                            query.Criterias.And(tmp);
                        }
                    }

	                query.Execute(true);
	                if (query.Result.Length > 0 && Convert.ToInt32(query.Result[0]) > 0)
	                {
	                    FireFailed(Action.Create, "Unique contraint violated. ");
	                    throw new UniqueConstraintViolatedException("attempted to create record with a duplicated value for " + member.MemberName + " belonging to object " + type.FullName);
	                }
                    
	            }
	        }
	    }

	    /// <summary>
        /// Applies changes and fire events after a succesful insert. This method always
        /// should be internal.
        /// </summary>
	    internal void AfterCreateSucceed()
	    {
	        if (objectInfo.MappedMembers.HasDbIdentityMember || objectInfo.MappedMembers.HasTimeStamp)
	        {
	            FieldList.RefreshObjectSource();

                if (!belongsTransaction) FireCreated();
	            FieldList.CommitChanges();
	        }
	        else
	        {
                if (!belongsTransaction) FireCreated();
	        }

	        objectInfo.SetBindStatus(BindingStatus.Bound);      
	        Bind(false);

	        QueryCache.Current.RemoveCacheForType(SourceObject.GetType());
	        NotifyOperation(Action.Create);
	        if (!belongsTransaction) FireAfterCreated();
	    }

	    #endregion

        ///<summary>
        /// Refresh current instance with data from the database.
        ///</summary>
        public void Refresh()
        {
           // FIRE EVENT
           FireBeforeLoaded();
           string sql = objectInfo.GetSelectStatement(SourceObject);
           Connection cnn = objectInfo.Schema.GetConnection();



           try
           {
              
               Command cmd = new Command(sql, cnn);
               cnn.Open();
               DataReader reader = cmd.ExecuteReader();

               // Read recordset
               if (reader.Read())
               {
                   MapMemberList memberList = objectInfo.MappedMembers;

                   for (int i = 0; i < memberList.Count; i++)
                   {
                       if (!memberList[i].Properties.IsPrimaryKey)
                       {

                           if (memberList[i].Properties.AcceptNulls && reader.IsDBNull(i))
                           {
                               memberList[i].SetDataSourceValue(SourceObject, null);
                               continue;
                           }

                           if (memberList[i].Properties.DataType != DataType.Binary)
                           {
                               memberList[i].SetDataSourceValue(SourceObject, reader.GetValue(i));

                               if (memberList[i].Properties.IsRecordDigestEnabled)
                               {
                                   string fullDigestString = Convert.ToString(reader.GetValue(i));
                                   int version = MapUtility.GetRecordVersion(fullDigestString);
                                   //recordDigest = MapUtility.GetDigest(fullDigestString);
                                   recordVersion = new RecordVersion(GetRecordIdentifier(), version);
                               }
                           }
                           else
                           {
                               if (!reader.IsDBNull(i))
                               {

                                   DataStream dataStream = new DataStream(memberList[i]);

                                   Byte[] buffer = new byte[ORMConfigurator.BufferSize];
                                   int startIndex = 0;
                                   long bytesRead;
                                   do
                                   {
                                       bytesRead = reader.GetBytes(i, startIndex, buffer, 0, buffer.Length);
                                       if (bytesRead == 0) break;
                                       dataStream.Write(buffer, 0, (int)bytesRead);
                                       dataStream.Flush();
                                       startIndex += buffer.Length;
                                   } while (buffer.Length == bytesRead);

                                   dataStream.Position = 0;

                                   memberList[i].SetDataSourceValue(SourceObject, dataStream);
                               }
                               else
                               {
                                   memberList[i].SetDataSourceValue(SourceObject, null);

                               }
                           }
                       }
                   }

                   // commit all files
                   foreach (MemberField field in memberFieldList)
                   {
                       field.Commit();
                   }

                   result = ActionResult.Succeed;
                   objectInfo.SetBindStatus(BindingStatus.Bound);

                  // Debug.WriteLine("Record Loaded from database. Identifier: " + GetRecordIdentifier(),
                  //                 Resources.TRACE_CATEGORY);
               }
               else
               {
                   FireRecordNotFound();
                   result = ActionResult.RecordNotFound;
               }

               reader.Close();

           }
           catch (Exception e)
           {
               //Fire Events.
               FireFailed(Action.Load, Utility.Resources["FAILED_REASON_EXCEPTION"]);
               FireError(Action.Load, e, sql);
               string exMsg = Utility.GetResourceString("RES_DataObjectSQLException", e.Message, sql);
               throw new ORMException(exMsg, e);
           }
           finally
           {
               cnn.Close();
           }

            // Notify Load Operation.
           NotifyOperation(Action.Load);

           // Fire Bind Event.
           if (result == ActionResult.Succeed)Bind(false);

           // Fire After Load Event.
           FireAfterLoaded();

        }

        internal void SetRecordDigest(string fullDigestString)
        {
            int version = MapUtility.GetRecordVersion(fullDigestString);
            //recordDigest = MapUtility.GetDigest(fullDigestString);
            recordVersion = new RecordVersion(GetRecordIdentifier(), version);
        }

        
        #region -- Async actions -- 
        
        #region -- Async Save methods and helper methods -- 

        private ORMObject InternalAsyncSave(bool load)
        {
            Save(load);
            return this;
        }

        /// <summary>
        /// Saves this object to the database asynchrously. 
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginSave(AsyncCallback callback, object state)
        {
            ORMSaveCaller ormActionDelegate = new ORMSaveCaller(InternalAsyncSave);
            return ormActionDelegate.BeginInvoke(false, callback, state);
        }

        public ORMObject EndSave(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMSaveCaller ormActionDelegate = (ORMSaveCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result); 
        }
   

        /// <summary>
        /// Saves this object to the database asynchrously. 
        /// </summary>
        /// <param name="load">Flags whether this object should be reloaded from the database.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginSave(bool load, AsyncCallback callback, object state)
        {
            ORMSaveCaller ormActionDelegate = new ORMSaveCaller(InternalAsyncSave);
            return ormActionDelegate.BeginInvoke(load, callback, state);
        }

        #endregion

        #region -- Async Create methods and helper methods --
        /// <summary>
        /// Creates a new record of this object in the database (insert)
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginCreate(AsyncCallback callback, object state)
        {
            ORMCreateCaller ormActionDelegate = new ORMCreateCaller(InternalAsyncCreate);
            return ormActionDelegate.BeginInvoke(callback, state);        
        }

        public ORMObject EndCreate(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMCreateCaller ormActionDelegate = (ORMCreateCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        private ORMObject InternalAsyncCreate()
        {
            Create();
            return this;
        }

        #endregion

        #region -- Async Load Methods --
        /// <summary>
        /// Saves this object to the database asynchrously. 
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginLoad(AsyncCallback callback, object state)
        {
            ORMLoadCaller ormActionDelegate = new ORMLoadCaller(InternalAsyncLoad);
            return ormActionDelegate.BeginInvoke(callback, state);            
        }

        public ORMObject EndLoad(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMLoadCaller ormActionDelegate = (ORMLoadCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        private ORMObject InternalAsyncLoad()
        {
            Load();
            return this;
        }

        #endregion 

        #region -- ASync Remove methods --
        /// <summary>
        /// Deletes this object from the database asynchrously.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginRemove(AsyncCallback callback, object state)
        {
            ORMRemoveCaller ormActionDelegate = new ORMRemoveCaller(InternalAsyncRemove);
            return ormActionDelegate.BeginInvoke(callback, state);          
        }

        public ActionResult EndRemove(IAsyncResult result)
        {
            if (result == null) return ActionResult.Undefined;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMRemoveCaller ormActionDelegate = (ORMRemoveCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        private ActionResult InternalAsyncRemove()
        {
            Remove();
            return this.Result;
        }

        #endregion

        #region -- Async Refresh methods --
        /// <summary>
        /// Refreshes the async.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public IAsyncResult BeginRefresh(AsyncCallback callback, object state)
        {
            ORMLoadCaller ormActionDelegate = new ORMLoadCaller(InternalAsyncRefresh);
            return ormActionDelegate.BeginInvoke(callback, state);
        }

        public ORMObject EndRefresh(IAsyncResult result)
        {
            if (result == null) return null;
            AsyncResult asyncResult = (AsyncResult)result;
            ORMLoadCaller ormActionDelegate = (ORMLoadCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        private ORMObject InternalAsyncRefresh()
        {
            Refresh();
            return this;
        }

        #endregion

        #endregion

        #region -- Load --
        ///<summary>
        /// Load current instance from the datasource.
        ///</summary>
        public virtual void Load()
	    {
	        if (objectInfo.IsLocked)
	        {
	            LockORMException e = new LockORMException(Utility.Resources["RES_LoadLocked"]);
	            FireFailed(Action.Load, Utility.Resources["FAILED_REASON_LOCKED"]);
	            FireError(Action.Load, e, String.Empty);
	            throw e;
	        }

	        // Try to get value from a shallow object with same key(if exists)
            if (objectInfo.Schema.IsCacheDisabled || !ORMConfigurator.GlobalCacheEnabled)
	        {
	            Refresh();
	            return;
	        }


	        object doSource = GlobalReferencesManager.Current.GetDataObjectFrom(GetRecordIdentifier());
	        //Debug.WriteLineIf(doSource == null, "System did not find existing reference in the cache. " + GetRecordIdentifier());

	        if (doSource == null)
	        {
	            // Load from database. Refresh method will fire AfterLoad event
	            // and will notify the Load Operation by invoking NotifyOperation
	            // method.
	            Refresh();
	            return;
	        }

	       // Debug.WriteLine(String.Format(Utility.Resources["TRACE:LOADED_FROM_REFERENCE"], GetRecordIdentifier()), Resources.TRACE_CATEGORY);

	        // Fires before load event.
	        FireBeforeLoaded();

	        // Defines that this is a copy.
	        //CopyFieldsFrom(doSource);

	        ORM.Copy(doSource, this.SourceObject);
	        FieldList.RefreshObjectSource();
	        FieldList.CommitChanges();

	        if (objectInfo.MappedMembers.HasDigestRecordMember)
	        {
	            foreach (MapMember member in objectInfo.MappedMembers)
	            {
	                if (member.Properties.IsRecordDigestEnabled)
	                {
	                    this.SetRecordDigest(Convert.ToString(member.GetMemberValue(SourceObject)));
	                }
	            }
	        }

	        objectInfo.RegisterAsCopy();
	        result = ActionResult.Succeed;
	        objectInfo.SetBindStatus(BindingStatus.Bound);

	        // Notify Load Operation.
	        NotifyOperation(Action.Load);

	        // Fire After Load Event.
	        Bind(true);
	        FireAfterLoaded();
        }

        #endregion

        #region -- Remove Methods --
        ///<summary>
        /// Remove the current instance from the datasource.
        ///</summary>
        ///<param name="load">
        /// Whether needs to be loaded or not.
        ///</param>
        public virtual void Remove(bool load)
        {
            if ((objectInfo.BindingStatus == BindingStatus.Unbind) && load)
            {
                Load();
                if (result == ActionResult.Succeed) Remove();
            }
            else
            {
                Remove();
            }
           
        }

        /// <summary>
        /// Gets the delete statement.
        /// </summary>
        /// <returns></returns>
        internal string GetRemoveStatement()
        {
            //Fire event
            FireBeforeRemoved();

            // Check if primary keys has not being changed.
            if (objectInfo.BindingStatus == BindingStatus.Bound) ValidatePrimaryKeysModifications();

            this.CheckReadOnlyState(Action.Remove);
            this.CheckLockedState(Action.Remove);

            return objectInfo.GetDeleteStatement(SourceObject);
        }


        ///<summary>
        /// Removes record from datasource.
        ///</summary>
        public virtual void Remove()
        {
           //Fire event
           FireBeforeRemoved();         

           // Check if primary keys has not being changed.
           if (objectInfo.BindingStatus == BindingStatus.Bound)ValidatePrimaryKeysModifications();
         
            this.CheckReadOnlyState(Action.Remove);
            this.CheckLockedState(Action.Remove);

            string sql = objectInfo.GetDeleteStatement(SourceObject);
            Connection cnn = objectInfo.Schema.GetConnection();
 
            int rv;

            try
            {
                ;
                Command cmd = new Command(sql, cnn);
                cnn.Open();
                rv = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                FireFailed(Action.Remove, Utility.Resources["FAILED_REASON_EXCEPTION"]);
                FireError(Action.Remove, e, sql);
                string exMsg = Utility.GetResourceString("RES_DataObjectSQLException", e.Message, sql);
                throw new ORMException(exMsg, e);

            }
            finally
            {
                cnn.Close();
            }

            result = (rv == 1) ? ActionResult.Succeed : ActionResult.Failed;
            if (result == ActionResult.Succeed)
            {
                // if the removal of the record was sucessfull, then update the identifier string,
                // if hasn't have any value for brocasting the removal.
              //InternalAfterOperationDelegate afterOp = new InternalAfterOperationDelegate(AfterRemoveSucceed);
             //  afterOp.BeginInvoke(null, null);
               AfterRemoveSucceed();
            }
            else
            {
                FireFailed(Action.Remove, Utility.Resources["FAILED_REASON_UNABLETOSUCCESS"]);
            }

        }

	    internal void AfterRemoveSucceed()
	    {
	        if (identifierstring == null || identifierstring.Length == 0)
	        {
	            identifierstring = GetRecordIdentifier();
	        }

            if (!belongsTransaction) FireRemoved();

	        objectInfo.SetBindStatus(BindingStatus.Removed);
	        GlobalReferencesManager.Current.BroadcastRemoval(this);

	       
                
	        //recordDigest = String.Empty;
	        QueryCache.Current.RemoveCacheForType(SourceObject.GetType());
	        NotifyOperation(Action.Remove);
	        if (!belongsTransaction) FireAfterRemoved();
	    }

	    #endregion

		#region -- Store Methods --

        internal void InternalLoadForSave()
        {
            LoadForSave();
            if (Result == ActionResult.Succeed)
            {
                loadedForSave = false;
            }
        }

	    /// <summary>
		/// Saves changes to the database. 
		/// </summary>
		/// <param name="load">Whether to load before saving.</param>
		public void Save(bool load)
		{

            if (load)
            {
                LoadForSave();
                if (Result == ActionResult.Succeed)
                {
                    InternalSave();
                    loadedForSave = false;
                }

                
            }
            else if (objectInfo.BindingStatus == BindingStatus.Bound)
            {
                InternalSave();
            }

            // Do not perform any operation if this object is not bounded to
            // the database.
            else if (objectInfo.BindingStatus == BindingStatus.Unbind)
            {
                FireFailed(Action.Save, Utility.Resources["FAILED_REASON_UNBOUNDED"]);
                result = ActionResult.Failed;
                return;
            }
            else if (objectInfo.BindingStatus == BindingStatus.Removed)
            {
                string exMsg = String.Format("Cannot save and object that has been removed. Object: {0}", GetRecordIdentifier());
                throw new ORMException(exMsg);
            }
            else
            {
                string exMsg = String.Format("Unrecognized Binding Status: " + objectInfo.BindingStatus);
                throw new ORMException(exMsg);
            }
		}

        // Reload again the object from data source and saves it with changes.
	    private void LoadForSave()
	    {
            // do not double load if its already bound to data.
            if (this.ObjectInfo.BindingStatus == BindingStatus.Bound)
            {
                return;
            }

	        Hashtable currentValuesList = new Hashtable(objectInfo.Schema.MappedMembers.Count);
	        Hashtable preserveValues = new Hashtable();

	        foreach (MapMember currentMapMember in objectInfo.MappedMembers) 
	        {
	            currentValuesList.Add(currentMapMember.MemberName, currentMapMember.GetMemberValue(SourceObject));
	        }

	        MemberInfo[] memberInfos = SourceObject.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance |
	                                                                     BindingFlags.Public | BindingFlags.DeclaredOnly);

	        foreach (MemberInfo memberInfo in memberInfos)
	        {
	            if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
	            {
	                continue;
	            }

	            PreserveAttribute[] presserveAttributes = (PreserveAttribute[]) memberInfo.GetCustomAttributes(typeof (PreserveAttribute), true);
                    
	            if (presserveAttributes.Length == 0) continue;

	            object value = null;
                    
	            if (memberInfo.MemberType == MemberTypes.Property)
	            {
	                PropertyInfo propertyInfo = (PropertyInfo) memberInfo;
	                if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
	                {
	                    throw new ORMException("Preserve Attribute can only be placed in a field or property with set and get accesors");
	                }

	                value = propertyInfo.GetValue(SourceObject, null);
	            }
                    
	            if (memberInfo.MemberType == MemberTypes.Field)
	            {
	                FieldInfo fieldInfo = (FieldInfo) memberInfo;
	                value = fieldInfo.GetValue(SourceObject);
	            }

	            if (value == null && presserveAttributes[0].PreserveNull)
	            {
	                preserveValues.Add(memberInfo, value);
	            }
	            else if (value != null)
	            {
	                preserveValues.Add(memberInfo, value);
	            }
	        }
             
	        // load object from database, so we can check existances and changes.
	        Load();

	        if (Result == ActionResult.Succeed)
	        {
	            loadedForSave = true;
	            foreach (MapMember currentMapMember in objectInfo.MappedMembers) 
	            {
	                if (currentMapMember.Properties.IsRecordDigestEnabled) continue;
	                object value = currentValuesList[currentMapMember.MemberName];

	                if (MapUtility.CanUpdateMember(currentMapMember, value) && !Equals(currentMapMember.Properties.DefaultValue, value))
	                {
	                    currentMapMember.SetMemberValue(SourceObject, value);
	                }              

	            }

	            foreach (MemberInfo memberInfo in preserveValues.Keys)
	            {
	                object oldValue = preserveValues[memberInfo];
	                if (memberInfo.MemberType == MemberTypes.Property)
	                {
	                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
	                    propertyInfo.SetValue(SourceObject, oldValue, null);
	                }

	                if (memberInfo.MemberType == MemberTypes.Field)
	                {
	                    FieldInfo fieldInfo = (FieldInfo)memberInfo;
	                    fieldInfo.SetValue(SourceObject, oldValue);
	                }
	            }

	            memberFieldList.RefreshObjectSource();

	            objectInfo.RemoveDirtyStatus(); 
                  
	        }
	    }

        /// <summary>
        /// Gets the save statement to be used in a batch.
        /// </summary>
        /// <returns></returns>
	    internal string GetSaveStatement()
        {
            bool load = (ObjectInfo.BindingStatus != BindingStatus.Bound);

            if (!load)
            {
                LoadForSave();
            }

            // TODO: Check if we can use the digest for a quick comparasion of modified fields.
            if (ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                DigestComparisonResult comparisonResult = MapUtility.CompareRecordDigest(ObjectInfo.MappedMembers, SourceObject);
                if (comparisonResult == DigestComparisonResult.Equal)
                {
                    result = ActionResult.NotModified;
                    return String.Empty;
                }
                if (comparisonResult == DigestComparisonResult.EmptyDigest)
                {
                    string fulldigestValue = MapUtility.SetRecordDigest(ObjectInfo.MappedMembers, SourceObject, 2);
                    //recordDigest = MapUtility.GetDigest(fulldigestValue);
                    recordVersion = new RecordVersion(GetRecordIdentifier(), 2);
                }
                else
                {
                    CheckRecordVersion();
                }
            }
            else
            {
                CheckRecordVersion();
            }

            FireBeforeSaved();

            CheckDirtyObject(Action.Save);

            // Do not perform any operation if this object is not bounded to
            // the database.
            if (objectInfo.BindingStatus == BindingStatus.Unbind)
            {
                FireFailed(Action.Save, Utility.Resources["FAILED_REASON_UNBOUNDED"]);
                result = ActionResult.Failed;
                return String.Empty;
            }

            CheckReadOnlyState(Action.Save);
            CheckLockedState(Action.Save);

            // Check that primary keys has not been changed.
            ValidatePrimaryKeysModifications();

            if (!memberFieldList.HasChanges)
            {
                result = ActionResult.NotModified;
                return String.Empty;
            }

            return objectInfo.GetUpdateStatement(this.FieldList, SourceObject); 
        }

        ///<summary>
        /// Saves changes to the database.
        ///</summary>
        public virtual void Save()
        {
            InternalSave();
        }


        private  void InternalSave()
        {
                
            // TODO: Check if we can use the digest for a quick comparasion of modified fields.
            if (ObjectInfo.MappedMembers.HasDigestRecordMember)
            {
                DigestComparisonResult comparisonResult = MapUtility.CompareRecordDigest(ObjectInfo.MappedMembers, SourceObject);
                if (comparisonResult == DigestComparisonResult.Equal)
                {
                    result = ActionResult.NotModified;
                    return;
                }
                if (comparisonResult == DigestComparisonResult.EmptyDigest)
                {
                    string fulldigestValue = MapUtility.SetRecordDigest(ObjectInfo.MappedMembers, SourceObject, 2);
                    //recordDigest = MapUtility.GetDigest(fulldigestValue);
                    recordVersion = new RecordVersion(GetRecordIdentifier(), 2);
                }
                else
                {
                    CheckRecordVersion();
                }
            }
            else
            {
                CheckRecordVersion();
            }

            FireBeforeSaved();

			CheckDirtyObject(Action.Save);

           // Do not perform any operation if this object is not bounded to
           // the database.
           if (objectInfo.BindingStatus == BindingStatus.Unbind)
           {
              FireFailed(Action.Save, Utility.Resources["FAILED_REASON_UNBOUNDED"]);
              result = ActionResult.Failed;
              return;
           }

            CheckReadOnlyState(Action.Save);
            CheckLockedState(Action.Save);

            int rv = 0;
            // Check that primary keys has not been changed.
            ValidatePrimaryKeysModifications();
            memberFieldList.RefreshObjectSource();
            if (!memberFieldList.HasChanges)
            {
                result = ActionResult.NotModified;
                return;
            }

            CheckUniqueValues(Action.Save);
            Connection cnn = objectInfo.Schema.GetConnection();

            lock(SyncRoot)
            {
                string sqltext = String.Empty;
                try
                {

                    Command cmd = objectInfo.GetUpdateCommand(cnn, memberFieldList, SourceObject);
                    if (cmd == null)
                    {
                        result = ActionResult.NotModified;
                        return;
                    }

                    cnn.Open();

                    sqltext = cmd.CommandText;
                    rv = cmd.ExecuteNonQuery();
                    //rv = 0;
                    //using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    //{
                    //    if (reader != null)
                    //    {
                    //        reader.Close();
                    //        rv = reader.RecordsAffected;
                    //    }
                        
                    //}
        


                }
                catch (Exception e)
                {
                    FireFailed(Action.Save, Utility.Resources["FAILED_REASON_EXCEPTION"]);
                    FireError(Action.Save, e, sqltext);
                    string exMsg = Utility.GetResourceString("RES_DataObjectSQLException", e.Message, sqltext);
                    throw new ORMException(exMsg, sqltext, e);

                }
                finally
                {
                    cnn.Close();
                }
            }

            result = (rv == 1) ? ActionResult.Succeed : ActionResult.Failed;

            if (result == ActionResult.Succeed)
            {
               //InternalAfterOperationDelegate afterSaveSucceed = new InternalAfterOperationDelegate(AfterSaveSucceed);
               // afterSaveSucceed.BeginInvoke(null, null);
                AfterSaveSucceed();
            }
            else
            {
                FireFailed(Action.Save, Utility.Resources["FAILED_REASON_UNABLETOSUCCESS"]);
            }

        }

	    internal void AfterSaveSucceed()
	    {
	        if (objectInfo.MappedMembers.HasTimeStamp)
	        {
	            FieldList.RefreshObjectSource();
	        }

            if (!belongsTransaction) FireSaved();

	        FieldList.CommitChanges();
	        ORMReferencesManager manager = GlobalReferencesManager.Current.GetManagerInstance(GetRecordIdentifier());
	        if (manager != null) manager.BroadcastUpdate(this);

	        QueryCache.Current.RemoveCacheForType(SourceObject.GetType());
	        NotifyOperation(Action.Save);
	        if (!belongsTransaction) FireAfterSaved();
	    }

	    #endregion

        public string GetDigest()
        {
            if (this.digest == null || this.digest.Length == 0)
            {
                digest = MapUtility.GenerateStandardRecordDigest(this.ObjectInfo.MappedMembers, this.SourceObject);
            }
            else if (FieldList.HasChanges)
            {
                digest = MapUtility.GenerateStandardRecordDigest(this.ObjectInfo.MappedMembers, this.SourceObject);
            }
            else
            {
                FieldList.RefreshObjectSource();
                if (FieldList.HasChanges)
                {
                    digest = MapUtility.GenerateStandardRecordDigest(this.ObjectInfo.MappedMembers, this.SourceObject);
                }
            }

            return digest;
        }

       #region Overrided Object methods

       ///<summary>
       /// Serves as a hash function for a particular type, 
       /// suitable for use in hashing algorithms and data structures like a hash table.
       ///</summary>
       public override int GetHashCode()
       {
           if (hashcode == 0)
           {
               Guid guid = Guid.NewGuid();
               hashcode = guid.GetHashCode();
           }

           return hashcode;
       }


       /// <summary>
       /// Determines whether two Object instances are equal.
       /// </summary>
       /// <param name="obj">The Object to compare with the current <b>Object.</b></param>
       /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
       public override bool Equals(object obj)
       {
           if (obj == null) return false;
           if(obj.GetType() != GetType())return false;
           return (obj.GetHashCode() == GetHashCode());
       }


       /// <summary>
       /// Returns a String that represents the current Object.
       /// </summary>
       /// <returns>A String that represents the current Object.</returns>
       public override string ToString()
       {
           string toString = GetRecordIdentifier();
           if (toString.Length == 0)
           {
               toString = String.Format("{0}>>Unbind", SourceObject);
           }

           return toString; 
       }


       #endregion

       #region ICloneable method.
        
        internal ORMObject InternalCopy()
        {
            //object[] args = new object[1];
            //args[0] = sourceBound;
            //ORMObject copy = (ORMObject)Activator.CreateInstance(Source.GetType(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
            Type type = SourceObject.GetType();
            object clone = Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(SourceObject);
                field.SetValue(clone, value);
            }
            
            ORMObject copy = new ORMObject(clone);
            copy.memberFieldList = new MemberFieldList(copy.ObjectInfo, clone);
            copy.identifierstring = identifierstring;
          //  Debug.Assert(copy.SourceObject == SourceObject, "After creating internal copy, they don't match.");
            return copy;
        }

	   /// <summary>
	   /// Clone this object.
	   /// </summary>
	   /// <returns>A new object identically to this instance.</returns>
       public virtual object Clone()
       {

          object cloneSourceObject = Activator.CreateInstance(SourceObject.GetType(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
          ORMObject copy = new ORMObject(cloneSourceObject);
          copy.CopyFieldsFrom(this);

        
          return copy;

       }


		internal void CopyFieldsFrom(ORMObject obj)
		{
           if (objectInfo.Schema.Type == SourceObject.GetType())
            {
                for (int i = 0; i < objectInfo.MappedMembers.Count; i++)
                {
                    MapMember mapMember = objectInfo.MappedMembers[i];
                    object value = mapMember.GetMemberValue(obj.SourceObject);
                    if (MapUtility.CanUpdateMember(mapMember, value))
                    {
                        mapMember.SetMemberValue(SourceObject, value);
                    }
                }

                memberFieldList = new MemberFieldList(objectInfo, SourceObject);
            }
          
		}

       #endregion

       #region Internal Methods.

       ///<summary>
       /// Returns the identifier string that identify uniquely this
       /// record.
       ///</summary>
       ///<remarks>
       /// This string does not uniquely identify the instance, it identify
       /// the record based in the primary keys.
       ///</remarks>
        internal string GetRecordIdentifier()
       {
           //Trace.Assert(objectInfo != null, "Assertion failed");

           if (objectInfo.BindingStatus == BindingStatus.Unbind)
           {
               return objectInfo.CreateIdentifierString(SourceObject);
           }


           if (objectInfo.BindingStatus == BindingStatus.Bound && (identifierstring == null || identifierstring.Length == 0))
           {
               identifierstring = objectInfo.CreateIdentifierString(SourceObject);
           }

           // Trace.Assert( (identifierstring != null && identifierstring.Length > 0), "returning an empty identifier string. Failed Assertion");
           return identifierstring;
       }


	    internal void MarkAsRemoved()
        {
            FireRemoved();
            objectInfo.SetBindStatus(BindingStatus.Removed);
        }


	   internal void RegisterTransaction(bool register)
       {
          belongsTransaction = register;
       }

       internal void InvokeCreated()
       {
           FireCreated();
           FireAfterCreated();
       }

       internal void InvokeRemoved()
       {
           FireRemoved();
           FireAfterRemoved();
       }

       internal void InvokeChanged()
       {
           FireSaved();
           FireAfterSaved();
       }

       internal void RegisterOperationNotifierDelegate(OperationNotifierDelegate notifier)
       {
          operationNotifier = notifier;
       }


		/// <summary>
		/// Add the reference to the syncRoot object.
		/// </summary>
		/// <param name="obj">The syncRoot object</param>
		internal void SetSyncRoot(object obj)
		{
			syncRoot = obj;
		}

       #endregion

       ///<summary>
       /// Forces to mark the object as if where bound to the datasource.
       ///</summary>
       ///<remarks>
       /// Invoke this method only on those object that has a relation
       /// with the datasource.
       ///</remarks>
       protected internal void BindByForce()
       {
          objectInfo.SetBindStatus(BindingStatus.Bound);
          Bind(false);
       }

	   
       private void ValidatePrimaryKeysModifications()
       {
         //Trace.Assert(memberFieldList != null);


         for (int i = 0; i < objectInfo.MappedMembers.Count; i++)
         {
             MapMember tmpMapMember = objectInfo.MappedMembers[i];

             if (!(tmpMapMember.Properties.IsPrimaryKey)) continue;

             MemberField field = memberFieldList.Get(tmpMapMember.MemberName);

             if (tmpMapMember.Properties.IsPrimaryKey && field.HasChanged)
             {
                 throw new PrimaryKeysChangedException(Utility.Resources["RES_PKChangedAfterBind"]);
             }
         }
       }

        public DigestComparisonResult CompareRecordDigest()
        {
            if (!objectInfo.MappedMembers.HasDigestRecordMember)
            {
                return DigestComparisonResult.NoDigestMemberExist;
            }

            if (objectInfo.BindingStatus != BindingStatus.Bound)
            {
                return DigestComparisonResult.NotBind;
            }

            if (objectInfo.IsDirty || memberFieldList.HasChanges)
            {
                return DigestComparisonResult.DirtyObject;
            }


            return MapUtility.CompareRecordDigest(objectInfo.MappedMembers, SourceObject);
           
        }

        ///<summary>
        /// Mark current object as binding to a datasource and
        /// fires the <see cref="DataBinding" /> event.
        ///</summary>
        ///<param name="isFromRef">
        /// Whether the binding where from a shallow reference or
        /// directly from the database.<br/>.
        ///</param>
        private void Bind(bool isFromRef)
        {
            //Trace.Assert((objectInfo.BindingStatus == BindingStatus.Bound),
                        // Utility.GetResourceString("TRACE:BIDDING_AUBOUND_OBJECT", GetRecordIdentifier()));


            if (!isFromRef)
            {
                memberFieldList = new MemberFieldList(objectInfo,SourceObject);
            }


            if (SourceObject is IORMObject)
            {
                ((IORMObject) SourceObject).IsLoaded = true;
            }

            bindtimestamp = DateTime.Now.Ticks;
            FireDataBinding();

            if (!objectInfo.IsIsolated)
            {
                try
                {
                    GlobalReferencesManager.Current.Add(this);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

	    private void NotifyOperation(Action op)
        {
           if (operationNotifier != null)operationNotifier(this, op);
        }

        /// <summary>
        /// returns the current record version if any. This method can return null in case 
        /// the ORM object does not have been tagged with a 'RecordDigest' attribute for 
        /// controlling record version.
        /// </summary>
        internal RecordVersion GetRecordVersion()
        {
            return recordVersion;    
        }


		#region -- Serialization constructor and methods --

        public virtual void GetObjectData(SerializationInfo si, StreamingContext cxt)
        {

            si.AddValue("Result", result);
            si.AddValue("DataObjectFields", memberFieldList);
            si.AddValue("ObjectInfo", objectInfo);

            if (SourceObject is ISerializable)
            {
                si.AddValue("Source", SourceObject);
            }

            for (int i = 0; i < objectInfo.MappedMembers.Count; i++)
            {
                MapMember col = objectInfo.MappedMembers[i];
                si.AddValue(col.MemberName, col.GetMemberValue(SourceObject));
            }
        
        }

        protected ORMObject(SerializationInfo si, StreamingContext ctx)
        {

            result = (ActionResult)si.GetValue("Result", typeof(ActionResult));
            memberFieldList = (MemberFieldList)si.GetValue("DataObjectFields", typeof(MemberFieldList));
            objectInfo = (ORMObjectInfo)si.GetValue("ObjectInfo", typeof(ORMObjectInfo));
            sourceBound = si.GetValue("Source", objectInfo.Schema.Type);
            //recordDigest = si.GetString("recordDigest");
            syncRoot = new Object();


            for (int i = 0; i < objectInfo.MappedMembers.Count; i++)
            {
                MapMember col = objectInfo.MappedMembers[i];
                col.SetMemberValue(SourceObject, si.GetValue(col.MemberName, typeof(Object)));
            }

        }

		#endregion

        #region -- virtual event methods --

        /// <summary>
        /// Invoked before the dataobject is created in the database.
        /// </summary>
        protected virtual void OnBeforeCreated()
        {
        }

        /// <summary>
        /// Invoked after the dataobject is created in the database.
        /// </summary>
        protected virtual void OnAfterCreated()
        {
        }

        /// <summary>
        /// Invoked just after dataobject has been created in the database.
        /// </summary>
        protected virtual void OnCreated()
        {
        }
        
        /// <summary>
        /// Invoked before the dataobject is updated in the database.
        /// </summary>
        protected virtual void OnBeforeSaved()
        {
        }

        /// <summary>
        ///  Invoked after this object is updated in the database.
        /// </summary>
        protected virtual void OnAfterSaved()
        {
        }

        /// <summary>
        /// Invoked before this object is removed from the database.
        /// </summary>
        protected virtual void OnBeforeRemoved()
        {
        }

        /// <summary>
        /// Invoked after this object is removed from the database.
        /// </summary>
        protected virtual void OnAfterRemoved()
        {
        }

        /// <summary>
        /// Invoked before this object is loaded from the database.
        /// </summary>
        protected virtual void OnBeforeLoaded()
        {
        }

        /// <summary>
        /// Invoked after this object has been loaded from the database.
        /// </summary>
        protected virtual void OnAfterLoaded()
        {
        }

        /// <summary>
        /// Invoked after object has completed initializing.
        /// </summary>
        protected virtual void OnInit()
        {
            
        }

        /// <summary>
        /// Invoked when this object failed to complete a database operation.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFailed(ActionFailedEventArgs args)
        {
        }

        /// <summary>
        /// Invoked when this object throws an exception.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnError(ORMErrorEventArgs args)
        {
        }

        /// <summary>
        /// Invoked when this object has been removed from the database.
        /// </summary>
        protected virtual void OnRemoved()
        {
        }

        /// <summary>
        /// Invoked when this object has commited changes in the database (update statement).
        /// </summary>
        protected virtual void OnSaved()
        {
        }

        /// <summary>
        /// Invoked when this object fail to load from the database before the record did not exists.
        /// </summary>
        protected virtual void OnRecordNotFound()
        {
        }

        /// <summary>
        /// Invoked when this object is bind to the database, by either it has been created or
        /// it has been loaded.
        /// </summary>
        protected virtual void OnDataBinding()
        {
        }

        #endregion

        #region -- Event Fire Events --

        /// <summary>
        /// Sets the result of the dataobject.
        /// </summary>
        /// <param name="doresult"></param>
        protected void SetResult(ActionResult doresult)
        {
           result = doresult;
        }

        /// <summary>
        /// Fire the <see cref="BeforeCreated" /> event.
        /// </summary>
        protected void FireBeforeCreated()
        {
			if (!objectInfo.CancelEvents)
			{
				OnBeforeCreated();
				if (BeforeCreated != null)
				{
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
				    BeforeCreated(this, args);
				}
			}
        }

        /// <summary>
        /// Fire the <see cref="AfterCreated" /> event.
        /// </summary>
        protected void FireAfterCreated()
        {
			if (!objectInfo.CancelEvents)
			{
				OnAfterCreated();
				if (AfterCreated != null)
				{
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    AfterCreated(this, args);
				}
			}
        }

        /// <summary>
        /// Fire the <see cref="BeforeLoaded" /> event.
        /// </summary>
        protected void FireBeforeLoaded()
        {
			if (!objectInfo.CancelEvents)
			{
				OnBeforeLoaded();
                if (BeforeLoaded != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    BeforeLoaded(this, args);
                }
			}
        }

        internal void InternalFireBeforeLoaded()
        {
            FireBeforeLoaded();
        }


        /// <summary>
        /// Fire the <see cref="AfterLoaded" /> event.
        /// </summary>
        protected void FireAfterLoaded()
        {
			if (!objectInfo.CancelEvents)
			{
				OnAfterLoaded();
                if (AfterLoaded != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    AfterLoaded(this, args);
                }
			}
		}

		/// <summary>
		/// Fire the <see cref="AfterLoaded" /> event.
		/// </summary>
        internal void InternalFireAfterLoaded()
        {
            FireAfterLoaded();
        }

        /// <summary>
        /// Fire the BeforeStored event.
        /// </summary>
        protected void FireBeforeSaved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnBeforeSaved();
                
                if (BeforeSaved != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    BeforeSaved(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the BeforeStored event.
        /// </summary>
        protected void FireOnInit()
        {
            if (!objectInfo.CancelEvents)
            {
                OnInit();

                if (Init != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    Init(this, args);
                }
            }
        }

        /// <summary>
        /// Fire the <see cref="AfterStored" /> event.
        /// </summary>
        protected void FireAfterSaved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnAfterSaved();
                if (AfterSaved != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    AfterSaved(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="BeforeRemoved" /> event.
        /// </summary>
        protected void FireBeforeRemoved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnBeforeRemoved();
                if (BeforeRemoved != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    BeforeRemoved(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="AfterRemoved" /> event.
        /// </summary>
        protected void FireAfterRemoved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnAfterRemoved();
                if (AfterRemoved != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    AfterRemoved(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="DataBinding" /> event.
        /// </summary>
        protected void FireDataBinding()
        {
			if (!objectInfo.CancelEvents)
			{
				OnDataBinding();
                if (DataBinding != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    DataBinding(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="Created" /> event.
        /// </summary>
        protected void FireCreated()
        {
			if (!objectInfo.CancelEvents)
			{
				OnCreated();
                if (Created != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    Created(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="Saved" /> event.
        /// </summary>
        protected void FireSaved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnSaved();
                if (Saved != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    Saved(this, args);
                }
			}
        }
        
        /// <summary>
        /// Fire the <see cref="Removed" /> event.
        /// </summary>
        protected void FireRemoved()
        {
			if (!objectInfo.CancelEvents)
			{
				OnRemoved();
                if (Removed != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    Removed(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="RecordNotFound" /> event.
        /// </summary>
        protected void FireRecordNotFound()
        {
			if (!objectInfo.CancelEvents)
			{
				OnRecordNotFound();

                if (RecordNotFound != null)
                {
                    ORMEventArgs args = new ORMEventArgs(SourceObject);
                    RecordNotFound(this, args);
                }
			}
        }

        /// <summary>
        /// Fire the <see cref="Failed" /> event.
        /// </summary>
        internal protected void FireFailed(Action op, string reason)
        {
            ActionFailedEventArgs args = new ActionFailedEventArgs(op, reason);
            OnFailed(args);
            if (Failed != null) Failed(SourceObject, args);
        }

        /// <summary>
        /// Fire the <see cref="Error" /> event.
        /// </summary>
        internal protected void FireError(Action action, Exception e, string sql)
        {
            ORMErrorEventArgs args = new ORMErrorEventArgs(action, e, sql);
            OnError(args);
            if (Error != null) Error(SourceObject, args);
        }

        #endregion

        #region private methods

        private void CheckReadOnlyState(Action action)
        {
            if (objectInfo.IsReadOnly)
            {

                ReadOnlyException e = new ReadOnlyException(Utility.Resources["RES_CreateReadOnly"]);

                //Fire Events.
                FireFailed(action, Utility.Resources["FAILED_REASON_READONLY"]);
                FireError(action, e, String.Empty);
                throw e;
            }
        }

        private void CheckLockedState(Action action)
        {
            if (objectInfo.IsLocked)
            {

                LockORMException e = new LockORMException(Utility.Resources["RES_CreateLocked"]);

                // fire events.
                FireFailed(action, Utility.Resources["FAILED_REASON_LOCKED"]);
                FireError(action, e, String.Empty);
                throw e;
            }
        }

        private void CheckRecordVersion()
        {

            if (recordVersion == null) return;

            // check if this record has been update by another app.
            //int version = 2;

            if (!loadedForSave)
            {
                RecordVersion globalVersion = RecordVersion.GetCurrentVersion(this);

                if (globalVersion != null && (recordVersion.VersionNumber < globalVersion.VersionNumber))
                {
                    string errorMsg =
                        String.Format(
                            "Dirty record update. A newer record exists in the database. RecordIdentifier: {0}",
                            GetRecordIdentifier());

                    ORMException ex = new ORMException(errorMsg);
                    FireFailed(Action.Save, ex.Message);
                    FireError(Action.Save, ex, String.Empty);
                    throw ex;
                }
            }


            try
            {
                RecordVersion.Update(this, recordVersion);

                int version = (recordVersion != null) ? recordVersion.VersionNumber : 0;
                //Debug.WriteLine("new record version for update: " + version);

                if (ObjectInfo.MappedMembers.HasDigestRecordMember)
                {
                    string fulldigestValue = MapUtility.SetRecordDigest(ObjectInfo.MappedMembers, SourceObject, version);
                    //recordDigest = MapUtility.GetDigest(fulldigestValue);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Failed to set record digest. " + e.Message);
            }
        }

	    private void CheckDirtyObject(Action action)
        {
            if (objectInfo.IsDirty)
            {
                string errorMsg = String.Format("Data Object is dirty.  " +
                                                "The database has been modified by another instance since this object " +
                                                "has been loaded therefore the values of this object is outdated. " +
                                                "Type: {0} Object: {1}", this.SourceObject.GetType().FullName, GetRecordIdentifier());

                ORMException ex = new ORMException(errorMsg);
                FireFailed(action, ex.Message);
                FireError(action, ex, String.Empty);
                throw ex;
            }
        }
        #endregion

	    public bool IsLoaded
	    {
            get { return isLoaded; }
            set { isLoaded = value; }
	    }
	}
}
