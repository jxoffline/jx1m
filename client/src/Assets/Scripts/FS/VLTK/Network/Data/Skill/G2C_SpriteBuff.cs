using System;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin từ Server gửi về Client thông báo Buff
    /// </summary>
    [ProtoContract]
    public class G2C_SpriteBuff
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID buff
        /// </summary>
        [ProtoMember(2)]
        public int SkillID { get; set; }

        /// <summary>
        /// ID Res của Buff
        /// </summary>
        [ProtoMember(3)]
        public int ResID { get; set; }

        /// <summary>
        /// Thời gian bắt đầu (Mili giây)
        /// </summary>
        [ProtoMember(4)]
        public long StartTick { get; set; }

        /// <summary>
        /// Thời gian duy trì (Mili giây)
        /// </summary>
        [ProtoMember(5)]
        public long Duration { get; set; }

        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        [ProtoMember(6)]
        public int Level { get; set; }

        /// <summary>
        /// Loại thao tác
        /// <para>0: Xóa Buff</para>
        /// <para>1: Thêm Buff</para>
        /// </summary>
        [ProtoMember(7)]
        public int PacketType { get; set; }

        /// <summary>
        /// Thuộc tính của Buff
        /// </summary>
        [ProtoMember(8)]
        public string Properties { get; set; }
    }
}
