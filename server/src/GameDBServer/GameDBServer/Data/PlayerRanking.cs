using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    public enum RankMode
    {
        CapDo = 0, // Xếp hạng
        TaiPhu = 1,     // Tài phú
        VoLam = 3,    // Xếp hạng võ lâm
        LienDau = 4,    // Xếp hạng võ lâm
        UyDanh = 5,    // Xếp hạng võ lâm

        // Bảng xếp hạng môn phái
        ThieuLam = 11,

        ThienVuong = 12,
        DuongMon = 13,
        NguDoc = 14,
        NgaMy = 15,
        ThuyYen = 16,
        CaiBang = 17,
        ThienNhan = 18,
        VoDang = 19,
        ConLon = 20,
        MinGiao = 21,
        DoanThi = 22,
    }

    /// <summary>
    /// Thứ hạng người chơi
    /// </summary>
    [ProtoContract]
    public class PlayerRanking
    {
        /// <summary>
        /// Loại thứ hạng
        /// </summary>
        [ProtoMember(1)]
        public int Type { get; set; }

        /// <summary>
        /// ID người chơi
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(3)]
        public string RoleName { get; set; }

        /// <summary>
        /// Môn phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID { get; set; }

        /// <summary>
        /// Hệ phái
        /// </summary>
        [ProtoMember(5)]
        public int RouteID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(6)]
        public int Level { get; set; }

        /// <summary>
        /// Giá trị trong hạng
        /// </summary>
        [ProtoMember(7)]
        public int Value { get; set; }

        /// <summary>
        /// Thứ hạng
        /// </summary>
        [ProtoMember(8)]
        public int ID { get; set; }


        /// <summary>
        /// Đánh dấu xem đã nhận thưởng này bao giwof chưa
        /// </summary>
        [ProtoMember(9)]
        public int Status { get; set; } = -1;

        /// <summary>
        /// Thứ hạng cuối cùng của thằng này sau khi sự kiện kết thúc
        /// </summary>
        [ProtoMember(10)]
        public int LastIndex { get; set; } = -1;
    }

    /// <summary>
    /// Thông tin xếp hạng
    /// </summary>
    [ProtoContract]
    public class Ranking
    {
        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        [ProtoMember(1)]
        public List<PlayerRanking> Players { get; set; }

        /// <summary>
        /// Loại xếp hạng
        /// </summary>
        [ProtoMember(2)]
        public int Type { get; set; }

        /// <summary>
        /// Tổng số người chơi
        /// </summary>
        [ProtoMember(3)]
        public int TotalPlayers { get; set; }
    }
}