using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Tmsk.Contract;

namespace Tmsk.Contract
{
    /// <summary>
    /// 跨服登录数据
    /// </summary>
    [ProtoContract]
    public class KuaFuServerLoginData
    {
        /// <summary>
        /// 登录token
        /// </summary>
        [ProtoMember(1)]
        public WebLoginToken WebLoginToken;

        /// <summary>
        /// 角色ID
        /// </summary>
        [ProtoMember(2)]
        public int RoleId = 0;

        /// <summary>
        /// 跨服游戏副本ID或跨服地图的地图编号
        /// </summary>
        [ProtoMember(3)]
        public long GameId;

        /// <summary>
        /// 跨服活动类型
        /// </summary>
        [ProtoMember(4)]
        public int GameType = 1;

        /// <summary>
        /// 本地副本序列ID
        /// </summary>
        [ProtoMember(5)]
        public int FuBenSeqId;

        /// <summary>
        /// 结束时间
        /// </summary>
        [ProtoMember(6)]
        public long EndTicks = 0;

        /// <summary>
        /// 玩家的的服务器ID
        /// </summary>
        [ProtoMember(7)]
        public int ServerId;

        /// <summary>
        /// 跨服服务器的IP
        /// </summary>
        [ProtoMember(8)]
        public string ServerIp;

        /// <summary>
        /// 跨服服务器的端口
        /// </summary>
        [ProtoMember(9)]
        public int ServerPort;
    }


}
