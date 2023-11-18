using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo có đối tượng mới xung quanh
    /// </summary>
    [ProtoContract]
    public class LoadAlreadyData
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(4)]
        public int Direction { get; set; }

        /// <summary>
        /// Động tác
        /// </summary>
        [ProtoMember(5)]
        public int Action { get; set; }

        /// <summary>
        /// Đường đi hiện tại
        /// </summary>
        [ProtoMember(6)]
        public string PathString { get; set; }

        /// <summary>
        /// Vị trí đích dịch chuyển tới X
        /// </summary>
        [ProtoMember(7)]
        public int ToX { get; set; }

        /// <summary>
        /// Vị trí đích dịch chuyển tới Y
        /// </summary>
        [ProtoMember(8)]
        public int ToY { get; set; }

        /// <summary>
        /// Camp
        /// </summary>
        [ProtoMember(9)]
        public int Camp { get; set; }
    }
}