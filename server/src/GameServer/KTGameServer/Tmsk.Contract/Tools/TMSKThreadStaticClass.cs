using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server.Tools;
using System.Net.Sockets;
using Server.TCP;
using Tmsk.Tools;

namespace Tmsk.Tools
{
    /// <summary>
    /// 将内存块还回缓存队列
    /// </summary>
    /// <param name="item"></param>
    public delegate void PushMemoryBlock(MemoryBlock item);

    /// <summary>
    /// 从缓存队列中提取所需内存对象
    /// </summary>
    /// <param name="needSize"></param>
    /// <returns></returns>
    public delegate MemoryBlock PopMemoryBlock(int needSize);

    public class TMSKThreadStaticClass
    {
        /// <summary>
        /// 将内存块还回缓存队列
        /// </summary>
        public PushMemoryBlock PushMemoryBlockHandler;

        /// <summary>
        /// 从缓存队列中提取所需内存对象
        /// </summary>
        public PopMemoryBlock PopMemoryBlockHandler;

        /// <summary>
        /// 线程本地存储
        /// </summary>
        [ThreadStatic]
        private static TMSKThreadStaticClass ThreadStaticClass = null;

        /// <summary>
        /// 内存池配置
        /// </summary>
        public static Dictionary<int, int> MemoryPoolConfigDict = new Dictionary<int, int>();

        private const int QueueMemoryStreamMaxSize = 30;
        private Queue<MemoryStream> QueueMemoryStream = new Queue<MemoryStream>();

        public static TMSKThreadStaticClass GetInstance()
        {
            if (null == ThreadStaticClass)
            {
                ThreadStaticClass = new TMSKThreadStaticClass();
            }

            return ThreadStaticClass;
        }

        #region 构造和析构函数

        public TMSKThreadStaticClass()
        {
            lock (MemoryPoolConfigDict)
            {
                foreach (var kv in MemoryPoolConfigDict)
                {
                    int index = Log2(kv.Key);
                    if (index < MemoryBlockNumArray.Length)
                    {
                        MemoryBlockStackArray[index] = new Stack<MemoryBlock>();
                        MemoryBlockNumArray[index] = Math.Max(10, kv.Value / 100);
                        MemoryBlockSizeArray[index] = kv.Key;
                    }
                }

                Stack<MemoryBlock> lastStackMemoryBlock = null;
                int lastMemoryBlockNum = 10;
                int lastMemoryBlockSize = 0;
                for (int i = MemoryBlockStackArray.Length - 1; i >= 0; i-- )
                {
                    if (null == MemoryBlockStackArray[i])
                    {
                        if (null == lastStackMemoryBlock)
                        {
                            MemoryBlockStackArray[i] = new Stack<MemoryBlock>();
                            MemoryBlockNumArray[i] = 10;
                            MemoryBlockSizeArray[i] = 0;
                        }
                        else
                        {
                            MemoryBlockStackArray[i] = lastStackMemoryBlock;
                            MemoryBlockNumArray[i] = lastMemoryBlockNum;
                            MemoryBlockSizeArray[i] = lastMemoryBlockSize;
                        }
                    }
                    else
                    {
                        lastStackMemoryBlock = MemoryBlockStackArray[i];
                        lastMemoryBlockNum = MemoryBlockNumArray[i];
                        lastMemoryBlockSize = MemoryBlockSizeArray[i];
                    }
                }
            }
        }

        ~TMSKThreadStaticClass()
        {
            for (int i = 0; i < QueueMemoryStream.Count; i++)
            {
                MemoryStream ms = QueueMemoryStream.Dequeue();
                ms.Dispose();
            }

            foreach (var stack in MemoryBlockStackArray)
            {
                while (stack.Count > 0)
                {
                    if (null != PushMemoryBlockHandler)
                    {
                        PushMemoryBlockHandler(stack.Pop());
                    }
                }
            }
        }

        #endregion 构造和析构函数

        #region StreamPool

        public void PushMemoryStream(MemoryStream ms)
        {
            try
            {
                if (QueueMemoryStream.Count <= QueueMemoryStreamMaxSize)
                {
                    QueueMemoryStream.Enqueue(ms);
                }
            }
            catch (System.Exception ex)
            {
                DataHelper2.WriteExceptionLogEx(ex, "");
            }
        }

        public MemoryStream PopMemoryStream()
        {
            try
            {
                if (QueueMemoryStream.Count > 0)
                {
                    MemoryStream ms = QueueMemoryStream.Dequeue();
                    ms.Position = 0;
                    ms.SetLength(0);
                    return ms;
                }
            }
            catch (System.Exception ex)
            {
                DataHelper2.WriteExceptionLogEx(ex, "");
            }

            return new MemoryStream();
        }


        #endregion StreamPool

        #region 内存池

        Stack<MemoryBlock>[] MemoryBlockStackArray = new Stack<MemoryBlock>[20];
        int[] MemoryBlockNumArray = new int[20];
        int[] MemoryBlockSizeArray = new int[20];

        /// <summary>
        /// 返回N,保证2的N次幂不小于size的.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int Log2(int size)
        {
            return (int)Math.Ceiling(Math.Log(size, 2));
        }

        /// <summary>
        /// 将内存块还回缓存队列
        /// </summary>
        /// <param name="item"></param>
        public void PushMemoryBlock(MemoryBlock item)
        {
            try
            {
                int index = Log2(item.BlockSize);
                int blockSize = MemoryBlockSizeArray[index];
                if (blockSize > 0)
                {
                    if (blockSize < item.BlockSize)
                    {
                        index++;
                    }
                    if (MemoryBlockStackArray[index].Count <= MemoryBlockNumArray[index])
                    {
                        MemoryBlockStackArray[index].Push(item);
                    }
                    else
                    {
                        if (null != PushMemoryBlockHandler)
                        {
                            PushMemoryBlockHandler(item);
                        }
                    }
                }
                else
                {
                    if (MemoryBlockStackArray[index].Count <= MemoryBlockNumArray[index])
                    {
                        MemoryBlockStackArray[index].Push(item);
                    }
                    else
                    {
                        if (null != PushMemoryBlockHandler)
                        {
                            PushMemoryBlockHandler(item);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                DataHelper2.WriteExceptionLogEx(ex, "");
            }
        }

        /// <summary>
        /// 从缓存队列中提取所需内存对象
        /// </summary>
        /// <param name="needSize"></param>
        /// <returns></returns>
        public MemoryBlock PopMemoryBlock(int needSize)
        {
            try
            {
                int index = Log2(needSize);
                int blockSize = MemoryBlockSizeArray[index];
                if (blockSize > 0)
                {
                    if (blockSize < needSize)
                    {
                        index++;
                    }
                    if (MemoryBlockStackArray[index].Count > 0)
                    {
                        return MemoryBlockStackArray[index].Pop();
                    }
                    else
                    {
                        if (null != PopMemoryBlockHandler)
                        {
                            return PopMemoryBlockHandler(needSize);
                        }

                        return null;
                    }
                }
                else
                {
                    if (MemoryBlockStackArray[index].Count > 0)
                    {
                        return MemoryBlockStackArray[index].Pop();
                    }
                    else
                    {
                        if (null != PopMemoryBlockHandler)
                        {
                            return PopMemoryBlockHandler(needSize);
                        }

                        return null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                DataHelper2.WriteExceptionLogEx(ex, "");
            }
            return null;
        }

        #endregion 内存池
    }


}
