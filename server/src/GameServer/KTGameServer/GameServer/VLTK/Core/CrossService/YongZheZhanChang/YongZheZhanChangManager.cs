using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using GameServer.Core.GameEvent.EventOjectImpl;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using KF.Client;
using KF.Contract.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Tmsk.Contract;

namespace GameServer.Logic
{
    /// <summary>
    /// 跨服天梯管理
    /// </summary>
    public partial class YongZheZhanChangManager : IManager, ICmdProcessorEx, IEventListener, IEventListenerEx, IManager2
    {
        #region 标准接口

        public const SceneUIClasses ManagerType = SceneUIClasses.YongZheZhanChang;

        private static YongZheZhanChangManager instance = new YongZheZhanChangManager();

        public static YongZheZhanChangManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// 配置和运行时数据
        /// </summary>
        public YongZheZhanChangData RuntimeData = new YongZheZhanChangData();

        public bool initialize()
        {
            //if (!InitConfig())
            //{
            //    return false;
            //}

            return true;
        }

        public bool initialize(ICoreInterface coreInterface)
        {
            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("YongZheZhanChangManager.TimerProc", TimerProc), 15000, 5000);
            return true;
        }

        public bool startup()
        {
            //注册指令处理器
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_JOIN, 1, 1, getInstance());
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_ENTER, 1, 1, getInstance());
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_STATE, 1, 1, getInstance());
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_AWARD_GET, 1, 1, getInstance());
            TCPCmdDispatcher.getInstance().registerProcessorEx((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_AWARD, 1, 1, getInstance());

            //向事件源注册监听器
            GlobalEventSource4Scene.getInstance().registerListener((int)GlobalEventTypes.KuaFuNotifyEnterGame, (int)SceneUIClasses.YongZheZhanChang, getInstance());
            GlobalEventSource4Scene.getInstance().registerListener((int)GlobalEventTypes.PlayerCaiJi, (int)SceneUIClasses.YongZheZhanChang, getInstance());
            GlobalEventSource.getInstance().registerListener((int)EventTypes.PlayerDead, getInstance());

            return true;
        }

        public bool showdown()
        {
            //向事件源删除监听器
            GlobalEventSource4Scene.getInstance().removeListener((int)GlobalEventTypes.KuaFuNotifyEnterGame, (int)SceneUIClasses.YongZheZhanChang, getInstance());
            GlobalEventSource4Scene.getInstance().removeListener((int)GlobalEventTypes.PlayerCaiJi, (int)SceneUIClasses.YongZheZhanChang, getInstance());
            GlobalEventSource.getInstance().removeListener((int)EventTypes.PlayerDead, getInstance());

            return true;
        }

        public bool destroy()
        {
            return true;
        }

        public bool processCmd(KPlayer client, string[] cmdParams)
        {
            return false;
        }

        public bool processCmdEx(KPlayer client, int nID, byte[] bytes, string[] cmdParams)
        {
            switch (nID)
            {
            }

            return true;
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventObject"></param>
        public void processEvent(EventObject eventObject)
        {
            int eventType = eventObject.getEventType();
            if (eventType == (int)EventTypes.PlayerDead)
            {
                PlayerDeadEventObject playerDeadEvent = eventObject as PlayerDeadEventObject;
                if (null != playerDeadEvent)
                {
                    if (playerDeadEvent.Type == PlayerDeadEventTypes.ByRole)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventObject"></param>
        public void processEvent(EventObjectEx eventObject)
        {
            int eventType = eventObject.EventType;
            switch (eventType)
            {
                case (int)GlobalEventTypes.KuaFuNotifyEnterGame:
                    {
                        KuaFuNotifyEnterGameEvent e = eventObject as KuaFuNotifyEnterGameEvent;
                        if (null != e)
                        {
                            KuaFuServerLoginData kuaFuServerLoginData = e.Arg as KuaFuServerLoginData;
                            if (null != kuaFuServerLoginData)
                            {
                                lock (RuntimeData.Mutex)
                                {
                                    RuntimeData.RoleIdKuaFuLoginDataDict[kuaFuServerLoginData.RoleId] = kuaFuServerLoginData;
                                    LogManager.WriteLog(LogTypes.Error, string.Format("通知角色ID={0}拥有进入勇者战场资格,跨服GameID={1}", kuaFuServerLoginData.RoleId, kuaFuServerLoginData.GameId));
                                }
                            }

                            eventObject.Handled = true;
                        }
                    }
                    break;

                case (int)GlobalEventTypes.PlayerCaiJi:
                    {
                    }
                    break;
            }
        }

        #endregion 标准接口

        #region 初始化配置

        private void TimerProc(object sender, EventArgs e)
        {
            bool notifyPrepareGame = false;
            bool notifyEnterGame = false;
            DateTime now = TimeUtil.NowDateTime();
            lock (RuntimeData.Mutex)
            {
                bool bInActiveTime = false;

                if (!bInActiveTime)
                {
                    if (RuntimeData.RoleIdKuaFuLoginDataDict.Count > 0)
                    {
                        RuntimeData.RoleIdKuaFuLoginDataDict.Clear();
                    }

                    if (RuntimeData.RoleId2JoinGroup.Count > 0)
                    {
                        RuntimeData.RoleId2JoinGroup.Clear();
                    }
                }
            }

            if (notifyPrepareGame)
            {
                LogManager.WriteLog(LogTypes.Error, "通知跨服中心开始分配所有报名玩家的活动场次");

                // GameServer和KF-GameServer都会通知准备游戏，所以中心要防止状态回滚
                string cmd = string.Format("{0} {1} {2}", GameStates.CommandName, GameStates.PrepareGame, (int)GameTypes.YongZheZhanChang);
                YongZheZhanChangClient.getInstance().ExecuteCommand(cmd);
            }

            if (notifyEnterGame)
            {
                lock (RuntimeData.Mutex)
                {
                    foreach (var kuaFuServerLoginData in RuntimeData.RoleIdKuaFuLoginDataDict.Values)
                    {
                        RuntimeData.NotifyRoleEnterDict.Add(kuaFuServerLoginData.RoleId, kuaFuServerLoginData);
                    }
                }
            }

            //通知报名的玩家进入活动,每次只通知一部分(按RoleID除以15的余数),防止所有玩家一起进入给服务器造成压力.
            List<KuaFuServerLoginData> list = null;
            lock (RuntimeData.Mutex)
            {
                int count = RuntimeData.NotifyRoleEnterDict.Count;
                if (count > 0)
                {
                    list = new List<KuaFuServerLoginData>();
                    KuaFuServerLoginData kuaFuServerLoginData = RuntimeData.NotifyRoleEnterDict.First().Value;
                    foreach (var kv in RuntimeData.NotifyRoleEnterDict)
                    {
                        if ((kv.Key % 15) == (kuaFuServerLoginData.RoleId % 15))
                        {
                            list.Add(kv.Value);
                        }
                    }

                    foreach (var data in list)
                    {
                        RuntimeData.NotifyRoleEnterDict.Remove(data.RoleId);
                    }
                }
            }

            if (null != list)
            {
                foreach (var kuaFuServerLoginData in list)
                {
                    KPlayer client = KTPlayerManager.Find(kuaFuServerLoginData.RoleId);
                    if (null != client)
                    {
                        client.SendPacket((int)TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_ENTER, 1);
                    }
                }
            }
        }

        #endregion 初始化配置

        public bool OnInitGame(KPlayer client)
        {
            int posX;
            int posY;
            int side;

            KuaFuServerLoginData kuaFuServerLoginData = Global.GetClientKuaFuServerLoginData(client);
            YongZheZhanChangFuBenData fuBenData;
            lock (RuntimeData.Mutex)
            {
                if (!RuntimeData.FuBenItemData.TryGetValue((int)kuaFuServerLoginData.GameId, out fuBenData))
                {
                    fuBenData = null;
                }
                else if (fuBenData.State >= GameFuBenState.End)
                {
                    return false;
                }
            }

            if (null == fuBenData)
            {
                //从中心查询副本信息
                YongZheZhanChangFuBenData newFuBenData = YongZheZhanChangClient.getInstance().GetKuaFuFuBenData((int)kuaFuServerLoginData.GameId);
                if (newFuBenData == null || newFuBenData.State == GameFuBenState.End)
                {
                    LogManager.WriteLog(LogTypes.Error, "获取不到有效的副本数据," + newFuBenData == null ? "fuBenData == null" : "fuBenData.State == GameFuBenState.End");
                    return false;
                }

                lock (RuntimeData.Mutex)
                {
                    if (!RuntimeData.FuBenItemData.TryGetValue((int)kuaFuServerLoginData.GameId, out fuBenData))
                    {
                        fuBenData = newFuBenData;
                        fuBenData.SequenceId = GameCoreInterface.getinstance().GetNewFuBenSeqId();
                        RuntimeData.FuBenItemData[fuBenData.GameId] = fuBenData;
                    }
                }
            }

            KuaFuFuBenRoleData kuaFuFuBenRoleData;
            if (!fuBenData.RoleDict.TryGetValue(client.RoleID, out kuaFuFuBenRoleData))
            {
                return false;
            }

            YongZheZhanChangSceneInfo sceneInfo;
            lock (RuntimeData.Mutex)
            {
                kuaFuServerLoginData.FuBenSeqId = fuBenData.SequenceId;
                if (!RuntimeData.SceneDataDict.TryGetValue(fuBenData.GroupIndex, out sceneInfo))
                {
                    return false;
                }

                client.MapCode = sceneInfo.MapCode;
            }

            client.PosX = 0;
            client.PosY = 0;

            return true;
        }
    }
}