using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime;
using System.Net.Sockets;
using Server.Protocol;
using Server.Tools;
using GameServer.Logic;
using GameServer.Server;
using Tmsk.Contract;

namespace Server.TCP
{
    /// <summary>
    /// Thông tin Socket của người chơi
    /// </summary>
    public class TMSKSocket : IDisposable
    {
        public TCPInPacket _TcpInPacket { get; set; }
        public SendBuffer _SendBuffer { get; set; }
        public TCPSession session { get; set; }


        public ulong SortKey64 { get; set; } = 0;

        /// <summary>
        /// Có phải đăng nhập vào liên máy chủ không
        /// </summary>
        public bool IsKuaFuLogin { get; set; } = false;

        /// <summary>
        /// ID máy chủ
        /// </summary>
        public int ServerId { get; set; } = 0;

        /// <summary>
        /// ID nhân vật
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// ID thiết bị
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// Dữ liệu đăng nhập liên máy chủ
        /// </summary>
        public KuaFuServerLoginData ClientKuaFuServerLoginData { get; set; } = new KuaFuServerLoginData();

        /// <summary>
        /// Đã bị hủy chưa
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Lý do bị hủy
        /// </summary>
        public string CloseReason { get; set; } = "";

        ~TMSKSocket()
        {
            MyDispose();
        }

