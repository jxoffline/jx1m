using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Nhóm các đối tượng xuất hiện xung quanh người chơi
    /// </summary>
    [ProtoContract]
    public class AddNewObjects
    {
        /// <summary>
        /// Danh sách quái
        /// </summary>
        [ProtoMember(1)]
        public List<MonsterData> Monsters { get; set; }

        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        [ProtoMember(2)]
        public List<RoleDataMini> Players { get; set; }

        /// <summary>
        /// Danh sách pet
        /// </summary>
        [ProtoMember(3)]
        public List<PetDataMini> Pets { get; set; }

        /// <summary>
        /// Danh sách điểm thu thập
        /// </summary>
        [ProtoMember(4)]
        public List<GrowPointObject> GrowPoints { get; set; }

        /// <summary>
        /// Danh sách khu vực động
        /// </summary>
        [ProtoMember(5)]
        public List<DynamicArea> DynamicAreas { get; set; }

        /// <summary>
        /// Danh sách Bot biểu diễn
        /// </summary>
        [ProtoMember(6)]
        public List<DecoBotData> DecoBots { get; set; }

        /// <summary>
        /// Danh sách Bot bán hàng
        /// </summary>
        [ProtoMember(7)]
        public List<StallBotData> StallBots { get; set; }

        /// <summary>
        /// Danh sách NPC
        /// </summary>
        [ProtoMember(8)]
        public List<NPCRole> NPCs { get; set; }

        /// <summary>
        /// Danh sách bẫy
        /// </summary>
        [ProtoMember(9)]
        public List<TrapRole> Traps { get; set; }

        /// <summary>
        /// Danh sách vật phẩm rơi
        /// </summary>
        [ProtoMember(10)]
        public List<NewGoodsPackData> GoodsPacks { get; set; }

        /// <summary>
        /// Danh sách xe tiêu
        /// </summary>
        [ProtoMember(11)]
        public List<TraderCarriageData> Carriages { get; set; }
    }
}
