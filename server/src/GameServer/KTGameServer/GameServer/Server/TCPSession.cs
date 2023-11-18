using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GameServer.Core.Executor;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using Server.TCP;

namespace GameServer.Server
{
    public class TCPSession
    {
        //套接字
        private TMSKSocket _currentSocket = null;

        //指令队列
        private Queue<TCPCmdWrapper> _cmdWrapperQueue = new Queue<TCPCmdWrapper>();

        //对象锁
        private Object _lock = new Object();

        public TCPSession(TMSKSocket socket) 
        {
            this._currentSocket = socket;

            //this._lock = new Object();
            //_cmdWrapperQueue = new Queue<TCPCmdWrapper>();

            //this._SendPacketLock = new Object();
            //_SendPacketWrapperQueue = new Queue<SendPacketWrapper>();
        }

        public void release()
        {
            this._currentSocket = null;

            lock (_cmdWrapperQueue)
            {
                if (null != _cmdWrapperQueue)
                {
                    _cmdWrapperQueue.Clear();
                }
                //_cmdWrapperQueue = null;
            }

            //lock (_SendPacketWrapperQueue)
            //{
            //    if (null != _SendPacketWrapperQueue)
            //    {
            //        _SendPacketWrapperQueue.Clear();
            //    }
            //    _SendPacketWrapperQueue = null;
            //}
        }

        public TMSKSocket CurrentSocket
        {
            get { return _currentSocket; }
        }

        public Object Lock
        {
            get { return _lock; }
        }

        public static int MaxPosCmdNumPer5Seconds = 8;
        
        public static int MaxAntiProcessJiaSuSubTicks = GameManager.GameConfigMgr.GetGameConfigItemInt("maxsubticks", 500);
        public static int MaxAntiProcessJiaSuSubNum = GameManager.GameConfigMgr.GetGameConfigItemInt("maxsubnum", 3);

        //是否是GM帐号（为测试方便，对于内网测试GM，这个值为false）
        public bool IsGM = false;

        public int gmPriority;

        //上次登出时的服务器时间,依据这个值避免不同服务器时间不同，跨服登录可能对逻辑造成损害的情况
        public long LastLogoutServerTicks;

        private int cmdNum = 0;
        private long beginTime = TimeUtil.NOW();

        //0=建立连接，1=login，2=getRoleList,3=initGame，4=startPlayGame, 5=wait
        private long[] _socketTime = { TimeUtil.NOW(), 0, 0, 0, 0, 0 };
        public long[] SocketTime
        {
            get { return _socketTime; }
            set { _socketTime = value; }
        }

        public void SetSocketTime(int index)
        {
            _socketTime[index] = TimeUtil.NOW();
            _socketState = index;
        }

        private int _socketState = 0;
        public int SocketState
        {
            get { return _socketState; }
            set { _socketState = value; }
        }

        //记录最后一次消息id，事件，总消息数量
        private int _cmdID = 0;
        private int _cmdCount = 0;
        private long _cmdTime = 0;

        public int CmdID
        {
            get { return _cmdID; }
            set 
            {
                _cmdID = value;
                _cmdCount++;
            }
        }

        public long CmdTime
        {
            get { return _cmdTime; }
            set { _cmdTime = value; }
        }

        public int CmdCount
        {
            get { return _cmdCount; }
        }

        private int _timeOutCount = 0;
        public int TimeOutCount
        {
            get { return _timeOutCount; }
            set { _timeOutCount = value; }
        }

        public void TimeOutCountAdd()
        {
            _timeOutCount++;
        }

        //外挂解包异常次数
        private int _decryptCount = 0;
        public int DecryptCount
        {
            get { return _decryptCount; }
            set { _decryptCount = value; }
        }

        public void DecryptCountAdd()
        {
            _decryptCount++;
        }

        /// <summary>
        /// 是否在IP白名单中
        /// </summary>
        public bool? InIpWhiteList;

        /// <summary>
        /// 是否在帐号白名单中
        /// </summary>
        public bool? InUseridWhiteList;

        //public long ParsePositionTicks(TCPCmdWrapper wrapper)
        //{
        //    SpritePositionData cmdData = null;

        //    try
        //    {
        //        cmdData = DataHelper.BytesToObject<SpritePositionData>(wrapper.Data, 0, wrapper.Count);
        //    }
        //    catch (Exception) //解析错误
        //    {
        //        return 0;
        //    }

        //    //解析用户名称和用户密码
        //    //string[] fields = cmdData.Split(':');
        //    //if (fields.Length != 5)
        //    if (null == cmdData)
        //    {
        //        return 0;
        //    }

        //    long currentPosTicks = cmdData.currentPosTicks;
        //    return currentPosTicks;
        //}

        public void addTCPCmdWrapper(TCPCmdWrapper wrapper, out int posCmdNum)
        {
            posCmdNum = 0;

            lock (_cmdWrapperQueue)
            {
                _cmdWrapperQueue.Enqueue(wrapper);
            }

            if ((int)TCPGameServerCmds.CMD_SPR_CHECK/*TCPGameServerCmds.CMD_SPR_POSITION*/ == wrapper.NID)
            {
//                 long ticks = ParsePositionTicks(wrapper);
//                 if (ticks <= 0)
//                 {
                    cmdNum++;

                    if ((TimeUtil.NOW() - beginTime) >= TimeUtil.SECOND * 10)
                    {
                        if (cmdNum >= MaxPosCmdNumPer5Seconds)
                        {
                            posCmdNum = cmdNum;
                            cmdNum = 0;
                            beginTime = TimeUtil.NOW();
                        }
                        else
                        {
                            cmdNum = 0;
                            beginTime = TimeUtil.NOW();
                        }
                    }
//                }
            }
        }

        public void CheckCmdNum(int cmdID, out int posCmdNum)
        {
            posCmdNum = 0;
            if ((int)TCPGameServerCmds.CMD_SPR_CHECK/*TCPGameServerCmds.CMD_SPR_POSITION*/ == cmdID)
            {
                cmdNum++;

                if ((TimeUtil.NOW() - beginTime) >= TimeUtil.SECOND * 10)
                {
                    if (cmdNum >= MaxPosCmdNumPer5Seconds)
                    {
                        posCmdNum = cmdNum;
                        cmdNum = 0;
                        beginTime = TimeUtil.NOW();
                    }
                    else
                    {
                        cmdNum = 0;
                        beginTime = TimeUtil.NOW();
                    }
                }
            }
        }

        public TCPCmdWrapper getNextTCPCmdWrapper()
        {
            lock (_cmdWrapperQueue)
            {
                if (_cmdWrapperQueue.Count > 0)
                {
                    return _cmdWrapperQueue.Dequeue();
                }
            }

            return null;
        }

        #region 发送指令缓存队列

        ////指令队列
        //private Queue<SendPacketWrapper> _SendPacketWrapperQueue = null;

        ////对象锁
        //private Object _SendPacketLock = null;

        //public Object SendPacketLock
        //{
        //    get { return _SendPacketLock; }
        //}

        //public void addSendPacketWrapper(SendPacketWrapper wrapper)
        //{
        //    lock (_SendPacketWrapperQueue)
        //    {
        //        _SendPacketWrapperQueue.Enqueue(wrapper);
        //    }           
        //}

        //public SendPacketWrapper getNextSendPacketWrapper()
        //{
        //    lock (_SendPacketWrapperQueue)
        //    {
        //        return _SendPacketWrapperQueue.Dequeue();
        //    }           
        //}

        #endregion 发送指令缓存队列
    }
}
