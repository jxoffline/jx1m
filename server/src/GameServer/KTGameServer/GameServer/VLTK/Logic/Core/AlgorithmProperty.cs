using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.KiemThe.Logic.KTSkillManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Thực thi LOGIC tính toán Damage
    /// </summary>
    public class AlgorithmProperty
    {
        /// Bảng hằng số hóa giải quan hàm
        private static readonly int[] ChopAbsorbDamagesPercent = new int[]
        {
            0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50
        };

        /// <summary>
        /// Tính lực tay
        /// </summary>
        /// <param name="client"></param>
        /// <param name="rcSkill"></param>
        /// <param name="additionDamagePercent"></param>
        /// <returns></returns>
        public static int GetDamageInfo(GameObject client, SkillLevelRef rcSkill, int additionDamagePercent)
        {
            try
            {
                /// Nếu Client NULL
                if (client == null)
                {
                    return 0;
                }
                /// Nếu Skill NULL
                else if (rcSkill == null)
                {
                    return 0;
                }

                // Nếu không phải skill gây sát thương return 0 luôn
                if (!rcSkill.Data.IsDamageSkill)
                {
                    return 0;
                }

                /// Nếu không có ProDict
                if (rcSkill.Properties == null)
                {
                    return 0;
                }

                Dictionary<MAGIC_ATTRIB, KMagicAttrib> AppendDameDic = new Dictionary<MAGIC_ATTRIB, KMagicAttrib>();

                bool IsDeadlyStrike = false;

                /// % sát thương cộng thêm từ kỹ năng khác
                int nDamageAddPercent = AlgorithmProperty.GetAddSkillDamagePercent(client, rcSkill.Data.ID);
                /// Cộng thêm % sát thương từ ngoài vào (có thể do xuyên suốt mục tiêu)
                nDamageAddPercent += additionDamagePercent;
                /// Sát thương cộng thêm từ kỹ năng khác
                int nDamageAdd = AlgorithmProperty.GetAddSkillDamage(client, rcSkill.Data.ID);

                /// Property Dictionary của kỹ năng
                PropertyDictionary skillPd = rcSkill.Properties.Clone();

                /// Cộng toàn bộ thuộc tính hỗ trợ
                if (client is KPlayer)
                {
                    //Console.WriteLine("SkillPD before = " + skillPd.ToString());
                    KPlayer player = client as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(rcSkill.SkillID);
                    if (enchantPd != null)
                    {
                        skillPd.AddProperties(enchantPd);
                    }
                    //Console.WriteLine("SkillPD after = " + skillPd.ToString());
                }

                /// Check xem có phải skill dame vật lý không
                bool bIsPhysical = rcSkill.Data.IsPhysical;

                /// Tỷ lệ thiệt hại ban đầu
                int nDamagePercent = 100;

                /// Phát huy lực tấn công kỹ năng
                int m_nSkillSelfDamagePTrim = client.m_nSkillSelfDamagePTrim;

                /// Nếu có dòng phát huy lực tấn công kỹ năng với mỗi 1% sinh lực mất từ thời điểm kích hoạt 
                if (client.m_nSkillDamageTrimPByLifeLoss > 0)
                {
                    /// Lượng máu đã mất (%)m_nLastLifeSkillDamageTrimP
                    float lifeLostP = client.m_CurrentLife / (float)client.m_nLastLifeSkillDamageTrimP;
                    /// Lực tấn công kỹ năng cộng thêm
                    m_nSkillSelfDamagePTrim += (int)(lifeLostP * client.m_nSkillDamageTrimPByLifeLoss);
                }

                /// Phát huy lực tấn công cơ bản :  Lấy từ các skill bị động + được apply vào nhân vật
                int m_nSkillDamagePTrim = client.m_nSkillDamagePTrim;

                if (client is KPlayer)
                {
                    m_nSkillSelfDamagePTrim += ((KPlayer)client).Skills.GetAppendDamageToSkill(rcSkill.SkillID);
                }

                int nSkillDamageT = PropertyDictionaryManager.GetSkillDamageP(skillPd) * 10 + PropertyDictionaryManager.GetSkillDamageP(skillPd) * m_nSkillSelfDamagePTrim * 10 / 100;  // Dame từ Skill
                int nAppendDamageT = PropertyDictionaryManager.GetAppendDamageP(skillPd) * 10 + PropertyDictionaryManager.GetAppendDamageP(skillPd) * m_nSkillDamagePTrim * 10 / 100;   //
                int nSkillDamageTrimP = PropertyDictionaryManager.GetSkillDamageP(skillPd) * m_nSkillSelfDamagePTrim / 100;
                int nAngerATK = 0;
                int nAngerPoiTime = 0;
                // Nếu thằng đánh bị suy yếu thì dame sẽ bị giảm
                if (client.HaveState(KE_STATE.emSTATE_WEAK))
                {
                    int nNewPercent = nDamagePercent * KTGlobal.WeakDamagePercent / 100;
                    nDamagePercent = nNewPercent;
                }

                /// 1. Nếu skill có sử dụng tỉ lệ chính xác
                if (rcSkill.Data.IsUseAR)
                {
                    /// Tạo mới KMagicAttribute
                    KMagicAttrib attrib = new KMagicAttrib();
                    /// Loại là tỷ lệ đánh trúng
                    attrib.nAttribType = MAGIC_ATTRIB.magic_damage_append_hitrate;

                    /// Chính xác ban đầu
                    int hitValue = 0;
                    /// Nếu kỹ năng có dòng % tỷ lệ đánh trúng
                    if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_attackrating_p))
                    {
                        /// Chính xác cơ bản
                        int m_AttackRating = client.m_AttackRating;
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating + m_AttackRating * PropertyDictionaryManager.GetHiteRateP(skillPd) / 100;
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }
                    /// Nếu kỹ năng có dòng tăng điểm đánh trúng
                    else if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_attackrating_v))
                    {
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating + PropertyDictionaryManager.GetHiteRateV(skillPd);
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }
                    /// Nếu kỹ năng không có chính xác
                    else
                    {
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating;
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }

                    /// Bỏ qua né tránh đối thủ
                    int m_CurrentIgnoreDefense = client.m_CurrentIgnoreDefense;
                    /// Tỷ lệ % bỏ qua né tránh đối thủ
                    int m_CurrentIgnoreDefensePercent = client.m_CurrentIgnoreDefensePercent;

                    /// Lưu lại vào KMagicAttribute
                    attrib.nValue[1] = PropertyDictionaryManager.GetIgnoreDefenseV(skillPd) + m_CurrentIgnoreDefense;
                    attrib.nValue[2] = PropertyDictionaryManager.GetIgnoreDefenseP(skillPd) + m_CurrentIgnoreDefensePercent;

                    /// Thêm vào ProDict
                    AppendDameDic.Add(attrib.nAttribType, attrib);
                }

                if (rcSkill.Data.IsDamageSkill)
                {
                    /// 1. TÍnh dame nộ nếu skill support
                    int AngerDamage = PropertyDictionaryManager.GetAngerDamage(skillPd);

                    bool bIsAnger = false;

                    if (AngerDamage > 0)
                    {
                        bIsAnger = true;
                        int nBaseAngerATK = KPlayerSetting.GetLevelBaseAngerATK(client.m_Level);

                        nAngerATK = (int)((AngerDamage / 100.0) * nBaseAngerATK) * nDamagePercent / 100 * nSkillDamageT / 1000;

                        nAngerPoiTime = KTGlobal.AngerTime;
                    }

                    /// 2. Sát thương ngũ hành

                    if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r)) // Nếu skill có chứa sát thương ngũ hành
                    {
                        //FILL CHỈ SỐ VÀO ĐÂY
                        // Dame khắc hệ của nhân vật
                        int m_CurrentSeriesConquar = client.m_CurrentSeriesConquar;  // SIMBOY : magic_seriesconquar_r
                        int m_CurrentSeriesEnhance = client.m_CurrentSeriesEnhance;  // SIMBOY : magic_seriesenhance_r  Tấn công người chơi khắc hệ, trị tương khắc của kỹ năng

                        int cSrcSeriesDamage = PropertyDictionaryManager.GetSeriesDamage(skillPd);

                        KMagicAttrib cSeriesDamage = new KMagicAttrib();

                        cSeriesDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_series;

                        cSeriesDamage.nValue[0] = cSrcSeriesDamage + m_CurrentSeriesConquar;
                        cSeriesDamage.nValue[1] = m_CurrentSeriesEnhance;

                        AppendDameDic.Add(cSeriesDamage.nAttribType, cSeriesDamage);
                    }

                    /// 4. Tính toán dame vật lý
                    {
                        KMagicAttrib cPhysicsDamage = new KMagicAttrib();
                        cPhysicsDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_physics;

                        int nMinDamage = nDamageAdd;
                        int nMaxDamage = nDamageAdd;

                        if (bIsAnger) // NẾU CÓ DAME NỘ
                        {
                            // NGŨ HÀNH CỦA NHÂN VẬT
                            // FILL CHỈ SỐ VÀO ĐÂY
                            KE_SERIES_TYPE m_Series = client.m_Series;

                            if (nAngerATK > 0 && (m_Series == KE_SERIES_TYPE.series_metal || m_Series == KE_SERIES_TYPE.series_none))
                            {
                                cPhysicsDamage.nValue[0] += nAngerATK;
                                cPhysicsDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            /// Nếu là sát thương ngoại công
                            if (bIsPhysical)
                            {
                                /// Vật công ngoại Min
                                nMinDamage = client.m_PhysicsDamage.nValue[0];
                                /// Vật công ngoại Max
                                nMaxDamage = client.m_PhysicsDamage.nValue[2];

                                /// Đánh dấu có phải người chơi không
                                bool bIsPlayer = client is KPlayer;
                                /// Người chơi tương ứng
                                KPlayer player = client as KPlayer;

                                /// Sát thương cộng thêm từ ngoài nếu là người chơi
                                if (bIsPlayer)
                                {
                                    int nEnhance = AlgorithmProperty.GetPhysicsEnhance((KPlayer)client);
                                    nMinDamage += nMinDamage * nEnhance / 100;
                                    nMaxDamage += nMaxDamage * nEnhance / 100;
                                }

                                /// Sát thương cộng thêm %
                                nMinDamage += (client.m_PhysicsDamage.nValue[0] * client.m_CurrentPhysicsDamageEnhanceP / 100);
                                nMaxDamage += (client.m_PhysicsDamage.nValue[2] * client.m_CurrentPhysicsDamageEnhanceP / 100);

                                /// Sát thương cơ bản của vũ khí
                                nMinDamage += (client.m_PhysicPhysic.nValue[0] + client.m_nWeaponBaseDamageTrim);
                                nMaxDamage += (client.m_PhysicPhysic.nValue[2] + client.m_nWeaponBaseDamageTrim);

                                /// Nếu là người chơi
                                if (bIsPlayer)
                                {
                                    ///  tăng vật công cơ bản dựa theo số Ngoại hiện tại
                                    nMinDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                    nMaxDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                }

                                /// Sát thương cộng thêm từ kỹ năng khác
                                nMinDamage = nMinDamage * nAppendDamageT / 1000;
                                nMaxDamage = nMaxDamage * nAppendDamageT / 1000;

                                /// Biến tạm
                                int nTemp = 0;

                                /// Kỹ năng hỗ trợ
                                int GetPhysicsDamageEnhanceP = PropertyDictionaryManager.GetPhysicsDamageEnhanceP(skillPd);
                                int GetPhysicsDamageEnhanceV = PropertyDictionaryManager.GetPhysicsDamageEnhanceV(skillPd);

                                /// % Sát thương từ các kỹ năng hỗ trợ
                                if (GetPhysicsDamageEnhanceP > 0)
                                {
                                    nTemp = (client.m_PhysicsDamage.nValue[0] * GetPhysicsDamageEnhanceP / 100);
                                }
                                /// Sát thương từ kỹ năng hỗ trợ
                                else if (GetPhysicsDamageEnhanceV > 0)
                                {
                                    nTemp = client.m_PhysicsDamage.nValue[2] + GetPhysicsDamageEnhanceV;
                                }

                                /// Tăng thêm ngoại công do cái gì khác
                                int nTempMin = nTemp + PropertyDictionaryManager.GetPhysicsTrimDamage(skillPd).nValue[0];
                                int nTempMax = nTemp + PropertyDictionaryManager.GetPhysicsTrimDamage(skillPd).nValue[2];

                                /// % sát thương cộng thêm
                                nTempMin += nTempMin * nDamageAddPercent / 100;
                                nTempMax += nTempMax * nDamageAddPercent / 100;
                                nTempMin *= nSkillDamageT / 1000;
                                nTempMax *= nSkillDamageT / 1000;

                                nMinDamage += nTempMin;
                                nMaxDamage += nTempMax;

                                /// Kết quả cuối cùng
                                nMinDamage += (client.m_AddPhysicsDamage * nAppendDamageT / 1000);
                                nMaxDamage += (client.m_AddPhysicsDamage * nAppendDamageT / 1000);
                            }
                            /// Nếu là sát thương nội công
                            else
                            {
                                /// Vật công nội Min
                                nMinDamage = client.m_MagicDamage.nValue[0];
                                /// Vật công nội Max
                                nMaxDamage = client.m_MagicDamage.nValue[2];

                                /// Đánh dấu có phải người chơi không
                                bool bIsPlayer = client is KPlayer;
                                /// Người chơi tương ứng
                                KPlayer player = client as KPlayer;

                                /// % vật công nội từ vũ khí
                                nMinDamage += (client.m_MagicDamage.nValue[0] * client.m_CurrentMagicPhysicsEnhanceP / 100);
                                nMaxDamage += (client.m_MagicDamage.nValue[2] * client.m_CurrentMagicPhysicsEnhanceP / 100);

                                /// Sát thương cơ bản của vũ khí
                                nMinDamage += (client.m_PhysicsMagic.nValue[0] + client.m_nWeaponBaseDamageTrim);
                                nMaxDamage += (client.m_PhysicsMagic.nValue[2] + client.m_nWeaponBaseDamageTrim);

                                /// Nếu là người chơi
                                if (bIsPlayer)
                                {
                                    /// tăng vật công cơ bản dựa theo số Ngoại hiện tại
                                    nMinDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                    nMaxDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                }

                                /// Sát thương cộng thêm từ kỹ năng hỗ trợ
                                nMinDamage = nMinDamage * nAppendDamageT / 1000;
                                nMaxDamage = nMaxDamage * nAppendDamageT / 1000;

                                /// Biến tạm
                                int nTemp = 0;

                                /// Kỹ năng hỗ trợ
                                int GetPhysicsDamageEnhanceP = PropertyDictionaryManager.GetPhysicsDamageEnhanceP(skillPd);
                                int GetPhysicsDamageEnhanceV = PropertyDictionaryManager.GetPhysicsDamageEnhanceV(skillPd);

                                /// % Sát thương từ các kỹ năng hỗ trợ
                                if (GetPhysicsDamageEnhanceP > 0)
                                {
                                    nTemp = (client.m_MagicDamage.nValue[0] * GetPhysicsDamageEnhanceP / 100);
                                }
                                /// Sát thương từ kỹ năng hỗ trợ
                                else if (GetPhysicsDamageEnhanceV > 0)
                                {
                                    nTemp = client.m_MagicDamage.nValue[0] + GetPhysicsDamageEnhanceV;
                                }

                                /// Tăng thêm nội công do cái gì khác
                                int nTempMin = nTemp + PropertyDictionaryManager.GetMagicTrimDamage(skillPd).nValue[0];
                                int nTempMax = nTemp + PropertyDictionaryManager.GetMagicTrimDamage(skillPd).nValue[2];

                                /// % sát thương cộng thêm
                                nTempMin += nTempMin * nDamageAddPercent / 100;
                                nTempMax += nTempMax * nDamageAddPercent / 100;
                                nTempMin *= nSkillDamageT / 1000;
                                nTempMax *= nSkillDamageT / 1000;

                                nMinDamage += nTempMin;
                                nMaxDamage += nTempMax;

                                /// Kết quả cuối cùng
                                nMinDamage += (client.m_MagicPhysicsDamage.nValue[0] * nAppendDamageT / 1000);
                                nMaxDamage += (client.m_MagicPhysicsDamage.nValue[2] * nAppendDamageT / 1000);
                            }

                            /// Nhận thêm sát thương bởi dame phép
                            if (client.m_nAttackAddedByMana > 0)
                            {
                                nMinDamage += client.m_CurrentMana * client.m_nAttackAddedByMana / 100;
                                nMaxDamage += client.m_CurrentMana * client.m_nAttackAddedByMana / 100;
                            }

                            cPhysicsDamage.nValue[0] = nMinDamage * nDamagePercent / 100;
                            cPhysicsDamage.nValue[2] = nMaxDamage * nDamagePercent / 100;
                        }

                        AppendDameDic.Add(cPhysicsDamage.nAttribType, cPhysicsDamage);
                    }

                    int nSkillPercent = nDamageAddPercent + 100; //% sức mạnh kỹ năng

                    /// 5. Tính toán dame băng công
                    {
                        KMagicAttrib cColdDamage = new KMagicAttrib();
                        cColdDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_cold;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_water)
                            {
                                cColdDamage.nValue[0] += nAngerATK;
                                cColdDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cColdDamage = CalcAttackDamage(PropertyDictionaryManager.GetColdDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_cold]);
                            cColdDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_cold;

                            ///  % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cColdDamage.nValue[0] += cColdDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_water] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_water]) / 100;
                            cColdDamage.nValue[2] += cColdDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_water] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_water]) / 100;
                        }

                        AppendDameDic.Add(cColdDamage.nAttribType, cColdDamage);
                    }
                    /// 6. Tính toán dame hỏa công
                    {
                        KMagicAttrib cFireDamage = new KMagicAttrib();
                        cFireDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_fire;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_fire)
                            {
                                cFireDamage.nValue[0] += nAngerATK;
                                cFireDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cFireDamage = CalcAttackDamage(PropertyDictionaryManager.GetFireDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_fire]);
                            cFireDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_fire;

                            ///% CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cFireDamage.nValue[0] += cFireDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_fire] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_fire]) / 100;
                            cFireDamage.nValue[2] += cFireDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_fire] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_fire]) / 100;
                        }

                        AppendDameDic.Add(cFireDamage.nAttribType, cFireDamage);
                    }
                    /// 7. Tính toán dane lôi công
                    {
                        KMagicAttrib cLightDamage = new KMagicAttrib();
                        cLightDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_light;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_fire)
                            {
                                cLightDamage.nValue[0] += nAngerATK;
                                cLightDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cLightDamage = CalcAttackDamage(PropertyDictionaryManager.GetLightDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_light]);
                            cLightDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_light;

                            ///  % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cLightDamage.nValue[0] += cLightDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_earth] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_earth]) / 100;
                            cLightDamage.nValue[2] += cLightDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_earth] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_earth]) / 100;
                        }

                        AppendDameDic.Add(cLightDamage.nAttribType, cLightDamage);
                    }

                    /// 8. Tính toán dame độc
                    {
                        KMagicAttrib cPoisonDamage = new KMagicAttrib();
                        cPoisonDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                        /// Nếu là sát thương nộ
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_wood)
                            {
                                KMagicAttrib angermagic = new KMagicAttrib();
                                angermagic.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                                angermagic.nValue[0] = nAngerATK * nSkillDamageT / 1000;
                                angermagic.nValue[1] = nAngerPoiTime;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, angermagic); //
                            }
                        }
                        else
                        {
                            cPoisonDamage.nValue[0] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[0] * nSkillPercent / 100 * nSkillDamageT / 1000 * nDamagePercent / 100;
                            cPoisonDamage.nValue[1] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[1];
                            cPoisonDamage.nValue[2] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[2] * nSkillPercent / 100 * nSkillDamageT / 1000 * nDamagePercent / 100;

                            if (cPoisonDamage.nValue[2] == 0)
                            {
                                cPoisonDamage.nValue[2] = cPoisonDamage.nValue[0] * cPoisonDamage.nValue[1];
                            }

                            KMagicAttrib magic = new KMagicAttrib();
                            magic.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                            if (bIsPhysical)// Nếu là skill vật lý thif tính độc ngoại công
                            {
                                // Tổng sát thương độc ngoại công
                                magic.nAttribType = client.m_CurrentPoisonDamage.nAttribType;
                                magic.nValue[0] = client.m_CurrentPoisonDamage.nValue[0] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                magic.nValue[1] = client.m_CurrentPoisonDamage.nValue[1];
                                magic.nValue[2] = client.m_CurrentPoisonDamage.nValue[2] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, magic);
                            }
                            else
                            {
                                // TỔng sát thương độc nội công
                                magic.nAttribType = client.m_MagicPoisonDamage.nAttribType;
                                magic.nValue[0] = client.m_MagicPoisonDamage.nValue[0] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                magic.nValue[1] = client.m_MagicPoisonDamage.nValue[1];
                                magic.nValue[2] = client.m_MagicPoisonDamage.nValue[2] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, magic);
                            }

                            /// % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cPoisonDamage.nValue[0] += cPoisonDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_wood] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_wood]) / 100;
                            cPoisonDamage.nValue[2] += cPoisonDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_wood] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_wood]) / 100;
                        }

                        // Nếu mà mà có dame độc
                        if (cPoisonDamage.nValue[0] > 0)
                        {
                            cPoisonDamage.nValue[1] += cPoisonDamage.nValue[1] * client.m_nPoisonTimeEnhanceP / 100;
                            if (cPoisonDamage.nValue[1] <= 0)
                            {
                                cPoisonDamage.nValue[0] = 0;
                                cPoisonDamage.nValue[1] = 0;
                                cPoisonDamage.nValue[2] = 0;
                            }
                        }

                        AppendDameDic.Add(cPoisonDamage.nAttribType, cPoisonDamage);
                    }
                    /// 9. TÍnh toán sát thương PHÉP
                    if (!bIsAnger)
                    {
                        KMagicAttrib cMagicDamage = new KMagicAttrib();
                        cMagicDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_magic;

                        cMagicDamage = CalcAttackDamage(PropertyDictionaryManager.GetMagicDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_magic]);

                        AppendDameDic.Add(cMagicDamage.nAttribType, cMagicDamage);
                    }
                    /// 10. Hút hít máu
                    {
                        KMagicAttrib cStealLife = new KMagicAttrib();
                        cStealLife.nAttribType = MAGIC_ATTRIB.magic_damage_append_steallife;
                        cStealLife.nValue[0] = PropertyDictionaryManager.GetLifeStealP(skillPd).nValue[0] + client.m_CurrentLifeStolen;
                        AppendDameDic.Add(cStealLife.nAttribType, cStealLife);
                    }

                    /// 11. Hút mana
                    {
                        KMagicAttrib cStealMana = new KMagicAttrib();
                        cStealMana.nAttribType = MAGIC_ATTRIB.magic_damage_append_stealmana;
                        cStealMana.nValue[0] = PropertyDictionaryManager.GetManaStealP(skillPd).nValue[0] + client.m_CurrentManaStolen;
                        AppendDameDic.Add(cStealMana.nAttribType, cStealMana);
                    }

                    /// 12. Hút thể lực
                    {
                        KMagicAttrib cStealStamina = new KMagicAttrib();
                        cStealStamina.nAttribType = MAGIC_ATTRIB.magic_damage_append_stealstamina;
                        cStealStamina.nValue[0] = PropertyDictionaryManager.GetStealStaminaP(skillPd).nValue[0] + client.m_CurrentStaminaStolen;
                        AppendDameDic.Add(cStealStamina.nAttribType, cStealStamina);
                    }
                }

                /// 13. Trạng thái hút
                {
                    KMagicAttrib cStealState = new KMagicAttrib();
                    cStealState.nAttribType = MAGIC_ATTRIB.magic_damage_append_stealstate;
                    cStealState.nValue[0] = PropertyDictionaryManager.GetStealState(skillPd).nValue[0];
                    cStealState.nValue[1] = PropertyDictionaryManager.GetStealState(skillPd).nValue[1];
                    cStealState.nValue[2] = PropertyDictionaryManager.GetStealState(skillPd).nValue[2];

                    AppendDameDic.Add(cStealState.nAttribType, cStealState);
                }

                /// 14.Apply toàn bộ trạng thái ngũ hành vào DIC
                List<KMagicAttrib> TotalEffectOfSkill = PropertyDictionaryManager.GetAllSkillEffectAppend(skillPd);

                foreach (KMagicAttrib _magic in TotalEffectOfSkill)
                {
                    KE_STATE _State = AlgorithmProperty.ConvertSkillEffectToState(_magic.nAttribType);
                    KMagicAttrib Final = AlgorithmProperty.AppendAttackAttrib(_magic, client, client.m_state[(int)_State], rcSkill);
                    AppendDameDic.Add(Final.nAttribType, Final);
                }

                int nSeries = rcSkill.Data.ElementalSeries;
                bool bIsMelee = rcSkill.Data.IsMelee;
                int dwSkillStyle = KTGlobal.GetSkillStyleDef(rcSkill.Data.SkillStyle);

                //for (int i = (int)MAGIC_ATTRIB.magic_state_effect_begin; i < (int)MAGIC_ATTRIB.magic_end; i++)
                //{
                //    if(AppendDameDic.ContainsKey(i))
                //    {
                //    }
                //}
                //100 : Hiệu suất tấn công.

                double MinDamge = 0;
                double MaxDamge = 0;
                int pAR = 0;

                foreach (KeyValuePair<MAGIC_ATTRIB, KMagicAttrib> entry in AppendDameDic)
                {
                    switch (entry.Value.nAttribType)
                    {
                        case MAGIC_ATTRIB.magic_damage_append_hitrate:
                            {
                                if (rcSkill.Data.IsUseAR)
                                {
                                    pAR = entry.Value.nValue[0];
                                }
                            }
                            break;

                        case MAGIC_ATTRIB.magic_damage_append_physics:
                        case MAGIC_ATTRIB.magic_damage_append_cold:
                        case MAGIC_ATTRIB.magic_damage_append_fire:
                        case MAGIC_ATTRIB.magic_damage_append_light:
                        case MAGIC_ATTRIB.magic_damage_append_magic:
                            {
                                MinDamge += entry.Value.nValue[0];
                                MaxDamge += entry.Value.nValue[2];
                            }
                            break;

                        case MAGIC_ATTRIB.magic_damage_append_poison:
                            {
                                int nValue = entry.Value.nValue[0];
                                int nPoisonDamage = entry.Value.nValue[2] * (100 + client.m_nPoisonTimeEnhanceP) / (9 * 100) + nValue;
                                MinDamge += nPoisonDamage;
                                MaxDamge += nPoisonDamage;
                            }
                            break;
                            // do something with entry.Value or entry.Key
                    }
                }

                if (MinDamge > 0 && MaxDamge > 0)
                {
                    /// Sát thương đầu ra
                    int nDamage = (int)((MinDamge + MaxDamge) / 2);
                    /// Mặc định sát thương chia 4 lần
                    nDamage /= 4;

                    /// Kết quả
                    return nDamage;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Đánh mục tiêu
        /// </summary>
        /// <param name="client"></param>
        /// <param name="enemy"></param>
        /// <param name="rcSkill"></param>
        /// <param name="additionDamagePercent"></param>
        /// <param name="skillPos"></param>
        public static bool AttackEnemy(GameObject client, GameObject enemy, SkillLevelRef rcSkill, int additionDamagePercent, UnityEngine.Vector2 skillPos, bool isSubPhrase)
        {
            /// Toác
            if (client == null || enemy == null || rcSkill == null)
            {
                return false;
            }

            /// Nếu đang có Buff tân thủ
            if (enemy is KPlayer && enemy.Buffs.HasBuff(200000))
            {
                if (client is KPlayer player)
                {
                    KTPlayerManager.ShowNotification(player, "Người chơi đang trong trạng thái bảo vệ, không thể tấn công!");
                    return false;
                }
            }

            /// Nếu đối tượng tấn công là người chơi
            if (client is KPlayer)
            {
                /// Nếu đối phương cũng là người chơi
                if (enemy is KPlayer)
                {
                    /// Nếu cấp độ bản thân dưới 30
                    if (client.m_Level < 30)
                    {
                        KTPlayerManager.ShowNotification(client as KPlayer, "Trên cấp 30 mới được đồ sát người chơi khác!");
                        return false;
                    }
                    /// Nếu cấp độ đối phương dưới 30
                    else if (enemy.m_Level < 30)
                    {
                        KTPlayerManager.ShowNotification(client as KPlayer, "Không thể đồ sát người chơi dưới cấp 30!");
                        return false;
                    }
                }
            }

            /// Nếu đang trong khu an toàn
            if (enemy is KPlayer)
            {
                KPlayer player = (KPlayer)enemy;
                if (player.IsInsideSafeZone)
                {
                    return false;
                }
            }

            try
            {
                /// Nếu không phải kỹ năng đánh luôn trúng
                if (!rcSkill.Data.AlwaysHit)
                {
                    /// Nếu đối tượng đang trong trạng thái khinh công thì bỏ qua
                    if (enemy.m_eDoing == KE_NPC_DOING.do_jump)
                    {
                        /// Tỷ lệ né sát thương khi khinh công
                        int flyDodgePercent = 70;
                        /// Tỷ lệ random
                        int randRate = KTGlobal.GetRandomNumber(1, 100);
                        /// Nếu có thể né được
                        if (randRate <= flyDodgePercent)
                        {
                            /// Bỏ qua
                            return false;
                        }
                    }
                }

                /// Tỷ lệ miễn nhiễm của đối phương
                int enemyImmuneToSkillP = enemy.GetImmuneToSkill(rcSkill.SkillID);
                /// Tỷ lệ miễn nhiễm
                if (enemyImmuneToSkillP > 0)
                {
                    /// Ngẫu nhiên
                    int nRand = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu miễn nhiễm
                    if (nRand <= enemyImmuneToSkillP)
                    {
                        /// Thông báo hóa giải
                        AlgorithmProperty.SyncAdjust(client, enemy);
                        /// Bỏ qua
                        return false;
                    }
                }

                /// Nếu cả 2 cùng là người chơi
                if (client is KPlayer attackerPlayer && enemy is KPlayer enemyPlayer)
                {
                    /// Quan hàm của thằng tấn công
                    int nAttackerChopLevel = attackerPlayer.m_ChopLevel;
                    /// Quan hàm của thằng bị tấn công
                    int nEnemyChopLevel = enemyPlayer.m_ChopLevel;

                    /// Độ lệch
                    int nDiff = nEnemyChopLevel - nAttackerChopLevel;
                    /// Nếu độ lệch dưới 0
                    if (nDiff < 0)
                    {
                        nDiff = 0;
                    }
                    /// Nếu độ lệch quá 10
                    if (nDiff > 10)
                    {
                        nDiff = 10;
                    }

                    /// Nếu có thể hóa giải
                    if (nDiff > 0)
                    {
                        /// Tỷ lệ hóa giải
                        int nAbsorbDamagesPercent = AlgorithmProperty.ChopAbsorbDamagesPercent[nDiff];
                        /// Nếu có thể hóa giải
                        if (KTGlobal.GetRandomNumber(1, 100) <= nAbsorbDamagesPercent)
                        {
                            /// Thông báo hóa giải
                            AlgorithmProperty.SyncAdjust(client, enemy);
                            /// Bỏ qua
                            return false;
                        }
                    }
                }

                /// Nếu là kỹ năng ngoại
                if (rcSkill.Data.IsPhysical)
                {
                    /// Nếu đối phương miễn nhiễm sát thương ngoại
                    if (enemy.m_ImmuneToPhysicDamage)
                    {
                        /// Thông báo hóa giải
                        AlgorithmProperty.SyncAdjust(client, enemy);
                        /// Bỏ qua
                        return false;
                    }
                }
                /// Nếu là kỹ năng nội
                else
                {
                    /// Nếu đối phương miễn nhiễm sát thương nội
                    if (enemy.m_ImmuneToMagicDamage)
                    {
                        /// Thông báo hóa giải
                        AlgorithmProperty.SyncAdjust(client, enemy);
                        /// Bỏ qua
                        return false;
                    }
                }

                /// Danh sách sát thương
                Dictionary<MAGIC_ATTRIB, KMagicAttrib> AppendDameDic = new Dictionary<MAGIC_ATTRIB, KMagicAttrib>();

                bool IsDeadlyStrike = false;

                /// % sát thương cộng thêm từ kỹ năng khác
                int nDamageAddPercent = AlgorithmProperty.GetAddSkillDamagePercent(client, rcSkill.Data.ID);
                /// Cộng thêm % sát thương từ ngoài vào (có thể do xuyên suốt mục tiêu)
                nDamageAddPercent += additionDamagePercent;
                /// Sát thương cộng thêm từ kỹ năng khác
                int nDamageAdd = AlgorithmProperty.GetAddSkillDamage(client, rcSkill.Data.ID);

                /// Property Dictionary của kỹ năng
                PropertyDictionary skillPd = rcSkill.Properties.Clone();

                /// Cộng toàn bộ thuộc tính hỗ trợ
                if (client is KPlayer)
                {
                    //Console.WriteLine("SkillPD before = " + skillPd.ToString());
                    KPlayer player = client as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(rcSkill.SkillID);
                    if (enchantPd != null)
                    {
                        skillPd.AddProperties(enchantPd);
                    }
                    //Console.WriteLine("SkillPD after = " + skillPd.ToString());
                }

                /// Check xem có phải skill dame vật lý không
                bool bIsPhysical = rcSkill.Data.IsPhysical;

                /// Tỷ lệ thiệt hại ban đầu
                int nDamagePercent = 100;

                /// Phát huy lực tấn công kỹ năng
                int m_nSkillSelfDamagePTrim = client.m_nSkillSelfDamagePTrim;

                // Console.Write("client.m_nSkillDamageTrimPByLifeLoss :" + client.m_nSkillDamageTrimPByLifeLoss);
                /// Nếu có dòng phát huy lực tấn công kỹ năng với mỗi 1% sinh lực mất từ thời điểm kích hoạt ( VIẾT THÊM)
                if (client.m_nSkillDamageTrimPByLifeLoss > 0)
                {
                    // Console.WriteLine("client.m_nSkillDamageTrimPByLifeLoss :" + client.m_nSkillDamageTrimPByLifeLoss);
                    // Nếu lượng máu hiện tại thấp hơn lượng máu từ lúc kích hoạt kỹ năng
                    if (client.m_CurrentLife < client.m_nLastLifeSkillDamageTrimP)
                    {
                        /// Lượng máu đã mất (%)
                        /// Số máu đã mất
                        ///

                        //Total HP LOSE
                        int HPLOSE = client.m_CurrentLife - client.m_nLastLifeSkillDamageTrimP;

                        //Console.WriteLine("HP LOSE :" + HPLOSE);

                        if (HPLOSE < 0)
                        {
                            float PERCENTLOSE = Math.Abs(HPLOSE) * 100 / client.m_CurrentLifeMax;

                            //Console.WriteLine("PERCENTLOSE :" + PERCENTLOSE);

                            int DamageAdd = (int)(PERCENTLOSE * client.m_nSkillDamageTrimPByLifeLoss) * 10;

                            // Console.WriteLine("BEFORE :" + m_nSkillSelfDamagePTrim);
                            /// Lực tấn công kỹ năng cộng thêm
                            m_nSkillSelfDamagePTrim += DamageAdd;

                            // Console.WriteLine("AFFTER :" + m_nSkillSelfDamagePTrim);
                        }
                    }
                }

                /// Phát huy lực tấn công cơ bản :  Lấy từ các skill bị động + được apply vào nhân vật
                int m_nSkillDamagePTrim = client.m_nSkillDamagePTrim;

                if (client is KPlayer)
                {
                    m_nSkillSelfDamagePTrim += ((KPlayer)client).Skills.GetAppendDamageToSkill(rcSkill.SkillID);
                }

                int nSkillDamageT = PropertyDictionaryManager.GetSkillDamageP(skillPd) * 10 + PropertyDictionaryManager.GetSkillDamageP(skillPd) * m_nSkillSelfDamagePTrim * 10 / 100;  // Dame từ Skill
                int nAppendDamageT = PropertyDictionaryManager.GetAppendDamageP(skillPd) * 10 + PropertyDictionaryManager.GetAppendDamageP(skillPd) * m_nSkillDamagePTrim * 10 / 100;   //
                int nSkillDamageTrimP = PropertyDictionaryManager.GetSkillDamageP(skillPd) * m_nSkillSelfDamagePTrim / 100;
                int nAngerATK = 0;
                int nAngerPoiTime = 0;
                // Nếu thằng đánh bị suy yếu thì dame sẽ bị giảm
                if (client.HaveState(KE_STATE.emSTATE_WEAK))
                {
                    int nNewPercent = nDamagePercent * KTGlobal.WeakDamagePercent / 100;
                    nDamagePercent = nNewPercent;
                }

                /// 1. Nếu skill có sử dụng tỉ lệ chính xác
                if (rcSkill.Data.IsUseAR)
                {
                    /// Tạo mới KMagicAttribute
                    KMagicAttrib attrib = new KMagicAttrib();
                    /// Loại là tỷ lệ đánh trúng
                    attrib.nAttribType = MAGIC_ATTRIB.magic_damage_append_hitrate;

                    /// Chính xác ban đầu
                    int hitValue = 0;
                    /// Nếu kỹ năng có dòng % tỷ lệ đánh trúng
                    if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_attackrating_p))
                    {
                        /// Chính xác cơ bản
                        int m_AttackRating = client.m_AttackRating;
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating + m_AttackRating * PropertyDictionaryManager.GetHiteRateP(skillPd) / 100;
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }
                    /// Nếu kỹ năng có dòng tăng điểm đánh trúng
                    else if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_attackrating_v))
                    {
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating + PropertyDictionaryManager.GetHiteRateV(skillPd);
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }
                    /// Nếu kỹ năng không có chính xác
                    else
                    {
                        /// Chính xác hiện có
                        int m_CurrentAttackRating = client.m_CurrentAttackRating;
                        /// Giá trị chính xác mới
                        hitValue = m_CurrentAttackRating;
                        /// Lưu lại giá trị vào KMagicAttribute
                        attrib.nValue[0] = hitValue;
                    }

                    /// Bỏ qua né tránh đối thủ
                    int m_CurrentIgnoreDefense = client.m_CurrentIgnoreDefense;
                    /// Tỷ lệ % bỏ qua né tránh đối thủ
                    int m_CurrentIgnoreDefensePercent = client.m_CurrentIgnoreDefensePercent;

                    /// Lưu lại vào KMagicAttribute
                    attrib.nValue[1] = PropertyDictionaryManager.GetIgnoreDefenseV(skillPd) + m_CurrentIgnoreDefense;
                    attrib.nValue[2] = PropertyDictionaryManager.GetIgnoreDefenseP(skillPd) + m_CurrentIgnoreDefensePercent;

                    /// Thêm vào ProDict
                    AppendDameDic.Add(attrib.nAttribType, attrib);
                }

                if (rcSkill.Data.IsDamageSkill)
                {
                    /// 1. TÍnh dame nộ nếu skill support
                    int AngerDamage = PropertyDictionaryManager.GetAngerDamage(skillPd);

                    bool bIsAnger = false;

                    if (AngerDamage > 0)
                    {
                        bIsAnger = true;
                        int nBaseAngerATK = KPlayerSetting.GetLevelBaseAngerATK(client.m_Level);

                        nAngerATK = (int)((AngerDamage / 100.0) * nBaseAngerATK) * nDamagePercent / 100 * nSkillDamageT / 1000;

                        nAngerPoiTime = KTGlobal.AngerTime;
                    }

                    /// 2. Sát thương ngũ hành

                    if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_trice_eff_seriesdamage_r)) // Nếu skill có chứa sát thương ngũ hành
                    {
                        //FILL CHỈ SỐ VÀO ĐÂY
                        // Dame khắc hệ của nhân vật
                        int m_CurrentSeriesConquar = client.m_CurrentSeriesConquar;  // SIMBOY : magic_seriesconquar_r
                        int m_CurrentSeriesEnhance = client.m_CurrentSeriesEnhance;  // SIMBOY : magic_seriesenhance_r  Tấn công người chơi khắc hệ, trị tương khắc của kỹ năng

                        int cSrcSeriesDamage = PropertyDictionaryManager.GetSeriesDamage(skillPd);

                        KMagicAttrib cSeriesDamage = new KMagicAttrib();

                        cSeriesDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_series;

                        cSeriesDamage.nValue[0] = cSrcSeriesDamage + m_CurrentSeriesConquar;
                        cSeriesDamage.nValue[1] = m_CurrentSeriesEnhance;

                        AppendDameDic.Add(cSeriesDamage.nAttribType, cSeriesDamage);
                    }

                    /// 3. Tính toán sát thương chí mạng ( VIẾT THÊM CỘNG VỚI KỸ NĂNG VỐN CÓ NỮA)
                    float m_CurrentFatallyStrike = (1 + (client.m_CurrentFatallyStrike + PropertyDictionaryManager.GetFatalStrikeEnhanceP(skillPd).nValue[0]) / 100f) * (client.m_CurrentDeadlyStrike + PropertyDictionaryManager.GetFatalStrikeEnhance(skillPd).nValue[0]);
                    /// Tỷ lệ ngẫu nhiên
                    int randCritPercent = KTGlobal.GetRandomNumber(1, 100);
                    /// Tỷ lệ tính toán cuối cùng
                    float critRate = (m_CurrentFatallyStrike / KTGlobal.DeadlyStrikeBaseRate) * 100;
                    /// Nếu đánh chí mạng
                    if (critRate > 0 && critRate > randCritPercent)
                    {
                        KMagicAttrib cFatallyStrike = new KMagicAttrib();
                        cFatallyStrike.nAttribType = MAGIC_ATTRIB.magic_damage_append_fatallystrike;
                        cFatallyStrike.nValue[0] = 1;
                        IsDeadlyStrike = true;
                        AppendDameDic.Add(cFatallyStrike.nAttribType, cFatallyStrike);
                    }

                    ///// 3. Tính toán sát thương chí mạng -   VIẾT LẠI - Võ lâm không có điểm chí mạng, chỉ có % thôi
                    int critPercent = client.m_CurrentFatallyStrike + PropertyDictionaryManager.GetFatalStrikeEnhanceP(skillPd).nValue[0];
                 
                    /// Nếu đánh chí mạng
                    if (randCritPercent <= critPercent)
                    {
                        KMagicAttrib cFatallyStrike = new KMagicAttrib();
                        cFatallyStrike.nAttribType = MAGIC_ATTRIB.magic_damage_append_fatallystrike;
                        cFatallyStrike.nValue[0] = 1;
                        IsDeadlyStrike = true;
                        AppendDameDic.Add(cFatallyStrike.nAttribType, cFatallyStrike);
                    }

                    /// 4. Tính toán dame vật lý
                    {
                        KMagicAttrib cPhysicsDamage = new KMagicAttrib();
                        cPhysicsDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_physics;

                        int nMinDamage = nDamageAdd;
                        int nMaxDamage = nDamageAdd;

                        if (bIsAnger) // NẾU CÓ DAME NỘ
                        {
                            // NGŨ HÀNH CỦA NHÂN VẬT
                            // FILL CHỈ SỐ VÀO ĐÂY
                            KE_SERIES_TYPE m_Series = client.m_Series;

                            if (nAngerATK > 0 && (m_Series == KE_SERIES_TYPE.series_metal || m_Series == KE_SERIES_TYPE.series_none))
                            {
                                cPhysicsDamage.nValue[0] += nAngerATK;
                                cPhysicsDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            /// Nếu là sát thương ngoại công
                            if (bIsPhysical)
                            {
                                /// Vật công ngoại Min
                                nMinDamage = client.m_PhysicsDamage.nValue[0];
                                /// Vật công ngoại Max
                                nMaxDamage = client.m_PhysicsDamage.nValue[2];

                                /// Vật công ngoại Min
                                nMinDamage = client.m_PhysicsDamage.nValue[0];
                                /// Vật công ngoại Max
                                nMaxDamage = client.m_PhysicsDamage.nValue[2];

                                /// Đánh dấu có phải người chơi không
                                bool bIsPlayer = client is KPlayer;
                                /// Người chơi tương ứng
                                KPlayer player = client as KPlayer;

                                /// Sát thương cộng thêm từ ngoài nếu là người chơi
                                if (bIsPlayer)
                                {
                                    int nEnhance = AlgorithmProperty.GetPhysicsEnhance((KPlayer)client);
                                    nMinDamage += nMinDamage * nEnhance / 100;
                                    nMaxDamage += nMaxDamage * nEnhance / 100;
                                }

                                /// Sát thương cộng thêm %
                                nMinDamage += (client.m_PhysicsDamage.nValue[0] * client.m_CurrentPhysicsDamageEnhanceP / 100);
                                nMaxDamage += (client.m_PhysicsDamage.nValue[2] * client.m_CurrentPhysicsDamageEnhanceP / 100);

                                /// Sát thương cơ bản của vũ khí
                                nMinDamage += (client.m_PhysicPhysic.nValue[0] + client.m_nWeaponBaseDamageTrim);
                                nMaxDamage += (client.m_PhysicPhysic.nValue[2] + client.m_nWeaponBaseDamageTrim);

                                /// Nếu là người chơi
                                if (bIsPlayer)
                                {
                                    ///  VIẾT THÊM tăng vật công cơ bản dựa theo số Ngoại hiện tại
                                    nMinDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                    nMaxDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                }

                                /// Sát thương cộng thêm từ kỹ năng khác
                                nMinDamage = nMinDamage * nAppendDamageT / 1000;
                                nMaxDamage = nMaxDamage * nAppendDamageT / 1000;

                                /// Biến tạm
                                int nTemp = 0;

                                /// Kỹ năng hỗ trợ
                                int GetPhysicsDamageEnhanceP = PropertyDictionaryManager.GetPhysicsDamageEnhanceP(skillPd);
                                int GetPhysicsDamageEnhanceV = PropertyDictionaryManager.GetPhysicsDamageEnhanceV(skillPd);

                                /// % Sát thương từ các kỹ năng hỗ trợ
                                if (GetPhysicsDamageEnhanceP > 0)
                                {
                                    nTemp = (client.m_PhysicsDamage.nValue[0] * GetPhysicsDamageEnhanceP / 100);
                                }
                                /// Sát thương từ kỹ năng hỗ trợ
                                else if (GetPhysicsDamageEnhanceV > 0)
                                {
                                    nTemp = client.m_PhysicsDamage.nValue[2] + GetPhysicsDamageEnhanceV;
                                }

                                /// Tăng thêm ngoại công do cái gì khác
                                int nTempMin = nTemp + PropertyDictionaryManager.GetPhysicsTrimDamage(skillPd).nValue[0];
                                int nTempMax = nTemp + PropertyDictionaryManager.GetPhysicsTrimDamage(skillPd).nValue[2];

                                /// % sát thương cộng thêm
                                nTempMin += nTempMin * nDamageAddPercent / 100;
                                nTempMax += nTempMax * nDamageAddPercent / 100;
                                nTempMin *= nSkillDamageT / 1000;
                                nTempMax *= nSkillDamageT / 1000;

                                nMinDamage += nTempMin;
                                nMaxDamage += nTempMax;

                                /// Kết quả cuối cùng
                                nMinDamage += (client.m_AddPhysicsDamage * nAppendDamageT / 1000);
                                nMaxDamage += (client.m_AddPhysicsDamage * nAppendDamageT / 1000);
                            }
                            /// Nếu là sát thương nội công
                            else
                            {
                                /// Vật công nội Min
                                nMinDamage = client.m_MagicDamage.nValue[0];
                                /// Vật công nội Max
                                nMaxDamage = client.m_MagicDamage.nValue[2];

                                /// % vật công nội từ vũ khí
                                nMinDamage += (client.m_MagicDamage.nValue[0] * client.m_CurrentMagicPhysicsEnhanceP / 100);
                                nMaxDamage += (client.m_MagicDamage.nValue[2] * client.m_CurrentMagicPhysicsEnhanceP / 100);

                                /// Sát thương cơ bản của vũ khí
                                nMinDamage += (client.m_PhysicsMagic.nValue[0] + client.m_nWeaponBaseDamageTrim);
                                nMaxDamage += (client.m_PhysicsMagic.nValue[2] + client.m_nWeaponBaseDamageTrim);

                                /// Đánh dấu có phải người chơi không
                                bool bIsPlayer = client is KPlayer;
                                /// Người chơi tương ứng
                                KPlayer player = client as KPlayer;

                                /// Nếu là người chơi
                                if (bIsPlayer)
                                {
                                    ///  VIẾT THÊM tăng vật công cơ bản dựa theo số Ngoại hiện tại
                                    nMinDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                    nMaxDamage += player.m_nAddWeaponBaseDamageTrimByVitality * player.GetCurVitality();
                                }

                                /// Sát thương cộng thêm từ kỹ năng hỗ trợ
                                nMinDamage = nMinDamage * nAppendDamageT / 1000;
                                nMaxDamage = nMaxDamage * nAppendDamageT / 1000;

                                /// Biến tạm
                                int nTemp = 0;

                                /// Kỹ năng hỗ trợ
                                int GetPhysicsDamageEnhanceP = PropertyDictionaryManager.GetPhysicsDamageEnhanceP(skillPd);
                                int GetPhysicsDamageEnhanceV = PropertyDictionaryManager.GetPhysicsDamageEnhanceV(skillPd);

                                /// % Sát thương từ các kỹ năng hỗ trợ
                                if (GetPhysicsDamageEnhanceP > 0)
                                {
                                    nTemp = (client.m_MagicDamage.nValue[0] * GetPhysicsDamageEnhanceP / 100);
                                }
                                /// Sát thương từ kỹ năng hỗ trợ
                                else if (GetPhysicsDamageEnhanceV > 0)
                                {
                                    nTemp = client.m_MagicDamage.nValue[0] + GetPhysicsDamageEnhanceV;
                                }

                                /// Tăng thêm nội công do cái gì khác
                                int nTempMin = nTemp + PropertyDictionaryManager.GetMagicTrimDamage(skillPd).nValue[0];
                                int nTempMax = nTemp + PropertyDictionaryManager.GetMagicTrimDamage(skillPd).nValue[2];

                                /// % sát thương cộng thêm
                                nTempMin += nTempMin * nDamageAddPercent / 100;
                                nTempMax += nTempMax * nDamageAddPercent / 100;
                                nTempMin *= nSkillDamageT / 1000;
                                nTempMax *= nSkillDamageT / 1000;

                                nMinDamage += nTempMin;
                                nMaxDamage += nTempMax;

                                /// Kết quả cuối cùng
                                nMinDamage += (client.m_MagicPhysicsDamage.nValue[0] * nAppendDamageT / 1000);
                                nMaxDamage += (client.m_MagicPhysicsDamage.nValue[2] * nAppendDamageT / 1000);
                            }

                            /// Nhận thêm sát thương bởi dame phép
                            if (client.m_nAttackAddedByMana > 0)
                            {
                                nMinDamage += client.m_CurrentMana * client.m_nAttackAddedByMana / 100;
                                nMaxDamage += client.m_CurrentMana * client.m_nAttackAddedByMana / 100;
                            }

                            cPhysicsDamage.nValue[0] = nMinDamage * nDamagePercent / 100;
                            cPhysicsDamage.nValue[2] = nMaxDamage * nDamagePercent / 100;
                        }

                        AppendDameDic.Add(cPhysicsDamage.nAttribType, cPhysicsDamage);
                    }

                    int nSkillPercent = nDamageAddPercent + 100; //% sức mạnh kỹ năng

                    /// 5. Tính toán dame băng công
                    {
                        KMagicAttrib cColdDamage = new KMagicAttrib();
                        cColdDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_cold;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_water)
                            {
                                cColdDamage.nValue[0] += nAngerATK;
                                cColdDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cColdDamage = CalcAttackDamage(PropertyDictionaryManager.GetColdDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_cold]);
                            cColdDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_cold;

                            ///  VIẾT THÊM - % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cColdDamage.nValue[0] += cColdDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_water] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_water]) / 100;
                            cColdDamage.nValue[2] += cColdDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_water] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_water]) / 100;
                        }

                        AppendDameDic.Add(cColdDamage.nAttribType, cColdDamage);
                    }
                    /// 6. Tính toán dame hỏa công
                    {
                        KMagicAttrib cFireDamage = new KMagicAttrib();
                        cFireDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_fire;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_fire)
                            {
                                cFireDamage.nValue[0] += nAngerATK;
                                cFireDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cFireDamage = CalcAttackDamage(PropertyDictionaryManager.GetFireDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_fire]);
                            cFireDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_fire;

                            ///  VIẾT THÊM - % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cFireDamage.nValue[0] += cFireDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_fire] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_fire]) / 100;
                            cFireDamage.nValue[2] += cFireDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_fire] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_fire]) / 100;
                        }

                        AppendDameDic.Add(cFireDamage.nAttribType, cFireDamage);
                    }
                    /// 7. Tính toán dane lôi công
                    {
                        KMagicAttrib cLightDamage = new KMagicAttrib();
                        cLightDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_light;
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_fire)
                            {
                                cLightDamage.nValue[0] += nAngerATK;
                                cLightDamage.nValue[2] += nAngerATK;
                            }
                        }
                        else
                        {
                            cLightDamage = CalcAttackDamage(PropertyDictionaryManager.GetLightDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_light]);
                            cLightDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_light;

                            ///  VIẾT THÊM - % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cLightDamage.nValue[0] += cLightDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_earth] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_earth]) / 100;
                            cLightDamage.nValue[2] += cLightDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_earth] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_earth]) / 100;
                        }

                        AppendDameDic.Add(cLightDamage.nAttribType, cLightDamage);
                    }

                    /// 8. Tính toán dame độc
                    {
                        KMagicAttrib cPoisonDamage = new KMagicAttrib();
                        cPoisonDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                        /// Nếu là sát thương nộ
                        if (bIsAnger)
                        {
                            if (nAngerATK > 0 && client.m_Series == KE_SERIES_TYPE.series_wood)
                            {
                                KMagicAttrib angermagic = new KMagicAttrib();
                                angermagic.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                                angermagic.nValue[0] = nAngerATK * nSkillDamageT / 1000;
                                angermagic.nValue[1] = nAngerPoiTime;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, angermagic); //
                            }
                        }
                        else
                        {
                            cPoisonDamage.nValue[0] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[0] * nSkillPercent / 100 * nSkillDamageT / 1000 * nDamagePercent / 100;
                            cPoisonDamage.nValue[1] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[1];
                            cPoisonDamage.nValue[2] = PropertyDictionaryManager.GetPoisonDamage(skillPd).nValue[2] * nSkillPercent / 100 * nSkillDamageT / 1000 * nDamagePercent / 100;

                            if (cPoisonDamage.nValue[2] == 0)
                            {
                                cPoisonDamage.nValue[2] = cPoisonDamage.nValue[0] * cPoisonDamage.nValue[1];
                            }

                            KMagicAttrib magic = new KMagicAttrib();
                            magic.nAttribType = MAGIC_ATTRIB.magic_damage_append_poison;
                            if (bIsPhysical)// Nếu là skill vật lý thif tính độc ngoại công
                            {
                                // Tổng sát thương độc ngoại công
                                magic.nAttribType = client.m_CurrentPoisonDamage.nAttribType;
                                magic.nValue[0] = client.m_CurrentPoisonDamage.nValue[0] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                magic.nValue[1] = client.m_CurrentPoisonDamage.nValue[1];
                                magic.nValue[2] = client.m_CurrentPoisonDamage.nValue[2] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, magic);
                            }
                            else
                            {
                                // TỔng sát thương độc nội công
                                magic.nAttribType = client.m_MagicPoisonDamage.nAttribType;
                                magic.nValue[0] = client.m_MagicPoisonDamage.nValue[0] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                magic.nValue[1] = client.m_MagicPoisonDamage.nValue[1];
                                magic.nValue[2] = client.m_MagicPoisonDamage.nValue[2] * nDamagePercent / 100 * nAppendDamageT / 1000;
                                cPoisonDamage = MixPoisonDamage(cPoisonDamage, magic);
                            }

                            ///  VIẾT THÊM - % CÔNG KÍCH NGŨ HÀNH TƯƠNG ỨNG
                            cPoisonDamage.nValue[0] += cPoisonDamage.nValue[0] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_wood] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_wood]) / 100;
                            cPoisonDamage.nValue[2] += cPoisonDamage.nValue[2] * (bIsPhysical ? client.SeriesEnhanceDamageP[KE_SERIES_TYPE.series_wood] : client.SeriesEnhanceMagicP[KE_SERIES_TYPE.series_wood]) / 100;
                        }

                        // Nếu mà mà có dame độc
                        if (cPoisonDamage.nValue[0] > 0)
                        {
                            cPoisonDamage.nValue[1] += cPoisonDamage.nValue[1] * client.m_nPoisonTimeEnhanceP / 100;
                            if (cPoisonDamage.nValue[1] <= 0)
                            {
                                cPoisonDamage.nValue[0] = 0;
                                cPoisonDamage.nValue[1] = 0;
                                cPoisonDamage.nValue[2] = 0;
                            }
                        }

                        /// Lượng hóa giải sát thương độc
                        int enemyAbsorbPoisonDamage = enemy.m_nPosionWeakenPoint;
                        /// % tối đa sát thương ban đầu hóa giải được
                        int enemyAbsorbPoisonDamageMaxBaseDamageP = enemy.m_nPoisonWeakenMaxDamageP;

                        /// Nếu có thể hóa giải sát thương
                        if (enemyAbsorbPoisonDamage > 0 && enemyAbsorbPoisonDamageMaxBaseDamageP > 0)
                        {
                            /// Sát thương tối đa có thể hóa giải được
                            int maxAbsorbPoisonDamage_Min = enemyAbsorbPoisonDamageMaxBaseDamageP * cPoisonDamage.nValue[0] / 100;
                            int maxAbsorbPoisonDamage_Max = enemyAbsorbPoisonDamageMaxBaseDamageP * cPoisonDamage.nValue[2] / 100;

                            /// Tính toán lại lượng sát thương độc gây ra
                            cPoisonDamage.nValue[0] -= Math.Min(enemyAbsorbPoisonDamage, maxAbsorbPoisonDamage_Min);
                            cPoisonDamage.nValue[2] -= Math.Min(enemyAbsorbPoisonDamage, maxAbsorbPoisonDamage_Max);
                        }

                        AppendDameDic.Add(cPoisonDamage.nAttribType, cPoisonDamage);
                    }
                    /// 9. TÍnh toán sát thương PHÉP
                    if (!bIsAnger)
                    {
                        KMagicAttrib cMagicDamage = new KMagicAttrib();
                        cMagicDamage.nAttribType = MAGIC_ATTRIB.magic_damage_append_magic;

                        cMagicDamage = CalcAttackDamage(PropertyDictionaryManager.GetMagicDamage(skillPd), nSkillPercent, bIsPhysical, nDamagePercent, nSkillDamageT, nAppendDamageT, client.m_damage[(int)DAMAGE_TYPE.damage_magic]);

                        AppendDameDic.Add(cMagicDamage.nAttribType, cMagicDamage);
                    }

                    /// 10. Hút máu
                    {
                        KMagicAttrib cStealLife = new KMagicAttrib();
                        cStealLife.nAttribType = MAGIC_ATTRIB.magic_damage_append_steallife;
                        cStealLife.nValue[0] = PropertyDictionaryManager.GetLifeStealP(skillPd).nValue[0] + client.m_CurrentLifeStolen;
                        AppendDameDic.Add(cStealLife.nAttribType, cStealLife);
                    }

                    /// 11. Hút mana
                    {
                        KMagicAttrib cStealMana = new KMagicAttrib();
                        cStealMana.nAttribType = MAGIC_ATTRIB.magic_damage_append_stealmana;
                        cStealMana.nValue[0] = PropertyDictionaryManager.GetManaStealP(skillPd).nValue[0] + client.m_CurrentManaStolen;
                        AppendDameDic.Add(cStealMana.nAttribType, cStealMana);
                    }

                    /// 12. Hút thể lực
                    {
                        KMagicAttrib cStealStamina = new KMagicAttrib();
                        cStealStamina.nAttribType = MAGIC_ATTRIB.magic_damage_append_stealstamina;
                        cStealStamina.nValue[0] = PropertyDictionaryManager.GetStealStaminaP(skillPd).nValue[0] + client.m_CurrentStaminaStolen;
                        AppendDameDic.Add(cStealStamina.nAttribType, cStealStamina);
                    }
                }

                /// 13.Apply toàn bộ trạng thái ngũ hành vào DIC
                List<KMagicAttrib> TotalEffectOfSkill = PropertyDictionaryManager.GetAllSkillEffectAppend(skillPd);

                foreach (KMagicAttrib _magic in TotalEffectOfSkill)
                {
                    KE_STATE _State = AlgorithmProperty.ConvertSkillEffectToState(_magic.nAttribType);
                    KMagicAttrib Final = AlgorithmProperty.AppendAttackAttrib(_magic, client, client.m_state[(int)_State], rcSkill);
                    AppendDameDic.Add(Final.nAttribType, Final);
                }

                int nSeries = rcSkill.Data.ElementalSeries;
                bool bIsMelee = rcSkill.Data.IsMelee;
                int dwSkillStyle = KTGlobal.GetSkillStyleDef(rcSkill.Data.SkillStyle);

                //100 : Hiệu suất tấn công.
                return ReceiveDamage(client, enemy, nSeries, bIsMelee, AppendDameDic, dwSkillStyle, IsDeadlyStrike, rcSkill, 100, skillPd, skillPos, isSubPhrase);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Thực hiện chịu sát thương
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="nSeries"></param>
        /// <param name="bIsMelee"></param>
        /// <param name="AppendDameDic"></param>
        /// <param name="dwSkillStyle"></param>
        /// <param name="bIsDeadlyStrike"></param>
        /// <param name="rcSkill"></param>
        /// <param name="nAdditionP"></param>
        /// <param name="SkillPD"></param>
        /// <param name="skillPos"></param>
        /// <returns></returns>
        private static bool ReceiveDamage(GameObject attacker, GameObject target, int nSeries, bool bIsMelee, Dictionary<MAGIC_ATTRIB, KMagicAttrib> AppendDameDic, int dwSkillStyle, bool bIsDeadlyStrike, SkillLevelRef rcSkill, int nAdditionP, PropertyDictionary SkillPD, UnityEngine.Vector2 skillPos, bool isSubPhrase)
        {
            /// Nếu mục tiêu đang trong trạng thái miễn nhiếm toàn bộ sát thương và hiệu ứng
            if (target.IsImmuneToAllDamageAndStates())
            {
                if (KTGlobal.IsTestModel)
                {
                    if (attacker is KPlayer)
                    {
                        KPlayer _Test = attacker as KPlayer;
                        KTPlayerManager.ShowNotification(_Test, "Đối phương đang trong trại thái khống chế nên đnáh misss");
                    }
                }

                /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                AlgorithmProperty.SyncMiss(attacker, target);
                return false;
            }

            /// Khởi tạo biến break khỏi loop
            bool bBreak = false;

            /// Đoạn này bảo vệ người chơi ko bị chịu sát thương trong thời gian bảo vệ.Hoặc bỏ qua toàn bộ tấn công.
            if (target.m_nIgnoreAttackOnTime > 0 && rcSkill.Data.IsDamageSkill)
            {
                /// Thời điểm Tick bỏ qua công kích lần trước
                long lastIgnoreAttackTick = target.m_nLastIgnoreAttackTime;
                /// Thời gian hiện tại của hệ thống
                long currentTick = KTGlobal.GetCurrentTimeMilis();
                /// Khoảng thời gian bỏ qua công kích
                long ignoreAttackTick = (long)(target.m_nIgnoreAttackOnTime / 18f * 1000);
                /// Khoảng thời gian đã trôi qua
                long nIntervalTime = currentTick - lastIgnoreAttackTick;
                /// Khoảng thời gian duy trì hiệu ứng bỏ qua công kích
                long ignoreAttackDurationTick = (long)(target.m_nIgnoreAttackDuration / 18f * 1000);

                /// Nếu đã đến thời điểm bỏ qua công kích
                if (nIntervalTime >= ignoreAttackTick)
                {
                    /// Nếu đã nằm ngoài khoảng bỏ qua công kích
                    if (nIntervalTime - ignoreAttackTick > ignoreAttackDurationTick)
                    {
                        /// Cập nhật thời điểm bỏ qua nửa giây công kích
                        target.m_nLastIgnoreAttackTime = KTGlobal.GetCurrentTimeMilis();
                    }
                    else
                    {
                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "Bỏ qua sát thương của đối phương vì vẫn trong thời gian bảo vệ");
                            }
                        }

                        /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
            }

            /// Nếu là cọc gỗ của Bot
            if (target is Monster mons && !string.IsNullOrEmpty(mons.Tag) && mons.Tag == "DecoBotColumn")
            {
                /// Không đánh được
                return false;
            }

            /// Nếu không phải là kỹ năng đánh luôn trúng
            if (!rcSkill.Data.AlwaysHit)
            {
                /// Nếu là damage ngoại
                if (rcSkill.Data.IsPhysical)
                {
                    int nRand = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu mục tiêu có thể tránh công ngoại
                    if (nRand <= target.m_sIgnorePhysicDamage)
                    {
                        //if (target is KPlayer)
                        //{
                        //    PlayerManager.ShowNotification(target as KPlayer, "Ignore Physic ATTACK");
                        //}

                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "Miss vì đối phương có bỏ qua sát thương vật lý");
                            }
                        }

                        /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
                /// Nếu là damage nội
                else
                {
                    int nRand = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu mục tiêu có thể tránh công nội
                    if (nRand <= target.m_sIgnoreMagicDamage)
                    {
                        //if (target is KPlayer)
                        //{
                        //    PlayerManager.ShowNotification(target as KPlayer, "Ignore Magic ATTACK");
                        //}

                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "Miss vì đối phương có bỏ qua sát thương nội");
                            }
                        }
                        /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
            }

            /// Nếu đối phương có dòng xác suất tránh công tầm xa hoặc gần
            if (target.m_sIgnoreMeleeDamage > 0 || target.m_sIgnoreRangerDamage > 0)
            {
                /// Khoảng cách với đối phương
                float distanceToEnemy = KTGlobal.GetDistanceBetweenGameObjects(attacker, target);
                /// Nếu dưới 300 là tầm gần
                if (distanceToEnemy <= 300)
                {
                    int nRand = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu mục tiêu có thể tránh công
                    if (nRand <= target.m_sIgnoreMeleeDamage)
                    {
                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "Miss vì đối phương có dòng xác suất tránh công tầm xa hoặc gần");
                            }
                        }
                        /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
                /// Nếu là tầm xa
                else
                {
                    int nRand = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu mục tiêu có thể tránh công
                    if (nRand <= target.m_sIgnoreRangerDamage)
                    {
                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "Miss vì đối phương có dòng xác suất tránh công tầm xa hoặc gần");
                            }
                        }
                        /// GỬI LẠI PACKET MISS CHO NGƯỜI CHƠI
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
            }

            /// Nếu không phải kỹ năng đánh luôn trúng
            if (!rcSkill.Data.AlwaysHit)
            {
                /// Nếu có dòng tỷ lệ đánh trúng
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_hitrate))
                {
                    /// Giá trị chính xác của bản thân
                    KMagicAttrib cHitRate = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_hitrate];

                    /// Chính xác
                    int nHitRate = cHitRate.nValue[0];
                    /// Né tránh của đối thủ
                    int nDefense = (target.m_CurrentDefend - cHitRate.nValue[1]) * (100 - cHitRate.nValue[2]) / 100;

                    /// Tỷ lệ đánh trúng
                    int nPercent;
                    /// Nếu độ chính xác dưới 0 thì độ chính xác = Min
                    if (nHitRate <= 0)
                    {
                        nPercent = KTGlobal.HitPercentMin;
                    }
                    /// Nếu nẽ tránh của đối thủ dưới 0 thì chính xác của bản thân = Max
                    else if (nDefense <= 0)
                    {
                        nPercent = KTGlobal.HitPercentMax;
                    }
                    /// Trường hợp còn lại sẽ tính toán
                    else
                    {
                        /// Nếu là phái Đường Môn
                        if (target is KPlayer targetPlayer && targetPlayer.m_cPlayerFaction.GetFactionId() == 3)
                        {
                            nPercent = nHitRate * 100 / (nHitRate + (int)(nDefense * 1.5f));
                        }
                        /// Trường hợp khác
                        else
                        {
                            /// Tỷ lệ đánh trúng ( add nhân thêm hệ số 3 vào né tránh để tạo khác biệt)
                            nPercent = nHitRate * 100 / (nHitRate + (int)(nDefense * 3));
                        }

                        /// Nếu tỷ lệ dưới Min thì thiết lập = Min
                        if (nPercent < KTGlobal.HitPercentMin)
                        {
                            nPercent = KTGlobal.HitPercentMin;
                        }
                        /// Nếu tỷ lệ dưới Max thì thiết lập = Max
                        else if (nPercent > KTGlobal.HitPercentMax)
                        {
                            nPercent = KTGlobal.HitPercentMax;
                        }
                    }

                    /// Số ngẫu nhiên tỷ lệ đánh trúng
                    int randomHitRate = KTGlobal.GetRandomNumber(0, 99);

                    /// Nếu đánh trượt
                    if (randomHitRate > nPercent)
                    {
                        if (KTGlobal.IsTestModel)
                        {
                            if (attacker is KPlayer)
                            {
                                KPlayer _Test = attacker as KPlayer;
                                KTPlayerManager.ShowNotification(_Test, "MIss vì né tránh của đối phương :" + nPercent);
                            }
                        }
                        /// Syns miss
                        AlgorithmProperty.SyncMiss(attacker, target);
                        return false;
                    }
                }
            }

            bool bIsOver = false;

            /// Nếu là kỹ năng gây sát thương
            if (rcSkill.Data.IsDamageSkill)
            {
                /// Tổng sát thương nhận được ban đầu
                int nDamagePercent = 100;
                nDamagePercent *= nAdditionP / 100;

                /// Sát thương nhận thêm khi bị tấn công bởi quái
                if (target is Monster)
                {
                    nDamagePercent += attacker.m_nDamageAddedPercentWhenHitNPC;
                }

                ///  VIẾT THÊM TĂNG SÁT THƯƠNG GÂY RA
                nDamagePercent += attacker.m_nDamageAddedPercent;

                /// Nếu nội lực đầy
                if (attacker.m_CurrentMana >= attacker.m_CurrentManaMax)
                {
                    /// Tăng thêm sát thương khi nội lực đầy
                    nDamagePercent += target.m_DamageMultipleWhenFullMana;
                }

                /// Loại skill gây dame bởi khoảng cách.
                if (target.m_sRdcLifeWithDis.nDamageSkillId == rcSkill.Data.ID)
                {
                    nDamagePercent = target.m_sRdcLifeWithDis.nDamageAddedP;
                }

                /// Nếu là đòn đánh chí mạng
                if (bIsDeadlyStrike)
                {
                    int nNewPercent = nDamagePercent * (KTGlobal.DeadlyStrikeDamagePercent + attacker.m_DeadlystrikeDamagePercent - target.m_nDefenceDeadlyStrikeDamageTrim) / 100;

                    nDamagePercent = nNewPercent;
                }

                NPC_CALC_DAMAGE_PARAM CALC_DAMAGE_PARAM = new NPC_CALC_DAMAGE_PARAM();
                int nTotalDamage = 0;
                for (int i = 0; i < AppendDameDic.Count; ++i)
                {
                    KMagicAttrib cDamage = AppendDameDic.Values.ElementAt(i);

                    int nDamage = (cDamage.nValue[0] + cDamage.nValue[2]) / 2;
                    if (nDamage < 0)
                    {
                        nDamage = 0;
                    }
                    nTotalDamage += (nDamage * nDamagePercent / 100);
                }

                CALC_DAMAGE_PARAM.nTotalDamage = nTotalDamage;

                bBreak = false;

                /// Tính toán dame ngũ hành
                int nSeriesConquarRes = 0;

                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_series))
                {
                    KMagicAttrib cSeriesDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_series];
                    nSeriesConquarRes = cSeriesDamage.nValue[0];

                    // NẾU HỆ CỦA SKILL ĐÁNH VÀO NGƯỜI KHẮC HỆ==> SKILL ĐƯỢC TĂNG CƯỜNG SỨC TẤN CÔNG
                    if (KTGlobal.g_IsConquer(nSeries, (int)target.m_Series))
                    {
                        int nTempParam = KTGlobal.SeriesTrimParam4 * attacker.m_nSeriesEnhance + target.m_Level * KTGlobal.SeriesTrimParam2 + KTGlobal.SeriesTrimParam3;

                        if (nTempParam == 0)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, "Bug 1");
                            return false;
                        }
                        // CƯỜNG HÓA DAME CỦA THẰNG TẤN CÔNG nAppendAttack
                        int nAppendAttack = (int)(nSeriesConquarRes * KTGlobal.SeriesTrimParam1 * attacker.m_nSeriesEnhance / nTempParam); // %x
                        if (attacker.m_nSeriesEnhance != 0)
                            nAppendAttack += nSeriesConquarRes * KTGlobal.SeriesTrimParam5 / 100;

                        nTempParam = (int)KTGlobal.SeriesTrimParam4 * target.m_nSeriesAbate + attacker.m_Level * KTGlobal.SeriesTrimParam2 + KTGlobal.SeriesTrimParam3;

                        if (nTempParam == 0)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, "Bug 2");
                            return false;
                        }

                        // KHÁNG NGŨ HÀNH BAO NHIÊU
                        int nAppendDefend = (int)(nSeriesConquarRes * KTGlobal.SeriesTrimParam1 * target.m_nSeriesAbate / nTempParam); // %b

                        if (target.m_nSeriesAbate != 0) // KHÁNG NGŨ HÀNH
                            nAppendDefend += nSeriesConquarRes * KTGlobal.SeriesTrimParam5 / 100;

                        int nAppend = nAppendAttack - nAppendDefend;

                        int nApMax = nSeriesConquarRes * KTGlobal.SeriesTrimMax / 100;

                        int nSymbol = nAppend > 0 ? 1 : -1;
                        nAppend = Math.Abs(nAppend) > nApMax ? (nApMax * nSymbol) : nAppend;

                        nSeriesConquarRes = nSeriesConquarRes + nAppend;

                        nSeriesConquarRes += cSeriesDamage.nValue[1] - target.m_CurrentSeriesResist;

                        if (nSeriesConquarRes > 0)
                            nSeriesConquarRes = -nSeriesConquarRes;
                        else
                            nSeriesConquarRes = 0;

                        // KẾT THÚC TÍNH DAME TƯƠNG KHẮC
                    }
                    else if (KTGlobal.g_IsConquer((int)target.m_Series, nSeries))
                    {
                        int nTempParam = (int)KTGlobal.SeriesTrimParam4 * attacker.m_nSeriesAbate + target.m_Level * KTGlobal.SeriesTrimParam2 + KTGlobal.SeriesTrimParam3;
                        if (nTempParam == 0)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, "Bug 3");
                            return false;
                        }
                        int nAppendAttack = (int)(nSeriesConquarRes * KTGlobal.SeriesTrimParam1 * attacker.m_nSeriesAbate / nTempParam);
                        if (attacker.m_nSeriesAbate != 0)
                            nAppendAttack += nSeriesConquarRes * KTGlobal.SeriesTrimParam5 / 100;

                        nTempParam = KTGlobal.SeriesTrimParam4 * target.m_nSeriesEnhance + attacker.m_Level * KTGlobal.SeriesTrimParam2 + KTGlobal.SeriesTrimParam3;
                        if (nTempParam == 0)
                        {
                            LogManager.WriteLog(LogTypes.Fatal, "Bug 4");
                            return false;
                        }
                        int nAppendDefend = (int)(nSeriesConquarRes * KTGlobal.SeriesTrimParam1 * target.m_nSeriesEnhance / nTempParam);   // %a
                        if (target.m_nSeriesEnhance != 0)
                            nAppendDefend += nSeriesConquarRes * KTGlobal.SeriesTrimParam5 / 100;

                        int nAppend = nAppendAttack - nAppendDefend;
                        int nApMax = nSeriesConquarRes * KTGlobal.SeriesTrimMax / 100;

                        int nSymbol = nAppend > 0 ? 1 : -1;
                        nAppend = Math.Abs(nAppend) > nApMax ? (nApMax * nSymbol) : nAppend;

                        nSeriesConquarRes = nSeriesConquarRes - nAppend;

                        nSeriesConquarRes += cSeriesDamage.nValue[1] - target.m_CurrentSeriesResist;

                        if (nSeriesConquarRes < 0)
                            nSeriesConquarRes = 0;
                    }
                    else
                    {
                        nSeriesConquarRes = 0;
                    }

                    CALC_DAMAGE_PARAM.nSeriesConquarRes = nSeriesConquarRes;
                }

                // 3.TÍnh Toán Dame chí mạng

                int nTempDecRes = 0;

                DAMAGE_TYPE eDamageType = DAMAGE_TYPE.damage_num;

                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_fatallystrike))
                {
                    KMagicAttrib cFatallyStrikeDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_fatallystrike];

                    switch (nSeries)
                    {
                        case (int)KE_SERIES_TYPE.series_metal:
                            eDamageType = DAMAGE_TYPE.damage_physics;
                            break;

                        case (int)KE_SERIES_TYPE.series_wood:
                            eDamageType = DAMAGE_TYPE.damage_poison;
                            break;

                        case (int)KE_SERIES_TYPE.series_water:
                            eDamageType = DAMAGE_TYPE.damage_cold;
                            break;

                        case (int)KE_SERIES_TYPE.series_fire:
                            eDamageType = DAMAGE_TYPE.damage_fire;
                            break;

                        case (int)KE_SERIES_TYPE.series_earth:
                            eDamageType = DAMAGE_TYPE.damage_light;
                            break;
                    }
                    if (eDamageType < DAMAGE_TYPE.damage_num)
                    {
                        nTempDecRes = KTGlobal.FatallyStrikePercent * target.m_damage[(int)eDamageType].GetCurResist() / 100;

                        target.m_damage[(int)eDamageType].AddCurResist(-nTempDecRes);
                    }
                }

                /// Có phải skill độc công không
                bool isPoisonSkill = AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_poison) && AppendDameDic[MAGIC_ATTRIB.magic_damage_append_poison].nValue[0] > 0;

                int nLife = target.m_CurrentLife;

                /// Lực tay nội ngoại ( viết thêm bỏ qua khiên nội lực nếu là chiêu đánh có độc công, )
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_physics))
                {
                    KMagicAttrib cPhysicsDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_physics];

                    /// Chỉ tính vật công nội
                    bIsOver = !CalcDamage(attacker, target, cPhysicsDamage.nValue[0], cPhysicsDamage.nValue[2], DAMAGE_TYPE.damage_physics, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase, !isPoisonSkill ? true : rcSkill.Data.IsPhysical);
                    /// Không tính vật công ngoại nên tạm comment để đó, sau nếu sửa thì mở comment là được
                    //bIsOver = !CalcDamage(attacker, target, cPhysicsDamage.nValue[0], cPhysicsDamage.nValue[2], DAMAGE_TYPE.damage_physics, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase, !isPoisonSkill ? true : false);
                }

                /// Băng công
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_cold))
                {
                    KMagicAttrib cColdDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_cold];

                    //TODO : Nếu giá trị băng công mà nhỏ hơn 100 điểm thì bỏ qua luôn ko tính damge | Nếu sát thương mà lớn 100 điểm thì sẽ trừ đi 100 rồi thừa bao nhiêu vứt vào sát thương
                    bIsOver = !CalcDamage(attacker, target, cColdDamage.nValue[0], cColdDamage.nValue[2], DAMAGE_TYPE.damage_cold, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase);
                }

                /// Hỏa công
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_fire))
                {
                    KMagicAttrib cFireDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_fire];

                    bIsOver = !CalcDamage(attacker, target, cFireDamage.nValue[0], cFireDamage.nValue[2], DAMAGE_TYPE.damage_fire, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase);
                }

                /// Lôi công
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_light))
                {
                    KMagicAttrib cLightDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_light];

                    bIsOver = !CalcDamage(attacker, target, cLightDamage.nValue[0], cLightDamage.nValue[2], DAMAGE_TYPE.damage_light, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase);
                }

                /// Độc công ( viết thêm bỏ qua khiên nội lực)
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_poison))
                {
                    KMagicAttrib cPoisonDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_poison];

                    if (!bIsOver)
                    {
                        int nPosionDamage = cPoisonDamage.nValue[0];
                        int nReceivePercent = CalcReceivePercent(CALC_DAMAGE_PARAM.nSeriesConquarRes, attacker, target, DAMAGE_TYPE.damage_poison, target.m_damage[(int)DAMAGE_TYPE.damage_poison]); //target.m_damage[DAMAGE_TYPE.damage_poison].CalcReceivePercent(CALC_DAMAGE_PARAM.nSeriesConquarRes, Npc[nLauncher], damage_poison);
                        nPosionDamage = nPosionDamage * nReceivePercent / 100;
                        bIsOver = !CalcDamage(attacker, target, nPosionDamage, nPosionDamage, DAMAGE_TYPE.damage_poison, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase, false);

                        if (!target.IsDead() && !bIsOver)
                        {
                            if (nPosionDamage > 0)
                            {
                                target.AddPoisonState(attacker, nPosionDamage * nDamagePercent / 100, cPoisonDamage.nValue[1]);
                            }
                        }
                    }
                }

                /// Vật công nội ( viết thêm bỏ qua khiên nội lực nếu là chiêu đánh có độc công)
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_magic))
                {
                    KMagicAttrib cMagicDamage = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_magic];

                    if (!bIsOver)
                        bIsOver = !CalcDamage(attacker, target, cMagicDamage.nValue[0], cMagicDamage.nValue[2], DAMAGE_TYPE.damage_magic, bIsMelee, CALC_DAMAGE_PARAM, false, nDamagePercent, isSubPhrase, !isPoisonSkill);
                }

                /// Nếu phòng thủ bị giảm thì thêm
                if (nTempDecRes != 0)
                {
                    target.m_damage[(int)eDamageType].AddCurResist(nTempDecRes);
                    nTempDecRes = 0;
                }

                /// Hút sinh lực ( VIẾT LẠI - bỏ Param[1] do luôn = 100%)
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_steallife))
                {
                    /// Thuộc tính tương ứng
                    KMagicAttrib cStealLife = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_steallife];
                    /// Tỷ lệ hút
                    int nRate = cStealLife.nValue[0];
                    /// Nếu có tỷ lệ hút
                    if (nRate > 0)
                    {
                        /// Tăng sinh lực của thằng tấn công lên
                        attacker.m_CurrentLife += (nLife - target.m_CurrentLife) * nRate / 100;
                        /// Nếu vượt quá ngưỡng
                        if (attacker.m_CurrentLife > attacker.m_CurrentLifeMax)
                        {
                            /// Thiết lập lại
                            attacker.m_CurrentLife = attacker.m_CurrentLifeMax;
                        }
                    }
                }

                /// Hút nội lực ( VIẾT LẠI - bỏ Param[1] do luôn = 100%)
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_stealmana))
                {
                    /// Thuộc tính tương ứng
                    KMagicAttrib cStealMana = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_stealmana];
                    /// Tỷ lệ hút
                    int nRate = cStealMana.nValue[0];
                    /// Nếu có tỷ lệ hút
                    if (nRate > 0)
                    {
                        /// Tăng nội lực của thằng tấn công lên
                        attacker.m_CurrentMana += (nLife - target.m_CurrentLife) * nRate / 100;
                        /// Nếu vượt quá ngưỡng
                        if (attacker.m_CurrentMana > attacker.m_CurrentManaMax)
                        {
                            /// Thiết lập lại
                            attacker.m_CurrentMana = attacker.m_CurrentManaMax;
                        }
                    }
                }

                /// Hút thể lực ( VIẾT LẠI - bỏ Param[1] do luôn = 100%)
                if (AppendDameDic.ContainsKey(MAGIC_ATTRIB.magic_damage_append_stealstamina))
                {
                    /// Thuộc tính tương ứng
                    KMagicAttrib cStealStamina = AppendDameDic[MAGIC_ATTRIB.magic_damage_append_stealstamina];
                    /// Tỷ lệ hút
                    int nRate = cStealStamina.nValue[0];
                    /// Nếu có tỷ lệ hút
                    if (nRate > 0)
                    {
                        /// Tăng thể lực của thằng tấn công lên
                        attacker.m_CurrentStamina += (nLife - target.m_CurrentLife) * nRate / 100;
                        /// Nếu vượt quá ngưỡng
                        if (attacker.m_CurrentStamina > attacker.m_CurrentStaminaMax)
                        {
                            /// Thiết lập lại
                            attacker.m_CurrentStamina = attacker.m_CurrentStaminaMax;
                        }
                    }
                }

                ///  VIẾT THÊM CHÍ TỬ
                /// Tỷ lệ chí tử trừ trực tiếp 10% máu
                int nBaseFatalStrikeP = attacker.m_CurrentFatalStrikePercent + PropertyDictionaryManager.GetFatalStrikeP(SkillPD).nValue[0];
                /// Tỷ lệ ngẫu nhiên có thể
                int nRandomFatalStrikePercent = KTGlobal.GetRandomNumber(1, 100);
                /// Nếu gây chí tử
                if (nRandomFatalStrikePercent <= nBaseFatalStrikeP)
                {
                    /// Nếu còn trên 50% máu thì mới kích chí tử
                    if (target.m_CurrentLife * 100 / target.m_CurrentLifeMax > 50)
                    {
                        /// Giảm trực tiếp 10% sinh lực tối đa
                        target.m_CurrentLife -= target.m_CurrentLife * 10 / 100;
                        /// Đánh dấu là chí mạng
                        bIsDeadlyStrike = true;
                    }
                }

                /// Nếu mất máu
                if (target.m_CurrentLife < nLife)
                {
                    /// Nếu là đòn chí mạng
                    if (bIsDeadlyStrike)
                    {
                        /// Thực hiện sự kiện gọi khi đánh chí mạng
                        attacker.OnDoCritOnTarget(target);
                        target.OnBeCritHit(attacker);
                        AlgorithmProperty.SynDeadly(attacker, target, nLife - target.m_CurrentLife);
                    }
                    else
                    {
                        /// SYSN Dame Về
                        AlgorithmProperty.SyncDamage(attacker, target, nLife - target.m_CurrentLife);
                    }

                    /// % sát thương được chuyển hóa thành nội lực ( VIẾT THÊM)
                    target.m_CurrentMana += (nLife - target.m_CurrentLife) * target.m_Damage2AddManaP / 100;
                    if (target.m_CurrentMana > target.m_CurrentManaMax)
                    {
                        target.m_CurrentMana = target.m_CurrentManaMax;
                    }

                    /// Nếu là đối tượng bị đánh là monster | Lưu lại thằng nào gây damge
                    if (target is Monster && (attacker is KPlayer || attacker is Pet))
                    {
                        /// Người chơi tương ứng
                        KPlayer player;
                        /// Nếu là pet
                        if (attacker is Pet pet)
                        {
                            /// Lấy thông tin chủ nhân
                            player = pet.Owner;
                        }
                        /// Nếu là người
                        else
                        {
                            player = attacker as KPlayer;
                        }

                        /// Nếu tìm thấy
                        if (player != null)
                        {
                            Monster monster = (Monster)target;

                            /// Nếu thằng tấn công có đội thì ID add vào QUEUE = ID đội
                            if (player.TeamID != -1)
                            {
                                monster.RecordDamage(player.TeamID, nLife - target.m_CurrentLife, true);
                            }
                            /// Không thì Add chính nó
                            else
                            {
                                monster.RecordDamage(attacker.RoleID, nLife - target.m_CurrentLife, false);
                            }
                        }
                    }
                }
            }
            else
            {
                /// Thông báo đối tượng vừa tấn công
                target.TakeDamage(attacker, 0);
            }

            /// Đã chết chưa
            bool bDeath = target.IsDead();
            /// Nếu chưa chết
            if (!bDeath && !bIsOver && !isSubPhrase)
            {
                /// Duyệt danh sách trạng thái ngũ hành
                for (int i = 0; i < AppendDameDic.Count; ++i)
                {
                    if (target.IsDead())
                        break;

                    KMagicAttrib cState = AppendDameDic.Values.ElementAt(i);
                    if (cState == null)
                    {
                        continue;
                    }

                    MAGIC_ATTRIB Atribute = AppendDameDic.Keys.ElementAt(i);

                    KE_STATE state = ConvertSkillEffectToState(Atribute);

                    if (cState.nValue[0] != 0)
                    {
                        // Viết thêm cơ chế tính toán thời gian băng sát dành riêng cho võ lâm dựa trên băng sát của nhân vật

                        //if (cState.nAttribType == MAGIC_ATTRIB.magic_damage_append_cold)
                        //{
                        //    int FinalTime = GetSlowAllValueBy_Damage_Cold(cState.nValue[2]);
                        //    // Set cho nó chắc chắn làm chậm
                        //    cState.nValue[0] = 100;
                        //    cState.nValue[1] = FinalTime;
                        //    cState.nValue[2] = 0;
                        //}

                        // Xử lý state cho người chơi
                        KSpecialStateManager.OnReceiveState(target, cState, state, attacker, rcSkill, skillPos, SkillPD);
                    }
                }

                ////CODE XỬ LÝ BĂNG SÁT
                KMagicAttrib _Magic = PropertyDictionaryManager.GetColdDamage(SkillPD);

                int Value = 0;
                // Xem skill có băng sát hay không
                if (_Magic.nValue[0] > 0)
                {
                    // Lấy ra giá trị randommaxxmin
                    Value = KTGlobal.GetRandomNumber(_Magic.nValue[0], _Magic.nValue[2]);
                }

                var DamgeGroup = attacker.m_damage[(int)DAMAGE_TYPE.damage_cold];
                if (DamgeGroup != null)
                {
                    // Lấy giá trị max của băng sát nội hoặc ngoại trên trang bị của nhân vật
                    int TmpValue = Math.Max(DamgeGroup.m_nEnhanceMagic, DamgeGroup.m_nEnhanceDamage);

                    if (TmpValue > Value)
                    {
                        Value = TmpValue;
                    }
                }
                // Nếu mà giá trị băng sát lớn hơn 100 điểm thì chỉ lấy 100 điểm để tính slow
                if (Value > 100)
                {
                    Value = 100;
                }
                // Nếu damge > 0 tức là có damage
                if (Value > 0)
                {
                    KE_STATE state = KE_STATE.emSTATE_SLOWALL;

                    KMagicAttrib cState = new KMagicAttrib(MAGIC_ATTRIB.state_slowall_attack);
                    int FinalTime = GetSlowAllValueBy_Damage_Cold(Value);

                    // nếu số khung hình lớn hơn 0
                    if (FinalTime > 0)
                    {
                        // Set cho nó chắc chắn làm chậm
                        cState.nValue[0] = 100;
                        cState.nValue[1] = FinalTime;
                        cState.nValue[2] = 0;

                        KSpecialStateManager.OnReceiveState(target, cState, state, attacker, rcSkill, skillPos, SkillPD);
                    }
                }
                // Lấy ra damge group cảu thằng tấn công

                //Console.WriteLine(DamgeGroup);
            }

            /// Thực thi Buff tương ứng nếu có Symbol magic_skill_statetime
            if (SkillPD.ContainsKey((int)MAGIC_ATTRIB.magic_skill_statetime))
            {
                KMagicAttrib magicAttrib = SkillPD.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_statetime);
                int buffDurationTick = magicAttrib.nValue[0] * 1000 / 18;
                BuffDataEx buff = new BuffDataEx()
                {
                    Skill = rcSkill,
                    Duration = buffDurationTick,
                    LoseWhenUsingSkill = false,
                    SaveToDB = false,
                    StartTick = KTGlobal.GetCurrentTimeMilis(),
                };
                /// Nếu không phải bùa chú
                if (buff.Skill.Data.TargetType == "self" || buff.Skill.Data.TargetType == "selfnothide" || (buff.Skill.Data.TargetType == "ally" || buff.Skill.Data.TargetType == "team" || buff.Skill.Data.TargetType == "teamnoself" || buff.Skill.Data.TargetType == "allyandnpc" || buff.Skill.Data.TargetType == "allynoself" || buff.Skill.Data.TargetType == "npcteamnoself") || (buff.Skill.Data.TargetType == "camp") || buff.Skill.Data.TargetType == "revivable")
                {
                    buff.CurseOwner = null;
                    /// Thêm Buff vào mục tiêu
                    target.Buffs.AddBuff(buff);
                }
                /// Nếu là bùa chú
                else
                {
                    /// Tỷ lệ bỏ qua bùa chú của đối phương
                    int targetIgnoreCurseP = target.m_CurrentIgnoreCursePercent;
                    /// Tỷ lệ ngẫu nhiên
                    int nRandIgnore = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu bỏ qua bùa chú
                    if (nRandIgnore <= targetIgnoreCurseP)
                    {
                        /// Bỏ qua
                        return false;
                    }

                    /// Tỷ lệ phản đòn bùa chú của đối phương
                    int targetReflectCurseP = target.m_CurrentReturnSkillPercent;
                    /// Tỷ lệ ngẫu nhiên
                    int nPercent = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu có thể phản đòn bùa chú
                    if (nPercent <= targetReflectCurseP)
                    {
                        /// Đánh dấu đối tượng gây trạng thái bất lợi này
                        buff.CurseOwner = target;
                        attacker.Buffs.AddBuff(buff);
                    }
                    /// Nếu không thể phản đòn bùa chú
                    else
                    {
                        /// Đánh dấu đối tượng gây trạng thái bất lợi này
                        buff.CurseOwner = attacker;
                        target.Buffs.AddBuff(buff);
                    }
                }
            }
            /// Thực hiện hàm gọi đến khi bị tấn công bởi kỹ năng
            target.OnBeHitBySkill(attacker, rcSkill);
            /// Thực hiện hàm gọi đến khi tấn công đối tượng khác bằng kỹ năng tương ứng
            attacker.OnHitTargetWithSkill(target, rcSkill);

            return true;
        }

        public static int GetSlowAllValueBy_Damage_Cold(int InputValue)
        {
            int OutPut = InputValue * 44 / 100;

            return OutPut;
        }

        /// <summary>
        /// Gửi gói tin thông báo Crit về Client
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        private static void SynDeadly(GameObject attacker, GameObject target, int damage)
        {
            /// Gửi gói tin sát thương về Client
            KTSkillManager.AppendSkillResult(attacker, target, SkillResult.Crit, damage);

            /// Thông báo đối tượng vừa tấn công
            target.TakeDamage(attacker, damage);

            /// Gọi hàm thực thi sự kiện khi đối tượng mất máu
            target.OnHPDropped(attacker);
        }

        /// <summary>
        /// Tính sát thương gây ra
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="nMin"></param>
        /// <param name="nMax"></param>
        /// <param name="nType"></param>
        /// <param name="bIsMelee"></param>
        /// <param name="pCALCDamage_Param"></param>
        /// <param name="bStateHurt"></param>
        /// <param name="nDamagePercent"></param>
        /// <param name="isSubPhrase"></param>
        /// <param name="ignoreManaShield"></param>
        /// <returns></returns>
        public static bool CalcDamage(GameObject attacker, GameObject target, int nMin, int nMax, DAMAGE_TYPE nType, bool bIsMelee, NPC_CALC_DAMAGE_PARAM pCALCDamage_Param, bool bStateHurt, int nDamagePercent, bool isSubPhrase, bool affectManaShield = true)
        {
            try
            {
                /// Nếu không có Attacker
                if (attacker == null)
                {
                    return false;
                }

                nMin = nMin * nDamagePercent / 100;
                nMax = nMax * nDamagePercent / 100;

                /// Nếu đối tượng đã chết hoặc đang bất tử, hoặc đang ở chế độ hòa bình
                if (target.IsDead() || target.m_CurrentInvincibility == 1 || (target is KPlayer player && player.GM_Immortality) || (attacker is KPlayer && target is KPlayer && !KTGlobal.IsOpposite(attacker, target)))
                {
                    return false;
                }

                /// Nếu đang miễn nhiễm sát thương
                if (target is KPlayer && attacker is KPlayer && (target as KPlayer).IsImmuneToAllDamagesOf(attacker as KPlayer))
                {
                    return false;
                }

                if (nMin + nMax <= 0)
                    return true;

                int nAvgDamage = (nMin + nMax) / 2;
                if (nAvgDamage < 0)
                    nAvgDamage = 0;

                int nDamageRange = nMax - nMin;
                int nDamage = 0;
                if (nDamageRange < 0)
                    nDamage = nMax + KTGlobal.GetRandomNumber(-nDamageRange, 0);
                else
                    nDamage = nMin + KTGlobal.GetRandomNumber(0, nDamageRange);

                if (nDamage <= 0)
                    return true;

                //LogManager.WriteLog(LogTypes.Fatal, "Damage Type: [" + nType + "], " + nDamage + "");

                int nReceivePercent = 100;
                if (nType != DAMAGE_TYPE.damage_poison)
                {
                    nReceivePercent = CalcReceivePercent(pCALCDamage_Param == null ? 0 : pCALCDamage_Param.nSeriesConquarRes, attacker, target, nType, target.m_damage[(int)nType]);
                }
                if (nReceivePercent <= 0)
                    return true;

                if (target.HaveState(KE_STATE.emSTATE_BURN))
                {
                    int nNewPercent = nReceivePercent * KTGlobal.BurnDamagePercent / 100;

                    //LogManager.WriteLog(LogTypes.Fatal, "BurnState ReceivePercent: " + nReceivePercent + " => " + nNewPercent + "");
                    nReceivePercent = nNewPercent;
                }

                if (nDamage > 1000000)
                    nDamage = nDamage / 100 * nReceivePercent;
                else
                    nDamage = nDamage * nReceivePercent / 100;

                if (nDamage <= 0)
                    return true;

                /// Nếu không phải là phản Damage
                if (nType != DAMAGE_TYPE.damage_return)
                {
                    int nRetPercent = 0, nRetPoint = 0;
                    if (bStateHurt)
                    {
                        if (nType == DAMAGE_TYPE.damage_poison) // Nếu là phản lại dame độc
                        {
                            nRetPercent = target.m_CurrentPoisonDmgRetPercent;
                            nRetPoint = target.m_CurrentPoisonDmgRet;
                        }
                    }
                    else if (bIsMelee)  // Phản lại dame cận chiến
                    {
                        nRetPercent = target.m_CurrentMeleeDmgRetPercent;
                        nRetPoint = target.m_CurrentMeleeDmgRet;
                    }
                    else   // Phản lại dame tầm xa
                    {
                        nRetPercent = target.m_CurrentRangeDmgRetPercent;
                        nRetPoint = target.m_CurrentRangeDmgRet;
                    }
                    /// Nếu bản thân có bùa bị phản sát thương khi tấn công mục tiêu
                    if (attacker.m_sReflectDamageWhenHitTargetID != -1 && attacker.m_sReflectDamageWhenHit > 0)
                    {
                        /// Nếu đối phương là mục tiêu bị tấn công
                        if (target.RoleID == attacker.m_sReflectDamageWhenHitTargetID)
                        {
                            /// Cộng thêm vào lượng phản
                            nRetPercent += attacker.m_sReflectDamageWhenHit;
                        }
                    }

                    /// Lượng phản lại
                    nRetPoint += nDamage * nRetPercent / 100;
                    if (nRetPoint > 0)
                    {
                        int nOldLife = attacker.m_CurrentLife;

                        AlgorithmProperty.CalcDamage(target, attacker, nRetPoint, nRetPoint, DAMAGE_TYPE.damage_return, false, null, false, 100, true);
                        AlgorithmProperty.SyncDamage(target, attacker, nOldLife - attacker.m_CurrentLife);

                        ///  Fix BUG chết không hiện khung
                        if (attacker.m_CurrentLife <= 0)
                        {
                            attacker.m_CurrentLife = 0;
                            attacker.DoDeath(target);
                            return true;
                        }
                    }
                }

                // Xử lý PK Dame nhân thêm 1 hệ số
                if ((target is KPlayer) && (attacker is KPlayer))
                {
                    nDamage = nDamage * KTGlobal.PKDamageRate / 100;
                    nDamage = nDamage > 0 ? nDamage : 1;
                }

                if (!(target is KPlayer) && !(attacker is KPlayer)) // Nếu là 2 con quái có quan hệ khác phhe
                {
                    //if (GetVirtualRelation().eRelationType == Npc[nAttacker].GetVirtualRelation().eRelationType && GetVirtualRelation().eRelationType == emNPCVRELATIONTYPE_TONE) // CHEKC PHE CHECK SAU
                    {
                        nDamage = nDamage * KTGlobal.NpcPKDamageRate / 100;
                    }
                }

                /// Nếu là quái thì damge chia 2 lần
                if (attacker is Monster)
                {
                    nDamage /= 2;
                }
                /// Mặc định sát thương chia 4 lần
                nDamage /= 4;

                /// Giảm sát thương tầm xa gần ( VIẾT THÊM)
                /// Khoảng cách giữa 2 đối tượng
                float distance = KTGlobal.GetDistanceBetweenGameObjects(attacker, target);
                /// Nếu khoảng gần
                if (distance <= 200)
                {
                    /// Nếu là sát thương nội công
                    if (nType == DAMAGE_TYPE.damage_magic)
                    {
                        /// Tính toán lại dựa theo tỷ lệ giảm sát thương
                        nDamage -= nDamage * target.m_ReduceNearMagicDamageP / 100;
                    }
                    /// Nếu là sát thương ngoại công
                    else if (nType == DAMAGE_TYPE.damage_physics)
                    {
                        /// Tính toán lại dựa theo tỷ lệ giảm sát thương
                        nDamage -= nDamage * target.m_ReduceNearPhysicDamageP / 100;
                    }
                }
                /// Nếu khoảng xa
                else
                {
                    /// Nếu là sát thương nội công
                    if (nType == DAMAGE_TYPE.damage_magic)
                    {
                        /// Tính toán lại dựa theo tỷ lệ giảm sát thương
                        nDamage -= nDamage * target.m_ReduceFarMagicDamageP / 100;
                    }
                    /// Nếu là sát thương ngoại công
                    else if (nType == DAMAGE_TYPE.damage_physics)
                    {
                        /// Tính toán lại dựa theo tỷ lệ giảm sát thương
                        nDamage -= nDamage * target.m_ReduceFarPhysicDamageP / 100;
                    }
                }

                /// Vừa bị hút mana vừa trúng độc
                if (target.m_CurrentPoison2Mana > 0 && nType == DAMAGE_TYPE.damage_poison)
                {
                    target.m_CurrentMana -= target.m_CurrentPoison2Mana * nDamage / 100;
                    if (target.m_CurrentMana > target.m_CurrentManaMax)
                    {
                        target.m_CurrentMana = target.m_CurrentManaMax;
                    }
                    else if (target.m_CurrentMana < 0)
                    {
                        target.m_CurrentMana = 0;
                    }
                }

                ///  VIẾT THÊM GIẢM SÁT THƯƠNG NHẬN ĐƯỢC
                nDamage -= nDamage * target.m_nDamageReceiveDecresedPercent / 100;

                /// Nếu có hiệu ứng hóa giải % sát thương ( VIẾT THÊM)
                if (target.m_CurrentDynamicShield > 0 && target.m_CurrentDynamicShieldMaxP > 0)
                {
                    /// Điểm sát thương hóa giải
                    int nShield = target.m_CurrentDynamicShield;
                    /// Tối đa sát thương ban đầu có thể hóa giải
                    int maxShieldAbsorbDamage = nDamage * target.m_CurrentDynamicShieldMaxP / 100;
                    /// Tính sát thương hóa giải
                    nDamage -= Math.Min(nShield, maxShieldAbsorbDamage);
                    /// Nếu không còn sát thương
                    if (nDamage <= 0)
                    {
                        //LogManager.WriteLog(LogTypes.Warning, string.Format("{0}'s damage taken = 0, by dynamicshield", target.RoleName));
                        /// Thực hiện hàm gọi đến khi tấn công mục tiêu
                        attacker.OnHitTarget(target, nDamage);
                        /// Thực hiện hàm gọi đến khi bị tấn công
                        target.OnBeHit(attacker, nDamage);

                        return false;
                    }
                }

                /// Nếu có khiên nội lực ( VIẾT LẠI)
                if (target.m_CurrentDynamicMagicShield > 0)
                {
                    /// Sinh mệnh khiên nội lực
                    int nShield = target.m_CurrentDynamicMagicShield;
                    /// Nếu sinh mệnh của khiên lớn hơn sát thương phải chịu
                    if (nShield >= nDamage)
                    {
                        nShield -= nDamage;
                        nDamage = 0;
                    }
                    else
                    {
                        nDamage -= nShield;
                        nShield = 0;
                    }

                    /// Cập nhật sinh mệnh cho khiên
                    target.m_CurrentDynamicMagicShield = nShield;

                    //if (target is KPlayer)
                    //{
                    //    PlayerManager.ShowNotification(target as KPlayer, "Sinh mệnh khiên nội lực -> " + nShield);
                    //}

                    /// Nếu khiên bị phá
                    if (nShield <= 0)
                    {
                        /// Hủy hiệu ứng tương ứng
                        target.Buffs.RemoveBuff(target.m_CurrentDynamicMagicShield_SkillID);
                    }
                }

                /// Nếu chịu ảnh hưởng bởi nội lực hộ thân
                if (affectManaShield)
                {
                    /// Sát thương chuyển hóa thành nội lực ( VIẾT LẠI)
                    int nManaDamage = 0;
                    if (target.m_CurrentManaShield > 0)
                    {
                        /// Lượng sát thương tối đa sẽ hấp thụ được
                        nManaDamage = nDamage * target.m_CurrentManaShield / 100;
                        /// Nếu lượng sát thương vào nội lực dưới 1
                        if (nManaDamage < 1)
                        {
                            nManaDamage = 1;
                        }

                        /// % nội lực hiện tại
                        int nManaPercent = target.m_CurrentMana * 100 / target.m_CurrentManaMax;
                        /// Nếu lượng nội lực hiện tại của đối tượng trên 15%
                        if (nManaPercent >= 15)
                        {
                            /// Nếu lượng nội lực hiện tại của đối tượng nhỏ hơn sát thương sẽ hấp thụ được
                            if (nManaDamage > target.m_CurrentMana)
                            {
                                nManaDamage = target.m_CurrentMana;
                            }

                            /// Giảm sát thương tương đương nội lực đã mất
                            nDamage -= nManaDamage;
                            /// Nội lực trước đó
                            int manaBefore = target.m_CurrentMana;
                            /// Giảm nội lực tương ứng của đối tượng
                            target.m_CurrentMana -= nManaDamage;
                        }

                        /// Nếu hết sát thương
                        if (nDamage <= 0)
                        {
                            //LogManager.WriteLog(LogTypes.Warning, string.Format("{0}'s damage taken = 0, by manashield", target.RoleName));

                            /// Nếu không phải damage bị phản
                            if (nType != DAMAGE_TYPE.damage_return)
                            {
                                /// Thực hiện hàm gọi đến khi tấn công mục tiêu
                                attacker.OnHitTarget(target, nDamage);
                                /// Thực hiện hàm gọi đến khi bị tấn công
                                target.OnBeHit(attacker, nDamage);
                            }

                            return false;
                        }
                    }
                }
                else
                {
                   // Console.WriteLine("BO QUA NOI LUC HO THAN!!!!!!!!!!!!!!!!!");
                }

                /// Nếu có sát thương
                if (nDamage > 0)
                {
                    /// Nếu lượng sát thương vượt quá sinh mệnh của mục tiêu hiện tại
                    if (nDamage > target.m_CurrentLife)
                    {
                        nDamage = target.m_CurrentLife;
                    }

                    /// Nếu không phản damage bị phản
                    if (nType != DAMAGE_TYPE.damage_return && !isSubPhrase)
                    {
                        /// Thực hiện hàm gọi đến khi tấn công mục tiêu
                        attacker.OnHitTarget(target, nDamage);
                        /// Thực hiện hàm gọi đến khi bị tấn công
                        target.OnBeHit(attacker, nDamage);
                    }

                    /// Thực hiện trừ máu
                    target.m_CurrentLife -= nDamage;
                }

                /// Nếu máu mục tiêu về 0 tức đã chết
                if (target.m_CurrentLife <= 0)
                {
                    target.m_CurrentLife = 0;
                    target.DoDeath(attacker);
                }
                /// Nếu máu mục tiêu lớn hơn máu tối đa
                else if (target.m_CurrentLife > target.m_CurrentLifeMax)
                {
                    target.m_CurrentLife = target.m_CurrentLifeMax;
                }

                return true;
            }
            catch (Exception exx)
            {
                LogManager.WriteLog(LogTypes.Exception, exx.ToString());
                return false;
            }
        }

        /// <summary>
        /// Gửi lại cho tằng target lượng dame gây ra do phản dame
        /// </summary>
        /// <param name="target"></param>
        /// <param name="attacker"></param>
        /// <param name="damage"></param>
        public static void SyncDamage(GameObject attacker, GameObject target, int damage)
        {
            //PlayerManager.ShowNotification(attacker as KPlayer, "Damage = " + damage);

            /// Gửi gói tin sát thương về Client
            KTSkillManager.AppendSkillResult(attacker, target, SkillResult.Normal, damage);

            /// Nếu đối tượng còn tồn tại
            if (target != null && !target.IsDead())
            {
                /// Thông báo đối tượng vừa tấn công
                target.TakeDamage(attacker, damage);
                /// Gọi hàm thực thi sự kiện khi đối tượng mất máu
                target.OnHPDropped(attacker);
            }
        }

        /// <summary>
        /// Thông báo cho người tấn công là đánh mục tiêu miss
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        private static void SyncMiss(GameObject attacker, GameObject target)
        {
            /// Gửi gói tin sát thương về Client
            KTSkillManager.AppendSkillResult(attacker, target, SkillResult.Miss, 0);

            /// Thông báo đối tượng vừa tấn công
            target.TakeDamage(attacker, 0);
        }

        /// <summary>
        /// Thông báo cho người tấn công là đánh mục tiêu bị hóa giải
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        private static void SyncAdjust(GameObject attacker, GameObject target)
        {
            /// Gửi gói tin sát thương về Client
            KTSkillManager.AppendSkillResult(attacker, target, SkillResult.Adjust, 0);

            /// Thông báo đối tượng vừa tấn công
            target.TakeDamage(attacker, 0);
        }

        /// <summary>
        /// Chuyển trạng thái ngũ hành kỹ năng sang đối tượng KE_STATE
        /// </summary>
        /// <param name="arrt"></param>
        /// <returns></returns>
        public static KE_STATE ConvertSkillEffectToState(MAGIC_ATTRIB arrt)
        {
            KE_STATE _State = KE_STATE.emSTATE_BEGIN;

            switch (arrt)
            {
                case MAGIC_ATTRIB.state_hurt_attack:
                    _State = KE_STATE.emSTATE_HURT;
                    break;

                // Thêm vào nếu có băng sát thì cho hiệu ứng slowlal vào
                case MAGIC_ATTRIB.state_slowall_attack:
                    // case MAGIC_ATTRIB.magic_damage_append_cold:
                    _State = KE_STATE.emSTATE_SLOWALL;
                    break;

                case MAGIC_ATTRIB.state_burn_attack:
                    _State = KE_STATE.emSTATE_BURN;
                    break;

                case MAGIC_ATTRIB.state_stun_attack:
                    _State = KE_STATE.emSTATE_STUN;
                    break;

                case MAGIC_ATTRIB.state_fixed_attack:
                    _State = KE_STATE.emSTATE_FIXED;
                    break;

                case MAGIC_ATTRIB.state_palsy_attack:
                    _State = KE_STATE.emSTATE_PALSY;
                    break;

                case MAGIC_ATTRIB.state_slowrun_attack:
                    _State = KE_STATE.emSTATE_SLOWRUN;
                    break;

                case MAGIC_ATTRIB.state_freeze_attack:
                    _State = KE_STATE.emSTATE_FREEZE;
                    break;

                case MAGIC_ATTRIB.state_confuse_attack:
                    _State = KE_STATE.emSTATE_CONFUSE;
                    break;

                case MAGIC_ATTRIB.state_knock_attack:
                    _State = KE_STATE.emSTATE_KNOCK;
                    break;

                case MAGIC_ATTRIB.state_drag_attack:
                    _State = KE_STATE.emSTATE_DRAG;
                    break;

                case MAGIC_ATTRIB.state_silence_attack:
                    _State = KE_STATE.emSTATE_SILENCE;
                    break;

                case MAGIC_ATTRIB.state_zhican_attack:
                    _State = KE_STATE.emSTATE_ZHICAN;
                    break;

                case MAGIC_ATTRIB.state_float_attack:
                    _State = KE_STATE.emSTATE_FLOAT;
                    break;
            }

            return _State;
        }

        /// <summary>
        /// Trả về sát thương nhận được tính kèm cả ngũ hành
        /// </summary>
        /// <param name="nSeriesConquarRes"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="nType"></param>
        /// <param name="damageGroup"></param>
        /// <returns></returns>
        public static int CalcReceivePercent(int nSeriesConquarRes, GameObject attacker, GameObject target, DAMAGE_TYPE nType, KNpcAttribGroup_Damage damageGroup)
        {
            /// Nếu không phải sát thương phản lại
            if (nType != DAMAGE_TYPE.damage_return)
            {
                /// Tổng kháng của mục tiêu
                int nAllRes = nSeriesConquarRes + damageGroup.m_nCurResist;
                /// Kháng gốc
                int nOriginRes = nAllRes;

                /// Ngũ hành tương ứng của sát thương
                int nSeries = (int)KE_SERIES_TYPE.series_metal;
                /// Nếu là sát thương vật công
                if (nType == DAMAGE_TYPE.damage_physics)
                {
                    nSeries = (int)KE_SERIES_TYPE.series_metal;
                }
                /// Nếu là sát thương băng công
                else if (nType == DAMAGE_TYPE.damage_cold)
                {
                    nSeries = (int)KE_SERIES_TYPE.series_water;
                }
                /// Nếu là sát thương hỏa công
                else if (nType == DAMAGE_TYPE.damage_fire)
                {
                    nSeries = (int)KE_SERIES_TYPE.series_fire;
                }
                /// Nếu là sát thương lôi công
                else if (nType == DAMAGE_TYPE.damage_light)
                {
                    nSeries = (int)KE_SERIES_TYPE.series_earth;
                }
                /// Nếu là sát thương độc công
                else if (nType == DAMAGE_TYPE.damage_poison)
                {
                    nSeries = (int)KE_SERIES_TYPE.series_wood;
                }

                /// Nếu có dòng bỏ qua kháng hệ tương ứng
                if (attacker.m_sIgnoreResists[nSeries].nIgnoreResistPMin > 0 && attacker.m_sIgnoreResists[nSeries].nIgnoreResistPMax > 0 && KTGlobal.IngoreResistMaxP > 0)
                {
                    /// Tỷ lệ bỏ qua kháng hệ tương ứng
                    int nIgnoreRate = KTGlobal.GetRandomNumber(attacker.m_sIgnoreResists[nSeries].nIgnoreResistPMin, attacker.m_sIgnoreResists[nSeries].nIgnoreResistPMax);
                    if (nIgnoreRate > KTGlobal.IngoreResistMaxP)
                    {
                        nIgnoreRate = KTGlobal.IngoreResistMaxP;
                    }

                    /// Kháng mục tiêu
                    int nResist = damageGroup.m_nCurResist;

                    /// Tính toán Max Min gì đó
                    int nValue1 = KTGlobal.IngoreResistMaxP - 1;
                    if (nValue1 != 0)
                    {
                        int nValue2 = (2 * KTGlobal.DefenceMaxPercent / nValue1);

                        if (nValue2 != 0)
                        {
                            int nMaxR = (10 * attacker.m_Level + 200) / nValue2;
                            if (nResist > nMaxR)
                            {
                                nResist = nMaxR;
                            }
                        }
                    }

                    /// Tính toán lại kháng của mục tiêu
                    nAllRes = nSeriesConquarRes + damageGroup.m_nCurResist - nResist * nIgnoreRate / 100;
                }

                /// Tỷ lệ sát thương nhận được
                int nResPercent = KTGlobal.DefenceMaxPercent * 2 * Math.Abs(nAllRes) / (Math.Abs(nAllRes) + attacker.m_Level * 10 + 200);
                if (nResPercent > KTGlobal.DefenceMaxPercent)
                {
                    nResPercent = KTGlobal.DefenceMaxPercent;
                }

                /// Nếu âm kháng
                if (nAllRes <= 0)
                {
                    nResPercent = 100 + nResPercent;
                }
                else
                {
                    /// Trả về kết quả
                    nResPercent = 100 - nResPercent;
                }

                //if (attacker is KPlayer)
                //{
                //    if (nSeries == (int) KE_SERIES_TYPE.series_earth)
                //    {
                //        PlayerManager.ShowNotification(attacker as KPlayer, string.Format("{0}'s OriginRes = {1}, FinalRes = {2}, Series = {3}, ResPercent = {4}", target.RoleName, nOriginRes, nAllRes, (KE_SERIES_TYPE) nSeries, nResPercent));
                //    }
                //}

                /// Trả về kết quả
                return nResPercent;
            }
            /// Nếu là sát thương phản lại
            else
            {
                /// Trả về tỷ lệ phản
                return damageGroup.m_nReceivePercent;
            }
        }

        /// <summary>
        /// Thêm trạng thái ngũ hành
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="client"></param>
        /// <param name="_State"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static KMagicAttrib AppendAttackAttrib(KMagicAttrib magic, GameObject client, KNpcAttribGroup_State _State, SkillLevelRef skill)
        {
            int nAttackRate = _State.StateAddRate;
            int nAttackTime = _State.StateAddTime;
            int nAddPercent = 0;
            int nAddFrame = 0;

            /// Cộng chỉ số từ kỹ năng hỗ trợ
            KE_STATE eState = _State.State;
            /// Nếu là người chơi
            if (client is KPlayer)
            {
                KPlayer player = client as KPlayer;
                /// ProDict kỹ năng hỗ trợ
                PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(skill.SkillID);
                /// Nếu có kỹ năng hỗ trợ
                if (enchantPd != null)
                {
                    switch (eState)
                    {
                        case KE_STATE.emSTATE_HURT:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_hurt_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_hurt_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_hurt_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_hurt_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_hurt_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_hurt_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_WEAK:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_weak_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_weak_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_weak_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_weak_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_weak_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_weak_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_SLOWALL:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowall_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowall_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowall_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowall_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowall_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowall_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_BURN:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_burn_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_burn_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_burn_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_burn_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_burn_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_burn_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_STUN:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_stun_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_stun_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_stun_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_stun_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_stun_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_stun_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_FIXED:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_fixed_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_fixed_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_fixed_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_fixed_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_fixed_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_fixed_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_PALSY:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_palsy_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_palsy_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_palsy_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_palsy_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_palsy_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_palsy_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_SLOWRUN:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowrun_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowrun_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowrun_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowrun_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_slowrun_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_slowrun_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_FREEZE:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_freeze_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_freeze_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_freeze_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_freeze_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_freeze_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_freeze_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_CONFUSE:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_confuse_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_confuse_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_confuse_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_confuse_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_confuse_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_confuse_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_KNOCK:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_knock_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_knock_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_knock_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_knock_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_knock_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_knock_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_SILENCE:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_silence_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_silence_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_silence_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_silence_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_silence_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_silence_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                        case KE_STATE.emSTATE_DRAG:
                            {
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_drag_attack))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_drag_attack);
                                    nAddPercent += magicAttrib.nValue[0];
                                    nAddFrame += magicAttrib.nValue[1];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_drag_attackrate))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_drag_attackrate);
                                    nAttackRate += magicAttrib.nValue[0];
                                }
                                if (enchantPd.ContainsKey((int)MAGIC_ATTRIB.state_drag_attacktime))
                                {
                                    KMagicAttrib magicAttrib = enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.state_drag_attacktime);
                                    nAttackTime += magicAttrib.nValue[0];
                                }
                                break;
                            }
                    }
                }
            }

            KMagicAttrib magicFilnal = new KMagicAttrib();
            magicFilnal.nAttribType = magic.nAttribType;

            int nBaseRateParam = KTGlobal.StateBaseRateParam;
            int nBaseTimeParam = KTGlobal.StateBaseTimeParam;

            magicFilnal.nValue[0] = magic.nValue[0] + ((nAttackRate + nBaseRateParam) == 0 ? 0 : (magic.nValue[0] * nAttackRate) / (nAttackRate + nBaseRateParam));
            magicFilnal.nValue[1] = magic.nValue[1] + ((nAttackTime + nBaseTimeParam) == 0 ? 0 : (magic.nValue[1] * nAttackTime) / (nAttackTime + nBaseTimeParam));
            /// Vị trí này chứa tham biến phụ sẽ dùng về sau
            magicFilnal.nValue[2] = magic.nValue[2];

            /// Cộng thêm hỗ trợ
            magicFilnal.nValue[0] += nAddPercent;
            magicFilnal.nValue[1] += nAddFrame;

            return magicFilnal;
        }

        /// <summary>
        /// Tính toán sát thương độc
        /// </summary>
        /// <param name="pDes"></param>
        /// <param name="pSrc"></param>
        /// <param name="stackCount"></param>
        /// <returns></returns>
        public static KMagicAttrib MixPoisonDamage(KMagicAttrib pDes, KMagicAttrib pSrc, int stackCount = 1)
        {
            if (pDes.nValue[2] == 0)
            {
                if (pDes.nValue[0] < 0 && pDes.nValue[1] < 0)
                {
                    pDes.nValue[2] = -pDes.nValue[0] * pDes.nValue[1];
                }
                else
                {
                    pDes.nValue[2] = pDes.nValue[0] * pDes.nValue[1];
                }
            }

            pDes.nValue[0] += pSrc.nValue[0];
            if (pDes.nValue[0] == 0)
            {
                pDes.nValue[1] = 0;
                pDes.nValue[2] = 0;

                return pDes;
            }

            if (pSrc.nValue[2] == 0)
            {
                if (pSrc.nValue[0] < 0 && pSrc.nValue[1] < 0)
                {
                    pDes.nValue[2] -= (pSrc.nValue[0] * pSrc.nValue[1]);
                }
                else
                {
                    pDes.nValue[2] += (pSrc.nValue[0] * pSrc.nValue[1]);
                }
            }
            else
            {
                pDes.nValue[2] += pSrc.nValue[2];
            }

            pDes.nValue[1] = pDes.nValue[2] / pDes.nValue[0];

            pDes *= stackCount;

            return pDes;
        }

        /// <summary>
        /// Tính sát thương gây ra
        /// </summary>
        /// <param name="magicSrc"></param>
        /// <param name="nSkillPercent"></param>
        /// <param name="bIsPhysical"></param>
        /// <param name="nDamagePercent"></param>
        /// <param name="nSrcT"></param>
        /// <param name="nAppendT"></param>
        /// <param name="_GroupDame"></param>
        /// <returns></returns>
        public static KMagicAttrib CalcAttackDamage(KMagicAttrib magicSrc, int nSkillPercent, bool bIsPhysical, int nDamagePercent, int nSrcT, int nAppendT, KNpcAttribGroup_Damage _GroupDame)
        {
            KMagicAttrib magicDes = new KMagicAttrib();

            magicDes.nValue[0] = magicSrc.nValue[0] * nSkillPercent * nSrcT / 1000 / 100;
            magicDes.nValue[2] = magicSrc.nValue[2] * nSkillPercent * nSrcT / 1000 / 100;

            if (bIsPhysical)
            {
                magicDes.nValue[0] += (_GroupDame.m_nEnhanceDamage * nAppendT / 1000);
                magicDes.nValue[2] += (_GroupDame.m_nEnhanceDamage * nAppendT / 1000);
            }
            else
            {
                magicDes.nValue[0] += (_GroupDame.m_nEnhanceMagic * nAppendT / 1000);
                magicDes.nValue[2] += (_GroupDame.m_nEnhanceMagic * nAppendT / 1000);
            }
            magicDes.nValue[0] = magicDes.nValue[0] * nDamagePercent / 100;
            magicDes.nValue[2] = magicDes.nValue[2] * nDamagePercent / 100;

            return magicDes;
        }

        /// <summary>
        /// Lấy tổng % sát thương được hỗ trợ bởi các kỹ năng khác
        /// </summary>
        /// <param name="SKILLID"></param>
        /// <returns></returns>
        public static int GetAddSkillDamagePercent(GameObject client, int nSkillId)
        {
            if (!(client is KPlayer))
            {
                return 0;
            }

            KPlayer player = client as KPlayer;
            return player.Skills.GetAppendDamageToSkillPercent(nSkillId);
        }

        /// <summary>
        /// Lấy tổng sát thương được hỗ trơ bởi các kỹ năng khác
        /// </summary>
        /// <param name="SKILLID"></param>
        /// <returns></returns>
        public static int GetAddSkillDamage(GameObject client, int nSkillId)
        {
            if (!(client is KPlayer))
            {
                return 0;
            }

            KPlayer player = client as KPlayer;
            return player.Skills.GetAppendDamageToSkill(nSkillId);
        }

        /// <summary>
        /// Trả về % sát thương vật công ngoại của vũ khí
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int GetPhysicsEnhance(KPlayer client)
        {
            KE_EQUIP_WEAPON_CATEGORY eWeaponCategory = client.GetWeaponCategory();

            if (eWeaponCategory < KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL || eWeaponCategory >= KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_NUM)
            {
                return 0;
            }
            int nEnhance = KTGlobal.m_arPhysicsEnhance[(int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL];
            if (eWeaponCategory == KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL)
            {
                return nEnhance;
            }
            if (eWeaponCategory > KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_MELEE && eWeaponCategory < KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_RANGE)
            {
                nEnhance += KTGlobal.m_arPhysicsEnhance[(int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_MELEE];
            }
            else if (eWeaponCategory > KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_RANGE)
            {
                nEnhance += KTGlobal.m_arPhysicsEnhance[(int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_RANGE];
            }

            return nEnhance + KTGlobal.m_arPhysicsEnhance[(int)eWeaponCategory];
        }
    }
}