using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Tools;
using GameServer.Logic;
using System.Net.Sockets;
using GameServer.Server;
using System.Threading;
using GameServer.Core.Executor;
using GameServer.Tools;
using Server.Protocol;

namespace Server.TCP
{
    /// <summary>
    /// 发送缓冲区
    /// </summary>
    public class SendBuffer
    {
        #region 缓存对象锁

        /// <summary>
        /// 缓存对象锁
        /// </summary>
        public Object BufferLock = new Object();

        /// <summary>
        /// 发送对象锁
        /// </summary>
        private Object SendLock = new Object();

        #endregion 缓存对象锁

        #region 全局变量

        public static int ConstMinSendSize = 32768; //512

        //发送数据包的最小间隔时间
        private static long _SendDataIntervalTicks = 80;

        /// <summary>
        /// 发送数据包的最小间隔时间[单位毫秒]
        /// </summary>
        public static long SendDataIntervalTicks
        {
            get { return _SendDataIntervalTicks; }
            set { _SendDataIntervalTicks = value; }
        }

        //单个套接字发送缓冲区默认的最大值，外部配置
        private static Int32 _MaxSingleSocketSendBufferSize = 0;

        /// <summary>
        /// 单个套接字发送缓冲区默认的最大值，外部配置
        /// </summary>
        public static Int32 MaxSingleSocketSendBufferSize
        {
            get { return _MaxSingleSocketSendBufferSize; }
            set { _MaxSingleSocketSendBufferSize = value; }
        }

        //单个套接字发送数据超时时间，单位毫秒[默认10秒]
        private static long _SendDataTimeOutTicks = 5 * 1000;

        /// <summary>
        /// 单个套接字发送数据超时时间，单位毫秒
        /// </summary>
        public static long SendDataTimeOutTicks
        {
            get { return _SendDataTimeOutTicks; }
            set { _SendDataTimeOutTicks = value; }
        }

        #endregion
        
        #region 基础变量

        //添加第一个数据包的时间
        private long _AddFirstPacketTicks = 0;

        /// <summary>
        /// 添加第一个数据包的时间[单位毫秒]
        /// </summary>
        public long AddFirstPacketTicks
        {
            get { return _AddFirstPacketTicks; }
            set { _AddFirstPacketTicks = value; }
        }

        //上次发送数据包的时间 毫秒
        private long _LastSendDataTicks = 0;

        /// <summary>
        /// 发送超时时间
        /// </summary>
        private long _SendTimeoutTickCount = long.MaxValue;

        /// <summary>
        /// 是否正在发送
        /// </summary>
        private bool _IsSendding = false;

        /// <summary>
        /// 上次发送数据包的时间 毫秒
        /// </summary>
        public long LastSendDataTicks
        {
            get { return _LastSendDataTicks; }
            set { _LastSendDataTicks = value; }
        }

        // 数据缓冲区
        private Byte[] _Buffer = null;

        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public Byte[] Buffer
        {
            get { return _Buffer; }
            set { _Buffer = value; }
        }

        public Queue<MemoryBlock> QueueMemoryBlock = new Queue<MemoryBlock>();

        // 发送缓冲区的最大值，外部配置
        private Int32 _MaxBufferSize = 0;

        /// <summary>
        /// 发送缓冲区的最大值，外部配置
        /// </summary>
        public Int32 MaxBufferSize
        {
            get { return _MaxBufferSize; }
            set { _MaxBufferSize = value; }
        }

        //当前的缓存数据大小
        private Int32 _CurrentBufferSize = 0;

        /// <summary>
        /// 当前的缓存数据大小
        /// </summary>
        public Int32 CurrentBufferSize
        {
            get { return _CurrentBufferSize; }
            set { _CurrentBufferSize = value; }
        }

        /// <summary>
        /// 已标记占用的大小
        /// </summary>
        private int _UsedBufferSize = 0;

        /// <summary>
        /// 可以发送大指令包的最大已用缓冲区大小
        /// </summary>
        public static int MaxBufferSizeForLargePackge = 65536;

