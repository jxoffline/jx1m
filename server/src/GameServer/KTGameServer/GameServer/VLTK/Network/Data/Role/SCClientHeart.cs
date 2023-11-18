using ProtoBuf;
using System;
using Tmsk.Contract;

namespace Server.Data
{
    /// <summary>
    /// Gói tin Ping gửi từ Server về Client
    /// </summary>
    [ProtoContract]
    public class SCClientHeart
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Random Token
        /// </summary>
        [ProtoMember(2)]
        public int RandToken { get; set; }

        /// <summary>
        /// Tick hiện tại của Server
        /// </summary>
        [ProtoMember(3)]
        public long Ticks { get; set; }

        /// <summary>
        /// Tick hiện tại của Client
        /// </summary>
        [ProtoMember(4)]
        public long ClientTicks { get; set; }
    }
}