#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;
using System.Diagnostics;

namespace Silvtek.ORMEngine.Data
{

    ///<summary>
    /// Represent a DataReader.
    ///</summary>
	public class DataReader : IDataReader
	{
		private readonly IDataReader reader;
		private readonly ProviderType type;

		/// <summary>
		/// Event fired on each row read.
		/// </summary>
        public event EventHandler Reading;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">Native reader</param>
        /// <param name="type">Provider type</param>
		internal DataReader(IDataReader reader, ProviderType type)
		{
			this.reader = reader;
			this.type = type;
		}

        ///<summary>
        /// Gets the number of columns in the current row.
        ///</summary>
        ///<value>
        /// When not positioned in a valid recordset, 0;
        /// otherwise the number of columns in the current record.
        /// default is -1.
        ///</value>
		public virtual int FieldCount
		{
			get
			{
				return reader.FieldCount;
			}
		}

        ///<summary>
        /// Gets the column located at the specified index.
        ///</summary>
        ///<param name="i">The index of the column to get.</param>
        ///<value>
        /// The column located at the specified index as an Object.
        ///</value>
		public virtual object this[int i]
		{
			get
			{
				return reader[i];
			}
		}

        ///<summary>
        /// Gets the column with the specified name.
        ///</summary>
        ///<param name="name">The name of the column to find.</param>
        ///<value>
        /// The column with the specified name as an Object.
        ///</value>
		public virtual object this[string name]
		{
			get
			{
				return reader[name];
			}
		}


		/// <summary>
		/// Gets a value indicating the depth of nesting for the current row.
		/// </summary>
		public virtual int Depth
		{
			get
			{
			  return reader.Depth;
			}
		}


		///<summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public virtual bool IsClosed
		{
			get
			{
				return reader.IsClosed;
			}
		}


		/// <summary>
		/// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		public virtual int RecordsAffected
		{
			get
			{
			  return reader.RecordsAffected;
			}
		}



