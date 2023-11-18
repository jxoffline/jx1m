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
    /// Quản lý Boss
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi cho Boss
        /// </summary>
        private readonly ConcurrentDictionary<int, MonsterTimer> bosses = new ConcurrentDictionary<int, MonsterTimer>();

        /// <summary>
        /// Danh sách Boss cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueueForBoss = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker cho Boss
        /// </summary>
        private BackgroundWorker workerForBoss;

        /// <summary>
        /// Chạy luồng quản lý Boss
        /// </summary>
        private void StartNormalBossTimer()
        {
            /// Khởi tạo worker cho Boss riêng
            this.workerForBoss = new BackgroundWorker();
            this.workerForBoss.DoWork += this.DoBackgroundWorkForBoss;
            this.workerForBoss.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timerForBoss = new System.Timers.Timer();
            timerForBoss.AutoReset = true;
            timerForBoss.Interval = 500;
            timerForBoss.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTMonsterTimerForBoss"));
                    /// Ngừng luồng
                    timerForBoss.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTMonsterTimerForBoss, total monster timers = " + this.bosses.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.workerForBoss.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, "Timeout => KTMonsterTimerForBoss, total monster timers = " + this.bosses.Count);
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    if (!this.workerForBoss.IsBusy)
                    {
                        /// Thực thi công việc
                        this.workerForBoss.RunWorkerAsync();
                    }
                }
            };
            timerForBoss.Start();
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoBackgroundWorkForBoss(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Duyệt danh sách chờ
                while (!this.waitingQueueForBoss.IsEmpty)
                {
                    if (this.waitingQueueForBoss.TryDequeue(out QueueItem item))
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
                            if (this.bosses.TryGetValue(newTimer.Owner.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.bosses.TryRemove(newTimer.Owner.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.bosses[newTimer.Owner.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            Monster monster = (Monster) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.bosses.TryGetValue(monster.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.bosses.TryRemove(monster.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> toRemoveMonsterTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.bosses.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.bosses.TryGetValue(key, out MonsterTimer monsterTimer))
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

                /// Duyệt danh sách cần xóa
                if (toRemoveMonsterTimers != null)
                {
                    foreach (int key in toRemoveMonsterTimers)
                    {
                        this.bosses.TryRemove(key, out _);
                    }
                    toRemoveMonsterTimers.Clear();
                    toRemoveMonsterTimers = null;
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
