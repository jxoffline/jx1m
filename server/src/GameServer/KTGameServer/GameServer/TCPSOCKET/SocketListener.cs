using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using Server.Tools;
using Server.Protocol;
using GameServer.Logic;
using GameServer.Server;
using GameServer.Tools;
using GameServer.Core.Executor;

namespace Server.TCP
{
    /*臭名昭著的指令乱序分析:
     * 1. 如果A socket的接收事件在处理过程中，被其他线程发送时错误，关闭socket，此时TCPInPacket中的socket句柄会被清空，导致错误。但是其他线程由于锁的作用，无法清空其
     * 正在处理的TCPInPacket内部数据，所以不会导致其指令乱序，但是当其接收异常，再次处理关闭时，由于无法在字典中找到TCPInPacket所以，无法对TCPInPacket不再处理，TCPInPacket
     * 被其他线程处理清空掉，此时逻辑是正确的，只是会再不加判断的情况下导致多次减少socket计数。
     * 2. 发送时如果处理关闭操作，会导致当对方关闭，自己发送失败时，多次执行关闭操作。但是对于指令流乱序没有影响。
     * 3. 如果A socket的接收时间在处理过程中，执行发送出错，会关闭socket，执行清空内存的操作，此时TCPInPacket中的socket句柄会被清空，导致错误。并且没有终止操作，继续递归循环处理。
     * 同时由于自己将TCPInPacket归还给了队列中，其他接收就会有机会取出使用，当其他socket 接收取出使用时，会重新对于socket赋值，导致A socket的接收中的发送处理逻辑出错，发送给了
     * 错误的客户端对象。同时，由于此时自己的递归循环退出时可能会遗留半截指令数据。其他使用这个TCPInPacket的socket接收到的数据，进去其继续进行处理则会导致乱序的指令流出现。
     * 就是A 的遗留半截指令，加上其他例如B的新的指令数据，导致B后续接收全部出错。呵呵。终于搞懂了，YES。
     * 解决方案:
     * a. 发送时不进行关闭处理，交给接收去做。
     * b. 接收时，如果出现错误，终止递归循环，退出。
     * c. 接收部分，发送数据处理出错，则立刻执行关闭TMSKSocket操作。
     * d. 就是原则上，关闭操作交给接收去处理。
     */

    /// <summary>
    /// 服务器侦听类
    /// </summary>
    public sealed class SocketListener
    {
        public int GTotalSendCount = 0;

        /// <summary>
        /// 是否开启ip白名单过滤
        /// </summary>
        private bool EnabledIPListFilter = false;

        /// <summary>
        /// IP白名单列表
        /// </summary>
        private Dictionary<string, bool> IPWhiteList = new Dictionary<string, bool>();

        /// <summary>
        /// 缓冲区的大小
        /// </summary>
        Int32 ReceiveBufferSize;

        /// <summary>
        /// Represents a large reusable set of buffers for all socket operations.
        /// </summary>
        private BufferManager bufferManager;

        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private TMSKSocket listenSocket;

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        //private Int32 numConnectedSockets;

        /// <summary>
        /// 已经连接的TMSKSocket词典对象
        /// </summary>
        private Dictionary<TMSKSocket, bool> ConnectedSocketsDict;

        /// <summary>
        /// 外部获取总的连接个数
        /// </summary>
        public Int32 ConnectedSocketsCount
        {
            get
            {
                Int32 n = 0;
                //Interlocked.Exchange(ref n, this.numConnectedSockets);
                lock (this.ConnectedSocketsDict)
                {
                    n = this.ConnectedSocketsDict.Count;
                }
                return n;
            }
        }

        /// <summary>
        /// the maximum number of connections the sample is designed to handle simultaneously.
        /// </summary>
        public Int32 numConnections;

        /// <summary>
        /// Read, write (don't alloc buffer space for accepts).
        /// </summary>
        private const Int32 opsToPreAlloc = 2;

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for read socket operations.
        /// </summary>
        public SocketAsyncEventArgsPool readPool; //线程安全

        public int ReadPoolCount
        {
            get { return readPool.Count; }
        }

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for write socket operations.
        /// </summary>
        public SocketAsyncEventArgsPool writePool; //线程安全

