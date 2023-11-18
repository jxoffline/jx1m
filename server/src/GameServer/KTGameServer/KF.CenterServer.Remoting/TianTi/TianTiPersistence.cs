using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KF.Contract.Data;
using System.Configuration;
using Maticsoft.DBUtility;
using KF.Remoting.Data;
using MySql.Data.MySqlClient;
using Tmsk.Tools;
using System.Collections.Concurrent;
using Tmsk.Contract.Const;
using Tmsk.Contract;
using System.Threading;
using Server.Tools;
using GameServer.Core.Executor;
using System.Xml.Linq;
using Tmsk.Tools.Tools;
using Tmsk.DbHelper;

namespace KF.Remoting
{
    /// <summary>
    /// 天梯系统相关部分
    /// </summary>
    public class TianTiPersistence
    {
        private TianTiPersistence() { }

        public static readonly TianTiPersistence Instance = new TianTiPersistence();

        #region 配置信息

        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        public object MutexPaiHang = new object();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        public int SignUpWaitSecs1 = 5;
        public int SignUpWaitSecs3 = 10;
        public int SignUpWaitSecsAll = 15;

        public int WaitForJoinMaxSecs = 60;

        /// <summary>
        /// 最大发送的排名
        /// </summary>
        private int MaxSendDetailDataCount = 100;

        public int MaxRolePairFightCount = 3;
        
        #endregion 配置信息

        /// <summary>
        /// 数据库更新队列,异步写数据库
        /// </summary>
        private Queue<GameFuBenStateDbItem> GameFuBenStateDbItemQueue = new Queue<GameFuBenStateDbItem>();

        /// <summary>
        /// 排名信息
        /// </summary>
        public TianTiRankData RankData = new TianTiRankData();

        /// <summary>
        /// 待写入的角色信息队列
        /// </summary>
        public ConcurrentQueue<TianTiRoleInfoData> TianTiRoleInfoDataQueue = new ConcurrentQueue<TianTiRoleInfoData>();


