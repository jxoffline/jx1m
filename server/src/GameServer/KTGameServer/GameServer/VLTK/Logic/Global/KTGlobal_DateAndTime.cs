using GameServer.Core.Executor;
using System;
using System.Globalization;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Trả về giờ hệ thống hiện tại dưới đơn vị Mili giây
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeMilis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Trả về thứ tự tuần trong tháng
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentWeekOfMonth()
        {
            CultureInfo curr = CultureInfo.CurrentCulture;
            int week = curr.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return week;
        }

        /// <summary>
        /// Lấy ra tổng số giờ kể từ năm 2022
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetOffsetHour(DateTime date)
        {
            TimeSpan ts = date - DateTime.Parse("2022-11-1");
            int hour = (int)ts.TotalHours;
            return hour;
        }

        /// <summary>
        /// Lấy ra tổng số giây kể từ năm 2022
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetOffsetSecond(DateTime date)
        {
            TimeSpan ts = date - DateTime.Parse("2022-11-1");
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// Lấy ra tổng số ngày kể từ năm 2022
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetOffsetDay(DateTime date)
        {
            TimeSpan ts = date - DateTime.Parse("2022-11-1");
            int day = (int)ts.TotalDays;
            return day;
        }

        /// <summary>
        /// Lấy ra tổng số ngày kể từ năm 2022
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetOffsetDay()
        {
            return KTGlobal.GetOffsetDay(DateTime.Now);
        }

        /// <summary>
        /// Lấy ra tổng số giờ kể từ năm 2022
        /// </summary>
        /// <returns></returns>
        public static int GetOffsetHour()
        {
            return KTGlobal.GetOffsetHour(DateTime.Now);
        }

        /// <summary>
        /// Lấy ra tổng số giây kể từ năm 2022
        /// </summary>
        /// <returns></returns>
        public static int GetOffsetSecond()
        {
            return KTGlobal.GetOffsetSecond(DateTime.Now);
        }
    }
}
