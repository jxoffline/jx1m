using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Quản lý vật phẩm của người chơi
    /// </summary>
    public partial class KPlayerGoods
    {
        #region Properties
        /// <summary>
        /// Đối tượng người chơi tương ứng
        /// </summary>
        public KPlayer Owner { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Quản lý vật phẩm của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="goodsData"></param>
        public KPlayerGoods(KPlayer player, List<GoodsData> goodsData)
        {
            /// Lưu người chơi lại
            this.Owner = player;
            if (goodsData != null)
            {
                /// Khởi tạo danh sách vật phẩm
                this.Initialize(goodsData);
            }
        }
        #endregion
    }
}
