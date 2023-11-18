using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoBuf;

namespace Tmsk.Contract.Data
{
    /// <summary>
    /// 充值王排行数据
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class InputKingPaiHangData
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [ProtoMember(1)]
        public string UserID;

        /// <summary>
        /// 排行
        /// </summary>
        [ProtoMember(2)]
        public int PaiHang;

        /// <summary>
        /// 排行时间
        /// </summary>
        [ProtoMember(3)]
        public string PaiHangTime = "";

        /// <summary>
        /// 排行值，用于计算排行的值,对于充值，这儿元宝数值
        /// </summary>
        [ProtoMember(4)]
        public int PaiHangValue;

        /// <summary>
        /// 最大等级角色名称
        /// </summary>
        [ProtoMember(5)]
        public string MaxLevelRoleName = "";

        /// <summary>
        /// 最大等级角色所在区
        /// </summary>
        [ProtoMember(6)]
        public int MaxLevelRoleZoneID = 1;
    }

    /// <summary>
    /// 平台充值王，跨服中心与后台查询交互
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class InputKingPaiHangDataEx
    {
        /// <summary>
        /// 排行榜数据
        /// </summary>
        [ProtoMember(1)]
        public List<InputKingPaiHangData> ListData;

        /// <summary>
        /// 排行时间
        /// </summary>
        [ProtoMember(2)]
        public string RankTime;

        /// <summary>
        /// 活动开始时间
        /// </summary>
        [ProtoMember(3)]
        public string StartTime;

        /// <summary>
        /// 活动结束时间
        /// </summary>
        [ProtoMember(4)]
        public string EndTime;
    }
}
