using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract;
using KF.Contract.Interface;
using Server.Tools;
using Tmsk.Contract;
using KF.Contract.Data;
using KF.Remoting.Data;

namespace KF.Remoting
{
    class ClientAgentManager
    {
        #region Singleton
        private static ClientAgentManager _AgentMgr = new ClientAgentManager();
        public static ClientAgentManager Instance()
        {
            return _AgentMgr;
        }
        #endregion

        private object Mutex = new object();

        /// <summary>
        /// ServerId,Agent
        /// </summary>
        private Dictionary<int, ClientAgent> ServerId2ClientAgent = new Dictionary<int, ClientAgent>();

        /// <summary>
        /// 所有的跨服服务器id
        /// </summary>
        private HashSet<int> AllKfServerId = new HashSet<int>();

        /// <summary>
        /// key : game types
        /// value: 统计信息
        /// 如果不设置，那么该gametype按默认算法统计
        /// </summary>
        private Dictionary<int, GameTypeStaticsData> GameTypeLoadDict = new Dictionary<int, GameTypeStaticsData>();

        private ClientAgentManager()
        {

        }

        #region 服务器管理
        /// <summary>
        /// 检测agent是否存活
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool IsAgentAlive(int serverId)
        {
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(serverId, out agent)
                    && agent.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在serverid的Agent
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool ExistAgent(int serverId)
        {
            lock (Mutex)
            {
                return ServerId2ClientAgent.ContainsKey(serverId);
            }
        }

        /// <summary>
        /// 是否有任意一个跨服服务器存活
        /// </summary>
        /// <returns></returns>
        public bool IsAnyKfAgentAlive()
        {
            lock (Mutex)
            {
                foreach (var sid in AllKfServerId)
                {
                    if (IsAgentAlive(sid))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否是跨服agent
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool IsKfAgent(int serverId)
        {
            lock (Mutex)
            {
                return AllKfServerId.Contains(serverId) && ExistAgent(serverId);
            }
        }

        /// <summary>
        /// 设置目前配置的所有跨服服务器id
        /// </summary>
        /// <param name="existKfIds"></param>
        public void SetAllKfServerId(HashSet<int> existKfIds)
        {
            lock (Mutex)
            {
                this.AllKfServerId = new HashSet<int>(existKfIds);
            }
        }

        /// <summary>
        /// 初始化GameServer代理
        /// 同一个GameServer的Service与ClientAgent是一个N:1的关系
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="clientInfo"></param>
        /// <param name="bFistInit">是否是首次初始化该client</param>
        /// <returns></returns>
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo, out bool bFistInit)
        {
            bFistInit = false;
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (!ServerId2ClientAgent.TryGetValue(clientInfo.ServerId, out agent)
                    || !agent.IsAlive)
                {
                    // clientInfo.ServerId 上的任意service都未连接过来，或者已连接，但是agent已超时死亡，需要重设agent
                    LogManager.WriteLog(LogTypes.Info,
                        string.Format("InitializeClient服务器连接1.ServerId:{0},ClientId:{1},info:{2},GameType:{3} [Service首次连接过来]",
                        clientInfo.ServerId, clientInfo.ClientId, clientInfo.Token, (GameTypes)clientInfo.GameType));
                    bFistInit = true;
                    clientInfo.ClientId = KuaFuServerManager.GetUniqueClientId();
                    agent = new ClientAgent(clientInfo, callback);
                    ServerId2ClientAgent[clientInfo.ServerId] = agent;
                }
                else
                {
                    // clientInfo.ServerId 上的至少有一个service已经连接过来了

                    // token 不一致，重复 ServerId重复！！！
                    if (clientInfo.Token != agent.ClientInfo.Token)
                    {
                        LogManager.WriteLog(LogTypes.Info,
                            string.Format("InitializeClient服务器ID重复,禁止连接.ServerId:{0},ClientId:{1},info:{2},GameType:{3}",
                            clientInfo.ServerId, clientInfo.ClientId, clientInfo.Token, (GameTypes)clientInfo.GameType));
                        return StdErrorCode.Error_Duplicate_Key_ServerId;
                    }

                    // clientInfo.ServerId 这个GameType的Service首次连接过来
                    if (clientInfo.ClientId <= 0)
                    {
                        bFistInit = true;
                        LogManager.WriteLog(LogTypes.Info,
                            string.Format("InitializeClient服务器连接2.ServerId:{0},ClientId:{1},info:{2},GameType:{3} [Service非首次连接过来]",
                            clientInfo.ServerId, clientInfo.ClientId, clientInfo.Token, (GameTypes)clientInfo.GameType));
                    }
                    else if (clientInfo.ClientId != agent.ClientInfo.ClientId)
                    {
                        // GameServer不重启，中心重启后，从第二个连过来的service开始会走到这里
                        // tcp的虚拟连接能够使Gameserver感知不到中心的重启，这里需要修改clientInfo.ClientId
                        bFistInit = true;
                        LogManager.WriteLog(LogTypes.Info,
                            string.Format("InitializeClient服务器连接3.ServerId:{0},ClientId:{1},info:{2},GameType:{3} [中心重启]",
                            clientInfo.ServerId, clientInfo.ClientId, clientInfo.Token, (GameTypes)clientInfo.GameType));
                    }
                }

                if (agent != null)
                {
                    clientInfo.ClientId = agent.ClientInfo.ClientId;
                    agent.ClientHeartTick();
                    agent.TryInitGameType(clientInfo.GameType);
                }

                return clientInfo.ClientId;
            }
        }

        /// <summary>
        /// 初始化GameServer代理
        /// 同一个GameServer的Service与ClientAgent是一个N:1的关系
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo)
        {
            bool bPlaceHolder = false;
            return InitializeClient(callback, clientInfo, out bPlaceHolder);
        }

        #endregion

        #region 异步事件管理
        /// <summary>
        /// 广播异步时间给所有的GameServer
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="evItems"></param>
        public void BroadCastAsyncEvent(GameTypes gameType, AsyncDataItem[] evItems)
        {
            if (evItems == null || evItems.Length <= 0) return;

            lock (Mutex)
            {
                for (int i = 0; i < evItems.Length; i++)
                {
                    BroadCastAsyncEvent(gameType, evItems[i]);
                }
            }
        }


        /// <summary>
        /// 广播异步时间给所有的GameServer
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="evItem"></param>
        public void BroadCastAsyncEvent(GameTypes gameType, AsyncDataItem evItem)
        {
            if (evItem == null) return;

            lock (Mutex)
            {
                foreach (var sid in ServerId2ClientAgent.Keys)
                {
                    PostAsyncEvent(sid, gameType, evItem);
                }
            }
        }

        /// <summary>
        /// 广播异步时间给所有的跨服GameServer
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="evItem"></param>
        public void KFBroadCastAsyncEvent(GameTypes gameType, AsyncDataItem evItem)
        {
            if (evItem == null) return;

            lock (Mutex)
            {
                foreach (var sid in AllKfServerId)
                {
                    PostAsyncEvent(sid, gameType, evItem);
                }
            }
        }

        /// <summary>
        /// 给特定GameServer投递异步事件
        /// </summary>
        /// <param name="ServerId"></param>
        /// <param name="gameType"></param>
        /// <param name="evItem"></param>
        public void PostAsyncEvent(int ServerId, GameTypes gameType, AsyncDataItem evItem)
        {
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(ServerId, out agent))
                {
                    agent.PostAsyncEvent(gameType, evItem);
                }
            }
        }

