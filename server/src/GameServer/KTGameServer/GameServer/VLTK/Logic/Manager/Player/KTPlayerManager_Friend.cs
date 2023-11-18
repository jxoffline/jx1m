using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Tìm bạn bè trong danh sách bạn có ID tương ứng
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FriendData FindFriendData(KPlayer client, int otherRoleID)
        {
            if (client.FriendDataList == null)
            {
                return null;
            }

            lock (client.FriendDataList)
            {
                for (int i = 0; i < client.FriendDataList.Count; i++)
                {
                    if (client.FriendDataList[i].OtherRoleID == otherRoleID)
                    {
                        return client.FriendDataList[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tìm bạn bè trong danh sách bạn có ID tương ứng
        /// </summary>
        /// <param name="dbID"></param>
        /// <returns></returns>
        public static FriendData GetFriendData(KPlayer client, int dbID)
        {
            if (client.FriendDataList == null)
            {
                return null;
            }

            lock (client.FriendDataList)
            {
                for (int i = 0; i < client.FriendDataList.Count; i++)
                {
                    if (client.FriendDataList[i].DbID == dbID)
                    {
                        return client.FriendDataList[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tìm bạn bè đầu tiên trong danh sách có loại tương ứng
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FriendData FindFirstFriendDataByType(KPlayer client, int friendType)
        {
            if (client.FriendDataList == null)
            {
                return null;
            }

            lock (client.FriendDataList)
            {
                for (int i = 0; i < client.FriendDataList.Count; i++)
                {
                    if (client.FriendDataList[i].FriendType == friendType)
                    {
                        return client.FriendDataList[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Thêm bạn bè
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static void AddFriendData(KPlayer client, FriendData friendData)
        {
            if (client.FriendDataList == null)
            {
                client.FriendDataList = new List<FriendData>();
            }

            lock (client.FriendDataList)
            {
                client.FriendDataList.Add(friendData);
            }
        }

        /// <summary>
        /// Xóa bạn bè
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static void RemoveFriendData(KPlayer client, int dbID)
        {
            if (client.FriendDataList == null)
            {
                return;
            }

            lock (client.FriendDataList)
            {
                for (int i = 0; i < client.FriendDataList.Count; i++)
                {
                    if (client.FriendDataList[i].DbID == dbID)
                    {
                        client.FriendDataList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Dánh sách bạn của thằng người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="friendType"></param>
        /// <returns></returns>
        public static int GetFriendCountByType(KPlayer client, int friendType)
        {
            if (client.FriendDataList == null)
            {
                return 0;
            }

            int totalCount = 0;
            lock (client.FriendDataList)
            {
                for (int i = 0; i < client.FriendDataList.Count; i++)
                {
                    if (client.FriendDataList[i].FriendType == friendType)
                    {
                        totalCount++;
                    }
                }
            }

            return totalCount;
        }
    }
}
