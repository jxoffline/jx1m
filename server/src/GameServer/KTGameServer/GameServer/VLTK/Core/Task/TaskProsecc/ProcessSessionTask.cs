using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Core.Executor;
using System.Threading;
using GameServer.Server;
using Server.Tools;
using System.Collections.Concurrent;

namespace GameServer.Logic
{
    /// <summary>
    /// Xử lý sesssion Task
    /// </summary>
    public class ProcessSessionTask : ScheduleTask
    {
        private TaskInternalLock _InternalLock = new TaskInternalLock();
        public TaskInternalLock InternalLock { get { return _InternalLock; } }

        public static long processCmdNum = 0;
        public static long processTotalTime = 0;
        public static ConcurrentDictionary<int, PorcessCmdMoniter> cmdMoniter = new ConcurrentDictionary<int, PorcessCmdMoniter>();
        public static DateTime StartTime = TimeUtil.NowDateTime();

        private TCPSession session = null;
        private long beginTime = 0;

        public ProcessSessionTask(TCPSession session)
        {
            beginTime = TimeUtil.NowEx();
            this.session = session;
        }
        public void run()
        {
            int cmdID = 0;
            long processTime = 0L;
            long processWaitTime = 0L;

            if (Monitor.TryEnter(session.Lock))
            {
                try
                {
                    long processBeginTime = TimeUtil.NowEx();
                    processWaitTime = processBeginTime - beginTime;

                    TCPCmdWrapper wrapper = session.getNextTCPCmdWrapper();
                    if (null != wrapper)
                    {
                        try
                        {
                            TCPCmdHandler.ProcessCmd(wrapper.TcpMgr, wrapper.TMSKSocket, wrapper.TcpClientPool, wrapper.TcpRandKey, wrapper.Pool, wrapper.NID, wrapper.Data, wrapper.Count);
                        }
                        catch (Exception ex)
                        {
                            DataHelper.WriteFormatExceptionLog(ex, string.Format("指令处理错误：{0},{1}", Global.GetDebugHelperInfo(wrapper.TMSKSocket), (TCPGameServerCmds)wrapper.NID), false);
                        }

                        processCmdNum++;
                        processTime = (TimeUtil.NowEx() - processBeginTime);
                        processTotalTime += (processWaitTime + processTime);

                        cmdID = wrapper.NID;

                        wrapper.release();
                        wrapper = null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Monitor.Exit(session.Lock);
                }

                if (cmdID > 0)
                {
                    TCPManager.RecordCmdDetail2(cmdID, processTime, processWaitTime);
                }
            }
            else
            {
                //如果当session有指令正在处理，把当前指令重新丢进队列，延迟5毫秒处理，防止同一session占用过多线程，保证资源合理利用
                TCPManager.getInstance().taskExecutor.scheduleExecute(this, 5);
            }
        }

    }
}
