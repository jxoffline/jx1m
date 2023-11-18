using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo trạng thái của sự kiện, hoạt động, phụ bản
    /// </summary>
    [ProtoContract]
    public class G2C_EventState
    {
        /// <summary>
        /// ID sự kiện
        /// </summary>
        [ProtoMember(1)]
        public int EventID { get; set; }

        /// <summary>
        /// Trạng thái
        /// <para>0: Đóng, thay bằng khung MiniTask, 1: Mở, ẩn khung MiniTask</para>
        /// </summary>
        [ProtoMember(2)]
        public int State { get; set; }
    }
}
