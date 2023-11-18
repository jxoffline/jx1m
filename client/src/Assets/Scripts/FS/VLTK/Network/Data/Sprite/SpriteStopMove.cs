using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client lên Server thông báo đối tượng ngừng di chuyển
    /// </summary>
    [ProtoContract]
    public class SpriteStopMove
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// Thời điểm dừng lại
        /// </summary>
        [ProtoMember(4)]
        public long StopTick { get; set; }

        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        [ProtoMember(5)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Hướng quay của nhân vật
        /// </summary>
        [ProtoMember(6)]
        public int Direction { get; set; }

        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public UnityEngine.Vector2 Position
        {
            get
            {
                return new UnityEngine.Vector2(this.PosX, this.PosY);
            }
        }
    }
}
