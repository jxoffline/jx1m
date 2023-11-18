using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic vòng sáng
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Thực hiện Logic của vòng sáng
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <param name="isChildCall"></param>
        public static void DoAruaLogic(GameObject caster, GameObject target, SkillLevelRef skill)
        {
            /// Nếu không phải kỹ năng dạng vòng sáng thì bỏ qua
            if (!skill.Data.IsArua)
            {
                return;
            }

            /// Nếu không phải gọi đến từ Timer của vòng sáng
            if (skill.Data.ParentAura == -1)
            {
                /// Thời gian Tick kiểm tra
                float tickTimeSec = 3.5f;

                #region Main Arua
                /// Thiết lập vòng sáng cho đối tượng
                caster.Buffs.SetArua(skill, 0f, null);
                #endregion

                /// ProDict kỹ năng
                PropertyDictionary skillPd = skill.Properties.Clone();
                /// Cộng toàn bộ thuộc tính hỗ trợ
                if (caster is KPlayer)
                {
                    KPlayer player = caster as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(skill.SkillID);
                    if (enchantPd != null)
                    {
                        skillPd.AddProperties(enchantPd);
                    }
                }


                /// Nếu tồn tại ProDict tương ứng
                if (skillPd != null)
                {
                    #region Child Arua 1
                    /// Vòng sáng con 1
                    SkillLevelRef childSkill_1 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 1
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_1 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_1 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_1 != null)
                        {
                            childSkill_1.AddedLevel = Math.Min(childSkill_1.Level, childAruaSkillMaxLevel);
                            childSkill_1.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 1
                    if (childSkill_1 != null && childSkill_1.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_1,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_1.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_1.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion

                    #region Child Arua 2
                    /// Vòng sáng con 2
                    SkillLevelRef childSkill_2 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 2
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill2))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill2).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill2).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_2 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_2 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_2 != null)
                        {
                            childSkill_2.AddedLevel = Math.Min(childSkill_2.Level, childAruaSkillMaxLevel);
                            childSkill_2.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 2
                    if (childSkill_2 != null && childSkill_2.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_2,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_2.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_2.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion

                    #region Child Arua 3
                    /// Vòng sáng con 3
                    SkillLevelRef childSkill_3 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 3
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill3))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill3).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill3).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_3 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_3 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_3 != null)
                        {
                            childSkill_3.AddedLevel = Math.Min(childSkill_3.Level, childAruaSkillMaxLevel);
                            childSkill_3.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 3
                    if (childSkill_3 != null && childSkill_3.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_3,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_3.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_3.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion

                    #region Child Arua 4
                    /// Vòng sáng con 4
                    SkillLevelRef childSkill_4 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 4
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill4))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill4).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill4).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_4 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_4 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_4 != null)
                        {
                            childSkill_4.AddedLevel = Math.Min(childSkill_4.Level, childAruaSkillMaxLevel);
                            childSkill_4.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 4
                    if (childSkill_4 != null && childSkill_4.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_4,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_4.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_4.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion

                    #region Child Arua 5
                    /// Vòng sáng con 5
                    SkillLevelRef childSkill_5 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 5
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill5))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill5).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill5).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_5 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_5 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_5 != null)
                        {
                            childSkill_5.AddedLevel = Math.Min(childSkill_5.Level, childAruaSkillMaxLevel);
                            childSkill_5.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 5
                    if (childSkill_5 != null && childSkill_5.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_5,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_5.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_5.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion

                    #region Child Arua 6
                    /// Vòng sáng con 6
                    SkillLevelRef childSkill_6 = null;
                    /// Nếu trong ProDict có tồn tại khai báo vòng sáng con 6
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_appendskill6))
                    {
                        int childAruaSkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill6).nValue[0];
                        int childAruaSkillMaxLevel = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_appendskill6).nValue[1];
                        /// Nếu là người chơi thì lấy kỹ năng hiện có
                        if (caster is KPlayer)
                        {
                            childSkill_6 = (caster as KPlayer).Skills.GetSkillLevelRef(childAruaSkillID)?.Clone();
                        }
                        /// Nếu là quái thì mặc định lấy kỹ năng cấp 1
                        else
                        {
                            SkillDataEx childSkillData = KSkill.GetSkillData(childAruaSkillID);
                            if (childSkillData != null)
                            {
                                childSkill_6 = new SkillLevelRef()
                                {
                                    AddedLevel = 1,
                                    BonusLevel = 0,
                                    CanStudy = false,
                                    Data = childSkillData,
                                };
                            }
                        }

                        /// Thiết lập cấp độ kỹ năng
                        if (childSkill_6 != null)
                        {
                            childSkill_6.AddedLevel = Math.Min(childSkill_6.Level, childAruaSkillMaxLevel);
                            childSkill_6.BonusLevel = 0;
                        }
                    }
                    /// Thêm vòng sáng con 6
                    if (childSkill_6 != null && childSkill_6.Level > 0)
                    {
                        /// Tạo mới vòng sáng tương ứng
                        BuffDataEx childArua = new BuffDataEx()
                        {
                            Duration = -1,
                            LoseWhenUsingSkill = false,
                            Skill = childSkill_6,
                            SaveToDB = false,
                            StartTick = KTGlobal.GetCurrentTimeMilis(),
                            CustomProperties = childSkill_6.Properties.Clone(),
                        };

                        /// Cộng toàn bộ thuộc tính hỗ trợ
                        if (caster is KPlayer)
                        {
                            KPlayer player = caster as KPlayer;
                            PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(childSkill_6.SkillID);
                            if (enchantPd != null)
                            {
                                childArua.CustomProperties.AddProperties(enchantPd);
                            }
                        }

                        /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                        if (childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && childArua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                        {
                            KMagicAttrib pMagicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                            KMagicAttrib magicAttrib = childArua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                            magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                            /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                            childArua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                        }

                        /// Thêm vòng sáng vào danh sách
                        caster.Buffs.AddArua(childArua, 0f, null);
                    }
                    #endregion
                }
            }
            /// Nếu là kỹ năng con gọi đến từ Timer của vòng sáng
            else if (skill.Data.TargetType != "self")
            {
                /// Phạm vi hiệu quả của vòng sáng
                int auraRange = 520;
                /// ID vòng sáng cha
                int parentAura = skill.Data.ParentAura;
                /// Nếu là người chơi
                if (caster is KPlayer)
                {
                    KPlayer player = caster as KPlayer;

                    /// Duyệt danh sách kỹ năng hỗ trợ xem có kỹ năng nào hỗ trợ tăng phạm vi vòng sáng không
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(parentAura);

                    if (enchantPd != null)
                    {
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange).nValue[1];
                            auraRange += value;
                        }
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange2).nValue[1];
                            auraRange += value;
                        }
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange3).nValue[1];
                            auraRange += value;
                        }
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange4).nValue[1];
                            auraRange += value;
                        }
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange5).nValue[1];
                            auraRange += value;
                        }
                        if (enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6))
                        {
                            int value = enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missileaaddition_addrange6).nValue[1];
                            auraRange += value;
                        }
                    }
                }


                /// Tạo mới Buff duy trì trong 5s
                BuffDataEx buff = new BuffDataEx()
                {
                    Duration = 8000,
                    LoseWhenUsingSkill = false,
                    Skill = skill,
                    StartTick = KTGlobal.GetCurrentTimeMilis(),
                    SaveToDB = false,
                    Tag = "Aura",
                    CustomProperties = skill.Properties.Clone(),
                };

                /// Cộng toàn bộ thuộc tính hỗ trợ
                if (caster is KPlayer)
                {
                    KPlayer player = caster as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(skill.SkillID);
                    if (enchantPd != null)
                    {
                        buff.CustomProperties.AddProperties(enchantPd);
                    }
                }

                /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                if (buff.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && buff.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                {
                    KMagicAttrib pMagicAttrib = buff.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                    KMagicAttrib magicAttrib = buff.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                    magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                    /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                    buff.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                }

                if (skill.Data.TargetType == "teamnoself")
                {
                    if (caster is KPlayer)
                    {
                        List<KPlayer> teammates = KTGlobal.GetNearByTeammates(caster as KPlayer, auraRange, 6);
                        foreach (KPlayer player in teammates)
                        {
                            if (player != caster)
                            {
                                player.Buffs.AddBuff(buff);
                            }
                        }
                    }
                }
                else if (skill.Data.TargetType == "team")
                {
                    if (caster is KPlayer)
                    {
                        List<KPlayer> teammates = KTGlobal.GetNearByTeammates(caster as KPlayer, auraRange, 6);
                        foreach (KPlayer player in teammates)
                        {
                            player.Buffs.AddBuff(buff);
                        }
                    }
                }
                else if (skill.Data.TargetType == "camp")
                {
                    List<GameObject> friends = KTGlobal.GetNearBySameCampObject(caster, auraRange, 6);
                    foreach (GameObject friend in friends)
                    {
                        friend.Buffs.AddBuff(buff);
                    }
                }
                else if (skill.Data.TargetType == "ally" || skill.Data.TargetType == "allyandnpc")
                {
                    if (caster is Monster)
                    {
                        List<Monster> friends = KTGlobal.GetNearBySameCampObject<Monster>(caster as Monster, auraRange, 6);
                        foreach (Monster friend in friends)
                        {
                            friend.Buffs.AddBuff(buff);
                        }
                    }
                    else if (caster is KPlayer)
                    {
                        List<KPlayer> friends = KTGlobal.GetNearBySameCampObject<KPlayer>(caster as KPlayer, auraRange, 6);
                        foreach (KPlayer friend in friends)
                        {
                            friend.Buffs.AddBuff(buff);
                        }
                    }
                }
                else if (skill.Data.TargetType == "allynoself" || skill.Data.TargetType == "npcteamnoself")
                {
                    if (caster is Monster)
                    {
                        List<Monster> friends = KTGlobal.GetNearBySameCampObject<Monster>(caster as Monster, auraRange, 6);
                        foreach (Monster friend in friends)
                        {
                            if (friend != caster)
                            {
                                friend.Buffs.AddBuff(buff);
                            }
                        }
                    }
                    else if (caster is KPlayer)
                    {
                        List<KPlayer> friends = KTGlobal.GetNearBySameCampObject<KPlayer>(caster as KPlayer, auraRange, 6);
                        foreach (KPlayer friend in friends)
                        {
                            if (friend != caster)
                            {
                                friend.Buffs.AddBuff(buff);
                            }
                        }
                    }
                }
                else if (skill.Data.TargetType == "enemy")
                {
                    List<GameObject> enemies = KTGlobal.GetNearByEnemies(caster, auraRange, 10);
                    foreach (GameObject enemy in enemies)
                    {
                        enemy.Buffs.AddBuff(buff);
                    }
                }
                else if (skill.Data.TargetType == "monstersnoself")
                {
                    List<Monster> monsters = KTGlobal.GetNearByObjects<Monster>(caster, auraRange, 10);
                    foreach (Monster monster in monsters)
                    {
                        if (monster != caster)
						{
                            monster.Buffs.AddBuff(buff);
                        }
                    }
                }
                else if (skill.Data.TargetType == "monsters")
                {
                    List<Monster> monsters = KTGlobal.GetNearByObjects<Monster>(caster, auraRange, 10);
                    foreach (Monster monster in monsters)
                    {
                        monster.Buffs.AddBuff(buff);
                    }
                }
            }

            /// Nếu có kỹ năng đi kèm mà chưa được kiểm tra
            SkillDataEx childSkill = KSkill.GetSkillData(skill.Data.StartSkillID);
            if (childSkill != null)
            {
                SkillLevelRef skillRef = new SkillLevelRef()
                {
                    Data = childSkill,
                    AddedLevel = skill.AddedLevel,
                    BonusLevel = skill.BonusLevel,
                    CanStudy = false,
                };

                /// Thực hiện kỹ năng con
                KTSkillManager.UseSkill_DoAction(caster, null, null, target, null, skillRef, true);
            }
        }

        /// <summary>
        /// Thực hiện Logic của vòng sáng tấn công
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <param name="isChildSkill"></param>
        /// <param name="auraStyle"></param>
        public static void DoOffensiveAruaLogic(GameObject caster, GameObject target, SkillLevelRef skill, bool isChildCall, string auraStyle)
        {
            /// Nếu chủ nhân vòng sáng đã tử vong
            if (caster.IsDead())
            {
                return;
            }
            /// Nếu kỹ năng không có ProDict
            else if (skill == null || skill.Properties == null)
			{
                return;
            }

            /// ProDict kỹ năng
            PropertyDictionary skillPd = skill.Properties.Clone();
            /// Cộng thêm từ kỹ năng hỗ trợ
            if (caster is KPlayer)
            {
                PropertyDictionary enchantPd = (caster as KPlayer).Skills.GetEnchantProperties(skill.SkillID);
                if (enchantPd != null)
                {
                    skillPd.AddProperties(enchantPd);
                }
            }

            /// Nếu không phải gọi đến từ Timer của vòng sáng
            if (!isChildCall)
            {
                /// Nếu là vòng sáng vừa Buff cho bản thân vừa tung bùa lên kẻ địch
                if (auraStyle == "aurarangebuffselfandcurseenemy")
                {
                    /// Thực hiện Buff lên bản thân
                    KTSkillManager.DoSkillSingleTargetBuff(caster, null, -1, skill, skillPd);
                }

                SkillDataEx startSkillData = KSkill.GetSkillData(skill.Data.StartSkillID);
                SkillLevelRef startSkill = null;
                if (startSkillData != null)
                {
                    startSkill = new SkillLevelRef()
                    {
                        Data = startSkillData,
                        AddedLevel = skill.Level,
                        BonusLevel = 0,
                        CanStudy = false,
                    };
                }

                /// Thời gian Tick kiểm tra
                float tickTimeSec = 2f;

                #region Main Arua
                /// Thiết lập vòng sáng cho đối tượng
                caster.Buffs.SetArua(skill, tickTimeSec, () => {
                    KTSkillManager.DoOffensiveAruaLogic(caster, target, startSkill ?? skill, true, auraStyle);
                });
                #endregion
            }
            else
            {
                int targetCount = -1;
                /// Nếu trong ProDict chứa số mục tiêu ảnh hưởng tối đa
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_hitcount))
                {
                    targetCount = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_hitcount).nValue[0];
                }

                if (skill.Data.TargetType == "enemy")
                {
                    List<GameObject> enemies = KTGlobal.GetNearByEnemies(caster, 520, targetCount);
                    int enemiesCount = 0;
                    foreach (GameObject enemy in enemies)
                    {
                        /// Nếu là chiêu đánh
                        if (auraStyle == "aurarangemagicattack")
                        {
                            /// Nếu kẻ địch đã chết
                            if (enemy.IsDead())
                            {
                                continue;
                            }

                            /// Toác cái gì đó
                            if (caster == null || enemy == null || skill == null)
							{
                                continue;
							}

                            bool attack = AlgorithmProperty.AttackEnemy(caster, enemy, skill, 0, new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y), true);
                            if (!attack)
                            {
                                /// Gửi gói tin sát thương về Client
                                KTSkillManager.AppendSkillResult(caster, enemy, SkillResult.Adjust, 0);

                                /// Thông báo đối tượng vừa tấn công
                                enemy.TakeDamage(caster, 0);
                            }
                        }
                        /// Nếu là chiêu hiệu ứng bùa chú
                        else if (auraStyle == "aurarangecurse" || auraStyle == "aurarangebuffselfandcurseenemy")
                        {
                            /// Thời gian duy trì
                            float duration = 0;
                            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_statetime))
                            {
                                if (skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_statetime).nValue[0] == -1)
                                {
                                    duration = -1;
                                }
                                else
                                {
                                    duration = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_statetime).nValue[0] / 18f;
                                }
                            }

                            /// Thực hiện Buff lên kẻ địch
                            KTSkillManager.DoSkillSingleTargetBuff(caster, enemy, duration, skill, skillPd);

                            /// Thông báo đối tượng vừa tấn công
                            enemy.TakeDamage(caster, 0);
                        }

                        /// Tăng số mục tiêu đã tấn công lên
                        enemiesCount++;
                        /// Nếu đã tấn công đủ số mục tiêu thì thoát
                        if (targetCount != -1 && enemiesCount >= targetCount)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện Logic vòng sáng
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="isChildSkill">Có phải kỹ năng con không</param>
        private static void DoSkillArua(GameObject caster, GameObject target, SkillLevelRef skill, bool isChildSkill)
        {
            /// Vòng sáng tấn công đối tượng xung quanh
            if (skill.Data.SkillStyle == "aurarangemagicattack" || skill.Data.SkillStyle == "aurarangebuffselfandcurseenemy" || skill.Data.SkillStyle == "aurarangecurse")
            {
                KTSkillManager.DoOffensiveAruaLogic(caster, target, skill, false, skill.Data.SkillStyle);
            }
            else
            {
                /// Thực hiện Logic vòng sáng
                KTSkillManager.DoAruaLogic(caster, target, skill);
            }
        }
    }
}
