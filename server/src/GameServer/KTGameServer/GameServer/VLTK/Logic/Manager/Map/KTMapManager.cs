using System.Collections.Generic;
using System.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bản đồ
    /// </summary>
    public static partial class KTMapManager
    {
        /// <summary>
        /// Danh sách bản đồ
        /// </summary>
        private static readonly Dictionary<int, GameMap> maps = new Dictionary<int, GameMap>();




        /// <summary>
        /// Trả về bản đồ có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static GameMap Find(int id)
        {
            /// Nếu tồn tại
            if (KTMapManager.maps.TryGetValue(id, out GameMap map))
            {
                /// Trả về kết quả
                return map;
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Trả về danh sách bản đồ hệ thống
        /// </summary>
        /// <returns></returns>
        public static List<GameMap> GetAll()
        {
            /// Trả về kết quả
            return KTMapManager.maps.Values.ToList();
        }
    }
}
