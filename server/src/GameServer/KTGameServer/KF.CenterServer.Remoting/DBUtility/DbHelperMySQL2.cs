using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
//using MySql.Data.MySqlClient;
using MySQLDriverCS;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using MILogin;
namespace Maticsoft.DBUtility
{
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) Maticsoft
    /// </summary>
    public abstract class DbHelperMySQL2
    {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        public static string connectionString = PubConstant.ConnectionString;
        public static string connectionLogString = PubConstant.ConnectionLogString;
        public static bool loadConnectStr = false;
        public static string realConnStrGame = "";
        public static string realConnStrLog = "";
        public DbHelperMySQL2()
        {
        }

        public static  void LoadConnectStr()
        {
            //char[] filters = { ' ', '\r', '\n', '\t' };
            //connectionString = connectionString.Trim(filters);
            //connectionLogString = connectionLogString.Trim(filters);
            Dictionary<string, string> mapParams = new Dictionary<string,string>();
            string[] strParams = connectionLogString.Split(';');
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
            MySQLConnectionString connStrLog = new MySQLConnectionString(
                mapParams["host"],
                mapParams["database"],
                mapParams["user id"],
                mapParams["password"]
                );

            mapParams.Clear();
            strParams = connectionString.Split(';');
            foreach (var param in strParams)
            {
                string[] map = param.Split('=');
                if (map.Length != 2)
                    continue;
                map[0] = map[0].Trim();
                map[1] = map[1].Trim();
                mapParams[map[0]] = map[1];
            }

            MySQLConnectionString connStrGame = new MySQLConnectionString(
                mapParams["host"],
                mapParams["database"],
                mapParams["user id"],
                mapParams["password"]
                );
            realConnStrGame = connStrGame.AsString;
            realConnStrLog = connStrLog.AsString;
            loadConnectStr = true;
        }

        /// <summary>
        /// 执行查询语句，返回MySQLDataReader ( 注意：调用该方法后，一定要对MySQLDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySQLDataReader</returns>
        public static MySQLDataReader ExecuteReader(string strSQL,bool islog = false)
        {
            if (!loadConnectStr)
            {
                LoadConnectStr();
            }

            MySQLConnection connection = null;
            if (islog)
            {
                connection = new MySQLConnection(realConnStrLog);
            }
            else
            {
                connection = new MySQLConnection(realConnStrGame);
            }

            MySQLCommand cmd = new MySQLCommand(strSQL, connection);
            MySQLCommand cmdname = new MySQLCommand("SET NAMES 'latin1'", connection);
            try
            {
                connection.Open();
                int res = cmdname.ExecuteNonQuery();
                MySQLDataReader myReader = cmd.ExecuteReaderEx();
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                throw e;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (cmdname != null)
                {
                    cmdname.Dispose();
                }
                if (connection != null)
                {
                    connection.Dispose();
                    connection.Close();
                }
            }

        }
    }

}
