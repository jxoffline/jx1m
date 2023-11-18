using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý sự thay đổi thuộc tính của đối tượng
    /// </summary>
    public static class KTAttributesModifier
    {
        /// <summary>
        /// Tăng vật công ngoại vũ khí MIN
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddWeaponDamageMinV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_PhysicPhysic.nValue[0] += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng vật công ngoại vũ khí MAX
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddWeaponDamageMaxV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_PhysicPhysic.nValue[2] += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng vật công nội vũ khí MIN
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddWeaponMagicMinV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_PhysicsMagic.nValue[0] += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng vật công nội vũ khí MAX
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddWeaponMagicMaxV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_PhysicsMagic.nValue[2] += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng % vật công ngoại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPhysicsDamageP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentPhysicsDamageEnhanceP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng % vật công nội
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPhysicsMagicP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentMagicPhysicsEnhanceP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng độc công ngoại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPoisonDamageV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            AlgorithmProperty.MixPoisonDamage(obj.m_CurrentPoisonDamage, magic, stackCount);
        }

        /// <summary>
        /// Tăng vật công ngoại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPhysicsDamageV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_AddPhysicsDamage += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Kháng tất cả
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllResR(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nType = (int)DAMAGE_TYPE.damage_physics; nType <= (int)DAMAGE_TYPE.damage_magic; ++nType)
            {
                obj.AddCurResist((DAMAGE_TYPE)nType, magic.nValue[0] * stackCount);
                //Console.WriteLine("Add {0} to {1}: +{2}", (DAMAGE_TYPE) nType, obj.RoleName, magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// Thay đổi giá trị kháng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ModResist(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            int nDamageType = magic.nValue[2];
            nDamageType = Math.Abs(nDamageType);
            if (nDamageType >= (int)DAMAGE_TYPE.damage_num)
            {
                return;
            }
            obj.m_damage[nDamageType].AddCurResist(magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer))
            {
                return;
            }

            /// Đối tượng người chơi
            KPlayer player = obj as KPlayer;

            /// Mức máu hiện tại
			int currentHP = player.m_CurrentLife;
            /// Mức khí hiện tại
            int currentMP = player.m_CurrentMana;
            /// Mức thể lực hiện tại
            int currentStamina = player.m_CurrentStamina;

            /// Detach các chỉ số bị động
            player.DeactivateAuraPassiveAndEnchantSkills();

            /// Giá trị tương ứng
            int nValue = magic.nValue[0] * stackCount;
            player.Skills.AllSkillBonusLevel += nValue;

            /// Attach các chỉ số bị động
            player.ReactivateAuraPassiveAndEnchantSkills();

            /// Cập nhật mức máu
            player.m_CurrentLife = Math.Min(player.m_CurrentLifeMax, currentHP);
            /// Cập nhật mức khí
            player.m_CurrentMana = Math.Min(player.m_CurrentManaMax, currentMP);
            /// Cập nhật mức thể lực
            player.m_CurrentStamina = Math.Min(player.m_CurrentStaminaMax, currentStamina);
        }

        /// <summary>
        /// % chính xác
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AttackRatingP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeAttackRating(0, magic.nValue[0] * stackCount, 0);
        }

        /// <summary>
        /// Chính xác
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AttackRatingV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeAttackRating(0, 0, magic.nValue[0] * stackCount);

        }

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AttackSpeedV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
          
            obj.ChangeAttackSpeed(0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void CastSpeedV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeCastSpeed(0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Chí mạng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DeadlyStrikeR(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentDeadlyStrike += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Giá trị Thân
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DexterityV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là người chơi
            if (obj is KPlayer player)
            {
                player.ChangeCurDexterity(magic.nValue[0] * stackCount);
            }
            /// Nếu là pet
            else if (obj is Pet pet)
            {
                pet.ChangeCurDexterity(magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// Giá trị Nội
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void EnergyV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là người chơi
            if (obj is KPlayer player)
            {
                player.ChangeCurEnergy(magic.nValue[0] * stackCount);
            }
            /// Nếu là pet
            else if (obj is Pet pet)
            {
                pet.ChangeCurEnergy(magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// Kỹ năng hệ thổ
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void EarthSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
        }

        /// <summary>
        /// % tốc độ di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FastWalkRunP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeRunSpeed(0, magic.nValue[0] * stackCount, 0);
        }

        /// <summary>
        /// Tốc độ di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FastWalkRunV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeRunSpeed(0, 0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Kỹ năng hệ Hỏa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FireSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
        }

        /// <summary>
        /// % sinh lực tối đa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifeMaxP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeLifeMax(0, magic.nValue[0] * stackCount, 0);
        }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifeMaxV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeLifeMax(0, 0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Phục hồi sinh lực mỗi 5 giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifeReplenishV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phục hồi sinh lực mỗi nửa giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FastLifeReplenishV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hiệu suất phục hồi sinh lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifeReplenishP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeReplenishPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hiệu suất phục hồi nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaReplenishP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaReplenishPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Sinh lực hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifeV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLife += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// May mắn hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LuckyV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (obj is KPlayer)
            {
                ((KPlayer)obj).ChangeCurLucky(magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// % nội lực tối đa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaMaxP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeManaMax(0, magic.nValue[0] * stackCount, 0);
        }

        /// <summary>
        /// Nội lực tối đa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaMaxV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeManaMax(0, 0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Nội lực hồi phục mỗi 5 giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaReplenishV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Nội lực hồi phục mỗi nửa giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FastManaReplenish(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Nội lực hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentMana += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// % hút sát thương của khiên nội lực khi nội lực trên 15%
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaShieldP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaShield += magic.nValue[0] * stackCount;
            obj.m_CurrentManaShield_SkillID = nSkillId;
        }

        /// <summary>
        /// % phản đòn cận chiến
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void MeleeDamageReturnP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentMeleeDmgRetPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phản đòn cận chiến
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void MeleeDamageReturnV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentMeleeDmgRet += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Kỹ năng hệ kim
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void MetalSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
        }

        /// <summary>
        /// Tăng sinh lực tối đa dựa theo % nội lực tối đa hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void AddMaxHPByMaxMP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                /// Lưu lại lượng sinh lực có trước khi Detach
                obj.m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP = obj.m_CurrentLife;
                /// Giảm sinh lực tương ứng
                obj.ChangeLifeMax(0, 0, -obj.m_MaxHPIncreasedByMaxMP);
                /// Xóa biến lưu tương ứng
                obj.m_MaxHPIncreasedByMaxMP = 0;
            }
            /// Nếu là pha Attach
            else
            {
                /// Nếu có biến lưu
                if (obj.m_MaxHPIncreasedByMaxMP > 0)
                {
                    /// Lưu lại lượng sinh lực có trước khi Detach
                    obj.m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP = obj.m_CurrentLife;
                    /// Giảm sinh lực tương ứng
                    obj.ChangeLifeMax(0, 0, -obj.m_MaxHPIncreasedByMaxMP);
                }
                /// Cập nhật giá trị mới
                obj.m_MaxHPIncreasedByMaxMP = obj.m_CurrentManaMax * magic.nValue[0] * stackCount / 100;
                /// Tăng sinh lực tương ứng
                obj.ChangeLifeMax(0, 0, obj.m_MaxHPIncreasedByMaxMP);

                /// Nếu có sinh lực lưu lại trước khi Detach
                if (obj.m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP != -1)
                {
                    /// Phục hồi sinh lực về như cũ nếu trước đó là pha Detach
                    int currentLife = Math.Min(obj.m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP, obj.m_CurrentLifeMax);
                    obj.m_CurrentLife = currentLife;

                    /// Reset
                    obj.m_LastHPBeforeDetachAddMaxHPIncreasedByMaxMP = -1;
                }
            }
        }

        /// <summary>
        /// Giảm sát thương độc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void PoisonDamageReduceV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KNpcAttribGroup_State state = obj.m_state[(int)KE_STATE.emSTATE_POISON];
            state.OtherParam -= magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Giảm % thời gian trúng độc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void PoisonTimeReduceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentPoisonTimeReducePercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phản đòn tầm xa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void RangeDamageReturnV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentRangeDmgRet += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// % phản đòn tầm xa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void RangeDamageReturnP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentRangeDmgRetPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phản đòn sát thương độc công
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void PoisonDamageReturnV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentPoisonDmgRet += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phản đòn % sát thương độc công
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void PoisonDamageReturnP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentPoisonDmgRetPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// % thể lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaminaMaxP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeStaminaMax(0, magic.nValue[0] * stackCount, 0);
        }

        /// <summary>
        /// Thể lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaminaMaxV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeStaminaMax(0, 0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Phục hồi thể lực mỗi 5 giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaminaReplenishV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentStaminaReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phục hồi thể lực mỗi nửa giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FastStaminaReplenish(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentFastStaminaReplenish += magic.nValue[0] * stackCount;

        }

        /// <summary>
        /// Thể lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaminaV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentStamina += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hút sinh lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StealLifeP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeStolen += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hút nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StealManaP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaStolen += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hút thể lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StealStaminaP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentStaminaStolen += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Sức
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StrengthV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là người chơi
            if (obj is KPlayer player)
            {
                player.ChangeCurStrength(magic.nValue[0] * stackCount);
            }
            /// Nếu là pet
            else if (obj is Pet pet)
            {
                pet.ChangeCurStrength(magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// Ngoại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void VitalityV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là người chơi
            if (obj is KPlayer player)
            {
                player.ChangeCurVitality(magic.nValue[0] * stackCount);
            }
            /// Nếu là pet
            else if (obj is Pet pet)
            {
                pet.ChangeCurVitality(magic.nValue[0] * stackCount);
            }
        }

        /// <summary>
        /// Kỹ năng hệ thủy
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void WaterSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
        }

        /// <summary>
        /// Kỹ năng hệ mộc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void WoodSkillV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
        }

        /// <summary>
        /// Né tránh
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ArmorDefenseV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeDefend(0, 0, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Né tránh
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ArmorDefenseP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.ChangeDefend(0, magic.nValue[0] * stackCount, 0);
        }
        /// <summary>
        /// Phục hồi sinh lực mỗi nửa giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void LifePotionV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phục hồi nội lực mỗi nửa giây
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ManaPotionV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Xác suất làm tấn công chí mạng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void FatallyStrikeP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentFatallyStrike += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng vật công nội
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPhysicsMagicV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_MagicPhysicsDamage.nValue[0] += magic.nValue[0] * stackCount;
            obj.m_MagicPhysicsDamage.nValue[2] += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng độc công nội
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPoisonMagicV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            AlgorithmProperty.MixPoisonDamage(obj.m_MagicPoisonDamage, magic, stackCount);
        }

        /// <summary>
        /// % sát thương tăng thêm
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void DamageAddedP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nDamageAddedPercent += magic.nValue[0] * stackCount;
        }
        /// <summary>
        /// Trạng thái miễn dịch sát thương
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StatusImmunityB(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (magic.nValue[0] > 0)
            {
                obj.m_CurrentStatusImmunity = true;
            }
            else
            {
                obj.m_CurrentStatusImmunity = false;
            }
        }

        /// <summary>
        /// Giá trị kinh nghiệm nhận được
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ExpEnhanceV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (!(obj is KPlayer))
            {
                return;
            }
            ((KPlayer)obj).ChangeExpEnhanceV(magic.nValue[0] * stackCount, magic.nValue[2] * stackCount);
        }
        /// <summary>
        /// Mỗi nửa giây phục hồi sinh lực dựa theo % Ngoại hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillId"></param>
        private static void FastLifeReplenishByVitality(GameObject obj, KMagicAttrib magic, int stackCount, int skillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer player))
            {
                /// Bỏ qua
                return;
            }

            /// Thiết lập giá trị
            player.m_nFastLifeReplenishByVitality += magic.nValue[0] * stackCount;
        }
        /// <summary>
        /// % kinh nghiệm nhận được
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ExpEnhanceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (!(obj is KPlayer))
            {
                return;
            }
            ((KPlayer)obj).ChangeExpEnhanceP(magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// % kinh nghiệm của kỹ năng cấp 120
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Add120SkillExpEnhanceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (!(obj is KPlayer))
                return;
            ((KPlayer)obj).Change120SKillExpEnhanceP(magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng cường ngũ hành tương khắc (gồm cả cường hóa và nhược hóa)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesConquarR(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentSeriesConquar += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Cường hóa ngũ hành tương khắc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesEnhanceR(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentSeriesEnhance += magic.nValue[0] * stackCount;
        }
        /// <summary>
        /// Tăng % lực tấn công kỹ năng theo mỗi 1% máu mất tính từ thời điểm kích hoạt
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillId"></param>
        private static void SkillDamageTrimPByLifeLoss(GameObject obj, KMagicAttrib magic, int stackCount, int skillId)
        {
            /// Lưu lại lượng sinh lực hiện tại
            obj.m_nLastLifeSkillDamageTrimP = obj.m_CurrentLife;
            /// Thiết lập giá trị
            obj.m_nSkillDamageTrimPByLifeLoss += magic.nValue[0] * stackCount / 100f;
        }

        /// <summary>
        /// % sát thương phải chịu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void DamageReceivedP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nDamageReceiveDecresedPercent += magic.nValue[0] * stackCount;
        }
        /// <summary>
        /// Lập tức phục hồi % sinh lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillId"></param>
        private static void HealHPP(GameObject obj, KMagicAttrib magic, int stackCount, int skillId)
        {
            /// Lượng hồi
            int hpHeal = magic.nValue[0] * stackCount * obj.m_CurrentLifeMax / 100;
            /// Mức máu cuối
            int finalHP = obj.m_CurrentLife + hpHeal;
            /// Nếu vượt quá ngưỡng
            if (finalHP > obj.m_CurrentLifeMax)
            {
                /// Thiết lập Max
                obj.m_CurrentLife = obj.m_CurrentLifeMax;
            }
        }
        /// <summary>
        /// Tăng vật công cơ bản dựa theo Ngoại hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillId"></param>
        private static void AddBaseDamageByVitality(GameObject obj, KMagicAttrib magic, int stackCount, int skillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer player))
            {
                /// Bỏ qua
                return;
            }
            /// Cập nhật giá trị
            player.m_nAddWeaponBaseDamageTrimByVitality += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Kháng ngũ hành tương khắc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesResP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentSeriesResist += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hóa giải sát thương không vượt quá % sát thương ban đầu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DynamicMagicShieldV(GameObject obj, KMagicAttrib magic, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                obj.m_CurrentDynamicShield = 0;
                obj.m_CurrentDynamicShieldMaxP = 0;
            }
            else
            {
                obj.m_CurrentDynamicShield = magic.nValue[0];
                obj.m_CurrentDynamicShieldMaxP = magic.nValue[1];
            }
        }

        /// <summary>
        /// Hạn chế tốc độ di chuyển
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void NoChangedMoveSpeed(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {

        }

        /// <summary>
        /// Thời gian duy trì độc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ChangePoisonTimeEnhanceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nPoisonTimeEnhanceP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Thiết lập trạng thái bảo vệ
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Protected(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.SetProtected(magic.nValue[0] * stackCount > 0);
        }

        /// <summary>
        /// Hủy bỏ trạng thái khiên nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void RemoveShield(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                /// Xóa Buff khiên nội lực tương ứng
                obj.Buffs.RemoveBuff(497);
                /// Cấm Buff khiên nội lực
                obj.Buffs.AddAvoidBuff(nSkillId, 497);
            }
            /// Nếu là pha Detach
            else
            {
                obj.Buffs.RemoveAvoidBuff(nSkillId, 497);
            }
        }

        /// <summary>
        /// Phát huy lực tấn công cơ bản
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SkillDamagePTrim(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nSkillDamagePTrim += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Phát huy lực tấn công kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SkillSelfDamagePTrim(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nSkillSelfDamagePTrim += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng % kinh nghiệm
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SkillExpAddtionP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nExpAddtionP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Thời gian kháng tất cả trạng thái ngũ hành
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllSeriesStateResistTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nState = (int)KE_STATE.emSTATE_SERISE_BEGIN; nState <= (int)KE_STATE.emSTATE_SERISE_END; ++nState)
            {
                obj.m_state[nState].StateRestTime += magic.nValue[0] * stackCount;
            }
        }



        /// <summary>
        /// Xác suất bị trạng thái ngũ hành giảm
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllSeriesStateResistRate(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nState = (int)KE_STATE.emSTATE_SERISE_BEGIN; nState <= (int)KE_STATE.emSTATE_SERISE_END; ++nState)
            {
                obj.m_state[nState].StateRestRate += magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Thời gian chịu trạng thái bất lợi
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllSpecialStateResistTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nState = (int)KE_STATE.emSTATE_SPECIAL_BEGIN; nState <= (int)KE_STATE.emSTATE_SPECIAL_END; ++nState)
            {
                obj.m_state[nState].StateRestTime += magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Xác suất bị trạng thái bất lợi
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AllSpecialStateResistRate(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nState = (int)KE_STATE.emSTATE_SPECIAL_BEGIN; nState <= (int)KE_STATE.emSTATE_SPECIAL_END; ++nState)
            {
                obj.m_state[nState].StateRestRate += magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Chịu sát thương chí mạng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DefenceDeadlyStrikeDamageTrim(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nDefenceDeadlyStrikeDamageTrim += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Cường hóa ngũ hành tương khắc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesEnhance(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nSeriesEnhance += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Nhược hóa ngũ hành tương khắc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesAbate(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nSeriesAbate += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Sát thương cơ bản của vũ khí
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void WeaponBaseDamageTrim(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nWeaponBaseDamageTrim += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hóa giải sát thương độc, không vượt quá % sát thương ban đầu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void PosionWeaken(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nPosionWeakenPoint += magic.nValue[0] * stackCount;
            obj.m_nPoisonWeakenMaxDamageP += magic.nValue[1];
        }

        /// <summary>
        /// Tấn công khi đánh chí mạng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DeadlyStrikeDamageEnhanceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_DeadlystrikeDamagePercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Xác suất phản đòn bùa chú
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ReturnSkillP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentReturnSkillPercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tỷ lệ bỏ qua bùa chú
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreCurseP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentIgnoreCursePercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tỷ lệ chí tử
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void FatalStrike(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentFatalStrikePercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Có thể nhìn thấy ẩn thân
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ShowHide(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (magic.nValue[0] > 0)
            {
                obj.m_CurrentShowHide = true;
            }
            else
            {
                obj.m_CurrentShowHide = false;
            }
        }
        /// <summary>
        /// Cấm sử dụng kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        private static void ForbidSkill(GameObject obj, KMagicAttrib magic)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] == -1)
            {
                /// Hủy cấm
                obj.RemoveForbidSkill(magic.nValue[0]);
            }
            /// Nếu là pha Attach
            else
            {
                /// Cấm
                obj.AddForbidSkill(magic.nValue[0]);
            }
        }

        /// <summary>
        /// Nội lực bị mất khi chịu sát thương độc
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Poison2DecManaP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentPoison2Mana += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Sinh mệnh của khiên nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaticMagicShieldV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddShieldState(magic.nValue[0] * stackCount, magic.nValue[1] * stackCount);
        }

        /// <summary>
        /// Sinh mệnh tối đa của khiên nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaticMagicShieldMaxP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha hủy Buff
            if (magic.nValue[0] <= 0)
            {
                obj.m_CurrentDynamicMagicShield = 0;
                obj.m_CurrentDynamicMagicShield_SkillID = -1;
            }
            else
            {
                /// Kỹ năng khiên nội lực
                obj.m_CurrentDynamicMagicShield_SkillID = nSkillId;
                /// Nội lực tối đa hiện có
                int currentMPMax = obj.m_CurrentManaMax;
                /// Chuyển hóa số lần nội lực tối đa thành khiên
                obj.m_CurrentDynamicMagicShield = currentMPMax * magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Dùng toàn bộ nội lực chuyển hóa thành khiên nội lực, tối thiểu còn 15% nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StaticMagicShieldCurP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha hủy Buff
            if (magic.nValue[0] <= 0)
            {
                obj.m_CurrentDynamicMagicShield = 0;
                obj.m_CurrentDynamicMagicShield_SkillID = -1;
            }
            else
            {
                /// Kỹ năng khiên nội lực
                obj.m_CurrentDynamicMagicShield_SkillID = nSkillId;
                /// Nội lực hiện có
                int currentMP = obj.m_CurrentMana;
                /// Giảm toàn bộ nội lực của đối tượng
                obj.m_CurrentMana = 0;

                /// Chuyển hóa % nội lực đã mất thành khiên
                obj.m_CurrentDynamicMagicShield = currentMP * magic.nValue[0] / 100 * stackCount;
            }
        }

        /// <summary>
        /// Thêm kinh nghiệm nhận được từ đồng đội
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddExpShare(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nShareExpP += magic.nValue[0] * stackCount;
            if (obj.m_nShareExpP < 0)
            {
                obj.m_nShareExpP = 0;
            }
        }

        /// <summary>
        /// Giảm kinh nghiệm khi tử vong
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SubExpLose(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nSubExpPLost += magic.nValue[0] * stackCount;
            if (obj.m_nSubExpPLost < 0)
            {
                obj.m_nSubExpPLost = 0;
            }
        }

        /// <summary>
        /// Mỗi khoảng thời gian sẽ bỏ qua nửa giây chịu sát thương
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreAttackOnTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nIgnoreAttackOnTime = magic.nValue[0] * stackCount;
            obj.m_nIgnoreAttackDuration = magic.nValue[1] * stackCount;
        }

        /// <summary>
        /// Vô địch
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Invincibility(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                obj.m_CurrentInvincibility = 1;
            }
            /// Nếu là pha Detach
            else
            {
                obj.m_CurrentInvincibility = 0;
            }
        }

        /// <summary>
        /// Bỏ qua cạm bẫy
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreTrap(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                obj.m_DetectTrap = true;
            }
            /// Nếu là pha Detach
            else
            {
                obj.m_DetectTrap = false;
            }
        }

        /// <summary>
        /// Hồi sinh
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Revive(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (!(obj is KPlayer))
            {
                return;
            }

            ((KPlayer)obj).ReceiveCure(magic.nValue[0] * stackCount, magic.nValue[1] * stackCount, magic.nValue[2] * stackCount);
        }

        /// <summary>
        /// Bỏ qua né tránh đối thủ
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreDefenseEnhanceV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (magic == null)
            {
                return;
            }
            obj.m_CurrentIgnoreDefense += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Bỏ qua % né tránh đối thủ
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreDefenseEnhanceP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentIgnoreDefensePercent += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Khóa đối tượng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void Locked(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {

        }

        /// <summary>
        /// Tỷ lệ bỏ qua kháng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreResistP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Ngũ hành tương ứng
            int nSeries = Math.Abs(magic.nValue[2]);
            if (obj.m_sIgnoreResists.TryGetValue(nSeries, out KNPC_IGNORERESIST ignoreRes))
            {
                ignoreRes.nIgnoreResistPMin += magic.nValue[1] * stackCount;
                ignoreRes.nIgnoreResistPMax += magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Xác suất tránh công tầm gần
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ChangeFeature1(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_sIgnoreRangerDamage += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Xác suất tránh công tầm xa
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ChangeFeature2(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_sIgnoreMeleeDamage += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Thêm số lần kỹ năng ngụy trang còn
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddStealFeatureSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {

        }

        /// <summary>
        /// Số lần kỹ năng ngụy trang còn
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void StealFeature(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*KNpc* pNpcLauncher = g_cMagicAttribMgr.GetMagicLauncher();
			if (!pNpcLauncher)
				return;
			INT nPlayer = pNpcLauncher->GetPlayerIdx();
			if (nPlayer <= 0)
				return;
			if (!npc.IsFeatureChanged())
				return;
			INT nTemplateId = npc.GetCurFeature();
			for (INT i = 0; i < countof(g_cMiscMgr.m_cGlbVal.m_arForbitFeature); i++)
			{
				if (nTemplateId == g_cMiscMgr.m_cGlbVal.m_arForbitFeature[i])
					return;
			}
			KITEM_POS pos;
			pos.eRoom = emROOM_EQUIP;
			pos.nX = emEQUIPPOS_MASK;
			INT nItem = Player[nPlayer].m_cPlayerItem.GetItemByPos(pos);
			if (nItem <= 0)
				return;
			KItem* pMask = &Item[nItem];
			//±ØÐëÔÚµÚ6¸ö»ù´¡ÊôÐÔÊÇ¼ÓÍµfeatureµÄ¼¼ÄÜ
			if (magic_addstealfeatureskill != pMask->m_aryBaseAttrib[6].nAttribType || pMask->m_aryBaseAttrib[6].nValue[0] <= 0)
				return;
			//½«Íæ¼ÒÃæ¾ßµÄÑù×ÓÉèÎªÕâ¸ö
			pMask->m_aryBaseAttrib[0].nValue[0] = nTemplateId;
			//¼¼ÄÜÊ¹ÓÃ´ÎÊý¼õÒ»
			pMask->m_aryBaseAttrib[6].nValue[0]--;

			MIX_PROTOCOL data;
			data.bySubProtocol = enumMASK_STEAL_TIMES;
			data.dwData[0] = pMask->GetID();
			data.dwData[1] = nTemplateId;
			data.dwData[2] = pMask->m_aryBaseAttrib[6].nValue[0];
			data.wProtocolSize = ((CHAR*)&data.dwData[3] - (CHAR*)&data) - 1;
			g_pCoreServer->SendData(Player[nPlayer].m_nNetConnectIdx, &data, data.wProtocolSize + 1);

			KNpcFeature nfNewFeature;
			nfNewFeature.m_eFeaturePriority = KNpcFeature::feature_prior_disguise_mask;
			nfNewFeature.m_eFeatureState = KNpcFeature::feature_state_disguise;
			nfNewFeature.m_eAvailableTimeType = KStateNode::state_time_normal;
			nfNewFeature.m_nNpcTemplateId = nTemplateId;
			pNpcLauncher->ChangeFeature(&nfNewFeature);*/
        }

        /// <summary>
        /// Giảm % giây thời gian giãn cách xuất hiện của kỹ năng
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AutoSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (magic.nValue[0] < 0)
            {
                obj.RemoveAutoSkill(magic.nValue[0] * -1);
            }
            else
            {
                obj.AddAutoSkill(magic.nValue[0], magic.nValue[1], nSkillId);
            }
        }


        /// <summary>
        /// Ẩn thân
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        /// <param name="skillLevel"></param>
        private static void Hide(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId, int skillLevel)
        {
            /// Nếu là pha hủy
            if (magic.nValue[1] < 0)
            {
                return;
            }

            /// Dữ liệu kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(nSkillId);
            if (skillData == null)
            {
                return;
            }
            SkillLevelRef skill = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = skillLevel,
                BonusLevel = 0,
                CanStudy = false,
            };

            /// Thêm trạng thái tàng hình
            obj.AddInvisibleState(skill.Level, magic.nValue[1] * stackCount, magic.nValue[0]);
        }

        /// <summary>
        /// Mỗi nửa giây phục hồi sinh lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void GrowLife(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentLifeFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Mỗi nửa giây phục hồi nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void GrowMana(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentManaFastReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Mỗi nửa giây phục hồi thể lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void GrowStamina(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_CurrentFastStaminaReplenish += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hủy bỏ trạng thái của kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void RemoveSkillState(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            int nStateSkillId = magic.nValue[0];
            obj.RemoveStateSkillEffect(nStateSkillId, true);
        }

        private static void DomainChangeSelf(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			if (g_cMagicAttribMgr.IsEndProcess())
			{
				npc.m_nChangeSelf = -1;
			}
			else
			{
				_ASSERT(magic.nValue[0] && magic.nValue[1] >= 0);
				npc.m_nChangeSelf = MAKELONG(magic.nValue[0], magic.nValue[1]);
				if (magic.nValue[2] > 0)
				{
					npc.m_nChangeSelfType = magic.nValue[2];
				}
				else
				{
					npc.m_nChangeSelfType = KNpc::emKCHANGESELF_ALL;
				}
			}
			*/
        }

        private static void AddDomainSkill1(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			if (magic.nValue[0] != 0)
			{
				if (g_cMagicAttribMgr.IsEndProcess())
				{
					npc.m_SkillList.RemoveSkill(abs(magic.nValue[0]));
				}
				else
				{
					npc.m_SkillList.AddSkill(magic.nValue[0], magic.nValue[1], 0, KNpcSkill::emKNPCFIGHTSKILL_TYPE_NONE, TRUE);
				}
			}
			*/
        }

        private static void AddDomainSkill2(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			if (magic.nValue[0] != 0)
			{
				if (g_cMagicAttribMgr.IsEndProcess())
				{
					npc.m_SkillList.RemoveSkill(abs(magic.nValue[0]));
				}
				else
				{
					npc.m_SkillList.AddSkill(magic.nValue[0], magic.nValue[1], 0, KNpcSkill::emKNPCFIGHTSKILL_TYPE_NONE, TRUE);
				}
			}
			*/
        }

        private static void AddDomainSkill3(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			if (magic.nValue[0] != 0)
			{
				if (g_cMagicAttribMgr.IsEndProcess())
				{
					npc.m_SkillList.RemoveSkill(abs(magic.nValue[0]));
				}
				else
				{
					npc.m_SkillList.AddSkill(magic.nValue[0], magic.nValue[1], 0, KNpcSkill::emKNPCFIGHTSKILL_TYPE_NONE, TRUE);
				}
			}
			*/
        }

        private static void StealSkillState(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			if (g_cMagicAttribMgr.IsEndProcess())
			{
				npc.ClearStealState();
			}
			*/
        }

        /// <summary>
        /// Hút % nội lực của mục tiêu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void WasteManaP(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                return;
            }

            int nWastCurrentMana = obj.m_CurrentManaMax * magic.nValue[0] * stackCount / 100;
            obj.m_CurrentMana -= nWastCurrentMana;
            if (obj.m_CurrentMana < 0)
            {
                obj.m_CurrentMana = 0;
            }
            else if (obj.m_CurrentMana > obj.m_CurrentManaMax)
            {
                obj.m_CurrentMana = obj.m_CurrentManaMax;
            }
        }

        /// <summary>
        /// Cường hóa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddEnchant(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			int nEnchantId = Math.Abs(magic.nValue[0]);
			int nEnchantLevel = Math.Abs(magic.nValue[1]);
			if (g_cMagicAttribMgr.IsEndProcess())
				obj.m_SkillList.m_cNpcEnchant.RemoveEnchant(nEnchantId, nEnchantLevel);
			else
				obj.m_SkillList.m_cNpcEnchant.AddEnchant(nEnchantId, nEnchantLevel);
			*/
        }

        /// <summary>
        /// Xác suất tránh công kích
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            int type = Math.Abs(magic.nValue[2]);
            if (type == 1)
            {
                obj.m_sIgnorePhysicDamage += magic.nValue[0] * stackCount;
            }
            else if (type == 2)
            {
                obj.m_sIgnoreMagicDamage += magic.nValue[0] * stackCount;
            }
            else if (type == 3)
            {
                obj.m_sIgnorePhysicDamage += magic.nValue[0] * stackCount;
                obj.m_sIgnoreMagicDamage += magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Loại bỏ và cấm trạng thái tương tự
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreInitiative(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Tổng số hiệu ứng sẽ bị cấm
            int number = magic.nValue[0];
            /// Nếu là pha Attach
            if (number > 0)
            {
                /// Duyệt để tìm các hiệu ứng bị cấm
                for (int i = 1; i <= number; i++)
                {
                    BuffDataEx buff = obj.Buffs.GetRandomPositiveBuff();
                    /// Nếu không tìm thấy Buff chủ động hỗ trợ nào
                    if (buff == null)
                    {
                        break;
                    }
                    else
                    {
                        /// Ngừng thực thi Buff
                        obj.Buffs.RemoveBuff(buff);
                        /// Thêm vào dánh cấm
                        obj.Buffs.AddAvoidBuff(nSkillId, buff.Skill.SkillID);
                    }
                }
            }
            /// Nếu là pha Detach
            else
            {
                obj.Buffs.RemoveAllAvoidBuffs(nSkillId);
            }
        }

        /// <summary>
        /// Khi tấn công mục tiêu sẽ bị phản đòn
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void InfectCurse(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                obj.m_sReflectDamageWhenHit = 0;
                obj.m_sReflectDamageWhenHitTargetID = -1;
            }
            /// Nếu là pha Attach
            else
            {
                /// Lấy Buff tương ứng
                BuffDataEx buff = obj.Buffs.GetBuff(nSkillId);
                /// Chủ nhân của bùa
                if (buff != null && buff.CurseOwner != null)
                {
                    obj.m_sReflectDamageWhenHit = magic.nValue[0];
                    obj.m_sReflectDamageWhenHitTargetID = buff.CurseOwner.RoleID;
                }
            }
        }

        /// <summary>
        /// Thời gian độc phát
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void InfectPoison(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Thiết lập giá trị
            obj.m_nPoisonInfect += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Mục tiêu bị trạng thái này sau số giây sẽ có tỷ lệ bị đột tử
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SuddenDeath(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			KNpc* pNpc = g_cMagicAttribMgr.GetMagicLauncher();
			if (!pNpc)
			{
				_ASSERT(FALSE);
				return;
			}
			ASSERT(!g_cMagicAttribMgr.IsEndProcess());  // ²»Ó¦¸Ã½«´ËÊôÐÔÌîÐ´ÔÚ×´Ì¬¼¼ÄÜÖÐ
			npc.AddSpecialState(KNpc::emSTATE_SUDDENDEATH, magic.nValue[1], magic.nValue[0], *pNpc, nSkillId);
			npc.m_nLastSuddenDeathIdx = pNpc->m_Index;
			*/
        }

        /// <summary>
        /// Tăng sát thương của kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddSkillDamagePercent(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            if (!(obj is KPlayer))
            {
                return;
            }

            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1] * stackCount;
            cMagic.nValue[2] = magic.nValue[2];

            KPlayer player = obj as KPlayer;
            player.Skills.AddAppendDamagePercentToSkill(cMagic.nValue[0], cMagic.nValue[1]);
        }

        /// <summary>
        /// Tăng thời gian ẩn thân của kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddSkillHideTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Giảm tốc độ xuất chiêu của kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DecreaseSkillCastTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Tăng cấp kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddSkillLevel(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer player))
            {
                /// Bỏ qua
                return;
            }

            /// ID kỹ năng
            int skillID = Math.Abs(magic.nValue[0]);
            /// Cấp độ
            int level = magic.nValue[1];

            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                /// Mức máu hiện tại
                int currentHP = player.m_CurrentLife;
                /// Mức khí hiện tại
                int currentMP = player.m_CurrentMana;
                /// Mức thể lực hiện tại
                int currentStamina = player.m_CurrentStamina;

                /// Detach các chỉ số bị động
                player.DeactivateAuraPassiveAndEnchantSkills();

                /// Giá trị tương ứng
                player.Skills.RemoveAdditionSkill(skillID, -level);

                /// Attach các chỉ số bị động
                player.ReactivateAuraPassiveAndEnchantSkills();

                /// Cập nhật mức máu
                player.m_CurrentLife = Math.Min(player.m_CurrentLifeMax, currentHP);
                /// Cập nhật mức khí
                player.m_CurrentMana = Math.Min(player.m_CurrentManaMax, currentMP);
                /// Cập nhật mức thể lực
                player.m_CurrentStamina = Math.Min(player.m_CurrentStaminaMax, currentStamina);
            }
            /// Nếu là pha Attach
            else
            {
                /// Mức máu hiện tại
                int currentHP = player.m_CurrentLife;
                /// Mức khí hiện tại
                int currentMP = player.m_CurrentMana;
                /// Mức thể lực hiện tại
                int currentStamina = player.m_CurrentStamina;

                /// Detach các chỉ số bị động
                player.DeactivateAuraPassiveAndEnchantSkills();

                /// Giá trị tương ứng
                player.Skills.RemoveAdditionSkill(skillID, level);

                /// Attach các chỉ số bị động
                player.ReactivateAuraPassiveAndEnchantSkills();

                /// Cập nhật mức máu
                player.m_CurrentLife = Math.Min(player.m_CurrentLifeMax, currentHP);
                /// Cập nhật mức khí
                player.m_CurrentMana = Math.Min(player.m_CurrentManaMax, currentMP);
                /// Cập nhật mức thể lực
                player.m_CurrentStamina = Math.Min(player.m_CurrentStaminaMax, currentStamina);
            }
        }

        /// <summary>
        /// Tăng phạm vi bay của đạn
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddMissileRange(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Tăng tỷ lệ xuyên suốt mục tiêu của đạn
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddMissileThroughRate(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/

        }

        /// <summary>
        /// Tăng sát thương mỗi khi xuyên mục tiêu của đạn
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddPowerWhenCol(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/

        }

        /// <summary>
        /// Tăng phạm vi hiểu quả của đạn sau mỗi lần xuyên mục tiêu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddRangeWhenCol(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Thêm kỹ năng đi kèm lúc xuất chiêu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddStartSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = Math.Abs(magic.nValue[1]);
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Thêm kỹ năng đạn bay (trường hợp bẫy nổ)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddFlySkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = Math.Abs(magic.nValue[1]);
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Thêm kỹ năng khi biến mất
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddVanishSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = Math.Abs(magic.nValue[1]);
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Giảm thời gian giãn cách xuất hiện của kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DecAutoSkillCDTime(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = Math.Abs(magic.nValue[1]);
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Tăng số đạn bay
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddMissileNum(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        /// <summary>
        /// Bỏ qua trạng thái bất lợi
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IgnoreDebuff(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            for (int nState = (int)(KE_STATE.emSTATE_BEGIN); nState < (int)(KE_STATE.emSTATE_NUM); ++nState)
            {
                /// Nếu là pha Attach
                if (magic.nValue[0] > 0)
                {
                    obj.RemoveSpecialState((KE_STATE)nState, true);
                    obj.m_state[nState].IgnoreRate = true;
                }
                /// Nếu là pha Detach
                else
                {
                    obj.m_state[nState].IgnoreRate = false;
                }
            }
        }


        private static void AddIgnoreSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = Math.Abs(magic.nValue[1]);
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        private static void AddFastManaReplenish_V(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            KMagicAttrib cMagic = new KMagicAttrib();
            cMagic.nAttribType = magic.nAttribType;
            cMagic.nValue[0] = Math.Abs(magic.nValue[0]);
            cMagic.nValue[1] = magic.nValue[1];
            cMagic.nValue[2] = magic.nValue[2];

            /*obj.m_SkillList.m_cSkillAddition.AddMagicAddtion(cMagic.nValue[0], cMagic, g_cMagicAttribMgr.IsEndProcess());*/
        }

        private static void ListenMsg(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*if (obj is KPlayer)
			{
				if (magic.nValue[0] > 0)
					Player[npc.GetPlayerIdx()].m_ListenMsgInfo |= 1 << magic.nValue[0];
				else
					Player[npc.GetPlayerIdx()].m_ListenMsgInfo &= ~(1 << (-1 * magic.nValue[0]));

			}*/
        }

        /// <summary>
        /// May mắn của đồng hành
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="KMagicAttrib"></param>
        /// <param name=""></param>
        /// <param name="nSkillId"></param>
        /// <returns></returns>
        private static void PARTNER_LuckyV(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*if (npc.IsPartner() && npc.m_pPartner)
			{
				KFightPartner* pPartner = (KFightPartner*)npc.m_pPartner;
				pPartner->SetCurLuckAttrib(pPartner->GetCurLuckAttrib() + magic.nValue[0], FALSE);
			}*/
        }

        /// <summary>
        /// Mỗi khoảng thời gian bỏ qua nửa giây tấn công
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void IngoreAttack(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_sIgnoreAttack.nIgnoreAttackBase += magic.nValue[2] * stackCount;
            obj.m_sIgnoreAttack.nValueModify += magic.nValue[1] * stackCount;
            obj.m_sIgnoreAttack.nIgnoreAttack += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tăng thêm sát thương khi tấn công quái
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DamageAdded(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nDamageAddedPercentWhenHitNPC += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Thời gian sát thương thêm ngũ hành tương đương ngũ hành hiện tại
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void SeriesStateAdded(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_nAddedSeriesStateRate += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Né trạng thái
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void DefenseState(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /*
			npc.m_nDefenseState += magic.nValue[0];
			KNPC_DEFENSE_STATE SyncDefenseState;
			SyncDefenseState.ProtocolType = s2c_npcdefensestate;
			SyncDefenseState.dwNpcId = npc.m_dwID;
			SyncDefenseState.byDefense = (BYTE)npc.m_nDefenseState;
			npc.SendDataToNearRegion(&SyncDefenseState, sizeof(SyncDefenseState));
			*/
        }

        /// <summary>
        /// Xóa Cooldown kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void ClearCD(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer))
            {
                return;
            }

            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                obj.m_sIgnoreSkillCooldowns = true;
                obj.m_sIgnoreSkillCooldownsBuffID = nSkillId;
                /// Xóa toàn bộ Cooldown các kỹ năng cũ
                (obj as KPlayer).Skills.ClearSkillCooldownListExcept(nSkillId);
            }
            /// Nếu là pha Detach
            else
            {
                obj.m_sIgnoreSkillCooldowns = false;
                obj.m_sIgnoreSkillCooldownsBuffID = 0;
            }
        }

        /// <summary>
        /// Giảm sinh lực tương đương khoảng cách
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void RdcLifeWithDis(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                obj.m_sRdcLifeWithDis.nMultiple = magic.nValue[0] * stackCount;
                obj.m_sRdcLifeWithDis.nMaxDis = magic.nValue[1];
                obj.m_sRdcLifeWithDis.nDamageSkillId = magic.nValue[2];
                obj.m_sRdcLifeWithDis.nSkillId = nSkillId;
                obj.m_sRdcLifeWithDis.nPrePosX = (int)obj.CurrentPos.X;
                obj.m_sRdcLifeWithDis.nPrePosY = (int)obj.CurrentPos.Y;

                /// Lấy Buff tương ứng
                BuffDataEx buff = obj.Buffs.GetBuff(nSkillId);
                if (buff != null && buff.CurseOwner != null)
                {
                    obj.m_sRdcLifeWithDis.nLauncher = buff.CurseOwner;
                    obj.m_sRdcLifeWithDis.nSkillLevel = buff.Level;
                }
            }
            /// Nếu là pha Detach
            else
            {
                obj.m_sRdcLifeWithDis.nMultiple = 0;
                obj.m_sRdcLifeWithDis.nMaxDis = 0;
                obj.m_sRdcLifeWithDis.nDamageSkillId = 0;
                obj.m_sRdcLifeWithDis.nSkillId = 0;
                obj.m_sRdcLifeWithDis.nPrePosX = 0;
                obj.m_sRdcLifeWithDis.nPrePosY = 0;
                obj.m_sRdcLifeWithDis.nLauncher = null;
                obj.m_sRdcLifeWithDis.nSkillLevel = 0;
            }
        }

        /// <summary>
        /// Kỹ năng cộng thêm với mỗi kẻ địch xung quanh
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AddedWithEnemyCount(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_sAddedWithEnemy_SkillID = magic.nValue[0];
            obj.m_sAddedWithEnemy_MaxCount = magic.nValue[1];
            obj.m_sAddedWithEnemy_Range = magic.nValue[2];
            obj.m_sAddedWithEnemy_OwnerSkillID = nSkillId;
        }

        /// <summary>
        /// Cường hóa sát thương bởi % nội lực hiện có
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void AttackEnchceByMana(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                obj.m_nAttackAddedByMana = 0;
            }
            else
            {
                obj.m_nAttackAddedByMana = magic.nValue[0] * stackCount;
            }
        }

        /// <summary>
        /// Sử dụng kỹ năng không làm mất trạng thái tàng hình
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void KeepHide(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu không phải người chơi
            if (!(obj is KPlayer))
            {
                return;
            }

            /// Người chơi
            KPlayer player = obj as KPlayer;

            /// Nếu là pha Attach
            if (magic.nValue[0] > 0)
            {
                if (!player.m_InvisibleNoLostOnUseSkills.Contains(nSkillId))
                {
                    player.m_InvisibleNoLostOnUseSkills.Add(nSkillId);
                }
            }
            /// Nếu là pha Detach
            else
            {
                if (player.m_InvisibleNoLostOnUseSkills.Contains(nSkillId))
                {
                    player.m_InvisibleNoLostOnUseSkills.Remove(nSkillId);
                }
            }
        }


        // CODE ADD 29_3_2021

        /// <summary>
        /// Giảm thời gian ngũ hành của trạng thái ngũ hành chỉ định
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        // NTOE CHECK
        private static void State_ResistTime_Modify(GameObject obj, KMagicAttrib magic, KE_STATE State, int stackCount, int nSkillId)
        {
            obj.m_state[(int)State].StateRestTime += magic.nValue[0] * stackCount;
        }


        /// <summary>
        /// Giảm tỉ lệ của hiệu ứng ngũ hành chỉ định
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void State_ResistRate_Modify(GameObject obj, KMagicAttrib magic, KE_STATE State, int stackCount, int nSkillId)
        {
            obj.m_state[(int)State].StateRestRate += magic.nValue[0] * stackCount;
        }



        /// <summary>
        /// Tăng thời gian của hiệu ứng ngũ hành chỉ định
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void State_AttackTime_Modify(GameObject obj, KMagicAttrib magic, KE_STATE State, int stackCount, int nSkillId)
        {
            obj.m_state[(int)State].StateAddTime += magic.nValue[0] * stackCount;
        }



        /// <summary>
        /// Tăng thời gian của trạng thái ngũ hành chỉ định
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        private static void State_AttackRate_Modify(GameObject obj, KMagicAttrib magic, KE_STATE State, int stackCount, int nSkillId)
        {
            obj.m_state[(int)State].StateAddRate += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Hóa giải và miễn nhiễm trạng thái ngũ hành tương ứng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <param name="nSkillId"></param>
        /// <param name="isDetachPhrase"></param>
        private static void State_RemoveAndImmune(GameObject obj, KE_STATE state, int nSkillId, bool isDetachPhrase = false)
        {
            /// Thiết lập miễn dịch với trạng thái
            obj.m_state[(int)state].IgnoreRate = isDetachPhrase ? false : true;

            /// Nếu là pha Attach
            if (!isDetachPhrase)
            {
                /// Xóa trạng thái tương ứng
                obj.RemoveSpecialState(state, true);
            }
        }

        /// <summary>
        /// Sát thương chuyển hóa thành nội lực
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void DamageToAddMana(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_Damage2AddManaP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Tỷ lệ miễn nhiễm với kỹ năng
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ImmuneToSkill(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            /// Nếu là pha Detach
            if (magic.nValue[0] < 0)
            {
                obj.RemoveImmuneToSkill(Math.Abs(magic.nValue[0]));
            }
            /// Nếu là pha Attach
            else
            {
                obj.SetImmuneToSkill(magic.nValue[0], magic.nValue[1]);
            }
        }

        /// <summary>
        /// Giảm sát thương nội công tầm gần
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ReduceNearMagicDamage(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_ReduceNearMagicDamageP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Giảm sát thương nội công tầm xa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ReduceFarMagicDamage(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_ReduceFarMagicDamageP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Giảm sát thương ngoại công tầm gần
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ReduceNearPhysicDamage(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_ReduceNearPhysicDamageP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Giảm sát thương ngoại công tầm xa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ReduceFarPhysicDamage(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_ReduceFarPhysicDamageP += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Khi nội lực đầy, tăng % sát thương
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="stackCount"></param>
        /// <param name="nSkillId"></param>
        private static void ManaToSkillEnhance(GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_DamageMultipleWhenFullMana += magic.nValue[0] * stackCount;
        }

        /// <summary>
        /// Trả về giá trị cộng dồn thuộc tính tương ứng
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="stackCount"></param>
        /// <returns></returns>
        public static PropertyDictionary GetStackProperties(PropertyDictionary pd, int stackCount)
        {
            /// Tạo đối tượng ProDict mới
            PropertyDictionary resultPd = new PropertyDictionary();
            /// Duyệt danh sách khóa trong ProDict gốc
            foreach (KeyValuePair<int, object> pair in pd.GetDictionary())
            {
                /// Nếu không phải KMagicAttrib
                if (!(pair.Value is KMagicAttrib))
                {
                    continue;
                }
                MAGIC_ATTRIB attribType = (MAGIC_ATTRIB)pair.Key;
                KMagicAttrib magicAttrib = (pair.Value as KMagicAttrib).Clone();

                switch (attribType)
                {
                    case MAGIC_ATTRIB.magic_lifepotion_v:
                    case MAGIC_ATTRIB.magic_manapotion_v:
                    case MAGIC_ATTRIB.magic_meleedamagereturn_v:
                    case MAGIC_ATTRIB.magic_meleedamagereturn_p:
                    case MAGIC_ATTRIB.magic_rangedamagereturn_v:
                    case MAGIC_ATTRIB.magic_rangedamagereturn_p:
                    case MAGIC_ATTRIB.magic_adddefense_v:
                    case MAGIC_ATTRIB.magic_adddefense_p:
                    case MAGIC_ATTRIB.magic_armordefense_v:
                    case MAGIC_ATTRIB.magic_lifemax_v:
                    case MAGIC_ATTRIB.magic_lifemax_p:
                    case MAGIC_ATTRIB.magic_life_v:
                    case MAGIC_ATTRIB.magic_lifereplenish_v:
                    case MAGIC_ATTRIB.magic_fastlifereplenish_v:
                    case MAGIC_ATTRIB.magic_manamax_v:
                    case MAGIC_ATTRIB.magic_manamax_p:
                    case MAGIC_ATTRIB.magic_mana_v:
                    case MAGIC_ATTRIB.magic_manareplenish_v:
                    case MAGIC_ATTRIB.magic_fastmanareplenish_v:
                    case MAGIC_ATTRIB.magic_staminamax_v:
                    case MAGIC_ATTRIB.magic_staminamax_p:
                    case MAGIC_ATTRIB.magic_stamina_v:
                    case MAGIC_ATTRIB.magic_staminareplenish_v:
                    case MAGIC_ATTRIB.magic_faststaminareplenish_v:
                    case MAGIC_ATTRIB.magic_strength_v:
                    case MAGIC_ATTRIB.magic_dexterity_v:
                    case MAGIC_ATTRIB.magic_vitality_v:
                    case MAGIC_ATTRIB.magic_energy_v:
                    case MAGIC_ATTRIB.magic_poisontimereduce_p:
                    case MAGIC_ATTRIB.magic_poisondamagereduce_v:
                    case MAGIC_ATTRIB.magic_fastwalkrun_p:
                    case MAGIC_ATTRIB.magic_fastwalkrun_v:
                    case MAGIC_ATTRIB.magic_trice_eff_attackrating_v:
                    case MAGIC_ATTRIB.magic_attackratingenhance_v:
                    case MAGIC_ATTRIB.magic_trice_eff_attackrating_p:
                    case MAGIC_ATTRIB.magic_attackratingenhance_p:
                    case MAGIC_ATTRIB.magic_attackspeed_v:
                    case MAGIC_ATTRIB.magic_castspeed_v:
                    case MAGIC_ATTRIB.magic_weapondamagemin_v:
                    case MAGIC_ATTRIB.magic_weaponmagicmin_v:
                    case MAGIC_ATTRIB.magic_addphysicsdamage_v:
                    case MAGIC_ATTRIB.magic_addfiredamage_v:
                    case MAGIC_ATTRIB.magic_addcolddamage_v:
                    case MAGIC_ATTRIB.magic_addlightingdamage_v:
                    case MAGIC_ATTRIB.magic_addpoisondamage_v:
                    case MAGIC_ATTRIB.magic_addphysicsdamage_p:
                    case MAGIC_ATTRIB.magic_addphysicsmagic_p:
                    case MAGIC_ATTRIB.magic_allskill_v:
                    case MAGIC_ATTRIB.magic_deadlystrikeenhance_r:
                    case MAGIC_ATTRIB.magic_manashield_p:
                    case MAGIC_ATTRIB.magic_fatallystrikeenhance_p:
                    case MAGIC_ATTRIB.magic_addfiremagic_v:
                    case MAGIC_ATTRIB.magic_addcoldmagic_v:
                    case MAGIC_ATTRIB.magic_addlightingmagic_v:
                    case MAGIC_ATTRIB.magic_addpoisonmagic_v:
                    case MAGIC_ATTRIB.magic_expenhance_p:
                    case MAGIC_ATTRIB.magic_add120skillexpenhance_p:
                    case MAGIC_ATTRIB.magic_seriesconquar_r:
                    case MAGIC_ATTRIB.magic_seriesenhance_r:
                    case MAGIC_ATTRIB.magic_seriesres_p:
                    case MAGIC_ATTRIB.magic_deadlystrikedamageenhance_p:
                    case MAGIC_ATTRIB.magic_lifereplenish_p:
                    case MAGIC_ATTRIB.magic_manareplenish_p:
                    case MAGIC_ATTRIB.magic_returnskill_p:
                    case MAGIC_ATTRIB.magic_poisondamagereturn_v:
                    case MAGIC_ATTRIB.magic_poisondamagereturn_p:
                    case MAGIC_ATTRIB.magic_poison2decmana_p:
                    case MAGIC_ATTRIB.magic_staticmagicshieldcur_p:
                    case MAGIC_ATTRIB.magic_lifegrow_v:
                    case MAGIC_ATTRIB.magic_managrow_v:
                    case MAGIC_ATTRIB.magic_staminagrow_v:
                    case MAGIC_ATTRIB.magic_addexpshare:
                    case MAGIC_ATTRIB.magic_poisontimeenhance_p:
                    case MAGIC_ATTRIB.magic_skilldamageptrim:
                    case MAGIC_ATTRIB.magic_defencedeadlystrikedamagetrim:
                    case MAGIC_ATTRIB.magic_seriesenhance:
                    case MAGIC_ATTRIB.magic_seriesabate:
                    case MAGIC_ATTRIB.magic_weaponbasedamagetrim:
                    case MAGIC_ATTRIB.magic_posionweaken:
                    case MAGIC_ATTRIB.magic_skillselfdamagetrim:
                    case MAGIC_ATTRIB.magic_skillexpaddtion_p:
                    case MAGIC_ATTRIB.magic_allseriesstateresisttime:
                    case MAGIC_ATTRIB.magic_allseriesstateresistrate:
                    case MAGIC_ATTRIB.magic_allspecialstateresisttime:
                    case MAGIC_ATTRIB.magic_allspecialstateresistrate:
                    case MAGIC_ATTRIB.magic_wastemanap:
                    case MAGIC_ATTRIB.magic_damage_all_resist:
                    case MAGIC_ATTRIB.magic_damage_physics_resist:
                    case MAGIC_ATTRIB.magic_damage_poison_resist:
                    case MAGIC_ATTRIB.magic_damage_fire_resist:
                    case MAGIC_ATTRIB.magic_damage_light_resist:
                    case MAGIC_ATTRIB.magic_damage_series_resist:
                    case MAGIC_ATTRIB.magic_damage_physics_receive_p:
                    case MAGIC_ATTRIB.magic_damage_poison_receive_p:
                    case MAGIC_ATTRIB.magic_damage_cold_receive_p:
                    case MAGIC_ATTRIB.magic_damage_fire_receive_p:
                    case MAGIC_ATTRIB.magic_damage_light_receive_p:
                    case MAGIC_ATTRIB.magic_damage_return_receive_p:
                    case MAGIC_ATTRIB.magic_ignoreskill:
                    case MAGIC_ATTRIB.magic_ignoredefenseenhance_v:
                    case MAGIC_ATTRIB.magic_ignoredefenseenhance_p:
                    case MAGIC_ATTRIB.magic_damage_added:
                    case MAGIC_ATTRIB.magic_seriesstate_added:
                    case MAGIC_ATTRIB.magic_rdclifewithdis:
                    case MAGIC_ATTRIB.magic_attackenhancebycostmana_p:
                    case MAGIC_ATTRIB.state_hurt_resisttime:
                    case MAGIC_ATTRIB.state_hurt_resistrate:
                    case MAGIC_ATTRIB.state_hurt_attacktime:
                    case MAGIC_ATTRIB.state_hurt_attackrate:
                    case MAGIC_ATTRIB.state_weak_resisttime:
                    case MAGIC_ATTRIB.state_weak_resistrate:
                    case MAGIC_ATTRIB.state_weak_attacktime:
                    case MAGIC_ATTRIB.state_weak_attackrate:
                    case MAGIC_ATTRIB.state_slowall_resisttime:
                    case MAGIC_ATTRIB.state_slowall_resistrate:
                    case MAGIC_ATTRIB.state_slowall_attacktime:
                    case MAGIC_ATTRIB.state_slowall_attackrate:
                    case MAGIC_ATTRIB.state_burn_resisttime:
                    case MAGIC_ATTRIB.state_burn_resistrate:
                    case MAGIC_ATTRIB.state_burn_attacktime:
                    case MAGIC_ATTRIB.state_burn_attackrate:
                    case MAGIC_ATTRIB.state_stun_resisttime:
                    case MAGIC_ATTRIB.state_stun_resistrate:
                    case MAGIC_ATTRIB.state_stun_attacktime:
                    case MAGIC_ATTRIB.state_stun_attackrate:
                    case MAGIC_ATTRIB.state_fixed_resisttime:
                    case MAGIC_ATTRIB.state_fixed_resistrate:
                    case MAGIC_ATTRIB.state_fixed_attacktime:
                    case MAGIC_ATTRIB.state_fixed_attackrate:
                    case MAGIC_ATTRIB.state_palsy_resisttime:
                    case MAGIC_ATTRIB.state_palsy_resistrate:
                    case MAGIC_ATTRIB.state_palsy_attacktime:
                    case MAGIC_ATTRIB.state_palsy_attackrate:
                    case MAGIC_ATTRIB.state_slowrun_resisttime:
                    case MAGIC_ATTRIB.state_slowrun_resistrate:
                    case MAGIC_ATTRIB.state_slowrun_attacktime:
                    case MAGIC_ATTRIB.state_slowrun_attackrate:
                    case MAGIC_ATTRIB.state_freeze_resisttime:
                    case MAGIC_ATTRIB.state_freeze_resistrate:
                    case MAGIC_ATTRIB.state_freeze_attacktime:
                    case MAGIC_ATTRIB.state_freeze_attackrate:
                    case MAGIC_ATTRIB.state_confuse_resisttime:
                    case MAGIC_ATTRIB.state_confuse_resistrate:
                    case MAGIC_ATTRIB.state_confuse_attacktime:
                    case MAGIC_ATTRIB.state_confuse_attackrate:
                    case MAGIC_ATTRIB.state_knock_resisttime:
                    case MAGIC_ATTRIB.state_knock_resistrate:
                    case MAGIC_ATTRIB.state_knock_attacktime:
                    case MAGIC_ATTRIB.state_knock_attackrate:
                    case MAGIC_ATTRIB.state_silence_resisttime:
                    case MAGIC_ATTRIB.state_silence_resistrate:
                    case MAGIC_ATTRIB.state_silence_attacktime:
                    case MAGIC_ATTRIB.state_silence_attackrate:
                    case MAGIC_ATTRIB.state_drag_resisttime:
                    case MAGIC_ATTRIB.state_drag_resistrate:
                    case MAGIC_ATTRIB.state_drag_attacktime:
                    case MAGIC_ATTRIB.state_drag_attackrate:
                    case MAGIC_ATTRIB.state_zhican_resisttime:
                    case MAGIC_ATTRIB.state_zhican_resistrate:
                    case MAGIC_ATTRIB.state_zhican_attacktime:
                    case MAGIC_ATTRIB.state_zhican_attackrate:
                    case MAGIC_ATTRIB.state_float_resisttime:
                    case MAGIC_ATTRIB.state_float_resistrate:
                    case MAGIC_ATTRIB.state_float_attacktime:
                    case MAGIC_ATTRIB.state_float_attackrate:
                    case MAGIC_ATTRIB.magic_ignore_curse_p:
                    case MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p:
                    case MAGIC_ATTRIB.magic_damage2addmana_p:
                    case MAGIC_ATTRIB.magic_immune_skill_1:
                    case MAGIC_ATTRIB.magic_immune_skill_2:
                    case MAGIC_ATTRIB.magic_immune_skill_3:
                    case MAGIC_ATTRIB.magic_immune_skill_4:
                    case MAGIC_ATTRIB.magic_immune_skill_5:
                    case MAGIC_ATTRIB.magic_immune_skill_6:
                    case MAGIC_ATTRIB.magic_immune_skill_7:
                    case MAGIC_ATTRIB.magic_immune_skill_8:
                    case MAGIC_ATTRIB.magic_immune_skill_9:
                    case MAGIC_ATTRIB.magic_immune_skill_10:
                    case MAGIC_ATTRIB.magic_immune_skill_11:
                    case MAGIC_ATTRIB.magic_immune_skill_12:
                    case MAGIC_ATTRIB.magic_immune_skill_13:
                    case MAGIC_ATTRIB.magic_immune_skill_14:
                    case MAGIC_ATTRIB.magic_immune_skill_15:
                    case MAGIC_ATTRIB.magic_immune_skill_16:
                    case MAGIC_ATTRIB.magic_immune_skill_17:
                    case MAGIC_ATTRIB.magic_immune_skill_18:
                    case MAGIC_ATTRIB.magic_immune_skill_19:
                    case MAGIC_ATTRIB.magic_immune_skill_20:
                    case MAGIC_ATTRIB.magic_reduce_near_magic_damage:
                    case MAGIC_ATTRIB.magic_reduce_far_magic_damage:
                    case MAGIC_ATTRIB.magic_reduce_near_physic_damage:
                    case MAGIC_ATTRIB.magic_reduce_far_physic_damage:
                    case MAGIC_ATTRIB.magic_manatoskill_enhance:
                    case MAGIC_ATTRIB.magic_changefeature1:
                    case MAGIC_ATTRIB.magic_changefeature2:
                    case MAGIC_ATTRIB.magic_infectpoison:
                    case MAGIC_ATTRIB.magic_addfiredamage_p:
                    case MAGIC_ATTRIB.magic_addcolddamage_p:
                    case MAGIC_ATTRIB.magic_addlightingdamage_p:
                    case MAGIC_ATTRIB.magic_addpoisondamage_p:
                    case MAGIC_ATTRIB.magic_addcoldmagic_p:
                    case MAGIC_ATTRIB.magic_addfiremagic_p:
                    case MAGIC_ATTRIB.magic_addlightingmagic_p:
                    case MAGIC_ATTRIB.magic_addpoisonmagic_p:
                    case MAGIC_ATTRIB.magic_damage_inc_p:
                    case MAGIC_ATTRIB.magic_redeivedamage_dec_p2:
                    case MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p:
                    case MAGIC_ATTRIB.magic_fastlifereplenish_byvitality:
                    case MAGIC_ATTRIB.magic_skilldamageptrimbylesshp:
                    case MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality:
                        {
                            magicAttrib.nValue[0] *= stackCount;
                            break;
                        }
                    case MAGIC_ATTRIB.magic_weapondamagemax_v:
                    case MAGIC_ATTRIB.magic_weaponmagicmax_v:
                        {
                            magicAttrib.nValue[2] *= stackCount;
                            break;
                        }
                    case MAGIC_ATTRIB.magic_hide:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5:
                    case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6:
                        {
                            magicAttrib.nValue[1] *= stackCount;
                            break;
                        }
                    case MAGIC_ATTRIB.magic_trice_eff_steallife_p:
                    case MAGIC_ATTRIB.magic_steallifeenhance_p:
                    case MAGIC_ATTRIB.magic_trice_eff_stealstamina_p:
                    case MAGIC_ATTRIB.magic_stealstaminaenhance_p:
                    case MAGIC_ATTRIB.magic_trice_eff_stealmana_p:
                    case MAGIC_ATTRIB.magic_stealmanaenhance_p:
                    case MAGIC_ATTRIB.magic_dynamicmagicshield_v:
                    case MAGIC_ATTRIB.magic_staticmagicshield_v:
                    case MAGIC_ATTRIB.magic_staticmagicshieldmax_p:
                    case MAGIC_ATTRIB.magic_ignoreattackontime:
                    case MAGIC_ATTRIB.magic_ignoreresist_p:
                        {
                            magicAttrib.nValue[0] *= stackCount;
                            magicAttrib.nValue[1] *= stackCount;
                            break;
                        }
                    case MAGIC_ATTRIB.magic_addphysicsmagic_v:
                    case MAGIC_ATTRIB.magic_expenhance_v:
                        {
                            magicAttrib.nValue[0] *= stackCount;
                            magicAttrib.nValue[2] *= stackCount;
                            break;
                        }
                    case MAGIC_ATTRIB.magic_revive:
                    case MAGIC_ATTRIB.magic_ignoreattack:
                        {
                            magicAttrib.nValue[0] *= stackCount;
                            magicAttrib.nValue[1] *= stackCount;
                            magicAttrib.nValue[2] *= stackCount;
                            break;
                        }
                }
                /// Thêm vào ProDict kết quả
                resultPd.Set<KMagicAttrib>(pair.Key, magicAttrib);
            }
            /// Trả về kết quả
            return resultPd;
        }

        //END CODE ADD

        /// <summary>
        /// Kích hoạt hoặc hủy kích hoạt thuộc tính tương ứng
        /// </summary>
        /// <param name="magicAttrib"></param>
        /// <param name="go"></param>
        /// <param name="isDetachPhrase"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillID"></param>
        /// <param name="skillLevel"></param>
        public static void AttachProperty(KMagicAttrib magicAttrib, GameObject go, bool isDetachPhrase, int stackCount = 1, int skillID = -1, int skillLevel = 1)
        {
            /// Nếu không tồn tại MagicAttrib
            if (magicAttrib == null)
            {
                return;
            }

            /// Nếu là pha Detach
            if (isDetachPhrase)
            {
                magicAttrib *= -1;
            }

            switch (magicAttrib.nAttribType)
            {
                case MAGIC_ATTRIB.magic_lifepotion_v:
                    KTAttributesModifier.LifePotionV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manapotion_v:
                    KTAttributesModifier.ManaPotionV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_meleedamagereturn_v:
                    KTAttributesModifier.MeleeDamageReturnV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_meleedamagereturn_p:
                    KTAttributesModifier.MeleeDamageReturnP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_rangedamagereturn_v:
                    KTAttributesModifier.RangeDamageReturnV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_rangedamagereturn_p:
                    KTAttributesModifier.RangeDamageReturnP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_adddefense_v:
                    KTAttributesModifier.ArmorDefenseV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_adddefense_p:
                    KTAttributesModifier.ArmorDefenseP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_armordefense_v:
                    KTAttributesModifier.ArmorDefenseV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lifemax_v:
                    KTAttributesModifier.LifeMaxV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lifemax_p:
                    KTAttributesModifier.LifeMaxP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_life_v:
                    KTAttributesModifier.LifeV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lifereplenish_v:
                    KTAttributesModifier.LifeReplenishV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fastlifereplenish_v:
                    KTAttributesModifier.FastLifeReplenishV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manamax_v:
                    KTAttributesModifier.ManaMaxV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manamax_p:
                    KTAttributesModifier.ManaMaxP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_mana_v:
                    KTAttributesModifier.ManaV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manareplenish_v:
                    KTAttributesModifier.ManaReplenishV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fastmanareplenish_v:
                    KTAttributesModifier.FastManaReplenish(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staminamax_v:
                    KTAttributesModifier.StaminaMaxV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staminamax_p:
                    KTAttributesModifier.StaminaMaxP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_stamina_v:
                    KTAttributesModifier.StaminaV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staminareplenish_v:
                    KTAttributesModifier.StaminaReplenishV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_faststaminareplenish_v:
                    KTAttributesModifier.FastStaminaReplenish(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_strength_v:
                    KTAttributesModifier.StrengthV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_dexterity_v:
                    KTAttributesModifier.DexterityV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_vitality_v:
                    KTAttributesModifier.VitalityV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_energy_v:
                    KTAttributesModifier.EnergyV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_poisontimereduce_p:
                    KTAttributesModifier.PoisonTimeReduceP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_poisondamagereduce_v:
                    KTAttributesModifier.PoisonDamageReduceV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fastwalkrun_p:
                    KTAttributesModifier.FastWalkRunP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fastwalkrun_v:
                    KTAttributesModifier.FastWalkRunV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_attackrating_v:
                    KTAttributesModifier.AttackRatingV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_attackratingenhance_v:
                    KTAttributesModifier.AttackRatingV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_attackrating_p:
                    KTAttributesModifier.AttackRatingP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_attackratingenhance_p:
                    KTAttributesModifier.AttackRatingP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_attackspeed_v:
                    KTAttributesModifier.AttackSpeedV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_castspeed_v:
                    KTAttributesModifier.CastSpeedV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_weapondamagemin_v:
                    KTAttributesModifier.AddWeaponDamageMinV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_weapondamagemax_v:
                    KTAttributesModifier.AddWeaponDamageMaxV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_weaponmagicmin_v:
                    KTAttributesModifier.AddWeaponMagicMinV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_weaponmagicmax_v:
                    KTAttributesModifier.AddWeaponMagicMaxV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addphysicsdamage_v:
                    KTAttributesModifier.AddPhysicsDamageV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addfiredamage_v:
                    KNpcAttribGroup_Damage.ModEnhanceDamage(DAMAGE_TYPE.damage_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addcolddamage_v:
                    KNpcAttribGroup_Damage.ModEnhanceDamage(DAMAGE_TYPE.damage_cold, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addlightingdamage_v:
                    KNpcAttribGroup_Damage.ModEnhanceDamage(DAMAGE_TYPE.damage_light, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addpoisondamage_v:
                    KTAttributesModifier.AddPoisonDamageV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addphysicsdamage_p:
                    KTAttributesModifier.AddPhysicsDamageP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addphysicsmagic_p:
                    KTAttributesModifier.AddPhysicsMagicP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lucky_v:
                    KTAttributesModifier.LuckyV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_steallife_p:
                    KTAttributesModifier.StealLifeP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_steallifeenhance_p:
                    KTAttributesModifier.StealLifeP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_stealstamina_p:
                    KTAttributesModifier.StealStaminaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_stealstaminaenhance_p:
                    KTAttributesModifier.StealStaminaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_stealmana_p:
                    KTAttributesModifier.StealManaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_stealmanaenhance_p:
                    KTAttributesModifier.StealManaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_allskill_v:
                    KTAttributesModifier.AllSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_metalskill_v:
                    KTAttributesModifier.MetalSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_woodskill_v:
                    KTAttributesModifier.WoodSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_waterskill_v:
                    KTAttributesModifier.WaterSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fireskill_v:
                    KTAttributesModifier.FireSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_earthskill_v:
                    KTAttributesModifier.EarthSkillV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_deadlystrikeenhance_r:
                    KTAttributesModifier.DeadlyStrikeR(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manashield_p:
                    KTAttributesModifier.ManaShieldP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_fatallystrikeenhance_p:
                    KTAttributesModifier.FatallyStrikeP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addphysicsmagic_v:
                    KTAttributesModifier.AddPhysicsMagicV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addfiremagic_v:
                    KNpcAttribGroup_Damage.ModEnhanceMagic(DAMAGE_TYPE.damage_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addcoldmagic_v:
                    KNpcAttribGroup_Damage.ModEnhanceMagic(DAMAGE_TYPE.damage_cold, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addlightingmagic_v:
                    KNpcAttribGroup_Damage.ModEnhanceMagic(DAMAGE_TYPE.damage_light, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addpoisonmagic_v:
                    KTAttributesModifier.AddPoisonMagicV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_statusimmunity_b:
                    KTAttributesModifier.StatusImmunityB(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_expenhance_v:
                    KTAttributesModifier.ExpEnhanceV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_expenhance_p:
                    KTAttributesModifier.ExpEnhanceP(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_add120skillexpenhance_p:
                    KTAttributesModifier.Add120SkillExpEnhanceP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesconquar_r:
                    KTAttributesModifier.SeriesConquarR(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesenhance_r:
                    KTAttributesModifier.SeriesEnhanceR(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesres_p:
                    KTAttributesModifier.SeriesResP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_dynamicmagicshield_v:
                    KTAttributesModifier.DynamicMagicShieldV(go, magicAttrib, skillID);
                    break;
                case MAGIC_ATTRIB.magic_nomovespeed:
                    KTAttributesModifier.NoChangedMoveSpeed(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_changefeature1:
                    KTAttributesModifier.ChangeFeature1(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_changefeature2:
                    KTAttributesModifier.ChangeFeature2(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_stealfeature:
                    KTAttributesModifier.StealFeature(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addstealfeatureskill:
                    KTAttributesModifier.AddStealFeatureSkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_deadlystrikedamageenhance_p:
                    KTAttributesModifier.DeadlyStrikeDamageEnhanceP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lifereplenish_p:
                    KTAttributesModifier.LifeReplenishP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_manareplenish_p:
                    KTAttributesModifier.ManaReplenishP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_returnskill_p:
                    KTAttributesModifier.ReturnSkillP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_poisondamagereturn_v:
                    KTAttributesModifier.PoisonDamageReturnV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_poisondamagereturn_p:
                    KTAttributesModifier.PoisonDamageReturnP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_autoskill:
                    KTAttributesModifier.AutoSkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_hide:
                    KTAttributesModifier.Hide(go, magicAttrib, stackCount, skillID, skillLevel);
                    break;
                case MAGIC_ATTRIB.magic_poison2decmana_p:
                    KTAttributesModifier.Poison2DecManaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staticmagicshield_v:
                    KTAttributesModifier.StaticMagicShieldV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staticmagicshieldmax_p:
                    KTAttributesModifier.StaticMagicShieldMaxP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staticmagicshieldcur_p:
                    KTAttributesModifier.StaticMagicShieldCurP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lifegrow_v:
                    KTAttributesModifier.GrowLife(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_managrow_v:
                    KTAttributesModifier.GrowMana(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_staminagrow_v:
                    KTAttributesModifier.GrowStamina(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addexpshare:
                    KTAttributesModifier.AddExpShare(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_subexplose:
                    KTAttributesModifier.SubExpLose(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoreattackontime:
                    KTAttributesModifier.IgnoreAttackOnTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_poisontimeenhance_p:
                    KTAttributesModifier.ChangePoisonTimeEnhanceP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_protected:
                    KTAttributesModifier.Protected(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_removestate:
                    KTAttributesModifier.RemoveSkillState(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_removeshield:
                    KTAttributesModifier.RemoveShield(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilldamageptrim:
                    KTAttributesModifier.SkillDamagePTrim(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_defencedeadlystrikedamagetrim:
                    KTAttributesModifier.DefenceDeadlyStrikeDamageTrim(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesenhance:
                    KTAttributesModifier.SeriesEnhance(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesabate:
                    KTAttributesModifier.SeriesAbate(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_weaponbasedamagetrim:
                    KTAttributesModifier.WeaponBaseDamageTrim(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_posionweaken:
                    KTAttributesModifier.PosionWeaken(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skillselfdamagetrim:
                    KTAttributesModifier.SkillSelfDamagePTrim(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skillexpaddtion_p:
                    KTAttributesModifier.SkillExpAddtionP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_allseriesstateresisttime:
                    KTAttributesModifier.AllSeriesStateResistTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_allseriesstateresistrate:
                    KTAttributesModifier.AllSeriesStateResistRate(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_allspecialstateresisttime:
                    KTAttributesModifier.AllSpecialStateResistTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_allspecialstateresistrate:
                    KTAttributesModifier.AllSpecialStateResistRate(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_domainchangeself:
                    KTAttributesModifier.DomainChangeSelf(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_adddomainskill1:
                    KTAttributesModifier.AddDomainSkill1(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_adddomainskill2:
                    KTAttributesModifier.AddDomainSkill2(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_adddomainskill3:
                    KTAttributesModifier.AddDomainSkill3(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_stealskillstate:
                    KTAttributesModifier.StealSkillState(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_wastemanap:
                    KTAttributesModifier.WasteManaP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addenchant:
                    KTAttributesModifier.AddEnchant(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_all_resist:
                    KTAttributesModifier.AllResR(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_physics_resist:
                    KNpcAttribGroup_Damage.ModResist(DAMAGE_TYPE.damage_physics, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_poison_resist:
                    KNpcAttribGroup_Damage.ModResist(DAMAGE_TYPE.damage_poison, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_cold_resist:
                    KNpcAttribGroup_Damage.ModResist(DAMAGE_TYPE.damage_cold, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_fire_resist:
                    KNpcAttribGroup_Damage.ModResist(DAMAGE_TYPE.damage_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_light_resist:
                    KNpcAttribGroup_Damage.ModResist(DAMAGE_TYPE.damage_light, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_series_resist:
                    KTAttributesModifier.ModResist(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_physics_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_physics, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_poison_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_poison, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_cold_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_cold, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_fire_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_light_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_light, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_return_receive_p:
                    KNpcAttribGroup_Damage.ModReceivePercent(DAMAGE_TYPE.damage_return, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_prop_showhide:
                    KTAttributesModifier.ShowHide(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_prop_invincibility:
                    KTAttributesModifier.Invincibility(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_prop_ignoretrap:
                    KTAttributesModifier.IgnoreTrap(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_revive:
                    KTAttributesModifier.Revive(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoreskill:
                    KTAttributesModifier.IgnoreSkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoreinitiative:
                    KTAttributesModifier.IgnoreInitiative(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_infectcurse:
                    KTAttributesModifier.InfectCurse(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_infectpoison:
                    KTAttributesModifier.InfectPoison(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoredefenseenhance_v:
                    KTAttributesModifier.IgnoreDefenseEnhanceV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoredefenseenhance_p:
                    KTAttributesModifier.IgnoreDefenseEnhanceP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_suddendeath:
                    KTAttributesModifier.SuddenDeath(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent2:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent3:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent4:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent5:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_adddamagepercent6:
                    KTAttributesModifier.AddSkillDamagePercent(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addhidetime:
                    KTAttributesModifier.AddSkillHideTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_decreasepercasttime:
                    KTAttributesModifier.DecreaseSkillCastTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addskilllevel:
                    KTAttributesModifier.AddSkillLevel(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange2:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange3:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange4:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange5:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_missileaaddition_addrange6:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addmissilethroughrate:
                    KTAttributesModifier.AddMissileRange(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol:
                    KTAttributesModifier.AddPowerWhenCol(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addrangewhencol:
                    KTAttributesModifier.AddRangeWhenCol(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addstartskill:
                    KTAttributesModifier.AddStartSkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addflyskill:
                    KTAttributesModifier.AddFlySkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addvanishskill:
                    KTAttributesModifier.AddVanishSkill(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_decautoskillcdtime:
                    KTAttributesModifier.DecAutoSkillCDTime(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_addmissilenum:
                    KTAttributesModifier.AddMissileNum(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignore_debuff:
                    KTAttributesModifier.IgnoreDebuff(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_lucky_v_partner:
                    KTAttributesModifier.PARTNER_LuckyV(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_listen_msg:
                    KTAttributesModifier.ListenMsg(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoreattack:
                    KTAttributesModifier.IngoreAttack(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage_added:
                    KTAttributesModifier.DamageAdded(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_seriesstate_added:
                    KTAttributesModifier.SeriesStateAdded(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_locked:
                    KTAttributesModifier.Locked(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_ignoreresist_p:
                    KTAttributesModifier.IgnoreResistP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_defense_state:
                    KTAttributesModifier.DefenseState(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_clearcd:
                    KTAttributesModifier.ClearCD(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_rdclifewithdis:
                    KTAttributesModifier.RdcLifeWithDis(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addedwithenemycount:
                    KTAttributesModifier.AddedWithEnemyCount(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_attackenhancebycostmana_p:
                    KTAttributesModifier.AttackEnchceByMana(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_skilladdition_keephide:
                    KTAttributesModifier.KeepHide(go, magicAttrib, stackCount, skillID);
                    break;

                // CODE ADD 29_3_2021 -- ADD TRẠNG THÁI NGŨ HÀNH
                // THỌ THƯƠNG
                case MAGIC_ATTRIB.state_hurt_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_HURT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_hurt_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_HURT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_hurt_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_HURT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_hurt_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_HURT, stackCount, skillID);
                    break;
                //END THỌ THƯƠNG


                // SUY YẾU
                case MAGIC_ATTRIB.state_weak_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_WEAK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_weak_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_WEAK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_weak_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_WEAK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_weak_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_WEAK, stackCount, skillID);
                    break;
                //END SUY YẾU


                // SUY YẾU
                case MAGIC_ATTRIB.state_slowall_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWALL, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowall_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWALL, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowall_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWALL, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowall_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWALL, stackCount, skillID);
                    break;
                //END SUY YẾU



                // BỎNG
                case MAGIC_ATTRIB.state_burn_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_BURN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_burn_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_BURN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_burn_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_BURN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_burn_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_BURN, stackCount, skillID);
                    break;
                //END SUY YẾU

                // CHOÁNG
                case MAGIC_ATTRIB.state_stun_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_STUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_stun_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_STUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_stun_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_STUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_stun_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_STUN, stackCount, skillID);
                    break;
                //END CHOÁNG


                // ĐỊNH THÂN
                case MAGIC_ATTRIB.state_fixed_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FIXED, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_fixed_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FIXED, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_fixed_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FIXED, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_fixed_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FIXED, stackCount, skillID);
                    break;
                //END ĐỊNH THÂN

                // TÊ LIỆT
                case MAGIC_ATTRIB.state_palsy_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_PALSY, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_palsy_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_PALSY, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_palsy_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_PALSY, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_palsy_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_PALSY, stackCount, skillID);
                    break;
                //END TÊ LIỆT



                // GIẢM TỐC CHẠY
                case MAGIC_ATTRIB.state_slowrun_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWRUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowrun_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWRUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowrun_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWRUN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_slowrun_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SLOWRUN, stackCount, skillID);
                    break;
                //END GIẢM TỐC CHẠY



                // ĐÓNG BĂNG
                case MAGIC_ATTRIB.state_freeze_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FREEZE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_freeze_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FREEZE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_freeze_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FREEZE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_freeze_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FREEZE, stackCount, skillID);
                    break;
                //END ĐÓNG BĂNG



                // HỖN LOẠN
                case MAGIC_ATTRIB.state_confuse_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_CONFUSE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_confuse_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_CONFUSE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_confuse_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_CONFUSE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_confuse_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_CONFUSE, stackCount, skillID);
                    break;
                //END HỖN

                // ĐẨY LÙI

                case MAGIC_ATTRIB.state_knock_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_KNOCK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_knock_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_KNOCK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_knock_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_KNOCK, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_knock_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_KNOCK, stackCount, skillID);
                    break;
                //END ĐẨY LÙI


                // ĐẨY LÙI

                case MAGIC_ATTRIB.state_silence_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SILENCE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_silence_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SILENCE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_silence_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_SILENCE, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_silence_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_SILENCE, stackCount, skillID);
                    break;
                //END ĐẨY LÙI


                // KÉO LẠI

                case MAGIC_ATTRIB.state_drag_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_DRAG, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_drag_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_DRAG, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_drag_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_DRAG, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_drag_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_DRAG, stackCount, skillID);
                    break;
                //END KÉO LẠI


                case MAGIC_ATTRIB.state_zhican_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_ZHICAN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_zhican_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_ZHICAN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_zhican_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_ZHICAN, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_zhican_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_ZHICAN, stackCount, skillID);
                    break;


                /// THỔI BAY
                case MAGIC_ATTRIB.state_float_resisttime:
                    KTAttributesModifier.State_ResistTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FLOAT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_float_resistrate:
                    KTAttributesModifier.State_ResistRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FLOAT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_float_attacktime:
                    KTAttributesModifier.State_AttackTime_Modify(go, magicAttrib, KE_STATE.emSTATE_FLOAT, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.state_float_attackrate:
                    KTAttributesModifier.State_AttackRate_Modify(go, magicAttrib, KE_STATE.emSTATE_FLOAT, stackCount, skillID);
                    break;

                /// HÓA GIẢI
                case MAGIC_ATTRIB.state_hurt_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_HURT, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_weak_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_WEAK, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_slowall_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_SLOWALL, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_burn_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_BURN, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_stun_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_STUN, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_fixed_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_FIXED, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_palsy_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_PALSY, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_slowrun_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_SLOWRUN, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_freeze_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_FREEZE, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_confuse_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_CONFUSE, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_knock_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_KNOCK, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_silence_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_SILENCE, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.state_drag_ignore:
                    KTAttributesModifier.State_RemoveAndImmune(go, KE_STATE.emSTATE_DRAG, skillID, isDetachPhrase);
                    break;
                case MAGIC_ATTRIB.magic_ignore_curse_p:
                    KTAttributesModifier.IgnoreCurseP(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_trice_eff_fatallystrike_p:
                    KTAttributesModifier.FatalStrike(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_damage2addmana_p:
                    KTAttributesModifier.DamageToAddMana(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_immune_skill_1:
                case MAGIC_ATTRIB.magic_immune_skill_2:
                case MAGIC_ATTRIB.magic_immune_skill_3:
                case MAGIC_ATTRIB.magic_immune_skill_4:
                case MAGIC_ATTRIB.magic_immune_skill_5:
                case MAGIC_ATTRIB.magic_immune_skill_6:
                case MAGIC_ATTRIB.magic_immune_skill_7:
                case MAGIC_ATTRIB.magic_immune_skill_8:
                case MAGIC_ATTRIB.magic_immune_skill_9:
                case MAGIC_ATTRIB.magic_immune_skill_10:
                case MAGIC_ATTRIB.magic_immune_skill_11:
                case MAGIC_ATTRIB.magic_immune_skill_12:
                case MAGIC_ATTRIB.magic_immune_skill_13:
                case MAGIC_ATTRIB.magic_immune_skill_14:
                case MAGIC_ATTRIB.magic_immune_skill_15:
                case MAGIC_ATTRIB.magic_immune_skill_16:
                case MAGIC_ATTRIB.magic_immune_skill_17:
                case MAGIC_ATTRIB.magic_immune_skill_18:
                case MAGIC_ATTRIB.magic_immune_skill_19:
                case MAGIC_ATTRIB.magic_immune_skill_20:
                    KTAttributesModifier.ImmuneToSkill(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_reduce_near_magic_damage:
                    KTAttributesModifier.ReduceNearMagicDamage(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_reduce_far_magic_damage:
                    KTAttributesModifier.ReduceFarMagicDamage(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_reduce_near_physic_damage:
                    KTAttributesModifier.ReduceNearPhysicDamage(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_reduce_far_physic_damage:
                    KTAttributesModifier.ReduceFarPhysicDamage(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_manatoskill_enhance:
                    KTAttributesModifier.ManaToSkillEnhance(go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addfiredamage_p:
                    KNpcAttribGroup_Damage.ModEnhanceDamageP(KE_SERIES_TYPE.series_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addcolddamage_p:
                    KNpcAttribGroup_Damage.ModEnhanceDamageP(KE_SERIES_TYPE.series_water, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addlightingdamage_p:
                    KNpcAttribGroup_Damage.ModEnhanceDamageP(KE_SERIES_TYPE.series_earth, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addpoisondamage_p:
                    KNpcAttribGroup_Damage.ModEnhanceDamageP(KE_SERIES_TYPE.series_wood, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addfiremagic_p:
                    KNpcAttribGroup_Damage.ModEnhanceMagicP(KE_SERIES_TYPE.series_fire, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addcoldmagic_p:
                    KNpcAttribGroup_Damage.ModEnhanceMagicP(KE_SERIES_TYPE.series_water, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addlightingmagic_p:
                    KNpcAttribGroup_Damage.ModEnhanceMagicP(KE_SERIES_TYPE.series_earth, go, magicAttrib, stackCount, skillID);
                    break;
                case MAGIC_ATTRIB.magic_addpoisonmagic_p:
                    KNpcAttribGroup_Damage.ModEnhanceMagicP(KE_SERIES_TYPE.series_wood, go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_damage_inc_p:
                    KTAttributesModifier.DamageAddedP(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_redeivedamage_dec_p2:
                    KTAttributesModifier.DamageReceivedP(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_ban_skill_1:
                case MAGIC_ATTRIB.magic_ban_skill_2:
                case MAGIC_ATTRIB.magic_ban_skill_3:
                case MAGIC_ATTRIB.magic_ban_skill_4:
                case MAGIC_ATTRIB.magic_ban_skill_5:
                case MAGIC_ATTRIB.magic_ban_skill_6:
                    KTAttributesModifier.ForbidSkill(go, magicAttrib);
                    break;

                case MAGIC_ATTRIB.magic_addmaxhpbymaxmp_p:
                    KTAttributesModifier.AddMaxHPByMaxMP(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_fastlifereplenish_byvitality:
                    KTAttributesModifier.FastLifeReplenishByVitality(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_skilldamageptrimbylesshp:
                    Console.WriteLine("magicAttrib :" + magicAttrib.nValue[0]);
                    KTAttributesModifier.SkillDamageTrimPByLifeLoss(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_immediatereplbymaxstate_p:
                    KTAttributesModifier.HealHPP(go, magicAttrib, stackCount, skillID);
                    break;

                case MAGIC_ATTRIB.magic_addweaponbasedamagetrimbyvitality:
                    KTAttributesModifier.AddBaseDamageByVitality(go, magicAttrib, stackCount, skillID);
                    break;
            }
        }

        #region PropertyDictionary
        /// <summary>
        /// Cộng thuộc tính từ PropertyDictionary
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="go"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillID"></param>
        public static void AttachProperties(PropertyDictionary pd, GameObject go, int stackCount, int skillID, int level)
        {
            if (pd == null)
            {
                return;
            }

            foreach (KeyValuePair<int, object> pair in pd.GetDictionary())
            {
                if (!(pair.Value is KMagicAttrib))
                {
                    continue;
                }

                /// Kích hoạt thuộc tính tương ứng
                KTAttributesModifier.AttachProperty(pair.Value as KMagicAttrib, go, false, stackCount, skillID, level);
            }
        }

        /// <summary>
        /// Giảm thuộc tính từ PropertyDictionary
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="go"></param>
        /// <param name="stackCount"></param>
        /// <param name="skillID"></param>
        public static void DetachProperties(PropertyDictionary pd, GameObject go, int stackCount, int skillID, int level)
        {
            if (pd == null)
            {
                return;
            }

            foreach (KeyValuePair<int, object> pair in pd.GetDictionary())
            {
                if (!(pair.Value is KMagicAttrib))
                {
                    continue;
                }

                /// Hủy kích hoạt thuộc tính tương ứng
                KTAttributesModifier.AttachProperty(pair.Value as KMagicAttrib, go, true, stackCount, skillID, level);
            }
        }
        #endregion

        #region Base Values
        /// <summary>
        /// Tải dữ liệu Base nhân vật từ file cấu hình
        /// </summary>
        /// <param name="client"></param>
        public static void LoadRoleBaseAttributes(KPlayer client)
        {
            #region Điểm tiềm năng, kỹ năng
            client.SetBaseRemainPotentialPoints(KPlayerSetting.GetLevelPotential(client.m_Level));
            client.SetBaseSkillPoints(KPlayerSetting.GetLevelFightSkillPoint(client.m_Level));
            #endregion

            #region Sức, Thân, Ngoại, Nội
            client.ChangeCurStrength(KPlayerSetting.GetLevelStrength(client.m_Level));
            client.ChangeCurDexterity(KPlayerSetting.GetLevelDexterity(client.m_Level));
            client.ChangeCurVitality(KPlayerSetting.GetLevelVitality(client.m_Level));
            client.ChangeCurEnergy(KPlayerSetting.GetLevelEnergy(client.m_Level));
            #endregion

            #region Sinh lực, Nội lực, Thể lực, May mắn
            client.ChangeLifeMax(KPlayerSetting.GetLevelLife(client.m_Level), 0, 0);
            client.ChangeManaMax(KPlayerSetting.GetLevelMana(client.m_Level), 0, 0);
            client.ChangeStaminaMax(KPlayerSetting.GetLevelStamina(client.m_Level), 0, 0);
            client.ChangeCurLucky(KPlayerSetting.GetLevelLuck(client.m_Level));
            #endregion

            #region Tốc chạy
            client.ChangeRunSpeed(KPlayerSetting.GetLevelSpeedRun(client.m_Level), KPlayerSetting.GetLevelMoveSpeedAddP(client.m_Level), 0);
            #endregion

            #region Tốc đánh
            client.ChangeAttackSpeed(KPlayerSetting.GetLevelAttackSpeed(client.m_Level), 0);
            client.ChangeCastSpeed(KPlayerSetting.GetLevelCastSpeed(client.m_Level), 0);
            #endregion

            #region Chính xác và né tránh
            client.ChangeAttackRating(KPlayerSetting.GetLevelAttackRate(client.m_Level), 0, 0);
            client.ChangeDefend(KPlayerSetting.GetLevelDefence(client.m_Level), 0, 0);
            #endregion

            #region Vật công
            client.ChangePhysicsDamage(KPlayerSetting.GetLevelDamagePhysics(client.m_Level));
            client.ChangeMagicDamage(KPlayerSetting.GetLevelDamageMagic(client.m_Level));
            #endregion

            #region Kháng ngũ hành
            client.AddResist(DAMAGE_TYPE.damage_physics, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_metal, client.m_Level, client.m_Series));
            client.AddResist(DAMAGE_TYPE.damage_magic, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_metal, client.m_Level, client.m_Series));
            client.AddResist(DAMAGE_TYPE.damage_poison, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_wood, client.m_Level, client.m_Series));
            client.AddResist(DAMAGE_TYPE.damage_cold, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_water, client.m_Level, client.m_Series));
            client.AddResist(DAMAGE_TYPE.damage_fire, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_fire, client.m_Level, client.m_Series));
            client.AddResist(DAMAGE_TYPE.damage_light, KPlayerSetting.GetLevelResist(KE_SERIES_TYPE.series_earth, client.m_Level, client.m_Series));
            #endregion
        }
        #endregion
    }
}
