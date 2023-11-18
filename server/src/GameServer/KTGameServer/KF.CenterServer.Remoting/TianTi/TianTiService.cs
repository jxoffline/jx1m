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
using GameServer.Core.Executor;
using Server.Tools;

namespace KF.Remoting
{
    /*
     *  锁优先级: FuBenData > RoleData
     */

    /// <summary>
    /// 幻影寺院Remoting服务对象
    /// </summary>
    public class TianTiService : MarshalByRefObject, ITianTiService, IExecCommand
    {
        public static TianTiService Instance = null;

        /// <summary>
        /// lock对象
        /// </summary>
        private object Mutex = new object();

        /// <summary>
        /// 
        /// </summary>
        public readonly GameTypes GameType = GameTypes.TianTi;

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

        /// <summary>
        /// 检查更新服务器状态的时间间隔
        /// </summary>
        private const double SaveServerStateProcInterval = 30;

        /// <summary>
        /// 更新服务器状态
        /// </summary>
        private DateTime SaveServerStateProcTime;

        public TianTiPersistence Persistence = TianTiPersistence.Instance;

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

        private int MaxTianTiDayCount = 1000;
        private int TianTiDayCount = 0;
        private int TianTiDayId = 0;

        public int[] AssignRangeArray = new int[] { 0, 1, 2, 100 };

        /// <summary>
        /// 幻影寺院副本信息
        /// </summary>
        public ConcurrentDictionary<int, TianTiFuBenData> TianTiFuBenDataDict = new ConcurrentDictionary<int, TianTiFuBenData>(1,4096);

        private SortedDictionary<RangeKey, TianTiFuBenData> ProcessTianTiFuBenDataDict = new SortedDictionary<RangeKey, TianTiFuBenData>(RangeKey.Comparer);

        private SortedList<long, int> RolePairFightCountDict = new SortedList<long, int>();

        /// <summary>
        /// 全局玩家信息字典
        /// </summary>
        //private Dictionary<int, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new Dictionary<int, KuaFuRoleData>();
        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<string, int> UserId2RoleIdActiveDict = new ConcurrentDictionary<string, int>(1, 16384);

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

        public TianTiService()
        {
            //BackgroundThread = new Thread(ThreadProc);
            //BackgroundThread.IsBackground = true;
            //BackgroundThread.Start();
        }

        ~TianTiService()
        {
            BackgroundThread.Abort();
        }

        public void ThreadProc(object state)
        {
            Persistence.InitConfig();
            ZhengBaManagerK.Instance().InitConfig();
            DateTime lastRunTime = TimeUtil.NowDateTime();
            Persistence.LoadTianTiRankData(lastRunTime);
            ZhengBaManagerK.Instance().ReloadSyncData(lastRunTime);
            CoupleArenaService.getInstance().StartUp();
            CoupleWishService.getInstance().StartUp();

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
                        int signUpCnt, startCnt;
                        CheckRoleTimerProc(now, out signUpCnt, out startCnt);
                        ClientAgentManager.Instance().SetGameTypeLoad(GameType, signUpCnt, startCnt);
                    }

                    if (now > SaveServerStateProcTime)
                    {
                        SaveServerStateProcTime = now.AddSeconds(SaveServerStateProcInterval);

                        if (now.Hour >= 3 && now.Hour < 4)
                        {
                            ClearRolePairFightCount();
                            Persistence.UpdateTianTiRankData(now);
                        }
                    }

                    if (now > CheckGameFuBenTime)
                    {
                        CheckGameFuBenTime = now.AddSeconds(CheckGameFuBenInterval);
                        CheckGameFuBenTimerProc(now);
                    }

                    AsyncDataItem[] asyncEvArray = ZhengBaManagerK.Instance().Update();
                    ClientAgentManager.Instance().BroadCastAsyncEvent(GameType, asyncEvArray);

                    Persistence.WriteRoleInfoDataProc();

                    CoupleArenaService.getInstance().Update();
                    CoupleWishService.getInstance().Update();

                    int sleepMS = (int)((TimeUtil.NowDateTime() - now).TotalMilliseconds);
                    Persistence.SaveCostTime(sleepMS);
                    sleepMS = 1000 - sleepMS; //最大睡眠1000ms,最少睡眠50ms
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

