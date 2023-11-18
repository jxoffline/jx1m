using System;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// 数据库命令池
    /// </summary>
    public class DBCmdPool
    {
        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        /// <param name="capacity">Maximum number of DBCommand objects the pool can hold.</param>
        internal DBCmdPool(Int32 capacity)
        {
            this.pool = new Stack<DBCommand>(capacity);
        }

        /// <summary>
        /// Pool of DBCommand.
        /// </summary>
        private Stack<DBCommand> pool;

        /// <summary>
        /// The number of DBCommand instances in the pool.
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
        /// Removes a DBCommand instance from the pool.
        /// </summary>
        /// <returns>DBCommand removed from the pool.</returns>
        internal DBCommand Pop()
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
        /// <param name="item">DBCommand instance to add to the pool.</param>
        internal void Push(DBCommand item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("添加到DBCommandPool 的item不能是空(null)");
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}