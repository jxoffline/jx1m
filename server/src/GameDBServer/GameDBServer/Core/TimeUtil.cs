using GameDBServer.Logic;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GameDBServer.Core
{
    public class TimeUtil
    {
        /// <summary>
        /// Mini sec
        /// </summary>
        public const long MILLISECOND = 1;

        /// <summary>
        /// Giây
        /// </summary>
        public const long SECOND = 1000 * MILLISECOND;

        /// <summary>
        /// Phút
        /// </summary>
        public const long MINITE = 60 * SECOND;

        /// <summary>
        /// Giờ
        /// </summary>
        public const long HOUR = 60 * MINITE;

        /// <summary>
        /// Ngày
        /// </summary>
        public const long DAY = 24 * HOUR;

        private static int CurrentTickCount = 0;

        private static long CurrentTicks = DateTime.Now.Ticks / 10000;

        private static DateTime _Now = DateTime.Now;

        /// <summary>
        /// Láy ra thời gian hiện tại theo minisec
        /// </summary>
        public static long NOW()
        {
            int tickCount = Environment.TickCount;
            if (tickCount != CurrentTickCount)
            {
                _Now = DateTime.Now;
                CurrentTickCount = tickCount;
                CurrentTicks = _Now.Ticks / 10000;
            }
            return CurrentTicks;
        }

        public static DateTime NowDateTime()
        {
            int tickCount = Environment.TickCount;
            if (tickCount != CurrentTickCount)
            {
                _Now = DateTime.Now;
                CurrentTickCount = tickCount;
                CurrentTicks = _Now.Ticks / 10000;
            }
            return _Now;
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(ref long x);


        private static long _CounterPerSecs = 0;
        private static bool _EnabelPerformaceCounter = false;

        /// <summary>
        /// Lấy ra thời gian hiện tại EX
        /// </summary>
        /// <returns></returns>
        public static long NowEx()
        {
            if (GameDBManager.StatisticsMode == 0)
            {
                return CurrentTicks;
            }
            else if (GameDBManager.StatisticsMode == 1)
            {
                return CurrentTicks;
            }
            else if (GameDBManager.StatisticsMode == 2)
            {
                return NOW();
            }
            else if (GameDBManager.StatisticsMode == 3)
            {
                return DateTime.Now.Ticks / 10000;
            }
            else
            {
                if (_EnabelPerformaceCounter)
                {
                    long counter = 0;
                    QueryPerformanceCounter(ref counter);
                    return counter;
                }
                else
                {
                    return DateTime.Now.Ticks;
                }
            }
        }

        /// <summary>
        /// Time to ms
        /// </summary>
        /// <param name="time"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public static double TimeMS(long time, int round = 2)
        {
            if (GameDBManager.StatisticsMode <= 3)
            {
                return time;
            }
            else
            {
                long timeDiff = TimeDiff(time, 0);
                return Math.Round(timeDiff / 10000.0, round);
            }
        }

        /// <summary>
        /// Lấy ra tuần trong năm
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Tính ra chênh lệch giữa 2 mốc thời gian
        /// </summary>
        /// <param name="timeEnd"></param>
        /// <param name="timeStart"></param>
        /// <returns></returns>
        public static long TimeDiff(long timeEnd, long timeStart = 0)
        {
            if (GameDBManager.StatisticsMode <= 3)
            {
                return timeEnd - timeStart;
            }
            else if (_EnabelPerformaceCounter)
            {
                long counter = timeEnd - timeStart;
                long count1;
                long secs = Math.DivRem(counter, _CounterPerSecs, out count1);
                return secs * 10000000 + count1 * 10000000 / _CounterPerSecs;
            }
            else
            {
                return timeEnd - timeStart;
            }
        }


        /// <summary>
        /// Lấy ra tổng số giờ kể từ năm 2011
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
        /// Lấy ra tổng số giây kể từ năm 2011
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetOffsetSecond(DateTime date)
        {
            TimeSpan ts = date - DateTime.Parse("2022-11-1");
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        /// Lấy ra tổng số ngày kể từ năm 2011
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
        /// Lấy ra số ngày tưới ngày hiện tại
        /// </summary>
        /// <returns></returns>
        public static int GetOffsetDayNow()
        {
            return GetOffsetDay(TimeUtil.NowDateTime());
        }
    }
}