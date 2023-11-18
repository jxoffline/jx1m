#define MultiMemoryDict
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GameServer.Tools;
using System.Diagnostics;
using Server.Tools;

namespace Server.TCP
{
    public class MemoryStackArray
    {
        public Stack<MemoryBlock>[] StackList = new Stack<MemoryBlock>[MemoryManager.ConstSplitePoolNum];
        public int PushIndex;
        public int PopIndex;
    }

    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/bb517542.aspx
    /// This class creates a single large TotalBuffer which can be divided up 
    /// and assigned to SocketAsyncEventArgs objects for use with each 
    /// socket I/O operation.  
    /// This enables bufffers to be easily reused and guards against 
    /// fragmenting heap memory.
    /// </summary>
    /// <remarks>The operations exposed on the TotalBufferManager class are not thread safe.</remarks>
    public class MemoryManager
    {
        private static Object MemoryLock = new Object();
        private static long TotalNewAllocMemorySize = 0;
        public const int ConstSplitePoolNum = 16;
        public const int ConstSplitePoolMask = 15;

#if MultiMemoryDict
        private Dictionary<int, MemoryStackArray> MemoryDict = new Dictionary<int, MemoryStackArray>();
#else
        private Dictionary<int, Stack<MemoryBlock>> MemoryDict = new Dictionary<int, Stack<MemoryBlock>>();
#endif

        /// <summary>
        /// 记录分配时的栈
        /// </summary>
        Dictionary<MemoryBlock, StackTrace> MemoryBlockStackTraceDict = new Dictionary<MemoryBlock, StackTrace>();

        //块大小列表，排好序的，主要用于方便查找
        private List<int> BlockSizeList = new List<int>();

        /// <summary>
        /// 用于标记是否在队列中的状态
        /// </summary>
        private Dictionary<MemoryBlock, byte> BlockDict = new Dictionary<MemoryBlock, byte>();

        /// <summary>
        /// Instantiates a TotalBuffer manager.
        /// </summary>
        /// <param name="blockNum">分块数量</param>
        /// <param name="blockSize">分块大小，每一个块的大小，单位字节</param>
        public MemoryManager()
        {
        }

        /// <summary>
        /// 批量添加内存块，大小blockSize，个数blockNum
        /// </summary>
        /// <param name="blockNum"></param>
        /// <param name="blockSize"></param>
        public void AddBatchBlock(int blockNum, int blockSize)
        {
#if MultiMemoryDict
            AddBatchBlock2(blockNum, blockSize);
#else
            Stack<MemoryBlock> blockList = new Stack<MemoryBlock>();
            for (int i = 0; i < blockNum; i++)
            {
                MemoryBlock mb = new MemoryBlock(blockSize, true);
                blockList.Push(mb);
                BlockDict[mb] = 1;
            }

            lock (MemoryDict)
            {
                //加入索引词典
                if (MemoryDict.ContainsKey(blockSize))
                {
                    MemoryDict.Remove(blockSize);
                }

                MemoryDict.Add(blockSize, blockList);
            }

            lock (BlockSizeList)
            {
                BlockSizeList.Add(blockSize);

                //从小到大排序
                BlockSizeList.Sort();
            }
#endif
        }

#if MultiMemoryDict
        
        /// <summary>
        /// 添加内存缓冲块,2.5倍于配置的个数,分成10份,以此减少存取时的锁定时间
        /// </summary>
        /// <param name="blockNum"></param>
        /// <param name="blockSize"></param>
        public void AddBatchBlock2(int blockNum, int blockSize)
        {
            blockNum /= ConstSplitePoolNum / 2;

            lock (MemoryDict)
            {
                //加入索引词典
                if (MemoryDict.ContainsKey(blockSize))
                {
                    MemoryDict.Remove(blockSize);
                }

                MemoryStackArray stackArray = new MemoryStackArray();
                for (int idx = 0; idx < ConstSplitePoolNum; idx++)
                {
                    Stack<MemoryBlock> blockList = new Stack<MemoryBlock>();
                    for (int i = 0; i < blockNum; i++)
                    {
                        MemoryBlock mb = new MemoryBlock(blockSize, true);
                        blockList.Push(mb);
                        BlockDict[mb] = 1;
                    }
                    stackArray.StackList[idx] = blockList;
                }
                MemoryDict.Add(blockSize, stackArray);
            }

            lock (BlockSizeList)
            {
                BlockSizeList.Add(blockSize);

                //从小到大排序
                BlockSizeList.Sort();
            }
        }
#endif
        /// <summary>
        /// 将内存块还回缓存队列
        /// </summary>
        /// <param name="item"></param>
        public void Push(MemoryBlock item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("添加到MemoryManager 的item不能是空(null)");
            }

            if (!item.isManaged)
            {
#if MultiMemoryDict
                Interlocked.Add(ref TotalNewAllocMemorySize, -item.BlockSize); //
#else
                lock (MemoryLock)
                {
                    TotalNewAllocMemorySize -= item.BlockSize;
                }
#endif
                if (GameManager.FlagTraceMemoryPool)
                {
                    lock (MemoryBlockStackTraceDict)
                    {
                        MemoryBlockStackTraceDict[item] = null;
                    }
                }

                //System.Diagnostics.Debug.WriteLine(String.Format("MemoryManager 一个内存块因为isManaged=false 被丢弃 blockSize={0}", item.BlockSize));
                return;
            }

