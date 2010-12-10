#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using Silvtek.ORMEngine.NativeSchema;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.Schema
{
    /// <summary>
    /// Represents the process of initizing an ORM object.
    /// </summary>
    /// <remarks>
    ///  by providing a abstract class we can create later different initializer
    /// </remarks>
    [Serializable]
    public abstract class ObjectSchemaInfo
    {
        private SqlBuilder sqlbuilder;
        private string connectionName;
        private DbTypeFormatter formatter;

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public abstract string TableName { get;}

        /// <summary>
        /// Gets the collection of mapped members;
        /// </summary>
        public abstract MapMemberList MappedMembers { get;}

        public abstract bool IsCacheDisabled { get;}

        /// <summary>
        /// Gets the name of the connection.
        /// </summary>
        public string ConnectionName 
        {
            get { return connectionName; }
            set { connectionName = value;}
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        public abstract Type Type { get;}

        /// <summary>
        /// Gets the formatter type.
        /// </summary>
        public DbTypeFormatter DbTypeFormatter 
        {
            get { return formatter; }
        }

        /// <summary>
        /// Gets whether the object read is valid or not.
        /// </summary>
        public abstract bool IsValid { get;}

        internal SqlBuilder SqlBuilder
        {
            get
            {
                if (sqlbuilder == null) sqlbuilder = new SqlBuilder(this);
                return sqlbuilder;
            }
        }

        internal abstract void Load();

        protected void SetConnectionName(string connectionName)
        {
            this.connectionName = connectionName;

            if (ConnectionManager.Current.Contains(connectionName))
            {
                ConnectionInfo connSettings = ConnectionManager.Current.GetConnectionSettings(connectionName);
                formatter = connSettings.DbTypeFormatter;
            }
            else
            {
                string exMsg = Utility.GetResourceString("RES_Connection_not_found", connectionName);
                throw new OrmMappingException(exMsg);
            }
        }

        /// <summary>
        /// Returns a connection for this instance.
        /// </summary>
        /// <returns></returns>
        public Connection GetConnection()
        {
            if (ConnectionName == null || ConnectionName.Length == 0)
            {
                string exMsg = Utility.GetResourceString("EXMSG_ObjectSchemaInfo_EMPTY_CONNECTION_NAME", Type.FullName);
                throw new InvalidOperationException(exMsg);
            }

            return ConnectionManager.Current.GetConnection(ConnectionName);
        }

        
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}", Type.FullName, ConnectionName);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;

            ObjectSchemaInfo info = (ObjectSchemaInfo)obj;
            return (info.GetHashCode() == GetHashCode());
        }

        public TableSchema GetNativeSchema()
        {
            ConnectionInfo cnnSettings = ConnectionManager.Current.GetConnectionSettings(ConnectionName);
            return cnnSettings.Provider.GetTableSchema(TableName, ConnectionName);
        }
    }
}
