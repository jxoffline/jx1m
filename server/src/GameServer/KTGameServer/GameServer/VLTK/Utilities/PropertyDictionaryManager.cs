using GameServer.KiemThe.Entities;
using System.Collections.Generic;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Quản lý từ điển thuộc tính đối tượng
    /// </summary>
    public static class PropertyDictionaryManager
    {
        /// <summary>
        /// Trả về giá trị trong ProDict tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pd"></param>
        /// <param name="defaultValue1"></param>
        /// <param name="defaultValue2"></param>
        /// <param name="defaultValue3"></param>
        /// <returns></returns>
        private static int[] GetValues(PropertyDictionary pd, MAGIC_ATTRIB key, int defaultValue1 = 0, int defaultValue2 = 0, int defaultValue3 = 0)
        {
            if (pd.ContainsKey((int)key))
            {
                KMagicAttrib attrib = pd.Get<KMagicAttrib>((int)key);
                if (attrib == null)
                {
                    return new int[] { defaultValue1, defaultValue2, defaultValue3 };
                }
                else
                {
                    return attrib.nValue;
                }
            }
            else
            {
                return new int[] { defaultValue1, defaultValue2, defaultValue3 };
            }
        }

        /// <summary>
        /// Thiết lập giá trị trong ProDict tương ứng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="key"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        private static void SetValues(PropertyDictionary pd, MAGIC_ATTRIB key, int value1 = 0, int value2 = 0, int value3 = 0)
        {
            KMagicAttrib attrib = new KMagicAttrib()
            {
                nAttribType = key,
                nValue = new int[]
                {
                    value1, value2, value3
                },
            };
            pd.Set<KMagicAttrib>((int)key, attrib);
        }

        #region Né tránh

        /// <summary>
        /// Né tránh
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetDodge(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_armordefense_v, 0)[0];
        }

        /// <summary>
        /// Thiết lập né tránh
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetDodge(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_armordefense_v, value);
        }

        #endregion Né tránh

        #region Sát thương của kỹ năng

        /// <summary>
        /// Sát thương của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetSkillDamageP(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_skilldamage_p, 100)[0];
        }

        /// <summary>
        /// Thiết lập Sát thương của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetSkillDamageP(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_skilldamage_p, value);
        }

        #endregion Sát thương của kỹ năng

        #region Phát huy lực tấn công cơ bản của kỹ năng

        /// <summary>
        /// Phát huy lực tấn công cơ bản của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetAppendDamageP(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_appenddamage_p, 100)[0];
        }

        /// <summary>
        /// Thiết lập Phát huy lực tấn công cơ bản của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetAppendDamageP(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_appenddamage_p, value);
        }

        #endregion Phát huy lực tấn công cơ bản của kỹ năng khác

        #region Tỷ lệ đánh trúng

        /// <summary>
        /// Tỷ lệ đánh trúng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetHiteRateP(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_attackrating_p, 0)[0];
        }

        /// <summary>
        /// Thiết lập Tỷ lệ đánh trúng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetHiteRateP(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_attackrating_p, value);
        }

        #endregion Tỷ lệ đánh trúng

        #region Chính xác

        /// <summary>
        /// Chính xác
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetHiteRateV(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_attackrating_v, 0)[0];
        }

        /// <summary>
        /// Thiết lập Chính xác
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetHiteRateV(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_attackrating_v, value);
        }

        #endregion Chính xác

        #region Bỏ qua né tránh đối thủ

        /// <summary>
        /// Bỏ qua né tránh đối thủ
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetIgnoreDefenseV(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_ignoredefense_v, 0)[0];
        }

        /// <summary>
        /// Thiết lập Bỏ qua né tránh đối thủ
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetIgnoreDefenseV(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_ignoredefense_v, value);
        }

        /// <summary>
        /// Trả về tỷ lệ bỏ qua né tránh đối thủ
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetIgnoreDefenseP(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_ignoredefense_p, 0)[0];
        }

        /// <summary>
        /// Thiết lập tỷ lệ bỏ qua né tránh đối thủ
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetIgnoreDefenseP(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_ignoredefense_p, value);
        }

        #endregion Bỏ qua né tránh đối thủ

        #region Phát huy lực tấn công của kỹ năng

        /// <summary>
        /// Phát huy lực tấn công của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetSkillSelfDamageTrim(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_skillselfdamagetrim, 0)[0];
        }

        /// <summary>
        /// Thiết lập Phát huy lực tấn công của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetSkillSelfDamageTrim(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_skillselfdamagetrim, value);
        }

        #endregion Phát huy lực tấn công của kỹ năng

        #region Phát huy lực tấn công cơ bản

        /// <summary>
        /// Phát huy lực tấn công cơ bản
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetSkillDamagePTrim(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_skilldamageptrim, 0)[0];
        }

        /// <summary>
        /// Thiết lập Phát huy lực tấn công cơ bản
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetSkillDamagePTrim(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_skilldamageptrim, value);
        }

        #endregion Phát huy lực tấn công cơ bản

        #region Hỗ trợ sát thương kỹ năng khác
        /// <summary>
        /// Trả về hỗ trợ sát thương kỹ năng khác
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static int GetAppendSkillDamage(PropertyDictionary pd, int skillID, int slot)
        {
            KMagicAttrib attrib = null;
            switch (slot)
            {
                case 1:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage1);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 2:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage2);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 3:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage3);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 4:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage4);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 5:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage5);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 6:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage6);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
            }
            return 0;
        }

        /// <summary>
        /// Thiết lập hỗ trợ sát thương kỹ năng khác
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="slot"></param>
        /// <param name="value"></param>
        public static void SetAppendSkillDamage(PropertyDictionary pd, int skillID, int slot, int value)
        {
            KMagicAttrib attrib = new KMagicAttrib();
            switch (slot)
            {
                case 1:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage1;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage1, attrib);
                    break;
                case 2:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage2;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage2, attrib);
                    break;
                case 3:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage3;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage3, attrib);
                    break;
                case 4:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage4;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage4, attrib);
                    break;
                case 5:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage5;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage5, attrib);
                    break;
                case 6:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skill_addskilldamage6;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_addskilldamage6, attrib);
                    break;
            }
        }
        #endregion

        #region Hỗ trợ % sát thương kỹ năng khác
        /// <summary>
        /// Trả về hỗ trợ % sát thương kỹ năng khác
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static int GetAppendSkillDamagePercent(PropertyDictionary pd, int skillID, int slot)
        {
            KMagicAttrib attrib = null;
            switch (slot)
            {
                case 1:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 2:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 3:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 4:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 5:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
                case 6:
                    attrib = pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6);
                    return attrib == null || attrib.nValue[0] != skillID ? 0 : attrib.nValue[1];
            }
            return 0;
        }

        /// <summary>
        /// Thiết lập hỗ trợ % sát thương kỹ năng khác
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="slot"></param>
        /// <param name="value"></param>
        public static void SetAppendSkillDamagePercent(PropertyDictionary pd, int skillID, int slot, int value)
        {
            KMagicAttrib attrib = new KMagicAttrib();
            switch (slot)
            {
                case 1:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent, attrib);
                    break;
                case 2:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2, attrib);
                    break;
                case 3:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3, attrib);
                    break;
                case 4:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4, attrib);
                    break;
                case 5:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5, attrib);
                    break;
                case 6:
                    attrib.nAttribType = MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6;
                    attrib.nValue = new int[] { skillID, value, 0 };
                    pd.Set<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6, attrib);
                    break;
            }
        }
        #endregion

        #region Sát thương NỘ
        /// <summary>
        /// Trả về sát thương nộ
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetAngerDamage(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_angerdamage_p, 0)[0];
        }

        /// <summary>
        /// Thiết lập sát thương nộ
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetAngerDamage(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_angerdamage_p, value);
        }
        #endregion

        #region Sát thương ngũ hành
        /// <summary>
        /// Trả về sát thương ngũ hành
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetSeriesDamage(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r, 0)[0];
        }

        /// <summary>
        /// Thiết lập sát thương ngũ hành
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetSeriesDamage(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r, value);
        }
        #endregion

        #region % vật công
        /// <summary>
        /// Trả về % sát thương vật công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetPhysicsDamageEnhanceP(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_physicsenhance_p, 0)[0];
        }

        /// <summary>
        /// Thiết lập % sát thương vật công
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetPhysicsDamageEnhanceP(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_physicsenhance_p, value);
        }

        /// <summary>
        /// Trả về sát thương vật công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetPhysicsDamageEnhanceV(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_trice_eff_physicsenhance_v, 0)[0];
        }

        /// <summary>
        /// Thiết lập sát thương vật công
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        public static void SetPhysicsDamageEnhanceV(PropertyDictionary pd, int value)
        {
            PropertyDictionaryManager.SetValues(pd, MAGIC_ATTRIB.magic_trice_eff_physicsenhance_v, value);
        }
        #endregion

        /// <summary>
        /// Tăng sát thương ngoại công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetPhysicsTrimDamage(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_physicsdamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_physicsdamage_v));
        }

        /// <summary>
        /// Tăng sát thương nội công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetMagicTrimDamage(PropertyDictionary pd)
		{
            return pd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_trice_eff_magicdamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_magicdamage_v));
        }

        #region Sát thương ngũ hành
        /// <summary>
        /// Trả về sát thương băng công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetColdDamage(PropertyDictionary pd)
        {
            
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_colddamage_v,new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_colddamage_v));
        }

        /// <summary>
        /// Trả về sát thương hỏa công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetFireDamage(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_firedamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_firedamage_v));
        }

        /// <summary>
        /// Trả về sát thương lôi công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetLightDamage(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_lightingdamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_lightingdamage_v));
        }

        /// <summary>
        /// Trả về sát thương độc công
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetPoisonDamage(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_poisondamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_poisondamage_v));
        }
        #endregion

        /// <summary>
        /// Trả về sát thương vật công nội
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetMagicDamage(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_magicdamage_v, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_magicdamage_v));
        }

        /// <summary>
        /// Lấy  ra  chỉ số hút hít vật lý
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetLifeStealP(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_steallife_p, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_steallife_p));
        }

        /// <summary>
        /// Lấy ra chỉ số hút mana của nhân vật
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetManaStealP(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_stealmana_p, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_stealmana_p));
        }

        /// <summary>
        /// Lấy ra chỉ số hút thể lực
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetStealStaminaP(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_stealstamina_p, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_stealstamina_p));
        }

        /// <summary>
        /// Tình trạng hút hít
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetStealState(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_trice_eff_stealstate, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_stealstate));
        }

        /// <summary>
        /// Tỷ lệ chí tử của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetFatalStrikeP(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p, new KMagicAttrib(MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p));
        }



        /// <summary>
        /// Điểm chí mạng của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetFatalStrikeEnhance(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_deadlystrike_r, new KMagicAttrib(MAGIC_ATTRIB.magic_skill_deadlystrike_r));
        }

        /// <summary>
        /// Tỷ lệ chí mạng của kỹ năng
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static KMagicAttrib GetFatalStrikeEnhanceP(PropertyDictionary pd)
        {
            return pd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fatallystrikeenhance_p, new KMagicAttrib(MAGIC_ATTRIB.magic_fatallystrikeenhance_p));
        }

        /// <summary>
        /// Lấy ra toàn bộ hiệu ứng gây ra bởi skill
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static List<KMagicAttrib> GetAllSkillEffectAppend(PropertyDictionary pd)
        {
            List<KMagicAttrib> allEfecct = new List<KMagicAttrib>();

            for (int i = (int)MAGIC_ATTRIB.magic_state_effect_begin; i < (int)MAGIC_ATTRIB.magic_end; i++)
            {
                if (pd.ContainsKey(i))
                {
                    KMagicAttrib tmp = pd.Get<KMagicAttrib>(i);
                    allEfecct.Add(tmp);
                }
            }

            return allEfecct;
        }

        /// <summary>
        /// Get Run Attack Dame Add
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static int GetRunAttackAdded(PropertyDictionary pd)
        {
            return PropertyDictionaryManager.GetValues(pd, MAGIC_ATTRIB.magic_runattack_damageadded, 0)[0];
        }
    }
}