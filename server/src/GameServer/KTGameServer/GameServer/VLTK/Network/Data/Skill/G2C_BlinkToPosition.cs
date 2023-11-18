using System;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client dịch chuyển mục tiêu chớp nhoáng đến vị trí chỉ định
    /// </summary>
    [ProtoContract]
    public class G2C_BlinkToPosition
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Vị trí đích đến X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí đích đến Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// Thời gian thực hiện hiệu ứng
        /// </summary>
        [ProtoMember(4)]
        public float Duration { get; set; }

        /// <summary>
        /// Hướng quay của đối tượng
        /// </summary>
        [ProtoMember(5)]
        public int Direction { get; set; }
    }
}
