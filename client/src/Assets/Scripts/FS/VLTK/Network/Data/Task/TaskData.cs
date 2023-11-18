using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin nhiệm vụ
    /// </summary>
    [ProtoContract]
    public class TaskData
    {
        /// <summary>
        /// DbID
        /// </summary>
        [ProtoMember(1)]
        public int DbID { get; set; }

        /// <summary>
        /// ID nhiệm vụ đang làm (trong file XML)
        /// </summary>
        [ProtoMember(2)]
        public int DoingTaskID { get; set; }

        /// <summary>
        /// Value 1 của nhiệm vụ
        /// </summary>
        [ProtoMember(3)]
        public int DoingTaskVal1 { get; set; }

        /// <summary>
        /// Value 2 của nhiệm vụ
        /// </summary>
        [ProtoMember(4)]
        public int DoingTaskVal2 { get; set; }

        /// <summary>
        /// Đang theo dõi nhiệm vụ nào
        /// </summary>
        [ProtoMember(5)]
        public int DoingTaskFocus { get; set; }

        /// <summary>
        /// Thời gian nhận
        /// </summary>
        [ProtoMember(6)]
        public long AddDateTime { get; set; }

        /// <summary>
        /// Tổng số lần đã làm
        /// </summary>
        [ProtoMember(7)]
        public int DoneCount { get; set; } = 0;

        /// <summary>
        /// Số sao của nhiệm vụ
        /// <para>Tạm thời chưa dùng</para>
        /// </summary>
        [ProtoMember(8)]
        public int StarLevel { get; set; } = 0;
    }
}
