using System;

namespace FS.GameEngine.TimerManager
{
    /// <summary>
    /// Sự kiện Timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate void DispatcherTimerEventHandler(object sender, EventArgs args);

    /// <summary>
    /// Timer
    /// </summary>
    public class DispatcherTimer : IDisposable
	{
		/// <summary>
        /// Sự kiện Tick
        /// </summary>
		public DispatcherTimerEventHandler Tick { get; set; } = null;

        /// <summary>
        /// Tên Timer
        /// </summary>
		private string _Name = "Undefined";
        /// <summary>
        /// Đã bắt đầu chưa
        /// </summary>
        private bool _Started = false;
        /// <summary>
        /// Thời điểm Tick lần trước
        /// </summary>
        private long _LastTicks = 0;

        /// <summary>
        /// Timer
        /// </summary>
        /// <param name="name"></param>
        public DispatcherTimer(string name)
		{
			this._Name = name;
            DispatcherTimerDriver.AddTimer(this);
		}
		
        /// <summary>
        /// Tên Timer
        /// </summary>
		public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }
		
        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
		private TimeSpan _Interval = TimeSpan.Zero;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
		public TimeSpan Interval
        {
            get
            {
                return this._Interval;
            }
            set
            {
                this._Interval = value;
            }
        }
		
        /// <summary>
        /// Bắt đầu Timer
        /// </summary>
		public void Start()
		{
            this._Started = true;
            this._LastTicks = DateTime.Now.Ticks;
		}
		
        /// <summary>
        /// Ngừng Timer
        /// </summary>
		public void Stop()
		{
            this._Started = false;
		}

        /// <summary>
        /// Hủy Timer
        /// </summary>
        public void Dispose()
        {
            DispatcherTimerDriver.RemoveTimer(this);
        }

        /// <summary>
        /// Thực thi Timer
        /// </summary>
        public void ExecuteTimer()
        {
            long ticks = DateTime.Now.Ticks;
            if (ticks - this._LastTicks < this._Interval.Ticks)
            {
                return;
            }

            this._LastTicks = ticks;

            if (null != Tick)
            {
                long startTicks = DateTime.Now.Ticks / 10000;

                Tick(this, EventArgs.Empty);

                long elapsedTicks = (DateTime.Now.Ticks / 10000) - startTicks;
                if (elapsedTicks >= 100)
                {
                    //KTDebug.Log("DispatcherTimer.ExecuteTimer, Name=" + _Name + ", Used ticks=" + elapsedTicks);
                }
            }
        }
	}
}
