using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo tốc độ di chuyển của đối tượng thay đổi
    /// </summary>
    [ProtoContract]
    public class G2C_MoveSpeedChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tốc độ di chuyển mới
        /// </summary>
        [ProtoMember(2)]
        public int MoveSpeed { get; set; }
    }
}
