//#define USE_SEMAPHORE_ON_SKILL

using GameServer.Interface;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Linq;
using System.Threading;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic kỹ năng
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Kết quả của kỹ năng
        /// </summary>
        public enum SkillResult
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Né
            /// </summary>
            Miss = 1,
            /// <summary>
            /// Hóa giải
            /// </summary>
            Adjust = 2,
            /// <summary>
            /// Miễn dịch
            /// </summary>
            Immune = 3,
            /// <summary>
            /// Sát thương thường
            /// </summary>
            Normal = 4,
            /// <summary>
            /// Sát thương chí mạng
            /// </summary>
            Crit = 5,
        }

        /// <summary>
        /// Kết quả trả về của hàm sử dụng kỹ năng kỹ năng
        /// </summary>
        public enum UseSkillResult
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Không có đối tượng xuất chiêu
            /// </summary>
            Caster_Is_Null = 1,
            /// <summary>
            /// Mục tiêu không nằm trong phạm vi đánh
            /// </summary>
            Target_Not_In_Range = 2,
            /// <summary>
            /// Không đủ nội lực
            /// </summary>
            Not_Enough_Mana = 3,
            /// <summary>
            /// Không tồn tại kỹ năng tương ứng
            /// </summary>
            No_Corresponding_Skill_Found = 4,
            /// <summary>
            /// Kỹ năng bị động không thể sử dụng
            /// </summary>
            Passive_Skill_Can_Not_Be_Used = 5,
            /// <summary>
            /// Kỹ năng đang trong trạng thái phục hồi
            /// </summary>
            Skill_Is_Cooldown = 6,
            /// <summary>
            /// Không có mục tiêu
            /// </summary>
            No_Target_Found = 7,
            /// <summary>
            /// Không tìm thấy dữ liệu cấu hình đạn
            /// </summary>
            Bullet_Data_Not_Found = 8,
            /// <summary>
            /// Không thể tấn công mục tiêu cùng trạng thái hòa bình
            /// </summary>
            Can_Not_Attack_Peace_Target = 9,
            /// <summary>
            /// Đối tượng xuất chiêu đã chết
            /// </summary>
            Caster_Is_Dead = 10,
            /// <summary>
            /// Không thể khinh công lúc này
            /// </summary>
            Can_Not_Fly_This_Time = 11,
            /// <summary>
            /// Trong trạng thái bị khống chế không thể dùng kỹ năng
            /// </summary>
            Can_Not_Use_Skill_While_Being_Locked = 12,
            /// <summary>
            /// Kỹ năng không thể dùng trong trạng thái cưỡi
            /// </summary>
            Can_Not_Use_Skill_While_Riding = 13,
            /// <summary>
            /// Vũ khí không thích hợp
            /// </summary>
            Unsuitable_Weapon = 14,
            /// <summary>
            /// Môn phái không thích hợp
            /// </summary>
            Unsuitable_Faction = 15,
            /// <summary>
            /// Hệ phái không thích hợp
            /// </summary>
            Unsuitable_Route = 16,
            /// <summary>
            /// Không đủ sinh lực
            /// </summary>
            Not_Enough_Life = 17,
            /// <summary>
            /// Không đủ thể lực
            /// </summary>
            Not_Enough_Stamina = 18,

            /// <summary>
            /// Không thể dùng kỹ năng lúc này
            /// </summary>
            Can_Not_Use_Skill = 19,
            /// <summary>
            /// Trạng thái yêu cầu không đủ
            /// </summary>
            Not_Enough_Buff = 20,
            /// <summary>
            /// Thành công
            /// </summary>
            Success,
        }

        #region Init
        /// <summary>
        /// Limit số luồng
        /// </summary>
        private static Semaphore limitation;

        /// <summary>
        /// Số luồng xử lý tối đa
        /// </summary>
        private static int MaxThreadsEach = 10;

        /// <summary>
        /// Khởi tạo đối tượng quản lý kỹ năng
        /// </summary>
        public static void Init()
        {
#if USE_SEMAPHORE_ON_SKILL
            /// Tạo mới đối tượng Semaphore
            KTSkillManager.limitation = new Semaphore(KTSkillManager.MaxThreadsEach, KTSkillManager.MaxThreadsEach);
#endif
        }

