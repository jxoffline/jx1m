using GameServer.Core.Executor;
using KF.Contract;
using KF.Contract.Data;
using KF.Contract.Data.HuanYingSiYuan;
using KF.Contract.Interface;
using KF.Remoting.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using Tmsk.Contract;

namespace KF.Remoting
{
    /*
     *  锁优先级: FuBenData > RoleData
     */

    /// <summary>
    /// 幻影寺院Remoting服务对象
    /// </summary>
    public class HuanYingSiYuanService : MarshalByRefObject, IKuaFuService
    {
        public static HuanYingSiYuanService Instance = null;

        /// <summary>
        /// lock对象
        /// </summary>
        private object Mutex = new object();

        /// <summary>
        ///
        /// </summary>
        public readonly GameTypes GameType = GameTypes.HuanYingSiYuan;

        /// <summary>
        /// 调用间隔
        /// </summary>
        private const double CheckGameFuBenInterval = 1000;

        /// <summary>
        /// 下次角色分配队伍的时间
        /// </summary>
        private DateTime CheckGameFuBenTime;

        /// <summary>
        /// 调用间隔
        /// </summary>
        private const double CheckRoleTimerProcInterval = 1.428;

        /// <summary>
        /// 下次清理过期副本数据的时间
        /// </summary>
        private DateTime CheckRoleTimerProcTime;

        public HuanYingSiYuanPersistence Persistence = HuanYingSiYuanPersistence.Instance;

        /// <summary>
        /// 现在存在的跨服副本数
        /// </summary>
        public int ExistKuaFuFuBenCount = 0;

        private int EnterGameSecs = 10 + 10;

        /// <summary>
        /// 断线保护时间(秒)
        /// </summary>
        public int DisconnectionProtectSecs = 15;

        /// <summary>
        /// 结束保护时间(秒)
        /// </summary>
        public int GameEndProtectSecs = 15;

        /// <summary>
        /// 最大服务器
        /// </summary>
        private int MaxServerLoad = 400;

        private int MaxHuanYingSiYuanDayCount = 1000;
        private int HuanYingSiYuanDayCount = 0;
        private int HuanYingSiYuanDayId = 0;

        /// <summary>
        /// 幻影寺院副本信息
        /// </summary>
        public ConcurrentDictionary<int, HuanYingSiYuanFuBenData> HuanYingSiYuanFuBenDataDict = new ConcurrentDictionary<int, HuanYingSiYuanFuBenData>(1, 4096);

        /// <summary>
        /// 全局玩家信息字典
        /// </summary>
        //private Dictionary<int, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new Dictionary<int, KuaFuRoleData>();
        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<string, int> UserId2RoleIdActiveDict = new ConcurrentDictionary<string, int>(1, 16384);

        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> WaitingKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<int, HuanYingSiYuanFuBenData> ShotOfRolesFuBenDataDict = new ConcurrentDictionary<int, HuanYingSiYuanFuBenData>(1, 4096);

        public Thread BackgroundThread;

        #region 定时处理

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

        public HuanYingSiYuanService()
        {
            BackgroundThread = new Thread(ThreadProc);
            BackgroundThread.IsBackground = true;
            BackgroundThread.Start();
        }

        ~HuanYingSiYuanService()
        {
            BackgroundThread.Abort();
        }

