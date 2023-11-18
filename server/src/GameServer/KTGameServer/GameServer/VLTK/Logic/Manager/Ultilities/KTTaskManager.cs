using GameServer.KiemThe.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng quản lý các sự kiện liên quan đến thời gian
    /// </summary>
    public class KTTaskManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý các sự kiện liên quan đến thời gian
        /// </summary>
        public static KTTaskManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTTaskManager() { }
        #endregion

        #region Initialize
        /// <summary>
        /// Khởi tạo luồng quản lý đạn bay
        /// </summary>
        public static void Init()
        {
            KTTaskManager.Instance = new KTTaskManager();
        }
        #endregion

        /// <summary>
        /// Thời gian mỗi bước thực thi Task
        /// </summary>
        private const float DefaultTaskPeriodTime = 0.1f;

        #region Inheritance
        #region Task
        /// <summary>
        /// Chuỗi công việc thực thi theo thời gian
        /// </summary>
        public class KTTask
        {
            /// <summary>
            /// Tên Task
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// ID tự tăng của đối tượng
            /// </summary>
            private static int AutoID = 0;

            /// <summary>
            /// ID chuỗi công việc
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Danh sách công việc
            /// <para>Key: Sự kiện</para>
            /// <para>Value: Thời gian delay (nếu là DelayTask)</para>
            /// </summary>
            protected readonly Queue<KeyValuePair<Action, float>> works = new Queue<KeyValuePair<Action, float>>();

            /// <summary>
            /// Private constructor
            /// </summary>
            protected KTTask()
            {
                KTTask.AutoID = (KTTask.AutoID + 1) % 100000007;
                this.ID = KTTask.AutoID;
            }

            /// <summary>
            /// Khởi tạo đối tượng Task thực thi công việc
            /// </summary>
            /// <param name="work"></param>
            /// <returns></returns>
            public static KTTask New(Action work)
            {
                KTTask task = new KTTask();
                task.works.Enqueue(new KeyValuePair<Action, float>(work, -1));
                return task;
            }

            /// <summary>
            /// Khởi tạo đối tượng Task thực thi công việc
            /// </summary>
            /// <returns></returns>
            public static KTTask New()
            {
                KTTask task = new KTTask();
                return task;
            }

            /// <summary>
            /// Thực thi công việc tiếp theo
            /// </summary>
            /// <param name="work"></param>
            public KTTask Then(Action work)
            {
                this.works.Enqueue(new KeyValuePair<Action, float>(work, -1));
                return this;
            }

            /// <summary>
            /// Tạm dừng một khoảng thời gian
            /// </summary>
            /// <param name="sec"></param>
            /// <returns></returns>
            public KTTask Wait(float sec)
            {
                this.works.Enqueue(new KeyValuePair<Action, float>(null, sec));
                return this;
            }
        }

        /// <summary>
        /// Đối tượng kế thừa KTTask, dùng nội bộ trong Class
        /// </summary>
        private class KTTaskInner : KTTask
        {
            /// <summary>
            /// Danh sách công việc
            /// </summary>
            public Queue<KeyValuePair<Action, float>> Works
            {
                get
                {
                    return this.works;
                }
            }

            /// <summary>
            /// Đối tượng kế thừa KTTask, dùng nội bộ trong Class
            /// </summary>
            public KTTaskInner() : base()
            {

            }
        }

        /// <summary>
        /// Kế thừa KTTimer
        /// </summary>
        private class KTTaskTimer : KTTimer
		{

        }

        /// <summary>
        /// Đối tượng dùng nội bộ trong lớp
        /// </summary>
        private class InnerTask
        {
            /// <summary>
            /// Task
            /// </summary>
            public KTTaskInner Task { get; set; }

            /// <summary>
            /// Timer thực thi
            /// </summary>
            public KTTaskTimer Timer { get; private set; }

            /// <summary>
            /// Khoảng thời gian chờ lần trước
            /// </summary>
            private long lastWaitedMoment;

            /// <summary>
            /// Khoảng thời gian đợi
            /// </summary>
            private long waitingForSecond;

            /// <summary>
            /// Sự kiện khi công việc hoàn tất
            /// </summary>
            public Action Done { get; set; }

            /// <summary>
            /// Đối tượng dùng nội bộ trong lớp
            /// </summary>
            public InnerTask()
            {
                this.Timer = new KTTaskTimer()
                {
                    Name = this.Task.Name,
                    Interval = -1,
                    PeriodActivation = KTTaskManager.DefaultTaskPeriodTime,
                    Start = () => {
                        this.DoWork();
                    },
                    Tick = () => {
                        this.DoWork();
                    },
                };
            }

            /// <summary>
            /// Thực thi công việc
            /// </summary>
            private void DoWork()
            {
                /// Nếu đang thực hiện Task Delay
                if (this.waitingForSecond > 0)
                {
                    /// Nếu đã hết thời gian
                    if (DateTime.Now.Ticks - this.lastWaitedMoment >= this.waitingForSecond)
                    {
                        this.lastWaitedMoment = 0;
                        this.waitingForSecond = 0;
                        return;
                    }
                }

                /// Nếu danh sách Task vẫn còn
                if (this.Task.Works.Count > 0)
                {
                    KeyValuePair<Action, float> work = this.Task.Works.Dequeue();

                    /// Nếu là Task Delay
                    if (work.Value > 0)
                    {
                        this.lastWaitedMoment = DateTime.Now.Ticks;
                        this.waitingForSecond = (long)(work.Value * 1000);
                    }
                    /// Nếu là Task thường
                    else
                    {
                        work.Key?.Invoke();
                    }
                }
                /// Nếu không phải thì gọi sự kiện Finish
                else
                {
                    this.Timer.Alive = false;
                }
            }

            /// <summary>
            /// Dừng thực hiện chuỗi Task
            /// </summary>
            public void Stop()
            {
                this.Timer.Alive = false;
                this.Done?.Invoke();
            }
        }
        #endregion

        #region Schedule
        /// <summary>
        /// Lịch trình công việc
        /// </summary>
        public class KTSchedule
        {
            /// <summary>
            /// Tên lịch trình
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// ID tự tăng của đối tượng
            /// </summary>
            private static int AutoID = 0;

            /// <summary>
            /// ID đối tượng
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Khoảng thời gian lặp đi lặp lại công việc
            /// <para>Nếu Loop = false thì chỉ thực thi 1 lần duy nhất sau thời gian</para>
            /// </summary>
            public float Interval { get; set; }

            /// <summary>
            /// Lặp đi lặp lại
            /// </summary>
            public bool Loop { get; set; }

            /// <summary>
            /// Công việc cần thực hiện
            /// </summary>
            public Action Work { get; set; }

            /// <summary>
            /// Lịch trình công việc
            /// </summary>
            public KTSchedule()
            {
                KTSchedule.AutoID = (KTSchedule.AutoID + 1) % 100000007;
                this.ID = KTSchedule.AutoID;
            }
        }

        /// <summary>
        /// Đối tượng lịch trình nội bộ dùng trong class
        /// </summary>
        private class InnerSchedule
        {
            /// <summary>
            /// Lịch trình công việc
            /// </summary>
            public KTSchedule Schedule { get; private set; }

            /// <summary>
            /// Timer thực thi
            /// </summary>
            public KTTaskTimer Timer { get; private set; }

            /// <summary>
            /// Sự kiện khi công việc hoàn tất
            /// </summary>
            public Action Done { get; set; }

            /// <summary>
            /// Đối tượng lịch trình nội bộ dùng trong class
            /// </summary>
            public InnerSchedule(KTSchedule schedule) : base()
            {
                this.Schedule = schedule;
                this.Timer = new KTTaskTimer()
                {
                    Name = this.Schedule.Name,
                    Interval = this.Schedule.Loop ? -1 : this.Schedule.Interval,
                    PeriodActivation = this.Schedule.Loop ? this.Schedule.Interval : -1,
                    Tick = this.Schedule.Loop ? this.Schedule.Work : null,
                    Finish = this.Schedule.Loop ? this.Done : () => {
                        this.Schedule.Work?.Invoke();
                        this.Done?.Invoke();
                    },
                };
            }
        }
        #endregion

        /// <summary>
        /// Lớp quản lý tác vụ
        /// </summary>
        private class TimerManager : KTTimerManager<KTTaskTimer>
        {
            /// <summary>
            /// Thời gian kích hoạt luồng kiểm tra
            /// </summary>
            protected override int PeriodTick
            {
                get
                {
                    return 100;
                }
            }

            /// <summary>
            /// Đối tượng quản lý tác vụ
            /// </summary>
            public TimerManager() : base()
            {

            }
        }
        #endregion

        /// <summary>
        /// Đối tượng quản lý tác vụ
        /// </summary>
        private readonly TimerManager timer = new TimerManager();

        /// <summary>
        /// Danh sách Task đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, InnerTask> tasks = new ConcurrentDictionary<int, InnerTask>();

        /// <summary>
        /// Danh sách lịch trình công việc đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, InnerSchedule> schedules = new ConcurrentDictionary<int, InnerSchedule>();

        #region Public methods
        /// <summary>
        /// Thêm lịch trình
        /// </summary>
        /// <param name="schedule"></param>
        public void AddSchedule(KTSchedule schedule)
        {
            InnerSchedule _schedule = new InnerSchedule(schedule)
            {
                Done = () => {
                    this.RemoveSchedule(schedule);
                },
            };
            this.schedules.TryRemove(schedule.ID, out _);
            this.timer.AddTimer(_schedule.Timer);
        }

        /// <summary>
        /// Xóa lịch trình
        /// </summary>
        /// <param name="schedule"></param>
        public void RemoveSchedule(KTSchedule schedule)
        {
            this.schedules.TryGetValue(schedule.ID, out InnerSchedule _schedule);
            if (_schedule != null)
            {
                this.schedules.TryRemove(schedule.ID, out _);
                this.timer.KillTimer(_schedule.Timer);
            }
        }

        /// <summary>
        /// Thêm công việc
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(KTTask task)
        {
            InnerTask _task = new InnerTask()
            {
                Task = (KTTaskInner)task,
                Done = () => {
                    this.RemoveTask(task);
                },
            };
            this.tasks[task.ID] = _task;
            this.timer.AddTimer(_task.Timer);
        }

        /// <summary>
        /// Xóa công việc
        /// </summary>
        /// <param name="task"></param>
        public void RemoveTask(KTTask task)
        {
            this.tasks.TryGetValue(task.ID, out InnerTask _task);
            if (_task != null)
            {
                this.tasks.TryRemove(task.ID, out _);
                this.timer.KillTimer(_task.Timer);
            }
        }
        #endregion
    }
}
