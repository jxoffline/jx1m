using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Core.Executor;
using GameServer.Logic;
using System.Threading;
using Server.Tools;
using System.Net.Sockets;

namespace GameServer.Server
{
    /// <summary>
    /// 发送指令管理
    /// </summary>
    public class SendPacketManager : IManager
    {
        private static SendPacketManager instance = new SendPacketManager();

        private SendPacketManager() { }

        public static SendPacketManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// 指令异步执行处理器
        /// </summary>
        //public ScheduleExecutor taskExecutor = null;

        /// <summary>
        /// 加入到发送指令队列
        /// </summary>
        /// <param name="wrapper"></param>
        public void addSendPacketWrapper(SendPacketWrapper wrapper)
        {
            //TCPSession session = null;
            //if (!TCPManager.getInstance().GetTCPSessions().TryGetValue(wrapper.socket, out session))
            //{
            //    return;
            //}

            //session.addSendPacketWrapper(wrapper);
            //taskExecutor.execute(new ProcessSendPacketTask(session));
        }

        public bool initialize()
        {
            //taskExecutor = new ScheduleExecutor(20);
            return true;
        }

        public bool startup()
        {
            //taskExecutor.start();
            return true;
        }

        public bool showdown()
        {
            //taskExecutor.stop();
            return true;
        }

        public bool destroy()
        {
            //taskExecutor = null;
            return true;
        }
    }

    /// <summary>
    /// 发送指令任务
    /// </summary>
    class ProcessSendPacketTask : ScheduleTask
    {
        private TaskInternalLock _InternalLock = new TaskInternalLock();
        public TaskInternalLock InternalLock { get { return _InternalLock; } }

        private TCPSession session = null;

        public ProcessSendPacketTask(TCPSession session)
        {
            this.session = session;
        }

        public void run()
        {
            //SendPacketManager SendPacketManager = SendPacketManager.getInstance();

            ////因为业务层处理时未做同步，暂时锁会话，保证每个玩家的指令处理时线性的
            //if (Monitor.TryEnter(session.SendPacketLock))
            //{
            //    TMSKSocket socket = null;

            //    try
            //    {
            //        SendPacketWrapper wrapper = session.getNextSendPacketWrapper();
            //        if (null != wrapper)
            //        {
            //            try
            //            {
            //                socket = wrapper.socket;

            //                //缓冲数据包
            //                Global._SendBufferManager.AddOutPacket(wrapper.socket, wrapper.tcpOutPacket);

            //                //还回tcpoutpacket
            //                Global._TCPManager.TcpOutPacketPool.Push(wrapper.tcpOutPacket);
            //            }
            //            finally
            //            {
            //                wrapper.Release();
            //                wrapper = null;
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // 格式化异常错误信息
            //        DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            //    }
            //    finally
            //    {
            //        Monitor.Exit(session.SendPacketLock);
            //    }
            //}
            //else
            //{
            //    //如果当session有指令正在处理，把当前指令重新丢进队列，延迟5毫秒处理，防止同一session占用过多线程，保证资源合理利用
            //    SendPacketManager.taskExecutor.scheduleExecute(this, 5);
            //}
        }
    }
}
