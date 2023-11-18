using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// 物品列表请求结果
    /// </summary>
    [ProtoContract]
    public class SaleGoodsSearchResultData
    {
        /// <summary>
        /// 一级分类
        /// </summary>
        [ProtoMember(1)]
        public int Type;

        /// <summary>
        /// 二级分类
        /// </summary>
        [ProtoMember(2)]
        public int ID;

        /// <summary>
        /// 有效值:1金币,2钻石,3金币和钻石
        /// </summary>
        [ProtoMember(3)]
        public int MoneyFlags;

        /// <summary>
        /// 按位存储的颜色过滤选项,白色为第0位(最低位)
        /// </summary>
        [ProtoMember(4)]
        public int ColorFlags;

        /// <summary>
        /// 0 desc,1 asc
        /// </summary>
        [ProtoMember(5)]
        public int OrderBy;

        /// <summary>
        /// 搜索结果开始索引
        /// </summary>
        [ProtoMember(6)]
        public int StartIndex = 0;

        /// <summary>
        /// 总个数
        /// </summary>
        [ProtoMember(7)]
        public int TotalCount = 0;

        /// <summary>
        /// 物品列表
        /// </summary>
        [ProtoMember(8)]
        public List<SaleGoodsData> saleGoodsDataList;
    }
}