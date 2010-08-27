#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Data;

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
        public virtual bool IsDBNull(int i)
        {
            return reader.IsDBNull(i);
        }


        /// <summary>
        /// Gets an <see cref="IDataReader" /> to be used when the field points to 
        /// more remote structured data.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>An IDataReader to be used when the field points to more remote structured data</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual IDataReader GetData(int i)
        {
           return reader.GetData(i);
        }

        /// <summary>
        /// Gets the date and time data value of the spcified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>

        public virtual DateTime GetDateTime(int i)
        {
            return reader.GetDateTime(i);
        }


        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual decimal GetDecimal(int i)
        {
            return reader.GetDecimal(i);
        }


        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The string value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual string GetString(int i)
        {
            return reader.GetString(i);
        }


        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The double-precision floating point number of the specified fiel</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual double GetDouble(int i)
        {
            return reader.GetDouble(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The single-precision floating point number of the specified field</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual float GetFloat(int i)
        {
            return reader.GetFloat(i);
        }


        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual long GetInt64(int i)
        {
            long val = 0;


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


            return val;
        }


        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual int GetInt32(int i)
        {
            int val = 0;


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


            return val;
        }


        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The 16-bit signed integer value of the specified field</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual short GetInt16(int i)
        {
            short val = 0;


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


            return val;
        }


        /// <summary>
        /// Returns the guid value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The guid value of the specified field.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual Guid GetGuid(int i)
        {
            return reader.GetGuid(i);

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
        public virtual long GetChars(int i,
                                     long fieldoffset,
                                     char[] buffer,
                                     int bufferoffset,
                                     int length)
        {

           return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

        }


        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <returns>The character value of the specified column.</returns>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual char GetChar(int i)
        {
            char c;

            c = reader.GetChar(i);


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
        public virtual long GetBytes(int i,
                                     long fieldOffset,
                                     Byte[] buffer,
                                     int bufferoffset,
                                     int length)
        {
            return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        ///<remarks>Gets the 8-bit unsigned integer value of the specified column</remarks>
        /// <param name="i">The zero-based column ordinalnd. </param>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual byte GetByte(int i)
        {
           return reader.GetByte(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>The value of the column.</returns>
        /// <param name="i">The zero-based column ordinalnd. </param>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual bool GetBoolean(int i)
        {
            return reader.GetBoolean(i);
        }


        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name">The name of the field to find. </param>
        /// <returns>The index of the named field.</returns>
        public virtual int GetOrdinal(string name)
        {
            return reader.GetOrdinal(name);
        }


        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <param name="values">An array of Object to copy the attribute fields into</param>
        /// <returns>The number of instances of Object in the array.</returns>
        public virtual int GetValues(object[] values)
        {
           return reader.GetValues(values);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns>The Object which will contain the field value upon return.</returns>
        public virtual object GetValue(int i)
        {
            //Trace.Assert((reader != null), "DataReader is null");
            object obj = reader.GetValue(i);


            //Type objType = obj.GetType();

            if (type == ProviderType.Oracle)
            {
                if (obj is Decimal) obj = Convert.ToInt32(obj);
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
        public virtual Type GetFieldType(int i)
        {
            return  reader.GetFieldType(i);
        }


        ///<summary>Gets the data type information for the specified field.</summary>
        /// <param name="i">The zero-based column ordinalnd. </param>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount" />.</exception>
        public virtual string GetDataTypeName(int i)
        {
           return reader.GetDataTypeName(i);
        }


        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i">The index of the field to find. </param>
        /// <returns></returns>
        public virtual string GetName(int i)
        {
            return reader.GetName(i);
        }


        ///<summary>Returns a DataTable that describes the column metadata of the IDataReader.</summary>
        ///<returns>A DataTable that describes the column metadata</returns>
        public virtual DataTable GetSchemaTable()
        {
            return reader.GetSchemaTable();
        }


        ///<summary>Advances the IDataReader to the next record.</summary>
        public virtual bool Read()
        {
            if (Reading != null) Reading(this, EventArgs.Empty);

            return reader.Read();
        }


        ///<summary>Advances the data reader to the next result, when reading the results of batch SQL statements.</summary>
        public virtual bool NextResult()
        {
            if (Reading != null) Reading(this, EventArgs.Empty);

            return reader.NextResult();
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

