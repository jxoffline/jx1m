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
using KF.Contract.Data.HuanYingSiYuan;
using Tmsk.Tools;
using System.Configuration;
using KF.Remoting.Data;
using System.Collections.Concurrent;
using System.Collections;
using Tmsk.Contract.Const;

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

        public Persistence Persistence = Persistence.Instance;

        /// <summary>
        /// ServerId,Agent
        /// </summary>
        public ConcurrentDictionary<int, HuanYingSiYuanAgent> ServerId2KuaFuClientAgent = new ConcurrentDictionary<int, HuanYingSiYuanAgent>();

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
        public ConcurrentDictionary<int, HuanYingSiYuanFuBenData> HuanYingSiYuanFuBenDataDict = new ConcurrentDictionary<int, HuanYingSiYuanFuBenData>(1,4096);

        /// <summary>
        /// 全局玩家信息字典
        /// </summary>
        //private Dictionary<int, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new Dictionary<int, KuaFuRoleData>();
        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<string, int> UserId2RoleIdActiveDict = new ConcurrentDictionary<string, int>(1, 16384);

        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> WaitingKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<int, HuanYingSiYuanFuBenData> ShotOfRolesFuBenDataDict = new ConcurrentDictionary<int, HuanYingSiYuanFuBenData>(1,4096);

        public Thread BackgroundThread;
        public Thread UpdateServerConfigThread;

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

            UpdateServerConfigThread = new Thread(UpdateServerConfigThreadProc);
            UpdateServerConfigThread.IsBackground = true;
            UpdateServerConfigThread.Start();
        }

        ~HuanYingSiYuanService()
        {
            BackgroundThread.Abort();
            UpdateServerConfigThread.Abort();
        }

        public void UpdateServerConfigThreadProc(object state)
        {
            do 
            {
                try
                {
                    //同步配置信息
                    Persistence.UpdateServerConfig();

                    Thread.Sleep(20000);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            } while (true);
        }

        public void ThreadProc(object state)
        {
            Persistence.InitConfig();

            DateTime lastRunTime = DateTime.Now;
            do 
            {
                try
                {
                    DateTime now = DateTime.Now;
                    Global.UpdateNowTime(now);

                    //处理游戏逻辑
                    if (now > CheckRoleTimerProcTime)
                    {
                        CheckRoleTimerProcTime = now.AddSeconds(CheckRoleTimerProcInterval);
                        CheckServerLoadProc(now, Persistence.ServerLoadContext);
                        CheckRoleTimerProc(now, Persistence.ServerLoadContext);
                        Persistence.SaveServerLoadData();
                    }

                    if (now > SaveServerStateProcTime)
                    {
                        SaveServerStateProcTime = now.AddSeconds(SaveServerStateProcInterval);
                        SaveServerStateProc();
                    }

                    if (now > CheckGameFuBenTime)
                    {
                        CheckGameFuBenTime = now.AddSeconds(CheckGameFuBenInterval);
                        CheckGameFuBenTimerProc(now);
                    }

                    int sleepMS = (int)((DateTime.Now - now).TotalMilliseconds);
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

        private void CheckRoleTimerProc(DateTime now, ServerLoadContext context)
        {
            context.SignUpRoleCount = 0;
            context.StartGameRoleCount = 0;
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
                    context.SignUpRoleCount++;
                    if (assgionGameFuBen)
                    {
                        assgionGameFuBen = AssignGameFuben(kuaFuRoleData, context, waitTicks1, waitTicks2, now);
                    }
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.SignUpWaiting)
                {
                    context.SignUpRoleCount++;
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.StartGame)
                {
                    context.StartGameRoleCount++;
                }

                if (oldGameId > 0)
                {
                    RemoveRoleFromFuBen(oldGameId, kuaFuRoleData.RoleId);
                    HuanYingSiYuanAgent huanYingSiYuanAgent = GetKuaFuAgent(kuaFuRoleData.ServerId);
                    if (null != huanYingSiYuanAgent)
                    {
                        huanYingSiYuanAgent.NotifyChangeState(kuaFuRoleData);
                    }
                }
            }
        }

        private void CheckServerLoadProc(DateTime now, ServerLoadContext context)
        {
            context.AlivedServerCount = 0;
            context.AlivedGameFuBenCount = 0;
            context.ServerLoadAvg = 0;
            context.IdelActiveServerQueue.Clear();

            foreach (var srv in Persistence.KuaFuServerIdGameConfigDict.Values)
            {
                if (srv.GameType == (int)GameTypes.HuanYingSiYuan)
                {
                    HuanYingSiYuanAgent huanYingSiYuanAgent;
                    if (ServerId2KuaFuClientAgent.TryGetValue(srv.ServerId, out huanYingSiYuanAgent))
                    {
                        int state = 0;
                        int load = huanYingSiYuanAgent.GetAliveGameFuBenCount();
                        if (huanYingSiYuanAgent.IsAlive)
                        {
                            context.AlivedServerCount++;
                            context.AlivedGameFuBenCount += load;
                            state = 1;

                            if (load < srv.Capacity)
                            {
                                context.IdelActiveServerQueue.AddLast(srv);
                            }
                        }

                        Persistence.UpdateServerLoadState(huanYingSiYuanAgent.ClientInfo.ServerId, load, state);
                    }
                }
            }

            context.CalcServerLoadAvg();
        }

        private void SaveServerStateProc()
        {
            foreach (var srv in Persistence.ServerIdServerInfoDict.Values)
            {
                if ((srv.Flags & ServerFlags.KuaFuServer) == 0)
                {
                    HuanYingSiYuanAgent huanYingSiYuanAgent;
                    if (ServerId2KuaFuClientAgent.TryGetValue(srv.ServerId, out huanYingSiYuanAgent))
                    {
                        int state = 0;
                        if (huanYingSiYuanAgent.IsAlive)
                        {
                            state = 1;
                        }

                        Persistence.UpdateServerLoadState(huanYingSiYuanAgent.ClientInfo.ServerId, srv.Load, state);
                    }
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
                    HuanYingSiYuanFuBenData huanYingSiYuanFuBenData;
                    lock (fuBenData)
                    {
                        if (fuBenData.CanRemove())
                        {
                            RemoveGameFuBen(fuBenData, null);
                        }
                        else if (fuBenData.EndTime < canRemoveTime)
                        {
                            RemoveGameFuBen(fuBenData, null);
                        }
                    }
                }
            }
        }

        #endregion 定时处理

        #region 接口实现

        public int PushFuBenSeqId(int serverId, List<int> list)
        {
            HuanYingSiYuanAgent huanYingSiYuanAgent;
            if (ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                return huanYingSiYuanAgent.PushFuBenSeqId(list);
            }
            else
            {
                return StdErrorCode.Error_Server_Not_Registed;
            }
        }

        public AsyncDataItem[] GetClientCacheItems(int serverId)
        {
            HuanYingSiYuanAgent huanYingSiYuanAgent;
            if (ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                return huanYingSiYuanAgent.GetCacheItems();
            }

            return null;
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
            int clientId = -1;
            bool firstInit = false;

            try
            {
                if (clientInfo.GameType == (int)GameTypes.HuanYingSiYuan && clientInfo.ServerId != 0)
                {
                    //如果是第一次初始化,需要清空原始的
                    if (clientInfo.ClientId == 0)
                    {
                        firstInit = true;
                    }

                    Lazy<HuanYingSiYuanAgent> lazy = new Lazy<HuanYingSiYuanAgent>(() => 
                        {
                            clientInfo.ClientId = Persistence.GetUniqueClientId(clientInfo.ServerId);
                            return new HuanYingSiYuanAgent(clientInfo, callback);
                        });

                    HuanYingSiYuanAgent huanYingSiYuanAgent = ServerId2KuaFuClientAgent.GetOrAdd(clientInfo.ServerId, (x) => { return lazy.Value; });
                    if (clientInfo.ClientId != huanYingSiYuanAgent.ClientInfo.ClientId)
                    {
                        if (clientInfo.ClientId > 0)
                        {
                            if (huanYingSiYuanAgent.IsClientAlive())
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("InitializeClient服务器ID重复,禁止连接.ServerId:{0},ClientId:{1}", clientInfo.ServerId, clientInfo.ClientId));
                                return StdErrorCode.Error_Duplicate_Key_ServerId;
                            }
                        }

                        clientId = huanYingSiYuanAgent.ClientInfo.ClientId;
                    }
                    else
                    {
                        clientId = clientInfo.ClientId;
                    }

                    if (firstInit)
                    {
                        huanYingSiYuanAgent.Reset();
                        List<int> list = huanYingSiYuanAgent.GetAliveGameFuBenList();
                        if (null != list && list.Count > 0)
                        {
                            foreach (var id in list)
                            {
                                HuanYingSiYuanFuBenData fuBenData;
                                if (HuanYingSiYuanFuBenDataDict.TryGetValue(id, out fuBenData))
                                {
                                    RemoveGameFuBen(fuBenData, huanYingSiYuanAgent);
                                }
                                else
                                {
                                    huanYingSiYuanAgent.RemoveGameFuBen(id);
                                }
                            }
                        }
                    }
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

            return clientId;
        }

        public int RoleSignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData)
        {
            int result = (int)KuaFuRoleStates.SignUp;
            IKuaFuClient kuaFuClient = null;

            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;
            HuanYingSiYuanAgent huanYingSiYuanAgent;
            if (!ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("RoleSignUp时ServerId错误.ServerId:{0},roleId:{1}", serverId, roleId));
                return -500;
            }

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
                        if (ChangeRoleState(huanYingSiYuanAgent, kuaFuRoleData, KuaFuRoleStates.None) < 0)
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
            HuanYingSiYuanAgent huanYingSiYuanAgent = null;

            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;
            if (!ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                return StdErrorCode.Error_Server_Internal_Error;
            }

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

            HuanYingSiYuanAgent huanYingSiYuanAgent = null;

            //查找对应的代理对象
            if (ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                //更新到新的状态
                if (kuaFuRoleData.GameId == gameId)
                {
                    ChangeRoleState(huanYingSiYuanAgent, kuaFuRoleData, (KuaFuRoleStates)state);
                }
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
            return Persistence.GetKuaFuServerInfoData(age);
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
                        RemoveGameFuBen(huanYingSiYuanFuBenData, null);
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

        private IKuaFuClient GetKuaFuClient(int serverId)
        {
            HuanYingSiYuanAgent huanYingSiYuanAgent;
            if (ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                return huanYingSiYuanAgent.KuaFuClient;
            }

            return null;
        }

        private HuanYingSiYuanAgent GetKuaFuAgent(int serverId)
        {
            HuanYingSiYuanAgent huanYingSiYuanAgent;
            if (ServerId2KuaFuClientAgent.TryGetValue(serverId, out huanYingSiYuanAgent))
            {
                return huanYingSiYuanAgent;
            }

            return null;
        }

        private int ChangeRoleState(HuanYingSiYuanAgent huanYingSiYuanAgent, KuaFuRoleData kuaFuRoleData, KuaFuRoleStates state)
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

                    kuaFuClient = huanYingSiYuanAgent.KuaFuClient;
                    roleData = (KuaFuRoleData)kuaFuRoleData;
                }

                if (oldGameId > 0)
                {
                    RemoveRoleFromFuBen(oldGameId, kuaFuRoleData.RoleId);
                }

                if (null != kuaFuClient && null != roleData)
                {
                    result = huanYingSiYuanAgent.NotifyChangeState(kuaFuRoleData);
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
                lock(fuBenData)
                {
                    int roleCount = fuBenData.RoleDict.Count;
                    foreach (var role in fuBenData.RoleDict.Values)
                    {
                        KuaFuRoleData kuaFuRoleData;
                        if (RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(role.ServerId, role.RoleId), out kuaFuRoleData))
                        {
                            HuanYingSiYuanAgent huanYingSiYuanAgent;
                            if (ServerId2KuaFuClientAgent.TryGetValue(kuaFuRoleData.ServerId, out huanYingSiYuanAgent))
                            {
                                huanYingSiYuanAgent.NotifyFuBenRoleCount(kuaFuRoleData, roleCount);
                            }
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
                            HuanYingSiYuanAgent huanYingSiYuanAgent;
                            if (ServerId2KuaFuClientAgent.TryGetValue(kuaFuRoleData.ServerId, out huanYingSiYuanAgent))
                            {
                                huanYingSiYuanAgent.NotifyEnterGame(kuaFuRoleData);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private bool AssignGameFuben(KuaFuRoleData kuaFuRoleData, ServerLoadContext context, long waitSecs1, long waitSecs2, DateTime now)
        {
            int roleCount = 0;
            DateTime stateEndTime = now.AddSeconds(EnterGameSecs);
            HuanYingSiYuanAgent huanYingSiYuanAgent = null;
            HuanYingSiYuanFuBenData huanYingSiYuanFuBenData = null;
            IKuaFuClient kuaFuClient = null;
            
            KuaFuFuBenRoleData kuaFuFuBenRoleData = new KuaFuFuBenRoleData()
                {
                    ServerId = kuaFuRoleData.ServerId,
                    RoleId = kuaFuRoleData.RoleId,
                    ZhanDouLi =  HuanYingSiYuanGameData.GetZhanDouLi(kuaFuRoleData.GameData),
                };

            List<KuaFuRoleData> updateRoleDataList = new List<KuaFuRoleData>();

            try
            {
                //先检测是否有需要补充人的队伍
                foreach (var fuBenData in ShotOfRolesFuBenDataDict.Values)
                {
                    //分组编号要相等
                    if (fuBenData.CanRemove())
                    {
                        RemoveGameFuBen(fuBenData);
                    }
                    else if (fuBenData.CanEnter(kuaFuRoleData.GroupIndex, waitSecs1, waitSecs2))
                    {
                        if (ServerId2KuaFuClientAgent.TryGetValue(fuBenData.ServerId, out huanYingSiYuanAgent) && huanYingSiYuanAgent.IsAlive)
                        {
                            roleCount = fuBenData.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData, GameFuBenRoleCountChangedHandler);
                            if (roleCount > 0)
                            {
                                huanYingSiYuanFuBenData = fuBenData;
                                break;
                            }
                        }
                    }
                }

                if (null == huanYingSiYuanFuBenData)
                {
                    //按负载状态在一个跨服活动服务器上分配一个新的游戏副本,并加入
                    LinkedListNode<KuaFuServerGameConfig> node = context.IdelActiveServerQueue.First;
                    int count = context.IdelActiveServerQueue.Count;
                    for (int i = 0; i < count && node != null; i++ )
                    {
                        KuaFuServerGameConfig srv = node.Value;
                        LinkedListNode<KuaFuServerGameConfig> next = node.Next;

                        if (ServerId2KuaFuClientAgent.TryGetValue(srv.ServerId, out huanYingSiYuanAgent) && huanYingSiYuanAgent.IsAlive)
                        {
                            int serverLoad = huanYingSiYuanAgent.GetAliveGameFuBenCount();
                            if (serverLoad < srv.Capacity && serverLoad <= context.ServerLoadAvg)
                            {
                                try
                                {
                                    huanYingSiYuanFuBenData = Persistence.CreateHysyGameFuBen(huanYingSiYuanAgent, kuaFuRoleData.GroupIndex, 1);
                                    if (huanYingSiYuanFuBenData != null)
                                    {
                                        AddGameFuBen(huanYingSiYuanFuBenData, huanYingSiYuanAgent);
                                        roleCount = huanYingSiYuanFuBenData.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData, GameFuBenRoleCountChangedHandler);
                                        if (roleCount > 0)
                                        {
                                            context.AlivedGameFuBenCount++;
                                            context.CalcServerLoadAvg();
                                            break;
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    huanYingSiYuanAgent.MaxActiveTicks = 0;
                                }
                            }
                            else
                            {
                                context.IdelActiveServerQueue.Remove(node);
                                if (serverLoad < srv.Capacity)
                                {
                                    context.IdelActiveServerQueue.AddLast(node);
                                }
                            }
                        }

                        node = next;
                    }
                }

                if (huanYingSiYuanFuBenData != null && roleCount > 0)
                {
                    if (roleCount == 1)
                    {
                        huanYingSiYuanFuBenData.EndTime = now; //第一个人进入时,重置副本创建时间
                    }

                    if (huanYingSiYuanFuBenData.State == GameFuBenState.Wait)
                    {
                        if (roleCount == Consts.HuanYingSiYuanRoleCountTotal)
                        {
                            List<KuaFuFuBenRoleData> roleList = huanYingSiYuanFuBenData.SortFuBenRoleList();
                            foreach (var role in roleList)
                            {
                                KuaFuRoleData kuaFuRoleDataTemp;
                                KuaFuRoleKey key = KuaFuRoleKey.Get(role.ServerId, role.RoleId);
                                if (RoleIdKuaFuRoleDataDict.TryGetValue(key, out kuaFuRoleDataTemp))
                                {
                                    kuaFuRoleDataTemp.UpdateStateTime(huanYingSiYuanFuBenData.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                                }
                            }

                            huanYingSiYuanFuBenData.State = GameFuBenState.Start;
                            NotifyFuBenRoleEnterGame(huanYingSiYuanFuBenData);
                        }
                        else
                        {
                            kuaFuRoleData.UpdateStateTime(huanYingSiYuanFuBenData.GameId, KuaFuRoleStates.SignUpWaiting, kuaFuRoleData.StateEndTicks);
                            NotifyFuBenRoleCount(huanYingSiYuanFuBenData);
                        }
                    }
                    else if (huanYingSiYuanFuBenData.State == GameFuBenState.Start)
                    {
                        kuaFuRoleData.UpdateStateTime(huanYingSiYuanFuBenData.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                        NotifyFuBenRoleEnterGame(huanYingSiYuanFuBenData);
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

        private void AddGameFuBen(HuanYingSiYuanFuBenData huanYingSiYuanFuBenData, HuanYingSiYuanAgent huanYingSiYuanAgent)
        {
            HuanYingSiYuanFuBenDataDict[huanYingSiYuanFuBenData.GameId] = huanYingSiYuanFuBenData;

            //增加服务器负载计数
            huanYingSiYuanAgent.AddGameFuBen(huanYingSiYuanFuBenData.GameId);
        }

        private void RemoveGameFuBen(HuanYingSiYuanFuBenData huanYingSiYuanFuBenData, HuanYingSiYuanAgent huanYingSiYuanAgent = null)
        {
            int gameId = huanYingSiYuanFuBenData.GameId;
            HuanYingSiYuanFuBenData huanYingSiYuanFuBenDataTemp;
            ShotOfRolesFuBenDataDict.TryRemove(gameId, out huanYingSiYuanFuBenDataTemp);
            if(HuanYingSiYuanFuBenDataDict.TryRemove(gameId, out huanYingSiYuanFuBenDataTemp))
            {
                huanYingSiYuanFuBenDataTemp.State = GameFuBenState.End;

                //减少服务器负载计数
                if (null == huanYingSiYuanAgent)
                {
                    if (!ServerId2KuaFuClientAgent.TryGetValue(huanYingSiYuanFuBenDataTemp.ServerId, out huanYingSiYuanAgent))
                    {
                        huanYingSiYuanAgent = null;
                    }
                }

                if (null != huanYingSiYuanAgent)
                {
                    huanYingSiYuanAgent.RemoveGameFuBen(huanYingSiYuanFuBenDataTemp.GameId);
                }
            }

            lock (huanYingSiYuanFuBenData)
            {
                foreach (var fuBenRoleData in huanYingSiYuanFuBenData.RoleDict.Values)
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
                            RemoveGameFuBen(huanYingSiYuanFuBenData, null);
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
