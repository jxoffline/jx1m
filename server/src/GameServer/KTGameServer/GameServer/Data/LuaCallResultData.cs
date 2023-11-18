//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Shapes;
using ProtoBuf;
using System;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu kết quả Call Script LUA
    /// </summary>
    [ProtoContract]
    public class LuaCallResultData
    {
        /// <summary>
        /// ID bản đồ
        /// </summary>
        [ProtoMember(1)]
        public int MapCode = 0;

        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(2)]
        public int RoleID = 0;

        /// <summary>
        /// ID NPC
        /// </summary>
        [ProtoMember(3)]
        public int NPCID = 0;

        /// <summary>
        /// Kết quả: -1, chưa rõ, 0: Thất bại, 1: Thành công
        /// </summary>
        [ProtoMember(4)]
        public int IsSuccess;

        /// <summary>
        /// Chuỗi kết quả đầu ra
        /// </summary>
        [ProtoMember(5)]
        public String Result;

        /// <summary>
        /// 执行标志，当客户端需要确切的返回结果并进行验证时使用
        /// </summary>
        [ProtoMember(6)]
        public int Tag;

        /// <summary>
        /// Code NPC
        /// </summary>
        [ProtoMember(7)]
        public int ExtensionID;

        /// <summary>
        /// Hàm thực hiện
        /// </summary>
        [ProtoMember(8)]
        public String LuaFunction;

        /// <summary>
        /// 是否强迫刷新
        /// </summary>
        [ProtoMember(9)]
        public int ForceRefresh = 0;
    }
}