using GameServer.Logic;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;

using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime; 

namespace GameServer.Tools
{
    public class TMSKThreadStaticClass
    {
        /// <summary>
        /// 线程本地存储
        /// </summary>
        [ThreadStatic]
        private static TMSKThreadStaticClass ThreadStaticClass = null;

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
            //lock (GameManager.MemoryPoolConfigDict)
            {
                foreach (var kv in GameManager.MemoryPoolConfigDict)
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
                    Global._MemoryManager.Push(stack.Pop());

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
                DataHelper.WriteExceptionLogEx(ex, "");
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
                DataHelper.WriteExceptionLogEx(ex, "");
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
                        Global._MemoryManager.Push(item);
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
                        Global._MemoryManager.Push(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
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
                        return Global._MemoryManager.Pop(needSize);
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
                        return Global._MemoryManager.Pop((int)Math.Pow(2, index));
                    }
                }
            }
            catch (System.Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, "");
            }
            return null;
        }

        #endregion 内存池
    }

    /// <summary> 
    /// 网络时间 
    /// </summary> 
    public class NetTMSK_KY
    {
        public static object TMSK1 = null;

        public NetTMSK_KY()
        {
            this.GetF();
        }
        /// <summary> 
        /// 获取标准北京时间，读取http://www.beijing-time.org/time.asp 
        /// </summary> 
        /// <returns>返回网络时间</returns> 
        public DateTime GetF()
        {
            DateTime dt = DateTime.Now;
          
             NetTMSK_KY.TMSK1 = dt.Ticks / 10000;
        
            return dt;
        }
        public DateTime get(string url)
        {
            HttpWebRequest request = null;
            request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.Proxy = null;
            string html = string.Empty;
            using (Stream stream = request.GetResponse().GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    html = sr.ReadToEnd();
                }
            }
            Hashtable obj = (Hashtable)MUJson.jsonDecode(html);
            if (obj == null)
            {
                return DateTime.Parse(Encoding.UTF8.GetString(Convert.FromBase64String("MjAxMS0xLTE=")));
            }
            else
            {
                string timeStamp = obj["stime"].ToString();
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow);
            }
        }
    }


}
