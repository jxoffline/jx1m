using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Server.TCP
{
    public class FullBufferManager
    {
        /// <summary>
        /// 发送超时
        /// </summary>
        public const int Error_SendTimeOut = 0;

        /// <summary>
        /// 缓冲区满
        /// </summary>
        public const int Error_BufferFull = 1;

        /// <summary>
        /// 缓冲区过半大数据包被丢弃
        /// </summary>
        public const int Error_DiscardBigPacket = 2;

        /// <summary>
        /// 返回错误信息字符串
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static string GetErrorStr(int errorCode)
        {
            string failedReason = "Unknow";

            switch (errorCode)
            {
                case 0:
                    failedReason = "Timeout";
                    break;
                case 1:
                    failedReason = "Buffer is full";
                    break;
                case 2:
                    failedReason = "Over size, auto crop packet";
                    break;
                default:
                    break;
            }

            return failedReason;
        }

        //int值 0表示【上次发送数据】超时， 1表示【上次发送数据不超时但本次缓冲区满】
        private Dictionary<TMSKSocket, int> ErrorDict = new Dictionary<TMSKSocket, int>();

        //错误信息列表，临时存储ErrorDict的值，避免锁ErrorDict
        private List<int> ListError = new List<int>();

        public FullBufferManager()
        {
        }

        /// <summary>
        /// 移除相应套接字的缓存
        /// </summary>
        /// <param name="s"></param>
        public void Remove(TMSKSocket s)
        {
            //不出错的时候ErrorDict.Count都小于等于0
            if (ErrorDict.Count > 0)
            {
                lock (ErrorDict)
                {
                    //if (ErrorDict.ContainsKey(s))
                    //{
                        ErrorDict.Remove(s);
                    //}
                }
            }
        }

        /// <summary>
        /// 添加套接字的缓存
        /// </summary>
        /// <param name="s"></param>
        public void Add(TMSKSocket s, int iError)
        {
            lock (ErrorDict)
            {
                if (!ErrorDict.ContainsKey(s))
                {
                    ErrorDict.Add(s, iError);
                }
                else
                {
                    ErrorDict[s] = iError;
                }
            }
        }

        /// <summary>
        /// 返回发送缓冲区满错误统计信息
        /// </summary>
        /// <returns></returns>
        public String GetFullBufferInfoStr()
        {
            int numTimerOut = 0, numBufferFull = 0, numDiscardPacket = 0,  numOther = 0;

            if (ErrorDict.Count > 0)
            {
                //不用对ErrorDict用锁,没有超时数据，这儿不会被执行
                ListError.Clear();

                lock (ErrorDict)
                {
                    ListError.AddRange(ErrorDict.Values);
                }

                foreach (var item in ListError)
                {
                    switch (item)
                    {
                        case Error_SendTimeOut:
                            numTimerOut++;
                            break;
                        case Error_BufferFull:
                            numBufferFull++;
                            break;
                        case Error_DiscardBigPacket:
                            numDiscardPacket++;
                            break;
                        default:
                            numOther++;
                            break;
                    }
                }
            }

            /**/return String.Format("Timeout: {0} packets, Buffer full: {1} packets, Oversize: {2} packets, Unknow: {3} packets", numTimerOut, numBufferFull, numDiscardPacket, numOther);
        }
    }
}
