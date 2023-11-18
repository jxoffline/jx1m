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
    /// Quản lý quái tùy chọn không bị xóa khi mất người
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Define
        /// <summary>
        /// Danh sách Timer đang thực thi cho NPC di động
        /// </summary>
        private readonly ConcurrentDictionary<int, MonsterTimer> dynamicNPCs = new ConcurrentDictionary<int, MonsterTimer>();

        /// <summary>
        /// Danh sách NPC di động cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueueDynamicNPC = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker cho NPC di động
        /// </summary>
        private BackgroundWorker DynamicNPCWorker;

        /// <summary>
        /// Khởi tạo luồng quản lý quái tùy chọn
        /// </summary>
        private void StartDynamicNPCTimer()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick;
            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick;

            /// Khởi tạo Worker
            this.DynamicNPCWorker = new BackgroundWorker();
            this.DynamicNPCWorker.DoWork += this.DoDynamicNPCBackgroundWork;
            this.DynamicNPCWorker.RunWorkerCompleted += this.Worker_Completed;
            lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
            lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 500;
            timer.Elapsed += (o, e) =>
            {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTMonsterTimer_DynamicNPC"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTMonsterTimer_DynamicNPC, total monster timers = " + this.dynamicNPCs.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.DynamicNPCWorker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTMonsterTimer_DynamicNPC, total monster timers = " + this.dynamicNPCs.Count));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    if (!this.DynamicNPCWorker.IsBusy)
                    {
                        this.DynamicNPCWorker.RunWorkerAsync();
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoDynamicNPCBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Duyệt danh sách chờ
                while (!this.waitingQueueDynamicNPC.IsEmpty)
                {
                    if (this.waitingQueueDynamicNPC.TryDequeue(out QueueItem item))
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
                            if (this.dynamicNPCs.TryGetValue(newTimer.Owner.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.dynamicNPCs.TryRemove(newTimer.Owner.RoleID, out _);
                            }

                            /// Thêm vào danh sách
                            this.dynamicNPCs[newTimer.Owner.RoleID] = newTimer;
                        }
                        else if (item.Type == 0)
                        {
                            Monster monster = (Monster) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.dynamicNPCs.TryGetValue(monster.RoleID, out MonsterTimer monsterTimer))
                            {
                                /// Thực hiện Reset đối tượng
                                monsterTimer.Owner.Reset();
                                /// Xóa Timer cũ
                                this.dynamicNPCs.TryRemove(monster.RoleID, out _);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> toRemoveMonsterTimers = null;
                /// Duyệt danh sách
                List<int> keys = this.dynamicNPCs.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.dynamicNPCs.TryGetValue(key, out MonsterTimer monsterTimer))
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


                /// Nếu có đánh dấu phải xóa luồng quái
                if (this.forceClearAllMonsterTimers)
                {
                    this.dynamicNPCs.Clear();
                }
                else
                {
                    /// Duyệt danh sách cần xóa
                    if (toRemoveMonsterTimers != null)
                    {
                        foreach (int key in toRemoveMonsterTimers)
                        {
                            this.dynamicNPCs.TryRemove(key, out _);
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
