using KF.Contract;
using KF.Contract.Data;
using KF.Contract.Interface;
using KF.Remoting.Data;
using System.Collections.Generic;
using System.Linq;

namespace KF.Remoting
{
    /// <summary>
    /// 各个Service不要直接调用ClientAgent，而是应该使用ClientAgentManager解耦
    /// </summary>
    internal sealed class ClientAgent
    {
        private object Mutex = new object();

        public KuaFuClientContext ClientInfo { get; private set; }
        public IKuaFuClient KuaFuClient { get; private set; }

        /// <summary>
        /// 视为无心跳的时间
        /// </summary>
        private long MaxActiveTicks = 0;

        /// <summary>
        /// 长时间无心跳，可能不会再连接上的时间
        /// </summary>
        private long MaxDeadTicks = 0;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive
        { get { return MaxActiveTicks > Global.NowTime.Ticks; } }

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsDead
        { get { return MaxDeadTicks < Global.NowTime.Ticks; } }

        /// <summary>
        /// //最多缓存100000条
        /// </summary>
        private const int MaxCachedAsyncDataItemCount = 100000;

        /// <summary>
        /// 按GameTypes发布的异步消息
        /// key: GameTypes
        /// val: async events
        /// </summary>
        private Dictionary<int, Queue<AsyncDataItem>> EvItemOfGameType = new Dictionary<int, Queue<AsyncDataItem>>();

        /// <summary>
        /// key: gameType
        /// value: {key: gameId, value: rolenum}
        /// </summary>
        private Dictionary<int, Dictionary<long, int>> AlivedGameDict = new Dictionary<int, Dictionary<long, int>>();

        /// <summary>
        /// 总人数负载
        /// </summary>
        public long TotalRolePayload
        { get { return TotalFubenRolePayLoad + TotalMainlineRolePayLoad; } }

        /// <summary>
        /// 总副本人数负载
        /// </summary>
        public long TotalFubenRolePayLoad { get; private set; }

        /// <summary>
        /// 总跨服主线人数负载
        /// </summary>
        public long TotalMainlineRolePayLoad { get; private set; }

        public ClientAgent(KuaFuClientContext clientInfo, IKuaFuClient callback)
        {
            ClientInfo = clientInfo;
            KuaFuClient = callback;
            ClientHeartTick();
        }

        /// <summary>
        /// key: gameType
        /// value: statics data
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, GameTypeStaticsData> GetGameTypeStatics()
        {
            var result = new Dictionary<int, GameTypeStaticsData>();

            if (IsAlive)
            {
                lock (Mutex)
                {
                    foreach (var kvp in AlivedGameDict)
                    {
                        GameTypeStaticsData data = null;
                        if (!result.TryGetValue(kvp.Key, out data))
                        {
                            data = new GameTypeStaticsData();
                            data.ServerAlived = 1;
                            result[kvp.Key] = data;
                        }

                        data.FuBenAlived = kvp.Value.Count;
                        data.SingUpRoleCount = 0; // not set
                        data.StartGameRoleCount = kvp.Value.ToList().Sum(c => c.Value);
                    }

                    if (TotalMainlineRolePayLoad > 0)
                    {
                        result.Add((int)GameTypes.KuaFuMap, new GameTypeStaticsData()
                        {
                            ServerAlived = 1,
                            StartGameRoleCount = (int)TotalMainlineRolePayLoad
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 设置跨服主线人数负载
        /// </summary>
        /// <param name="payload"></param>
        public void SetMainlinePayload(int payload)
        {
            lock (Mutex)
            {
                TotalMainlineRolePayLoad = payload;
            }
        }

        /// <summary>
        /// 增加副本负载
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="gameId"></param>
        /// <param name="roleNum"></param>
        public void AssginKfFuben(GameTypes gameType, long gameId, int roleNum)
        {
            lock (Mutex)
            {
                Dictionary<long, int> dict = null;
                if (!AlivedGameDict.TryGetValue((int)gameType, out dict))
                {
                    dict = new Dictionary<long, int>();
                    AlivedGameDict[(int)gameType] = dict;
                }

                dict.Add(gameId, roleNum);
                TotalFubenRolePayLoad += roleNum;
            }
        }

        /// <summary>
        /// 移除副本负载
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="gameId"></param>
        public void RemoveKfFuben(GameTypes gameType, long gameId)
        {
            lock (Mutex)
            {
                Dictionary<long, int> dict = null;
                if (AlivedGameDict.TryGetValue((int)gameType, out dict)
                    && dict.ContainsKey(gameId))
                {
                    TotalFubenRolePayLoad -= dict[gameId];
                    dict.Remove(gameId);
                }
            }
        }

        /// <summary>
        /// 心跳
        /// </summary>
        public void ClientHeartTick()
        {
            MaxActiveTicks = Global.NowTime.AddSeconds(Consts.RenewServerActiveTicks).Ticks;
            MaxDeadTicks = Global.NowTime.AddSeconds(Consts.RenewServerDeadTicks).Ticks;
        }

        /// <summary>
        /// 发布异步消息
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="evItem"></param>
        public void PostAsyncEvent(GameTypes gameType, AsyncDataItem evItem)
        {
            lock (Mutex)
            {
                Queue<AsyncDataItem> evQ = null;
                if (!EvItemOfGameType.TryGetValue((int)gameType, out evQ))
                {
                    evQ = new Queue<AsyncDataItem>();
                    EvItemOfGameType[(int)gameType] = evQ;
                }

                evQ.Enqueue(evItem);

                while (evQ.Count > MaxCachedAsyncDataItemCount)
                {
                    evQ.Dequeue();
                }
            }
        }

        /// <summary>
        /// 取出异步消息
        /// </summary>
        /// <param name="gameType"></param>
        /// <returns></returns>
        public AsyncDataItem[] PickAsyncEvent(GameTypes gameType)
        {
            lock (Mutex)
            {
                ClientHeartTick();
                Queue<AsyncDataItem> evQ = null;
                if (EvItemOfGameType.TryGetValue((int)gameType, out evQ))
                {
                    AsyncDataItem[] result = evQ.ToArray();
                    evQ.Clear();
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 初始化GameTypes
        /// </summary>
        /// <param name="gameType"></param>
        public void TryInitGameType(int gameType)
        {
            lock (Mutex)
            {
                if (!AlivedGameDict.ContainsKey(gameType))
                {
                    AlivedGameDict[gameType] = new Dictionary<long, int>();
                }
            }
        }
    }
}