using Server.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameServer.KiemThe.GameEvents.SpecialEvent
{
    /// <summary>
    /// Quản lý Timer sự kiện đặc biệt
    /// </summary>
    public partial class SpecialEventManager
    {
        #region Core
        /// <summary>
        /// Worker thực thi công việc
        /// </summary>
        private BackgroundWorker worker = null;

        /// <summary>
        /// Bắt đầu Timer
        /// </summary>
        private void StartTimer()
        {
            /// Khởi tạo Worker
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.Worker_DoWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "SpecialEventManager"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => SpecialEventManager");
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, "Timeout => SpecialEventManager");
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    if (!this.worker.IsBusy)
                    {
                        /// Thực thi công việc
                        this.worker.RunWorkerAsync();
                    }
                }
            };
            timer.Start();
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
        /// Thực hiện công việc của Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Gọi hàm xử lý sự kiện
                this.Timer_Tick();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
