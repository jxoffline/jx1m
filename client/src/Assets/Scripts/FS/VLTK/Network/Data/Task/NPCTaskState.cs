using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server thông báo trạng thái nhiệm vụ của NPC
    /// </summary>
    [ProtoContract]
    public class NPCTaskState
    {
        /// <summary>
        /// ResID của NPC
        /// </summary>
        [ProtoMember(1)]
        public int NPCID
        {
            get;
            set;
        }

        /// <summary>
        /// Trạng thái nhiệm vụ
        /// </summary>
        [ProtoMember(2)]
        public int TaskState
        {
            get;
            set;
        }
    }
}