        /// <summary>
        /// 取出特定GameServer的异步事件
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="gameType"></param>
        /// <returns></returns>
        public AsyncDataItem[] PickAsyncEvent(int serverId, GameTypes gameType)
        {
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(serverId, out agent))
                {
                    return agent.PickAsyncEvent(gameType);
                }
            }

            return null;
        }
        #endregion

        #region 活动分配管理
        public bool AssginKfFuben(GameTypes gameType, long gameId, int roleNum, out int kfSrvId)
        {
            kfSrvId = 0;

            lock (Mutex)
            {
                long payload = long.MaxValue;
                ClientAgent assignAgent = null;

                foreach (var sid in AllKfServerId)
                {
                    ClientAgent agent = null;
                    if (ServerId2ClientAgent.TryGetValue(sid, out agent)
                        && agent.IsAlive
                        && agent.TotalRolePayload < payload)
                    {
                        payload = agent.TotalRolePayload;
                        assignAgent = agent;
                    }
                }

                if (assignAgent != null)
                {
                    assignAgent.AssginKfFuben(gameType, gameId, roleNum);
                    kfSrvId = assignAgent.ClientInfo.ServerId;
                    return true;
                }
            }

            return false;
        }

        public void RemoveKfFuben(GameTypes gameType, int kfSrvId, long gameId)
        {
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(kfSrvId, out agent))
                {
                    agent.RemoveKfFuben(gameType, gameId);
                }
            }
        }
        #endregion

        #region 服务器状态，负载统计
        public void SetMainlinePayload(int serverId, int payload)
        {
            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(serverId, out agent))
                {
                    agent.SetMainlinePayload(payload);
                }
            }
        }

        public void SetGameTypeLoad(GameTypes gameType, int signUpCount, int startCount)
        {
            lock (Mutex)
            {
                GameTypeStaticsData data = null;
                if (!GameTypeLoadDict.TryGetValue((int)gameType, out data))
                {
                    data = new GameTypeStaticsData();
                    GameTypeLoadDict[(int)gameType] = data;
                }

                data.SingUpRoleCount = signUpCount;
                data.StartGameRoleCount = startCount;
            }
        }

        public void GetServerState(int serverId, out int state, out int load)
        {
            state = 0;
            load = 0;

            lock (Mutex)
            {
                ClientAgent agent = null;
                if (ServerId2ClientAgent.TryGetValue(serverId, out agent))
                {
                    if (agent.IsAlive)
                    {
                        state = 1;
                        load = (int)agent.TotalRolePayload;
                    }
                }
            }
        }

        public Dictionary<int, GameTypeStaticsData> GetGameTypeStatics()
        {
            var result = new Dictionary<int, GameTypeStaticsData>();
            lock (Mutex)
            {
                foreach (var serverId in AllKfServerId)
                {
                    ClientAgent agent = null;
                    if (ServerId2ClientAgent.TryGetValue(serverId, out agent))
                    {
                        foreach (var tmpKvp in agent.GetGameTypeStatics())
                        {
                            GameTypeStaticsData existData = null;
                            if (!result.TryGetValue(tmpKvp.Key, out existData))
                            {
                                existData = new GameTypeStaticsData();
                                result[tmpKvp.Key] = existData;
                            }

                            existData.ServerAlived += tmpKvp.Value.ServerAlived;
                            existData.FuBenAlived += tmpKvp.Value.FuBenAlived;
                            existData.SingUpRoleCount += tmpKvp.Value.SingUpRoleCount;
                            existData.StartGameRoleCount += tmpKvp.Value.StartGameRoleCount;
                        }
                    }
                }

                foreach (var kvp in GameTypeLoadDict)
                {
                    if (result.ContainsKey(kvp.Key))
                    {
                        result[kvp.Key].SingUpRoleCount = kvp.Value.SingUpRoleCount;
                        result[kvp.Key].StartGameRoleCount = kvp.Value.StartGameRoleCount;
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
