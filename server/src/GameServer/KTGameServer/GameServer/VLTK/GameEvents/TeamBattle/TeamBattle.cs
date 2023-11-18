using GameServer.KiemThe.GameEvents.SpecialEvent;
using GameServer.Logic;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.TeamBattle
{
    /// <summary>
    /// Võ lâm liên đấu
    /// </summary>
    public static class TeamBattle
    {
        #region Define
        /// <summary>
        /// Định nghĩa thông tin chiến đội
        /// </summary>
        [ProtoContract]
        public class TeamBattleInfo
        {
            /// <summary>
            /// ID
            /// </summary>
            [ProtoMember(1)]
            public int ID { get; set; }

            /// <summary>
            /// Tên chiến đội
            /// </summary>
            [ProtoMember(2)]
            public string Name { get; set; }

            /// <summary>
            /// Danh sách thành viên tương ứng
            /// </summary>
            [ProtoMember(3)]
            public Dictionary<int, string> Members { get; set; }

            /// <summary>
            /// Thời gian đăng ký
            /// </summary>
            [ProtoMember(4)]
            public DateTime RegisterTime { get; set; }

            /// <summary>
            /// Số điểm
            /// </summary>
            [ProtoMember(5)]
            public int Point { get; set; }

            /// <summary>
            /// Tổng số trận thi đấu vòng tròn đã tham gia
            /// </summary>
            [ProtoMember(6)]
            public int TotalBattles { get; set; }

            /// <summary>
            /// Hạng chiến đấu của chiến đội
            /// </summary>
            [ProtoMember(7)]
            public int Stage { get; set; }

            /// <summary>
            /// Xếp hạng chiến đội tính đến hôm nay
            /// </summary>
            [ProtoMember(8)]
            public int Rank { get; set; }

            /// <summary>
            /// Có phần thưởng để nhận không
            /// </summary>
            [ProtoMember(9)]
            public bool HasAwards { get; set; }

            /// <summary>
            /// Thời điểm cập nhật xếp hạng chiến đội
            /// </summary>
            [ProtoMember(10)]
            public DateTime LastUpdateRankTime { get; set; } = DateTime.MinValue;

            /// <summary>
            /// Thời điểm thắng cuộc lần trước
            /// </summary>
            [ProtoMember(11)]
            public DateTime LastWinTime { get; set; } = DateTime.MinValue;

            /// <summary>
            /// Tạo ra bản sao của đối tượng
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public TeamBattleInfo Clone()
            {
                return new TeamBattleInfo()
                {
                    ID = this.ID,
                    Name = this.Name,
                    Members = this.Members,
                    RegisterTime = this.RegisterTime,
                    Point = this.Point,
                    TotalBattles = this.TotalBattles,
                    Stage = this.Stage,
                    Rank = this.Rank,
                    HasAwards = this.HasAwards,
                    LastUpdateRankTime = this.LastUpdateRankTime,
                    LastWinTime = this.LastWinTime,
                };
            }
        }

        /// <summary>
        /// Thiết lập sự kiện
        /// </summary>
        public class EventConfig
        {
            /// <summary>
            /// ID bản đồ hội trường
            /// </summary>
            public int EnterMapID { get; set; }

            /// <summary>
            /// Vị trí X vào bản đồ hội trường
            /// </summary>
            public int EnterPosX { get; set; }

            /// <summary>
            /// Vị trí Y vào bản đồ hội trường
            /// </summary>
            public int EnterPosY { get; set; }

            /// <summary>
            /// Cấp độ tối thiểu tham gia
            /// </summary>
            public int MinLevel { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EventConfig Parse(XElement xmlNode)
            {
                return new EventConfig()
                {
                    EnterMapID = int.Parse(xmlNode.Attribute("EnterMapID").Value),
                    EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
                    EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
                    MinLevel = int.Parse(xmlNode.Attribute("MinLevel").Value),
                };
            }
        }

        /// <summary>
        /// Thiết lập báo danh
        /// </summary>
        public class RegisterConfig
        {
            /// <summary>
            /// Ngày bắt đầu đăng ký
            /// </summary>
            public int FromDay { get; set; }

            /// <summary>
            /// Ngày kết thúc đăng ký
            /// </summary>
            public int ToDay { get; set; }

            /// <summary>
            /// Số thành viên nhóm tham gia
            /// <para>Giá trị trong khoảng <b>[1..4]</b>, <b>-1</b> thì sẽ chọn ngẫu nhiên trong khoảng này</para>
            /// </summary>
            public int TeamCapacity { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static RegisterConfig Parse(XElement xmlNode)
            {
                RegisterConfig registerConfig = new RegisterConfig()
                {
                    FromDay = int.Parse(xmlNode.Attribute("FromDay").Value),
                    ToDay = int.Parse(xmlNode.Attribute("ToDay").Value),
                    TeamCapacity = int.Parse(xmlNode.Attribute("TeamCapacity").Value),
                };

                /// Nếu số thành viên chọn ngẫu nhiên
                if (registerConfig.TeamCapacity == -1)
                {
                    /// Tháng này
                    int currentMonth = DateTime.Now.Month;
                    /// Nếu tháng này chia hết cho 3
                    if (currentMonth % 3 == 0)
                    {
                        /// Tam đấu
                        registerConfig.TeamCapacity = 3;
                    }
                    /// Nếu tháng này chia hết cho 2
                    else if (currentMonth % 2 == 0)
                    {
                        /// Song đấu
                        registerConfig.TeamCapacity = 2;
                    }
                    /// Còn lại
                    else
                    {
                        /// Đơn đấu
                        registerConfig.TeamCapacity = 1;
                    }
                }

                return registerConfig;
            }
        }

        /// <summary>
        /// Thông tin trận đấu
        /// </summary>
        public class BattleInfo
        {
            /// <summary>
            /// Thiết lập trận đấu
            /// </summary>
            public class BattleConfig
            {
                /// <summary>
                /// Thời gian đấu (đơn vị Mili-giây)
                /// </summary>
                public int Duration { get; set; }

                /// <summary>
                /// Thời gian chờ sau khi phân thắng bại mỗi hiệp đấu để tự đẩy ra khỏi đấu trường (đơn vị Mili-giây)
                /// </summary>
                public int FinishWaitDuration { get; set; }

                /// <summary>
                /// ID bản đồ thi đấu
                /// </summary>
                public int MapID { get; set; }

                /// <summary>
                /// Vị trí X tiến vào bản đồ liên đấu
                /// </summary>
                public int PosX { get; set; }

                /// <summary>
                /// Vị trí Y tiến vào bản đồ liên đấu
                /// </summary>
                public int PosY { get; set; }

                /// <summary>
                /// Tổng số trận đấu vòng tròn tối đa được tham dự
                /// </summary>
                public int MaxCircleRoundBattles { get; set; }

                /// <summary>
                /// Tổng số đội được chọn vào vòng chung kết
                /// </summary>
                public int ToFinalRoundTotalTeams { get; set; }

                /// <summary>
                /// Tự xếp hạng các chiến đội vào vòng sau lúc 0:00 ngày tương ứng
                /// </summary>
                public int ArrangeToFinalRoundAtDay { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static BattleConfig Parse(XElement xmlNode)
                {
                    return new BattleConfig()
                    {
                        Duration = int.Parse(xmlNode.Attribute("Duration").Value),
                        FinishWaitDuration = int.Parse(xmlNode.Attribute("FinishWaitDuration").Value),
                        MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                        MaxCircleRoundBattles = int.Parse(xmlNode.Attribute("MaxCircleRoundBattles").Value),
                        ToFinalRoundTotalTeams = int.Parse(xmlNode.Attribute("ToFinalRoundTotalTeams").Value),
                        ArrangeToFinalRoundAtDay = int.Parse(xmlNode.Attribute("ArrangeToFinalRoundAtDay").Value),
                    };
                }
            }

            /// <summary>
            /// Thời gian diễn ra
            /// </summary>
            public class EventTime
            {
                /// <summary>
                /// Thời gian giờ phút
                /// </summary>
                public class TimeStamp
                {
                    /// <summary>
                    /// Giờ
                    /// </summary>
                    public int Hour { get; set; }

                    /// <summary>
                    /// Phút
                    /// </summary>
                    public int Minute { get; set; }
                }

                /// <summary>
                /// Ngày trong tháng
                /// </summary>
                public int Day { get; set; }

                /// <summary>
                /// Các mốc giờ trong ngày
                /// </summary>
                public List<TimeStamp> Times { get; set; }

                /// <summary>
                /// Xếp hạng trận đấu
                /// </summary>
                public int Stage { get; set; }

                /// <summary>
                /// Có tăng bậc của chiến đội cho đội thắng cuộc không
                /// </summary>
                public bool IncreaseStageForWinnerTeam { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventTime Parse(XElement xmlNode)
                {
                    EventTime eventTime = new EventTime()
                    {
                        Day = int.Parse(xmlNode.Attribute("Day").Value),
                        Stage = int.Parse(xmlNode.Attribute("Stage").Value),
                        IncreaseStageForWinnerTeam = bool.Parse(xmlNode.Attribute("IncreaseStageForWinnerTeam").Value),
                        Times = new List<TimeStamp>(),
                    };

                    string eventTimeStrings = xmlNode.Attribute("Times").Value;
                    if (!string.IsNullOrEmpty(eventTimeStrings))
                    {
                        foreach (string eventTimeString in eventTimeStrings.Split(';'))
                        {
                            string[] fields = eventTimeString.Split(':');
                            TimeStamp time = new TimeStamp()
                            {
                                Hour = int.Parse(fields[0]),
                                Minute = int.Parse(fields[1]),
                            };
                            eventTime.Times.Add(time);
                        }
                    }

                    return eventTime;
                }
            }

            /// <summary>
            /// Thiết lập trận đấu
            /// </summary>
            public BattleConfig Config { get; set; }

            /// <summary>
            /// Các mốc thời gian diễn ra sự kiện
            /// </summary>
            public List<EventTime> EventTimes { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static BattleInfo Parse(XElement xmlNode)
            {
                BattleInfo battleInfo = new BattleInfo()
                {
                    Config = BattleConfig.Parse(xmlNode.Element("Config")),
                    EventTimes = new List<EventTime>(),
                };

                foreach (XElement node in xmlNode.Element("EventTimes").Elements("Time"))
                {
                    battleInfo.EventTimes.Add(EventTime.Parse(node));
                }

                return battleInfo;
            }
        }

        /// <summary>
        /// Phần thưởng
        /// </summary>
        public class AwardInfo
        {
            /// <summary>
            /// Thông tin vật phẩm
            /// </summary>
            public class ItemInfo
            {
                /// <summary>
                /// ID vật phẩm
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Số lượng
                /// </summary>
                public int Quantity { get; set; }
            }

            /// <summary>
            /// Thiết lập nhận thưởng
            /// </summary>
            public class AwardConfig
            {
                /// <summary>
                /// Ngày bắt đầu nhận thưởng
                /// </summary>
                public int FromDay { get; set; }

                /// <summary>
                /// Ngày kết thúc nhận thưởng
                /// </summary>
                public int ToDay { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static AwardConfig Parse(XElement xmlNode)
                {
                    return new AwardConfig()
                    {
                        FromDay = int.Parse(xmlNode.Attribute("FromDay").Value),
                        ToDay = int.Parse(xmlNode.Attribute("ToDay").Value),
                    };
                }
            }

            /// <summary>
            /// Phần thưởng trong vòng đấu bất kỳ
            /// </summary>
            public class RoundAward
            {
                /// <summary>
                /// Kinh nghiệm
                /// </summary>
                public int Exp { get; set; }

                /// <summary>
                /// Bạc khóa
                /// </summary>
                public int BoundMoney { get; set; }

                /// <summary>
                /// Danh vọng
                /// </summary>
                public int Repute { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static RoundAward Parse(XElement xmlNode)
                {
                    RoundAward winRoundAward = new RoundAward()
                    {
                        Exp = int.Parse(xmlNode.Attribute("Exp").Value),
                        BoundMoney = int.Parse(xmlNode.Attribute("BoundMoney").Value),
                        Repute = int.Parse(xmlNode.Attribute("Repute").Value),
                    };

                    return winRoundAward;
                }
            }

            /// <summary>
            /// Phần thưởng xếp hạng cuối tháng
            /// </summary>
            public class FinalRankAward
            {
                /// <summary>
                /// Từ thứ hạng
                /// </summary>
                public int FromRank { get; set; }

                /// <summary>
                /// Đến thứ hạng
                /// </summary>
                public int ToRank { get; set; }

                /// <summary>
                /// Kinh nghiệm
                /// </summary>
                public int Exp { get; set; }

                /// <summary>
                /// Danh vọng
                /// </summary>
                public int Repute { get; set; }

                /// <summary>
                /// Danh sách vật phẩm thưởng
                /// </summary>
                public List<ItemInfo> Items { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static FinalRankAward Parse(XElement xmlNode)
                {
                    FinalRankAward finalRankAward = new FinalRankAward()
                    {
                        FromRank = int.Parse(xmlNode.Attribute("FromRank").Value),
                        ToRank = int.Parse(xmlNode.Attribute("ToRank").Value),
                        Exp = int.Parse(xmlNode.Attribute("Exp").Value),
                        Repute = int.Parse(xmlNode.Attribute("Repute").Value),
                        Items = new List<ItemInfo>(),
                    };

                    string itemInfoStrings = xmlNode.Attribute("Items").Value;
                    if (!string.IsNullOrEmpty(itemInfoStrings))
                    {
                        foreach (string itemInfoStr in itemInfoStrings.Split(';'))
                        {
                            string[] fields = itemInfoStr.Split('_');
                            finalRankAward.Items.Add(new ItemInfo()
                            {
                                ID = int.Parse(fields[0]),
                                Quantity = int.Parse(fields[1]),
                            });
                        }
                    }

                    return finalRankAward;
                }
            }

            /// <summary>
            /// Thiết lập nhận thưởng
            /// </summary>
            public AwardConfig Config { get; set; }

            /// <summary>
            /// Phần thưởng khi thắng trong vòng đấu thành phần
            /// </summary>
            public RoundAward WinRound { get; set; }

            /// <summary>
            /// Phần thưởng khi bắt đầu trận
            /// </summary>
            public RoundAward EnterRound { get; set; }

            /// <summary>
            /// Phần thưởng xếp hạng
            /// </summary>
            public List<FinalRankAward> FinalRanks { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static AwardInfo Parse(XElement xmlNode)
            {
                AwardInfo award = new AwardInfo()
                {
                    Config = AwardConfig.Parse(xmlNode.Element("Config")),
                    WinRound = RoundAward.Parse(xmlNode.Element("WinRound")),
                    EnterRound = RoundAward.Parse(xmlNode.Element("EnterRound")),
                    FinalRanks = new List<FinalRankAward>(),
                };

                foreach (XElement node in xmlNode.Element("FinalRank").Elements("RankInfo"))
                {
                    award.FinalRanks.Add(FinalRankAward.Parse(node));
                }

                return award;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Thiết lập sự kiện
        /// </summary>
        public static EventConfig Config { get; set; }

        /// <summary>
        /// Đăng ký
        /// </summary>
        public static RegisterConfig Register { get; set; }

        /// <summary>
        /// Vòng đấu
        /// </summary>
        public static BattleInfo Battle { get; set; }

        /// <summary>
        /// Phần thưởng
        /// </summary>
        public static AwardInfo Award { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Khởi tạo dữ liệu Võ lâm liên đấu
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/TeamBattle.xml");
            TeamBattle.Config = EventConfig.Parse(xmlNode.Element("Config"));
            TeamBattle.Register = RegisterConfig.Parse(xmlNode.Element("RegisterConfig"));
            TeamBattle.Battle = BattleInfo.Parse(xmlNode.Element("Battle"));
            TeamBattle.Award = AwardInfo.Parse(xmlNode.Element("Award"));

            /// Khởi tạo Timer
            TeamBattle_Timer.Init();
        }

        /// <summary>
        /// Kiểm tra người chơi có nằm trong bản đồ Võ lâm liên đấu không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInTeamBattleMap(KPlayer player)
        {
            if (TeamBattle.Config == null)
            {
                return false;
            }

            return player.CurrentMapCode == TeamBattle.Config.EnterMapID;
        }

        /// <summary>
        /// Kiểm tra người chơi có nằm trong lôi đài Võ lâm liên đấu không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInTeamBattlePKMap(KPlayer player)
        {
            if (TeamBattle.Config == null)
            {
                return false;
            }

            return player.CurrentMapCode == TeamBattle.Battle.Config.MapID;
        }
        #endregion
    }
}
