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
    /// Quản lý luồng vật phẩm rơi
    /// </summary>
    public partial class KTStallBotTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, StallBotTimer> bots = new ConcurrentDictionary<int, StallBotTimer>();

        /// <summary>
        /// Danh sách quái cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Chạy luồng quản lý quái thường
        /// </summary>
        private void StartTimer()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = 0;
            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Khởi tạo Worker
            this.worker = new BackgroundWorker();
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.DoBackgroundWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;
            lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
            lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTStallBotTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTStallBotTimer, total timers = " + this.bots.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTStallBotTimer, total timers = " + this.bots.Count));
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
                            StallBotTimer newTimer = (StallBotTimer) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.bots.TryGetValue(newTimer.RefObject.RoleID, out StallBotTimer timer))
                            {
                                /// Xóa Timer cũ
                                this.bots.TryRemove(timer.RefObject.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.bots[newTimer.RefObject.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            KStallBot bot = (KStallBot) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.bots.TryGetValue(bot.RoleID, out StallBotTimer timer))
                            {
                                /// Thực hiện hủy đối tượng
                                timer.RefObject.Destroy();
                                /// Xóa Timer cũ
                                this.bots.TryRemove(bot.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> totalRemoveTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.bots.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.bots.TryGetValue(key, out StallBotTimer timer))
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu đã hết thời gian
                    if (timer.IsOver)
                    {
                        /// Nếu danh sách chưa tồn tại
                        if (totalRemoveTimers == null)
                        {
                            /// Tạo mới
                            totalRemoveTimers = new List<int>();
                        }

                        /// Thêm vào danh sách hủy
                        totalRemoveTimers.Add(key);
                        /// Thực hiện hủy đối tượng
                        timer.RefObject.Destroy();
                    }
                }


                /// Nếu có đánh dấu phải xóa luồng quái
                if (this.forceClearAllTimers)
                {
                    this.bots.Clear();
                }
                else
                {
                    /// Duyệt danh sách cần xóa
                    if (totalRemoveTimers != null)
                    {
                        foreach (int key in totalRemoveTimers)
                        {
                            this.bots.TryRemove(key, out _);
                        }
                        totalRemoveTimers.Clear();
                        totalRemoveTimers = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
