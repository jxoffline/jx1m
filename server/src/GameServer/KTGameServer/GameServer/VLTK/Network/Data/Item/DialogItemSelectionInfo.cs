using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin vật phẩm chọn trong danh sách của NPCDialog hoặc ItemDialog
    /// </summary>
    [ProtoContract]
    public class DialogItemSelectionInfo
    {
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(1)]
        public int ItemID { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        [ProtoMember(2)]
        public int Quantity { get; set; }

        /// <summary>
        /// Khóa hay không
        /// </summary>
        [ProtoMember(3)]
        public int Binding { get; set; }
    }
}