        public void ThreadProc(object state)
        {
            Persistence.InitConfig();

            DateTime lastRunTime = TimeUtil.NowDateTime();
            do
            {
                try
                {
                    DateTime now = TimeUtil.NowDateTime();
                    Global.UpdateNowTime(now);

                    //处理游戏逻辑
                    if (now > CheckRoleTimerProcTime)
                    {
                        CheckRoleTimerProcTime = now.AddSeconds(CheckRoleTimerProcInterval);
                        int signUpCount, startCount;
                        CheckRoleTimerProc(now, out signUpCount, out startCount);
                        ClientAgentManager.Instance().SetGameTypeLoad(GameType, signUpCount, startCount);
                    }

                    if (now > CheckGameFuBenTime)
                    {
                        CheckGameFuBenTime = now.AddSeconds(CheckGameFuBenInterval);
                        CheckGameFuBenTimerProc(now);
                    }

                    int sleepMS = (int)((TimeUtil.NowDateTime() - now).TotalMilliseconds);
                    Persistence.SaveCostTime(sleepMS);
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

        private void CheckRoleTimerProc(DateTime now, out int signUpCount, out int startCount)
        {
            signUpCount = 0;
            startCount = 0;

            bool assgionGameFuBen = true;
            long maxRemoveRoleTicks = now.AddHours(-2).Ticks;
            long waitTicks1 = now.AddSeconds(-Persistence.SignUpWaitSecs1).Ticks;
            long waitTicks2 = now.AddSeconds(-Persistence.SignUpWaitSecs2).Ticks;
            foreach (var kuaFuRoleData in RoleIdKuaFuRoleDataDict.Values)
            {
                int oldGameId = 0;
                lock (kuaFuRoleData)
                {
                    //清理超时角色
                    if (kuaFuRoleData.State == KuaFuRoleStates.None || kuaFuRoleData.State > KuaFuRoleStates.StartGame)
                    {
                        if (kuaFuRoleData.StateEndTicks < maxRemoveRoleTicks)
                        {
                            KuaFuRoleData kuaFuRoleDataTemp;
                            RoleIdKuaFuRoleDataDict.TryRemove(KuaFuRoleKey.Get(kuaFuRoleData.ServerId, kuaFuRoleData.RoleId), out kuaFuRoleDataTemp);
                            continue;
                        }
                    }
                    else if (kuaFuRoleData.State == KuaFuRoleStates.NotifyEnterGame || kuaFuRoleData.State == KuaFuRoleStates.EnterGame)
                    {
                        if (kuaFuRoleData.StateEndTicks < now.Ticks)
                        {
                            kuaFuRoleData.Age++;
                            kuaFuRoleData.State = KuaFuRoleStates.None;
                            oldGameId = kuaFuRoleData.GameId;
                        }
                    }
                }

                if (kuaFuRoleData.State == KuaFuRoleStates.SignUp)
                {
                    signUpCount++;
                    if (assgionGameFuBen)
                    {
                        assgionGameFuBen = AssignGameFuben(kuaFuRoleData, waitTicks1, waitTicks2, now);
                    }
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.SignUpWaiting)
                {
                    signUpCount++;
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.StartGame)
                {
                    startCount++;
                }

                if (oldGameId > 0)
                {
                    RemoveRoleFromFuBen(oldGameId, kuaFuRoleData.RoleId);
                    AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.RoleStateChange, kuaFuRoleData);
                    ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, GameType, evItem);
                }
            }
        }

        private void CheckGameFuBenTimerProc(DateTime now)
        {
            if (HuanYingSiYuanFuBenDataDict.Count > 0)
            {
                DateTime canRemoveTime = now.AddMinutes(-Consts.HuanYingSiYuanGameFuBenMaxExistMinutes);
                foreach (var fuBenData in HuanYingSiYuanFuBenDataDict.Values)
                {
                    lock (fuBenData)
                    {
                        if (fuBenData.CanRemove())
                        {
                            RemoveGameFuBen(fuBenData);
                        }
                        else if (fuBenData.EndTime < canRemoveTime)
                        {
                            RemoveGameFuBen(fuBenData);
                        }
                    }
                }
            }
        }

        #endregion 定时处理

        #region 接口实现

        public AsyncDataItem[] GetClientCacheItems(int serverId)
        {
            return ClientAgentManager.Instance().PickAsyncEvent(serverId, GameType);
        }

        /// <summary>
        /// 初始化跨服客户端回调对象
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="age"></param>
        /// <param name="version"></param>
        /// <returns>clientId</returns>
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo)
        {
            try
            {
                if (clientInfo.GameType == (int)GameTypes.HuanYingSiYuan && clientInfo.ServerId != 0)
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

        public int RoleSignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData)
        {
            int result = (int)KuaFuRoleStates.SignUp;
            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;

            //保持活动的角色和帐号唯一,不加锁,不严格保证
            int rid;
            if (!UserId2RoleIdActiveDict.TryGetValue(userId, out rid))
            {
                UserId2RoleIdActiveDict[userId] = roleId;
            }
            else
            {
                if (rid > 0 && rid != roleId)
                {
                    if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(serverId, rid), out kuaFuRoleData))
                    {
                        if (ChangeRoleState(kuaFuRoleData, KuaFuRoleStates.None) < 0)
                        {
                            //return -1;
                        }
                    }

                    UserId2RoleIdActiveDict[userId] = roleId;
                }
            }

