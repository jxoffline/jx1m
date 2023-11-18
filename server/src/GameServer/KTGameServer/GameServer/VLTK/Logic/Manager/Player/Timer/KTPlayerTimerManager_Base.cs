using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTPlayerTimerManager
    {
        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker[] baseWorker;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer_Base()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxPlayerStoryBoardThread];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxPlayerStoryBoardThread];
            /// Khởi tạo worker
            this.baseWorker = new BackgroundWorker[ServerConfig.Instance.MaxPlayerStoryBoardThread];
            for (int i = 0; i < this.baseWorker.Length; i++)
            {
                this.baseWorker[i] = new BackgroundWorker();
                this.baseWorker[i].DoWork += this.DoBaseBackgroundWork;
                this.baseWorker[i].RunWorkerCompleted += this.Worker_Completed;
                /// Thời gian báo cáo trạng thái lần cuối
                lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                /// Thời gian thực thi công việc lần cuối
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
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "PlayerTimerBase"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.baseWorker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => PlayerTimerBase, total timers: " + this.players.Count);
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.baseWorker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => PlayerTimerBase, total timers:" + this.players.Count));
                        }
                    }
                    else
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi công việc
                        if (!this.baseWorker[i].IsBusy)
                        {
                            this.baseWorker[i].RunWorkerAsync(i);
                        }
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoBaseBackgroundWork(object sender, DoWorkEventArgs e)
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
                        if (item.Type == 1)
                        {
                            /// Thêm
                            this.players[item.Player.RoleID] = item.Player;
                        }
                        else if (item.Type == 0)
                        {
                            /// Xóa
                            this.players.TryRemove(item.Player.RoleID, out _);
                        }
                    }
                }

                /// Duyệt danh sách người chơi
                List<int> keys = this.players.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu người chơi không tồn tại
                    if (!this.players.TryGetValue(key, out KPlayer player))
                    {
                        continue;
                    }

                    /// Nếu đây không phải việc của Worker này
                    if (player.StaticID % ServerConfig.Instance.MaxPlayerStoryBoardThread != workerID)
                    {
                        continue;
                    }

                    /// Nếu đang Logout
                    if (player.ClosingClientStep > 0)
                    {
                        continue;
                    }

                    /// Thực thi sự kiện Tick
                    this.ExecuteAction(player.Tick, null);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
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
        /// Thực thi sự kiện tương ứng
        /// </summary>
        /// <param name="work"></param>
        /// <param name="onException"></param>
        private void ExecuteAction(Action work, Action<Exception> onException)
        {
            try
            {
                /// Thực thi sự kiện
                work?.Invoke();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
