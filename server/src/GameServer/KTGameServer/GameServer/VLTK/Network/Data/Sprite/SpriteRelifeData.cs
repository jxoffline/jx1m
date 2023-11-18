using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Hồi phục sinh lực, nội lực, thể lực
    /// </summary>
    [ProtoContract]
    public class SpriteRelifeData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

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
        /// Hướng quay
        /// </summary>
        [ProtoMember(4)]
        public int Direction { get; set; }

        /// <summary>
        /// Sinh lực mới
        /// </summary>
        [ProtoMember(5)]
        public int HP { get; set; }

        /// <summary>
        /// Nội lực mới
        /// </summary>
        [ProtoMember(6)]
        public int MP { get; set; }

        /// <summary>
        /// Thể lực mới
        /// </summary>
        [ProtoMember(7)]
        public int Stamina { get; set; }
    }
}
