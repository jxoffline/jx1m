using System;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client về Server yêu cầu sử dụng kỹ năng
    /// </summary>
    [ProtoContract]
    public class C2G_UseSkill
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(1)]
        public int SkillID { get; set; }

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        [ProtoMember(2)]
        public int Direction { get; set; }

        /// <summary>
        /// ID mục tiêu
        /// </summary>
        [ProtoMember(3)]
        public int TargetID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(4)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(5)]
        public int PosY { get; set; }

        /// <summary>
        /// Vị trí mục tiêu X
        /// </summary>
        [ProtoMember(6)]
        public int TargetPosX { get; set; }

        /// <summary>
        /// Vị trí mục tiêu Y
        /// </summary>
        [ProtoMember(7)]
        public int TargetPosY { get; set; }
    }
}
