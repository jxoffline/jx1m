using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Data;
using KF.Contract.Interface;
using System.Threading;
using Tmsk.Contract;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using KF.Contract;
using Maticsoft.DBUtility;
using MySql.Data.MySqlClient;
using Tmsk.Tools;
using System.Configuration;
using KF.Remoting.Data;
using System.Collections.Concurrent;
using System.Collections;
using Tmsk.Contract.Const;
using Server.Tools;
using GameServer.Core.Executor;

namespace KF.Remoting
{
    /// <summary>
    /// 跨服副本Remoting服务对象
    /// </summary>
    public partial class KuaFuCopyService : MarshalByRefObject, IKuaFuCopyService
    {
        public static KuaFuCopyService Instance = null;

        /// <summary>
        /// 跨服副本活动配置数据
        /// </summary>
        public readonly GameTypes GameType = GameTypes.KuaFuCopy;

        /// <summary>
        /// 跨服副本队伍管理
        /// </summary>
        private KFCopyTeamManager teamMgr = new KFCopyTeamManager();

        /// <summary>
        /// 上次存储服务器状态的时间
        /// </summary>
        private long LastSaveServerStateMs = 0;

        /// <summary>
        /// 跨服副本持久化
        /// </summary>
        public KuaFuCopyDbMgr dbMgr = KuaFuCopyDbMgr.Instance;

        public Thread BackgroundThread;
        public Thread PlatChargeKingThread;

        #region 定时处理

        #region 扩展部分 平台充值王
        private PlatChargeKingManager PCKMgr = new PlatChargeKingManager();
        public object GetPlatChargeKing()
        {
            return PCKMgr.GetRankEx();
        }
        #endregion

        /// <summary>
        /// 生存期控制
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            Instance = this;

            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromDays(2000);
                //lease.RenewOnCallTime = TimeSpan.FromHours(20);
                //lease.SponsorshipTimeout = TimeSpan.FromHours(20);
            }

            return lease;
        }

        public KuaFuCopyService()
        {
            BackgroundThread = new Thread(ThreadProc);
            BackgroundThread.IsBackground = true;
            BackgroundThread.Start();

            PlatChargeKingThread = new Thread(() => {
                for (; ; )
                {
                    try
                    {
                        // 平台充值王
                        PCKMgr.Update();
                        Thread.Sleep(20000);
                    }
                    catch (System.Exception ex)
                    {
                        LogManager.WriteExceptionUseCache(ex.ToString());
                    }
                }
            });
            PlatChargeKingThread.IsBackground = true;
            PlatChargeKingThread.Start();

            // 找不到好的地方去设置this了，先放这里吧，一般来说构造函数中最好不要把this外传
            teamMgr.SetService(this);
        }

        ~KuaFuCopyService()
        {
            BackgroundThread.Abort();
            PlatChargeKingThread.Abort();
        }

        public void RemoveGameTeam(int kfSrvId, long teamId)
        {
            ClientAgentManager.Instance().RemoveKfFuben(GameType, kfSrvId, teamId);
        }

        private void ThreadProc(object state)
        {
            dbMgr.InitConfig();

            do
            {
                try
                {
                    long nowMs = TimeUtil.NOW();
                    DateTime now = DateTime.Now;
                    Global.UpdateNowTime(now);

                    // 每个30秒存储服务器状态
                    const int SaveServerStateIntervalMs = 30 * 1000;
                    if (nowMs >= LastSaveServerStateMs + SaveServerStateIntervalMs)
                    {
                        LastSaveServerStateMs = nowMs;
                        dbMgr.SaveCopyTeamAnalysisData(teamMgr.BuildAnalysisData());
                    }

                    // 队伍管理器(处理超时关闭)
                    teamMgr.Update();
                    AsyncDataItem[] evList = teamMgr.PopAsyncEvent();
                    ClientAgentManager.Instance().BroadCastAsyncEvent(GameType, evList);
                    dbMgr.CheckLogAsyncEvents(evList);

                    int sleepMS = (int)((DateTime.Now - now).TotalMilliseconds);
                    dbMgr.SaveCostTime(sleepMS);
                    sleepMS = 1600 - sleepMS; //最大睡眠1600ms,最少睡眠50ms
                    if (sleepMS < 50)
                    {
                        sleepMS = 50;
                    }

                    Thread.Sleep(sleepMS);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            } while (true);
        }

        #endregion 定时处理

        #region 接口实现
        public AsyncDataItem[] GetClientCacheItems(int serverId)
        {
            return ClientAgentManager.Instance().PickAsyncEvent(serverId, GameType);
        }

        public KFCopyTeamCreateRsp CreateTeam(KFCopyTeamCreateReq req)
        {
            return teamMgr.CreateTeam(req);
        }

        public KFCopyTeamJoinRsp JoinTeam(KFCopyTeamJoinReq req)
        {
            return teamMgr.JoinTeam(req);
        }

        public KFCopyTeamKickoutRsp KickoutTeam(KFCopyTeamKickoutReq req)
        {
            return teamMgr.KickoutTeam(req);
        }

        public KFCopyTeamLeaveRsp LeaveTeam(KFCopyTeamLeaveReq req)
        {
            return teamMgr.LeaveTeam(req);
        }
        public KFCopyTeamStartRsp StartGame(KFCopyTeamStartReq req)
        {
            return teamMgr.StartGame(req);
        }
        public KFCopyTeamSetReadyRsp TeamSetReady(KFCopyTeamSetReadyReq req)
        {
            return teamMgr.TeamSetReady(req);
        }

        /// <summary>
        /// 跨服活动服务器接到分给自己了一个副本的消息时，从中心取一下最新的队伍信息
        /// 并且向中心发回确认，我已经准备好了，请玩家切过来吧
        /// </summary>
        public CopyTeamData GetTeamData(long teamid)
        {
            return teamMgr.GetTeamData(teamid);
        }


        public void RemoveTeam(long teamId)
        {
            teamMgr.RemoveTeam(teamId);
        }

        // 初始化跨服客户端回调对象 <returns>clientId</returns>
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo)
        {
            try
            {
                if (clientInfo.GameType == (int)GameType && clientInfo.ServerId != 0)
                {
                    return ClientAgentManager.Instance().InitializeClient(callback, clientInfo);
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Warning, string.Format("InitializeClient时GameType错误,禁止连接.ServerId:{0},GameType:{1}", clientInfo.ServerId, clientInfo.GameType));
                    return StdErrorCode.Error_Invalid_GameType;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(string.Format("InitializeClient服务器ID重复,禁止连接.ServerId:{0},ClientId:{1}", clientInfo.ServerId, clientInfo.ClientId));
                return StdErrorCode.Error_Server_Internal_Error;
            }
        }

        public List<KuaFuServerInfo> GetKuaFuServerInfoData(int age)
        {
            return KuaFuServerManager.GetKuaFuServerInfoData(age);
        }  

        #endregion 接口实现
    }
}
