//using System.Windows.Forms;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
     * c. 接收部分，发送数据处理出错，则立刻执行关闭Socket操作。
     * d. 就是原则上，关闭操作交给接收去处理。
     */

    /// <summary>
    /// 服务器侦听类
    /// </summary>
    public sealed class SocketListener
    {
        /// <summary>
        /// 缓冲区的大小
        /// </summary>
        private Int32 ReceiveBufferSize;

        /// <summary>
        /// Represents a large reusable set of buffers for all socket operations.
        /// </summary>
        private BufferManager bufferManager;

        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        private Int32 numConnectedSockets;

        /// <summary>
        /// 已经连接的Socket词典对象
        /// </summary>
        private Dictionary<Socket, bool> ConnectedSocketsDict;

        /// <summary>
        /// 外部获取总的连接个数
        /// </summary>
        public Int32 ConnectedSocketsCount
        {
            get
            {
                Int32 n = 0;
                Interlocked.Exchange(ref n, this.numConnectedSockets);
                return n;
            }
        }

        /// <summary>
        /// the maximum number of connections the sample is designed to handle simultaneously.
        /// </summary>
        private Int32 numConnections;

        /// <summary>
        /// Read, write (don't alloc buffer space for accepts).
        /// </summary>
        private const Int32 opsToPreAlloc = 1;

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for read socket operations.
        /// </summary>
        private SocketAsyncEventArgsPool readPool; //线程安全

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for write socket operations.
        /// </summary>
        private SocketAsyncEventArgsPool writePool; //线程安全

        /// <summary>
        /// Controls the total number of clients connected to the server.
        /// </summary>
        private Semaphore semaphoreAcceptedClients;

        /// <summary>
        /// Total # bytes counter received by the server.
        /// </summary>
        private Int32 totalBytesRead;

        /// <summary>
        /// 获取总的接收的字节数
        /// </summary>
        public Int32 TotalBytesReadSize
        {
            get
            {
                Int32 n = 0;
                Interlocked.Exchange(ref n, this.totalBytesRead);
                return n;
            }
        }

        /// <summary>
        /// Total # bytes counter received by the server.
        /// </summary>
        private Int32 totalBytesWrite;

        /// <summary>
        /// 获取总的发送的字节数
        /// </summary>
        public Int32 TotalBytesWriteSize
        {
            get
            {
                Int32 n = 0;
                Interlocked.Exchange(ref n, this.totalBytesWrite);
                return n;
            }
        }

        /// 连接成功通知函数
        public event SocketConnectedEventHandler SocketConnected = null;

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
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.ReceiveBufferSize = receiveBufferSize;

            // Allocate buffers such that the maximum number of sockets can have one outstanding read and
            // write posted to the socket simultaneously .
            this.bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            // 已经连接的Socket词典对象
            this.ConnectedSocketsDict = new Dictionary<Socket, bool>(numConnections);

            this.readPool = new SocketAsyncEventArgsPool(numConnections);
            this.writePool = new SocketAsyncEventArgsPool(numConnections * 5);
            this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// 添加socket对象
        /// </summary>
        /// <param name="socket"></param>
        private void AddSocket(Socket socket)
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
        private void RemoveSocket(Socket socket)
        {
            lock (this.ConnectedSocketsDict)
            {
                this.ConnectedSocketsDict.Remove(socket);
            }
        }

        /// <summary>
        /// 查找Socket对象
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool FindSocket(Socket socket)
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
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken aut = e.UserToken as AsyncUserToken;

            try
            {
                Socket s = aut.CurrentSocket;
                if (!FindSocket(s)) //已经关闭了
                {
                    return;
                }

                RemoveSocket(s);

                try
                {
                    LogManager.WriteLog(LogTypes.Info, string.Format("Close socket: {0}, last operation: {1}, error: {2}", s.RemoteEndPoint, e.LastOperation, e.SocketError));
                }
                catch (Exception)
                {
                }

                // Decrement the counter keeping track of the total number of clients connected to the server.
                this.semaphoreAcceptedClients.Release(); //有时会超过最大数
                Interlocked.Decrement(ref this.numConnectedSockets);

                /// 断开成功通知函数
                if (null != SocketClosed)
                {
                    SocketClosed(this, e);
                }

                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // Throws if client process has already closed.
                }

                try
                {
                    s.Close();
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                aut.CurrentSocket = null; //释放
                aut.Tag = null; //释放

                // Free the SocketAsyncEventArg so they can be reused by another client.
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    e.SetBuffer(null, 0, 0); //回收内存
                    this.writePool.Push(e);
                }
                else if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    this.readPool.Push(e);
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

            for (Int32 i = 0; i < this.numConnections; i++)
            {
                // Preallocate a set of reusable SocketAsyncEventArgs.
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                readWriteEventArg.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };

                // Assign a Byte buffer from the buffer pool to the SocketAsyncEventArg object.
                this.bufferManager.SetBuffer(readWriteEventArg);

                // Add SocketAsyncEventArg to the pool.
                this.readPool.Push(readWriteEventArg);

                for (Int32 j = 0; j < 5; j++)
                {
                    readWriteEventArg = new SocketAsyncEventArgs();
                    readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                    readWriteEventArg.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };
                    this.writePool.Push(readWriteEventArg);
                }
            }
        }

        /// <summary>
        /// Callback method associated with Socket.AcceptAsync
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
                LogManager.WriteLog(LogTypes.Error, string.Format("SocketListener::_ReceiveAsync got exception {0}", ex.ToString()));
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //});
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
                LogManager.WriteLog(LogTypes.Error, string.Format("SocketListener::_ReceiveAsync got exception {0}", ex.ToString()));
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //});
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
                Socket s = (readEventArgs.UserToken as AsyncUserToken).CurrentSocket;
                return s.ReceiveAsync(readEventArgs);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("SocketListener::_ReceiveAsync got exception {0}", ex.ToString()));
                this.CloseClientSocket(readEventArgs);
                return true;
            }
        }

        /// <summary>
        /// 向Socket发送数据
        /// </summary>
        /// <param name="writeEventArgs"></param>
        /// <returns></returns>
        private bool _SendAsync(SocketAsyncEventArgs writeEventArgs, out bool exception)
        {
            exception = false;

            try
            {
                Socket s = (writeEventArgs.UserToken as AsyncUserToken).CurrentSocket;
                return s.SendAsync(writeEventArgs);
            }
            catch (Exception ex) //此处有可能是对象非法等异常, 例如Socket对象已经无效
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("SocketListener::_ReceiveAsync got exception {0}", ex.ToString()));
                exception = true;
                //this.CloseClientSocket(writeEventArgs);
                return true;
            }
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        internal bool SendData(Socket s, TCPOutPacket tcpOutPacket)
        {
            SocketAsyncEventArgs writeEventArgs = this.writePool.Pop(); //线程安全的操作
            if (null == writeEventArgs)
            {
                writeEventArgs = new SocketAsyncEventArgs();
                writeEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                writeEventArgs.UserToken = new AsyncUserToken() { CurrentSocket = null, Tag = null };
            }

            //字节排序
            //DataHelper.SortBytes(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);

            writeEventArgs.SetBuffer(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);
            (writeEventArgs.UserToken as AsyncUserToken).CurrentSocket = s;
            (writeEventArgs.UserToken as AsyncUserToken).Tag = (object)tcpOutPacket;

            bool exception = false;
            Boolean willRaiseEvent = _SendAsync(writeEventArgs, out exception);
            if (!willRaiseEvent)
            {
                this.ProcessSend(writeEventArgs);
            }

            return (!exception);
        }

        /// <summary>
        /// Process the accept for the socket listener.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            SocketAsyncEventArgs readEventArgs = null;

            //增加计数器
            Interlocked.Increment(ref this.numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the
            // ReadEventArg object user token.
            Socket s = e.AcceptSocket;
            readEventArgs = this.readPool.Pop();
            if (null == readEventArgs)
            {
                try
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("New connection: {0}, force close coz connection pool is full, count: {1}", s.RemoteEndPoint, ConnectedSocketsCount));
                }
                catch (Exception)
                {
                }

                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // Throws if client process has already closed.
                }

                try
                {
                    s.Close();
                }
                catch (Exception)
                {
                }

                //减少计数器
                Interlocked.Decrement(ref this.numConnectedSockets);
                this.StartAccept(e);
                return;
            }

            (readEventArgs.UserToken as AsyncUserToken).CurrentSocket = e.AcceptSocket;

            byte[] inOptionValues = new byte[sizeof(uint) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)120000).CopyTo(inOptionValues, sizeof(uint));
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, sizeof(uint) * 2);
            s.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

            AddSocket(s);

            try
            {
                LogManager.WriteLog(LogTypes.Info, string.Format("New socket connection: {0}, total count: {1}", s.RemoteEndPoint, numConnectedSockets));
            }
            catch (Exception)
            {
            }

            if (null != SocketConnected)
            {
                SocketConnected(this, readEventArgs);
            }

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
                    //DataHelper.SortBytes(e.Buffer, e.Offset, e.BytesTransferred);
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
                    this.CloseClientSocket(e);
                }
            }
            else
            {
                this.CloseClientSocket(e);
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
            (e.UserToken as AsyncUserToken).CurrentSocket = null; //释放
            (e.UserToken as AsyncUserToken).Tag = null; //释放
            this.writePool.Push(e);
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
            this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Associate the socket with the local endpoint.
            this.listenSocket.Bind(localEndPoint);

            // Start the server with a listen backlog of 100 connections.
            this.listenSocket.Listen(100);

            // Post accepts on the listening socket.
            this.StartAccept(null);
        }

        /// <summary>
        /// 关闭侦听的Socket
        /// </summary>
        public void Stop()
        {
            Socket s = this.listenSocket;
            this.listenSocket = null;
            s.Close();
        }

        /// <summary>
        /// 关闭指定的Socket连接
        /// </summary>
        /// <param name="s"></param>
        private void CloseSocket(Socket s)
        {
            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // Throws if client process has already closed.
            }

            //由接收事件去释放处理
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
                // Socket must be cleared since the context object is being reused.
                acceptEventArg.AcceptSocket = null;
            }

            this.semaphoreAcceptedClients.WaitOne(); //控制总的连接数
            Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                this.ProcessAccept(acceptEventArg);
            }
        }
    }
}