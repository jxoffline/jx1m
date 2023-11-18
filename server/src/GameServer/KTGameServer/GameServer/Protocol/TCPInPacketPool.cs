using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Server.TCP;

namespace Server.Protocol
{
    /// <summary>
    /// TCPInPacket缓冲池实现
    /// </summary>
    public class TCPInPacketPool
    {
        /// <summary>
        /// Pool of TCPInPacket.
        /// </summary>
        Stack<TCPInPacket> pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of TCPInPacket objects the pool can hold.</param>
        internal TCPInPacketPool(Int32 capacity)
        {
            this.pool = new Stack<TCPInPacket>(capacity);
        }

        /// <summary>
        /// The number of TCPInPacket instances in the pool. 
        /// </summary>
        internal Int32 Count
        {
            get 
            {
                int count = 0;
                lock (this.pool)
                {
                    count = this.pool.Count;
                }

                return count;
            }
        }

        /// <summary>
        /// Removes a TCPInPacket instance from the pool.
        /// </summary>
        /// <returns>TCPInPacket removed from the pool.</returns>
        internal TCPInPacket Pop(TMSKSocket s, TCPCmdPacketEventHandler TCPCmdPacketEvent)
        {
            lock (this.pool)
            {
                TCPInPacket tcpInPacket = null;
                if (this.pool.Count <= 0)
                {
                    //临时分配
                    tcpInPacket = new TCPInPacket() { CurrentSocket = s };
                    tcpInPacket.TCPCmdPacketEvent += TCPCmdPacketEvent;
                    return tcpInPacket;
                }

                tcpInPacket = this.pool.Pop();
                tcpInPacket.CurrentSocket = s;
                return tcpInPacket;
            }
        }

        /// <summary>
        /// Add a TCPInPacket instance to the pool. 
        /// </summary>
        /// <param name="item">TCPInPacket instance to add to the pool.</param>
        internal void Push(TCPInPacket item)
        {
            if (item == null) 
            {
                throw new ArgumentNullException("添加到TCPInPacketPool 的item不能是空(null)"); 
            }
            lock (this.pool)
            {
                item.Reset();
                this.pool.Push(item);
            }
        }
    }
}
