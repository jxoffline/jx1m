using GameServer.KiemThe.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý Logic kỹ năng bị động tăng sát thương cho các kỹ năng khác
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Danh sách sát thương được cộng thêm bởi kỹ năng khác
        /// </summary>
        private readonly ConcurrentDictionary<int, int> appendSkillDamages = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Danh sách sát thương được cộng thêm bởi kỹ năng khác
        /// </summary>
        private readonly ConcurrentDictionary<int, int> appendSkillDamagesPercent = new ConcurrentDictionary<int, int>();


        /// <summary>
        /// Thực hiện cộng sát thương cho kỹ năng hỗ trợ
        /// </summary>
        /// <param name="supportSkillId"></param>
        /// <param name="skill"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        private void DoProcessSkillAppendDamages(SkillDataEx skill, int oldLevel, int newLevel)
        {
            /// ProDict cấp độ cũ
            PropertyDictionary oldLevelPd = oldLevel > 0 ? skill.Properties[oldLevel] : null;
            /// ProDict cấp độ mới
            PropertyDictionary newLevelPd = newLevel > 0 ? skill.Properties[newLevel] : null;

            /// % sát thương
            /// Nếu tồn tại ProDict cấp độ cũ
            if (oldLevelPd != null)
            {
                for (int i = (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent; i <= (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6; i++)
                {
                    if (oldLevelPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = oldLevelPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damagePercent = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamagesPercent.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamagesPercent[supportSkillID] = 0;
                            }
                            this.appendSkillDamagesPercent[supportSkillID] -= damagePercent;
                        }
                    }
                }
            }

            /// Nếu tồn tại ProDict cấp độ mới
            if (newLevelPd != null)
            {
                for (int i = (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent; i <= (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6; i++)
                {
                    if (newLevelPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = newLevelPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damagePercent = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamagesPercent.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamagesPercent[supportSkillID] = 0;
                            }
                            this.appendSkillDamagesPercent[supportSkillID] += damagePercent;
                        }
                    }
                }
            }

            /// Sát thương
            /// Nếu tồn tại ProDict cấp độ cũ
            if (oldLevelPd != null)
            {
                for (int i = (int) MAGIC_ATTRIB.magic_skill_addskilldamage1; i <= (int) MAGIC_ATTRIB.magic_skill_addskilldamage6; i++)
                {
                    if (oldLevelPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = oldLevelPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damage = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamages.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamages[supportSkillID] = 0;
                            }
                            this.appendSkillDamages[supportSkillID] -= damage;
                        }
                    }
                }
            }

            /// Nếu tồn tại ProDict cấp độ mới
            if (newLevelPd != null)
            {
                for (int i = (int) MAGIC_ATTRIB.magic_skill_addskilldamage1; i <= (int) MAGIC_ATTRIB.magic_skill_addskilldamage6; i++)
                {
                    if (newLevelPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = newLevelPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damage = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamages.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamages[supportSkillID] = 0;
                            }
                            this.appendSkillDamages[supportSkillID] += damage;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Xóa bỏ toàn danh sách hỗ trợ tăng sát thương kỹ năng
        /// </summary>
        public void ClearPassiveSkillAppendDamages()
        {
            this.appendSkillDamages.Clear();
            this.appendSkillDamagesPercent.Clear();
        }

        /// <summary>
        /// Thực hiện hỗ trợ tăng sát thương kỹ năng khác
        /// </summary>
        public void ProcessPassiveSkillsAppendDamages()
        {
            /// Làm rỗng danh sách cũ
            this.ClearPassiveSkillAppendDamages();

            /// Danh sách kỹ năng theo ID
            List<int> skillIDs = this.listStudiedSkills.Keys.ToList();
            /// Duyệt toàn bộ danh sách
            foreach (int skillID in skillIDs)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skill = this.GetSkillLevelRef(skillID);
                /// Toác gì đó
                if (skill == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu kỹ năng không hợp lệ
                if (!this.IsValidSkill(skill))
                {
                    continue;
                }

                PropertyDictionary skillPd = skill.Properties;
                if (skillPd == null)
                {
                    continue;
                }

                /// % sát thương
                for (int i = (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent; i <= (int) (int) MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6; i++)
                {
                    if (skillPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = skillPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damagePercent = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamagesPercent.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamagesPercent[supportSkillID] = 0;
                            }
                            this.appendSkillDamagesPercent[supportSkillID] += damagePercent;

                            SkillDataEx supportSkillData = KSkill.GetSkillData(supportSkillID);
                            //Console.WriteLine("% Damage of '{0}' += {1}% => VALUE = {2}%", supportSkillData.Name, damagePercent, this.appendSkillDamagesPercent[supportSkillID]);
                        }
                    }
                }

                /// Sát thương
                for (int i = (int) MAGIC_ATTRIB.magic_skill_addskilldamage1; i <= (int) (int) MAGIC_ATTRIB.magic_skill_addskilldamage6; i++)
                {
                    if (skillPd.ContainsKey(i))
                    {
                        KMagicAttrib attrib = skillPd.Get<KMagicAttrib>(i);
                        int supportSkillID = attrib.nValue[0];
                        int damage = attrib.nValue[1];
                        if (KSkill.IsSkillExist(supportSkillID))
                        {
                            if (!this.appendSkillDamages.TryGetValue(supportSkillID, out _))
                            {
                                this.appendSkillDamages[supportSkillID] = 0;
                            }
                            this.appendSkillDamages[supportSkillID] += damage;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thêm giá trị sát thương cộng thêm cho kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="value"></param>
        public void AddAppendDamageToSkill(int skillID, int value)
        {
            if (this.appendSkillDamages.TryGetValue(skillID, out _))
            {
                this.appendSkillDamages[skillID] += value;
            }
        }

        /// <summary>
        /// Trả về sát thương cộng thêm cho kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetAppendDamageToSkill(int skillID)
        {
            if (this.appendSkillDamages.TryGetValue(skillID, out int damage))
            {
                return damage;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Thiết lập giá trị % sát thương cộng thêm cho kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="value"></param>
        public void AddAppendDamagePercentToSkill(int skillID, int value)
        {
            if (this.appendSkillDamagesPercent.TryGetValue(skillID, out _))
            {
                this.appendSkillDamagesPercent[skillID] += value;
            }
        }

        /// <summary>
        /// Trả về % sát thương cộng thêm cho kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetAppendDamageToSkillPercent(int skillID)
        {
            if (this.appendSkillDamagesPercent.TryGetValue(skillID, out int damagePercent))
            {
                return damagePercent;
            }
            else
            {
                return 0;
            }
        }
    }
}
