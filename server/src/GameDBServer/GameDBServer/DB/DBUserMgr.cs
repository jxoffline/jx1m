using GameDBServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameDBServer.DB
{
    public class DBUserMgr
    {
        private ConcurrentDictionary<string, MyWeakReference> DictUserInfos = new ConcurrentDictionary<string, MyWeakReference>();

        public int GetUserInfoCount()
        {
            return DictUserInfos.Count;
        }

        /// <summary>
        /// 定位用户信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public DBUserInfo FindDBUserInfo(string userID)
        {
            DBUserInfo dbUserInfo = null;

            MyWeakReference weakRef = null;

            if (DictUserInfos.Count > 0)
            {
                if (DictUserInfos.TryGetValue(userID, out weakRef))
                {
                    if (weakRef.IsAlive)
                    {
                        dbUserInfo = weakRef.Target as DBUserInfo;
                    }
                }
            }

            if (null != dbUserInfo)
            {
                dbUserInfo.LastReferenceTicks = DateTime.Now.Ticks / 10000;
            }

            return dbUserInfo;
        }

        public void AddDBUserInfo(DBUserInfo dbUserInfo)
        {
            MyWeakReference weakRef = null;

            if (DictUserInfos.TryGetValue(dbUserInfo.UserID, out weakRef))
            {
                weakRef.Target = dbUserInfo;
            }
            else
            {
                DictUserInfos.TryAdd(dbUserInfo.UserID, new MyWeakReference(dbUserInfo));
            }
        }

        public void RemoveDBUserInfo(string userID)
        {
            MyWeakReference weakRef = null;

            if (DictUserInfos.TryGetValue(userID, out weakRef))
            {
                weakRef.Target = null;
            }
        }

        public void ReleaseIdleDBUserInfos(int ticksSlot)
        {
            long nowTicks = DateTime.Now.Ticks / 10000;
            List<string> idleUserIDList = new List<string>();

            foreach (var weakRef in DictUserInfos.Values)
            {
                if (weakRef.IsAlive)
                {
                    DBUserInfo dbUserInfo = (weakRef.Target as DBUserInfo);
                    if (null != dbUserInfo)
                    {
                        if (nowTicks - dbUserInfo.LastReferenceTicks >= ticksSlot)
                        {
                            idleUserIDList.Add(dbUserInfo.UserID);
                        }
                    }
                }
            }

            for (int i = 0; i < idleUserIDList.Count; i++)
            {
                RemoveDBUserInfo(idleUserIDList[i]);
            }
        }
    }
}