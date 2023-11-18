using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// 群邮件项数据
    /// </summary>
    [ProtoContract]
    internal class GroupMailData
    {
        /// <summary>
        /// GMailID
        /// </summary>
        [ProtoMember(1)]
        public int GMailID = 0;

        /// <summary>
        /// 邮件主题
        /// </summary>
        [ProtoMember(2)]
        public string Subject = "";

        /// <summary>
        /// 内容
        /// </summary>
        [ProtoMember(3)]
        public string Content = "";

        /// <summary>
        /// 发放人的条件
        /// </summary>
        [ProtoMember(4)]
        public string Conditions = "";

        /// <summary>
        /// 输入的时间
        /// </summary>
        [ProtoMember(5)]
        public long InputTime = 0;

        /// <summary>
        /// 截止的时间
        /// </summary>
        [ProtoMember(6)]
        public long EndTime = 0;

        /// <summary>
        /// 发送的银两
        /// </summary>
        [ProtoMember(7)]
        public int BoundToken = 0;

        /// <summary>
        /// 发送的铜钱
        /// </summary>
        [ProtoMember(8)]
        public int Tongqian = 0;

        /// <summary>
        /// 发送的元宝
        /// </summary>
        [ProtoMember(9)]
        public int YuanBao = 0;

        /// <summary>
        /// 邮件物品列表
        /// </summary>
        [ProtoMember(10)]
        public List<GoodsData> GoodsList = null;
    }
}