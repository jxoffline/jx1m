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
    /// Quản lý luồng Bot biểu diễn
    /// </summary>
    public partial class KTDecoBotTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, DecoBotTimer> bots = new ConcurrentDictionary<int, DecoBotTimer>();

        /// <summary>
        /// Danh sách quái cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker[] worker;

        /// <summary>
        /// Chạy luồng quản lý quái thường
        /// </summary>
        private void StartTimer()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxBotTimer];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxBotTimer];

            /// Khởi tạo Worker
            this.worker = new BackgroundWorker[ServerConfig.Instance.MaxBotTimer];
            for (int i = 0; i < this.worker.Length; i++)
            {
                this.worker[i] = new BackgroundWorker();
                this.worker[i].DoWork += this.DoBackgroundWork;
                this.worker[i].RunWorkerCompleted += this.Worker_Completed;
                lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                lastWorkerBusyTick[i] = 0;
            }

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 500;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTDecoBotTimerManager"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.worker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTDecoBotTimerManager, total monster timers = " + this.bots.Count);
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.worker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTDecoBotTimerManager, total monster timers = " + this.bots.Count));
                        }
                    }
                    else
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi công việc
                        if (!this.worker[i].IsBusy)
                        {
                            this.worker[i].RunWorkerAsync(i);
                        }
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
                /// ID luồng
                int workerID = (int) e.Argument;

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
                            DecoBotTimer newTimer = (DecoBotTimer) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.bots.TryGetValue(newTimer.RefObject.RoleID, out DecoBotTimer timer))
                            {
                                ///// Thực hiện Reset đối tượng
                                //timer.RefObject.Reset();
                                /// Xóa Timer cũ
                                this.bots.TryRemove(timer.RefObject.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.bots[newTimer.RefObject.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            KDecoBot bot = (KDecoBot) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.bots.TryGetValue(bot.RoleID, out DecoBotTimer timer))
                            {
                                /// Thực hiện Reset đối tượng
                                timer.RefObject.Reset();
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
                    if (!this.bots.TryGetValue(key, out DecoBotTimer timer))
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu Null
                    if (timer == null || timer.RefObject == null || timer.RefObject.IsDead())
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu không phải công việc của Worker này
                    if (timer.RefObject.RoleID % ServerConfig.Instance.MaxBotTimer != workerID)
                    {
                        continue;
                    }

                    /// Nếu chưa Start
                    if (!timer.IsStarted)
                    {
                        this.ExecuteAction(timer.Start, null);
                        /// Đánh dấu đã thực hiện Start
						timer.IsStarted = true;
                    }

                    /// Nếu đã đến thời điểm Tick
                    if (timer.IsTickTime && timer.HasCompletedLastTick)
                    {
                        /// Thời điểm Tick hiện tại
                        long currentTick = KTGlobal.GetCurrentTimeMilis();
                        timer.LastTick = currentTick;

                        /// Đánh dấu chưa hoàn thành Tick lần trước
                        timer.HasCompletedLastTick = false;
                        /// Thực thi sự kiện
                        this.ExecuteAction(timer.Tick, null);
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
