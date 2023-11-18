using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
//using System.Windows.Forms;
//using System.Windows.Threading;
using GameServer.Logic;

namespace GameServer.Server
{
    /// <summary>
    /// TCP连接DB服务器端的客户端类
    /// </summary>
    public class TCPClient
    {
        public TCPClient()
        {          
        }

        /// <summary>
        /// 互斥锁对象
        /// </summary>
        private object MutexSocket = new object();

        /// <summary>
        /// 连接成功的TMSKSocket
        /// </summary>
        private TMSKSocket _Socket = null;

        /// <summary>
        /// 主窗口对象
        /// </summary>
        public IConnectInfoContainer RootWindow
        {
            get;
            set;
        }

        /// <summary>
        /// 在ListBox中的索引值
        /// </summary>
        public int ListIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 记录要连接的IP
        /// </summary>
        private string ServerIP = "";

        /// <summary>
        /// 记录要连接的端口
        /// </summary>
        private int ServerPort = 0;

        /// <summary>
        /// 要连接的服务器的名称
        /// </summary>
        public string ServerName = "";

        /// <summary>
        /// Chấp nhận delay hay không
        /// </summary>
        public bool NoDelay = true;

        public bool ValidateIpPort(string ip, int port)
        {
            if (ip != ServerIP || port != ServerPort)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port, string serverName)
        {
            ServerName = serverName;

            lock (MutexSocket)
            {
                if (null != _Socket) return; //已经连接

                ServerIP = ip;
                ServerPort = port;
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _Socket = new TMSKSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _Socket.SendTimeout = 30 * 1000;//设置TMSKSocket的接收超时时间为30S。
                _Socket.ReceiveTimeout = 30 * 1000;//设置TMSKSocket的接收超时时间为30S。
                _Socket.NoDelay = NoDelay;

                try
                {
                    _Socket.Connect(remoteEndPoint);
                }
                catch (Exception)
                {
                    //通知主窗口显示连接数
                    RootWindow.AddDBConnectInfo(ListIndex, string.Format("{0}, failed to connect to {1}: {2}:{3}", ListIndex, ServerName, ip, port));

                    LogManager.WriteLog(LogTypes.Error, string.Format("{0}, failed to connect to {1}: {2}:{3}", ListIndex, ServerName, ip, port));
                    _Socket = null;
                    throw; //继续抛出异常
                }

                Global.SendGameServerHeart(this);   // 心跳
                RootWindow.AddDBConnectInfo(ListIndex, string.Format("{0}, successfully connected to {1}: {2}", ListIndex, ServerName, remoteEndPoint));
            }
        }

        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        public void Disconnect()
        {
            lock (MutexSocket)
            {
                if (null == _Socket) return; //无连接

                RootWindow.AddDBConnectInfo(ListIndex, string.Format("{0}, disconnected from {1}: {2}", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket)));

                try
                {
                    _Socket.Shutdown(SocketShutdown.Receive);
                    _Socket.Close(30);
                }
                catch (Exception)
                {
                }

                _Socket = null;
            }
        }

        /// <summary>
        /// 是否还在连接中
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            bool ret = false;
            lock (MutexSocket)
            {
                ret = (null != _Socket);
            }

            return ret;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="tcpOutPacket"></param>
        public byte[] SendData(TCPOutPacket tcpOutPacket)
        {
            lock (MutexSocket)
            {
                if (null == _Socket) return null; //还没连接

                try
                {
                    //调试信息
                    if (LogManager.LogTypeToWrite >= LogTypes.Info && LogManager.LogTypeToWrite <= LogTypes.Warning)
                    {
                        //string cmdData = new UTF8Encoding().GetString(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);
                        //LogManager.WriteLog(LogTypes.Warning, string.Format("{0}, 向{1}:{2}, 发送命令{3}, 数据 {4}", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket), (TCPGameServerCmds)tcpOutPacket.PacketCmdID, cmdData));
                    }

                    //字节排序
                    //DataHelper.SortBytes(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);

                    //将数据发送给对方
                    _Socket.Send(tcpOutPacket.GetPacketBytes(), tcpOutPacket.PacketDataSize, SocketFlags.None);

                    //从对方哪儿接收数据
                    byte[] data = new byte[4];
                    int n = _Socket.Receive(data, 0, 4, SocketFlags.None);
                    if (n != 4) //返回失败
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("{0}, failed send data to {1}: {2}, packet length is incorrect", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket)));
                        return null;
                    }

                    //字节排序
                    //DataHelper.SortBytes(data, 0, 4);

                    Int32 length = BitConverter.ToInt32(data, 0);

                    byte[] dataTmp = new byte[length + 4];
                    DataHelper.CopyBytes(dataTmp, 0, data, 0, 4);

                    data = dataTmp;

                    Int32 totalReceived = 0;
                    while (totalReceived < length)
                    {
                        n = _Socket.Receive(data, 4 + totalReceived, length - totalReceived, SocketFlags.None);

                        //字节排序
                        //DataHelper.SortBytes(data, 4 + totalReceived, n);

                        totalReceived += (Int32)n;
                    }

                    if (totalReceived != length) //返回失败
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("{0}, failed to send data to {1}: {2}, the returned packet data length: {3} does not match the received data length: {4}", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket), length, totalReceived));
                        return null;
                    }

                    return data;
                }
                catch (Exception ex)
                {
                    //断开连接
                    Disconnect();

                    try
                    {
                        string cmdData = new UTF8Encoding().GetString(tcpOutPacket.GetPacketBytes(), 6, tcpOutPacket.PacketDataSize - 6);
                        LogManager.WriteLog(LogTypes.Error, string.Format("{0}, failed to send data to {1}:{2}, command {3}, data {4}, length {5}, exception message: {6}", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket), (TCPGameServerCmds)tcpOutPacket.PacketCmdID, cmdData, tcpOutPacket.PacketDataSize - 6, ex.Message));

                        //打印错误堆栈
                        DataHelper.WriteExceptionLogEx(ex, string.Format("Exception while sending data to {0}", ServerName));
                    }
                    catch (Exception)
                    {
                    }

                    RootWindow.AddDBConnectInfo(ListIndex, string.Format("{0}, failed to send data to {1}: {2}", ListIndex, ServerName, Global.GetSocketRemoteEndPoint(_Socket)));
                }

                return null;
            }
        }
    }
}
