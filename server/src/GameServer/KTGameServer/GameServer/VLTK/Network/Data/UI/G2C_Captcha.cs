using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Đối tượng Captcha
    /// </summary>
    [ProtoContract]
    public class G2C_Captcha
    {
        /// <summary>
        /// Chuỗi mã hóa Captcha
        /// </summary>
        [ProtoMember(1)]
        public byte[] Data { get; set; }

        /// <summary>
        /// Danh sách câu trả lời
        /// </summary>
        [ProtoMember(2)]
        public List<string> Answers { get; set; }

        /// <summary>
        /// Chiều rộng
        /// </summary>
        [ProtoMember(3)]
        public short Width { get; set; }

        /// <summary>
        /// Chiều cao
        /// </summary>
        [ProtoMember(4)]
        public short Height { get; set; }
    }
}
