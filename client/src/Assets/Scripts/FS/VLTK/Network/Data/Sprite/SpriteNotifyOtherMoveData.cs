using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đối tượng khác di chuyển
    /// </summary>
    [ProtoContract]
    public class SpriteNotifyOtherMoveData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Vị trí bắt đầu X
        /// </summary>
        [ProtoMember(2)]
        public int FromX { get; set; }

        /// <summary>
        /// Vị trí bắt đầu Y
        /// </summary>
        [ProtoMember(3)]
        public int FromY { get; set; }

        /// <summary>
        /// Vị trí đích X
        /// </summary>
        [ProtoMember(4)]
        public int ToX { get; set; }

        /// <summary>
        /// Vị trí đích Y
        /// </summary>
        [ProtoMember(5)]
        public int ToY { get; set; }

        /// <summary>
        /// Chuỗi mã hóa đoạn đường cần di chuyển
        /// </summary>
        [ProtoMember(6)]
        public string PathString { get; set; }

        /// <summary>
        /// Động tác di chuyển
        /// </summary>
        [ProtoMember(7)]
        public int Action { get; set; }

        /// <summary>
        /// Vị trí xuất phát
        /// </summary>
        public UnityEngine.Vector2 FromPos
        {
            get
            {
                return new UnityEngine.Vector2(this.FromX, this.FromY);
            }
        }

        /// <summary>
        /// Vị trí đích
        /// </summary>
        public UnityEngine.Vector2 ToPos
        {
            get
            {
                return new UnityEngine.Vector2(this.ToX, this.ToY);
            }
        }

    }
}