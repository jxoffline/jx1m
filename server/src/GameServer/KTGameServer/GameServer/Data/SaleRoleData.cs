using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 出售物品的角色数据
    /// </summary>
    [ProtoContract]
    public class SaleRoleData
    {
        /// <summary>
        /// 出售者的角色ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID = 0;

        /// <summary>
        /// 出售者的角色名称
        /// </summary>
        [ProtoMember(2)]
        public string RoleName = "";

        /// <summary>
        /// 出售者的角色级别
        /// </summary>
        [ProtoMember(3)]
        public int RoleLevel = 0;

        /// <summary>
        /// 出售者的角色级别
        /// </summary>
        [ProtoMember(4)]
        public int SaleGoodsNum = 0;
    }
}