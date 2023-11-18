using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin Bot biểu diễn
    /// </summary>
    [ProtoContract]
    public class DecoBotData
    {
        /// <summary>
        /// ID bot
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên bot
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(3)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(4)]
        public int PosY { get; set; }

        /// <summary>
        /// ID áo
        /// </summary>
        [ProtoMember(5)]
        public int ArmorID { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        [ProtoMember(6)]
        public int HelmID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        [ProtoMember(7)]
        public int MantleID { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [ProtoMember(8)]
        public int Sex { get; set; }

        /// <summary>
        /// ID vũ khí
        /// </summary>
        [ProtoMember(9)]
        public int WeaponID { get; set; }

        /// <summary>
        /// Ngũ hành vũ khí
        /// </summary>
        [ProtoMember(10)]
        public int WeaponSeries { get; set; }

        /// <summary>
        /// Cấp độ cường hóa vũ khí
        /// </summary>
        [ProtoMember(11)]
        public int WeaponEnhanceLevel { get; set; }

        /// <summary>
        /// ID ngựa
        /// </summary>
        [ProtoMember(12)]
        public int HorseID { get; set; }

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        [ProtoMember(13)]
        public List<BufferData> Buffs { get; set; }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        [ProtoMember(14)]
        public string Title { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        [ProtoMember(15)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Tốc đánh ngoại công
        /// </summary>
        [ProtoMember(16)]
        public int AtkSpeed { get; set; }

        /// <summary>
        /// Tốc đánh nội công
        /// </summary>
        [ProtoMember(17)]
        public int CastSpeed { get; set; }
    }
}
