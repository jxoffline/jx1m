using GameServer.KiemThe.Logic;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý kỹ năng của pet
    /// </summary>
    public partial class PetSkillTree
    {
        /// <summary>
        /// Làm mới hiệu ứng của kỹ năng bị động
        /// </summary>
        public void ProcessPassiveSkills()
        {
            /// Duyệt toàn bộ danh sách
            foreach (SkillLevelRef skill in this.listStudiedSkills.Values)
            {
                /// Nếu tồn tại và là kỹ năng bị động
                if (skill.Data.Type == 3 && skill.Level > 0)
                {
                    KTSkillManager.ProcessPassiveSkill(this.Pet, skill);
                }
            }
        }

        /// <summary>
        /// Xóa toàn bộ hiệu ứng của kỹ năng bị động
        /// </summary>
        public void RemovePassiveSkillEffects()
        {
            /// Duyệt toàn bộ danh sách
            foreach (SkillLevelRef skill in this.listStudiedSkills.Values)
            {
                /// Nếu tồn tại và là kỹ năng bị động
                if (skill.Data.Type == 3 && skill.Level > 0)
                {
                    /// Nếu có Buff tương ứng
                    if (this.Pet.Buffs.HasBuff(skill.SkillID))
                    {
                        /// Xóa Buff tương ứng
                        this.Pet.Buffs.RemoveBuff(skill.SkillID);
                    }
                }
            }
        }
    }
}
