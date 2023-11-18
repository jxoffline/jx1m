using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Timer
    /// </summary>
    public partial class KTCaptchaManager
    {
        #region Define
        /// <summary>
        /// Danh sách cần thêm vào
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();
        #endregion

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker workers;

        /// <summary>
        /// Chạy luồng quản lý
        /// </summary>
        private void StartTimer()
        {
            /// Khởi tạo worker
            this.workers = new BackgroundWorker();
            this.workers.DoWork += this.DoBackgroundWork;
            this.workers.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 500;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTCaptchaManager"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTCaptchaManager, total queue items = " + this.waitingQueue.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.workers.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, "Timeout => KTCaptchaManager, total queue items = " + this.waitingQueue.Count);
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    if (!this.workers.IsBusy)
                    {
                        /// Thực thi công việc
                        this.workers.RunWorkerAsync();
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
                /// Danh sách thực thi
                List<KPlayer> captchaPlayers = null;

                /// Duyệt danh sách chờ
                while (!this.waitingQueue.IsEmpty)
                {
                    if (this.waitingQueue.TryDequeue(out QueueItem item))
                    {
                        /// Nếu đối tượng không tồn tại
                        if (item.Player == null)
                        {
                            continue;
                        }

                        /// Nếu không tồn tại danh sách
                        if (captchaPlayers == null)
                        {
                            /// Tạo mới
                            captchaPlayers = new List<KPlayer>();
                        }
                        /// Thêm vào danh sách
                        captchaPlayers.Add(item.Player);
                    }
                }

                /// Nếu không tồn tại danh sách cần thực thi
                if (captchaPlayers == null)
                {
                    return;
                }

                /// Duyệt danh sách
                foreach (KPlayer player in captchaPlayers)
                {
                    /// Thực hiện
                    try
                    {
                        this.DoGenerate(player);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }
                }

                /// Làm rỗng danh sách
                captchaPlayers.Clear();
                captchaPlayers = null;
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
        #endregion

        #region API
        /// <summary>
        /// Tạo Captcha cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public void Generate(KPlayer player)
        {
            /// Thêm vào danh sách chờ
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Player = player,
            });
        }
        #endregion
    }
}