        public RangeKey GetAssignRange(int baseValue, long startTicks, long waitTicks1, long waitTicks3, long waitTicksAll)
        {
            int rangeIndex;
            if (startTicks > waitTicks3)
            {
                if (startTicks > waitTicks1)
                {
                    rangeIndex = 0;
                }
                else
                {
                    rangeIndex = 1;
                }
            }
            else
            {
                if (startTicks > waitTicksAll)
                {
                    rangeIndex = 2;
                }
                else
                {
                    rangeIndex = 3;
                }
            }

            int expend = AssignRangeArray[rangeIndex];
            return new RangeKey(baseValue - expend, baseValue + expend);
        }

        private void CheckRoleTimerProc(DateTime now, out int signUpCnt, out int startCount)
        {
            signUpCnt = 0;
            startCount = 0;

            bool assgionGameFuBen = true;
            long maxRemoveRoleTicks = now.AddHours(-2).Ticks;

            long waitTicks1 = now.AddSeconds(-Persistence.SignUpWaitSecs1).Ticks;
            long waitTicks3 = now.AddSeconds(-Persistence.SignUpWaitSecs3).Ticks;
            long waitTicksAll = now.AddSeconds(-Persistence.SignUpWaitSecsAll).Ticks;
            long waitTicksMax = now.AddSeconds(-Persistence.WaitForJoinMaxSecs).Ticks;

            ProcessTianTiFuBenDataDict.Clear();
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
                    else if (kuaFuRoleData.State == KuaFuRoleStates.SignUp)
                    {
                        if (kuaFuRoleData.StateEndTicks < waitTicksMax)
                        {
                            kuaFuRoleData.Age++;
                            kuaFuRoleData.State = KuaFuRoleStates.None;
                        }
                    }
                }

