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
    public partial class KTMonsterTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, MonsterTimer> monsters = new ConcurrentDictionary<int, MonsterTimer>();

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
        private void StartNormalMonsterTimer()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxMonsterTimer];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxMonsterTimer];

            /// Khởi tạo Worker
            this.worker = new BackgroundWorker[ServerConfig.Instance.MaxMonsterTimer];
            for (int i = 0; i < this.worker.Length; i++)
            {
                this.worker[i] = new BackgroundWorker();
                this.worker[i].DoWork += this.DoNormalMonsterBackgroundWork;
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
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTMonsterTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.worker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTMonsterTimer, total monster timers = " + this.monsters.Count);
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.worker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTMonsterTimer, total monster timers = " + this.monsters.Count));
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
		private void DoNormalMonsterBackgroundWork(object sender, DoWorkEventArgs e)
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
                            MonsterTimer newTimer = (MonsterTimer) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.monsters.TryGetValue(newTimer.Owner.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.monsters.TryRemove(newTimer.Owner.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.monsters[newTimer.Owner.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            Monster monster = (Monster) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.monsters.TryGetValue(monster.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.monsters.TryRemove(monster.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> toRemoveMonsterTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.monsters.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.monsters.TryGetValue(key, out MonsterTimer monsterTimer))
                    {
                        if (toRemoveMonsterTimers == null)
                        {
                            toRemoveMonsterTimers = new List<int>();
                        }
                        toRemoveMonsterTimers.Add(key);
                        continue;
                    }

                    /// Nếu Null
                    if (monsterTimer == null || monsterTimer.Owner == null || monsterTimer.Owner.IsDead())
                    {
                        if (toRemoveMonsterTimers == null)
                        {
                            toRemoveMonsterTimers = new List<int>();
                        }
                        toRemoveMonsterTimers.Add(key);
                        continue;
                    }

                    /// Nếu không phải công việc của Worker này
                    if (monsterTimer.Owner.RoleID % ServerConfig.Instance.MaxMonsterTimer != workerID)
                    {
                        continue;
                    }

                    /// Nếu chưa Start
                    if (!monsterTimer.IsStarted)
                    {
                        this.ExecuteAction(monsterTimer.Start, null);
                        /// Đánh dấu đã thực hiện Start
						monsterTimer.IsStarted = true;
                    }

                    /// Nếu đã đến thời điểm Tick
                    if (monsterTimer.IsTickTime && monsterTimer.HasCompletedLastTick)
                    {
                        /// Thời điểm Tick hiện tại
                        long currentTick = KTGlobal.GetCurrentTimeMilis();
                        monsterTimer.LastTick = currentTick;

                        /// Đánh dấu chưa hoàn thành Tick lần trước
                        monsterTimer.HasCompletedLastTick = false;
                        /// Thực thi sự kiện
                        this.ExecuteAction(monsterTimer.Tick, null);
                    }
                }


                /// Nếu có đánh dấu phải xóa luồng quái
                if (this.forceClearAllMonsterTimers)
                {
                    this.monsters.Clear();
                }
                else
                {
                    /// Duyệt danh sách cần xóa
                    if (toRemoveMonsterTimers != null)
                    {
                        foreach (int key in toRemoveMonsterTimers)
                        {
                            this.monsters.TryRemove(key, out _);
                        }
                        toRemoveMonsterTimers.Clear();
                        toRemoveMonsterTimers = null;
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
