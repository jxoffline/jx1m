using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySQLDriverCS;
using Server.Tools;
using Tmsk.Tools;

namespace Tmsk.DbHelper
{
    public class MyDbConnection1
    {
        MySQLConnection DbConn = null;
        MySQLConnectionString ConnStr = null;

        public string DatabaseKey = null;
        private string DatabaseName = null;

        public MyDbConnection1(string connStr, string dbNames)
        {
            bool success = false;
            MySQLConnection dbConn = null;

            try
            {
                Dictionary<string, string> mapParams = new Dictionary<string, string>();
                string[] strParams = connStr.Split(';');
                foreach (var param in strParams)
                {
                    string[] map = param.Split('=');
                    if (map.Length != 2)
                        continue;
                    map[0] = map[0].Trim();
                    map[1] = map[1].Trim();
                    mapParams[map[0]] = map[1];
                }
                //Hashtable hashLog = (Hashtable)MUJson.jsonDecode("{" + connectionLogString + "}");
                //Hashtable hashGame = (Hashtable)MUJson.jsonDecode("{" + connectionString + "}");
                ConnStr = new MySQLConnectionString(
                    mapParams["host"],
                    mapParams["database"],
                    mapParams["user id"],
                    mapParams["password"]
                    );

                dbConn = new MySQLConnection(ConnStr.AsString);
                dbConn.Open(); //打开数据库连接

                MySQLCommand cmd = null;

                //执行链接查询的字符集
                if (!string.IsNullOrEmpty(dbNames))
                {
                    cmd = new MySQLCommand(string.Format("SET names '{0}'", dbNames), dbConn);
                    cmd.ExecuteNonQuery();
                }

                DatabaseName = DbConn.Database;
                success = true;
                DbConn = dbConn;
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            if (!success && null != dbConn)
            {
                try
                {
                    dbConn.Close();
                }
                catch { }
            }
        }

        public bool IsConnected()
        {
            if (null != DbConn)
            {
                if (DbConn.State != ConnectionState.Closed && 0 == ((int)DbConn.State & (int)ConnectionState.Broken))
                {
                    return true;
                }
            }

            return false;
        }

        public void Close()
        {
            if (null != DbConn)
            {
                try
                {
                    DbConn.Close();
                }
                catch { }
            }
        }

        public void UseDatabase(string databaseKey, string databaseName)
        {
            MySQLCommand cmd = null;
            if (databaseKey != DatabaseKey)
            {
                try
                {
                    cmd = new MySQLCommand(string.Format("use '{0}'", databaseName), DbConn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    DatabaseKey = databaseKey;
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            }
        }
    }

}
