using GameDBServer.DB;
using MySQLDriverCS;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameDBServer.Logic.Name
{

    public class NameUsedMgr : SingletonTemplate<NameUsedMgr>
    {
        private NameUsedMgr()
        { }

        private HashSet<string> cannotUse = new HashSet<string>();
        private HashSet<string> cannotUse_BangHui = new HashSet<string>();


        public void LoadFromDatabase(DBManager dbMgr)
        {
            MySQLConnection conn = null;


            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT oldname FROM t_change_name");

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                try
                {
                    MySQLDataReader reader = cmd.ExecuteReaderEx();

                    lock (cannotUse)
                    {
                        cannotUse.Clear();
                        while (reader.Read())
                        {
                            string name = reader["oldname"].ToString();
                            if (!string.IsNullOrEmpty(name) && !cannotUse.Contains(name))
                            {
                                cannotUse.Add(name);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("查询数据库失败: {0}", cmdText));
                }

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }
        }

        #region


        public bool AddCannotUse_Ex(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            lock (cannotUse)
            {
                if (cannotUse.Contains(name))
                {
                    return false;
                }

                cannotUse.Add(name);
            }

            return true;
        }


        public bool DelCannotUse_Ex(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            lock (cannotUse)
            {
                if (cannotUse.Contains(name))
                {
                    cannotUse.Remove(name);
                    return true;
                }
            }

            return false;
        }

        #endregion


    }
}