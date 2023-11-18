using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Server.Protocol;
using Server.Tools;
using GameServer.Server;
using GameServer.Logic;
using System.Threading;
using GameServer.Tools;

namespace Server.TCP
{
    /// <summary>
    /// 发送缓冲区管理器
    /// </summary>
    public class SendBufferManager
    {
        /// <summary>
        /// 记录发送出去的最大的包的大小
        /// </summary>
        public int MaxOutPacketSize = 0;

        /// <summary>
        /// 记录发送出去的最大的包的指令ID
        /// </summary>
        public int MaxOutPacketSizeCmdID = 0;

        private Dictionary<TMSKSocket, SendBuffer> BufferDict = new Dictionary<TMSKSocket, SendBuffer>();

        //是否退出
        private bool _Exit = false;

        /// <summary>
        /// 是否退出
        /// </summary>
        public bool Exit
        {
            get { return _Exit; }
            set { _Exit = value; }
        }

        public SendBufferManager()
        {
        }

        /// <summary>
        /// 添加输出数据包
        /// </summary>
        /// <param name="s"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public bool AddOutPacket(TMSKSocket s, TCPOutPacket tcpOutPacket)
        {
            bool bRet;
            //发出去的信息不再进行加密处理, 客户端的接受也修改为不做解密处理
            //字节排序
            //DataHelper.SortBytes(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);

            SendBuffer sendBuffer = null;
            if (GameManager.FlagOptimizeLock)
            {
                sendBuffer = s._SendBuffer;
                if (null == sendBuffer)
                {
                    return false;
                }
            }
            else
            {
                lock (BufferDict)
                {
                    if (!BufferDict.TryGetValue(s, out sendBuffer))
                    {
                        //sendBuffer = new SendBuffer();
                        //lock (BufferDict)
                        //{
                        //    BufferDict.Add(s, sendBuffer);
                        //}
                        //不要在这儿动态添加，外部调用这个函数的时机很复杂，可能会在连接断开之后再调用，产生死socket的缓存
                        return false;
                    }
                }
            }

            int canNotSendReason = -1;
            if (GameManager.FlagOptimizeThreadPool4)
            {
                bRet = sendBuffer.CanSend2(s, tcpOutPacket, ref canNotSendReason);
            }
            else
            {
                bRet = sendBuffer.CanSend(tcpOutPacket.PacketDataSize, tcpOutPacket.PacketCmdID, out canNotSendReason, tcpOutPacket.GetPacketBytes(), s);
            }
            if (!bRet)
            {
                //避免大量的重复日志记录
                if (sendBuffer.CanLog(canNotSendReason))
                {
                    string failedReason = FullBufferManager.GetErrorStr(canNotSendReason);
                    LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to client {0} failed, PacketID: {1}, Size: {2}, Issue: {3}", Global.GetSocketRemoteEndPoint(s), (TCPGameServerCmds)tcpOutPacket.PacketCmdID, tcpOutPacket.PacketDataSize, failedReason));
                }

                if (!GameManager.FlagOptimizeLockTrace)
                {
                    Global._FullBufferManager.Add(s, canNotSendReason);
                }
                return (canNotSendReason == (int)FullBufferManager.Error_SendTimeOut); //如果等于超时就返回true， 让外边不再频繁记日志，只有丢弃的包才让外边也记日志
            }

            //加入发送缓存
            //bRet = sendBuffer.AddBuffer(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize, s);

            //记录发出的最大数据包
            if (tcpOutPacket.PacketDataSize > MaxOutPacketSize)
            {
                MaxOutPacketSize = tcpOutPacket.PacketDataSize;
                MaxOutPacketSizeCmdID = tcpOutPacket.PacketCmdID;
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     

            //返回错误，应该是缓冲区满，立即发送
            if (!bRet)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Send packet to client {0} failed, buffer size is not enough, PacketID: {1}, Size: {2}", Global.GetSocketRemoteEndPoint(s), (TCPGameServerCmds)tcpOutPacket.PacketCmdID, tcpOutPacket.PacketDataSize));

                /*
                //这儿不用再判断数据是否发送成功
                sendBuffer.TrySend(s, true);
                //if (sendBuffer.TrySend(s, true))
                //{
                    //发送完毕之后，再尝试添加缓冲数据
                    bRet = sendBuffer.AddBuffer(tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize);

                    //如果添加失败，则直接发送----对于大数据有用
                    if (!bRet)
                    {
                        int nSendLen = tcpOutPacket.PacketDataSize;
                        //System.Diagnostics.Debug.WriteLine("SendBufferManager 某些数据包太大，直接发送 tcpOutPacket.PacketDataSize== " + tcpOutPacket.PacketDataSize);

                        //如果还失败，直接发送--->异步发送的时候是拷贝了内存，如果内存块是被管理的，这儿就多拷贝一次
                        if (!tcpOutPacket.MyMemoryBlock.isManaged)
                        {
                            //对于不受管理的内存，这儿直接发送
                            bRet = Global._TCPManager.MySocketListener.SendData(s, tcpOutPacket.GetPacketBytes(), 0, tcpOutPacket.PacketDataSize, null);
                        }
                        else
                        {
                            //对于受管理的内存，考虑到tcpOutPacket会还回内存池,该内存可能会被用做其他用途，导致混乱，这儿重新拷贝一份
                            MemoryBlock block = Global._MemoryManager.Pop(nSendLen);
                            DataHelper.CopyBytes(block.Buffer, 0, tcpOutPacket.GetPacketBytes(), 0, nSendLen);

                            bRet = Global._TCPManager.MySocketListener.SendData(s, block.Buffer, 0, nSendLen, block);
                        }
                    }
                //}
                */
            }

            return bRet;
        }

