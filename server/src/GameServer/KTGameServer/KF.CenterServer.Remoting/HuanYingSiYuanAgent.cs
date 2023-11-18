using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KF.Contract;
using KF.Contract.Data;
using KF.Contract.Interface;
using KF.Remoting.Data;

namespace KF.Remoting
{
    public class HuanYingSiYuanAgent : ClientAgent
    {
        public Queue<AsyncDataItem> CacheItemQueue = new Queue<AsyncDataItem>();
        public Queue<int> CacheFuBenSeqIdQueue = new Queue<int>();
        private const int MaxCacheFuBenSeqIdCount = 30;
        private const int MaxCachedAsyncDataItemCount = 10000; //最多缓存10000条

        private HashSet<int> AlivedGameFuBenIdDict = new HashSet<int>();

        public HuanYingSiYuanAgent(KuaFuClientContext clientInfo, IKuaFuClient callback)
            : base(clientInfo, callback)
        {
        }

        public int GetAliveGameFuBenCount()
        {
            lock(AlivedGameFuBenIdDict)
            {
                return AlivedGameFuBenIdDict.Count;
            }
        }

        public void AddGameFuBen(int gameId)
        {
            lock (AlivedGameFuBenIdDict)
            {
                AlivedGameFuBenIdDict.Add(gameId);
            }
        }

        public void RemoveGameFuBen(int gameId)
        {
            lock (AlivedGameFuBenIdDict)
            {
                AlivedGameFuBenIdDict.Remove(gameId);
            }
        }

        /// <summary>
        /// 获取存在的列表
        /// </summary>
        /// <returns></returns>
        public List<int> GetAliveGameFuBenList()
        {
            lock (AlivedGameFuBenIdDict)
            {
                if (AlivedGameFuBenIdDict.Count > 0)
                {
                    return AlivedGameFuBenIdDict.ToList();
                }
            }

            return null;
        }

        public void EnqueueCacheItem(KuaFuEventTypes eventType, params object[] args)
        {
            lock (CacheItemQueue)
            {
                AsyncDataItem item = new AsyncDataItem() { EventType = eventType, Args = args };
                int count = CacheItemQueue.Count;
                if (count > MaxCachedAsyncDataItemCount)
                {
                    for (count -= MaxCachedAsyncDataItemCount; count >= 0; count--)
                    {
                        CacheItemQueue.Dequeue();
                    }
                }

                CacheItemQueue.Enqueue(item);
            }
        }

        public int PushFuBenSeqId(List<int> list)
        {
            lock (CacheFuBenSeqIdQueue)
            {
                if (null != list && list.Count != 0)
                {
                    foreach (var id in list)
                    {
                        CacheFuBenSeqIdQueue.Enqueue(id);
                    }
                }

                return MaxCacheFuBenSeqIdCount - CacheFuBenSeqIdQueue.Count;
            }
        }

        public int PopFuBenSeqId()
        {
            lock (CacheFuBenSeqIdQueue)
            {
                if (CacheFuBenSeqIdQueue.Count > 0)
                {
                    return CacheFuBenSeqIdQueue.Dequeue();
                }
            }

            return -1;
        }

        public AsyncDataItem[] GetCacheItems()
        {
            ClientHeartTick();

            lock (CacheItemQueue)
            {
                if (CacheItemQueue.Count > 0)
                {
                    AsyncDataItem[] array = CacheItemQueue.ToArray();
                    CacheItemQueue.Clear();
                    return array;
                }
            }

            return null;
        }

        public void NotifyEnterGame(KuaFuRoleData kuaFuRoleData)
        {
            EnqueueCacheItem(KuaFuEventTypes.UpdateAndNotifyEnterGame, kuaFuRoleData);
        }

        public void NotifyFuBenRoleCount(KuaFuRoleData kuaFuRoleData, int roleCount)
        {
            EnqueueCacheItem(KuaFuEventTypes.NotifyWaitingRoleCount, kuaFuRoleData.RoleId, roleCount);
        }

        public int NotifyChangeState(KuaFuRoleData kuaFuRoleData)
        {
            try
            {
                EnqueueCacheItem(KuaFuEventTypes.RoleStateChange, kuaFuRoleData);
            }
            catch (System.Exception ex)
            {
                return -1;
            }

            return 0;
        }

        public int NotifyAddRole(KuaFuRoleData kuaFuRoleData)
        {
            int result = -1;

            try
            {
                EnqueueCacheItem(KuaFuEventTypes.RoleStateChange, kuaFuRoleData);
            }
            catch (System.Exception ex)
            {
                return -1;
            }

            return result;
        }

        public void ClientHeartTick()
        {
            MaxActiveTicks = Global.NowTime.AddSeconds(Consts.RenewServerActiveTicks).Ticks;
        }

        public void Reset()
        {
            lock (CacheItemQueue)
            {
                CacheItemQueue.Clear();
            }
        }
    }

}
