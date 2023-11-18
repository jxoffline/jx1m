using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public class KTBuffManager
    {
        #region Singleton - Instance

        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTBuffManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTBuffManager() : base() { }
        #endregion Singleton - Instance

        #region Define
        /// <summary>
        /// Thông tin yêu cầu
        /// </summary>
        private class QueueItem
		{
            /// <summary>
            /// Loại yêu cầu
            /// <para>1: Thêm, 0: Xóa</para>
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// Dữ liệu
            /// </summary>
            public object Data { get; set; }
		}

        /// <summary>
        /// Luồng quản lý Buff
        /// </summary>
        private class BuffTimer
        {
            /// <summary>
            /// Chủ nhân
            /// </summary>
            public GameObject Owner { get; set; }

            /// <summary>
            /// Dữ liệu Buff
            /// </summary>
            public BuffDataEx Buff { get; set; }

            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public Action Tick { get; set; }

            /// <summary>
            /// Sự kiện hết thời gian
            /// </summary>
            public Action Finish { get; set; }

            /// <summary>
            /// Thời điểm bắt đầu
            /// </summary>
            public long InitTicks { get; set; }

            /// <summary>
            /// Thời điểm Tick lần trước
            /// </summary>
            public long LastTick { get; set; }

            /// <summary>
            /// Thời gian Tick liên tục
            /// </summary>
            public int PeriodTicks { get; set; }

            /// <summary>
            /// Thời gian tồn tại
            /// </summary>
            public long LifeTime { get; set; }

            /// <summary>
            /// Đã đến thời gian thực hiện Tick
            /// </summary>
            /// <returns></returns>
            public bool TimeToProcessTick
			{
				get
				{
                    return KTGlobal.GetCurrentTimeMilis() - this.LastTick >= this.PeriodTicks;
                }
            }

            /// <summary>
            /// Đã hết thời gian chưa
            /// </summary>
            public bool IsOver
			{
				get
				{
                    if (this.LifeTime == -1)
					{
                        return false;
					}
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.LifeTime;
                }
			}

            /// <summary>
            /// Detach mà không gọi hàm Finish
            /// </summary>
            public bool DetachWithoutCallingFinishFunction { get; set; } = false;
        }
        #endregion

        #region Initialize

        /// <summary>
        /// Hàm này gọi đến khởi tạo đối tượng
        /// </summary>
        public static void Init()
        {
            KTBuffManager.Instance = new KTBuffManager();
            KTBuffManager.Instance.StartTimer();
        }

        #endregion Initialize

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Đối tượng Semaphore
        /// </summary>
        private Semaphore limitation;

        /// <summary>
        /// Biến đánh dấu buộc xóa luồng thực thi Buff ngay lập tức
        /// </summary>
        private bool forceClearBuffTimers = false;

        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer()
        {
            /// Nếu có sử dụng đa luồng
            if (ServerConfig.Instance.MaxBuffTimer > 1)
            {
                this.limitation = new Semaphore(ServerConfig.Instance.MaxBuffTimer, ServerConfig.Instance.MaxBuffTimer);
            }

            /// Khởi tạo worker
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.DoBackgroundWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;

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
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTBuffManager, total timers = " + this.buffs.Count));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTBuffManager, total timers = " + this.buffs.Count);
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTBuffManager, total timers = " + this.buffs.Count));
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
                LogManager.WriteLog(LogTypes.Exception, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực hiện công việc
		/// </summary>
		private void DoBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Chừng nào vẫn còn yêu cầu
                while (!this.waitingQueue.IsEmpty)
				{
                    if (this.waitingQueue.TryDequeue(out QueueItem item))
					{
                        /// Nếu là yêu cầu thêm
                        if (item.Type == 1)
						{
                            /// Thêm
                            this.buffs.Add((BuffTimer) item.Data);
                        }
                        /// Nếu là yêu cầu xóa
						else if (item.Type == 0)
						{
                            KeyValuePair<GameObject, BuffDataEx> pair = (KeyValuePair<GameObject, BuffDataEx>) item.Data;
                            /// Buff cũ
                            List<BuffTimer> buffTimers = this.buffs.Where(x => x.Owner == pair.Key && x.Buff == pair.Value).ToList();
                            /// Duyệt danh sách Buff cũ
                            foreach (BuffTimer buffTimer in buffTimers)
							{
                                /// Nếu tồn tại Buff cũ
                                if (buffTimer != null)
                                {
                                    /// Xóa
                                    this.buffs.Remove(buffTimer);
                                }
                            }
                        }
					}
                }

                /// Làm rỗng danh sách Buff cần xóa
                this.toRemoveBuffs.Clear();

                /// Duyệt danh sách Buff
                foreach (BuffTimer buffTimer in this.buffs)
                {
                    /// Nếu đã hết thời gian
                    if (buffTimer.IsOver)
					{
                        /// Nếu chưa chạy pha Detach
                        if (!buffTimer.Buff.IsDetached)
						{
                            /// Thực hiện hàm Finish
                            this.ExecuteAction(buffTimer.Finish, null);
                        }
                        /// Xóa Buff
                        this.toRemoveBuffs.Add(buffTimer);
                        continue;
                    }

                    /// Nếu đã đến thời gian Tick
                    if (buffTimer.TimeToProcessTick)
					{
                        /// Thời điểm Tick hiện tại
                        long currentTick = KTGlobal.GetCurrentTimeMilis();
                        buffTimer.LastTick = currentTick;

                        /// Thực hiện hàm Tick
                        this.ExecuteAction(buffTimer.Tick, null);
                    }

                    /// Nếu không có chủ nhân
                    if (buffTimer.Owner == null)
					{
                        /// Xóa Buff
                        this.toRemoveBuffs.Add(buffTimer);
                        continue;
                    }
                }

                /// Nếu có đánh dấu buộc xóa toàn bộ luồng thực thi Buff
                if (this.forceClearBuffTimers)
				{
                    this.buffs.Clear();
				}
				else
				{
                    /// Nếu tồn tại danh sách Buff cần xóa
                    if (this.toRemoveBuffs.Count > 0)
                    {
                        /// Duyệt danh sách cần xóa
                        foreach (BuffTimer buffTimer in this.toRemoveBuffs)
                        {
                            /// Xóa Buff
                            this.buffs.Remove(buffTimer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
		/// Thực thi sự kiện gì đó
		/// </summary>
		/// <param name="work"></param>
		/// <param name="onError"></param>
		private void ExecuteAction(Action work, Action<Exception> onError)
        {
            /// Nếu không sử dụng đa luồng
            if (ServerConfig.Instance.MaxBuffTimer <= 1)
            {
                try
                {
                    work?.Invoke();
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    onError?.Invoke(ex);
                }
            }
            /// Nếu sử dụng đa luồng
            else
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
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                        onError?.Invoke(ex);
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
        }

        /// <summary>
        /// Xóa toàn bộ luồng thực thi Buff
        /// </summary>
        public void ClearAllBuffTimers()
		{
            this.forceClearBuffTimers = true;
		}
        #endregion

        /// <summary>
        /// Danh sách yêu cầu đang chờ thực thi
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        private readonly List<BuffTimer> buffs = new List<BuffTimer>();

        /// <summary>
        /// Danh sách Buff cần xóa
        /// </summary>
        private readonly List<BuffTimer> toRemoveBuffs = new List<BuffTimer>();

        /// <summary>
        /// Thêm Buff vào danh sách
        /// </summary>
        /// <param name="go"></param>
        /// <param name="buff"></param>
        public void AddBuff(GameObject go, BuffDataEx buff)
        {
            if (go == null || buff == null)
			{
                return;
			}

            BuffTimer buffTimer = new BuffTimer()
            {
                Owner = go,
                Buff = buff,
                InitTicks = buff.StartTick,
                LifeTime = buff.Duration,
                PeriodTicks = -1,
                LastTick = 0,
                Tick = null,
                Finish = () => {
                    go.Buffs.RemoveBuff(buff);
                },
            };

            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 1,
                Data = buffTimer,
            });
        }

        /// <summary>
        /// Thêm Buff vào danh sách
        /// </summary>
        /// <param name="go"></param>
        /// <param name="buff"></param>
        /// <param name="tick"></param>
        /// <param name="fucntion"></param>
        public void AddBuff(GameObject go, BuffDataEx buff, int tick, Action fucntion)
        {
            if (go == null || buff == null)
            {
                return;
            }

            BuffTimer buffTimer = new BuffTimer()
            {
                Owner = go,
                Buff = buff,
                InitTicks = buff.StartTick,
                LifeTime = buff.Duration,
                PeriodTicks = tick,
                LastTick = 0,
                Tick = fucntion,
                Finish = () => {
                    go.Buffs.RemoveBuff(buff);
                },
            };

            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 1,
                Data = buffTimer,
            });
        }

        /// <summary>
        /// Xóa Buff khỏi danh sách
        /// </summary>
        /// <param name="player"></param>
        public void RemoveBuff(GameObject go, BuffDataEx buff)
        {
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 0,
                Data = new KeyValuePair<GameObject, BuffDataEx>(go, buff),
            });
        }
    }
}