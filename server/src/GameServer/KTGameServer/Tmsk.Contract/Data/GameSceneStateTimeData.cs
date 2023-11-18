using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tmsk.Contract
{
    [ProtoContract]
    public class GameSceneStateTimeData
    {
        /// <summary>
        /// 游戏类型
        /// </summary>
        [ProtoMember(1)]
        public int GameType;

        /// <summary>
        /// 状态
        /// </summary>
        [ProtoMember(2)]
        public int State;

        /// <summary>
        /// 结束时间
        /// </summary>
        [ProtoMember(3)]
        public long EndTicks;

        /// <summary>
        /// 标识
        /// </summary>
        [ProtoMember(4)]
        public int Flags;

        /// <summary>
        /// 附加数据(保留扩展)
        /// </summary>
        [ProtoMember(5)]
        public byte[] ExtraData;
    }
}
