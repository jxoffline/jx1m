using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin vật phẩm trong vòng quay
    /// </summary>
    [ProtoContract]
    public class G2C_LuckyCircle_ItemInfo
    {
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(1)]
        public int ItemID { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        [ProtoMember(2)]
        public int Quantity { get; set; }

        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        [ProtoMember(3)]
        public int EffectType { get; set; }
    }

    /// <summary>
    /// Dữ liệu vòng quay may mắn từ Server gửi về Client
    /// </summary>
    [ProtoContract]
    public class G2C_LuckyCircle
    {
        /// <summary>
        /// Danh sách vật phẩm trong vòng quay
        /// <para></para>
        /// Key: ID vật phẩm
        /// <br></br>
        /// Value: Số lượng
        /// </summary>
        [ProtoMember(1)]
        public List<G2C_LuckyCircle_ItemInfo> Items { get; set; }

        /// <summary>
        /// Vị trí dừng lại lần trước, chưa nhận thưởng
        /// <para></para>
        /// -1: Không có vị trí dừng
        /// </summary>
        [ProtoMember(2)]
        public int LastStopPos { get; set; }

        /// <summary>
        /// Danh sách các tham biến đi kèm
        /// <para></para>
        /// Vị trí đầu tiên sẽ là Loại thao tác (0: Mở vòng quay, 1: Thực hiện quay, 2: Nhận thưởng, 3: Kích hoạt Button chức năng)
        /// </summary>
        [ProtoMember(3)]
        public int[] Fields { get; set; }
    }
}
