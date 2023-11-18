using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin giữa Client và Server cập nhật vị trí của đối tượng
    /// </summary>
    [ProtoContract]
    public class SpritePositionData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID bản đồ đang đứng
        /// </summary>
        [ProtoMember(2)]
        public int MapCode { get; set; }

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
        /// Danh sách nhãn các khu Obs động đã được mở
        /// </summary>
        [ProtoMember(5)]
        public byte[] DynamicObsLabel { get; set; }

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