#if USE_SEMAPHORE_ON_SKILL
        /// <summary>
        /// Thực thi sự kiện ở kiến trúc đa luồng
        /// </summary>
        /// <param name="work"></param>
        /// <param name="OnError"></param>
        /// <returns></returns>
        private static void ExecuteActionWithMultipleThreading(Action work, Action OnError)
        {
            /// Tạo luồng mới
            Thread thread = new Thread(() => {
                try
                {
                    /// Đợi đến khi có luồng Free
                    KTSkillManager.limitation.WaitOne();
                    /// Thực thi công việc
                    work?.Invoke();
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at KTSkillManager\n{0}", ex.ToString()));
                    OnError?.Invoke();
                }
                finally
                {
                    KTSkillManager.limitation.Release();
                }
            });
            /// Hủy chế độ chạy ngầm
            thread.IsBackground = false;
            /// Bắt đầu luồng
            thread.Start();
        }
#endif
        #endregion

        #region Basic checking
        /// <summary>
        /// Tìm đối tượng theo ID có thể sử dụng kỹ năng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IObject FindSpriteByID(int id)
        {
            /// Người chơi
            KPlayer player = KTPlayerManager.Find(id);
            /// Nếu là người chơi
            if (player != null)
            {
                return player;
            }
            else
            {
                /// Quái
                Monster monster = KTMonsterManager.Find(id);
                /// Nếu là quái
                if (monster != null)
                {
                    return monster;
                }

                /// Pet
                Pet pet = KTPetManager.FindPet(id);
                /// Nếu là Pet
                if (pet != null)
                {
                    return pet;
                }
            }

            /// Không tìm thấy thì trả ra NULL
            return null;
        }

        /// <summary>
        /// Kiểm tra điều kiện sử dụng kỹ năng
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private static UseSkillResult UseSkill_CheckCondition(GameObject caster, GameObject target, SkillLevelRef skill)
        {
            /// Nếu đối tượng xuất chiêu NULL
            if (caster == null)
            {
                return UseSkillResult.Caster_Is_Null;
            }
            /// Nếu đối tượng xuất chiêu đã chết
            else if (caster.m_eDoing == KE_NPC_DOING.do_death || caster.m_eDoing == KE_NPC_DOING.do_revive)
            {
                return UseSkillResult.Caster_Is_Dead;
            }

            /// Kiểm tra tốc đánh
            if (caster is KPlayer)
            {
                KPlayer player = caster as KPlayer;

                /// Tốc đánh hiện tại
                int attackSpeed = player.GetCurrentAttackSpeed();
                int castSpeed = player.GetCurrentCastSpeed();
                /// Khoảng giãn cách mỗi lần ra chiêu
                long tickPerAttack;

                /// Kỹ năng lần trước
                SkillDataEx lastSkill = KSkill.GetSkillData(player.LastUseSkillID);
                /// Nếu có kỹ năng lần trước
                if (lastSkill != null)
                {
                    tickPerAttack = (long)(KTGlobal.AttackSpeedToFrameDuration(lastSkill.IsPhysical ? attackSpeed : castSpeed) * 1000);
                }
                /// Nếu không có kỹ năng lần trước
                else
                {
                    tickPerAttack = 0;
                }

                /// Nếu chưa đến thời gian có thể xuất chiêu
                if (KTGlobal.GetCurrentTimeMilis() - player.LastUseSkillNoAffectAtkSpeedTick < 100 || KTGlobal.GetCurrentTimeMilis() - player.LastAttackTicks < tickPerAttack + KTGlobal.AttackSpeedAdditionDuration * 1000)
                {
                    return UseSkillResult.None;
                }
            }
            /// Nếu là pet
            else if (caster is Pet pet)
            {
                /// Tốc đánh hiện tại
                int attackSpeed = pet.GetCurrentAttackSpeed();
                int castSpeed = pet.GetCurrentCastSpeed();
                /// Khoảng giãn cách mỗi lần ra chiêu
                long tickPerAttack;

                /// Kỹ năng lần trước
                SkillDataEx lastSkill = KSkill.GetSkillData(pet.LastUseSkillID);
                /// Nếu có kỹ năng lần trước
                if (lastSkill != null)
                {
                    tickPerAttack = (long)(KTGlobal.AttackSpeedToFrameDuration(lastSkill.IsPhysical ? attackSpeed : castSpeed) * 1000);
                }
                /// Nếu không có kỹ năng lần trước
                else
                {
                    tickPerAttack = 0;
                }

                /// Nếu chưa đến thời gian có thể xuất chiêu
                if (KTGlobal.GetCurrentTimeMilis() - pet.LastUseSkillNoAffectAtkSpeedTick < 100 || KTGlobal.GetCurrentTimeMilis() - pet.LastAttackTicks < tickPerAttack + KTGlobal.AttackSpeedAdditionDuration * 1000)
                {
                    return UseSkillResult.None;
                }
            }
            else
            {
                /// Tốc đánh hiện tại
                int attackSpeed = caster.GetCurrentAttackSpeed();

                /// Công thức quy đổi này sau cần làm cho chính xác hơn
                long tickPerAttack = (long)(KTGlobal.AttackSpeedToFrameDuration(attackSpeed) * 1000);

                /// Nếu chưa đến thời gian có thể xuất chiêu
                if (KTGlobal.GetCurrentTimeMilis() - caster.LastUseSkillNoAffectAtkSpeedTick < 100 || KTGlobal.GetCurrentTimeMilis() - caster.LastAttackTicks < tickPerAttack + KTGlobal.AttackSpeedAdditionDuration * 1000)
                {
                    return UseSkillResult.None;
                }
            }

            /// ProDict của kỹ năng
            PropertyDictionary skillPd = skill.Properties;

            /// ProDict được hỗ trợ
            PropertyDictionary enchantPd = null;
            if (caster is KPlayer)
            {
                enchantPd = (caster as KPlayer).Skills.GetEnchantProperties(skill.SkillID);
            }

            /// Cự ly thi triển
            int attackRange = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_attackradius))
            {
                attackRange = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }
            /// Nếu có kỹ năng hỗ trợ tăng cự ly thi triển kỹ năng
            if (enchantPd != null && enchantPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_attackradius))
            {
                attackRange += enchantPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Kiểm tra nếu là kỹ năng bị động thì không cho dùng
            if (skill.Data.Type == 3)
            {
                return UseSkillResult.Passive_Skill_Can_Not_Be_Used;
            }
            /// Nếu không phải kỹ năng dùng được
            else if (skill.Data.Type == -1)
            {
                return UseSkillResult.None;
            }

            /// Nếu kỹ năng bị cấm sử dụng
            if (caster.IsSkillForbidden(skill.SkillID))
            {
                return UseSkillResult.Can_Not_Use_Skill;
            }

            /// Danh sách các Symbol Buff yêu cầu
            int[] requireBuffSymbols = new int[]
            {
                (int) MAGIC_ATTRIB.magic_skill_cost_buff1layers_v,
                (int) MAGIC_ATTRIB.magic_skill_cost_buff2layers_v,
                (int) MAGIC_ATTRIB.magic_skill_cost_buff3layers_v,
                (int) MAGIC_ATTRIB.magic_skill_cost_buff4layers_v,
                (int) MAGIC_ATTRIB.magic_skill_cost_buff5layers_v,
                (int) MAGIC_ATTRIB.magic_skill_cost_buff6layers_v,
            };
            /// Duyệt danh sách các Symbol Buff yêu cầu
            foreach (int symbolID in requireBuffSymbols)
            {
                /// Nếu kỹ năng yêu cầu Buff
                if (skillPd.ContainsKey(symbolID))
                {
                    /// MagicAttrib tương ứng
                    KMagicAttrib attrib = skillPd.Get<KMagicAttrib>(symbolID);
                    /// ID Buff
                    int buffID = attrib.nValue[0];
                    /// Xếp chồng
                    int stack = attrib.nValue[1];

                    /// Nếu không có xếp chồng
                    if (stack <= 0)
                    {
                        /// Nếu không có Buff tương ứng
                        if (!caster.Buffs.HasBuff(buffID))
                        {
                            return UseSkillResult.Not_Enough_Buff;
                        }
                    }
                    /// Nếu có xếp chồng
                    else
                    {
                        /// Buff tương ứng
                        BuffDataEx buffData = caster.Buffs.GetBuff(buffID);
                        /// Không tồn tại
                        if (buffData == null)
                        {
                            return UseSkillResult.Not_Enough_Buff;
                        }

                        /// Nếu không đủ số tầng
                        if (buffData.StackCount < stack)
                        {
                            return UseSkillResult.Not_Enough_Buff;
                        }
                    }
                }
            }

            /// Nếu là người chơi
            if (caster is KPlayer)
            {
                KPlayer player = caster as KPlayer;

                /// Có kỹ năng tương ứng trong SkillTree không
                if (!player.Skills.HasSkill(skill.SkillID))
                {
                    return UseSkillResult.No_Corresponding_Skill_Found;
                }

                /// Kiểm tra vũ khí có phù hợp không
                if (!skill.Data.WeaponLimit.Contains((int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ALL))
                {
                    /// Vũ khí hiện tại
                    GoodsData weapon = player.GetPlayEquipBody().GetItemByPostion((int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON)?._GoodDatas;

                    /// Nếu không tồn tại
                    if (weapon == null)
                    {
                        /// Coi như là tay không
                        if (!skill.Data.WeaponLimit.Contains((int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND))
                        {
                            /// Không thỏa mãn vũ khí yêu cầu
                            return UseSkillResult.Unsuitable_Weapon;
                        }
                    }
                    else
                    {
                        /// Loại vũ khí
                        if (ItemManager._TotalGameItem.TryGetValue(weapon.GoodsID, out ItemData itemData))
                        {
                            /// Nếu vũ khí yêu cầu không phù hợp
                            if (!skill.Data.WeaponLimit.Contains(itemData.Category))
                            {
                                /// Không thỏa mãn vũ khí yêu cầu
                                return UseSkillResult.Unsuitable_Weapon;
                            }
                        }
                    }
                }

                /// Kiểm tra môn phái có phù hợp không
                if (skill.Data.FactionID != 0)
                {
                    /// Nếu môn phái không phù hợp
                    if (player.m_cPlayerFaction.GetFactionId() != skill.Data.FactionID)
                    {
                        return UseSkillResult.Unsuitable_Faction;
                    }
                }

                /// Có thể dùng kỹ năng trong trạng thái cưỡi không
                if (player.IsRiding && skill.Data.HorseLimit)
                {
                    return UseSkillResult.Can_Not_Use_Skill_While_Riding;
                }

                /// Kiểm tra kỹ năng có đang trong trạng thái phục hồi không
                if (player.Skills.IsSkillCooldown(skill.SkillID) && (!player.m_sIgnoreSkillCooldowns || player.m_sIgnoreSkillCooldownsBuffID == skill.SkillID))
                {
                    return UseSkillResult.Skill_Is_Cooldown;
                }

                /// Kiểm tra năng lượng mất
                if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_cost_v))
                {
                    /// Số điểm mất
                    int pointCost = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_cost_v).nValue[0];

                    /// Nếu là loại mất sinh lực
                    if (skill.Data.SkillCostType == 0 && caster.m_CurrentLife < pointCost)
                    {
                        return UseSkillResult.Not_Enough_Life;
                    }
                    /// Nếu là loại mất nội lực
                    else if (skill.Data.SkillCostType == 1 && caster.m_CurrentMana < pointCost)
                    {
                        return UseSkillResult.Not_Enough_Mana;
                    }
                    /// Nếu là loại mất thể lực
                    else if (skill.Data.SkillCostType == 2 && caster.m_CurrentStamina < pointCost)
                    {
                        return UseSkillResult.Not_Enough_Stamina;
                    }
                }

                /// Nếu là kỹ năng khinh công
                if (skill.Data.Type == 1 && skill.Data.Form == 2 && player.m_eDoing == KE_NPC_DOING.do_jump)
                {
                    return UseSkillResult.Can_Not_Fly_This_Time;
                }
            }

            /// Nếu có mục tiêu
            if (target != null)
            {
                UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((int)caster.CurrentPos.X, (int)caster.CurrentPos.Y);
                UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int)target.CurrentPos.X, (int)target.CurrentPos.Y);
                float distanceToTarget = UnityEngine.Vector2.Distance(casterPos, targetPos);
                /// Kiểm tra khoảng cách có thỏa mãn không
                if (distanceToTarget - attackRange >= 5f)
                {
                    return UseSkillResult.Target_Not_In_Range;
                }
            }

            /// Kiểm tra các trạng thái ngũ hành
            if (!caster.IsCanDoLogic())
            {
                return UseSkillResult.Can_Not_Use_Skill_While_Being_Locked;
            }
            /// Nếu có trạng thái cấm chiêu
            else if (caster.HaveState(KE_STATE.emSTATE_SILENCE))
            {
                return UseSkillResult.Can_Not_Use_Skill_While_Being_Locked;
            }

            /// Kiểm tra nếu kỹ năng yêu cầu tác dụng lên mục tiêu đã tử nạn
            if (skill.Data.TargetType == "revivable")
            {
                if (target == null || !target.IsDead() || !(target is KPlayer))
                {
                    return UseSkillResult.No_Target_Found;
                }
            }

            /// Nếu thỏa mãn các điều kiện thì trả ra kết quả cho dùng kỹ năng
            return UseSkillResult.Success;
        }


        /// <summary>
        /// Thực hiện trừ các giá trị tương ứng
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        private static UseSkillResult UseSkill_DoSkillCost(GameObject caster, GameObject target, SkillLevelRef skill)
        {
            /// ProDict của kỹ năng
            PropertyDictionary skillPd = skill.Properties;

            /// ProDict được hỗ trợ
            PropertyDictionary enchantPd = null;
            if (caster is KPlayer)
            {
                enchantPd = (caster as KPlayer).Skills.GetEnchantProperties(skill.SkillID);
            }

            /// Nếu là người chơi
            if (caster is KPlayer)
            {
                KPlayer player = caster as KPlayer;

                /// Trừ giá trị nội lực
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_cost_v))
                {
                    /// Số điểm mất
                    int pointCost = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_cost_v).nValue[0];

                    /// Nếu là loại mất sinh lực
                    if (skill.Data.SkillCostType == 0)
                    {
                        caster.m_CurrentLife -= pointCost;
                    }
                    /// Nếu là loại mất nội lực
                    else if (skill.Data.SkillCostType == 1)
                    {
                        caster.m_CurrentMana -= pointCost;
                    }
                    /// Nếu là loại mất thể lực
                    else if (skill.Data.SkillCostType == 2)
                    {
                        caster.m_CurrentStamina -= pointCost;
                    }
                }

                /// Thông báo thời gian phục hồi (nếu có)
                int cooldown = 0;
                if (player.IsRiding)
                {
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v))
                    {
                        cooldown += (int) ((skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v).nValue[0] / 18f) * 1000);
                    }
                    if (enchantPd != null && enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v))
                    {
                        cooldown += (int) ((enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercastonhorse_v).nValue[0] / 18f) * 1000);
                    }
                }
                else
                {
                    if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v))
                    {
                        cooldown += (int) ((skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v).nValue[0] / 18f) * 1000);
                    }
                    if (enchantPd != null && enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v))
                    {
                        cooldown += (int) ((enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v).nValue[0] / 18f) * 1000);
                    }
                }

                /// Nếu có thời gian giãn cách thi triển
                if (cooldown > 0 && (!player.m_sIgnoreSkillCooldowns || player.m_sIgnoreSkillCooldownsBuffID == skill.SkillID))
                {
                    player.Skills.AddSkillCooldown(skill.SkillID, cooldown);
                }

                /// Nếu là kỹ năng ảnh hưởng bởi tốc đánh
                if (!skill.Data.IsSkillNoAddAttackSpeedCooldown)
                {
                    /// Cập nhật thời gian dùng kỹ năng
                    player.LastAttackTicks = KTGlobal.GetCurrentTimeMilis();
                }
                else
                {
                    /// Cập nhật thời gian dùng kỹ năng không ảnh hưởng tốc đánh
                    player.LastUseSkillNoAffectAtkSpeedTick = KTGlobal.GetCurrentTimeMilis();
                }
            }
            /// Nếu là Pet
            else if (caster is Pet pet)
            {
                /// Thông báo thời gian phục hồi (nếu có)
                int cooldown = 0;
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v))
                {
                    cooldown += (int) ((skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v).nValue[0] / 18f) * 1000);
                }
                if (enchantPd != null && enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v))
                {
                    cooldown += (int) ((enchantPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_mintimepercast_v).nValue[0] / 18f) * 1000);
                }

                /// Nếu có thời gian giãn cách thi triển
                if (cooldown > 0)
                {
                    pet.SkillTree.AddSkillCooldown(skill.SkillID, cooldown);
                }

                /// Nếu là kỹ năng ảnh hưởng bởi tốc đánh
                if (!skill.Data.IsSkillNoAddAttackSpeedCooldown)
                {
                    /// Cập nhật thời gian dùng kỹ năng
                    pet.LastAttackTicks = KTGlobal.GetCurrentTimeMilis();
                }
                else
                {
                    /// Cập nhật thời gian dùng kỹ năng không ảnh hưởng tốc đánh
                    pet.LastUseSkillNoAffectAtkSpeedTick = KTGlobal.GetCurrentTimeMilis();
                }
            }
            else
            {
                /// Nếu là kỹ năng ảnh hưởng bởi tốc đánh
                if (!skill.Data.IsSkillNoAddAttackSpeedCooldown)
                {
                    /// Cập nhật thời gian dùng kỹ năng
                    caster.LastAttackTicks = KTGlobal.GetCurrentTimeMilis();
                }
                else
                {
                    /// Cập nhật thời gian dùng kỹ năng không ảnh hưởng tốc đánh
                    caster.LastUseSkillNoAffectAtkSpeedTick = KTGlobal.GetCurrentTimeMilis();
                }
            }

            /// Trả ra kết quả thành công
            return UseSkillResult.Success;
        }
        #endregion

        #region Core
        /// <summary>
        /// Thực hiện sử dụng kỹ năng
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="skill"></param>
        /// <param name="isChildSkill"></param>
        /// <returns></returns>
        private static UseSkillResult UseSkill_DoAction(GameObject caster, UnityEngine.Vector2? castPosProperty, UnityEngine.Vector2? targetPosProperty, GameObject target, UnityEngine.Vector2? dirVectorProperty, SkillLevelRef skill, bool isChildSkill, int[] initParams = null)
        {
            /// ProDict của kỹ năng
            PropertyDictionary skillPd = skill.Properties;

            /// Vị trí xuất chiêu
            UnityEngine.Vector2 castPos;

            /// Nếu có thiết lập vị trí xuất chiêu trước đó
            if (castPosProperty.HasValue)
            {
                castPos = castPosProperty.Value;
            }
            /// Nếu không có thiết lập vị trí xuất chiêu trước đó thì lấy chính vị trí hiện tại của đối tượng xuất chiêu
            else
            {
                castPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);
            }

            /// Vị trí mục tiêu
            UnityEngine.Vector2? targetPos = null;

            /// Nếu không phải kỹ năng con và kỹ năng bị động và vòng sáng
            if (!isChildSkill && skill.Data.Type != 3 && !skill.Data.IsArua)
            {
                /// Nếu đang trong trạng thái di chuyển thì dừng luôn
                if (caster is KPlayer)
                {
                    KTPlayerStoryBoardEx.Instance.Remove(caster as KPlayer);
                }
                else if (caster is Monster)
                {
                    KTMonsterStoryBoardEx.Instance.Remove(caster as Monster);
                }
                else if (caster is KDecoBot)
                {
                    KTBotStoryBoardEx.Instance.Remove(caster as KDecoBot);
                }

                /// Nếu có mục tiêu
                if (target != null)
                {
                    /// Nếu mục tiêu khác bản thân và là kẻ địch
                    if (target != caster && KTGlobal.IsOpposite(caster, target))
                    {
                        /// Đánh dấu mục tiêu trước đó
                        caster.LastTarget = target;
                    }
                    targetPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
                }
                /// Nếu không có mục tiêu nhưng có vị trí truyền vào
                else if (targetPosProperty.HasValue)
                {
                    targetPos = targetPosProperty.Value;
                }

                /// Nếu vị trí hiện tại khác với vị trí xuất chiêu thì tính lại hướng quay của đối tượng xuất chiêu
                if (targetPos.HasValue && targetPos.Value != castPos)
                {
                    UnityEngine.Vector2 dirVector = targetPos.Value - castPos;
                    float angle = KTMath.GetAngle360WithXAxis(dirVector);
                    caster.CurrentDir = KTMath.GetDirectionByAngle360(angle);

                    /// Thiết lập hướng xuất chiêu
                    dirVectorProperty = dirVector;
                }
            }
            /// Nếu là kỹ năng con
            else if (isChildSkill)
            {
                /// Nếu có thiết lập hướng xuất chiêu ban đầu
                if (dirVectorProperty.HasValue)
                {
                    UnityEngine.Vector2 dirVector = dirVectorProperty.Value;
                    float angle = KTMath.GetAngle360WithXAxis(dirVector);
                    caster.CurrentDir = KTMath.GetDirectionByAngle360(angle);
                }
            }

            float delay = skill.Data.WaitTime / 18f;

            /// Nếu là chiêu Ngự Tuyết Ẩn thức phụ thì delay 2s mới kích hoạt
            if (isChildSkill && skill.Data.ID == 122)
            {
                delay += 2f;
            }

            if (delay <= 0.1f)
            {
#if USE_SEMAPHORE_ON_SKILL
                KTSkillManager.ExecuteActionWithMultipleThreading(() => {
                    KTSkillManager.DoAction(caster, castPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill, initParams);
                }, null);
#else
                KTSkillManager.DoAction(caster, castPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill, initParams);
#endif
            }
            else
            {
                KTSkillManager.SetTimeout(delay, () => {
                    KTSkillManager.DoAction(caster, castPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill, initParams);
                });
            }

            /// Gửi tín hiệu về những người chơi xung quanh thông báo đối tượng này đang sử dụng kỹ năng
            if (!isChildSkill && skill.Data.Type != 3 && !skill.Data.IsArua)
            {
                bool isSpecialAttack = false;
                if (KTGlobal.GetRandomNumber(1, 100) <= 50)
                {
                    isSpecialAttack = true;
                }
                KT_TCPHandler.SendObjectUseSkill(caster, skill.Data, isSpecialAttack);
            }

            return UseSkillResult.Success;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sử dụng kỹ năng
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="skill"></param>
        /// <param name="level"></param>
        /// <param name="isChildSkill"></param>
        /// <param name="castPosProperty"></param>
        public static UseSkillResult UseSkill(GameObject caster, GameObject target, UnityEngine.Vector2? targetPosProperty, SkillLevelRef skill, bool isChildSkill = false, UnityEngine.Vector2? castPosProperty = null)
        {
            /// Nếu kỹ năng không yêu cầu mục tiêu thì thiết lập mục tiêu NULL
            if (skill.Data.IsSkillNoTarget)
            {
                target = null;
            }

            /// Nếu không phải kỹ năng con
            if (!isChildSkill)
            {
                /// Kiểm tra điều kiện dùng kỹ năng
                UseSkillResult conditionCheck = KTSkillManager.UseSkill_CheckCondition(caster, target, skill);
                if (conditionCheck != UseSkillResult.Success)
                {
                    return conditionCheck;
                }

                /// Nếu là người chơi
                if (caster is KPlayer)
                {
                    /// Người chơi tương ứng
                    KPlayer player = caster as KPlayer;
                    /// Nếu đang thao tác gì đó thì dừng
                    player.CurrentProgress = null;
                }

                /// Thực hiện trừ các giá trị tương ứng khi dùng kỹ năng
                UseSkillResult skillCost = KTSkillManager.UseSkill_DoSkillCost(caster, target, skill);
                if (skillCost != UseSkillResult.Success)
                {
                    return skillCost;
                }

                /// Thiết lập trạng thái đứng cho đối tượng
                caster.m_eDoing = KE_NPC_DOING.do_stand;

                /// Nếu đối tượng ra chiêu là người chơi
                if (caster is KPlayer)
                {
                    /// Người chơi tương ứng
                    KPlayer player = caster as KPlayer;
                    /// Prodict kỹ năng
                    PropertyDictionary skillPd = skill.Properties.Clone();
                    /// Cộng các kỹ năng hỗ trợ
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(skill.SkillID);
                    /// Nếu tồn tại kỹ năng hỗ trợ
                    if (enchantPd != null)
                    {
                        skillPd.AddProperties(enchantPd);
                    }

                    /// Nếu đang trong trạng thái tàng hình, đồng thời kỹ năng đang sử dụng không nằm trong danh sách không làm mất tàng hình khi sử dụng
                    if (caster.IsInvisible() && caster.m_InvisibleType >= 0 && !caster.m_InvisibleNoLostOnUseSkills.Contains(skill.SkillID) && !skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_keephide))
                    {
                        /// Xóa trạng thái tàng hình
                        caster.RemoveInvisibleState();

                        /// Nếu có Buff ngự tuyết ẩn thì xóa luôn
                        caster.Buffs.RemoveBuff(10085);

                        /// Duyệt danh sách Buff xem có Buff nào loại mất khi không ở trong trạng thái tàng hình không
                        BuffDataEx buff = caster.Buffs.ListBuffs.Where(x => x.Skill.Data.SkillStyle == "bufflostwithoutinvisiblity").FirstOrDefault();
                        if (buff != null)
                        {
                            /// Tạo bản sao ra tránh BUG
                            buff = buff.Clone();
                            /// Đếm lùi 1s sau tự xóa
                            KTSkillManager.SetTimeout(1f, () => {
                                /// Nếu lại có Buff Ngự tuyết ẩn
                                if (caster.Buffs.HasBuff(10085))
                                {
                                    return;
                                }
                                caster.Buffs.RemoveBuff(buff);
                            });
                        }
                    }

                    /// Nếu không ở trong trạng thái tàng hình
                    if (!caster.IsInvisible())
                    {
                        caster.m_LastUseSkillCauseLosingInvisibleTick = KTGlobal.GetCurrentTimeMilis();
                    }

                    /// Đánh dấu kỹ năng dùng lần trước
                    player.LastUseSkillID = skill.SkillID;

                    /// Nếu có thiết lập vị trí ra chiêu
                    if (targetPosProperty.HasValue)
                    {
                        /// Vị trí ra chiêu
                        UnityEngine.Vector2 castPos = new UnityEngine.Vector2(player.PosX, player.PosY);
                        /// Nếu là bẫy
                        if (skill.Data.SkillStyle == "trap")
                        {
                            /// Vị trí đích luôn là chân đối tượng ra chiêu
                            targetPosProperty = castPos;
                        }
                        /// Nếu không phải bẫy
                        else
                        {
                            /// Khoảng cách đến vị trí ra chiêu
                            float distanceToTargetPos = UnityEngine.Vector2.Distance(targetPosProperty.Value, castPos);
                            /// Nếu quá phạm vi đánh của kỹ năng
                            if (distanceToTargetPos > skill.Data.AttackRadius)
                            {
                                /// Vector hướng
                                UnityEngine.Vector2 dirVector = targetPosProperty.Value - castPos;
                                /// Cập nhật lại vị trí đích mới nằm trong phạm vi ra chiêu
                                targetPosProperty = KTMath.FindPointInVectorWithDistance(castPos, dirVector, skill.Data.AttackRadius);
                            }
                        }
                    }
                }
                else
                {
                    /// Nếu đang trong trạng thái tàng hình, đồng thời kỹ năng đang sử dụng không nằm trong danh sách không làm mất tàng hình khi sử dụng
                    if (caster.IsInvisible() && (caster.m_InvisibleType >= 0 && !caster.m_InvisibleNoLostOnUseSkills.Contains(skill.SkillID)))
                    {
                        caster.RemoveInvisibleState();

                        /// Duyệt danh sách Buff xem có Buff nào loại mất khi không ở trong trạng thái tàng hình không
                        BuffDataEx buff = caster.Buffs.ListBuffs.Where(x => x.Skill.Data.SkillStyle == "bufflostwithoutinvisiblity").FirstOrDefault();
                        if (buff != null)
                        {
                            /// Tạo bản sao ra tránh BUG
                            buff = buff.Clone();
                            /// Đếm lùi 0.2s sau tự xóa
                            KTSkillManager.SetTimeout(0.2f, () => {
                                caster.Buffs.RemoveBuff(buff);
                            });
                        }
                    }

                    /// Nếu không ở trong trạng thái tàng hình
                    if (!caster.IsInvisible())
                    {
                        caster.m_LastUseSkillCauseLosingInvisibleTick = KTGlobal.GetCurrentTimeMilis();
                    }
                }
            }

            /// Thực hiện sử dụng kỹ năng
            return KTSkillManager.UseSkill_DoAction(caster, castPosProperty, targetPosProperty, target, null, skill, isChildSkill);
        }

        /// <summary>
        /// Thực thi kỹ năng bị động
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="skill"></param>
        public static UseSkillResult ProcessPassiveSkill(GameObject caster, SkillLevelRef skill)
        {
            /// Nếu là người chơi
            if (caster is KPlayer player)
            {
                /// Nếu kỹ năng không hợp lệ
                if (!player.Skills.IsValidSkill(skill))
                {
                    return UseSkillResult.None;
                }
            }
            
            /// Sử dụng kỹ năng
            return KTSkillManager.UseSkill_DoAction(caster, null, null, null, null, skill, false);
        }
        #endregion
    }
}
