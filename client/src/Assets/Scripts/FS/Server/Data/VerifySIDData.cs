using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin đăng nhập ở Client
    /// </summary>
    [ProtoContract]
    public class ClientVerifySIDData
    {
        /// <summary>
        /// Server ID
        /// </summary>
        [ProtoMember(1)]
        public string strSID { get; set; } = "";

        /// <summary>
        /// Thời gian đăng nhập
        /// </summary>
        [ProtoMember(2)]
        public long lTime { get; set; }

        /// <summary>
        /// Mã MD5
        /// </summary>        
        [ProtoMember(3)]
        public string strMD5 { get; set; } = "";
    }

    /// <summary>
    /// Thông tin xác thực đăng nhập ở Server
    /// </summary>
    [ProtoContract]
    public class ServerVerifySIDData
    {
        /// <summary>
        /// ID thiết bị
        /// </summary>
        [ProtoMember(1)]
        public string strPlatformUserID { get; set; } = "";

        /// <summary>
        /// Tài khoản
        /// </summary>
        [ProtoMember(2)]
        public string strAccountName { get; set; } = "";

        /// <summary>
        /// Thời gian đăng nhập
        /// </summary>
        [ProtoMember(3)]
        public long lTime { get; set; } = 0;

        /// <summary>
        /// WTF
        /// </summary>
        [ProtoMember(4)]
        public string strCM { get; set; } = "1";

        /// <summary>
        /// Chuỗi Token
        /// </summary>
        [ProtoMember(5)]
        public string strToken { get; set; } = "";
    }
}
