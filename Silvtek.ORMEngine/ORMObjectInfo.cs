using System;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    [Serializable]
    [SoapType("ORMObjectInfo")]
    public sealed class ORMObjectInfo : ISerializable
    {
       
        private bool isLocked;
        private bool isReadOnly;
        private bool isIsolated;
        private bool cancelEvent;
        private bool isDirty;
        private BindingStatus bindingStatus;
        private readonly ObjectSchemaInfo schema;
        private bool belongsTransaction;
        private bool isCopy;
        private string lastSql;

        [SoapIgnore]
        public bool IsLocked
        {
            get { return isLocked; }
        }

        [SoapIgnore]
        public bool IsReadOnly
        {
            get { return isReadOnly; }
        }

        /// <summary>
        /// Gets whether the current instance is isolated from the object pool. It always load from the database.
        /// </summary>
        [SoapIgnore]
        public bool IsIsolated
        {
            get { return isIsolated; }
        }

        [SoapIgnore]
        public bool IsDirty
        {
            get { return isDirty; }
        }

        /// <summary>
        /// Get whether the current instance is a copy. If is true, the current instace is a copy, if it is false
        /// the current instance was loaded directly from the database.
        /// </summary>
        [SoapElement("IsCopy")]
        public bool IsCopy
        {
            get { return isCopy; }
        }

        /// <summary>
        /// Gets the last sql command executed.
        /// </summary>
        [SoapIgnore]
        public string LastSql
        {
            get { return lastSql; }
        }

        [SoapIgnore]
        public bool CancelEvents
        {
            get { return cancelEvent; }
            set { cancelEvent = value; }
        }

        [SoapElement("BindStatus")]
        public BindingStatus BindingStatus
        {
            get { return bindingStatus; }
        }


        internal ObjectSchemaInfo Schema
        {
            get { return schema; }
        }

        internal MapMemberList MappedMembers
        {
            get { return schema.MappedMembers; }
        }

        internal bool BelongsTransaction
        {
            get { return belongsTransaction; }
            set { belongsTransaction = value; }
        }

        internal ORMObjectInfo(Type type)
        {
            schema = ObjectSchemaContainer.GetObjectSchema(type);
            isLocked = false;
            isReadOnly = false;
            isIsolated = schema.IsCacheDisabled;
            cancelEvent = false;
            isDirty = false;
            isCopy = false;
            bindingStatus = BindingStatus.Unbind;
        }

        public void Locked()
        {
            isLocked = true;
        }

        public void SetReadOnly()
        {
            isReadOnly = true;
        }

        public void Isolated()
        {
            isIsolated = true;
        }

        internal void MarkAsDirty()
        {
            isDirty = true;
        }

        internal void RemoveDirtyStatus()
        {
            isDirty = false;
        }

        

        #region -- Get Insert Statement --

        /// <summary>
        /// Returns a prepared sql statement for the insert.
        /// </summary>
        /// <param name="source">The source object from which the value will be read and insert</param>
        //internal string GetInsertStatement(object source)
        //{
        //    if (schema.MappedMembers.HasSequentialMember) LoadSequentialMembers(source);
        //    lastSql = Schema.SqlBuilder.GetInsertStatement(source);
        //    return lastSql;
        //}

        internal string GetSelectStatement(object source)
        {
            lastSql = Schema.SqlBuilder.GetSelectStatement(source);
            return lastSql;
        }

        internal string GetDeleteStatement(object source)
        {
            lastSql = Schema.SqlBuilder.GetDeleteStatement(source);
            return lastSql;
        }

        //internal string GetUpdateStatement(MemberFieldList fields, object source)
        //{           
        //    lastSql = Schema.SqlBuilder.GetUpdateStatement(fields, source);
        //    return lastSql;
        //}

        internal Command GetInsertCommand(Connection cnn, object source)
        {
            if (schema.MappedMembers.HasSequentialMember) LoadSequentialMembers(source);
            Command cmd = Schema.SqlBuilder.GetInsertCommand(cnn, source);
            lastSql = cmd.CommandText;
            return cmd;
        }

        internal Command GetUpdateCommand(Connection cnn, MemberFieldList fields, object source)
        {
            Command cmd = Schema.SqlBuilder.GetUpdateCommand(cnn, fields, source);
            if (cmd != null)
            {
                lastSql = cmd.CommandText;
            }

            return cmd;
        }

        /// <summary>
        /// Updates the source object with any sequential value returned by the sequence provider.
        /// </summary>
        private void LoadSequentialMembers(object source)
        {

            MapMember[] sequentials = schema.MappedMembers.GetAllSequentialMembers();

            for (int i = 0; i < sequentials.Length; i++)
            {
                object seqNum = sequentials[i].GetSequence(source);

                if (Convert.ToInt32(seqNum) == -1)
                {
                    string exMsg = Utility.GetResourceString("RES_NonSecuencialNumber", sequentials[i].MemberName);
                    throw new ORMException(exMsg);
                }

                if (sequentials[i].Properties.DataType == DataType.Int32)
                {
                    int int32Value = Convert.ToInt32(seqNum);
                    sequentials[i].SetMemberValue(source, int32Value);
                }
                else if (sequentials[i].Properties.DataType == DataType.Int64)
                {
                    long int64Value = Convert.ToInt64(seqNum);
                    sequentials[i].SetMemberValue(source, int64Value);
                }
                else
                {
                    string exMsg = Utility.GetResourceString("RES_SecuencialInt32OrInt64", sequentials[i].MemberName);
                    throw new ORMException(exMsg);
                }
            }
        }

        #endregion

        internal void SetBindStatus(BindingStatus status)
        {
            bindingStatus = status;
        }

        internal void RegisterAsCopy()
        {
            isCopy = true;
        }

        internal string CreateIdentifierString(object source)
        {
            StringBuilder sbuilder = new StringBuilder(400);

            sbuilder.Append(source.GetType().FullName + "->");

            MapMember[] pks = schema.MappedMembers.GetAllPrimaryKeyMembers();
            for (int i = 0; i < pks.Length; i++)
            {

                sbuilder.Append("[" + pks[i].ColumnName + ":");
                sbuilder.Append(Convert.ToString( pks[i].GetMemberValue(source)).ToLower() + "]");
                
            }

            return sbuilder.ToString();
        }



        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("isLocked", isLocked);
            info.AddValue("isReadOnly", isReadOnly);
            info.AddValue("isIsolated", isIsolated);
            info.AddValue("cancelEvent", cancelEvent);
            info.AddValue("Type", schema.Type);

        }

        public ORMObjectInfo(SerializationInfo info, StreamingContext context)
        {
            isLocked = info.GetBoolean("isLocked");
            isReadOnly = info.GetBoolean("isReadOnly");
            isIsolated = info.GetBoolean("isIsolated");
            cancelEvent = info.GetBoolean("cancelEvent");

            Type type = (Type)info.GetValue("Type", typeof(Type));
            schema = ObjectSchemaContainer.GetObjectSchema(type);
            isDirty = false;
            isCopy = false;
            bindingStatus = BindingStatus.Unbind;

        }

        #endregion
    }
}
