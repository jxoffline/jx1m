using Server.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Quản lý vật phẩm của người chơi
    /// </summary>
    public partial class KPlayerGoods
    {
        /// <summary>
        /// Danh sách vật phẩm hiện có với khóa là DBID
        /// </summary>
        private readonly ConcurrentDictionary<int, GoodsData> goodsData = new ConcurrentDictionary<int, GoodsData>();

        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="goodsData"></param>
        private void Initialize(List<GoodsData> goodsData)
        {
            
            /// Duyệt danh sách vật phẩm
            foreach (GoodsData itemGD in goodsData)
            {
                /// Thêm vào danh sách
                this.goodsData[itemGD.Id] = itemGD;
            }
        }
    }
}
