using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.TCP
{
    public class MemoryBlock
    {
        public MemoryBlock(Int32 blockSize, bool isManaged = true)
        {
            this.Buffer = new Byte[blockSize];
            this.isManaged = isManaged;
            this.BlockSize = blockSize;
        }

        /// <summary>
        /// 缓冲区
        /// </summary>
        public byte[] Buffer = null;

        /// <summary>
        /// 是否被内存管理器管理，如果被管理，则需要被还回内存管理队列
        /// </summary>
        public bool isManaged = true;

        /// <summary>
        /// 块大小
        /// </summary>
        public int BlockSize = 0;
    }
}
