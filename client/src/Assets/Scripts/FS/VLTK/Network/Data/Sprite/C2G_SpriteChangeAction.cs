using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client về Server yêu cầu thay đổi động tác đối tượng
    /// </summary>
    [ProtoContract]
    public class C2G_SpriteChangeAction
    {
        /// <summary>
        /// Hướng của đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int Direction { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// ID động tác
        /// </summary>
        [ProtoMember(4)]
        public int ActionID { get; set; }

        /// <summary>
        /// Thời điểm gửi gói tin đi
        /// </summary>
        [ProtoMember(5)]
        public long StartTick { get; set; }

        /// <summary>
        /// Vị trí của đối tượng
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
