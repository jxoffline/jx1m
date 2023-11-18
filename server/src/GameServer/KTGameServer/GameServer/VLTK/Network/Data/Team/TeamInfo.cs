using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo thông tin nhóm thay đổi
    /// </summary>
    [ProtoContract]
    public class TeamInfo
    {
        /// <summary>
        /// ID nhóm
        /// </summary>
        [ProtoMember(1)]
        public int TeamID { get; set; }

        /// <summary>
        /// ID nhóm trưởng
        /// </summary>
        [ProtoMember(2)]
        public int TeamLeaderID { get; set; }

        /// <summary>
        /// Danh sách đội viên
        /// </summary>
        [ProtoMember(3)]
        public List<RoleDataMini> Members { get; set; }

        /// <summary>
        /// Tổng số đội viên
        /// </summary>
        public int TotalMembers
        {
            get
            {
                return this.Members.Count;
            }
        }
    }
}
