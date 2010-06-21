#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace Silvtek.ORMEngine.Data
{
    public abstract class ConnectionSettings
    {
        public abstract string ConnectionString
        {
            get;
        }

        public abstract string ConnectionName
        {
            get;
        }

        public abstract int Timeout
        {
            get;
        }

        public abstract string ConnectionProviderAssembly
        {
            get;
        }

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
