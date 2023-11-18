using GameServer.Logic;
using ProtoBuf;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Logic.Manager.Battle
{
    public class BattleNotify
    {
        public int Rank { get; set; }
        public int Kill { get; set; }
        public int BeKill { get; set; }
        public int Score { get; set; }
    }

    /// <summary>
    /// Thông tin xếp hạng Tống Kim
    /// </summary>
    [ProtoContract]
    public class SongJinBattleRankingInfo
    {
        /// <summary>
        /// Danh sách xếp hạng người chơi
        /// </summary>
        [ProtoMember(1)]
        public List<SongJinRanking> PlayerRanks { get; set; }

        /// <summary>
        /// Tổng điểm phe Tống
        /// </summary>
        [ProtoMember(2)]
        public string SongTotalScore { get; set; }

        /// <summary>
        /// Tổng điểm phe Kim
        /// </summary>
        [ProtoMember(3)]
        public string JinTotalScore { get; set; }
    }

    /// <summary>
    /// Xếp hạng tống kim
    /// </summary>
    [ProtoContract]
    public class SongJinRanking
    {
        /// <summary>
        /// Hạng
        /// </summary>
        [ProtoMember(1)]
        public int Rank { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(2)]
        public string PlayerName { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// Môn phái
        /// </summary>
        [ProtoMember(4)]
        public int Faction { get; set; }

        /// <summary>
        /// Số giết
        /// </summary>
        [ProtoMember(5)]
        public int KillCount { get; set; }

        /// <summary>
        /// Điểm
        /// </summary>
        [ProtoMember(6)]
        public int Score { get; set; }

        /// <summary>
        /// Liên trảm tối đa
        /// </summary>
        [ProtoMember(7)]
        public int MaxKillStreak { get; set; }

        /// <summary>
        /// Phe
        /// </summary>
        [ProtoMember(8)]
        public string Camp { get; set; }
    }

    public class BattlePlayer
    {
        public KPlayer Player { get; set; }

        public int Kill { get; set; }

        public int BeKill { get; set; }

        public int Rank { get; set; }

        public Point LastPoint { get; set; }
        public int Score { get; set; } = 0;

        public int Camp { get; set; }

        public int KillStreak { get; set; }

        public int MaxKillStreak { get; set; }

        public bool IsReviceAward { get; set; }

        public BattlePlayer(KPlayer _Player, int _Kill, int _BeKill, int _Rank, int _Score, int _KillStreak, int _Camp, int _MaxKillStreak)
        {
            this.Player = _Player;
            this.Kill = _Kill;
            this.BeKill = _BeKill;
            this.Rank = _Rank;
            this.Score = _Score;
            this.KillStreak = _KillStreak;
            this.MaxKillStreak = _MaxKillStreak;
            this.Camp = _Camp;
            this.IsReviceAward = false;
        }
    }

    public enum BattelStatus
    {
        STATUS_NULL = 0,
        STATUS_PREPARE = 1,
        STATUS_START = 2,
        STATUS_PREPAREEND = 3,
        STATUS_END = 4,
        STATUS_CLEAR = 5,
    }

    [XmlRoot(ElementName = "BattelAward")]
    public class BattelAward
    {
        [XmlAttribute(AttributeName = "Score")]
        public int Score { get; set; }

        [XmlAttribute(AttributeName = "Point")]
        public int Point { get; set; }

        [XmlAttribute(AttributeName = "PointType")]
        public int PointType { get; set; }

        [XmlAttribute(AttributeName = "Money")]
        public int Money { get; set; }

     
    }

    [XmlRoot(ElementName = "BattelAwardExtras")]
    public class BattelAwardExtras
    {
        [XmlAttribute(AttributeName = "Rank")]
        public int Rank { get; set; }

        [XmlAttribute(AttributeName = "Point")]
        public int Point { get; set; }

        [XmlAttribute(AttributeName = "PointType")]
        public int PointType { get; set; }

        [XmlAttribute(AttributeName = "ItemList")]
        public string ItemList { get; set; }
    }

    [XmlRoot(ElementName = "BattleRank")]
    public class BattleRank
    {
        [XmlAttribute(AttributeName = "RankID")]
        public int RankID { get; set; }

        [XmlAttribute(AttributeName = "RankTitle")]
        public string RankTitle { get; set; }

        [XmlAttribute(AttributeName = "Color")]
        public string Color { get; set; }

        [XmlAttribute(AttributeName = "Score")]
        public int Score { get; set; }
    }

    [XmlRoot(ElementName = "Region")]
    public class Region
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "RegionCamp")]
        public int RegionCamp { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }
    }

    [XmlRoot(ElementName = "TimeBattle")]
    public class TimeBattle
    {
        [XmlAttribute(AttributeName = "Hours")]
        public int Hours { get; set; }

        [XmlAttribute(AttributeName = "Minute")]
        public int Minute { get; set; }
    }

    [XmlRoot(ElementName = "MonsterBattle")]
    public class MonsterBattle
    {
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "IsBoss")]
        public bool IsBoss { get; set; }

        [XmlAttribute(AttributeName = "RebornTime")]
        public int RebornTime { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }

        [XmlAttribute(AttributeName = "AIType")]
        public int AIType { get; set; }

        [XmlAttribute(AttributeName = "HP")]
        public int HP { get; set; }

        [XmlAttribute(AttributeName = "RespawnTick")]
        public int RespawnTick { get; set; }

        [XmlAttribute(AttributeName = "Camp")]
        public int Camp { get; set; }
    }

    [XmlRoot(ElementName = "MonsterRank")]
    public class MonsterRank
    {
        [XmlAttribute(AttributeName = "MonsterID")]
        public int MonsterID { get; set; }

        [XmlAttribute(AttributeName = "RankID")]
        public int RankID { get; set; }

        [XmlAttribute(AttributeName = "RankTitle")]
        public string RankTitle { get; set; }
    }

    /// <summary>
    /// Thực thể định nghĩa chiến trường
    /// </summary>
    [XmlRoot(ElementName = "BattleConfig")]
    public class BattleConfig
    {
        /// <summary>
        ///  Ngày nào sẽ mở
        /// </summary>
        ///
        [XmlElement(ElementName = "DayOfWeek")]
        public List<int> DayOfWeek { get; set; }

        /// <summary>
        /// Tên chiến trường
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "BattleName")]
        public string BattleName { get; set; }

        [XmlAttribute(AttributeName = "MapID")]
        public int MapID { get; set; }

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "RegisterDualtion")]
        public int RegisterDualtion { get; set; }

        /// <summary>
        /// Thời gian chiến trường
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "BattleDualtion")]
        public int BattleDualtion { get; set; }

        /// <summary>
        /// Thời gian chiến trường
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "ReviceAwardDualtion")]
        public int ReviceAwardDualtion { get; set; }

        /// <summary>
        /// Thời gian mở
        /// </summary>
        ///
        [XmlElement(ElementName = "OpenTime")]
        public List<TimeBattle> OpenTime { get; set; }

        [XmlElement(ElementName = "MonsterRank")]
        public List<MonsterRank> MonsterRank { get; set; }

        /// <summary>
        /// Quái trong chiến trường
        /// </summary>
        ///
        [XmlElement(ElementName = "BattelMonster")]
        public List<MonsterBattle> BattelMonster { get; set; }

        [XmlAttribute(AttributeName = "MinLevel")]
        public int MinLevel { get; set; }

        [XmlAttribute(AttributeName = "MaxLevel")]
        public int MaxLevel { get; set; }

        [XmlElement(ElementName = "Rank")]
        public List<BattleRank> Rank { get; set; }

        [XmlElement(ElementName = "Region")]
        public List<Region> Region { get; set; }

        [XmlElement(ElementName = "BattelAward")]
        public List<BattelAward> BattelAward { get; set; }

        [XmlElement(ElementName = "BattelAwardExtras")]
        public List<BattelAwardExtras> BattelAwardExtras { get; set; }
    }
}