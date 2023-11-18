using GameServer.KiemThe.Entities;
using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo thêm vật phẩm vào túi
    /// </summary>
    [ProtoContract]
    public class AddGoodsData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// DbID vật phẩm
        /// </summary>
        [ProtoMember(2)]
        public int ID { get; set; }

        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(3)]
        public int GoodsID { get; set; }

        /// <summary>
        /// Cấp cường hóa
        /// </summary>
        [ProtoMember(4)]
        public int ForgeLevel { get; set; }

        /// <summary>
        /// Tổng số vật phẩm tại ô tương ứng
        /// </summary>
        [ProtoMember(5)]
        public int GoodsNum { get; set; }

        /// <summary>
        /// Cố định không
        /// </summary>
        [ProtoMember(6)]
        public int Binding { get; set; }

        /// <summary>
        /// Vị trí túi (0: Hành trang, 1: Thương khố)
        /// </summary>
        [ProtoMember(7)]
        public int Site { get; set; }

        /// <summary>
        /// Thực hiện hiệu ứng biểu diễn vừa nhặt được vật phẩm
        /// </summary>
        [ProtoMember(8)]
        public int NewHint { get; set; }

        /// <summary>
        /// Thời gian hết hạn
        /// </summary>
        [ProtoMember(9)]
        public string NewEndTime { get; set; }

        /// <summary>
        /// Độ bền
        /// </summary>
        [ProtoMember(10)]
        public int Strong { get; set; }

        /// <summary>
        /// Vị trí trong túi
        /// </summary>
        [ProtoMember(11)]
        public int BagIndex { get; set; }

        /// <summary>
        /// Danh sách các thông số khác
        /// <para>Tiền tệ, etc....</para>
        /// </summary>
        [ProtoMember(12)]
        public Dictionary<ItemPramenter, string> OtherParams { get; set; } = null;

        /// <summary>
        /// Vị trí mặc trang bị trên người (-1 nếu ở trong túi đồ)
        /// </summary>
        [ProtoMember(13)]
        public int Using { get; set; } = -1;

        /// <summary>
        /// Prop thông tin trang bị (NULL nếu là vật phẩm)
        /// </summary>
        [ProtoMember(14)]
        public string Props { get; set; } = null;

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [ProtoMember(15)]
        public int Series { get; set; }
    }
}
