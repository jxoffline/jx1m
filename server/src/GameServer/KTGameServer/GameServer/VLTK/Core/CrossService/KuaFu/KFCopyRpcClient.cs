using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Tmsk.Contract;
using KF.Contract.Data;
using KF.Contract;
using KF.Contract.Interface;
using Tmsk.Tools;
using Server.Tools;
using Tmsk.Contract.Const;
using Tmsk.Contract.Data;

namespace KF.Client
{
    public class KFCopyRpcClient : MarshalByRefObject, IKuaFuClient, IManager2
    {
        #region Singleton
        private static readonly KFCopyRpcClient instance = new KFCopyRpcClient();

        public static KFCopyRpcClient getInstance()
        {
            return instance;
        }
        #endregion

        #region Member
        /// <summary>
        /// lock对象
        /// </summary>
        object Mutex = new object();

        object RemotingMutex = new object();

        /// <summary>
        /// 当前并发数
        /// </summary>
        private int CurrentRequestCount = 0;

        /// <summary>
        /// 最大并发数
        /// </summary>
        private const int MaxRequestCount = 50;

        /// <summary>
        /// 角色ID到跨服角色信息的字典
        /// </summary>
        Dictionary<int, KuaFuRoleData> RoleId2RoleDataDict = new Dictionary<int, KuaFuRoleData>();

        /// <summary>
        /// 角色ID到跨服状态的缓存字典
        /// </summary>
        Dictionary<int, int> RoleId2KuaFuStateDict = new Dictionary<int, int>();

        /// <summary>
        /// 获取的服务器信息的年龄
        /// </summary>
        private int ServerInfoAsyncAge = 0;

        /// <summary>
        /// 服务器信息
        /// </summary>
        private Dictionary<int, KuaFuServerInfo> ServerIdServerInfoDict = new Dictionary<int, KuaFuServerInfo>();

        /// <summary>
        /// 本地服务器信息
        /// </summary>
        private KuaFuClientContext ClientInfo = new KuaFuClientContext();

        /// <summary>
        /// 服务地址
        /// </summary>
        private string RemoteServiceUri = null;

        /// <summary>
        /// 跨服中心服务对象
        /// </summary>
       private  IKuaFuCopyService KuaFuService = null;

        /// <summary>
        /// 游戏管理接口
        /// </summary>
        private ICoreInterface CoreInterface = null;

        /// <summary>
        /// 服务器标志:0 仅区服务器,1 跨服服务器,2 区服务器,3 前两者均可
        /// </summary>
        private int LocalServerFlags = 0; //默认0

        /// <summary>
        /// 特殊场景类型
        /// </summary>
        public const int SceneType = (int)SceneUIClasses.KuaFuCopy;
        #endregion

        #region Implement IManager2
        public bool initialize(ICoreInterface coreInterface)
        {
            CoreInterface = coreInterface;
            ClientInfo.ServerId = CoreInterface.GetLocalServerId();
            ClientInfo.GameType = (int)GameTypes.KuaFuCopy;
            return true;
        }

        public bool startup()
        {
            return true;
        }

        public bool showdown()
        {
            return true;
        }

        public bool destroy()
        {
            return true;
        }
        #endregion

