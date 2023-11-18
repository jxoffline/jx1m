using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Server;

namespace GameServer.Logic
{
    public class PorcessCmdMoniter
    {
        public int cmd;
        public int processNum = 0;
        public long processTotalTime = 0;
        public long processMaxTime = 0;
        public long waitProcessTotalTime = 0;
        public long maxWaitProcessTime = 0;
        public long TotalBytes = 0;

        public long SendNum = 0;
        public long OutPutBytes = 0;

        public long Num_Faild;
        public long Num_OK;
        public long Num_WithData;

        public PorcessCmdMoniter(int cmd, long processTime)
        {
            this.cmd = cmd;
            processNum++;
            processTotalTime += processTime;
            processMaxTime = processTime;
        }

        public void Reset()
        {
            lock (this)
            {
                maxWaitProcessTime = 0;
                processMaxTime = 0;
                processNum = 0;
                processTotalTime = 0;
                waitProcessTotalTime = 0;
                TotalBytes = 0;
                SendNum = 0;
                OutPutBytes = 0;
            }
        }

        /// <summary>
        /// 使用指令队列时的统计
        /// </summary>
        /// <param name="processTime"></param>
        /// <param name="waitTime"></param>
        public void onProcess(long processTime, long waitTime)
        {
            lock (this)
            {
                processNum++;
                processTotalTime += processTime;
                processMaxTime = processMaxTime >= processTime ? processMaxTime : processTime;
                waitProcessTotalTime += waitTime;
                maxWaitProcessTime = maxWaitProcessTime >= waitTime ? maxWaitProcessTime : waitTime;
            }
        }

        public long avgWaitProcessTime()
        {
            return processNum > 0 ? waitProcessTotalTime / processNum : 0;
        }

        /// <summary>
        /// IO线程直接处理的统计，包括处理时间和字节数，处理结果
        /// </summary>
        /// <param name="processTime"></param>
        /// <param name="dataSize"></param>
        /// <param name="result"></param>
        public void onProcessNoWait(long processTime, long dataSize, TCPProcessCmdResults result)
        {
            lock (this)
            {
                processNum++;
                processTotalTime += processTime;
                TotalBytes += dataSize;
                if (processMaxTime >= processTime)
                {
                    processMaxTime = processTime;
                }
                switch (result)
                {
                    case TCPProcessCmdResults.RESULT_FAILED:
                        Num_Faild++;
                        break;
                    case TCPProcessCmdResults.RESULT_OK:
                        Num_OK++;
                        break;
                    case TCPProcessCmdResults.RESULT_DATA:
                        Num_WithData++;
                        break;
                }
            }
        }

        public void OnOutputData(long dataSize)
        {
            lock (this)
            {
                SendNum++;
                OutPutBytes += dataSize;
            }
        }

        public long avgProcessTime()
        {
            int num = processNum;
            if (num > 0)
            {
                return processTotalTime / num;
            }

            return 0;
        }

        public long GetTotalBytes()
        {
            return TotalBytes;
        }
    }
}