        public void MyDispose()
        {
            try
            {
                if (null != GlobalWritePool)
                {
                    lock (SocketAsyncEventArgsWritePool)
                    {
                        IsDisposed = true;
                        while (SocketAsyncEventArgsWritePool.Count > 0)
                        {
                            GlobalWritePool.Push(SocketAsyncEventArgsWritePool.Pop());
                        }
                    }
                }
                if (null != GlobalReadPool)
                {
                    lock (SocketAsyncEventArgsReadPool)
                    {
                        IsDisposed = true;
                        while (SocketAsyncEventArgsReadPool.Count > 0)
                        {
                            GlobalReadPool.Push(SocketAsyncEventArgsReadPool.Pop());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "~TMSKSocket()异常:");
            }
        }

        #region 异步对象收发池

        Stack<SocketAsyncEventArgs> SocketAsyncEventArgsReadPool = new Stack<SocketAsyncEventArgs>();
        int maxStackSocketAsyncEventArgsRead = 3;

        Stack<SocketAsyncEventArgs> SocketAsyncEventArgsWritePool = new Stack<SocketAsyncEventArgs>();
        int maxStackSocketAsyncEventArgsWrite = 3;

        public SocketAsyncEventArgsPool GlobalWritePool;
        public SocketAsyncEventArgsPool GlobalReadPool;

        /// <summary>
        /// 将内存块还回缓存队列
        /// </summary>
        /// <param name="item"></param>
        public void PushWriteSocketAsyncEventArgs(SocketAsyncEventArgs item)
        {
            try
            {
                lock(SocketAsyncEventArgsWritePool)
                {
                    if (!IsDisposed && SocketAsyncEventArgsWritePool.Count <= maxStackSocketAsyncEventArgsWrite)
                    {
                        SocketAsyncEventArgsWritePool.Push(item);
                        return;
                    }
                }
                if (null == GlobalWritePool)
                {
                    GlobalWritePool = Global._TCPManager.MySocketListener.writePool;
                }
                GlobalWritePool.Push(item);
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
            }
        }

        /// <summary>
        /// 从缓存队列中提取所需内存对象
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        public SocketAsyncEventArgs PopWriteSocketAsyncEventArgs()
        {
            try
            {
                lock(SocketAsyncEventArgsWritePool)
                {
                    if (SocketAsyncEventArgsWritePool.Count > 0)
                    {
                        return SocketAsyncEventArgsWritePool.Pop();
                    }
                }
                if (null == GlobalWritePool)
                {
                    GlobalWritePool = Global._TCPManager.MySocketListener.writePool;
                }
                return GlobalWritePool.Pop();
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
            }
            return null;
        }

        /// <summary>
        /// 将内存块还回缓存队列
        /// </summary>
        /// <param name="item"></param>
        public void PushReadSocketAsyncEventArgs(SocketAsyncEventArgs item)
        {
            try
            {
                lock(SocketAsyncEventArgsReadPool)
                {
                    if (!IsDisposed && SocketAsyncEventArgsReadPool.Count <= maxStackSocketAsyncEventArgsRead)
                    {
                        SocketAsyncEventArgsReadPool.Push(item);
                        return;
                    }
                }
                if (null == GlobalReadPool)
                {
                    GlobalReadPool = Global._TCPManager.MySocketListener.readPool;
                }
                GlobalReadPool.Push(item);
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
            }
        }

        /// <summary>
        /// 从缓存队列中提取所需内存对象
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        public SocketAsyncEventArgs PopReadSocketAsyncEventArgs()
        {
            try
            {
                lock(SocketAsyncEventArgsReadPool)
                {
                    if (SocketAsyncEventArgsReadPool.Count > 0)
                    {
                        return SocketAsyncEventArgsReadPool.Pop();
                    }
                }
                if (null == GlobalReadPool)
                {
                    GlobalReadPool = Global._TCPManager.MySocketListener.readPool;
                }
                return GlobalReadPool.Pop();
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
            }
            return null;
        }

        public long SendCount;


        #endregion 异步对象收发池

        public Socket m_Socket = null;

        #region 包装方法

        // 摘要:
        //     使用 System.Net.Sockets.TMSKSocket.DuplicateAndClose(System.Int32) 返回的指定的值初始化 System.Net.Sockets.TMSKSocket
        //     类的新实例。
        //
        // 参数:
        //   socketInformation:
        //     System.Net.Sockets.TMSKSocket.DuplicateAndClose(System.Int32) 返回的套接字信息。
        public TMSKSocket(Socket socket)
        {
            m_Socket = socket;
        }

        // 摘要:
        //     使用 System.Net.Sockets.TMSKSocket.DuplicateAndClose(System.Int32) 返回的指定的值初始化 System.Net.Sockets.TMSKSocket
        //     类的新实例。
        //
        // 参数:
        //   socketInformation:
        //     System.Net.Sockets.TMSKSocket.DuplicateAndClose(System.Int32) 返回的套接字信息。
        public TMSKSocket(SocketInformation socketInformation)
        {
            m_Socket = new Socket(socketInformation);
        }
        //
        // 摘要:
        //     使用指定的地址族、套接字类型和协议初始化 System.Net.Sockets.TMSKSocket 类的新实例。
        //
        // 参数:
        //   addressFamily:
        //     System.Net.Sockets.AddressFamily 值之一。
        //
        //   socketType:
        //     System.Net.Sockets.SocketType 值之一。
        //
        //   protocolType:
        //     System.Net.Sockets.ProtocolType 值之一。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     addressFamily、socketType 和 protocolType 的组合会导致无效套接字。
        public TMSKSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)           
        {
            m_Socket = new Socket(addressFamily, socketType, protocolType);
        }

        // 摘要:
        //     获取 System.Net.Sockets.Socket 的地址族。
        //
        // 返回结果:
        //     System.Net.Sockets.AddressFamily 值之一。
        public AddressFamily AddressFamily {
            get
            {
                return m_Socket.AddressFamily;
            } 
        }
        //
        // 摘要:
        //     获取已经从网络接收且可供读取的数据量。
        //
        // 返回结果:
        //     从网络接收的、可供读取的数据的字节数。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Available 
        {
            get
            {
                return m_Socket.Available;
            }
        }

        //
        // 摘要:
        //     获取或设置一个值，该值指示 System.Net.Sockets.Socket 是否处于阻止模式。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 将阻止，则为 true；否则为 false。默认值为 true。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool Blocking 
        {
            get
            {
                return m_Socket.Blocking;
            }

            set
            {
                m_Socket.Blocking = value;
            }
        }
        //
        // 摘要:
        //     获取一个值，该值指示 System.Net.Sockets.Socket 是在上次 Overload:System.Net.Sockets.Socket.Send
        //     还是 Overload:System.Net.Sockets.Socket.Receive 操作时连接到远程主机。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 在最近操作时连接到远程资源，则为 true；否则为 false。
        public bool Connected 
        { 
            get
            {
                return m_Socket.Connected;
            } 
        }
        //
        // 摘要:
        //     获取或设置 System.Boolean 值，该值指定 System.Net.Sockets.Socket 是否允许将 Internet 协议 (IP)
        //     数据报分段。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 允许数据报分段，则为 true；否则为 false。默认值为 true。
        //
        // 异常:
        //   System.NotSupportedException:
        //     只有对于在 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     族中的套接字，才可以设置此属性。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool DontFragment 
        {
            get
            {
                return m_Socket.DontFragment;
            }

            set
            {
                m_Socket.DontFragment = value;
            }
        }
        //
        // 摘要:
        //     获取或设置一个 System.Boolean 值，该值指定 System.Net.Sockets.Socket 是否可以发送或接收广播数据包。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 允许广播数据包，则为 true；否则为 false。默认值为 false。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     此选项仅对数据报套接字有效。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool EnableBroadcast 
        {
            get
            {
                return m_Socket.EnableBroadcast;
            }

            set
            {
                m_Socket.EnableBroadcast = value;
            }
        }
        //
        // 摘要:
        //     获取或设置 System.Boolean 值，该值指定 System.Net.Sockets.Socket 是否仅允许一个进程绑定到端口。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 仅允许一个套接字绑定到特定端口，则为 true；否则为 false。对于 Windows
        //     Server 2003 和 Windows XP Service Pack 2，默认值为 true，对于其他所有版本，默认值为 false。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.InvalidOperationException:
        //     已为此 System.Net.Sockets.Socket 调用了 System.Net.Sockets.Socket.Bind(System.Net.EndPoint)。
        public bool ExclusiveAddressUse
        {
            get
            {
                return m_Socket.ExclusiveAddressUse;
            }

            set
            {
                m_Socket.ExclusiveAddressUse = value;
            }
        }
        //
        // 摘要:
        //     获取 System.Net.Sockets.Socket 的操作系统句柄。
        //
        // 返回结果:
        //     一个 System.IntPtr，它表示 System.Net.Sockets.Socket 的操作系统句柄。
        public IntPtr Handle 
        {
            get
            {
                return m_Socket.Handle;
            }
        }

        //
        // 摘要:
        //     获取一个值，该值指示 System.Net.Sockets.Socket 是否绑定到特定本地端口。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 绑定到本地端口，则为 true；否则为 false。
        public bool IsBound 
        {
            get
            {
                return m_Socket.IsBound;
            }
        }
        //
        // 摘要:
        //     获取或设置一个值，该值指定 System.Net.Sockets.Socket 在尝试发送所有挂起数据时是否延迟关闭套接字。
        //
        // 返回结果:
        //     一个 System.Net.Sockets.LingerOption，它指定关闭套接字时如何逗留。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public LingerOption LingerState
        { 
            get
            {
                return m_Socket.LingerState;
            }

            set
            {
                m_Socket.LingerState = value;
            }
        }

        //
        // 摘要:
        //     获取本地终结点。
        //
        // 返回结果:
        //     System.Net.Sockets.Socket 当前用以进行通信的 System.Net.EndPoint。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public EndPoint LocalEndPoint
        {
            get
            {
                return m_Socket.LocalEndPoint;
            }
        }
        //
        // 摘要:
        //     获取或设置一个值，该值指定传出的多路广播数据包是否传递到发送应用程序。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 接收传出的多路广播数据包，则为 true；否则为 false。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool MulticastLoopback
        {
            get
            {
                return m_Socket.MulticastLoopback;
            }

            set
            {
                m_Socket.MulticastLoopback = value;
            }
        }

        //
        // 摘要:
        //     获取或设置 System.Boolean 值，该值指定流 System.Net.Sockets.Socket 是否正在使用 Nagle 算法。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 使用 Nagle 算法，则为 false；否则为 true。默认值为 false。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问 System.Net.Sockets.Socket 时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool NoDelay 
        { 
            get
            {
                return m_Socket.NoDelay;
            }

            set
            {
                m_Socket.NoDelay = value;
            }
        }

        //
        // 摘要:
        //     指示基础操作系统和网络适配器是否支持 Internet 协议第 4 版 (IPv4)。
        //
        // 返回结果:
        //     如果操作系统和网络适配器支持 IPv4 协议，则为 true；否则为 false。
        public static bool OSSupportsIPv4
        {
            get
            {
                return Socket.OSSupportsIPv4;
            }
        }

        //
        // 摘要:
        //     指示基础操作系统和网络适配器是否支持 Internet 协议第 6 版 (IPv6)。
        //
        // 返回结果:
        //     如果操作系统和网络适配器支持 IPv6 协议，则为 true；否则为 false。
        public static bool OSSupportsIPv6
        {
            get
            {
                return Socket.OSSupportsIPv6;
            }
        }

        //
        // 摘要:
        //     获取 System.Net.Sockets.Socket 的协议类型。
        //
        // 返回结果:
        //     System.Net.Sockets.ProtocolType 值之一。
        public ProtocolType ProtocolType 
        {
            get
            {
                return m_Socket.ProtocolType;
            }
        }

        //
        // 摘要:
        //     获取或设置一个值，它指定 System.Net.Sockets.Socket 接收缓冲区的大小。
        //
        // 返回结果:
        //     System.Int32，它包含接收缓冲区的大小（以字节为单位）。默认值为 8192。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     为设置操作指定的值小于 0。
        public int ReceiveBufferSize
        {
            get
            {
                return m_Socket.ReceiveBufferSize;
            }

            set
            {
                m_Socket.ReceiveBufferSize = value;
            }
        }

        //
        // 摘要:
        //     获取或设置一个值，该值指定之后同步 Overload:System.Net.Sockets.Socket.Receive 调用将超时的时间长度。
        //
        // 返回结果:
        //     超时值（以毫秒为单位）。默认值为 0，指示超时期限无限大。指定 -1 还会指示超时期限无限大。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     为设置操作指定的值小于 -1。
        public int ReceiveTimeout 
        {
            get
            {
                return m_Socket.ReceiveTimeout;
            }

            set
            {
                m_Socket.ReceiveTimeout = value;
            }
        }

        //
        // 摘要:
        //     获取远程终结点。
        //
        // 返回结果:
        //     当前和 System.Net.Sockets.Socket 通信的 System.Net.EndPoint。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public EndPoint RemoteEndPoint 
        {
            get
            {
                return m_Socket.RemoteEndPoint;
            }
        }

        //
        // 摘要:
        //     获取或设置一个值，该值指定 System.Net.Sockets.Socket 发送缓冲区的大小。
        //
        // 返回结果:
        //     System.Int32，它包含发送缓冲区的大小（以字节为单位）。默认值为 8192。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     为设置操作指定的值小于 0。
        public int SendBufferSize 
        {
            get
            {
                return m_Socket.SendBufferSize;
            }

            set
            {
                m_Socket.SendBufferSize = value;
            }
        }

        //
        // 摘要:
        //     获取或设置一个值，该值指定之后同步 Overload:System.Net.Sockets.Socket.Send 调用将超时的时间长度。
        //
        // 返回结果:
        //     超时值（以毫秒为单位）。如果将该属性设置为 1 到 499 之间的值，该值将被更改为 500。默认值为 0，指示超时期限无限大。指定 -1 还会指示超时期限无限大。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     为设置操作指定的值小于 -1。
        public int SendTimeout
        {
            get
            {
                return m_Socket.SendTimeout;
            }

            set
            {
                m_Socket.SendTimeout = value;
            }
        }

        //
        // 摘要:
        //     获取 System.Net.Sockets.Socket 的类型。
        //
        // 返回结果:
        //     System.Net.Sockets.SocketType 值之一。
        public SocketType SocketType 
        {
            get
            {
                return m_Socket.SocketType;
            }
        }

        //
        // 摘要:
        //     获取一个值，该值指示在当前主机上 IPv4 支持是否可用并且已启用。
        //
        // 返回结果:
        //     如果当前主机支持 IPv4 协议，则为 true；否则为 false。
        [Obsolete("SupportsIPv4 is obsoleted for this type, please use OSSupportsIPv4 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static bool SupportsIPv4 
        {
            get
            {
                return Socket.SupportsIPv4;
            }
        }
        
        //
        // 摘要:
        //     获取一个值，该值指示 Framework 对某些已过时的 System.Net.Dns 成员是否支持 IPv6。
        //
        // 返回结果:
        //     如果 Framework 对某些已过时的 System.Net.Dns 方法支持 IPv6，则为 true；否则为 false。
        [Obsolete("SupportsIPv6 is obsoleted for this type, please use OSSupportsIPv6 instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static bool SupportsIPv6
        {
            get
            {
                return Socket.SupportsIPv6;
            }
        }
        
        //
        // 摘要:
        //     获取或设置一个值，指定 System.Net.Sockets.Socket 发送的 Internet 协议 (IP) 数据包的生存时间 (TTL)
        //     值。
        //
        // 返回结果:
        //     TTL 值。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     TTL 值不能设置为负数。
        //
        //   System.NotSupportedException:
        //     只有对于在 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     族中的套接字，才可以设置此属性。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。在尝试将 TTL 设置为大于 255 的值时，也将返回此错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public short Ttl 
        {
            get
            {
                return m_Socket.Ttl;
            }

            set
            {
                m_Socket.Ttl = value;
            }
        }

        //
        // 摘要:
        //     指定套接字是否应仅使用重叠 I/O 模式。
        //
        // 返回结果:
        //     如果 System.Net.Sockets.Socket 仅使用重叠 I/O，则为 true；否则为 false。默认值为 false。
        //
        // 异常:
        //   System.InvalidOperationException:
        //     套接字已绑定到完成端口。
        public bool UseOnlyOverlappedIO
        {
            get
            {
                return m_Socket.UseOnlyOverlappedIO;
            }

            set
            {
                m_Socket.UseOnlyOverlappedIO = value;
            }
        }

        // 摘要:
        //     为新建连接创建新的 System.Net.Sockets.Socket。
        //
        // 返回结果:
        //     新建连接的 System.Net.Sockets.Socket。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.InvalidOperationException:
        //     正在接受的套接字不会侦听连接。在调用 System.Net.Sockets.Socket.Accept() 之前必须调用 System.Net.Sockets.Socket.Bind(System.Net.EndPoint)
        //     和 System.Net.Sockets.Socket.Listen(System.Int32)。
        public Socket Accept()
        {
            return m_Socket.Accept();
        }

        //
        // 摘要:
        //     开始一个异步操作来接受一个传入的连接尝试。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentException:
        //     参数无效。如果所提供的缓冲区不够大，将会发生此异常。缓冲区必须至少为 2 * (sizeof(SOCKADDR_STORAGE + 16) 字节。如果指定了多个缓冲区，即
        //     System.Net.Sockets.SocketAsyncEventArgs.BufferList 属性不为 null，也会发生此异常。
        //
        //   System.ArgumentOutOfRangeException:
        //     参数超出范围。如果 System.Net.Sockets.SocketAsyncEventArgs.Count 小于 0，将会发生此异常。
        //
        //   System.InvalidOperationException:
        //     请求了无效操作。如果接收方 System.Net.Sockets.Socket 未侦听连接或者绑定了接受的套接字，将发生此异常。System.Net.Sockets.Socket.Bind(System.Net.EndPoint)
        //     和 System.Net.Sockets.Socket.Listen(System.Int32) 方法必须先于 System.Net.Sockets.Socket.AcceptAsync(System.Net.Sockets.SocketAsyncEventArgs)
        //     方法调用。如果套接字已连接或使用指定的 e 参数的套接字操作已经在进行中，也会发生此异常。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool AcceptAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.AcceptAsync(e);
        }

        //
        // 摘要:
        //     开始一个异步操作来接受一个传入的连接尝试。
        //
        // 参数:
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个 System.IAsyncResult，它引用异步 System.Net.Sockets.Socket 创建。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.InvalidOperationException:
        //     正在接受的套接字不会侦听连接。在调用 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     之前必须调用 System.Net.Sockets.Socket.Bind(System.Net.EndPoint) 和 System.Net.Sockets.Socket.Listen(System.Int32)。-
        //     或 -已接受的套接字是绑定的。
        //
        //   System.ArgumentOutOfRangeException:
        //     receiveSize 小于 0。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return m_Socket.BeginAccept(callback, state);
        }

        //
        // 摘要:
        //     开始异步操作以接受传入的连接尝试并接收客户端应用程序发送的第一个数据块。
        //
        // 参数:
        //   receiveSize:
        //     要从发送方读取的字节数。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个 System.IAsyncResult，它引用异步 System.Net.Sockets.Socket 创建。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.InvalidOperationException:
        //     正在接受的套接字不会侦听连接。在调用 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     之前必须调用 System.Net.Sockets.Socket.Bind(System.Net.EndPoint) 和 System.Net.Sockets.Socket.Listen(System.Int32)。-
        //     或 -已接受的套接字是绑定的。
        //
        //   System.ArgumentOutOfRangeException:
        //     receiveSize 小于 0。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IAsyncResult BeginAccept(int receiveSize, AsyncCallback callback, object state)
        {
            return m_Socket.BeginAccept(receiveSize, callback, state);
        }

        //
        // 摘要:
        //     开始异步操作以接受从指定套接字传入的连接尝试并接收客户端应用程序发送的第一个数据块。
        //
        // 参数:
        //   acceptSocket:
        //     接受的 System.Net.Sockets.Socket 对象。此值可以是 null。
        //
        //   receiveSize:
        //     要接收的最大字节数。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个 System.IAsyncResult 对象，它引用异步 System.Net.Sockets.Socket 对象创建。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.InvalidOperationException:
        //     正在接受的套接字不会侦听连接。在调用 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     之前必须调用 System.Net.Sockets.Socket.Bind(System.Net.EndPoint) 和 System.Net.Sockets.Socket.Listen(System.Int32)。-
        //     或 -已接受的套接字是绑定的。
        //
        //   System.ArgumentOutOfRangeException:
        //     receiveSize 小于 0。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public IAsyncResult BeginAccept(Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
        {
            return m_Socket.BeginAccept(acceptSocket, receiveSize, callback, state);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。
        //
        // 参数:
        //   remoteEP:
        //     System.Net.EndPoint，它表示远程主机。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     System.IAsyncResult，它引用异步连接。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_Socket.BeginConnect(remoteEP, callback, state);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。主机由 System.Net.IPAddress 和端口号指定。
        //
        // 参数:
        //   address:
        //     远程主机的 System.Net.IPAddress。
        //
        //   port:
        //     远程主机的端口号。
        //
        //   requestCallback:
        //     一个 System.AsyncCallback 委托，它引用连接操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含连接操作的相关信息。操作完成时，此对象传递给了 requestCallback 委托。
        //
        // 返回结果:
        //     System.IAsyncResult，它引用异步连接。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     address 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     System.Net.Sockets.Socket 不在套接字族中。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.ArgumentException:
        //     address 的长度为零。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return m_Socket.BeginConnect(address, port, requestCallback, state);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。主机由 System.Net.IPAddress 数组和端口号指定。
        //
        // 参数:
        //   addresses:
        //     至少一个 System.Net.IPAddress，指定远程主机。
        //
        //   port:
        //     远程主机的端口号。
        //
        //   requestCallback:
        //     一个 System.AsyncCallback 委托，它引用连接操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含连接操作的相关信息。操作完成时，此对象传递给了 requestCallback 委托。
        //
        // 返回结果:
        //     System.IAsyncResult，它引用异步连接。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     addresses 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     此方法对 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     的套接字有效。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.ArgumentException:
        //     address 的长度为零。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return m_Socket.BeginConnect(addresses, port, requestCallback, state);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。主机由主机名和端口号指定。
        //
        // 参数:
        //   host:
        //     远程主机的名称。
        //
        //   port:
        //     远程主机的端口号。
        //
        //   requestCallback:
        //     一个 System.AsyncCallback 委托，它引用连接操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含连接操作的相关信息。操作完成时，此对象传递给了 requestCallback 委托。
        //
        // 返回结果:
        //     System.IAsyncResult，它引用异步连接。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     host 为 null。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     此方法对 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     系列中的套接字有效。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            return m_Socket.BeginConnect(host, port, requestCallback, state);
        }

        //
        // 摘要:
        //     开始异步请求从远程终结点断开连接。
        //
        // 参数:
        //   reuseSocket:
        //     如果关闭该连接后可以重用此套接字，则为 true；否则为 false。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个 System.IAsyncResult 对象，它引用异步操作。
        //
        // 异常:
        //   System.NotSupportedException:
        //     操作系统为 Windows 2000 或更低版本，而此方法需要在 Windows XP 中使用。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public IAsyncResult BeginDisconnect(bool reuseSocket, AsyncCallback callback, object state)
        {
            return m_Socket.BeginDisconnect(reuseSocket, callback, state);
        }

        //
        // 摘要:
        //     开始从连接的 System.Net.Sockets.Socket 中异步接收数据。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   callback:
        //     一个 System.AsyncCallback 委托，它引用操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含接收操作的相关信息。操作完成时，此对象传递给了 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     委托。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceive(buffers, socketFlags, callback, state);
        }

        //
        // 摘要:
        //     开始从连接的 System.Net.Sockets.Socket 中异步接收数据。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        //   callback:
        //     一个 System.AsyncCallback 委托，它引用操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含接收操作的相关信息。操作完成时，此对象传递给了 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     委托。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginReceive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceive(buffers, socketFlags, out errorCode, callback, state);
        }

        //
        // 摘要:
        //     开始从连接的 System.Net.Sockets.Socket 中异步接收数据。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储所接收数据的位置，该位置从零开始计数。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   callback:
        //     一个 System.AsyncCallback 委托，它引用操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含接收操作的相关信息。操作完成时，此对象传递给了 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     委托。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceive(buffer, offset, size, socketFlags, callback, state);
        }

