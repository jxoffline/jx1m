using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đối tượng GrowPoint
    /// </summary>
    [ProtoContract]
    public class GrowPointObject
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID ngoại hình
        /// </summary>
        [ProtoMember(2)]
        public int ResID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [ProtoMember(3)]
        public string Name { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(4)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(5)]
        public int PosY { get; set; }
    }
}
