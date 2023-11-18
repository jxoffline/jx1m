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

        
        public byte[] Buffer = null;

      
        public bool isManaged = true;

        public int BlockSize = 0;
    }
}
