using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 
    /// </summary>
    [ProtoContract]
    public class HuanYingSiYuanRequestInfo
    {
        /// <summary>
        /// 位置
        /// </summary>
        [ProtoMember(1)]
        public int Site = 0;

        /// <summary>
        /// 当前申请的帮会ID
        /// </summary>
        [ProtoMember(2)]
        public int BHID = 0;

        /// <summary>
        /// 出价
        /// </summary>
        [ProtoMember(3)]
        public int BidMoney = 0;
    }

    /// <summary>
    /// 幻影寺院双方得分和人数信息
    /// </summary>
    [ProtoContract]
    public class HuanYingSiYuanScoreInfoData
    {
        /// <summary>
        /// 阵营1人数
        /// </summary>
        [ProtoMember(1)]
        public int RoleCount1;

        /// <summary>
        /// 阵营2人数
        /// </summary>
        [ProtoMember(2)]
        public int RoleCount2;

        /// <summary>
        /// 阵营1积分
        /// </summary>
        [ProtoMember(3)]
        public int Score1;

        /// <summary>
        /// 阵营1积分
        /// </summary>
        [ProtoMember(4)]
        public int Score2;
    }

    /// <summary>
    /// 活动状态和时间信息
    /// </summary>
    [ProtoContract]
    public class HuoDongTimeAndStateData
    {
        /// <summary>
        /// 活动类型ID
        /// </summary>
        [ProtoMember(1)]
        public int HuoDongType = 0;

        /// <summary>
        /// 活动状态
        /// </summary>
        [ProtoMember(2)]
        public int HuoDongState = 0;

        /// <summary>
        /// 倒计时开始计时时间
        /// </summary>
        [ProtoMember(3)]
        public long StartTicks = 0;

        /// <summary>
        /// 时长
        /// </summary>
        [ProtoMember(4)]
        public int TimeSecs = 0;

        /// <summary>
        /// 倒计时结束后的操作（1 返回之前的地图）
        /// </summary>
        [ProtoMember(5)]
        public int TimeOutFunction = 0;
    }

    /// <summary>
    /// 幻影寺院奖励信息
    /// </summary>
    [ProtoContract]
    public class HuanYingSiYuanAwardData
    {
        /// <summary>
        /// 胜负
        /// </summary>
        [ProtoMember(1)]
        public bool Win = false;

        /// <summary>
        /// 奖励元素粉末
        /// </summary>
        [ProtoMember(2)]
        public int YuanSuFenMo = 0;

        /// <summary>
        /// 奖励经验
        /// </summary>
        [ProtoMember(3)]
        public long Exp = 0;

        /// <summary>
        /// 阵营1得分
        /// </summary>
        [ProtoMember(4)]
        public int Score1 = 0;
        
        /// <summary>
        /// 阵营2得分
        /// </summary>
        [ProtoMember(5)]
        public int Score2 = 0;
    }
}
