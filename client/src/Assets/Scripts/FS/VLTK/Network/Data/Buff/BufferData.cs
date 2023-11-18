using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu BUFF nhân vật
    /// </summary>
    [ProtoContract]
    public class BufferData
    {
        /// <summary>
        /// ID Buff
        /// </summary>
        [ProtoMember(1)]
        public int BufferID { get; set; }

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        [ProtoMember(2)]
        public long StartTime { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [ProtoMember(3)]
        public long BufferSecs { get; set; }

        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        [ProtoMember(4)]
        public long BufferVal { get; set; }

        /// <summary>
        /// ProDict tùy chọn (có từ vật phẩm)
        /// </summary>
        [ProtoMember(5)]
        public string CustomProperty { get; set; }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID: {0}", this.BufferID);
        }
    }
}
