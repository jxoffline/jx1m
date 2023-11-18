using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using KF.Client;
using KF.Contract.Data;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Tmsk.Contract;

namespace GameServer.Logic
{
    public class KuaFuDbConnection
    {
        public int ServerId;
        public int ErrorCount = 0;
        public GameDbClientPool[] Pool = new GameDbClientPool[2] { new GameDbClientPool(), new GameDbClientPool() };

        public KuaFuDbConnection(int serverId)
        {
            ServerId = serverId;
        }

        ~KuaFuDbConnection()
        {
            Pool[0].Clear();
            Pool[1].Clear();
        }
    }

    /// <summary>
    /// 王城战管理
    /// </summary>
    public class KuaFuManager : IManager, ICmdProcessorEx, IEventListener, IEventListenerEx, IManager2
    {
        #region 标准接口

        private ICoreInterface CoreInterface = null;

        private static KuaFuManager instance = new KuaFuManager();

        private static BlockingCollection<KuaFuChatData> MsgConnect = new BlockingCollection<KuaFuChatData>();

        public static KuaFuManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// 配置和运行时数据
        /// </summary>
        public KuaFuDataData RuntimeData = new KuaFuDataData();

        public bool initialize()
        {
            return true;
        }

        public bool initialize(ICoreInterface coreInterface)
        {
            try
            {
                CoreInterface = coreInterface;
                if (!InitConfig())
                {
                    return false;
                }

                System.Runtime.Remoting.RemotingConfiguration.Configure(Process.GetCurrentProcess().MainModule.FileName + ".config", false);

                if (!YongZheZhanChangClient.getInstance().initialize(coreInterface))
                {
                    return false;
                }

                if (!KFCopyRpcClient.getInstance().initialize(coreInterface))
                {
                    return false;
                }

                GlobalEventSource.getInstance().registerListener((int)EventTypes.PlayerLogout, getInstance());
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool startup()
        {
            try
            {
                ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("YongZheZhanChangClient.TimerProc", YongZheZhanChangClient.getInstance().TimerProc), 2000, 3389);

                ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("KFCopyRpcClient.TimerProc", KFCopyRpcClient.getInstance().TimerProc), 2000, 1732);

                lock (RuntimeData.Mutex)
                {
                    if (null == RuntimeData.BackGroundThread)
                    {
                        RuntimeData.BackGroundThread = new Thread(BackGroudThreadProc);
                        RuntimeData.BackGroundThread.IsBackground = true;
                        RuntimeData.BackGroundThread.Start();
                    }
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool showdown()
        {
            try
            {
                lock (RuntimeData.Mutex)
                {
                    RuntimeData.BackGroundThread.Abort();
                    RuntimeData.BackGroundThread = null;
                }

                GlobalEventSource.getInstance().removeListener((int)EventTypes.PlayerLogout, getInstance());
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool destroy()
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool processCmd(KPlayer client, string[] cmdParams)
        {
            return false;
        }

        public bool processCmdEx(KPlayer client, int nID, byte[] bytes, string[] cmdParams)
        {
            return true;
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventObject"></param>
        public void processEvent(EventObject eventObject)
        {
            int nID = eventObject.getEventType();
            switch (nID)
            {
                case (int)EventTypes.PlayerLogout:
                    break;
            }
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventObject"></param>
        public void processEvent(EventObjectEx eventObject)
        {
        }

        #endregion 标准接口

        #region 初始化配置

        /// <summary>
        /// 初始化配置
        /// </summary>
        public bool InitConfig()
        {
            bool success = true;
            XElement xml = null;
            string fileName = "";
            string fullPathFileName = "";
            IEnumerable<XElement> nodes;

            lock (RuntimeData.Mutex)
            {
                try
                {
                    int open = 0;
                    string kuaFuUriKeyNamePrefix = null;
                    int serverId = CoreInterface.GetLocalServerId();
                    PlatformTypes platfromType = CoreInterface.GetPlatformType();
                    fileName = string.Format("Config/ThroughService_{0}.xml", platfromType.ToString());
                    fullPathFileName = KTGlobal.GetDataPath(fileName); //Global.IsolateResPath(fileName);

                    //如果配置文件存在,则读配置文件,否则读默认配置
                    if (File.Exists(fullPathFileName))
                    {
                        xml = XElement.Load(fullPathFileName);
                        nodes = xml.Elements();

                        foreach (var node in nodes)
                        {
                            int startServer = (int)Global.GetSafeAttributeLong(node, "StartServer");
                            int endServer = (int)Global.GetSafeAttributeLong(node, "EndServer");
                            if (startServer <= serverId && serverId < endServer)
                            {
                                open = (int)Global.GetSafeAttributeLong(node, "Open");
                                kuaFuUriKeyNamePrefix = Global.GetSafeAttributeStr(node, "ID");
                                break;
                            }
                        }
                    }
                    else
                    {
                        open = 1;
                        kuaFuUriKeyNamePrefix = null;
                    }

                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.KuaFuGongNeng, open.ToString());
                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.KuaFuUriKeyNamePrefix, kuaFuUriKeyNamePrefix);

                    string huanYingSiYuanUri = null;
                    string tianTiUri = null;
                    string yongZheZhanChangUri = null;
                    //  string moRiJudgeUri = null;
                    // string elementWarUri = null;
                    string kfcopyUri = null;
                    string spreadUri = null;
                    string allyUri = null;

                    if (open > 0)
                    {
                        string huanYingSiYuanUriKeyName = RuntimeVariableNames.HuanYingSiYuanUri + kuaFuUriKeyNamePrefix;
                        huanYingSiYuanUri = CoreInterface.GetGameConfigStr(huanYingSiYuanUriKeyName, null);

                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(huanYingSiYuanUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            huanYingSiYuanUri = ConfigurationManager.AppSettings.Get(huanYingSiYuanUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(huanYingSiYuanUri))
                            {
                                huanYingSiYuanUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.HuanYingSiYuanUri);
                            }
                        }

                        string tianTiUriKeyName = RuntimeVariableNames.TianTiUri + kuaFuUriKeyNamePrefix;
                        tianTiUri = CoreInterface.GetGameConfigStr(tianTiUriKeyName, null);

                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(tianTiUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            tianTiUri = ConfigurationManager.AppSettings.Get(tianTiUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(tianTiUri))
                            {
                                tianTiUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.TianTiUri);
                            }
                        }

                        string yongZheZhanChangUriKeyName = RuntimeVariableNames.YongZheZhanChangUri + kuaFuUriKeyNamePrefix;
                        yongZheZhanChangUri = CoreInterface.GetGameConfigStr(yongZheZhanChangUriKeyName, null);

                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(yongZheZhanChangUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            yongZheZhanChangUri = ConfigurationManager.AppSettings.Get(yongZheZhanChangUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(yongZheZhanChangUri))
                            {
                                yongZheZhanChangUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.YongZheZhanChangUri);
                            }
                        }

                        string kfcopyUriKeyName = RuntimeVariableNames.KuaFuCopyUri + kuaFuUriKeyNamePrefix;
                        kfcopyUri = CoreInterface.GetGameConfigStr(kfcopyUriKeyName, null);

                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(kfcopyUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            kfcopyUri = ConfigurationManager.AppSettings.Get(kfcopyUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(kfcopyUri))
                            {
                                kfcopyUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.KuaFuCopyUri);
                            }
                        }

                        //
                        string SpreadUriKeyName = RuntimeVariableNames.SpreadUri + kuaFuUriKeyNamePrefix;
                        spreadUri = CoreInterface.GetGameConfigStr(SpreadUriKeyName, null);
                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(spreadUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            spreadUri = ConfigurationManager.AppSettings.Get(SpreadUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(spreadUri))
                            {
                                spreadUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.SpreadUri);
                            }
                        }

                        string AllyUriKeyName = RuntimeVariableNames.AllyUri + kuaFuUriKeyNamePrefix;
                        allyUri = CoreInterface.GetGameConfigStr(AllyUriKeyName, null);
                        //如果数据库没配置,则读默认配置文件
                        if (string.IsNullOrEmpty(allyUri))
                        {
                            ConfigurationManager.RefreshSection("appSettings");
                            allyUri = ConfigurationManager.AppSettings.Get(AllyUriKeyName);

                            //如果没有指定后缀的配置,则读默认配置
                            if (string.IsNullOrEmpty(allyUri))
                            {
                                allyUri = ConfigurationManager.AppSettings.Get(RuntimeVariableNames.AllyUri);
                            }
                        }
                    }

                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.HuanYingSiYuanUri, huanYingSiYuanUri);
                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.TianTiUri, tianTiUri);
                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.YongZheZhanChangUri, yongZheZhanChangUri);

                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.KuaFuCopyUri, kfcopyUri);
                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.SpreadUri, spreadUri);
                    CoreInterface.SetRuntimeVariable(RuntimeVariableNames.AllyUri, allyUri);
                }
                catch (System.Exception ex)
                {
                    success = false;
                    LogManager.WriteLog(LogTypes.Fatal, string.Format("XML toạch :{0}, lỗi。", fileName), ex);
                }
            }

            return success;
        }

        #endregion 初始化配置

        #region 辅助接口

        public bool OnUserLogin2(TMSKSocket socket, int verSign, string userID, string userName, string lastTime, string isadult, string signCode)
        {
            WebLoginToken webLoginToken = new WebLoginToken()
            {
                VerSign = verSign,
                UserID = userID,
                UserName = userName,
                LastTime = lastTime,
                Isadult = isadult,
                SignCode = signCode,
            };

            socket.ClientKuaFuServerLoginData.WebLoginToken = webLoginToken;
            return true;
        }

        public bool OnUserLogin(TMSKSocket socket, int verSign, string userID, string userName, string lastTime, string userToken, string isadult, string signCode, int serverId, string ip, int port, int roleId, int gameType, long gameId)
        {
            KuaFuServerLoginData kuaFuServerLoginData = socket.ClientKuaFuServerLoginData;
            kuaFuServerLoginData.ServerId = serverId;
            kuaFuServerLoginData.ServerIp = ip;
            kuaFuServerLoginData.ServerPort = port;
            kuaFuServerLoginData.RoleId = roleId;
            kuaFuServerLoginData.GameType = gameType;
            kuaFuServerLoginData.GameId = gameId;

            if (kuaFuServerLoginData.WebLoginToken == null)
            {
                kuaFuServerLoginData.WebLoginToken = new WebLoginToken()
                {
                    VerSign = verSign,
                    UserID = userID,
                    UserName = userName,
                    LastTime = lastTime,
                    Isadult = isadult,
                    SignCode = signCode,
                };
            }

            if (roleId > 0)
            {
                if (GameManager.ServerLineID != GameManager.ServerId)
                {
                    LogManager.WriteLog(LogTypes.Error, "GameManager.ServerLineID未配置,禁止跨服登录");
                    return false;
                }

                if (!string.IsNullOrEmpty(ip) && port > 0 && gameType > 0 && gameId > 0/* && HuanYingSiYuanClient.getInstance().CanKuaFuLogin()*/)
                {
                    string dbIp = "";
                    int dbPort = 0;
                    string logDbIp = "";
                    int logDbPort = 0;
                    socket.ServerId = serverId;
                    switch (gameType)
                    {
                        case (int)GameTypes.KuaFuMap:
                            {
                                if (!YongZheZhanChangClient.getInstance().CanKuaFuLogin()) return false;

                                socket.IsKuaFuLogin = YongZheZhanChangClient.getInstance().CanEnterKuaFuMap(kuaFuServerLoginData);
                                if (!YongZheZhanChangClient.getInstance().GetKuaFuDbServerInfo(serverId, out dbIp, out dbPort, out logDbIp, out logDbPort))
                                {
                                    LogManager.WriteLog(LogTypes.Error, string.Format("The server IP and port of the original server of the role {0} could not be found The server IP and port of the original server of the role {0} were not found", kuaFuServerLoginData.RoleId));
                                    return false;
                                }
                            }
                            break;
                    }

                    if (socket.IsKuaFuLogin && serverId != 0)
                    {
                        if (serverId != 0)
                        {
                            if (!InitGameDbConnection(serverId, dbIp, dbPort, logDbIp, logDbPort))
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Failed to connect GameDBServer and LogDBServer of the original server of role {0}", kuaFuServerLoginData.RoleId));
                                return false;
                            }
                        }

                        return socket.IsKuaFuLogin;
                    }
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("The role {0} failed to find this server in the server list as a cross-server server", kuaFuServerLoginData.RoleId));
                    return false;
                }
            }
            else
            {
                if (YongZheZhanChangClient.getInstance().LocalLogin(userID))
                {
                    kuaFuServerLoginData.RoleId = 0;
                    kuaFuServerLoginData.GameId = 0;
                    kuaFuServerLoginData.GameType = 0;
                    kuaFuServerLoginData.ServerId = 0;
                    socket.ServerId = 0;
                    socket.IsKuaFuLogin = false;
                    return true;
                }
            }

            LogManager.WriteLog(LogTypes.Error, string.Format("未能找到角色{0}的跨服活动或副本信息", kuaFuServerLoginData.RoleId));
            return false;
        }

        public bool OnInitGame(KPlayer client)
        {
            int gameType = Global.GetClientKuaFuServerLoginData(client).GameType;
            switch (gameType)
            {
                case (int)GameTypes.HuanYingSiYuan:
                    break;

                case (int)GameTypes.TianTi:
                    break;

                case (int)GameTypes.YongZheZhanChang:
                    return YongZheZhanChangManager.getInstance().OnInitGame(client);

                case (int)GameTypes.KingOfBattle:
                    break;

                case (int)GameTypes.MoRiJudge:
                    break;

                case (int)GameTypes.ElementWar:
                    break;

                case (int)GameTypes.KuaFuBoss:
                    break;

                case (int)GameTypes.KuaFuMap: // MAP LIÊN MÁY CHỦ
                    return KuaFuMapManager.getInstance().OnInitGame(client);

                case (int)GameTypes.KuaFuCopy:
                    break;

                case (int)GameTypes.LangHunLingYu:
                    break;

                case (int)GameTypes.ZhengBa:
                    break;
            }

            return false;
        }

        public void OnStartPlayGame(KPlayer client)
        {
            int gameType = Global.GetClientKuaFuServerLoginData(client).GameType;
            switch (gameType)
            {
                case (int)GameTypes.KuaFuMap:
                    KuaFuMapManager.getInstance().OnStartPlayGame(client);
                    break;
            }
        }

        public void OnLeaveScene(KPlayer client, SceneUIClasses sceneType, bool logout = false)
        {
            if (client.ClientSocket.IsKuaFuLogin)
            {
                if (!logout)
                {
                    GotoLastMap(client);
                }
            }
        }

        /// <summary>
        ///  xử lý sự kiện khi người chơi thoát
        /// </summary>
        /// <param name="client"></param>
        public void OnLogout(KPlayer client)
        {
        }

        /// <summary>
        /// Đưa người chơi về bản đồ cuối cùng
        /// </summary>
        /// <param name="client"></param>
        public void GotoLastMap(KPlayer client)
        {
            client.SendPacket((int)TCPGameServerCmds.CMD_SPR_KF_SWITCH_SERVER, new KuaFuServerLoginData() { RoleId = 0 });
        }

        #endregion 辅助接口

        #region 跨服数据库连接管理

        private void BackGroudThreadProc()
        {
            do
            {
                try
                {
                    HandleTransferChatMsg();
                }
                catch
                {
                }

                Thread.Sleep(1800);
            } while (true);
        }

        private object DbMutex = new object();

        //private Dictionary<int, GameDbClientPool[]> GameDbConnectPoolDict = new Dictionary<int, GameDbClientPool[]>();
        private Dictionary<int, KuaFuDbConnection> GameDbConnectPoolDict = new Dictionary<int, KuaFuDbConnection>();

        public bool InitGameDbConnection(int serverId, string ip, int port, string logIp, int logPort)
        {
            KuaFuDbConnection pool;
            bool init = false;
            lock (DbMutex)
            {
                if (!GameDbConnectPoolDict.TryGetValue(serverId, out pool))
                {
                    pool = new KuaFuDbConnection(serverId);

                    GameDbConnectPoolDict[serverId] = pool;
                    init = true;
                }
                else
                {
                    pool.Pool[0].ChangeIpPort(ip, port);
                    pool.Pool[1].ChangeIpPort(logIp, logPort);
                }
            }

            if (init)
            {
                if (!pool.Pool[0].Init(8, ip, port, string.Format("server_db_{0}", serverId)) || !pool.Pool[1].Init(8, logIp, logPort, string.Format("server_log_{0}", serverId)))
                {
                    return false;
                }
            }
            else
            {
                return pool.Pool[0].Supply() && pool.Pool[1].Supply();
            }

            return true;
        }

        public TCPClient PopGameDbClient(int serverId, int poolId)
        {
            try
            {
                KuaFuDbConnection pool;
                lock (DbMutex)
                {
                    if (!GameDbConnectPoolDict.TryGetValue(serverId, out pool))
                    {
                        return null;
                    }
                }

                return pool.Pool[poolId].Pop();
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return null;
        }

        public void PushChatData(KuaFuChatData ChatData)
        {
            try
            {
                // Attack ChatData
                MsgConnect.Add(ChatData);
            }
            catch (System.Exception ex)
            {
            }
        }

        public void PushGameDbClient(int serverId, TCPClient tcpClient, int poolId)
        {
            try
            {
                KuaFuDbConnection pool;
                lock (DbMutex)
                {
                    if (!GameDbConnectPoolDict.TryGetValue(serverId, out pool))
                    {
                        return;
                    }
                }

                pool.Pool[poolId].Push(tcpClient);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        /// <summary>
        /// Thời gian gần đây nhất lấy ra chat list
        /// </summary>
        public long LastTransferTicks = 0;

        public long LastSysnChatTicks = 0;

        /// <summary>
        /// Thời gian giữ kết nối với Server
        /// </summary>
        public int SendServerHeartCount = 0;

        /// <summary>
        /// Các DB đang được kết nối với máy chủ liên server
        /// </summary>
        private List<KuaFuDbConnection> ActiveServerIdList = new List<KuaFuDbConnection>();

        private List<KuaFuDbConnection> ChatServerActive = new List<KuaFuDbConnection>();

        /// <summary>
        /// Lấy ra chát từ máy chủ lên máy chủ liên server
        /// </summary>
        ///

        private void HandleTransferChatMsg()
        {
            long ticks = TimeUtil.NOW();
            if (ticks - LastTransferTicks < (1000))
            {
                return;
            }

            LastTransferTicks = ticks;

            string strcmd = "";

            TCPOutPacket tcpOutPacket = null;
            strcmd = string.Format("{0}:{1}:{2}:{3}", GameManager.ServerLineID, 0, SendServerHeartCount, "");
            SendServerHeartCount++;
            ChatServerActive.Clear();
            ActiveServerIdList.Clear();
            lock (DbMutex)
            {
                foreach (var connection in GameDbConnectPoolDict.Values)
                {
                    ChatServerActive.Add(connection);
                    if ((connection.ServerId % 3) == (SendServerHeartCount % 3))
                    {
                        ActiveServerIdList.Add(connection);
                    }
                }
            }

            foreach (var connection in ActiveServerIdList)
            {
                try
                {
                    List<string> chatMsgList = Global.SendToDB<List<string>, string>((int)TCPGameServerCmds.CMD_DB_GET_CHATMSGLIST, strcmd, connection.ServerId);

                    if (null != chatMsgList && chatMsgList.Count > 0)
                    {
                        for (int i = 0; i < chatMsgList.Count; i++)
                        {
                            // Thực hiện showw toàn bộ các tin nhắn của những sv khác lên sv này (bang hội,gia tộc)
                            KT_TCPHandler.TransferChatMsg(chatMsgList[i]);
                        }
                    }

                    connection.ErrorCount = 0;
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                    connection.ErrorCount++;
                }

                if (connection.ErrorCount > 20)
                {
                    //失联持续60秒后进来
                    lock (DbMutex)
                    {
                        GameDbConnectPoolDict.Remove(connection.ServerId);
                    }
                }
            }

            if (MsgConnect.Count > 0)
            {
                while (MsgConnect.Count > 0)
                {
                    var msg = MsgConnect.Take();

                    LogManager.WriteLog(LogTypes.Chat, "TAKE MSG :" + msg.serverLineID);

                    foreach (var connection in ChatServerActive)
                    {
                        LogManager.WriteLog(LogTypes.Chat, "connection:" + connection.ServerId);

                        if (connection.ServerId != msg.serverLineID)
                        {
                            string ChatData = msg.roleID + ":" + msg.roleName + ":" + msg.status + ":" + msg.toRoleName + ":" + msg.index + ":" + DataHelper.EncodeBase64(msg.textMsg) + ":" + msg.chatType;
                            try
                            {
                                LogManager.WriteLog(LogTypes.Chat, "[" + connection.ServerId + "]PUSH CHAT TO SERVER :" + ChatData + "| LINE :" + msg.serverLineID);

                                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_SPR_CHAT,
                                                      string.Format("{0}:{1}:{2}", ChatData, msg.extTag1, msg.serverLineID),
                                                      connection.ServerId);
                            }
                            catch (Exception ex)
                            {
                                LogManager.WriteLog(LogTypes.Error, "[" + connection.ServerId + "]BUG CHAT KUAFU SERVER :" + ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        #endregion 跨服数据库连接管理

        #region 跨服活动免排队管理

        public bool IsKuaFuMap(int mapCode)
        {
            return KuaFuMapManager.getInstance().IsKuaFuMap(mapCode);
        }

        #endregion 跨服活动免排队管理

        #region 跨服副本时间相关

        // 报名后等待匹配的最大时间
        public int SingUpMaxSeconds { get; private set; }

        // 匹配成功后，客户端点击确定进入的最大等待时间
        public int AutoCancelMaxSeconds { get; private set; }

        // 匹配成功后离开，那么在接下来的一段时间内，不能参加任何跨服副本
        public int CannotJoinCopyMaxSeconds { get; private set; }

        public void SetCannotJoinKuaFu_UseAutoEndTicks(KPlayer client)
        {
            if (CannotJoinCopyMaxSeconds <= 0) return;

            SetCannotJoinKuaFuCopyEndTicks(client, DateTime.Now.AddSeconds(CannotJoinCopyMaxSeconds).Ticks);
        }

        public void SetCannotJoinKuaFuCopyEndTicks(KPlayer client, long endTicks)
        {
            if (client == null) return;

            Global.SaveRoleParamsInt64ValueToDB(client, RoleParamName.CannotJoinKFCopyEndTicks, endTicks, true);
        }

        // 是否处于不能参加跨服副本的时间
        public bool IsInCannotJoinKuaFuCopyTime(KPlayer client)
        {
            if (client == null) return true;

            long endTicks = Global.GetRoleParamsInt64FromDB(client, RoleParamName.CannotJoinKFCopyEndTicks);
            return DateTime.Now.Ticks < endTicks;
        }

        public void NotifyClientCannotJoinKuaFuCopyEndTicks(KPlayer client)
        {
            long endTicks = Global.GetRoleParamsInt64FromDB(client, RoleParamName.CannotJoinKFCopyEndTicks);
        }

        #endregion 跨服副本时间相关
    }
}