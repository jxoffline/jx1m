using GameServer.KiemThe.Utilities;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý Schedule Task
	/// </summary>
	public class KTScheduleTaskManager
    {
        /// <summary>
        /// Số luồng ScheduleTask tối đa để tự kích hoạt chế độ tự hủy
        /// </summary>
        private const int MaxScheduleTask = 500;

        /// <summary>
        /// Buộc tự hủy
        /// </summary>
        private bool forceClear = false;

        /// <summary>
        /// Quản lý Schedule Task
        /// </summary>
        public static KTScheduleTaskManager Instance { get; private set; }

        /// <summary>
        /// Phần tử chờ thao tác trong hàng đợi
        /// </summary>
        private class QueueItem
		{
            /// <summary>
            /// Loại thao tác
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// Dữ liệu
            /// </summary>
            public object Data { get; set; }
		}

        /// <summary>
        /// Đối tượng Schedule Task dùng nội bộ
        /// </summary>
        private class InnerScheduleTask
		{
            /// <summary>
            /// Đối tượng ScheduleTask
            /// </summary>
            public ScheduleTask Task { get; set; }

            /// <summary>
            /// Thời điểm Tick lần trước
            /// </summary>
            public long LastTick { get; set; }
		}

        /// <summary>
        /// Danh sách Task đang thực thi
        /// </summary>
        private readonly Dictionary<long, InnerScheduleTask> Tasks = new Dictionary<long, InnerScheduleTask>();

        /// <summary>
        /// Danh sách quái cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
		{
            KTScheduleTaskManager.Instance = new KTScheduleTaskManager();
		}

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Quản lý Schedule Task
        /// </summary>
        public KTScheduleTaskManager()
		{
            /// Khởi tạo worker
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.DoBackgroundWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTScheduleTaskManager"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                //Console.WriteLine(string.Format("Tick alive => {0}", "KTScheduleTaskManager"));

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTScheduleTaskManager, total timers = " + this.Tasks.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTScheduleTaskManager, total timers = " + this.Tasks.Count));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    if (!this.worker.IsBusy)
                    {
                        this.worker.RunWorkerAsync();
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Sự kiện khi Background Worker hoàn tất công việc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Exception, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Duyệt danh sách chờ
                while (!this.waitingQueue.IsEmpty)
                {
                    if (this.waitingQueue.TryDequeue(out QueueItem item))
                    {
                        /// Nếu đối tượng không tồn tại
                        if (item.Data == null)
                        {
                            continue;
                        }

                        if (item.Type == 1)
                        {
                            ScheduleTask task = (ScheduleTask) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.Tasks.ContainsKey(task.ID))
                            {
                                /// Xóa Timer cũ
                                this.Tasks.Remove(task.ID);
                            }
                            /// Thêm vào danh sách
                            this.Tasks[task.ID] = new InnerScheduleTask()
                            {
                                Task = task,
                                LastTick = KTGlobal.GetCurrentTimeMilis(),
                            };
                        }
                        else if (item.Type == 0)
                        {
                            ScheduleTask task = (ScheduleTask) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.Tasks.ContainsKey(task.ID))
                            {
                                /// Xóa Timer cũ
                                this.Tasks.Remove(task.ID);
                            }
                        }
                    }
                }

                /// Đánh dấu tự hủy
                bool needToDestroy = false;

                /// Tổng số Timer đã xử lý
                int idx = 0;

                /// Duyệt danh sách
                foreach (InnerScheduleTask task in this.Tasks.Values)
                {
                    /// Tăng tổng số Timer đã xử lý
                    idx++;
                    /// Nếu đã quá luồng thì tự hủy
                    if (idx >= KTScheduleTaskManager.MaxScheduleTask)
                    {
                        /// Gửi yêu cầu tự hủy
                        needToDestroy = true;
                        /// Thoát
                        break;
                    }

                    /// Nếu đã đến thời điểm Tick
                    if (KTGlobal.GetCurrentTimeMilis() - task.LastTick >= task.Task.PeriodTick)
					{
                        /// Đánh dấu thời điểm Tick trước
                        task.LastTick = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi sự kiện Tick
                        this.ExecuteAction(task.Task.Tick);
					}
                }

                /// Nếu có yêu cầu tự hủy
                if (needToDestroy || this.forceClear)
                {
                    /// Hủy yêu cầu tự hủy
                    this.forceClear = false;
                    /// Thực hiện tự hủy
                    this.DoClearScheduleTasks();
                }
            }
			catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Thực thi sự kiện gì đó
        /// </summary>
        /// <param name="work"></param>
        private bool ExecuteAction(Action work)
        {
            try
            {
                work?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Thực hiện xóa toàn bộ ScheduleTask đang thực thi
        /// </summary>
        private void DoClearScheduleTasks()
        {
            LogManager.WriteLog(LogTypes.Skill, "Auto clear ScheduleTasks, count = " + this.Tasks.Count);
            this.Tasks.Clear();

            /// Xóa hàng đợi luôn
            while (!this.waitingQueue.IsEmpty)
            {
                this.waitingQueue.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Xóa toàn bộ luồng đang thực thi
        /// </summary>
        public void ClearScheduleTasks()
        {
            this.forceClear = true;
        }
        #endregion

        /// <summary>
        /// Thêm Schedule Task tương ứng vào danh sách
        /// </summary>
        /// <param name="task"></param>
        public void Add(ScheduleTask task)
		{
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 1,
                Data = task,
            });
		}

        /// <summary>
        /// Xóa Schedule Task tương ứng khỏi danh sách
        /// </summary>
        /// <param name="task"></param>
        public void Remove(ScheduleTask task)
		{
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 0,
                Data = task,
            });
		}
	}
}
