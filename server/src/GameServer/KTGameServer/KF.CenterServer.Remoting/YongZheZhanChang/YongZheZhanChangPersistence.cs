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
using Tmsk.Tools.Tools;
using System.Xml.Linq;
using Tmsk.DbHelper;

namespace KF.Remoting
{
    public class CityLevelInfo
    {
        public int ID;
        public int CityLevel;
        public int CityNum;
        public int MaxNum;
        public int[] AttackWeekDay;
    }

    /// <summary>
    /// 天梯系统相关部分
    /// </summary>
    public class YongZheZhanChangPersistence
    {
        private YongZheZhanChangPersistence() { }

        public static readonly YongZheZhanChangPersistence Instance = new YongZheZhanChangPersistence();

        #region 配置信息

        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        private int CurrGameId = Global.UninitGameId;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// 最大排名
        /// </summary>
        private int MaxPaiMingRank = 100;

        /// <summary>
        /// 跨服Boss每场基准人数
        /// </summary>
        public int KuaFuBossRoleCount = 50;

        /// <summary>
        /// 勇者战场每场基准人数
        /// </summary>
        public int YongZheZhanChangRoleCount = 100;

        /// <summary>
        /// 王者战场每场基准人数
        /// </summary>
        public int KingOfBattleRoleCount = 40;

        /// <summary>
        /// 当前活动的每场基准人数
        /// </summary>
        public int MinEnterCount = 100;

        /// <summary>
        /// 单服务器最大负载这种活动副本的数量
        /// </summary>
        public int MaxServerCapcity = 30;

        /// <summary>
        /// 当前的服务器负载阈值系数
        /// </summary>
        public int ServerCapacityRate = 1;

        /// <summary>
        /// 狼魂领域的负载系数
        /// </summary>
        public int LangHunLingYuServerCapacityRate = 5;
        
        #endregion 配置信息
        /// <summary>
        /// 数据库更新队列,异步写数据库
        /// </summary>
        private Queue<GameFuBenStateDbItem> GameFuBenStateDbItemQueue = new Queue<GameFuBenStateDbItem>();

        /// <summary>
        /// 待写入的副本统计信息队列
        /// </summary>
        public ConcurrentQueue<object> YongZheZhanChangRoleInfoDataQueue = new ConcurrentQueue<object>();

        /// <summary>
        /// 圣域争霸加载完成
        /// </summary>
        public bool LangHunLingYuInitialized = false;

        /// <summary>
        /// 重置城池进攻方的时间，战斗结束后和下一次开始报名时间之间（5分钟）
        /// </summary>
        public TimeSpan LangHunLingYuResetCityTime;

        public DateTime LastLangHunLingYuResetCityTime;

        public int OtherListUpdateOffsetDay = 0;

