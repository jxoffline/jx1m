using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin bạn bè
    /// </summary>
    [ProtoContract]
    public class FriendData
    {
        /// <summary>
        /// DbID thông tin bạn bè
        /// </summary>
        [ProtoMember(1)]
        public int DbID { get; set; }

        /// <summary>
        /// ID người chơi
        /// </summary>
        [ProtoMember(2)]
        public int OtherRoleID { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(3)]
        public string OtherRoleName { get; set; }

        /// <summary>
        /// Cấp độ người chơi
        /// </summary>
        [ProtoMember(4)]
        public int OtherLevel { get; set; }

        /// <summary>
        /// Môn phái người chơi
        /// </summary>
        [ProtoMember(5)]
        public int FactionID { get; set; }

        /// <summary>
        /// Trạng thái Online của người chơi
        /// </summary>
        [ProtoMember(6)]
        public int OnlineState { get; set; }

        /// <summary>
        /// Bản đồ đang đứng
        /// </summary>
        [ProtoMember(7)]
        public int MapCode { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(8)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(9)]
        public int PosY { get; set; }

        /// <summary>
        /// Kiểu bạn bè
        /// <para>0: Bạn, 1: Đen, 2: Thù</para>
        /// </summary>
        [ProtoMember(10)]
        public int FriendType { get; set; }

        /// <summary>
        /// Có phải vợ chồng không
        /// </summary>
        [ProtoMember(11)]
        public int SpouseId { get; set; }

        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(12)]
        public int GuildID { get; set; }

        /// <summary>
        /// Độ thân mật
        /// </summary>
        [ProtoMember(13)]
        public int Relationship { get; set; }

        /// <summary>
        /// Avarta người chơi
        /// </summary>
        [ProtoMember(14)]
        public int PicCode { get; set; }
    }
}
