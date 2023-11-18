using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client yêu cầu mở khung bất kỳ
    /// </summary>
    [ProtoContract]
    public class G2C_OpenUI
    {
        /// <summary>
        /// Tên khung
        /// </summary>
        [ProtoMember(1)]
        public string UIName { get; set; }

        /// <summary>
        /// Danh sách các tham biến đi kèm
        /// </summary>
        [ProtoMember(2)]
        public List<int> Parameters { get; set; }
    }
}
