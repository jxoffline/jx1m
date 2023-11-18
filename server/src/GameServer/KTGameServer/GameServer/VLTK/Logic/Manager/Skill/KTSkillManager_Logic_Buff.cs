using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic Buff
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Thực hiện Buff đơn mục tiêu
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="duration">Thời gian duy trì Buff</param>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        private static void DoSkillSingleTargetBuff(GameObject caster, GameObject target, float duration, SkillLevelRef skill, PropertyDictionary skillPd)
        {
            /// Tạo mới Buff và tăng thêm 1 giây xét độ delay
            BuffDataEx buffData = new BuffDataEx()
            {
                Skill = skill,
                LoseWhenUsingSkill = false,
                SaveToDB = false,
                Duration = duration == -1 ? -1 : (int) (duration * 1000 + 1000),
                StartTick = KTGlobal.GetCurrentTimeMilis(),
                CustomProperties = skillPd.Clone(),
            };

            /// Thiết lập lại giá trị của SkillPd
            skillPd = buffData.CustomProperties;

            /// Nếu là khiên nội lực thì thời gian duy trì được lấy ở ngay thuộc tính khiên
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_staticmagicshieldcur_p))
            {
                buffData.Duration = (int) (skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_staticmagicshieldcur_p).nValue[1] / 18f * 1000);
            }

            bool hasStealStateSymbol = false;
            int stealStateCount = 0;
            int stealStatePercent = 0;
            int stealStateMaxLevel = 0;
            /// Nếu có Symbol ngẫu nhiên đánh cắp 1 trạng thái của kẻ địch xung quanh
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_trice_eff_stealstate))
            {
                hasStealStateSymbol = true;
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_trice_eff_stealstate);
                stealStateCount = magicAttrib.nValue[0];
                stealStatePercent = magicAttrib.nValue[1];
                stealStateMaxLevel = magicAttrib.nValue[2];
            }

            /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
            {
                KMagicAttrib pMagicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                skillPd.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
            }

            /// Nếu Buff lên bản thân
            if (skill.Data.TargetType == "self")
            {
                if (caster is KPlayer && hasStealStateSymbol)
                {
                    KPlayer player = caster as KPlayer;
                    int totalStolenStatesChanceLeft = stealStateCount;
                    caster.Buffs.AddBuff(buffData, 2f, () => {
                        player.StealRandomPositiveBuffOfNearbyEnemy(stealStatePercent, stealStateMaxLevel, ref totalStolenStatesChanceLeft);
                        if (totalStolenStatesChanceLeft <= 0)
                        {
                            caster.Buffs.RemoveBuff(buffData);
                        }
                    });
                }
                else
                {
                    caster.Buffs.AddBuff(buffData);
                }
            }
            /// Nếu Buff lên bản thân không phải trong trạng thái tàng hình
            else if (skill.Data.TargetType == "selfnothide")
            {
                if (!caster.IsInvisible())
                {
                    if (caster is KPlayer && hasStealStateSymbol)
                    {
                        KPlayer player = caster as KPlayer;
                        int totalStolenStatesChanceLeft = stealStateCount;
                        caster.Buffs.AddBuff(buffData, 2f, () => {
                            player.StealRandomPositiveBuffOfNearbyEnemy(stealStatePercent, stealStateMaxLevel, ref totalStolenStatesChanceLeft);
                            if (totalStolenStatesChanceLeft <= 0)
                            {
                                caster.Buffs.RemoveBuff(buffData);
                            }
                        });
                    }
                    else
                    {
                        caster.Buffs.AddBuff(buffData);
                    }
                }
            }
            /// Nếu Buff cho chủ nhân
            else if (skill.Data.TargetType == "owner")
            {
                /// Nếu bản thân là Pet
                if (caster is Pet pet)
                {
                    /// Nếu chủ nhân còn sống
                    if (pet.Owner != null && !pet.Owner.IsDead())
                    {
                        /// Buff cho chủ nhân
                        pet.Owner.Buffs.AddBuff(buffData);
                    }
                }
            }
            /// Nếu Buff cho pet
            else if (skill.Data.TargetType == "pet")
            {
                /// Nếu bản thân là người chơi
                if (caster is KPlayer player)
                {
                    /// Nếu có pet
                    if (player.CurrentPet != null && !player.CurrentPet.IsDead())
                    {
                        /// Buff cho pet
                        player.CurrentPet.Buffs.AddBuff(buffData);
                    }
                }
            }
            /// Nếu Buff lên đồng đội
            else if (skill.Data.TargetType == "ally")
            {
                /// Nếu không có mục tiêu tức là Buff cho bản thân
                if (target == null || (target != null && KTGlobal.IsOpposite(caster, target)))
                {
                    caster.Buffs.AddBuff(buffData);
                }
                else
                {
                    target.Buffs.AddBuff(buffData);
                }
            }
            /// Nếu Buff lên toàn thể đồng đội
            else if (skill.Data.TargetType == "team")
            {
                int radius = 0;
                /// Phạm vi ảnh hưởng
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                    radius = magicAttrib.nValue[2];
                    radius *= 30;
                }
                /// Nếu không có
                else
                {
                    /// Lấy mặc định cự ly ra chiêu
                    radius = skill.Data.AttackRadius;
                }

                if (caster is KPlayer)
                {
                    KPlayer player = caster as KPlayer;

                    /// Nếu nhóm không tồn tại
                    if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
                    {
                        /// Buff cho bản thân
                        player.Buffs.AddBuff(buffData);

                        /// Nếu có pet nằm trong phạm vi kỹ năng
                        if (player.CurrentPet != null && !player.CurrentPet.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, player.CurrentPet) <= radius)
                        {
                            player.CurrentPet.Buffs.AddBuff(buffData);
                        }
                        /// Nếu có xe tiêu nằm trong phạm vi kỹ năng
                        if (player.CurrentTraderCarriage != null && !player.CurrentTraderCarriage.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, player.CurrentTraderCarriage) <= radius)
                        {
                            player.CurrentTraderCarriage.Buffs.AddBuff(buffData);
                        }
                    }
                    else
                    {
                        List<KPlayer> teammates = KTGlobal.GetNearByTeammates(caster as KPlayer, radius);
                        foreach (KPlayer teammate in teammates)
                        {
                            /// Buff cho đồng đội
                            teammate.Buffs.AddBuff(buffData.Clone());

                            /// Nếu có pet nằm trong phạm vi kỹ năng
                            if (teammate.CurrentPet != null && !teammate.CurrentPet.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, teammate.CurrentPet) <= radius)
                            {
                                teammate.CurrentPet.Buffs.AddBuff(buffData);
                            }
                            /// Nếu có xe tiêu nằm trong phạm vi kỹ năng
                            if (teammate.CurrentTraderCarriage != null && !teammate.CurrentTraderCarriage.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, teammate.CurrentTraderCarriage) <= radius)
                            {
                                teammate.CurrentTraderCarriage.Buffs.AddBuff(buffData);
                            }
                        }
                    }
                }
            }
            /// Nếu Buff lên toàn thể đồng đội mà không bao gồm bản thân
            else if (skill.Data.TargetType == "teamnoself")
            {
                int radius = 0;
                /// Phạm vi ảnh hưởng
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                    radius = magicAttrib.nValue[2];
                    radius *= 30;
                }
                /// Nếu không có
                else
                {
                    /// Lấy mặc định cự ly ra chiêu
                    radius = skill.Data.AttackRadius;
                }

                if (caster is KPlayer)
                {
                    KPlayer player = caster as KPlayer;

                    /// Nếu nhóm tồn tại
                    if (player.TeamID != -1 || KTTeamManager.IsTeamExist(player.TeamID))
                    {
                        List<KPlayer> teammates = KTGlobal.GetNearByTeammates(caster as KPlayer, radius);
                        foreach (KPlayer teammate in teammates)
                        {
                            /// Nếu đồng đội khác bản thân
                            if (teammate != player)
                            {
                                /// Buff cho đồng đội
                                teammate.Buffs.AddBuff(buffData.Clone());

                                /// Nếu có pet nằm trong phạm vi kỹ năng
                                if (teammate.CurrentPet != null && !teammate.CurrentPet.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, teammate.CurrentPet) <= radius)
                                {
                                    teammate.CurrentPet.Buffs.AddBuff(buffData);
                                }
                                /// Nếu có xe tiêu nằm trong phạm vi kỹ năng
                                if (teammate.CurrentTraderCarriage != null && !teammate.CurrentTraderCarriage.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, teammate.CurrentTraderCarriage) <= radius)
                                {
                                    teammate.CurrentTraderCarriage.Buffs.AddBuff(buffData);
                                }
                            }
                            /// Nếu đồng đội là bản thân
                            else
                            {
                                /// Nếu có pet nằm trong phạm vi kỹ năng
                                if (player.CurrentPet != null && !player.CurrentPet.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, player.CurrentPet) <= radius)
                                {
                                    player.CurrentPet.Buffs.AddBuff(buffData);
                                }
                                /// Nếu có xe tiêu nằm trong phạm vi kỹ năng
                                if (player.CurrentTraderCarriage != null && !player.CurrentTraderCarriage.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(player, player.CurrentTraderCarriage) <= radius)
                                {
                                    player.CurrentTraderCarriage.Buffs.AddBuff(buffData);
                                }
                            }
                        }
                    }
                }
            }
            /// Nếu Buff lên kẻ địch
            else if (skill.Data.TargetType == "enemy")
            {
                /// Nếu không tìm thấy mục tiêu
                if (target == null)
                {
                    return;
                }
                else if (!KTGlobal.IsOpposite(caster, target))
                {
                    return;
                }

                /// Tỷ lệ bỏ qua bùa chú của đối phương
                int targetIgnoreCurseP = target.m_CurrentIgnoreCursePercent;
                /// Tỷ lệ ngẫu nhiên
                int nRandIgnore = KTGlobal.GetRandomNumber(1, 100);
                /// Nếu bỏ qua bùa chú
                if (nRandIgnore <= targetIgnoreCurseP)
                {
                    /// Bỏ qua
                    return;
                }

                /// Tỷ lệ phản đòn bùa chú của đối phương
                int targetReflectCurseP = target.m_CurrentReturnSkillPercent;
                /// Tỷ lệ ngẫu nhiên
                int nPercent = KTGlobal.GetRandomNumber(1, 100);
                /// Nếu có thể phản đòn bùa chú
                if (nPercent <= targetReflectCurseP)
                {
                    /// Đánh dấu đối tượng gây trạng thái bất lợi này
                    buffData.CurseOwner = target;
                    caster.Buffs.AddBuff(buffData);
                }
                /// Nếu không thể phản đòn bùa chú
                else
                {
                    /// Đánh dấu đối tượng gây trạng thái bất lợi này
                    buffData.CurseOwner = caster;
                    target.Buffs.AddBuff(buffData);
                }

                /// Nếu là quái
                if (target is Monster)
                {
                    Monster monster = target as Monster;
                    if (monster != null)
                    {
                        /// Thông báo sự kiện chịu tấn công của đối tượng tương ứng
                        monster.TakeDamage(caster, 0);
                    }
                }
            }
            /// Nếu Buff lên nhiều kẻ địch
            else if (skill.Data.TargetType == "enemies")
            {
                int radius = 0;
                /// Phạm vi ảnh hưởng
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                    radius = magicAttrib.nValue[2];
                    radius *= 30;
                }
                /// Nếu không có
                else
                {
                    /// Lấy mặc định cự ly ra chiêu
                    radius = skill.Data.AttackRadius;
                }

                /// Danh sách kẻ địch xung quanh
                List<GameObject> enemies = KTGlobal.GetNearByEnemies(caster, radius, 10);
                /// Duyệt danh sách kẻ địch
                foreach (GameObject enemy in enemies)
                {
                    /// Tỷ lệ bỏ qua bùa chú của đối phương
                    int targetIgnoreCurseP = enemy.m_CurrentIgnoreCursePercent;
                    /// Tỷ lệ ngẫu nhiên
                    int nRandIgnore = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu bỏ qua bùa chú
                    if (nRandIgnore <= targetIgnoreCurseP)
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Tỷ lệ phản đòn bùa chú của đối phương
                    int targetReflectCurseP = enemy.m_CurrentReturnSkillPercent;
                    /// Tỷ lệ ngẫu nhiên
                    int nPercent = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu có thể phản đòn bùa chú
                    if (nPercent <= targetReflectCurseP)
                    {
                        /// Đánh dấu đối tượng gây trạng thái bất lợi này
                        buffData.CurseOwner = enemy;
                        caster.Buffs.AddBuff(buffData);
                    }
                    /// Nếu không thể phản đòn bùa chú
                    else
                    {
                        /// Đánh dấu đối tượng gây trạng thái bất lợi này
                        buffData.CurseOwner = caster;
                        enemy.Buffs.AddBuff(buffData);
                    }

                    /// Nếu là quái
                    if (enemy is Monster)
                    {
                        Monster monster = enemy as Monster;
                        if (monster != null)
                        {
                            /// Thông báo sự kiện chịu tấn công của đối tượng tương ứng
                            monster.TakeDamage(caster, 0);
                        }
                    }
                }
            }
            /// Nếu Buff lên người chơi tử nạn
            else if (skill.Data.TargetType == "revivable")
            {
                if (target != null && target is KPlayer)
                {
                    KPlayer player = target as KPlayer;
                    if (player.IsDead())
                    {
                        player.EnableReviveAtCurrentPos(buffData, caster);
                    }
                }
            }
            /// Nếu Buff vào quái và loại trừ Caster
            else if (skill.Data.TargetType == "monstersnoself")
            {
                int radius = 0;
                /// Phạm vi ảnh hưởng
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                    radius = magicAttrib.nValue[2];
                    radius *= 30;
                }
                /// Nếu không có
                else
                {
                    /// Lấy mặc định cự ly ra chiêu
                    radius = skill.Data.AttackRadius;
                }

                List<Monster> monsters = KTGlobal.GetNearByObjects<Monster>(caster, radius, 10);
                foreach (Monster monster in monsters)
                {
                    if (monster != caster)
                    {
                        monster.Buffs.AddBuff(buffData.Clone());

                        /// Thông báo đối tượng vừa tấn công
                        monster.TakeDamage(caster, 0);
                    }
                }
            }
            /// Nếu Buff vào quái bao gồm cả Caster
            else if (skill.Data.TargetType == "monsters")
            {
                int radius = 0;
                /// Phạm vi ảnh hưởng
                if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
                {
                    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range);
                    radius = magicAttrib.nValue[2];
                    radius *= 30;
                }
                /// Nếu không có
                else
                {
                    /// Lấy mặc định cự ly ra chiêu
                    radius = skill.Data.AttackRadius;
                }

                List<Monster> monsters = KTGlobal.GetNearByObjects<Monster>(caster, radius, 10);
                foreach (Monster monster in monsters)
                {
                    monster.Buffs.AddBuff(buffData.Clone());

                    /// Thông báo đối tượng vừa tấn công
                    monster.TakeDamage(caster, 0);
                }
            }
        }
    }
}
