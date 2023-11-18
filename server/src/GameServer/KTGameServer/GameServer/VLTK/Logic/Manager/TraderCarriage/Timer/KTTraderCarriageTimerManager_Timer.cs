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
    /// Quản lý quái thường
    /// </summary>
    public partial class KTTraderCarriageTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, TraderCarriageTimer> carriages = new ConcurrentDictionary<int, TraderCarriageTimer>();

        /// <summary>
        /// Danh sách quái cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker cho quái
        /// </summary>
        private BackgroundWorker[] worker;

        /// <summary>
        /// Chạy luồng quản lý quái thường
        /// </summary>
        private void StartTimer()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxTraderCarriageTimer];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxTraderCarriageTimer];

            /// Khởi tạo Worker
            this.worker = new BackgroundWorker[ServerConfig.Instance.MaxTraderCarriageTimer];
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
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTTraderCarriageTimerManager"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.worker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTTraderCarriageTimerManager, total monster timers = " + this.carriages.Count);
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.worker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTTraderCarriageTimerManager, total monster timers = " + this.carriages.Count));
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
                            TraderCarriageTimer newTimer = (TraderCarriageTimer) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.carriages.TryGetValue(newTimer.RefObject.RoleID, out TraderCarriageTimer carriageTimer))
                            {
                                ///// Thực hiện Reset đối tượng
                                //carriageTimer.RefObject.Reset();
                                /// Xóa Timer cũ
                                this.carriages.TryRemove(carriageTimer.RefObject.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.carriages[newTimer.RefObject.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            TraderCarriage carriage = (TraderCarriage) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.carriages.TryGetValue(carriage.RoleID, out TraderCarriageTimer carriageTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                carriageTimer.RefObject.Reset();
                                /// Xóa Timer cũ
                                this.carriages.TryRemove(carriage.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> totalRemoveTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.carriages.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.carriages.TryGetValue(key, out TraderCarriageTimer carriageTimer))
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu Null
                    if (carriageTimer == null || carriageTimer.RefObject == null || carriageTimer.RefObject.Owner == null || carriageTimer.RefObject.Owner.IsDead() || !carriageTimer.RefObject.Owner.IsOnline())
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu không phải công việc của Worker này
                    if (carriageTimer.RefObject.RoleID % ServerConfig.Instance.MaxTraderCarriageTimer != workerID)
                    {
                        continue;
                    }

                    /// Nếu chưa Start
                    if (!carriageTimer.IsStarted)
                    {
                        this.ExecuteAction(carriageTimer.Start, null);
                        /// Đánh dấu đã thực hiện Start
						carriageTimer.IsStarted = true;
                    }

                    /// Nếu đã đến thời điểm Tick
                    if (carriageTimer.IsTickTime && carriageTimer.HasCompletedLastTick)
                    {
                        /// Thời điểm Tick hiện tại
                        long currentTick = KTGlobal.GetCurrentTimeMilis();
                        carriageTimer.LastTick = currentTick;

                        /// Đánh dấu chưa hoàn thành Tick lần trước
                        carriageTimer.HasCompletedLastTick = false;
                        /// Thực thi sự kiện
                        this.ExecuteAction(carriageTimer.Tick, null);
                    }

                    /// Nếu đã quá thời gian tồn tại
                    if (carriageTimer.IsTimeout)
                    {
                        /// Thực thi sự kiện
                        this.ExecuteAction(carriageTimer.Timeout, null);
                    }
                }

                /// Nếu có đánh dấu phải xóa luồng quái
                if (this.forceClearAllTimers)
                {
                    this.carriages.Clear();
                }
                else
                {
                    /// Duyệt danh sách cần xóa
                    if (totalRemoveTimers != null)
                    {
                        foreach (int key in totalRemoveTimers)
                        {
                            this.carriages.TryRemove(key, out _);
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
