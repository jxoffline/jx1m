using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tmsk.Contract
{
    /// <summary>
    /// Thông tin đăng nhập Web
    /// </summary>
    [ProtoContract]
    public class WebLoginToken
    {
        /// <summary>
        /// Version
        /// </summary>
        [ProtoMember(1)]
        public int VerSign;

        /// <summary>
        /// UserID
        /// </summary>
        [ProtoMember(2)]
        public string UserID;

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        [ProtoMember(3)]
        public string UserName;

        /// <summary>
        /// Thời gian lần trước
        /// </summary>
        [ProtoMember(4)]
        public string LastTime;

        /// <summary>
        /// WTF
        /// </summary>
        [ProtoMember(5)]
        public string Isadult;

        /// <summary>
        /// Mã đăng nhập
        /// </summary>
        [ProtoMember(6)]
        public string SignCode;
    }
}
