using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tmsk.Contract
{
    /// <summary>
    /// Thông tin Liên máy chủ
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class KuaFuLineData
    {
        /// <summary>
        /// Line ID
        /// </summary>
        [ProtoMember(1)]
        public int Line;

        /// <summary>
        /// Trạng thái
        /// </summary>
        [ProtoMember(2)]
        public int State;

        /// <summary>
        /// Số lượng Online
        /// </summary>
        [ProtoMember(3)]
        public int OnlineCount;
        
        /// <summary>
        /// Số lượng Online tối đa
        /// </summary>
        [ProtoMember(4)]
        public int MaxOnlineCount;

        /// <summary>
        /// ID máy chủ
        /// </summary>
        [ProtoMember(5)]
        public int ServerId;

        /// <summary>
        /// Map code
        /// </summary>
        [ProtoMember(6)]
        public int MapCode;
    }
}
