using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu Pet Mini gửi về Client
    /// </summary>
    [ProtoContract]
    public class PetDataMini
    {
        /// <summary>
        /// ID pet
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID chủ nhân
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        [ProtoMember(3)]
        public int ResID { get; set; }

        /// <summary>
        /// Tên pet
        /// </summary>
        [ProtoMember(4)]
        public string Name { get; set; }

        /// <summary>
        /// Danh hiệu pet
        /// </summary>
        [ProtoMember(5)]
        public string Title { get; set; }

        /// <summary>
        /// Vị trí đang đứng X
        /// </summary>
        [ProtoMember(6)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí đang đứng Y
        /// </summary>
        [ProtoMember(7)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(8)]
        public int Direction { get; set; }

        /// <summary>
        /// Sinh lực hiện tại
        /// </summary>
        [ProtoMember(9)]
        public int HP { get; set; }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        [ProtoMember(10)]
        public int MaxHP { get; set; }

        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        [ProtoMember(11)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Tốc đánh ngoại công
        /// </summary>
        [ProtoMember(12)]
        public int AtkSpeed { get; set; }

        /// <summary>
        /// Tốc đánh nội công
        /// </summary>
        [ProtoMember(13)]
        public int CastSpeed { get; set; }

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        [ProtoMember(14)]
        public Dictionary<int, int> Skills { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(15)]
        public int Level { get; set; }
    }
}
