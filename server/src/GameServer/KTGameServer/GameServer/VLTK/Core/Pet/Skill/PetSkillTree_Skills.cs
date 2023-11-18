using System.Collections.Concurrent;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý kỹ năng của pet
    /// </summary>
    public partial class PetSkillTree
    {
        /// <summary>
        /// Danh sách kỹ năng mà pet học được
        /// <para>Key: ID kỹ năng</para>
        /// <para>Value: Cấp độ kỹ năng</para>
        /// </summary>
        private readonly ConcurrentDictionary<int, SkillLevelRef> listStudiedSkills = new ConcurrentDictionary<int, SkillLevelRef>();

        /// <summary>
        /// Trả về thông tin kỹ năng
        /// </summary>
        /// <param name="id"></param>
        public SkillLevelRef GetSkillLevelRef(int id)
        {
            if (this.listStudiedSkills.TryGetValue(id, out SkillLevelRef levelRef))
            {
                return levelRef;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra trong SkillTree có kỹ năng tương ứng không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasSkill(int id)
        {
            return this.GetSkillLevelRef(id) != null;
        }
    }
}
