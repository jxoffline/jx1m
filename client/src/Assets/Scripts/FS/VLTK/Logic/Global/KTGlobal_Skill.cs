using FS.GameEngine.Logic;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.SuperToolTip;
using GameServer.VLTK.Utilities;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FS.VLTK.Entities.Config.EnchantSkill;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Skill

        /// <summary>
        /// ID kỹ năng khinh công
        /// </summary>
        public const int FlySkillID = 10;

        /// <summary>
        /// Danh sách kỹ năng tấn công tân thủ
        /// </summary>
        public static readonly HashSet<int> ListNewbieAttackSkill = new HashSet<int>()
        {
            14000, 14001
        };

        /// <summary>
        /// ID kỹ năng tân thủ đánh quyền
        /// </summary>
        public static readonly int NewbieHandAttackSkill = 14000;

        /// <summary>
        /// Trả về ID kỹ năng tân thủ tương ứng vũ khí đang cầm
        /// </summary>
        /// <returns></returns>
        public static SkillDataEx GetNewbieSkillCorrespondingToCurrentWeapon()
        {
            /// Vũ khí tương ứng
            GoodsData weaponGD = KTGlobal.GetEquipData(Global.Data.RoleData, KE_EQUIP_POSITION.emEQUIPPOS_WEAPON);
            /// Nếu không có vũ khí
            if (weaponGD == null)
            {
                /// Trả ra tay không/triền thủ
                return null;
            }
            /// Thông tin vũ khí tương ứng
            ItemData weaponData = null;
            /// Nếu không có thông tin vũ khí tương ứng
            if (!Loader.Loader.Items.TryGetValue(weaponGD.GoodsID, out weaponData))
            {
                return null;
            }

            /// Duyệt danh sách kỹ năng tân thủ
            foreach (int skillID in KTGlobal.ListNewbieAttackSkill)
            {
                /// Thông tin kỹ năng tương ứng
                if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                {
                    /// Nếu trùng loại vũ khí yêu cầu
                    if (skillData.WeaponLimit.Contains(0) || skillData.WeaponLimit.Contains(weaponData.Category))
                    {
                        return skillData;
                    }
                }
            }

            /// Không tìm thấy trả ra NULL
            return null;
        }

        /// <summary>
        /// Kiểm tra kỹ năng tương ứng có dùng được không
        /// </summary>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public static bool IsCanUseSkill(SkillDataEx skillData)
        {
            /// Nếu là kỹ năng bị động
            if (skillData.Type == 3)
            {
                return false;
            }

            /// Nếu là các kỹ năng tân thủ theo vũ khí
            if (KTGlobal.ListNewbieAttackSkill.Contains(skillData.ID))
            {
                /// Kiểm tra loại vũ khí
                if (!skillData.WeaponLimit.Contains((int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL))
                {
                    GoodsData weapon = KTGlobal.GetEquipData(Global.Data.RoleData, KE_EQUIP_POSITION.emEQUIPPOS_WEAPON);
                    /// Nếu vũ khí yêu cầu là triền thủ
                    if (skillData.WeaponLimit.Contains((int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND))
                    {
                        /// Nếu có vũ khí
                        if (weapon != null)
                        {
                            if (Loader.Loader.Items.TryGetValue(weapon.GoodsID, out ItemData itemData))
                            {
                                /// Nếu vũ khí không phải triền thủ
                                if (!skillData.WeaponLimit.Contains(itemData.Category))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    /// Nếu vũ khí yêu cầu không phải triền thủ
                    else
                    {
                        if (weapon == null)
                        {
                            return false;
                        }
                        else
                        {
                            if (Loader.Loader.Items.TryGetValue(weapon.GoodsID, out ItemData itemData))
                            {
                                /// Nếu vũ khí không phải triền thủ
                                if (!skillData.WeaponLimit.Contains(itemData.Category))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            /// Kiểm tra môn phái có phù hợp không
            if (skillData.FactionID != 0)
            {
                /// Nếu môn phái không phù hợp
                if (Global.Data.RoleData.FactionID != skillData.FactionID)
                {
                    return false;
                }
                /// Nếu nhánh không phù hợp
                else if (skillData.SubID != 0 && Global.Data.RoleData.SubID != skillData.SubID)
                {
                    return false;
                }
            }

            /// Trả ra kết quả có thể dùng kỹ năng
            return true;
        }

        #endregion Skill

        #region Skill Super ToolTip

        /// <summary>
        /// Trả về tên trạng thái ngũ hành tương ứng
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static string GetSeriesStateText(KE_STATE state)
        {
            switch (state)
            {
                case KE_STATE.emSTATE_BURN:
                    {
                        return "Bỏng";
                    }
                case KE_STATE.emSTATE_CONFUSE:
                    {
                        return "Hỗn loạn";
                    }
                case KE_STATE.emSTATE_DRAG:
                    {
                        return "Kéo lại";
                    }
                case KE_STATE.emSTATE_FIXED:
                    {
                        return "Bất động";
                    }
                case KE_STATE.emSTATE_FLOAT:
                    {
                        return "Hút lên cao";
                    }
                case KE_STATE.emSTATE_FREEZE:
                    {
                        return "Đóng băng";
                    }
                case KE_STATE.emSTATE_HURT:
                    {
                        return "Thọ thương";
                    }
                case KE_STATE.emSTATE_KNOCK:
                    {
                        return "Đẩy lui";
                    }
                case KE_STATE.emSTATE_PALSY:
                    {
                        return "Tê liệt";
                    }
                case KE_STATE.emSTATE_POISON:
                    {
                        return "Trúng độc";
                    }
                case KE_STATE.emSTATE_SLOWALL:
                    {
                        return "Làm chậm";
                    }
                case KE_STATE.emSTATE_SLOWRUN:
                    {
                        return "Giảm tốc";
                    }
                case KE_STATE.emSTATE_STUN:
                    {
                        return "Choáng";
                    }
                case KE_STATE.emSTATE_WEAK:
                    {
                        return "Suy yếu";
                    }
            }
            return "";
        }

        /// <summary>
        /// Trả về Text ngũ hành tương ứng
        /// </summary>
        /// <param name="seriesID"></param>
        /// <returns></returns>
        private static string GetSeriesText(int seriesID)
        {
            switch (seriesID)
            {
                case 1:
                    return string.Format("<color={0}>{1}</color>", "#ffe552", Loader.Loader.Elements[Elemental.METAL].Name);

                case 2:
                    return string.Format("<color={0}>{1}</color>", "#77ff33", Loader.Loader.Elements[Elemental.WOOD].Name);

                case 3:
                    return string.Format("<color={0}>{1}</color>", "#61d7ff", Loader.Loader.Elements[Elemental.WATER].Name);

                case 4:
                    return string.Format("<color={0}>{1}</color>", "#ff4242", Loader.Loader.Elements[Elemental.FIRE].Name);

                case 5:
                    return string.Format("<color={0}>{1}</color>", "#debba0", Loader.Loader.Elements[Elemental.EARTH].Name);

                default:
                    return string.Format("<color={0}>{1}</color>", "#cccccc", "Vô");
            }
        }

        /// <summary>
        /// Trả về mô tả kỹ năng tự động
        /// </summary>
        /// <param name="autoSkillID"></param>
        /// <param name="autoSkillLevel"></param>
        /// <returns></returns>
        private static string GetAutoSkillDesc(int autoSkillID, int autoSkillLevel)
        {
            if (autoSkillLevel > 0 && Loader.Loader.AutoSkills.TryGetValue(autoSkillID, out AutoSkill autoSkill))
            {
                StringBuilder builder = new StringBuilder();

                int castPercent = autoSkill.CastPercentByLevel[autoSkillLevel].Value;
                int delayPerCast = autoSkill.DelayPerCastByLevel[autoSkillLevel].Value / 18;
                int castSkillID = autoSkill.CastSkillID;
                int castSkillLevel = autoSkill.CastSkillLevelByLevel[autoSkillLevel].Value;
                int maxCastCount = autoSkill.MaxCastCountByLevel[autoSkillLevel].Value;
                if (castSkillLevel == -1)
                {
                    castSkillLevel = autoSkillLevel;
                }
                if (!Loader.Loader.Skills.TryGetValue(castSkillID, out SkillDataEx castSkill))
                {
                    return null;
                }
                string castSkillDesc = KTGlobal.AnalysisPropertyInfo(castSkill, castSkill.Properties[(byte)castSkillLevel], castSkillLevel);

                /// Kích hoạt nếu mất hiệu ứng
                if (autoSkill.ActivateIfLostBuff != -1)
                {
                    if (!Loader.Loader.Skills.TryGetValue(autoSkill.ActivateIfLostBuff, out SkillDataEx buffSkill))
                    {
                        return null;
                    }
                    builder.AppendFormat("Khi mất trạng thái <color=#ffd429>[{0}]</color>, có tỷ lệ <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", buffSkill.Name, castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu bị đánh trúng hoặc đánh trúng
                else if (autoSkill.ActivateWhenBeHit && autoSkill.ActivateIfHitTarget)
                {
                    builder.AppendFormat("Khi bị đánh trúng hoặc bị đánh trúng có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu đánh trúng mục tiêu
                else if (autoSkill.ActivateIfHitTarget)
                {
                    builder.AppendFormat("Đánh trúng mục tiêu có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt khi sinh lực giảm xuống dưới ngưỡng
                else if (autoSkill.ActivateWhenHPPercentDropBelow != -1)
                {
                    builder.AppendFormat("Khi sinh lực giảm xuống dưới <color=#ffd429>{0}%</color>, có tỷ lệ <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", autoSkill.ActivateWhenHPPercentDropBelow, castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }

                /// Kích hoạt nếu bị đánh trúng
                else if (autoSkill.ActivateWhenBeHit)
                {
                    builder.AppendFormat("Khi bị đánh trúng có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu đánh chí mạng
                else if (autoSkill.ActivateWhenDoCritHit)
                {
                    builder.AppendFormat("Khi đánh chí mạng có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                else if (autoSkill.ActivateWhenBeCritHit)
                {
                    builder.AppendFormat("Khi bị đánh chí mạng có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu trọng thương
                else if (autoSkill.ActivateAfterDie)
                {
                    builder.AppendFormat("Khi bị trọng thương có tỷ lệ <color=#ffd429>{0}%</color> kích hoạt:\n<color=#42ff29>{1} (Cấp {2})</color>\n{3}\n", castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu đánh chí mạng đạt số lần
                else if (autoSkill.ActivateAfterDoTotalCritHit > 0)
                {
                    builder.AppendFormat("Khi đánh chí mạng tổng cộng {0} lần, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", autoSkill.ActivateAfterDoTotalCritHit, castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu bị đánh trúng bởi một trong số các kỹ năng
                else if (autoSkill.ActivateWhenBeHitBySkills.Count > 0)
                {
                    List<string> skillNames = new List<string>();
                    foreach (int skillID in autoSkill.ActivateWhenBeHitBySkills)
                    {
                        if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                        {
                            skillNames.Add(string.Format("<color=yellow>[{0}]</color>", skillData.Name));
                        }
                    }
                    string skillsDesc = string.Join(", ", skillNames);

                    builder.AppendFormat("Khi bị đánh trúng bởi một trong các kỹ năng {0}, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", skillsDesc, castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt khi đánh trúng mục tiêu bằng một trong số các kỹ năng
                else if (autoSkill.ActivateWhenHitWithSkills.Count > 0)
                {
                    List<string> skillNames = new List<string>();
                    foreach (int skillID in autoSkill.ActivateWhenHitWithSkills)
                    {
                        if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                        {
                            skillNames.Add(string.Format("<color=yellow>[{0}]</color>", skillData.Name));
                        }
                    }
                    string skillsDesc = string.Join(", ", skillNames);

                    builder.AppendFormat("Khi đánh trúng mục tiêu bằng một trong các kỹ năng {0}, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", skillsDesc, castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu không sử dụng kỹ năng trong khoảng
                else if (autoSkill.ActivateIfNoUseSkillForFrame != -1)
                {
                    builder.AppendFormat("Nếu không dùng kỹ năng trong vòng {0} giây, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", Utils.Truncate(autoSkill.ActivateIfNoUseSkillForFrame / 18f, 1), castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt nếu không sử dụng kỹ năng làm mất trạng thái tàng hình trong khoảng
                else if (autoSkill.ActivateIfNoUseSkillCauseLostInvisibilityForFrame != -1)
                {
                    builder.AppendFormat("Nếu không dùng kỹ năng làm mất trạng thái ẩn thân trong vòng {0} giây, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", Utils.Truncate(autoSkill.ActivateIfNoUseSkillCauseLostInvisibilityForFrame / 18f, 1), castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }
                /// Kích hoạt sau mỗi khoảng
                else if (autoSkill.ActivateEachFrame != -1)
                {
                    builder.AppendFormat("Sau mỗi {0} giây, có <color=#ffd429>{1}%</color> kích hoạt:\n<color=#42ff29>{2} (Cấp {3})</color>\n{4}\n", Utils.Truncate(autoSkill.ActivateEachFrame / 18f, 1), castPercent, castSkill.Name, castSkillLevel, castSkillDesc);
                    if (delayPerCast > 0)
                    {
                        builder.AppendFormat("Giãn cách kích hoạt liên tục: <color=#ffd429>{0} giây</color>\n", delayPerCast);
                    }
                    if (maxCastCount > 0)
                    {
                        builder.AppendLine(string.Format("Kích hoạt tối đa: <color=#ffd429>{0} lần</color>", maxCastCount));
                    }
                }

                return builder.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Trả về danh sách thuộc tính được hỗ trợ từ kỹ năng khác
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private static PropertyDictionary GetEnchantProperties(SkillDataEx skill)
        {
            PropertyDictionary supportPd = new PropertyDictionary();

            foreach (Server.Data.SkillData skillData in Global.Data.RoleData.SkillDataList)
            {
                if (skillData.Level > 0 && Loader.Loader.Skills.TryGetValue(skillData.SkillID, out SkillDataEx _skill))
                {
                    /// Nếu môn phái không phù hợp
                    if (_skill.FactionID != 0 && _skill.FactionID != Global.Data.RoleData.FactionID)
                    {
                        continue;
                    }
                    /// Nếu nhánh không phf hợp
                    else if (_skill.SubID != 0 && _skill.SubID != Global.Data.RoleData.SubID)
                    {
                        continue;
                    }

                    PropertyDictionary skillPd = _skill.Properties[skillData.Level];
                    if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_addenchant))
                    {
                        KMagicAttrib attrib = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_addenchant);
                        int enchantID = attrib.nValue[0];
                        int enchantSkillLevel = attrib.nValue[1];
                        if (enchantSkillLevel > 0 && Loader.Loader.EnchantSkills.TryGetValue(enchantID, out EnchantSkill enchantSkill))
                        {
                            if (enchantSkill.RelatedSkills.TryGetValue(skill.ID, out RelatedSkill relatedSkill))
                            {
                                supportPd.AddProperties(relatedSkill.Properties[enchantSkillLevel]);
                            }
                        }
                    }
                }
            }

            return supportPd;
        }

        /// <summary>
        /// Trả về tổng số sát thương nhận được từ kỹ năng khác
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private static int GetTotalAppendDamageByOtherSkills(SkillDataEx skill)
        {
            int totalEnchantDamagePercent = 0;
            foreach (Server.Data.SkillData skillData in Global.Data.RoleData.SkillDataList)
            {
                if (skillData.Level > 0 && Loader.Loader.Skills.TryGetValue(skillData.SkillID, out SkillDataEx _skill))
                {
                    /// Nếu môn phái không phù hợp
                    if (_skill.FactionID != 0 && _skill.FactionID != Global.Data.RoleData.FactionID)
                    {
                        continue;
                    }
                    /// Nếu nhánh không phf hợp
                    else if (_skill.SubID != 0 && _skill.SubID != Global.Data.RoleData.SubID)
                    {
                        continue;
                    }

                    PropertyDictionary _skillPd = _skill.Properties[skillData.Level];
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                    if (_skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6))
                    {
                        KMagicAttrib attrib = _skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6);
                        int enchantID = attrib.nValue[0];
                        int enchantDamagePercent = attrib.nValue[1];
                        if (enchantID == skill.ID)
                        {
                            totalEnchantDamagePercent += enchantDamagePercent;
                        }
                    }
                }
            }
            return totalEnchantDamagePercent;
        }

        /// <summary>
        /// Phân tích dữ liệu kỹ năng dựa theo ProDict
        /// <param name="skill"></param>
        /// <param name="pd"></param>
        /// <param name="level"></param>
        /// <param name="isChildSkill"></param>
        /// </summary>
        private static string AnalysisPropertyInfo(SkillDataEx skill, PropertyDictionary pd, int level, bool isChildSkill = false, bool isSupportSkill = false)
        {
            StringBuilder builder = new StringBuilder();

            /// Tạo bản sao của ProDict tương ứng
            pd = pd.Clone();
            if (!isSupportSkill)
            {
                /// Cộng thêm các kỹ năng hỗ trợ khác
                pd.AddProperties(KTGlobal.GetEnchantProperties(skill));
            }

            /// Danh sách các thuộc tính của ProDict
            Dictionary<short, object> propertyDictionary = pd.Clone().GetDictionary();
            /// Nội lực mất
            if (!isChildSkill && propertyDictionary.ContainsKey((int)MAGIC_ATTRIB.magic_skill_cost_v))
            {
                KMagicAttrib attrib = propertyDictionary[(int)MAGIC_ATTRIB.magic_skill_cost_v] as KMagicAttrib;
                if (attrib != null)
                {
                    /// Nếu kỹ năng tiêu hao sinh lực
                    if (skill.SkillCostType == 0)
                    {
                        builder.AppendLine(string.Format("<color=#3d9eff>Sinh lực mất:</color> <color=#ffff1a>{0}</color>", attrib.nValue[0]));
                    }
                    /// Nếu kỹ năng tiêu hao nội lực
                    else if (skill.SkillCostType == 1)
                    {
                        builder.AppendLine(string.Format("<color=#3d9eff>Nội lực mất:</color> <color=#ffff1a>{0}</color>", attrib.nValue[0]));
                    }
                    /// Nếu kỹ năng tiêu hao thể lực
                    else if (skill.SkillCostType == 2)
                    {
                        builder.AppendLine(string.Format("<color=#3d9eff>Thể lực mất:</color> <color=#ffff1a>{0}</color>", attrib.nValue[0]));
                    }
                }
            }
            if (!isChildSkill)
            {
                if (propertyDictionary.ContainsKey((int)MAGIC_ATTRIB.magic_skill_attackradius))
                {
                    KMagicAttrib attrib = propertyDictionary[(int)MAGIC_ATTRIB.magic_skill_attackradius] as KMagicAttrib;
                    if (attrib != null && attrib.nValue[0] > 0)
                    {
                        builder.AppendLine(string.Format("<color=#3d9eff>" + PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_attackradius].Description + "</color>", attrib.nValue[0]));
                    }
                }
                else if (skill.AttackRadius > 0)
                {
                    builder.AppendLine(string.Format("<color=#3d9eff>" + PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_attackradius].Description + "</color>", skill.AttackRadius));
                }
            }

            EnchantSkill enchantSkill = null;
            int enchantSkillLevel = 0;
            SkillDataEx startSkill = null;
            int startSkillRate = 100;
            int collideSkillRate = 100;
            int vanishSkillRate = 100;
            int flySkillRate = 100;
            SkillDataEx flySkill = null;
            SkillDataEx vanishSkill = null;
            SkillDataEx collidSkill = null;

            if (skill.StartSkillID > 0)
            {
                Loader.Loader.Skills.TryGetValue(skill.StartSkillID, out startSkill);
            }
            if (skill.VanishSkillID > 0)
            {
                Loader.Loader.Skills.TryGetValue(skill.VanishSkillID, out vanishSkill);
            }
            if (skill.FinishSkillID > 0)
            {
                Loader.Loader.Skills.TryGetValue(skill.FinishSkillID, out collidSkill);
            }

            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill1Property = null;
            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill2Property = null;
            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill3Property = null;
            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill4Property = null;
            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill5Property = null;
            KeyValuePair<SkillDataEx, KMagicAttrib>? appendSkill6Property = null;

            /// Hỗ trợ kỹ năng (symbol dạng magic_skilladdition)
            Dictionary<int, List<string>> skillAdditions = new Dictionary<int, List<string>>();

            float cooldownOnHorse = 0f, cooldown = 0f;
            int autoSkillID = -1, autoSkillLevel = -1;

            foreach (KeyValuePair<short, object> record in propertyDictionary)
            {
                KMagicAttrib attrib = record.Value as KMagicAttrib;
                if (attrib == null)
                {
                    continue;
                }
                switch (record.Key)
                {
                    case (int)MAGIC_ATTRIB.magic_missile_hitcount:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missile_hitcount].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_statetime:
                        if (!isChildSkill && attrib.nValue[0] > 0)
                        {
                            builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_statetime].Description, Utils.Truncate(attrib.nValue[0] / 18f, 1)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_mintimepercast_v:
                        cooldown = Utils.Truncate(attrib.nValue[0] / 18f, 1);
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v:
                        cooldownOnHorse = Utils.Truncate(attrib.nValue[0] / 18f, 1);
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_maxmissile:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_maxmissile].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missile_lifetime_v:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missile_lifetime_v].Description, Utils.Truncate(attrib.nValue[0] / 18f, 1)));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_startevent:
                        Loader.Loader.Skills.TryGetValue(attrib.nValue[2], out startSkill);
                        startSkillRate = attrib.nValue[1];
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_flyevent:
                        Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out flySkill);
                        flySkillRate = attrib.nValue[1];
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_vanishedevent:
                        Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out vanishSkill);
                        vanishSkillRate = attrib.nValue[1];
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_collideevent:
                        Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out collidSkill);
                        collideSkillRate = attrib.nValue[1];
                        break;

                    case (int)MAGIC_ATTRIB.magic_addenchant:
                        int enchantID = attrib.nValue[0];
                        enchantSkillLevel = attrib.nValue[1];
                        Loader.Loader.EnchantSkills.TryGetValue(enchantID, out enchantSkill);
                        break;

                    case (int)MAGIC_ATTRIB.magic_autoskill:
                        autoSkillID = attrib.nValue[0];
                        autoSkillLevel = attrib.nValue[1];
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill))
                        {
                            appendSkill1Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill2))
                        {
                            appendSkill2Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill2, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill3))
                        {
                            appendSkill3Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill3, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill4))
                        {
                            appendSkill4Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill4, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill5))
                        {
                            appendSkill5Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill5, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx addSkill6))
                        {
                            appendSkill6Property = new KeyValuePair<SkillDataEx, KMagicAttrib>(addSkill6, attrib);
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange2:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange2].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange3:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange3].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange4:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange4].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange5:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange5].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_missileaaddition_addrange6:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missileaaddition_addrange6].Description, attrib.nValue[1]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addmissilethroughrate:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addmissilethroughrate].Description, attrib.nValue[1], attrib.nValue[2]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol].Description, attrib.nValue[1], attrib.nValue[2]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addrangewhencol:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addrangewhencol].Description, attrib.nValue[1], attrib.nValue[2]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addstartskill:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[1], out SkillDataEx startSkillData) && Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx fromSkillData1))
                        {
                            skillAdditions[attrib.nValue[0]].Add(string.Format("Khi thi triển <color=yellow>[{0}]</color> đồng thời thi triển <color=yellow>[{1}]</color>\n{2}", fromSkillData1.Name, startSkillData.Name, KTGlobal.AnalysisPropertyInfo(startSkillData, startSkillData.Properties[(byte)attrib.nValue[2]], attrib.nValue[2], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addflyskill:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx flySkillData) && Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx fromSkillData2))
                        {
                            skillAdditions[attrib.nValue[0]].Add(string.Format("Khi thi triển <color=yellow>[{0}]</color> đồng thời thi triển <color=yellow>[{1}]</color>\n{2}", fromSkillData2.Name, flySkillData.Name, KTGlobal.AnalysisPropertyInfo(flySkillData, flySkillData.Properties[(byte)attrib.nValue[2]], attrib.nValue[2], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addvanishskill:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx vanishSkillData) && Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx fromSkillData3))
                        {
                            skillAdditions[attrib.nValue[0]].Add(string.Format("Khi thi triển <color=yellow>[{0}]</color> đồng thời thi triển <color=yellow>[{1}]</color>\n{2}", fromSkillData3.Name, vanishSkillData.Name, KTGlobal.AnalysisPropertyInfo(vanishSkillData, vanishSkillData.Properties[(byte)attrib.nValue[2]], attrib.nValue[2], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime].Description, Utils.Truncate(attrib.nValue[2] / 18f, 1)));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilladdition_addmissilenum:
                        if (!skillAdditions.ContainsKey(attrib.nValue[0]))
                        {
                            skillAdditions[attrib.nValue[0]] = new List<string>();
                        }
                        skillAdditions[attrib.nValue[0]].Add(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addmissilenum].Description, attrib.nValue[1], attrib.nValue[2]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_attackradius:
                        if (isSupportSkill)
                        {
                            builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_attackradius].Description, attrib.nValue[0]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_addedwithenemycount:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addedwithenemycount].Description, attrib.nValue[2], attrib.nValue[1]));

                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx skillAddedWithEnemy))
                        {
                            builder.AppendLine(KTGlobal.AnalysisPropertyInfo(skill, skillAddedWithEnemy.Properties[(byte)level], level, true));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_missile_ablility:
                        if (isChildSkill && isSupportSkill)
                        {
                            builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missile_ablility].Description, attrib.nValue[0]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_missile_speed_v:
                        if (isChildSkill && isSupportSkill)
                        {
                            builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missile_speed_v].Description, attrib.nValue[0]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_missile_range:
                        if (isChildSkill && isSupportSkill)
                        {
                            builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_missile_range].Description, attrib.nValue[2]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_param1_v:
                        if (skill.RawPropertiesConfig == "tangmen120")
                        {
                            builder.AppendLine(string.Format("Cự ly di chuyển tối đa: <color=#ffff1a>{0}</color>", pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_param1_v).nValue[0]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r].Description, attrib.nValue[0], KTGlobal.GetSeriesText(skill.ElementalSeries)));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill1))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill1.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill1, appendSkill1.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill2:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill2))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill2.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill2, appendSkill2.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill3:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill3))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill3.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill3, appendSkill3.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill4:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill4))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill4.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill4, appendSkill4.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill5:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill5))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill5.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill5, appendSkill5.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_appendskill6:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx appendSkill6))
                        {
                            builder.AppendLine(string.Format("<color=#0cf500>[{0} (Cấp {1})]</color>\n{2}", appendSkill6.Name, attrib.nValue[1], KTGlobal.AnalysisPropertyInfo(appendSkill6, appendSkill6.Properties[(byte)attrib.nValue[1]], attrib.nValue[1], true, false)));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_immune_skill_1:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_2:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_3:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_4:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_5:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_6:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_7:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_8:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_9:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_10:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_11:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_12:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_13:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_14:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_15:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_16:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_17:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_18:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_19:
                    case (int)MAGIC_ATTRIB.magic_immune_skill_20:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx immuneToSkill))
                        {
                            builder.AppendLine(string.Format("Xác suất <color=#ffff1a>{1}%</color> vô hiệu hóa kỹ năng <color=#0cf500>[{0}]</color>", immuneToSkill.Name, attrib.nValue[1]));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_reduce_near_magic_damage:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_reduce_near_magic_damage].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_reduce_far_magic_damage:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_reduce_far_magic_damage].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_reduce_near_physic_damage:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_reduce_near_physic_damage].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_reduce_far_physic_damage:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_reduce_far_physic_damage].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_manatoskill_enhance:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manatoskill_enhance].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addfiredamage_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiredamage_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addcolddamage_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcolddamage_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addlightingdamage_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingdamage_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addpoisondamage_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisondamage_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addcoldmagic_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcoldmagic_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addfiremagic_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiremagic_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addlightingmagic_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingmagic_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addpoisonmagic_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisonmagic_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_create_illusion:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_create_illusion].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_damage_inc_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_inc_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_redeivedamage_dec_p2:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_redeivedamage_dec_p2].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_ban_skill_1:
                    case (int)MAGIC_ATTRIB.magic_ban_skill_2:
                    case (int)MAGIC_ATTRIB.magic_ban_skill_3:
                    case (int)MAGIC_ATTRIB.magic_ban_skill_4:
                    case (int)MAGIC_ATTRIB.magic_ban_skill_5:
                    case (int)MAGIC_ATTRIB.magic_ban_skill_6:
                        if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx BandSKilLName))
                        {
                            builder.AppendLine(string.Format("Sau khi sử dụng vô hiệu hóa kỹ năng <color=#0cf500>[{0}]</color>", BandSKilLName.Name));
                        }
                        break;

                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff1layers_v:
                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff2layers_v:
                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff3layers_v:
                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff4layers_v:
                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff5layers_v:
                    case (int)MAGIC_ATTRIB.magic_skill_cost_buff6layers_v:

                        if (!Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx buffSkill))
                        {
                            return null;
                        }

                        builder.AppendLine("Yêu cầu phải có hiệu ứng [" + buffSkill.Name + "] mới có thể kích hoạt kỹ năng!");
                        break;

                    case (int)MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_fastlifereplenish_byvitality:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastlifereplenish_byvitality].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_skilldamageptrimbylesshp:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilldamageptrimbylesshp].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_immediatereplbymaxstate_p:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_immediatereplbymaxstate_p].Description, attrib.nValue[0]));
                        break;

                    case (int)MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality:
                        builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality].Description, attrib.nValue[0]));
                        break;

                    default:
                        string str = KTGlobal.GetAttributeDescription(attrib);
                        if (!string.IsNullOrEmpty(str))
                        {
                            builder.AppendLine(str);
                        }
                        break;
                }
            }

            /// Tổng số % sát thương được cộng thêm từ các kỹ năng hỗ trợ
            int totalEnchantDamagePercent = KTGlobal.GetTotalAppendDamageByOtherSkills(skill);
            if (!isChildSkill && totalEnchantDamagePercent > 0)
            {
                builder.AppendLine(string.Format("Sát thương cộng thêm từ các kỹ năng hỗ trợ: <color=#ffff1a>{0}%</color>", totalEnchantDamagePercent));
            }

            /// Thời gian giãn cách thi triển
            if (Math.Abs(cooldown) > 0)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_mintimepercast_v].Description, cooldown));
            }
            if (Math.Abs(cooldownOnHorse) > 0 && cooldown != cooldownOnHorse)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v].Description, cooldownOnHorse));
            }

            if (!isChildSkill && appendSkill1Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent].Description, appendSkill1Property.Value.Key.Name, appendSkill1Property.Value.Value.nValue[1], appendSkill1Property.Value.Value.nValue[2]));
            }
            if (!isChildSkill && appendSkill2Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2].Description, appendSkill2Property.Value.Key.Name, appendSkill2Property.Value.Value.nValue[1], appendSkill2Property.Value.Value.nValue[2]));
            }
            if (!isChildSkill && appendSkill3Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3].Description, appendSkill3Property.Value.Key.Name, appendSkill3Property.Value.Value.nValue[1], appendSkill3Property.Value.Value.nValue[2]));
            }
            if (!isChildSkill && appendSkill4Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4].Description, appendSkill4Property.Value.Key.Name, appendSkill4Property.Value.Value.nValue[1], appendSkill4Property.Value.Value.nValue[2]));
            }
            if (!isChildSkill && appendSkill5Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5].Description, appendSkill5Property.Value.Key.Name, appendSkill5Property.Value.Value.nValue[1], appendSkill5Property.Value.Value.nValue[2]));
            }
            if (!isChildSkill && appendSkill6Property.HasValue)
            {
                builder.AppendLine(string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6].Description, appendSkill6Property.Value.Key.Name, appendSkill6Property.Value.Value.nValue[1], appendSkill6Property.Value.Value.nValue[2]));
            }

            /// Hỗ trợ
            foreach (KeyValuePair<int, List<string>> pair in skillAdditions)
            {
                int enchantSkillID = pair.Key;
                if (Loader.Loader.Skills.TryGetValue(enchantSkillID, out SkillDataEx enchantSkillData))
                {
                    builder.AppendLine(string.Format("<color=#f868f1>Hỗ trợ:</color> <color=#0dff00>[{0}]</color>", enchantSkillData.Name));
                    foreach (string enchantDesc in pair.Value)
                    {
                        builder.AppendLine(enchantDesc);
                    }
                }
            }
            if (!isChildSkill && enchantSkill != null && enchantSkillLevel > 0)
            {
                builder.AppendLine();
                foreach (RelatedSkill relatedSkill in enchantSkill.RelatedSkills.Values)
                {
                    if (Loader.Loader.Skills.TryGetValue(relatedSkill.ID, out SkillDataEx enchantSkillData))
                    {
                        builder.AppendLine(string.Format("<color=#f868f1>Hỗ trợ:</color> <color=#0dff00>[{0}]</color>", enchantSkillData.Name));
                        builder.AppendLine(KTGlobal.AnalysisPropertyInfo(enchantSkillData, relatedSkill.Properties[enchantSkillLevel], enchantSkillLevel, true, true));
                    }
                }
            }

            /// Kỹ năng đi kèm
            if (!isChildSkill && startSkill != null)
            {
                if (Loader.Loader.Skills.TryGetValue(startSkill.ID, out SkillDataEx startSkillData))
                {
                    if (startSkillData.RawPropertiesConfig != "empty")
                    {
                        builder.AppendLine();
                        if (startSkillRate < 100)
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đồng thời có <color=#ffff1a>{2}%</color> thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", startSkill.Name, level, startSkillRate));
                        }
                        else
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đồng thời thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", startSkill.Name, level));
                        }
                        builder.AppendLine(KTGlobal.AnalysisPropertyInfo(startSkill, startSkill.Properties[(byte)level], level, true));
                    }
                }
            }

            if (!isChildSkill && flySkill != null)
            {
                if (Loader.Loader.Skills.TryGetValue(flySkill.ID, out SkillDataEx flySkillData))
                {
                    if (flySkillData.RawPropertiesConfig != "empty")
                    {
                        builder.AppendLine();
                        if (flySkillRate < 100)
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đồng thời có <color=#ffff1a>{2}%</color> thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", flySkill.Name, level, flySkillRate));
                        }
                        else
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đồng thời thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", flySkill.Name, level));
                        }
                        builder.AppendLine(KTGlobal.AnalysisPropertyInfo(flySkill, flySkill.Properties[(byte)level], level, true));
                    }
                }
            }

            /// Kỹ năng thi triển khi kết thúc
            if (!isChildSkill && vanishSkill != null)
            {
                if (Loader.Loader.Skills.TryGetValue(vanishSkill.ID, out SkillDataEx vanishSkillData))
                {
                    if (vanishSkillData.RawPropertiesConfig != "empty")
                    {
                        if (vanishSkillRate < 100)
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Kết thúc có <color=#ffff1a>{2}%</color> thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", vanishSkill.Name, level, vanishSkillRate));
                        }
                        else
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Kết thúc thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", vanishSkill.Name, level));
                        }
                        builder.AppendLine(KTGlobal.AnalysisPropertyInfo(vanishSkill, vanishSkill.Properties[(byte)level], level, true));
                    }
                }
            }

            /// Kỹ năng thi triển khi trúng mục tiêu
            if (!isChildSkill && collidSkill != null)
            {
                if (Loader.Loader.Skills.TryGetValue(collidSkill.ID, out SkillDataEx collidSkillData))
                {
                    if (collidSkillData.RawPropertiesConfig != "empty")
                    {
                        if (collideSkillRate < 100)
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đánh trúng có <color=#ffff1a>{2}%</color> thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", collidSkill.Name, level, collideSkillRate));
                        }
                        else
                        {
                            builder.AppendLine(string.Format("<color=#ffadfa>Đánh trúng thi triển:</color> <color=#0dff00>[{0} (Cấp {1})]</color>", collidSkill.Name, level));
                        }
                        builder.AppendLine(KTGlobal.AnalysisPropertyInfo(collidSkill, collidSkill.Properties[(byte)level], level, true));
                    }
                }
            }

            /// Kỹ năng tự động kích hoạt
            if (autoSkillID != -1 && autoSkillLevel != -1)
            {
                string autoSkillDesc = KTGlobal.GetAutoSkillDesc(autoSkillID, autoSkillLevel);
                if (!string.IsNullOrEmpty(autoSkillDesc))
                {
                    builder.Append(autoSkillDesc);
                }
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Hiển thị SuperTooltip thông tin kỹ năng tương ứng
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="baseLevel"></param>
        /// <param name="level"></param>
        public static void ShowSkillItemInfo(SkillDataEx skill, int baseLevel, int level)
        {
            PropertyDictionary currentLevelPd = null;
            if (level > 0)
            {
                currentLevelPd = skill.Properties[(byte)level];
            }
            PropertyDictionary nextLevelPd = skill.Properties[(byte)(level + 1)];

            /// Hiện khung SuperToolTip
            PlayZone.Instance.OpenUISuperToolTip();
            PlayZone.Instance.UISuperToolTip.ShowSubToolTip = false;

            UISuperToolTip_Component uiSuperToolTip = PlayZone.Instance.UISuperToolTip.MainToolTip;
            uiSuperToolTip.ShowItemBoxBackground = false;
            uiSuperToolTip.Title = skill.Name;
            uiSuperToolTip.ShortDesc = skill.ShortDesc;
            uiSuperToolTip.IconBundleDir = skill.IconBundleDir;
            uiSuperToolTip.IconAtlasName = skill.IconAtlasName;
            uiSuperToolTip.IconSpriteName = skill.Icon;
            uiSuperToolTip.TotalStar = 0;
            uiSuperToolTip.ShowFunctionButtons = false;
            uiSuperToolTip.ShowBottomDesc = true;
            uiSuperToolTip.AuthorName = "";
            uiSuperToolTip.PriceText = "";

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.AppendLine(string.Format("<color=#b8f2ff>{0}</color>", skill.FullDesc));

            contentBuilder.AppendLine();

            bool foundSpecial = false;
            /// Ngũ hành yêu cầu
            string seriesText = KTGlobal.GetSeriesText(skill.ElementalSeries);
            if (skill.ElementalSeries != 0)
            {
                contentBuilder.AppendLine(string.Format("Ngũ hành: {0}", seriesText));
                foundSpecial = true;
            }

            /// Vũ khí yêu cầu
            if (!skill.WeaponLimit.Contains((int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL))
            {
                List<string> weapons = new List<string>();
                foreach (int weaponCategory in skill.WeaponLimit)
                {
                    weapons.Add(KTGlobal.GetWeaponKind(weaponCategory));
                }
                string weaponRequireText = string.Join(", ", weapons);
                contentBuilder.AppendLine(string.Format("Vũ khí: <color=#ffd70f>{0}</color>", weaponRequireText));
                foundSpecial = true;
            }

            /// Không thể thi triển khi cưỡi
            if (skill.HorseLimit)
            {
                contentBuilder.AppendLine("Không thể thi triển khi cưỡi");
                foundSpecial = true;
            }

            /// Yêu cầu môn phái hoặc hệ phái
            if (skill.FactionID != 0)
            {
                if (Loader.Loader.Factions.TryGetValue(skill.FactionID, out FactionXML factionXML))
                {
                    /// Nếu môn phái không thích hợp
                    if (Global.Data.RoleData.FactionID != skill.FactionID)
                    {
                        contentBuilder.AppendLine(string.Format("<color=red>Yêu cầu môn phái: {0}</color>", factionXML.Name));

                        foundSpecial = true;
                    }
                    /// Nếu không yêu cầu hệ phái
                    else if (skill.SubID == 0)
                    {
                        KTGlobal.GetElementString(factionXML.Elemental, out Color color);
                        string factionColorHex = ColorUtility.ToHtmlStringRGB(color);
                        contentBuilder.AppendLine(string.Format("Yêu cầu môn phái: <color=#{0}>{1}</color>", factionColorHex, factionXML.Name));

                        foundSpecial = true;
                    }
                    /// Nếu hệ phái không phù hợp
                    else if (Global.Data.RoleData.SubID != skill.SubID)
                    {
                        if (factionXML.Subs.TryGetValue(skill.SubID, out FactionXML.Sub route))
                        {
                            contentBuilder.AppendLine(string.Format("<color=red>Yêu cầu hệ phái: {0}</color>", route.Name));

                            foundSpecial = true;
                        }
                    }
                    else
                    {
                        if (factionXML.Subs.TryGetValue(skill.SubID, out FactionXML.Sub route))
                        {
                            KTGlobal.GetElementString(factionXML.Elemental, out Color color);
                            string factionColorHex = ColorUtility.ToHtmlStringRGB(color);
                            contentBuilder.AppendLine(string.Format("Yêu cầu hệ phái: <color=#{0}>{1}</color>", factionColorHex, route.Name));

                            foundSpecial = true;
                        }
                    }
                }
            }

            if (foundSpecial)
            {
                contentBuilder.AppendLine();
            }

            if (baseLevel >= 0)
            {
                if (baseLevel == 0)
                {
                    contentBuilder.AppendLine(string.Format("<color=#3dfff5>Cấp học: <color=#ffdd00>{0}/{1}</color></color>", 1, skill.MaxSkillLevel));
                    contentBuilder.AppendLine(KTGlobal.AnalysisPropertyInfo(skill, skill.Properties[1], 1));
                }
                else
                {
                    contentBuilder.AppendLine(string.Format("<color=#3dfff5>Cấp hiện tại: <color=#ffdd00>{0}/{1}</color></color>", level, skill.MaxSkillLevel));
                    if (baseLevel >= skill.MaxSkillLevel)
                    {
                        contentBuilder.AppendLine("<color=#ff0f0f>Đã đạt cấp cao nhất</color>");
                    }

                    contentBuilder.AppendLine(KTGlobal.AnalysisPropertyInfo(skill, currentLevelPd, level));
                }
            }

            if (baseLevel > 0 && level < skill.MaxSkillLevel)
            {
                {
                    contentBuilder.AppendLine();

                    contentBuilder.AppendLine(string.Format("<color=#3dfff5>Cấp kế tiếp: <color=#ffdd00>{0}/{1}</color></color>", level + 1, skill.MaxSkillLevel));
                    if (skill.AttackRadius > 0)
                    {
                        contentBuilder.AppendLine(string.Format("<color=#3d9eff>Cự ly thi triển:</color> <color=#ffff1a>{0}</color>", skill.AttackRadius));
                    }
                    contentBuilder.AppendLine(KTGlobal.AnalysisPropertyInfo(skill, nextLevelPd, level + 1));
                }
            }
            uiSuperToolTip.Content = contentBuilder.ToString();

            /// Tạo Tooltip
            PlayZone.Instance.UISuperToolTip.Build();
        }

        #endregion Skill Super ToolTip
    }
}