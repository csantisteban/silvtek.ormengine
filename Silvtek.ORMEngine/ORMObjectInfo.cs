#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using Silvtek.ORMEngine.Data;
using Silvtek.ORMEngine.Schema;

namespace Silvtek.ORMEngine
{
    /// <summary>
    /// Holds information about ORMObject
    /// </summary>
    [Serializable]
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
      

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value><c>true</c> if this instance is locked; otherwise, <c>false</c>.</value>
        public bool IsLocked
        {
            get { return isLocked; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
        }

        /// <summary>
        /// Gets whether the current instance is isolated from the object pool. It always load from the database.
        /// </summary>
        public bool IsIsolated
        {
            get { return isIsolated; }
        }

        public bool IsDirty
        {
            get { return isDirty; }
        }

        /// <summary>
        /// Get whether the current instance is a copy. If is true, the current instace is a copy, if it is false
        /// the current instance was loaded directly from the database.
        /// </summary>
        public bool IsCopy
        {
            get { return isCopy; }
        }

        /// <summary>
        /// Gets the last sql command executed.
        /// </summary>
        public string LastSql
        {
            get { return lastSql; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [cancel events].
        /// </summary>
        /// <value><c>true</c> if [cancel events]; otherwise, <c>false</c>.</value>
        public bool CancelEvents
        {
            get { return cancelEvent; }
            set { cancelEvent = value; }
        }

        /// <summary>
        /// Gets the binding status.
        /// </summary>
        /// <value>The binding status.</value>
        public BindingStatus BindingStatus
        {
            get { return bindingStatus; }
        }


        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <value>The schema.</value>
        internal ObjectSchemaInfo Schema
        {
            get { return schema; }
        }

        /// <summary>
        /// Gets the mapped members.
        /// </summary>
        /// <value>The mapped members.</value>
        internal MapMemberList MappedMembers
        {
            get { return schema.MappedMembers; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [belongs transaction].
        /// </summary>
        /// <value><c>true</c> if [belongs transaction]; otherwise, <c>false</c>.</value>
        internal bool BelongsTransaction
        {
            get { return belongsTransaction; }
            set { belongsTransaction = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ORMObjectInfo"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
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

        /// <summary>
        /// Lockeds this instance.
        /// </summary>
        public void Locked()
        {
            isLocked = true;
        }

        /// <summary>
        /// Sets the read only.
        /// </summary>
        public void SetReadOnly()
        {
            isReadOnly = true;
        }

        /// <summary>
        /// Isolateds this instance. An isolated object will not listen or broadcast updates
        /// </summary>
        public void Isolated()
        {
            isIsolated = true;
        }

        /// <summary>
        /// Marks as dirty.
        /// </summary>
        internal void MarkAsDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// Removes the dirty status.
        /// </summary>
        internal void RemoveDirtyStatus()
        {
            isDirty = false;
        }

        #region -- Get Insert Statement --

        /// <summary>
        /// Returns a prepared sql statement for the insert.
        /// </summary>
        /// <param name="source">The source object from which the value will be read and insert</param>
        internal string GetInsertStatement(object source)
        {
            if (schema.MappedMembers.HasSequentialMember) LoadSequentialMembers(source);
            lastSql = Schema.SqlBuilder.GetInsertStatement(source);
            return lastSql;
        }

        /// <summary>
        /// Gets the select statement.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal string GetSelectStatement(object source)
        {
            lastSql = Schema.SqlBuilder.GetSelectStatement(source);
            return lastSql;
        }

        /// <summary>
        /// Gets the delete statement.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal string GetDeleteStatement(object source)
        {
            lastSql = Schema.SqlBuilder.GetDeleteStatement(source);
            return lastSql;
        }

        /// <summary>
        /// Gets the update statement.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal string GetUpdateStatement(MemberFieldList fields, object source)
        {
            lastSql = Schema.SqlBuilder.GetUpdateStatement(fields, source);
            return lastSql;
        }

        /// <summary>
        /// Gets the insert command.
        /// </summary>
        /// <param name="cnn">The CNN.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal Command GetInsertCommand(Connection cnn, object source)
        {
            if (schema.MappedMembers.HasSequentialMember) LoadSequentialMembers(source);

            if (cnn.Settings.PreparedStatement || this.MappedMembers.HasBinaryMember)
            {
                Command cmd = Schema.SqlBuilder.GetInsertCommand(cnn, source);
                lastSql = cmd.CommandText;
                return cmd;
            }
            else
            {
                Command cmd = new Command(cnn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = GetInsertStatement(source);

                return cmd;
            }
        }

        internal Command GetUpdateCommand(Connection cnn, MemberFieldList fields, object source)
        {
            if (cnn.Settings.PreparedStatement || this.MappedMembers.HasBinaryMember)
            {
                Command cmd = Schema.SqlBuilder.GetUpdateCommand(cnn, fields, source);
                if (cmd != null)
                {
                    lastSql = cmd.CommandText;
                }

                return cmd;
            }
            else
            {
                lastSql =  GetUpdateStatement(fields, source);

                if (lastSql == null || lastSql.Length == 0) return null;
                Command cmd = new Command(cnn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = lastSql;
                return cmd;
            }

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

        /// <summary>
        /// Sets the bind status.
        /// </summary>
        /// <param name="status">The status.</param>
        internal void SetBindStatus(BindingStatus status)
        {
            bindingStatus = status;
        }

        /// <summary>
        /// Registers as copy.
        /// </summary>
        internal void RegisterAsCopy()
        {
            isCopy = true;
        }

        /// <summary>
        /// Creates the identifier string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal string CreateIdentifierString(object source)
        {
            //StringBuilder sbuilder = new StringBuilder(1000);

            byte[] identifier;
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] objectName = Utility.ConvertToBytes(source.GetType().FullName);
                stream.Write(objectName, 0, objectName.Length);

                MapMember[] pks = schema.MappedMembers.GetAllPrimaryKeyMembers();

                for (int i = 0; i < pks.Length; i++)
                {
                    byte[] bytes = Utility.ConvertToBytes(pks[i].GetMemberValue(source));
                    stream.Write(bytes, 0, bytes.Length);
                }

                stream.Position = 0;
                identifier = stream.ToArray();
                stream.Close();
            }

            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] result = sha1.ComputeHash(identifier);
            string hex = Utility.BytesToHex(result);
            if (hex.Length < 2) return String.Empty;

            return hex;
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
