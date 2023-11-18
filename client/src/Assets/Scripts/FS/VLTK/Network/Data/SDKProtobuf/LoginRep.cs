using ProtoBuf;

namespace FS.GameEngine.SDK.ProtoModel
{
    /// <summary>
    /// Đối tượng phản hồi từ SDK về login
    /// </summary>
    [ProtoContract]
    public class LoginRep
    {
        /// <summary>
        /// Mã lỗi
        /// </summary>
        [ProtoMember(1)]
        public int ErrorCode = 0;

        /// <summary>
        /// Mô tả
        /// </summary>
        [ProtoMember(2)]
        public string ErorrMsg = "";

        /// <summary>
        /// AccessToken
        /// </summary>
        [ProtoMember(3)]
        public string AccessToken = "";

        /// <summary>
        /// Thời gian login lần trước
        /// </summary>
        [ProtoMember(4)]
        public string LastLoginTime = "";

        /// <summary>
        /// IP login lần trước
        /// </summary>
        [ProtoMember(5)]
        public string LastLoginIP = "";
    }
}
