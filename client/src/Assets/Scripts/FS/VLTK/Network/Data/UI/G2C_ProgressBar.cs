using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo thao tác ProgressBar
    /// </summary>
    [ProtoContract]
    public class G2C_ProgressBar
    {
        /// <summary>
        /// Loại thao tác
        /// <para>1: Cập nhật thay đổi</para>
        /// <para>2: Đóng</para>
        /// </summary>
        [ProtoMember(1)]
        public int Type { get; set; }

        /// <summary>
        /// Thời gian duy trì
        /// </summary>
        [ProtoMember(2)]
        public float Duration { get; set; }

        /// <summary>
        /// Thời gian hiện tại
        /// </summary>
        [ProtoMember(3)]
        public float CurrentLifeTime { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        [ProtoMember(4)]
        public string Text { get; set; }
    }
}
