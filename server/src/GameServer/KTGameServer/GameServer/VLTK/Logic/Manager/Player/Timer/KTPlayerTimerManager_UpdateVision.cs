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
        /// Worker cập nhật vị trí
        /// </summary>
        private BackgroundWorker[] updateGridWorker;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer_UpdateVision()
        {
            /// Thời gian báo cáo trạng thái lần cuối
            long[] lastReportStateTick = new long[ServerConfig.Instance.MaxUpdateGridThread];
            /// Thời gian thực thi công việc lần cuối
            long[] lastWorkerBusyTick = new long[ServerConfig.Instance.MaxUpdateGridThread];
            /// Khởi tạo worker
            this.updateGridWorker = new BackgroundWorker[ServerConfig.Instance.MaxUpdateGridThread];
            for (int i = 0; i < this.updateGridWorker.Length; i++)
            {
                this.updateGridWorker[i] = new BackgroundWorker();
                this.updateGridWorker[i].DoWork += this.DoUpdateGridBackgroundWork;
                this.updateGridWorker[i].RunWorkerCompleted += this.Worker_Completed;
                /// Thời gian báo cáo trạng thái lần cuối
                lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                /// Thời gian thực thi công việc lần cuối
                lastWorkerBusyTick[i] = 0;
            }

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 200;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "PlayerUpdateGridTimer"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                for (int i = 0; i < this.updateGridWorker.Length; i++)
                {
                    if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick[i] >= 5000)
                    {
                        LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => PlayerUpdateGridTimer");
                        lastReportStateTick[i] = KTGlobal.GetCurrentTimeMilis();
                    }

                    if (this.updateGridWorker[i].IsBusy)
                    {
                        /// Quá thời gian thì Cancel
                        if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick[i] >= 2000)
                        {
                            /// Cập nhật thời gian thực thi công việc lần cuối
                            lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                            LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => PlayerUpdateGridTimer"));
                        }
                    }
                    else
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick[i] = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi công việc
                        if (!this.updateGridWorker[i].IsBusy)
                        {
                            this.updateGridWorker[i].RunWorkerAsync(i);
                        }
                    }
                }
            };
            timer.Start();
        }

        /// <summary>
		/// Thực hiện công việc cập nhật lưới
		/// </summary>
		private void DoUpdateGridBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// ID luồng
                int workerID = (int) e.Argument;

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
                    if (player.StaticID % ServerConfig.Instance.MaxUpdateGridThread != workerID)
                    {
                        continue;
                    }

                    /// Nếu đang Logout
                    if (player.ClosingClientStep > 0)
                    {
                        continue;
                    }

                    try
                    {
                        /// Cập nhật tầm nhìn
                        KTRadarMapManager.UpdateVision(player);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
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
