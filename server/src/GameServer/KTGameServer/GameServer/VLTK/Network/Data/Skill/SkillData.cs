using GameServer.KiemThe;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu kỹ năng
    /// </summary>
    [ProtoContract]
    public class SkillData
    {
        /// <summary>
        /// ID được lưu trong DB
        /// </summary>
        [ProtoMember(1)]
        public int DbID { get; set; }

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(2)]
        public int SkillID { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        [ProtoMember(3)]
        public int SkillLevel { get; set; }

        /// <summary>
        /// Thời gian sử dụng lần trước
        /// </summary>
        [ProtoMember(4)]
        public long LastUsedTick { get; set; }

        /// <summary>
        /// Thời gian cooldown
        /// </summary>
        [ProtoMember(5)]
        public int CooldownTick { get; set; }

        /// <summary>
        /// Cấp độ cộng thêm từ trang bị, hoặc các kỹ năng khác
        /// </summary>
        [ProtoMember(6)]
        public int BonusLevel { get; set; }

        /// <summary>
        /// Có thể học được không (áp dụng các kỹ năng cần hoàn thành gì đó mới cho học, ví dụ kỹ năng 110)
        /// </summary>
        [ProtoMember(7)]
        public bool CanStudy { get; set; }

        /// <summary>
        /// Kinh nghiệm kỹ năng
        /// </summary>
        [ProtoMember(8)]
        public int Exp { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        public int Level
        {
            get
            {
                if (this.SkillLevel <= 0)
                {
                    return 0;
                }
                return this.SkillLevel + this.BonusLevel;
            }
        }

        /// <summary>
        /// Kỹ năng có đang trong trạng thái Cooldown không
        /// </summary>
        public bool IsCooldown
        {
            get
            {
                return this.CooldownTick > KTGlobal.GetCurrentTimeMilis() - this.LastUsedTick;
            }
        }
    }
}