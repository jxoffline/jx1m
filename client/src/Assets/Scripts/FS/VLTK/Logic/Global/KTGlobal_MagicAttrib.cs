using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using GameServer.VLTK.Utilities;
using Server.Data;
using System.Collections.Generic;
using System.Text;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Quản lý MagicAttrib
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Trả về thông tin chỉ số cộng điểm của Buff
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string GetBuffAttributeDescription(BufferData buff)
        {
            /// ProDict tương ứng
            PropertyDictionary buffPd = buff.CustomProperty == null ? null : PropertyDictionary.FromPortableDBString(buff.CustomProperty);
            if (buffPd == null)
            {
                if (Loader.Loader.Skills.TryGetValue(buff.BufferID, out SkillDataEx skillData))
                {
                    return skillData.ShortDesc;
                }
                else
                {
                    return "";
                }
            }

            /// Đối tượng StringBuilder
            StringBuilder contentBuilder = new StringBuilder();

            /// Duyệt danh sách
            foreach (KeyValuePair<short, object> record in buffPd.GetDictionary())
            {
                KMagicAttrib attrib = record.Value as KMagicAttrib;
                if (attrib == null)
                {
                    continue;
                }

                /// Nội dung mô tả MagicAttrib tương ứng
                string strDesc = KTGlobal.GetAttributeDescription(attrib);
                if (!string.IsNullOrEmpty(strDesc))
                {
                    contentBuilder.AppendLine(strDesc);
                }
            }

            string result = contentBuilder.ToString();
            if (string.IsNullOrEmpty(result))
            {
                if (Loader.Loader.Skills.TryGetValue(buff.BufferID, out SkillDataEx skillData))
                {
                    result = skillData.ShortDesc;
                }
                else
                {
                    return "";
                }
            }

            /// Trả về kết quả
            return result;
        }

        /// <summary>
        /// Trả về mô tả của KMagicAttrib tương ứng
        /// </summary>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public static string GetAttributeDescription(KMagicAttrib attrib)
        {
            string result = "";
            switch (attrib.nAttribType)
            {
                case MAGIC_ATTRIB.magic_appenddamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_appenddamage_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_physicsenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_physicsenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_physicsenhance_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_physicsenhance_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_colddamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_colddamage_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_firedamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_firedamage_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_lightingdamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_lightingdamage_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_poisondamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_poisondamage_v].Description, attrib.nValue[0], Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_magicdamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_magicdamage_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.state_hurt_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_slowall_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_burn_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_stun_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_weak_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_fixed_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_palsy_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));

                    break;

                case MAGIC_ATTRIB.state_slowrun_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_freeze_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_confuse_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_knock_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_attack].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1] * attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.state_silence_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_drag_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_attack].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1] * attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.state_float_attack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_float_attack].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.state_hurt_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_weak_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowall_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_burn_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_stun_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_fixed_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_palsy_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowrun_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_freeze_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_confuse_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_knock_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_silence_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_drag_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_float_resisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_resisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_hurt_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_weak_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowall_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_burn_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_stun_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_fixed_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_palsy_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowrun_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_freeze_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_confuse_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_knock_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_silence_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_drag_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_float_resistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_float_resistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_hurt_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_weak_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowall_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_burn_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_stun_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_fixed_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_palsy_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowrun_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_freeze_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_confuse_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_knock_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_silence_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_drag_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_float_attacktime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_float_attacktime].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_hurt_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_weak_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowall_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_burn_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_stun_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_fixed_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_palsy_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_slowrun_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_freeze_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_confuse_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_knock_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_silence_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_drag_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.state_float_attackrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_float_attackrate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_superpose_magic:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_superpose_magic].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_lifepotion_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifepotion_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_manapotion_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manapotion_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_meleedamagereturn_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_meleedamagereturn_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_meleedamagereturn_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_meleedamagereturn_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_rangedamagereturn_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_rangedamagereturn_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_rangedamagereturn_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_rangedamagereturn_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_adddefense_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_adddefense_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_armordefense_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_armordefense_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lifemax_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifemax_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lifemax_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifemax_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_life_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_life_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lifereplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifereplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_fastlifereplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastlifereplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manamax_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manamax_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manamax_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manamax_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_mana_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_mana_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manareplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manareplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_fastmanareplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastmanareplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_staminamax_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staminamax_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_staminamax_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staminamax_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_stamina_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stamina_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_staminareplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staminareplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_faststaminareplenish_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_faststaminareplenish_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_strength_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_strength_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_dexterity_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_dexterity_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_vitality_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_vitality_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_energy_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_energy_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_poisontimereduce_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poisontimereduce_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_poisondamagereduce_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poisondamagereduce_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_fastwalkrun_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastwalkrun_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_fastwalkrun_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastwalkrun_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_attackrating_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_attackrating_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_attackratingenhance_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_attackratingenhance_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_attackrating_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_attackrating_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_attackratingenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_attackratingenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_attackspeed_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_attackspeed_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_castspeed_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_castspeed_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_weapondamagemin_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_weapondamagemin_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_weapondamagemax_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_weapondamagemax_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_weaponmagicmin_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_weaponmagicmin_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_weaponmagicmax_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_weaponmagicmax_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addphysicsdamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addphysicsdamage_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addfiredamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiredamage_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addcolddamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcolddamage_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addlightingdamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingdamage_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addpoisondamage_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisondamage_v].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.magic_addphysicsdamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addphysicsdamage_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addphysicsmagic_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addphysicsmagic_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_slowmissile_b:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_slowmissile_b].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_changecamp_b:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_changecamp_b].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lucky_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lucky_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_steallife_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_steallife_p].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_steallifeenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_steallifeenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_stealstamina_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_stealstamina_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_stealstaminaenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stealstaminaenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_stealmana_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_stealmana_p].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_stealmanaenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stealmanaenhance_p].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_allskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_allskill_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_metalskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_metalskill_v].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_woodskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_woodskill_v].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_waterskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_waterskill_v].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_fireskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fireskill_v].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_earthskill_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_earthskill_v].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_deadlystrikeenhance_r:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_deadlystrikeenhance_r].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manashield_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manashield_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_fatallystrikeenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fatallystrikeenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addphysicsmagic_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addphysicsmagic_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_addfiremagic_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiremagic_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addcoldmagic_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcoldmagic_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addlightingmagic_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingmagic_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_addpoisonmagic_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisonmagic_v].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.magic_statusimmunity_b:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_statusimmunity_b].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_expenhance_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_expenhance_v].Description, attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_expenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_expenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_setblurstate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_setblurstate].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_add120skillexpenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_add120skillexpenhance_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_seriesconquar_r:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesconquar_r].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_seriesenhance_r:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesenhance_r].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_seriesres_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesres_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_dynamicmagicshield_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_dynamicmagicshield_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_nomovespeed:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_nomovespeed].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_changefeature1:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_changefeature1].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_changefeature2:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_changefeature2].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_stealfeature:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stealfeature].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addstealfeatureskill:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stealfeature].Description);
                    break;

                case MAGIC_ATTRIB.magic_deadlystrikedamageenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_deadlystrikedamageenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lifereplenish_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifereplenish_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manareplenish_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manareplenish_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_returnskill_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_returnskill_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_poisondamagereturn_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poisondamagereturn_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_poisondamagereturn_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poisondamagereturn_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_hide:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_hide].Description);
                    break;

                case MAGIC_ATTRIB.magic_poison2decmana_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poison2decmana_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_staticmagicshield_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staticmagicshield_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_staticmagicshieldmax_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staticmagicshieldmax_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_staticmagicshieldcur_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staticmagicshieldcur_p].Description, attrib.nValue[0].AttributeToString(), Utils.Truncate(attrib.nValue[1] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.magic_lifegrow_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lifegrow_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_managrow_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_managrow_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_staminagrow_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_staminagrow_v].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_addexpshare:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addexpshare].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_subexplose:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_subexplose].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_ignoreattackontime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoreattackontime].Description, Utils.Truncate(attrib.nValue[0] / 18f, 1));
                    break;

                case MAGIC_ATTRIB.magic_poisontimeenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_poisontimeenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_protected:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_protected].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_removestate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_removestate].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_removeshield:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_removeshield].Description);
                    break;

                case MAGIC_ATTRIB.magic_skilldamageptrim:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilldamageptrim].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_defencedeadlystrikedamagetrim:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_defencedeadlystrikedamagetrim].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_seriesenhance:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesenhance].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_seriesabate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesabate].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_weaponbasedamagetrim:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_weaponbasedamagetrim].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_posionweaken:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_posionweaken].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_skillselfdamagetrim:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skillselfdamagetrim].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_skillexpaddtion_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skillexpaddtion_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_allseriesstateresisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_allseriesstateresisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_allseriesstateresistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_allseriesstateresistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_allspecialstateresisttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_allspecialstateresisttime].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_allspecialstateresistrate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_allspecialstateresistrate].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_domainchangeself:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_domainchangeself].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_adddomainskill1:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_adddomainskill1].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_adddomainskill2:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_adddomainskill2].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_adddomainskill3:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_adddomainskill3].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_stealskillstate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_stealskillstate].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_wastemanap:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_wastemanap].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_damage_all_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_all_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_physics_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_physics_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_poison_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_poison_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_cold_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_cold_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_fire_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_fire_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_light_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_light_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_series_resist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_series_resist].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_physics_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_physics_receive_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_poison_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_poison_receive_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_cold_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_cold_receive_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_fire_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_fire_receive_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_light_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_light_receive_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_damage_return_receive_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_return_receive_p].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_prop_showhide:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_prop_showhide].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_prop_invincibility:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_prop_invincibility].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_prop_ignoretrap:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_prop_ignoretrap].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_revive:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_revive].Description, attrib.nValue[0].AttributeToString(), attrib.nValue[1].AttributeToString(), attrib.nValue[2].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_ignoreskill:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoreskill].Description, attrib.nValue[2] == 1 ? "ngoại" : attrib.nValue[2] == 2 ? "nội" : "nội ngoại", attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_ignoreinitiative:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoreinitiative].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_infectcurse:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_infectcurse].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_infectpoison:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_infectpoison].Description, (-attrib.nValue[0]).AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_ignoredefenseenhance_v:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoredefenseenhance_v].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_ignoredefenseenhance_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoredefenseenhance_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_suddendeath:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_suddendeath].Description, attrib.nValue[0], attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_ignore_debuff:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignore_debuff].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_lucky_v_partner:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_lucky_v_partner].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_listen_msg:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_listen_msg].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_ignoreattack:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoreattack].Description, attrib.nValue[0], attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_damage_added:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_added].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_seriesstate_added:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_seriesstate_added].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_locked:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_locked].Description, attrib.nValue[0], attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_ignoreresist_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignoreresist_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_defense_state:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_defense_state].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_hide_all:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_hide_all].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_clearcd:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_clearcd].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_rdclifewithdis:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_rdclifewithdis].Description, attrib.nValue[0], attrib.nValue[1]);
                    break;

                case MAGIC_ATTRIB.magic_runattackmany:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_runattackmany].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_stealstate:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_stealstate].Description, attrib.nValue[1], attrib.nValue[0], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.state_hurt_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_hurt_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_weak_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_weak_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_slowall_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowall_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_burn_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_burn_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_stun_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_stun_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_fixed_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_fixed_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_palsy_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_palsy_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_slowrun_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_slowrun_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_freeze_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_freeze_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_confuse_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_confuse_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_knock_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_knock_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_silence_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_silence_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_drag_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_drag_ignore].Description);
                    break;

                case MAGIC_ATTRIB.state_float_ignore:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.state_float_ignore].Description);
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_keephide:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_keephide].Description);
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_addmagicbydist:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addmagicbydist].Description, KTGlobal.GetSeriesStateText((KE_STATE)attrib.nValue[0]), Utils.Truncate(attrib.nValue[1] / 100f, 1), attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_attackenhancebycostmana_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_attackenhancebycostmana_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_addhidetime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_addhidetime].Description, attrib.nValue[0], attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_decreasepercasttime:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilladdition_decreasepercasttime].Description, attrib.nValue[0], attrib.nValue[1], attrib.nValue[2]);
                    break;

                case MAGIC_ATTRIB.magic_skilladdition_addskilllevel:
                    if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx skillData) && attrib.nValue[1] > 0)
                    {
                        result = string.Format("Kỹ năng <color=#4ce600>[{0}]</color> <color=ffff1a>+{1}</color>", skillData.Name, attrib.nValue[1]);
                    }
                    break;

                case MAGIC_ATTRIB.magic_ignore_curse_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_ignore_curse_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_damage2addmana_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage2addmana_p].Description, attrib.nValue[0].AttributeToString());
                    break;

                case MAGIC_ATTRIB.magic_manatoskill_enhance:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_manatoskill_enhance].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addfiredamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiredamage_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addcolddamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcolddamage_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addlightingdamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingdamage_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addpoisondamage_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisondamage_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addcoldmagic_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addcoldmagic_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addfiremagic_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addfiremagic_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addlightingmagic_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addlightingmagic_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addpoisonmagic_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addpoisonmagic_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_create_illusion:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_create_illusion].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_damage_inc_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_damage_inc_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_redeivedamage_dec_p2:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_redeivedamage_dec_p2].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_ban_skill_1:
                case MAGIC_ATTRIB.magic_ban_skill_2:
                case MAGIC_ATTRIB.magic_ban_skill_3:
                case MAGIC_ATTRIB.magic_ban_skill_4:
                case MAGIC_ATTRIB.magic_ban_skill_5:
                case MAGIC_ATTRIB.magic_ban_skill_6:
                    if (Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx BandSKilLName))
                    {
                        result = string.Format("Sau khi sử dụng vô hiệu hóa kỹ năng <color=#0cf500>[{0}]</color>", BandSKilLName.Name);
                    }
                    break;

                case MAGIC_ATTRIB.magic_skill_cost_buff1layers_v:
                case MAGIC_ATTRIB.magic_skill_cost_buff2layers_v:
                case MAGIC_ATTRIB.magic_skill_cost_buff3layers_v:
                case MAGIC_ATTRIB.magic_skill_cost_buff4layers_v:
                case MAGIC_ATTRIB.magic_skill_cost_buff5layers_v:
                case MAGIC_ATTRIB.magic_skill_cost_buff6layers_v:

                    if (!Loader.Loader.Skills.TryGetValue(attrib.nValue[0], out SkillDataEx buffSkill))
                    {
                        return null;
                    }

                    result = "Yêu cầu phải có hiệu ứng [" + buffSkill.Name + "] mới có thể kích hoạt kỹ năng!";
                    break;

                case MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_fastlifereplenish_byvitality:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_fastlifereplenish_byvitality].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_skilldamageptrimbylesshp:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_skilldamageptrimbylesshp].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_immediatereplbymaxstate_p:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_immediatereplbymaxstate_p].Description, attrib.nValue[0]);
                    break;

                case MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality:
                    result = string.Format(PropertyDefine.PropertiesByID[(int)MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality].Description, attrib.nValue[0]);
                    break;
            }

            /// Trả về kết quả
            return result;
        }
    }
}