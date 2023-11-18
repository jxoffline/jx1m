using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin bản ghi trong xếp hạng Phong Hỏa Liên Thành
    /// </summary>
    [ProtoContract]
    public class FHLC_Record
    {
        /// <summary>
        /// Thứ hạng
        /// </summary>
        [ProtoMember(1)]
        public int Rank { get; set; }

        /// <summary>
        /// Tên nhân vật
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// Môn phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID { get; set; }

        /// <summary>
        /// Điểm tích lũy
        /// </summary>
        [ProtoMember(5)]
        public int Score { get; set; }
    }

    /// <summary>
    /// Bảng xếp hạng v
    /// </summary>
    [ProtoContract]
    public class FHLCScoreboardData
    {
        /// <summary>
        /// Danh sách bản ghi
        /// </summary>
        [ProtoMember(1)]
        public List<FHLC_Record> Records { get; set; }

        /// <summary>
        /// Thứ hạng bản thân
        /// </summary>
        [ProtoMember(2)]
        public int SelfRank { get; set; }
    }
}
