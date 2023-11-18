using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server.Protocol
{
    /// <summary>
    /// TCPOutPacket缓冲池实现
    /// </summary>
    public class TCPOutPacketPool
    {
        /// <summary>
        /// Pool of TCPOutPacket.
        /// </summary>
        Stack<TCPOutPacket> pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of TCPOutPacket objects the pool can hold.</param>
//         internal TCPOutPacketPool(Int32 capacity)
//         {
//             this.pool = new Stack<TCPOutPacket>(capacity);
//         }

        private static TCPOutPacketPool instance = new TCPOutPacketPool();

        private TCPOutPacketPool() { }

        public static TCPOutPacketPool getInstance()
        {
            return instance;
        }

        public void initialize(Int32 capacity)
        {
            this.pool = new Stack<TCPOutPacket>(capacity);
        }

        /// <summary>
        /// The number of TCPOutPacket instances in the pool. 
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
        /// Removes a TCPOutPacket instance from the pool.
        /// </summary>
        /// <returns>TCPOutPacket removed from the pool.</returns>
        internal TCPOutPacket Pop()
        {
            //lock (this.pool)
            //{
            //    if (this.pool.Count <= 0)
            //    {
            //        //临时分配
            //        return new TCPOutPacket();
            //    }

            //    TCPOutPacket item = this.pool.Pop();
            //    return item;
            //}

            return new TCPOutPacket();
        }

        /// <summary>
        /// Add a TCPOutPacket instance to the pool. 
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(TCPOutPacket item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("添加到TCPOutPacketPool 的item不能是空(null)");
            }

            //lock (this.pool)
            //{
            //    /*bool found = false;
            //    foreach(var oldItem in this.pool)
            //    {
            //        if (oldItem == item)
            //        {
            //            found = true;
            //            break;
            //        }
            //    }*/

            //    //必须在这儿调用，保证分配的内存被还回缓冲区
            //    item.Reset();
            //    if (this.pool.Count < 5000) //限制最大1000个缓存
            //    {
            //        this.pool.Push(item);
            //    }
            //    else
            //    {
            //        item.Dispose(); //释放
            //    }
            //}

            //必须在这儿调用，保证分配的内存被还回缓冲区
            item.Reset();
            item.Dispose(); //释放
        }
    }
}
