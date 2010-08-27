#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion


/*
File:  Command.cs
Version :  1.0

Author: Carlos Silva-Santisteban.

Date: 1/20/2003

Revision:


To do:

*/


namespace Silvtek.ORMEngine.Data
{
	using System;
	using System.Data;

    ///<summary>
    /// Represents an SQL statement that is executed
    /// while connected to a data source
    ///</summary>
	public class Command :  IDbCommand
	{
        
		// Hold a command object.
		private IDbCommand cmd;

        // Hold the driver provider (i.e Sql, ODBC, etc);
		private ProviderType type;

        // Hold the database type (i.e MySQL, Oracle, MSSql)
		private DbTypeFormatter dbtype;

        // Hold the connection properties.
        private ConnectionInfo settings;

        // Hold the time taken for the last execution.
        private TimeSpan executionTime;
        
        private Connection currentConnection;

          
		/// <summary>
		/// Constructor.
		/// </summary>
		public Command()
		{
			
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cnn">The <see cref="Connection" /> which this instance will be bound.</param>
	    public Command(Connection cnn) : this(String.Empty, cnn)
	    {
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cmdText">Sql Command</param>
		/// <param name="cnn">The <see cref="Connection" /> which this instance will be bound.</param>
        public Command(string cmdText, Connection cnn)
		{

		    cmd = cnn.CreateCommand();


		    if (cmdText != null && cmdText.Length > 0)
		    {
		        CommandText = cmdText;
		    }

		    type = cnn.ProviderType;
		    dbtype = cnn.DbTypeFormatter;
		    settings = cnn.Settings;
		    executionTime = TimeSpan.Zero;
		    currentConnection = cnn;
		    this.CommandTimeout = cnn.CommandTimeout;

		}

        ///<summary>
        /// Destructor
        ///</summary>
        ///<remarks>
        /// Invoke <see cref="Dispose" /> method.
        ///</remarks>
        ~Command()
        {
           Dispose();
        }

        ///<summary>
        /// Gets the duration of the last execution.
        ///</summary>
        ///<value>
        /// The duration of the last execution.
        ///</value>
        public TimeSpan LastExecutionDuration
        {
          get  {return executionTime;}
        }



        ///<summary>
        /// Gets a <see cref="IDbConnection" />.
        ///</summary>
		public virtual IDbConnection Connection
		{
			get
			{
				return cmd.Connection;
			}
			set
			{
				 Connection cnn = (Connection)value;
				 cmd.Connection = cnn;
			     type = cnn.ProviderType;
		         dbtype = cnn.DbTypeFormatter;
                 this.settings = cnn.Settings;
			}
		}


		/// <summary>
		/// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
		/// </summary>
		public virtual IDbTransaction Transaction
		{
			get
			{
			   return cmd.Transaction;
			}
			set
			{
			  cmd.Transaction = value;
			}
		}

        /// <summary>
        /// Gets or sets the text command to run against the data source
        /// </summary>
		public virtual string CommandText
		{
			get
			{
			  return cmd.CommandText;
			}
			set
			{
			  cmd.CommandText = value;
			}
		}

		/// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// </summary>
		public virtual int CommandTimeout
		{
			get
			{
				return cmd.CommandTimeout;
			}
			set
			{
				cmd.CommandTimeout = value;
			}
		}

		/// <summary>
		/// Indicates or specifies how the <see cref="CommandText" /> CommandText property is interpreted
		/// </summary>
		public virtual CommandType CommandType
		{
			get
			{
				return cmd.CommandType;
			}
			set
			{
			   cmd.CommandType = value;
			}
		}
		  
		/// <summary>
		/// Gets the parameter collection.
		/// </summary>
		public virtual IDataParameterCollection Parameters
		{
			get
			{
				return cmd.Parameters;
			}
		}

		/// <summary>
		/// Gets or sets how command results are applied to the DataRow 
		/// when used by the Update method of a DbDataAdapter.
		/// </summary>
		public virtual UpdateRowSource UpdatedRowSource
		{
			get
			{
				return cmd.UpdatedRowSource;
			}
			set
			{
				cmd.UpdatedRowSource = value;
			}
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored..
		/// </summary>
		public virtual object ExecuteScalar()
		{
		   object obj = null;
           DateTime start  = DateTime.Now;
		   
		   UpdateConnectionExecutionTime();

           DataReader reader = this.ExecuteReader();
          

		   if (reader.Read())
		   {
		      obj = reader.GetValue(0);
		   }

		   reader.Close();
           
		   this.Connection.Close();
           executionTime = DateTime.Now - start;
           settings.Stats.SetExecutionTime(executionTime);
		   if (Convert.IsDBNull(obj))obj = Convert.ToInt32(0);
		   return obj;
		}


		/// <summary>
		/// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored..
		/// </summary>
		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
          DateTime start = DateTime.Now;
		  
		  UpdateConnectionExecutionTime();

          DataReader reader = new DataReader(cmd.ExecuteReader(behavior), type);
         
          
          executionTime = DateTime.Now - start;
          settings.Stats.SetExecutionTime(executionTime);
		  return (IDataReader)reader;
		}

		/// <summary>
		/// Execute sql command and returns a <see cref="DataReader " /> instance.
		/// </summary>
		/// <param name="behavior">The command behavior.</param>
		/// <returns>An instance of the datareader</returns>
        public DataReader ExecuteReader(CommandBehavior behavior)
		{
		    DateTime start = DateTime.Now;


		    UpdateConnectionExecutionTime();

		    DataReader reader = new DataReader(cmd.ExecuteReader(behavior), type);


		    executionTime = DateTime.Now - start;
		    settings.Stats.SetExecutionTime(executionTime);
		    return reader;
		}

        /// <summary>
		/// Execute sql command and returns a <see cref="DataReader " /> instance.
		/// </summary>
		/// <returns>An instance of the datareader</returns>
		IDataReader IDbCommand.ExecuteReader()
		{
            DateTime start = DateTime.Now;
			DataReader reader;
			
			UpdateConnectionExecutionTime();
          
               reader = new DataReader(cmd.ExecuteReader(), type);
          
            
            executionTime = DateTime.Now - start;
            settings.Stats.SetExecutionTime(executionTime);
			return (IDataReader)reader;
		}

	   /// <summary>
	   /// Executes the <see cref="CommandText" /> against the <see cref="Connection" /> and builds an <see cref="DataReader" />.
	   /// </summary>
	   /// <returns>An DataReader object.</returns>
	   public DataReader ExecuteReader()
	   {
            DateTime start = DateTime.Now;
			DataReader reader;
			
			UpdateConnectionExecutionTime();
           
              reader = new DataReader(cmd.ExecuteReader(), type);
          
            
            executionTime = DateTime.Now - start;
            settings.Stats.SetExecutionTime(executionTime);
			return reader;
	   }

		/// <summary>
		/// Executes an SQL statement against the Connection object of
		///  a .NET Framework data provider, and returns the number of rows affected
		/// </summary>
		/// <returns>The number of rows affected.</returns>
		public virtual int ExecuteNonQuery()
		{
            DateTime start = DateTime.Now;
            
            UpdateConnectionExecutionTime();
            
              int rv = cmd.ExecuteNonQuery();
           

            
            executionTime = DateTime.Now - start;
            settings.Stats.SetExecutionTime(executionTime);
			return rv;
		}

		/// <summary>
		/// Creates a new instance of an <see cref="IDbDataParameter" /> object.
		/// </summary>
		/// <returns>An <b>IDbDataParameter</b> object.</returns>
		public virtual IDbDataParameter CreateParameter()
		{
			return cmd.CreateParameter();
		}

		/// <summary>
		/// Attempts to cancels the execution of an IDbCommand.
		/// </summary>
		public virtual void Cancel()
		{
			cmd.Cancel();
		}

		/// <summary>
		/// Creates a prepared (or compiled) version of the command on the data source.
		/// </summary>
		public virtual void Prepare()
		{
			cmd.Prepare();
		}

		/// <summary>
		/// Dispose resources.
		/// </summary>
		public virtual void Dispose()
		{
			cmd.Dispose();
		}

		/// <summary>
		/// Returns the encapsuled <see cref="IDbCommand" />.
		/// </summary>
		protected IDbCommand IDbCommand
		{
			set { cmd = value; 	}
		}

		private void UpdateConnectionExecutionTime()
		{
		    currentConnection.UpdateExecutionDateTime();
		}

	}
}

