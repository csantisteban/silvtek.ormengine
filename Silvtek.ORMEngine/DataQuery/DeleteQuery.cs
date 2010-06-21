#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Diagnostics;
using Silvtek.ORMEngine.Caching;
using Silvtek.ORMEngine.DataQuery.Parsers;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.DataQuery
{
    /// <summary>
    /// Creates a delete query. Executing this class will delete records from database.
    /// </summary>
    public sealed class DeleteQuery : QueryBase 
    {
        private int recordsAffected;

        public DeleteQuery(Type type) : base(type)
        {
            recordsAffected = 0;
        }

        public int RecordsAffected
        {
            get { return recordsAffected; }
        }

// ReSharper disable UnusedPrivateMember

        private new void Execute(TimeSpan span, bool slideTime)
        {
        }

// ReSharper restore UnusedPrivateMember
       
        private new void Execute(DateTime expirationTime)
        {
        }

// ReSharper disable UnusedPrivateMember

        private new void Execute(bool realtime)
        {}

// ReSharper restore UnusedPrivateMember

        public new void Execute()
        {
            recordsAffected = 0;
            string sql = GetSql();

            Connection cnn = Types.MainType.GetConnection();
            cnn.Open();
            Command cmd = new Command(sql, cnn);

            try
            {
                recordsAffected = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Trace.WriteLine(Utility.GetResourceString("TRACE_EXECUTE_QUERY_EXCEPTION", sql, e.Message, e.StackTrace), Resources.TRACE_CATEGORY);
                throw;
            }
            finally
            {
                cnn.Close();
            }      
        }


        public override string GetSql()
        {
            DeleteQueryParser parser = new DeleteQueryParser(this);
            return parser.GetSql();       
        }

        internal override void Execute(bool realtime, TimeController controller)
        {
            Execute();
        }
    }
}
