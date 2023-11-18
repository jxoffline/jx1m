using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu về nhiệm vụ đã làm
    /// </summary>
    [ProtoContract]
    public class OldTaskData
    {
        /// <summary>
        /// Task ID
        /// </summary>
        [ProtoMember(1)]
        public int TaskID;

        /// <summary>
        /// Đã làm bao nhiêu lần
        /// </summary>
        [ProtoMember(2)]
        public int DoCount;

        /// <summary>
        /// Thuộc về loại nhiệm vụ gì
        /// </summary>
        [ProtoMember(3)]
        public int TaskClass;
    }
}