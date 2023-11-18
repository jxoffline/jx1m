using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// 名字验证数据
    /// </summary>
    [ProtoContract]
    public class NameRegisterData
    {
        /// <summary>
        /// 名字
        /// </summary>
        [ProtoMember(1)]
        public string Name;

        /// <summary>
        /// 平台ID
        /// </summary>
        [ProtoMember(2)]
        public string PingTaiID;

        /// <summary>
        /// 区号
        /// </summary>
        [ProtoMember(3)]
        public int ZoneID;

        /// <summary>
        /// 名字类型(角色名0,帮派名1)
        /// </summary>
        [ProtoMember(4)]
        public int NameType;

        /// <summary>
        /// 用户ID
        /// </summary>
        [ProtoMember(5)]
        public string UserID;

        /// <summary>
        /// 注册时间
        /// </summary>
        [ProtoMember(6)]
        public string RegTime;
    }
}