        public int WritePoolCount
        {
            get { return writePool.Count; }
        }

        /// <summary>
        /// Controls the total number of clients connected to the server.
        /// </summary>
        //private Semaphore semaphoreAcceptedClients;

        /// <summary>
        /// Total # bytes counter received by the server.
        /// </summary>
        private long totalBytesRead;

        /// <summary>
        /// 获取总的接收的字节数
        /// </summary>
        public long TotalBytesReadSize
        {
            get
            {
                long n = 0;
                Interlocked.Exchange(ref n, this.totalBytesRead);
                return n;
            }
        }

        /// <summary>
        /// Total # bytes counter received by the server.
        /// </summary>
        private long totalBytesWrite;

        /// <summary>
        /// 获取总的发送的字节数
        /// </summary>
        public long TotalBytesWriteSize
        {
            get
            {
                long n = 0;
                Interlocked.Exchange(ref n, this.totalBytesWrite);
                return n;
            }
        }

        //是否不再接受新的用户(默认不接受)
        private bool _DontAccept = true;

        /// <summary>
        /// 是否不再接受新的用户
        /// </summary>
        public bool DontAccept
        {
            get { return _DontAccept; }
            set { _DontAccept = value; }
        }

        /// 连接成功通知函数
        public event SocketConnectedEvnetHandler SocketConnected = null;

        /// 断开成功通知函数
        public event SocketClosedEventHandler SocketClosed = null;

        /// 接收数据通知函数
        public event SocketReceivedEventHandler SocketReceived = null;

