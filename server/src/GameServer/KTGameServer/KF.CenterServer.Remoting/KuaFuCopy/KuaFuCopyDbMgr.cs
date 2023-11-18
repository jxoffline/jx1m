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
    /// 末日审判持久化管理
    /// </summary>
    public class KuaFuCopyDbMgr
    {
        #region Singletion
        private KuaFuCopyDbMgr() { }
        private static readonly KuaFuCopyDbMgr g_Instance = new KuaFuCopyDbMgr();
        public static KuaFuCopyDbMgr Instance { get { return g_Instance; } }
        #endregion

        #region 配置信息

        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// 队伍和副本数量的控制
        /// </summary>
        private KFTeamCountControl _Control = null;
        private object _ControlMutex = new object();
        public KFTeamCountControl TeamControl
        {
            get { lock (_ControlMutex) return _Control; }
            private set { lock (_ControlMutex) _Control = value; }
        }

        #endregion 配置信息

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {
                KFTeamCountControl control = new KFTeamCountControl();

                // 创建队伍后，最长等待开始时间
                XElement xmlFile = ConfigHelper.Load("config.xml");
                control.TeamMaxWaitMinutes = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "FuBenTeamMaxWaitMinutes", "value", 10);

                this.TeamControl = control;
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
                    LogManager.WriteLog(LogTypes.Warning, "KFCopyTeam 执行时间(ms):" + ms);
                }
            }
            catch
            {

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

        /// <summary>
        /// 检查事件是否需要记录日志
        /// 队伍创建，开始，结束等
        /// </summary>
        /// <param name="evList"></param>
        public void CheckLogAsyncEvents(AsyncDataItem[] evList)
        {
            if (evList == null) return;

            foreach (var ev in evList)
            {
                string sql = string.Empty;
                try
                {
                    if (ev.EventType == KuaFuEventTypes.KFCopyTeamCreate)
                    {
                        // 副本创建的事件
                        CopyTeamCreateData data = ev.Args[1] as CopyTeamCreateData;
                        sql = string.Format("INSERT INTO t_kuafu_fuben_game_team(teamid,copyid,by_serverid,by_roleid,createtime) VALUES({0},{1},{2},{3},'{4}')",
                            data.TeamId, data.CopyId, data.Member.ServerId, data.Member.RoleID, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else if (ev.EventType == KuaFuEventTypes.KFCopyTeamStart)
                    {
                        // 玩家点击开始的事件
                        CopyTeamStartData data = ev.Args[1] as CopyTeamStartData;
                        sql = string.Format("UPDATE t_kuafu_fuben_game_team SET starttime='{0}', kf_serverid={1} WHERE teamid={2}",
                            TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), data.ToServerId, data.TeamId);
                    }
                    else if (ev.EventType == KuaFuEventTypes.KFCopyTeamDestroty)
                    {
                        // 跨服副本队伍清除的事件
                        CopyTeamDestroyData data = ev.Args[0] as CopyTeamDestroyData;
                        sql = string.Format("UPDATE t_kuafu_fuben_game_team SET endtime='{0}' WHERE teamid={1}",
                            TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), data.TeamId);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Error, "KuaFuCopyDbMgr.CheckLogAsyncEvents Failed!!!", ex);
                }
                
                if (!string.IsNullOrEmpty(sql))
                {
                    ExecuteSqlNoQuery(sql);
                }
            }
        }

        /// <summary>
        /// 存储副本统计信息
        /// </summary>
        /// <param name="data"></param>
        public void SaveCopyTeamAnalysisData(KFCopyTeamAnalysis data)
        {
            if (data == null) return;

            const string FormatSql =
                "REPLACE INTO t_kuafu_fuben_game_log(fuben_id,total_fuben_count,start_fuben_count,unstart_fuben_count,total_role_count,start_role_count,unstart_role_count,time) "
                 + " VALUES({0},{1},{2},{3},{4},{5},{6},'{7}') ";

            string nowTime = TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss");
            KFCopyTeamAnalysis.Item totalItem = new KFCopyTeamAnalysis.Item();
            foreach (var kvp in data.AnalysisDict)
            {
                int copyId = kvp.Key;
                KFCopyTeamAnalysis.Item item = kvp.Value;

                totalItem.TotalCopyCount += item.TotalCopyCount;
                totalItem.StartCopyCount += item.StartCopyCount;
                totalItem.UnStartCopyCount += item.UnStartCopyCount;
                totalItem.TotalRoleCount += item.TotalRoleCount;
                totalItem.StartRoleCount += item.StartRoleCount;
                totalItem.UnStartRoleCount += item.UnStartRoleCount;

                // 存储每个副本的负载信息
                string sql = string.Format(
                    FormatSql,
                    copyId, item.TotalCopyCount, item.StartCopyCount, item.UnStartCopyCount, item.TotalRoleCount, item.StartRoleCount, item.UnStartRoleCount, nowTime);

                ExecuteSqlNoQuery(sql);
            }

            // 存储所有副本的负载信息，fubenid == -1
            string totalSql = string.Format(
                FormatSql,
                -1, totalItem.TotalCopyCount, totalItem.StartCopyCount, totalItem.UnStartCopyCount, totalItem.TotalRoleCount, totalItem.StartRoleCount, totalItem.UnStartRoleCount, nowTime);
            ExecuteSqlNoQuery(totalSql);

            string deleteSql = string.Format("DELETE FROM t_kuafu_fuben_game_log WHERE time != '{0}'", nowTime);
            ExecuteSqlNoQuery(deleteSql);
        }
    }
}