            Stack<MemoryBlock> blockList = null;
#if !MultiMemoryDict
            if (MemoryDict.TryGetValue(item.BlockSize, out blockList))
            {
#else
            MemoryStackArray stackArray;
            if (MemoryDict.TryGetValue(item.BlockSize, out stackArray))
            {
                //stackArray.PushIndex = (++stackArray.PushIndex) % ConstSplitePoolMask;
                int index = Interlocked.Increment(ref stackArray.PushIndex) & ConstSplitePoolMask;
                blockList = stackArray.StackList[index];
#endif
                lock (blockList)
                {
                    //if (!blockList.Contains(item))
                    byte state = 0;
                    BlockDict.TryGetValue(item, out state);
                    if (state <= 0)
                    {
                        blockList.Push(item);
                        BlockDict[item] = 1;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine(String.Format("MemoryManager 错误---尝试还回一个已进存在的内存块 blockList.Count={0}, blockSize={1}", blockList.Count, item.BlockSize));
                    }
                    //System.Diagnostics.Debug.WriteLine(String.Format(" MemoryManager 一个内存块被还原之后 blockList.Count={0}, blockSize={1}", blockList.Count, item.BlockSize));
                }
            }
        }
        /// <summary>
        /// 从缓存队列中提取所需内存对象
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        public MemoryBlock Pop(int needSize)
        {
            Stack<MemoryBlock> blockList = null;
            MemoryBlock item;
#if !MultiMemoryDict
            //先尝试从缓存队列查询
            if (MemoryDict.TryGetValue(GetIndex(needSize), out blockList))
            {
#else
            MemoryStackArray stackArray;
            if (MemoryDict.TryGetValue(GetIndex(needSize), out stackArray))
            {
                //stackArray.PopIndex = (++stackArray.PopIndex) % ConstSplitePoolMask;
                int index = Interlocked.Increment(ref stackArray.PopIndex) & ConstSplitePoolMask;
                blockList = stackArray.StackList[index];
#endif
                lock (blockList)
                {
                    if (blockList.Count > 0)
                    {
                        item = blockList.Pop();
                        BlockDict[item] = 0;

                        //System.Diagnostics.Debug.WriteLine(String.Format("MemoryManager 一个内存块被取出之后 blockList.Count={0}, needSize={1}, blockSize={2}", blockList.Count, needSize, item.BlockSize));
                        return item;
                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine(String.Format("MemoryManager 一个 isManaged=false 的内存块 被创建 needSize={0}", needSize));
            //没有找到适合的大小或者缓存没有了，则直接new 新对象

#if MultiMemoryDict
                Interlocked.Add(ref TotalNewAllocMemorySize, needSize); 
#else
            lock (MemoryLock)
            {
                TotalNewAllocMemorySize += needSize;
            }
#endif

            item = new MemoryBlock(needSize, false);
            if (GameManager.FlagTraceMemoryPool)
            {
                lock (MemoryBlockStackTraceDict)
                {
                    MemoryBlockStackTraceDict[item] = new StackTrace();
                }
            }

            return item;
        }

        /// <summary>
        /// 返回块大小对应的最适合的缓存列表索引
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        private int GetIndex(int needSize)
        {
            int destSize = -1;

            //BlockSizeList键值是从小到大的
            foreach (var nSizeKey in BlockSizeList)
            {
                if (needSize <= nSizeKey)
                {
                    destSize = nSizeKey;
                    break;
                }
            }

            return destSize;
        }

        /// <summary>
        /// 返回发送内存缓存信息
        /// </summary>
        /// <returns></returns>
        public String GetCacheInfoStr()
        {
            StringBuilder bufferTxt = new StringBuilder();

#if MultiMemoryDict
            Dictionary<int, List<int>> memoryInfoDict = new Dictionary<int, List<int>>();
            foreach (var item in MemoryDict)
            {
                int blockSize = item.Key;
                List<int> blockNumList = null;

                foreach (var sk in item.Value.StackList)
                {
                    lock (sk)
                    {
                        if (memoryInfoDict.TryGetValue(blockSize, out blockNumList))
                        {
                            blockNumList.Add(sk.Count);
                        }
                        else
                        {
                            memoryInfoDict[blockSize] = new List<int>() { sk.Count };
                        }
                    }
                }
            }

            //BlockSizeList键值是从小到大的
            foreach (var item in memoryInfoDict)
            {
                int totalCount = 0;
                string countListStr = "";
                //item.Value.ForEach((x) => { totalCount += x; countListStr += " " + x; });
                item.Value.ForEach((x) => { totalCount += x; countListStr = x.ToString(); });
                bufferTxt.AppendFormat(String.Format("大小 {0} bytes 缓存中数量 {1} [{2}]\r\n", item.Key, totalCount, countListStr));
            }
#else
            foreach (var item in MemoryDict)
            {
                bufferTxt.AppendFormat(String.Format("大小 {0} bytes 缓存中数量 {1} 个 \r\n", item.Key, item.Value.Count));
            }
#endif

            bufferTxt.AppendFormat("非缓存分配，正在使用的内存: {0}", GetNewAllocMemorySize());

            return bufferTxt.ToString();
        }

        /// <summary>
        /// 获取泄漏的内存的分配时调用栈
        /// </summary>
        /// <returns></returns>
        public String GetUsedMemoryAllocStackTrace()
        {
            StringBuilder sb = new StringBuilder();
            lock(MemoryBlockStackTraceDict)
            {
                foreach (var kv in MemoryBlockStackTraceDict)
                {
                    if (kv.Value != null)
                    {
                        sb.AppendFormat("BlockSize:{0},StackTrace:{1}\r\n", kv.Key.BlockSize, kv.Value.ToString());
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取新分配的等待使用的内存
        /// </summary>
        /// <returns></returns>
        public static long GetNewAllocMemorySize()
        {
            long memorySize = 0;
            lock (MemoryLock)
            {
                memorySize = TotalNewAllocMemorySize;
            }

            return memorySize;
        }
    }
}
