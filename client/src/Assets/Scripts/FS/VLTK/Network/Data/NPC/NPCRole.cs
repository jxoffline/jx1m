using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin NPC
    /// </summary>
    [ProtoContract]
    public class NPCRole
    {
        /// <summary>
        /// ID NPC
        /// </summary>
        [ProtoMember(1)]
        public int NPCID { get; set; }

        /// <summary>
        /// ID Res NPC
        /// </summary>
        [ProtoMember(2)]
        public int ResID { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [ProtoMember(3)]
        public string Name { get; set; }

        /// <summary>
        /// Danh hiệu
        /// </summary>
        [ProtoMember(4)]
        public string Title { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(5)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(6)]
        public int PosY { get; set; }

        /// <summary>
        /// ID bản đồ đang đứng
        /// </summary>
        [ProtoMember(7)]
        public int MapCode { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(8)]
        public int Dir { get; set; }

        /// <summary>
        /// Có hiển thị ở bản đồ khu vực không
        /// </summary>
        [ProtoMember(9)]
        public bool VisibleOnMinimap { get; set; }
    }
}