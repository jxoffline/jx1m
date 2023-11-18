using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Interface;
using KF.Contract.Data;
using Tmsk.Contract;
using System.Globalization;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using KF.Contract;
using System.Diagnostics;
using System.Configuration;
using Tmsk.Tools;
using Tmsk.Contract.Const;
using Server.Tools;
using GameServer.Core.Executor;

namespace KF.Client
{
    public class YongZheZhanChangClient : MarshalByRefObject, IKuaFuClient, IManager2
    {
        #region Interface

        private static YongZheZhanChangClient instance = new YongZheZhanChangClient();

        public static YongZheZhanChangClient getInstance()
        {
            return instance;
        }

        public bool initialize()
        {
            return true;
        }

        public bool initialize(ICoreInterface coreInterface)
        {
            CoreInterface = coreInterface;
            ClientInfo.ServerId = CoreInterface.GetLocalServerId();
            ClientInfo.GameType = (int)GameTypes.YongZheZhanChang;
            ClientInfo.Token = CoreInterface.GetLocalAddressIPs();
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

        #endregion 标准接口

        #region 运行时成员

        /// <summary>
        /// lock对象
        /// </summary>
        object Mutex = new object();

        object RemotingMutex = new object();

        /// <summary>
        /// 游戏管理接口
        /// </summary>
        ICoreInterface CoreInterface = null;

        /// <summary>
        /// 跨服中心服务对象
        /// </summary>
        IYongZheZhanChangService KuaFuService = null;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool ClientInitialized = false;

        /// <summary>
        /// 本地服务器信息
        /// </summary>
        private KuaFuClientContext ClientInfo = new KuaFuClientContext();

        /// <summary>
        /// 特殊场景类型
        /// </summary>
        public int SceneType = (int)SceneUIClasses.YongZheZhanChang;

        /// <summary>
        /// 当前并发数
        /// </summary>
        private int CurrentRequestCount = 0;

        /// <summary>
        /// 最大并发数
        /// </summary>
        private int MaxRequestCount = 50;

        /// <summary>
        /// 角色ID到跨服角色信息的字典
        /// </summary>
        Dictionary<int, KuaFuRoleData> RoleId2RoleDataDict = new Dictionary<int, KuaFuRoleData>();

        /// <summary>
        /// 角色ID到跨服状态的缓存字典
        /// </summary>
        Dictionary<int, int> RoleId2KuaFuStateDict = new Dictionary<int, int>();

        /// <summary>
        /// 缓存的副本信息
        /// </summary>
        Dictionary<int, YongZheZhanChangFuBenData> FuBenDataDict = new Dictionary<int, YongZheZhanChangFuBenData>();

        /// <summary>
        /// 缓存的副本信息
        /// </summary>
        Dictionary<int, LangHunLingYuFuBenData> LangHunLingYuFuBenDataDict = new Dictionary<int, LangHunLingYuFuBenData>();

        /// <summary>
        /// 上次清除副本的时间
        /// </summary>
        private DateTime NextClearFuBenTime;

        /// <summary>
        /// 获取的服务器信息的年龄
        /// </summary>
        private int ServerInfoAsyncAge = 0;

        /// <summary>
        /// 服务器信息
        /// </summary>
        Dictionary<int, KuaFuServerInfo> ServerIdServerInfoDict = new Dictionary<int, KuaFuServerInfo>();

        /// <summary>
        /// 服务器标志:0 仅区服务器,1 跨服服务器,2 区服务器,3 前两者均可
        /// </summary>
        private int LocalServerFlags = 0; //默认0

        /// <summary>
        /// 服务地址
        /// </summary>
        private string RemoteServiceUri = null;

        #endregion 运行时成员

        #region 内部函数

        public bool LocalLogin(string userId)
        {
            if (LocalServerFlags == ServerFlags.NormalServerOnly)
            {
                return true;
            }

            if ((ServerFlags.NormalServer & LocalServerFlags) != 0)
            {
                return true;
            }

            return true;
        }

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

        public void ExecuteEventCallBackAsync(object state)
        {
            AsyncDataItem[] items = state as AsyncDataItem[];
            if (null != items && items.Length > 0)
            {
                foreach (var item in items)
                {
                    EventCallBackHandler((int)item.EventType, item.Args);
                }
            }
        }

        /// <summary>
        /// 添加跨服地图的地图编号
        /// </summary>
        /// <param name="mapCode"></param>
        public void UpdateKuaFuMapClientCount(Dictionary<int, int> dict)
        {
            lock (Mutex)
            {
                ClientInfo.MapClientCountDict = dict;
            }
        }

        public void TimerProc(object sender, EventArgs e)
        {
            try
            {
                DateTime now = TimeUtil.NowDateTime();
                if (NextClearFuBenTime < now)
                {
                    NextClearFuBenTime = now.AddHours(1);
                    ClearOverTimeFuBen(now);
                }
                
                string YongZheZhanChangUri = CoreInterface.GetRuntimeVariable(RuntimeVariableNames.YongZheZhanChangUri, null);
                if (RemoteServiceUri != YongZheZhanChangUri)
                {
                    RemoteServiceUri = YongZheZhanChangUri;
                }

                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    if (ClientInfo.ClientId > 0)
                    {
                        List<KuaFuServerInfo> dict = kuaFuService.GetKuaFuServerInfoData(ServerInfoAsyncAge);
                        if (null != dict && dict.Count > 0)
                        {
                            lock(Mutex)
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
                            //ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteEventCallBackAsync), items);
                            ExecuteEventCallBackAsync(items);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ResetKuaFuService();
            }
        }

        private void ResetKuaFuService()
        {
            RemoteServiceUri = CoreInterface.GetRuntimeVariable("YongZheZhanChangUri", null);
            lock (Mutex)
            {
                KuaFuService = null;
            }
        }

        private IYongZheZhanChangService GetKuaFuService(bool noWait = false)
        {
            IYongZheZhanChangService kuaFuService = null;
            int clientId = -1;

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

                lock (RemotingMutex)
                {
                    if (KuaFuService == null)
                    {
                        kuaFuService = (IYongZheZhanChangService)Activator.GetObject(typeof(IYongZheZhanChangService), RemoteServiceUri);
                        if (null == kuaFuService)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        kuaFuService = KuaFuService;
                    }

               

                    clientId = kuaFuService.InitializeClient(this, ClientInfo);

                    if (null != kuaFuService && (clientId != ClientInfo.ClientId || KuaFuService != kuaFuService))
                    {
                        lock (Mutex)
                        {
                            if (clientId > 0)
                            {
                                KuaFuService = kuaFuService;
                            }
                            else
                            {
                                KuaFuService = null;
                            }

                            ClientInfo.ClientId = clientId;
                            return KuaFuService;
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

        /// <summary>
        /// 服务器通知添加一个角色信息
        /// </summary>
        /// <param name="kuaFuRoleData"></param>
        /// <returns></returns>
        public int UpdateRoleData(KuaFuRoleData kuaFuRoleData, int roleId = 0)
        {
            int result = (int)KuaFuRoleStates.None;
            if (kuaFuRoleData == null)
            {
                return result;
            }

            roleId = kuaFuRoleData.RoleId;
            KuaFuRoleData oldKuaFuRoleData, newKuaFuRoleData;
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

        /// <summary>
        /// 角色状态修改
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="gameType"></param>
        /// <param name="groupIndex"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int RoleChangeState(int serverId, int rid, int state)
        {
            int result = StdErrorCode.Error_Operation_Faild;

            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    result = kuaFuService.RoleChangeState(serverId, rid, state);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }

            return result;
        }

        /// <summary>
        /// 游戏副本状态变更
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="state"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public int GameFuBenChangeState(int gameId, GameFuBenState state, DateTime time)
        {
            int result = StdErrorCode.Error_Server_Busy;
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    YongZheZhanChangFuBenData yongZheZhanChangFuBenData;
                    lock (Mutex)
                    {
                        if (FuBenDataDict.TryGetValue(gameId, out yongZheZhanChangFuBenData))
                        {
                            yongZheZhanChangFuBenData.State = state;
                        }
                    }

                    result = kuaFuService.GameFuBenChangeState(gameId, state, time);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                    result = StdErrorCode.Error_Server_Internal_Error;
                }
            }

            return result;
        }

        #endregion 内部函数

        #region 回调函数

        public int GetNewFuBenSeqId()
        {
            if (null != CoreInterface)
            {
                return CoreInterface.GetNewFuBenSeqId();
            }

            return StdErrorCode.Error_Operation_Faild;
        }

        public object GetDataFromClientServer(int dataType, params object[] args)
        {
            return null;
        }

        public void EventCallBackHandler(int eventType, params object[] args)
        {
            try
            {
                switch (eventType)
                {
                    case (int)KuaFuEventTypes.RoleSignUp:
                    case (int)KuaFuEventTypes.RoleStateChange:
                        {
                            if (args.Length == 1)
                            {
                                KuaFuRoleData kuaFuRoleData = args[0] as KuaFuRoleData;
                                if (null != kuaFuRoleData)
                                {
                                    UpdateRoleData(kuaFuRoleData, kuaFuRoleData.RoleId);
                                }
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateAndNotifyEnterGame:
                        {
                            if (args.Length == 1)
                            {
                                KuaFuRoleData kuaFuRoleData = args[0] as KuaFuRoleData;
                                if (null != kuaFuRoleData)
                                {
                                    UpdateRoleData(kuaFuRoleData, kuaFuRoleData.RoleId);

                                    YongZheZhanChangFuBenData fuBenData = GetKuaFuFuBenData(kuaFuRoleData.GameId);
                                    if (null != fuBenData && fuBenData.State == GameFuBenState.Start)
                                    {
                                        KuaFuServerLoginData kuaFuServerLoginData = new KuaFuServerLoginData()
                                        {
                                            RoleId = kuaFuRoleData.RoleId,
                                            GameType = kuaFuRoleData.GameType,
                                            GameId = kuaFuRoleData.GameId,
                                            EndTicks = kuaFuRoleData.StateEndTicks,
                                        };

                                        kuaFuServerLoginData.ServerId = ClientInfo.ServerId;
                                        lock (Mutex)
                                        {
                                            KuaFuServerInfo kuaFuServerInfo;
                                            if (ServerIdServerInfoDict.TryGetValue(fuBenData.ServerId, out kuaFuServerInfo))
                                            {
                                                kuaFuServerLoginData.ServerIp = kuaFuServerInfo.Ip;
                                                kuaFuServerLoginData.ServerPort = kuaFuServerInfo.Port;
                                            }
                                        }

                                        switch ((GameTypes)kuaFuRoleData.GameType)
                                        {
                                            case GameTypes.YongZheZhanChang:
                                                CoreInterface.GetEventSourceInterface().fireEvent(new KuaFuNotifyEnterGameEvent(kuaFuServerLoginData), (int)SceneUIClasses.YongZheZhanChang);
                                                break;
                                            case GameTypes.KuaFuBoss:
                                                CoreInterface.GetEventSourceInterface().fireEvent(new KuaFuNotifyEnterGameEvent(kuaFuServerLoginData), (int)SceneUIClasses.KuaFuBoss);
                                                break;
                                            case GameTypes.KingOfBattle:
                                                CoreInterface.GetEventSourceInterface().fireEvent(new KuaFuNotifyEnterGameEvent(kuaFuServerLoginData), (int)SceneUIClasses.KingOfBattle);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateLhlyBhData:
                        {
                            if (args.Length == 1)
                            {
                                LangHunLingYuBangHuiDataEx data = args[0] as LangHunLingYuBangHuiDataEx;
                                CoreInterface.GetEventSourceInterface().fireEvent(new NotifyLhlyBangHuiDataGameEvent(data), (int)SceneUIClasses.LangHunLingYu);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateLhlyCityData:
                        {
                            if (args.Length == 1)
                            {
                                LangHunLingYuCityDataEx data = args[0] as LangHunLingYuCityDataEx;
                                CoreInterface.GetEventSourceInterface().fireEvent(new NotifyLhlyCityDataGameEvent(data), (int)SceneUIClasses.LangHunLingYu);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateLhlyOtherCityList:
                        {
                            if (args.Length == 1)
                            {
                                Dictionary<int, List<int>> data = args[0] as Dictionary<int, List<int>>;
                                CoreInterface.GetEventSourceInterface().fireEvent(new NotifyLhlyOtherCityListGameEvent(data), (int)SceneUIClasses.LangHunLingYu);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateLhlyCityOwnerList:
                        {
                            if (args.Length == 1)
                            {
                                List<LangHunLingYuKingHist> data = args[0] as List<LangHunLingYuKingHist>;
                                CoreInterface.GetEventSourceInterface().fireEvent(new NotifyLhlyCityOwnerHistGameEvent(data), (int)SceneUIClasses.LangHunLingYu);
                            }
                        }
                        break;
                    case (int)KuaFuEventTypes.UpdateLhlyCityOwnerAdmire:
                        {
                            if (args.Length == 2)
                            {
                                int rid = (int)args[0];
                                int admirecount = (int)args[1];
                                CoreInterface.GetEventSourceInterface().fireEvent(new NotifyLhlyCityOwnerAdmireGameEvent(rid, admirecount),
                                    (int)SceneUIClasses.LangHunLingYu);
                            }
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
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

        #endregion 回调函数

        #region 接口函数

        /// <summary>
        /// 匹配报名
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="zoneId"></param>
        /// <param name="gameType"></param>
        /// <param name="groupIndex"></param>
        /// <returns></returns>
        public int YongZheZhanChangSignUp(string userId, int roleId, int zoneId, int gameType, int groupIndex, int zhanDouLi)
        {
            int result;
            if (string.IsNullOrEmpty(userId) || roleId <= 0)
            {
                return StdErrorCode.Error_Not_Exist;
            }

            userId = userId.ToUpper();
            int count = Interlocked.Increment(ref CurrentRequestCount);
            try
            {
                if (count < MaxRequestCount)
                {
                    lock (Mutex)
                    {
                        KuaFuRoleData kuaFuRoleData;
                        if (RoleId2RoleDataDict.TryGetValue(roleId, out kuaFuRoleData))
                        {
                            //如果服务器ID不同,表明是跨服登录角色,不应该在此报名
                            if (kuaFuRoleData.ServerId != ClientInfo.ServerId)
                            {
                                return StdErrorCode.Error_Operation_Faild;
                            }
                        }
                    }

                    IYongZheZhanChangService kuaFuService = GetKuaFuService();
                    if (null != kuaFuService)
                    {
                        try
                        {
                            YongZheZhanChangGameData YongZheZhanChangGameData = new YongZheZhanChangGameData(){ZhanDouLi = zhanDouLi};
                            result = kuaFuService.RoleSignUp(ClientInfo.ServerId, userId, zoneId, roleId, gameType, groupIndex, YongZheZhanChangGameData);
                        }
                        catch (System.Exception ex)
                        {
                            ResetKuaFuService();
                        }
                    }
                    else
                    {
                        return StdErrorCode.Error_Server_Not_Registed;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
            finally
            {
                Interlocked.Decrement(ref CurrentRequestCount);
            }

            return StdErrorCode.Error_Success;
        }

        public int ChangeRoleState(int roleId, KuaFuRoleStates state, bool noWait = false)
        {
            int result = StdErrorCode.Error_Operation_Faild;

            IYongZheZhanChangService kuaFuService = null;
            KuaFuRoleData kuaFuRoleData = null;
            int serverId = ClientInfo.ServerId;
            lock (Mutex)
            {
                if (RoleId2RoleDataDict.TryGetValue(roleId, out kuaFuRoleData))
                {
                    serverId = kuaFuRoleData.ServerId;
                }
            }

            kuaFuService = GetKuaFuService(noWait);
            if (null != kuaFuService)
            {
                try
                {
                    result = kuaFuService.RoleChangeState(serverId, roleId, (int)state);
                    if (result >= 0)
                    {
                        lock (Mutex)
                        {
                            if (RoleId2RoleDataDict.TryGetValue(roleId, out kuaFuRoleData))
                            {
                                kuaFuRoleData.State = (KuaFuRoleStates)result;
                            }
                        }

                        if (null != kuaFuRoleData)
                        {
                            UpdateRoleData(kuaFuRoleData);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    result = StdErrorCode.Error_Server_Internal_Error;
                }
            }

            return result;
        }

        public YongZheZhanChangFuBenData GetKuaFuFuBenData(int gameId)
        {
            YongZheZhanChangFuBenData yongZheZhanChangFuBenData = null;
            lock (Mutex)
            {
                if (FuBenDataDict.TryGetValue(gameId, out yongZheZhanChangFuBenData))
                {
                    return yongZheZhanChangFuBenData;
                }
            }

            if (yongZheZhanChangFuBenData == null)
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        yongZheZhanChangFuBenData = (YongZheZhanChangFuBenData)kuaFuService.GetFuBenData(gameId);
                        if (null != yongZheZhanChangFuBenData)
                        {
                            lock (Mutex)
                            {
                                FuBenDataDict[gameId] = yongZheZhanChangFuBenData;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        yongZheZhanChangFuBenData = null;
                    }
                }
            }

            return yongZheZhanChangFuBenData;
        }

        private void ClearOverTimeFuBen(DateTime now)
        {
            lock (Mutex)
            {
                List<int> list = new List<int>();
                foreach (var kv in FuBenDataDict)
                {
                    if (kv.Value.EndTime < now)
                    {
                        list.Add(kv.Key);
                    }
                }

                foreach (var key in list)
                {
                    FuBenDataDict.Remove(key);
                }
            }
        }

        /// <summary>
        /// 从服务器获取
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public int GetKuaFuRoleState(int roleId)
        {
            int state = 0;

            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    object result = kuaFuService.GetRoleExtendData(ClientInfo.ServerId, roleId, (int)KuaFuRoleExtendDataTypes.KuaFuRoleState);
                    if (null != result)
                    {
                        state = (int)result;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return state;
        }

        /// <summary>
        /// 改变角色的在某个游戏副本中的状态
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="state"></param>
        /// <param name="serverId">如果不知道,传0</param>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public int GameFuBenRoleChangeState(int roleId, int state, int serverId = 0, int gameId = 0)
        {
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    if (serverId <= 0 || gameId <= 0)
                    {
                        KuaFuRoleData kuaFuRoleData;
                        if (!RoleId2RoleDataDict.TryGetValue(roleId, out kuaFuRoleData))
                        {
                            return (int)KuaFuRoleStates.None;
                        }

                        serverId = kuaFuRoleData.ServerId;
                        gameId = kuaFuRoleData.GameId;
                    }

                    return KuaFuService.GameFuBenRoleChangeState(serverId, roleId, gameId, state);
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
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

        /// <summary>
        /// 从服务器获取角色数据
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public KuaFuRoleData GetKuaFuRoleDataFromServer(int serverId, int roleId)
        {
            KuaFuRoleData kuaFuRoleData = null;
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    kuaFuRoleData = (KuaFuRoleData)kuaFuService.GetKuaFuRoleData(serverId, roleId);
                    UpdateRoleData(kuaFuRoleData); //更新
                }
                catch (System.Exception ex)
                {
                    kuaFuRoleData = null;
                }
            }

            return kuaFuRoleData;

        }

        /// <summary>
        /// 验证角色是否有本服务器的跨服活动
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="gameType"></param>
        /// <param name="gameId"></param>
        /// <returns>是否有效</returns>
        public bool KuaFuLogin(KuaFuServerLoginData kuaFuServerLoginData)
        {
            YongZheZhanChangFuBenData YongZheZhanChangFuBenData = GetKuaFuFuBenData((int)kuaFuServerLoginData.GameId);
            if (null != YongZheZhanChangFuBenData && YongZheZhanChangFuBenData.State < GameFuBenState.End)
            {
                if (YongZheZhanChangFuBenData.ServerId == ClientInfo.ServerId)
                {
                    if (YongZheZhanChangFuBenData.RoleDict.ContainsKey(kuaFuServerLoginData.RoleId))
                    {
                        kuaFuServerLoginData.FuBenSeqId = YongZheZhanChangFuBenData.SequenceId;
                        KuaFuRoleData kuaFuRoleData = GetKuaFuRoleDataFromServer(kuaFuServerLoginData.ServerId, kuaFuServerLoginData.RoleId);
                        if (kuaFuRoleData.GameId == YongZheZhanChangFuBenData.GameId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取跨服登录的角色的原服务器的IP和服务端口
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="dbIp"></param>
        /// <param name="dbPort"></param>
        /// <param name="logIp"></param>
        /// <param name="logPort"></param>
        /// <returns></returns>
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

        public void PushGameResultData(object data)
        {
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    kuaFuService.UpdateStatisticalData(data);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }
        }

        public int ExecuteCommand(string cmd)
        {
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    return kuaFuService.ExecuteCommand(cmd);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }

            return StdErrorCode.Error_Server_Internal_Error;
        }

        public object GetKuaFuLineDataList(int mapCode)
        {
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    return kuaFuService.GetKuaFuLineDataList(mapCode);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }

            return null;
        }

        public int EnterKuaFuMap(int roleId, int mapCode, int kuaFuLine, int roleSourceServerId, KuaFuServerLoginData kuaFuServerLoginData)
        {
            int kuaFuServerId;
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    kuaFuServerId = kuaFuService.EnterKuaFuMap(ClientInfo.ServerId, roleId, mapCode, kuaFuLine);
                    if (kuaFuServerId > 0)
                    {
                        kuaFuServerLoginData.RoleId = roleId;
                        kuaFuServerLoginData.ServerId = roleSourceServerId;
                        kuaFuServerLoginData.GameType = (int)GameTypes.KuaFuMap;
                        kuaFuServerLoginData.GameId = mapCode;
                        lock (Mutex)
                        {
                            KuaFuServerInfo kuaFuServerInfo;
                            if (ServerIdServerInfoDict.TryGetValue(kuaFuServerId, out kuaFuServerInfo))
                            {
                                kuaFuServerLoginData.ServerIp = kuaFuServerInfo.Ip;
                                kuaFuServerLoginData.ServerPort = kuaFuServerInfo.Port;
                                return kuaFuServerId;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }

            return StdErrorCode.Error_Server_Internal_Error;
        }

        public bool CanEnterKuaFuMap(KuaFuServerLoginData kuaFuServerLoginData)
        {
            //从中心查询副本信息
            KuaFuMapRoleData kuaFuMapRoleData = YongZheZhanChangClient.getInstance().GetKuaFuMapRoleData(kuaFuServerLoginData.RoleId);
            if (kuaFuMapRoleData == null || kuaFuMapRoleData.KuaFuServerId != ClientInfo.ServerId || kuaFuMapRoleData.KuaFuMapCode != kuaFuServerLoginData.GameId)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("{0}不具有进入跨服地图{1}的资格", kuaFuServerLoginData.RoleId, kuaFuServerLoginData.GameId));
                return false;
            }

            return true;
        }

        public KuaFuMapRoleData GetKuaFuMapRoleData(int roleId)
        {
            IYongZheZhanChangService kuaFuService = GetKuaFuService();
            if (null != kuaFuService)
            {
                try
                {
                    return kuaFuService.GetKuaFuMapRoleData(roleId);
                }
                catch (System.Exception ex)
                {
                    ResetKuaFuService();
                }
            }

            return null;
        }

        #region 圣域争霸

        /// <summary>
        /// 匹配报名
        /// </summary>
        /// <param name="bhName"></param>
        /// <param name="bhid"></param>
        /// <param name="zoneId"></param>
        /// <param name="gameType"></param>
        /// <param name="groupIndex"></param>
        /// <returns></returns>
        public int LangHunLingYuSignUp(string bhName, int bhid, int zoneId, int gameType, int groupIndex, int zhanDouLi)
        {
            int result = StdErrorCode.Error_Server_Busy;
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        result = kuaFuService.LangHunLingYuSignUp(bhName, bhid, zoneId, gameType, groupIndex, zhanDouLi);
                    }
                    catch (System.Exception ex)
                    {
                        result = StdErrorCode.Error_Server_Busy;
                        ResetKuaFuService();
                    }
                }
            }
            catch (System.Exception ex)
            {
                result = StdErrorCode.Error_Server_Busy;
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// 一场活动结束结果
        /// </summary>
        /// <param name="cityId">城池ID</param>
        /// <param name="bhid">占领方帮会ID</param>
        /// <returns></returns>
        public int GameFuBenComplete(LangHunLingYuStatisticalData data)
        {
            int result = StdErrorCode.Error_Server_Busy;
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        result = kuaFuService.GameFuBenComplete(data);
                    }
                    catch (System.Exception ex)
                    {
                        result = StdErrorCode.Error_Server_Busy;
                        ResetKuaFuService();
                    }
                }
            }
            catch (System.Exception ex)
            {
                result = StdErrorCode.Error_Server_Busy;
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// 膜拜
        /// </summary>
        public bool LangHunLingYunAdmire(int rid)
        {
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        return kuaFuService.LangHunLingYuAdmaire(rid);
                    }
                    catch (System.Exception ex)
                    {
                        ResetKuaFuService();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// 根据帮会ID和城池ID获取将要战斗的跨服服务器ID
        /// </summary>
        /// <param name="bhid"></param>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public LangHunLingYuFuBenData GetLangHunLingYuGameFuBenData(int gameId)
        {
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        return kuaFuService.GetLangHunLingYuGameFuBenData(gameId);
                    }
                    catch (System.Exception ex)
                    {
                        ResetKuaFuService();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }

        public bool LangHunLingYuKuaFuLoginData(int roleId, int cityId, int gameId, KuaFuServerLoginData kuaFuServerLoginData)
        {
            try
            {
                IYongZheZhanChangService kuaFuService = GetKuaFuService();
                if (null != kuaFuService)
                {
                    try
                    {
                        LangHunLingYuFuBenData fuBenData;
                        lock (Mutex)
                        {
                            if (!LangHunLingYuFuBenDataDict.TryGetValue(gameId, out fuBenData))
                            {
                                fuBenData = null;
                            }
                        }
                        if (null == fuBenData)
                        {
                            fuBenData = kuaFuService.GetLangHunLingYuGameFuBenDataByCityId(cityId);
                        }
                        if (null != fuBenData)
                        {
                            kuaFuServerLoginData.RoleId = roleId;
                            kuaFuServerLoginData.GameId = fuBenData.GameId;
                            kuaFuServerLoginData.GameType = (int)GameTypes.LangHunLingYu;
                            kuaFuServerLoginData.EndTicks = fuBenData.EndTime.Ticks;
                            kuaFuServerLoginData.ServerId = ClientInfo.ServerId;
                            lock (Mutex)
                            {
                                KuaFuServerInfo kuaFuServerInfo;
                                if (ServerIdServerInfoDict.TryGetValue(fuBenData.ServerId, out kuaFuServerInfo))
                                {
                                    kuaFuServerLoginData.ServerIp = kuaFuServerInfo.Ip;
                                    kuaFuServerLoginData.ServerPort = kuaFuServerInfo.Port;
                                    return true;
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ResetKuaFuService();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return false;
        }

        #endregion 圣域争霸

        #endregion 接口函数
    }
}
