using System;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client yêu cầu hiển thị bảng hồi sinh khi tử nạn
    /// </summary>
    [ProtoContract]
    public class G2C_ShowReviveFrame
    {
        /// <summary>
        /// Nội dung khung
        /// </summary>
        [ProtoMember(1)]
        public string Message { get; set; }

        /// <summary>
        /// Cho phép hồi sinh tại chỗ
        /// </summary>
        [ProtoMember(2)]
        public bool AllowReviveAtPos { get; set; }
    }
}
