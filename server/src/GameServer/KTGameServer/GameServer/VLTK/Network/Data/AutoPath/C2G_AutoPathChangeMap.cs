using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Auto tìm đường của Client lên Server yêu cầu chuyển Map
    /// </summary>
    [ProtoContract]
    public class C2G_AutoPathChangeMap
    {
        /// <summary>
        /// ID bản đồ dịch tới
        /// </summary>
        [ProtoMember(1)]
        public int ToMapCode { get; set; }

        /// <summary>
        /// ID vật phẩm tương ứng dùng
        /// </summary>
        [ProtoMember(2)]
        public int ItemID { get; set; }

        /// <summary>
        /// Dịch chuyển ở NPC
        /// </summary>
        [ProtoMember(3)]
        public bool UseNPC { get; set; }
    }
}
