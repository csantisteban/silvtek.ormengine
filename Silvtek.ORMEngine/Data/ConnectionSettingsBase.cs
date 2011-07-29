#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Silvtek.ORMEngine.Data
{
 
    /// <summary>
    /// Base class for connection setting implementation for the ORM Engine.
    /// </summary>
    public abstract class ConnectionSettingsBase
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public abstract string ConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets the connection name.
        /// </summary>
        public abstract string ConnectionName
        {
            get;
        }

        /// <summary>
        /// Gets the time out.
        /// </summary>
        public abstract int Timeout
        {
            get;
        }

        /// <summary>
        /// Gets the serialization formatter.
        /// </summary>
        /// <value>The serialization formatter.</value>
        public abstract IFormatter SerializationFormatter
        { 
            get;
        }

        /// <summary>
        /// Gets the connection provider assembly.
        /// </summary>
        public abstract string ConnectionProviderAssembly
        {
            get;
        }

        /// <summary>
        /// Gets the Connection provider full class name.
        /// </summary>
        public abstract string ConnectionProviderClass
        {
            get;
        }

        /// <summary>
        /// Gets the formatter provider assembly.
        /// </summary>
        /// <value>The formatter provider assembly.</value>
        public abstract string FormatterProviderAssembly
        {
            get;
        }

        /// <summary>
        /// Gets the formatter provider class.
        /// </summary>
        /// <value>The formatter provider class.</value>
        public abstract string FormatterProviderClass
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsDefault
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether [prepared statements].
        /// </summary>
        /// <value><c>true</c> if [prepared statements]; otherwise, <c>false</c>.</value>
        public abstract bool PreparedStatements
        { 
            get;
        }

        /// <summary>
        /// Gets the command timeout.
        /// </summary>
        /// <value>The command timeout.</value>
        public abstract int CommandTimeout
        { 
            get;
        }


        /// <summary>
        /// Gets the list of exceptions and its code or message values that if matched the ORM will retry 
        /// before throwing an database exception.
        /// </summary>
        /// <value>The retry exceptions.</value>
        public abstract RetryExceptions RetryExceptions
        {
            get;
        }

        /// <summary>
        /// Gets the connection provider.
        /// </summary>
        /// <returns></returns>
        public ConnectionProvider GetConnectionProvider()
        {

            ConnectionProvider provider = null;
            try
            {
                // Initializing Provider
                Assembly tmpProviderAssembly = Assembly.Load(ConnectionProviderAssembly);
                Type providerTypeClass = tmpProviderAssembly.GetType(ConnectionProviderClass);
                provider = (ConnectionProvider)Activator.CreateInstance(providerTypeClass);
            }
            catch (TypeInitializationException e)
            {
                string exMsg = Utility.GetResourceString("ConnectionInfo:ProviderInitializationException", ConnectionProviderClass, ConnectionProviderAssembly);
                throw new ProviderInitializationException(exMsg, ConnectionProviderClass, e);
            }

            return provider;
        
        }

        /// <summary>
        /// Gets the db type formatter.
        /// </summary>
        /// <returns></returns>
        public DbTypeFormatter GetDbTypeFormatter()
        {

            DbTypeFormatter dbtype = null;
            try
            {
                Assembly tmpTypeAssembly = Assembly.Load(FormatterProviderAssembly);
                Type dbTypeType = tmpTypeAssembly.GetType(FormatterProviderClass);
                dbtype = (DbTypeFormatter)Activator.CreateInstance(dbTypeType);
            }
            catch (TypeInitializationException e)
            {
                string exMsg = Utility.GetResourceString("ConnectionInfo:DbTypeInitializationException", FormatterProviderClass, FormatterProviderAssembly);
                throw new DbTypeInitializationException(exMsg, FormatterProviderClass, e);
            }

            return dbtype;
        }

    }
}
