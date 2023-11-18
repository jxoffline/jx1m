using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class GuildWarMiniMap
    {
        /// <summary>
        /// Bản đồ thuộc về Bang nào
        /// </summary>
        ///
        [ProtoMember(1)]
        public int GuildID { get; set; }

        /// <summary>
        /// Tên bang hội
        /// </summary>
        ///
        [ProtoMember(2)]
        public string GuildName { get; set; }

        /// <summary>
        /// ID bản đồ
        /// </summary>
        ///
        [ProtoMember(3)]
        public int MapID { get; set; }

        /// <summary>
        /// Mã màu của bang này
        /// </summary>
        ///
        [ProtoMember(4)]
        public string HexColor { get; set; }

        // 0: Lảnh Thổ | Màu của HExColor
        // 1 : Thành Chính | HÌnh cái NGôi Sao
        // 2 : Thân THủ Thôn | hình cái ngôn nhà
        // 3 : Nhấy nháy tấn công hình cái kiếm chéo vào nhau
        // 4 : Nhấp nháy lân cận 1 cái gạch ngang
        // 5 : 1 Gạch ngang nhưng không nhấp nháy

        [ProtoMember(5)]
        public int MapType { get; set; }


        [ProtoMember(6)]
        public int Tax { get; set; }


    }
}
