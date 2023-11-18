using GameServer.Core.Executor;
using GameServer.KiemThe.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Timers;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Lớp quản lý các luồng thực thi
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KTTimerManager<T> where T : KTTimer
    {
        /// <summary>
        /// Inner Timer
        /// </summary>
        private class KTTimerInner
        {
            /// <summary>
            /// ID tự tăng
            /// </summary>
            private static int AutoID = 0;

            /// <summary>
            /// Inner Timer
            /// </summary>
            public KTTimerInner()
            {
                KTTimerInner.AutoID = (KTTimerInner.AutoID + 1) % 100000007;
                this.ID = KTTimerInner.AutoID;
            }

            /// <summary>
            /// ID Timer
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Luồng thực thi
            /// </summary>
            public T Timer { get; set; }

            /// <summary>
            /// Thời gian bắt đầu
            /// </summary>
            public long StartTick { get; set; }

            /// <summary>
            /// Thời gian tick liên tục hiện tại (nếu PeriodActivation của Timer = -1 thì không có tác dụng)
            /// </summary>
            public long NowPeriodTick { get; set; }

            /// <summary>
            /// Đã gọi hàm bắt đầu chưa
            /// </summary>
            public bool IsStarted { get; set; }

            /// <summary>
            /// Đã kết thúc chưa
            /// </summary>
            public bool IsFinished { get; set; } = false;
        }

        /// <summary>
        /// Queue chứa yêu cầu đợi thực thi
        /// </summary>
        private ConcurrentQueue<KTTimerInner> listWaitingRequest = new ConcurrentQueue<KTTimerInner>();

        /// <summary>
        /// Danh sách Timer
        /// </summary>
        private readonly List<KTTimerInner> timers = new List<KTTimerInner>();

        /// <summary>
        /// Danh sách Timer cần xóa
        /// </summary>
        private readonly List<KTTimerInner> toDeleteTimers = new List<KTTimerInner>();

        /// <summary>
        /// Thời gian nghỉ giữa mỗi lần thực thi
        /// </summary>
        protected abstract int PeriodTick { get; }

        /// <summary>
        /// Thời gian Tick lần trước
        /// </summary>
        private long lastTick;

        /// <summary>
        /// Sử dụng đa luồng
        /// </summary>
        protected virtual bool UseMultiThreading { get; private set; } = false;

        /// <summary>
        /// Số luồng sinh ra đồng thời cùng lúc
        /// <para>Chỉ hoạt động khi sử dụng đa luồng</para>
        /// </summary>
        protected virtual int MaxThreadsEach { get; private set; } = 1;

        /// <summary>
        /// Hàm khởi tạo mặc định
        /// </summary>
        protected KTTimerManager(bool useMultipleThreading = false, int maxThreadsEach = 1)
        {
            this.UseMultiThreading = useMultipleThreading;
            this.MaxThreadsEach = maxThreadsEach;
            this.StartTimer();
        }

        #region Private methods
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Limit số luồng thực thi
        /// </summary>
        private Semaphore limitation;

        /// <summary>
        /// Chạy luồng thực thi
        /// </summary>
        private void StartTimer()
        {
            /// Nếu sử dụng đa luồng
            if (this.UseMultiThreading)
			{
                /// Tạo mới đối tượng Semaphore
                this.limitation = new Semaphore(this.MaxThreadsEach, this.MaxThreadsEach);
			}

            /// Khởi tạo worker
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.Worker_DoWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = this.PeriodTick;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
				{
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", typeof(T).Name));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
				}

                //Console.WriteLine(string.Format("Tick alive => {0}", typeof(T).Name));

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Tick alive => {0}", typeof(T).Name));
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 10000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => {0}", typeof(T).Name));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    this.worker.RunWorkerAsync();
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
                LogManager.WriteLog(LogTypes.Exception, string.Format("KTTimerManager<{0}>.this.Worker_DoWork", typeof(T).Name));
            }
        }

        /// <summary>
        /// Thực thi công việc của Background Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            long nowTick = KTGlobal.GetCurrentTimeMilis();

            try
			{
                /// Duyệt toàn bộ các yêu cầu đang chơ trong Queue và xử lý
                while (!this.listWaitingRequest.IsEmpty)
                {
                    if (this.listWaitingRequest.TryDequeue(out KTTimerInner timer))
                    {
                        this.timers.Add(timer);
                    }
                }

                /// Duyệt toàn bộ danh sách Timer trong luồng
                foreach (KTTimerInner timer in this.timers)
                {
                    /// Nếu đã chết
                    if (!timer.Timer.Alive)
                    {
                        /// Nếu quá thời gian 
                        if (timer.Timer.Interval != -1 && nowTick - timer.StartTick >= timer.Timer.Interval * 1000)
                        {
                            /// Nếu chưa thực thi sự kiện Finish
                            if (!timer.IsFinished)
                            {
                                /// Nếu sử dụng đa luồng
                                if (this.UseMultiThreading)
                                {
                                    /// Thực hiện hàm Finish
                                    this.ExecuteActionWithMultipleThreading(timer.Timer.Finish, null);
                                }
                                else
                                {
                                    /// Thực hiện hàm Finish
                                    this.ExecuteAction(timer.Timer.Finish);
                                }
                            }
                        }

                        /// Nếu sử dụng đa luồng
                        if (this.UseMultiThreading)
						{
                            /// Thực hiện hàm Destroy
                            this.ExecuteActionWithMultipleThreading(timer.Timer.Destroy, null);
                        }
						else
						{
                            /// Thực thi hàm Destroy
                            this.ExecuteAction(timer.Timer.Destroy);
                        }

                        /// Thêm vào danh sách cần xóa
                        this.toDeleteTimers.Add(timer);
                        continue;
                    }

                    /// Nếu chưa thực hiện hàm Start thì thực hiện
                    if (!timer.IsStarted)
                    {
                        /// Nếu sử dụng đa luồng
                        if (this.UseMultiThreading)
                        {
                            /// Thực hiện hàm Start
                            this.ExecuteActionWithMultipleThreading(timer.Timer.Start, () => {
                                timer.Timer.Alive = false;
                            });
                        }
                        else
                        {
                            /// Thực thi hàm Start
                            bool ret = this.ExecuteAction(timer.Timer.Start);
                            /// Nếu bị lỗi
                            if (!ret)
                            {
                                timer.Timer.Alive = false;
                            }
                        }

                        /// Đánh dấu đã thực hiện hàm Start
                        timer.IsStarted = true;
                        continue;
                    }

                    /// Thực hiện hàm Tick
                    if (timer.Timer.PeriodActivation != -1)
                    {
                        timer.NowPeriodTick += nowTick - this.lastTick;

                        /// Nếu đã đến thời gian Tick
                        if (timer.NowPeriodTick >= timer.Timer.PeriodActivation * 1000)
                        {
                            /// Reset thời điểm Tick lần trước
                            timer.NowPeriodTick = 0;
                            /// Nếu sử dụng đa luồng
                            if (this.UseMultiThreading)
                            {
                                /// Thực hiện hàm Tick
                                this.ExecuteActionWithMultipleThreading(timer.Timer.Tick, () => {
                                    timer.Timer.Alive = false;
                                });
                            }
                            else
                            {
                                /// Thực thi hàm Tick
                                bool ret = this.ExecuteAction(timer.Timer.Tick);
                                /// Nếu bị lỗi
                                if (!ret)
                                {
                                    timer.Timer.Alive = false;
                                    continue;
                                }
                            }
                        }
                    }

                    /// Nếu quá thời gian
                    if (timer.Timer.Interval != -1 && nowTick - timer.StartTick >= timer.Timer.Interval * 1000)
                    {
                        /// Đánh dấu đã thực thi sự kiện Finish
                        timer.IsFinished = true;
                        /// Nếu sử dụng đa luồng
                        if (this.UseMultiThreading)
                        {
                            /// Thực hiện hàm Finish
                            this.ExecuteActionWithMultipleThreading(timer.Timer.Finish, null);
                        }
                        else
                        {
                            /// Thực thi hàm Finish
                            bool ret = this.ExecuteAction(timer.Timer.Finish);
                            /// Nếu bị lỗi
                            if (!ret)
                            {
                                timer.Timer.Alive = false;
                            }
                        }
                        /// Đánh dấu đã chết
                        timer.Timer.Alive = false;
                    }
                }

                /// Duyệt danh sách cần xóa
                foreach (KTTimerInner timer in this.toDeleteTimers)
                {
                    /// Xóa danh sách chờ
                    this.timers.Remove(timer);
				}

                /// Cập nhật thời điểm Tick cuối
                this.lastTick = KTGlobal.GetCurrentTimeMilis();

				//if (this.lastCount != this.timers.Count)
				//{
				//	this.lastCount = this.timers.Count;
				//	Console.WriteLine("Total " + typeof(T).Name + " timers = " + this.lastCount);
				//}
			}
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at {0}\n{1}", typeof(T).Name, ex.ToString()));
            }
        }

        /// <summary>
        /// Tổng số Timer trước
        /// </summary>
        private int lastCount = 0;

        /// <summary>
        /// Thực thi sự kiện
        /// </summary>
        /// <param name="work"></param>
        private bool ExecuteAction(Action work)
		{
			try
			{
                ///// Thời điểm Tick hiện tại
                //long currentTick = KTGlobal.GetCurrentTimeMilis();

                /// Thực thi công việc
                work?.Invoke();

                ///// Tổng thời gian thực thi
                //long totalProcessTicks = KTGlobal.GetCurrentTimeMilis() - currentTick;
                ///// Nếu quá 1s
                //if (totalProcessTicks >= 500)
                //{
                //    LogManager.WriteLog(LogTypes.Info, string.Format("Tick {0} => Total ticks = {1}(ms)", typeof(T).Name, totalProcessTicks));
                //}

                return true;
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at {0}\n{1}", typeof(T).Name, ex.ToString()));
                return false;
            }
		}

        /// <summary>
        /// Thực thi sự kiện ở kiến trúc đa luồng
        /// </summary>
        /// <param name="work"></param>
        /// <param name="OnError"></param>
        /// <returns></returns>
        private void ExecuteActionWithMultipleThreading(Action work, Action OnError)
		{
            /// Tạo luồng mới
            Thread thread = new Thread(() => {
                try
                {
                    /// Đợi đến khi có luồng Free
                    this.limitation.WaitOne();
                    /// Thực thi công việc
                    work?.Invoke();
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at {0}\n{1}", typeof(T).Name, ex.ToString()));
                    OnError?.Invoke();
                }
				finally
				{
                    this.limitation.Release();
				}
            });
            /// Hủy chế độ chạy ngầm
            thread.IsBackground = false;
            /// Bắt đầu luồng
            thread.Start();
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Thêm luồng thực thi thời gian vào danh sách
        /// </summary>
        /// <param name="timer"></param>
        public void AddTimer(T timer)
        {
            this.listWaitingRequest.Enqueue(new KTTimerInner()
            {
                Timer = timer,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
                NowPeriodTick = 0,
                IsStarted = false,
            });
        }

        /// <summary>
        /// Xóa luồng thực thi thời gian khỏi danh sách, nếu không tìm thấy thì không xử lý
        /// </summary>
        /// <param name="timer"></param>
        public void KillTimer(T timer)
        {
            timer.Alive = false;
        }
        #endregion Public methods
    }
}