using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin pet của người chơi tương ứng
    /// </summary>
    [ProtoContract]
    public class OtherRolePetData
    {
        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(1)]
        public string RoleName { get; set; }

        /// <summary>
        /// Danh sách Pet
        /// </summary>
        [ProtoMember(2)]
        public List<PetData> Pets { get; set; }

        /// <summary>
        /// Danh sách trang bị pet
        /// </summary>
        [ProtoMember(3)]
        public List<GoodsData> PetEquips { get; set; }
    }
}
