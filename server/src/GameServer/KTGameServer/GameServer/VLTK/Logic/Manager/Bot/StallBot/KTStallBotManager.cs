using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Bot bán hàng
    /// </summary>
    public static partial class KTStallBotManager
    {
        #region Define
        /// <summary>
        /// Danh sách Bot bán hàng
        /// </summary>
        private static readonly ConcurrentDictionary<int, KStallBot> bots = new ConcurrentDictionary<int, KStallBot>();
        #endregion

        #region Find
        /// <summary>
        /// Tìm bot theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KStallBot FindBot(int id)
        {
            if (KTStallBotManager.bots.TryGetValue(id, out KStallBot bot))
            {
                return bot;
            }
            return null;
        }

        /// <summary>
        /// Tìm Bot bán hàng theo ID chủ nhân tương ứng
        /// </summary>
        /// <param name="ownerRoleID">ID chủ nhân</param>
        /// <returns></returns>
        public static KStallBot FindBotByOwnerID(int ownerRoleID)
        {
            /// Trả về kết quả
            return KTStallBotManager.FindBot(x => x.OwnerRoleID == ownerRoleID);
        }

        /// <summary>
        /// Tìm bot đầu tiên thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate">Điều kiện</param>
        /// <returns></returns>
        public static KStallBot FindBot(Predicate<KStallBot> predicate)
        {
            /// Danh sách khóa
            List<int> keys = KTStallBotManager.bots.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTStallBotManager.bots.TryGetValue(key, out KStallBot bot))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(bot))
                {
                    /// Trả về kết quả
                    return bot;
                }
            }

            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm bot thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate">Điều kiện</param>
        /// <returns></returns>
        public static List<KStallBot> FindBots(Predicate<KStallBot> predicate)
        {
            /// Tạo mới
            List<KStallBot> bots = new List<KStallBot>();

            /// Danh sách khóa
            List<int> keys = KTStallBotManager.bots.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTStallBotManager.bots.TryGetValue(key, out KStallBot bot))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(bot))
                {
                    /// Thêm vào danh sách
                    bots.Add(bot);
                }
            }

            /// Trả về kết quả
            return bots;
        }
        #endregion
    }
}
