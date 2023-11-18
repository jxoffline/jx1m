using System.Collections.Generic;
using ProtoBuf;
using System;

namespace Server.Data
{
    public enum YongZheZhanChangGameStates
    {
        None, //无
        SignUp, //报名时间
        Wait, //等待开始
        Start, //开始
        Awards, //有未领取奖励
        NotJoin, // 未参加本次活动
    }

    /// <summary>
    /// 角色的天梯数据
    /// </summary>
    [ProtoContract]
    public class YongZheZhanChangScoreData
    {
        /// <summary>
        /// 阵营1得分
        /// </summary>
        [ProtoMember(1)]
        public int Score1;

        /// <summary>
        /// 阵营2得分
        /// </summary>
        [ProtoMember(2)]
        public int Score2;
    }

    /// <summary>
    /// 分数增加
    /// </summary>
    [ProtoContract]
    public class YongZheZhanChangSelfScore
    {
        /// <summary>
        /// 分数
        /// </summary>
        [ProtoMember(1)]
        public int AddScore;

        /// <summary>
        /// 区号
        /// </summary>
        [ProtoMember(2)]
        public int ZoneID;

        /// <summary>
        /// 名字
        /// </summary>
        [ProtoMember(3)]
        public string Name = "";

        /// <summary>
        /// 阵营方
        /// </summary>
        [ProtoMember(4)]
        public int Side;

        /// <summary>
        /// 得分的角色ID
        /// </summary>
        [ProtoMember(5)]
        public int RoleId;

        /// <summary>
        /// 如果是连杀得分,这个值表示连杀数,否则为0
        /// </summary>
        [ProtoMember(6)]
        public int ByLianShaNum;

        /// <summary>
        /// 职业
        /// </summary>
        [ProtoMember(7)]
        public int Occupation;

        /// <summary>
        /// 个人的总积分
        /// </summary>
        [ProtoMember(8)]
        public int TotalScore;
    }

    /// <summary>
    /// 战斗结束的结果和奖励
    /// </summary>
    [ProtoContract]
    public class YongZheZhanChangAwardsData
    {
        /// <summary>
        /// 战斗结果(0失败,1胜利)
        /// </summary>
        [ProtoMember(1)]
        public int Success;

        /// <summary>
        /// 奖励绑定金币
        /// </summary>
        [ProtoMember(2)]
        public int BindJinBi;

        /// <summary>
        /// 奖励经验值
        /// </summary>
        [ProtoMember(3)]
        public long Exp;


        /// <summary>
        /// 阵营1得分
        /// </summary>
        [ProtoMember(5)]
        public int SideScore1;

        /// <summary>
        /// 阵营2得分
        /// </summary>
        [ProtoMember(6)]
        public int SideScore2;

        /// <summary>
        /// 自己得分
        /// </summary>
        [ProtoMember(7)]
        public int SelfScore;
    }
}
