using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo số lượng liên trảm trong chiến trường hoặc hoạt động gì đó
    /// </summary>
    [ProtoContract]
    public class G2C_KillStreak
    {
        /// <summary>
        /// Số liên trảm
        /// </summary>
        [ProtoMember(1)]
        public int KillNumber { get; set; }
    }
}
