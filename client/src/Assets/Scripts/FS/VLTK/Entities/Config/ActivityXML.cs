using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Định nghĩa hoạt động
    /// </summary>
    public class ActivityXML
    {
        /// <summary>
        /// Trạng thái sự kiện
        /// </summary>
        public enum ActivityState
        {
            /// <summary>
            /// Có thể tham gia
            /// </summary>
            CANJOIN,
            /// <summary>
            /// Đang diễn ra
            /// </summary>
            OPEN,
            /// <summary>
            /// Đã kết thúc
            /// </summary>
            HASEND,
            /// <summary>
            /// Không mở
            /// </summary>
            NOTOPEN,
        }

        /// <summary>
        /// Thời gian
        /// </summary>
        public class ActivityTime
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
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string hourStr = this.Hour.ToString();
                while (hourStr.Length < 2)
                {
                    hourStr = "0" + hourStr;
                }
                string minuteStr = this.Minute.ToString();
                while (minuteStr.Length < 2)
                {
                    minuteStr = "0" + minuteStr;
                }
                return string.Format("{0}:{1}", hourStr, minuteStr);
            }
        }

        /// <summary>
        /// Thêm cái này vào để sau muốn check hoạt động đã bắt đầu hay kết thúc
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Sự kiện diễn ra ngày nào trong tuần | Nếu = ngày hôm nay thì cho lên list ko thì thôi
        /// </summary>
        public List<int> DayOfWeek { get; set; }

        /// <summary>
        /// Danh sách quà có thể nhận được để vận hành config
        /// </summary>
        public List<int> ItemReward { get; set; }

        /// <summary>
        /// Tên sự kiện 
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Các mốc thời gian sẽ diễn ra. nếu list này NULL thì tức là sự kiện diễn ra toàn thời gian
        /// </summary>
        public List<ActivityTime> Time { get; set; }

        /// <summary>
        /// Map code chứa NPC sẽ đứng
        /// </summary>
        public int MapCode { get; set; }

        /// <summary>
        /// ID của NPC
        /// </summary>
        public int NPCID { get; set; }

        /// <summary>
        /// Cấp độ yêu cầu
        /// </summary>
        public int LevelJoin { get; set; }

        /// <summary>
        /// Trạng thái mặc định của sự kiện
        /// </summary>
        public ActivityState State { get; set; }

        /// <summary>
        /// Loại hình
        /// </summary>
        public string TypeDesc { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Yêu cầu thêm
        /// </summary>
        public string OtherRequiration { get; set; }

        /// <summary>
        /// Phần thưởng thêm
        /// </summary>
        public string OtherRewards { get; set; }

        /// <summary>
        /// Nếu thuộc tính này tồn tại thì khi Click vào Button Tham gia sẽ hiện dòng thông báo nội dung tương ứng, nếu không sẽ tự tìm đường đến NPC và bản đồ tương ứng
        /// </summary>
        public string MsgTips { get; set; }

        /// <summary>
        /// Lấy ra trạng thái sự kiện hiện tại
        /// </summary>
        /// <returns></returns>
        public ActivityState GetActivityState()
        {
            /// Nếu mặc định đóng sự kiện thì không mở
            if (this.State == ActivityState.NOTOPEN)
            {
                return this.State;
            }

            /// Trạng thái mặc định của hoạt động
            ActivityState state = this.State;

            /// Thời gian hiện tại
            //DateTime now = DateTime.Now;
            DateTime now = new DateTime(KTGlobal.GetServerTime() * TimeSpan.TicksPerMillisecond);
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;

            /// Nếu là sự kiện toàn ngày thì lúc nào cũng có thể tham gia
            if (this.Time.Count == 0)
            {
                return ActivityState.CANJOIN;
            }

            /// Tìm xem thời điểm hiện tại có đang diễn ra gì không
            ActivityTime activityTime = this.Time.Where(x => x.Hour == nHour).FirstOrDefault();

            /// Nếu tìm thấy sự kiện đang diễn ra
            if (activityTime != null)
            {
                /// Nếu thời gian hiện tại > thời gian diễn ra
                if (nMinute > activityTime.Minute)
                {
                    /// Nếu vừa diễn ra chưa đầy 10 phút
                    if (nMinute - activityTime.Minute >= 5 && nMinute - activityTime.Minute < 10)
                    {
                        /// Đánh dấu trạng thái là đang diễn ra
                        state = ActivityState.OPEN;
                    }
                    /// Nếu đã diễn ra quá 10 phút
                    else if (nMinute - activityTime.Minute > 10)
                    {
                        /// Đánh dấu trạng thái là đã kết thúc
                        state = ActivityState.HASEND;
                    }
                }
                /// Nếu chưa đến thời gian bắt đầu
                else
                {   /// Đánh dấu trạng thái là có thể tham gia
                    state = ActivityState.CANJOIN;
                }
            }
            /// Nếu không tìm thấy sự kiện nghĩa là cả ngày
            else
            {
                /// Tìm ra 1 mốc thời gian sắp tới sẽ diễn ra nếu mà thời gian đang ko = thời gian bắt đầu sự kiện
                ActivityTime nextActivityTime = Time.Where(x => x.Hour > nHour).FirstOrDefault();

                /// Nếu tìm thấy
                if (nextActivityTime != null)
                {
                    /// Đánh dấu trạng thái có thể tham gia
                    state = ActivityState.CANJOIN;
                }
                /// Nếu không tìm ra được mốc nào thì tức là sự kiện này đã hết thời gian rồi
                else
                {
                    /// Đánh dấu trạng thái đã kết thúc
                    state = ActivityState.HASEND;
                }
            }

            /// Trả về trạng thái
            return state;
        }

        /// <summary>
        /// Trả về các mốc giờ của hoạt động
        /// </summary>
        /// <returns></returns>
        public string GetActivityTimes()
        {
            if (this.Time == null)
            {
                return "Toàn ngày";
            }
            else
            {
                List<string> timeInfoStrings = new List<string>();
                /// Duyệt danh sách thời gian
                foreach (ActivityTime activityTime in this.Time)
                {
                    timeInfoStrings.Add(activityTime.ToString());
                }
                /// Trả về kết quả
                return string.Join(", ", timeInfoStrings);
            }
        }

        /// <summary>
        /// Trả về 2 mốc thời gian gần nhất của hoạt động
        /// </summary>
        /// <returns></returns>
        public string GetNearestActivityTimes()
        {
            if (this.Time == null)
            {
                return "Toàn ngày";
            }
            else
            {
                List<string> timeInfoStrings = new List<string>();
                /// Thời gian hiện tại
                long nowTick = (long) (KTGlobal.GetServerTime() * TimeSpan.TicksPerMillisecond);
                DateTime nowDT = new DateTime(nowTick);
                /// Sắp xếp danh sách tăng dần theo thời gian
                List<ActivityTime> nearestActivities = this.Time.Where((activityTime) =>
                {
                    DateTime activityDT = new DateTime(nowDT.Year, nowDT.Month, nowDT.Day, activityTime.Hour, activityTime.Minute, 0, 0);
                    return activityDT >= nowDT;
                }).OrderBy((activityTime) =>
                {
                    DateTime activityDT = new DateTime(nowDT.Year, nowDT.Month, nowDT.Day, activityTime.Hour, activityTime.Minute, 0, 0);
                    return activityDT;
                }).Take(2).ToList();

                /// Duyệt danh sách thời gian
                foreach (ActivityTime activityTime in nearestActivities)
                {
                    timeInfoStrings.Add(activityTime.ToString());
                }
                /// Trả về kết quả
                return string.Join(", ", timeInfoStrings);
            }
        }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ActivityXML Parse(XElement xmlNode)
        {
            int ID = int.Parse(xmlNode.Attribute("ID").Value);
            Console.WriteLine(ID);
            /// Tạo mới đối tượng
            ActivityXML activity = new ActivityXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                DayOfWeek = new List<int>(),
                ActivityName = xmlNode.Attribute("ActivityName").Value,
                MapCode = int.Parse(xmlNode.Attribute("MapCode").Value),
                NPCID = int.Parse(xmlNode.Attribute("NPCID").Value),
                LevelJoin = int.Parse(xmlNode.Attribute("LevelJoin").Value),
                OtherRequiration = xmlNode.Attribute("OtherRequiration").Value,
                OtherRewards = xmlNode.Attribute("OtherRewards").Value,
                Description = xmlNode.Attribute("Description").Value,
                TypeDesc = xmlNode.Attribute("TypeDesc").Value,
                State = (ActivityState) System.Enum.Parse(typeof(ActivityState), xmlNode.Attribute("State").Value),
                MsgTips = xmlNode.Attribute("MsgTips") != null ? xmlNode.Attribute("MsgTips").Value : "",
                ItemReward = new List<int>(),
                Time = new List<ActivityTime>(),
            };

            /// Duyệt danh sách ngày trong tuần
            foreach (string dayStr in xmlNode.Attribute("DayOfWeek").Value.Split(';'))
            {
                /// Thêm vào danh sách
                activity.DayOfWeek.Add(int.Parse(dayStr));
            }

            /// Duyệt danh sách phần thưởng
            foreach (XElement node in xmlNode.Elements("ItemReward"))
            {
                /// Thêm vào danh sách
                activity.ItemReward.Add(int.Parse(node.Value));
            }

            /// Duyệt danh sách thời gian
            foreach (XElement node in xmlNode.Elements("Time"))
            {
                /// Thêm vào danh sách
                activity.Time.Add(new ActivityTime()
                {
                    Hour = int.Parse(node.Attribute("Hours").Value),
                    Minute = int.Parse(node.Attribute("Minute").Value),
                });
            }

            /// Trả về đối tượng
            return activity;
        }
    }
}
