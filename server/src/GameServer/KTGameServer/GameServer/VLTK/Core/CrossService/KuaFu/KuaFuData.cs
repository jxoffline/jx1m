using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Server.TCP;
using Server.Protocol;
using GameServer;
using Server.Data;
using ProtoBuf;
using System.Threading;
using GameServer.Server;
using Server.Tools;
using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using System.Xml.Linq;
using GameServer.Core.GameEvent.EventOjectImpl;
using KF.Contract.Data;

namespace GameServer.Logic
{
    [Serializable]
    public class KuaFuRoleAndServerInfo
    {
        public KuaFuRoleData RoleData;
    }


    /// <summary>
    /// 配置信息和运行时数据
    /// </summary>
    public class KuaFuDataData
    {
        /// <summary>
        /// 保证数据完整性,敏感数据操作需加锁
        /// </summary>
        public object Mutex = new object();

        /// <summary>
        /// 后台线程对象
        /// </summary>
        public Thread BackGroundThread = null;

        /// <summary>
        /// 幻影寺院服务地址
        /// </summary>
        public string HuanYingSiYuanUri = null;

        public Dictionary<string, long> AllowLoginUserDict = new Dictionary<string, long>();
    }
}
