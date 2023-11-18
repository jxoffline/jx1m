using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo trạng thái ẩn thân của đối tượng thay đổi
    /// </summary>
    [ProtoContract]
    public class G2C_SpriteInvisibleStateChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Loại trạng thái
        /// <para>0: Mất trạng thái</para>
        /// <para>1: Vào trạng thái</para>
        /// </summary>
        [ProtoMember(2)]
        public int Type { get; set; }
    }
}