        #region Implement IKuaFuClient
        public void EventCallBackHandler(int eventType, params object[] args)
        {
            try
            {
                switch (eventType)
                {
                    case (int)KuaFuEventTypes.KFCopyTeamCreate: // 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomCreateEvent((CopyTeamCreateData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamJoin:// 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomJoinEvent((CopyTeamJoinData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamKickout:// 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomKickoutEvent((CopyTeamKickoutData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamLeave:// 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomLeaveEvent((CopyTeamLeaveData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamSetReady:// 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomReadyEvent((CopyTeamReadyData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamStart:// 只处理其他服异步转发的消息
                        {
                            if (args != null && args.Length == 2 && (int)args[0] != ClientInfo.ServerId)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyRoomStartEvent((CopyTeamStartData)args[1]), SceneType);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.KFCopyTeamDestroty:
                        {
                            if (args != null && args.Length == 1)
                            {
                                CoreInterface.GetEventSourceInterface().fireEvent(
                                    new KFCopyTeamDestroyEvent((CopyTeamDestroyData)args[0]), SceneType);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }
        public object GetDataFromClientServer(int dataType, params object[] args)
        {
            return null;
        }
        public int GetNewFuBenSeqId()
       {
           if (null != CoreInterface)
           {
               return CoreInterface.GetNewFuBenSeqId();
           }

           return StdErrorCode.Error_Operation_Faild;
       }
        public int UpdateRoleData(KuaFuRoleData kuaFuRoleData, int roleId = 0)
        {
            int result = (int)KuaFuRoleStates.None;
            if (kuaFuRoleData == null)
            {
                return result;
            }

            roleId = kuaFuRoleData.RoleId;
            lock (Mutex)
            {
                if (kuaFuRoleData.State == KuaFuRoleStates.None)
                {
                    RemoveRoleData(kuaFuRoleData.RoleId);
                    return (int)KuaFuRoleStates.None;
                }

                RoleId2RoleDataDict[roleId] = kuaFuRoleData;
                RoleId2KuaFuStateDict[roleId] = (int)kuaFuRoleData.State;
            }

            return result;
        }

        public int OnRoleChangeState(int roleId, int state, int age)
        {
            lock (Mutex)
            {
                KuaFuRoleData kuaFuRoleData;
                if (!RoleId2RoleDataDict.TryGetValue(roleId, out kuaFuRoleData))
                {
                    return -1;
                }

                if (age > kuaFuRoleData.Age)
                {
                    kuaFuRoleData.State = (KuaFuRoleStates)state;
                }
            }

            return 0;
        }

        /// <summary>
        /// 移除指定ID的角色的缓存
        /// </summary>
        /// <param name="roleId"></param>
        public void RemoveRoleData(int roleId)
        {
            lock (Mutex)
            {
                RoleId2RoleDataDict.Remove(roleId);
                RoleId2KuaFuStateDict.Remove(roleId);
            }
        }

        #endregion

        public bool CanKuaFuLogin()
        {
            if (LocalServerFlags == ServerFlags.NormalServerOnly)
            {
                return false;
            }

            if ((ServerFlags.KuaFuServer & LocalServerFlags) != 0)
            {
                return true;
            }

            return false;
        }

        public void TimerProc(object sender, EventArgs e)
        {
            try
            {
                string moRiJudgeUri = CoreInterface.GetRuntimeVariable(RuntimeVariableNames.KuaFuCopyUri, null);
                if (RemoteServiceUri != moRiJudgeUri)
                {
                    RemoteServiceUri = moRiJudgeUri;
                }

                IKuaFuCopyService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    if (ClientInfo.ClientId > 0)
                    {
                        List<KuaFuServerInfo> dict = kuaFuService.GetKuaFuServerInfoData(ServerInfoAsyncAge);
                        if (null != dict && dict.Count > 0)
                        {
                            lock (Mutex)
                            {
                                ServerIdServerInfoDict.Clear();
                                bool first = true;
                                foreach (var item in dict)
                                {
                                    ServerIdServerInfoDict[item.ServerId] = item;
                                    if (first)
                                    {
                                        first = false;
                                        ServerInfoAsyncAge = item.Age;
                                    }
                                    if (ClientInfo.ServerId == item.ServerId)
                                    {
                                        LocalServerFlags = item.Flags;
                                    }
                                }
                            }
                        }

                        //同步数据
                        AsyncDataItem[] items = kuaFuService.GetClientCacheItems(ClientInfo.ServerId);
                        if (null != items && items.Length > 0)
                        {
                            foreach (var item in items)
                            {
                                EventCallBackHandler((int)item.EventType, item.Args);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "KFCopyRpcClient.TimerProc调度异常", ex);
                ResetKuaFuService();
            }
        }

        #region KuaFuService
        private void ResetKuaFuService()
        {
            RemoteServiceUri = CoreInterface.GetRuntimeVariable(RuntimeVariableNames.KuaFuCopyUri, null);
            lock (Mutex)
            {
                KuaFuService = null;
            }
        }

        private IKuaFuCopyService GetKuaFuService(bool noWait = false)
        {
            try
            {
                lock (Mutex)
                {
                    if (string.IsNullOrEmpty(RemoteServiceUri))
                    {
                        return null;
                    }

                    if (null == KuaFuService && noWait)
                    {
                        return null;
                    }
                }

                IKuaFuCopyService tmpKuaFuService = null;
                lock (RemotingMutex)
                {
                    if (KuaFuService == null)
                    {
                        tmpKuaFuService = (IKuaFuCopyService)Activator.GetObject(typeof(IKuaFuCopyService), RemoteServiceUri);
                        if (null == tmpKuaFuService)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        tmpKuaFuService = KuaFuService;
                    }

                    int clientId = tmpKuaFuService.InitializeClient(this, ClientInfo);
                    if (null != tmpKuaFuService && (clientId != ClientInfo.ClientId || KuaFuService != tmpKuaFuService))
                    {
                        lock (Mutex)
                        {
                            KuaFuService = tmpKuaFuService;
                            ClientInfo.ClientId = clientId;
                            return tmpKuaFuService;
                        }
                    }

                    return KuaFuService;
                }
            }
            catch (System.Exception ex)
            {
                ResetKuaFuService();
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }
        #endregion

        public bool GetKuaFuGSInfo(int serverId, out string gsIp, out int gsPort)
        {
            gsIp = string.Empty;
            gsPort = 0;
            KuaFuServerInfo info = null;
            lock (Mutex)
            {
                if (!ServerIdServerInfoDict.TryGetValue(serverId, out info))
                {
                    return false;
                }

                gsIp = info.Ip;
                gsPort = info.Port;
                return true;
            }
        }

        public bool GetKuaFuDbServerInfo(int serverId, out string dbIp, out int dbPort, out string logIp, out int logPort)
        {
            KuaFuServerInfo kuaFuServerInfo;
            lock (Mutex)
            {
                if (ServerIdServerInfoDict.TryGetValue(serverId, out kuaFuServerInfo))
                {
                    dbIp = kuaFuServerInfo.DbIp;
                    dbPort = kuaFuServerInfo.DbPort;
                    logIp = kuaFuServerInfo.LogDbIp;
                    logPort = kuaFuServerInfo.LogDbPort;
                    return true;
                }
            }

            dbIp = null;
            dbPort = 0;
            logIp = null;
            logPort = 0;
            return false;
        }     

        #region 包装RPC调用
        public KFCopyTeamCreateRsp CreateTeam(KFCopyTeamCreateReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.CreateTeam(req);
        }

        public KFCopyTeamJoinRsp JoinTeam(KFCopyTeamJoinReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.JoinTeam(req);
        }
        public KFCopyTeamStartRsp StartGame(KFCopyTeamStartReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.StartGame(req);
        }

        public KFCopyTeamKickoutRsp KickoutTeam(KFCopyTeamKickoutReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.KickoutTeam(req);
        }

        public KFCopyTeamLeaveRsp LeaveTeam(KFCopyTeamLeaveReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.LeaveTeam(req);
        }

        public KFCopyTeamSetReadyRsp SetReady(KFCopyTeamSetReadyReq req)
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            return service.TeamSetReady(req);
        }

        /// <summary>
        /// 跨服活动服务器接到分给自己了一个副本的消息时，从中心取一下最新的队伍信息
        /// 并且向中心发回确认，我已经准备好了，请玩家切过来吧
        /// </summary>
        public CopyTeamData GetTeamData(long teamid)
        {
            IKuaFuCopyService service = GetKuaFuService();
            if (service == null) return null;

            return service.GetTeamData(teamid);
        }

        public void KFCopyTeamRemove(long teamId)
        {
            IKuaFuCopyService service = GetKuaFuService();
            if (service == null) return;

            service.RemoveTeam(teamId);
        }
        #endregion

        public InputKingPaiHangDataEx GetPlatChargeKing()
        {
            IKuaFuCopyService service = GetKuaFuService(true);
            if (service == null) return null;

            object obj = service.GetPlatChargeKing();
            return obj != null ? obj as InputKingPaiHangDataEx : null;
        }
    }
}