        /// 发送数据通知函数
        public event SocketSendedEventHandler SocketSended = null;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method.
        /// </summary>
        /// <param name="numConnections">Maximum number of connections to be handled simultaneously.</param>
        /// <param name="receiveBufferSize">Buffer size to use for each socket I/O operation.</param>
        internal SocketListener(Int32 numConnections, Int32 receiveBufferSize)
        {
            this.totalBytesRead = 0;
            this.totalBytesWrite = 0;
            //this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.ReceiveBufferSize = receiveBufferSize;

            int readBuffNum = numConnections * 3;

            // Allocate buffers such that the maximum number of sockets can have one outstanding read and 
            // write posted to the socket simultaneously .
            this.bufferManager = new BufferManager(receiveBufferSize * readBuffNum,
                receiveBufferSize);

            // 已经连接的TMSKSocket词典对象
            this.ConnectedSocketsDict = new Dictionary<TMSKSocket, bool>(readBuffNum);

            this.readPool = new SocketAsyncEventArgsPool(readBuffNum);
            this.writePool = new SocketAsyncEventArgsPool(readBuffNum);
            //this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// 添加socket对象
        /// </summary>
        /// <param name="socket"></param>
        private void AddSocket(TMSKSocket socket)
        {
            lock (this.ConnectedSocketsDict)
            {
                this.ConnectedSocketsDict.Add(socket, true);
            }
        }

        /// <summary>
        /// 删除socket对象
        /// </summary>
        /// <param name="socket"></param>
        private void RemoveSocket(TMSKSocket socket)
        {
            lock (this.ConnectedSocketsDict)
            {
                this.ConnectedSocketsDict.Remove(socket);
            }
        }

        /// <summary>
        /// 查找TMSKSocket对象
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool FindSocket(TMSKSocket socket)
        {
            bool ret = false;
            lock (this.ConnectedSocketsDict)
            {
                ret = this.ConnectedSocketsDict.ContainsKey(socket);
            }

            return ret;
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e, string reason)
        {
            AsyncUserToken aut = e.UserToken as AsyncUserToken;

            TMSKSocket s = null;
            try
            {
                s = aut.CurrentSocket;

                string ip = "未知";

                try
                {
                    ip = string.Format("{0}", s.RemoteEndPoint);
                }
                catch (System.Exception)
                {
                }

                //LogManager.WriteLog(LogTypes.Error, string.Format("Close socket: {0}, Total sockets: {1}, Result 1: {2}, Result 2: {3}", ip, ConnectedSocketsCount, reason, s.CloseReason));
                CloseSocket(s);
            }
            finally
            {
                aut.CurrentSocket = null; //释放
                aut.Tag = null; //释放

                // Free the SocketAsyncEventArg so they can be reused by another client.
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    e.SetBuffer(null, 0, 0); //回收内存
                    if (GameManager.FlagOptimizeThreadPool3)
                    {
                        //TMSKThreadStaticClass.GetInstance().PushReadSocketAsyncEventArgs(e);
                        if (null != s) s.PushWriteSocketAsyncEventArgs(e);
                    }
                    else
                    {
                        this.writePool.Push(e);
                    }
                }
                else if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    if (GameManager.FlagOptimizeThreadPool3)
                    {
                        //TMSKThreadStaticClass.GetInstance().PushReadSocketAsyncEventArgs(e);
                        if (null != s) s.PushReadSocketAsyncEventArgs(e);
                    }
                    else
                    {
                        this.readPool.Push(e);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        internal void Init()
        {
            // Allocates one large Byte buffer which all I/O operations use a piece of. This guards 
            // against memory fragmentation.
            this.bufferManager.InitBuffer();

            // Preallocate pool of SocketAsyncEventArgs objects.
            SocketAsyncEventArgs readWriteEventArg;

            int readBuffNum = numConnections * 3;
            for (Int32 i = 0; i < readBuffNum; i++)
            {
                //for (int j = 0; j < opsToPreAlloc; j++)
                {
                    // Preallocate a set of reusable SocketAsyncEventArgs.
                    readWriteEventArg = new SocketAsyncEventArgs();
                    readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                    readWriteEventArg.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };

                    // Assign a Byte buffer from the buffer pool to the SocketAsyncEventArg object.
                    this.bufferManager.SetBuffer(readWriteEventArg);

                    // Add SocketAsyncEventArg to the pool.
                    this.readPool.Push(readWriteEventArg);
                }
            }

            for (Int32 i = 0; i < readBuffNum; i++)
            {
                //for (Int32 j = 0; j < 5; j++)
                //{
                    readWriteEventArg = new SocketAsyncEventArgs();
                    readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                    readWriteEventArg.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };
                    this.writePool.Push(readWriteEventArg);
                //}
            }
        }

        /// <summary>
        /// Callback method associated with TMSKSocket.AcceptAsync 
        /// operations and is invoked when an accept operation is complete.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (null == this.listenSocket) return; //已经不在侦听

                this.ProcessAccept(e);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Exception at SocketListener::OnAcceptCompleted.\n{0}", ex.ToString()));
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                    //throw ex;
                //});
                DataHelper.WriteFormatExceptionLog(ex, "OnAcceptCompleted", false);
            }
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                // Determine which type of operation just completed and call the associated handler.
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        this.ProcessReceive(e);
                        break;
                    case SocketAsyncOperation.Send:
                        this.ProcessSend(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Exception at SocketListener::OnIOCompleted.\n{0}", ex.ToString()));
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                    //throw ex;
                //});
                DataHelper.WriteFormatExceptionLog(ex, "OnIOCompleted", false);
            }
        }

        /// <summary>
        /// 从socket接收数据
        /// </summary>
        /// <param name="s"></param>
        /// <param name="readEventArgs"></param>
        private bool _ReceiveAsync(SocketAsyncEventArgs readEventArgs)
        {
            try
            {
                TMSKSocket s = (readEventArgs.UserToken as AsyncUserToken).CurrentSocket;
                return s.ReceiveAsync(readEventArgs);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Exception at SocketListener::_ReceiveAsync.\n{0}", ex.ToString()));
                string str = ex.Message.ToString();
                this.CloseClientSocket(readEventArgs, str.Replace('\n', ' '));
                return true;
            }
        }

