using GameServer.Logic;
using System.Collections.Concurrent;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý vật phẩm rơi
    /// </summary>
    public static partial class KTGoodsPackManager
    {
        /// <summary>
        /// Danh sách vật phẩm rơi
        /// </summary>
        private static readonly ConcurrentDictionary<int, KGoodsPack> goodsPacks = new ConcurrentDictionary<int, KGoodsPack>();

        /// <summary>
        /// Tìm vật phẩm rơi theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KGoodsPack FindGoodsPack(int id)
        {
            if (KTGoodsPackManager.goodsPacks.TryGetValue(id, out KGoodsPack goodsPack))
            {
                return goodsPack;
            }
            return null;
        }
    }
}
