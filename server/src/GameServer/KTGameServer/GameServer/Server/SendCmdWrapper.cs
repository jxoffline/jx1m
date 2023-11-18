using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Server.Protocol;
using Server.TCP;

namespace GameServer.Server
{
    /// <summary>
    /// 要发送的指令的队列项封装
    /// </summary>
    public class SendPacketWrapper
    {
        public SendPacketWrapper()
        {
            IncInstanceCount();
        }

        /// <summary>
        /// socket对象
        /// </summary>
        public TMSKSocket socket = null;

        /// <summary>
        /// 发送指令包对象
        /// </summary>
        public TCPOutPacket tcpOutPacket = null;

        public void Release()
        {
            DecInstanceCount();
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
