using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo có thành viên mới gia nhập hoặc thành viên cũ rời nhóm
    /// </summary>
    [ProtoContract]
    public class TeamMemberChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Bản đồ hiện tại
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
        /// Sinh lực hiện tại
        /// </summary>
        [ProtoMember(5)]
        public int HP { get; set; }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        [ProtoMember(6)]
        public int MaxHP { get; set; }

        /// <summary>
        /// Loại (0: Rời nhóm, 1: Gia nhập nhóm)
        /// </summary>
        [ProtoMember(7)]
        public int Type { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [ProtoMember(8)]
        public string RoleName { get; set; }

        /// <summary>
        /// ID phái
        /// </summary>
        [ProtoMember(9)]
        public int FactionID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(10)]
        public int Level { get; set; }

        /// <summary>
        /// ID trưởng nhóm
        /// </summary>
        [ProtoMember(11)]
        public int TeamLeaderID { get; set; }

        /// <summary>
        /// ID quần áo
        /// </summary>
        [ProtoMember(12)]
        public int ArmorID { get; set; }

        /// <summary>
        /// ID vũ khí
        /// </summary>
        [ProtoMember(13)]
        public int WeaponID { get; set; }

        /// <summary>
        /// Cấp cường hóa vũ khí
        /// </summary>
        [ProtoMember(14)]
        public int WeaponEnhanceLevel { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        [ProtoMember(15)]
        public int HelmID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        [ProtoMember(16)]
        public int MantleID { get; set; }

        /// <summary>
        /// ID Avarta
        /// </summary>
        [ProtoMember(17)]
        public int AvartaID { get; set; }
    }
}
