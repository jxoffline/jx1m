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
        /// Số luồng Bullet tối đa để tự kích hoạt chế độ tự hủy
        /// </summary>
        private const int MaxBulletTimers = 2000;

		#region Core
        /// <summary>
        /// Worker
        /// </summary>
        private Dictionary<int, BackgroundWorker> worker;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer()
        {
            /// Thời gian thực thi công việc lần cuối
            Dictionary<int, long> lastWorkerBusyTicks = new Dictionary<int, long>();
            /// Thời gian báo cáo trạng thái lần cuối
            Dictionary<int, long> lastReportStateTicks = new Dictionary<int, long>();
            /// Khởi tạo worker
            this.worker = new Dictionary<int, BackgroundWorker>();
            /// Duyệt danh sách phái
            foreach (KFaction.KFactionAttirbute faction in KFaction.GetFactions())
			{
                /// ID phái
                int factionID = faction.nID;
                /// Khởi tạo worker
                this.worker[factionID] = new BackgroundWorker();
                this.worker[factionID].DoWork += this.DoBackgroundWork;
                this.worker[factionID].RunWorkerCompleted += this.Worker_Completed;
                /// Thiết lập biến đếm giờ
                lastWorkerBusyTicks[factionID] = 0;
                lastReportStateTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                /// Tạo Dictionary
                this.bulletTimers[factionID] = new List<BulletTimer>();
                this.toRemoveBulletTimers[factionID] = new List<BulletTimer>();
                /// Tạo Queue
                this.listWaitingToBeAdded[factionID] = new ConcurrentQueue<BulletTimer>();
            }


            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "BulletTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                /// Duyệt danh sách Worker
                foreach (KeyValuePair<int, BackgroundWorker> pair in this.worker)
                {
                    /// ID môn phái tương ứng
                    int factionID = pair.Key;
                    /// Worker tương ứng
                    BackgroundWorker worker = pair.Value;

                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTicks[factionID] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Tick alive => BulletTimer, total faction ID '{0}' timers = {1}", factionID, this.bulletTimers[factionID].Count));
                        lastReportStateTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (worker.IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTicks[factionID] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTicks[factionID] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => BulletTimer, total faction ID '{0}' timers = {1}", factionID, this.bulletTimers[factionID].Count));
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
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Skill, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// ID môn phái
                int factionID = (int) e.Argument;

                /// Duyệt danh sách cần thêm vào
                while (!this.listWaitingToBeAdded[factionID].IsEmpty)
                {
                    if (this.listWaitingToBeAdded[factionID].TryDequeue(out BulletTimer bulletTimer))
                    {
                        /// Thêm
                        this.bulletTimers[factionID].Add(bulletTimer);
                        bulletTimer.LifeTime = 0;
                    }
                }

                /// Đánh dấu tự hủy
                bool needToDestroy = false;

                /// Tổng số Timer đã xử lý
                int idx = 0;
                /// Duyệt danh sách người chơi
                while (idx < this.bulletTimers[factionID].Count)
                {
                    /// Tăng tổng số Timer đã xử lý
                    idx++;
                    /// Nếu đã quá luồng thì tự hủy
                    if (idx >= KTBulletManager.MaxBulletTimers)
                    {
                        /// Gửi yêu cầu tự hủy
                        needToDestroy = true;
                        this.toRemoveBulletTimers[factionID].Clear();
                        /// Thoát
                        break;
                    }

                    BulletTimer bulletTimer = this.bulletTimers[factionID][idx - 1];

                    /// Nếu toác
                    if (bulletTimer == null)
                    {
                        this.bulletTimers[factionID].RemoveAt(idx - 1);
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu không còn sống
                    if (!bulletTimer.Alive)
					{
                        /// Thực thi sự kiện Destroy
                        this.ExecuteAction(bulletTimer.Destroy);
                        /// Thêm vào danh sách xóa
                        this.toRemoveBulletTimers[factionID].Add(bulletTimer);
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu đã hết thời gian
                    if (bulletTimer.IsOver)
                    {
                        /// Đánh dấu đã chết
                        bulletTimer.Alive = false;
                        /// Thêm vào danh sách cần xóa
                        this.toRemoveBulletTimers[factionID].Add(bulletTimer);
                        /// Thực thi sự kiện Finish
                        this.ExecuteAction(bulletTimer.Finish);
                        /// Thực thi sự kiện Destroy
                        this.ExecuteAction(bulletTimer.Destroy);
                        /// Bỏ qua
                        continue;
                    }

					/// Nếu chưa bắt đầu
					if (!bulletTimer.IsStarted)
					{
						/// Đánh dấu đã bắt đầu
						bulletTimer.IsStarted = true;
						/// Thực thi sự kiện Start
						bool ret = this.ExecuteAction(bulletTimer.Start);
						/// Nếu có lỗi
						if (!ret)
						{
							/// Nếu có thiết lập xóa khi gặp lỗi
							if (bulletTimer.StopWhenExceptionOccurs)
							{
								/// Đánh dấu đã chết
								bulletTimer.Alive = false;
								/// Thêm vào danh sách cần xóa
								this.toRemoveBulletTimers[factionID].Add(bulletTimer);
								/// Thực thi sự kiện Destroy
								this.ExecuteAction(bulletTimer.Destroy);
								/// Bỏ qua
								continue;
							}
						}
						/// Đánh dấu thời điểm Tick
						bulletTimer.LastTick = KTGlobal.GetCurrentTimeMilis();
					}

					/// Nếu đã đến thời gian Tick
					if (bulletTimer.IsTickTime)
                    {
                        /// Tăng thời gian sống lên
                        bulletTimer.LifeTime += bulletTimer.PeriodTicks;
                        /// Nếu đã hết thời gian
                        if (bulletTimer.IsOver)
                        {
                            /// Đánh dấu đã chết
                            bulletTimer.Alive = false;
                            /// Thêm vào danh sách cần xóa
                            this.toRemoveBulletTimers[factionID].Add(bulletTimer);
                            /// Thực thi sự kiện Finish
                            this.ExecuteAction(bulletTimer.Finish);
                            /// Thực thi sự kiện Destroy
                            this.ExecuteAction(bulletTimer.Destroy);
                            /// Bỏ qua
                            continue;
                        }
                        /// Đánh dấu thời điểm Tick
                        bulletTimer.LastTick = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi sự kiện Tick
                        bool ret = this.ExecuteAction(bulletTimer.Tick);
                        //Console.WriteLine("Tick => " + bulletTimer.Bullet.Skill.Data.Name);
                        /// Nếu có lỗi
                        if (!ret)
                        {
                            /// Nếu có thiết lập xóa khi gặp lỗi
                            if (bulletTimer.StopWhenExceptionOccurs)
                            {
                                /// Đánh dấu đã chết
                                bulletTimer.Alive = false;
                                /// Thêm vào danh sách cần xóa
                                this.toRemoveBulletTimers[factionID].Add(bulletTimer);
                                /// Thực thi sự kiện Destroy
                                this.ExecuteAction(bulletTimer.Destroy);
                                /// Bỏ qua
                                continue;
                            }
                        }
                    }
                }

                /// Nếu nhiều hơn 10k luồng hoặc có yêu cầu tự hủy thì tự hủy
                if (needToDestroy)
                {
                    this.ClearBulletTimer(factionID);
                    this.ClearBulletDelayTask(factionID);
                }
				else
				{
                    /// Nếu tồn tại danh sách cần xóa
                    if (this.toRemoveBulletTimers[factionID].Count > 0)
                    {
                        int _idx = 0;
                        while (_idx < this.toRemoveBulletTimers[factionID].Count)
                        {
                            this.bulletTimers[factionID].Remove(this.toRemoveBulletTimers[factionID][_idx]);
                            _idx++;
                        }
                    }
                    /// Làm rỗng danh sách cần xóa
                    this.toRemoveBulletTimers[factionID].Clear();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
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
        /// Xóa luồng thực thi Logic Bullet của phái tương ứng
        /// </summary>
        /// <param name="factionID"></param>
        private void ClearBulletTimer(int factionID)
		{
            LogManager.WriteLog(LogTypes.Skill, "Auto clear BulletTimer of faction '" + factionID + "', count = " + this.bulletTimers[factionID].Count);
            this.bulletTimers[factionID].Clear();
            /// Danh sách chờ
            ConcurrentQueue<BulletTimer> queue = this.listWaitingToBeAdded[factionID];
            /// Xóa hàng đợi luôn
            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out _);
            }

            /// Làm rỗng danh sách cần xóa
            this.toRemoveBulletTimers[factionID].Clear();
        }

        /// <summary>
        /// Xóa luồng thực thi Logic Bullet
        /// </summary>
        private void ClearBulletTimers()
		{
            /// Duyệt danh sách đang chờ
            foreach (KeyValuePair<int, ConcurrentQueue<BulletTimer>> pair in this.listWaitingToBeAdded)
            {
                this.ClearBulletTimer(pair.Key);
            }
        }
        #endregion

        /// <summary>
        /// Queue chứa danh sách đạn cần thêm vào
        /// </summary>
        private readonly Dictionary<int, ConcurrentQueue<BulletTimer>> listWaitingToBeAdded = new Dictionary<int, ConcurrentQueue<BulletTimer>>();

        /// <summary>
        /// Danh sách đạn
        /// </summary>
        private readonly Dictionary<int, List<BulletTimer>> bulletTimers = new Dictionary<int, List<BulletTimer>>();

        /// <summary>
        /// Danh sách đạn
        /// </summary>
        private readonly Dictionary<int, List<BulletTimer>> toRemoveBulletTimers = new Dictionary<int, List<BulletTimer>>();

        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="bulletTimer"></param>
        private void AddBulletTimer(BulletTimer bulletTimer, GameObject caster)
        {
            /// ID phái
            int factionID = 0;
            /// Nếu là người chơi
            if (caster != null && caster is KPlayer player)
            {
                /// Gắn ID phái tương ứng
                factionID = player.m_cPlayerFaction.GetFactionId();
            }
            /// Thêm vào Timer
            this.listWaitingToBeAdded[factionID].Enqueue(bulletTimer);
        }
    }
}
