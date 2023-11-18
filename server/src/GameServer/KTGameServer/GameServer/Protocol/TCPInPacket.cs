using GameServer.Server;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Protocol
{

    public delegate bool TCPCmdPacketEventHandler(object sender, int doType);


    public class TCPInPacket : IDisposable
    {
        private Object mutex = new Object();

        public TCPInPacket(int recvBufferSize = (int)TCPCmdPacketSize.RECV_MAX_SIZE)
        {
            PacketBytes = new byte[recvBufferSize];
            IncInstanceCount();
        }

        private byte[] PacketBytes = null;


        public byte[] GetPacketBytes()
        {
            return PacketBytes;
        }


        private int _LastCheckTicks = 0;


        public int LastCheckTicks
        {
            get { return _LastCheckTicks; }
            set { _LastCheckTicks = value; }
        }

        private TMSKSocket _Socket = null;


        public TMSKSocket CurrentSocket
        {
            get { return _Socket; }
            set { _Socket = value; }
        }


        private UInt16 _PacketCmdID = 0;


        public UInt16 PacketCmdID
        {
            get
            {
                UInt16 ret = 0;

                {
                    ret = _PacketCmdID;
                }
                return ret;
            }
        }


        public UInt16 LastPacketCmdID
        {
            get;
            set;
        }


        private Int32 _PacketDataSize = 0;


        public Int32 PacketDataSize
        {
            get
            {
                Int32 ret = 0;

                {
                    ret = _PacketDataSize;
                }
                return ret;
            }
        }

        #region 命令包处理相关

        /// <summary>
        /// 命令包缓冲池
        /// </summary>
        private Queue<CmdPacket> _cmdPacketPool = new Queue<CmdPacket>();

        /// <summary>
        /// 命令包缓冲池
        /// </summary>
        public Queue<CmdPacket> CmdPacketPool
        {
            get { return _cmdPacketPool; }
        }

        /// <summary>
        /// 当前TMSKSocket的数据包是否被某个工作线程处理中
        /// </summary>
        private bool _isDealingByWorkerThread = false;

        /// <summary>
        /// 当前TMSKSocket的数据包是否被某个工作线程处理中
        /// 同时只能被一个线程处理
        /// </summary>
        public bool IsDealingByWorkerThread
        {
            get { return _isDealingByWorkerThread; }
            set { _isDealingByWorkerThread = value; }
        }

        /// <summary>
        /// 缓存一个命令包数据
        /// </summary>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool CacheCmdPacketData(int nID, byte[] data, int count)
        {
            //不允许缓存太多的命令，命令太多，宁可丢弃,最多100个差不多了
            if (_cmdPacketPool.Count > 100)
            {
                return false;
            }

            lock (_cmdPacketPool)
            {
                _cmdPacketPool.Enqueue(new CmdPacket(nID, data, count));
            }

            return true;
        }

        /// <summary>
        /// 每次最多取6个命令
        /// </summary>
        /// <param name="ls"></param>
        public bool PopCmdPackets(Queue<CmdPacket> ls)
        {
            ls.Clear();

            lock (_cmdPacketPool)
            {
                //有线程正在处理，不返回任何包
                if (_isDealingByWorkerThread || _cmdPacketPool.Count <= 0)
                {
                    return false;
                }

                //执行到这儿，ls肯定会返回数据,意味着外部肯定会处理
                _isDealingByWorkerThread = true;

                //每次最多取6个命令
                for (int n = 0; n < 6; n++)
                {
                    if (_cmdPacketPool.Count <= 0)
                    {
                        break;
                    }

                    ls.Enqueue(_cmdPacketPool.Dequeue());
                }

                return true;
            }
        }

        /// <summary>
        /// 外部工作线程处理完数据包时调用
        /// </summary>
        public void OnThreadDealingComplete()
        {
            //这儿应该不锁也行
            lock (_cmdPacketPool)
            {
                _isDealingByWorkerThread = false;
            }
        }

        /// <summary>
        /// 返回待处理数据包个数
        /// </summary>
        /// <returns></returns>
        public int GetCacheCmdPacketCount()
        {
            lock (_cmdPacketPool)
            {
                return _cmdPacketPool.Count();
            }
        }

        #endregion 命令包处理相关

        /// <summary>
        /// 已经接收到的命令的数据长度
        /// </summary>
        private Int32 PacketDataHaveSize = 0;

        /// <summary>
        /// 是否正在等待数据
        /// </summary>
        private bool IsWaitingData = false;

        /// <summary>
        /// 释放函数
        /// </summary>
        public void Dispose()
        {
            Reset();
            DecInstanceCount();
        }

        /// <summary>
        /// 接收完毕后通知外部处理的事件
        /// </summary>
        public event TCPCmdPacketEventHandler TCPCmdPacketEvent;

        /// <summary>
        /// 命令头缓冲
        /// </summary>
        private byte[] CmdHeaderBuffer = new byte[6];

        /// <summary>
        /// 已经接收到的命令头的大小
        /// </summary>
        private int CmdHeaderSize = 0;

        /// <summary>
        /// 将收到的数据写入
        /// </summary>
        /// <param name="buffer">收到的数据缓存</param>
        /// <param name="offset">从字节偏移处拷贝</param>
        /// <param name="count">写入的长度</param>
        public bool WriteData(byte[] buffer, int offset, int count)
        {
            //先锁定，否则字节流的顺序未必会是正确的顺序
            lock (mutex)
            {
                //一定要在系统设计时避免大的命令包，过长的数据拆分成小指令包发送
                //这个缓冲一定要避免越界, 各个分系统之间一定要设计好指定包的最大长度

                //1.首先判断当前是否正在等待数据
                if (IsWaitingData)
                {
                    int copyCount = (count >= (_PacketDataSize - PacketDataHaveSize)) ? _PacketDataSize - PacketDataHaveSize : count;
                    if (copyCount > 0)
                    {
                        DataHelper.CopyBytes(PacketBytes, PacketDataHaveSize, buffer, offset, copyCount);
                        PacketDataHaveSize += copyCount;
                    }

                    //判断命令包的数据是否接收完毕, 否则继续等待
                    if (PacketDataHaveSize >= _PacketDataSize)
                    {
                        bool eventReturn = true;

                        //通知外部事件处理
                        if (null != TCPCmdPacketEvent)
                        {
                            // 这里应该新生成一个对象，不应该在锁内
                            eventReturn = TCPCmdPacketEvent(this, 0);
                        }
                        //不再直接让外部处理，缓存起来，让外部线程处理,缓存可能超过最大命令数量限制，
                        //不用处理，直接丢弃该数据就行,也可以考虑eventReturn为false直接断开这个链接
                        //CacheCmdPacketData(PacketCmdID, GetPacketBytes(), PacketDataSize);

                        //清空当前的状态
                        LastPacketCmdID = _PacketCmdID;
                        _PacketCmdID = 0;
                        _PacketDataSize = 0;
                        PacketDataHaveSize = 0;
                        IsWaitingData = false;
                        CmdHeaderSize = 0;

                        if (!eventReturn) //处理失败，要求关闭连接
                        {
                            return false;
                        }

                        //处理剩余的字节
                        if (count > copyCount)
                        {
                            //改变参数
                            offset += copyCount;
                            count -= copyCount;

                            //接收数据(递归实现, 简单)
                            return WriteData(buffer, offset, count);
                        }
                    }

                    return true;
                }
                else
                {
                    //正在等待接收命令头
                    int copyLeftSize = count > (6 - CmdHeaderSize) ? (6 - CmdHeaderSize) : count;
                    DataHelper.CopyBytes(CmdHeaderBuffer, CmdHeaderSize, buffer, offset, copyLeftSize);
                    CmdHeaderSize += copyLeftSize;
                    if (CmdHeaderSize < 6)
                    {
                        return true; //继续等待
                    }

                    //首先读取4个字节的整数
                    _PacketDataSize = BitConverter.ToInt32(CmdHeaderBuffer, 0);

                    //再读取2个字节的指令
                    _PacketCmdID = BitConverter.ToUInt16(CmdHeaderBuffer, 4);

                    if (_Socket == null)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("TcpInPacket.WriteData, _Socket is NULL: {0}[{1}]", (TCPGameServerCmds)_PacketCmdID, _PacketCmdID));
                    }


                    if (_PacketDataSize <= 0 || _PacketDataSize >= (Int32)TCPCmdPacketSize.RECV_MAX_SIZE)
                    {
                        /* 不需要了，注释掉，chenjingui. 20150901
                        //处理策略文件的请求(有时策略服务器响应不过来，则会直接请求这里来)
                        if (-1128292116 == _PacketDataSize && -19527 == _PacketCmdID)
                        {
                            if (HandlePolicyFileRequest(buffer, offset, count))
                            {
                                return true; //继续等待，或者已经处理
                            }
                        }
                        else if (-1884833884 == _PacketDataSize && -6212 == _PacketCmdID)
                        {
                            int headerLength = 0;

                            //处理TGW(腾讯防火墙)的请求
                            	if (HandleTGWRequest(buffer, offset, count, ref headerLength))
                            {
                                offset += headerLength;
                                count -= headerLength;
                                if (count > 0)
                                {
                                    //接收数据(递归实现, 简单)
                                    return WriteData(buffer, offset, count);
                                }

                                return true; //继续等待，或者已经处理
                            }
                        }*/
                        //throw new Exception(string.Format("接收到的非法数据长度的tcp命令, Cmd={0}, Length={1} , 需要立即和服务器端断开!", (TCPGameServerCmds)_PacketCmdID, _PacketDataSize));
                        LogManager.WriteLog(LogTypes.Error, string.Format("Packet length is not correct, Cmd={0}, Length={1}, offset={2}, count={3}", (TCPGameServerCmds)_PacketCmdID, _PacketDataSize, offset, count));
                        return false;
                    }

                    //增加偏移
                    offset += copyLeftSize;

                    //减少拷贝量
                    count -= copyLeftSize;

                    //等待数据中...
                    IsWaitingData = true;

                    //接收的字节归0
                    PacketDataHaveSize = 0;
                    _PacketDataSize -= 2; //减去命令的长度

                    //接收数据(递归实现, 简单)
                    return WriteData(buffer, offset, count);
                }
            }
        }

        /// <summary>
        /// 为不同的连接重复使用
        /// </summary>
        public void Reset()
        {
            lock (mutex)
            {
                _Socket = null;
                _PacketCmdID = 0;
                LastPacketCmdID = 0;
                _PacketDataSize = 0;
                PacketDataHaveSize = 0;
                IsWaitingData = false;
                CmdHeaderSize = 0;
                _cmdPacketPool.Clear();//以前少写了
                _LastCheckTicks = 0;
            }
        }

        #region 特殊指令处理-Flash策略

        public const string POLICY_STRING = "<policy-file-request/>\0";//这个是固定的

        /// <summary>
        /// 处理策略文件的请求
        /// </summary>
        private bool HandlePolicyFileRequest(byte[] buffer, int offset, int count)
        {
            //正在等待接收命令头
            //if (count == 23)
            //{
            //byte[] bytes1 = Encoding.UTF8.GetBytes(POLICY_STRING);
            //byte[] bytes2 = new byte[count];

            //字节排序
            //DataHelper.SortBytes(buffer, offset, count);
            //DataHelper.CopyBytes(bytes2, 0, buffer, offset, count);
            //if (DataHelper.CompBytes(bytes1, bytes2))
            //{
            TCPCmdPacketEvent(this, 1);

            //清空当前的状态
            _PacketCmdID = 0;
            _PacketDataSize = 0;
            PacketDataHaveSize = 0;
            IsWaitingData = false;
            CmdHeaderSize = 0;
            _LastCheckTicks = 0;

            return true;
            //}
            //}

            //return false;
        }

        #endregion 特殊指令处理-Flash策略

        #region 特殊指令处理-腾讯策略

        //         public const string TGW_STRING_HEADER = "tgw_l7_forward\r\n";//这个是固定的
        //
        //         /// <summary>
        //         /// 处理TGW(腾讯防火墙)的请求
        //         /// </summary>
        //         private bool HandleTGWRequest(byte[] buffer, int offset, int count, ref int headerLength)
        //         {
        //             DataHelper.SortBytes(buffer, offset, count);
        //             string cmdData = new UTF8Encoding().GetString(buffer, offset, count);
        //             if (0 == cmdData.IndexOf(TGW_STRING_HEADER))
        //             {
        //                 int findEnd = cmdData.IndexOf("\r\n\r\n");
        //                 if (findEnd > 0)
        //                 {
        //                     findEnd += 4;
        //                     headerLength = findEnd;
        //
        //                     //清空当前的状态
        //                     _PacketCmdID = 0;
        //                     _PacketDataSize = 0;
        //                     PacketDataHaveSize = 0;
        //                     IsWaitingData = false;
        //                     CmdHeaderSize = 0;
        //                     _LastCheckTicks = 0;
        //
        //                     DataHelper.SortBytes(buffer, offset, count);
        //                     return true;
        //                 }
        //             }
        //
        //             DataHelper.SortBytes(buffer, offset, count);
        //             return false;
        //         }

        #endregion 特殊指令处理-腾讯策略

        #region 计数器

        /// <summary>
        /// 静态计数锁
        /// </summary>
        private static Object CountLock = new Object();

        /// <summary>
        /// 总的怪物计数
        /// </summary>
        private static int TotalInstanceCount = 0;

        /// <summary>
        /// 增加计数
        /// </summary>
        public static void IncInstanceCount()
        {
            lock (CountLock)
            {
                TotalInstanceCount++;
            }
        }

        /// <summary>
        /// 减少计数
        /// </summary>
        public static void DecInstanceCount()
        {
            lock (CountLock)
            {
                TotalInstanceCount--;
            }
        }

        /// <summary>
        /// 获取计数
        /// </summary>
        public static int GetInstanceCount()
        {
            int count = 0;
            lock (CountLock)
            {
                count = TotalInstanceCount;
            }

            return count;
        }

        #endregion 计数器
    }
}
