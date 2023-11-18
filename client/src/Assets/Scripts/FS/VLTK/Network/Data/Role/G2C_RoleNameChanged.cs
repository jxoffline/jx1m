using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo tên đối tượng thay đổi
    /// </summary>
    [ProtoContract]
    public class G2C_RoleNameChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }
    }
}
