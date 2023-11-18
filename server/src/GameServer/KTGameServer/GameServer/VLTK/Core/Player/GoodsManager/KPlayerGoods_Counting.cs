using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Quản lý vật phẩm của người chơi
    /// </summary>
    public partial class KPlayerGoods
    {
        /// <summary>
        /// Đếm số lượng các vật phẩm trong túi thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int CountAll(Predicate<GoodsData> predicate)
        {
            /// Số lượng
            int count = 0;

            /// Danh sách khóa
            List<int> keys = this.goodsData.Keys.ToList();
            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!this.goodsData.TryGetValue(key, out GoodsData itemGD))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn Predicate
                if (predicate(itemGD))
                {
                    /// Tăng số lượng
                    count += itemGD.GCount;
                }
            }

            /// Trả về kết quả
            return count;
        }

        /// <summary>
        /// Trả về tổng số các vật phẩm trong túi
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public int CountAll(int itemID)
        {
            return this.CountAll(x => x.GoodsID == itemID);
        }

        /// <summary>
        /// Trả về tổng số các vật phẩm có ID nằm trong túi tương ứng
        /// </summary>
        /// <param name="site"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public int CountAll(int site, int itemID)
        {
            return this.CountAll(x => x.Site == site && x.GoodsID == itemID);
        }
    }
}