        /// <summary>
        /// 尝试发送所有的数据包
        /// </summary>
        public void TrySendAll()
        {
            //最大睡眠时间 毫秒
            //int maxSleepMiniSecs = 1;
            //int  sleepMiniSecs = 0;
            List<TMSKSocket> lsSocket = new List<TMSKSocket>(2000);
            SendBuffer sendBuffer = null;
            bool bFind = false;

            while(!_Exit)
            {
                lsSocket.Clear();

                //System.Diagnostics.Debug.WriteLine("SendBufferManager BufferDict.Count == " + BufferDict.Count);
                //遍历需要加锁吗？当这儿拿到一个buffer，Remove同时被触发，这儿的buffer仍然是有效的，最多产生异常
                //long preTicks = TimeUtil.NOW();

                //保证锁住BufferDict的时间很小
                lock (BufferDict)
                {
                    lsSocket.AddRange(BufferDict.Keys);
                }

                int lsSocketCount = lsSocket.Count;
                //foreach (var s in lsSocket)
                for (int i = 0; i < lsSocketCount; i++)
                {
                    TMSKSocket s = lsSocket[i];
                    if (null == s)
                    {
                        continue;
                    }

                    if (GameManager.FlagOptimizeLock)
                    {
                        sendBuffer = s._SendBuffer;
                        bFind = (sendBuffer != null);
                    }
                    else
                    {
                        lock (BufferDict)
                        {
                            bFind = BufferDict.TryGetValue(s, out sendBuffer);
                        }
                    }

                    if (bFind && null != sendBuffer)
                    {
                        sendBuffer.ExternalTrySend(s, true, 0); //尝试发送，如果锁定超时，继续处理下边的指令
                    }
                }
                //foreach (var buffer in BufferDict)
                //{
                //    buffer.Value.TrySend(buffer.Key);
                //}

                /*int usedTicks = (int)(TimeUtil.NOW() - preTicks);

                sleepMiniSecs = Math.Max(1, maxSleepMiniSecs - usedTicks);
                if (sleepMiniSecs > 0)
                {
                    Thread.Sleep(sleepMiniSecs);
                }*/

                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// 移除相应套接字的缓存
        /// </summary>
        /// <param name="s"></param>
        public void Remove(TMSKSocket s)
        {
            SendBuffer sendBuffer = null;
            lock (BufferDict)
            {
                if (BufferDict.TryGetValue(s, out sendBuffer))
                {
                    BufferDict.Remove(s);
                    if (GameManager.FlagOptimizeLock)
                    {
                        s._SendBuffer = null;
                    }
                }
            }

            //在外部调用，多线程重复也无所谓
            if (null != sendBuffer)
            {
                if (GameManager.FlagOptimizeThreadPool2)
                {
                    TMSKThreadStaticClass.GetInstance().PushMemoryBlock(sendBuffer.MyMemoryBlock);
                }
                else
                {
                    Global._MemoryManager.Push(sendBuffer.MyMemoryBlock);
                }
            }

            if (!GameManager.FlagOptimizeLockTrace)
                Global._FullBufferManager.Remove(s);
        }

        /// <summary>
        /// 添加套接字的缓存
        /// </summary>
        /// <param name="s"></param>
        public void Add(TMSKSocket s)
        {
            lock (BufferDict)
            {
                if (!BufferDict.ContainsKey(s))
                {
                    SendBuffer sendBuffer = new SendBuffer();
                    if (GameManager.FlagOptimizeLock)
                    {
                        s._SendBuffer = sendBuffer;
                    }
                    BufferDict.Add(s, sendBuffer);
                }
            }
        }

        /// <summary>
        /// 发送成功
        /// </summary>
        /// <param name="s"></param>
        public void OnSendBufferOK(TMSKSocket s)
        {
            if (GameManager.FlagOptimizeLock)
            {
                SendBuffer sendBuffer = s._SendBuffer;
                if (null != sendBuffer)
                {
                    sendBuffer.OnSendOK();
                }
            }
            else
            {
                SendBuffer sendBuffer;
                lock (BufferDict)
                {
                    BufferDict.TryGetValue(s, out sendBuffer);
                }

                if (null != sendBuffer)
                {
                    sendBuffer.OnSendOK();
                }
            }

            if (!GameManager.FlagOptimizeLockTrace)
                Global._FullBufferManager.Remove(s);
        }
    }
}
