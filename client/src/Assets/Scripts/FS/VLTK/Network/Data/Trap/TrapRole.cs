using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Đối tượng bẫy
    /// </summary>
    [ProtoContract]
    public class TrapRole
    {
        /// <summary>
        /// ID bẫy
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        [ProtoMember(2)]
        public int ResID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(3)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(4)]
        public int PosY { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [ProtoMember(5)]
        public float LifeTime { get; set; }

        /// <summary>
        /// ID đối tượng xuất chiêu
        /// </summary>
        [ProtoMember(6)]
        public int CasterID { get; set; }

        /// <summary>
        /// Vị trí đặt bẫy
        /// </summary>
        public UnityEngine.Vector2 Pos
        {
            get
            {
                return new UnityEngine.Vector2(this.PosX, this.PosY);
            }
        }
    }
}
