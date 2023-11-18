using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;

namespace GameServer.SafeData
{
    /// <summary>
    /// TaskData 的线程安全类
    /// </summary>
    public class SafeTaskData
    {
        public SafeTaskData(TaskData taskData)
        {
            _TaskData = taskData;
        }

        /// <summary>
        /// 私有数据
        /// </summary>
        private TaskData _TaskData = null;

        /// <summary>
        /// 数据库ID
        /// </summary>
        public int DbID
        {
            get { lock (this) { return _TaskData.DbID; } }
            set { lock (this) { _TaskData.DbID = value; } }
        }

        /// <summary>
        /// 已经接受的任务列表
        /// </summary>
        public int DoingTaskID
        {
            get { lock (this) { return _TaskData.DoingTaskID; } }
            set { lock (this) { _TaskData.DoingTaskID = value; } }
        }

        /// <summary>
        /// 已经接受的任务数值列表1
        /// </summary>
        public int DoingTaskVal1
        {
            get { lock (this) { return _TaskData.DoingTaskVal1; } }
            set { lock (this) { _TaskData.DoingTaskVal1 = value; } }
        }

        /// <summary>
        /// 已经接受的任务数值列表2
        /// </summary>
        public int DoingTaskVal2
        {
            get { lock (this) { return _TaskData.DoingTaskVal2; } }
            set { lock (this) { _TaskData.DoingTaskVal2 = value; } }
        }

        /// <summary>
        /// 已经接受的任务追踪列表
        /// </summary>
        public int DoingTaskFocus
        {
            get { lock (this) { return _TaskData.DoingTaskFocus; } }
            set { lock (this) { _TaskData.DoingTaskFocus = value; } }
        }

        /// <summary>
        /// 任务添加的时间(单位秒)
        /// </summary>
        public long AddDateTime
        {
            get { lock (this) { return _TaskData.AddDateTime; } }
            set { lock (this) { _TaskData.AddDateTime = value; } }
        }
    }
}
