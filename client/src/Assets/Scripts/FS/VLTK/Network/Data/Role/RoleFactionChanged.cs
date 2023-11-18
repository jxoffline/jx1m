using ProtoBuf;
using System;

namespace Server.Data
{
    /// <summary>
    /// Thông báo môn phái người chơi thay đổi
    /// </summary>
    [ProtoContract]
    public class RoleFactionChanged
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID môn phái
        /// </summary>
        [ProtoMember(2)]
        public int FactionID { get; set; }

        /// <summary>
        /// ID nhánh
        /// </summary>
        [ProtoMember(3)]
        public int RouteID { get; set; }
    }
}