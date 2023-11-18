#define USING_MULTIPLE_TIMERS

using GameServer.KiemThe.Utilities;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý Logic của kỹ năng
	/// </summary>
	public static partial class KTSkillManager
	{
#if !USING_MULTIPLE_TIMERS
        #region Define
        /// <summary>
        /// Đối tượng luồng quản lý theo thời gian
        /// </summary>
        private static readonly SkillInnerTimer timer = new SkillInnerTimer();


		/// <summary>
		/// Kế thừa KTTimer
		/// </summary>
		private class KTSkillTimer : KTTimer
        {

        }

        /// <summary>
        /// Đối tượng quản lý luồng thực thi thời gian
        /// </summary>
        private class SkillInnerTimer : KTTimerManager<KTSkillTimer>
        {
            /// <summary>
            /// Thời gian Tick
            /// </summary>
            protected override int PeriodTick
            {
                get
                {
                    return 100;
                }
            }

            /// <summary>
            /// Thời gian tick mỗi lần của luồng
            /// </summary>
            public int TimerTick
            {
                get
                {
                    return this.PeriodTick;
                }
            }

            /// <summary>
            /// Đánh dấu sử dụng đa luồng
            /// </summary>
			protected override bool UseMultiThreading
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Số luồng sinh ra đồng thời cùng một lúc
            /// </summary>
			protected override int MaxThreadsEach
            {
                get
                {
                    return 100;
                }
            }

            /// <summary>
            /// Đối tượng quản lý luồng thực thi thời gian
            /// </summary>
            public SkillInnerTimer() : base()
            {
            }

            /// <summary>
            /// Tạo mới một Timer thực thi sự kiện
            /// </summary>
            /// <param name="sec">Thời gian chờ (giây), tối thiểu 0.1s, nếu dưới 0.1s thì thực thi ngay lập tức</param>
            /// <param name="function">Sự kiện</param>
            public void SetTimeout(float sec, Action function)
            {
                KTSkillTimer timer = new KTSkillTimer
                {
                    Name = "SkillTimer",
                    Interval = sec,
                    PeriodActivation = -1,
                    Finish = function,
                    Alive = true,
                };
                this.AddTimer(timer);
            }

            /// <summary>
            /// Tạo mới một lịch trình thực hiện lặp đi lặp lại công việc trong khoảng thời gian cho trước
            /// </summary>
            /// <param name="tick">Thời gian tick liên tục (giây), tối thiểu 0.1s</param>
            /// <param name="interval">Thời gian tồn tại (giây), tối thiểu 0.1s</param>
            /// <param name="function">Sự kiện</param>
            /// <returns></returns>
            public KTTimer SetSchedule(float tick, float interval, Action function)
            {
                KTSkillTimer timer = new KTSkillTimer()
                {
                    Name = "SkillTimer",
                    Interval = interval,
                    PeriodActivation = tick,
                    Tick = function,
                    Alive = true,
                };
                this.AddTimer(timer);
                return timer;
            }

            /// <summary>
            /// Tạo mới một lịch trình thực hiện lặp đi lặp lại công việc trong khoảng thời gian cho trước
            /// </summary>
            /// <param name="tick">Thời gian tick liên tục (giây), tối thiểu 0.1s</param>
            /// <param name="interval">Thời gian tồn tại (giây), tối thiểu 0.1s</param>
            /// <param name="function">Sự kiện</param>
            /// <param name="finish">Sự kiện khi hoàn tất</param>
            /// <returns></returns>
            public KTTimer SetSchedule(float tick, float interval, Action function, Action finish)
            {
                KTSkillTimer timer = new KTSkillTimer()
                {
                    Name = "SkillTimer",
                    Interval = interval,
                    PeriodActivation = tick,
                    Tick = function,
                    Finish = finish,
                    Alive = true,
                };
                this.AddTimer(timer);
                return timer;
            }

            /// <summary>
            /// Tạo mới một lịch trình thực hiện lặp đi lặp lại công việc
            /// </summary>
            /// <param name="tick">Thời gian tick liên tục (giây), tối thiểu 0.1s</param>
            /// <param name="function">Sự kiện</param>
            /// <returns></returns>
            public KTTimer SetSchedule(float tick, Action function)
            {
                KTSkillTimer timer = new KTSkillTimer()
                {
                    Name = "SkillTimer",
                    Interval = -1,
                    PeriodActivation = tick,
                    Tick = function,
                    Alive = true,
                };
                this.AddTimer(timer);
                return timer;
            }
        }
        #endregion
#endif

#if USING_MULTIPLE_TIMERS
        #region Core
        /// <summary>
        /// Thực thi sự kiện
        /// </summary>
        /// <param name="work"></param>
        private static bool ExecuteAction(Action work)
        {
            try
            {
                /// Thời điểm Tick hiện tại
                long currentTick = KTGlobal.GetCurrentTimeMilis();

                /// Thực thi công việc
                work?.Invoke();

                /// Tổng thời gian thực thi
                long totalProcessTicks = KTGlobal.GetCurrentTimeMilis() - currentTick;
                /// Nếu quá 1s
                if (totalProcessTicks >= 500)
                {
                    LogManager.WriteLog(LogTypes.Info, string.Format("Tick {0} => Total ticks = {1}(ms)", "KTSkillTimer", totalProcessTicks));
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, string.Format("Exception at {0}\n{1}", "KTSkillTimer", ex.ToString()));
                return false;
            }
        }
        #endregion
#endif

        #region Basic functions
        /// <summary>
        /// Thực thi Task sau khoảng Delay chỉ định
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="callback"></param>
        public static void SetTimeout(float delay, Action callback)
        {
#if !USING_MULTIPLE_TIMERS
            KTSkillManager.timer.SetTimeout(delay, callback);
#else
            if (delay <= 0.1f)
			{
                callback?.Invoke();
			}
			else
			{
                ScheduleTask task = new ScheduleTask()
                {
                    PeriodTick = -1,
                    Duration = (int) (delay * 1000),
                    Work = callback,
                };
                task.Start();
            }
#endif
        }

        /// <summary>
        /// Thực hiện công việc lặp đi lặp lại
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="callback"></param>
        public static void SetSchedule(float tick, Action callback)
        {
#if !USING_MULTIPLE_TIMERS
            KTSkillManager.timer.SetSchedule(tick, callback);
#else
            ScheduleTask task = new ScheduleTask()
            {
                PeriodTick = (int) (tick * 1000),
                Duration = -1,
                Work = callback,
            };
            task.Start();
#endif
        }

        /// <summary>
        /// Thực hiện công việc lặp đi lặp lại trong khoảng tương ứng
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="interval"></param>
        /// <param name="callback"></param>
        public static void SetSchedule(float tick, float interval, Action callback)
        {
#if !USING_MULTIPLE_TIMERS
            KTSkillManager.timer.SetSchedule(tick, interval, callback);
#else
            ScheduleTask task = new ScheduleTask()
            {
                PeriodTick = (int) (tick * 1000),
                Duration = (int) (interval * 1000),
                Work = callback,
            };
            task.Start();
#endif
        }

        /// <summary>
        /// Tạo mới một lịch trình thực hiện lặp đi lặp lại công việc trong khoảng thời gian cho trước
        /// </summary>
        /// <param name="tick">Thời gian tick liên tục (giây), tối thiểu 0.1s</param>
        /// <param name="interval">Thời gian tồn tại (giây), tối thiểu 0.1s</param>
        /// <param name="function">Sự kiện</param>
        /// <param name="finish">Sự kiện khi hoàn tất</param>
        /// <returns></returns>
        public static void SetSchedule(float tick, float interval, Action function, Action finish)
        {
#if !USING_MULTIPLE_TIMERS
            KTSkillManager.timer.SetSchedule(tick, interval, function, finish);
#else
            ScheduleTask task = new ScheduleTask()
            {
                PeriodTick = (int) (tick * 1000),
                Duration = (int) (interval * 1000),
                Work = function,
                Finish = finish,
            };
            task.Start();
#endif
        }
        #endregion
    }
}
