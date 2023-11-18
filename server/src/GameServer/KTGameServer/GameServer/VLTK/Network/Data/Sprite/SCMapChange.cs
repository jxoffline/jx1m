using ProtoBuf;
using System;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi thông báo đối tượng chuyển map
    /// </summary>
    [ProtoContract]
    public class SCMapChange
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID cổng dịch chuyển
        /// </summary>
        [ProtoMember(2)]
        public int TeleportID { get; set; }

        /// <summary>
        /// ID bản đồ dịch đến
        /// </summary>
        [ProtoMember(3)]
        public int MapCode { get; set; }

        /// <summary>
        /// Vị trí X dịch đến
        /// </summary>
        [ProtoMember(4)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y dịch đến
        /// </summary>
        [ProtoMember(5)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(6)]
        public int Direction { get; set; }

        /// <summary>
        /// Mã lỗi trả về
        /// </summary>
        [ProtoMember(7)]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Vị trí đích đến
        /// </summary>
        public UnityEngine.Vector2 Position
        {
            get
            {
                return new UnityEngine.Vector2(this.PosX, this.PosY);
            }
        }
    }
}