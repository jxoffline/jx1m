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
    /*
     *  锁优先级: FuBenData > RoleData
     */

    /// <summary>
    /// 勇者战场Remoting服务对象
    /// </summary>
    public class YongZheZhanChangService : MarshalByRefObject, IYongZheZhanChangService, IExecCommand
    {
        public static YongZheZhanChangService Instance = null;

        /// <summary>
        /// lock对象
        /// </summary>
        private object Mutex = new object();

        public readonly GameTypes YzzcGameType = GameTypes.YongZheZhanChang;
        public readonly GameTypes LhlyGameType = GameTypes.LangHunLingYu;
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

        public YongZheZhanChangPersistence Persistence = YongZheZhanChangPersistence.Instance;

        /// <summary>
        /// 4个其他城池的列表(城池等级对应的展示位置）
        /// </summary>
        public int[] OtherCityLevelList = new int[11] { -1, 0, -1, 1, -1, 2, 3, -1, -1, -1, -1 };

        /// <summary>
        /// 4个其他城池的数组
        /// </summary>
        public Dictionary<int, List<int>> OtherCityList = new Dictionary<int, List<int>>();

        /// <summary>
        /// 现在存在的跨服副本数
        /// </summary>
        public int ExistKuaFuFuBenCount = 0;

        /// <summary>
        /// 进入时间最大值(秒)
        /// </summary>
        private int EnterGameSecs = 60 * 60;

        /// <summary>
        /// 当前活动状态
        /// </summary>
        private int GameState = GameStates.SignUp;
        private bool AssginGameFuBenComplete = false;

        private int RunTimeGameType;

        private DateTime PrepareStartGameTime = DateTime.MinValue;

        /// <summary>
        /// 副本信息
        /// </summary>
        public ConcurrentDictionary<int, YongZheZhanChangFuBenData> FuBenDataDict = new ConcurrentDictionary<int, YongZheZhanChangFuBenData>(1, 4096);

        /// <summary>
        /// 临时场次分配容器
        /// </summary>
        private SortedDictionary<int, YongZheZhanChangGameFuBenPreAssignData> PreAssignGameFuBenDataDict = new SortedDictionary<int, YongZheZhanChangGameFuBenPreAssignData>();

        /// <summary>
        /// 全局玩家信息字典
        /// </summary>
        //private Dictionary<int, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new Dictionary<int, KuaFuRoleData>();
        private ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData> RoleIdKuaFuRoleDataDict = new ConcurrentDictionary<KuaFuRoleKey, KuaFuRoleData>();

        public ConcurrentDictionary<string, int> UserId2RoleIdActiveDict = new ConcurrentDictionary<string, int>(1, 16384);

        /// <summary>
        /// 角色ID可以进入的跨服主线地图的编号字典
        /// </summary>
        private ConcurrentDictionary<int, KuaFuMapRoleData> RoleId2KuaFuMapIdDict = new ConcurrentDictionary<int, KuaFuMapRoleData>();

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

        public YongZheZhanChangService()
        {
            BackgroundThread = new Thread(ThreadProc);
            BackgroundThread.IsBackground = true;
            BackgroundThread.Start();
        }

        ~YongZheZhanChangService()
        {
            BackgroundThread.Abort(); ;
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

                    RunLangHunLingYuTimerProc();

                    //处理游戏逻辑
                    if (now > CheckRoleTimerProcTime)
                    {
                        CheckRoleTimerProcTime = now.AddSeconds(CheckRoleTimerProcInterval);
                        int signUpCount, startCount;
                        lock (Mutex)
                        {
                            CheckRoleTimerProc(now, out signUpCount, out startCount);
                        }
                        ClientAgentManager.Instance().SetGameTypeLoad((GameTypes)RunTimeGameType, signUpCount, startCount);
                    }

                    if (now > CheckGameFuBenTime)
                    {
                        CheckGameFuBenTime = now.AddSeconds(CheckGameFuBenInterval);
                        CheckGameFuBenTimerProc(now);
                        CheckOverTimeLangHunLingYuGameFuBen(now);
                    }

                    Persistence.WriteRoleInfoDataProc();

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

            int gameState;
            lock (Mutex)
            {
                gameState = GameState;
            }

            if (gameState == GameStates.PrepareGame)
            {
                LogManager.WriteLog(LogTypes.Info, "清除上场遗留的活动副本信息,开始统计报名玩家列表");
                FuBenDataDict.Clear();
                PreAssignGameFuBenDataDict.Clear();
            }

            DateTime stateEndTime = now.AddSeconds(EnterGameSecs);
            DateTime removeRoleTime = now.AddSeconds(-EnterGameSecs);
            foreach (var kuaFuRoleData in RoleIdKuaFuRoleDataDict.Values)
            {
                if (kuaFuRoleData.StateEndTicks < removeRoleTime.Ticks)
                {
                    KuaFuRoleData kuaFuRoleDataTemp;
                    RoleIdKuaFuRoleDataDict.TryRemove(KuaFuRoleKey.Get(kuaFuRoleData.ServerId, kuaFuRoleData.RoleId), out kuaFuRoleDataTemp);
                    continue;
                }

                if (kuaFuRoleData.State >= KuaFuRoleStates.SignUp && kuaFuRoleData.State < KuaFuRoleStates.StartGame)
                {
                    signUpCount++;
                    if (kuaFuRoleData.State == KuaFuRoleStates.SignUp)
                    {
                        if (gameState == GameStates.PrepareGame)
                        {
                            AssignGameFubenStep1(kuaFuRoleData, stateEndTime.Ticks);
                        }
                    }
                }
                else if (kuaFuRoleData.State == KuaFuRoleStates.StartGame)
                {
                    startCount++;
                }
            }

            if (gameState == GameStates.PrepareGame)
            {
                LogManager.WriteLog(LogTypes.Info, string.Format("对玩家进行场次分组:SignUpRoleCount={0},StartGameRoleCount={1}", signUpCount, startCount));
                AssignGameFubenStep2();
                AssginGameFuBenComplete = false;
                lock (Mutex)
                {
                    GameState = GameStates.PrepareGame2;
                }
            }
            else if (gameState == GameStates.PrepareGame2)
            {
                if (!AssginGameFuBenComplete)
                {
                    LogManager.WriteLog(LogTypes.Info, string.Format("尝试为场次创建活动副本"));
                    AssginGameFuBenComplete = AssignGameFubenStep3(stateEndTime);
                }
                else
                {
                    lock (Mutex)
                    {
                        GameState = GameStates.SignUp;

                        GameLogItem gameLogItem = new GameLogItem();
                        //gameLogItem.ServerCount = context.AlivedServerCount;
                        //gameLogItem.FubenCount = context.AlivedGameFuBenCount;
                        gameLogItem.SignUpCount = signUpCount;
                        gameLogItem.EnterCount = startCount;
                        gameLogItem.GameType = RunTimeGameType;
                        Persistence.UpdateRoleInfoData(gameLogItem);
                    }
                }
            }
        }

        private void CheckGameFuBenTimerProc(DateTime now)
        {
            if (FuBenDataDict.Count > 0)
            {
                DateTime canRemoveTime = now;
                foreach (var fuBenData in FuBenDataDict.Values)
                {
                    YongZheZhanChangFuBenData fubenData;
                    lock (fuBenData)
                    {
                        if (fuBenData.CanRemove() || fuBenData.EndTime < canRemoveTime)
                        {
                            RemoveGameFuBen(fuBenData);
                        }
                    }
                }
            }
        }

        #endregion 定时处理

        #region 接口实现

        #region 通用

        public AsyncDataItem[] GetClientCacheItems(int serverId)
        {
            return ClientAgentManager.Instance().PickAsyncEvent(serverId, YzzcGameType);
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
                if (clientInfo.ServerId != 0)
                {
                    bool bFirstInit = false;
                    int ret = ClientAgentManager.Instance().InitializeClient(callback, clientInfo, out bFirstInit);
                    if (ret > 0)
                    {
                        if (null != clientInfo.MapClientCountDict && clientInfo.MapClientCountDict.Count > 0)
                        {
                            KuaFuServerManager.UpdateKuaFuLineData(clientInfo.ServerId, clientInfo.MapClientCountDict);
                            ClientAgentManager.Instance().SetMainlinePayload(clientInfo.ServerId, clientInfo.MapClientCountDict.Values.ToList().Sum());
                        }

                        if (bFirstInit)
                        {
                            lock (Mutex)
                            {
                                // GameServer 重启 ---> clientId 变更
                                // 中心重启 ---> LhlyDataSendFlagDict为空
                                // 上述两种情况都会重新发送
                                foreach (var item in LangHunLingYuBangHuiDataExDict.Values)
                                {
                                    AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.UpdateLhlyBhData, item);
                                    ClientAgentManager.Instance().PostAsyncEvent(clientInfo.ServerId, YzzcGameType, evItem);
                                }
                                foreach (var item in LangHunLingYuCityDataExDict)
                                {
                                    if (null != item)
                                    {
                                        AsyncDataItem evItem = new AsyncDataItem(KuaFuEventTypes.UpdateLhlyCityData, item);
                                        ClientAgentManager.Instance().PostAsyncEvent(clientInfo.ServerId, YzzcGameType, evItem);
                                    }
                                }
                                ClientAgentManager.Instance().PostAsyncEvent(clientInfo.ServerId, YzzcGameType,
                                    new AsyncDataItem(KuaFuEventTypes.UpdateLhlyOtherCityList, new Dictionary<int, List<int>>(OtherCityList)));
                                ClientAgentManager.Instance().PostAsyncEvent(clientInfo.ServerId, YzzcGameType,
                                    new AsyncDataItem(KuaFuEventTypes.UpdateLhlyCityOwnerList, GetLangHunLingYuCityOwnerHist()));


                             //   System.Console.WriteLine("send lhlydata" + clientInfo.ServerId + ", " + clientInfo.ClientId);
                            }
                        }
                    }

                    return ret;
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("InitializeClient时GameType错误,禁止连接.ServerId:{0},GameType:{1}", clientInfo.ServerId, clientInfo.GameType));
                    return StdErrorCode.Error_Invalid_GameType;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(string.Format("InitializeClient服务器ID重复,禁止连接.ServerId:{0},ClientId:{1},info:{2}", clientInfo.ServerId, clientInfo.ClientId, clientInfo.Token));
                return StdErrorCode.Error_Server_Internal_Error;
            }
        }

        public static char[] WriteSpaceChars = { ' ' };
        public int ExecuteCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                return StdErrorCode.Error_Invalid_Params;
            }

            string[] args = cmd.Split(WriteSpaceChars, StringSplitOptions.RemoveEmptyEntries);
            return ExecCommand(args);
        }

        public void UpdateStatisticalData(object data)
        {
            Persistence.UpdateRoleInfoData(data);
        }

        #endregion 通用

        #region 勇者战场

        public int RoleSignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData)
        {
            int result = (int)KuaFuRoleStates.SignUp;
            IKuaFuClient kuaFuClient = null;

            if (GameState != GameStates.SignUp)
            {
                return StdErrorCode.Error_Not_In_valid_Time;
            }

            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;
            if (!ClientAgentManager.Instance().ExistAgent(serverId))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("RoleSignUp时ServerId错误.ServerId:{0},roleId:{1}", serverId, roleId));
                return -500;
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
            lock (kuaFuRoleData)
            {
                //更新到新的状态
                kuaFuRoleData.Age++;
                kuaFuRoleData.State = KuaFuRoleStates.SignUp;
                kuaFuRoleData.ServerId = serverId;
                kuaFuRoleData.ZoneId = zoneId;
                kuaFuRoleData.GameData = gameData;
                kuaFuRoleData.GroupIndex = groupIndex;
                kuaFuRoleData.StateEndTicks = Global.NowTime.Ticks;
            }

            return result;
        }

        public int RoleChangeState(int serverId, int roleId, int state)
        {
            //查找对应的代理对象
            KuaFuRoleData kuaFuRoleData;
            if (!ClientAgentManager.Instance().ExistAgent(serverId))
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

            if (oldGameId > 0)
            {
                RemoveRoleFromFuBen(oldGameId, roleId);
            }

            return state;
        }

        public int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state)
        {
            YongZheZhanChangFuBenData fuBenData;
            if (FuBenDataDict.TryGetValue(gameId, out fuBenData))
            {
                lock (fuBenData)
                {
                    KuaFuFuBenRoleData kuaFuFuBenRoleData;
                    if (fuBenData.RoleDict.TryGetValue(roleId, out kuaFuFuBenRoleData))
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

            if (dataType == (int)KuaFuRoleExtendDataTypes.KuaFuRoleState)
            {
                return kuaFuRoleData.State;
            }

            return null;
        }

        public List<KuaFuServerInfo> GetKuaFuServerInfoData(int age)
        {
            return KuaFuServerManager.GetKuaFuServerInfoData(age);
        }

        public IKuaFuFuBenData GetFuBenData(int gameId)
        {
            YongZheZhanChangFuBenData kuaFuFuBenData = null;
            if (FuBenDataDict.TryGetValue(gameId, out kuaFuFuBenData) && kuaFuFuBenData.State < GameFuBenState.End)
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
            YongZheZhanChangFuBenData fubenData = null;

            if (FuBenDataDict.TryGetValue(gameId, out fubenData))
            {
                lock (fubenData)
                {
                    fubenData.State = state;
                    if (state == GameFuBenState.End)
                    {
                        RemoveGameFuBen(fubenData);
                    }
                }
            }
            else
            {
                return StdErrorCode.Error_Not_Exist;
            }

            return result;
        }

        #endregion 勇者战场

        #region 跨服主线地图

        public object GetKuaFuLineDataList(int mapCode)
        {
            return KuaFuServerManager.GetKuaFuLineDataList(mapCode);
        }

        public int EnterKuaFuMap(int serverId, int roleId, int mapCode, int kuaFuLine)
        {
            int kuaFuServerId = KuaFuServerManager.EnterKuaFuMapLine(kuaFuLine, mapCode);
            if (kuaFuServerId > 0)
            {
                KuaFuMapRoleData kuaFuMapRoleData = new KuaFuMapRoleData();
                kuaFuMapRoleData = RoleId2KuaFuMapIdDict.GetOrAdd(roleId, kuaFuMapRoleData);
                kuaFuMapRoleData.ServerId = serverId;
                kuaFuMapRoleData.RoleId = roleId;
                kuaFuMapRoleData.KuaFuMapCode = mapCode;
                kuaFuMapRoleData.KuaFuServerId = kuaFuServerId;
                return kuaFuServerId;
            }

            return StdErrorCode.Error_Server_Connections_Limit;
        }

        public KuaFuMapRoleData GetKuaFuMapRoleData(int roleId)
        {
            KuaFuMapRoleData kuaFuMapRoleData;
            RoleId2KuaFuMapIdDict.TryGetValue(roleId, out kuaFuMapRoleData);
            return kuaFuMapRoleData;
        }

        #endregion 跨服主线地图

        #region 圣域争霸

        /// <summary>
        /// 帮会信息
        /// </summary>
        public Dictionary<long, LangHunLingYuBangHuiDataEx> LangHunLingYuBangHuiDataExDict = new Dictionary<long, LangHunLingYuBangHuiDataEx>();

        /// <summary>
        /// 城池信息,城池ID越小，城池等级越高。10级城池ID为1，9级城池ID为2和3，依此类推
        /// </summary>
        public LangHunLingYuCityDataEx[] LangHunLingYuCityDataExDict = new LangHunLingYuCityDataEx[Consts.LangHunLingYuMaxCityID + 1];

        /// <summary>
        /// 圣域城主历史记录
        /// </summary>
        public List<LangHunLingYuKingHist> LangHunLingYuCityHistList = new List<LangHunLingYuKingHist>();

        /// <summary>
        /// 膜拜数据 rid vs admire_count
        /// </summary>
        public Dictionary<int, int> LangHunLingYuAdmireDict = new Dictionary<int, int>();

        /// <summary>
        /// 圣域争霸副本对象字典
        /// </summary>
        public Dictionary<int, LangHunLingYuFuBenData> LangHunLingYuFuBenDataDict = new Dictionary<int, LangHunLingYuFuBenData>();

        private void RunLangHunLingYuTimerProc()
        {
            DateTime now = TimeUtil.NowDateTime();
            lock (Mutex)
            {
                if (!Persistence.LangHunLingYuInitialized)
                {
                    List<LangHunLingYuBangHuiDataEx> bangHuilist = new List<LangHunLingYuBangHuiDataEx>();
                    List<LangHunLingYuCityDataEx> cityList = new List<LangHunLingYuCityDataEx>();
                    List<LangHunLingYuKingHist> cityOwnerHistList = new List<LangHunLingYuKingHist>();
                    if (Persistence.LoadBangHuiDataExList(bangHuilist) && Persistence.LoadCityDataExList(cityList)
                        && Persistence.LoadCityOwnerHistory(cityOwnerHistList))
                    {
                        foreach (var b in bangHuilist)
                        {
                            LangHunLingYuBangHuiDataEx bangHuiDataEx;
                            if (!LangHunLingYuBangHuiDataExDict.TryGetValue(b.Bhid, out bangHuiDataEx))
                            {
                                LangHunLingYuBangHuiDataExDict[b.Bhid] = b;
                            }
                            else
                            {
                                bangHuiDataEx.Bhid = b.Bhid;
                                bangHuiDataEx.BhName = b.BhName;
                                bangHuiDataEx.ZoneId = b.ZoneId;
                            }
                        }

                        foreach (var c in cityList)
                        {
                            if (LangHunLingYuCityDataExDict[c.CityId] == null)
                            {
                                LangHunLingYuCityDataExDict[c.CityId] = c;
                            }
                            else
                            {
                                LangHunLingYuCityDataExDict[c.CityId].CityId = c.CityId;
                                LangHunLingYuCityDataExDict[c.CityId].CityLevel = c.CityLevel;
                                Array.Copy(c.Site, LangHunLingYuCityDataExDict[c.CityId].Site, LangHunLingYuCityDataExDict[c.CityId].Site.Length);
                            }
                        }

                        // 圣域城主历史记录
                        foreach (var d in cityOwnerHistList)
                        {
                            int admire_count = 0;
                            if (!LangHunLingYuAdmireDict.TryGetValue(d.rid, out admire_count))
                            {
                                LangHunLingYuAdmireDict[d.rid] = d.AdmireCount;
                            }
                        }
                        LangHunLingYuCityHistList = cityOwnerHistList;
                        //NotifyUpdateCityOwnerHist(GetLangHunLingYuCityOwnerHist());

                        CalcBangHuiCityLevel();
                        Persistence.LangHunLingYuInitialized = true;
                    }
                }

                //如果当前时间在设置的重置城池攻击者信息时间，且和上次重设的时间差超过1小时（容错）
                double secs = (now.TimeOfDay - Persistence.LangHunLingYuResetCityTime).TotalSeconds;
                if (secs >= 0 && secs < 300 && (now - Persistence.LastLangHunLingYuResetCityTime).TotalHours >= 1)
                {
                    Persistence.LastLangHunLingYuResetCityTime = now;
                    foreach (var c in LangHunLingYuCityDataExDict)
                    {
                        bool reset = false;
                        if (c != null)
                        {
                            c.GameId = 0;
                            for (int i = Consts.LangHunLingYuCityAttackerSite; i < Consts.LangHunLingYuCitySiteCount; i++)
                            {
                                if (c.Site[i] > 0)
                                {
                                    c.Site[i] = 0;
                                    reset = true;
                                }
                            }
                            if (reset)
                            {
                                LogManager.WriteLog(LogTypes.Info, string.Format("清空城池{0}进攻者状态", c.CityId));
                                Persistence.SaveCityData(c);
                                NotifyUpdateCityDataEx(c);
                            }
                        }
                    }

                    // 检查历届圣域城主信息
                    FilterOwnerHistListData();
                }

                //更新4个其他城池
                //int offsetDay = DataHelper2.GetOffsetDay(now);
                //if (Persistence.OtherListUpdateOffsetDay != offsetDay)
                //{
                //    Persistence.OtherListUpdateOffsetDay = offsetDay;
                //    UpdateOtherCityList(true);
                //}
            }
        }

        private int GetCityLevelById(int cityId)
        {
            int cityLevel = Consts.MaxLangHunCityLevel - (int)Math.Log(cityId, 2);
            return cityLevel;
        }

        private void NotifyUpdatBangHuiDataEx(LangHunLingYuBangHuiDataEx bangHuiDataEx)
        {
            Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyBhData, bangHuiDataEx));
        }

        private void NotifyUpdateCityDataEx(LangHunLingYuCityDataEx cityDataEx)
        {
            Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyCityData, cityDataEx));
        }

        private void NotifyUpdateOtherCityList(Dictionary<int, List<int>> list)
        {
            Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyOtherCityList, list));
        }

        private void NotifyUpdateCityOwnerHist(List<LangHunLingYuKingHist> list)
        {
            Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyCityOwnerList, list));
        }

        private void NotifyUpdateCityOwnerAdmire(int rid, int admirecount)
        {
            Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyCityOwnerAdmire, rid, admirecount));
        }

        /// <summary>
        /// 获取指定等级的城池ID列表,优先排列有占领者的
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int CalcNeedNextLevelCityCount(int cityLevel, int maxAttackerPerCity)
        {
            if (cityLevel <= 0)
            {
                return Consts.LangHunLingYuMaxCityID;
            }

            int cityWithOwnerCount = 0;
            int minCityId = 1 << (Consts.MaxLangHunCityLevel - cityLevel);
            int maxCityId = minCityId * 2;
            for (int i = minCityId; i < maxCityId; i++)
            {
                if (LangHunLingYuCityDataExDict[i] != null)
                {
                    if (LangHunLingYuCityDataExDict[i].Site[0] > 0)
                    {
                        cityWithOwnerCount++;
                    }
                }
            }

            return (int)Math.Ceiling(((double)cityWithOwnerCount) / maxAttackerPerCity);
        }


        /// <summary>
        /// 获取指定等级的城池ID列表,优先排列有占领者的
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<int> GetRandomCityListByLevel(int cityLevel, int reserveCount)
        {
            List<int> list = new List<int>();
            int cityWithOwnerCount = 0;
            int minCityId = 1 << (Consts.MaxLangHunCityLevel - cityLevel);
            int maxCityId = minCityId * 2;

            HashSet<int> cityHashSet = new HashSet<int>();
            //先加有人的
            for (int i = minCityId; i < maxCityId; i++)
            {
                if (LangHunLingYuCityDataExDict[i] != null)
                {
                    if (LangHunLingYuCityDataExDict[i].Site.Any((x) => x > 0))
                    {
                        cityHashSet.Add(i);
                        cityWithOwnerCount++;
                    }
                }
            }

            //乱序排列
            int[] array = cityHashSet.ToArray();
            foreach (var cityId in array)
            {
                int rnd = Global.GetRandomNumber(0, list.Count);
                list.Insert(rnd, cityId);
            }

            //如果不够，依城池ID顺序补充
            if (cityWithOwnerCount < reserveCount)
            {
                for (int i = minCityId; i < maxCityId; i++)
                {
                    if (!cityHashSet.Contains(i))
                    {
                        list.Add(i);
                        cityWithOwnerCount++;
                        if (cityWithOwnerCount >= reserveCount)
                        {
                            break;
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 无人进攻10级城城主自动连任的情况，跨服中心不处理。
        /// 此处自动生成补齐数据
        /// </summary>
        private void FilterOwnerHistListData()
        {
            lock (Mutex)
            {
                if (LangHunLingYuCityHistList == null || LangHunLingYuCityHistList.Count == 0)
                    return;

                CityLevelInfo sceneItem = null;
                if (!Persistence.CityLevelInfoDict.TryGetValue(Consts.MaxLangHunCityLevel, out sceneItem))
                    return;

                bool NeedCheckData = false; // 是否需要检查数据
                DateTime now = TimeUtil.NowDateTime();
                for (int loop = 0; loop < sceneItem.AttackWeekDay.Length; ++loop)
                {
                    if ((int)now.DayOfWeek == sceneItem.AttackWeekDay[loop])
                    {
                        NeedCheckData = true;
                        break;
                    }
                }

                // 无需检查
                if (!NeedCheckData)
                    return;

                // 当前圣域领主信息
                LangHunLingYuKingHist CurKingHist = LangHunLingYuCityHistList[LangHunLingYuCityHistList.Count - 1];
                DateTime NowDate = new DateTime(now.Year, now.Month, now.Day); // 年月日
                DateTime ComDate = new DateTime(CurKingHist.CompleteTime.Year, CurKingHist.CompleteTime.Month, CurKingHist.CompleteTime.Day);
                if (DateTime.Compare(NowDate, ComDate) == 0)
                    return;

                // 复制一份上次的圣域领主数据添加
                CurKingHist.CompleteTime = now;
                LangHunLingYuCityHistList.Add(CurKingHist);

                // save to db
                Persistence.InsertCityOwnerHistory(CurKingHist);
                NotifyUpdateCityOwnerHist(GetLangHunLingYuCityOwnerHist());
            }
        }

        /// <summary>
        /// 统计圣域城主历史记录
        /// </summary>
        private List<LangHunLingYuKingHist> GetLangHunLingYuCityOwnerHist()
        {
            List<LangHunLingYuKingHist> CityHistList = null;
            lock (Mutex)
            {
                // 获取上限为Consts.LangHunLingYuAdmireHistCount
                if (LangHunLingYuCityHistList.Count != 0 && LangHunLingYuCityHistList.Count > Consts.LangHunLingYuAdmireHistCount)
                {
                    int index = LangHunLingYuCityHistList.Count - Consts.LangHunLingYuAdmireHistCount;
                    CityHistList = LangHunLingYuCityHistList.GetRange(index, Consts.LangHunLingYuAdmireHistCount);
                }
                else
                {
                    CityHistList = new List<LangHunLingYuKingHist>(LangHunLingYuCityHistList);
                }
            }
            return CityHistList;
        }

        /// <summary>
        /// 狼魂领域膜拜
        /// </summary>
        public bool LangHunLingYuAdmaire(int rid)
        {
            lock (Mutex)
            {
                if (!Persistence.LangHunLingYuInitialized)
                {
                    return false;
                }

                int admire_count = 0;
                if (!LangHunLingYuAdmireDict.TryGetValue(rid, out admire_count))
                    return false;

                // 更新数据
                LangHunLingYuAdmireDict[rid] = ++admire_count;
                foreach (var d in LangHunLingYuCityHistList)
                {
                    if (d.rid == rid)
                    {
                        d.AdmireCount++;
                    }
                }

                // save to db
                Persistence.AdmireCityOwner(rid);
                NotifyUpdateCityOwnerAdmire(rid, admire_count);
            }
            return true;
        }

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
            int result = 0;
            try
            {
                lock (Mutex)
                {
                    if (!Persistence.LangHunLingYuInitialized)
                    {
                        result = StdErrorCode.Error_Server_Busy;
                        return result;
                    }

                    LangHunLingYuBangHuiDataEx banghuiDataEx;
                    if (!LangHunLingYuBangHuiDataExDict.TryGetValue(bhid, out banghuiDataEx))
                    {
                        banghuiDataEx = new LangHunLingYuBangHuiDataEx() { Bhid = bhid, BhName = bhName, ZoneId = zoneId };
                        LangHunLingYuBangHuiDataExDict[bhid] = banghuiDataEx;
                        NotifyUpdatBangHuiDataEx(banghuiDataEx);
                        Persistence.SaveBangHuiData(banghuiDataEx);
                    }
                    else if (banghuiDataEx.BhName != bhName)
                    {
                        banghuiDataEx.BhName = bhName;
                        NotifyUpdatBangHuiDataEx(banghuiDataEx);
                        Persistence.SaveBangHuiData(banghuiDataEx);
                    }

                    int cityLevel = banghuiDataEx.Level + 1; //报名下一级
                    if (cityLevel > Consts.MaxLangHunCityLevel)
                    {
                        //已经达到最高级城池，不能再申请进攻
                        result = StdErrorCode.Error_Reach_Max_Level;
                        return result;
                    }

                    int attackerCountLimit = Consts.LangHunLingYuCitySiteCount - 1;//默认3个
                    CityLevelInfo ci;
                    if (Persistence.CityLevelInfoDict.TryGetValue(cityLevel, out ci))
                    {
                        attackerCountLimit = ci.MaxNum;
                    }

                    //根据城池状况计算评分，评分高者分配为本次战斗城池
                    int toCityId = -1;
                    int toCitySite = 0;
                    int reserveCount = CalcNeedNextLevelCityCount(cityLevel - 1, attackerCountLimit);
                    List<int> cityList = GetRandomCityListByLevel(cityLevel, reserveCount);
                    foreach (var cityId in cityList)
                    {
                        if (LangHunLingYuCityDataExDict[cityId] == null)
                        {
                            if (toCityId < 0)
                            {
                                toCityId = cityId;
                                toCitySite = Consts.LangHunLingYuCityAttackerSite;
                            }
                        }
                        else
                        {
                            for (int j = Consts.LangHunLingYuCityAttackerSite; j <= attackerCountLimit; j++)
                            {
                                if (LangHunLingYuCityDataExDict[cityId].Site[j] == bhid)
                                {
                                    //已报名
                                    result = StdErrorCode.Error_ZhanMeng_Has_SignUp;
                                    return result;
                                }
                                if (toCityId < 0)
                                {
                                    if (LangHunLingYuCityDataExDict[cityId].Site[j] == 0)
                                    {
                                        toCityId = cityId;
                                        toCitySite = j;
                                    }
                                }
                            }
                        }
                    }

                    if (toCityId >= 0)
                    {
                        if (LangHunLingYuCityDataExDict[toCityId] == null)
                        {
                            LangHunLingYuCityDataExDict[toCityId] = new LangHunLingYuCityDataEx() { CityId = toCityId, CityLevel = cityLevel };
                        }
                        LangHunLingYuCityDataExDict[toCityId].Site[toCitySite] = bhid;

                        LangHunLingYuCityDataEx data = LangHunLingYuCityDataExDict[toCityId].Clone() as LangHunLingYuCityDataEx;

                        //保存城池数据
                        Persistence.SaveCityData(data);
                        NotifyUpdateCityDataEx(data);
                    }
                    else
                    {
                        result = StdErrorCode.Error_Player_Count_Limit;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                result = StdErrorCode.Error_Server_Internal_Error;
            }

            return result;
        }

        private void CalcBangHuiCityLevel(HashSet<long> reCalcBangHuiLevelHashSet = null, bool broadcast = false)
        {
            lock (Mutex)
            {
                Dictionary<long, int> dict = new Dictionary<long, int>();
                if (null != reCalcBangHuiLevelHashSet)
                {
                    foreach (var bhid in reCalcBangHuiLevelHashSet)
                    {
                        dict[bhid] = 0;
                    }
                }
                else
                {
                    foreach (var bhid in LangHunLingYuBangHuiDataExDict.Keys)
                    {
                        dict[bhid] = 0;
                    }
                }
                foreach (var cityDataEx in LangHunLingYuCityDataExDict)
                {
                    if (null != cityDataEx)
                    {
                        long bhid = cityDataEx.Site[0];
                        int level;
                        if (bhid > 0 && dict.TryGetValue(bhid, out level))
                        {
                            if (cityDataEx.CityLevel > level)
                            {
                                dict[bhid] = cityDataEx.CityLevel;
                            }
                        }
                    }
                }
                foreach (var kv in dict)
                {
                    LangHunLingYuBangHuiDataEx bangHuiDataEx;
                    if (LangHunLingYuBangHuiDataExDict.TryGetValue(kv.Key, out bangHuiDataEx))
                    {
                        if (bangHuiDataEx.Level != kv.Value)
                        {
                            bangHuiDataEx.Level = kv.Value;
                            Persistence.SaveBangHuiData(bangHuiDataEx);
                            if (broadcast)
                            {
                                Broadcast2GsAgent(new AsyncDataItem(KuaFuEventTypes.UpdateLhlyBhData, bangHuiDataEx.Clone() as LangHunLingYuBangHuiDataEx));
                            }
                        }
                    }
                }
            }
        }

        private void UpdateOtherCityList(bool broadcast = false)
        {
            lock (Mutex)
            {
                Dictionary<int, List<int>> dict = new Dictionary<int, List<int>>();
                foreach (var cityDataEx in LangHunLingYuCityDataExDict)
                {
                    if (null != cityDataEx && OtherCityLevelList[cityDataEx.CityLevel] >= 0)
                    {
                        int c0 = cityDataEx.Site.Count(x => { return x > 0; });
                        if (c0 > 0)
                        {
                            List<int> list;
                            if (!dict.TryGetValue(cityDataEx.CityLevel, out list))
                            {
                                list = new List<int>();
                                dict[cityDataEx.CityLevel] = list;
                            }

                            list.Add(cityDataEx.CityId);
                        }
                    }
                }

                string log = "";
                foreach (var kv in dict)
                {
                    int index = OtherCityLevelList[kv.Key];
                    int rnd = Global.GetRandomNumber(0, kv.Value.Count);
                    int cityId = kv.Value[rnd];

                    List<int> list;
                    if (!OtherCityList.TryGetValue(kv.Key, out list))
                    {
                        list = new List<int>();
                        OtherCityList[kv.Key] = list;
                    }

                    list.Clear();
                    list.Add(cityId);
                    if (kv.Value.Count >= 2)
                    {
                        if (rnd > 0)
                        {
                            list.Add(kv.Value[rnd - 1]);
                        }
                        else
                        {
                            list.Add(kv.Value[rnd + 1]);
                        }
                    }

                    log += string.Format("Level={0}:{1},{2};", kv.Key, kv.Value.Count >= 1 ? kv.Value[0] : 0, kv.Value.Count >= 2 ? kv.Value[1] : 0);
                }

                LogManager.WriteLog(LogTypes.Info, string.Format("重新计算他人城池展示列表{0}", log));
                if (broadcast)
                {
                    NotifyUpdateOtherCityList(new Dictionary<int, List<int>>(OtherCityList));
                }
            }
        }

        /// <summary>
        /// 一场活动结束结果
        /// </summary>
        /// <param name="cityId">城池ID</param>
        /// <param name="owner">占领方帮会ID</param>
        /// <returns></returns>
        public int GameFuBenComplete(LangHunLingYuStatisticalData data)
        {
            int result = 0;
            try
            {
                int cityId = data.CityId;
                int[] siteBhid = data.SiteBhids;
                lock (Mutex)
                {
                    if (!Persistence.LangHunLingYuInitialized)
                    {
                        result = StdErrorCode.Error_Server_Busy;
                        return result;
                    }

                    int hours = (int)(TimeUtil.NowDateTime() - data.CompliteTime).TotalHours;
                    bool setAttacker = true;
                    if (data.GameId > 0 && Math.Abs(hours) >= 20)
                    {
                        setAttacker = false;
                        LogManager.WriteLog(LogTypes.Error, string.Format("更新城池占领者,CityID={0},占领者={1},但时间已超过预期时间{2}小时,将不重置进攻者", data.CityId, data.SiteBhids[0], hours));
                    }

                    //需要重新计算帮会占领的最高城池等级的帮会集合
                    HashSet<long> reCalcBangHuiLevelHashSet = new HashSet<long>();
                    LangHunLingYuCityDataEx cityDataEx = LangHunLingYuCityDataExDict[cityId];
                    if (null == cityDataEx)
                    {
                        cityDataEx = LangHunLingYuCityDataExDict[cityId] = new LangHunLingYuCityDataEx() { CityId = cityId, CityLevel = GetCityLevelById(cityId) };
                    }

                    //记录新的占领帮会，清除进攻方
                    for (int i = 0; i < cityDataEx.Site.Length; i++)
                    {
                        if (setAttacker || i == 0)
                        {
                            LogManager.WriteLog(LogTypes.Info, string.Format("更新城池信息,CityID={0},Site={1},old bhid={2},new bhid={3}", cityDataEx.CityId, i, cityDataEx.Site[i], siteBhid[i]));
                            if (cityDataEx.Site[i] > 0 && !reCalcBangHuiLevelHashSet.Contains(cityDataEx.Site[i])) reCalcBangHuiLevelHashSet.Add(cityDataEx.Site[i]);
                            cityDataEx.Site[i] = siteBhid[i];
                            if (cityDataEx.Site[i] > 0 && !reCalcBangHuiLevelHashSet.Contains(cityDataEx.Site[i])) reCalcBangHuiLevelHashSet.Add(cityDataEx.Site[i]);
                        }
                    }

                    CalcBangHuiCityLevel(reCalcBangHuiLevelHashSet, true);
                    cityDataEx = cityDataEx.Clone() as LangHunLingYuCityDataEx;

                    // save to db
                    Persistence.SaveCityData(cityDataEx);
                    NotifyUpdateCityDataEx(cityDataEx);

                    // 圣域城主历史记录
                    if (cityDataEx.CityLevel == Consts.MaxLangHunCityLevel && data.CityOwnerRoleData != null)
                    {
                        int admire_count = 0;
                        if (!LangHunLingYuAdmireDict.TryGetValue(data.rid, out admire_count))
                        {
                            LangHunLingYuAdmireDict[data.rid] = admire_count;
                        }

                        LangHunLingYuKingHist CityOwnerData = new LangHunLingYuKingHist()
                        {
                            rid = data.rid,
                            AdmireCount = admire_count,
                            CompleteTime = data.CompliteTime,
                            CityOwnerRoleData = data.CityOwnerRoleData
                        };

                        LangHunLingYuCityHistList.Add(CityOwnerData);

                        // save to db
                        Persistence.InsertCityOwnerHistory(CityOwnerData);
                        NotifyUpdateCityOwnerHist(GetLangHunLingYuCityOwnerHist());
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                result = StdErrorCode.Error_Server_Internal_Error;
            }

            return result;
        }

        private bool CreateLangHunLingYuGameFuBen(LangHunLingYuFuBenData fuBenData, DateTime stateEndTime)
        {
            try
            {
                int magicRoleCount = (int)GameTypes.LangHunLingYu;

                int gameId = Persistence.GetNextGameId();
                int kfSrvId = 0;
                bool createSuccess = ClientAgentManager.Instance().AssginKfFuben(LhlyGameType, gameId, magicRoleCount, out kfSrvId);
                if (createSuccess)
                {
                    fuBenData.ServerId = kfSrvId;
                    fuBenData.GameId = gameId;
                    fuBenData.EndTime = Global.NowTime.AddMinutes(Consts.YongZheZhanChangGameFuBenMaxExistMinutes);
                    AddLangHunLingYuGameFuBen(fuBenData);
                    LogManager.WriteLog(LogTypes.Info, string.Format("创建圣域争霸副本GameID={0},CityId={1},ServerID={2}", fuBenData.GameId, fuBenData.CityId, fuBenData.ServerId));
                    Persistence.LogCreateYongZheFuBen(kfSrvId, gameId, 0, magicRoleCount);
                }
                else
                {
                    //如果分配失败,则返回false,本轮不在尝试分配
                    LogManager.WriteLog(LogTypes.Error, string.Format("暂时没有可用的服务器可以给活动副本分配,稍后重试"));
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
        /// 根据城池ID获取副本信息
        /// </summary>
        /// <param name="gameId">作为CityId使用</param>
        /// <returns></returns>
        public LangHunLingYuFuBenData GetLangHunLingYuGameFuBenDataByCityId(int cityId)
        {
            LangHunLingYuFuBenData result = null;

            try
            {
                int cityLevel = GetCityLevelById(cityId);
                lock (Mutex)
                {
                    LangHunLingYuCityDataEx cityDataEx = LangHunLingYuCityDataExDict[cityId];
                    if (null == cityDataEx)
                    {
                        return null;
                    }
                    if (!LangHunLingYuFuBenDataDict.TryGetValue(cityDataEx.GameId, out result))
                    {
                        result = new LangHunLingYuFuBenData();
                        result.CityId = cityId;
                        if (!CreateLangHunLingYuGameFuBen(result, Global.NowTime.AddHours(1)))
                        {
                            return null;
                        }
                        else
                        {
                            cityDataEx.GameId = result.GameId;
                            Persistence.SaveCityData(cityDataEx);
                            NotifyUpdateCityDataEx(result.CityDataEx);
                        }
                    }

                    result.CityDataEx = cityDataEx.Clone() as LangHunLingYuCityDataEx;
                    return result;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                result = null;
            }

            return result;
        }

        /// <summary>
        /// 根据GameID获取副本信息
        /// </summary>
        /// <param name="gameId">作为CityId使用</param>
        /// <returns></returns>
        public LangHunLingYuFuBenData GetLangHunLingYuGameFuBenData(int gameId)
        {
            LangHunLingYuFuBenData result = null;

            try
            {
                lock (Mutex)
                {
                    if (!LangHunLingYuFuBenDataDict.TryGetValue(gameId, out result))
                    {
                        return null;
                    }

                    return result;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                result = null;
            }

            return result;
        }

        private void AddLangHunLingYuGameFuBen(LangHunLingYuFuBenData fuBenData)
        {
            lock (Mutex)
            {
                LangHunLingYuFuBenDataDict[fuBenData.GameId] = fuBenData;
            }
        }

        private void CheckOverTimeLangHunLingYuGameFuBen(DateTime now)
        {
            lock (Mutex)
            {
                List<LangHunLingYuFuBenData> removeList = new List<LangHunLingYuFuBenData>();
                foreach (var fubenData in LangHunLingYuFuBenDataDict.Values)
                {
                    if (now > fubenData.EndTime)
                    {
                        removeList.Add(fubenData);
                    }
                }

                foreach (var fuBenData in removeList)
                {
                    int gameId = fuBenData.GameId;
                    if (LangHunLingYuFuBenDataDict.Remove(gameId))
                    {
                        fuBenData.State = GameFuBenState.End;
                        ClientAgentManager.Instance().RemoveKfFuben(LhlyGameType, fuBenData.ServerId, fuBenData.GameId);
                    }
                }
            }
        }

        #endregion 圣域争霸

        #endregion 接口实现

        #region 辅助函数

        #region 广播事件

        /// <summary>
        ///  给所有的Agent广播事件
        /// </summary>
        private void Broadcast2GsAgent(AsyncDataItem item)
        {
            ClientAgentManager.Instance().BroadCastAsyncEvent(YzzcGameType, item);
        }

        #endregion

        private void ChangeRoleState(KuaFuRoleData kuaFuRoleData, KuaFuRoleStates state)
        {
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
                    ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, YzzcGameType, evItem);
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private void NotifyFuBenRoleCount(YongZheZhanChangFuBenData fuBenData)
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
                            ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, YzzcGameType, evItem);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void NotifyFuBenRoleEnterGame(YongZheZhanChangFuBenData fuBenData)
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
                            ClientAgentManager.Instance().PostAsyncEvent(kuaFuRoleData.ServerId, YzzcGameType, evItem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
            }
        }

        private void AssignGameFubenStep1(KuaFuRoleData kuaFuRoleData, long endStateTicks)
        {
            YongZheZhanChangGameFuBenPreAssignData PreAssignData;
            KuaFuFuBenRoleData kuaFuFuBenRoleData = new KuaFuFuBenRoleData()
                {
                    ServerId = kuaFuRoleData.ServerId,
                    RoleId = kuaFuRoleData.RoleId,
                    ZhanDouLi = kuaFuRoleData.ZhanDouLi,
                };

            if (!PreAssignGameFuBenDataDict.TryGetValue(kuaFuRoleData.GroupIndex, out PreAssignData))
            {
                PreAssignData = new YongZheZhanChangGameFuBenPreAssignData();
                PreAssignGameFuBenDataDict.Add(kuaFuRoleData.GroupIndex, PreAssignData);
            }

            if (null == PreAssignData.RemainFuBenData)
            {
                PreAssignData.RemainFuBenData = new YongZheZhanChangFuBenData() { GroupIndex = kuaFuRoleData.GroupIndex, };
            }

            int roleCount = PreAssignData.RemainFuBenData.AddKuaFuFuBenRoleData(kuaFuFuBenRoleData);
            if (roleCount >= Persistence.MinEnterCount)
            {
                PreAssignData.FullFuBenDataList.Add(PreAssignData.RemainFuBenData);
                PreAssignData.RemainFuBenData = null;
            }

            kuaFuRoleData.UpdateStateTime(0, KuaFuRoleStates.SignUpWaiting, endStateTicks);
        }

        private void AssignGameFubenStep2()
        {
            List<int> keyList = PreAssignGameFuBenDataDict.Keys.Reverse().ToList();
            int count = keyList.Count;
            for (int i = 0; i < count; i++)
            {
                YongZheZhanChangGameFuBenPreAssignData previous = null;
                YongZheZhanChangGameFuBenPreAssignData current = null;
                current = PreAssignGameFuBenDataDict[keyList[i]];
                if (i > 0)
                    previous = PreAssignGameFuBenDataDict[keyList[i - 1]];

                if (previous != null && previous.RemainFuBenData != null)
                {
                    int fc = current.FullFuBenDataList.Count;
                    int rc = previous.RemainFuBenData.RoleDict.Count;
                    List<int> roleList = previous.RemainFuBenData.RoleDict.Keys.ToList();
                    if (fc > 0)
                    {
                        //平均将previous.RemainFuBenData中的角色分配到current.FullFuBenDataList里的副本中
                        for (int m = 0; m < fc; m++)
                        {
                            YongZheZhanChangFuBenData fuBenData = current.FullFuBenDataList[m];
                            for (int n = m; n < rc; n += fc)
                            {
                                fuBenData.AddKuaFuFuBenRoleData(previous.RemainFuBenData.RoleDict[roleList[n]]);
                            }
                        }
                    }
                    else
                    {
                        if (null != current.RemainFuBenData)
                        {
                            //将previous.RemainFuBenData中的角色添加到current.RemainFuBenData中
                            foreach (var r in previous.RemainFuBenData.RoleDict.Values)
                            {
                                if (current.RemainFuBenData.AddKuaFuFuBenRoleData(r) >= Persistence.MinEnterCount)
                                {
                                    // 防止多个档次的副本在第一步都没有分配满，然后就一直迭代的分配到最后一个档次的RemainFuBenData副本中, 人数有可能超出
                                    var tmp = new YongZheZhanChangFuBenData() { GroupIndex = current.RemainFuBenData.GroupIndex };
                                    current.FullFuBenDataList.Add(current.RemainFuBenData);
                                    current.RemainFuBenData = tmp;
                                }
                            }
                        }
                        else
                        {
                            current.RemainFuBenData = previous.RemainFuBenData;
                            current.RemainFuBenData.GroupIndex = keyList[i];
                        }
                    }

                    previous.RemainFuBenData = null;
                }

                // 最后一个档次的非满副本
                if (current.RemainFuBenData != null && i == count - 1 && current.RemainFuBenData.GetFuBenRoleCount() > 0)
                {
                    if (current.RemainFuBenData.GetFuBenRoleCount() >= Persistence.MinEnterCount)
                    {
                        current.FullFuBenDataList.Add(current.RemainFuBenData);
                    }
                    else if (current.FullFuBenDataList.Count <= 0)
                    {
                        // 这里有个比较蛋疼的问题，所以单独把这个分支抽出来
                        // 可能会出现这个副本里面的人数非常非常少
                        current.FullFuBenDataList.Add(current.RemainFuBenData);
                    }
                    else
                    {
                        int fc = current.FullFuBenDataList.Count;
                        int rc = current.RemainFuBenData.RoleDict.Count;
                        List<int> roleList = current.RemainFuBenData.RoleDict.Keys.ToList();

                        //平均将current.RemainFuBenData中的角色分配到current.FullFuBenDataList里的副本中
                        for (int m = 0; m < fc; m++)
                        {
                            YongZheZhanChangFuBenData fuBenData = current.FullFuBenDataList[m];
                            for (int n = m; n < rc; n += fc)
                            {
                                fuBenData.AddKuaFuFuBenRoleData(current.RemainFuBenData.RoleDict[roleList[n]]);
                            }
                        }
                    }

                    current.RemainFuBenData = null;
                }
            }
        }

        private bool AssignGameFubenStep3(DateTime stateEndTime)
        {
            DateTime nextTime = TimeUtil.NowDateTime();
            DateTime now;
            int count = 0;
            foreach (var preAssignData in PreAssignGameFuBenDataDict.Values)
            {
                foreach (var fubenData in preAssignData.FullFuBenDataList)
                {
                    if (fubenData.State < GameFuBenState.Start)
                    {
                        if (fubenData.GetFuBenRoleCount() <= 0) continue;

                        if (!CreateGameFuBenOnServer(fubenData, stateEndTime))
                        {
                            return false;
                        }

                        //按照勇者战场实际情况,控制分配速度,每秒不超过15个
                        count++;
                        if (count >= 15)
                        {
                            count = 0;
                            now = TimeUtil.NowDateTime();
                            if (now < nextTime)
                            {
                                Thread.Sleep((int)(nextTime - now).TotalMilliseconds);
                            }
                            else
                            {
                                nextTime = now.AddSeconds(1);
                            }
                        }
                    }
                }

                preAssignData.FullFuBenDataList.Clear();

                if (null != preAssignData.RemainFuBenData && preAssignData.RemainFuBenData.GetFuBenRoleCount() > 0)
                {
                    if (preAssignData.RemainFuBenData.State < GameFuBenState.Start)
                    {
                        if (!CreateGameFuBenOnServer(preAssignData.RemainFuBenData, stateEndTime))
                        {
                            return false;
                        }
                    }

                    preAssignData.RemainFuBenData = null;
                }
            }

            return true;
        }

        private bool CreateGameFuBenOnServer(YongZheZhanChangFuBenData fuBenData, DateTime stateEndTime)
        {
            try
            {
                int gameId = Persistence.GetNextGameId();
                int kfSrvId = 0;
                bool createSuccess = ClientAgentManager.Instance().AssginKfFuben(YzzcGameType, gameId, fuBenData.GetFuBenRoleCount(), out kfSrvId);
                if (createSuccess)
                {
                    fuBenData.ServerId = kfSrvId;
                    fuBenData.GameId = gameId;
                    fuBenData.EndTime = Global.NowTime.AddMinutes(Consts.YongZheZhanChangGameFuBenMaxExistMinutes);

                    AddGameFuBen(fuBenData);
                    fuBenData.SortFuBenRoleList();
                    foreach (var role in fuBenData.RoleDict.Values)
                    {
                        KuaFuRoleData kuaFuRoleDataTemp;
                        KuaFuRoleKey key = KuaFuRoleKey.Get(role.ServerId, role.RoleId);
                        if (RoleIdKuaFuRoleDataDict.TryGetValue(key, out kuaFuRoleDataTemp))
                        {
                            LogManager.WriteLog(LogTypes.Info, string.Format("通知活动副本GameID={0}的角色{1}准备进入ServerID={2}开始游戏", fuBenData.GameId, kuaFuRoleDataTemp.RoleId, fuBenData.ServerId));
                            kuaFuRoleDataTemp.UpdateStateTime(fuBenData.GameId, KuaFuRoleStates.NotifyEnterGame, stateEndTime.Ticks);
                        }
                    }

                    fuBenData.State = GameFuBenState.Start;
                    NotifyFuBenRoleEnterGame(fuBenData);
                    Persistence.LogCreateYongZheFuBen(kfSrvId, gameId, 0, fuBenData.GetFuBenRoleCount());
                }
                else
                {
                    //如果分配失败,则返回false,本轮不在尝试分配
                    LogManager.WriteLog(LogTypes.Error, string.Format("暂时没有可用的服务器可以给活动副本分配,稍后重试"));
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

        private void AddGameFuBen(YongZheZhanChangFuBenData fubenData)
        {
            FuBenDataDict[fubenData.GameId] = fubenData;
        }

        private void RemoveGameFuBen(YongZheZhanChangFuBenData fuBenData)
        {
            int gameId = fuBenData.GameId;
            YongZheZhanChangFuBenData tmpFuben;
            if (FuBenDataDict.TryRemove(gameId, out tmpFuben))
            {
                tmpFuben.State = GameFuBenState.End;                
            }
            ClientAgentManager.Instance().RemoveKfFuben(YzzcGameType, tmpFuben.ServerId, tmpFuben.GameId);

            lock (fuBenData)
            {
                foreach (var fuBenRoleData in fuBenData.RoleDict.Values)
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
                YongZheZhanChangFuBenData fuBenData;
                if (FuBenDataDict.TryGetValue(gameId, out fuBenData))
                {
                    lock (fuBenData)
                    {
                        fuBenData.RemoveKuaFuFuBenRoleData(roleId);
                        if (fuBenData.CanRemove())
                        {
                            RemoveGameFuBen(fuBenData);
                        }
                    }
                }
            }
        }

        #endregion 辅助函数

        #region 执行命令

        public int ExecCommand(string[] args)
        {
            int result = StdErrorCode.Error_Success_No_Info;

            try
            {
                if (0 == string.Compare(args[0], GameStates.CommandName, true))
                {
                    if (args.Length >= 2)
                    {
                        int gameType = (int)GameTypes.YongZheZhanChang;
                        int gameState = int.Parse(args[1]);
                        if (args.Length >= 3)
                        {
                            int.TryParse(args[2], out gameType);
                        }

                        DateTime now = TimeUtil.NowDateTime();
                        lock (Mutex)
                        {
                            if (gameState == GameStates.PrepareGame && (now - PrepareStartGameTime).TotalHours >= 1)
                            {
                                PrepareStartGameTime = now;

                                //本期报名和分配未完成时,不能将状态回退.
                                if (GameState > gameState)
                                {
                                    LogManager.WriteLog(LogTypes.Info, "ExecCommand Set GameState to ignore" + gameState);
                                    return StdErrorCode.Error_Data_Overdue;
                                }

                                GameState = gameState;
                                RunTimeGameType = gameType;
                                switch (gameType)
                                {
                                    case (int)GameTypes.KuaFuBoss:
                                        Persistence.MinEnterCount = Persistence.KuaFuBossRoleCount;
                                        Persistence.ServerCapacityRate = 2;
                                        break;
                                    case (int)GameTypes.YongZheZhanChang:
                                        Persistence.MinEnterCount = Persistence.YongZheZhanChangRoleCount;
                                        Persistence.ServerCapacityRate = 1;
                                        break;
                                    case (int)GameTypes.KingOfBattle:
                                        Persistence.MinEnterCount = Persistence.KingOfBattleRoleCount;
                                        Persistence.ServerCapacityRate = 2;
                                        break;
                                }

                                LogManager.WriteLog(LogTypes.Info, "ExecCommand Set GameState to " + gameState);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
            }

            return result;
        }

        #endregion 执行命令
    }
}
