using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Danh sách các đối tượng bị xóa
    /// </summary>
    [ProtoContract]
    public class RemoveObjects
    {
        /// <summary>
        /// Danh sách quái
        /// </summary>
        [ProtoMember(1)]
        public List<int> Monsters { get; set; }

        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        [ProtoMember(2)]
        public List<int> Players { get; set; }

        /// <summary>
        /// Danh sách pet
        /// </summary>
        [ProtoMember(3)]
        public List<int> Pets { get; set; }

        /// <summary>
        /// Danh sách điểm thu thập
        /// </summary>
        [ProtoMember(4)]
        public List<int> GrowPoints { get; set; }

        /// <summary>
        /// Danh sách khu vực động
        /// </summary>
        [ProtoMember(5)]
        public List<int> DynamicAreas { get; set; }

        /// <summary>
        /// Danh sách Bot biểu diễn
        /// </summary>
        [ProtoMember(6)]
        public List<int> DecoBots { get; set; }

        /// <summary>
        /// Danh sách Bot bán hàng
        /// </summary>
        [ProtoMember(7)]
        public List<int> StallBots { get; set; }

        /// <summary>
        /// Danh sách NPC
        /// </summary>
        [ProtoMember(8)]
        public List<int> NPCs { get; set; }

        /// <summary>
        /// Danh sách bẫy
        /// </summary>
        [ProtoMember(9)]
        public List<int> Traps { get; set; }

        /// <summary>
        /// Danh sách vật phẩm rơi
        /// </summary>
        [ProtoMember(10)]
        public List<int> GoodsPacks { get; set; }

        /// <summary>
        /// Danh sách xe tiêu
        /// </summary>
        [ProtoMember(11)]
        public List<int> Carriages { get; set; }
    }
}