        /// <summary>
        /// 向TMSKSocket发送数据
        /// </summary>
        /// <param name="writeEventArgs"></param>
        /// <returns></returns>
        private bool _SendAsync(SocketAsyncEventArgs writeEventArgs, out bool exception)
        {
            exception = false;

            try
            {
                TMSKSocket s = (writeEventArgs.UserToken as AsyncUserToken).CurrentSocket;
                return s.SendAsync(writeEventArgs);
            }
            catch (Exception ex) //此处有可能是对象非法等异常, 例如TMSKSocket对象已经无效
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Exception at SocketListener::_SendAsync.\n{0}", ex.ToString()));
                exception = true;
                //this.CloseClientSocket(writeEventArgs);
                return true;
            }
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        internal bool SendData(TMSKSocket s, TCPOutPacket tcpOutPacket, bool pushBack = true)
        {
            if (s != null && tcpOutPacket != null)
            {
                // 广播的时候多个客户端同时使用同一个byte[]，要特别注意
                UInt16 SendPacketId = tcpOutPacket.PacketCmdID;
                Array.Copy(BitConverter.GetBytes(SendPacketId), 0, tcpOutPacket.GetPacketBytes(), 4, 2);
            }

#if true //原来的直接发送，改为异步线程发送，减少锁定

            bool bRet = false;
            if (null != s)
            {
                // 只有连接状态才发送消息 ChenXiaojun
                if ((s as TMSKSocket).Connected)
                {
                    //缓冲数据包
                    if (GameManager.FlagSkipSendDataCall)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = Global._SendBufferManager.AddOutPacket(s, tcpOutPacket);
                    }
                }
            }

            //还回tcpoutpacket
            if (pushBack)
            {
                Global._TCPManager.TcpOutPacketPool.Push(tcpOutPacket);
            }

            return bRet;
#else

            //将发送的指令送到异步队列
            SendPacketManager.getInstance().addSendPacketWrapper(new SendPacketWrapper()
                {
                    socket = s,
                    tcpOutPacket = tcpOutPacket,
                });

            return true;
#endif //
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        public bool SendData(TMSKSocket s, Byte[] buffer, int offset, int count, MemoryBlock item)
        {
            GTotalSendCount++;
            SocketAsyncEventArgs writeEventArgs;
            if (GameManager.FlagOptimizeThreadPool3)
            {
                writeEventArgs = s.PopWriteSocketAsyncEventArgs(); //线程安全的操作
            }
            else
            {
                writeEventArgs = this.writePool.Pop(); //线程安全的操作
            }
            if (null == writeEventArgs)
            {
                writeEventArgs = new SocketAsyncEventArgs();
                writeEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                writeEventArgs.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };
            }

            writeEventArgs.SetBuffer(buffer, offset, count);
            AsyncUserToken userToken = (writeEventArgs.UserToken as AsyncUserToken);
            userToken.CurrentSocket = s;
            userToken.Tag = item;

            bool exception = false;
            if (GameManager.FlagSkipSocketSend)
            {
                writeEventArgs.SocketError = SocketError.Success;
                this.ProcessSend(writeEventArgs);
            }
            else
            {
                Boolean willRaiseEvent = _SendAsync(writeEventArgs, out exception);
                if (!willRaiseEvent)
                {
                    this.ProcessSend(writeEventArgs);
                }
            }

            if (exception) //此处不处理会导致内存泄露
            {
                /// 发送数据通知函数
                if (null != SocketSended)
                {
                    SocketSended(this, writeEventArgs);
                }

                //什么事情都不做, 收回使用的e和buffer
                // Free the SocketAsyncEventArg so they can be reused by another client.
                writeEventArgs.SetBuffer(null, 0, 0); //回收内存
                userToken.CurrentSocket = null; //释放
                userToken.Tag = null; //释放
                if (GameManager.FlagOptimizeThreadPool3)
                {
                    //TMSKThreadStaticClass.GetInstance().PushSocketAsyncEventArgs(writeEventArgs);
                    s.PushWriteSocketAsyncEventArgs(writeEventArgs);
                }
                else
                {
                    this.writePool.Push(writeEventArgs);
                }
            }

            return (!exception);
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        public bool SendData(TMSKSocket s, Byte[] buffer, int offset, int count, MemoryBlock item, SendBuffer sendBuffer)
        {
            GTotalSendCount++;
            SocketAsyncEventArgs writeEventArgs;
            if (GameManager.FlagOptimizeThreadPool3)
            {
                writeEventArgs = s.PopWriteSocketAsyncEventArgs(); //线程安全的操作
            }
            else
            {
                writeEventArgs = this.writePool.Pop(); //线程安全的操作
            }
            if (null == writeEventArgs)
            {
                writeEventArgs = new SocketAsyncEventArgs();
                writeEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                writeEventArgs.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };
            }

