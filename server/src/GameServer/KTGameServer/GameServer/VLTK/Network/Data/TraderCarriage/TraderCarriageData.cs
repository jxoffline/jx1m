using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu xe tiêu
    /// </summary>
    [ProtoContract]
    public class TraderCarriageData
    {
        /// <summary>
        /// ID xe tiêu
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID chủ nhân xe tiêu
        /// </summary>
        [ProtoMember(2)]
        public int OwnerID { get; set; }

        /// <summary>
        /// Tên xe tiêu
        /// </summary>
        [ProtoMember(3)]
        public string Name { get; set; }

        /// <summary>
        /// Danh hiệu xe tiêu
        /// </summary>
        [ProtoMember(4)]
        public string Title { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(5)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(6)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(7)]
        public int Direction { get; set; }

        /// <summary>
        /// Sinh lực hiện tại
        /// </summary>
        [ProtoMember(8)]
        public int HP { get; set; }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        [ProtoMember(9)]
        public int MaxHP { get; set; }

        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        [ProtoMember(10)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        [ProtoMember(11)]
        public int ResID { get; set; }
    }
}
