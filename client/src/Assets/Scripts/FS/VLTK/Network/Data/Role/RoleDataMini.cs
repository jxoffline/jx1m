using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin người chơi khác
    /// </summary>
    [ProtoContract]
    public class RoleDataMini
    {
        /// <summary>
        /// ID máy chủ
        /// </summary>
        [ProtoMember(1)]
        public int ZoneID { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [ProtoMember(3)]
        public string RoleName { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [ProtoMember(4)]
        public int RoleSex { get; set; }

        /// <summary>
        /// ID môn phái
        /// </summary>
        [ProtoMember(5)]
        public int FactionID { get; set; }

        /// <summary>
        /// ID nhánh
        /// </summary>
        [ProtoMember(6)]
        public int RouteID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(7)]
        public int Level { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(8)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(9)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng hiện tại
        /// </summary>
        [ProtoMember(10)]
        public int CurrentDir { get; set; }

        /// <summary>
        /// Sinh lực hiện tại
        /// </summary>
        [ProtoMember(11)]
        public int HP { get; set; }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        [ProtoMember(12)]
        public int MaxHP { get; set; }

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        [ProtoMember(13)]
        public List<BufferData> BufferDataList { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        [ProtoMember(14)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// ID quần áo
        /// </summary>
        [ProtoMember(15)]
        public int ArmorID { get; set; }

        /// <summary>
        /// ID vũ khí
        /// </summary>
        [ProtoMember(16)]
        public int WeaponID { get; set; }

        /// <summary>
        /// Cấp cường hóa vũ khí
        /// </summary>
        [ProtoMember(17)]
        public int WeaponEnhanceLevel { get; set; }

        /// <summary>
        /// ID mũ
        /// </summary>
        [ProtoMember(18)]
        public int HelmID { get; set; }

        /// <summary>
        /// ID phi phong
        /// </summary>
        [ProtoMember(19)]
        public int MantleID { get; set; }

        /// <summary>
        /// ID ngựa
        /// </summary>
        [ProtoMember(20)]
        public int HorseID { get; set; }

        /// <summary>
        /// Đang trong trạng thái cưỡi ngựa
        /// </summary>
        [ProtoMember(21)]
        public bool IsRiding { get; set; }

        /// <summary>
        /// ID nhóm
        /// </summary>
        [ProtoMember(22)]
        public int TeamID { get; set; }

        /// <summary>
        /// ID đội trưởng
        /// </summary>
        [ProtoMember(23)]
        public int TeamLeaderID { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        [ProtoMember(24)]
        public int MapCode { get; set; }

        /// <summary>
        /// ID Avarta
        /// </summary>
        [ProtoMember(25)]
        public int AvartaID { get; set; }

        /// <summary>
        /// Ngũ hành vũ khí
        /// </summary>
        [ProtoMember(26)]
        public int WeaponSeries { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// </summary>
        [ProtoMember(27)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// </summary>
        [ProtoMember(28)]
        public int CastSpeed { get; set; }

        /// <summary>
        /// Trạng thái PK
        /// </summary>
        [ProtoMember(29)]
        public int PKMode { get; set; }

        /// <summary>
        /// Sát khí
        /// </summary>
        [ProtoMember(30)]
        public int PKValue { get; set; }

        /// <summary>
        /// Cờ PK
        /// </summary>
        [ProtoMember(31)]
        public int Camp { get; set; }

        /// <summary>
        /// Tên sạp hàng đang bày bán
        /// </summary>
        [ProtoMember(32)]
        public string StallName { get; set; }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        [ProtoMember(33)]
        public string Title { get; set; }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        [ProtoMember(34)]
        public string GuildTitle { get; set; }

        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(35)]
        public int GuildID { get; set; }

        /// <summary>
        /// Tên bang hội
        /// </summary>
        [ProtoMember(36)]
        public string GuildName { get; set; }

        /// <summary>
        /// Tổng tài phú
        /// </summary>
        [ProtoMember(37)]
        public long TotalValue { get; set; }

        /// <summary>
        /// ID tộc
        /// </summary>
        [ProtoMember(38)]
        public int FamilyID { get; set; }

        /// <summary>
        /// Tên tộc
        /// </summary>
        [ProtoMember(39)]
        public string FamilyName { get; set; }

        /// <summary>
        /// Chức vụ trong bang
        /// </summary>
        [ProtoMember(40)]
        public int GuildRank { get; set; }

        /// <summary>
        /// Chức vụ trong tộc
        /// </summary>
        [ProtoMember(41)]
        public int FamilyRank { get; set; }

        /// <summary>
        /// Quan hàm
        /// </summary>
        [ProtoMember(42)]
        public int OfficeRank { get; set; }

        /// <summary>
        /// ID danh hiệu hiện tại của bản thân
        /// </summary>
        [ProtoMember(43)]
        public int SelfCurrentTitleID { get; set; }

        /// <summary>
        /// ID mặt nạ
        /// </summary>
        [ProtoMember(44)]
        public int MaskID { get; set; }

        /// <summary>
        /// Danh hiệu đặc biệt
        /// </summary>
        [ProtoMember(45)]
        public int SpecialTitle { get; set; }
    }
}