        private int CurrGameId = Global.UninitGameId;
        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {
                XElement xmlFile = ConfigHelper.Load("config.xml");

                SignUpWaitSecs3 = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SignUpWaitSecs3", "value", 10);
                SignUpWaitSecsAll = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SignUpWaitSecsAll", "value", 15);
                RankData.MaxPaiMingRank = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "MaxPaiMingRank", "value", 50000);
                MaxSendDetailDataCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "MaxSendDetailDataCount", "value", 100);
                MaxRolePairFightCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "MaxRolePairFightCount", "value", 3);

                if (CurrGameId == Global.UninitGameId)
                {
                    CurrGameId = (int)((long)DbHelperMySQL.GetSingle("SELECT IFNULL(MAX(id),0) FROM t_tianti_game_fuben;"));
                }

                Initialized = true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public void SaveCostTime(int ms)
        {
            try
            {
                if (ms > KuaFuServerManager.WritePerformanceLogMs)
                {
                    LogManager.WriteLog(LogTypes.Warning, "TianTi 执行时间(ms):" + ms);
                }
            }
            catch
            {
            	
            }
        }

        public TianTiRankData GetTianTiRankData(DateTime modifyTime)
        {
            TianTiRankData tianTiRankData = new TianTiRankData();
            lock (Mutex)
            {
                tianTiRankData.ModifyTime = RankData.ModifyTime;
                tianTiRankData.MaxPaiMingRank = RankData.MaxPaiMingRank;
                if (modifyTime < RankData.ModifyTime && null != RankData.TianTiRoleInfoDataList)
                {
                    tianTiRankData.TianTiRoleInfoDataList = new List<TianTiRoleInfoData>(RankData.TianTiRoleInfoDataList);
                }
                if (modifyTime < RankData.ModifyTime && null != RankData.TianTiMonthRoleInfoDataList)
                {
                    tianTiRankData.TianTiMonthRoleInfoDataList = new List<TianTiRoleInfoData>(RankData.TianTiMonthRoleInfoDataList);
                }
            }

            return tianTiRankData;
        }

        private bool ReloadTianTiRankDayList(List<TianTiRoleInfoData> tianTiRoleInfoDataList)
        {
            MySqlDataReader sdr = null;
            string strSql;

            try
            {
                strSql = string.Format("SELECT rid,rname,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,data1,data2 FROM t_tianti_roles where duanweijifen>0 ORDER BY duanweijifen DESC,duanweirank DESC LIMIT {0};", RankData.MaxPaiMingRank);
                sdr = DbHelperMySQL.ExecuteReader(strSql);
                for (int index = 1; sdr.Read(); index++)
                {
                    TianTiRoleInfoData tianTiRoleInfoData = new TianTiRoleInfoData();
                    tianTiRoleInfoData.RoleId = (int)Convert.ToInt32(sdr["rid"]);
                    if (index <= MaxSendDetailDataCount)
                    {
                        tianTiRoleInfoData.ZoneId = (int)Convert.ToInt32(sdr["zoneid"]);
                        tianTiRoleInfoData.DuanWeiId = (int)Convert.ToInt32(sdr["duanweiid"]);
                        tianTiRoleInfoData.DuanWeiJiFen = (int)Convert.ToInt32(sdr["duanweijifen"]);
                        //tianTiRoleInfoData.DuanWeiRank = (int)Convert.ToInt32(sdr["duanweirank"]);
                        tianTiRoleInfoData.ZhanLi = (int)Convert.ToInt32(sdr["zhanli"]);
                        tianTiRoleInfoData.RoleName = sdr["rname"].ToString();
                        if (!sdr.IsDBNull(sdr.GetOrdinal("data1"))) tianTiRoleInfoData.TianTiPaiHangRoleData = (byte[])(sdr["data1"]);
                        //if (!sdr.IsDBNull(sdr.GetOrdinal("data2"))) tianTiRoleInfoData.PlayerJingJiMirrorData = (byte[])(sdr["data2"]);
                    }

                    tianTiRoleInfoData.DuanWeiRank = index;
                    tianTiRoleInfoDataList.Add(tianTiRoleInfoData);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
            finally
            {
                if (null != sdr)
                {
                    sdr.Close();
                }
            }

            return false;
        }

        private bool LoadTianTiRankDayList(List<TianTiRoleInfoData> tianTiRoleInfoDataList)
        {
            bool result = false;
            MySqlDataReader sdr = null;
            string strSql;

            try
            {
                strSql = string.Format("SELECT r.rid,rname,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,data1,data2 FROM t_tianti_roles r, t_tianti_day_paihang d WHERE r.rid=d.rid ORDER BY d.`rank` ASC LIMIT {0};", RankData.MaxPaiMingRank);
                sdr = DbHelperMySQL.ExecuteReader(strSql);
                for (int index = 1; sdr.Read(); index++)
                {
                    TianTiRoleInfoData tianTiRoleInfoData = new TianTiRoleInfoData();
                    tianTiRoleInfoData.RoleId = (int)Convert.ToInt32(sdr["rid"]);
                    if (index <= MaxSendDetailDataCount)
                    {
                        tianTiRoleInfoData.ZoneId = (int)Convert.ToInt32(sdr["zoneid"]);
                        tianTiRoleInfoData.DuanWeiId = (int)Convert.ToInt32(sdr["duanweiid"]);
                        tianTiRoleInfoData.DuanWeiJiFen = (int)Convert.ToInt32(sdr["duanweijifen"]);
                        //tianTiRoleInfoData.DuanWeiRank = (int)Convert.ToInt32(sdr["duanweirank"]);
                        tianTiRoleInfoData.ZhanLi = (int)Convert.ToInt32(sdr["zhanli"]);
                        tianTiRoleInfoData.RoleName = sdr["rname"].ToString();
                        if (!sdr.IsDBNull(sdr.GetOrdinal("data1"))) tianTiRoleInfoData.TianTiPaiHangRoleData = (byte[])(sdr["data1"]);
                        //if (!sdr.IsDBNull(sdr.GetOrdinal("data2"))) tianTiRoleInfoData.PlayerJingJiMirrorData = (byte[])(sdr["data2"]);
                    }

                    tianTiRoleInfoData.DuanWeiRank = index;
                    tianTiRoleInfoDataList.Add(tianTiRoleInfoData);

                    result = true;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
            finally
            {
                if (null != sdr)
                {
                    sdr.Close();
                }
            }

            return result;
        }

        public void LoadTianTiRankData(DateTime now)
        {
            try
            {
                ExecuteSqlNoQuery("INSERT IGNORE INTO t_async(`id`,`value`) VALUES(4,1);");
                object ageObj = DbHelperMySQL.GetSingle("select value from t_async where id = " + AsyncTypes.TianTiPaiHangModifyOffsetDay);
                if (null != ageObj)
                {
                    int dayId = (int)ageObj;
                    DateTime modifyDate = DataHelper2.GetRealDate(dayId);

                    List<TianTiRoleInfoData> tianTiRoleInfoDataList = new List<TianTiRoleInfoData>();
                    List<TianTiRoleInfoData> tianTiMonthRoleInfoDataList = new List<TianTiRoleInfoData>();
                    MySqlDataReader sdr = null;

                    try
                    {
                        LoadTianTiRankDayList(tianTiRoleInfoDataList);
                        //ReloadTianTiRankDayList(tianTiRoleInfoDataList);

                        sdr = DbHelperMySQL.ExecuteReader(string.Format("SELECT rid,rname,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,data1,data2 FROM t_tianti_month_paihang ORDER BY `duanweirank` ASC LIMIT {0};", RankData.MaxPaiMingRank));
                        for (int index = 1; sdr.Read(); index++)
                        {
                            TianTiRoleInfoData tianTiRoleInfoData = new TianTiRoleInfoData();
                            tianTiRoleInfoData.RoleId = (int)Convert.ToInt32(sdr["rid"]);
                            if (index <= MaxSendDetailDataCount)
                            {
                                tianTiRoleInfoData.ZoneId = (int)Convert.ToInt32(sdr["zoneid"]);
                                tianTiRoleInfoData.DuanWeiId = (int)Convert.ToInt32(sdr["duanweiid"]);
                                tianTiRoleInfoData.DuanWeiJiFen = (int)Convert.ToInt32(sdr["duanweijifen"]);
                                //tianTiRoleInfoData.DuanWeiRank = (int)Convert.ToInt32(sdr["duanweirank"]);
                                tianTiRoleInfoData.ZhanLi = (int)Convert.ToInt32(sdr["zhanli"]);
                                tianTiRoleInfoData.RoleName = sdr["rname"].ToString();
                                if (!sdr.IsDBNull(sdr.GetOrdinal("data1"))) tianTiRoleInfoData.TianTiPaiHangRoleData = (byte[])(sdr["data1"]);
                               // if (!sdr.IsDBNull(sdr.GetOrdinal("data2"))) tianTiRoleInfoData.PlayerJingJiMirrorData = (byte[])(sdr["data2"]);
                            }

                            tianTiRoleInfoData.DuanWeiRank = index;
                            tianTiMonthRoleInfoDataList.Add(tianTiRoleInfoData);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogManager.WriteExceptionUseCache(ex.ToString());
                    }
                    finally
                    {
                        if (null != sdr)
                        {
                            sdr.Close();
                        }
                    }

                    lock (Mutex)
                    {
                        RankData.ModifyTime = modifyDate;
                        RankData.TianTiRoleInfoDataList = tianTiRoleInfoDataList;
                        RankData.TianTiMonthRoleInfoDataList = tianTiMonthRoleInfoDataList;
                    }

                    if (DataHelper2.GetOffsetDay(now) != dayId)
                    {
                        UpdateTianTiRankData(now, modifyDate.Month != now.Month, true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
            }
        }

        public void UpdateTianTiRankData(DateTime now, bool monthRank = false, bool force = false)
        {
            if (Monitor.TryEnter(MutexPaiHang))
            {
                try
                {
                    if (!force)
                    {
                        lock (Mutex)
                        {
                            if (RankData.ModifyTime.DayOfYear == now.DayOfYear)
                            {
                                return;
                            }
                        }
                    }

                    if (!monthRank)
                    {
                        if (now.Day == 1)
                        {
                            monthRank = true;
                        }
                    }

                    List<TianTiRoleInfoData> tianTiRoleInfoDataList = new List<TianTiRoleInfoData>();
                    string strSql = "";
                    MySqlDataReader sdr = null;

                    try
                    {
                        ReloadTianTiRankDayList(tianTiRoleInfoDataList);
                    }
                    catch (System.Exception ex)
                    {
                        LogManager.WriteExceptionUseCache(ex.ToString());
                        return;
                    }
                    finally
                    {
                        if (null != sdr)
                        {
                            sdr.Close();
                        }
                    }

                    int ret = 0;
                    try
                    {
                        if (tianTiRoleInfoDataList.Count > 0)
                        {
                            ret = DbHelperMySQL.ExecuteSql(string.Format("UPDATE t_tianti_roles SET `duanweirank`={0}", RankData.MaxPaiMingRank + 1));
                            if (ret >= 0)
                            {
                                ret = DbHelperMySQL.ExecuteSql("DELETE FROM t_tianti_day_paihang;");
                            }
                            if (ret >= 0)
                            {
                                int c = tianTiRoleInfoDataList.Count;
                                int numPerExec = 50;
                                for (int i = 0; i < c; i++ )
                                {
                                    if (i % numPerExec == 0)
                                    {
                                        strSql = "INSERT INTO t_tianti_day_paihang(rid,rank) VALUES";
                                    }

                                    strSql += string.Format("({0},{1})", tianTiRoleInfoDataList[i].RoleId, tianTiRoleInfoDataList[i].DuanWeiRank);
                                    if ((i % numPerExec) == (numPerExec - 1) || i == (c - 1))
                                    {
                                        DbHelperMySQL.ExecuteSql(strSql);
                                    }
                                    else
                                    {
                                        strSql += ',';
                                    }
                                }

                                DbHelperMySQL.ExecuteSql("UPDATE t_tianti_roles r, t_tianti_day_paihang d SET r.`duanweirank` = d.`rank` WHERE r.`rid` = d.`rid`;");

                                if (monthRank)
                                {
                                    DbHelperMySQL.ExecuteSql("DELETE FROM t_tianti_month_paihang;");
                                    strSql = "INSERT INTO t_tianti_month_paihang SELECT * FROM t_tianti_roles WHERE rid IN (SELECT rid FROM t_tianti_day_paihang) ORDER BY `duanweirank` ASC;";
                                    DbHelperMySQL.ExecuteSql(strSql);

                                    DbHelperMySQL.ExecuteSql("DELETE FROM t_tianti_day_paihang;");
                                    DbHelperMySQL.ExecuteSql("UPDATE t_tianti_roles SET `duanweirank`=0,`duanweijifen`=0,`duanweiid`=0;");
                                }
                            }

                            if (ret >= 0)
                            {
                                strSql = string.Format("UPDATE t_async SET `value`={1} WHERE `id`={0};", AsyncTypes.TianTiPaiHangModifyOffsetDay, DataHelper2.GetOffsetDay(now));
                                ExecuteSqlNoQuery(strSql);
                            }
                        }

                        lock (Mutex)
                        {
                            RankData.ModifyTime = now;
                            if (monthRank)
                            {
                                RankData.TianTiRoleInfoDataList = new List<TianTiRoleInfoData>();
                                RankData.TianTiMonthRoleInfoDataList = tianTiRoleInfoDataList;
                            }
                            else
                            {
                                RankData.TianTiRoleInfoDataList = tianTiRoleInfoDataList;
                            }
                        }

                        if (monthRank)
                        {
                            try
                            {
                                // 这里是为了保证跨服中心月初跨天的时候，更新
                                ZhengBaManagerK.Instance().ReloadSyncData(now);
                            }
                            catch (Exception ex)
                            {
                                LogManager.WriteLog(LogTypes.Error, "UpdateTianTiRankData -> zhengba reload execption", ex);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ret = -1;
                        LogManager.WriteException(ex.ToString());
                    }
                }
                finally
                {
                    Monitor.Exit(MutexPaiHang);
                }
            }
        }

        public void UpdateRoleInfoData(TianTiRoleInfoData data)
        {
            if (TianTiRoleInfoDataQueue.Count > 100000)
            {
                TianTiRoleInfoData tmpData;
                TianTiRoleInfoDataQueue.TryDequeue(out tmpData);
            }

            TianTiRoleInfoDataQueue.Enqueue(data);
        }

        public void WriteRoleInfoDataToDb(TianTiRoleInfoData data)
        {
            //写数据库
            try
            {
                List<Tuple<string, byte[]>> imgList = new List<Tuple<string, byte[]>>();
                imgList.Add(new Tuple<string, byte[]>("content", data.TianTiPaiHangRoleData));
                imgList.Add(new Tuple<string, byte[]>("mirror", data.PlayerJingJiMirrorData));
                //IConvertible
                DbHelperMySQL.ExecuteSqlInsertImg(string.Format("INSERT INTO t_tianti_roles(rid,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,rname,data1,data2) " +
                                                            "VALUES({0},{1},{2},{3},{4},{5},'{6}',@content,@mirror) " +
                                                            "ON DUPLICATE KEY UPDATE `zoneid`={1},`duanweiid`={2},`duanweijifen`={3},`duanweirank`={4},`zhanli`={5},`rname`='{6}',`data1`=@content,`data2`=@mirror;",
                                                            data.RoleId, data.ZoneId, data.DuanWeiId, data.DuanWeiJiFen, data.DuanWeiRank, data.ZhanLi, data.RoleName),
                                                    imgList);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public void WriteRoleInfoDataProc()
        {
            TianTiRoleInfoData data;
            for (int i = 0; i < 1000; i++)
            {
                if (!TianTiRoleInfoDataQueue.TryDequeue(out data))
                {
                    break;
                }

                WriteRoleInfoDataToDb(data);

                lock (Mutex)
                {
                    if (RankData.TianTiRoleInfoDataList.Count < 3)
                    {
                        if (!RankData.TianTiRoleInfoDataList.Exists((x) => x.RoleId == data.RoleId))
                        {
                            RankData.ModifyTime = TimeUtil.NowDateTime();
                            RankData.TianTiRoleInfoDataList.Add(data);
                            data.DuanWeiRank = RankData.TianTiRoleInfoDataList.Count;
                        }
                    }
                }
            }
        }

        public void ExecuteSqlNoQuery(string sqlCmd)
        {
            //写数据库
            try
            {
                DbHelperMySQL.ExecuteSql(sqlCmd);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public int GetNextGameId()
        {
            return Interlocked.Add(ref CurrGameId, 1);
        }

        public void LogCreateTianTiFuBen(int gameId, int serverId, int fubenSeqId, int roleCount)
        {
            string sql = string.Format("INSERT INTO t_tianti_game_fuben(`id`,`serverid`,`fubensid`,`createtime`,`rolenum`) VALUES({0},{1},{2},'{3}',{4});",
               gameId, serverId, fubenSeqId, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), roleCount);

            ExecuteSqlNoQuery(sql);
        }        
    }
}