        /// <summary>
        /// 绑定的内存块
        /// </summary>
        private MemoryBlock _MemoryBlock = null;

        /// <summary>
        /// 绑定的内存块
        /// </summary>
        public MemoryBlock MyMemoryBlock
        {
            get { return _MemoryBlock; }
            set { _MemoryBlock = value; }
        }

        /// <summary>
        /// 是否记录过超时日志
        /// </summary>
        public bool HasLogTimeOutError = false;

        /// <summary>
        /// 是否记录过缓冲区满日志
        /// </summary>
        public bool HasLogBufferFull = false;

        /// <summary>
        /// 是否记录过忽略大数据包日志
        /// </summary>
        public bool HasLogDiscardBigPacket = false;

        #endregion

        /// <summary>
        /// 初始化缓冲区大小参数单位是字节，必须小于等于[4M] 4 * 1024 * 1024, 默认从 SystemParmas.xml 的 MaxSingleSocketSendBufferSize字段读取
        /// </summary>
        /// <param name="maxBufferSize"></param>
        public SendBuffer(Int32 maxBufferSize = 0)
        {
            if (maxBufferSize <= 0)
            {
                _MaxBufferSize = _MaxSingleSocketSendBufferSize; 
            }
            else
            {
                _MaxBufferSize = maxBufferSize;
            }

            Reset(true);
        }

        /// <summary>
        /// 重置buffer, 然后可以重新使用,内部调用的函数，构造函数和发送完毕之后调用，外部锁上了，理论上这儿不用锁
        /// </summary>
        protected void Reset(bool init = false)
        {
            //lock (this)
            //{
                if (init || !GameManager.FlagOptimizeThreadPool4)
                {
                    _AddFirstPacketTicks = 0;
                    _CurrentBufferSize = 0;
                }

                if (_MaxBufferSize > 0 && _MaxBufferSize <= (4 * 1024 * 1024))
                {
                    bool needMemoryBlock = true;
                    if (GameManager.FlagOptimizeThreadPool5)
                    {
                        if (null != _MemoryBlock)
                        {
                            if (_MemoryBlock.BlockSize < _MaxBufferSize)
                            {
                                if (GameManager.FlagOptimizeThreadPool2)
                                {
                                    TMSKThreadStaticClass.GetInstance().PushMemoryBlock(_MemoryBlock);
                                }
                                else
                                {
                                    Global._MemoryManager.Push(_MemoryBlock);
                                }
                                _MemoryBlock = null;
                                _Buffer = null;
                            }
                            else
                            {
                                needMemoryBlock = false;
                            }
                        }
                    }
                    if (needMemoryBlock)
                    {
                        //理论上不会返回null,除非内存耗尽或出错
                        if (GameManager.FlagOptimizeThreadPool2)
                        {
                            _MemoryBlock = TMSKThreadStaticClass.GetInstance().PopMemoryBlock(_MaxBufferSize);
                        }
                        else
                        {
                            _MemoryBlock = Global._MemoryManager.Pop(_MaxBufferSize);
                        }
                        _Buffer = _MemoryBlock.Buffer;
                    }
                }
            //}
        }

        /// <summary>
        /// 优化版的Reset
        /// </summary>
        public void Reset2()
        {
            lock (BufferLock)
            {
                int remainSize = _CurrentBufferSize - _UsedBufferSize;
                if (remainSize > 0)
                {
                    Array.Copy(_Buffer, _UsedBufferSize, _Buffer, 0, remainSize);
                }
                _CurrentBufferSize = remainSize;
                _UsedBufferSize = 0;
            }
        }