        public Dictionary<int, CityLevelInfo> CityLevelInfoDict = new Dictionary<int, CityLevelInfo>();

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {
                XElement xmlFile = ConfigHelper.Load("config.xml");

                YongZheZhanChangRoleCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "MinEnterCount", "value", 100); //勇者战场
                KuaFuBossRoleCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "KuaFuBossRoleCount", "value", 50); //跨服Boss
                KingOfBattleRoleCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "KingOfBattleRoleCount", "value", 40); //王者战场

                string strLangHunLingYuResetCityTime = ConfigHelper.GetElementAttributeValue(xmlFile, "add", "key", "LangHunLingYuResetCityTime", "value", ""); //跨服Boss
                if (string.IsNullOrEmpty(strLangHunLingYuResetCityTime) || !TimeSpan.TryParse(strLangHunLingYuResetCityTime, out LangHunLingYuResetCityTime))
                {
                    //如果未配置或者解析出错，则设置充值时间为不可达到的值，防止在错误的时间重置引起玩家数据丢失
                    LangHunLingYuResetCityTime = TimeSpan.MaxValue;
                }

                //强制限制单服最大活动副本数
                int MaxRoleCount = Math.Max(YongZheZhanChangRoleCount, KuaFuBossRoleCount);
                MaxRoleCount = Math.Max(MaxRoleCount, KingOfBattleRoleCount);
                MaxServerCapcity = Math.Max(3000 / MaxRoleCount, 30);

                InitLangHunLingYuConfig();

                if (CurrGameId == Global.UninitGameId)
                {
                    CurrGameId = (int)((long)DbHelperMySQL.GetSingle("SELECT IFNULL(MAX(id),0) FROM t_yongzhezhanchang_game_fuben;")); 
                }

                Initialized = true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        private void InitLangHunLingYuConfig()
        {
            string fileName = "";
            string fullPathFileName = "";
            IEnumerable<XElement> nodes;
            lock (Mutex)
            {
                try
                {
                    Dictionary<int, CityLevelInfo> cityLevelInfoDict = new Dictionary<int, CityLevelInfo>();

                    fileName = "KT_Map/Map_Dispute.xml";
                    fullPathFileName = KuaFuServerManager.GetResourcePath(fileName, KuaFuServerManager.ResourcePathTypes.GameRes);
                    XElement xml = ConfigHelper.Load(fullPathFileName);

                    nodes = xml.Elements();
                    int cityId = 0;
                    foreach (var t in nodes)
                    {
                        string type = ConfigHelper.GetElementAttributeValue(t, "TypeID");
                        if (string.Compare(type, KuaFuServerManager.platformType.ToString(), true) == 0)
                        {
                            foreach (var node in t.Elements())
                            {
                                CityLevelInfo item = new CityLevelInfo();
                                item.ID = (int)ConfigHelper.GetElementAttributeValueLong(node, "ID");
                                item.CityLevel = (int)ConfigHelper.GetElementAttributeValueLong(node, "CityLevel");
                                item.CityNum = (int)ConfigHelper.GetElementAttributeValueLong(node, "CityNum");
                                item.MaxNum = (int)ConfigHelper.GetElementAttributeValueLong(node, "MaxNum");

                                string strAttackWeekDay = ConfigHelper.GetElementAttributeValue(node, "AttackWeekDay");
                                item.AttackWeekDay = ConfigHelper.String2IntArray(strAttackWeekDay);

                                cityLevelInfoDict[item.CityLevel] = item;
                            }

                            break;
                        }
                    }

                    CityLevelInfoDict = cityLevelInfoDict;
                    if (CityLevelInfoDict.Count == 0)
                    {
                        LogManager.WriteLog(LogTypes.Fatal, string.Format("读取配置{0}失败,读取到的城池配置数为0"));
                        KuaFuServerManager.LoadConfigSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Fatal, string.Format("加载xml配置文件:{0}, 失败。{1}", fileName, ex.ToString()));
                    KuaFuServerManager.LoadConfigSuccess = false;
                }
            }
        }

        public void SaveCostTime(int ms)
        {
            try
            {
                if (ms > KuaFuServerManager.WritePerformanceLogMs)
                {
                    LogManager.WriteLog(LogTypes.Warning, "YongZheZhanChang 执行时间(ms):" + ms);
                }
            }
            catch
            {
            	
            }
        }

        public void UpdateRoleInfoData(object data)
        {
            if (YongZheZhanChangRoleInfoDataQueue.Count > 100000)
            {
                object tmpData;
                YongZheZhanChangRoleInfoDataQueue.TryDequeue(out tmpData);
            }

            YongZheZhanChangRoleInfoDataQueue.Enqueue(data);
        }

        public void WriteRoleInfoDataToDb(object obj)
        {
            //写数据库
            try
            {
                string sql = "";
                if (obj is YongZheZhanChangStatisticalData)
                {
                    YongZheZhanChangStatisticalData data = obj as YongZheZhanChangStatisticalData;
                    sql = string.Format("INSERT INTO t_yongzhezhanchang_fuben_log(gameid,allrolecount,winrolecount,loserolecount,lianshascore,zhongjiescore,caijiscore,bossscore,killscore,gametime) " +
                                                                "VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},now())",
                                                                data.GameId, data.AllRoleCount, data.WinRoleCount, data.LoseRoleCount, data.LianShaScore,
                                                                data.ZhongJieScore, data.CaiJiScore, data.BossScore, data.KillScore);
                }
                else if(obj is KingOfBattleStatisticalData)
                {
                    KingOfBattleStatisticalData data = obj as KingOfBattleStatisticalData;
                    sql = string.Format("INSERT INTO t_kingofbattle_fuben_log(gameid,allrolecount,winrolecount,loserolecount,lianshascore,zhongjiescore,caijiscore,bossscore,killscore,gametime) " +
                                                                "VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},now())",
                                                                data.GameId, data.AllRoleCount, data.WinRoleCount, data.LoseRoleCount, data.LianShaScore,
                                                                data.ZhongJieScore, data.CaiJiScore, data.BossScore, data.KillScore);
                }
                else if(obj is KuaFuBossStatisticalData)
                {
                    KuaFuBossStatisticalData data = obj as KuaFuBossStatisticalData;
                    for (int i = 0; i < data.MonsterDieTimeList.Count - 1; i += 2 )
                    {
                        //INSERT INTO t_kuafu_boss_log(gameid,monsterid,killtime) VALUES({0},{1},{2});
                        sql += string.Format("INSERT INTO t_kuafu_boss_log VALUES({0},{1},{2});", data.GameId, data.MonsterDieTimeList[i], data.MonsterDieTimeList[i + 1]);
                    }
                }
                else if (obj is GameLogItem)
                {
                    GameLogItem data = obj as GameLogItem;
                    sql = string.Format("INSERT INTO t_yongzhezhanchang_role_statistics_log(servercount,fubencount,signupcount,entercount,gametime) VALUES({0},{1},{2},{3},now());", 
                                            data.ServerCount, data.FubenCount, data.SignUpCount, data.EnterCount);
                }

                if (!string.IsNullOrEmpty(sql))
                {
                    DbHelperMySQL.ExecuteSql(sql);
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public void WriteRoleInfoDataProc()
        {
            object data;
            for (int i = 0; i < 1000; i++)
            {
                if (!YongZheZhanChangRoleInfoDataQueue.TryDequeue(out data))
                {
                    break;
                }

                WriteRoleInfoDataToDb(data);
            }
        }

        private void ExecuteSqlNoQuery(string sqlCmd)
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
            return Interlocked.Add(ref this.CurrGameId, 1);
        }

        public void LogCreateYongZheFuBen(int kfSrvId, int gameId, int fubenSeq, int roleNum)
        {
            string sql = string.Format("INSERT INTO t_yongzhezhanchang_game_fuben(`id`,`serverid`,`fubensid`,`createtime`,`rolenum`) VALUES({0},{1},{2},'{3}',{4});",
                gameId, kfSrvId, fubenSeq, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), roleNum);
            ExecuteSqlNoQuery(sql);
        }

        /// <summary>
        /// 保存城池数据
        /// </summary>
        /// <param name="cityDataEx"></param>
        public void SaveCityData(LangHunLingYuCityDataEx cityDataEx)
        {
            ExecuteSqlNoQuery(string.Format("INSERT INTO t_lhly_city(cityid,citylevel,owner,attacker1,attacker2,attacker3)" + 
                " VALUES({0},{1},{2},{3},{4},{5})" +
                " ON DUPLICATE KEY UPDATE citylevel={1},owner={2},attacker1={3},attacker2={4},attacker3={5};"
                , cityDataEx.CityId, cityDataEx.CityLevel, cityDataEx.Site[0], cityDataEx.Site[1], cityDataEx.Site[2], cityDataEx.Site[3]));
        }

        /// <summary>
        /// 保存帮会数据
        /// </summary>
        /// <param name="bangHuiDataEx"></param>
        public void SaveBangHuiData(LangHunLingYuBangHuiDataEx bangHuiDataEx)
        {
            ExecuteSqlNoQuery(string.Format("INSERT INTO t_lhly_banghui(bhid,bhname,zoneid,level) VALUES({0},'{1}',{2},{3}) ON DUPLICATE KEY UPDATE bhname='{1}',zoneid={2},level={3};",
                bangHuiDataEx.Bhid, bangHuiDataEx.BhName, bangHuiDataEx.ZoneId, bangHuiDataEx.Level));
        }

        /// <summary>
        /// 加载历史圣域城主历史记录数据
        /// </summary>
        /// <param name="CityOwnerHist"></param>
        /// <returns></returns>
        public bool LoadCityOwnerHistory(List<LangHunLingYuKingHist> LHLYCityOwnerList)
        {
            MySqlDataReader sdr = null;
            string strSql;

            try
            {
                strSql = string.Format("SELECT * FROM `t_lhly_hist`");
                sdr = DbHelperMySQL.ExecuteReader(strSql);
                for (int index = 1; sdr.Read(); index++)
                {
                    LangHunLingYuKingHist OwnerData = new LangHunLingYuKingHist();
                    OwnerData.rid = Convert.ToInt32(sdr["role_id"]);
                    OwnerData.AdmireCount = Convert.ToInt32(sdr["admire_count"]);
                    OwnerData.CompleteTime = DateTime.Parse(sdr["time"].ToString());
                    if (!sdr.IsDBNull(sdr.GetOrdinal("data"))) OwnerData.CityOwnerRoleData = (byte[])(sdr["data"]);
                    LHLYCityOwnerList.Add(OwnerData);
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

        /// <summary>
        /// 添加圣域城主历史记录数据
        /// </summary>
        /// <param name="CityOwnerData"></param>
        public void InsertCityOwnerHistory(LangHunLingYuKingHist CityOwnerData)
        {
            try
            {
                string sql = string.Format("INSERT INTO t_lhly_hist(role_id, admire_count, time, data) VALUES({0}, {1}, '{2}', @content)", 
                    CityOwnerData.rid, CityOwnerData.AdmireCount, CityOwnerData.CompleteTime.ToString());
                List<Tuple<string, byte[]>> imgList = new List<Tuple<string, byte[]>>();
                imgList.Add(new Tuple<string, byte[]>("content", CityOwnerData.CityOwnerRoleData));
                DbHelperMySQL.ExecuteSqlInsertImg(sql, imgList);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        /// <summary>
        /// 圣域城主膜拜
        /// </summary>
        public void AdmireCityOwner(int rid)
        {
            try
            {
                string sql = string.Format("UPDATE t_lhly_hist SET admire_count=admire_count+1 WHERE role_id={0}", rid);
                DbHelperMySQL.ExecuteSql(sql);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        /// <summary>
        /// 加载帮会数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool LoadBangHuiDataExList(List<LangHunLingYuBangHuiDataEx> list)
        {
            MySqlDataReader sdr = null;
            string strSql;

            try
            {
                strSql = string.Format("SELECT * FROM `t_lhly_banghui`;");
                sdr = DbHelperMySQL.ExecuteReader(strSql);
                for (int index = 1; sdr.Read(); index++)
                {
                    LangHunLingYuBangHuiDataEx data = new LangHunLingYuBangHuiDataEx();
                    data.Bhid = Convert.ToInt32(sdr["bhid"]);
                    data.ZoneId = Convert.ToInt32(sdr["zoneid"]);
                    data.BhName = sdr["bhname"].ToString();
                    //data.Level = Convert.ToInt32(sdr["level"].ToString());
                    list.Add(data);
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

        /// <summary>
        /// 加载城池数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool LoadCityDataExList(List<LangHunLingYuCityDataEx> list)
        {
            MySqlDataReader sdr = null;
            string strSql;

            try
            {
                strSql = string.Format("SELECT * FROM `t_lhly_city`;");
                sdr = DbHelperMySQL.ExecuteReader(strSql);
                for (int index = 1; sdr.Read(); index++)
                {
                    LangHunLingYuCityDataEx data = new LangHunLingYuCityDataEx();
                    data.CityId = Convert.ToInt32(sdr["cityid"]);
                    data.CityLevel = Convert.ToInt32(sdr["citylevel"]);
                    data.Site[0] = Convert.ToInt32(sdr["owner"].ToString());
                    data.Site[1] = Convert.ToInt32(sdr["attacker1"].ToString());
                    data.Site[2] = Convert.ToInt32(sdr["attacker2"].ToString());
                    data.Site[3] = Convert.ToInt32(sdr["attacker3"].ToString());
                    list.Add(data);
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
    }
}
