using Server.Data;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.LuckyCircle
{
    /// <summary>
    /// Định nghĩa Vòng quay may mắn
    /// </summary>
    public class KTLuckyCircle
    {
        /// <summary>
        /// Thiết lập vòng quay
        /// </summary>
        public class ConfigInfo
        {
            /// <summary>
            /// Kích hoạt không
            /// </summary>
            public bool Activate { get; set; }

            /// <summary>
            /// Giới hạn cấp độ
            /// </summary>
            public int LimitLevel { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static ConfigInfo Parse(XElement xmlNode)
            {
                return new ConfigInfo()
                {
                    Activate = bool.Parse(xmlNode.Attribute("Activate").Value),
                    LimitLevel = int.Parse(xmlNode.Attribute("LimitLevel").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin yêu cầu
        /// </summary>
        public class RequirationInfo
        {
            /// <summary>
            /// Số đồng yêu cầu cho mỗi lượt quay
            /// </summary>
            public int RequireToken { get; set; }

            /// <summary>
            /// Số đồng khóa yêu cầu cho mỗi lượt quay
            /// </summary>
            public int RequireBoundToken { get; set; }

            /// <summary>
            /// Vật phẩm yêu cầu cho mỗi lượt quay
            /// </summary>
            public int RequireItemID { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static RequirationInfo Parse(XElement xmlNode)
            {
                return new RequirationInfo()
                {
                    RequireToken = int.Parse(xmlNode.Attribute("RequireToken").Value),
                    RequireBoundToken = int.Parse(xmlNode.Attribute("RequireBoundToken").Value),
                    RequireItemID = int.Parse(xmlNode.Attribute("RequireItemID").Value),
                };
            }
        }

        /// <summary>
        /// Thời gian sự kiện
        /// </summary>
        public class EventTime
        {
            /// <summary>
            /// Múi giờ
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
            /// Các thứ trong tuần
            /// </summary>
            public List<DayOfWeek> WeekDays { get; set; }

            /// <summary>
            /// Các mốc giờ
            /// </summary>
            public List<TimeStamp> Times { get; set; }

            /// <summary>
            /// Thời gian duy trì
            /// </summary>
            public int Duration { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EventTime Parse(XElement xmlNode)
            {
                /// Tạo mới
                EventTime time = new EventTime()
                {
                    WeekDays = new List<DayOfWeek>(),
                    Times = new List<TimeStamp>(),
                    Duration = int.Parse(xmlNode.Attribute("Duration").Value),
                };

                /// Danh sách thứ
                string dayOfWeekStrings = xmlNode.Attribute("WeekDays").Value;
                /// Nếu tồn tại
                if (!string.IsNullOrEmpty(dayOfWeekStrings))
                {
                    /// Duyệt danh sách
                    foreach (string dayOfWeekString in dayOfWeekStrings.Split(';'))
                    {
                        /// Thứ tương ứng
                        DayOfWeek day = (DayOfWeek) int.Parse(dayOfWeekString);
                        /// Thêm vào danh sách
                        time.WeekDays.Add(day);
                    }
                }

                /// Danh sách giờ
                string timeStrings = xmlNode.Attribute("Times").Value;
                /// Nếu tồn tại
                if (!string.IsNullOrEmpty(timeStrings))
                {
                    /// Duyệt danh sách
                    foreach (string timeString in timeStrings.Split(';'))
                    {
                        /// Các trường
                        string[] fields = timeString.Split(':');
                        /// Giờ
                        int hour = int.Parse(fields[0]);
                        /// Phút
                        int minute = int.Parse(fields[1]);
                        /// Thêm vào danh sách
                        time.Times.Add(new TimeStamp()
                        {
                            Hour = hour,
                            Minute = minute,
                        });
                    }
                }

                /// Trả về kết quả
                return time;
            }
        }

        /// <summary>
        /// Thông tin ô vòng quay
        /// </summary>
        public class CellInfo
        {
            /// <summary>
            /// Thứ tự ô
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// ID vật phẩm
            /// </summary>
            public int ItemID { get; set; }

            /// <summary>
            /// Số lượng
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào nếu sử dụng đồng (phần 10.000)
            /// </summary>
            public int RateUsingToken { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào nếu sử dụng đồng khóa (phần 10.000)
            /// </summary>
            public int RateUsingBoundToken { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào nếu sử dụng đồng thẻ (phần 10.000)
            /// </summary>
            public int RateUsingCard { get; set; }

            /// <summary>
            /// Danh sách CheckPoint
            /// </summary>
            public HashSet<int> CheckPoints { get; set; }

            /// <summary>
            /// Thông báo nếu nhận được
            /// </summary>
            public bool NotifyAfterTaken { get; set; }

            /// <summary>
            /// Loại hiệu ứng
            /// </summary>
            public int EffectType { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static CellInfo Parse(XElement xmlNode)
            {
                CellInfo cellInfo = new CellInfo()
                {
                    ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                    Quantity = int.Parse(xmlNode.Attribute("Quantity").Value),
                    RateUsingToken = int.Parse(xmlNode.Attribute("RateUsingToken").Value),
                    RateUsingBoundToken = int.Parse(xmlNode.Attribute("RateUsingBoundToken").Value),
                    RateUsingCard = int.Parse(xmlNode.Attribute("RateUsingCard").Value),
                    NotifyAfterTaken = bool.Parse(xmlNode.Attribute("NotifyAfterTaken").Value),
                    EffectType = int.Parse(xmlNode.Attribute("EffectType").Value),
                };

                /// Danh sách CheckPoint
                string checkPointStrings = xmlNode.Attribute("CheckPoint").Value;
                /// Nếu có áp dụng CheckPoint
                if (checkPointStrings != "-1")
                {
                    /// Tạo mới danh sách
                    cellInfo.CheckPoints = new HashSet<int>();
                    /// Duyệt danh sách CheckPoint
                    foreach (string checkPointString in checkPointStrings.Split(';'))
                    {
                        int checkPoint = int.Parse(checkPointString);
                        cellInfo.CheckPoints.Add(checkPoint);
                    }
                }

                return cellInfo;
            }
        }

        /// <summary>
        /// Thiết lập Vòng quay
        /// </summary>
        public ConfigInfo Config { get; set; }

        /// <summary>
        /// Yêu cầu
        /// </summary>
        public RequirationInfo Requiration { get; set; }

        /// <summary>
        /// Thông tin thời gian
        /// </summary>
        public EventTime Time { get; set; }

        /// <summary>
        /// Danh sách ô vật phẩm trong vòng quay
        /// </summary>
        public List<CellInfo> Cells { get; set; }

        /// <summary>
        /// Danh sách vật phẩm trích xuất gửi qua tầng Net
        /// </summary>
        public List<G2C_LuckyCircle_ItemInfo> CellDataToExport { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static KTLuckyCircle Parse(XElement xmlNode)
        {
            KTLuckyCircle luckyCircle = new KTLuckyCircle()
            {
                Config = ConfigInfo.Parse(xmlNode.Element("Config")),
                Requiration = RequirationInfo.Parse(xmlNode.Element("Requiration")),
                Time = EventTime.Parse(xmlNode.Element("Time")),
                Cells = new List<CellInfo>(),
                CellDataToExport = new List<G2C_LuckyCircle_ItemInfo>(),
            };

            /// Thứ tự ô
            int cellID = -1;
            /// Duyệt danh sách ô quay
            foreach (XElement node in xmlNode.Element("CellData").Elements("Cell"))
            {
                /// Tăng thứ tự ô
                cellID++;
                /// Thông tin ô tương ứng
                CellInfo cellInfo = CellInfo.Parse(node);
                /// Gắn ID cho ô
                cellInfo.ID = cellID;
                /// Thêm vào danh sách
                luckyCircle.Cells.Add(cellInfo);
                luckyCircle.CellDataToExport.Add(new G2C_LuckyCircle_ItemInfo()
                {
                    ItemID = cellInfo.ItemID,
                    Quantity = cellInfo.Quantity,
                    EffectType = cellInfo.EffectType,
                });
            }

            return luckyCircle;
        }
    }
}
