using GameServer.Logic;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý kỹ năng của pet
    /// </summary>
    public partial class PetSkillTree
    {
        /// <summary>
        /// Pet tương ứng
        /// </summary>
        public Pet Pet { get; private set; }

        /// <summary>
        /// Quản lý kỹ năng của Pet
        /// </summary>
        /// <param name="pet"></param>
        public PetSkillTree(Pet pet)
        {
            this.Pet = pet;
            this.BuildSkillTree();
        }

        /// <summary>
        /// Xây danh sách kỹ năng Pet
        /// </summary>
        private void BuildSkillTree()
        {
            this.listStudiedSkills.Clear();

            if (this.Pet.Skills != null)
            {
                foreach (KeyValuePair<int, int> pair in this.Pet.Skills)
                {
                    SkillDataEx skillData = KSkill.GetSkillData(pair.Key);
                    if (skillData != null)
                    {
                        SkillLevelRef levelRef = new SkillLevelRef()
                        {
                            AddedLevel = pair.Value,
                            Data = skillData,
                            BonusLevel = 0,
                            CanStudy = false,
                            Exp = 0,
                        };
                        this.listStudiedSkills[skillData.ID] = levelRef;
                    }
                }
            }

            /// Thêm kỹ năng đánh thường
            {
                SkillDataEx skillData = KSkill.GetSkillData(KPet.Config.BaseAttackSkillID);
                if (skillData != null)
                {
                    SkillLevelRef levelRef = new SkillLevelRef()
                    {
                        AddedLevel = 1,
                        Data = skillData,
                        BonusLevel = 0,
                        CanStudy = false,
                        Exp = 0,
                    };
                    this.listStudiedSkills[skillData.ID] = levelRef;
                }
            }
        }
    }
}
