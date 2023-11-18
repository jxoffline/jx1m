using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 随身仓库数据
    /// </summary>
    [ProtoContract]
    public class PortableBagData
    {
        /// <summary>
        /// 用户扩展的格子个数
        /// </summary>
        [ProtoMember(1)]
        public int ExtGridNum = 0;

        /// <summary>
        /// 当前物品使用的格子的个数(不存数据库，每次加载后计算)
        /// </summary>
        [ProtoMember(2)]
        public int GoodsUsedGridNum = 0;
    }
}