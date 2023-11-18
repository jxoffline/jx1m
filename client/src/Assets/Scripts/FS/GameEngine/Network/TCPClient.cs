#define InternalTcpClient_On
#if InternalTcpClient_On
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Timers;
using Server.Tools;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;

namespace FS.GameEngine.Network
{
	//定义通讯连接通知事件
	public delegate void SocketConnectEventHandler(object sender, SocketConnectEventArgs e);

    public delegate bool ProcessServerCmdHandler(TCPClient client, int nID, byte[] data, int count);



    /// <summary>
    /// TCP连接客户端管理类
    /// </summary>
    public class TCPClient
    {
        public static ProcessServerCmdHandler ProcessServerCmd = null;

        // 统计发送与接收的总字节数
        public static int snTotalSendCount = 0;
        public static int snTotalRecvCount = 0;

        public TCPClient(int capacity = 2)
        {
			_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_MyTCPInPacket = new TCPInPacket((int)TCPCmdPacketSize.MAX_SIZE);
			_MyTCPInPacket.TCPCmdPacketEvent += TCPCmdPacketEvent;
			
            tcpOutPacketPool = new TCPOutPacketPool(capacity * 5);			
        }

        /// <summary>
        /// 清空操作
        /// </summary>
        public void Destroy()
        {
            //KTDebug.Log("TCPClient Destroy");
            StopConnectTimer();
			_MyTCPInPacket.TCPCmdPacketEvent -= TCPCmdPacketEvent;
            _Socket = null;
        }

        /// <summary>
        /// 接收数据缓冲
        /// </summary>
        private byte[] mReceiveBuffer = new byte[(int)TCPCmdPacketSize.RECV_MAX_SIZE];

        /// <summary>
        /// 异步接收处理对象
        /// </summary>
        //private SocketAsyncEventArgs mReceiveAsyncArgs;

        /// <summary>
        /// 发送的命令包缓冲池
        /// </summary>
        private TCPOutPacketPool tcpOutPacketPool = null;

        /// <summary>
        /// TCP命令出口缓冲池
        /// </summary>
        public TCPOutPacketPool OutPacketPool
        {
            get { return tcpOutPacketPool; }
        }		
		
		private bool _Connected = false;		
		public bool Connected
		{
			get { return _Connected; }
			set { _Connected = value; }
		}

        /// <summary>
        /// 记录要连接的IP
        /// </summary>
        private string _ServerIP = "";
		public string RemoteIP
		{
			get { return _ServerIP; }
			set { _ServerIP = value; }
		}		

        /// <summary>
        /// 记录要连接的端口
        /// </summary>
        private int _ServerPort = 0;
		public int RemotePort
		{
			get { return _ServerPort; }
			set { _ServerPort = value; }
		}
		
        /// <summary>
        /// 连接成功的Socket
        /// </summary>
        private Socket _Socket = null;

        /// <summary>
        /// Socket属性
        /// </summary>
        public Socket CurrentSocket
        {
            get { return _Socket; }
        }
		
		private TCPInPacket _MyTCPInPacket = null;
		public TCPInPacket MyTCPInPacket
		{
			get { return _MyTCPInPacket; }
		}
		
		private long _LastSendDataTicks = 0;		
		public long LastSendDataTicks
		{
			get { return _LastSendDataTicks; }
		}

        /// <summary>
        /// 接收超时
        /// </summary>
        private int _ReceiveTimeout = 10 * 1000 * 100;

        /// <summary>
        /// 接收超时
        /// </summary>
        public int ReceiveTimeout { get { return _ReceiveTimeout; } set { _ReceiveTimeout = value; } }

        /// <summary>
        /// 发送超时
        /// </summary>
        private int _SendTimeout = 10 * 1000;

        /// <summary>
        /// 发送超时
        /// </summary>
        public int SendTimeout { get { return _SendTimeout; } set { _SendTimeout = value; } }

        //定义通知事件
        public event SocketConnectEventHandler SocketConnect;

        /// <summary>
        /// 连接超时检测定时器
        /// </summary>
        private Timer timerConnectTimeout;

