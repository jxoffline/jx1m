using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class JiQingHuiKuiData
    {
        /// <summary>
        /// 冲级领取神装剩余名额
        /// </summary>
        [ProtoMember(1)]
        public List<int> ChongJiQingQuShenZhuangQuota;

        /// <summary>
        /// 神装回赠剩余名额
        /// </summary>
        [ProtoMember(2)]
        public int ShenZhuangHuiZengQuoto;

        /// <summary>
        /// 幸运大抽奖剩余次数
        /// </summary>
        [ProtoMember(3)]
        public int XingYunChouJiangCount;

        /// <summary>
        /// 今日的充值元宝数
        /// </summary>
        [ProtoMember(4)]
        public int TodayYuanBao;

        /// <summary>
        /// 今日的充值元宝领取状态
        /// </summary>
        [ProtoMember(5)]
        public int TodayYuanBaoState;

        /// <summary>
        /// 冲级领取神装的领取状态
        /// </summary>
        [ProtoMember(6)]
        public int ChongJiLingQuShenZhuangState;

        /// <summary>
        /// 幸运大抽奖活动期间充值金额
        /// </summary>
        [ProtoMember(7)]
        public int XingYunChouJiangYuanBao;


        /// <summary>
        /// 神装回赠领取状态
        /// </summary>
        [ProtoMember(8)]
        public int ShenZhuangHuiZengState;


    }
}

