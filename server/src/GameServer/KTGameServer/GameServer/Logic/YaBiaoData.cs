using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 押镖数据
    /// </summary>
    [ProtoContract]
    public class YaBiaoData
    {
        /// <summary>
        /// 押镖ID
        /// </summary>
        [ProtoMember(1)]
        public int YaBiaoID = 0;

        /// <summary>
        /// 开始时间
        /// </summary>
        [ProtoMember(2)]
        public long StartTime = 0;

        /// <summary>
        /// 押镖状态(0:正常, 1:失败)
        /// </summary>
        [ProtoMember(3)]
        public int State = 0;

        /// <summary>
        /// 接镖时的线路ID
        /// </summary>
        [ProtoMember(4)]
        public int LineID = 0;

        /// <summary>
        /// 是否做了投保, 0: 没做 1:做了
        /// </summary>
        [ProtoMember(5)]
        public int TouBao = 0;

        /// <summary>
        /// 押镖的日ID
        /// </summary>
        [ProtoMember(6)]
        public int YaBiaoDayID = 0;

        /// <summary>
        /// 每日押镖的次数
        /// </summary>
        [ProtoMember(7)]
        public int YaBiaoNum = 0;

        /// <summary>
        /// 是否取到了货物
        /// </summary>
        [ProtoMember(8)]
        public int TakeGoods = 0;
    }
}
