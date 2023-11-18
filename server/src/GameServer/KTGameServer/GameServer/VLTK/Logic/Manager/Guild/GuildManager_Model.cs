using GameServer.KiemThe.Entities;
using GameServer.Logic;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.GuildManager
{
    /// <summary>
    /// Thực thể lưu trữ t
    /// </summary>
    public class GuildTeamFightMember
    {
        public int GuildID { get; set; }

        public string GuildName { get; set; }

        public List<KPlayer> Members { get; set; }

        public int CityAttack { get; set; }

        /// <summary>
        /// Lưu lại lần nữa cho những thằng online muộn có cơ hội tham gia
        /// </summary>
        public string TeamList { get; set; }
    }

    public enum ROUNDSTATE
    {
        NONE = 0,
        PREADING = 1,
        START = 2,
        END = 3,
    }

    /// <summary>
    /// Thực thể lôi đài bao gồm team nào đấu ới team nào và thắng nào thắng thắng nào thua
    /// </summary>
    public class ELIMINATION_INFO
    {
        /// <summary>
        /// Ngươi chơi 1
        /// </summary>
        public GuildTeamFightMember Team_1 { get; set; }

        /// <summary>
        /// Người chơi 2
        /// </summary>
        public GuildTeamFightMember Team_2 { get; set; }

        public ROUNDSTATE _ROUNDSTATE { get; set; }

        /// <summary>
        /// Thằng nào thắng ROUND này
        /// </summary>
        public GuildTeamFightMember WinThisRound { get; set; }

        /// <summary>
        /// Round này là round mấy
        /// </summary>
        public int ROUNDID { get; set; }
    }

    [XmlRoot(ElementName = "SkillProb")]
    public class SkillProb
    {
        /// <summary>
        /// Symboy thuộc tính
        /// </summary>
        [XmlAttribute(AttributeName = "PropType")]
        public string PropType { get; set; }

        [XmlAttribute(AttributeName = "Value1")]
        public int Value1 { get; set; }

        [XmlAttribute(AttributeName = "Value2")]
        public int Value2 { get; set; }

        [XmlAttribute(AttributeName = "Value3")]
        public int Value3 { get; set; }
    }

    [XmlRoot(ElementName = "ItemRequest")]
    public class ItemRequest
    {
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        [XmlAttribute(AttributeName = "ItemNum")]
        public int ItemNum { get; set; }
    }

    [XmlRoot(ElementName = "SkillGuildLevelData")]
    public class SkillGuildLevelData
    {
        /// <summary>
        /// Cấp độ nào của kỹ năng
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }

        /// <summary>
        /// Những thuộc tính của cấp độ đó
        /// </summary>
        [XmlElement(ElementName = "SkillProb")]
        public List<SkillProb> SkillProb { get; set; }

        [XmlAttribute(AttributeName = "PointUpdate")]
        public int PointUpdate { get; set; }

        [XmlElement(ElementName = "ItemRequest")]
        public List<ItemRequest> ItemRequest { get; set; }
    }

    [XmlRoot(ElementName = "GuildLevelUp")]
    public class GuildLevelUp
    {
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }

        [XmlAttribute(AttributeName = "ExpRequest")]
        public int ExpRequest { get; set; }

        [XmlAttribute(AttributeName = "PointUpdate")]
        public int PointUpdate { get; set; }

        [XmlElement(ElementName = "ItemRequest")]
        public List<ItemRequest> ItemRequest { get; set; }
    }

    [XmlRoot(ElementName = "GuildDef")]
    public class GuildDef
    {
        [XmlAttribute(AttributeName = "RequireItem")]
        public int RequireItem { get; set; }

        [XmlAttribute(AttributeName = "ChangeQuestCost")]
        public int ChangeQuestCost { get; set; }

        [XmlAttribute(AttributeName = "RequireMoney")]
        public int RequireMoney { get; set; }

        [XmlAttribute(AttributeName = "RequireAdditionString")]
        public string RequireAdditionString { get; set; }

        /// <summary>
        /// Một ngày làm được tối đa bao nhiêu nhiệm vụ
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MaxQuestPerDay")]
        public int MaxQuestPerDay { get; set; }

        [XmlElement(ElementName = "LevelUps")]
        public List<GuildLevelUp> LevelUps { get; set; }

        [XmlElement(ElementName = "Skills")]
        public List<SkillGuildInfo> Skills { get; set; }

        [XmlElement(ElementName = "Donates")]
        public DonateRequest Donates { get; set; }
    }

    [XmlRoot(ElementName = "ItemDonate")]
    public class ItemDonate
    {
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        [XmlAttribute(AttributeName = "Point")]
        public int Point { get; set; }
    }

    [XmlRoot(ElementName = "Donate")]
    public class DonateRequest
    {
        /// <summary>
        /// Số điểm cống hiến cá nhân sẽ nhận được trên 1 bạc cống hiến
        /// </summary>

        [XmlAttribute(AttributeName = "PointPerGold")]
        public double PointPerGold { get; set; }

        [XmlElement(ElementName = "ItemDonate")]
        public List<ItemDonate> ItemDonate { get; set; }
    }

    [XmlRoot(ElementName = "SkillGuildInfo")]
    public class SkillGuildInfo
    {
        /// <summary>
        /// ID của kỹ năng
        /// </summary>
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        /// <summary>
        /// Tên của kỹ năng
        /// </summary>

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Icon của kỹ năng sẽ hiển thị ở client
        /// </summary>
        [XmlAttribute(AttributeName = "Icon")]
        public string Icon { get; set; }

        /// <summary>
        ///  Danh sách thuộc tính của kỹ năng bang
        /// </summary>
        [XmlElement(ElementName = "LevelData")]
        public List<SkillGuildLevelData> LevelData { get; set; }

        /// <summary>
        /// Mô tả về kỹ năng này
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "Desc")]
        public string Desc { get; set; }
    }

    [ProtoContract]
    public class GuildInfo
    {
        [ProtoMember(1)]
        public int MoneyGuild { get; set; }

        [ProtoMember(2)]
        public int WeekPoint { get; set; }

        [ProtoMember(3)]
        public MiniGuildInfo _GuildInfo { get; set; }
    }

    [ProtoContract]
    public class GuildTask
    {
        [ProtoMember(1)]
        public int GuildID { get; set; }

        [ProtoMember(2)]
        public int TaskID { get; set; }

        [ProtoMember(3)]
        public int TaskValue { get; set; }

        [ProtoMember(4)]
        public int DayCreate { get; set; } = -1;

        [ProtoMember(5)]
        public int TaskCountInDay { get; set; }
    }

    [ProtoContract]
    public class GuildWar
    {
        [ProtoMember(1)]
        public int GuildID { get; set; }

        [ProtoMember(2)]
        public string GuildName { get; set; }

        [ProtoMember(3)]
        public string TeamList { get; set; }

        [ProtoMember(4)]
        public int WeekID { get; set; }
    }

    [ProtoContract]
    public class MiniGuildInfo
    {
        /// <summary>
        /// ID của bang
        /// </summary>
        [ProtoMember(1)]
        public int GuildId { get; set; }

        /// <summary>
        /// Tên bang hội
        /// </summary>
        [ProtoMember(2)]
        public string GuildName { get; set; }

        /// <summary>
        /// Cấp bang hội
        /// </summary>
        [ProtoMember(3)]
        public int GuilLevel { get; set; }

        /// <summary>
        /// Exp hiện tại của bang
        /// </summary>
        [ProtoMember(4)]
        public int GuildExp { get; set; }

        /// <summary>
        /// Tên bang chủ
        /// </summary>
        [ProtoMember(5)]
        public string HostName { get; set; }

        /// <summary>
        /// Tổng số thành viên
        /// </summary>
        [ProtoMember(6)]
        public int TotalMember { get; set; }

        /// <summary>
        /// Thông tin kỹ năng
        /// </summary>
        [ProtoMember(7)]
        public string SkillNote { get; set; }

        [ProtoMember(8)]
        public int GuildMoney { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cống hiến vào bang
        /// Mã hóa theo string array
        /// </summary>
        [ProtoMember(9)]
        public string ItemStore { get; set; }

        /// <summary>
        /// Công cáo bang hội
        /// </summary>
        [ProtoMember(10)]
        public string GuildNotify { get; set; }

        /// <summary>
        /// Thông tin nhiệm vụ
        /// </summary>
        [ProtoMember(11)]
        public GuildTask Task { get; set; }

        [ProtoMember(12)]
        public GuildWar GuildWar { get; set; }

        [ProtoMember(13)]
        public int IsMainCity { get; set; }

        [ProtoMember(14)]
        public int Total_Copy_Scenes_This_Week { get; set; }

        [ProtoMember(15)]
        public int MoneyBound { get; set; }

        /// Toàn bộ thuộc tính của skill khi INIT
        /// </summary>
        public List<KMagicAttrib> SkillProbsKMagicAttribs { get; set; }

        public bool IsFinishTaskInDay = false;

        /// <summary>
        /// Call hàm này để create AtributeTable
        /// </summary>
        public void IntKMagicAttribs()
        {
            this.SkillProbsKMagicAttribs = new List<KMagicAttrib>();
            if (this.SkillNote.Length > 0)
            {
                string[] Skills = this.SkillNote.Split('|');

                foreach (string _Skill in Skills)
                {
                    string[] Pram = _Skill.Split('_');
                    int SkillID = Int32.Parse(Pram[0]);

                    int SkillLevel = Int32.Parse(Pram[1]);
                    //Tìm ra tmp của skill đã được định nghĩa trong xml

                    var FindTmpSkill = GuildManager._GuildConfig.Skills.Where(x => x.ID == SkillID).FirstOrDefault();
                    if (FindTmpSkill != null)
                    {
                        // Lấy ra thuộc tính của level hiện tại
                        var LevelData = FindTmpSkill.LevelData.Where(x => x.Level == SkillLevel).FirstOrDefault();

                        if (LevelData != null)
                        {
                            foreach (var prob in LevelData.SkillProb)
                            {
                                KMagicAttrib _kMagic = new KMagicAttrib();

                                _kMagic.Init(prob.PropType, prob.Value1, prob.Value2, prob.Value3);

                                this.SkillProbsKMagicAttribs.Add(_kMagic);
                            }
                        }
                    }
                }
            }
        }
    }

    public enum GUILD_RESOURCE
    {
        GUILD_MONEY = 0,
        ITEM = 1,
        SKILL = 2,
        EXP = 3,
        LEVEL = 4,
        JOINEVENT = 5,
        CITYSTATUS = 6,
        GUILD_BOUND_MONEY = 7,
    }

    /// <summary>
    /// Trạng thái công thành của các bang
    /// </summary>
    public enum GUILD_CITY_STATUS
    {
        /// <summary>
        /// Bang này chưa có cái đéo j cả
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Bang này đang là chủ thành
        /// </summary>
        HOSTCITY = 1,

        /// <summary>
        /// Bang này đã đang ký thi đấu
        /// </summary>
        REGISTERFIGHT = 2,

        /// <summary>
        /// Bang này sẽ là bang công thành ngày hôm sau
        /// </summary>
        ATTACKCITY = 3,
    }

    [ProtoContract]
    public class SkillDef
    {
        [ProtoMember(1)] public int SkillID { get; set; }

        [ProtoMember(2)] public int Level { get; set; }
    }

    /// <summary>
    /// Thực thể công thành chiến
    /// </summary>
    ///

    [XmlRoot(ElementName = "CityInfo")]
    ///Danh sách thành và chủ thành
    [ProtoContract]
    public class CityInfo
    {
        [XmlAttribute(AttributeName = "CityName")]
        [ProtoMember(1)]
        public string CityName { get; set; }

        [XmlAttribute(AttributeName = "CityID")]
        [ProtoMember(2)]
        public int CityID { get; set; }

        /// <summary>
        /// thành chủ là bọn nào
        /// </summary>
        [XmlAttribute(AttributeName = "HostName")]
        [ProtoMember(3)]
        public string HostName { get; set; }

        [XmlAttribute(AttributeName = "TeamFightDay")]
        [ProtoMember(4)]
        public int TeamFightDay { get; set; }

        [XmlAttribute(AttributeName = "CityFightDay")]
        [ProtoMember(5)]
        public int CityFightDay { get; set; }

        /// <summary>
        /// Bản đồ sẽ diễn ra công thành chiến
        /// </summary>
        ///

        [XmlAttribute(AttributeName = "FightMapID")]
        public int FightMapID { get; set; }
    }

    public enum GUILD_WAR_STATE
    {
        // Không phải thời gian diễn ra sự kiện
        STATUS_NULL = 0,

        /// <summary>
        /// Chuẩn bị bắt đầu
        /// </summary>
        STATUS_PREPARSTART = 1,

        // Chinh chiến
        STATUS_START = 3,

        // Ngưng chiến
        STATUS_PREPAREEND = 4,

        // Kết quả
        STATUS_END = 5,

        //Rest lại chiến trường
        STATUS_CLEAR = 6,
    }

    public enum SOLOFIGHT_STATE
    {
        // Không phải thời gian diễn ra sự kiện
        FIGHT_NULL = 0,

        /// <summary>
        /// Chuẩn bị bắt đầu
        /// </summary>
        FIGHT_PREPARSTART = 1,

        // Chinh chiến
        /// <summary>
        /// Bắt đầu đánh vòng 1 |  16 người đấu với nnau
        /// </summary>
        FREE_TIME = 2,

        /// <summary>
        /// Nghỉ vòng 1
        /// </summary>
        FIGHT_ELIMINATION_ROUND = 3,

        // Công bố kết quả ghi vào CSDL bang thủ và bang công
        FIGHT_FIGHT_END = 4,

        //Rest lại chiến trường
        FIGHT_CLEAR = 5,
    }

    [ProtoContract]
    public class MemberRegister
    {
        [ProtoMember(1)]
        public int RoleID { get; set; }

        [ProtoMember(2)]
        public string RoleName { get; set; }

        [ProtoMember(3)]
        public int Level { get; set; }

        [ProtoMember(4)]
        public int Faction { get; set; }
    }

    [ProtoContract]
    public class GuildWarInfo
    {
        //Danh sách thành viên tham gia lôi đài

        [ProtoMember(1)]
        public List<string> MemberRegister { get; set; }

        // Danh sách bang đã đăng ký

        [ProtoMember(2)]
        public List<string> GuildResgister { get; set; }

        // trạng thái công thành
        // trường status này để xử lý trạng thái cho nút đăng ký công thành
        //0 : Có thể đăng ký công thành | 1 : Đã hết thời gian đăng ký | 2 : Đang diễn ra công thành  |3 : Hết thời gian công thành

        [ProtoMember(3)]
        public int Status { get; set; }

        /// <summary>
        /// Dánh sách các thành sẽ diễn ra công thành
        /// Nhưng theo vận hành nói nên giờ chỉ dùng 1 thành
        /// </summary>
        [ProtoMember(4)]
        public List<CityInfo> ListCity { get; set; }

        /// <summary>
        /// Thành nào sẽ diễn ra công thành trong tuần này
        /// </summary>
        [ProtoMember(5)]
        public int CityFightID { get; set; }

        /// <summary>
        /// Dánh sách bọn người chơi trong bang có thể chọn cho lên lôi đài thi đấu
        /// </summary>
        [ProtoMember(6)]
        public List<MemberRegister> ListMemberCanPick { get; set; }
    }

    /// <summary>
	/// Sửa tôn chỉ bang hội
	/// </summary>
	[ProtoContract]
    public class GuildChangeSlogan
    {
        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(1)]
        public int GuildID { get; set; }

        /// <summary>
        /// Tôn chỉ bang hội
        /// </summary>
        [ProtoMember(2)]
        public string Slogan { get; set; }
    }

    [XmlRoot(ElementName = "TimeBattle")]
    public class TimeBattle
    {
        [XmlAttribute(AttributeName = "Hours")]
        public int Hours { get; set; }

        [XmlAttribute(AttributeName = "Minute")]
        public int Minute { get; set; }
    }

    /// <summary>
    /// Tọa độ đặt các đối tượng trên bản đồ
    /// </summary>
    ///
    [XmlRoot(ElementName = "ObjectivePostion")]
    public class ObjectivePostion
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "Hp")]
        public int Hp { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }

        [XmlAttribute(AttributeName = "IsMonster")]
        public bool IsMonster { get; set; }
    }

    ///
    [XmlRoot(ElementName = "DefMonster")]
    public class DefMonster
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "Hp")]
        public int Hp { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }

        [XmlAttribute(AttributeName = "IsMonster")]
        public bool IsMonster { get; set; }
    }

    [XmlRoot(ElementName = "Cost")]
    public class Cost
    {
        [XmlAttribute(AttributeName = "AttackCost")]
        public int AttackCost { get; set; }

        [XmlAttribute(AttributeName = "DefCost")]
        public int DefCost { get; set; }
    }

    [XmlRoot(ElementName = "AttackMonster")]
    public class AttackMonster
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "Hp")]
        public int Hp { get; set; }

        [XmlAttribute(AttributeName = "TargetPosX")]
        public int TargetPosX { get; set; }

        [XmlAttribute(AttributeName = "TargetPosY")]
        public int TargetPosY { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }

        [XmlAttribute(AttributeName = "IsMonster")]
        public bool IsMonster { get; set; }
    }

    /// <summary>
    /// Danh sách phần thưởng
    /// </summary>
    [XmlRoot(ElementName = "GuildWarCityAward")]
    public class GuildWarCityAward
    {
        /// <summary>
        /// Điểm danh vọng tranh đoạt
        /// </summary>
        [XmlAttribute(AttributeName = "Point")]
        public int Point { get; set; }

        /// <summary>
        /// Hạng gì sẽ được gì
        /// </summary>
        [XmlAttribute(AttributeName = "Rank")]
        public int Rank { get; set; }

        /// <summary>
        /// Số lượng hộp sẽ nhận được
        /// </summary>
        [XmlAttribute(AttributeName = "BoxCount")]
        public string BoxCount { get; set; }

        /// <summary>
        /// Điểm hoạt động tuần
        /// </summary>
        [XmlAttribute(AttributeName = "WeekPoint")]
        public int WeekPoint { get; set; }

        /// <summary>
        /// Điểm cá cống hiến cá nhân bảng hội sử dụng để mua vật phẩm
        /// </summary>
        [XmlAttribute(AttributeName = "GuildMoney")]
        public int GuildMoney { get; set; }
    }

    /// <summary>
    /// Định nghĩa bên nào thủ bên nào công
    /// </summary>
    public enum WARFLAG
    {
        DEF = 0,

        ATTACK = 1,
    }

    /// <summary>
    /// Công thành giữa bang nào và bang nào
    /// </summary>
    public class GuildWarBattleReport
    {
        public int GuildID { get; set; }

        public string GuildName { get; set; }
        public WARFLAG Flag { get; set; }

        /// <summary>
        /// Xếp hạng hiện tại của bang
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Tổng tích lũy hiện tịa của bang trong trận đánh này
        /// </summary>
        public int TotalScore { get; set; }
    }

    /// <summary>
    /// Thực thể lưu lại bản ghi
    /// </summary>
    public class GuildWarPlayer
    {
        public WARFLAG Flag { get; set; }

        /// <summary>
        /// Thằng người chơi là thằng nào
        /// </summary>
        public KPlayer _Player { get; set; }

        /// <summary>
        /// Ở bang nào
        /// </summary>
        public int GuildID { get; set; }

        /// <summary>
        /// Bao nhiêu điểm
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Phá hủy mấy lần cột trụ
        /// </summary>
        public int DestroyCount { get; set; }

        /// <summary>
        /// Giết bao nhiêu thằng
        /// </summary>
        public int KillCount { get; set; }

        /// <summary>
        /// Hạng hiện tại
        /// </summary>
        public int CurentRank { get; set; }

        /// <summary>
        /// Giết liên tiếp bao nhiêu thằng
        /// </summary>
        public int MaxKillSteak { get; set; }

        /// <summary>
        /// Số lần giết liên tiếp hiện tại
        /// </summary>
        public int CurentKillSteak { get; set; }

        /// <summary>
        /// Đã nhận thưởng hay chưa
        /// </summary>
        public bool IsReviceReward { get; set; }
    }

    /// <summary>
    /// GuildWarConfgi
    /// </summary>
    ///
    /// <summary>
    /// Điểm hồi sinh
    /// </summary>
    ///
    [XmlRoot(ElementName = "RespawnPoint")]
    public class RespawnPoint
    {
        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }
    }

    [XmlRoot(ElementName = "GuildWarCityConfig")]
    public class GuildWarCityConfig
    {
        /// <summary>
        /// Dánh sách các thành sẽ tổ chức công thành chiến
        /// </summary>
        ///
        [XmlElement(ElementName = "Citys")]
        public List<CityInfo> Citys { get; set; }

        /// <summary>
        /// Danh sách phần thưởng dựa trên hạng của thằng người chơi
        /// </summary>
        ///
        [XmlElement(ElementName = "Award")]
        public List<GuildWarCityAward> Award { get; set; }

        /// <summary>
        /// Giờ nào sẽ bắt đàu sự kiện
        /// </summary>
        ///

        [XmlElement(ElementName = "OpenTime")]
        public TimeBattle OpenTime { get; set; }

        [XmlElement(ElementName = "Cost")]
        public Cost CostPrice { get; set; }

        /// <summary>
        /// Vị trí đặt các đối tượng trong bản đồ
        /// </summary>
        [XmlElement(ElementName = "ObjectPostion")]
        public List<ObjectivePostion> ObjectPostion { get; set; }

        [XmlElement(ElementName = "DefMonster")]
        public List<DefMonster> DefMonster { get; set; }

        [XmlElement(ElementName = "AttackMonster")]
        public List<AttackMonster> AttackMonster { get; set; }

        /// <summary>
        /// Danh sách các điểm của bang thủ thành
        /// </summary>
        [XmlElement(ElementName = "DefPoint")]
        public List<RespawnPoint> DefPoint { get; set; }

        /// <summary>
        /// Danh sách điểm của bang công thành
        /// </summary>
        [XmlElement(ElementName = "AttackPoint")]
        public List<RespawnPoint> AttackPoint { get; set; }
    }
}