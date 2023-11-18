using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo danh hiệu của đối tượng thay đổi
    /// </summary>
    [ProtoContract]
    public class G2C_RoleTitleChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        [ProtoMember(2)]
        public string Title { get; set; }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        [ProtoMember(3)]
        public string GuildTitle { get; set; }
    }
}
