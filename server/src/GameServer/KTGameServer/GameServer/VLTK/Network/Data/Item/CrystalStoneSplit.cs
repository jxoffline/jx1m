using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin tách Huyền Tinh
    /// </summary>
    [ProtoContract]
    public class CrystalStoneSplit
    {
        /// <summary>
        /// DbID trang bị cần tách trong túi đồ
        /// </summary>
        [ProtoMember(1)]
        public int EquipDbID { get; set; }

        /// <summary>
        /// Loại tiền sử dụng
        /// </summary>
        [ProtoMember(2)]
        public int MoneyType { get; set; }
    }
}
