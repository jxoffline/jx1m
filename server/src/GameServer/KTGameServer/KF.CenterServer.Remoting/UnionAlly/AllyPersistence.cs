using KF.Contract.Data;
using Maticsoft.DBUtility;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Tmsk.Contract.KuaFuData;
using Tmsk.Tools.Tools;

namespace KF.Remoting
{
    public class AllyPersistence
    {
        #region ----------
        private AllyPersistence() { }
        public static readonly AllyPersistence Instance = new AllyPersistence();

        public long DataVersion = 0;

        public void InitConfig()
        {
            try
            {
                DataVersion = DateTime.Now.Ticks;
                XElement xmlFile = ConfigHelper.Load("config.xml");
                Consts.AllyNumMax = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "AllyNumMax", "value", 5);
                Consts.AllyRequestClearSecond = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "AllyRequestClearSecond", "value", 86400);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        private int ExecuteSqlNoQuery(string sqlCmd)
        {
            int i = 0;
            try
            {
                i = DbHelperMySQL.ExecuteSql(sqlCmd);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return i;
        }

        #endregion

        #region ----------日志

        public List<AllyLogData> DBAllyLogList(int unionID)
        {
            List<AllyLogData> list = new List<AllyLogData>();
            try
            {
                string strSql = string.Format(@"SELECT l.myUnionID,l.unionID,u.unionZoneID,u.unionName,l.logTime,l.logState
                                                FROM t_ally_log l,t_ally_union u
                                                WHERE l.unionID = u.unionID AND l.unionID='{0}'", unionID);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    AllyLogData item = new AllyLogData();
                    item.MyUnionID = (int)Convert.ToInt32(sdr["myUnionID"]);
                    item.UnionID = (int)Convert.ToInt32(sdr["unionID"]);
                    item.LogTime = Convert.ToDateTime(sdr["logTime"]);
                    item.LogState = (int)Convert.ToInt32(sdr["logState"]);

                    list.Add(item);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return list;
        }

        public bool DBAllyLogDel(int unionID)
        {
            string sql = string.Format("DELETE FROM t_ally_log WHERE myUnionID='{0}'",unionID);
            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public bool DBAllyLogAdd(AllyLogData logData)
        {
            string sql = string.Format("INSERT INTO t_ally_log(myUnionID, unionID, logTime,logState) VALUES('{0}','{1}','{2}','{3}')",
                logData.MyUnionID, logData.UnionID, logData.LogTime, logData.LogState);

            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        #endregion

        #region ----------战盟信息

        public KFAllyData DBUnionDataGet(int unionID)
        {
            KFAllyData item = null;
            try
            {
                string strSql = string.Format(@"SELECT unionID,unionZoneID,unionName,unionLevel,unionNum,leaderID,leaderZoneID,leaderName,logTime,serverID
                                                FROM t_ally_union
                                                WHERE unionID='{0}'", unionID);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    item = new KFAllyData();
                    item.UnionID = (int)Convert.ToInt32(sdr["unionID"]);
                    item.UnionZoneID = (int)Convert.ToInt32(sdr["unionZoneID"]);
                    item.UnionName = sdr["unionName"].ToString();
                    item.UnionLevel = (int)Convert.ToInt32(sdr["unionLevel"]);
                    item.UnionNum = (int)Convert.ToInt32(sdr["unionNum"]);
                    item.LeaderID = (int)Convert.ToInt32(sdr["leaderID"]);
                    item.LeaderZoneID = (int)Convert.ToInt32(sdr["leaderZoneID"]);
                    item.LeaderName = sdr["leaderName"].ToString();
                    item.LogTime = Convert.ToDateTime(sdr["logTime"]);
                    item.ServerID = (int)Convert.ToInt32(sdr["serverID"]);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return item;
        }

        public KFAllyData DBUnionDataGet(int unionZoneID, string unionName)
        {
            KFAllyData item = null;
            try
            {
                string strSql = string.Format(@"SELECT unionID,unionZoneID,unionName,unionLevel,unionNum,leaderID,leaderZoneID,leaderName,logTime,serverID
                                                FROM t_ally_union
                                                WHERE unionZoneID='{0}' and unionName='{1}'", unionZoneID, unionName);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    item = new KFAllyData();
                    item.UnionID = (int)Convert.ToInt32(sdr["unionID"]);
                    item.UnionZoneID = (int)Convert.ToInt32(sdr["unionZoneID"]);
                    item.UnionName = sdr["unionName"].ToString();
                    item.UnionLevel = (int)Convert.ToInt32(sdr["unionLevel"]);
                    item.UnionNum = (int)Convert.ToInt32(sdr["unionNum"]);
                    item.LeaderID = (int)Convert.ToInt32(sdr["leaderID"]);
                    item.LeaderZoneID = (int)Convert.ToInt32(sdr["leaderZoneID"]);
                    item.LeaderName = sdr["leaderName"].ToString();
                    item.LogTime = Convert.ToDateTime(sdr["logTime"]);
                    item.ServerID = (int)Convert.ToInt32(sdr["serverID"]);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return item;
        }

        public bool DBUnionDataUpdate(KFAllyData data)
        {
            string sql = string.Format(@"REPLACE INTO t_ally_union(unionID,unionZoneID,unionName,unionLevel,unionNum,leaderID,leaderZoneID,leaderName,logTime,serverID) 
                                        VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                                        data.UnionID, data.UnionZoneID, data.UnionName, data.UnionLevel, data.UnionNum,
                                        data.LeaderID, data.LeaderZoneID, data.LeaderName,
                                        data.LogTime, data.ServerID);

            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public bool DBUnionDataDel(int unionID)
        {
            string sql = string.Format(@"DELETE FROM t_ally_union WHERE unionID={0}", unionID);
            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        #endregion

        #region ----------结盟信息，请求

        public List<int> DBAllyIDList(int unionID)
        {
            List<int> idList = new List<int>();
            try
            {
                string strSql = string.Format(@"SELECT DISTINCT(unionID2) uid from t_ally where unionID1='{0}' 
                                                UNION
                                                SELECT DISTINCT(unionID1) uid from t_ally where unionID2='{0}' 
                                                ORDER BY uid", unionID);

                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    idList.Add(Convert.ToInt32(sdr["uid"]));
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return idList;
        }

        public bool DBAllyAdd(int myUnionID, int unionID, DateTime logTime)
        {
            string sql = string.Format("INSERT INTO t_ally(unionID1, unionID2, logTime) VALUES('{0}','{1}','{2}')",
                myUnionID, unionID, logTime);

            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public bool DBAllyDel(int unionID,int targetID)
        {
            string sql = string.Format("DELETE FROM t_ally WHERE (unionID1='{0}' and unionID2='{1}') or(unionID1='{1}' and unionID2='{0}')", unionID, targetID); ;
            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public List<KFAllyData> DBAllyRequestList(int unionID)
        {
            List<KFAllyData> list = new List<KFAllyData>();
            try
            {
                string strSql = string.Format(@"SELECT unionID,logTime,logState FROM t_ally_request WHERE myUnionID='{0}'", unionID);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    KFAllyData item = new KFAllyData();
                    item.UnionID = (int)Convert.ToInt32(sdr["unionID"]);
                    item.LogTime = Convert.ToDateTime(sdr["logTime"]);
                    item.LogState = (int)Convert.ToInt32(sdr["logState"]);

                    list.Add(item);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return list;
        }

        public bool DBAllyRequestAdd(int myUnionID, int unionID, DateTime logTime, int logState)
        {
            string sql = string.Format("INSERT INTO t_ally_request(myUnionID, unionID, logTime,logState) VALUES('{0}','{1}','{2}','{3}')",
                myUnionID, unionID, logTime, logState);

            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public bool DBAllyRequestDel(int myUnionID, int unionID)
        {
            string sql = string.Format("DELETE FROM t_ally_request WHERE myUnionID='{0}' and unionID='{1}'", myUnionID,unionID);
            int i = ExecuteSqlNoQuery(sql);
            return i > 0;
        }

        public List<KFAllyData> DBAllyAcceptList(int unionID)
        {
            List<KFAllyData> list = new List<KFAllyData>();
            try
            {
                string strSql = string.Format(@"SELECT myUnionID,logTime,logState FROM t_ally_request WHERE unionID='{0}'", unionID);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    KFAllyData item = new KFAllyData();
                    item.UnionID = (int)Convert.ToInt32(sdr["myUnionID"]);
                    item.LogTime = Convert.ToDateTime(sdr["logTime"]);
                    item.LogState = (int)Convert.ToInt32(sdr["logState"]);

                    list.Add(item);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return list;
        }

        #endregion
    }
}
