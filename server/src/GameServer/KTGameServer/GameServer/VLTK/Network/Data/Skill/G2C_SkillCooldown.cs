using ProtoBuf;
using System;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo thời gian phục hồi kỹ năng
    /// </summary>
    [ProtoContract]
    public class G2C_SkillCooldown
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(1)]
        public int SkillID { get; set; }

        /// <summary>
        /// Thời gian bắt đầu thiết lập
        /// </summary>
        [ProtoMember(2)]
        public long StartTick { get; set; }

        /// <summary>
        /// Thời gian phục hồi
        /// </summary>
        [ProtoMember(3)]
        public int CooldownTick { get; set; }

        /// <summary>
        /// Đã hết thời gian Cooldown chưa
        /// </summary>
        public bool IsOver
        {
            get
            {
                return DateTime.Now.Ticks - this.StartTick >= this.CooldownTick;
            }
        }
    }
}
