using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Đối tượng Buff
    /// </summary>
    [ProtoContract]
    public class BufferData
    {
        /// <summary>
        /// Buffer DbID
        /// </summary>
        [ProtoMember(1)]
        public int BufferID { get; set; } = 0;

        /// <summary>
        /// Thời gian kích hoạt Buff (Milis)
        /// </summary>
        [ProtoMember(2)]
        public long StartTime { get; set; } = 0;

        /// <summary>
        /// Thời gian tồn tại Buff (Milis)
        /// </summary>
        [ProtoMember(3)]
        public long BufferSecs { get; set; } = 0;

        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        [ProtoMember(4)]
        public long BufferVal { get; set; } = 0;

        /// <summary>
        /// Thuộc tính tùy chỉnh
        /// </summary>
        [ProtoMember(5)]
        public string CustomProperty { get; set; } = "";
    }
}