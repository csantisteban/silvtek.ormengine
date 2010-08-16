#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;

namespace Silvtek.ORMEngine
{
    /// <summary>
    ///  Creates a ORM proxy from a object. The wrapped object must be a valid Data Object.
    /// </summary>
    public sealed class ORMObjectProxy
    {
        #region -- Event Declarations --

        public ORMObject OrmObject
        {
            get { return ormObject; }
        }

        public object BoundObject
        {
            get { return ormObject.SourceObject; }
        }

        /// <summary>
        /// Occurs before a record is created in the database.
        /// </summary>
        public event ORMEventHandler BeforeCreated;


        /// <summary>
        /// Occurs after a record is created in the database.
        /// </summary>
        public event ORMEventHandler AfterCreated;

        /// <summary>
        /// Occurs when [before saved].
        /// </summary>
        
        public event ORMEventHandler BeforeSaved;
        /// <summary>
        /// Occurs when [after saved].
        /// </summary>
        
        public event ORMEventHandler AfterSaved;
        /// <summary>
        /// Occurs when [before removed].
        /// </summary>
        
        public event ORMEventHandler BeforeRemoved;
        /// <summary>
        /// Occurs when [after removed].
        /// </summary>
        
        public event ORMEventHandler AfterRemoved;
        /// <summary>
        /// Occurs when [before loaded].
        /// </summary>

        public event ORMEventHandler BeforeLoaded;
        /// <summary>
        /// Occurs when [after loaded].
        /// </summary>
        public event ORMEventHandler AfterLoaded;

        /// <summary>
        /// Occurs when [created].
        /// </summary>
        public event ORMEventHandler Created;

        /// <summary>
        /// Occurs when [saved].
        /// </summary>
        public event ORMEventHandler Saved;


        /// <summary>
        /// Occurs when [removed].
        /// </summary>
        public event ORMEventHandler Removed;

        [NonSerialized]
        private bool synchronized;

        private readonly object obj;
        private readonly ORMObject ormObject;
        #endregion

        [NonSerialized]
        private bool cancelEvents;



        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <value>The object.</value>
        public object Object
        {
            get { return obj; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [cancel events].
        /// </summary>
        /// <value><c>true</c> if [cancel events]; otherwise, <c>false</c>.</value>
        public bool CancelEvents
        {
            get { return cancelEvents; }
            set { cancelEvents = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ORMObjectProxy"/> is synchronized.
        /// </summary>
        /// <value><c>true</c> if synchronized; otherwise, <c>false</c>.</value>
        public bool Synchronized
        {
            get { return synchronized; }
            set { synchronized = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMObjectProxy"/> class.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public ORMObjectProxy(object obj)
        {
            this.obj = obj;
            ormObject = ORM.Instanciate(obj);
        }

        ///<summary>
        /// Creates a record in the database for the object passed.
        ///</summary>
        public  ActionResult Create()
        {

            ormObject.ObjectInfo.CancelEvents = CancelEvents;
            ormObject.BeforeCreated += BeforeCreated;
            ormObject.AfterCreated += AfterCreated;
            ormObject.Created += Created;

            if (Synchronized)
            {
                lock (ormObject.SyncRoot)
                {
                    ormObject.Create();
                }
            }
            else
            {
                ormObject.Create();
            }



            return ormObject.Result;
        }

        ///<summary>
        /// Removes a record in the database for the object passed.
        ///</summary>
        public ActionResult Remove()
        {

            ormObject.ObjectInfo.CancelEvents = CancelEvents;
            ormObject.BeforeRemoved += BeforeRemoved;
            ormObject.AfterRemoved += AfterRemoved;
            ormObject.Removed += Removed;

            if (Synchronized)
            {
                lock (ormObject.SyncRoot)
                {
                    ormObject.Remove();
                }
            }
            else
            {
                ormObject.Remove();
            }

            return ormObject.Result;
        }

        ///<summary>
        /// Load a record in the database for the object passed The object passed <b>MUST</b> have their 
        /// primary key populated.
        ///</summary>
        public ActionResult Load()
        {


            ormObject.ObjectInfo.CancelEvents = CancelEvents;
            ormObject.BeforeLoaded += BeforeLoaded;
            ormObject.AfterLoaded += AfterLoaded;
            ormObject.Load();

            return ormObject.Result;

        }

        ///<summary>
        /// Saves (Updates) a record in the database for the object passed.
        ///</summary>
        public ActionResult Save()
        {
            ormObject.ObjectInfo.CancelEvents = CancelEvents;
            ormObject.BeforeSaved += BeforeSaved;
            ormObject.AfterSaved += AfterSaved;
            ormObject.Saved += Saved;


            bool load = (ormObject.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (Synchronized)
            {

                lock (ormObject.SyncRoot)
                {
                    ormObject.Save(load);
              //      Trace.WriteLineIf(ormObject.Result != ActionResult.Succeed, "failed to update " + obj.GetType().FullName + ". result: " + ormObject.Result);
                }
            }
            else
            {
                ormObject.Save(load);
            //    Trace.WriteLineIf(ormObject.Result != ActionResult.Succeed, "failed to update " + obj.GetType().FullName + ". result: " + ormObject.Result);

            }


            return ormObject.Result; 
        }

        public ORMObject GetORMObject()
        {
            return ormObject;
        }
    }
}
