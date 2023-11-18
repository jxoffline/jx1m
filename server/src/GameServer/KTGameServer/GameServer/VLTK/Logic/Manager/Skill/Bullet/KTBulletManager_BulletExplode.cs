using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý đạn bay
	/// </summary>
	public partial class KTBulletManager
	{
        /// <summary>
        /// Số luồng BulletDelayTask tối đa để tự kích hoạt chế độ tự hủy
        /// </summary>
        private const int MaxBulletDelayTasks = 500;

        #region Define
        /// <summary>
        /// Delay Task
        /// </summary>
        private class DelayTask
		{
            /// <summary>
            /// Sự kiện
            /// </summary>
            public Action Work { get; set; }

            /// <summary>
            /// Thời gian Delay
            /// </summary>
            public int DelayTicks { get; set; }

            /// <summary>
            /// Thời điểm khởi tạo
            /// </summary>
            public long InitTicks { get; set; } = 0;

            /// <summary>
            /// Đã đến thời gian chưa
            /// </summary>
            public bool IsInTime
			{
				get
				{
                    if (this.InitTicks <= 0)
					{
                        return false;
					}
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.DelayTicks;
				}
			}
		}
		#endregion

		#region Core
		/// <summary>
		/// Worker DelayTask
		/// </summary>
		private Dictionary<int, BackgroundWorker> bulletDelayTaskWorker;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartDelayTaskTimer()
        {
            /// Thời gian thực thi công việc lần cuối
            Dictionary<int, long> lastWorkerBusyTicks = new Dictionary<int, long>();
            /// Thời gian báo cáo trạng thái lần cuối
            Dictionary<int, long> lastReportStateTicks = new Dictionary<int, long>();
            /// Khởi tạo worker
            this.bulletDelayTaskWorker = new Dictionary<int, BackgroundWorker>();
            /// Duyệt danh sách phái
            foreach (KFaction.KFactionAttirbute faction in KFaction.GetFactions())
			{
                /// ID phái
                int factionID = faction.nID;
                /// Tạo worker tương ứng
                this.bulletDelayTaskWorker[factionID] = new BackgroundWorker();
                this.bulletDelayTaskWorker[factionID].DoWork += this.DoDelayTaskBackgroundWork;
                this.bulletDelayTaskWorker[factionID].RunWorkerCompleted += this.DelayTaskWorker_Completed;
                /// Thiết lập biến đếm giờ
                lastWorkerBusyTicks[factionID] = 0;
                lastReportStateTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                /// Tạo Dictionary
                this.listDelayTasks[factionID] = new List<DelayTask>();
                this.toRemoveDelayTasks[factionID] = new List<DelayTask>();
                /// Tạo Queue
                this.waitToBeAddedDelayTasks[factionID] = new ConcurrentQueue<DelayTask>();
            }

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "BulletDelayTaskTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                /// Duyệt danh sách Worker
                foreach (KeyValuePair<int, BackgroundWorker> pair in this.bulletDelayTaskWorker)
				{
                    /// ID môn phái tương ứng
                    int factionID = pair.Key;
                    /// Worker tương ứng
                    BackgroundWorker worker = pair.Value;

                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTicks[factionID] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Tick alive => BulletDelayTaskTimer, total faction ID '{0}' timers = {1}", factionID, this.listDelayTasks[factionID].Count));
                        lastReportStateTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (worker.IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTicks[factionID] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Tick alive => BulletDelayTaskTimer, total faction ID '{0}' timers = {1}", factionID, this.listDelayTasks[factionID].Count));
                        }
                    }
                    else
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi công việc
                        if (!worker.IsBusy)
                        {
                            worker.RunWorkerAsync(factionID);
                        }
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
        private void DelayTaskWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Skill, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoDelayTaskBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// ID môn phái
                int factionID = (int) e.Argument;

                /// Duyệt danh sách cần thêm
                while (!this.waitToBeAddedDelayTasks[factionID].IsEmpty)
                {
                    if (this.waitToBeAddedDelayTasks[factionID].TryDequeue(out DelayTask delayTask))
                    {
                        /// Xóa
                        this.listDelayTasks[factionID].Add(delayTask);
                    }
                }

                /// Đánh dấu tự hủy
                bool needToDestroy = false;

                /// Tổng số Timer đã xử lý
                int idx = 0;
                /// Duyệt danh sách người chơi
                while (idx < this.listDelayTasks[factionID].Count)
                {
                    /// Tăng tổng số Timer đã xử lý
                    idx++;
                    /// Nếu đã quá luồng thì tự hủy
                    if (idx >= KTBulletManager.MaxBulletDelayTasks)
                    {
                        /// Gửi yêu cầu tự hủy
                        needToDestroy = true;
                        this.toRemoveDelayTasks[factionID].Clear();
                        /// Thoát
                        break;
                    }

                    DelayTask delayTask = this.listDelayTasks[factionID][idx - 1];
                    /// Nếu đã đến giờ thực thi
                    if (delayTask.IsInTime)
					{
                        /// Thêm vào danh sách cần xóa
                        this.toRemoveDelayTasks[factionID].Add(delayTask);
                        /// Thực thi sự kiện
                        this.ExecuteAction(delayTask.Work);
                    }
                }

                /// Nếu có yêu cầu tự hủy
                if (needToDestroy)
                {
                    this.ClearBulletTimer(factionID);
                    this.ClearBulletDelayTask(factionID);
                }
				else
				{
                    /// Nếu có danh sách cần xóa
                    if (this.toRemoveDelayTasks[factionID].Count > 0)
                    {
                        int _idx = 0;
                        while (_idx < this.toRemoveDelayTasks[factionID].Count)
                        {
                            this.listDelayTasks[factionID].Remove(this.toRemoveDelayTasks[factionID][_idx]);
                            _idx++;
                        }
                    }
                    /// Làm rỗng danh sách cần xóa
                    this.toRemoveDelayTasks[factionID].Clear();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa rỗng danh sách Bullet Delay Task của phái tương ứng
        /// </summary>
        /// <param name="factionID"></param>
        private void ClearBulletDelayTask(int factionID)
		{
            LogManager.WriteLog(LogTypes.Skill, "Auto clear BulletDelayTaskTimer of faction '" + factionID + "', count = " + this.listDelayTasks[factionID].Count);
            this.listDelayTasks[factionID].Clear();

            /// Danh sách chờ
            ConcurrentQueue<DelayTask> queue = this.waitToBeAddedDelayTasks[factionID];
            /// Xóa hàng đợi luôn
            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out _);
            }

            /// Làm rỗng danh sách cần xóa
            this.toRemoveDelayTasks[factionID].Clear();
        }

        /// <summary>
        /// Xóa rỗng danh sách Bullet Delay Task
        /// </summary>
        private void ClearBulletDelayTasks()
		{
            /// Duyệt danh sách đang chờ
            foreach (KeyValuePair<int, ConcurrentQueue<DelayTask>> pair in this.waitToBeAddedDelayTasks)
			{
                this.ClearBulletDelayTask(pair.Key);
            }
        }
        #endregion

        /// <summary>
        /// Queue chứa danh sách sự kiện Delay cần thêm vào
        /// </summary>
        private readonly Dictionary<int, ConcurrentQueue<DelayTask>> waitToBeAddedDelayTasks = new Dictionary<int, ConcurrentQueue<DelayTask>>();

        /// <summary>
        /// Danh sách sự kiện Delay
        /// </summary>
        private readonly Dictionary<int, List<DelayTask>> listDelayTasks = new Dictionary<int, List<DelayTask>>();

        /// <summary>
        /// Danh sách sự kiện Delay cần xóa
        /// </summary>
        private readonly Dictionary<int, List<DelayTask>> toRemoveDelayTasks = new Dictionary<int, List<DelayTask>>();

        /// <summary>
        /// Thêm DelayTask vào danh sách
        /// </summary>
        /// <param name="task"></param>
        private void AddDelayTask(DelayTask task, GameObject caster)
        {
            /// ID phái
            int factionID = 0;
            /// Nếu là người chơi
            if (caster != null && caster is KPlayer player)
			{
                /// Gắn ID phái tương ứng
                factionID = player.m_cPlayerFaction.GetFactionId();
            }
            /// Thêm vào danh sách
            this.waitToBeAddedDelayTasks[factionID].Enqueue(task);
        }
    }
}
