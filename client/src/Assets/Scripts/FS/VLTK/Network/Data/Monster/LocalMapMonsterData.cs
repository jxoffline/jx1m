using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin quái ở bản đồ khu vực
    /// </summary>
    [ProtoContract]
    public class LocalMapMonsterData
    {
        /// <summary>
        /// Tên quái
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// Có phải Boss không
        /// </summary>
        [ProtoMember(4)]
        public bool IsBoss { get; set; }
    }
}