        //
        // 摘要:
        //     开始从连接的 System.Net.Sockets.Socket 中异步接收数据。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 中存储所接收数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        //   callback:
        //     一个 System.AsyncCallback 委托，它引用操作完成时要调用的方法。
        //
        //   state:
        //     一个用户定义对象，其中包含接收操作的相关信息。操作完成时，此对象传递给了 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     委托。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceive(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }

        //
        // 摘要:
        //     开始从指定网络设备中异步接收数据。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储数据的位置，该位置从零开始计数。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     一个 System.Net.EndPoint，它表示数据的来源。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }

        //
        // 摘要:
        //     开始使用指定的 System.Net.Sockets.SocketFlags 将指定字节数的数据异步接收到数据缓冲区的指定位置，然后存储终结点和数据包信息。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储数据的位置，该位置从零开始计数。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     一个 System.Net.EndPoint，它表示数据的来源。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个引用异步读取的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     操作系统为 Windows 2000 或更低版本，而此方法需要在 Windows XP 中使用。
        public IAsyncResult BeginReceiveMessageFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_Socket.BeginReceiveMessageFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }

        //
        // 摘要:
        //     将数据异步发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     引用异步发送的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。
        //
        //   System.ArgumentException:
        //     buffers 为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSend(buffers, socketFlags, callback, state);
        }

        //
        // 摘要:
        //     将数据异步发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     引用异步发送的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。
        //
        //   System.ArgumentException:
        //     buffers 为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginSend(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSend(buffers, socketFlags, out errorCode, callback, state);
        }

        //
        // 摘要:
        //     将数据异步发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     buffer 参数中开始发送数据的位置，该位置从零开始计数。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     引用异步发送的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 小于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSend(buffer, offset, size, socketFlags, callback, state);
        }

        //
        // 摘要:
        //     将数据异步发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     buffer 参数中开始发送数据的位置，该位置从零开始计数。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     引用异步发送的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 小于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public IAsyncResult BeginSend(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSend(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }

        //
        // 摘要:
        //     使用 System.Net.Sockets.TransmitFileOptions.UseDefaultWorkerThread 标志，将文件 fileName
        //     发送到连接的 System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   fileName:
        //     一个字符串，它包含要发送的文件的路径和名称。此参数可以为 null。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     一个 System.IAsyncResult 对象，它表示异步发送。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.NotSupportedException:
        //     套接字未连接到远程主机。
        //
        //   System.IO.FileNotFoundException:
        //     未找到文件 fileName。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        public IAsyncResult BeginSendFile(string fileName, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSendFile(fileName, callback, state);
        }

        //
        // 摘要:
        //     将文件和数据缓冲区异步发送到连接的 System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   fileName:
        //     一个字符串，它包含要发送的文件的路径和名称。此参数可以为 null。
        //
        //   preBuffer:
        //     一个 System.Byte 数组，包含发送文件前要发送的数据。此参数可以为 null。
        //
        //   postBuffer:
        //     一个 System.Byte 数组，包含发送文件后要发送的数据。此参数可以为 null。
        //
        //   flags:
        //     System.Net.Sockets.TransmitFileOptions 值的按位组合。
        //
        //   callback:
        //     一个 System.AsyncCallback 委托，将在此操作完成时调用它。此参数可以为 null。
        //
        //   state:
        //     一个用户定义的对象，它包含此请求的状态信息。此参数可以为 null。
        //
        // 返回结果:
        //     一个 System.IAsyncResult 对象，它表示异步操作。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.NotSupportedException:
        //     操作系统不是 Windows NT 或更高版本。- 或 -套接字未连接到远程主机。
        //
        //   System.IO.FileNotFoundException:
        //     未找到文件 fileName。
        public IAsyncResult BeginSendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSendFile(fileName, preBuffer, postBuffer, flags, callback, state);
        }

        //
        // 摘要:
        //     向特定远程主机异步发送数据。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     buffer 中的从其开始发送数据的、从零开始编排的位置。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     System.Net.EndPoint，表示远程设备。
        //
        //   callback:
        //     System.AsyncCallback 委托。
        //
        //   state:
        //     一个对象，它包含此请求的状态信息。
        //
        // 返回结果:
        //     引用异步发送的 System.IAsyncResult。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return m_Socket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, callback, state);
        }

        //
        // 摘要:
        //     使 System.Net.Sockets.Socket 与一个本地终结点相关联。
        //
        // 参数:
        //   localEP:
        //     要与 System.Net.Sockets.Socket 关联的本地 System.Net.EndPoint。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     localEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public void Bind(EndPoint localEP)
        {
            m_Socket.Bind(localEP);
        }

        //
        // 摘要:
        //     取消一个对远程主机连接的异步请求。
        //
        // 参数:
        //   e:
        //     System.Net.Sockets.SocketAsyncEventArgs 对象，该对象用于通过调用 System.Net.Sockets.Socket.ConnectAsync(System.Net.Sockets.SocketType,System.Net.Sockets.ProtocolType,System.Net.Sockets.SocketAsyncEventArgs)
        //     方法之一，请求与远程主机的连接。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     e 参数不能为 null，并且 System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public static void CancelConnectAsync(SocketAsyncEventArgs e)
        {
            Socket.CancelConnectAsync(e);
        }

        //
        // 摘要:
        //     关闭 System.Net.Sockets.Socket 连接并释放所有关联的资源。
        public void Close()
        {
            m_Socket.Close();
        }

        //
        // 摘要:
        //     关闭 System.Net.Sockets.Socket 连接并释放所有关联的资源。
        //
        // 参数:
        //   timeout:
        //     等待 timeout 秒以发送所有剩余数据，然后关闭该套接字。
        public void Close(int timeout)
        {
            m_Socket.Close(timeout);
        }

        //
        // 摘要:
        //     建立与远程主机的连接。
        //
        // 参数:
        //   remoteEP:
        //     System.Net.EndPoint，表示远程设备。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public void Connect(EndPoint remoteEP)
        {
            m_Socket.Connect(remoteEP);
        }

        //
        // 摘要:
        //     建立与远程主机的连接。主机由 IP 地址和端口号指定。
        //
        // 参数:
        //   address:
        //     远程主机的 IP 地址。
        //
        //   port:
        //     远程主机的端口号。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     address 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     此方法对 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     系列中的套接字有效。
        //
        //   System.ArgumentException:
        //     address 的长度为零。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public void Connect(IPAddress address, int port)
        {
            m_Socket.Connect(address, port);
        }

        //
        // 摘要:
        //     建立与远程主机的连接。主机由 IP 地址的数组和端口号指定。
        //
        // 参数:
        //   addresses:
        //     远程主机的 IP 地址。
        //
        //   port:
        //     远程主机的端口号。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     addresses 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     此方法对 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     系列中的套接字有效。
        //
        //   System.ArgumentException:
        //     address 的长度为零。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public void Connect(IPAddress[] addresses, int port)
        {
            m_Socket.Connect(addresses, port);
        }

        //
        // 摘要:
        //     建立与远程主机的连接。主机由主机名和端口号指定。
        //
        // 参数:
        //   host:
        //     远程主机的名称。
        //
        //   port:
        //     远程主机的端口号。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     host 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     此端口号无效。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     此方法对 System.Net.Sockets.AddressFamily.InterNetwork 或 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     系列中的套接字有效。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 为 System.Net.Sockets.Socket.Listen(System.Int32)。
        public void Connect(string host, int port)
        {
            m_Socket.Connect(host, port);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentException:
        //     参数无效。如果指定了多个缓冲区，即 System.Net.Sockets.SocketAsyncEventArgs.BufferList 属性不为
        //     null，将会发生此异常。
        //
        //   System.ArgumentNullException:
        //     e 参数不能为 null，并且 System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为空。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 正在侦听或已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs
        //     对象执行套接字操作。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。如果本地终结点和 System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint
        //     不是相同的地址族，也会发生此异常。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public bool ConnectAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.ConnectAsync(e);
        }

        //
        // 摘要:
        //     开始一个对远程主机连接的异步请求。
        //
        // 参数:
        //   socketType:
        //     System.Net.Sockets.SocketType 值之一。
        //
        //   protocolType:
        //     System.Net.Sockets.ProtocolType 值之一。
        //
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentException:
        //     参数无效。如果指定了多个缓冲区，即 System.Net.Sockets.SocketAsyncEventArgs.BufferList 属性不为
        //     null，将会发生此异常。
        //
        //   System.ArgumentNullException:
        //     e 参数不能为 null，并且 System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为空。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 正在侦听或已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs
        //     对象执行套接字操作。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。如果本地终结点和 System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint
        //     不是相同的地址族，也会发生此异常。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈上部的调用方无权执行所请求的操作。
        public static bool ConnectAsync(SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e)
        {
            return Socket.ConnectAsync(socketType, protocolType, e);
        }

        //
        // 摘要:
        //     关闭套接字连接并允许重用套接字。
        //
        // 参数:
        //   reuseSocket:
        //     如果关闭当前连接后可以重用此套接字，则为 true；否则为 false。
        //
        // 异常:
        //   System.PlatformNotSupportedException:
        //     此方法需要 Windows 2000 或更低版本，否则将引发异常。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public void Disconnect(bool reuseSocket)
        {
            m_Socket.Disconnect(reuseSocket);
        }

        //
        // 摘要:
        //     开始异步请求从远程终结点断开连接。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     e 参数不能为 null。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        public bool DisconnectAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.DisconnectAsync(e);
        }

        //
        // 摘要:
        //     释放由 System.Net.Sockets.Socket 类的当前实例占用的所有资源。
        public void Dispose()
        {
            MyDispose();
            m_Socket.Dispose();
        }

        // ChenXiaojun 这个函数不需要实现
        // 摘要:
        //     释放由 System.Net.Sockets.Socket 使用的非托管资源，并可根据需要释放托管资源。
        //
        // 参数:
        //   disposing:
        //     如果为 true，则释放托管资源和非托管资源；如果为 false，则仅释放非托管资源。
        protected virtual void Dispose(bool disposing)
        {
            //m_Socket.Dispose(disposing);
        }

        //
        // 摘要:
        //     重复目标进程的套接字引用，并关闭此进程的套接字。
        //
        // 参数:
        //   targetProcessId:
        //     从中创建重复套接字引用的目标进程的 ID。
        //
        // 返回结果:
        //     要传递到目标进程的套接字引用。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     targetProcessID 不是有效的进程 ID。- 或 -套接字引用的复制失败。
        public SocketInformation DuplicateAndClose(int targetProcessId)
        {
            return m_Socket.DuplicateAndClose(targetProcessId);
        }

        //
        // 摘要:
        //     异步接受传入的连接尝试，并创建新的 System.Net.Sockets.Socket 来处理远程主机通信。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及任何用户定义数据。
        //
        // 返回结果:
        //     一个 System.Net.Sockets.Socket，它处理与远程主机的通信。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     asyncResult 并不是通过对 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     的调用创建的。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndAccept(System.IAsyncResult) 方法以前被调用过。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        public Socket EndAccept(IAsyncResult asyncResult)
        {
            return m_Socket.EndAccept(asyncResult);
        }

        //
        // 摘要:
        //     异步接受传入的连接尝试，并创建新的 System.Net.Sockets.Socket 对象来处理远程主机通信。此方法返回包含所传输的初始数据的缓冲区。
        //
        // 参数:
        //   buffer:
        //     包含所传输字节的类型 System.Byte 的数组。
        //
        //   asyncResult:
        //     System.IAsyncResult 对象，它存储此异步操作的状态信息以及任何用户定义数据。
        //
        // 返回结果:
        //     一个 System.Net.Sockets.Socket 对象，它处理与远程主机的通信。
        //
        // 异常:
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.ArgumentNullException:
        //     asyncResult 为空。
        //
        //   System.ArgumentException:
        //     asyncResult 并不是通过对 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     的调用创建的。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndAccept(System.IAsyncResult) 方法以前被调用过。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问 System.Net.Sockets.Socket 时发生错误。有关更多信息，请参见备注部分。
        public Socket EndAccept(out byte[] buffer, IAsyncResult asyncResult)
        {
            return m_Socket.EndAccept(out buffer, asyncResult);
        }

        //
        // 摘要:
        //     异步接受传入的连接尝试，并创建新的 System.Net.Sockets.Socket 对象来处理远程主机通信。此方法返回一个缓冲区，其中包含初始数据和传输的字节数。
        //
        // 参数:
        //   buffer:
        //     包含所传输字节的类型 System.Byte 的数组。
        //
        //   bytesTransferred:
        //     已传输的字节数。
        //
        //   asyncResult:
        //     System.IAsyncResult 对象，它存储此异步操作的状态信息以及任何用户定义数据。
        //
        // 返回结果:
        //     一个 System.Net.Sockets.Socket 对象，它处理与远程主机的通信。
        //
        // 异常:
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.ArgumentNullException:
        //     asyncResult 为空。
        //
        //   System.ArgumentException:
        //     asyncResult 并不是通过对 System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object)
        //     的调用创建的。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndAccept(System.IAsyncResult) 方法以前被调用过。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问 System.Net.Sockets.Socket 时发生错误。有关更多信息，请参见备注部分。
        public Socket EndAccept(out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
        {
            return m_Socket.EndAccept(out buffer, out bytesTransferred, asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的异步连接请求。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginConnect(System.Net.EndPoint,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步连接调用过 System.Net.Sockets.Socket.EndConnect(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void EndConnect(IAsyncResult asyncResult)
        {
            m_Socket.EndConnect(asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的异步断开连接请求。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult 对象，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        // 异常:
        //   System.NotSupportedException:
        //     操作系统为 Windows 2000 或更低版本，而此方法需要在 Windows XP 中使用。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginDisconnect(System.Boolean,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步连接调用过 System.Net.Sockets.Socket.EndDisconnect(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.Net.WebException:
        //     断开连接请求已超时。
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            m_Socket.EndDisconnect(asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的异步读取。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步读取调用过 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndReceive(IAsyncResult asyncResult)
        {
            return m_Socket.EndReceive(asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的异步读取。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步读取调用过 System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndReceive(IAsyncResult asyncResult, out SocketError errorCode)
        {
            return m_Socket.EndReceive(asyncResult, out errorCode);
        }

        //
        // 摘要:
        //     结束挂起的、从特定终结点进行异步读取。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        //   endPoint:
        //     源 System.Net.EndPoint。
        //
        // 返回结果:
        //     如果成功，则返回已接收的字节数。如果不成功，则返回 0。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步读取调用过 System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint)
        {
            return m_Socket.EndReceiveFrom(asyncResult, ref endPoint);
        }

        //
        // 摘要:
        //     结束挂起的、从特定终结点进行异步读取。此方法还显示有关数据包而不是 System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@)
        //     的更多信息。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        //   socketFlags:
        //     所接收数据包的 System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   endPoint:
        //     源 System.Net.EndPoint。
        //
        //   ipPacketInformation:
        //     所接收数据包的 System.Net.IPAddress 和接口。
        //
        // 返回结果:
        //     如果成功，则返回已接收的字节数。如果不成功，则返回 0。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。- 或 -endPoint 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginReceiveMessageFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步读取调用过 System.Net.Sockets.Socket.EndReceiveMessageFrom(System.IAsyncResult,System.Net.Sockets.SocketFlags@,System.Net.EndPoint@,System.Net.Sockets.IPPacketInformation@)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndReceiveMessageFrom(IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
        {
            return m_Socket.EndReceiveMessageFrom(asyncResult, ref socketFlags, ref endPoint, out ipPacketInformation);
        }

        //
        // 摘要:
        //     结束挂起的异步发送。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息。
        //
        // 返回结果:
        //     如果成功，则将返回向 System.Net.Sockets.Socket 发送的字节数；否则会返回无效 System.Net.Sockets.Socket
        //     错误。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     方法调用后未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前为异步发送已调用过 System.Net.Sockets.Socket.EndSend(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndSend(IAsyncResult asyncResult)
        {
            return m_Socket.EndSend(asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的异步发送。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     如果成功，则将返回向 System.Net.Sockets.Socket 发送的字节数；否则会返回无效 System.Net.Sockets.Socket
        //     错误。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     方法调用后未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前为异步发送已调用过 System.Net.Sockets.Socket.EndSend(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndSend(IAsyncResult asyncResult, out SocketError errorCode)
        {
            return m_Socket.EndSend(asyncResult, out errorCode);
        }

        //
        // 摘要:
        //     结束文件的挂起异步发送。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult 对象，它存储此异步操作的状态信息。
        //
        // 异常:
        //   System.NotSupportedException:
        //     此方法需要 Windows NT。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.ArgumentNullException:
        //     asyncResult 为空。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginSendFile(System.String,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前曾为异步 System.Net.Sockets.Socket.BeginSendFile(System.String,System.AsyncCallback,System.Object)
        //     调用过 System.Net.Sockets.Socket.EndSendFile(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        public void EndSendFile(IAsyncResult asyncResult)
        {
            m_Socket.EndSendFile(asyncResult);
        }

        //
        // 摘要:
        //     结束挂起的、向指定位置进行的异步发送。
        //
        // 参数:
        //   asyncResult:
        //     System.IAsyncResult，它存储此异步操作的状态信息以及所有用户定义数据。
        //
        // 返回结果:
        //     如果成功，则返回已发送的字节数；否则会返回无效 System.Net.Sockets.Socket 错误。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     asyncResult 为 null。
        //
        //   System.ArgumentException:
        //     System.Net.Sockets.Socket.BeginSendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint,System.AsyncCallback,System.Object)
        //     方法调用未返回 asyncResult。
        //
        //   System.InvalidOperationException:
        //     先前为异步发送已调用过 System.Net.Sockets.Socket.EndSendTo(System.IAsyncResult)。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int EndSendTo(IAsyncResult asyncResult)
        {
            return m_Socket.EndSendTo(asyncResult);
        }

        //
        // 摘要:
        //     返回指定的 System.Net.Sockets.Socket 选项的值，表示为一个对象。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        // 返回结果:
        //     一个对象，表示选项的值。当将 optionName 参数设置为 System.Net.Sockets.SocketOptionName.Linger
        //     时，返回值为 System.Net.Sockets.LingerOption 类的一个实例。当将 optionName 设置为 System.Net.Sockets.SocketOptionName.AddMembership
        //     或 System.Net.Sockets.SocketOptionName.DropMembership 时，返回值为 System.Net.Sockets.MulticastOption
        //     类的一个实例。当 optionName 为其他任何值时，返回值为整数。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。- 或 -optionName 设置为不支持的值 System.Net.Sockets.SocketOptionName.MaxConnections。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return m_Socket.GetSocketOption(optionLevel, optionName);
        }

        //
        // 摘要:
        //     返回指定的 System.Net.Sockets.Socket 选项设置，表示为字节数组。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionValue:
        //     System.Byte 类型的数组，用于接收选项设置。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。- 或 -在 .NET Compact Framework 应用程序中，Windows CE
        //     默认缓冲区的空间被设置为 32768 字节。通过调用 Overload:System.Net.Sockets.Socket.SetSocketOption，可以更改每个套接字缓冲区的空间。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            m_Socket.GetSocketOption(optionLevel, optionName, optionValue);
        }

        //
        // 摘要:
        //     返回数组中指定的 System.Net.Sockets.Socket 选项的值。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionLength:
        //     所需返回值的长度（以字节为单位）。
        //
        // 返回结果:
        //     System.Byte 类型的数组，它包含套接字选项的值。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。- 或 -在 .NET Compact Framework 应用程序中，Windows CE
        //     默认缓冲区的空间被设置为 32768 字节。通过调用 Overload:System.Net.Sockets.Socket.SetSocketOption，可以更改每个套接字缓冲区的空间。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            return m_Socket.GetSocketOption(optionLevel, optionName, optionLength);
        }

        //
        // 摘要:
        //     使用数字控制代码，为 System.Net.Sockets.Socket 设置低级操作模式。
        //
        // 参数:
        //   ioControlCode:
        //     一个 System.Int32 值，该值指定要执行的操作的控制代码。
        //
        //   optionInValue:
        //     一个 System.Byte 数组，它包含操作需要的输入数据。
        //
        //   optionOutValue:
        //     一个 System.Byte 数组，它包含操作返回的输出数据。
        //
        // 返回结果:
        //     optionOutValue 参数中的字节数。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.InvalidOperationException:
        //     尝试不使用 System.Net.Sockets.Socket.Blocking 属性更改阻止模式。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return m_Socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        //
        // 摘要:
        //     使用 System.Net.Sockets.IOControlCode 枚举指定控制代码，为 System.Net.Sockets.Socket
        //     设置低级操作模式。
        //
        // 参数:
        //   ioControlCode:
        //     一个 System.Net.Sockets.IOControlCode 值，它指定要执行的操作的控制代码。
        //
        //   optionInValue:
        //     System.Byte 类型的数组，包含操作要求的输入数据。
        //
        //   optionOutValue:
        //     System.Byte 类型的数组，包含由操作返回的输出数据。
        //
        // 返回结果:
        //     optionOutValue 参数中的字节数。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.InvalidOperationException:
        //     尝试不使用 System.Net.Sockets.Socket.Blocking 属性更改阻止模式。
        public int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return m_Socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        //
        // 摘要:
        //     将 System.Net.Sockets.Socket 置于侦听状态。
        //
        // 参数:
        //   backlog:
        //     挂起连接队列的最大长度。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void Listen(int backlog)
        {
            m_Socket.Listen(backlog);
        }

        //
        // 摘要:
        //     确定 System.Net.Sockets.Socket 的状态。
        //
        // 参数:
        //   microSeconds:
        //     等待响应的时间（以微秒为单位）。
        //
        //   mode:
        //     System.Net.Sockets.SelectMode 值之一。
        //
        // 返回结果:
        //     基于 mode 参数中传递的轮询模式值的 System.Net.Sockets.Socket 的状态。模式返回值System.Net.Sockets.SelectMode.SelectRead如果已调用
        //     System.Net.Sockets.Socket.Listen(System.Int32) 并且有挂起的连接，则为 true。- 或 -如果有数据可供读取，则为
        //     true。- 或 -如果连接已关闭、重置或终止，则返回 true；否则，返回 false。System.Net.Sockets.SelectMode.SelectWrite如果正在处理
        //     System.Net.Sockets.Socket.Connect(System.Net.EndPoint) 并且连接已成功，则为 true；-
        //     或 -如果可以发送数据，则返回 true；否则，返回 false。System.Net.Sockets.SelectMode.SelectError如果正在处理不阻止的
        //     System.Net.Sockets.Socket.Connect(System.Net.EndPoint)，并且连接已失败，则为 true；-
        //     或 -如果 System.Net.Sockets.SocketOptionName.OutOfBandInline 未设置，并且带外数据可用，则为
        //     true；否则，返回 false。
        //
        // 异常:
        //   System.NotSupportedException:
        //     mode 参数不是一个 System.Net.Sockets.SelectMode 值。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public bool Poll(int microSeconds, SelectMode mode)
        {
            return m_Socket.Poll(microSeconds, mode);
        }

        //
        // 摘要:
        //     从绑定的 System.Net.Sockets.Socket 套接字接收数据，将数据存入接收缓冲区。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int Receive(byte[] buffer)
        {
            return m_Socket.Receive(buffer);
        }

        //
        // 摘要:
        //     从绑定的 System.Net.Sockets.Socket 接收数据，将数据存入接收缓冲区列表中。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的一个 System.ArraySegment<T> 列表，包含接收的数据。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 参数为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时出现错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int Receive(IList<ArraySegment<byte>> buffers)
        {
            return m_Socket.Receive(buffers);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收数据，将数据存入接收缓冲区。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            return m_Socket.Receive(buffer, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收数据，将数据存入接收缓冲区列表中。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的一个 System.ArraySegment<T> 列表，包含接收的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。- 或 -buffers.Count 是零。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时出现错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            return m_Socket.Receive(buffers, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收指定字节数的数据，并将数据存入接收缓冲区。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     size 超出了 buffer 的大小。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return m_Socket.Receive(buffer, size, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收数据，将数据存入接收缓冲区列表中。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的一个 System.ArraySegment<T> 列表，包含接收的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。- 或 -buffers.Count 是零。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时出现错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
        {
            return m_Socket.Receive(buffers, socketFlags, out errorCode);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收指定的字节数，存入接收缓冲区的指定偏移量位置。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 中存储所接收数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -未设置 System.Net.Sockets.Socket.LocalEndPoint 属性。-
        //     或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return m_Socket.Receive(buffer, offset, size, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，从绑定的 System.Net.Sockets.Socket 接收数据，将数据存入接收缓冲区。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储所接收数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -未设置 System.Net.Sockets.Socket.LocalEndPoint 属性。-
        //     或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            return m_Socket.Receive(buffer, offset, size, socketFlags, out errorCode);
        }

        //
        // 摘要:
        //     开始一个异步请求以便从连接的 System.Net.Sockets.Socket 对象中接收数据。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentException:
        //     参数无效。e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Buffer 或 System.Net.Sockets.SocketAsyncEventArgs.BufferList
        //     属性必须引用有效的缓冲区。可以设置这两个属性中的某一个，但不能同时设置这两个属性。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public bool ReceiveAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.ReceiveAsync(e);
        }

        //
        // 摘要:
        //     将数据报接收到数据缓冲区并存储终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   remoteEP:
        //     按引用传递的 System.Net.EndPoint，表示远程服务器。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            return m_Socket.ReceiveFrom(buffer, ref remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags 将数据报接收到数据缓冲区并存储终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     按引用传递的 System.Net.EndPoint，表示远程服务器。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return m_Socket.ReceiveFrom(buffer, socketFlags, ref remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags 将指定的字节数接收到数据缓冲区并存储终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     按引用传递的 System.Net.EndPoint，表示远程服务器。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     size 小于 0。- 或 -size 大于 buffer 的长度。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -未设置 System.Net.Sockets.Socket.LocalEndPoint 属性。-
        //     或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return m_Socket.ReceiveFrom(buffer, size, socketFlags, ref remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags 将指定字节数的数据接收到数据缓冲区的指定位置并存储终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储所接收数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     按引用传递的 System.Net.EndPoint，表示远程服务器。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去偏移量参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -未设置 System.Net.Sockets.Socket.LocalEndPoint 属性。-
        //     或 -尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return m_Socket.ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP);
        }

        //
        // 摘要:
        //     开始从指定网络设备中异步接收数据。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为 null。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        public bool ReceiveFromAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.ReceiveFromAsync(e);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags 将指定字节数的数据接收到数据缓冲区的指定位置，然后存储终结点和数据包信息。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它是存储接收到的数据的位置。
        //
        //   offset:
        //     buffer 参数中存储所接收数据的位置。
        //
        //   size:
        //     要接收的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     按引用传递的 System.Net.EndPoint，表示远程服务器。
        //
        //   ipPacketInformation:
        //     System.Net.Sockets.IPPacketInformation，它保存地址和接口信息。
        //
        // 返回结果:
        //     接收到的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去偏移量参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -未设置 System.Net.Sockets.Socket.LocalEndPoint 属性。-
        //     或 -.NET Framework 运行在 AMD 64 位处理器上。- 或 -尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.NotSupportedException:
        //     操作系统为 Windows 2000 或更低版本，而此方法需要在 Windows XP 中使用。
        public int ReceiveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
        {
            return m_Socket.ReceiveMessageFrom(buffer, offset, size, ref socketFlags, ref remoteEP, out ipPacketInformation);
        }

        //
        // 摘要:
        //     开始使用指定的 System.Net.Sockets.SocketAsyncEventArgs.SocketFlags 将指定字节数的数据异步接收到数据缓冲区的指定位置，并存储终结点和数据包信息。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为 null。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。
        public bool ReceiveMessageFromAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.ReceiveMessageFromAsync(e);
        }

        //
        // 摘要:
        //     确定一个或多个套接字的状态。
        //
        // 参数:
        //   checkRead:
        //     要检查可读性的 System.Net.Sockets.Socket 实例的 System.Collections.IList。
        //
        //   checkWrite:
        //     一个 System.Net.Sockets.Socket 实例的 System.Collections.IList，用于检查可写性。
        //
        //   checkError:
        //     要检查错误的 System.Net.Sockets.Socket 实例的 System.Collections.IList。
        //
        //   microSeconds:
        //     超时值（以毫秒为单位）。A -1 值指示超时值为无限大。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     checkRead 参数为 null 或为空。－和－checkWrite 参数为 null 或空。－和－checkError 参数为 null 或为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public static void Select(IList checkRead, IList checkWrite, IList checkError, int microSeconds)
        {
            Socket.Select(checkRead, checkWrite, checkError, microSeconds);
        }

        //
        // 摘要:
        //     将数据发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(byte[] buffer)
        {
            return m_Socket.Send(buffer);
        }

        //
        // 摘要:
        //     将列表中的一组缓冲区发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的 System.ArraySegment<T> 的列表，它包含要发送的数据。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。
        //
        //   System.ArgumentException:
        //     buffers 为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。请参见下面的备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int Send(IList<ArraySegment<byte>> buffers)
        {
            return m_Socket.Send(buffers);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags 将数据发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            return m_Socket.Send(buffer, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将列表中的一组缓冲区发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的 System.ArraySegment<T> 的列表，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。
        //
        //   System.ArgumentException:
        //     buffers 为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            return m_Socket.Send(buffers, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将指定字节数的数据发送到已连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     size 小于 0 或超过缓冲区的大小。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -访问套接字时发生操作系统错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return m_Socket.Send(buffer, size, socketFlags);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将列表中的一组缓冲区发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffers:
        //     System.Byte 类型的 System.ArraySegment<T> 的列表，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffers 为 null。
        //
        //   System.ArgumentException:
        //     buffers 为空。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
        {
            return m_Socket.Send(buffers, socketFlags, out errorCode);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将指定字节数的数据发送到已连接的 System.Net.Sockets.Socket（从指定的偏移量开始）。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     数据缓冲区中开始发送数据的位置。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return m_Socket.Send(buffer, offset, size, socketFlags);
        }

        //
        // 摘要:
        //     从指定的偏移量开始使用指定的 System.Net.Sockets.SocketFlags 将指定字节数的数据发送到连接的 System.Net.Sockets.Socket。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     数据缓冲区中开始发送数据的位置。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   errorCode:
        //     一个 System.Net.Sockets.SocketError 对象，它存储套接字错误。
        //
        // 返回结果:
        //     已发送到 System.Net.Sockets.Socket 的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            return m_Socket.Send(buffer, offset, size, socketFlags, out errorCode);
        }

        //
        // 摘要:
        //     将数据异步发送到连接的 System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentException:
        //     e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Buffer 或 System.Net.Sockets.SocketAsyncEventArgs.BufferList
        //     属性必须引用有效的缓冲区。可以设置这两个属性中的某一个，但不能同时设置这两个属性。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     System.Net.Sockets.Socket 尚未连接或者尚未通过 System.Net.Sockets.Socket.Accept()、System.Net.Sockets.Socket.AcceptAsync(System.Net.Sockets.SocketAsyncEventArgs)
        //     或 Overload:System.Net.Sockets.Socket.BeginAccept 方法获得。
        public bool SendAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.SendAsync(e);
        }

        //
        // 摘要:
        //     使用 System.Net.Sockets.TransmitFileOptions.UseDefaultWorkerThread 传输标志，将文件
        //     fileName 发送到连接的 System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   fileName:
        //     一个 System.String，它包含要发送的文件的路径和名称。此参数可以为 null。
        //
        // 异常:
        //   System.NotSupportedException:
        //     套接字未连接到远程主机。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 对象不处于阻止模式，无法接受此同步调用。
        //
        //   System.IO.FileNotFoundException:
        //     未找到文件 fileName。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public void SendFile(string fileName)
        {
            m_Socket.SendFile(fileName);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.TransmitFileOptions 值，将文件 fileName 和数据缓冲区发送到连接的
        //     System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   fileName:
        //     一个 System.String，它包含要发送的文件的路径和名称。此参数可以为 null。
        //
        //   preBuffer:
        //     一个 System.Byte 数组，包含发送文件前要发送的数据。此参数可以为 null。
        //
        //   postBuffer:
        //     一个 System.Byte 数组，包含发送文件后要发送的数据。此参数可以为 null。
        //
        //   flags:
        //     一个或多个 System.Net.Sockets.TransmitFileOptions 值。
        //
        // 异常:
        //   System.NotSupportedException:
        //     操作系统不是 Windows NT 或更高版本。- 或 -套接字未连接到远程主机。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.InvalidOperationException:
        //     System.Net.Sockets.Socket 对象不处于阻止模式，无法接受此同步调用。
        //
        //   System.IO.FileNotFoundException:
        //     未找到文件 fileName。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public void SendFile(string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
        {
            m_Socket.SendFile(fileName, preBuffer, postBuffer, flags);
        }

        //
        // 摘要:
        //     将文件集合或者内存中的数据缓冲区以异步方法发送给连接的 System.Net.Sockets.Socket 对象。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.IO.FileNotFoundException:
        //     未找到在 System.Net.Sockets.SendPacketsElement.FilePath 属性中指定的文件。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。如果 System.Net.Sockets.Socket 未连接到远程主机，也会发生此异常。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     使用的是无连接 System.Net.Sockets.Socket，并且所发送的文件超过了基础传输的最大数据包大小。
        public bool SendPacketsAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.SendPacketsAsync(e);
        }

        //
        // 摘要:
        //     将数据发送到指定的终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   remoteEP:
        //     System.Net.EndPoint，它表示数据的目标位置。
        //
        // 返回结果:
        //     已发送的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            return m_Socket.SendTo(buffer, remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将数据发送到特定的终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     System.Net.EndPoint，它表示数据的目标位置。
        //
        // 返回结果:
        //     已发送的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return m_Socket.SendTo(buffer, socketFlags, remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将指定字节数的数据发送到指定的终结点。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     System.Net.EndPoint，它表示数据的目标位置。
        //
        // 返回结果:
        //     已发送的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     指定的 size 超出 buffer 的大小。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return m_Socket.SendTo(buffer, size, socketFlags, remoteEP);
        }

        //
        // 摘要:
        //     使用指定的 System.Net.Sockets.SocketFlags，将指定字节数的数据发送到指定终结点（从缓冲区中的指定位置开始）。
        //
        // 参数:
        //   buffer:
        //     System.Byte 类型的数组，它包含要发送的数据。
        //
        //   offset:
        //     数据缓冲区中开始发送数据的位置。
        //
        //   size:
        //     要发送的字节数。
        //
        //   socketFlags:
        //     System.Net.Sockets.SocketFlags 值的按位组合。
        //
        //   remoteEP:
        //     System.Net.EndPoint，它表示数据的目标位置。
        //
        // 返回结果:
        //     已发送的字节数。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     buffer 为 null。- 或 -remoteEP 为 null。
        //
        //   System.ArgumentOutOfRangeException:
        //     offset 小于 0。- 或 -offset 大于 buffer 的长度。- 或 -size 小于 0。- 或 -size 大于 buffer
        //     的长度减去 offset 参数的值。
        //
        //   System.Net.Sockets.SocketException:
        //     socketFlags 不是值的有效组合。- 或 -访问 System.Net.Sockets.Socket 时发生操作系统错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Security.SecurityException:
        //     调用堆栈中的调用方没有所需的权限。
        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return m_Socket.SendTo(buffer, offset, size, socketFlags, remoteEP);
        }

        //
        // 摘要:
        //     向特定远程主机异步发送数据。
        //
        // 参数:
        //   e:
        //     要用于此异步套接字操作的 System.Net.Sockets.SocketAsyncEventArgs 对象。
        //
        // 返回结果:
        //     如果 I/O 操作挂起，将返回 true。操作完成时，将引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件。如果 I/O 操作同步完成，将返回 false。在这种情况下，将不会引发 e 参数的 System.Net.Sockets.SocketAsyncEventArgs.Completed
        //     事件，并且可能在方法调用返回后立即检查作为参数传递的 e 对象以检索操作的结果。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     System.Net.Sockets.SocketAsyncEventArgs.RemoteEndPoint 不能为 null。
        //
        //   System.InvalidOperationException:
        //     已经在使用 e 参数中指定的 System.Net.Sockets.SocketAsyncEventArgs 对象执行套接字操作。
        //
        //   System.NotSupportedException:
        //     此方法需要 Windows XP 或更高版本。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     指定的协议是面向连接的，但 System.Net.Sockets.Socket 尚未连接。
        public bool SendToAsync(SocketAsyncEventArgs e)
        {
            return m_Socket.SendToAsync(e);
        }

        //
        // 摘要:
        //     设置套接字的 IP 保护级别。
        //
        // 参数:
        //   level:
        //     要为此套接字设置的 IP 保护级别。
        //
        // 异常:
        //   System.ArgumentException:
        //     level 参数不能为 System.Net.Sockets.IPProtectionLevel.Unspecified。IP 保护级别不能设置为未指定。
        //
        //   System.NotSupportedException:
        //     套接字的 System.Net.Sockets.AddressFamily 必须为 System.Net.Sockets.AddressFamily.InterNetworkV6
        //     或 System.Net.Sockets.AddressFamily.InterNetwork。
        public void SetIPProtectionLevel(IPProtectionLevel level)
        {
            m_Socket.SetIPProtectionLevel(level);
        }

        //
        // 摘要:
        //     将指定的 System.Net.Sockets.Socket 选项设置为指定的 System.Boolean 值。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionValue:
        //     选项的值，表示为 System.Boolean。
        //
        // 异常:
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 对象已关闭。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            m_Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        //
        // 摘要:
        //     将指定的 System.Net.Sockets.Socket 选项设置为指定的值，表示为字节数组。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionValue:
        //     System.Byte 类型的数组，表示选项值。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            m_Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        //
        // 摘要:
        //     将指定的 System.Net.Sockets.Socket 选项设置为指定的整数值。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionValue:
        //     该选项的值。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            m_Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        //
        // 摘要:
        //     将指定的 System.Net.Sockets.Socket 选项设置为指定值，表示为对象。
        //
        // 参数:
        //   optionLevel:
        //     System.Net.Sockets.SocketOptionLevel 值之一。
        //
        //   optionName:
        //     System.Net.Sockets.SocketOptionName 值之一。
        //
        //   optionValue:
        //     一个 System.Net.Sockets.LingerOption 或 System.Net.Sockets.MulticastOption，它包含该选项的值。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     optionValue 为 null。
        //
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            m_Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        //
        // 摘要:
        //     禁用某 System.Net.Sockets.Socket 上的发送和接收。
        //
        // 参数:
        //   how:
        //     System.Net.Sockets.SocketShutdown 值之一，它指定不再允许执行的操作。
        //
        // 异常:
        //   System.Net.Sockets.SocketException:
        //     尝试访问套接字时发生错误。有关更多信息，请参见备注部分。
        //
        //   System.ObjectDisposedException:
        //     System.Net.Sockets.Socket 已关闭。
        public void Shutdown(SocketShutdown how)
        {
            //LogManager.WriteLog(LogTypes.Error, new System.Diagnostics.StackTrace().ToString());
            m_Socket.Shutdown(how);
        }

        #endregion 包装方法

    }
}
