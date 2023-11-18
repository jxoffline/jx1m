using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// 出售物品数据
    /// </summary>
    [ProtoContract]
    public class SaleGoodsData : IComparer<SaleGoodsData>
    {
        public int Compare(SaleGoodsData x, SaleGoodsData y)
        {
            return x.GoodsDbID - y.GoodsDbID;
        }

        /// <summary>
        /// 物品的数据库ID
        /// </summary>
        [ProtoMember(1)]
        public int GoodsDbID = 0;

        /// <summary>
        /// 出售的物品的数据
        /// </summary>
        [ProtoMember(2)]
        public GoodsData SalingGoodsData = null;

        /// <summary>
        /// 出售者的角色ID
        /// </summary>
        [ProtoMember(3)]
        public int RoleID = 0;

        /// <summary>
        /// 出售者的角色名称
        /// </summary>
        [ProtoMember(4)]
        public string RoleName = "";

        /// <summary>
        /// 出售者的角色级别
        /// </summary>
        [ProtoMember(5)]
        public int RoleLevel = 0;
    }
}