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
        /// Thời gian sử dụng lần cuối
        /// </summary>
        [ProtoMember(4)]
        public long LastUsedTick { get; set; }

        /// <summary>
        /// Thời gian Cooldown
        /// </summary>
        [ProtoMember(5)]
        public int Cooldown { get; set; }

        /// <summary>
        /// Cấp độ cộng thêm từ trang bị, hoặc các kỹ năng khác
        /// </summary>
        [ProtoMember(6)]
        public int BonusLevel { get; set; } = 0;

        /// <summary>
        /// Có thể học được không (áp dụng các kỹ năng cần hoàn thành gì đó mới cho học, ví dụ kỹ năng 110)
        /// </summary>
        [ProtoMember(7)]
        public bool CanStudy { get; set; } = false;

        /// <summary>
        /// Thời gian Cooldown
        /// </summary>
        [ProtoMember(8)]
        public int Exp { get; set; }
    }
}