using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client về Server phản hồi sự kiện người chơi chọn phương thức hồi sinh
    /// </summary>
    [ProtoContract]
    public class C2G_ClientRevive
    {
        /// <summary>
        /// ID phương thức được chọn
        /// </summary>
        [ProtoMember(1)]
        public int SelectedID { get; set; }
    }
}