        /// <summary>
        /// 连接后是否回调
        /// </summary>
        private bool bSocketConnectCallbacked;

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public bool Connect(string ip, int port)
        {
			if (_Connected  || _Socket == null)
			{
				return false;
			}


            //Debug.LogError(ip + "PORT :" + port);
			_ServerIP = ip;
			_ServerPort = port;
			
			try
			{
                // 每10秒对其中一个字典进行检测
                timerConnectTimeout = new Timer(15 * 1000);
                timerConnectTimeout.Elapsed += new ElapsedEventHandler(CheckConnectTimeout);
                timerConnectTimeout.Interval = 15 * 1000;
                timerConnectTimeout.Enabled = true;

#if UNITY_IPHONE
				IPHostEntry ip222 = System.Net.Dns.GetHostEntry(_ServerIP);
				IPAddress ipAddress = ip222.AddressList[0];
				
				if(!Socket.OSSupportsIPv6 || ipAddress.AddressFamily != AddressFamily.InterNetworkV6 )
				{
					KTDebug.LogError("IPV6 not support!");
					_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				}
				else
				{
                    KTDebug.LogError("IPV6 support!");
					_Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
				}
#else


#endif
                IPAddress[] hostAddresses = Dns.GetHostAddresses(this._ServerIP);
                IPAddress iPAddress = hostAddresses[0];

                //Debug.Log("ipAddress = " + iPAddress);
                //Debug.Log("ipAddress.AddressFamily = " + iPAddress.AddressFamily);

                IPEndPoint iep = new IPEndPoint(iPAddress, _ServerPort);

                _Socket.ReceiveTimeout = _ReceiveTimeout;
                _Socket.SendTimeout = _SendTimeout;

                bSocketConnectCallbacked = false;
				_Socket.BeginConnect(iep, new AsyncCallback(SocketConnected), null);
                //KTDebug.Log("TCPClient Connect:");
				return true;
			}
			catch(Exception e)
			{
                KTDebug.LogException(e);
            }

            //if (null != SocketConnect)
            //{
            //    SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Connect Fail", NetSocketType = (int)NetSocketTypes.SOCKET_CONN });
            //}
			
			return false;			
        }

        private void StopConnectTimer()
        {
            if (null != timerConnectTimeout)
            {
                timerConnectTimeout.Stop();
                timerConnectTimeout.Enabled = false;
                timerConnectTimeout = null;
            }
        }

