using System;
using System.Diagnostics;

namespace Silvtek.ORMEngine
{
    public sealed class ORMObjectProxy
    {
        #region -- Event Declarations --

        public ORMObject OrmObject
        {
            get { return ormObject; }
        }

        public event ORMEventHandler BeforeCreated;
        public event ORMEventHandler AfterCreated;
        public event ORMEventHandler BeforeSaved;
        public event ORMEventHandler AfterSaved;
        public event ORMEventHandler BeforeRemoved;
        public event ORMEventHandler AfterRemoved;
        public event ORMEventHandler BeforeLoaded;
        public event ORMEventHandler AfterLoaded;

        [NonSerialized]
        private bool synchronized;

        private readonly object obj;
        private ORMObject ormObject;
        #endregion

        [NonSerialized]
        private bool cancelEvents;



        public object Object
        {
            get { return obj; }
        }

        public bool CancelEvents
        {
            get { return cancelEvents; }
            set { cancelEvents = value; }
        }

        public bool Synchronized
        {
            get { return synchronized; }
            set { synchronized = value; }
        }

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


            bool load = (ormObject.ObjectInfo.BindingStatus != BindingStatus.Bound);
            if (Synchronized)
            {

                lock (ormObject.SyncRoot)
                {
                    ormObject.Save(load);
                    Trace.WriteLineIf(ormObject.Result != ActionResult.Succeed, "failed to update " + obj.GetType().FullName + ". result: " + ormObject.Result);
                }
            }
            else
            {
                ormObject.Save(load);
                Trace.WriteLineIf(ormObject.Result != ActionResult.Succeed, "failed to update " + obj.GetType().FullName + ". result: " + ormObject.Result);

            }


            return ormObject.Result; 
        }

        public ORMObject GetORMObject()
        {
            return ormObject;
        }
    }
}
