using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo thông tin thành viên nhóm
    /// </summary>
    [ProtoContract]
    public class TeamMemberAttributes
    {
        /// <summary>
        /// ID thành viên
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
        /// Avarta nhân vật
        /// </summary>
        [ProtoMember(7)]
        public int AvartaID { get; set; }

        /// <summary>
        /// ID môn phái
        /// </summary>
        [ProtoMember(8)]
        public int FactionID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(9)]
        public int Level { get; set; }
    }
}
