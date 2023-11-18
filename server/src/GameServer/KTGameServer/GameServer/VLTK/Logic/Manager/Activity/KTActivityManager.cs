using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents;
using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý sự kiện trong Game
    /// </summary>
    public static class KTActivityManager
    {
        #region Thiết lập
        /// <summary>
        /// Danh sách sự kiện
        /// </summary>
        private static readonly ConcurrentDictionary<int, KTActivity> activities = new ConcurrentDictionary<int, KTActivity>();

        /// <summary>
        /// Tổng số sự kiện
        /// </summary>
        public static int TotalActivities
        {
            get
            {
                return KTActivityManager.activities.Count;
            }
        }
        #endregion

        #region Khởi tạo
        /// <summary>
        /// Tải danh sách sự kiện trong hệ thống
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Activity/Activity.xml");
            foreach (XElement node in xmlNode.Elements("Activity"))
            {
                ActivityXML activity = ActivityXML.Parse(node);
                KTActivityManager.activities[activity.ID] = new KTActivity()
                {
                    Data = activity,
                    IsStarted = false,
                };
            }

            /// Thực thi kiểm tra sự kiện liên tục
            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("KTActivityManager", (o, e) => {
                KTActivityManager.ProcessCheckActivities();
            }), 0, 10000);
        }

        /// <summary>
        /// Thực thi kiểm tra và bắt đầu các sự kiện khi đến thời gian
        /// </summary>
        private static void ProcessCheckActivities()
        {
            /// Duyệt danh sách sự kiện
            foreach (KTActivity activity in KTActivityManager.activities.Values)
            {
                /// Nếu đã đến thời gian bắt đầu
                if (!activity.IsStarted && KTActivityManager.IsActivityOnTime(activity))
                {
                    KTActivityManager.StartActivity(activity.Data.ID);
                }
            }
        }

        /// <summary>
        /// Kiểm tra sự kiện đã đến thời gian chưa
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private static bool IsActivityOnTime(KTActivity activity)
        {
            /// Nếu sự kiện đã bắt đầu rồi
            if (activity.IsStarted)
            {
                return false;
            }
            /// Nếu sự kiện không mở
            if (activity.Data.Type == ActivityType.None)
            {
                return false;
            }

            try
            {
                /// Duyệt danh sách thời gian
                foreach (string timeString in activity.Data.Time.Split('_'))
				{
                    /// Thiết lập thời gian
                    string[] timeSettings = timeString.Split(';');
                    /// Loại sự kiện
                    switch (activity.Data.Type)
                    {
                        /// Nếu là sự kiện 1 lần duy nhất sau khi khởi động máy chủ
                        case ActivityType.OnceOnly:
                        {
                            /// Nếu thiết lập thời gian không rỗng
                            if (timeSettings.Length != 0)
                            {
                                continue;
                            }

                            return true;
                        }
                        /// Nếu là sự kiện mỗi ngày
                        case ActivityType.Everyday:
                        {
                            /// Nếu thiết lập thời gian không thỏa mãn
                            if (timeSettings.Length != 2)
                            {
                                continue;
                            }
                            /// Giờ
                            int nHour = int.Parse(timeSettings[0]);
                            /// Phút
                            int nMinute = int.Parse(timeSettings[1]);

                            /// Giờ hiện tại
                            int currentHour = DateTime.Now.Hour;
                            /// Phút hiện tại
                            int currentMinute = DateTime.Now.Minute;

                            /// Trả về kết quả
                            if (nHour == currentHour && nMinute == currentMinute)
							{
                                return true;
							}

                            break;
                        }
                        /// Nếu là sự kiện mỗi tuần
                        case ActivityType.EveryWeek:
                        {
                            /// Nếu thiết lập thời gian không thỏa mãn
                            if (timeSettings.Length != 3)
                            {
                                continue;
                            }
                            /// Thứ tự ngày trong tuần
                            int nWeekDay = int.Parse(timeSettings[0]);
                            /// Giờ
                            int nHour = int.Parse(timeSettings[1]);
                            /// Phút
                            int nMinute = int.Parse(timeSettings[2]);

                            /// Thứ trong tuần của ngày hiện tại
                            int currentWeekDay = (int) DateTime.Now.DayOfWeek;
                            /// Giờ hiện tại
                            int currentHour = DateTime.Now.Hour;
                            /// Phút hiện tại
                            int currentMinute = DateTime.Now.Minute;

                            /// Trả về kết quả
                            if (currentWeekDay == nWeekDay && nHour == currentHour && nMinute == currentMinute)
							{
                                return true;
							}

                            break;
                        }
                        case ActivityType.EveryMonthDay:
                        {
                            /// Nếu thiết lập thời gian không thỏa mãn
                            if (timeSettings.Length != 3)
                            {
                                continue;
                            }
                            /// Ngày trong tháng
                            int nDay = int.Parse(timeSettings[0]);
                            /// Giờ
                            int nHour = int.Parse(timeSettings[1]);
                            /// Phút
                            int nMinute = int.Parse(timeSettings[2]);

                            /// Ngày hiện tại
                            int currentDay = DateTime.Now.Day;
                            /// Giờ hiện tại
                            int currentHour = DateTime.Now.Hour;
                            /// Phút hiện tại
                            int currentMinute = DateTime.Now.Minute;

                            /// Trả về kết quả
                            if (currentDay == nDay && nHour == currentHour && nMinute == currentMinute)
							{
                                return true;
							}

                            break;
                        }
                        case ActivityType.EveryMonthWeekDay:
                        {
                            /// Nếu thiết lập thời gian không thỏa mãn
                            if (timeSettings.Length != 4)
                            {
                                continue;
                            }
                            /// Tuần trong tháng
                            int nWeek = int.Parse(timeSettings[0]);
                            /// Thứ tự ngày trong tuần
                            int nWeekDay = int.Parse(timeSettings[1]);
                            /// Giờ
                            int nHour = int.Parse(timeSettings[2]);
                            /// Phút
                            int nMinute = int.Parse(timeSettings[3]);

                            /// Tuần hiện tại
                            int currentWeek = KTGlobal.GetCurrentWeekOfMonth();
                            /// Ngày hiện tại
                            int currentWeekDay = (int) DateTime.Now.DayOfWeek;
                            /// Giờ hiện tại
                            int currentHour = DateTime.Now.Hour;
                            /// Phút hiện tại
                            int currentMinute = DateTime.Now.Minute;

                            /// Trả về kết quả
                            if (currentWeek == nWeek && currentWeekDay == nWeekDay && nHour == currentHour && nMinute == currentMinute)
							{
                                return true;
							}

                            break;
                        }
                        case ActivityType.FixedDateTime:
                        {
                            /// Nếu thiết lập thời gian không thỏa mãn
                            if (timeSettings.Length != 5)
                            {
                                continue;
                            }
                            /// Ngày
                            int nDate = int.Parse(timeSettings[0]);
                            /// Tháng
                            int nMonth = int.Parse(timeSettings[1]);
                            /// Năm
                            int nYear = int.Parse(timeSettings[2]);
                            /// Giờ
                            int nHour = int.Parse(timeSettings[3]);
                            /// Phút
                            int nMinute = int.Parse(timeSettings[4]);

                            /// Ngày hiện tại
                            int currentDate = DateTime.Now.DayOfYear;
                            /// Tháng hiện tại
                            int currentMonth = DateTime.Now.Month;
                            /// Năm hiện tại
                            int currentYear = DateTime.Now.Year;
                            /// Giờ hiện tại
                            int currentHour = DateTime.Now.Hour;
                            /// Phút hiện tại
                            int currentMinute = DateTime.Now.Minute;

                            /// Trả về kết quả
                            if (currentYear == nYear && currentMonth == nMonth && currentDate == nDate && nHour == currentHour && nMinute == currentMinute)
							{
                                return true;
							}

                            break;
                        }
                    }
                }
            }
            catch (Exception) { }

            /// Nếu không thỏa mãn
            return false;
        }

        /// <summary>
        /// Buộc bắt đầu một sự kiện nào đó mà không cần đợi đến thời gian thiết lập
        /// </summary>
        /// <param name="id"></param>
        public static void StartActivity(int id)
        {
            if (KTActivityManager.activities.TryGetValue(id, out KTActivity activity))
            {
                GameMapEventsManager.StartGameMapEvent(activity);
            }
        }

        /// <summary>
        /// Buộc kết thúc một sự kiện nào đó mà không cần đợi đến thười gian thiết lập
        /// </summary>
        /// <param name="id"></param>
        public static void StopActivity(int id)
        {
            if (KTActivityManager.activities.TryGetValue(id, out KTActivity activity))
            {
                activity.IsStarted = false;

                GameMapEventsManager.StopGameMapEvent(activity);
            }
        }
        #endregion
    }
}
