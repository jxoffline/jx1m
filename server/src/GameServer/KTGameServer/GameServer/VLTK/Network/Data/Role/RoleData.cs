using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Toàn bộ thông tin nhân vạt
    /// </summary>
    [ProtoContract]
    public class RoleData
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên nhân vật
        /// </summary>
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Giới tính (0: Nam, 1: Nữ)
        /// </summary>
        [ProtoMember(3)]
        public int RoleSex { get; set; }

        /// <summary>
        /// ID môn phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID { get; set; }

        /// <summary>
        /// Cấp độ nhân vật
        /// </summary>
        [ProtoMember(5)]
        public int Level { get; set; }

        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(6)]
        public int GuildID { get; set; }

        /// <summary>
        /// Bạc
        /// </summary>
        [ProtoMember(7)]
        public int Money { get; set; }

        /// <summary>
        /// ID tộc
        /// </summary>
        [ProtoMember(8)]
        public int FamilyID { get; set; }

        /// <summary>
        /// Kinh nghiệm
        /// </summary>
        [ProtoMember(9)]
        public long Experience { get; set; }

        /// <summary>
        /// Chế độ PK
        /// </summary>
        [ProtoMember(10)]
        public int PKMode { get; set; }

        /// <summary>
        /// Giá trị PK
        /// </summary>
        [ProtoMember(11)]
        public int PKValue { get; set; }

        /// <summary>
        /// Bản đồ đang đứng hiện tại
        /// </summary>
        [ProtoMember(12)]
        public int MapCode { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(13)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(14)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng nhân vật
        /// </summary>
        [ProtoMember(15)]
        public int RoleDirection { get; set; }

        /// <summary>
        /// Giá trị máu hiện tại của nhân vật
        /// </summary>
        [ProtoMember(16)]
        public int CurrentHP { get; set; }

        /// <summary>
        /// Giá trị HP tối đa của nhân vật
        /// </summary>
        [ProtoMember(17)]
        public int MaxHP { get; set; }

        /// <summary>
        /// Giá trị MP hiện tại của nhân vật
        /// </summary>
        [ProtoMember(18)]
        public int CurrentMP { get; set; }

        /// <summary>
        /// Giá trị máu tối đa của nhân vật
        /// </summary>
        [ProtoMember(19)]
        public int MaxMP { get; set; }

        /// <summary>
        /// Thể lực hiện tại của nhân vật
        /// </summary>
        [ProtoMember(20)]
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Giá trị thể lực tối đa của nhân vật
        /// </summary>
        [ProtoMember(21)]
        public int MaxStamina { get; set; }

        /// <summary>
        /// Tinh lực
        /// </summary>
        [ProtoMember(22)]
        public int GatherPoint { get; set; }

        /// <summary>
        /// Hoạt lực
        /// </summary>
        [ProtoMember(23)]
        public int MakePoint { get; set; }

        /// <summary>
        /// Avata đại diện của nhân vật
        /// </summary>
        [ProtoMember(24)]
        public int RolePic { get; set; }

        /// <summary>
        /// Số ô đồ đã mở
        /// </summary>
        [ProtoMember(25)]
        public int BagNum { get; set; }

        /// <summary>
        /// Nhiệm vụ của nhân vật
        /// </summary>
        [ProtoMember(26)]
        public List<TaskData> TaskDataList { get; set; }

        /// <summary>
        /// Danh sách vật phẩm trong túi đồ
        /// </summary>
        [ProtoMember(27)]
        public List<GoodsData> GoodsDataList { get; set; }

        /// <summary>
        /// Danh Sách Skill
        /// </summary>
        [ProtoMember(28)]
        public List<SkillData> SkillDataList { get; set; }

        /// <summary>
        /// Trạng thái nhiệm vụ
        /// </summary>
        [ProtoMember(29)]
        public List<NPCTaskState> NPCTaskStateList { get; set; }

        /// <summary>
        /// Danh sách kỹ năng chính
        /// </summary>
        [ProtoMember(30)]
        public string MainQuickBarKeys { get; set; }

        /// <summary>
        /// Danh sách kỹ năng vòng sáng
        /// </summary>
        [ProtoMember(31)]
        public string OtherQuickBarKeys { get; set; }

        /// <summary>
        /// Đồng
        /// </summary>
        [ProtoMember(32)]
        public int Token { get; set; }

        /// <summary>
        /// Tên gian hàng
        /// </summary>
        [ProtoMember(33)]
        public string StallName { get; set; }

        /// <summary>
        /// ID Team hiện tại
        /// </summary>
        [ProtoMember(34)]
        public int TeamID { get; set; }

        /// <summary>
        /// ID của đội trưởng
        /// </summary>
        [ProtoMember(35)]
        public int TeamLeaderRoleID { get; set; }

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        [ProtoMember(36)]
        public List<BufferData> BufferDataList { get; set; }

        /// <summary>
        /// Zone ID của máy chủ
        /// </summary>
        [ProtoMember(37)]
        public int ZoneID { get; set; }

        /// <summary>
        /// Cấp bậc trong Bang
        /// </summary>
        [ProtoMember(38)]
        public int GuildRank { get; set; }

        /// <summary>
        /// Cấp bậc trong tộc
        /// </summary>
        [ProtoMember(39)]
        public int FamilyRank { get; set; }

        /// <summary>
        /// Đồng khóa
        /// </summary>
        [ProtoMember(40)]
        public int BoundToken { get; set; }

        /// <summary>
        ///  Bạc khóa hiện có
        /// </summary>
        [ProtoMember(41)]
        public int BoundMoney { get; set; }

        /// <summary>
        /// Bạc ở kho
        /// </summary>
        [ProtoMember(42)]
        public int StoreMoney { get; set; }

        /// <summary>
        /// Có phải GM không
        /// </summary>
        [ProtoMember(43)]
        public int GMAuth { get; set; }

        /// <summary>
        /// Kinh nghiệm thăng cấp
        /// </summary>
        [ProtoMember(44)]
        public long MaxExperience { get; set; }

        /// <summary>
        /// ID nhánh tu luyện
        /// </summary>
        [ProtoMember(45)]
        public int SubID { get; set; }

        /// <summary>
        /// Vinh dự võ lâm
        /// </summary>
        [ProtoMember(46)]
        public int WorldHonor { get; set; }

        /// <summary>
        /// Uy Danh
        /// </summary>
        [ProtoMember(47)]
        public int Prestige { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// </summary>
        [ProtoMember(48)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// Điểm kỹ năng
        /// </summary>
        [ProtoMember(49)]
        public int SkillPoint { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        [ProtoMember(50)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Cờ PK
        /// </summary>
        [ProtoMember(51)]
        public int Camp { get; set; }

        /// <summary>
        /// Thông số thiết lập Auto
        /// </summary>
        [ProtoMember(52)]
        public string AutoSettings { get; set; }

        /// <summary>
        /// Thiết lập hệ thống
        /// </summary>
        [ProtoMember(53)]
        public string SystemSettings { get; set; }

        /// <summary>
        /// Có đang cưỡi không
        /// </summary>
        [ProtoMember(54)]
        public bool IsRiding { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// </summary>
        [ProtoMember(55)]
        public int CastSpeed { get; set; }

        /// <summary>
        /// Danh sách kỹ năng sống
        /// </summary>
        [ProtoMember(56)]
        public Dictionary<int, LifeSkillPram> LifeSkills { get; set; }

        /// <summary>
        /// Danh hiệu tạm thời
        /// </summary>
        [ProtoMember(57)]
        public string Title { get; set; }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        [ProtoMember(58)]
        public string GuildTitle { get; set; }

        /// <summary>
        /// Danh sách danh vọng
        /// </summary>
        [ProtoMember(59)]
        public List<ReputeInfo> Repute { get; set; }

        /// <summary>
        /// Vinh dự tài phú
        /// </summary>
        [ProtoMember(60)]
        public long TotalValue { get; set; }

        /// <summary>
        /// Thông tin nhiệm vụ đã hoàn thành
        /// </summary>
        [ProtoMember(61)]
        public List<QuestInfo> QuestInfo { get; set; }

        /// <summary>
        /// Quan hàm
        /// </summary>
        [ProtoMember(62)]
        public int OfficeRank { get; set; }

        /// <summary>
        /// Danh sách danh hiệu bản thân
        /// </summary>
        [ProtoMember(63)]
        public List<int> SelfTitles { get; set; }

        /// <summary>
        /// ID danh hiệu hiện tại của bản thân
        /// </summary>
        [ProtoMember(64)]
        public int SelfCurrentTitleID { get; set; }

        /// <summary>
        /// Sức mạnh
        /// </summary>

        [ProtoMember(65)]
        public int m_wStrength { get; set; }

        /// <summary>
        /// Thân pháp
        /// </summary>

        [ProtoMember(66)]
        public int m_wDexterity { get; set; }

        /// <summary>
        /// Sinh khí
        /// </summary>
        [ProtoMember(67)]
        public int m_wVitality { get; set; }

        /// <summary>
        /// Nội lực
        /// </summary>
        [ProtoMember(68)]
        public int m_wEnergy { get; set; }

        /// <summary>
        /// Danh sách vật phẩm dùng ở khay dùng nhanh
        /// </summary>
        [ProtoMember(69)]
        public string QuickItems { get; set; } = "";

        /// <summary>
        /// ID Pet đang tham chiến hiện tại
        /// </summary>
        [ProtoMember(70)]
        public int CurrentPetID { get; set; }

        /// <summary>
        /// Danh sách pet
        /// </summary>
        [ProtoMember(71)]
        public List<PetData> Pets { get; set; }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        [ProtoMember(72)]
        public int SpecialTitleID { get; set; }



        /// <summary>
        /// Guild Money Tiền bang hội
        /// </summary>
        [ProtoMember(73)]
        public int GuildMoney { get; set; }
    }
}