		/// <summary>
		/// Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>true if the specified field is set to null, otherwise false.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual bool IsDBNull(int i)
		{
            bool isnull = false;
            
            try
            {
			  isnull = reader.IsDBNull(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return isnull;
		}


		/// <summary>
		/// Gets an <see cref="IDataReader" /> to be used when the field points to 
		/// more remote structured data.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>An IDataReader to be used when the field points to more remote structured data</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual IDataReader GetData(int i)
		{
            IDataReader r;
            try
            {
			  r = reader.GetData(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return r;

		}

        /// <summary>
        /// Gets the date and time data value of the spcified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The date and time data value of the specified field.</returns>
 		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual DateTime GetDateTime(int i)
		{
            DateTime time = DateTime.MinValue;
            try
            {
			   time = reader.GetDateTime(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return time;
		}


		/// <summary>
		/// Gets the fixed-position numeric value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The fixed-position numeric value of the specified field.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual decimal GetDecimal(int i)
		{
            decimal val = 0;

            try
            {
              val = reader.GetDecimal(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch (Exception e)
            {
              throw new DbDataException(e);
		    }

            return val;
        }


		/// <summary>
		/// Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The string value of the specified field.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual string GetString(int i)
		{
            string val = String.Empty;
            try
            {
              val = reader.GetString(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}

     
		/// <summary>
		/// Gets the double-precision floating point number of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The double-precision floating point number of the specified fiel</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual double GetDouble(int i)
		{
            double val = 0.0;
            try
            {
              val = reader.GetDouble(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
		/// <param name="i">The index of the field to find. </param>
        /// <returns>The single-precision floating point number of the specified field</returns>
 		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual float GetFloat(int i)
		{
            float val = 0F;
            try
            {
              val = reader.GetFloat(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}


		/// <summary>
		/// Gets the 64-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The 64-bit signed integer value of the specified field.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual long GetInt64(int i)
		{
            long val = 0;

            try
            {
              if (type == ProviderType.Oracle)
              {
                checked
                {
                	val = Convert.ToInt64(reader.GetValue(i));
                }
              }
              else
              {
                 val = reader.GetInt64(i);
              }
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
         }


		/// <summary>
		/// Gets the 32-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The 32-bit signed integer value of the specified field.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual int GetInt32(int i)
		{
            int val = 0;

            try
            {
              if (type == ProviderType.Oracle)
              {
                checked
                {
                	val = Convert.ToInt32(reader.GetValue(i));
                }
              }
              else
              {
                 val = reader.GetInt32(i);
              }
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}


		/// <summary>
		/// Gets the 16-bit signed integer value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The 16-bit signed integer value of the specified field</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual short GetInt16(int i)
		{
            short val = 0;

            try
            {
              if (type == ProviderType.Oracle)
              {
                checked
                {
                	val = Convert.ToInt16(reader.GetValue(i));
                }
              }
              else
              {
                 val = reader.GetInt16(i);
              }
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}


		/// <summary>
		/// Returns the guid value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns>The guid value of the specified field.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual Guid GetGuid(int i)
		{
           Guid val = Guid.Empty;
           try
           {
             val = reader.GetGuid(i);
           }
           catch(IndexOutOfRangeException)
           {
            	throw;
           }
           catch(Exception e)
           {
             throw new DbDataException(e);
           }

           return val;
		}


		/// <summary>
		/// Reads a stream of characters from the specified column offset into the buffer as an array, 
		/// starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldoffset">The index within the row from which to begin the read operation. </param>
		/// <param name="buffer">The buffer into which to read the stream of bytes</param>
		/// <param name="bufferoffset">The index for buffer to begin the read operation. </param>
		/// <param name="length">The number of bytes to read. </param>
		/// <returns>The actual number of characters read.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual long GetChars(int i,
                                     long fieldoffset,
                                     char[] buffer,
                                     int bufferoffset,
                                     int length)
		{
            long val = 0;
            try
            {
		 	  val = reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;

		}


		/// <summary>
		/// Gets the character value of the specified column.
		/// </summary>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <returns>The character value of the specified column.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual char GetChar(int i)
		{
            char c;
            try
            {
              c = reader.GetChar(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return c;
		}


		/// <summary>
		/// Reads a stream of bytes from the specified column offset into the buffer as an array, 
		/// starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <param name="fieldOffset">The index within the field from which to begin the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
		/// <param name="bufferoffset">The index for buffer to begin the read operation.</param>
		/// <param name="length">The number of bytes to read</param>
		/// <returns>The actual number of bytes read.</returns>
		/// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual long GetBytes(int i,
                                     long fieldOffset,
                                     Byte[] buffer,
                                     int bufferoffset,
                                     int length)
		{
            long val = 0;
            try
            {
			  val = reader.GetBytes(i, fieldOffset, buffer,  bufferoffset, length);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;
		}

		///<remarks>Gets the 8-bit unsigned integer value of the specified column</remarks>
		/// <param name="i">The zero-based column ordinalnd. </param>
	    /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
	    public virtual byte GetByte(int i)
		{
            byte b = 0;
            try
            {
			   b = reader.GetByte(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
                throw new DbDataException(e);
            }

            return b;
		}

		/// <summary>
		/// Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <returns>The value of the column.</returns>
	    /// <param name="i">The zero-based column ordinalnd. </param>
	    /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual bool GetBoolean(int i)
		{
            bool bol = false;
            try
            {
              bol = reader.GetBoolean(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return bol;
		}


		/// <summary>
		/// Return the index of the named field.
		/// </summary>
		/// <param name="name">The name of the field to find. </param>
		/// <returns>The index of the named field.</returns>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual int GetOrdinal(string name)
		{
            int ordinal = -1;
            try
            {
              ordinal = reader.GetOrdinal(name);
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return ordinal;
		}


		/// <summary>
		/// Gets all the attribute fields in the collection for the current record.
		/// </summary>
		/// <param name="values">An array of Object to copy the attribute fields into</param>
		/// <returns>The number of instances of Object in the array.</returns>
		public virtual int GetValues(object[] values)
		{
            int val = -1;
            try
            {
              val = reader.GetValues(values);
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return val;

		}

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
		/// <param name="i">The index of the field to find. </param>
        /// <returns>The Object which will contain the field value upon return.</returns>
		public virtual object GetValue(int i)
		{
  		   object obj;
           try
           {
             //Trace.Assert((reader != null), "DataReader is null");
             obj = reader.GetValue(i);
           }
           catch(IndexOutOfRangeException)
           {
             	throw;
           }
           catch(Exception e)
           {
             throw new DbDataException(e);
           }

		   //Type objType = obj.GetType();

           if (type == ProviderType.Oracle)
           {
           	 if (obj is Decimal)obj = Convert.ToInt32(obj);
           }
           return obj;
		}



		/// <summary>
		/// Gets the Type information corresponding to the 
		/// type of Object that would be returned from GetValue.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns></returns>
	    /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual Type GetFieldType(int i)
		{
           Type type;
           try
           {
			  type = reader.GetFieldType(i);
           }
           catch(IndexOutOfRangeException)
           {
            	throw;
           }
           catch(Exception e)
           {
             throw new DbDataException(e);
           }

           return type;
		}


		///<summary>Gets the data type information for the specified field.</summary>
	    /// <param name="i">The zero-based column ordinalnd. </param>
	    /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
		/// <exception cref="DbDataException">An error reading the database.</exception>
		public virtual string GetDataTypeName(int i)
		{
            string name = String.Empty;
            try
            {
			  name = reader.GetDataTypeName(i);
            }
            catch(IndexOutOfRangeException)
            {
            	throw;
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return name;
		}


		/// <summary>
		/// Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find. </param>
		/// <returns></returns>
		public virtual string GetName(int i)
		{
            string name = String.Empty;
            try
            {
			  name = reader.GetName(i);
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return name;
		}


		///<summary>Returns a DataTable that describes the column metadata of the IDataReader.</summary>
		///<returns>A DataTable that describes the column metadata</returns>
		public virtual DataTable GetSchemaTable()
		{
            DataTable table;

            try
            {
              table =  reader.GetSchemaTable();
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            return table;
		}


		///<summary>Advances the IDataReader to the next record.</summary>
		public virtual bool Read()
		{
            bool nextRecord = false;

            if(Reading != null)Reading(this, EventArgs.Empty);
            try
            {
              nextRecord = reader.Read();
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }

            
			return nextRecord;
		}


		///<summary>Advances the data reader to the next result, when reading the results of batch SQL statements.</summary>
		public virtual bool NextResult()
		{
			bool nextRecord = false;

			if(Reading != null)Reading(this, EventArgs.Empty);
            try
            {
              nextRecord = reader.NextResult();
            }
            catch(Exception e)
            {
              throw new DbDataException(e);
            }
            
			return nextRecord;
		}


		///<summary>Closes Reader</summary>
		public virtual void Close()
		{
			reader.Close();
		}

		

		///<summary>Closes all resource opened by this object.</summary>
		public virtual void Dispose()
		{
			reader.Dispose();
		}


	}
}

