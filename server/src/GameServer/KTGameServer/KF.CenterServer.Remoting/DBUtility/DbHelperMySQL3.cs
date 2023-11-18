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
using Tmsk.Tools;
using System.Threading;
using MySql.Data.MySqlClient;
using Tmsk.DbHelper;
using Server.Tools;
namespace Maticsoft.DBUtility
{
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) Maticsoft
    /// </summary>
    public abstract class DbHelperMySQL3
    {
        public static object Mutex = new object();

        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        public static string connectionString = PubConstant.ConnectionString;

        public const int LimitCount = 100;

        public const int InitCount = 1;

        /// <summary>
        /// 最大数据库连接数
        /// </summary>
        public static int MaxCount = 5;

        public static int ConnCount = 0;

        /// <summary>
        /// 设置客户端的连接字符集
        /// </summary>
        public static string CodePageNames = "utf8";

        /// <summary>
        /// 用于数据连接的代码页编号
        /// </summary>
        public static int CodePage = 65001;

        public const bool UsePool = false;

        private static Dictionary<string, MyDbConnectionPool> DBConnsDict = new Dictionary<string, MyDbConnectionPool>();

        public static Dictionary<string, string> ConnectionStringDict = new Dictionary<string, string>();

        /// <summary>
        /// 控制获取连接数
        /// </summary>
        public static Semaphore SemaphoreClientsNoPool = new Semaphore(50, 50);

        /// <summary>
        /// 从连接队列中取一个空闲数据库连接
        /// </summary>
        /// <returns></returns>
        public static MyDbConnection2 PopDBConnection(string dbKey)
        {
            MyDbConnection2 conn = null;

            if (UsePool)
            {
                int round = 0;
                MyDbConnectionPool pool = null;
                lock (Mutex)
                {
                    if (!DBConnsDict.TryGetValue(dbKey, out pool))
                    {
                        string connectionString = PubConstant.ConnectionString;
                        string dbName = PubConstant.GetDatabaseName(dbKey);
                        if (null == dbName)
                        {
                            return null;
                        }

                        int idx0 = connectionString.IndexOf("database=") + "database=".Length;
                        int idx1 = connectionString.IndexOf(';', idx0);
                        string datebaseName = connectionString.Substring(idx0, idx1 - idx0);

                        pool = new MyDbConnectionPool();
                        DBConnsDict[dbKey] = pool;
                        pool.DatabaseKey = dbKey;
                        pool.ConnectionString = connectionString.Replace(datebaseName, dbName);
                    }
                }

                if (null != pool)
                {
                    do 
                    {
                        //防止无法获取， 阻塞等待
                        if (pool.SemaphoreClients.WaitOne(1000))
                        {
                            lock (pool.DBConns)
                            {
                                conn = pool.DBConns.Dequeue();
                                break;
                            }
                        }
                        else
                        {
                            lock (Mutex)
                            {
                                if (pool.ConnCount < MaxCount)
                                {
                                    try
                                    {
                                        conn = new MyDbConnection2(pool.ConnectionString, CodePageNames);
                                        if (conn.Open())
                                        {
                                            conn.DatabaseKey = dbKey;
                                            pool.ConnCount++;
                                            break;
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        LogManager.WriteExceptionUseCache(ex.ToString());
                                    }
                                }
                            }
                        }
                    } while (round++ < 150);
                }
            }
            else
            {
                SemaphoreClientsNoPool.WaitOne();

                try
                {
                    string connectionString;
                    bool change = false;
                    lock (Mutex)
                    {
                        if (!ConnectionStringDict.TryGetValue(dbKey, out connectionString))
                        {
                            connectionString = PubConstant.ConnectionString;
                            string dbName = PubConstant.GetDatabaseName(dbKey);
                            int idx0 = connectionString.IndexOf("database=") + "database=".Length;
                            int idx1 = connectionString.IndexOf(';', idx0);
                            string datebaseName = connectionString.Substring(idx0, idx1 - idx0);
                            connectionString = connectionString.Replace(datebaseName, dbName);
                            ConnectionStringDict[dbKey] = connectionString;
                        }
                    }

                    conn = new MyDbConnection2(connectionString, CodePageNames);
                    if (!conn.Open())
                    {
                        conn = null;
                    }
                }
                catch (System.Exception ex)
                {
                    conn = null;
                }
                finally
                {
                    if (null == conn)
                    {
                        SemaphoreClientsNoPool.Release();
                    }
                }
            }

            return conn;
        }

        /// <summary>
        /// 将数据库连接添加到空闲数据库连接中
        /// </summary>
        /// <param name="conn"></param>
        public static void PushDBConnection(MyDbConnection2 conn)
        {
            if (UsePool)
            {
                if (null != conn)
                {
                    MyDbConnectionPool pool = null;
                    lock (Mutex)
                    {
                        if (!DBConnsDict.TryGetValue(conn.DatabaseKey, out pool))
                        {
                            pool = new MyDbConnectionPool();
                            DBConnsDict[conn.DatabaseKey] = pool;
                        }
                    }

                    if (null != pool)
                    {
                        lock (Mutex)
                        {
                            if (pool.ConnCount <= MaxCount && conn.IsConnected())
                            {
                                lock (pool.DBConns)
                                {
                                    pool.DBConns.Enqueue(conn);
                                }

                                pool.SemaphoreClients.Release();
                            }
                            else
                            {
                                pool.ConnCount--;
                                conn.Close();
                            }
                        }
                    }
                }
            }
            else
            {
                if (conn != null)
                {
                    SemaphoreClientsNoPool.Release();
                    conn.Close();
                }
            }
        }

        #region 公用方法

        /// <summary>
        /// 得到最大值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int GetMaxID(string dbKey, string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(dbKey, strsql);
            if (obj == null)
            {
                return 1;
            }
            else
            {
                return int.Parse(obj.ToString());
            }
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists(string dbKey, string strSql)
        {
            object obj = GetSingle(dbKey, strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 是否存在（基于MySqlParameter）
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists(string dbKey, string strSql, params MySqlParameter[] cmdParms)
        {
            object obj = GetSingle(dbKey, strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string dbKey, string SQLString)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteNonQuery(SQLString);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return -1;
        }

        public static int ExecuteSqlByTime(string dbKey, string SQLString, int Times)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteNonQuery(SQLString, Times);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return -1;
        }

        /// <summary>
        /// 执行MySql和Oracle滴混合事务
        /// </summary>
        /// <param name="list">SQL命令行列表</param>
        /// <param name="oracleCmdSqlList">Oracle命令行列表</param>
        /// <returns>执行结果 0-由于SQL造成事务失败 -1 由于Oracle造成事务失败 1-整体事务执行成功</returns>
        public static int ExecuteSqlTran(string dbKey, List<CommandInfo> list, List<CommandInfo> oracleCmdSqlList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteSqlTran(list, oracleCmdSqlList);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return 0;
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(string dbKey, List<String> SQLStringList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteSqlTran(SQLStringList);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return 0;
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string dbKey, string SQLString, string content)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteWithContent(SQLString, content);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return 0;
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static object ExecuteSqlGet(string dbKey, string SQLString, string content)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return conn.ExecuteSqlGet(SQLString, content);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return null;
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string dbKey, string strSQL, List<Tuple<string, byte[]>> imgList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.ExecuteSqlInsertImg(strSQL, imgList);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return -1;
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string dbKey, string SQLString)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.GetSingle(SQLString);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return null;
        }
        public static object GetSingle(string dbKey, string SQLString, int Times)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null != conn)
                {
                    return conn.GetSingle(SQLString, Times);
                }
            }
            finally
            {
                PushDBConnection(conn);
            }

            return null;
        }
        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// 因为外部要读取数据，不能立即回收连接，所以以MyDataReader封装一下关闭回收连接的操作
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MyDataReader ExecuteReader(string dbKey, string strSQL)
        {
            MyDbConnection2 conn = null;
            conn = PopDBConnection(dbKey);
            if (null != conn)
            {
                MySqlDataReader mySqlDataReader = conn.ExecuteReader(strSQL);
                MyDataReader myDataReader = new MyDataReader(conn, mySqlDataReader);
                return myDataReader;
            }

            return null;
        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string dbKey, string SQLString)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return null;
                }

                return conn.Query(SQLString);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }

        public static DataSet Query(string dbKey, string SQLString, int Times)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return null;
                }

                return conn.Query(SQLString, Times);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }



        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string dbKey, string SQLString, params MySqlParameter[] cmdParms)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return -1;
                }

                return conn.ExecuteSql(SQLString, cmdParms);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTran(string dbKey, Hashtable SQLStringList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return;
                }

                conn.ExecuteSqlTran(SQLStringList);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static int ExecuteSqlTran(string dbKey, System.Collections.Generic.List<CommandInfo> cmdList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return -1;
                }

                return conn.ExecuteSqlTran(cmdList);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity(string dbKey, System.Collections.Generic.List<CommandInfo> SQLStringList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return;
                }

                conn.ExecuteSqlTranWithIndentity(SQLStringList);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity(string dbKey, Hashtable SQLStringList)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return;
                }

                conn.ExecuteSqlTranWithIndentity(SQLStringList);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string dbKey, string SQLString, params MySqlParameter[] cmdParms)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return -1;
                }

                return conn.GetSingle(SQLString, 0, cmdParms);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string dbKey, string SQLString, params MySqlParameter[] cmdParms)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return null;
                }

                return conn.ExecuteReader(SQLString, cmdParms);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string dbKey, string SQLString, params MySqlParameter[] cmdParms)
        {
            MyDbConnection2 conn = null;
            try
            {
                conn = PopDBConnection(dbKey);
                if (null == conn)
                {
                    return null;
                }

                return conn.Query(SQLString, cmdParms);
            }
            finally
            {
                PushDBConnection(conn);
            }
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion
    }

}
