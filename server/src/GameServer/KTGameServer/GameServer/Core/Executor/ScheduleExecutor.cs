using Server.Tools;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GameServer.Core.Executor
{
    public class TaskInternalLock
    {
        private int _LockCount;
        private bool LogRunTime;

        public bool TryEnter()
        {
            if (Interlocked.CompareExchange(ref _LockCount, 1, 0) != 0)
            {
                LogRunTime = true;
                return false;
            }
            return true;
        }

        public bool Leave()
        {
            bool logRunTime = LogRunTime;
            Interlocked.CompareExchange(ref _LockCount, 0, 1);
            LogRunTime = false;
            return logRunTime;
        }
    }

    public interface ScheduleTask
    {
        TaskInternalLock InternalLock { get; }

        void run();
    }


    internal class TaskWrapper : IComparer<TaskWrapper>
    {

        private ScheduleTask currentTask;


        private long startTime = -1;


        private long periodic = -1;


        private int executeCount = 0;

        public bool canExecute = true;

        public TaskWrapper(ScheduleTask task, long delay, long periodic)
        {
            this.currentTask = task;
            this.startTime = TimeUtil.NOW() + delay;
            this.periodic = periodic;
        }

        public ScheduleTask CurrentTask
        {
            get { return currentTask; }
        }

        public long StartTime
        {
            get { return startTime; }
        }


        public void resetStartTime()
        {
            this.startTime = this.startTime + this.periodic;
        }

        public long Periodic
        {
            get { return periodic; }
        }

        public void release()
        {
            currentTask = null;
        }

        public void addExecuteCount()
        {
            executeCount++;
        }

        public int ExecuteCount
        {
            get { return this.executeCount; }
        }

        public int Compare(TaskWrapper x, TaskWrapper y)
        {
            long ret = x.startTime - y.startTime;
            if (ret == 0)
            {
                return 0;
            }
            else if (ret > 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public interface PeriodicTaskHandle
    {
        void cannel();

        bool isCanneled();

        int getExecuteCount();

        long getPeriodic();
    }

    internal class PeriodicTaskHandleImpl : PeriodicTaskHandle
    {
        private TaskWrapper taskWrapper;
        private ScheduleExecutor executor;
        private bool canneled = false;

        public PeriodicTaskHandleImpl(TaskWrapper taskWrapper, ScheduleExecutor executor)
        {
            this.taskWrapper = taskWrapper;
            this.executor = executor;
        }

        public void cannel()
        {
            if (canneled)
                return;

            if (null != executor && null != taskWrapper)
            {
                executor.removeTask(taskWrapper);
                executor = null;
            }

            canneled = true;
        }

        public bool isCanneled()
        {
            return canneled;
        }

        public int getExecuteCount()
        {
            return taskWrapper.ExecuteCount;
        }

        public long getPeriodic()
        {
            return taskWrapper.Periodic;
        }
    }


    internal class Worker
    {
        private ScheduleExecutor executor = null;

        private Thread currentThread = null;
        private static int nThreadCount = 0;
        private int nThreadOrder = 0;


        private static Object _lock = new Object();

        public Worker(ScheduleExecutor executor)
        {
            this.executor = executor;
        }

        public Thread CurrentThread
        {
            set { this.currentThread = value; }
        }

        private TaskWrapper getCanExecuteTask(long ticks)
        {
            TaskWrapper taskWrapper = executor.GetPreiodictTask(ticks);

            if (null != taskWrapper)
            {
                return taskWrapper;
            }

            int getNum = 0;
            int nMaxProcCount = 200;
            int nTaskCount = executor.GetTaskCount();

            if (nTaskCount == 0)
            {
                return null;
            }
            else if (nTaskCount < nMaxProcCount)
            {
                nMaxProcCount = nTaskCount;
            }

            while (null != (taskWrapper = executor.getTask()))
            {

                if (ticks < taskWrapper.StartTime)
                {
                    if (taskWrapper.canExecute)
                        executor.addTask(taskWrapper);
                    getNum++;


                    if (getNum >= nMaxProcCount)
                    {
                        break;
                    }
                    continue;
                }

                return taskWrapper;
            }

            return null;
        }


        public void work()
        {
            lock (_lock)
            {
                nThreadCount++;
                nThreadOrder = nThreadCount;
            }

            TaskWrapper taskWrapper = null;

            int lastTickCount = int.MinValue;
            while (!Program.NeedExitServer)
            {

                int tickCount = Environment.TickCount;
                if (tickCount <= lastTickCount + 5)
                {
                    if (lastTickCount <= 0 || tickCount >= 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }
                }
                lastTickCount = tickCount;

                long ticks = TimeUtil.NOW();
                while (true)
                {
                    try
                    {
                        taskWrapper = getCanExecuteTask(ticks);
                        if (null == taskWrapper || null == taskWrapper.CurrentTask)
                            break;

                        if (taskWrapper.canExecute)
                        {
                            try
                            {
                                taskWrapper.CurrentTask.run();
                            }
                            catch (System.Exception ex)
                            {
                                DataHelper.WriteFormatExceptionLog(ex, "异步调度任务执行异常", false);
                            }
                        }


                        if (taskWrapper.Periodic > 0 && taskWrapper.canExecute)
                        {

                            taskWrapper.resetStartTime();
                            executor.addTask(taskWrapper);
                            taskWrapper.addExecuteCount();
                        }
                    }
                    catch (System.Exception/* ex*/)
                    {

                    }
                }
            }

            SysConOut.WriteLine(string.Format("ScheduleTask Worker{0}退出...", nThreadOrder));
        }
    }

    public class ScheduleExecutor
    {
        private List<Worker> workerQueue = null;
        private List<Thread> threadQueue = null;
        private LinkedList<TaskWrapper> TaskQueue = null;
        private List<TaskWrapper> PreiodictTaskList = new List<TaskWrapper>();

        private int maxThreadNum = 0;

        /// <summary>
        /// Max threading
        /// </summary>
        /// <param name="maxThreadNum"></param>
        public ScheduleExecutor(int maxThreadNum)
        {
            this.maxThreadNum = maxThreadNum;
            threadQueue = new List<Thread>();
            workerQueue = new List<Worker>();
            TaskQueue = new LinkedList<TaskWrapper>();
            for (int i = 0; i < maxThreadNum; i++)
            {
                Worker worker = new Worker(this);
                Thread thread = new Thread(new ThreadStart(worker.work));
                worker.CurrentThread = thread;
                workerQueue.Add(worker);
                threadQueue.Add(thread);
            }
        }

        public void start()
        {
            lock (threadQueue)
            {
                foreach (Thread thread in threadQueue)
                {
                    thread.Start();
                }
            }
        }

        public void stop()
        {
            lock (threadQueue)
            {
                foreach (Thread thread in threadQueue)
                {
                    thread.Abort();
                }

                threadQueue.Clear();
            }

            lock (workerQueue)
            {
                workerQueue.Clear();
            }
        }


        public bool execute(ScheduleTask task)
        {
            TaskWrapper wrapper = new TaskWrapper(task, -1, -1);

            addTask(wrapper);

            return true;
        }

        public PeriodicTaskHandle scheduleExecute(ScheduleTask task, long delay)
        {
            return scheduleExecute(task, delay, -1);
        }


        public PeriodicTaskHandle scheduleExecute(ScheduleTask task, long delay, long periodic)
        {
            TaskWrapper wrapper = new TaskWrapper(task, delay, periodic);

            PeriodicTaskHandle handle = new PeriodicTaskHandleImpl(wrapper, this);

            addTask(wrapper);

            return handle;
        }

        internal TaskWrapper GetPreiodictTask(long ticks)
        {
            lock (PreiodictTaskList)
            {
                if (PreiodictTaskList.Count == 0)
                {
                    return null;
                }
                else if (PreiodictTaskList[0].StartTime > ticks)
                {
                    return null;
                }

                TaskWrapper taskWrapper = PreiodictTaskList[0];
                PreiodictTaskList.RemoveAt(0);
                return taskWrapper;
            }
        }

        internal TaskWrapper getTask()
        {
            lock (TaskQueue)

            {
                try
                {
                    if (TaskQueue.Count <= 0)
                    {
                        return null;
                    }
                    else
                    {
                        TaskWrapper currentTask = TaskQueue.First.Value;
                        TaskQueue.RemoveFirst();
                        if (currentTask.canExecute)
                        {
                            return currentTask;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (System.Exception/* ex*/)
                {
                }

            }


            return null;
        }

        internal int GetTaskCount()
        {
            lock (TaskQueue)
            {
                return TaskQueue.Count;
            }
        }

        internal void addTask(TaskWrapper taskWrapper)
        {
            if (taskWrapper.Periodic > 0)
            {
                lock (PreiodictTaskList)
                {
                    ListExt.BinaryInsertAsc(PreiodictTaskList, taskWrapper, taskWrapper);
                }
            }
            else
            {
                lock (TaskQueue)
                {
                    TaskQueue.AddLast(taskWrapper);
                    taskWrapper.canExecute = true;
                }
            }
        }

        internal void removeTask(TaskWrapper taskWrapper)
        {
            lock (TaskQueue)
            {
                TaskQueue.Remove(taskWrapper);
                taskWrapper.canExecute = false;
            }
        }
    }
}