            //添加或更新角色信息和状态
            Lazy<KuaFuRoleData> lazy = new Lazy<KuaFuRoleData>(() =>
                {
                    return new KuaFuRoleData()
                    {
                        RoleId = roleId,
                        UserId = userId,
                        GameType = gameType,
                    };
                });

            KuaFuRoleKey key = KuaFuRoleKey.Get(serverId, roleId);
            kuaFuRoleData = RoleIdKuaFuRoleDataDict.GetOrAdd(key, (x) => { return lazy.Value; });
            int oldGameId = 0;
            lock (kuaFuRoleData)
            {
                oldGameId = kuaFuRoleData.GameId;
                kuaFuRoleData.GameId = 0;

                //更新到新的状态
                kuaFuRoleData.Age++;
                kuaFuRoleData.State = KuaFuRoleStates.SignUp;
                kuaFuRoleData.ServerId = serverId;
                kuaFuRoleData.ZoneId = zoneId;
                kuaFuRoleData.GameData = gameData;
                kuaFuRoleData.GroupIndex = groupIndex;
                kuaFuRoleData.StateEndTicks = Global.NowTime.Ticks;
            }

            //WaitingKuaFuRoleDataDict[key] = kuaFuRoleData;

            //移除旧的游戏状态
            if (oldGameId > 0)
            {
                RemoveRoleFromFuBen(oldGameId, roleId);
            }

            return result;
        }

        public int RoleChangeState(int serverId, int roleId, int state)
        {
            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;

            //添加或更新角色信息和状态
            KuaFuRoleKey key = KuaFuRoleKey.Get(serverId, roleId);
            if (!RoleIdKuaFuRoleDataDict.TryGetValue(key, out kuaFuRoleData))
            {
                return StdErrorCode.Error_Server_Internal_Error;
            }

            //更新到新的状态
            int oldGameId = 0;
            lock (kuaFuRoleData)
            {
                if (state == (int)KuaFuRoleStates.None)
                {
                    oldGameId = kuaFuRoleData.GameId;
                    kuaFuRoleData.GameId = 0;
                }

                kuaFuRoleData.Age++;
                kuaFuRoleData.State = (KuaFuRoleStates)state;
            }

            //if (kuaFuRoleData.State != KuaFuRoleStates.SignUp)
            //{
            //    KuaFuRoleData kuaFuRoleDataTemp;
            //    WaitingKuaFuRoleDataDict.TryRemove(key, out kuaFuRoleDataTemp);
            //}

            if (oldGameId > 0)
            {
                RemoveRoleFromFuBen(oldGameId, roleId);
            }

            return state;
        }

