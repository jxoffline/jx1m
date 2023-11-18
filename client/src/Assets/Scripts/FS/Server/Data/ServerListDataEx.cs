using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thông tin Server
    /// </summary>
    [ProtoContract]
    public class BuffServerInfo
    {
        /// <summary>
        /// ID nhóm
        /// </summary>
        [ProtoMember(1)]
        public int nServerOrder;

        /// <summary>
        /// ID Server
        /// </summary>
        [ProtoMember(2)]
        public int nServerID;

        /// <summary>
        /// Số người Online
        /// </summary>
        [ProtoMember(3)]
        public int nOnlineNum;

        /// <summary>
        /// Danh sách bản đồ Online
        /// </summary>
        [ProtoMember(4)]
        public List<int> listMapOnline = new List<int>();

        /// <summary>
        /// Tên Server
        /// </summary>
        [ProtoMember(5)]
        public string strServerName = "";

        /// <summary>
        /// Thời gian mở
        /// </summary>
        [ProtoMember(6)]
        public string strStartTime = "";

        /// <summary>
        /// Trạng thái
        /// </summary>
        [ProtoMember(7)]
        public int nStatus;

        /// <summary>
        /// Địa chỉ IP
        /// </summary>
        [ProtoMember(8)]
        public string strURL;

        /// <summary>
        /// Port
        /// </summary>
        [ProtoMember(9)]
        public int nServerPort;

        /// <summary>
        /// Text trạng thái bảo trì
        /// </summary>
        [ProtoMember(10)]
        public String strMaintainTxt;

        /// <summary>
        /// Text thời gian bảo trì
        /// </summary>
        [ProtoMember(11)]
        public String strMaintainStarTime;

        /// <summary>
        /// Hết hạn thời gian bảo trì
        /// </summary>
        [ProtoMember(12)]
        public String strMaintainTerminalTime;

        /// <summary>
        /// Nội dung Hint máy chủ
        /// </summary>
        [ProtoMember(13)]
        public string Msg;
    }

    /// <summary>
    /// Thông tin danh sách máy chủ
    /// </summary>
    [ProtoContract]
    public class BuffServerListDataEx
    {
        /// <summary>
        /// Danh sách server
        /// </summary>
        [ProtoMember(1)]
        public List<BuffServerInfo> ListServerData = new List<BuffServerInfo>();

        /// <summary>
        /// Danh sách server đề xuất
        /// </summary>
        [ProtoMember(2)]
        public List<BuffServerInfo> RecommendListServerData = new List<BuffServerInfo>();

        /// <summary>
        /// Server đã chọn lần trước
        /// </summary>
        [ProtoMember(3)]
        public BuffServerInfo LastServerLogin = new BuffServerInfo();
    }


    /// <summary>
    /// Thông tin máy chủ SDK
    /// </summary>
    [ProtoContract]
    public class ClientServerListDataEx
    {
        /// <summary>
        /// Thời gian
        /// </summary>
        [ProtoMember(1)]
        public long Time;

        /// <summary>
        /// Mã MD5
        /// </summary>
        [ProtoMember(2)]
        public string Md5 = "";

        /// <summary>
        /// ID Server
        /// </summary>
        [ProtoMember(3)]
        public int ServerId;

        /// <summary>
        /// ID phiên đăng nhập
        /// </summary>
        [ProtoMember(4)]
        public string UserId;
    }
}
