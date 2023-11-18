using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin lãnh thổ trong bản đồ lãnh thổ
    /// </summary>
    [ProtoContract]
    public class TerritoryData
    {
        /// <summary>
        /// ID bản đồ
        /// </summary>
        [ProtoMember(1)]
        public int MapID { get; set; }

        /// <summary>
        /// ID bang chiếm giữ
        /// </summary>
        [ProtoMember(2)]
        public int GuildID { get; set; }

        /// <summary>
        /// Thuế
        /// </summary>
        [ProtoMember(3)]
        public int Tax { get; set; }

        /// <summary>
        /// Tên bang chiếm giữ
        /// </summary>
        [ProtoMember(4)]
        public string GuildName { get; set; }

        /// <summary>
        /// Có phải thành chính không
        /// </summary>
        [ProtoMember(5)]
        public int IsCity { get; set; }
    }
}