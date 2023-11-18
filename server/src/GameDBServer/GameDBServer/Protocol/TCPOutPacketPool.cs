using System;
using System.Collections.Generic;

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
        private Stack<TCPOutPacket> pool;

        private static TCPOutPacketPool instance = new TCPOutPacketPool();

        private TCPOutPacketPool()
        { }

        public static TCPOutPacketPool getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of TCPOutPacket objects the pool can hold.</param>
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
            lock (this.pool)
            {
                if (this.pool.Count <= 0)
                {
                    //临时分配
                    return new TCPOutPacket();
                }

                return this.pool.Pop();
            }
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
            lock (this.pool)
            {
                item.Reset();
                this.pool.Push(item);
            }
        }
    }
}