using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo vật phẩm vừa bán vào Shop thành công
    /// </summary>
    [ProtoContract]
    public class G2C_PlayerSellItemToNPCShop
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Thông tin vật phẩm tương ứng
        /// </summary>
        [ProtoMember(2)]
        public GoodsData ItemGD { get; set; }

        /// <summary>
        /// Số bạc khóa có
        /// </summary>
        [ProtoMember(3)]
        public int BoundMoneyHave { get; set; }
    }
}
