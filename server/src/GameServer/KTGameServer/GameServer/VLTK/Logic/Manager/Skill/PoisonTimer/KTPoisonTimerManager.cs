using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer
{
	/// <summary>
	/// Quản lý Timer trúng độc
	/// </summary>
	public class KTPoisonTimerManager
	{
		#region Singleton - Instance
		/// <summary>
		/// Quản lý Timer trúng độc
		/// </summary>
		public static KTPoisonTimerManager Instance { get; private set; }
		#endregion

		#region Define
		/// <summary>
		/// Phần tử chờ thao tác trong hàng đợi
		/// </summary>
		private class QueueItem
		{
			/// <summary>
			/// Loại thao tác
			/// </summary>
			public int Type { get; set; }

			/// <summary>
			/// Dữ liệu
			/// </summary>
			public object Data { get; set; }
		}

        /// <summary>
        /// Đối tượng dùng nội bộ
        /// </summary>
        private class InnerObject
		{
            /// <summary>
            /// Đối tượng tương ứng
            /// </summary>
            public GameObject Owner { get; set; }

            /// <summary>
            /// ID đối tượng
            /// </summary>
            public int RoleID { get; set; }

            /// <summary>
            /// Thời điểm Tick lần trước
            /// </summary>
            public long LastTick { get; set; }

            /// <summary>
            /// Thời gian sống
            /// </summary>
            public int LifeTime { get; set; }

            /// <summary>
            /// Đã đến thời điểm Tick chưa
            /// </summary>
            public bool IsInTime
			{
				get
				{
                    return KTGlobal.GetCurrentTimeMilis() - this.LastTick >= 500;
				}
			}
		}

		/// <summary>
		/// Danh sách chờ thêm vào
		/// </summary>
		private ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();

		/// <summary>
		/// Danh sách đối tượng chịu ảnh hưởng của sát thương độc
		/// </summary>
		private Dictionary<int, InnerObject> affectedObjects = new Dictionary<int, InnerObject>();
		#endregion

		/// <summary>
		/// Khởi tạo
		/// </summary>
		public static void Init()
		{
			KTPoisonTimerManager.Instance = new KTPoisonTimerManager();
		}

        #region Core
        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Quản lý Schedule Task
        /// </summary>
        public KTPoisonTimerManager()
        {
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
            timer.Interval = 100;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTPoisonTimer, total timers = " + this.affectedObjects.Count));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                //Console.WriteLine(string.Format("Tick alive => {0}", "KTScheduleTaskManager"));

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, "Tick alive => KTPoisonTimer");
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => KTPoisonTimer, total timers = " + this.affectedObjects.Count));
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
                /// Duyệt danh sách chờ
                while (!this.waitingQueue.IsEmpty)
                {
                    if (this.waitingQueue.TryDequeue(out QueueItem item))
                    {
                        /// Nếu đối tượng không tồn tại
                        if (item.Data == null)
                        {
                            continue;
                        }

                        if (item.Type == 1)
                        {
                            InnerObject go = (InnerObject) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.affectedObjects.ContainsKey(go.RoleID))
                            {
                                /// Xóa Timer cũ
                                this.affectedObjects.Remove(go.RoleID);
                            }
                            /// Thêm vào danh sách
                            this.affectedObjects[go.RoleID] = go;
                        }
                        else if (item.Type == 0)
                        {
                            GameObject go = (GameObject) item.Data;
                            /// Nếu Timer cũ tồn tại
                            if (this.affectedObjects.ContainsKey(go.RoleID))
                            {
                                /// Xóa Timer cũ
                                this.affectedObjects.Remove(go.RoleID);
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                List<int> toRemoveObjs = null;
                /// Duyệt danh sách
                foreach (InnerObject obj in this.affectedObjects.Values)
                {
                    /// Nếu đối tượng đã chết
                    if (obj.Owner == null || obj.Owner.IsDead())
					{
                        /// Thêm vào danh sách cần xóa
                        if (toRemoveObjs == null)
						{
                            toRemoveObjs = new List<int>();
                        }
                        toRemoveObjs.Add(obj.RoleID);
                        continue;
					}

                    /// Nếu đã đến thời điểm Tick
                    if (obj.IsInTime)
                    {
                        /// Tăng thời gian sống
                        obj.LifeTime += 500;
                        /// Đánh dấu thời điểm Tick trước
                        obj.LastTick = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi sự kiện DoPoisonState
                        this.ExecuteAction(obj.Owner.DoPoisonState);
                    }
                }

                /// Nếu tồn tại danh sách cần xóa
                if (toRemoveObjs != null)
				{
                    /// Duyệt danh sách
                    foreach (int roleID in toRemoveObjs)
					{
                        /// Xóa khỏi danh sách
                        this.affectedObjects.Remove(roleID);
					}
				}

                //if (this.lastCount != this.affectedObjects.Count)
                //{
                //    this.lastCount = this.affectedObjects.Count;
                //    Console.WriteLine("Total Poison timers = " + this.lastCount);
                //}
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Tổng số Timer trước
        /// </summary>
        private int lastCount = 0;

        /// <summary>
        /// Thực thi sự kiện gì đó
        /// </summary>
        /// <param name="work"></param>
        private bool ExecuteAction(Action work)
        {
            try
            {
                work?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Thêm trạng thái trúng độc cho đối tượng
        /// </summary>
        /// <param name="go"></param>
        /// <param name="duration"></param>
        public void AddPoisonState(GameObject go)
		{
            /// Toác
            if (go == null)
			{
                return;
			}

            this.waitingQueue.Enqueue(new QueueItem()
            {
                Data = new InnerObject()
                {
                    Owner = go,
                    RoleID = go.RoleID,
                },
                Type = 1,
            });
		}

        /// <summary>
        /// Xóa trạng thái trúng độc của đối tượng
        /// </summary>
        /// <param name="go"></param>
        public void RemovePoisonState(GameObject go)
		{
            /// Toác
            if (go == null)
			{
                return;
			}

            this.waitingQueue.Enqueue(new QueueItem()
            {
                Data = go,
                Type = 0,
            });
		}
    }
}
