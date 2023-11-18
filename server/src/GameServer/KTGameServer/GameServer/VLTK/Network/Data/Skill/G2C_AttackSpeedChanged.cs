using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo tốc đánh của đối tượng thay đổi
    /// </summary>
    [ProtoContract]
    public class G2C_AttackSpeedChanged
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// </summary>
        [ProtoMember(2)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// </summary>
        [ProtoMember(3)]
        public int CastSpeed { get; set; }
    }
}
