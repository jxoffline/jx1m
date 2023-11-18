using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic các loại kỹ năng
    /// </summary>
    public static partial class KTSkillManager
    {
        #region Bullet
        /// <summary>
        /// Thực hiện Logic kỹ năng tạo đạn nhảy liên tục các mục tiêu xung quanh
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="castPos">Vị trí ra chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="targetPos">Vị trí mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng tấn công</param>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict của kỹ năng</param>
        /// <param name="isChildSkill">Có phải kỹ năng con không</param>
        /// <returns>Vị trí xuất hiện đạn</returns>
        private static UnityEngine.Vector2 DoBulletJumpTowardTargets(GameObject caster, UnityEngine.Vector2 castPos, GameObject target, UnityEngine.Vector2? targetPos, UnityEngine.Vector2? dirVectorProperty, SkillLevelRef skill, PropertyDictionary skillPd, bool isChildSkill)
        {
            /// Vị trí xuất hiện đạn
            UnityEngine.Vector2 bulletPos = castPos;

            /// Cấu hình đạn
            BulletConfig bulletConfig = KSkill.GetBulletConfig(skill.Data.BulletID);
            /// Nếu không có cấu hình đạn thì bỏ qua
            if (bulletConfig == null)
            {
                return bulletPos;
            }
            /// Nếu không có ProDict
            else if (skillPd == null)
            {
                return bulletPos;
            }

            /// Cự ly thi triển
            int attackRange = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                attackRange = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Tổng số đạn
            int bulletCount = skill.Data.BulletCount;
            /// Nếu có cấu hình số tia đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_missilenum_v))
            {
                bulletCount = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_missilenum_v).nValue[0];
            }

            /// Tổng số lượt ra đạn
            int bulletTotalRound = skill.Data.ShootCount;
            /// Nếu có số lượt ra chiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_shotnumber))
            {
                bulletTotalRound = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_shotnumber).nValue[0];
            }

            /// Tốc độ bay của đạn
            int bulletVelocity = bulletConfig.MoveSpeed;
            /// Nếu có tốc độ bay của đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_speed_v))
            {
                bulletVelocity = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_speed_v).nValue[0];
            }

            /// Số mục tiêu chạm tối đa
            int maxTargetTouch = bulletConfig.MaxTargetTouch;
            /// Nếu trong ProDict có tồn tại số mục tiêu chạm tối đa thì lấy ở ProDict ra thay thế
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_hitcount))
            {
                maxTargetTouch = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_hitcount).nValue[0];
            }

            /// Phạm vi nổ của đạn
            int bulletExplodeRadius = bulletConfig.ExplodeRadius;
            /// Nếu có phạm vi nổ (số ô)
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
            {
                bulletExplodeRadius = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range).nValue[2] * 10;
            }
            bulletExplodeRadius *= (1 + bulletConfig.ExplodeRadiusAddTimes);

            /// Xuyên suốt mục tiêu
            int pieceThroughTargetsPercent = bulletConfig.PieceThroughTargetsPercent;
            /// Nếu có thiết lập xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_ablility))
            {
                pieceThroughTargetsPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_ablility).nValue[0];
            }

            /// Xuất chiêu tại 1 điểm
            if (skill.Data.Form == 1)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu không có thiết lập vị trí sẵn
                    if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    /// Nếu không có mục tiêu thì tính toán vị trí đích dựa vào hướng quay hiện tại của nhân vật và cự ly xuất chiêu
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillStaticBulletJumpTowardTargetsAtPosition(skill, skillPd, dirVectorProperty, caster, setPos, bulletConfig, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, attackRange);
                bulletPos = setPos;
            }

            return bulletPos;
        }

        /// <summary>
        /// Thực hiện Logic kỹ năng tạo đạn (Kỹ năng với Type = 5)
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="castPos">Vị trí ra chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="targetPos">Vị trí mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng tấn công</param>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict của kỹ năng</param>
        /// <param name="ignoredTarget">Mục tiêu bỏ qua</param>
        /// <param name="isChildSkill">Có phải kỹ năng con không</param>
        /// <returns>Vị trí xuất hiện đạn</returns>
        private static UnityEngine.Vector2 DoBulletSkillLogic(GameObject caster, UnityEngine.Vector2 castPos, GameObject target, UnityEngine.Vector2? targetPos, UnityEngine.Vector2? dirVectorProperty, SkillLevelRef skill, PropertyDictionary skillPd, bool isChildSkill, int[] initParams)
        {
            /// Vị trí xuất hiện đạn
            UnityEngine.Vector2 bulletPos = castPos;

            /// Cấu hình đạn
            BulletConfig bulletConfig = KSkill.GetBulletConfig(skill.Data.BulletID);
            /// Nếu không có cấu hình đạn thì bỏ qua
            if (bulletConfig == null)
            {
                return bulletPos;
            }
            /// Nếu không có ProDict
            else if (skillPd == null)
            {
                return bulletPos;
            }

            /// Cự ly thi triển
            int attackRange = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                attackRange = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Tổng số đạn
            int bulletCount = skill.Data.BulletCount;
            /// Nếu có cấu hình số tia đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_missilenum_v))
            {
                bulletCount = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_missilenum_v).nValue[0];
            }

            /// Tổng số lượt ra đạn
            int bulletTotalRound = skill.Data.ShootCount;
            /// Nếu có số lượt ra chiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_shotnumber))
            {
                bulletTotalRound = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_shotnumber).nValue[0];
            }

            /// Tốc độ bay của đạn
            int bulletVelocity = bulletConfig.MoveSpeed;
            /// Nếu có tốc độ bay của đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_speed_v))
            {
                int nBulletVelocity = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_speed_v).nValue[0];
                /// Chống BUG
                if (nBulletVelocity > bulletVelocity)
				{
                    bulletVelocity = nBulletVelocity;
				}
            }

            /// Số mục tiêu chạm tối đa
            int maxTargetTouch = bulletConfig.MaxTargetTouch;
            /// Nếu trong ProDict có tồn tại số mục tiêu chạm tối đa thì lấy ở ProDict ra thay thế
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_hitcount))
            {
                maxTargetTouch = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_hitcount).nValue[0];
            }

            /// Phạm vi nổ của đạn
            int bulletExplodeRadius = bulletConfig.ExplodeRadius;
            /// Nếu có phạm vi nổ (số ô)
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
            {
                bulletExplodeRadius = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range).nValue[2] * 10;
            }
            bulletExplodeRadius *= (1 + bulletConfig.ExplodeRadiusAddTimes);

            /// Xuyên suốt mục tiêu
            int pieceThroughTargetsPercent = bulletConfig.PieceThroughTargetsPercent;
            /// Nếu có thiết lập xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_ablility))
            {
                pieceThroughTargetsPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_ablility).nValue[0];
            }

            /// Nếu là kỹ năng tạo đạn bay theo 1 đường từ vị trí xuất chiêu
            if (skill.Data.Form == 0 || skill.Data.Form == 7 || skill.Data.Form == 8)
            {
                /// Nếu bay theo đường thẳng
                if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                {
                    /// Khoảng cách bay
                    float distance = bulletConfig.LifeTime / 18f * bulletVelocity;

                    /// Vị trí đặt
                    UnityEngine.Vector2 setPos;
                    /// Nếu không phải kỹ năng con
                    if (!isChildSkill)
                    {
                        setPos = castPos;
                    }
                    /// Nếu là kỹ năng con
                    else
                    {
                        setPos = castPos;
                    }

                    /// Vị trí đích tới
                    UnityEngine.Vector2 destPos;

                    /// Bản đồ hiện tại
                    GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

                    /// Nếu có thiết lập vị trí đich trước đó
                    if (targetPos.HasValue)
                    {
                        UnityEngine.Vector2 _targetPos = targetPos.Value;
                        /// Vị trí không có vật cản trên đường đi
                        _targetPos = KTGlobal.FindLinearNoObsPoint(gameMap, setPos, _targetPos, caster.CurrentCopyMapID);

                        /// Nếu vị trí đầu sát với vị trí cuối
                        if (UnityEngine.Vector2.Distance(setPos, _targetPos) <= 10f)
                        {
                            /// Nếu có hướng bay ban đầu
                            if (dirVectorProperty.HasValue)
                            {
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, dirVectorProperty.Value, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                            /// Nếu không có hướng ban đầu thì căn cứ hướng quay hiện tại của đối tượng ra chiêu
                            else
                            {
                                UnityEngine.Vector2 _dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, _dirVector, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                        }

                        destPos = skill.Data.Form == 8 ? targetPos.Value : KTMath.FindPointInVectorWithDistance(setPos, _targetPos - setPos, distance);
                    }
                    /// Nếu có mục tiêu thì vị trí đích tới là vị trí của mục tiêu
                    else if (target != null)
                    {
                        UnityEngine.Vector2 _targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
                        /// Vị trí không có vật cản trên đường đi
                        _targetPos = KTGlobal.FindLinearNoObsPoint(gameMap, setPos, _targetPos, caster.CurrentCopyMapID);

                        /// Nếu vị trí đầu sát với vị trí cuối
                        if (UnityEngine.Vector2.Distance(setPos, _targetPos) <= 10f)
                        {
                            /// Nếu có hướng bay ban đầu
                            if (dirVectorProperty.HasValue)
                            {
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, dirVectorProperty.Value, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                            /// Nếu không có hướng ban đầu thì căn cứ hướng quay hiện tại của đối tượng ra chiêu
                            else
                            {
                                UnityEngine.Vector2 _dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, _dirVector, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                        }

                        destPos = skill.Data.Form == 8 ? _targetPos : KTMath.FindPointInVectorWithDistance(setPos, _targetPos - setPos, distance);
                    }
                    /// Nếu không có mục tiêu thì căn cứ hướng hiện tại của đối tượng xuất chiêu phát ra
                    else
                    {
                        UnityEngine.Vector2 _targetPos = KTMath.MoveTowardByDirection(setPos, caster.CurrentDir, distance);
                        /// Vị trí không có vật cản trên đường đi
                        _targetPos = KTGlobal.FindLinearNoObsPoint(gameMap, setPos, _targetPos, caster.CurrentCopyMapID);

                        /// Nếu vị trí đầu sát với vị trí cuối
                        if (UnityEngine.Vector2.Distance(setPos, _targetPos) <= 10f)
                        {
                            /// Nếu có hướng bay ban đầu
                            if (dirVectorProperty.HasValue)
                            {
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, dirVectorProperty.Value, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                            /// Nếu không có hướng ban đầu thì căn cứ hướng quay hiện tại của đối tượng ra chiêu
                            else
                            {
                                UnityEngine.Vector2 _dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
                                _targetPos = KTMath.FindPointInVectorWithDistance(setPos, _dirVector, bulletVelocity * bulletConfig.LifeTime / 18f);
                            }
                        }

                        destPos = _targetPos;
                    }

                    /// Nếu là kỹ năng thường
                    if (skill.Data.Form == 0)
                    {
                        KTSkillManager.DoSkillBulletFlyByDirection(skill, skillPd, dirVectorProperty, caster, setPos, target, destPos, bulletConfig, bulletVelocity, bulletExplodeRadius, bulletTotalRound, maxTargetTouch, pieceThroughTargetsPercent);
                    }
                    /// Nếu là kỹ năng gọi kỹ năng phụ tại vị trí đạn bay mỗi Tick
                    else if (skill.Data.Form == 7 || skill.Data.Form == 8)
                    {
                        KTSkillManager.DoSkillBulletFlyByDirectionWithinFlySkill(skill, skillPd, dirVectorProperty, caster, setPos, target, destPos, bulletConfig, bulletVelocity, bulletExplodeRadius, bulletTotalRound, maxTargetTouch, pieceThroughTargetsPercent);
                    }
                    bulletPos = setPos;
                }
                /// Nếu bay theo đường tròn từ vị trí đối tượng xuất chiêu
                else if (bulletConfig.MoveKind == 3)
                {
                    /// Thực hiện
                    KTSkillManager.DoSkillBulletFlyByCircle(skill, skillPd, dirVectorProperty, caster, bulletConfig, bulletVelocity, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                    bulletPos = new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                }
                /// Nếu bay theo nửa đường tròn từ vị trí đối tượng xuất chiêu
                else if (bulletConfig.MoveKind == 4)
                {
                    /// Thực hiện
                    KTSkillManager.DoSkillBulletFlyByHalfCircle(skill, skillPd, dirVectorProperty, caster, bulletConfig, bulletVelocity, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                    bulletPos = new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                }
            }
            /// Nếu là kỹ năng phép thả 1 viên đạn tại vị trí chỉ định
            else if (skill.Data.Form == 1)
            {
                /// Nếu là bẫy, và không phải kỹ năng con
                if ((skill.Data.SkillStyle == "trap" || skill.Data.SkillStyle == "trapnodestroyontouch") && !isChildSkill)
                {
                    /// Vị trí đặt
                    UnityEngine.Vector2 setPos;
                    /// Nếu không phải kỹ năng con
                    if (!isChildSkill)
                    {
                        /// Nếu không có thiết lập vị trí sẵn
                        if (targetPos.HasValue)
                        {
                            setPos = targetPos.Value;
                        }
                        /// Nếu không có mục tiêu thì vị trí là vị trí xuất chiêu
                        else
                        {
                            setPos = castPos;
                        }
                    }
                    /// Nếu là kỹ năng con
                    else
                    {
                        setPos = castPos;
                    }

                    KTSkillManager.DoSkillTrapAtPosition(skill, skillPd, dirVectorProperty, caster, setPos, bulletConfig, bulletExplodeRadius, maxTargetTouch, bulletTotalRound);
                    bulletPos = setPos;
                }
                /// Nếu là kỹ năng xuất chiêu tại 1 điểm, hoặc kỹ năng con bẫy phát nổ
                else
                {
                    /// Vị trí đặt
                    UnityEngine.Vector2 setPos;
                    /// Nếu không phải kỹ năng con
                    if (!isChildSkill)
                    {
                        /// Nếu không có thiết lập vị trí sẵn
                        if (targetPos.HasValue)
                        {
                            setPos = targetPos.Value;
                        }
                        /// Nếu không có mục tiêu thì tính toán vị trí đích dựa vào hướng quay hiện tại của nhân vật và cự ly xuất chiêu
                        else
                        {
                            setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                        }
                    }
                    /// Nếu là kỹ năng con
                    else
                    {
                        setPos = castPos;
                    }

                    KTSkillManager.DoSkillStaticBulletAtPosition(skill, skillPd, dirVectorProperty, caster, setPos, bulletConfig, bulletExplodeRadius, maxTargetTouch, bulletTotalRound);
                    bulletPos = setPos;
                }
            }
            /// Nếu là kỹ năng thả đạn theo đường thẳng vuông góc với hướng quay của nhân vật
            else if (skill.Data.Form == 2)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu là kỹ năng bay được
                    if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        setPos = castPos;
                    }
                    /// Nếu có thiết lập vị trí sẵn
                    else if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillBulletFireWallAtPosition(skill, skillPd, caster, setPos, target, dirVectorProperty, bulletConfig, bulletVelocity, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                bulletPos = setPos;
            }
            /// Nếu là kỹ năng thả đạn theo hình quạt
            else if (skill.Data.Form == 3)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu là kỹ năng bay được
                    if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        setPos = castPos;
                    }
                    /// Nếu có thiết lập vị trí sẵn
                    else if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillBulletByFanShapeAtPosition(skill, skillPd, caster, setPos, target, dirVectorProperty, bulletConfig, bulletVelocity, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                bulletPos = setPos;
            }
            /// Nếu là kỹ năng thả đạn tỏa ra từ tâm một đường tròn
            else if (skill.Data.Form == 4)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;

                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu không có thiết lập vị trí sẵn
                    if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    /// Nếu không có mục tiêu thì tính toán vị trí đích dựa vào hướng quay hiện tại của nhân vật và cự ly xuất chiêu
                    else
                    {
                        /// Nếu là kỹ năng bay được
                        if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5 || bulletConfig.MoveKind == 6)
                        {
                            setPos = castPos;
                        }
                        
                        /// Nếu không phải kỹ năng bay được
                        else
                        {
                            setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                        }
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                /// Nếu là kỹ năng bay ra từ vị trí ra chiêu
                if (bulletConfig.MoveKind == 10)
                {
                    setPos = new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                    target = null;
                }

                KTSkillManager.DoSkillBulletByCircleAtPosition(skill, skillPd, dirVectorProperty, caster, setPos, target, bulletConfig, bulletVelocity, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent, initParams == null ? 0 : initParams[0]);
                bulletPos = setPos;
            }
            /// Nếu là kỹ năng thả đạn trong phạm vi xung quanh vị trí theo hình vuông tại vị trí chỉ định
            else if (skill.Data.Form == 5)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu là kỹ năng bay được
                    if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        setPos = castPos;
                    }
                    /// Nếu có thiết lập vị trí sẵn
                    else if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillBulletByRectangleAroundPosition(skill, skillPd, caster, setPos, target, dirVectorProperty, bulletConfig, bulletVelocity, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                bulletPos = setPos;
            }
            /// Nếu là kỹ năng thả đạn xung quanh vị trí chỉ định theo hình thoi
            else if (skill.Data.Form == 6)
            {
                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu là kỹ năng bay được
                    if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        setPos = castPos;
                    }
                    /// Nếu có thiết lập vị trí sẵn
                    else if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillBulletByDiamondShapeAroundPosition(skill, skillPd, caster, setPos, target, dirVectorProperty, bulletConfig, bulletVelocity, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound, pieceThroughTargetsPercent);
                bulletPos = setPos;
            }
            /// Nếu là kỹ năng thả đạn ngẫu nhiên xung quanh vị trí chỉ định
            else if (skill.Data.Form == 12)
            {

                /// Vị trí đặt
                UnityEngine.Vector2 setPos;
                /// Nếu không phải kỹ năng con
                if (!isChildSkill)
                {
                    /// Nếu không có thiết lập vị trí sẵn
                    if (targetPos.HasValue)
                    {
                        setPos = targetPos.Value;
                    }
                    /// Nếu không có mục tiêu thì tính toán vị trí đích dựa vào hướng quay hiện tại của nhân vật và cự ly xuất chiêu
                    else
                    {
                        setPos = KTMath.MoveTowardByDirection(castPos, caster.CurrentDir, attackRange);
                    }
                }
                /// Nếu là kỹ năng con
                else
                {
                    setPos = castPos;
                }

                KTSkillManager.DoSkillRandomBulletAroundPosition(skill, skillPd, dirVectorProperty, caster, setPos, bulletConfig, bulletCount, bulletExplodeRadius, maxTargetTouch, bulletTotalRound);
                bulletPos = setPos;
            }

            return bulletPos;
        }
        #endregion

        #region Non-Bullet
        /// <summary>
        /// Thực hiện kỹ năng không phải dạng biểu diễn đạn bay (kỹ năng với Type = 1)
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="castPos"></param>
        /// <param name="target"></param>
        /// <param name="targetPos"></param>
        /// <param name="dirVectorProperty"></param>
        /// <param name="skill"></param>
        /// <param name="skillPd"></param>
        /// <param name="isChildSkill"></param>
        /// <returns></returns>
        private static UnityEngine.Vector2 DoNoneBulletSkillLogic(GameObject caster, UnityEngine.Vector2 castPos, GameObject target, UnityEngine.Vector2? targetPos, UnityEngine.Vector2? dirVectorProperty, SkillLevelRef skill, PropertyDictionary skillPd, bool isChildSkill)
        {
            /// Vị trí xuất hiện đạn
            UnityEngine.Vector2 bulletPos = castPos;

            /// Cấu hình đạn
            BulletConfig bulletConfig = KSkill.GetBulletConfig(skill.Data.BulletID);
            /// Nếu không có cấu hình đạn thì bỏ qua
            if (bulletConfig == null)
            {
                return bulletPos;
            }
            /// Nếu không có ProDict
            else if (skillPd == null)
            {
                return bulletPos;
            }

            /// Cự ly thi triển
            int attackRange = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                attackRange = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Tổng số đạn
            int bulletCount = skill.Data.BulletCount;
            /// Nếu có cấu hình số tia đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_missilenum_v))
            {
                bulletCount = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_missilenum_v).nValue[0];
            }

            /// Tổng số lượt ra đạn
            int bulletTotalRound = skill.Data.ShootCount;
            /// Nếu có số lượt ra chiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_shotnumber))
            {
                bulletTotalRound = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_shotnumber).nValue[0];
            }

            /// Tốc độ bay của đạn
            int bulletVelocity = bulletConfig.MoveSpeed;
            /// Nếu có tốc độ bay của đạn
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_speed_v))
            {
                bulletVelocity = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_speed_v).nValue[0];
            }

            /// Số mục tiêu chạm tối đa
            int maxTargetTouch = bulletConfig.MaxTargetTouch;
            /// Nếu trong ProDict có tồn tại số mục tiêu chạm tối đa thì lấy ở ProDict ra thay thế
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_hitcount))
            {
                maxTargetTouch = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_hitcount).nValue[0];
            }

            /// Phạm vi nổ của đạn
            int bulletExplodeRadius = bulletConfig.ExplodeRadius;
            /// Nếu có phạm vi nổ (số ô)
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_range))
            {
                bulletExplodeRadius = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_range).nValue[2] * 10;
            }
            bulletExplodeRadius *= (1 + bulletConfig.ExplodeRadiusAddTimes);

            /// Xuyên suốt mục tiêu
            int pieceThroughTargetsPercent = bulletConfig.PieceThroughTargetsPercent;
            /// Nếu có thiết lập xuyên suốt mục tiêu
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_ablility))
            {
                pieceThroughTargetsPercent = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_ablility).nValue[0];
            }

            /// Nếu là kỹ năng tấn công phía trước
            if (skill.Data.Form == 5)
            {
                /// Nếu bay theo đường thẳng
                if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                {
                    /// Khoảng cách bay
                    float distance = bulletConfig.LifeTime / 18f * bulletVelocity;

                    /// Vị trí đặt
                    UnityEngine.Vector2 setPos;
                    /// Nếu không phải kỹ năng con
                    if (!isChildSkill)
                    {
                        setPos = castPos;
                    }
                    /// Nếu là kỹ năng con
                    else
                    {
                        setPos = castPos;
                    }

                    /// Vị trí đích tới
                    UnityEngine.Vector2 destPos;

                    /// Nếu có thiết lập vị trí đich trước đó
                    if (targetPos.HasValue)
                    {
                        destPos = KTMath.FindPointInVectorWithDistance(setPos, targetPos.Value - setPos, distance);
                    }
                    /// Nếu có mục tiêu thì vị trí đích tới là vị trí của mục tiêu
                    else if (target != null)
                    {
                        UnityEngine.Vector2 _targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
                        destPos = KTMath.FindPointInVectorWithDistance(setPos, _targetPos - setPos, distance);
                    }
                    /// Nếu không có mục tiêu thì căn cứ hướng hiện tại của đối tượng xuất chiêu phát ra
                    else
                    {
                        destPos = KTMath.MoveTowardByDirection(setPos, caster.CurrentDir, distance);
                    }

                    KTSkillManager.DoSkillBulletFlyByDirection(skill, skillPd, dirVectorProperty, caster, setPos, target, destPos, bulletConfig, bulletVelocity, bulletExplodeRadius, bulletTotalRound, maxTargetTouch, pieceThroughTargetsPercent);
                    bulletPos = setPos;
                }
            }

            return bulletPos;
        }
        #endregion

        #region Child skill
        /// <summary>
        /// Thực hiện gọi kỹ năng con
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="childSkillID">ID kỹ năng con</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="castPos">Vị trí xuất chiêu</param>
        /// <param name="target">Đối tượng ảnh hưởng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        private static void DoCallChildSkill(SkillLevelRef skill, int childSkillID, GameObject caster, UnityEngine.Vector2 castPos, UnityEngine.Vector2? targetPosProperty, GameObject target, UnityEngine.Vector2? dirVectorProperty, int[] initParams = null)
        {
            SkillDataEx bulletSkill = KSkill.GetSkillData(childSkillID);
            if (bulletSkill != null)
            {
                SkillLevelRef skillRef = new SkillLevelRef()
                {
                    Data = bulletSkill,
                    AddedLevel = skill.AddedLevel,
                    BonusLevel = skill.BonusLevel,
                    CanStudy = false,
                };

                /// Thực hiện kỹ năng con
                KTSkillManager.UseSkill_DoAction(caster, castPos, targetPosProperty, target, dirVectorProperty, skillRef, true, initParams);
            }
        }
        #endregion

        /// <summary>
        /// Thực hiện biểu diễn kỹ năg
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="castPos">Vị trí xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="targetPos">Vị trí mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="ignoredTarget">Mục tiêu bị bỏ qua</param>
        /// <param name="isChildSkill">Có phải kỹ năng con không</param>
        private static void DoAction(GameObject caster, UnityEngine.Vector2 castPos, GameObject target, UnityEngine.Vector2? targetPos, UnityEngine.Vector2? dirVectorProperty, SkillLevelRef skill, PropertyDictionary skillPd, bool isChildSkill, int[] initParams)
        {
            /// Nếu không có ProDict
            if (skillPd == null)
            {
                return;
            }

            /// Đối tượng người chơi
            KPlayer player = caster as KPlayer;

            /// ProDict kỹ năng hỗ trợ kỹ năng này
            PropertyDictionary enchantPd = player == null ? null : player.Skills.GetEnchantProperties(skill.SkillID);
            /// Nếu tồn tại kỹ năng hỗ trợ
            if (enchantPd != null)
            {
                skillPd = skillPd.Clone();
                skillPd.AddProperties(enchantPd);
            }

            /// Vị trí phát chiêu thức con
            UnityEngine.Vector2 childSkillPos = castPos;

            /// Đánh dấu đã gọi kỹ năng con chưa
            bool bulletChildSkillCalled = false;

            /// Nếu là kỹ năng quần thể
            if (skill.Data.Type == 5)
            {
                /// Nếu không phải đạn thì thực thi kỹ năng đạn tương ứng gọi đến
                if (!skill.Data.IsBullet)
                {
                    /// Nếu có vị trí mục tiêu
                    if (targetPos.HasValue)
                    {
                        childSkillPos = targetPos.Value;
                    }
                    KTSkillManager.DoCallChildSkill(skill, skill.Data.BulletID, caster, childSkillPos, targetPos, target, dirVectorProperty);
                    bulletChildSkillCalled = true;
                }
                /// Nếu là đạn thì thực hiện biểu diễn
                else
                {
                     childSkillPos = KTSkillManager.DoBulletSkillLogic(caster, childSkillPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill, initParams);
                }
            }
            /// Nếu là kỹ năng Buff đơn mục tiêu
            else if (skill.Data.Type == 2)
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

                KTSkillManager.DoSkillSingleTargetBuff(caster, target, duration, skill, skillPd);
            }
            /// Nếu là kỹ năng xuất chiêu tại một vị trí nhưng nhảy sang các mục tiêu kế cận
            else if (skill.Data.Type == 6)
            {
                childSkillPos = KTSkillManager.DoBulletJumpTowardTargets(caster, childSkillPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill);
            }
            /// Tác động đến các đối tượng xung quanh bản thân
            else if (skill.Data.Type == 4)
            {
                /// Vòng sáng
                if (skill.Data.IsArua)
                {
                    KTSkillManager.DoSkillArua(caster, target, skill, isChildSkill);
                }
            }
            /// Nếu là kỹ năng bị động
            else if (skill.Data.Type == 3)
            {
                KTSkillManager.DoSkillPassive(caster, skill);
            }
            /// Nếu là kỹ năng đơn theo hướng quay của nhân vật hiện tại
            else if (skill.Data.Type == 1)
            {
                /// Kỹ năng tốc biến
                if (skill.Data.Form == 6)
                {
                    KTSkillManager.DoSkillBlinkToPosition(skill, caster, skillPd);
                }
                /// Kỹ năng lao nhanh đến phía mục tiêu
                else if (skill.Data.Form == 4)
                {
                    KTSkillManager.DoSkillBlinkToTargetPosition(skill, caster, target, skillPd, false);
                }
                /// Kỹ năng xuất chiêu phía trước
                else if (skill.Data.Form == 5)
                {
                    /// Nếu không phải đạn thì thực thi kỹ năng con tương ứng gọi đến
                    if (!skill.Data.IsBullet)
                    {
                        /// Nếu có vị trí mục tiêu
                        if (targetPos.HasValue)
                        {
                            childSkillPos = targetPos.Value;
                        }
                        KTSkillManager.DoCallChildSkill(skill, skill.Data.BulletID, caster, childSkillPos, targetPos, target, dirVectorProperty);
                        bulletChildSkillCalled = true;
                    }
                    else
                    {
                        childSkillPos = KTSkillManager.DoNoneBulletSkillLogic(caster, childSkillPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill);
                    }
                }
                /// Kỹ năng khinh công phía trước theo hướng hiện tại
                else if (skill.Data.Form == 2)
                {
                    KTSkillManager.DoSkillFlyToPosition(skill, caster, skillPd);
                }
                /// Kỹ năng lao nhanh đến phía mục tiêu kèm gọi kỹ năng đạn bay
                else if (skill.Data.Form == 10)
                {
                    KTSkillManager.DoSkillBlinkToTargetPositionWithinFlySkill(skill, caster, target, skillPd);
                }
                /// Kỹ năng lao nhanh đến theo hướng mục tiêu
                else if (skill.Data.Form == 3)
                {
                    KTSkillManager.DoSkillBlinkToTargetPosition(skill, caster, target, skillPd, true);
                }
                /// Kỹ năng xuất chiêu phía trước nhân vật mỗi khoảng Tick một lần đến khi hết thời gian
                else if (skill.Data.Form == 12)
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

                    /// Thời gian Tick mỗi lần thực thi
                    float tick = skill.Data.Params[0] / 18f;

                    /// Góc của điểm bắt đầu nằm trên đường tròn
                    int angle = 0;

                    void Execute()
                    {
                        /// Vị trí hiện tại của đối tượng ra chiêu
                        UnityEngine.Vector2 currentCasterPos = new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                        /// Vị trí hiện tại của mục tiêu
                        UnityEngine.Vector2 currentTargetPos = default;
                        if (target != null)
                        {
                            targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
                        }

                        /// Nếu không phải đạn thì thực thi kỹ năng đạn tương ứng gọi đến
                        if (!skill.Data.IsBullet)
                        {
                            /// Nếu có vị trí mục tiêu
                            if (targetPos.HasValue)
                            {
                                childSkillPos = targetPos.Value;
                            }
                            KTSkillManager.DoCallChildSkill(skill, skill.Data.BulletID, caster, skill.Data.StartPoint == 2 && target != null ? currentTargetPos : skill.Data.StartPoint == 3 ? currentCasterPos : childSkillPos, targetPos, target, dirVectorProperty, new int[] { angle });
                            bulletChildSkillCalled = true;
                        }
                        else
                        {
                            childSkillPos = KTSkillManager.DoNoneBulletSkillLogic(caster, skill.Data.StartPoint == 2 && target != null ? currentTargetPos : skill.Data.StartPoint == 3 ? currentCasterPos : childSkillPos, target, targetPos, dirVectorProperty, skill, skillPd, isChildSkill);
                        }

                        angle += skill.Data.Params[1];
                    }
                    Execute();

                    KTSkillManager.SetSchedule(tick, duration, () => {
                        Execute();
                    });
                }
                /// Kỹ năng xuất chiêu liên tục nhảy giữa các mục tiêu xung quanh
                else if (skill.Data.Form == 11)
                {
                    KTSkillManager.DoSkillBlinkTowardTargets(skill, caster, target, skillPd);
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
                KTSkillManager.UseSkill_DoAction(caster, skillRef.Data.Form == 0 ? castPos : childSkillPos, targetPos, target, dirVectorProperty, skillRef, true);
            }
            if (enchantPd != null && enchantPd.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_addstartskill);
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
                            AddedLevel = magicAttrib.nValue[2],
                            BonusLevel = 0,
                            CanStudy = false,
                        };
                        KTSkillManager.UseSkill_DoAction(caster, skillRef.Data.Form == 0 ? castPos : childSkillPos, targetPos, target, dirVectorProperty, skillRef, true);

                        //Console.WriteLine("Auto activate start skill -> " + subSkill.Name);
                    }
                }
            }
            else if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_startevent))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_startevent);
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
                        KTSkillManager.UseSkill_DoAction(caster, skillRef.Data.Form == 0 ? castPos : childSkillPos, targetPos, target, dirVectorProperty, skillRef, true);
                    }
                }
            }

            /// Nếu có kỹ năng con đi kèm, đồng thời chưa thực thi bên trên
            if (!bulletChildSkillCalled && !skill.Data.IsBullet)
            {
                SkillDataEx bulletSkill = KSkill.GetSkillData(skill.Data.BulletID);
                if (bulletSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = bulletSkill.Clone(),
                        AddedLevel = skill.AddedLevel,
                        BonusLevel = skill.BonusLevel,
                        CanStudy = false,
                    };

                    /// Thực hiện kỹ năng con
                    KTSkillManager.UseSkill_DoAction(caster, skillRef.Data.Form == 0 ? castPos : childSkillPos, targetPos, target, dirVectorProperty, skillRef, true);
                }
            }
            /*
            if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_flyevent))
            {
                SkillDataEx bulletSkill = KSkill.GetSkillData(skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_flyevent).nValue[0]);
                if (bulletSkill != null)
                {
                    SkillLevelRef skillRef = new SkillLevelRef()
                    {
                        Data = bulletSkill,
                        AddedLevel = skill.AddedLevel,
                        BonusLevel = skill.BonusLevel,
                        CanStudy = false,
                    };

                    /// Thực hiện kỹ năng con
                    KTSkillManager.UseSkill_DoAction(caster, skillRef.Data.Form == 0 ? castPos : childSkillPos, target, dirVectorProperty, skillRef, true);
                }
            }
            */
        }
    }
}
