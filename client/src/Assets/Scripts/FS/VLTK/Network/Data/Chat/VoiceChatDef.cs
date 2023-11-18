using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Data
{
    /// <summary>
    /// Thông tin Chat Voice truyền về từ Server
    /// </summary>
    [ProtoContract]
    public class ServerGetAudioChatData
    {
        /// <summary>
        /// Chuỗi Byte nội dung
        /// </summary>
        [ProtoMember(1)]
        public byte[] arrAudioChat { get; set; } = null;
    }


    /// <summary>
    /// Phản hồi Push Chat Voice
    /// </summary>
    [ProtoContract]
    public class VoiceChatResponse
    {
        /// <summary>
        /// Trạng thái
        /// </summary>
        [ProtoMember(1)]
        public int Status { get; set; }
    }


    /// <summary>
    /// Packet send lên để lấy chat voice tương ứng
    /// </summary>
    [ProtoContract]
    public class GetVoiceFile
    {
        /// <summary>
        /// ID Chat
        /// </summary>
        [ProtoMember(1)]
        public string ChatID { get; set; }
    }

    /// <summary>
    /// Data gửi lên sv để lưu lại chat voice của nhân vật
    /// Đồng thời là data gửi về khi gửi request lấy voice data về
    /// </summary>
    [ProtoContract]
    public class ChatVoiceData
    {
        /// <summary>
        /// ID Chat
        /// ID thằng chat | 1 số RANDOM
        /// </summary>
        [ProtoMember(1)]
        public string ChatID { get; set; }

        /// <summary>
        /// Tên đối tượng gửi
        /// </summary>
        [ProtoMember(2)]
        public string FromRoleName { get; set; }

        /// <summary>
        /// Tên đối tượng nhận
        /// </summary>
        [ProtoMember(3)]
        public string ToRoleName { get; set; }

        /// <summary>
        /// Kênh
        /// </summary>
        [ProtoMember(4)]
        public int Channel { get; set; }

        /// <summary>
        /// Dữ liệu Chat
        /// </summary>
        [ProtoMember(5)]
        public ServerGetAudioChatData VoiceData { get; set; }
    }
}
