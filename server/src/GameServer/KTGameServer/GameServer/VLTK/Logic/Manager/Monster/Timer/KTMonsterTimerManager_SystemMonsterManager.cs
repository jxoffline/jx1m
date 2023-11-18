using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý sự kiện tái sinh quái trong hệ thống
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Core
        /// <summary>
        /// Worker quản lý
        /// </summary>
        private BackgroundWorker monsterManagerWorker;

        /// <summary>
        /// Chạy luồng quản lý sự kiện tái sinh quái trong hệ thống
        /// </summary>
        private void StartSystemMonsterManagerTimer()
        {
            /// Khởi tạo worker
            this.monsterManagerWorker = new BackgroundWorker();
            this.monsterManagerWorker.DoWork += this.ProcessSystemMonsterManager;
            this.monsterManagerWorker.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer monsterManagerTimer = new System.Timers.Timer();
            monsterManagerTimer.AutoReset = true;
            monsterManagerTimer.Interval = 100;
            monsterManagerTimer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTMonsterTimerMain"));
                    /// Ngừng luồng
                    monsterManagerTimer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTMonsterTimerMain");
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.monsterManagerWorker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 5000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTMonsterTimerMain"));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    this.monsterManagerWorker.RunWorkerAsync();
                }
            };
            monsterManagerTimer.Start();
        }

        /// <summary>
        /// Thực thi quản lý quái của hệ thống
        /// </summary>
        private void ProcessSystemMonsterManager(object sender, DoWorkEventArgs e)
        {
            try
            {
                KTMonsterManager.Tick();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
