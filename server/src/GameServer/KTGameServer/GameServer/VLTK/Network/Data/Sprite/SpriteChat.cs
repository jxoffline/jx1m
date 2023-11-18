using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu Chat
    /// </summary>
    [ProtoContract]
    public class SpriteChat
    {
        /// <summary>
        /// ID đối tượng gửi
        /// </summary>
        [ProtoMember(1)]
        public int FromRoleID { get; set; }

        /// <summary>
        /// Tên đối tượng gửi
        /// </summary>
        [ProtoMember(2)]
        public string FromRoleName { get; set; }

        /// <summary>
        /// Tên đối tượng nhận
        /// </summary>
        [ProtoMember(3)]
        public string ToRoleName { get; set; }

        /// <summary>
        /// Nội dung Chat
        /// </summary>
        [ProtoMember(4)]
        public string Content { get; set; }

        /// <summary>
        /// Kênh Chat
        /// </summary>
        [ProtoMember(5)]
        public int Channel { get; set; }

        /// <summary>
        /// Danh sách vật phẩm được đính kèm
        /// </summary>
        [ProtoMember(6)]
        public List<GoodsData> Items { get; set; }

        /// <summary>
        /// Danh sách Pet được đính kèm
        /// </summary>
        [ProtoMember(7)]
        public List<PetData> Pets { get; set; }

        /// <summary>
        /// Thời điểm Client nhận được tin nhắn
        /// </summary>
        public long TickTime { get; set; }
    }
}
