using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin kỹ năng tu luyện
    /// </summary>
    [ProtoContract]
    public class ExpSkillData
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(1)]
        public int SkillID { get; set; }

        /// <summary>
        /// Cấp độ hiện tại
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }

        /// <summary>
        /// Kinh nghiệm hiện tại
        /// </summary>
        [ProtoMember(3)]
        public int CurrentExp { get; set; }

        /// <summary>
        /// Kinh nghiệm thăng cấp
        /// </summary>
        [ProtoMember(4)]
        public int LevelUpExp { get; set; }

        /// <summary>
        /// Có phải kỹ năng đang tu luyện hiện tại không
        /// </summary>
        [ProtoMember(5)]
        public bool IsCurrentExpSkill { get; set; }
    }
}
