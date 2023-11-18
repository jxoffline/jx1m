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
        #region Tìm toàn bộ
        /// <summary>
        /// Tìm toàn bộ các vật phẩm thỏa mãn điều kiện cho trước
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<GoodsData> FindAll(Predicate<GoodsData> predicate)
        {
            /// Tạo mới kết quả
            List<GoodsData> goods = new List<GoodsData>();

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
                if (predicate(itemGD) && itemGD.GCount > 0)
                {
                    /// Thêm vào danh sách
                    goods.Add(itemGD);
                }
            }

            /// Trả về kết quả
            return goods;
        }

        /// <summary>
        /// Tìm toàn bộ vật phẩm có ID tương ứng
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public List<GoodsData> FindAll(int itemID)
        {
            /// Trả về kết quả
            return this.FindAll(x => x.GoodsID == itemID);
        }

        /// <summary>
        /// Tìm toàn bộ vật phẩm có ID nằm trong túi tương ứng
        /// </summary>
        /// <param name="site"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public List<GoodsData> FindAll(int site, int itemID)
        {
            /// Trả về kết quả
            return this.FindAll(x => x.Site == site && x.GoodsID == itemID);
        }
        #endregion

        #region Tìm đơn lẻ
        /// <summary>
        /// Tìm vật phẩm đầu tiên trong danh sách thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public GoodsData Find(Predicate<GoodsData> predicate)
        {
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
                if (predicate(itemGD) && itemGD.GCount > 0)
                {
                    /// Trả về kết quả
                    return itemGD;
                }
            }

            /// Không tìm thấy
            return null;
        }
       
        /// <summary>
        /// Tìm vật phẩm có ID tương ứng
        /// </summary>
        /// <param name="itemDbID"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public GoodsData Find(int itemDbID, int site = -1)
        {
            /// Nếu tồn tại trong danh sách
            if (this.goodsData.TryGetValue(itemDbID, out GoodsData itemGD))
            {
                /// Nếu số lượng về 0
                if (itemGD.GCount <= 0)
                {
                    /// Không tìm thấy
                    return null;
                }
                
                /// Nếu có yêu cầu vị trí ở đâu
                if (site != -1)
                {
                    /// Nếu vị trí không thích hợp
                    if (itemGD.Site != site)
                    {
                        /// Không tìm thấy
                        return null;
                    }
                }

                /// Trả về kết quả
                return itemGD;
            }
            /// Không tìm thấy
            return null;
        }
        #endregion
    }
}
