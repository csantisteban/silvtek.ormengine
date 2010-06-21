using System;
using System.Collections;
using System.Diagnostics;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    internal delegate void OperationNotifierDelegate(ORMObject obj, Action op);

	/// <summary>
	///  Represents a transaction.
	/// </summary>
	public class Transaction : IDisposable
	{

        private ArrayList dataObjects;
        private Hashtable backupDataObjects;
        private Hashtable dataObjectCommand;
        private bool commited;
	    private bool isRollingBack;
        private bool isRollBack;
		private bool disposed;


        //public delegate void TransactionRollBackEventHandler();

        //public event RollBackEventHandler RollBacked;

        public event EventHandler RollBacked;
        public event EventHandler Commited;
        


		public Transaction()
		{
            dataObjects = new ArrayList(32);
            backupDataObjects = new Hashtable(32);
            dataObjectCommand = new Hashtable(32);
		    disposed = false;
		    isRollBack = false;
			isRollingBack = false;
			commited = false;
		}

        ~Transaction()
        {
			Dispose(false);
        }

        ///<summary>
        /// Adds <paramref name="obj" /> to a transaction.
        ///</summary>
        ///<param name="obj">
        /// The <see cref="ORMObject" /> to add.
        ///</param>
        public void Add(ORMObject obj)
        {
           // set is not committed.
           commited = false;

           // set is not rollback.
           isRollBack = false;

           //Set dataobject as belonging to a transaction.
           obj.RegisterTransaction(true);

           //register events
//           obj.Failed += new DataObjectFailedEventHandler(OnFail);
//           obj.Error += new DataObjectErrorEventHandler(OnError);
           obj.RegisterOperationNotifierDelegate(new OperationNotifierDelegate(ReceiveOperationNotification));

           dataObjects.Add(obj);
           
           if (obj.ObjectInfo.BindingStatus == BindingStatus.Bound)
           {
				backupDataObjects.Add(obj.GetRecordIdentifier(), (ORMObject)obj.Clone());
           }
        }

        public void Commit()
        {
         
           foreach(ORMObject obj in dataObjects)
           {
              if (!dataObjectCommand.Contains(obj.GetRecordIdentifier()))continue;
              
              Action cmd = (Action)dataObjectCommand[obj.GetRecordIdentifier()];
              switch(cmd)
              {
                case Action.Create:
                  obj.InvokeCreated();
                  backupDataObjects[obj.GetRecordIdentifier()] = obj.FieldList.Copy();
                  break;

                case Action.Save:
                  obj.InvokeChanged();
                  backupDataObjects[obj.GetRecordIdentifier()] = obj.FieldList.Copy();
                  break;

                case Action.Remove:
                  obj.InvokeRemoved();
                  backupDataObjects.Remove(obj.GetRecordIdentifier());
                  break;

                case Action.Load:
                  //do nothing.
                  break;

                default:
                  // do nothing.
                  break;
               }

               dataObjectCommand.Clear();
           }

           if (Commited != null)Commited(this, EventArgs.Empty);

        }

        public void RollBack()
        {
           // do not rollback, when it is already rolling back.
           if (isRollingBack)return;
           
            // lock this instance from any changes.
            lock(this)
            {
                isRollingBack = true;
                foreach(ORMObject obj in dataObjects)
                {
                    Debug.WriteLine("Rolling Back: "+obj.GetRecordIdentifier());

                    if (!dataObjectCommand.Contains(obj.GetRecordIdentifier()))
                    {
                        continue;
                    }

                    Action cmd = (Action)dataObjectCommand[obj.GetRecordIdentifier()];

                    switch(cmd)
                    {
                        case Action.Create:
                        {
                            obj.Remove();
                            break;
                        }
                        case Action.Save:
                        {
                            Debug.WriteLine("Rolling Back Store For: "+obj.GetRecordIdentifier());
                            RollBackValues(obj);
                            obj.Save();
                            break;
                        }
                        case Action.Remove:
                        {
                            Debug.WriteLine("Rolling Back Remove For: "+obj.GetRecordIdentifier());
                            obj.Create();
                            break;
                        }
                        case Action.Load:
                        {
                            break; // do nothing. 
                        }

                        default:
                        {
                            break; // do nothing.
                        }
                    }

                    WriteRollBackTrace(obj, cmd);
                }
        
                isRollingBack = false;
                isRollBack = true;
                if (RollBacked != null)RollBacked(this, EventArgs.Empty);
            }
        }

        [Conditional("TRACE")]
        private void WriteRollBackTrace(ORMObject obj, Action operation)
        {
            string text = Utility.GetResourceString("TRANSACTION:ROLLBACK_MESSAGE", operation, obj.GetRecordIdentifier());
            Trace.WriteLine(text, Resources.TRACE_CATEGORY);
        }

        public void ReceiveOperationNotification(ORMObject obj, Action op)
        {
           if (isRollingBack)return;
//           Debug.WriteLine("Operation: "+op+" for "+obj.GetRecordIdentifier()+" saved ", "Transaction");

           dataObjectCommand[obj.GetRecordIdentifier()] = op;
        }

//        private void OnFail(object sender, DataObjectFailedEventArgs evargs)
//        {
//            string failMsg = String.Format(Resources.Manager["TRACE_TransFailed"], sender.GetType(), evargs.Operation, evargs.Reason);
//            Trace.WriteLine(failMsg, GetType().FullName);   
//            RollBack();
//            if (RollBacked != null)RollBacked(this, EventArgs.Empty);
//        }
//
//        private void OnError(object sender, DataObjectExceptionEventArgs evargs)
//        {
//            string errorMsg = String.Format(Resources.Manager["TRACE_TransObjectError"], sender.GetType(), evargs.Exception);
//            Trace.WriteLine(errorMsg, GetType().FullName);
//            RollBack();
//            if (RollBacked != null)RollBacked(this, EventArgs.Empty);
//        }


        public void RollBackValues(ORMObject obj)
        {

          ORMObject bku = (ORMObject)backupDataObjects[obj.GetRecordIdentifier()];
         // obj.FieldList = (DataObjectFieldList)bku.FieldList.Clone();

          foreach (MapMember col in obj.ObjectInfo.MappedMembers)
          {
	
			 if (col.Properties.IsPrimaryKey)continue;

             // Check if the field value has been changed.
             foreach (MemberField field in obj.FieldList.Values)
             {
               // Find the field by name.
               if (field.Name == col.MemberName)
               {
                 col.SetDataSourceValue(obj, bku.FieldList.Get(col.MemberName).ActualValue);
                 //obj.FieldList.Get(col.Name).Changed = true;
                 break;
               }

             }
           }

		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);	
		}

		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if(disposing)
				{
					Debug.WriteLine("is committed: "+commited, "[Transaction]"); 
					if (!commited && !isRollBack)RollBack();
				}          
			}

			disposed = true;         
		}

		#endregion
	}
}
