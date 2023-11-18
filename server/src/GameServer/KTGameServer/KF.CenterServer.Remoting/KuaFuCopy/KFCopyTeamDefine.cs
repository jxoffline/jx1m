using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Remoting
{
    /// <summary>
    /// 跨服副本的数量控制
    /// </summary>
    public class KFTeamCountControl
    {
        /*
        // 默认的每个副本同时存在的最大数量
        public int DefaultMaxCount = 100;

        // 特殊指定的副本的同时最大数量 key： 副本id  value：上限
        public Dictionary<int, int> SpecialCountDict = new Dictionary<int, int>();
        */
        // 副本创建完成之后，最大等待时间，超时还不进入，强制清除队伍
        public int TeamMaxWaitMinutes = 10;
    }

    /// <summary>
    /// 统计信息
    /// </summary>
    public class KFCopyTeamAnalysis
    {
        public class Item
        {
            public int TotalCopyCount;
            public int StartCopyCount;
            public int UnStartCopyCount;
            public int TotalRoleCount;
            public int StartRoleCount;
            public int UnStartRoleCount;
        }

        // key: 副本id  value: 副本信息
        public Dictionary<int, Item> AnalysisDict = new Dictionary<int, Item>();
    }
}
