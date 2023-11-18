using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Maticsoft.DBUtility;
using MySql.Data.MySqlClient;
using MySQLDriverCS;
using Server.Tools;
using Tmsk.Tools;
using System.Threading;

namespace Tmsk.DbHelper
{
    public class MyDbConnectionPool
    {
        public int ConnCount;
        public string DatabaseKey;
        public string ConnectionString;

        /// <summary>
        /// 控制获取连接数
        /// </summary>
        public Semaphore SemaphoreClients = new Semaphore(0, 100);

        /// <summary>
        /// 全局的数据库连接对象队列(长连接)
        /// </summary>
        public Queue<MyDbConnection2> DBConns = new Queue<MyDbConnection2>();
    }

    public class MyDataReader
    {
        private MyDbConnection2 MyConnection;
        public MySqlDataReader DataReader;

        public MyDataReader(MyDbConnection2 conn, MySqlDataReader dataReader)
        {
            MyConnection = conn;
            DataReader = dataReader;
        }
        public MySqlDataReader GetDataReader()
        {
            return DataReader;
        }
        public object this[int i] { get { return DataReader[i]; } }
        public object this[string name] { get { return DataReader[name]; } }

        public bool Read()
        {
            return DataReader.Read();
        }
        public bool IsDBNull(int i)
        {
            return DataReader.IsDBNull(i);
        }
        public int GetOrdinal(string name)
        {
            return DataReader.GetOrdinal(name);
        }
        public void Close()
        {
            try
            {
                DataReader.Close();
            }
            finally
            {
                DbHelperMySQL3.PushDBConnection(MyConnection);
            }
        }
    }

    public class MyDbConnection2
    {
        public MySqlConnection DbConn = null;

        public string DatabaseKey = null;
        private string DatabaseName = null;
        private string ConnStr;
        private string PageCodeNames;

        public MyDbConnection2(string connStr, string pageCodeNames)
        {
            ConnStr = connStr;
            PageCodeNames = pageCodeNames;
        }

        public bool Open()
        {
            bool success = false;
            MySqlConnection dbConn = null;

            try
            {
                dbConn = new MySqlConnection(ConnStr);
                dbConn.Open(); //打开数据库连接
                DatabaseName = dbConn.Database;
                success = true;
                DbConn = dbConn;

                //执行链接查询的字符集
                if (!string.IsNullOrEmpty(PageCodeNames))
                {
                    ExecuteNonQuery(string.Format("SET names '{0}'", PageCodeNames));
                }
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

            return success;
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
                    DbConn.Dispose();
                }
                catch { }
            }
        }

        public void UseDatabase(string databaseKey, string databaseName)
        {
            MySqlCommand cmd = null;
            if (databaseKey != DatabaseKey && !string.IsNullOrEmpty(databaseName))
            {
                DbConn.ChangeDatabase(databaseName);
                //if (ExecuteNonQuery(string.Format("use `{0}`", databaseName)) >= 0)
                {
                    DatabaseKey = databaseKey;
                }
            }
        }

        public int ExecuteNonQuery(string sql, int commandTimeout = 0)
        {
            int result = -1;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    if (commandTimeout > 0)
                    {
                        cmd.CommandTimeout = commandTimeout;
                    }

                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return result;
        }

        public int ExecuteWithContent(string sql, string content)
        {
            int result = 0;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", content);
                    //myParameter.Value = content;
                    cmd.Parameters.Add(myParameter);

                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return result;
        }

        public object GetSingle(string sql, int commandTimeout = 0, params MySqlParameter[] cmdParms)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    if (commandTimeout > 0)
                    {
                        cmd.CommandTimeout = commandTimeout;
                    }

                    if (cmdParms.Length > 0)
                    {
                        PrepareCommand(cmd, DbConn, null, sql, cmdParms);
                    }