            writeEventArgs.SetBuffer(buffer, offset, count);
            AsyncUserToken userToken = (writeEventArgs.UserToken as AsyncUserToken);
            userToken.CurrentSocket = s;
            userToken.Tag = item;
            userToken._SendBuffer = sendBuffer;

            bool exception = false;
            if (GameManager.FlagSkipSocketSend)
            {
                writeEventArgs.SocketError = SocketError.Success;
                this.ProcessSend(writeEventArgs);
            }
            else
            {
                Boolean willRaiseEvent = _SendAsync(writeEventArgs, out exception);
                if (!willRaiseEvent)
                {
                    this.ProcessSend(writeEventArgs);
                }
            }

            if (exception) //此处不处理会导致内存泄露
            {
                /// 发送数据通知函数
                if (null != SocketSended)
                {
                    SocketSended(this, writeEventArgs);
                }

                //什么事情都不做, 收回使用的e和buffer
                // Free the SocketAsyncEventArg so they can be reused by another client.
                writeEventArgs.SetBuffer(null, 0, 0); //回收内存
                userToken.CurrentSocket = null; //释放
                userToken.Tag = null; //释放
                if (GameManager.FlagOptimizeThreadPool3)
                {
                    s.PushWriteSocketAsyncEventArgs(writeEventArgs);
                }
                else
                {
                    this.writePool.Push(writeEventArgs);
                }
            }

