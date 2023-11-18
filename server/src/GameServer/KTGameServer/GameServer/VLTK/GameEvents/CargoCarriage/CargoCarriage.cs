using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.CargoCarriage
{
    /// <summary>
    /// Sự kiện vận tiêu
    /// </summary>
    public static class CargoCarriage
    {
        #region Define
        /// <summary>
        /// Dữ liệu sự kiện vận tiêu
        /// </summary>
        public class CargoCarriageData
        {
            /// <summary>
            /// Thông tin vật phẩm thưởng
            /// </summary>
            public class EventItem
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
                /// Khóa khi nhận không
                /// </summary>
                public bool Bound { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventItem Parse(XElement xmlNode)
                {
                    return new EventItem()
                    {
                        ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                        Quantity = int.Parse(xmlNode.Attribute("Quantity").Value),
                        Bound = bool.Parse(xmlNode.Attribute("Bound").Value),
                    };
                }
            }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public class EventConfig
            {
                /// <summary>
                /// Số lượt vận tiêu trong ngày
                /// </summary>
                public int MaxRoundsPerDay { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventConfig Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    return new EventConfig()
                    {
                        MaxRoundsPerDay = int.Parse(xmlNode.Attribute("MaxRoundsPerDay").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin xe tiêu
            /// </summary>
            public class CarriageData
            {
                /// <summary>
                /// Loại xe tiêu
                /// </summary>
                public int Type { get; set; }

                /// <summary>
                /// Tên
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// ID Res
                /// </summary>
                public int ResID { get; set; }

                /// <summary>
                /// Tốc độ di chuyển
                /// </summary>
                public int MoveSpeed { get; set; }

                /// <summary>
                /// Tầm nhìn
                /// </summary>
                public int Vision { get; set; }

                /// <summary>
                /// Sinh lực tối đa
                /// </summary>
                public int MaxHP { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static CarriageData Parse(XElement xmlNode)
                {
                    return new CarriageData()
                    {
                        Type = int.Parse(xmlNode.Attribute("Type").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        ResID = int.Parse(xmlNode.Attribute("ResID").Value),
                        MoveSpeed = int.Parse(xmlNode.Attribute("MoveSpeed").Value),
                        Vision = int.Parse(xmlNode.Attribute("Vision").Value),
                        MaxHP = int.Parse(xmlNode.Attribute("MaxHP").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin NPC
            /// </summary>
            public class NPCData
            {
                /// <summary>
                /// ID NPC
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Tên NPC
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Danh hiệu
                /// </summary>
                public string Title { get; set; }

                /// <summary>
                /// ID bản đồ
                /// </summary>
                public int MapID { get; set; }

                /// <summary>
                /// Vị trí X
                /// </summary>
                public int PosX { get; set; }

                /// <summary>
                /// Vị trí Y
                /// </summary>
                public int PosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static NPCData Parse(XElement xmlNode)
                {
                    return new NPCData()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        MapID = int.Parse(xmlNode.Attribute("MapID").Value),
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin phần thưởng
            /// </summary>
            public class AwardData
            {
                /// <summary>
                /// Loại vận tiêu
                /// </summary>
                public int Type { get; set; }

                /// <summary>
                /// Phần thưởng bạc
                /// </summary>
                public int Money { get; set; }

                /// <summary>
                /// Phần thưởng bạc khóa
                /// </summary>
                public int BoundMoney { get; set; }

                /// <summary>
                /// Phần thưởng KNB
                /// </summary>
                public int Token { get; set; }

                /// <summary>
                /// Phần thưởng KNB khóa
                /// </summary>
                public int BoundToken { get; set; }

                /// <summary>
                /// Danh sách vật phẩm thưởng
                /// </summary>
                public List<EventItem> AwardItems { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static AwardData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    AwardData awardData = new AwardData()
                    {
                        Type = int.Parse(xmlNode.Attribute("Type").Value),
                        Money = int.Parse(xmlNode.Attribute("Money").Value),
                        BoundMoney = int.Parse(xmlNode.Attribute("BoundMoney").Value),
                        Token = int.Parse(xmlNode.Attribute("Token").Value),
                        BoundToken = int.Parse(xmlNode.Attribute("BoundToken").Value),
                        AwardItems = new List<EventItem>(),
                    };

                    /// Duyệt danh sách vật phẩm thưởng
                    foreach (XElement node in xmlNode.Elements("Item"))
                    {
                        /// Thêm vào danh sách
                        awardData.AwardItems.Add(EventItem.Parse(node));
                    }

                    /// Trả về kết quả
                    return awardData;
                }
            }

            /// <summary>
            /// Thông tin rơi vật phẩm khi tiêu chết
            /// </summary>
            public class DeadPunishmentData
            {
                /// <summary>
                /// Loại xe tiêu
                /// </summary>
                public int Type { get; set; }

                /// <summary>
                /// Danh sách vật phẩm rơi
                /// </summary>
                public List<EventItem> DropItems { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static DeadPunishmentData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    DeadPunishmentData data = new DeadPunishmentData()
                    {
                        Type = int.Parse(xmlNode.Attribute("Type").Value),
                        DropItems = new List<EventItem>(),
                    };

                    /// Duyệt danh sách vật phẩm rơi
                    foreach (XElement node in xmlNode.Elements("Drop"))
                    {
                        /// Thêm vào danh sách
                        data.DropItems.Add(EventItem.Parse(node));
                    }

                    /// Trả về kết quả
                    return data;
                }
            }

            /// <summary>
            /// Thông tin vận tiêu
            /// </summary>
            public class CargoData
            {
                /// <summary>
                /// Quãng đường vận tiêu
                /// </summary>
                public class CargoPath
                {
                    /// <summary>
                    /// ID bản đồ xuất phát
                    /// </summary>
                    public int FromMapID { get; set; }

                    /// <summary>
                    /// ID bản đồ đích đến
                    /// </summary>
                    public int ToMapID { get; set; }

                    /// <summary>
                    /// Cấp độ nhân vật tối thiểu sẽ giao quãng đường này
                    /// </summary>
                    public int MinLevel { get; set; }

                    /// <summary>
                    /// Chuyển đối tượng từ XMLNode
                    /// </summary>
                    /// <param name="xmlNode"></param>
                    /// <returns></returns>
                    public static CargoPath Parse(XElement xmlNode)
                    {
                        return new CargoPath()
                        {
                            FromMapID = int.Parse(xmlNode.Attribute("FromMapID").Value),
                            ToMapID = int.Parse(xmlNode.Attribute("ToMapID").Value),
                            MinLevel = int.Parse(xmlNode.Attribute("MinLevel").Value),
                        };
                    }
                }

                /// <summary>
                /// Loại xe tiêu
                /// </summary>
                public int Type { get; set; }

                /// <summary>
                /// Cấp độ yêu cầu
                /// </summary>
                public int RequireLevel { get; set; }

                /// <summary>
                /// Yêu cầu cọc bạc thường
                /// </summary>
                public int RequireMoney { get; set; }

                /// <summary>
                /// Yêu cầu cọc bạc khóa
                /// </summary>
                public int RequireBoundMoney { get; set; }

                /// <summary>
                /// Yêu cầu cọc KNB
                /// </summary>
                public int RequireToken { get; set; }

                /// <summary>
                /// Yêu cầu cọc KNB khóa
                /// </summary>
                public int RequireBoundToken { get; set; }

                /// <summary>
                /// Thời gian vận tiêu tối đa
                /// </summary>
                public int LimitTime { get; set; }

                /// <summary>
                /// Thông báo kênh hệ thống khi nhận tiêu không
                /// </summary>
                public bool NotifySystem { get; set; }

                /// <summary>
                /// Danh sách quãng đường di chuyển
                /// </summary>
                public List<CargoPath> Paths { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static CargoData Parse(XElement xmlNode)
                {
                    /// Tạo mới
                    CargoData cargoData = new CargoData()
                    {
                        Type = int.Parse(xmlNode.Attribute("Type").Value),
                        RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
                        RequireMoney = int.Parse(xmlNode.Attribute("RequireMoney").Value),
                        RequireBoundMoney = int.Parse(xmlNode.Attribute("RequireBoundMoney").Value),
                        RequireToken = int.Parse(xmlNode.Attribute("RequireToken").Value),
                        RequireBoundToken = int.Parse(xmlNode.Attribute("RequireBoundToken").Value),
                        LimitTime = int.Parse(xmlNode.Attribute("LimitTime").Value),
                        NotifySystem = bool.Parse(xmlNode.Attribute("NotifySystem").Value),
                        Paths = new List<CargoPath>(),
                    };

                    /// Duyệt danh sách đường đi
                    foreach (XElement node in xmlNode.Elements("MovePath"))
                    {
                        /// Lưu lại
                        cargoData.Paths.Add(CargoPath.Parse(node));
                    }

                    /// Trả về kết quả
                    return cargoData;
                }
            }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public EventConfig Config { get; private set; }

            /// <summary>
            /// Danh sách xe tiêu
            /// </summary>
            public List<CarriageData> Carriages { get; private set; }

            /// <summary>
            /// Danh sách NPC vận tiêu
            /// </summary>
            public List<NPCData> NPCs { get; private set; }

            /// <summary>
            /// Danh sách phần thưởng
            /// </summary>
            public List<AwardData> Awards { get; private set; }

            /// <summary>
            /// Danh sách vật phẩm rơi ra khi chết
            /// </summary>
            public List<DeadPunishmentData> DeathPunishments { get; private set; }

            /// <summary>
            /// Danh sách quãng đường vận tiêu
            /// </summary>
            public List<CargoData> Paths { get; private set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static CargoCarriageData Parse(XElement xmlNode)
            {
                /// Tạo mới
                CargoCarriageData data = new CargoCarriageData()
                {
                    Config = EventConfig.Parse(xmlNode.Element("Config")),
                    Carriages = new List<CarriageData>(),
                    NPCs = new List<NPCData>(),
                    Awards = new List<AwardData>(),
                    DeathPunishments = new List<DeadPunishmentData>(),
                    Paths = new List<CargoData>(),
                };

                /// Duyệt danh sách xe tiêu
                foreach (XElement node in xmlNode.Element("Carriages").Elements("Carriage"))
                {
                    /// Thêm vào danh sách
                    data.Carriages.Add(CarriageData.Parse(node));
                }

                /// Duyệt danh sách NPC vận tiêu
                foreach (XElement node in xmlNode.Element("CargoNPCs").Elements("NPC"))
                {
                    /// Thêm vào danh sách
                    data.NPCs.Add(NPCData.Parse(node));
                }

                /// Duyệt danh sách phần thưởng
                foreach (XElement node in xmlNode.Element("Awards").Elements("Award"))
                {
                    /// Thêm vào danh sách
                    data.Awards.Add(AwardData.Parse(node));
                }

                /// Duyệt danh sách vật phẩm rơi ra khi chết
                foreach (XElement node in xmlNode.Element("DeathPunishs").Elements("Punishment"))
                {
                    /// Thêm vào danh sách
                    data.DeathPunishments.Add(DeadPunishmentData.Parse(node));
                }

                /// Duyệt danh sách quãng đường vận tiêu
                foreach (XElement node in xmlNode.Element("Paths").Elements("Path"))
                {
                    /// Thêm vào danh sách
                    data.Paths.Add(CargoData.Parse(node));
                }

                /// Trả về kết quả
                return data;
            }
        }

        /// <summary>
        /// Thông tin nhiệm vụ vận tiêu của người chơi
        /// </summary>
        public class CargoCarriageTaskData
        {
            /// <summary>
            /// Loại xe tiêu
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// Thông tin NPC giao nhiệm vụ
            /// </summary>
            public CargoCarriageData.NPCData BeginNPC { get; set; }

            /// <summary>
            /// Thông tin NPC trả nhiệm vụ
            /// </summary>
            public CargoCarriageData.NPCData DoneNPC { get; set; }

            /// <summary>
            /// Quãng đường vận tiêu
            /// </summary>
            public CargoCarriageData.CargoData.CargoPath MovePath { get; set; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu vận tiêu
        /// </summary>
        public static CargoCarriageData Data { get; private set; }

        /// <summary>
        /// Sự kiện đã bắt đầu chưa
        /// </summary>
        public static bool IsStarted { get; set; } = false;
        #endregion

        #region Initialize
        /// <summary>
        /// Khởi tạo dữ liệu sự kiện vận tiêu
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/CargoCarriage.xml");
            /// Đọc dữ liệu vào
            CargoCarriage.Data = CargoCarriageData.Parse(xmlNode);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra sự kiện đã bắt đầu chưa
        /// </summary>
        /// <returns></returns>
        public static bool IsEventStarted()
        {
            return CargoCarriage.IsStarted;
        }
        #endregion
    }
}
