using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 任务奖励数据
    /// </summary>
    [ProtoContract]
    public class AwardsItemData
    {
        /// <summary>
        /// 职业标识
        /// </summary>
        [ProtoMember(1)]
        public int Occupation = 0;

        /// <summary>
        /// 物品ID
        /// </summary>
        [ProtoMember(2)]
        public int GoodsID = 0;

        /// <summary>
        /// 物品数量
        /// </summary>
        [ProtoMember(3)]
        public int GoodsNum = 0;

        /// <summary>
        /// 是否绑定物品
        /// </summary>
        [ProtoMember(4)]
        public int Binding = 0;

        /// <summary>
        /// 物品的级别
        /// </summary>
        [ProtoMember(5)]
        public int Level = 0;

        /// <summary>
        /// 物品的品质
        /// </summary>
        [ProtoMember(6)]
        public int Quality = 0;

        /// <summary>
        /// 物品的截止时间
        /// </summary>
        [ProtoMember(7)]
        public string EndTime = "";

        /// <summary>
        /// 物品的天生
        /// </summary>
        [ProtoMember(8)]
        public int BornIndex = 0;

        /// <summary>
        /// 性别标示
        /// </summary>
        [ProtoMember(9)]
        public int RoleSex = 0;
    }
}
