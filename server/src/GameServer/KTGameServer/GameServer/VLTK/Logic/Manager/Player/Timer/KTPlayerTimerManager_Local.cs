using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi nhân vật
    /// </summary>
    public partial class KTPlayerTimerManager
    {
        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker localWorker;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer_Local()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = 0;
            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;
            /// Khởi tạo worker
            this.localWorker = new BackgroundWorker();
            this.localWorker.DoWork += this.DoLocalBackgroundWork;
            this.localWorker.RunWorkerCompleted += this.Worker_Completed;
            /// Thời gian báo cáo trạng thái lần cuối
            lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
            /// Thời gian thực thi công việc lần cuối
            lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 500;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "PlayerTimerLocal"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => PlayerTimerLocal, total timers: " + this.players.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.localWorker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => PlayerTimerLocal, total timers:" + this.players.Count));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    if (!this.localWorker.IsBusy)
                    {
                        this.localWorker.RunWorkerAsync();
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoLocalBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Duyệt danh sách người chơi
                List<int> keys = this.players.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu người chơi không tồn tại
                    if (!this.players.TryGetValue(key, out KPlayer player))
                    {
                        continue;
                    }

                    /// Nếu đang Logout
                    if (player.ClosingClientStep > 0)
                    {
                        continue;
                    }

                    /// Thực thi sự kiện Tick
                    this.ExecuteAction(() =>
                    {
                        KTPlayerManager.BackgroundWork.DoLocalUpdate(player);
                    }, null);
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
