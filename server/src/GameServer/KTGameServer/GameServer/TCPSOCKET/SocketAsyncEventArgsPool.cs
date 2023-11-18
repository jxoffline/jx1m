using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.TCP
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.socketasynceventargs.aspx
    /// Represents a collection of reusable SocketAsyncEventArgs objects.  
    /// 固定的预先分配缓冲 + 临时扩展的缓冲(因为Send次数不确定, 所以固定缓冲可能不够使用)
    /// </summary>
    public sealed class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// Pool of SocketAsyncEventArgs.
        /// </summary>
        Stack<SocketAsyncEventArgs> pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// The number of SocketAsyncEventArgs instances in the pool. 
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
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        internal SocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                if (this.pool.Count <= 0) return null; //防止固定缓冲不够使用
                return this.pool.Pop();
            }
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool. 
        /// </summary>
        /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null) 
            { 
                throw new ArgumentNullException("添加到SocketAsyncEventArgsPool 的item不能是空(null)"); 
            }
            lock (this.pool)
            {
                if (this.pool.Count < 30000) //限制最大3000个缓存
                {
                    this.pool.Push(item);
                }
                else
                {
                    item.Dispose(); //释放
                }
            }
        }
    }
}
