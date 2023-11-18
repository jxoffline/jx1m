using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu Bot bán hàng
    /// </summary>
    [ProtoContract]
    public class StallBotData
    {
        /// <summary>
        /// ID bot
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên chủ nhân
        /// </summary>
        [ProtoMember(2)]
        public string OwnerRoleName { get; set; }

        /// <summary>
        /// Tên cửa hàng
        /// </summary>
        [ProtoMember(3)]
        public string StallName { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(4)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(5)]
        public int PosY { get; set; }

        /// <summary>
        /// ID áo
        /// </summary>
        [ProtoMember(6)]
        public int ArmorID { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        [ProtoMember(7)]
        public int HelmID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        [ProtoMember(8)]
        public int MantleID { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [ProtoMember(9)]
        public int Sex { get; set; }
    }
}
