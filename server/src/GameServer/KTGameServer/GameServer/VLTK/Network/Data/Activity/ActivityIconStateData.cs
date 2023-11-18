using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Định nghĩa trạng thái của các Button chức năng
    /// </summary>
    [ProtoContract]
    public class ActivityIconStateData
    {
        /// <summary>
        /// Danh sách trạng thái
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<int, int> IconState = new Dictionary<int, int>();
    }
}