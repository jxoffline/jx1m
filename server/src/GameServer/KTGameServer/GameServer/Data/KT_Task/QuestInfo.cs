using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class QuestInfo
    {
        /// <summary>
        /// 0 : Chính tuyến | 2 Nghĩa Quân | ......
        /// </summary>
        /// 
        [ProtoMember(1)]
        public int TaskClass { get; set; }

        /// <summary>
        /// ID Nhiệm vụ hiện tại
        /// </summary>
        /// 
        [ProtoMember(2)]
        public int CurTaskIndex { get; set; }

    }
}