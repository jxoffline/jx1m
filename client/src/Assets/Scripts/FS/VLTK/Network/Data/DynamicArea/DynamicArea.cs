using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Khu vực động
    /// </summary>
    [ProtoContract]
    public class DynamicArea
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// ID Res đối tượng
        /// </summary>
        [ProtoMember(4)]
        public int ResID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [ProtoMember(5)]
        public string Name { get; set; }
    }
}
