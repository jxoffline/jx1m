using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Tools;
using System.Net.Sockets;
using System.Net;
using Server.TCP;
using GameServer.Logic;
using GameServer.Tools;
using System.Threading;

namespace Server.Protocol
{
    /// <summary>
    /// TCP命令发送包处理(非线程安全)
    /// </summary>
    public class TCPOutPacket : IDisposable
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sendBufferSize">初始化接收缓冲</param>
        public TCPOutPacket()
        {
            IncInstanceCount();
        }

        /// <summary>
        /// 保存接收到的数据包
        /// </summary>
        private byte[] PacketBytes = null;

        /// <summary>
        /// 获取接收缓存的指针
        /// </summary>
        /// <returns></returns>
        public byte[] GetPacketBytes()
        {
            return PacketBytes;
        }

        /// <summary>
        /// 接收到的数据包的命令ID
        /// </summary>
        private UInt16 _PacketCmdID = 0;

        /// <summary>
        /// 数据包的命令ID属性
        /// </summary>
        public UInt16 PacketCmdID
        {
            get { return _PacketCmdID; }
            set { _PacketCmdID = value; }
        }

        /// <summary>
        /// 发送的数据包的命令数据长度
        /// </summary>
        private Int32 _PacketDataSize = 0;

        /// <summary>
        /// 要发送的数据包的命令数据长度属性
        /// </summary>
        public Int32 PacketDataSize
        {
            get { return (Int32)(_PacketDataSize + 6); }
        }

        /// <summary>
        /// 扩展对象
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

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
        /// 数据写入
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="offset">从字节偏移处拷贝</param>
        /// <param name="count">写入的长度</param>
        public bool FinalWriteData(byte[] buffer, int offset, int count)
        {
            if (null != PacketBytes)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("TCP发出命令包不能被重复写入数据, 命令ID: {0}", PacketCmdID));
                return false;
            }

            //先判断是否超出的最大包的大小
            if ((6 + count) >= (int)TCPCmdPacketSize.MAX_SIZE)
            {
                //throw new Exception(string.Format("TCP命令包长度最大不能超过: {0}", (int)TCPCmdPacketSize.MAX_SIZE));
                LogManager.WriteLog(LogTypes.Error, string.Format("TCP命令包长度:{0}, 最大不能超过: {1}, 命令ID: {2}", count, (int)TCPCmdPacketSize.MAX_SIZE, PacketCmdID));
                return false;
            }

            //理论上这儿是不会返回NULL的,如果返回，直接崩溃,外部会接收到异常
            if (GameManager.FlagOptimizeThreadPool2)
            {
                _MemoryBlock = TMSKThreadStaticClass.GetInstance().PopMemoryBlock(count + 6);
            }
            else
            {
                _MemoryBlock = Global._MemoryManager.Pop(count + 6);
            }
            PacketBytes = _MemoryBlock.Buffer;

            //写入数据
            int offsetTo = (int)6;
            DataHelper.CopyBytes(PacketBytes, offsetTo, buffer, offset, count);
            _PacketDataSize = count;
            Final();

            return true;
        }

        public int GetPacketCmdData(out string cmddata)
        {
            if (null == PacketBytes || 0 >= _PacketDataSize)
            {
                cmddata = null;
                return 0;
            }

            cmddata = new UTF8Encoding().GetString(PacketBytes, 6, _PacketDataSize);
            return _PacketDataSize;
        }

        /// <summary>
        /// 生成要发送的指令包字节序
        /// </summary>
        private void Final()
        {
            //拷贝数据长度
            Int32 length = (Int32)(_PacketDataSize + 2);
            DataHelper.CopyBytes(PacketBytes, 0, BitConverter.GetBytes(length), 0, 4);

            //拷贝指令
            DataHelper.CopyBytes(PacketBytes, 4, BitConverter.GetBytes(_PacketCmdID), 0, 2);
        }

        /// <summary>
        /// 重复利用指令包
        /// </summary>
        public void Reset()
        {
            PacketBytes = null;
            PacketCmdID = 0;
            _PacketDataSize = 0;

            if (null != _MemoryBlock)
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
            }
        }

        /// <summary>
        /// 释放函数
        /// </summary>
        public void Dispose()
        {
            DecInstanceCount();
            Tag = null;
        }

        /// <summary>
        /// 生成TCPOutPacket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cmd"></param>
        public static TCPOutPacket MakeTCPOutPacket(TCPOutPacketPool pool, string data, int cmd)
        {

            //连接成功, 立即发送请求登陆的指令
            TCPOutPacket tcpOutPacket = pool.Pop();
            tcpOutPacket.PacketCmdID = (UInt16)cmd;
            byte[] bytesCmd = new UTF8Encoding().GetBytes(data);
            tcpOutPacket.FinalWriteData(bytesCmd, 0, bytesCmd.Length);
            return tcpOutPacket;
        }

        /// <summary>
        /// 生成TCPOutPacket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cmd"></param>
        public static TCPOutPacket MakeTCPOutPacket(TCPOutPacketPool pool, byte[] data, int offset, int length, int cmd)
        {
            //if (pool.Count <= 0) //如果缓存为空，则打印日志，看是谁在分配
            //{
            //    //调试信息
            //    if (LogManager.LogTypeToWrite >= LogTypes.Info && LogManager.LogTypeToWrite <= LogTypes.Error)
            //    {
            //        try
            //        {
            //            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            //            string methodName1 = "", methodName2 = "";
            //            if (stackTrace.FrameCount > 1)
            //            {
            //                methodName2 = stackTrace.GetFrame(1).GetMethod().Name;
            //            }

            //            if (stackTrace.FrameCount > 2)
            //            {
            //                methodName1 = stackTrace.GetFrame(2).GetMethod().Name;
            //            }

            //            LogManager.WriteLog(LogTypes.Error, string.Format("{0}->{1}, 调用MakeTCPOutPacket", methodName1, methodName2));
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    }
            //}

            //连接成功, 立即发送请求登陆的指令
            TCPOutPacket tcpOutPacket = pool.Pop();
            tcpOutPacket.PacketCmdID = (UInt16)cmd;
            tcpOutPacket.FinalWriteData(data, offset, length);
            return tcpOutPacket;
        }

        /// <summary>
        /// 生成TCPOutPacket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cmd"></param>
        public static TCPOutPacket MakeTCPOutPacket(TCPOutPacketPool pool, byte[] data, int cmd)
        {
            TCPOutPacket tcpOutPacket = pool.Pop();
            tcpOutPacket.PacketCmdID = (UInt16)cmd;
            tcpOutPacket.FinalWriteData(data, 0, data.Length);
            return tcpOutPacket;
        }

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
            Interlocked.Increment(ref TotalInstanceCount);
        }

        /// <summary>
        /// 减少计数
        /// </summary>
        public static void DecInstanceCount()
        {
            Interlocked.Decrement(ref TotalInstanceCount);
        }

        /// <summary>
        /// 获取计数
        /// </summary>
        public static int GetInstanceCount()
        {
            return TotalInstanceCount;
        }

        #endregion 计数器
    }
}
