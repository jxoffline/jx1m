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
    /// Quản lý Logic kỹ năng hỗ trợ kỹ năng khác
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Danh sách thuộc tính của kỹ năng được hỗ trợ
        /// </summary>
        private readonly ConcurrentDictionary<int, PropertyDictionary> enchantSkills = new ConcurrentDictionary<int, PropertyDictionary>();

        /// <summary>
        /// Làm rỗng danh sách kỹ năng hỗ trợ kỹ năng khác
        /// </summary>
        public void ClearEnchantSkills()
        {
            this.enchantSkills.Clear();
        }

        /// <summary>
        /// Thực hiện Logic của các kỹ năng hỗ trợ kỹ năng khác
        /// </summary>
        public void ProcessEnchantSkills()
        {
            /// Làm rỗng danh sách cũ
            this.ClearEnchantSkills();

            /// Danh sách kỹ năng theo ID
            List<int> keys = this.listStudiedSkills.Keys.ToList();
            /// Duyệt toàn bộ danh sách
            foreach (int key in keys)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skill = this.GetSkillLevelRef(key);
                /// Toác gì đó
                if (skill == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu kỹ năng không phù hợp
                if (!this.IsValidSkill(skill))
                {
                    continue;
                }

                PropertyDictionary skillPd = skill.Properties;
                if (skillPd == null)
                {
                    continue;
                }

                /// Nếu tồn tại hỗ trợ kỹ năng khác
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_addenchant))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_addenchant).Clone();
                    if (attrib != null)
                    {
                        int enchantID = attrib.nValue[0];
                        int enchantSkillLevel = attrib.nValue[1];

                        EnchantSkill enchantSkill = KSkill.GetEnchantSkill(enchantID);
                        if (enchantSkill != null)
                        {
                            foreach (EnchantSkill.RelatedSkill relatedSkill in enchantSkill.RelatedSkills.Values)
                            {
                                if (!this.enchantSkills.TryGetValue(relatedSkill.ID, out _))
                                {
                                    this.enchantSkills[relatedSkill.ID] = new PropertyDictionary();
                                }
                                this.enchantSkills[relatedSkill.ID].AddProperties(relatedSkill.Properties[enchantSkillLevel].Clone());
                            }
                        }
                    }
                }

                /// Nếu tồn tại các thuộc tính hỗ trợ cho kỹ năng khác
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol, attrib);
                            }
                            /// Nếu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                    _attrib.nValue[2] += attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol, attrib);
                            }
                            /// Nếu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addrangewhencol);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                    _attrib.nValue[2] += attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] += attrib.nValue[1];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime);
                                if (_attrib != null)
								{
                                    _attrib.nValue[2] += attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                                if (_attrib != null)
								{
                                    _attrib.nValue[0] += attrib.nValue[0];
                                    _attrib.nValue[2] += attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] = attrib.nValue[1];
                                    _attrib.nValue[2] = attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] = attrib.nValue[1];
                                    _attrib.nValue[2] = attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill))
                {
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill).Clone();
                    if (attrib != null)
                    {
                        int skillID = attrib.nValue[0];
                        if (KSkill.IsSkillExist(skillID))
                        {
                            if (!this.enchantSkills.TryGetValue(skillID, out _))
                            {
                                this.enchantSkills[skillID] = new PropertyDictionary();
                            }

                            /// Nếu chưa tồn tại trong ProDict cũ thì thêm vào
                            if (!this.enchantSkills[skillID].ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill))
                            {
                                this.enchantSkills[skillID].Set<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill, attrib);
                            }
                            /// Néu đã tồn tại trong ProDict cũ thì tiến hành cộng giá trị
                            else
                            {
                                KMagicAttrib _attrib = this.enchantSkills[skillID].Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill);
                                if (_attrib != null)
								{
                                    _attrib.nValue[1] = attrib.nValue[1];
                                    _attrib.nValue[2] = attrib.nValue[2];
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trả về danh sách thuộc tính hỗ trợ kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public PropertyDictionary GetEnchantProperties(int skillID)
        {
            if (this.enchantSkills.TryGetValue(skillID, out PropertyDictionary pd))
            {
                return pd;
            }
            else
            {
                return null;
            }
        }
    }
}
