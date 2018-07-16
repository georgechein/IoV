using DBLibrary.Database;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using static DBLibrary.Database.DBHelper;

namespace InternetOfVehicles
{
    public class RawDataToDB
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RawDataToDB));
        public class Thread
        {
            //要傳遞的參數
            private string data;
            public Thread(string DATA)
            {
                data = DATA;
            }
            //要丟給執行緒執行的方法，無 return value 就是為了能讓ThreadStart來呼叫
            public void ThreadProc()
            {
                try
                {
                    string[] strs = data.Split(new string[] { "7E" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string d in strs)
                    {
                        dataToDB("7E" + d + "7E");
                    }
                }
                catch (Exception e)
                {
                    log.Fatal(e.ToString());
                }
            }
            private void dataToDB(string raw_data)
            {
                ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["IOV_ConnectionString"];
                DBHelper db = new DBHelper(connectionString.ProviderName, connectionString.ConnectionString);
                try
                {
                    string sql = @"INSERT INTO IOV_RAW_DATA(TYPE, ID, DATA, TIME) VALUES(@TYPE, @ID, @DATA, @TIME);";
                    Parameters[] param = new Parameters[4];
                    param[0] = getCommandParameter("@TYPE", raw_data.Substring(2, 4) ?? null);
                    param[1] = getCommandParameter("@ID", raw_data.Substring(10, 12) ?? null);
                    param[2] = getCommandParameter("@DATA", raw_data);
                    param[3] = getCommandParameter("@TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));


                    DbDataReader reader = db.ExecuteReader(CommandType.Text, sql, param);
                    db.OpenFactoryConnection();
                    reader.Read();
                    reader.Close();
                    db.CloseFactoryConnection();
                }
                catch (Exception e)
                {
                    log.Fatal(e.ToString());
                    if (db != null)
                    {
                        db.Rollback();
                        db.CloseFactoryConnection();
                    }
                }
            }
            private static Parameters getCommandParameter(string name, object value)
            {
                Parameters parameter = new Parameters();
                parameter.ParamName = name;
                parameter.ParamValue = value;
                return parameter;
            }
        }
    }
}