        /// <summary>
        /// 将buffer加入缓存
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool AddBuffer(Byte[] buffer, int offset, int count, TMSKSocket s)
        {
            if (null == buffer || buffer.Length < offset + count)
            {
                return false;
            }

            //改写内容的时候，锁住
            lock (BufferLock)
            {
                //判读剩下空间是否足够
                if (_MaxBufferSize - _CurrentBufferSize <= count)
                {
                    return false;
                }

                if (0 == _CurrentBufferSize)
                {
                    _AddFirstPacketTicks = TimeUtil.NOW();
                }

                DataHelper.CopyBytes(_Buffer, _CurrentBufferSize, buffer, offset, count);
                _CurrentBufferSize += count;

                //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager add Size == {0}, _CurrentBufferSize = {1}", count, _CurrentBufferSize));

                //立刻尝试发送，无延迟，对于网络条件较好时，不需要等待发送
                if (!GameManager.FlagSkipTrySendCall)
                {
                    TrySend(s, _CurrentBufferSize > ConstMinSendSize);
                }
            }

            return true;
        }

        /// <summary>
        /// 尝试发送数据,到最小间隔时间或者外部强迫发送才发送
        /// </summary>
        /// <param name="s"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        private bool TrySend(TMSKSocket s, bool force = false)
        {
            //到【最小间隔时间或者外部强迫发送】且 【_LastSendDataTicks小于等于0，即上次发送结束或者从未发送过】
            //且【_CurrentBufferSize>0,避免_CurrentBufferSize<=0时对this的锁定】 才发送

            long ticks = TimeUtil.NOW();
            if (GameManager.FlagOptimizeAlgorithm)
            {
                if (!_IsSendding && _CurrentBufferSize > 0 && (force || ticks - AddFirstPacketTicks >= _SendDataIntervalTicks))
                {
                    //lock (this)
                    {
                        //System.Diagnostics.Debug.WriteLine("SendBufferManager TrySend Size == " + _CurrentBufferSize);

                        _IsSendding = true;
                        if (GameManager.FlagOptimizeLock2)
                        {
                            Interlocked.Exchange(ref _SendTimeoutTickCount, ticks + _SendDataTimeOutTicks);
                        }
                        else
                        {
                            _SendTimeoutTickCount = ticks + _SendDataTimeOutTicks;
                        }
                        Global._TCPManager.MySocketListener.SendData(s, _Buffer, 0, _CurrentBufferSize, _MemoryBlock);

                        //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager:TrySend _LastSendDataTicks == {0}", _LastSendDataTicks));

                        Reset();
                    }

                    return true;
                }
            }
            else
            {
                if ((force || ticks - AddFirstPacketTicks >= _SendDataIntervalTicks) && _LastSendDataTicks <= 0 && _CurrentBufferSize > 0)
                {
                    //lock (this)
                    {
                        //System.Diagnostics.Debug.WriteLine("SendBufferManager TrySend Size == " + _CurrentBufferSize);

                        _LastSendDataTicks = ticks;
                        Global._TCPManager.MySocketListener.SendData(s, _Buffer, 0, _CurrentBufferSize, _MemoryBlock);

                        //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager:TrySend _LastSendDataTicks == {0}", _LastSendDataTicks));

                        Reset();
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 如果当前socket没有异步发送在进行,且时间间隔或累积数据量达到阈值,才进行异步发送
        /// 异步发送中只锁SendLock,但条件判断需同时锁SendLock和BufferLock
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ticks"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        private bool TrySend2(TMSKSocket s, long ticks, bool force = false)
        {
            int sendSize = 0;
            lock (BufferLock)
            {
                if (!_IsSendding && _CurrentBufferSize > 0 && (force || ticks - AddFirstPacketTicks >= _SendDataIntervalTicks))
                {
                    _IsSendding = true;

                    sendSize = _UsedBufferSize = _CurrentBufferSize;
                    if (GameManager.FlagOptimizeLock2)
                    {
                        Interlocked.Exchange(ref _SendTimeoutTickCount, ticks + _SendDataTimeOutTicks);
                    }
                    else
                    {
                        _SendTimeoutTickCount = ticks + _SendDataTimeOutTicks;
                    }
                    _AddFirstPacketTicks = 0;
                }
            }

            if (sendSize > 0)
            {
                Global._TCPManager.MySocketListener.SendData(s, _Buffer, 0, sendSize, _MemoryBlock, this);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试发送数据,到最小间隔时间或者外部强迫发送才发送
        /// </summary>
        /// <param name="s"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public bool ExternalTrySend(TMSKSocket s, bool force = false, int milliseconds = 0)
        {
            if (GameManager.FlagOptimizeThreadPool4)
            {
                long ticks = TimeUtil.NOW();
                if (ticks - AddFirstPacketTicks >= _SendDataIntervalTicks || ticks < AddFirstPacketTicks)
                {
                    if (Monitor.TryEnter(SendLock))
                    {
                        try
                        {
                            return TrySend2(s, ticks, force);
                        }
                        finally
                        {
                            Monitor.Exit(SendLock);
                        }
                    }
                }
            }
            else
            {
                if (Monitor.TryEnter(BufferLock, milliseconds))
                {
                    try
                    {
                        return TrySend(s, force);
                    }
                    finally
                    {
                        // Ensure that the lock is released.
                        Monitor.Exit(BufferLock);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 判读发送是否超时
        /// </summary>
        /// <returns></returns>
        protected bool IsSendTimeOut()
        {
            //内部调用函数，去掉一个锁，减少一次是一次
            //lock (this)
            {
                //上次没发送过数据包
                if (_LastSendDataTicks <= 0)
                {
                    return false;
                }

                //超时返回true
                long ticks = TimeUtil.NOW();

                //必须加 _LastSendDataTicks > 0 ，防止 ticks - _LastSendDataTicks 时 _LastSendDataTicks 被更改为0，即正好发送成功返回
                if (ticks - _LastSendDataTicks >= _SendDataTimeOutTicks && _LastSendDataTicks > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 指令是否可以丢弃,某些指令丢弃会导致严重的逻辑错误
        /// </summary>
        /// <param name="cmdID"></param>
        /// <returns></returns>
        protected bool CanDiscardCmd(int cmdID)
        {
            switch (cmdID)
            {
                case (int)TCPGameServerCmds.CMD_INIT_GAME:
                case (int)TCPGameServerCmds.CMD_PLAY_GAME:
                case (int)TCPGameServerCmds.CMD_SPR_LOADALREADY:
                case (int)TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP:
                case (int)TCPGameServerCmds.CMD_SPR_MAPCHANGE:
                case (int)TCPGameServerCmds.CMD_SPR_DEAD:
                case (int)TCPGameServerCmds.CMD_SPR_REALIVE:
                case (int)TCPGameServerCmds.CMD_SPR_RELIFE:
                case (int)TCPGameServerCmds.CMD_SPR_LEAVE:
                case (int)TCPGameServerCmds.CMD_SPR_TASKLIST_KEY:
                case (int)TCPGameServerCmds.CMD_SPR_REFRESH_ICON_STATE:
                case (int)TCPGameServerCmds.CMD_NTF_CMD_BASE_ID:
                //case (int)TCPGameServerCmds.CMD_SPR_PLAYBOSSANIMATION:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 判读是否可以发送数据，不能发送的原因 0表示超时， 1表示缓冲区不够， 2表示大指令数据由于缓冲区被填充过半，不再发送
        /// </summary>
        /// <returns></returns>
        public bool CanSend(int bytesCount, int cmdID, out int canNotSendReason, byte[] buffer, TMSKSocket s)
        {
            canNotSendReason = -1;
            long ticks = TimeUtil.NOW();
            lock (BufferLock)
            {
                if (GameManager.FlagOptimizeLock2)
                {
                    //判断是否超时
                    if (ticks > Interlocked.Read(ref _SendTimeoutTickCount) && CanDiscardCmd(cmdID))
                    {
                        canNotSendReason = FullBufferManager.Error_SendTimeOut;
                        return false;
                    }
                }
                else
                {
                    //判断是否超时
                    if (IsSendTimeOut() && CanDiscardCmd(cmdID))
                    {
                        canNotSendReason = FullBufferManager.Error_SendTimeOut;
                        return false;
                    }
                }

                //上次发送过数据包
                /*if (_LastSendDataTicks > 0)
                {
                    //判读剩下空间是否足够
                    if (_MaxBufferSize - _CurrentBufferSize <= bytesCount)
                    {
                        canNotSendReason = FullBufferManager.Error_BufferFull;
                        return false;
                    }
                }*/

                //对于地图特效指令，当网络延迟较大时，可以不再发送
                //if (_CurrentBufferSize > 1024)
                //{
                //    if ((int)(TCPGameServerCmds.CMD_SPR_NEWDECO) == cmdID)
                //    {
                //        canNotSendReason = FullBufferManager.Error_DiscardBigPacket;
                //        return false;
                //    }
                //}

                //对于 CMD_OTHER_ROLE_DATA 这种大数据指令，如果缓冲区只剩下一半，则不再发送
                
                if ((int)(TCPGameServerCmds.CMD_OTHER_ROLE) == cmdID)
                {
                    if (_CurrentBufferSize >= ((_MaxBufferSize - _CurrentBufferSize) << 2))//等价优化运算公式 if (_CurrentBufferSize >= (int)(_MaxBufferSize * 2 / 3))
                    {
                        canNotSendReason = FullBufferManager.Error_DiscardBigPacket;
                        return false;
                    }
                }

                //加入发送缓存
                if (!GameManager.FlagSkipAddBuffCall)
                {
                    return AddBuffer(buffer, 0, bytesCount, s);
                }
            }

            return true;
        }

        /// <summary>
        /// 判读是否可以发送数据，不能发送的原因 0表示超时， 1表示缓冲区不够， 2表示大指令数据由于缓冲区被填充过半，不再发送
        /// 拆分操作发送缓冲区和异步发送调用的锁的优化版本
        /// </summary>
        /// <param name="s"></param>
        /// <param name="tcpOutPacket"></param>
        /// <param name="canNotSendReason"></param>
        /// <returns></returns>
        public bool CanSend2(TMSKSocket s, TCPOutPacket tcpOutPacket, ref int canNotSendReason)
        {
            int cmdID = tcpOutPacket.PacketCmdID;
            long ticks = TimeUtil.NOW();
            byte[] buffer = tcpOutPacket.GetPacketBytes();
            int count = tcpOutPacket.PacketDataSize;
            if (null == buffer || count > buffer.Length)
            {
                return false;
            }

            TCPManager.RecordCmdOutputDataSize(cmdID, count);

            int needRemainSize = _MaxBufferSize - count;
            bool isLargePackge = ((int)(TCPGameServerCmds.CMD_OTHER_ROLE) == cmdID);
            lock (BufferLock)
            {
                //判读剩下空间是否足够
                if (_CurrentBufferSize >= needRemainSize)
                {
                    canNotSendReason = FullBufferManager.Error_BufferFull; 
                    return false;
                }
                else if (0 == _CurrentBufferSize)
                {
                    _AddFirstPacketTicks = ticks;
                }

                if (GameManager.FlagOptimizeLock2)
                {
                    //判断是否超时
                    if (ticks > Interlocked.Read(ref _SendTimeoutTickCount) && CanDiscardCmd(cmdID))
                    {
                        canNotSendReason = FullBufferManager.Error_SendTimeOut;
                        return false;
                    }
                }
                else
                {
                    //判断是否超时
                    if (IsSendTimeOut() && CanDiscardCmd(cmdID))
                    {
                        canNotSendReason = FullBufferManager.Error_SendTimeOut;
                        return false;
                    }
                }

                //对于地图特效指令，当网络延迟较大时，可以不再发送
                //if (_CurrentBufferSize > 1024)
                //{
                //    if ((int)(TCPGameServerCmds.CMD_SPR_NEWDECO) == cmdID)
                //    {
                //        canNotSendReason = FullBufferManager.Error_DiscardBigPacket;
                //        return false;
                //    }
                //}

                //对于 CMD_OTHER_ROLE_DATA 这种大数据指令，如果缓冲区只剩下一半，则不再发送

                if (isLargePackge)
                {
                    if (_CurrentBufferSize >= MaxBufferSizeForLargePackge)
                    {
                        canNotSendReason = FullBufferManager.Error_DiscardBigPacket;
                        return false;
                    }
                }

                //改写内容的时候，锁住
                DataHelper.CopyBytes(_Buffer, _CurrentBufferSize, buffer, 0, count);
                _CurrentBufferSize += count;
                //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager add Size == {0}, _CurrentBufferSize = {1}", count, _CurrentBufferSize));
            } 

            //尝试发送
            if (!GameManager.FlagSkipAddBuffCall)
            {
                if (Monitor.TryEnter(SendLock))
                {
                    bool force = (_CurrentBufferSize > ConstMinSendSize);
                    try
                    {
                        //立刻尝试发送，无延迟，对于网络条件较好时，不需要等待发送
                        if (!GameManager.FlagSkipTrySendCall)
                        {
                            TrySend2(s, ticks, force);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(SendLock);
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// 发送成功后的通知
        /// </summary>
        public void OnSendOK()
        {
            if (GameManager.FlagOptimizeLock2 && GameManager.FlagOptimizeAlgorithm)
            {
                //这项优化基于一个前提:一个Socket同一时刻只能执行有一个异步发送,由_IsSendding控制
                Interlocked.Exchange(ref _SendTimeoutTickCount, long.MaxValue);

                // 是否记录过超时日志
                HasLogTimeOutError = false;

                // 是否记录过缓冲区满日志
                HasLogBufferFull = false;

                // 是否记录过忽略大数据包日志
                HasLogDiscardBigPacket = false;

                //防止编译时优化将_IsSendding的设置提到_SendTimeoutTickCount前面
                Thread.MemoryBarrier();

                //是否正在发送
                _IsSendding = false;
            }
            else if (GameManager.FlagOptimizeAlgorithm)
            {
                lock (BufferLock)
                {
                    _LastSendDataTicks= 0;

                    // 是否记录过超时日志
                    HasLogTimeOutError = false;

                    // 是否记录过缓冲区满日志
                    HasLogBufferFull = false;

                    // 是否记录过忽略大数据包日志
                    HasLogDiscardBigPacket = false;

                    //是否正在发送
                    _IsSendding = false;

                    //重置超时时间
                    _SendTimeoutTickCount = long.MaxValue;
                }
            }
            else
            {
                lock (BufferLock)
                {
                    //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager::OnSendOK()_start _LastSendDataTicks == {0}", _LastSendDataTicks));
                    _LastSendDataTicks = 0;
                    //System.Diagnostics.Debug.WriteLine(String.Format("SendBufferManager::OnSendOK()_end _LastSendDataTicks == {0}", _LastSendDataTicks));

                    // 是否记录过超时日志
                    HasLogTimeOutError = false;

                    // 是否记录过缓冲区满日志
                    HasLogBufferFull = false;

                    // 是否记录过忽略大数据包日志
                    HasLogDiscardBigPacket = false;
                }
            }
        }

        /// <summary>
        /// 判断是否可以记录日志，如果同类日志记录过一次，不再记录，发送成功后，清空日志记录历史
        /// </summary>
        /// <param name="canNotSendReason"></param>
        /// <returns></returns>
        public bool CanLog(int canNotSendReason)
        {
            bool bCanLog = false;
            switch (canNotSendReason)
            {
                case FullBufferManager.Error_SendTimeOut:
                    {
                        if (!HasLogTimeOutError)
                        {
                            HasLogTimeOutError = true;
                            bCanLog = true;
                        }
                        break;
                    }
                case FullBufferManager.Error_BufferFull:
                    {
                        if (!HasLogBufferFull)
                        {
                            HasLogBufferFull = true;
                            bCanLog = true;
                        }
                        break;
                    }
                case FullBufferManager.Error_DiscardBigPacket:
                    {
                        if (!HasLogDiscardBigPacket)
                        {
                            HasLogDiscardBigPacket = true;
                            bCanLog = true;
                        }
                        break;
                    }
                default:
                    break;
            }

            return bCanLog;
        }
    }
}
