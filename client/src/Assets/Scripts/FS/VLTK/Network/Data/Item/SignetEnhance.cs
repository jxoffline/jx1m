using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin Cường hóa Ngũ hành ấn
    /// </summary>
    [ProtoContract]
    public class SignetEnhance
    {
        /// <summary>
        /// DbID trang bị trong túi đồ
        /// </summary>
        [ProtoMember(1)]
        public int SignetDbID { get; set; }

        /// <summary>
        /// DbID các vật phẩm Huyền Tinh trong túi đồ
        /// </summary>
        [ProtoMember(2)]
        public List<int> FSDbID { get; set; }

        /// <summary>
        /// Loại thao tác
        /// <para>0: Cường hóa</para>
        /// <para>1: Nhược hóa</para>
        /// </summary>
        [ProtoMember(3)]
        public int Type { get; set; }
    }
}
