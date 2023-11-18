using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin Cường hóa trang bị
    /// </summary>
    [ProtoContract]
    public class EquipEnhance
    {
        /// <summary>
        /// DbID trang bị trong túi đồ
        /// </summary>
        [ProtoMember(1)]
        public int EquipDbID { get; set; }

        /// <summary>
        /// DbID các vật phẩm Huyền Tinh trong túi đồ
        /// </summary>
        [ProtoMember(2)]
        public List<int> CrystalStonesDbID { get; set; }

        /// <summary>
        /// Loại tiền sử dụng
        /// </summary>
        [ProtoMember(3)]
        public int MoneyType { get; set; }
    }
}
