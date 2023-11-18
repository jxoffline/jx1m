using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 线路数据
    /// </summary>
    [ProtoContract]
    public class LineData
    {
        /// <summary>
        /// 线路ID
        /// </summary>
        [ProtoMember(1)]
        public int LineID = 0;

        /// <summary>
        /// 游戏服务器IP
        /// </summary>
        [ProtoMember(2)]
        public string GameServerIP = "";

        /// <summary>
        /// 游戏服务器端口
        /// </summary>
        [ProtoMember(3)]
        public int GameServerPort = 0;

        /// <summary>
        /// 在线人数
        /// </summary>
        [ProtoMember(4)]
        public int OnlineCount = 0;
    }
}
