using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo thay đổi trang bị
    /// </summary>
    [ProtoContract]
    public class ChangeEquipData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Trang bị
        /// </summary>
        [ProtoMember(2)]
        public GoodsData EquipGoodsData { get; set; } = null;

        /// <summary>
        /// Loại thao tác
        /// <para>-1: Mặc vào, > 0: Vị trí trang bị cũ trên người</para>
        /// </summary>
        [ProtoMember(3)]
        public int Type { get; set; }
    }
}
