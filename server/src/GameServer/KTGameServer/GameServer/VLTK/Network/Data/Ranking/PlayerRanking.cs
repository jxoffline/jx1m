using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{


    [ProtoContract]
    public class ChangeSlogenFamily
    {
        /// <summary>
        /// ID Của gia tộc
        /// </summary>
        [ProtoMember(1)]
        public int FamilyID { get; set; }

        /// <summary>
        /// Chuỗi Slogen gia tộc
        /// </summary>
        [ProtoMember(2)]
        public string Slogen { get; set; }
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
