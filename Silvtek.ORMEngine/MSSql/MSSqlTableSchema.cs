#region Copyright
// Copyright (c) 2002-2014 Carlos Silva-Santisteban. All rights reserved.
// See LICENSE in the repository root.
#endregion

using System;
using System.Collections;
using Silvtek.ORMEngine.NativeSchema;
using Silvtek.ORMEngine.Data;

namespace Silvtek.ORMEngine.MSSql
{
	/// <summary>
	/// Summary description for MSSqlTableSchema.
	/// </summary>
	public class MSSqlTableSchema  : TableSchema
	{
		private int id;
		private string name;
		private DateTime createdDate;
		private MSSqlColumnSchema[] columns;

		public override int ID
		{
			get {return id;}
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override ColumnSchema[] Columns
		{
			get {return columns;}
		}

		public override DateTime CreateDate
		{
			get {return createdDate;}
		}



		private  MSSqlTableSchema()
		{
		    id = 0;
			name = String.Empty;
			createdDate = DateTime.Now;
			columns = null;
		}

		public override ColumnSchema GetColumn(int id)
		{
			for(int i = 0; i < columns.Length; i++)
			{
				if (columns[i].ID == id)return columns[i];
			}

			// returns an empty column.
			return new MSSqlColumnSchema();
		}

		public override ColumnSchema GetColumn(string columnName)
		{
			if (columnName == null)new MSSqlColumnSchema();
			
			columnName = columnName.ToLower();

			for(int i = 0; i < columns.Length; i++)
			{
				if (columns[i].Name.ToLower() == columnName)return columns[i];
			}

			// returns an empty column.
			return new MSSqlColumnSchema();
		}


		public static MSSqlTableSchema GetSchema(string tableName, string connectionName)
		{
			
			const string sql = "SELECT o.name as TableName, o.id as TableID, o.crdate, c.name as ColName, c.length as ColLen, c.isnullable as IsNull, "+
				   		       "c.colid as ColID, t.Name Type FROM SYSOBJECTS o "+
				               "INNER JOIN   SYSCOLUMNS c ON o.id = c.id	"+
						       "INNER JOIN SYSTYPES t on c.xtype = t.xtype "+
						       "WHERE  o.name = '{0}' "+
						       "ORDER BY C.colid";

			string sqlCommand = String.Format(sql, tableName);

			Connection cnn = null;
			Command cmd = null;
			DataReader reader = null;
			MSSqlTableSchema tableSchema = new MSSqlTableSchema();
			
			ArrayList list = new ArrayList();
			try
			{
				cnn = ConnectionManager.Current.GetConnection(connectionName);
				cnn.Open();
				cmd = new Command(sqlCommand, cnn);
				reader = cmd.ExecuteReader();


				while (reader.Read())
				{
					tableSchema.name = Convert.ToString(reader[0]);
					tableSchema.id = Convert.ToInt32(reader[1]);
					tableSchema.createdDate = Convert.ToDateTime(reader[2]);
					string columnName = Convert.ToString(reader[3]);
					int colID = Convert.ToInt32(reader[6]);
					int len = Convert.ToInt32(reader[4]);
					bool isNull = ( Convert.ToInt32(reader[5]) == 1 );
					string colType = Convert.ToString(reader[7]);

					MSSqlColumnSchema columnSchema = new MSSqlColumnSchema(tableSchema.id, colID, columnName, len, colType, isNull);
					list.Add(columnSchema);
				}

				
				tableSchema.columns = (MSSqlColumnSchema[])list.ToArray(typeof(MSSqlColumnSchema ));
				

			}
			catch(Exception)
			{
			   throw;
			}
			finally
			{
			   if (cnn != null)cnn.Close();
			   if (reader != null)reader.Close();
			   if (cmd != null)cmd.Dispose();
			}

			return tableSchema;
		}


	}
}
