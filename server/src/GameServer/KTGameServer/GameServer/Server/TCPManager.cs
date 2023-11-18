#define UseTimer
using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.Logic;
//using System.Windows.Forms;
using GameServer.Logic;
using GameServer.Tools;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Server
{
    /// <summary>
    /// TCP连接管理对象类
    /// </summary>
    public class TCPManager
    {
        private static TCPManager instance = new TCPManager();

        public const bool UseWorkerPool = false;

        public static int ServerPort = 0;

        private TCPManager() { }

        public static TCPManager getInstance()
        {
            return instance;
        }

        public void initialize(int capacity)
        {
            MaxConnectedClientLimit = capacity;

            socketListener = new SocketListener(capacity, (int)TCPCmdPacketSize.RECV_MAX_SIZE);
            socketListener.SocketClosed += SocketClosed;
            socketListener.SocketConnected += SocketConnected;
            socketListener.SocketReceived += SocketReceived;
            socketListener.SocketSended += SocketSended;

            _tcpClientPool = TCPClientPool.getInstance();
            _tcpClientPool.initialize(100);
            _tcpLogClientPool = TCPClientPool.getLogInstance();
            _tcpLogClientPool.initialize(100);

            tcpInPacketPool = new TCPInPacketPool(capacity);
            /*tcpOutPacketPool = new TCPOutPacketPool(capacity);*/
            TCPOutPacketPool.getInstance().initialize(capacity);
            tcpOutPacketPool = TCPOutPacketPool.getInstance();
            dictInPackets = new Dictionary<TMSKSocket, TCPInPacket>(capacity);
            tcpSessions = new Dictionary<TMSKSocket, TCPSession>();
            TCPCmdDispatcher.getInstance().initialize();
#if UseTimer
            taskExecutor = new ScheduleExecutor(0);
#else
            taskExecutor = new ScheduleExecutor(1);
#endif
            taskExecutor.start();

        }

        /// <summary>
        /// 指令异步执行处理器
        /// </summary>
        public ScheduleExecutor taskExecutor = null;

        /// <summary>
        /// 最大连接数限制
        /// </summary>
        public int MaxConnectedClientLimit = 0;

        /// <summary>
        /// 服务器端的侦听对象
        /// </summary>
        private SocketListener socketListener = null;

        /// <summary>
        /// 服务器端的侦听对象 属性
        /// </summary>
        public SocketListener MySocketListener
        {
            get { return socketListener; }
        }

        /// <summary>
        /// 接收的命令包缓冲池
        /// </summary>
        private TCPInPacketPool tcpInPacketPool = null;

        /// <summary>
        /// 接收的命令包缓冲池
        /// </summary>
        public TCPInPacketPool TcpInPacketPool
        {
            get { return tcpInPacketPool; }
        }

        /// <summary>
        /// 发送的命令包缓冲池
        /// </summary>
        private TCPOutPacketPool tcpOutPacketPool = null;

        /// <summary>
        /// 发送的命令包缓冲池
        /// </summary>
        public TCPOutPacketPool TcpOutPacketPool
        {
            get { return tcpOutPacketPool; }
        }

        /// <summary>
        /// 主窗口对象
        /// </summary>
        public Program RootWindow
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端连接池
        /// </summary>
        private TCPClientPool _tcpClientPool = null;


        /// <summary>
        /// 客户端连接
        /// </summary>
        public TCPClientPool tcpClientPool
        {
            get { return _tcpClientPool; }
        }

        /// <summary>
        /// 日志客户端连接池
        /// </summary>
        private TCPClientPool _tcpLogClientPool = null;


        /// <summary>
        /// 日志客户端连接
        /// </summary>
        public TCPClientPool tcpLogClientPool
        {
            get { return _tcpLogClientPool; }
        }

        /// <summary>
        /// 随机密码生成器
        /// </summary>
        private TCPRandKey _tcpRandKey = new TCPRandKey(10000);

        /// <summary>
        /// 随机密码生成器
        /// </summary>
        public TCPRandKey tcpRandKey
        {
            get { return _tcpRandKey; }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port"></param>
        public void Start(string ip, int port)
        {
            ServerPort = port;

            socketListener.Init();
            socketListener.Start(ip, port);
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        /// <param name="port"></param>
        public void Stop()
        {
            socketListener.Stop();
            taskExecutor.stop();
        }


        /// <summary>
        /// Thực hiện ngắt kết nối người chơi
        /// </summary>
        /// <param name="s"></param>
        public void ForceCloseSocket(TMSKSocket s)
        {
            lock (dictInPackets)
            {
                if (dictInPackets.ContainsKey(s))
                {
                    TCPInPacket tcpInPacket = dictInPackets[s];
                    tcpInPacketPool.Push(tcpInPacket); //缓冲回收
                    dictInPackets.Remove(s);
                }
            }

            bool bIsExistClient = false;
            //TÌM GAME CLIENT
            KPlayer gameClient = KTPlayerManager.Find(s);
            if (null != gameClient)
            {
                // CALL TỚI LOGIC LOGOUT CLIENT
                KTGlobal.Logout(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, gameClient);


                gameClient = null;
                bIsExistClient = true;
            }

            string userID = GameManager.OnlineUserSession.FindUserID(s);

            //Remove Sesion ONLINE
            GameManager.OnlineUserSession.RemoveSession(s);

            //Remove UserName LOGIN
            GameManager.OnlineUserSession.RemoveUserName(s);



            // NẾU NGƯỜI CHƠI CHƯA TỪNG ĐĂNG NHẬP THÌ GHI LẠI THÔNG TIN USER VÀO DB
            if (!string.IsNullOrEmpty(userID))
            {
                GameDb.RegisterUserIDToDBServer(userID, 0, s.ServerId, ref s.session.LastLogoutServerTicks);
            }
            //已经关闭之后再调用一次
            Global._SendBufferManager.Remove(s);
        }

        #region 外部线程数据包处理

        /// <summary>
        /// 返回待处理数据包信息 用于界面显示用
        /// </summary>
        /// <returns></returns>
        public String GetAllCacheCmdPacketInfo()
        {
            int nTotal = 0;
            int nMaxNum = 0;
            lock (dictInPackets) //锁定接收包队列
            {
                for (int n = 0; n < dictInPackets.Values.Count; n++)
                {
                    TCPInPacket tcpInPacket = dictInPackets.Values.ElementAt(n);

                    if (tcpInPacket.GetCacheCmdPacketCount() > nMaxNum)
                    {
                        nMaxNum = tcpInPacket.GetCacheCmdPacketCount();
                    }

                    nTotal += tcpInPacket.GetCacheCmdPacketCount();
                }
            }

            /**/
            return String.Format("总共缓存命令包{0}个,单个连接最大缓存{1}个", nTotal, nMaxNum);
        }

        /// <summary>
        /// 返回下一个需要处理的TcpInPacket，避免处理过程中一直锁定dictInPackets
        /// 处理一次，简单的锁一次
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private TCPInPacket GetNextTcpInPacket(int index)
        {
            lock (dictInPackets) //锁定接收包队列
            {
                if (dictInPackets.Values.Count > index && index >= 0)
                {
                    return dictInPackets.Values.ElementAt(index);
                }
            }

            return null;
        }

        /// <summary>
        /// 在多个线程内部循环调用
        /// </summary>
        public void ProcessCmdPackets(Queue<CmdPacket> ls)
        {
            //dictInPackets.Values值的个数可能会变化，每次处理先保存好最大个数
            //同时给出20个冗余数量
            int maxCount = dictInPackets.Values.Count + 20;

            for (int n = 0; n < maxCount; n++)
            {
                TCPInPacket tcpInPacket = GetNextTcpInPacket(n);

                //这意味着列表数据包变少了
                if (null == tcpInPacket)
                {
                    break;
                }

                ls.Clear();

                if (tcpInPacket.PopCmdPackets(ls)) //判断是否取到了指令列表
                {
                    try
                    {
                        while (ls.Count > 0)
                        {
                            CmdPacket cmdPacket = ls.Dequeue();
                            //接收到了完整的命令包
                            TCPOutPacket tcpOutPacket = null;
                            TCPProcessCmdResults result = TCPProcessCmdResults.RESULT_FAILED;

                            result = TCPCmdHandler.ProcessCmd(this, tcpInPacket.CurrentSocket, tcpClientPool, tcpRandKey, tcpOutPacketPool, cmdPacket.CmdID, cmdPacket.Data, cmdPacket.Data.Length, out tcpOutPacket);

                            if (result == TCPProcessCmdResults.RESULT_DATA && null != tcpOutPacket)
                            {
                                //向登陆客户端返回数据
                                socketListener.SendData(tcpInPacket.CurrentSocket, tcpOutPacket);
                            }
                            else if (result == TCPProcessCmdResults.RESULT_FAILED)//解析失败, 直接close socket
                            {
                                if (cmdPacket.CmdID != (int)TCPGameServerCmds.CMD_LOG_OUT)
                                {
                                    LogManager.WriteLog(LogTypes.Error, string.Format("Execute packet faild: {0},{1}, Close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
                                }

                                //这儿需要关闭链接--->这样关闭对吗?
                                socketListener.CloseSocket(tcpInPacket.CurrentSocket);

                                break;
                            }
                        }
                    }
                    finally
                    {
                        //处理结束之后设置相关标志位
                        tcpInPacket.OnThreadDealingComplete();
                    }
                }
            }
        }

        #endregion 外部线程数据包处理

        #region 事件处理

        public static void RecordCmdDetail(int cmdId, long processTime, long dataSize, TCPProcessCmdResults result)
        {
            PorcessCmdMoniter moniter = null;
            if (!ProcessSessionTask.cmdMoniter.TryGetValue(cmdId, out moniter))
            {
                moniter = new PorcessCmdMoniter(cmdId, processTime);
                moniter = ProcessSessionTask.cmdMoniter.GetOrAdd(cmdId, moniter);
            }

            moniter.onProcessNoWait(processTime, dataSize, result);
        }

        public static void RecordCmdDetail2(int cmdId, long processTime, long waitTime)
        {
            PorcessCmdMoniter moniter = null;
            if (!ProcessSessionTask.cmdMoniter.TryGetValue(cmdId, out moniter))
            {
                moniter = new PorcessCmdMoniter(cmdId, processTime);
                moniter = ProcessSessionTask.cmdMoniter.GetOrAdd(cmdId, moniter);
            }

            moniter.onProcess(processTime, waitTime);
        }

        public static void RecordCmdOutputDataSize(int cmdId, long dataSize)
        {
            PorcessCmdMoniter moniter = null;
            if (!ProcessSessionTask.cmdMoniter.TryGetValue(cmdId, out moniter))
            {
                moniter = new PorcessCmdMoniter(cmdId, 0);
                moniter = ProcessSessionTask.cmdMoniter.GetOrAdd(cmdId, moniter);
            }

            moniter.OnOutputData(dataSize);
        }

        /// <summary>
        /// 检测客户端的数据完整性
        /// </summary>
        /// <param name="packetCmdID"></param>
        /// <param name="bytesData"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        private byte[] CheckClientDataValid(int packetCmdID, byte[] bytesData, int dataSize, int lastClientCheckTicks, out int clientCheckTicks, out int errorCode)
        {
            errorCode = 0;
            clientCheckTicks = 0;

            if (dataSize < (1 + 4))
            {
                errorCode = 1;
                return null;
            }

            int crc32Num_client = (int)bytesData[0];
            clientCheckTicks = BitConverter.ToInt32(bytesData, 1);
            if (clientCheckTicks < lastClientCheckTicks) ///如果新收到的数据包的校验毫秒数小于旧的说明是外挂在作弊
            {
                errorCode = 2;
                return null;
            }

            CRC32 crc32 = new CRC32();
            crc32.update(bytesData, 1, dataSize - 1);
            uint cc = crc32.getValue() % 255;
            uint cc2 = (uint)(packetCmdID % 255);
            int cc3 = (int)(cc ^ cc2);

            if ((int)cc3 != crc32Num_client)
            {
                errorCode = 3;
                return null;
            }

            byte[] newByteData = new byte[dataSize - 1 - 4];
            DataHelper.CopyBytes(newByteData, 0, bytesData, 1 + 4, dataSize - 1 - 4);

            return newByteData;
        }

        /// <summary>
        /// 命令包接收完毕后的回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool TCPCmdPacketEvent(object sender, int doType)
        {
            TCPInPacket tcpInPacket = sender as TCPInPacket;

            if (0 == doType) //正常的登录指令包
            {
                int thisTimeCheckTicks = 0;
                int checkErrorCode = 0;

                if (0xffff == tcpInPacket.PacketCmdID)
                {
                    String cmdData = new UTF8Encoding().GetString(tcpInPacket.GetPacketBytes(), 0, tcpInPacket.PacketDataSize);
                    if (cmdData == "serveripinfo")
                    {
                        String strinfo = string.Format("{0}_{1}", ServerPort, KTGlobal.GetLocalAddressIPs());
                        byte[] arrSendData = Encoding.UTF8.GetBytes(strinfo);
                        if (null != arrSendData)
                        {
                            tcpInPacket.CurrentSocket.Send(arrSendData, arrSendData.Length, SocketFlags.None);
                        }
                    }
                    return false;
                }
                //这里实际上是有一个指令流的拷贝，占用的很零碎的内存，这个是否考虑使用内存池????
                byte[] bytesData = CheckClientDataValid(tcpInPacket.PacketCmdID, tcpInPacket.GetPacketBytes(), tcpInPacket.PacketDataSize, tcpInPacket.LastCheckTicks, out thisTimeCheckTicks, out checkErrorCode);

                if (null != bytesData)
                {
                    tcpInPacket.LastCheckTicks = thisTimeCheckTicks; //记忆此次的心跳数据
                                                                     //#if false
                    if (UseWorkerPool)
                    {
                        //杰隆的异步处理指令入口代码
                        TCPCmdWrapper wrapper = TCPCmdWrapper.Get(this, tcpInPacket.CurrentSocket, tcpClientPool, tcpRandKey, tcpOutPacketPool, tcpInPacket.PacketCmdID, bytesData, tcpInPacket.PacketDataSize - 1 - 4);

                        TCPSession session = null;
                        if (GameManager.FlagOptimizeLock3)
                        {
                            if (null != tcpInPacket.CurrentSocket)
                            {
                                session = tcpInPacket.CurrentSocket.session;
                            }
                            if (null == session)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("No TCPClientSession has been initialized: {0},{1}, ErrorCode: {2}, close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), checkErrorCode));
                                return false;
                            }
                        }
                        else
                        {
                            lock (tcpSessions)
                            {
                                if (!tcpSessions.TryGetValue(tcpInPacket.CurrentSocket, out session))
                                {
                                    LogManager.WriteLog(LogTypes.Error, string.Format("No TCPClientSession has been initialized: {0},{1}, ErrorCode: {2}, close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), checkErrorCode));
                                    return false;
                                }
                            }
                        }

                        //int posCmdNum = 0;
                        //session.addTCPCmdWrapper(wrapper, out posCmdNum);
                        //if (posCmdNum > 0)
                        //{
                        //    int banSpeedUpMinutes = GameManager.PlatConfigMgr.GetGameConfigItemInt(PlatConfigNames.BanSpeedUpMinutes2, 10); //加速禁止登陆的时间
                        //    KPlayer client = KTPlayerManager.Find(tcpInPacket.CurrentSocket);
                        //    if (null != client)
                        //    {
                        //        GameManager.ClientMgr.NotifyImportantMsg(this.MySocketListener, tcpOutPacketPool, client, StringUtil.substitute(Global.GetLang("本游戏禁止使用加速软件，{0}分钟内将禁止登陆!"), banSpeedUpMinutes), GameInfoTypeIndexes.Error, ShowGameInfoTypes.HintAndBox);
                        //        BanManager.BanRoleName(Global.FormatRoleName(client, client.RoleName), banSpeedUpMinutes);
                        //    }

                        //    LogManager.WriteLog(LogTypes.Error, string.Format("通过POSITION指令判断客户端加速: {0}, 指令个数:{1}, 断开连接", Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), posCmdNum));
                        //    return false;
                        //}

                        taskExecutor.execute(new ProcessSessionTask(session));
                    }
                    //#else
                    else
                    {
                        TCPSession session = null;
                        if (GameManager.FlagOptimizeLock3)
                        {
                            if (null != tcpInPacket.CurrentSocket)
                            {
                                session = tcpInPacket.CurrentSocket.session;
                            }
                            if (null == session)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("No TCPClientSession has been initialized: {0},{1}, ErrorCode: {2}, close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), checkErrorCode));
                                return false;
                            }
                        }
                        else
                        {
                            lock (tcpSessions)
                            {
                                if (!tcpSessions.TryGetValue(tcpInPacket.CurrentSocket, out session))
                                {
                                    LogManager.WriteLog(LogTypes.Error, string.Format("No TCPClientSession has been initialized: {0},{1}, ErrorCode: {2}, close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), checkErrorCode));
                                    return false;
                                }
                            }
                        }

                        //int posCmdNum = 0;
                        //session.CheckCmdNum(tcpInPacket.PacketCmdID, out posCmdNum);
                        //if (posCmdNum > 0)
                        //{
                        //    int banSpeedUpMinutes = GameManager.PlatConfigMgr.GetGameConfigItemInt(PlatConfigNames.BanSpeedUpMinutes2, 10); //加速禁止登陆的时间
                        //    KPlayer client = KTPlayerManager.Find(tcpInPacket.CurrentSocket);
                        //    if (null != client)
                        //    {
                        //        if (client.CheckCheatData.ProcessBooster)
                        //        {
                        //            GameManager.ClientMgr.NotifyImportantMsg(this.MySocketListener, tcpOutPacketPool, client, StringUtil.substitute(Global.GetLang("本游戏禁止使用加速软件，{0}分钟内将禁止登陆!"), banSpeedUpMinutes), GameInfoTypeIndexes.Error, ShowGameInfoTypes.HintAndBox);
                        //            BanManager.BanRoleName(Global.FormatRoleName(client, client.RoleName), banSpeedUpMinutes);

                        //            LogManager.WriteLog(LogTypes.Error, string.Format("通过POSITION指令判断客户端加速: {0}, 指令个数:{1}, 断开连接", Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), posCmdNum));
                        //            return false;
                        //        }
                        //        else if (client.CheckCheatData.ProcessBoosterTicks == 0)
                        //        {
                        //            client.CheckCheatData.ProcessBoosterTicks = TimeUtil.NOW();
                        //        }
                        //    }
                        //}

                        TCPOutPacket tcpOutPacket = null;
                        long processBeginTime = TimeUtil.NowEx();
                        TCPProcessCmdResults result = TCPCmdHandler.ProcessCmd(this, tcpInPacket.CurrentSocket, tcpClientPool, tcpRandKey, tcpOutPacketPool, tcpInPacket.PacketCmdID, bytesData, tcpInPacket.PacketDataSize - 1 - 4, out tcpOutPacket);

                        long processTime = (TimeUtil.NowEx() - processBeginTime);
                        if (GameManager.StatisticsMode > 0 || processTime > 50 || result == TCPProcessCmdResults.RESULT_FAILED)
                        {
                            RecordCmdDetail(tcpInPacket.PacketCmdID, processTime, tcpInPacket.PacketDataSize, result);
                        }

                        if (result == TCPProcessCmdResults.RESULT_DATA && null != tcpOutPacket)
                        {
                            //向登陆客户端返回数据
                            socketListener.SendData(tcpInPacket.CurrentSocket, tcpOutPacket);
                        }
                        else if (result == TCPProcessCmdResults.RESULT_FAILED)//解析失败, 直接close socket
                        {
                            if (tcpInPacket.PacketCmdID != (int)TCPGameServerCmds.CMD_LOG_OUT)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Resolve packet faild: {0}, {1}, Close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
                            }
                            return false;
                        }
                    }
                    //#endif
                }
                else
                {
                    var _s = tcpInPacket.CurrentSocket;
                    string uid = _s != null ? GameManager.OnlineUserSession.FindUserID(_s) : "socket is nil";
                    LogManager.WriteLog(LogTypes.Error, string.Format("Verify packet faild: {0},{1}, PacketID: {2}, uid={3}, Close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket), checkErrorCode, uid));
                    return false;
                }
            }
            else if (1 == doType) //正常的登录指令包//策略验证请求
            {
                //直接发送策略数据
                DirectSendPolicyFileData(tcpInPacket);
            }
            else
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Unknow error: {0}, {1}, Close socket", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
                //socketListener.CloseSocket(tcpInPacket.CurrentSocket);
                return false;
            }

            return true;
        }

        //接收的数据包队列
        private Dictionary<TMSKSocket, TCPInPacket> dictInPackets = null;

        public UInt16 LastPacketCmdID(TMSKSocket s)
        {
            UInt16 cmd = (UInt16)TCPGameServerCmds.CMD_UNKNOWN;
            if (s != null && dictInPackets != null)
            {
                lock (dictInPackets)
                {
                    TCPInPacket inPacket = null;
                    if (dictInPackets.TryGetValue(s, out inPacket))
                    {
                        cmd = inPacket.LastPacketCmdID;
                    }
                }
            }
            return cmd;
        }

        //会话队列
        private Dictionary<TMSKSocket, TCPSession> tcpSessions = null;

        /// <summary>
        /// 连接成功通知函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketConnected(object sender, SocketAsyncEventArgs e)
        {
            SocketListener sl = sender as SocketListener;

            //第一次建立连接时创建session
            if (GameManager.FlagOptimizeLock3)
            {
                TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;
                if (null == s.session)
                {
                    s.session = new TCPSession(s);
                }

                s.SortKey64 = DataHelper.SortKey64;
            }
            else
            {
                lock (tcpSessions)
                {
                    TCPSession session = null;
                    TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;
                    if (!tcpSessions.TryGetValue(s, out session))
                    {
                        session = new TCPSession(s);
                        tcpSessions.Add(s, session);
                    }
                }
            }
            //RootWindow.Dispatcher.BeginInvoke((MethodInvoker)delegate
            //{
            //    // 通知界面修改连接数
            //    RootWindow.textBlock1.Text = string.Format("{0}/{1}", KTPlayerManager.GetPlayersCount(), socketListener.ConnectedSocketsCount);
            //});
        }

        /// <summary>
        /// 断开成功通知函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketClosed(object sender, TMSKSocket s)
        {
            SocketListener sl = sender as SocketListener;
            // TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;

            //外部强制清空某个连接
            ExternalClearSocket(s);
        }

        /// <summary>
        /// 外部强制清空某个连接
        /// </summary>
        /// <param name="s"></param>
        public void ExternalClearSocket(TMSKSocket s)
        {
            //关闭某个socket对应的角色连接
            ForceCloseSocket(s);

            //断开连接时，清除session
            if (GameManager.FlagOptimizeLock3)
            {
                if (null != s.session)
                {
                    s.session.release();
                }
            }
            else
            {
                lock (tcpSessions)
                {
                    TCPSession session = null;
                    if (tcpSessions.TryGetValue(s, out session))
                    {
                        session.release();
                        tcpSessions.Remove(s);
                    }
                }
            }

            if (GameManager.FlagOptimizeThreadPool3)
            {
                s.MyDispose();
            }
        }

        /// <summary>
        /// 接收数据通知函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool SocketReceived(object sender, SocketAsyncEventArgs e)
        {
            SocketListener sl = sender as SocketListener;
            TCPInPacket tcpInPacket = null;
            AsyncUserToken userToken = (e.UserToken as AsyncUserToken);
            TMSKSocket s = userToken.CurrentSocket;
            if (GameManager.FlagOptimizeLock)
            {
                tcpInPacket = s._TcpInPacket;
                if (null == tcpInPacket)
                {
                    lock (dictInPackets) //锁定接收包队列
                    {
                        if (!dictInPackets.TryGetValue(s, out tcpInPacket))
                        {
                            tcpInPacket = tcpInPacketPool.Pop(s, TCPCmdPacketEvent);
                            dictInPackets[s] = tcpInPacket;
                        }
                    }
                    s._TcpInPacket = tcpInPacket;
                }
            }
            else
            {
                lock (dictInPackets) //锁定接收包队列
                {
                    if (!dictInPackets.TryGetValue(s, out tcpInPacket))
                    {
                        tcpInPacket = tcpInPacketPool.Pop(s, TCPCmdPacketEvent);
                        dictInPackets[s] = tcpInPacket;
                    }
                }
            }

            //处理收到的包
            if (!tcpInPacket.WriteData(e.Buffer, e.Offset, e.BytesTransferred))
            {
                //LogManager.WriteLog(LogTypes.Error, string.Format("接收到非法数据长度的tcp命令, 需要立即断开!"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 发送数据通知函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketSended(object sender, SocketAsyncEventArgs e)
        {
            ////SocketListener sl = sender as SocketListener;
            ////TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;
            //TCPOutPacket tcpOutPacket = (e.UserToken as AsyncUserToken).Tag as TCPOutPacket;
            ////采用发送数据缓存后Tag被绑定为null
            //if (null != tcpOutPacket)
            //{
            //    tcpOutPacketPool.Push(tcpOutPacket);
            //}

            AsyncUserToken userToken = (e.UserToken as AsyncUserToken);
            if (!GameManager.FlagOptimizeThreadPool5 && !GameManager.FlagOptimizeThreadPool4)
            {
                MemoryBlock item = userToken.Tag as MemoryBlock;
                if (null != item)
                {
                    if (GameManager.FlagOptimizeThreadPool2)
                    {
                        TMSKThreadStaticClass.GetInstance().PushMemoryBlock(item);
                    }
                    else
                    {
                        Global._MemoryManager.Push(item);
                    }
                }
            }

            if (GameManager.FlagOptimizeThreadPool4)
            {
                SendBuffer sendBuffer = userToken._SendBuffer;
                if (null != sendBuffer)
                {
                    sendBuffer.Reset2();
                }
            }

            TMSKSocket s = userToken.CurrentSocket;
            Global._SendBufferManager.OnSendBufferOK(s);
        }

        #endregion //事件处理

        #region 直接发送数据(同步发送，阻塞方式)

        /// <summary>
        /// 直接发送策略数据
        /// </summary>
        /// <param name="tcpInStream"></param>
        private void DirectSendPolicyFileData(TCPInPacket tcpInPacket)
        {
            TMSKSocket s = tcpInPacket.CurrentSocket;

            try
            {
                s.Send(TCPPolicy.PolicyServerFileContent, TCPPolicy.PolicyServerFileContent.Length, SocketFlags.None);

                LogManager.WriteLog(LogTypes.Info, string.Format("向客户端返回策略文件数据: {0}", Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Info, string.Format("向客户端返回策略文件时，socket出现异常，对方已经关闭: {0}", Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
            }
        }

        #endregion 直接发送数据(同步发送，阻塞方式)
    }
}
