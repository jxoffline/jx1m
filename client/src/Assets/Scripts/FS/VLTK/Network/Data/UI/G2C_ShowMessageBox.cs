using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server yêu cầu hiện bảng thông báo ở Client
    /// </summary>
    [ProtoContract]
    public class G2C_ShowMessageBox
    {
        /// <summary>
        /// ID bảng thông báo
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// Loại bảng thông báo
        /// <para>0: Bảng thông báo thường, Params = {}</para>
        /// <para>1: Bảng nhập số, Params = {Giá trị MIN, Giá trị MAX, Giá trị mặc định ban đầu}</para>
        /// </summary>
        [ProtoMember(2)]
        public int MessageType { get; set; }

        /// <summary>
        /// Tiêu đề bảng
        /// </summary>
        [ProtoMember(3)]
        public string Title { get; set; }

        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        [ProtoMember(4)]
        public string Text { get; set; }

        /// <summary>
        /// Danh sách tham biến đi kèm
        /// </summary>
        [ProtoMember(5)]
        public List<string> Params { get; set; }
    }
}
