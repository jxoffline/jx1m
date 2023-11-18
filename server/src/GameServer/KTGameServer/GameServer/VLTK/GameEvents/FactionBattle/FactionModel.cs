using GameServer.Logic;
using ProtoBuf;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace GameServer.KiemThe.GameEvents.FactionBattle
{
    [ProtoContract]
    public class ELIMINATION_SCOREBOARD
    {
        /// <summary>
        /// Tên người chơi 1
        /// </summary>
        ///
        [ProtoMember(1)]
        public string Player_1 { get; set; }

        /// <summary>
        /// Tên người chơi 2
        /// </summary>
        ///
        [ProtoMember(2)]
        public string Player_2 { get; set; }

        /// <summary>
        /// Trạng thái của trận
        /// </summary>
        ///
        [ProtoMember(3)]
        public ROUNDSTATE _ROUNDSTATE { get; set; }

        /// <summary>
        /// Round này là round mấy
        /// </summary>
        ///
        [ProtoMember(4)]
        public int ROUNDID { get; set; }

        /// <summary>
        /// Arena nào
        /// </summary>
        ///
        [ProtoMember(5)]
        public int ARENAID { get; set; }
    }

    [ProtoContract]
    public class FACTION_PVP_RANKING_INFO
    {
        /// <summary>
        /// Danh sách xếp hạng người chơi
        /// </summary>
        [ProtoMember(1)]
        public List<FACTION_PVP_RANKING> PlayerRanks { get; set; }

        /// <summary>
        /// Hạng hiện tại của người chơi
        /// </summary>
        [ProtoMember(2)]
        public int PlayRank { get; set; }

        /// <summary>
        /// Danh sách bản thi đấu
        /// </summary>
        [ProtoMember(3)]
        public List<ELIMINATION_SCOREBOARD> ELIMINATION_SCORE { get; set; }

        /// <summary>
        /// Trạng thái SHOW BẢNG XẾP HẠNG
        /// Nếu là 0 thì show PVP RANKING PK TỰ DO
        /// Nếu là 1 thì show RANK BẢNG SOLO 1VS1
        /// </summary>
        [ProtoMember(4)]
        public int State { get; set; }
    }

    /// <summary>
    /// Xếp hạng tống kim
    /// </summary>
    [ProtoContract]
    public class FACTION_PVP_RANKING
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
    }

    /// <summary>
    /// Thông tin các lôi đài
    /// </summary>
    public class ELIMINATION_INFO
    {
        /// <summary>
        /// Ngươi chơi 1
        /// </summary>
        public FactionPlayer Player_1 { get; set; }

        /// <summary>
        /// Người chơi 2
        /// </summary>
        public FactionPlayer Player_2 { get; set; }

        public ROUNDSTATE _ROUNDSTATE { get; set; }

        public bool IsCreateBox { get; set; }

        /// <summary>
        /// Thằng nào thắng ROUND này
        /// </summary>
        public FactionPlayer WinThisRound { get; set; }

        /// <summary>
        /// Round này là round mấy
        /// </summary>
        public int ROUNDID { get; set; }

        /// <summary>
        /// Arena nào
        /// </summary>
        public int ARENAID { get; set; }
    }

    public class FactionPlayer
    {
        /// <summary>
        /// Chính là số mạng đã giết
        /// </summary>
        public int nScore { get; set; }

        public int nArenaId { get; set; }

        public int KillStreak { get; set; }

        public int MaxKillStreak { get; set; } = 0;

        public int Rank { get; set; }

        public Point LastPosition { get; set; }

        public int BestElimateRank { get; set; }

        public KPlayer player { get; set; }

        public int nDeathCount { get; set; }

        public bool IsWinRound { get; set; }

        public bool IsReconnect { get; set; } = false;

        public bool ReviceTanNhanVuong { get; set; }

        public bool AreadyGetAward { get; set; }

        public int TotalFlagCollect { get; set; }

        /// <summary>
        ///  Bảng logs ghi lại sát thương mà thằng này gây ra
        /// </summary>
        public long DamgeRecore { get; set; }
    }

    public enum ROUNDSTATE
    {
        NONE = 0,
        PREADING = 1,
        START = 2,
        END = 3,
    }

    public enum FactionState
    {
        /// <summary>
        /// Khong có gì
        /// </summary>
        NOTHING = 0,

        /// <summary>
        /// Đăng ký
        /// </summary>
        SIGN_UP = 1,

        /// <summary>
        /// Tham gia hỗn chiến
        /// </summary>
        MELEE_ROUND_1,

        /// <summary>
        /// Nghỉ 3p
        /// </summary>
        FREE_MELEE,

        /// <summary>
        /// Bắt đầu tham gia hỗn chiến hiệp 2
        /// </summary>
        MELEE_ROUND_2,

        /// <summary>
        /// Chuẩn bị nhặt cờ | Thông báo bản xếp hạng
        /// </summary>
        READY_ELIMINATION,

        /// <summary>
        /// Bắt đầu đánh vòng 1 |  16 người đấu với nnau
        /// </summary>
        ELIMINATION_ROUND_1,

        /// <summary>
        /// Nghỉ vòng 1 đi nhặt cờ
        /// </summary>
        FREE_ELIMINATION_ROUND_1,

        /// <summary>
        /// Bắt đầu vòng 2 | 8 người đấu với nhau
        /// </summary>
        ELIMINATION_ROUND_2,

        /// <summary>
        /// Nghỉ vòng 2
        /// </summary>
        FREE_ELIMINATION_ROUND_2,

        /// <summary>
        /// Bắt đầu vòng 3 | 4 người đấu với nhau
        /// </summary>
        ELIMINATION_ROUND_3,

        /// <summary>
        /// Nghỉ vòng 2
        /// </summary>
        FREE_ELIMINATION_ROUND_3,

        /// <summary>
        /// Bắt đầu vòng 4 | 2 người đấu với nhau
        /// </summary>
        ELIMINATION_ROUND_4,

        /// <summary>
        /// Thời gian nhận thưởng | Thông báo tân nhân vương
        /// </summary>
        CHAMPION_AWARD,

        /// <summary>
        ///  Kết thúc
        /// </summary>

        END
    }

    [XmlRoot(ElementName = "FactionMap")]
    public class FactionMap
    {
        /// <summary>
        /// Phái nào
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "FactionID")]
        public int FactionID { get; set; }

        /// <summary>
        /// Bản đồ nào
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MapId")]
        public int MapId { get; set; }
    }

    /// <summary>
    /// Điểm hồi sinh random trong PVP
    /// </summary>
    ///
    [XmlRoot(ElementName = "Postion")]
    public class Postion
    {
        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }
    }

    /// <summary>
    /// Config khu vực hỗn chiến
    /// </summary>
    ///
    [XmlRoot(ElementName = "PVPArena")]
    public class PVPArena
    {
    }

    /// <summary>
    /// Thông tin toàn bộ danh sách đấu trường
    /// </summary>
    ///
    [XmlRoot(ElementName = "ArenaPos")]
    public class ArenaPos
    {
        /// <summary>
        /// Arena ID
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "ArenaID")]
        public int ArenaID { get; set; }

        /// <summary>
        /// Pos X
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        /// <summary>
        /// PosY
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }

        public Point ConvertToPoint
        {
            get
            {
                return new Point(PosX, PosY);
            }
        }
    }

    [XmlRoot(ElementName = "FactionAward")]
    public class FactionAward
    {
        [XmlAttribute(AttributeName = "TopRank")]
        public int TopRank { get; set; }

        [XmlAttribute(AttributeName = "Point")]
        public int Point { get; set; }

        [XmlAttribute(AttributeName = "PointType")]
        public int PointType { get; set; }

        [XmlAttribute(AttributeName = "ItemList")]
        public string ItemList { get; set; }
    }

    [XmlRoot(ElementName = "TimeBattle")]
    public class TimeBattle
    {
        [XmlAttribute(AttributeName = "Hours")]
        public int Hours { get; set; }

        [XmlAttribute(AttributeName = "Minute")]
        public int Minute { get; set; }

        [XmlAttribute(AttributeName = "Second")]
        public int Second { get; set; }
    }

    [XmlRoot(ElementName = "TimeActivty")]
    public class TimeActivty
    {
        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "SIGN_UP_DULATION")]
        public int SIGN_UP_DULATION { get; set; }

        /// <summary>
        /// Thời gian hỗn chiến
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MELEE_PVP_DULATION")]
        public int MELEE_PVP_DULATION { get; set; }

        /// <summary>
        /// Thời gian nghỉ hỗn chiến
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MELEE_PVP_FREE_DUALTION")]
        public int MELEE_PVP_FREE_DUALTION { get; set; }

        /// <summary>
        /// Thời gian nghỉ PVP nhặt cờ
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "FREE_ELIMINATION")]
        public int FREE_ELIMINATION { get; set; }

        /// <summary>
        /// Thời gian thi đấu
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "ELIMINATION_ROUND_ROUND")]
        public int ELIMINATION_ROUND_ROUND { get; set; }

        /// <summary>
        /// Thời gian nhận thưởng
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "REVICE_AWARD")]
        public int REVICE_AWARD { get; set; }
    }

    [XmlRoot(ElementName = "Box")]
    public class Box
    {
        [XmlElement(ElementName = "RandomBox")]
        public List<Postion> RandomBox { get; set; }

        [XmlAttribute(AttributeName = "ArenaID")]
        public int ArenaID { get; set; }
    }

    [XmlRoot(ElementName = "TitleFaction")]
    public class TitleFaction
    {
        [XmlAttribute(AttributeName = "FactionID")]
        public int FactionID { get; set; }

        [XmlAttribute(AttributeName = "TitleID")]
        public int TitleID { get; set; }
    }

    /// <summary>
    /// Định nghĩa thi đấu môn phái
    /// </summary>
    ///
    [XmlRoot(ElementName = "FactionDef")]
    public class FactionDef
    {
        /// <summary>
        /// Số người tham dự tối đa
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MAX_ATTEND_PLAYER")]
        public int MAX_ATTEND_PLAYER { get; set; }

        /// <summary>
        /// Số người đăng ký tối thiểu có thể chạy
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "MIN_ATTEND_PLAYER")]
        public int MIN_ATTEND_PLAYER { get; set; }

        /// <summary>
        /// Mở vào ngày nào trong tuần
        /// </summary>
        ///
        [XmlElement(ElementName = "OPEN_WEEK_DATE")]
        public List<int> OPEN_WEEK_DATE { get; set; }

        /// <summary>
        /// Danh sách các bản đồ sẽ diễn ra sự kiện
        /// </summary>
        ///
        [XmlElement(ElementName = "MapFactionList")]
        public List<FactionMap> MapFactionList { get; set; }

        /// <summary>
        /// Danh sách các lôi đài và tọa độ
        /// </summary>
        ///
        [XmlElement(ElementName = "TotalArena")]
        public List<ArenaPos> TotalArena { get; set; }

        /// <summary>
        /// Tọa độ mới đầu vào
        /// </summary>
        [XmlElement(ElementName = "GoIn")]
        public Postion GoIn { get; set; }

        /// <summary>
        /// Tọa độ random cờ
        /// </summary>
        [XmlElement(ElementName = "RandomFlag")]
        public List<Postion> RandomFlag { get; set; }

        /// <summary>
        /// Tọa độ random BOX
        /// </summary>
        [XmlElement(ElementName = "Box")]
        public List<Box> Box { get; set; }

        /// <summary>
        /// Tọa độ hồi sinh ngẫu nhiên tỏng PVP hỗn chiến
        /// </summary>

        [XmlElement(ElementName = "PVPRandomRevice")]
        public List<Postion> PVPRandomRevice { get; set; }



        /// <summary>
        /// dANH SÁCH DANH HIỆU
        /// </summary>
        [XmlElement(ElementName = "TitleFaction")]
        public List<TitleFaction> TitleFaction { get; set; }


        /// <summary>
        /// dANH SÁCH QUÀ TẶNG
        /// </summary>
        [XmlElement(ElementName = "FactionAward")]
        public List<FactionAward> FactionAward { get; set; }

        /// <summary>
        /// Cấp độ thấp nhất có thể tham gia
        /// </summary>
        [XmlAttribute(AttributeName = "MIN_LEVEL")]
        public int MIN_LEVEL { get; set; }

        /// <summary>
        /// Thời gian của các sự kiện
        /// </summary>
        [XmlElement(ElementName = "TimeAct")]
        public TimeActivty TimeAct { get; set; }

        [XmlElement(ElementName = "TimeBattle")]
        public TimeBattle TimeBattle { get; set; }
    }
}