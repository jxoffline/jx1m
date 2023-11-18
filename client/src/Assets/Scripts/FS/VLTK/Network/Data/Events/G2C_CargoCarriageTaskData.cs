using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin quà thưởng vận tiêu
    /// </summary>
    [ProtoContract]
    public class CargoCarriageAwardItemData
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
        /// Khóa hay không
        /// </summary>
        [ProtoMember(3)]
        public bool Bound { get; set; }
    }

    /// <summary>
    /// Gói tin gửi từ GS về Client thông báo nhiệm vụ vận tiêu đang làm
    /// </summary>
    [ProtoContract]
    public class G2C_CargoCarriageTaskData
    {
        /// <summary>
        /// Tên NPC giao tiêu
        /// </summary>
        [ProtoMember(1)]
        public string SourceNPCName { get; set; }

        /// <summary>
        /// ID bản đồ giao tiêu
        /// </summary>
        [ProtoMember(2)]
        public int SourceMapCode { get; set; }

        /// <summary>
        /// Tọa độ X NPC giao tiêu
        /// </summary>
        [ProtoMember(3)]
        public int SourceNPCPosX { get; set; }

        /// <summary>
        /// Tọa độ Y NPC giao tiêu
        /// </summary>
        [ProtoMember(4)]
        public int SourceNPCPosY { get; set; }

        /// <summary>
        /// Tên NPC trả tiêu
        /// </summary>
        [ProtoMember(5)]
        public string DestinationNPCName { get; set; }

        /// <summary>
        /// ID bản đồ trả tiêu
        /// </summary>
        [ProtoMember(6)]
        public int DestinationMapCode { get; set; }

        /// <summary>
        /// Tọa độ X NPC giao tiêu
        /// </summary>
        [ProtoMember(7)]
        public int DestinationNPCPosX { get; set; }

        /// <summary>
        /// Tọa độ Y NPC giao tiêu
        /// </summary>
        [ProtoMember(8)]
        public int DestinationNPCPosY { get; set; }

        /// <summary>
        /// Loại xe tiêu
        /// </summary>
        [ProtoMember(9)]
        public int Type { get; set; }

        /// <summary>
        /// Bạc cần đặt cọc
        /// </summary>
        [ProtoMember(10)]
        public int RequireMoney { get; set; }

        /// <summary>
        /// Bạc khóa cần cọc
        /// </summary>
        [ProtoMember(11)]
        public int RequireBoundMoney { get; set; }

        /// <summary>
        /// KNB cần cọc
        /// </summary>
        [ProtoMember(12)]
        public int RequireToken { get; set; }

        /// <summary>
        /// KNB khóa cần cọc
        /// </summary>
        [ProtoMember(13)]
        public int RequireBoundToken { get; set; }

        /// <summary>
        /// Bạc nhận được
        /// </summary>
        [ProtoMember(14)]
        public int AwardMoney { get; set; }

        /// <summary>
        /// Bạc khóa nhận được
        /// </summary>
        [ProtoMember(15)]
        public int AwardBoundMoney { get; set; }

        /// <summary>
        /// KNB nhận được
        /// </summary>
        [ProtoMember(16)]
        public int AwardToken { get; set; }

        /// <summary>
        /// KNB khóa nhận được
        /// </summary>
        [ProtoMember(17)]
        public int AwardBoundToken { get; set; }

        /// <summary>
        /// Danh sách vật phẩm thưởng
        /// </summary>
        [ProtoMember(18)]
        public List<CargoCarriageAwardItemData> AwardItems { get; set; }
    }
}
