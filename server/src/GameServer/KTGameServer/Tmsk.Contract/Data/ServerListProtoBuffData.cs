using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Tmsk.Contract
{
    /// <summary>
    /// 服务器信息
    /// </summary>
    [ProtoContract]
    public class BuffServerInfo
    {
        /// <summary>
        /// 服务器编号
        /// </summary>
        [ProtoMember(1)]
        public int nServerOrder;

        /// <summary>
        /// 服务器ID
        /// </summary>
        [ProtoMember(2)]
        public int nServerID;

        /// <summary>
        /// 总的在线数
        /// </summary>
        [ProtoMember(3)]
        public int nOnlineNum;

        /// <summary>
        /// MAP在线数
        /// </summary>
        [ProtoMember(4)]
        public List<int> listMapOnline = new List<int>();

        /// <summary>
        /// 服务器名称
        /// </summary>
        [ProtoMember(5)]
        public string strServerName = "";

        /// <summary>
        /// 服务器启动时间
        /// </summary>
        [ProtoMember(6)]
        public string strStartTime = "";

        /// <summary>
        /// 服务器状态
        /// </summary>
        [ProtoMember(7)]
        public int nStatus;

        /// <summary>
        /// 服务器地址
        /// </summary>
        [ProtoMember(8)]
        public string strURL;

        /// <summary>
        /// 服务器端口
        /// </summary>
        [ProtoMember(9)]
        public int nServerPort;

        /// <summary>
        /// 维护公告内容
        /// </summary>
        [ProtoMember(10)]
        public String strMaintainTxt;

        /// <summary>
        /// 维护开始时间
        /// </summary>
        [ProtoMember(11)]
        public String strMaintainStarTime;

        /// <summary>
        /// 维护结束时间
        /// </summary>
        [ProtoMember(12)]
        public String strMaintainTerminalTime;
    }

    /// <summary>
    /// GM工具请求服务器列表时，传过的参数数据
    /// </summary>
    [ProtoContract]
    public class ClientServerListData
    {
        /// <summary>
        /// 发送消息时的时间戳
        /// </summary>
        [ProtoMember(1)]
        public long lTime;

        /// <summary>
        /// 时间戳格式化成字符串后，与私钥生成的MD5码，以便作简单的验证。
        /// </summary>        
        [ProtoMember(2)]
        public string strMD5 = "";
    }

    /// <summary>
    /// 服务器列表数据
    /// </summary>
    [ProtoContract]
    public class ServerListData
    {
        /// <summary>
        /// 服务器列表数据
        /// </summary>
        [ProtoMember(1)]
        public string content = "";
    }

    /// <summary>
    /// 服务器列表数据
    /// </summary>
    [ProtoContract]
    public class BuffServerListData
    {
        /// <summary>
        /// 服务器列表数据
        /// </summary>
        [ProtoMember(1)]
        public List<BuffServerInfo> listServerData = null;
    }
}