        public int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state)
        {
            HuanYingSiYuanFuBenData huanYingSiYuanFuBenData;
            if (HuanYingSiYuanFuBenDataDict.TryGetValue(gameId, out huanYingSiYuanFuBenData))
            {
                lock (huanYingSiYuanFuBenData)
                {
                    KuaFuFuBenRoleData kuaFuFuBenRoleData;
                    if (huanYingSiYuanFuBenData.RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
                    {
                        if ((KuaFuRoleStates)state == KuaFuRoleStates.Offline || (KuaFuRoleStates)state == KuaFuRoleStates.None)
                        {
                            RemoveRoleFromFuBen(gameId, roleId);
                        }
                    }
                }
            }

            KuaFuRoleData kuaFuRoleData;

            //添加或更新角色信息和状态
            if (!RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(serverId, roleId), out kuaFuRoleData))
            {
                return StdErrorCode.Error_Not_Exist;
            }

            //更新到新的状态
            if (kuaFuRoleData.GameId == gameId)
            {
                ChangeRoleState(kuaFuRoleData, (KuaFuRoleStates)state);
            }

            return state;
        }

        public KuaFuRoleData GetKuaFuRoleData(int serverId, int roleId)
        {
            KuaFuRoleData kuaFuRoleData = null;
            if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(serverId, roleId), out kuaFuRoleData) && kuaFuRoleData.State != KuaFuRoleStates.None)
            {
                return kuaFuRoleData;
            }

            return null;
        }

        /// <summary>
        /// 获取角色相关的扩展数据
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="roleId"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public object GetRoleExtendData(int serverId, int roleId, int dataType)
        {
            KuaFuRoleData kuaFuRoleData = null;
            if (!RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(serverId, roleId), out kuaFuRoleData))
            {
                return null;
            }

            if (dataType == (int)KuaFuRoleExtendDataTypes.GameFuBenRoleCount)
            {
                int roleCount = 0;
                if (kuaFuRoleData.State == KuaFuRoleStates.SignUp)
                {
                    roleCount = 1;
                }

                if (kuaFuRoleData.GameId > 0)
                {
                    HuanYingSiYuanFuBenData huanYingSiYuanFuBenData;
                    if (HuanYingSiYuanFuBenDataDict.TryGetValue(kuaFuRoleData.GameId, out huanYingSiYuanFuBenData))
                    {
                        if (huanYingSiYuanFuBenData.State < GameFuBenState.Start)
                        {
                            roleCount = huanYingSiYuanFuBenData.GetFuBenRoleCount();
                        }
                        else
                        {
                            RemoveRoleFromFuBen(kuaFuRoleData.GameId, roleId);
                            RoleChangeState(serverId, roleId, (int)KuaFuRoleStates.None);
                            roleCount = 0;
                        }
                    }
                }

                return roleCount;
            }

            return null;
        }

        public List<KuaFuServerInfo> GetKuaFuServerInfoData(int age)
        {
            return KuaFuServerManager.GetKuaFuServerInfoData(age);
        }

        public IKuaFuFuBenData GetFuBenData(int gameId)
        {
            HuanYingSiYuanFuBenData kuaFuFuBenData = null;
            if (HuanYingSiYuanFuBenDataDict.TryGetValue(gameId, out kuaFuFuBenData) && kuaFuFuBenData.State < GameFuBenState.End)
            {
                return kuaFuFuBenData;
            }

            return null;
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
            int result = StdErrorCode.Error_Operation_Faild;

            IKuaFuService kuaFuService = null;
            HuanYingSiYuanFuBenData huanYingSiYuanFuBenData = null;

            if (HuanYingSiYuanFuBenDataDict.TryGetValue(gameId, out huanYingSiYuanFuBenData))
            {
                lock (huanYingSiYuanFuBenData)
                {
                    huanYingSiYuanFuBenData.State = state;
                    if (state == GameFuBenState.End)
                    {
                        RemoveGameFuBen(huanYingSiYuanFuBenData);
                    }
                }
            }
            else
            {
                return StdErrorCode.Error_Not_Exist;
            }

            return result;
        }

        #endregion 接口实现

        #region 辅助函数

        private int ChangeRoleState(KuaFuRoleData kuaFuRoleData, KuaFuRoleStates state)
        {
            int result = -1;

            try
            {
                KuaFuRoleData roleData = null;
                int oldGameId = 0;
                lock (kuaFuRoleData)
                {
                    kuaFuRoleData.Age++;
                    kuaFuRoleData.State = state;
                    if (state == KuaFuRoleStates.None && kuaFuRoleData.GameId > 0)
                    {
                        oldGameId = kuaFuRoleData.GameId;
                    }

                    roleData = (KuaFuRoleData)kuaFuRoleData;
                }

                if (oldGameId > 0)
                {
                    RemoveRoleFromFuBen(oldGameId, kuaFuRoleData.RoleId);
                }

                if (null != roleData)
                {
                    AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.RoleStateChange, kuaFuRoleData);
                    ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, GameType, evItem);
                }
            }
            catch (System.Exception ex)
            {
                return -1;
            }

            return result;
        }

        private void NotifyFuBenRoleCount(HuanYingSiYuanFuBenData fuBenData)
        {
            try
            {
                lock (fuBenData)
                {
                    int roleCount = fuBenData.RoleDict.Count;
                    foreach (var role in fuBenData.RoleDict.Values)
                    {
                        KuaFuRoleData kuaFuRoleData;
                        if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(role.ServerId, role.RoleId), out kuaFuRoleData))
                        {
                            AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.NotifyWaitingRoleCount, kuaFuRoleData.RoleId, roleCount);
                            ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, GameType, evItem);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void NotifyFuBenRoleEnterGame(HuanYingSiYuanFuBenData fuBenData)
        {
            try
            {
                lock (fuBenData)
                {
                    foreach (var role in fuBenData.RoleDict.Values)
                    {
                        KuaFuRoleData kuaFuRoleData;
                        if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(role.ServerId, role.RoleId), out kuaFuRoleData) && kuaFuRoleData.State == KuaFuRoleStates.NotifyEnterGame)
                        {
                            AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.UpdateAndNotifyEnterGame, kuaFuRoleData);
                            ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, GameType, evItem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private bool AssignGameFuben(KuaFuRoleData kuaFuRoleData, long waitSecs1, long waitSecs2, DateTime now)
        {
            int roleCount = 0;
            DateTime stateEndTime = now.AddSeconds(EnterGameSecs);
            HuanYingSiYuanFuBenData selectFuben = null;

            KuaFuFuBenRoleData kuaFuFuBenRoleData = new KuaFuFuBenRoleData()
            {
                ServerId = kuaFuRoleData.ServerId,
                RoleId = kuaFuRoleData.RoleId,
                ZhanDouLi = HuanYingSiYuanGameData.GetZhanDouLi(kuaFuRoleData.GameData),
            };

            try
            {
                //先检测是否有需要补充人的队伍
                foreach (var tmpFuben in ShotOfRolesFuBenDataDict.Values)
                {
                    //分组编号要相等
                    if (tmpFuben.CanRemove())
                    {
                        RemoveGameFuBen(tmpFuben);
                    }
                    else if (tmpFuben.CanEnter(kuaFuRoleData.GroupIndex, waitSecs1, waitSecs2))
                    {
                        if (ClientAgentManager.Instance().IsAgentAlive(tmpFuben.ServerId))
                        {
                            roleCount = tmpFuben.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData, GameFuBenRoleCountChangedHandler);
                            if (roleCount > 0)
                            {
                                selectFuben = tmpFuben;
                                break;
                            }
                        }
                    }
                }

                if (null == selectFuben)
                {
                    int gameId = Persistence.GetNextGameId();
                    int kfSrvId = 0;
                    if (ClientAgentManager.Instance().AssginKfFuben(GameType, gameId, 1, out kfSrvId))
                    {
                        selectFuben = new HuanYingSiYuanFuBenData();
                        selectFuben.ServerId = kfSrvId;
                        selectFuben.GameId = gameId;
                        selectFuben.GroupIndex = kuaFuRoleData.GroupIndex;
                        selectFuben.EndTime = Global.NowTime.AddMinutes(Consts.HuanYingSiYuanGameFuBenMaxExistMinutes);
                        AddGameFuBen(selectFuben);
                        roleCount = selectFuben.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData, GameFuBenRoleCountChangedHandler);
                        Persistence.LogCreateHysyFuben(gameId, kfSrvId, 0, 1);
                    }
                }

                if (selectFuben != null && roleCount > 0)
                {
                    if (roleCount == 1)
                    {
                        selectFuben.EndTime = now; //第一个人进入时,重置副本创建时间
                    }

                    if (selectFuben.State == GameFuBenState.Wait)
                    {
                        if (roleCount == Consts.HuanYingSiYuanRoleCountTotal)
                        {
                            List<KuaFuFuBenRoleData> roleList = selectFuben.SortFuBenRoleList();
                            foreach (var role in roleList)
                            {
                                KuaFuRoleData kuaFuRoleDataTemp;
                                KuaFuRoleKey key = KuaFuRoleKey.Get(role.ServerId, role.RoleId);
                                if (RoleIdKuaFuRoleDataDict.TryGetValue(key, out kuaFuRoleDataTemp))
                                {
                                    kuaFuRoleDataTemp.UpdateStateTime(selectFuben.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                                }
                            }

                            selectFuben.State = GameFuBenState.Start;
                            NotifyFuBenRoleEnterGame(selectFuben);
                        }
                        else
                        {
                            kuaFuRoleData.UpdateStateTime(selectFuben.GameId, KuaFuRoleStates.SignUpWaiting, kuaFuRoleData.StateEndTicks);
                            NotifyFuBenRoleCount(selectFuben);
                        }
                    }
                    else if (selectFuben.State == GameFuBenState.Start)
                    {
                        kuaFuRoleData.UpdateStateTime(selectFuben.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                        NotifyFuBenRoleEnterGame(selectFuben);
                    }
                }
                else
                {
                    //如果分配失败,则返回false,本轮不在尝试分配
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// 活动副本人数变化处理函数
        /// </summary>
        /// <param name="huanYingSiYuanFuBenData"></param>
        /// <param name="roleCount"></param>
        private void GameFuBenRoleCountChangedHandler(HuanYingSiYuanFuBenData huanYingSiYuanFuBenData, int roleCount)
        {
            HuanYingSiYuanFuBenData huanYingSiYuanFuBenDataTemp;
            if (roleCount == Consts.HuanYingSiYuanRoleCountTotal)
            {
                ShotOfRolesFuBenDataDict.TryRemove(huanYingSiYuanFuBenData.GameId, out huanYingSiYuanFuBenDataTemp);
            }
            else if (!huanYingSiYuanFuBenData.CanRemove())
            {
                ShotOfRolesFuBenDataDict[huanYingSiYuanFuBenData.GameId] = huanYingSiYuanFuBenData;
            }
        }

        private void AddGameFuBen(HuanYingSiYuanFuBenData huanYingSiYuanFuBenData)
        {
            HuanYingSiYuanFuBenDataDict[huanYingSiYuanFuBenData.GameId] = huanYingSiYuanFuBenData;
        }

        private void RemoveGameFuBen(HuanYingSiYuanFuBenData fubenData)
        {
            int gameId = fubenData.GameId;
            HuanYingSiYuanFuBenData tmpFuben;
            ShotOfRolesFuBenDataDict.TryRemove(gameId, out tmpFuben);
            if (HuanYingSiYuanFuBenDataDict.TryRemove(gameId, out tmpFuben))
            {
                tmpFuben.State = GameFuBenState.End;
            }
            ClientAgentManager.Instance().RemoveKfFuben(GameType, tmpFuben.ServerId, tmpFuben.GameId);

            lock (fubenData)
            {
                foreach (var fuBenRoleData in fubenData.RoleDict.Values)
                {
                    KuaFuRoleData kuaFuRoleData;
                    if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(fuBenRoleData.ServerId, fuBenRoleData.RoleId), out kuaFuRoleData))
                    {
                        if (kuaFuRoleData.GameId == gameId)
                        {
                            kuaFuRoleData.State = KuaFuRoleStates.None;
                        }
                    }
                }
            }
        }

        private void RemoveRoleFromFuBen(int gameId, int roleId)
        {
            if (gameId > 0)
            {
                HuanYingSiYuanFuBenData huanYingSiYuanFuBenData;
                if (HuanYingSiYuanFuBenDataDict.TryGetValue(gameId, out huanYingSiYuanFuBenData))
                {
                    int count = 0;
                    lock (huanYingSiYuanFuBenData)
                    {
                        count = huanYingSiYuanFuBenData.RemoveKuaFuFuBenRoleData(roleId, GameFuBenRoleCountChangedHandler);

                        if (huanYingSiYuanFuBenData.CanRemove())
                        {
                            RemoveGameFuBen(huanYingSiYuanFuBenData);
                        }
                        else
                        {
                            if (huanYingSiYuanFuBenData.State < GameFuBenState.Start)
                            {
                                NotifyFuBenRoleCount(huanYingSiYuanFuBenData);
                            }
                        }
                    }
                }
            }
        }

        #endregion 辅助函数
    }
}