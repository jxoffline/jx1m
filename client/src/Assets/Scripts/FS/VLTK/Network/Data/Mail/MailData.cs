using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu thư
    /// </summary>
    [ProtoContract]
    public class MailData
    {
        /// <summary>
        /// ID thư
        /// </summary>
        [ProtoMember(1)]
        public int MailID { get; set; } = 0;

        /// <summary>
        /// ID đối tượng gửi thư
        /// </summary>
        [ProtoMember(2)]
        public int SenderRID { get; set; } = 0;

        /// <summary>
        /// Tên đối tượng gửi thư
        /// </summary>
        [ProtoMember(3)]
        public string SenderRName { get; set; } = "";

        /// <summary>
        /// Thời gian gửi thư
        /// </summary>
        [ProtoMember(4)]
        public string SendTime { get; set; } = "";

        /// <summary>
        /// ID đối tượng nhận thư
        /// </summary>
        [ProtoMember(5)]
        public int ReceiverRID { get; set; } = 0;

        /// <summary>
        /// Tên đối tượng nhận thư
        /// </summary>
        [ProtoMember(6)]
        public string ReveiverRName { get; set; } = "";

        /// <summary>
        /// Thời gian nhận
        /// </summary>
        [ProtoMember(7)]
        public string ReadTime { get; set; } = "1900-01-01 12:00:00";

        /// <summary>
        /// Đánh dấu đã đọc chưa (0, 1)
        /// </summary>
        [ProtoMember(8)]
        public int IsRead { get; set; } = 0;

        /// <summary>
        /// Loại thư (0: Kèm tiền thường, 1: Kèm tiền khóa)
        /// </summary>
        [ProtoMember(9)]
        public int MailType { get; set; } = 0;

        /// <summary>
        /// Có đính kèm không (tiền hoặc vật phẩm)
        /// </summary>
        [ProtoMember(10)]
        public int HasFetchAttachment { get; set; } = 0;

        /// <summary>
        /// Tiêu đề thư
        /// </summary>
        [ProtoMember(11)]
        public string Subject { get; set; } = "";

        /// <summary>
        /// Nội dung thư
        /// </summary>
        [ProtoMember(12)]
        public string Content { get; set; } = "";

        /// <summary>
        /// Số KNB đính kèm
        /// </summary>
        [ProtoMember(13)]
        public int Token { get; set; } = 0;

        /// <summary>
        /// Số bạc đính kèm
        /// </summary>
        [ProtoMember(14)]
        public int Money { get; set; } = 0;

        /// <summary>
        /// Danh sách vật phẩm đính kèm
        /// </summary>
        [ProtoMember(15)]
        public List<GoodsData> GoodsList { get; set; } = null;
    }
}