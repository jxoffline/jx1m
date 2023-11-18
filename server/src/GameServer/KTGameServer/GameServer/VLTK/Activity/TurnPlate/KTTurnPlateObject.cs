using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.TurnPlate
{
    /// <summary>
    /// Định nghĩa Vòng quay may mắn - đặc biệt
    /// </summary>
    public class KTTurnPlate
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
        /// Dữ liệu CheckPoint vật phẩm cố định đạt được
        /// </summary>
        public class FixedCheckPointItemData
        {
            /// <summary>
            /// Mốc
            /// </summary>
            public int CheckPoint { get; set; }

            /// <summary>
            /// ID vật phẩm
            /// </summary>
            public int ItemID { get; set; }

            /// <summary>
            /// Số lượng
            /// </summary>
            public int Quantity { get; set; }

            /// <summary>
            /// Thông báo khi nhận được
            /// </summary>
            public bool NotifyAfterTaken { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static FixedCheckPointItemData Parse(XElement xmlNode)
            {
                return new FixedCheckPointItemData()
                {
                    CheckPoint = int.Parse(xmlNode.Attribute("CheckPoint").Value),
                    ItemID = int.Parse(xmlNode.Attribute("ItemID").Value),
                    Quantity = int.Parse(xmlNode.Attribute("Quantity").Value),
                    NotifyAfterTaken = bool.Parse(xmlNode.Attribute("NotifyAfterTaken").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin ô vòng quay
        /// </summary>
        public class CellInfo
        {
            /// <summary>
            /// ID ô
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
            /// Tỷ lệ quay vào (phần 10.000)
            /// </summary>
            public int Rate { get; set; }

            /// <summary>
            /// Danh sách CheckPoint
            /// </summary>
            public HashSet<int> CheckPoints { get; set; }

            /// <summary>
            /// Thông báo nếu nhận được
            /// </summary>
            public bool NotifyAfterTaken { get; set; }

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
                    Rate = int.Parse(xmlNode.Attribute("Rate").Value),
                    NotifyAfterTaken = bool.Parse(xmlNode.Attribute("NotifyAfterTaken").Value),
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
        /// Danh sách các mốc được phần thưởng cố định kèm theo
        /// </summary>
        public Dictionary<int, FixedCheckPointItemData> FixedCheckPointItems { get; set; }

        /// <summary>
        /// Danh sách ô vật phẩm trong vòng quay
        /// </summary>
        public List<CellInfo> Cells { get; set; }

        /// <summary>
        /// Danh sách vật phẩm trích xuất gửi qua tầng Net
        /// </summary>
        public List<KeyValuePair<int, int>> CellDataToExport { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static KTTurnPlate Parse(XElement xmlNode)
        {
            KTTurnPlate turnPlate = new KTTurnPlate()
            {
                Config = ConfigInfo.Parse(xmlNode.Element("Config")),
                Requiration = RequirationInfo.Parse(xmlNode.Element("Requiration")),
                Time = EventTime.Parse(xmlNode.Element("Time")),
                Cells = new List<CellInfo>(),
                CellDataToExport = new List<KeyValuePair<int, int>>(),
                FixedCheckPointItems = new Dictionary<int, FixedCheckPointItemData>(),
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
                turnPlate.Cells.Add(cellInfo);
                turnPlate.CellDataToExport.Add(new KeyValuePair<int, int>(cellInfo.ItemID, cellInfo.Quantity));
            }

            /// Duyệt danh sách phần thưởng cố định theo mốc
            foreach (XElement node in xmlNode.Element("FixedCheckPointItems").Elements("Data"))
            {
                /// Phần thưởng tương ứng
                FixedCheckPointItemData data = FixedCheckPointItemData.Parse(node);
                /// Thêm vào danh sách
                turnPlate.FixedCheckPointItems[data.CheckPoint] = data;
            }

            return turnPlate;
        }
    }
}
