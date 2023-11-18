using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông báo nội dung công cáo sự kiện, hoạt động, phụ bản
    /// </summary>
    [ProtoContract]
    public class G2C_EventNotification
    {
        /// <summary>
        /// Tên sự kiện
        /// </summary>
        [ProtoMember(1)]
        public string EventName { get; set; }

        /// <summary>
        /// Thông tin ngắn, nếu chứa TIME| ở đầu thì đằng sau sẽ là số giây
        /// </summary>
        [ProtoMember(2)]
        public string ShortDetail { get; set; }

        /// <summary>
        /// Danh sách nội dung
        /// </summary>
        [ProtoMember(3)]
        public List<string> TotalInfo { get; set; }
    }
}
