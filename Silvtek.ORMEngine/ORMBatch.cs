using System;
using System.Collections;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine
{
    public delegate int ExecuteBatchCaller(string connectionName, ExecuteBatchMode mode);

    /// <summary>
    /// Creates a batch sql
    /// </summary>
    public class ORMBatch
    {
        private readonly ArrayList batch;
        private ArrayList inProgressBatch;
        private bool preLoadUnboundObjects;

        private string lastSql;
        private double timems;
        private bool transactional;


        /// <summary>
        /// Initializes a new instance of the <see cref="ORMBatch"/> class.
        /// </summary>
        public  ORMBatch()
        {
            batch = new ArrayList();
            lastSql = String.Empty;
            preLoadUnboundObjects = false;
            transactional = true;

        }

        private enum BatchAction
        {
            Create,
            Save,
            Remove
        }

        /// <summary>
        /// Gets or sets the last batch SQL.
        /// </summary>
        /// <value>The last batch SQL.</value>
        public string LastBatchSql
        {
            get { return lastSql; }
        }

        /// <summary>
        /// Gets the total milliseconds that took to run the last batch.
        /// </summary>
        /// <value>The total milliseconds.</value>
        public double TotalMilliseconds
        {
            get { return timems; }
        }

        public bool PreLoadUnboundObjects
        {
            get { return preLoadUnboundObjects; }
            set { preLoadUnboundObjects = value; }
        }

        public bool IsTransactional
        {
            get { return transactional; }
            set { transactional = value; }
        }


        /// <summary>
        /// Adds a object to be saved in the database using a batch
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void AddSave(object obj)
        {
            BatchItem item = new BatchItem();
            item.Object = obj;
            item.Action = BatchAction.Save;
            batch.Add(item);
        }

        /// <summary>
        /// Adds a object to be created (insert) in the database using a batch.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void AddCreate(object obj)
        {
            BatchItem item = new BatchItem();
            item.Object = obj;
            item.Action = BatchAction.Create;
            batch.Add(item);
        }

        /// <summary>
        /// Adds the remove.
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void AddRemove(object obj)
        {
            BatchItem item = new BatchItem();
            item.Object = obj;
            item.Action = BatchAction.Remove;
            batch.Add(item);
        }

        public void Clear()
        {
            batch.Clear();
        }

        public IAsyncResult BeginExecuteBatch(string connectionName, ExecuteBatchMode mode, AsyncCallback callback, object state)
        {
            ExecuteBatchCaller _delegate = new ExecuteBatchCaller(ExecuteBatch);
            return _delegate.BeginInvoke(connectionName,mode, callback, state);
        }

        public int EndExecuteBatch(IAsyncResult result)
        {
            if (result == null) return 0;
            AsyncResult asyncResult = (AsyncResult)result;
            ExecuteBatchCaller ormActionDelegate = (ExecuteBatchCaller)asyncResult.AsyncDelegate;
            return ormActionDelegate.EndInvoke(result);
        }

        private int ExecuteBatchSingleMode(string connectionName)
        {
            ConnectionInfo info = ConnectionManager.Current.GetConnectionSettings(connectionName);
            
            DateTime start = DateTime.Now;
            StringBuilder builder = new StringBuilder();
            inProgressBatch = new ArrayList(batch);

            //if(this.transactional)
            //{
            //    builder.Append(info.DbTypeFormatter.BeginTransactionCommand());
            //    builder.Append(";");
            //    builder.Append(Environment.NewLine);
            //}

            foreach (BatchItem item in inProgressBatch)
            {
                ORMObject tmp = item.Object as ORMObject;
                if (tmp == null)
                {
                    tmp = ORM.Instanciate(item.Object);
                    item.Object = tmp;
                }

                switch (item.Action)
                {
                    case BatchAction.Create:
                        builder.Append(tmp.GetCreateStatement());
                        builder.Append(Environment.NewLine);
                        break;
                    case BatchAction.Save:
                        if (tmp.ObjectInfo.BindingStatus == BindingStatus.Bound)
                        {
                            tmp.FieldList.RefreshObjectSource();
                            builder.Append(tmp.GetSaveStatement());
                            builder.Append(";");
                            builder.Append(Environment.NewLine);
                        }
                        else
                        {
                            if (preLoadUnboundObjects)
                            {
                                tmp.InternalLoadForSave();

                                if (tmp.Result != ActionResult.RecordNotFound)
                                {
                                    tmp.FieldList.RefreshObjectSource();
                                    builder.Append(tmp.GetSaveStatement());
                                    builder.Append(";");
                                    builder.Append(Environment.NewLine);
                                }
                            }
                            else
                            {
                                throw new ORMException("Can't create batch for Update if the object is not bounded (preloaded) or PreLoadUnboundObjects is set to true");
                            }
                        }
                        break;
                    case BatchAction.Remove:
                        builder.Append(tmp.GetRemoveStatement());
                        builder.Append(";");
                        builder.Append(Environment.NewLine);
                        break;
                    default:
                        throw new InvalidOperationException("BatchAction " + item.Action + " is not supported.");

                }



            }

            //if (this.transactional)
            //{
            //    builder.Append(info.DbTypeFormatter.CommitTransactionCommand());
            //    builder.Append(";");
            //    builder.Append(Environment.NewLine);
            //}

            Connection cnn = null;
            int result = 0;
            string sqltext = builder.ToString().Trim();
            if (sqltext == null || sqltext.Length == 0) return 0;

            try
            {
                cnn = ConnectionManager.Current.GetConnection(connectionName);
                cnn.Open();                

                if (this.IsTransactional)
                {
                    StartTransaction(cnn);
                }

                Command cmd = new Command(cnn);
                cmd.CommandText = sqltext;
                cmd.CommandType = CommandType.Text;
                result = cmd.ExecuteNonQuery();
                this.lastSql = cmd.CommandText;

                if (this.IsTransactional)
                {
                    CommitTransation(cnn);
                }
            }
            catch (Exception ex)
            {
                if (this.IsTransactional)
                {
                    RollbackTransaction(cnn);
                    RollbackObjects(ex);
                }
               
                FireFailed(ex);
                throw;
            }
            finally
            {
                if (cnn != null) cnn.Close();

            }

            TimeSpan ts = DateTime.Now - start;
            this.timems = ts.TotalMilliseconds;
            //InternalAfterOperationDelegate afterOp = new InternalAfterOperationDelegate(UpdateObjects);
            //afterOp.BeginInvoke(new AsyncCallback(UpdateObjectsCompleted), null);
            UpdateObjects();
            inProgressBatch.Clear();
            return result;            
        }

        private void StartTransaction(Connection cnn)
        {
            if (cnn.State == ConnectionState.Open)
            {
                Command cmd = new Command(cnn);
                cmd.CommandText = cnn.DbTypeFormatter.BeginTransactionCommand();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                this.lastSql = cmd.CommandText;
            } 
        }

        private void CommitTransation(Connection cnn)
        {
            if (cnn.State == ConnectionState.Open)
            {
                Command cmd = new Command(cnn);
                cmd.CommandText = cnn.DbTypeFormatter.CommitTransactionCommand();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                this.lastSql = cmd.CommandText;
            }    
        }

        private void RollbackTransaction(Connection cnn)
        {
            if (cnn.State == ConnectionState.Open)
            {
                Command cmd = new Command(cnn);
                cmd.CommandText = cnn.DbTypeFormatter.RollbackTranscationCommand();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                this.lastSql = cmd.CommandText;
            }
        }

        private int ExecuteBatchMultipleCallsMode(string connectionName)
        {
            DateTime start = DateTime.Now;
            inProgressBatch = new ArrayList(batch);
            int result = 0;

            Connection cnn = ConnectionManager.Current.GetConnection(connectionName);
            try
            {

                Command cmd = null;
                cnn.Open();
                if (this.IsTransactional)
                {
                    StartTransaction(cnn);
                }

                foreach (BatchItem item in inProgressBatch)
                {
                    ORMObject tmp = item.Object as ORMObject;
                    if (tmp == null)
                    {
                        tmp = ORM.Instanciate(item.Object);
                        item.Object = tmp;
                    }


                    switch (item.Action)
                    {
                        case BatchAction.Create:
                            cmd = tmp.ObjectInfo.GetInsertCommand(cnn, tmp.SourceObject);
                            break;
                        case BatchAction.Save:
                            if (tmp.ObjectInfo.BindingStatus == BindingStatus.Bound)
                            {
                                tmp.FieldList.RefreshObjectSource();
                                cmd = tmp.ObjectInfo.GetUpdateCommand(cnn, tmp.FieldList, tmp.SourceObject);
                            }
                            else
                            {
                                if (preLoadUnboundObjects)
                                {
                                    tmp.InternalLoadForSave();

                                    if (tmp.Result != ActionResult.RecordNotFound)
                                    {
                                        tmp.FieldList.RefreshObjectSource();
                                        cmd = tmp.ObjectInfo.GetUpdateCommand(cnn, tmp.FieldList, tmp.SourceObject);
                                    }
                                }
                                else
                                {
                                    throw new ORMException("Can't create batch for Update if the object is not bounded (preloaded) or PreLoadUnboundObjects is set to true");
                                }
                            }
                            break;
                        case BatchAction.Remove:
                            string removesql = tmp.GetRemoveStatement();
                            cmd = new Command(removesql, cnn);
                            cmd.CommandType = CommandType.Text;
                            break;
                        default:
                            throw new InvalidOperationException("BatchAction " + item.Action + " is not supported.");
                    }

                    if (cmd == null) return 0;


                    int total = cmd.ExecuteNonQuery();
                    cnn.UpdateExecutionDateTime();

                    result = result + total;
                   
                }




                if (this.IsTransactional)
                {
                    CommitTransation(cnn);
                }

                this.lastSql = cmd.CommandText;
            }
            catch (Exception ex)
            {
                if (this.IsTransactional)
                {
                    RollbackTransaction(cnn);
                    RollbackObjects(ex);
                }

                FireFailed(ex);
                throw;
            }
            finally
            {
                if (cnn != null) cnn.Close();

            }

            TimeSpan ts = DateTime.Now - start;
            this.timems = ts.TotalMilliseconds;
            UpdateObjects();
            inProgressBatch.Clear();
            return result;
        }

        public int ExecuteBatch(string connectionName, ExecuteBatchMode mode)
        {
            switch (mode)
            {
                case ExecuteBatchMode.SingleCall:
                    this.ExecuteBatchSingleMode(connectionName);
                    break;

                case ExecuteBatchMode.MultipleCalls:
                    this.ExecuteBatchMultipleCallsMode(connectionName);
                    break;
                default:
                    return 0;
            }

            return 0;

        }

        private void UpdateObjects()
        {
            foreach(BatchItem item in batch)
            {
                ORMObject tmp = item.Object as ORMObject;
                if (tmp == null) continue;
                switch (item.Action)
                {
                    case BatchAction.Create:
                        tmp.AfterCreateSucceed();
                        break;
                    case BatchAction.Save:
                        tmp.AfterSaveSucceed();
                        break;
                    case BatchAction.Remove:
                        tmp.AfterRemoveSucceed();
                        break;
                    default:
                        throw new InvalidOperationException("BatchAction " + item.Action + " is not supported.");
                }
            }
        }

        public void RollbackObjects(Exception e)
        {
            foreach (BatchItem item in batch)
            {
                ORMObject tmp = item.Object as ORMObject;
                if (tmp == null) continue;
                switch (item.Action)
                {
                    case BatchAction.Create:
                        tmp.FireFailed(Action.Create, e.Message);
                        break;
                    case BatchAction.Save:
                        tmp.InternalLoadForSave();
                        tmp.FireFailed(Action.Save, e.Message);
                        break;
                    case BatchAction.Remove:
                        tmp.Load();
                        tmp.FireFailed(Action.Remove, e.Message);
                        break;
                    default:
                        throw new InvalidOperationException("BatchAction " + item.Action + " is not supported.");
                }
            }
        }

        private void FireFailed(Exception e)
        {
            foreach (BatchItem item in batch)
            {
                ORMObject tmp = item.Object as ORMObject;
                if (tmp == null) continue;
                tmp.FireError(Action.Batch, e, String.Empty);   
            }
        }


        private class BatchItem
        {
            public object Object;
            public BatchAction Action;
        }
    }


}