                    object obj = cmd.ExecuteScalar();
                    if (cmdParms.Length > 0)
                    {
                        cmd.Parameters.Clear();
                    }

                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return null;
        }

        public object ExecuteSqlGet(string sql, string content)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", content);
                    //myParameter.Value = content;
                    cmd.Parameters.Add(myParameter);

                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return null;
        }

        public MySqlDataReader ExecuteReader(string sql, params MySqlParameter[] cmdParms)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    if (cmdParms.Length > 0)
                    {
                        PrepareCommand(cmd, DbConn, null, sql, cmdParms);
                    }

                    //MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    MySqlDataReader myReader = cmd.ExecuteReader();
                    if (cmdParms.Length > 0)
                    {
                        cmd.Parameters.Clear();
                    }

                    return myReader;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return null;
        }

        public DataSet Query(string SQLString, int Times = 0)
        {
            DataSet ds = new DataSet();
            MySqlDataAdapter command = null;

            using (command = new MySqlDataAdapter(SQLString, DbConn))
            {
                try
                {
                    if (Times > 0)
                    {
                        command.SelectCommand.CommandTimeout = Times;
                    }

                    command.Fill(ds, "ds");
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    throw new Exception(SQLString + "\r\n" + ex.Message);
                }
            }

            return ds;
        }

        public DataSet Query(string SQLString, params MySqlParameter[] cmdParms)
        {
            DataSet ds = new DataSet();
            MySqlDataAdapter command = null;

            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, DbConn, null, SQLString, cmdParms);
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                }

                return ds;
            }
        }

        public int ExecuteSqlTran(List<String> SQLStringList)
        {
            MySqlConnection connection = DbConn;
            using (MySqlCommand cmd = new MySqlCommand())
            {
                cmd.Connection = connection;
                MySqlTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch
                {
                    tx.Rollback();
                    return 0;
                }
            }
        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlInsertImg(string strSQL, List<Tuple<string, byte[]>> imgList)
        {
            int result = 0;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(strSQL, DbConn))
                {
                    foreach (var t in imgList)
                    {
                        string imgTag = t.Item1;
                        byte[] imgData = t.Item2;
                        MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter(imgTag, imgData);
                        //myParameter.Value = content;
                        cmd.Parameters.Add(myParameter);
                    }

                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(strSQL + "\r\n" + ex.ToString());
            }

            return result;
        }

        public int ExecuteSqlTran(List<CommandInfo> list, List<CommandInfo> oracleCmdSqlList)
        {
            MySqlConnection connection = DbConn;
            using (MySqlCommand cmd = new MySqlCommand())
            {
                cmd.Connection = connection;
                MySqlTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (CommandInfo myDE in list)
                    {
                        string cmdText = myDE.CommandText;
                        MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Parameters;
                        PrepareCommand(cmd, connection, tx, cmdText, cmdParms);
                        if (myDE.EffentNextType == EffentNextType.SolicitationEvent)
                        {
                            if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                            {
                                tx.Rollback();
                                throw new Exception("违背要求" + myDE.CommandText + "必须符合select count(..的格式");
                                //return 0;
                            }

                            object obj = cmd.ExecuteScalar();
                            bool isHave = false;
                            if (obj == null && obj == DBNull.Value)
                            {
                                isHave = false;
                            }
                            isHave = Convert.ToInt32(obj) > 0;
                            if (isHave)
                            {
                                //引发事件
                                myDE.OnSolicitationEvent();
                            }
                        }
                        if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                        {
                            if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                            {
                                tx.Rollback();
                                throw new Exception("SQL:违背要求" + myDE.CommandText + "必须符合select count(..的格式");
                                //return 0;
                            }

                            object obj = cmd.ExecuteScalar();
                            bool isHave = false;
                            if (obj == null && obj == DBNull.Value)
                            {
                                isHave = false;
                            }
                            isHave = Convert.ToInt32(obj) > 0;

                            if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                            {
                                tx.Rollback();
                                throw new Exception("SQL:违背要求" + myDE.CommandText + "返回值必须大于0");
                                //return 0;
                            }
                            if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                            {
                                tx.Rollback();
                                throw new Exception("SQL:违背要求" + myDE.CommandText + "返回值必须等于0");
                                //return 0;
                            }
                            continue;
                        }
                        int val = cmd.ExecuteNonQuery();
                        if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                        {
                            tx.Rollback();
                            throw new Exception("SQL:违背要求" + myDE.CommandText + "必须有影响行");
                            //return 0;
                        }
                        cmd.Parameters.Clear();
                    }
                    string oraConnectionString = PubConstant.GetConnectionString("ConnectionStringPPC");
                    bool res = OracleHelper.ExecuteSqlTran(oraConnectionString, oracleCmdSqlList);
                    if (!res)
                    {
                        tx.Rollback();
                        throw new Exception("执行失败");
                        // return -1;
                    }
                    tx.Commit();
                    return 1;
                }
                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    tx.Rollback();
                    throw e;
                }
            }
        }

        public int ExecuteSql(string sql, params MySqlParameter[] cmdParms)
        {
            int result = 0;

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, DbConn))
                {
                    PrepareCommand(cmd, DbConn, null, sql, cmdParms);
                    result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(sql + "\r\n" + ex.ToString());
            }

            return result;
        }

        public void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (MySqlTransaction trans = DbConn.BeginTransaction())
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            PrepareCommand(cmd, DbConn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ExecuteSqlTranWithIndentity(Hashtable SQLStringList)
        {
            using (MySqlTransaction trans = DbConn.BeginTransaction())
            {
                MySqlCommand cmd = new MySqlCommand();
                try
                {
                    int indentity = 0;
                    //循环
                    foreach (DictionaryEntry myDE in SQLStringList)
                    {
                        string cmdText = myDE.Key.ToString();
                        MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                        foreach (MySqlParameter q in cmdParms)
                        {
                            if (q.Direction == ParameterDirection.InputOutput)
                            {
                                q.Value = indentity;
                            }
                        }
                        PrepareCommand(cmd, DbConn, trans, cmdText, cmdParms);
                        int val = cmd.ExecuteNonQuery();
                        foreach (MySqlParameter q in cmdParms)
                        {
                            if (q.Direction == ParameterDirection.Output)
                            {
                                indentity = Convert.ToInt32(q.Value);
                            }
                        }
                        cmd.Parameters.Clear();
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public int ExecuteSqlTran(System.Collections.Generic.List<CommandInfo> cmdList)
        {
            using (MySqlTransaction trans = DbConn.BeginTransaction())
            {
                MySqlCommand cmd = new MySqlCommand();
                try
                {
                    int count = 0;
                    //循环
                    foreach (CommandInfo myDE in cmdList)
                    {
                        string cmdText = myDE.CommandText;
                        MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Parameters;
                        PrepareCommand(cmd, DbConn, trans, cmdText, cmdParms);

                        if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                        {
                            if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                            {
                                trans.Rollback();
                                return 0;
                            }

                            object obj = cmd.ExecuteScalar();
                            bool isHave = false;
                            if (obj == null && obj == DBNull.Value)
                            {
                                isHave = false;
                            }
                            isHave = Convert.ToInt32(obj) > 0;

                            if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                            {
                                trans.Rollback();
                                return 0;
                            }
                            if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                            {
                                trans.Rollback();
                                return 0;
                            }
                            continue;
                        }
                        int val = cmd.ExecuteNonQuery();
                        count += val;
                        if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                        {
                            trans.Rollback();
                            return 0;
                        }
                        cmd.Parameters.Clear();
                    }
                    trans.Commit();
                    return count;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public void ExecuteSqlTranWithIndentity(System.Collections.Generic.List<CommandInfo> SQLStringList)
        {
            using (MySqlTransaction trans = DbConn.BeginTransaction())
            {
                MySqlCommand cmd = new MySqlCommand();
                try
                {
                    int indentity = 0;
                    //循环
                    foreach (CommandInfo myDE in SQLStringList)
                    {
                        string cmdText = myDE.CommandText;
                        MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Parameters;
                        foreach (MySqlParameter q in cmdParms)
                        {
                            if (q.Direction == ParameterDirection.InputOutput)
                            {
                                q.Value = indentity;
                            }
                        }
                        PrepareCommand(cmd, DbConn, trans, cmdText, cmdParms);
                        int val = cmd.ExecuteNonQuery();
                        foreach (MySqlParameter q in cmdParms)
                        {
                            if (q.Direction == ParameterDirection.Output)
                            {
                                indentity = Convert.ToInt32(q.Value);
                            }
                        }
                        cmd.Parameters.Clear();
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    if (cmd != null)
                    {
                        cmd.Dispose();
                    }
                }
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
    }

}
