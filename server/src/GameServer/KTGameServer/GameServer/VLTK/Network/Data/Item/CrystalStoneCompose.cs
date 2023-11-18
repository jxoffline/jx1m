using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin ghép Huyền Tinh
    /// </summary>
    [ProtoContract]
    public class CrystalStoneCompose
    {
        /// <summary>
        /// DbID các vật phẩm Huyền Tinh trong túi đồ
        /// </summary>
        [ProtoMember(1)]
        public List<int> CrystalStonesDbID { get; set; }

        /// <summary>
        /// Loại tiền sử dụng
        /// </summary>
        [ProtoMember(2)]
        public int MoneyType { get; set; }
    }
}
