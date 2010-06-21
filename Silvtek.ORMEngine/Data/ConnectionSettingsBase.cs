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

        public abstract IFormatter IFormatter
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

        public abstract string FormatterProviderAssembly
        {
            get;
        }

        public abstract string FormatterProviderClass
        {
            get;
        }

        public abstract bool IsDefault
        {
            get;
        }

        public abstract bool PreparedStatements
        { 
            get;
        }

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
