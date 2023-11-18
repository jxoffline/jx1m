using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu quái đổ về Client
    /// </summary>
    [ProtoContract]
    public class MonsterData
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        [ProtoMember(3)]
        public string Title { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [ProtoMember(4)]
        public int Elemental { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(5)]
        public int Level { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(6)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(7)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(8)]
        public int Direction { get; set; }

        /// <summary>
        /// Sinh lực
        /// </summary>
        [ProtoMember(9)]
        public int HP { get; set; }

        /// <summary>
        /// Sinh lực thượng hạn
        /// </summary>
        [ProtoMember(10)]
        public int MaxHP { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        [ProtoMember(11)]
        public int ExtensionID { get; set; }

        /// <summary>
        /// Cờ PK
        /// </summary>
        [ProtoMember(12)]
        public int Camp { get; set; }

        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        [ProtoMember(13)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        [ProtoMember(14)]
        public List<BufferData> ListBuffs { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu
        /// </summary>
        [ProtoMember(15)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// Loại quái
        /// </summary>
        [ProtoMember(17)]
        public int MonsterType { get; set; }
    }
}