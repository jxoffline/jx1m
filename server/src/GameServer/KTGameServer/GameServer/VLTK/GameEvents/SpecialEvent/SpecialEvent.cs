using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.SpecialEvent
{
    /// <summary>
    /// Sự kiện đặc biệt
    /// </summary>
    public static class SpecialEvent
    {
        #region Define
        /// <summary>
        /// Loại sự kiện
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Đánh quái nhặt nguyên liệu đổi quà
            /// </summary>
            KillMonsterGetMaterials = 1,
            /// <summary>
            /// Thu thập nguyên liệu đổi quà
            /// </summary>
            CollectGrowPoints = 2,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// Thông tin sự kiện
        /// </summary>
        public class EventInfo
        {
            /// <summary>
            /// Thiết lập thời gian
            /// </summary>
            public class TimeConfigInfo
            {
                /// <summary>
                /// Từ ngày
                /// </summary>
                public DateTime FromDay { get; set; }

                /// <summary>
                /// Tới ngày
                /// </summary>
                public DateTime ToDay { get; set; }

                /// <summary>
                /// Có trong thời gian diễn ra sự kiện không
                /// </summary>
                public bool InTime
                {
                    get
                    {
                        DateTime now = DateTime.Now;
                        return now >= this.FromDay && now <= this.ToDay;
                    }
                }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static TimeConfigInfo Parse(XElement xmlNode)
                {
                    TimeConfigInfo timeConfig = new TimeConfigInfo();
                    string[] fromDayStr = xmlNode.Attribute("FromDay").Value.Split('/');
                    timeConfig.FromDay = new DateTime(int.Parse(fromDayStr[2]), int.Parse(fromDayStr[1]), int.Parse(fromDayStr[0]));
                    string[] toDayStr = xmlNode.Attribute("ToDay").Value.Split('/');
                    timeConfig.ToDay = new DateTime(int.Parse(toDayStr[2]), int.Parse(toDayStr[1]), int.Parse(toDayStr[0]));
                    return timeConfig;
                }
            }

            /// <summary>
            /// Thông tin vật phẩm rơi ở quái
            /// </summary>
            public class DropInfo
            {
                /// <summary>
                /// Thông tin vật phẩm rơi
                /// </summary>
                public class DropItemInfo
                {
                    /// <summary>
                    /// ID vật phẩm
                    /// </summary>
                    public int ID { get; set; }

                    /// <summary>
                    /// Tỷ lệ rơi (phần 10.000)
                    /// </summary>
                    public int Rate { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static DropItemInfo Parse(XElement xmlNode)
                    {
                        return new DropItemInfo()
                        {
                            ID = int.Parse(xmlNode.Attribute("ID").Value),
                            Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                        };
                    }
                }

                /// <summary>
                /// ID quái
                /// </summary>
                public int MonsterID { get; set; }

                /// <summary>
                /// Danh sách ID bản đồ, ngăn cách bởi dấu ; (-1 sẽ có tác dụng trên tất cả bản đồ)
                /// </summary>
                public List<int> MapIDs { get; set; }

                /// <summary>
                /// Danh sách vật phẩm rơi
                /// </summary>
                public List<DropItemInfo> Items { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static DropInfo Parse(XElement xmlNode)
                {
                    DropInfo dropInfo = new DropInfo()
                    {
                        MonsterID = int.Parse(xmlNode.Attribute("MonsterID").Value),
                        MapIDs = new List<int>(),
                        Items = new List<DropItemInfo>(),
                    };

                    /// Danh sách bản đồ
                    string mapIDStrings = xmlNode.Attribute("MapIDs").Value;
                    if (!string.IsNullOrEmpty(mapIDStrings))
                    {
                        foreach (string mapIDString in mapIDStrings.Split(';'))
                        {
                            int mapID = int.Parse(mapIDString);
                            dropInfo.MapIDs.Add(mapID);
                        }
                    }

                    /// Duyệt danh sách vật phẩm rơi
                    foreach (XElement node in xmlNode.Elements("Item"))
                    {
                        dropInfo.Items.Add(DropItemInfo.Parse(node));
                    }

                    return dropInfo;
                }
            }

            /// <summary>
            /// Thông tin điểm thu thập
            /// </summary>
            public class GrowPointInfo
            {
                /// <summary>
                /// Thông tin vật phẩm thu thập được
                /// </summary>
                public class CollectItemInfo
                {
                    /// <summary>
                    /// ID vật phẩm
                    /// </summary>
                    public int ID { get; set; }

                    /// <summary>
                    /// Tỷ lệ thu thập (phần 10.000)
                    /// </summary>
                    public int Rate { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static CollectItemInfo Parse(XElement xmlNode)
                    {
                        return new CollectItemInfo()
                        {
                            ID = int.Parse(xmlNode.Attribute("ID").Value),
                            Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                        };
                    }
                }

                /// <summary>
                /// ID điểm thu thập
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Tên điểm thu thập (bỏ trống sẽ lấy tên ở file NPCs.xml)
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// ID bản đồ
                /// </summary>
                public int MapID { get; set; }

                /// <summary>
                /// Danh sách vị trí
                /// </summary>
                public List<UnityEngine.Vector2Int> Pos { get; set; }

                /// <summary>
                /// Thời gian xuất hiện tính từ khi sự kiện bắt đầu (Mili giây) (-1 sẽ không xuất hiện)
                /// </summary>
                public int RespawnTicks { get; set; }

                /// <summary>
                /// Thời gian tồn tại (Mili giây) (-1 là vĩnh viễn)
                /// </summary>
                public int DurationTicks { get; set; }

                /// <summary>
                /// Thời gian thu thập (Mili giây)
                /// </summary>
                public int CollectTicks { get; set; }

                /// <summary>
                /// Tổng số vị trí sẽ sinh ra (-1 sẽ sinh ra toàn bộ vị trí đã chỉ định)
                /// </summary>
                public int SpawnCount { get; set; }

                /// <summary>
                /// Yêu cầu số ô trống trong túi trước khi thu thập
                /// </summary>
                public int RequireBagSpaces { get; set; }

                /// <summary>
                /// Xuất hiện từ thời điểm (trong ngày)
                /// </summary>
                public TimeStamp FromHour { get; set; }

                /// <summary>
                /// Xuất hiện tới thời điểm (trong ngày)
                /// </summary>
                public TimeStamp ToHour { get; set; }

                /// <summary>
                /// Danh sách vật phẩm thu thập
                /// </summary>
                public List<CollectItemInfo> Items { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static GrowPointInfo Parse(XElement xmlNode)
                {
                    GrowPointInfo growPointInfo = new GrowPointInfo()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                        Pos = new List<UnityEngine.Vector2Int>(),
                        RespawnTicks = int.Parse(xmlNode.Attribute("RespawnTicks").Value),
                        DurationTicks = int.Parse(xmlNode.Attribute("DurationTicks").Value),
                        CollectTicks = int.Parse(xmlNode.Attribute("CollectTicks").Value),
                        SpawnCount = int.Parse(xmlNode.Attribute("SpawnCount").Value),
                        RequireBagSpaces = int.Parse(xmlNode.Attribute("RequireBagSpaces").Value),
                        Items = new List<CollectItemInfo>(),
                    };

                    foreach (string positionString in xmlNode.Attribute("Pos").Value.Split(';'))
                    {
                        string[] fields = positionString.Split('_');
                        int posX = int.Parse(fields[0]);
                        int posY = int.Parse(fields[1]);
                        growPointInfo.Pos.Add(new UnityEngine.Vector2Int(posX, posY));
                    }

                    string fromHourString = xmlNode.Attribute("FromHour").Value;
                    if (fromHourString == "-1")
                    {
                        growPointInfo.FromHour = new TimeStamp(0, 0);
                    }
                    else
                    {
                        string[] fields = fromHourString.Split(':');
                        int nHour = int.Parse(fields[0]);
                        int nMinute = int.Parse(fields[1]);
                        growPointInfo.FromHour = new TimeStamp(nHour, nMinute);
                    }

                    string toHourString = xmlNode.Attribute("ToHour").Value;
                    if (toHourString == "-1")
                    {
                        growPointInfo.ToHour = new TimeStamp(23, 59);
                    }
                    else
                    {
                        string[] fields = toHourString.Split(':');
                        int nHour = int.Parse(fields[0]);
                        int nMinute = int.Parse(fields[1]);
                        growPointInfo.ToHour = new TimeStamp(nHour, nMinute);
                    }

                    /// Duyệt danh sách vật phẩm thu thập
                    foreach (XElement node in xmlNode.Elements("Item"))
                    {
                        growPointInfo.Items.Add(CollectItemInfo.Parse(node));
                    }

                    return growPointInfo;
                }
            }

            /// <summary>
            /// Thông tin phần thưởng
            /// </summary>
            public class AwardInfo
            {
                /// <summary>
                /// Thông tin vật phẩm yêu cầu
                /// </summary>
                public class RequireItemInfo
                {
                    /// <summary>
                    /// ID vật phẩm
                    /// </summary>
                    public int ItemID { get; set; }

                    /// <summary>
                    /// Số lượng yêu cầu
                    /// </summary>
                    public int Quantity { get; set; }
                }

                /// <summary>
                /// Thông tin vật phẩm thưởng
                /// </summary>
                public class AwardItemInfo
                {
                    /// <summary>
                    /// ID vật phẩm
                    /// </summary>
                    public int ItemID { get; set; }

                    /// <summary>
                    /// Số lượng
                    /// </summary>
                    public int Quantity { get; set; }

                    /// <summary>
                    /// Tỷ lệ nhận được (phần 10.000)
                    /// </summary>
                    public int Rate { get; set; }

                    /// <summary>
                    /// Có khóa không
                    /// </summary>
                    public bool Bound { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static AwardItemInfo Parse(XElement xmlNode)
                    {
                        return new AwardItemInfo()
                        {
                            ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                            Quantity = int.Parse(xmlNode.Attribute("Quantity").Value),
                            Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                            Bound = bool.Parse(xmlNode.Attribute("Bound").Value),
                        };
                    }
                }

                /// <summary>
                /// ID lựa chọn
                /// </summary>
                public int SelectionID { get; set; }

                /// <summary>
                /// Lựa chọn ở NPC
                /// </summary>
                public string Selection { get; set; }

                /// <summary>
                /// Danh sách vật phẩm yêu cầu
                /// </summary>
                public List<RequireItemInfo> RequireItems { get; set; }

                /// <summary>
                /// Tổng số phần thưởng nhận được
                /// </summary>
                public int AwardCount { get; set; }

                /// <summary>
                /// Yêu cầu số ô trống trong túi
                /// </summary>
                public int RequireFreeBagSpace { get; set; }

                /// <summary>
                /// Yêu cầu bạc
                /// </summary>
                public int RequireMoney { get; set; }

                /// <summary>
                /// Yêu cầu bạc khóa
                /// </summary>
                public int RequireBoundMoney { get; set; }

                /// <summary>
                /// Yêu cầu đồng
                /// </summary>
                public int RequireToken { get; set; }

                /// <summary>
                /// Yêu cầu đồng khóa
                /// </summary>
                public int RequireBoundToken { get; set; }

                /// <summary>
                /// Danh sách vật phẩm thưởng
                /// </summary>
                public List<AwardItemInfo> AwardItems { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static AwardInfo Parse(XElement xmlNode)
                {
                    AwardInfo awardInfo = new AwardInfo()
                    {
                        Selection = xmlNode.Attribute("Selection").Value,
                        RequireItems = new List<RequireItemInfo>(),
                        AwardCount = int.Parse(xmlNode.Attribute("AwardCount").Value),
                        RequireFreeBagSpace = int.Parse(xmlNode.Attribute("RequireFreeBagSpace").Value),
                        RequireMoney = int.Parse(xmlNode.Attribute("RequireMoney").Value),
                        RequireBoundMoney = int.Parse(xmlNode.Attribute("RequireBoundMoney").Value),
                        RequireToken = int.Parse(xmlNode.Attribute("RequireToken").Value),
                        RequireBoundToken = int.Parse(xmlNode.Attribute("RequireBoundToken").Value),
                        AwardItems = new List<AwardItemInfo>(),
                    };

                    /// Thông tin vật phẩm yêu cầu
                    string requireMaterialsStrings = xmlNode.Attribute("RequireItems").Value;
                    if (!string.IsNullOrEmpty(requireMaterialsStrings))
                    {
                        foreach (string requireMaterialString in requireMaterialsStrings.Split(';'))
                        {
                            string[] fields = requireMaterialString.Split('_');
                            awardInfo.RequireItems.Add(new RequireItemInfo()
                            {
                                ItemID = int.Parse(fields[0]),
                                Quantity = int.Parse(fields[1]),
                            });
                        }
                    }

                    /// Duyệt danh sách phần thưởng
                    foreach (XElement node in xmlNode.Elements("Award"))
                    {
                        awardInfo.AwardItems.Add(AwardItemInfo.Parse(node));
                    }

                    return awardInfo;
                }
            }

            /// <summary>
            /// Thời gian trong ngày
            /// </summary>
            public struct TimeStamp
            {
                /// <summary>
                /// Giờ
                /// </summary>
                public int Hour { get; set; }

                /// <summary>
                /// Phút
                /// </summary>
                public int Minute { get; set; }

                /// <summary>
                /// Thời gian trong ngày
                /// </summary>
                /// <param name="hour"></param>
                /// <param name="minute"></param>
                public TimeStamp(int hour, int minute)
                {
                    this.Hour = hour;
                    this.Minute = minute;
                }

                /// <summary>
                /// So sánh lớn hơn về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator >(TimeStamp a, TimeStamp b)
                {
                    /// Nếu lớn hơn về giờ
                    if (a.Hour > b.Hour)
                    {
                        return true;
                    }
                    /// Nếu bằng giờ nhưng lớn hơn phút
                    else if (a.Hour == b.Hour && a.Minute > b.Minute)
                    {
                        return true;
                    }
                    /// Không thỏa mãn
                    return false;
                }

                /// <summary>
                /// So sánh nhỏ hơn về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator <(TimeStamp a, TimeStamp b)
                {
                    /// Nếu nhỏ hơn về giờ
                    if (a.Hour < b.Hour)
                    {
                        return true;
                    }
                    /// Nếu bằng giờ nhưng nhỏ hơn phút
                    else if (a.Hour == b.Hour && a.Minute < b.Minute)
                    {
                        return true;
                    }
                    /// Không thỏa mãn
                    return false;
                }

                /// <summary>
                /// So sánh lớn hơn về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator >=(TimeStamp a, TimeStamp b)
                {
                    /// Nếu lớn hơn về giờ
                    if (a.Hour > b.Hour)
                    {
                        return true;
                    }
                    /// Nếu bằng giờ nhưng lớn hơn phút
                    else if (a.Hour == b.Hour && a.Minute >= b.Minute)
                    {
                        return true;
                    }
                    /// Không thỏa mãn
                    return false;
                }

                /// <summary>
                /// So sánh nhỏ hơn về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator <=(TimeStamp a, TimeStamp b)
                {
                    /// Nếu nhỏ hơn về giờ
                    if (a.Hour < b.Hour)
                    {
                        return true;
                    }
                    /// Nếu bằng giờ nhưng nhỏ hơn phút
                    else if (a.Hour == b.Hour && a.Minute <= b.Minute)
                    {
                        return true;
                    }
                    /// Không thỏa mãn
                    return false;
                }

                /// <summary>
                /// So sánh bằng về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator ==(TimeStamp a, TimeStamp b)
                {
                    /// Trả về kết quả
                    return a.Hour == b.Hour && a.Minute == b.Minute;
                }

                /// <summary>
                /// So sánh khác về thời gian trong ngày
                /// </summary>
                /// <param name="a"></param>
                /// <param name="b"></param>
                /// <returns></returns>
                public static bool operator !=(TimeStamp a, TimeStamp b)
                {
                    /// Trả về kết quả
                    return a.Hour != b.Hour || a.Minute != b.Minute;
                }

                /// <summary>
                /// Chuyển thời gian về dạng chuỗi
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    return string.Format("{0}:{1}", this.Hour, this.Minute);
                }

                /// <summary>
                /// So sánh bằng
                /// </summary>
                /// <param name="obj"></param>
                /// <returns></returns>
                public override bool Equals(object obj)
                {
                    return this == (TimeStamp) obj;
                }

                /// <summary>
                /// Trả về chuỗi Hash
                /// </summary>
                /// <returns></returns>
                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            /// <summary>
            /// ID tự tăng
            /// </summary>
            private static int AutoID = 0;

            /// <summary>
            /// ID sự kiện
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Sự kiện có được kích hoạt không
            /// </summary>
            public bool Activate { get; set; }

            /// <summary>
            /// Loại sự kiện
            /// </summary>
            public EventType Type { get; set; }

            /// <summary>
            /// Tên sự kiện
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Lời thoại từ NPC
            /// </summary>
            public string NPCTalk { get; set; }

            /// <summary>
            /// Thiết lập thời gian
            /// </summary>
            public TimeConfigInfo TimeConfig { get; set; }

            /// <summary>
            /// Danh sách điểm thu thập
            /// </summary>
            public List<GrowPointInfo> GrowPoints { get; set; }

            /// <summary>
            /// Danh sách nguyên liệu rơi
            /// </summary>
            public List<DropInfo> Materials { get; set; }

            /// <summary>
            /// Danh sách ID quái rơi nguyên liệu
            /// </summary>
            public HashSet<int> DropMonsters { get; private set; }

            /// <summary>
            /// Danh sách phần thưởng
            /// </summary>
            public Dictionary<int, AwardInfo> Awards { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EventInfo Parse(XElement xmlNode)
            {
                /// Tăng ID tự động
                EventInfo.AutoID++;
                EventInfo eventInfo = new EventInfo()
                {
                    ID = EventInfo.AutoID,
                    Activate = bool.Parse(xmlNode.Attribute("Activate").Value),
                    Type = (EventType) int.Parse(xmlNode.Attribute("Type").Value),
                    Name = xmlNode.Attribute("Name").Value,
                    NPCTalk = xmlNode.Attribute("NPCTalk").Value,
                    TimeConfig = TimeConfigInfo.Parse(xmlNode.Element("TimeConfig")),
                    GrowPoints = new List<GrowPointInfo>(),
                    Materials = new List<DropInfo>(),
                    DropMonsters = new HashSet<int>(),
                    Awards = new Dictionary<int, AwardInfo>(),
                };

                /// Duyệt danh sách điểm thu thập
                foreach (XElement node in xmlNode.Element("Materials").Elements("GrowPointInfo"))
                {
                    GrowPointInfo growPointInfo = GrowPointInfo.Parse(node);
                    eventInfo.GrowPoints.Add(growPointInfo);
                }

                /// Duyệt danh sách nguyên liệu rơi
                foreach (XElement node in xmlNode.Element("Materials").Elements("DropInfo"))
                {
                    DropInfo dropInfo = DropInfo.Parse(node);
                    eventInfo.Materials.Add(dropInfo);
                    eventInfo.DropMonsters.Add(dropInfo.MonsterID);
                }

                /// ID phần thưởng
                int awardID = 0;
                /// Duyệt danh sách phần thưởng
                foreach (XElement node in xmlNode.Element("Awards").Elements("AwardInfo"))
                {
                    /// Tăng ID phần thưởng
                    awardID++;
                    AwardInfo awardInfo = AwardInfo.Parse(node);
                    awardInfo.SelectionID = awardID;
                    eventInfo.Awards[awardInfo.SelectionID] = awardInfo;
                }

                return eventInfo;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách toàn bộ sự kiện
        /// </summary>
        public static Dictionary<int, EventInfo> AllEvents { get; } = new Dictionary<int, EventInfo>();

        /// <summary>
        /// Danh sách sự kiện khả dụng (loại bỏ các sự kiện không được kích hoạt và quá thời gian)
        /// </summary>
        public static Dictionary<int, EventInfo> Events { get; } = new Dictionary<int, EventInfo>();
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/SpecialEvent.xml");
            SpecialEvent.AllEvents.Clear();
            SpecialEvent.Events.Clear();

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Event"))
            {
                /// Thông tin sự kiện
                EventInfo eventInfo = EventInfo.Parse(node);
                /// Thêm vào sự kiện toàn bộ
                SpecialEvent.AllEvents[eventInfo.ID] = eventInfo;
                /// Thời gian hiện tại
                DateTime now = DateTime.Now;
                now = new DateTime(now.Year, now.Month, now.Day);
                /// Nếu sự kiện đang kích hoạt và chưa quá hạn
                if (eventInfo.Activate && eventInfo.TimeConfig.ToDay >= now)
                {
                    /// Thêm vào sự kiện khả dụng
                    SpecialEvent.Events[eventInfo.ID] = eventInfo;
                }
            }

            /// Chạy Timer
            SpecialEventManager.Init();
        }
        #endregion
    }
}
