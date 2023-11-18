using GameServer.KiemThe.Logic;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Utilities
{
	/// <summary>
	/// Đối tượng thực thi công việc theo lịch trình
	/// </summary>
	public class ScheduleTask
	{
		/// <summary>
		/// ID tự tăng
		/// </summary>
		private static long AutoID { get; set; }

		/// <summary>
		/// Đối tượng thực thi công việc theo lịch trình
		/// </summary>
		public ScheduleTask()
		{
			ScheduleTask.AutoID = (ScheduleTask.AutoID + 1);
			this.ID = ScheduleTask.AutoID;
		}

		/// <summary>
		/// ID đối tượng
		/// </summary>
		public long ID { get; private set; }

		/// <summary>
		/// Thời gian tồn tại
		/// </summary>
		public long LifeTime { get; private set; }

		/// <summary>
		/// Thời gian Tick liên tục
		/// </summary>
		public int PeriodTick { get; set; }

		/// <summary>
		/// Thời gian tối đa
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Công việc cần thực thi
		/// </summary>
		public Action Work { get; set; }

		/// <summary>
		/// Sự kiện khi hoàn tất công việc
		/// </summary>
		public Action Finish { get; set; }

		/// <summary>
		/// Thời điểm Tick khởi tạo (áp dụng cho loại Task chạy một lần)
		/// </summary>
		private long initTicks;

		/// <summary>
		/// Bắt đầu Timer
		/// </summary>
		public void Start()
		{
			/// Thêm vào luồng quản lý
			KTScheduleTaskManager.Instance.Add(this);
			/// Đánh dấu thời điểm khởi tạo
			this.initTicks = KTGlobal.GetCurrentTimeMilis();
		}

		/// <summary>
		/// Kết thúc Timer
		/// </summary>
		public void Stop()
		{
			/// Xóa khỏi luồng quản lý
			KTScheduleTaskManager.Instance.Remove(this);
		}

		/// <summary>
		/// Sự kiện Tick
		/// </summary>
		/// <param name="o"></param>
		/// <param name="e"></param>
		public void Tick()
		{
			/// Nếu tồn tại trên 30s thì tự hủy
			if (KTGlobal.GetCurrentTimeMilis() - this.initTicks >= 30000)
            {
				/// Kết thúc Timer
				this.Stop();
				/// Bỏ qua
				return;
			}

			/// Nếu là Task chạy một lần
			if (this.PeriodTick == -1)
			{
				/// Nếu thời gian tồn tại đã vượt quá
				if (KTGlobal.GetCurrentTimeMilis() - this.initTicks >= this.Duration)
				{
					/// Thực thi sự kiện
					this.ExecuteAction(this.Work);
					/// Kết thúc Timer
					this.Stop();
					/// Thực thi sự kiện hoàn tất
					this.ExecuteAction(this.Finish);
				}
			}
			/// Nếu là Task chạy liên tục
			else
			{
				/// Nếu tồn tại vĩnh viễn
				if (this.Duration == -1)
				{
					/// Thực thi sự kiện
					this.ExecuteAction(this.Work);
				}
				/// Nếu có thời gian tồn tại
				else
				{
					/// Tăng thời gian đã tồn tại
					this.LifeTime += this.PeriodTick;
					/// Nếu thời gian tồn tại đã vượt quá
					if (this.LifeTime > this.Duration)
					{
						/// Kết thúc Timer
						this.Stop();
						/// Thực thi sự kiện hoàn tất
						this.ExecuteAction(this.Finish);
						/// Thoát
						return;
					}
					/// Thực thi sự kiện
					this.ExecuteAction(this.Work);
				}
			}
		}

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
				//	LogManager.WriteLog(LogTypes.Info, string.Format("Tick {0} => Total ticks = {1}(ms)", "ScheduleTask", totalProcessTicks));
				//}

				return true;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at {0}\n{1}", "ScheduleTask", ex.ToString()));
				return false;
			}
		}
	}
}
