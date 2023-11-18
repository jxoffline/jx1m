using GameDBServer.Core;

using GameDBServer.DB;
using GameDBServer.Logic;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameDBServer.Server
{

    internal class TCPManager
    {
        private static TCPManager instance = new TCPManager();

        public static long processCmdNum = 0;
        public static long processTotalTime = 0;
        public static Dictionary<int, PorcessCmdMoniter> cmdMoniter = new Dictionary<int, PorcessCmdMoniter>();

        private TCPManager()
        { }

        public static TCPManager getInstance()
        {
            return instance;
        }

        public void initialize(int capacity)
        {
            capacity = Math.Max(capacity, 250);
            socketListener = new SocketListener(capacity, (int)TCPCmdPacketSize.MAX_SIZE / 4);
            socketListener.SocketClosed += SocketClosed;
            socketListener.SocketConnected += SocketConnected;
            socketListener.SocketReceived += SocketReceived;
            socketListener.SocketSended += SocketSended;

            tcpInPacketPool = new TCPInPacketPool(capacity);
            /*            tcpOutPacketPool = new TCPOutPacketPool(capacity * 5);*/
            tcpOutPacketPool = TCPOutPacketPool.getInstance();
            tcpOutPacketPool.initialize(capacity * 5);
            TCPCmdDispatcher.getInstance().initialize();
            dictInPackets = new Dictionary<Socket, TCPInPacket>(capacity);
            gameServerClients = new Dictionary<Socket, GameServerClient>();
        }

        public GameServerClient getClient(Socket socket)
        {
            GameServerClient client = null;
            gameServerClients.TryGetValue(socket, out client);
            return client;
        }


        private SocketListener socketListener = null;

        public SocketListener MySocketListener
        {
            get { return socketListener; }
        }


        private TCPInPacketPool tcpInPacketPool = null;


        private TCPOutPacketPool tcpOutPacketPool = null;

        public Program RootWindow
        {
            get;
            set;
        }

        /// <summary>

        public DBManager DBMgr
        {
            get;
            set;
        }


        public void Start(string ip, int port)
        {
            socketListener.Init();
            socketListener.Start(ip, port);
        }

        public void Stop()
        {
            socketListener.Stop();
        }

        #region

        [ThreadStatic]
        public static GameServerClient CurrentClient;

        private bool TCPCmdPacketEvent(object sender)
        {
            TCPInPacket tcpInPacket = sender as TCPInPacket;


            TCPOutPacket tcpOutPacket = null;
            TCPProcessCmdResults result = TCPProcessCmdResults.RESULT_FAILED;

            GameServerClient client = null;
            if (!gameServerClients.TryGetValue(tcpInPacket.CurrentSocket, out client))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("未建立会话或会话已关闭: {0},{1}, 关闭连接", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));
                return false;
            }

            CurrentClient = client;


            long processBeginTime = TimeUtil.NowEx();

            result = TCPCmdHandler.ProcessCmd(client, DBMgr, tcpOutPacketPool, tcpInPacket.PacketCmdID, tcpInPacket.GetPacketBytes(), tcpInPacket.PacketDataSize, out tcpOutPacket);

            long processTime = (TimeUtil.NowEx() - processBeginTime);
            if (result == TCPProcessCmdResults.RESULT_DATA && null != tcpOutPacket)
            {

                socketListener.SendData(tcpInPacket.CurrentSocket, tcpOutPacket);
            }
            else if (result == TCPProcessCmdResults.RESULT_FAILED)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("解析并执行命令失败: {0},{1}, 关闭连接", (TCPGameServerCmds)tcpInPacket.PacketCmdID, Global.GetSocketRemoteEndPoint(tcpInPacket.CurrentSocket)));

                return false;
            }

            lock (cmdMoniter)
            {
                int cmdID = tcpInPacket.PacketCmdID;
                PorcessCmdMoniter moniter = null;
                if (!cmdMoniter.TryGetValue(cmdID, out moniter))
                {
                    moniter = new PorcessCmdMoniter(cmdID, processTime);
                    cmdMoniter.Add(cmdID, moniter);
                }

                moniter.onProcessNoWait(processTime);
            }

            CurrentClient = null;
            return true;
        }


        private Dictionary<Socket, TCPInPacket> dictInPackets = null;


        private Dictionary<Socket, GameServerClient> gameServerClients = null;

        private void SocketConnected(object sender, SocketAsyncEventArgs e)
        {
            SocketListener sl = sender as SocketListener;

            RootWindow.TotalConnections = sl.ConnectedSocketsCount;

            lock (gameServerClients)
            {
                GameServerClient client = null;
                Socket s = (e.UserToken as AsyncUserToken).CurrentSocket;
                if (!gameServerClients.TryGetValue(s, out client))
                {
                    client = new GameServerClient(s);
                    gameServerClients.Add(s, client);
                }
            }
        }


        private void SocketClosed(object sender, SocketAsyncEventArgs e)
        {
            SocketListener sl = sender as SocketListener;
            Socket s = (e.UserToken as AsyncUserToken).CurrentSocket;

            lock (dictInPackets)
            {
                if (dictInPackets.ContainsKey(s))
                {
                    TCPInPacket tcpInPacket = dictInPackets[s];
                    tcpInPacketPool.Push(tcpInPacket); //缓冲回收
                    dictInPackets.Remove(s);
                }
            }


            lock (gameServerClients)
            {
                GameServerClient client = null;
                if (gameServerClients.TryGetValue(s, out client))
                {
                    client.release();
                    gameServerClients.Remove(s);
                }
            }


            RootWindow.TotalConnections = sl.ConnectedSocketsCount;

        }


        private bool SocketReceived(object sender, SocketAsyncEventArgs e)
        {
            SocketListener sl = sender as SocketListener;
            TCPInPacket tcpInPacket = null;
            Socket s = (e.UserToken as AsyncUserToken).CurrentSocket;
            lock (dictInPackets) //锁定接收包队列
            {
                if (!dictInPackets.TryGetValue(s, out tcpInPacket))
                {
                    tcpInPacket = tcpInPacketPool.Pop(s, TCPCmdPacketEvent);
                    dictInPackets[s] = tcpInPacket;
                }
            }


            if (!tcpInPacket.WriteData(e.Buffer, e.Offset, e.BytesTransferred))
            {

                return false;
            }

            return true;
        }


        private void SocketSended(object sender, SocketAsyncEventArgs e)
        {

            TCPOutPacket tcpOutPacket = (e.UserToken as AsyncUserToken).Tag as TCPOutPacket;
            tcpOutPacketPool.Push(tcpOutPacket);
        }

        #endregion
    }
}