        /// <summary>
        /// 检测连接是否超时
        /// </summary>
        private void CheckConnectTimeout(object source, ElapsedEventArgs e)
        {
            StopConnectTimer();

            // 连接超时，发送无法连接消息
            if (!bSocketConnectCallbacked)
            {
                if (null != SocketConnect)
                {
                    SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Connect Fail", NetSocketType = (int)NetSocketTypes.SOCKET_CONN });
                }

                Disconnect();
            }
        }


     

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="tcpOutPacket"></param>
        public bool SendData(TCPOutPacket tcpOutPacket)
        {
            if (((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SPR_CHECK" ||
                ((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SPR_POSITION" ||
                ((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SPR_USEGOODS" ||
                ((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SPR_MOVE" ||
                ((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SPR_UPDATE_ROLEDATA" ||
                ((TCPGameServerCmds)tcpOutPacket.PacketCmdID).ToString() == "CMD_SYNC_TIME_BY_CLIENT"
                )
            { }
            else
            {
                //KTDebug.Log("Send packet from Client: " + (TCPGameServerCmds) (int) tcpOutPacket.PacketCmdID);
            }


            if (null == _Socket)
			{
				return false;
			}

            // 未连接的时候不发送
            if (!_Socket.Connected)
            {
                return false;
            }
			
			try
			{
				//字节排序
				DataHelper.SortBytes(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);

                // 统计发送字节数
                snTotalSendCount += tcpOutPacket.PacketDataSize;

                _Socket.BeginSend(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize, SocketFlags.None, new AsyncCallback(SocketSended), null);				
				_LastSendDataTicks = DateTime.Now.Ticks / 10000;
				
				return true;
			}
			catch(Exception e)
			{
                KTDebug.LogException(e);
            }
			
		    this.SocketConnect?.Invoke(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Failed", NetSocketType = (int)NetSocketTypes.SOCKET_SEND });
			
			return false;			
        }

        /// <summary>
        /// 断开与客户端的连接
        /// </summary>
        public void Disconnect(SocketShutdown how = SocketShutdown.Both)
        {
            if (null == _Socket)
                return;

            //KTDebug.Log("TCPClient: Disconnect");

			_Connected = false;
            //_Socket?.Close();
            if (_Socket.Connected)
            {
                // KTDebug.LogError("Đang báo Memory Leak cái mẹ gì ở đây. Check sau");
                _Socket.Shutdown(how);
            }
            _Socket = null;
        }

        /// <summary>
        /// 通知接收到的命令
        /// </summary>
        /// <param name="e"></param>
        public void NotifyRecvData(SocketConnectEventArgs e)
        {
            if (null != SocketConnect)
            {
                SocketConnect(this, e);
            }
        }
		
		public string GetRemoteEndPoint()
		{
			try
			{
				 return string.Format("{0}:{1}", _ServerIP, _ServerPort);
			}
			catch (Exception e)
			{
                KTDebug.LogException(e);
            }
			
			return null;
		}		

        #region 事件处理

        /// <summary>
        /// 命令包接收完毕后的回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool TCPCmdPacketEvent(object sender)
        {
            TCPInPacket tcpInPacket = sender as TCPInPacket;
            Socket s = tcpInPacket.CurrentSocket;

            //接收到了完整的命令包
            bool ret = false;
            ret = TCPCmdHandler.ProcessServerCmd(this, tcpInPacket.PacketCmdID, tcpInPacket.GetPacketBytes(), tcpInPacket.PacketDataSize);
            return ret;
        }

        /// <summary>
        /// 连接成功通知函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketConnected(IAsyncResult iar)
        {
			try
			{
                //KTDebug.Log("TCPClient SocketConnected");
				if (null == _Socket)
				{
					return;
				}
				
	            // Complete the connection.
	            _Socket.EndConnect(iar);
				
				//KTDebug.Log("SocketConnected 连接已经建立");
			
				if (ProtocolTypes.EnableTengXunTGW)
				{
					string tgwHeader = string.Format("tgw_l7_forward\r\nHost: {0}:{1}\r\n\r\n", this._ServerIP, this._ServerPort);
					byte[] bytesCmd = new UTF8Encoding().GetBytes(tgwHeader);					
					_Socket.Send(bytesCmd);
				}
			
				_Connected = true;
                //苹果64位引发崩溃
                //mReceiveAsyncArgs = new SocketAsyncEventArgs();
                //mReceiveAsyncArgs.Completed += SocketReceived;
                //mReceiveAsyncArgs.SetBuffer(mReceiveBuffer, 0, mReceiveBuffer.Length);
                //_Socket.ReceiveAsync(mReceiveAsyncArgs);

                _Socket.BeginReceive(mReceiveBuffer, 0, mReceiveBuffer.Length, SocketFlags.None,
                                     new AsyncCallback(SocketReceived), mReceiveBuffer);
				
	            if (null != SocketConnect)
	            {
                    DataHelper.ClearKey(); // !!! important
	                SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Success", NetSocketType = (int)NetSocketTypes.SOCKET_CONN });
	            }
			}
			catch(Exception e)
			{
                KTDebug.LogException(e);

                if (null != SocketConnect)
                {
                    SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Connect Fail", NetSocketType = (int) NetSocketTypes.SOCKET_CONN });
                }
            }
        }

		/// <summary>
		/// 断开成功通知函数
		/// </summary>
        private void DoSocketClosed()
        {
            //KTDebug.LogStackMsg("DoSocketClosed: \r\n");
            //判断是否通知窗体
            if (null != _Socket)
            {
                if (null != SocketConnect)
                {
                    SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Success", NetSocketType = (int)NetSocketTypes.SOCKET_CLOSE });
                }
            }
            //KTDebug.Log("DoSocketClosed");

            if (_Socket != null)
            {
                //将socket清空
                _Socket.Close();
                _Socket = null;
            }

            _Connected = false;            
        }

        /// <summary>
        /// 接收数据通知函数,使用老版本接口
        /// </summary>
        /// <returns>
        /// The received.
        /// </returns>
        private void SocketReceived(IAsyncResult iar)
        {
            try
            {
                if (null == _Socket)
                {
                    return;
                }

                SocketError socketError = SocketError.Success;
                int recvLength = 0;
                if (_Socket.Connected)  //添加判断是否是connected，避免被close后，抛出System.ObjectDisposedException: The object was used after being disposed异常
                {
                    recvLength = _Socket.EndReceive(iar, out socketError);
                }

                //if (socketError != SocketError.Success)
                if (recvLength <= 0)
                {
                    //断开成功通知函数
                    DoSocketClosed();
                    return;
                }

                // 统计接收字节数
                snTotalRecvCount += recvLength;

                //处理收到的包
                if (!_MyTCPInPacket.WriteData(mReceiveBuffer, 0, recvLength))
                {
                    return;
                }

                // 收到第一个包，停止连接超时检测
                if (!bSocketConnectCallbacked)
                {
                    bSocketConnectCallbacked = true;
                    StopConnectTimer();
                }

                _Socket.BeginReceive(mReceiveBuffer, 0, mReceiveBuffer == null ? 0 : mReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(SocketReceived), mReceiveBuffer);
                return;
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }

            //如果是接收失败, 则就要立即告诉界面，同时通知客户端无法连接数据库
            if (null != SocketConnect)
            {
                SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Failed", NetSocketType = (int)NetSocketTypes.SOCKET_RECV });
            }

        }
        /*
		/// <summary>
		/// 接收数据通知函数，使用新版本接口，已确认在苹果64位上引发崩溃
		/// </summary>
		/// <returns>
		/// The received.
		/// </returns>
        private void SocketReceived(object obj, SocketAsyncEventArgs e)
        {
			try
			{
                if (null == _Socket)
				{
					return;
				}
				
                if (!_Socket.Connected)  //添加判断是否是connected，避免被close后，抛出System.ObjectDisposedException: The object was used after being disposed异常
                {
                    return;
                }
				
                if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)   //yaozb修复空指针
				{
					//断开成功通知函数
        			DoSocketClosed();
					return;
				}

                // 统计接收字节数
                snTotalRecvCount += e.BytesTransferred;

	            //处理收到的包
	            if (!_MyTCPInPacket.WriteData(e.Buffer, 0, e.BytesTransferred))
	            {
	                return;
	            }

                // 收到第一个包，停止连接超时检测
                if (!bSocketConnectCallbacked)
                {
                    bSocketConnectCallbacked = true;
                    StopConnectTimer();
                }

                if (!_Socket.ReceiveAsync(mReceiveAsyncArgs))
                    SocketReceived(null, mReceiveAsyncArgs);
				return;
			}
			catch(Exception ex)
			{
                //KTDebug.LogException(ex);
            }
			
            //如果是接收失败, 则就要立即告诉界面，同时通知客户端无法连接数据库
			if (null != SocketConnect)
			{
				SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Failed", NetSocketType = (int)NetSocketTypes.SOCKET_RECV });
			}
        }
        */
		/// <summary>
		/// 发送数据通知函数
		/// </summary>
		/// <param name='iar'>
		/// Iar.
		/// </param>
        private void SocketSended(IAsyncResult iar)
        {
			try
			{
				if (null == _Socket)
				{
					return;
				}
				
				SocketError socketError = SocketError.Success;
				_Socket.EndSend(iar, out socketError);
				
				if (socketError != SocketError.Success)
				{
					//断开成功通知函数
        			DoSocketClosed();
					return;
				}
				
				return;
			}
			catch(Exception e)
			{
                KTDebug.LogException(e);
            }
			
            //如果是发送失败, 则就要立即告诉界面，同时通知客户端无法连接数据库
			if (null != SocketConnect)
			{
				SocketConnect(this, new SocketConnectEventArgs() { RemoteEndPoint = GetRemoteEndPoint(), Error = "Failed", NetSocketType = (int)NetSocketTypes.SOCKET_SEND });
			}
        }

        #endregion //事件处理
    }
}
#endif