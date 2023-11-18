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
    public partial class KTPetTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, PetTimer> pets = new ConcurrentDictionary<int, PetTimer>();

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
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxPetTimer];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxPetTimer];

            /// Khởi tạo Worker
            this.worker = new BackgroundWorker[ServerConfig.Instance.MaxPetTimer];
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
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTPetTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.worker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTPetTimer, total monster timers = " + this.pets.Count);
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.worker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTPetTimer, total monster timers = " + this.pets.Count));
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
                            PetTimer newTimer = (PetTimer) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.pets.TryGetValue(newTimer.RefObject.RoleID, out PetTimer petTimer))
                            {
                                ///// Thực hiện Reset đối tượng
                                //petTimer.RefObject.Reset();
                                /// Xóa Timer cũ
                                this.pets.TryRemove(petTimer.RefObject.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.pets[newTimer.RefObject.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            Pet pet = (Pet) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.pets.TryGetValue(pet.RoleID, out PetTimer petTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                petTimer.RefObject.Destroy(true);
                                /// Xóa Timer cũ
                                this.pets.TryRemove(pet.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> totalRemoveTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.pets.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.pets.TryGetValue(key, out PetTimer petTimer))
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu Null
                    if (petTimer == null || petTimer.RefObject == null || petTimer.RefObject.Owner == null || petTimer.RefObject.Owner.IsDead() || !petTimer.RefObject.Owner.IsOnline())
                    {
                        if (totalRemoveTimers == null)
                        {
                            totalRemoveTimers = new List<int>();
                        }
                        totalRemoveTimers.Add(key);
                        continue;
                    }

                    /// Nếu không phải công việc của Worker này
                    if (petTimer.RefObject.RoleID % ServerConfig.Instance.MaxPetTimer != workerID)
                    {
                        continue;
                    }

                    /// Nếu chưa Start
                    if (!petTimer.IsStarted)
                    {
                        this.ExecuteAction(petTimer.Start, null);
                        /// Đánh dấu đã thực hiện Start
						petTimer.IsStarted = true;
                    }

                    /// Nếu đã đến thời điểm Tick
                    if (petTimer.IsTickTime && petTimer.HasCompletedLastTick)
                    {
                        /// Thời điểm Tick hiện tại
                        long currentTick = KTGlobal.GetCurrentTimeMilis();
                        petTimer.LastTick = currentTick;

                        /// Đánh dấu chưa hoàn thành Tick lần trước
                        petTimer.HasCompletedLastTick = false;
                        /// Thực thi sự kiện
                        this.ExecuteAction(petTimer.Tick, null);
                    }
                }


                /// Nếu có đánh dấu phải xóa luồng quái
                if (this.forceClearAllTimers)
                {
                    this.pets.Clear();
                }
                else
                {
                    /// Duyệt danh sách cần xóa
                    if (totalRemoveTimers != null)
                    {
                        foreach (int key in totalRemoveTimers)
                        {
                            this.pets.TryRemove(key, out _);
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
