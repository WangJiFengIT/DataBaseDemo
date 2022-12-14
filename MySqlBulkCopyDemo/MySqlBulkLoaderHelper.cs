using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySqlBulkCopyDemo
{
    public class MySqlBulkLoaderHelper
    {
        const string ConnectionString = "server=192.168.10.38;port=3306;user=root;password=asdfasdf;database=test;SslMode = none;AllowLoadLocalInfile=true";

        public static int BulkInsert<T>(List<T> entities, string tableName)
        {
            DataTable dt = entities.ToDataTable();
            using (MySqlConnection conn = new MySqlConnection())
            {
                conn.ConnectionString = ConnectionString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                if (tableName.IsNullOrEmpty())
                {
                    var tableAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
                    if (tableAttribute != null)
                        tableName = ((TableAttribute)tableAttribute).Name;
                    else
                        tableName = typeof(T).Name;
                }

                int insertCount = 0;
                string tmpPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString() + "_" + Guid.NewGuid().ToString() + ".tmp");
                string csv = dt.ToCsvStr();
                File.WriteAllText(tmpPath, csv, Encoding.UTF8);

                using (MySqlTransaction tran = conn.BeginTransaction())
                {
                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                    {
                        FieldTerminator = ",",
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",
                        FileName = tmpPath,
                        Local = true,
                        NumberOfLinesToSkip = 0,
                        TableName = tableName,
                        CharacterSet = "utf8"
                    };
                    try
                    {
                        bulk.Columns.AddRange(dt.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList());
                        insertCount = bulk.Load();
                        tran.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        if (tran != null)
                            tran.Rollback();

                        throw ex;
                    }
                }
                File.Delete(tmpPath);
                return insertCount;
            }
        }
    }
}
