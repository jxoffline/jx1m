using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class KMagicInfo
    {
        [ProtoMember(1)]
        public int nAttribType { get; set; }

        [ProtoMember(2)]
        public int nLevel { get; set; }

        [ProtoMember(3)]
        public int Value_1 { get; set; }

        [ProtoMember(4)]
        public int Value_2 { get; set; }

        [ProtoMember(5)]
        public int Value_3 { get; set; }
    }

    /// <summary>
    /// Định nghĩa lại phần mã hóa DATA
    /// </summary>
    [ProtoContract]
    public class ItemDataByteCode
    {
        /// <summary>
        /// Tổng có bao nhiêu thông tin cơ bản
        /// </summary>
        [ProtoMember(1)]
        public int BasicPropCount { get; set; }

        /// <summary>
        /// Thông tin về chỉ số
        /// </summary>
        [ProtoMember(2)]
        public List<KMagicInfo> BasicProp { get; set; }


        /// <summary>
        /// Số dòng xanh hiện không ẩn
        /// </summary>
        [ProtoMember(3)]
        public int GreenPropCount { get; set; }

        [ProtoMember(4)]
        public List<KMagicInfo> GreenProp { get; set; }


        /// <summary>
        /// Có bao nhiêu dòng ẩn
        /// </summary>
        [ProtoMember(5)]
        public int HiddenProbsCount { get; set; }

        [ProtoMember(6)]
        public List<KMagicInfo> HiddenProbs { get; set; }

    }
}