            return (!exception);
        }

        /// <summary>
        /// Process the accept for the socket listener.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //Socket s = e.AcceptSocket;
            TMSKSocket s = new TMSKSocket(e.AcceptSocket);

            //检查IP名单
            bool disableConnect = false;
            bool? inIpWriteList = null;
            if (EnabledIPListFilter)
            {
                lock (IPWhiteList)
                {
                    if (EnabledIPListFilter && null != s && null != s.RemoteEndPoint)
                    {
                        IPEndPoint remoteIPEndPoint = (s.RemoteEndPoint as IPEndPoint);
                        if (null != remoteIPEndPoint && null != remoteIPEndPoint.Address)
                        {
                            string remoteIP = remoteIPEndPoint.Address.ToString();
                            if (!string.IsNullOrEmpty(remoteIP) && !IPWhiteList.ContainsKey(remoteIP))
                            {
                                disableConnect = true;
                                inIpWriteList = false;
                            }
                            else
                            {
                                inIpWriteList = true;
                            }
                        }
                    }
                }
            }

            //是否不再接受新的用户
            if (DontAccept || disableConnect)
            {
                try
                {
                    if (disableConnect)
                    {
                        //LogManager.WriteLog(LogTypes.Error, string.Format("New connection: {0}, but IP address was banned，Force close: {1}", s.RemoteEndPoint, ConnectedSocketsCount));
                    }
                    else if(DontAccept)
                    {
                        //LogManager.WriteLog(LogTypes.Error, string.Format("New connection: {0}, but server don't accept，Force close: {1}", s.RemoteEndPoint, ConnectedSocketsCount));
                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    s.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception)
                {
                    // Throws if client process has already closed.
                }

                try
                {
                    s.Close(30);
                }
                catch (Exception)
                {
                }

                // Accept the next connection request.
                this.StartAccept(e);
                return;
            }

            byte[] inOptionValues = new byte[sizeof(uint) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)120000).CopyTo(inOptionValues, sizeof(uint));
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, sizeof(uint) * 2);
            (s as TMSKSocket).IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

            SocketAsyncEventArgs readEventArgs = null;
            if (GameManager.FlagOptimizeThreadPool3)
            {
                readEventArgs = s.PopReadSocketAsyncEventArgs(); //线程安全的操作
            }
            else
            {
                readEventArgs = this.readPool.Pop();
            }
            if (null == readEventArgs)
            {
                try
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("New connect: {0}, but pool has already full，force close: {1}", s.RemoteEndPoint, ConnectedSocketsCount));
                }
                catch (Exception)
                {
                }

                try
                {
                    s.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception)
                {
                    // Throws if client process has already closed.
                }

                try
                {
                    s.Close(30);
                }
                catch (Exception)
                {
                }

                // Accept the next connection request.
                this.StartAccept(e);
                return;
            }

            //增加计数器
            //Interlocked.Increment(ref this.numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the 
            // ReadEventArg object user token.
            (readEventArgs.UserToken as AsyncUserToken).CurrentSocket = s;

            Global._SendBufferManager.Add(s);

            AddSocket(s);

           // try
           // {
           //     //LogManager.WriteLog(LogTypes.Error, string.Format("New connection: {0}, Total sockets: {1}", s.RemoteEndPoint, ConnectedSocketsCount));
           // }
            //catch (Exception)
           // {
           // }

            /// 连接成功通知函数
            if (null != SocketConnected)
            {
                SocketConnected(this, readEventArgs);
            }

            s.session.InIpWhiteList = inIpWriteList;
            // As soon as the client is connected, post a receive to the connection.
            Boolean willRaiseEvent = _ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                this.ProcessReceive(readEventArgs);
            }

            // Accept the next connection request.
            this.StartAccept(e);
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation.</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken userToken = (e.UserToken as AsyncUserToken);
            TMSKSocket s = userToken.CurrentSocket;

            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                // Increment the count of the total bytes receive by the server.
                Interlocked.Add(ref this.totalBytesRead, e.BytesTransferred);

                bool recvReturn = true;

                // Get the message received from the listener.
                //e.Buffer, e.Offset, e.bytesTransferred);
                //通知外部有新的socket到达
                if (null != SocketReceived)
                {
                    //字节排序
                    if (GameManager.FlagUseWin32Decrypt)
                    {
                        //使用动态库解密
                        int length = e.BytesTransferred;
                        unsafe
                        {
                            fixed (byte* p = e.Buffer)
                            {
                               Win32API.SortBytes(p, e.Offset, e.BytesTransferred, s.SortKey64);
                            }
                        }
                    }
                    else
                    {
                       // Win32API.SortBytes(e.Buffer, e.Offset, e.BytesTransferred, s.SortKey64);
                       DataHelper.SortBytes(e.Buffer, e.Offset, e.BytesTransferred, s.SortKey64);
                    }

                    try
                    {
                        recvReturn = SocketReceived(this, e);
                    }
                    catch (System.Exception ex)
                    {
                        //所有异常应当在下层捕获，不应抛到这层来
                        LogManager.WriteException(ex.ToString());
                        recvReturn = false;
                    }
                }

                if (recvReturn)
                {
                    //继续接收流程(以数据流为驱动)
                    Boolean willRaiseEvent = _ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    UInt16 lastPacketCmd = TCPManager.getInstance().LastPacketCmdID(s);
                    string reason = string.Format("CMD={0}", ((TCPGameServerCmds)lastPacketCmd).ToString());
                    this.CloseClientSocket(e, reason);
                }
            }
            else
            {
                string reason = string.Format("[{0}]{1}", (int)e.SocketError, e.SocketError.ToString());
                this.CloseClientSocket(e, reason);
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
             /// 发送数据通知函数
            if (null != SocketSended)
            {
                SocketSended(this, e);
            }

            if (e.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref this.totalBytesWrite, e.BytesTransferred);
            }
            else
            {
                //this.CloseClientSocket(e);
            }

            //什么事情都不做, 收回使用的e和buffer
            // Free the SocketAsyncEventArg so they can be reused by another client.
            e.SetBuffer(null, 0, 0); //回收内存
            TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;
            (e.UserToken as AsyncUserToken).CurrentSocket = null; //释放
            (e.UserToken as AsyncUserToken).Tag = null; //释放
            if (GameManager.FlagOptimizeThreadPool3)
            {
                if(null != s) s.PushWriteSocketAsyncEventArgs(e);
            }
            else
            {
                this.writePool.Push(e);
            }
        }

        /// <summary>
        /// Starts the server such that it is listening for incoming connection requests.    
        /// </summary>
        /// <param name="localEndPoint">The endpoint which the server will listening for connection requests on.</param>
        internal void Start(string ip, int port)
        {
            if ("" == ip) ip = "0.0.0.0"; //防止IP无效

            // Get endpoint for the listener.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Create the socket which listens for incoming connections.
            this.listenSocket = new TMSKSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Associate the socket with the local endpoint.
            this.listenSocket.Bind(localEndPoint);

            // Start the server with a listen backlog of 100 connections.
            this.listenSocket.Listen(100);

            // Post accepts on the listening socket.
            this.StartAccept(null);
        }
        
        /// <summary>
        /// 关闭侦听的TMSKSocket
        /// </summary>
        public void Stop()
        {
            TMSKSocket s = this.listenSocket;
            this.listenSocket = null;
            s.Close();
        }

        /// <summary>
        /// 关闭指定的TMSKSocket连接
        /// </summary>
        /// <param name="s"></param>
        /// <param name="reason">强制关闭的原因</param>
        public bool CloseSocket(TMSKSocket s, string reason = "")
        {
            //try
            //{               
            //    s.Shutdown(SocketShutdown.Both);

            //    // 需要Close 不然不会收到套接字关闭 ChenXiaojun
            //    s.Close();
            //}
            //catch (Exception ex)
            //{
            //    // Throws if client process has already closed.
            //    try
            //    {
            //        LogManager.WriteLog(LogTypes.Info, string.Format("CloseSocket 异常: {0}, {1}", s.RemoteEndPoint, ex.Message));
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}

            if (!FindSocket(s)) //已经关闭了
            {
                //已经关闭之后再调用一次
                Global._SendBufferManager.Remove(s);
                return false;
            }

            if (!string.IsNullOrEmpty(reason))
            {
                s.CloseReason = reason;
            }
            RemoveSocket(s);

            /// 断开成功通知函数
            if (null != SocketClosed)
            {
                SocketClosed(this, s);
            }

            //在这个位置调用，避免遗漏,重复调用不会出错
            Global._SendBufferManager.Remove(s);

            try
            {
                s.Shutdown(SocketShutdown.Receive);
            }
            catch (Exception ex)
            {
                // Throws if client process has already closed.
                try
                {
                    LogManager.WriteLog(LogTypes.Info, string.Format("CloseSocket s.Shutdown() error: {0}, {1}", s.RemoteEndPoint, ex.Message));
                }
                catch (Exception)
                {
                }
            }

            try
            {
                s.Close(30);
            }
            catch (Exception ex)
            {
                // Throws if client process has already closed.
                try
                {
                    LogManager.WriteLog(LogTypes.Info, string.Format("CloseSocket s.Close() error: {0}, {1}", s.RemoteEndPoint, ex.Message));
                }
                catch (Exception)
                {
                }
            }

            //由接收事件去释放处理
            return true;
        }

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                // TMSKSocket must be cleared since the context object is being reused.
                acceptEventArg.AcceptSocket = null;
            }

            //this.semaphoreAcceptedClients.WaitOne(); //控制总的连接数
            Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

        /// <summary>
        /// 初始化IP白名单列表
        /// </summary>
        /// <param name="ipList"></param>
        public List<string> InitIPWhiteList(string[] ipList, bool enabeld = true)
        {
            List<string> resultList = new List<string>();
            lock (IPWhiteList)
            {
                EnabledIPListFilter = false;
                IPWhiteList.Clear();
                if (null != ipList && ipList.Length > 0)
                {
                    foreach (var ipStr in ipList)
                    {
                        IPAddress ipAddress;
                        if (IPAddress.TryParse(ipStr, out ipAddress))
                        {
                            resultList.Add(ipAddress.ToString());
                            IPWhiteList[ipAddress.ToString()] = true;
                        }
                    }

                    if (IPWhiteList.Count > 0)
                    {
                        EnabledIPListFilter = enabeld;
                    }
                }

                return resultList;
            }
        }

        public void ClearTimeoutSocket()
        {
            bool ret = false;
            long nowTicks = TimeUtil.NOW();
            lock (this.ConnectedSocketsDict)
            {
                foreach (var socket in this.ConnectedSocketsDict.Keys)
                {
                    if (socket.session.SocketTime[1] == 0 && socket.Connected)
                    {
                        if (Math.Abs(nowTicks - socket.session.SocketTime[0]) > 30 * 1000)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(socket.CloseReason))
                                {
                                    socket.CloseReason = "ClearTimeoutSocket";
                                }

                                socket.Shutdown(SocketShutdown.Receive);
                            }
                            catch { }
                            try
                            {
                                socket.Close(30);
                            }
                            catch { }
                        }
                    }
                }
            }
        }
    }
}