                if (kuaFuRoleData.State == KuaFuRoleStates.SignUp)
                {
                    signUpCnt++;
                    if (assgionGameFuBen)
                    {
                        RangeKey range = GetAssignRange(kuaFuRoleData.GroupIndex, kuaFuRoleData.StateEndTicks, waitTicks1, waitTicks3, waitTicksAll);
                        assgionGameFuBen = AssignGameFuben(kuaFuRoleData, range, now);
                    }
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.SignUpWaiting)
                {
                    signUpCnt++;
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
            if (TianTiFuBenDataDict.Count > 0)
            {
                DateTime canRemoveTime = now.AddMinutes(-Consts.TianTiGameFuBenMaxExistMinutes);
                foreach (var fuBenData in TianTiFuBenDataDict.Values)
                {
                    TianTiFuBenData tianTiFuBenData;
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
                if (clientInfo.GameType == (int)GameTypes.TianTi && clientInfo.ServerId != 0)
                {
                    return ClientAgentManager.Instance().InitializeClient(callback, clientInfo);
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("InitializeClient时GameType错误,禁止连接.ServerId:{0},GameType:{1}", clientInfo.ServerId, clientInfo.GameType));
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
            lock(kuaFuRoleData)
            {
                if (state == (int)KuaFuRoleStates.None)
                {
                    oldGameId = kuaFuRoleData.GameId;
                    kuaFuRoleData.GameId = 0;
                }

                kuaFuRoleData.Age++;
                kuaFuRoleData.State = (KuaFuRoleStates)state;
            }

            if (oldGameId > 0)
            {
                RemoveRoleFromFuBen(oldGameId, roleId);
            }

            return state;
        }

        public int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state)
        {
            TianTiFuBenData tianTiFuBenData;
            if (TianTiFuBenDataDict.TryGetValue(gameId, out tianTiFuBenData))
            {
                lock (tianTiFuBenData)
                {
                    KuaFuFuBenRoleData kuaFuFuBenRoleData;
                    if (tianTiFuBenData.RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
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
                    TianTiFuBenData tianTiFuBenData;
                    if (TianTiFuBenDataDict.TryGetValue(kuaFuRoleData.GameId, out tianTiFuBenData))
                    {
                        if (tianTiFuBenData.State < GameFuBenState.Start)
                        {
                            roleCount = tianTiFuBenData.GetFuBenRoleCount();
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
            return  KuaFuServerManager.GetKuaFuServerInfoData(age);
        }

        public IKuaFuFuBenData GetFuBenData(int gameId)
        {
            TianTiFuBenData kuaFuFuBenData = null;
            if (TianTiFuBenDataDict.TryGetValue(gameId, out kuaFuFuBenData) && kuaFuFuBenData.State < GameFuBenState.End)
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
            TianTiFuBenData tianTiFuBenData = null;

            if (TianTiFuBenDataDict.TryGetValue(gameId, out tianTiFuBenData))
            {
                AddRolePairFightCount(tianTiFuBenData);
                lock (tianTiFuBenData)
                {
                    tianTiFuBenData.State = state;
                    if (state == GameFuBenState.End)
                    {
                        RemoveGameFuBen(tianTiFuBenData);
                    }
                }
            }
            else
            {
                return StdErrorCode.Error_Not_Exist;
            }

            return result;
        }

        public TianTiRankData GetRankingData(DateTime modifyTime)
        {
            return Persistence.GetTianTiRankData(modifyTime);
        }

        public void UpdateRoleInfoData(TianTiRoleInfoData data)
        {
            Persistence.UpdateRoleInfoData(data);
        }

        #endregion 接口实现

        #region 众神争霸 IZhengBaService
        public ZhengBaSyncData SyncZhengBaData(ZhengBaSyncData lastSyncData)
        {
            return ZhengBaManagerK.Instance().SyncZhengBaData(lastSyncData);
        }
        public int ZhengBaSupport(ZhengBaSupportLogData data)
        {
            return ZhengBaManagerK.Instance().ZhengBaSupport(data);
        }
        public int ZhengBaRequestEnter(int roleId, int gameId, EZhengBaEnterType enter)
        {
            return ZhengBaManagerK.Instance().ZhengBaRequestEnter(roleId, gameId, enter);
        }
        public int ZhengBaKuaFuLogin(int roleid, int gameId)
        {
            return ZhengBaManagerK.Instance().ZhengBaKuaFuLogin(roleid, gameId);
        }
        public List<ZhengBaNtfPkResultData> ZhengBaPkResult(int gameId, int winner, int FirstLeaveRoleId)
        {
            return ZhengBaManagerK.Instance().ZhengBaPkResult(gameId, winner, FirstLeaveRoleId);
        }
        #endregion

        #region 辅助函数

        private int ChangeRoleState( KuaFuRoleData kuaFuRoleData, KuaFuRoleStates state)
        {
            int result = -1;

            try
            {
                IKuaFuClient kuaFuClient = null;
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

        private void NotifyFuBenRoleCount(TianTiFuBenData fuBenData)
        {
            try
            {
                lock(fuBenData)
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

        private void NotifyFuBenRoleEnterGame(TianTiFuBenData fuBenData)
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

        private long MakeRolePairKey(int roleId1, int roleId2)
        {
            long rolePairKey;
            if (roleId1 < roleId2)
            {
                rolePairKey = (((long)roleId1) << 32) + roleId2;
            }
            else
            {
                rolePairKey = (((long)roleId2) << 32) + roleId1;
            }

            return rolePairKey;
        }

        private void ClearRolePairFightCount()
        {
            lock (RolePairFightCountDict)
            {
                RolePairFightCountDict.Clear();
            }
        }

        private void AddRolePairFightCount(TianTiFuBenData tianTiFuBenData)
        {
            int roleId1 = 0;
            int roleId2 = 0;

            if (tianTiFuBenData.RoleDict.Count >= 2)
            {
                foreach (var role in tianTiFuBenData.RoleDict.Values)
                {
                    if (roleId1 == 0)
                    {
                        roleId1 = role.RoleId;
                    }
                    else
                    {
                        roleId2 = role.RoleId;
                    }
                }

                long rolePairKey = MakeRolePairKey(roleId1, roleId2);

                int fightCount;
                lock (RolePairFightCountDict)
                {
                    if (!RolePairFightCountDict.TryGetValue(rolePairKey, out fightCount))
                    {
                        RolePairFightCountDict[rolePairKey] = 1;
                    }
                    else
                    {
                        RolePairFightCountDict[rolePairKey] = fightCount + 1;
                    }
                }
            }
        }

        private bool CanAddFuBenRole(TianTiFuBenData tianTiFuBenData, KuaFuRoleData kuaFuRoleData)
        {
            if (tianTiFuBenData.RoleDict.Count == 0)
            {
                return true;
            }

            KuaFuFuBenRoleData existRole = tianTiFuBenData.RoleDict.Values.FirstOrDefault();
            long rolePairKey = MakeRolePairKey(kuaFuRoleData.RoleId, existRole.RoleId);

            int fightCount;
            lock (RolePairFightCountDict)
            {
                if (!RolePairFightCountDict.TryGetValue(rolePairKey, out fightCount) || fightCount < Persistence.MaxRolePairFightCount)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AssignGameFuben(KuaFuRoleData kuaFuRoleData, RangeKey range, DateTime now)
        {
            int roleCount = 0;
            DateTime stateEndTime = now.AddSeconds(EnterGameSecs);
            TianTiFuBenData tianTiFuBenData = null;
                     
            KuaFuFuBenRoleData kuaFuFuBenRoleData = new KuaFuFuBenRoleData()
                {
                    ServerId = kuaFuRoleData.ServerId,
                    RoleId = kuaFuRoleData.RoleId,
                };

            List<KuaFuRoleData> updateRoleDataList = new List<KuaFuRoleData>();

            if (!ProcessTianTiFuBenDataDict.TryGetValue(range, out tianTiFuBenData))
            {
                tianTiFuBenData = new TianTiFuBenData();
                ProcessTianTiFuBenDataDict.Add(range, tianTiFuBenData);
            }
            else if (!CanAddFuBenRole(tianTiFuBenData, kuaFuRoleData))
            {
                return true;
            }

            roleCount = tianTiFuBenData.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData);
            if (roleCount < Consts.TianTiRoleCountTotal)
            {
                return true;
            }

            try
            {
                int kfSrvId = 0;
                int gameId = Persistence.GetNextGameId();
                bool createSuccess = ClientAgentManager.Instance().AssginKfFuben(GameType, gameId, roleCount, out kfSrvId);
                if (createSuccess)
                {
                    tianTiFuBenData.ServerId = kfSrvId;
                    tianTiFuBenData.GameId = gameId;
                    tianTiFuBenData.EndTime = Global.NowTime.AddMinutes(Consts.TianTiGameFuBenMaxExistMinutes);
                    AddGameFuBen(tianTiFuBenData);

                    Persistence.LogCreateTianTiFuBen(tianTiFuBenData.GameId, tianTiFuBenData.ServerId, 0, roleCount);

                    foreach (var role in tianTiFuBenData.RoleDict.Values)
                    {
                        KuaFuRoleData kuaFuRoleDataTemp;
                        KuaFuRoleKey key = KuaFuRoleKey.Get(role.ServerId, role.RoleId);
                        if (RoleIdKuaFuRoleDataDict.TryGetValue(key, out kuaFuRoleDataTemp))
                        {
                            kuaFuRoleDataTemp.UpdateStateTime(tianTiFuBenData.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                        }
                    }

                    tianTiFuBenData.State = GameFuBenState.Start;
                    NotifyFuBenRoleEnterGame(tianTiFuBenData);

                    ProcessTianTiFuBenDataDict.Remove(range);
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

        private void AddGameFuBen(TianTiFuBenData tianTiFuBenData)
        {
            TianTiFuBenDataDict[tianTiFuBenData.GameId] = tianTiFuBenData;
        }

        private void RemoveGameFuBen(TianTiFuBenData tianTiFuBenData)
        {
            int gameId = tianTiFuBenData.GameId;
            TianTiFuBenData tianTiFuBenDataTemp;
            if(TianTiFuBenDataDict.TryRemove(gameId, out tianTiFuBenDataTemp))
            {
                tianTiFuBenDataTemp.State = GameFuBenState.End;
            }

            ClientAgentManager.Instance().RemoveKfFuben(GameType, tianTiFuBenData.ServerId, tianTiFuBenData.GameId);

            lock (tianTiFuBenData)
            {
                foreach (var fuBenRoleData in tianTiFuBenData.RoleDict.Values)
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
                TianTiFuBenData tianTiFuBenData;
                if (TianTiFuBenDataDict.TryGetValue(gameId, out tianTiFuBenData))
                {
                    int count = 0;
                    lock (tianTiFuBenData)
                    {
                        count = tianTiFuBenData.RemoveKuaFuFuBenRoleData(roleId);

                        if (tianTiFuBenData.CanRemove())
                        {
                            RemoveGameFuBen(tianTiFuBenData);
                        }
                        else
                        {
                            if (tianTiFuBenData.State < GameFuBenState.Start)
                            {
                                NotifyFuBenRoleCount(tianTiFuBenData);
                            }
                        }
                    }
                }
            }
        }

        #endregion 辅助函数

        #region 排行榜

        public int ExecCommand(string[] args)
        {
            int result = -1;

            try
            {
                if (0 == string.Compare(args[0], "reload") && 0 == string.Compare(args[1], "paihang"))
                {
                    bool monthRank = false;
                    if (args.Length >= 3)
                    {
                        monthRank = true;
                    }

                    Persistence.UpdateTianTiRankData(TimeUtil.NowDateTime(), monthRank, true);
                }
                else if (0 == string.Compare(args[0], "load"))
                {
                    Persistence.LoadTianTiRankData(TimeUtil.NowDateTime());
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
            }

            return result;
        }

        #endregion 排行榜

        #region Interface `ICoupleArenaService`
        public int CoupleArenaJoin(int roleId1, int roleId2, int serverId)
        {
            return CoupleArenaService.getInstance().CoupleArenaJoin(roleId1, roleId2, serverId);
        }
        public int CoupleArenaQuit(int roleId1, int roleId2)
        {
            return CoupleArenaService.getInstance().CoupleArenaQuit(roleId1, roleId2);
        }
        public CoupleArenaSyncData CoupleArenaSync(DateTime lastSyncTime)
        {
            return CoupleArenaService.getInstance().CoupleArenaSync(lastSyncTime);
        }
        public int CoupleArenaPreDivorce(int roleId1, int roleId2)
        {
            return CoupleArenaService.getInstance().CoupleArenaPreDivorce(roleId1, roleId2);
        }
        public CoupleArenaFuBenData GetFuBenData(long gameId)
        {
            return CoupleArenaService.getInstance().GetFuBenData(gameId);
        }
        public CoupleArenaPkResultRsp CoupleArenaPkResult(CoupleArenaPkResultReq req)
        {
            return CoupleArenaService.getInstance().CoupleArenaPkResult(req);
        }
        #endregion

        #region Interface `ICoupleWishService`
        public int CoupleWishWishRole(CoupleWishWishRoleReq req)
        {
            return CoupleWishService.getInstance().CoupleWishWishRole(req);
        }
        public List<CoupleWishWishRecordData> CoupleWishGetWishRecord(int roleId)
        {
            return CoupleWishService.getInstance().CoupleWishGetWishRecord(roleId);
        }
        public CoupleWishSyncData CoupleWishSyncCenterData(DateTime oldThisWeek, DateTime oldLastWeek, DateTime oldStatue)
        {
            return CoupleWishService.getInstance().CoupleWishSyncCenterData(oldThisWeek, oldLastWeek, oldStatue);
        }
        public int CoupleWishPreDivorce(int man, int wife)
        {
            return CoupleWishService.getInstance().CoupleWishPreDivorce(man, wife);
        }
        public void CoupleWishReportCoupleStatue(CoupleWishReportStatueData req)
        {
            CoupleWishService.getInstance().CoupleWishReportCoupleStatue(req);
        }
        public int CoupleWishAdmire(int fromRole, int fromZone, int admireType, int toCoupleId)
        {
            return CoupleWishService.getInstance().CoupleWishAdmire(fromRole, fromZone, admireType, toCoupleId);
        }
        public int CoupleWishJoinParty(int fromRole, int fromZone, int toCoupleId)
        {
            return CoupleWishService.getInstance().CoupleWishJoinParty(fromRole, fromZone, toCoupleId);
        }
        #endregion
    }
}
