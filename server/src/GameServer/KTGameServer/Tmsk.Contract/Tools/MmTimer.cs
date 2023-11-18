using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading
{
    public sealed class MmTimer : IComponent, IDisposable
    {
        // Fields
        private static MmTimerCaps caps;
        private int interval;
        private bool isRunning;
        private MmTimerMode mode;
        private int resolution;
        private TimeProc timeProcOneShot;
        private TimeProc timeProcPeriodic;
        private int timerID;

        // Events
        public event EventHandler Disposed;

        public event EventHandler Tick;

        // Methods
        static MmTimer()
        {
            timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
        }

        public MmTimer()
        {
            this.interval = caps.periodMin;
            this.resolution = caps.periodMin;
            this.mode = MmTimerMode.Periodic;
            this.isRunning = false;
            this.timeProcPeriodic = new TimeProc(this.TimerPeriodicEventCallback);
            this.timeProcOneShot = new TimeProc(this.TimerOneShotEventCallback);
        }

        public MmTimer(IContainer container)
            : this()
        {
            container.Add(this);
        }

        public void Dispose()
        {
            timeKillEvent(this.timerID);
            GC.SuppressFinalize(this);
            EventHandler disposed = this.Disposed;
            if (disposed != null)
            {
                disposed(this, EventArgs.Empty);
            }
        }

        ~MmTimer()
        {
            timeKillEvent(this.timerID);
        }

        private void OnTick(EventArgs e)
        {
            EventHandler tick = this.Tick;
            if (tick != null)
            {
                tick(this, e);
            }
        }

        public void Start()
        {
            if (!this.isRunning)
            {
                if (this.Mode == MmTimerMode.Periodic)
                {
                    this.timerID = timeSetEvent(this.interval, this.resolution, this.timeProcPeriodic, 0, (int)this.Mode);
                }
                else
                {
                    this.timerID = timeSetEvent(this.interval, this.resolution, this.timeProcOneShot, 0, (int)this.Mode);
                }
                if (this.timerID == 0)
                {
                    throw new Exception("Unable to start MmTimer");
                }
                this.isRunning = true;
            }
        }

        public void Stop()
        {
            if (this.isRunning)
            {
                timeKillEvent(this.timerID);
                this.isRunning = false;
            }
        }

        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref MmTimerCaps caps, int sizeOfTimerCaps);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);
        private void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
        {
            this.OnTick(EventArgs.Empty);
            this.Stop();
        }

        private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
        {
            this.OnTick(EventArgs.Empty);
        }

        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

        // Properties
        public int Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                if ((value < caps.periodMin) || (value > caps.periodMax))
                {
                    throw new Exception("invalid period");
                }
                this.interval = value;
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public MmTimerMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;
            }
        }

        public ISite Site { get; set; }

        // Nested Types
        private delegate void TimeProc(int id, int msg, int user, int param1, int param2);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MmTimerCaps
    {
        public int periodMin;
        public int periodMax;
    }

    public enum MmTimerMode
    {
        OneShot,
        Periodic
    }

}
