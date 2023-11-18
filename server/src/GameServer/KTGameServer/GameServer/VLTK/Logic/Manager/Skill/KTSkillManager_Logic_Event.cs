using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý sự kiện
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Hàm thực thi sự kiện đạn tan biến
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="caster"></param>
        /// <param name="explodePosProperty"></param>
        /// <param name="target"></param>
        /// <param name="dirVectorProperty"></param>
        public static void BulletVanished(SkillLevelRef skill, GameObject caster, UnityEngine.Vector2? explodePosProperty, GameObject target, UnityEngine.Vector2? dirVectorProperty)
        {
            /// Check NULL
            if (skill == null)
            {
                return;
            }
            else if (skill.Properties == null)
            {
                return;
            }
            else if (caster == null)
            {
                return;
            }

            /// Vị trí nổ
            UnityEngine.Vector2 explodePos;

            /// Nếu có thiết lập vị trí nổ
            if (explodePosProperty != null && explodePosProperty.HasValue)
            {
                explodePos = explodePosProperty.Value;
            }
            /// Nếu không có thiết lập vị trí nổ thì lấy vị trí hiện tại của mục tiêu
            else if (target != null)
            {
                explodePos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
            }
            /// Nếu không tìm được vị trí nổ thì không làm gì
            else
            {
                return;
            }

            /// Đối tượng người chơi
            KPlayer player = caster as KPlayer;
            /// ProDict kỹ năng hỗ trợ kỹ năng này
            PropertyDictionary enchantPd = player == null ? null : player.Skills.GetEnchantProperties(skill.SkillID);
            /// ProDict kỹ năng
            PropertyDictionary skillPd = skill.Properties.Clone();

            /// Nếu tồn tại kỹ năng hỗ trợ
            if (enchantPd != null)
            {
                skillPd.AddProperties(enchantPd);
            }

            /// Kiểm tra có kỹ năng tan biến thì gọi
            if (skill.Data.VanishSkillID > 0)
            {
                SkillDataEx subSkill = KSkill.GetSkillData(skill.Data.VanishSkillID);
                if (subSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = subSkill,
                        AddedLevel = skill.AddedLevel,
                        BonusLevel = skill.BonusLevel,
                        CanStudy = false,
                    };
                    KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                }
            }
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addvanishskill);
                int subSkillID = magicAttrib.nValue[1];
                int subSkillLevel = magicAttrib.nValue[2] == -1 ? skill.Level : magicAttrib.nValue[2];
                int percent = 100;

                /// Nếu đủ tỷ lệ ra chiêu
                if (KTGlobal.GetRandomNumber(0, 100) <= percent)
                {
                    SkillDataEx subSkill = KSkill.GetSkillData(subSkillID);
                    if (subSkill != null)
                    {
                        SkillLevelRef skillRef = new SkillLevelRef()
                        {
                            Data = subSkill,
                            AddedLevel = subSkillLevel,
                            BonusLevel = 0,
                            CanStudy = false,
                        };
                        KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                    }
                }
            }
            else if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_vanishedevent))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_vanishedevent);
                int subSkillID = magicAttrib.nValue[0];
                int subSkillLevel = magicAttrib.nValue[2] == -1 ? skill.Level : magicAttrib.nValue[2];
                int percent = magicAttrib.nValue[1];

                /// Nếu đủ tỷ lệ ra chiêu
                if (KTGlobal.GetRandomNumber(0, 100) <= percent)
                {
                    SkillDataEx subSkill = KSkill.GetSkillData(subSkillID);
                    if (subSkill != null)
                    {
                        SkillLevelRef skillRef = new SkillLevelRef()
                        {
                            Data = subSkill,
                            AddedLevel = subSkillLevel,
                            BonusLevel = 0,
                            CanStudy = false,
                        };
                        KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                    }
                }
            }
        }

        /// <summary>
        /// Hàm thực thi sự kiện đạn chạm mục tiêu lần đầu tiên
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="caster"></param>
        /// <param name="explodePosProperty"></param>
        /// <param name="target"></param>
        /// <param name="dirVectorProperty"></param>
        public static void BulletFirstTouchTarget(SkillLevelRef skill, GameObject caster, UnityEngine.Vector2? explodePosProperty, GameObject target, UnityEngine.Vector2? dirVectorProperty)
        {
            /// Check NULL
            if (skill == null)
            {
                return;
            }
            else if (skill.Properties == null)
            {
                return;
            }
            else if (caster == null)
            {
                return;
            }

            /// Vị trí nổ
            UnityEngine.Vector2 explodePos;

            /// Nếu có thiết lập vị trí nổ
            if (explodePosProperty != null && explodePosProperty.HasValue)
            {
                explodePos = explodePosProperty.Value;
            }
            /// Nếu không có thiết lập vị trí nổ thì lấy vị trí hiện tại của mục tiêu
            else if (target != null)
            {
                explodePos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
            }
            /// Nếu không tìm được vị trí nổ thì không làm gì
            else
            {
                return;
            }

            /// Đối tượng người chơi
            KPlayer player = caster as KPlayer;
            /// ProDict kỹ năng hỗ trợ kỹ năng này
            PropertyDictionary enchantPd = player == null ? null : player.Skills.GetEnchantProperties(skill.SkillID);
            /// ProDict kỹ năng
            PropertyDictionary skillPd = skill.Properties.Clone();

            /// Nếu tồn tại kỹ năng hỗ trợ
            if (enchantPd != null)
            {
                skillPd.AddProperties(enchantPd);
            }

            ///// Có kỹ năng tạo đạn bay khi nổ thì kích hoạt
            //if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill))
            //{
            //    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addflyskill);
            //    int subSkillID = magicAttrib.nValue[1];
            //    int subSkillLevel = magicAttrib.nValue[2] == -1 ? skill.Level : magicAttrib.nValue[2];
            //    int percent = 100;

            //    /// Nếu đủ tỷ lệ ra chiêu
            //    if (KTGlobal.GetRandomNumber(0, 100) <= percent)
            //    {
            //        SkillDataEx subSkill = KSkill.GetSkillData(subSkillID);
            //        if (subSkill != null)
            //        {
            //            SkillLevelRef skillRef = new SkillLevelRef()
            //            {
            //                Data = subSkill,
            //                AddedLevel = subSkillLevel,
            //                BonusLevel = 0,
            //                CanStudy = false,
            //            };
            //            KTSkillManager.UseSkill_DoAction(caster, explodePos, target, dirVectorProperty, skillRef, true);
            //        }
            //    }
            //}
            //else if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_flyevent))
            //{
            //    KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_flyevent);
            //    int subSkillID = magicAttrib.nValue[0];
            //    int subSkillLevel = magicAttrib.nValue[2] == -1 ? skill.Level : magicAttrib.nValue[2];
            //    int percent = magicAttrib.nValue[1];

            //    /// Nếu đủ tỷ lệ ra chiêu
            //    if (KTGlobal.GetRandomNumber(0, 100) <= percent)
            //    {
            //        SkillDataEx subSkill = KSkill.GetSkillData(subSkillID);
            //        if (subSkill != null)
            //        {
            //            SkillLevelRef skillRef = new SkillLevelRef()
            //            {
            //                Data = subSkill,
            //                AddedLevel = subSkillLevel,
            //                BonusLevel = 0,
            //                CanStudy = false,
            //            };
            //            KTSkillManager.UseSkill_DoAction(caster, explodePos, target, dirVectorProperty, skillRef, true);
            //        }
            //    }
            //}
            if (skill.Data.BulletSkillID > 0)
            {
                SkillDataEx subSkill = KSkill.GetSkillData(skill.Data.BulletSkillID);
                if (subSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = subSkill,
                        AddedLevel = skill.AddedLevel,
                        BonusLevel = skill.BonusLevel,
                        CanStudy = false,
                    };
                    KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                }
            }
        }

        /// <summary>
        /// Hàm thực thi sự kiện đạn nổ
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="explodePosProperty">Vị trí nổ</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="pieceTargetNumber">Thứ tự lần xuyên suốt mục tiêu</param>
        /// <returns></returns>
        public static void BulletExplode(SkillLevelRef skill, GameObject caster, UnityEngine.Vector2? explodePosProperty, GameObject target, UnityEngine.Vector2? dirVectorProperty, int pieceTargetNumber = 0, int targetNumber = 0)
        {
            /// Check NULL
            if (skill == null)
            {
                return;
            }
            else if (skill.Properties == null)
            {
                return;
            }
            else if (caster == null)
            {
                return;
            }

            /// Nếu mục tiêu NULL hoặc đã chết thì không làm gì cả
            if (target == null && target.IsDead())
            {
                return;
            }

            /// Vị trí nổ
            UnityEngine.Vector2 explodePos;

            /// Nếu có thiết lập vị trí nổ
            if (explodePosProperty != null && explodePosProperty.HasValue)
            {
                explodePos = explodePosProperty.Value;
            }
            /// Nếu không có thiết lập vị trí nổ thì lấy vị trí hiện tại của mục tiêu
            else
            {
                explodePos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
            }

            /// Đối tượng người chơi
            KPlayer player = caster as KPlayer;
            /// ProDict kỹ năng hỗ trợ kỹ năng này
            PropertyDictionary enchantPd = player == null ? null : player.Skills.GetEnchantProperties(skill.SkillID);
            /// ProDict kỹ năng
            PropertyDictionary skillPd = skill.Properties.Clone();

            /// Nếu tồn tại kỹ năng hỗ trợ
            if (enchantPd != null)
            {
                skillPd.AddProperties(enchantPd);
            }

            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_collideevent))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_collideevent);
                int subSkillID = magicAttrib.nValue[0];
                int subSkillLevel = magicAttrib.nValue[2] == -1 ? skill.Level : magicAttrib.nValue[2];
                int percent = magicAttrib.nValue[1];

                /// Nếu đủ tỷ lệ ra chiêu
                if (KTGlobal.GetRandomNumber(0, 100) <= percent)
                {
                    SkillDataEx subSkill = KSkill.GetSkillData(subSkillID);
                    if (subSkill != null)
                    {
                        SkillLevelRef skillRef = new SkillLevelRef()
                        {
                            Data = subSkill,
                            AddedLevel = subSkillLevel,
                            BonusLevel = 0,
                            CanStudy = false,
                        };
                        KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                    }
                }
            }
            if (skill.Data.CollideSkillID > 0)
            {
                SkillDataEx subSkill = KSkill.GetSkillData(skill.Data.CollideSkillID);
                if (subSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = subSkill,
                        AddedLevel = skill.AddedLevel,
                        BonusLevel = skill.BonusLevel,
                        CanStudy = false,
                    };
                    KTSkillManager.UseSkill_DoAction(caster, explodePos, null, target, dirVectorProperty, skillRef, true);
                }
            }

            /// % sát thương cộng thêm mỗi lần chạm mục tiêu
            int touchTargetsAddDamagesPercent = 0;
            /// Nếu có thiết lập % sát thương cộng thêm mỗi lần xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_runattack_damageadded))
            {
                touchTargetsAddDamagesPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_runattack_damageadded).nValue[0];
            }
            /// Nếu có kỹ năng hỗ trợ tăng % sát thương cộng thêm mỗi lần xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_runattack_damageadded))
            {
                touchTargetsAddDamagesPercent += skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_runattack_damageadded).nValue[0];
            }

            /// % sát thương cộng thêm mỗi lần xuyên suốt mục tiêu
            int pieceThroughTargetsAddDamagesPercent = 0;
            int pieceThroughTargetsMaxDamagesPercent = 0;
            /// Nếu có thiết lập % sát thương cộng thêm mỗi lần xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol))
            {
                pieceThroughTargetsAddDamagesPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol).nValue[1];
                pieceThroughTargetsMaxDamagesPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol).nValue[2];
            }
            /// Nếu có kỹ năng hỗ trợ tăng % sát thương cộng thêm mỗi lần xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol))
            {
                pieceThroughTargetsAddDamagesPercent += skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol).nValue[1];
                pieceThroughTargetsMaxDamagesPercent += skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addpowerwhencol).nValue[2];
            }

            /// Tổng số % sát thương nhận thêm
            int totalAddDamagePercent = touchTargetsAddDamagesPercent * targetNumber + Math.Min(pieceThroughTargetsMaxDamagesPercent * pieceTargetNumber, pieceThroughTargetsAddDamagesPercent);
            //Console.WriteLine("TargetNumber = {0}, OriginSkillAddDamagePercentPieceTarget = {1}, OriginSkillMaxAddDamagePercentPieceTarget = {2}, TotalAddDamagePercent = {3}%", targetNumber, pieceThroughTargetsAddDamagesPercent, pieceThroughTargetsMaxDamagesPercent, totalAddDamagePercent);
            /// Tính sát thương gây ra
            bool attack = AlgorithmProperty.AttackEnemy(caster, target, skill, totalAddDamagePercent, explodePos, false);
            if (!attack)
            {
                ///// Gửi gói tin sát thương về Client
                //KTSkillManager.AppendSkillResult(caster, target, SkillResult.Adjust, 0);

                /// Thông báo đối tượng vừa tấn công
                target.TakeDamage(caster, 0);
            }
        }
    }
}
