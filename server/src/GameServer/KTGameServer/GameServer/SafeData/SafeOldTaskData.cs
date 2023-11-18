using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Data;

namespace GameServer.SafeData
{
    /// <summary>
    /// OldTaskData 线程安全类
    /// </summary>
    public class SafeOldTaskData
    {
        public SafeOldTaskData(OldTaskData oldTaskData)
        {
            _OldTaskData = oldTaskData;
        }

        /// <summary>
        /// 私有数据
        /// </summary>
        private OldTaskData _OldTaskData = null;

        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskID
        {
            get { lock (this) { return _OldTaskData.TaskID; } }
            set { lock (this) { _OldTaskData.TaskID = value; } }
        }

        /// <summary>
        /// 做过的数量
        /// </summary>
        public int DoCount
        {
            get { lock (this) { return _OldTaskData.TaskID; } }
            set { lock (this) { _OldTaskData.TaskID = value; } }
        }
    }
}
