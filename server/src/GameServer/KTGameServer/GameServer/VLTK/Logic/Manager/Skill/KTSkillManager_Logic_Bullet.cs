using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Network.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static GameServer.KiemThe.Logic.KTBulletManager;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic kỹ năng tạo đạn bay
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Thực hiện biểu diễn kỹ năng bay theo hướng
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="toPos">Vị trí đích đến</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc bay của đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ của đạn</param>
        /// <param name="bulletTotalRound">Tổng số lượt ra đạn</param>
        /// <param name="maxTargetTouch">Số mục tiêu chạm tối đa</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletFlyByDirection(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 fromPos, GameObject target, UnityEngine.Vector2 toPos, BulletConfig bulletConfig, int bulletVelocity, int bulletExplodeRadius, int bulletTotalRound, int maxTargetTouch, int pieceThroughTargetsPercent)
        {
            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullet(bool showEffect)
            {
                /// Tạo đạn
                BulletPacketBuilder builder = new BulletPacketBuilder();
                /// Nếu đạn đuổi mục tiêu
                if (bulletConfig.IsFollowTarget && target != null)
                {
                    KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, fromPos, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    //KT_TCPHandler.SendCreateBullet(caster, result.BulletID, bulletConfig.ID, fromPos, fromPos, target, false, bulletVelocity, 0, bulletConfig.LoopAnimation, 0);
                    builder.Append(result.BulletID, bulletConfig.ID, fromPos, fromPos, target, bulletVelocity, 0, bulletConfig.LoopAnimation, 0, caster, false);
                }
                /// Nếu không phải đạn đuổi mục tiêu
                else
                {
                    KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, fromPos, toPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                    if (showEffect)
                    {
                        /// Vị trí đích đến
                        UnityEngine.Vector2 _toPos = result.ToPos == UnityEngine.Vector2.zero ? toPos : result.ToPos;
                        /// Khoảng cách bay được của đạn theo thiết lập
                        float bulletCanFlyDistance = bulletVelocity * bulletConfig.LifeTime / 18f;
                        /// Khoảng cách kiểm tra
                        float minDistance = Math.Min(bulletCanFlyDistance, 100);
                        /// Nếu khoảng cách quá nhỏ
                        if (UnityEngine.Vector2.Distance(fromPos, _toPos) <= minDistance)
                        {
                            _toPos = KTMath.FindPointInVectorWithDistance(fromPos, _toPos - fromPos, minDistance);
                        }
                        /// Thời gian cần để bay đến đích
                        float lifeTime = UnityEngine.Vector2.Distance(fromPos, _toPos);
                        /// Gửi gói tin tạo đạn đến người chơi xung quanh
                        //KT_TCPHandler.SendCreateBullet(caster, result.BulletID, bulletConfig.ID, fromPos, _toPos, null, false, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0);
                        builder.Append(result.BulletID, bulletConfig.ID, fromPos, _toPos, null, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0, caster, bulletConfig.IsComeback);
                    }
                }
                /// Gửi gói tin
                KT_TCPHandler.SendCreateBullets(caster, builder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullet(true);

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullet(true);
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn kỹ năng bay theo hướng đồng thời gọi kỹ năng phụ mỗi khoảng 0.2s
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="toPos">Vị trí đích đến</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc bay của đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ của đạn</param>
        /// <param name="bulletTotalRound">Tổng số lượt ra đạn</param>
        /// <param name="maxTargetTouch">Số mục tiêu chạm tối đa</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletFlyByDirectionWithinFlySkill(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 fromPos, GameObject target, UnityEngine.Vector2 toPos, BulletConfig bulletConfig, int bulletVelocity, int bulletExplodeRadius, int bulletTotalRound, int maxTargetTouch, int pieceThroughTargetsPercent)
        {
            /// Cấu hình kỹ năng đi kèm không
            int flySkillID = skill.Data.BulletSkillID;
            /// Kiểm tra trong ProDict có kỹ năng đi kèm không
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_flyevent))
            {
                flySkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_flyevent).nValue[0];
            }

            /// Kỹ năng bay
            SkillDataEx flySkill = KSkill.GetSkillData(flySkillID);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullet(bool showEffect)
            {
                /// Tạo đạn
                BulletPacketBuilder builder = new BulletPacketBuilder();
                /// Nếu đạn đuổi mục tiêu
                if (bulletConfig.IsFollowTarget && target != null)
                {
                    KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, fromPos, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, 100f, (bulletPos) => {
                        if (flySkill != null)
                        {
                            KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_flyevent);
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
                                    KTSkillManager.UseSkill_DoAction(caster, bulletPos, null, target, dirVectorProperty, skillRef, true);
                                }
                            }
                        }
                    });

                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    //KT_TCPHandler.SendCreateBullet(caster, result.BulletID, bulletConfig.ID, fromPos, fromPos, target, false, bulletVelocity, 0, bulletConfig.LoopAnimation, 0);
                    builder.Append(result.BulletID, bulletConfig.ID, fromPos, fromPos, target, bulletVelocity, 0, bulletConfig.LoopAnimation, 0, caster, false);
                }
                /// Nếu không phải đạn đuổi mục tiêu
                else
                {
                    KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, fromPos, toPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback, 100f, (bulletPos) => {
                        if (flySkill != null)
                        {
                            KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_flyevent);
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
                                    KTSkillManager.UseSkill_DoAction(caster, bulletPos, null, target, dirVectorProperty, skillRef, true);
                                }
                            }
                        }
                    });

                    if (showEffect)
                    {
                        /// Vị trí đích đến
                        UnityEngine.Vector2 _toPos = result.ToPos == UnityEngine.Vector2.zero ? toPos : result.ToPos;
                        /// Khoảng cách bay được của đạn theo thiết lập
                        float bulletCanFlyDistance = bulletVelocity * bulletConfig.LifeTime / 18f;
                        /// Khoảng cách kiểm tra
                        float minDistance = Math.Min(bulletCanFlyDistance, 100);
                        /// Nếu khoảng cách quá nhỏ
                        if (UnityEngine.Vector2.Distance(fromPos, _toPos) <= minDistance)
                        {
                            _toPos = KTMath.FindPointInVectorWithDistance(fromPos, _toPos - fromPos, minDistance);
                        }
                        /// Thời gian cần để bay đến đích
                        float lifeTime = UnityEngine.Vector2.Distance(fromPos, _toPos);
                        /// Gửi gói tin tạo đạn đến người chơi xung quanh
                        //KT_TCPHandler.SendCreateBullet(caster, result.BulletID, bulletConfig.ID, fromPos, _toPos, null, false, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0);
                        builder.Append(result.BulletID, bulletConfig.ID, fromPos, _toPos, null, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0, caster, bulletConfig.IsComeback);
                    }
                }
                /// Gửi gói tin
                KT_TCPHandler.SendCreateBullets(caster, builder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullet(true);

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullet(true);
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn đặt bẫy tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí đặt bẫy</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Tổng số lượt ra chiêu</param>
        private static void DoSkillTrapAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, BulletConfig bulletConfig, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound)
        {
            /// Thời gian tồn tại tối đa của bẫy
            float trapLifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                trapLifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// <summary>
            /// Khởi tạo bẫy
            /// </summary>
            void CreateTrap()
            {
                int id = KTBulletManager.Instance.CreateTrap(skill, dirVectorProperty, caster, pos, bulletExplodeRadius, bulletConfig.DamageInterval / 18f, maxTargetTouch, trapLifeTime);

                /// Gửi gói tin tạo bẫy đến người chơi xung quanh
                //KT_TCPHandler.SendCreateBullet(caster, id, bulletConfig.ID, pos, pos, null, true, 0, trapLifeTime, bulletConfig.LoopAnimation, 0);
                KTTrapManager.AddTrap(id, caster.CurrentMapCode, caster, bulletConfig.ID, (int) pos.x, (int) pos.y, trapLifeTime);
            }

            /// Tạo viên đạn tương ứng
            CreateTrap();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo bẫy tương ứng
                    CreateTrap();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn đạn tĩnh tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ của đạn</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        private static void DoSkillRandomBulletAroundPosition(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, BulletConfig bulletConfig, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullet()
            {
                /// Tạo đạn
                BulletPacketBuilder builder = new BulletPacketBuilder();
                /// Bán kính ngẫu nhiên
                int randomRadius = skill.Data.Params[0];
                /// Thời gian xuất hiện lần lượt giữa các điểm
                float timeDelayEach = skill.Data.Params[1] / 18f;
                /// Chỉ hiển thị ở Client còn GS chỉ tick 1 lần damage duy nhất không
                bool clientOnly = skill.Data.Params[2] == 1;
                /// Tạo ngẫu nhiên các viên đạn xung quanh
                List<UnityEngine.Vector2> randomPoint = KTMath.GetRandomPointsAroundPos(pos, randomRadius, bulletCount);

                /// Nếu chỉ hiển thị ở client
                if (clientOnly)
                {
                    /// Tạo đạn
                    KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, pos, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage, 0);

                }

                /// Tổng thời gian đã sinh ra đạn
                float totalTime = 0;
                /// Duyệt danh sách điểm
                foreach (UnityEngine.Vector2 point in randomPoint)
                {
                    /// Nếu cả GS
                    if (!clientOnly)
                    {
                        /// Tạo đạn
                        int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage, totalTime);
                        /// Thêm đạn
                        builder.Append(id, bulletConfig.ID, point, point, null, 0, lifeTime, bulletConfig.LoopAnimation, totalTime, caster, false);
                    }
                    /// Nếu chỉ hiển thị ở client
                    else
                    {
                        /// Thêm đạn
                        builder.Append(-1, bulletConfig.ID, point, point, null, 0, lifeTime, bulletConfig.LoopAnimation, totalTime, caster, false);
                    }

                    /// Tăng tổng thời gian đã sinh ra
                    totalTime += timeDelayEach;
                }
                /// Gửi gói tin
                KT_TCPHandler.SendCreateBullets(caster, builder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullet();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullet();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn đạn tĩnh tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ của đạn</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        private static void DoSkillStaticBulletAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, BulletConfig bulletConfig, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Thời gian ném
            float throwingTime = 0f;
            /// Nếu là kỹ năng ném
            if (skill.Data.SkillStyle == "throw")
            {
                /// Khoảng cách đến mục tiêu
                float distance = UnityEngine.Vector2.Distance(new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y), pos);
                /// Tốc độ bay của đạn
                float bulletVelocity = bulletConfig.MoveSpeed;
                /// Thời gian bay đến
                throwingTime = distance / bulletVelocity;
            }

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullet(bool showEffect)
            {
                UnityEngine.Vector2 castPos = skill.Data.StartPoint == 10 ? new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y) : pos;

                /// Tạo đạn
                BulletPacketBuilder builder = new BulletPacketBuilder();
                int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, castPos, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage, throwingTime);

                if (showEffect)
                {
                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    //KT_TCPHandler.SendCreateBullet(caster, id, bulletConfig.ID, pos, pos, null, false, skill.Data.SkillStyle == "throw" ? bulletConfig.MoveSpeed : 0, lifeTime + throwingTime, bulletConfig.LoopAnimation, 0);
                    builder.Append(id, bulletConfig.ID, castPos, castPos, null, skill.Data.SkillStyle == "throw" ? bulletConfig.MoveSpeed : 0, lifeTime + throwingTime, bulletConfig.LoopAnimation, 0, caster, false);

                    /// Gửi gói tin
                    KT_TCPHandler.SendCreateBullets(caster, builder.Build());
                }
            }

            /// Tạo viên đạn tương ứng
            CreateBullet(true);

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullet(false);
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn đạn tĩnh tại vị trí chỉ định và nhảy sang các mục tiêu kế cận
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ của đạn</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        /// <param name="attackRange">Phạm vi tấn công</param>
        private static void DoSkillStaticBulletJumpTowardTargetsAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, BulletConfig bulletConfig, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int attackRange)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Tạo đối tượng xây dựng gói tin gửi về Client
            BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullet()
            {
                /// Danh sách các mục tiêu xung quanh
                List<GameObject> targets = KTGlobal.GetNearByEnemies(caster, attackRange);

                float spawnTick = 0.5f;
                int idx = -1;

                /// Tổng số mục tiêu đã chạm phải
                int totalTouchTargets = 0;

                /// Duyệt danh sách mục tiêu tìm được
                foreach (GameObject target in targets)
                {
                    idx++;

                    /// Nếu đã chết
                    if (target.IsDead())
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Tăng số mục tiêu đã chạm phải
                    totalTouchTargets++;

                    /// Vị trí mục tiêu
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
                    /// Tạo tia đạn tương ứng
                    int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, targetPos, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage, idx * spawnTick);

                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    packetBuilder.Append(id, bulletConfig.ID, targetPos, targetPos, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, idx * spawnTick, caster, false);

                    /// Nếu đã vượt quá số mục tiêu
                    if (totalTouchTargets >= maxTargetTouch)
                    {
                        /// Thoát
                        break;
                    }
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullet();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullet();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn kỹ năng tường lửa ở vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí tâm đường thẳng</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng ra chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Tổng số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletFireWallAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, GameObject caster, UnityEngine.Vector2 pos, GameObject target, UnityEngine.Vector2? dirVectorProperty, BulletConfig bulletConfig, int bulletVelocity, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Khoảng cách bay của đạn
            float distance = bulletVelocity * bulletConfig.LifeTime / 18f;

            /// Đường thẳng nhận Vector chỉ hướng quay làm pháp tuyến
            KTMath.Line line = KTMath.GetLineFromNormalVector(pos, dirVector);

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                /// Thực hiện Logic với tia đạn tương ứng
                void DoLogic(UnityEngine.Vector2 point, float delayAnimation)
                {
                    /// Nếu là đạn đứng yên một chỗ
                    if (bulletConfig.MoveKind == 0)
                    {
                        int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage);

                        /// Gửi gói tin tạo đạn đến người chơi xung quanh
                        packetBuilder.Append(id, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, delayAnimation, caster, false);
                    }
                    /// Nếu là đạn bay theo đường thẳng
                    else if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        /// Nếu đạn đuổi mục tiêu
                        if (bulletConfig.IsFollowTarget && target != null)
                        {
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, point, point, target, bulletVelocity, 0, bulletConfig.LoopAnimation, delayAnimation, caster, false);
                        }
                        /// Nếu không phải đạn đuổi mục tiêu
                        else
                        {
                            UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(point, dirVector, distance);
                            /// Tìm vị trí không có vật cản
                            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, point, destPos, caster.CurrentCopyMapID);
                            /// Nếu không trùng với vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(point, destPos) > 10)
                            {
                                KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, destPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                                /// Gửi gói tin tạo đạn đến người chơi xung quanh
                                packetBuilder.Append(result.BulletID, bulletConfig.ID, point, result.ToPos == UnityEngine.Vector2.zero ? destPos : result.ToPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, delayAnimation, caster, bulletConfig.IsComeback);
                            }
                        }
                    }
                }

                /// Kỹ năng đặc biệt
                if (skill.Data.StartPoint == 3)
                {
                    int paragraphLength = skill.Data.Params[0] * bulletCount;
                    List<UnityEngine.Vector2> points = KTMath.GetPointsInParagraphWithDistance(line, pos, paragraphLength, bulletCount);

                    foreach (UnityEngine.Vector2 point in points)
                    {
                        float delayAnimation = KTGlobal.GetRandomNumber(0f, 0.4f);
                        DoLogic(point, delayAnimation);
                    }
                }
                else
                {
                    int paragraphLength = skill.Data.Params[0] * bulletCount;
                    List<UnityEngine.Vector2> points = KTMath.GetPointsInParagraphWithDistance(line, pos, paragraphLength, bulletCount);
                    foreach (UnityEngine.Vector2 point in points)
                    {
                        DoLogic(point, 0f);
                    }
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn kỹ năng tạo đạn theo hình quạt tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng ra chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng bay</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Tổng số lượt ra đạn</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletByFanShapeAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, GameObject caster, UnityEngine.Vector2 pos, GameObject target, UnityEngine.Vector2? dirVectorProperty, BulletConfig bulletConfig, int bulletVelocity, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Khoảng cách bay của đạn
            float distance = bulletVelocity * bulletConfig.LifeTime / 18f;

            /// Vị trí ban đầu phát chiêu
            int startRadius = skill.Data.Params[0];
            if (startRadius < 5)
            {
                startRadius = 5;
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                int angle = 5;
                /// Nếu có thiết lập góc
                if (skill.Data.Params[2] > 0)
                {
                    angle = skill.Data.Params[2];
                }

                int degree = angle * 2 * (bulletCount - 1);
                List<UnityEngine.Vector2> points = KTMath.GetArcPointsFromVector(pos, dirVector, startRadius, degree, bulletCount);

                int idx = -1;
                foreach (UnityEngine.Vector2 point in points)
                {
                    idx++;

                    /// Nếu là đạn đứng yên một chỗ
                    if (bulletConfig.MoveKind == 0)
                    {
                        int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage);

                        /// Gửi gói tin tạo đạn đến người chơi xung quanh
                        packetBuilder.Append(id, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, 0, caster, false);
                    }
                    /// Nếu là đạn bay theo đường thẳng
                    else if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        /// Nếu đạn đuổi mục tiêu
                        if (bulletConfig.IsFollowTarget && target != null)
                        {
                            /// Vị trí xuất phát
                            UnityEngine.Vector2 sPos;

                            /// Nếu vị trí xuất phát là từ trong ra ngoài
                            if (skill.Data.StartPoint == 1 || skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6)
                            {
                                sPos = point;
                            }
                            /// Nếu vị trí xuất phát là từ ngoài vào trong
                            else
                            {
                                sPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                            }

                            /// Tạo đạn
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, sPos, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, sPos, sPos, target, bulletVelocity, 0, bulletConfig.LoopAnimation, 0, caster, false);
                        }
                        /// Nếu không phải đạn đuổi mục tiêu
                        else
                        {
                            /// Vị trí xuất phát
                            UnityEngine.Vector2 sPos;
                            /// Vị trí đích
                            UnityEngine.Vector2 toPos;

                            /// Nếu vị trí xuất phát là từ trong ra ngoài
                            if (skill.Data.StartPoint == 1 || skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6)
                            {
                                sPos = point;
                                toPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                            }
                            /// Nếu vị trí xuất phát là từ ngoài vào trong
                            else if (skill.Data.StartPoint == 3)
                            {
                                sPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                                toPos = pos;
                            }
                            /// Nếu vị trí xuất phát là từ ngoài vào trong
                            else
                            {
                                sPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                                toPos = point;
                            }

                            /// Tìm vị trí không có vật cản
                            toPos = KTGlobal.FindLinearNoObsPoint(gameMap, sPos, toPos, caster.CurrentCopyMapID);
                            /// Nếu không trùng với vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(sPos, toPos) > 10)
                            {
                                KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, sPos, toPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                                /// Gửi gói tin tạo đạn đến người chơi xung quanh
                                packetBuilder.Append(result.BulletID, bulletConfig.ID, sPos, result.ToPos == UnityEngine.Vector2.zero ? toPos : result.ToPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6 ? 0.1f * idx : 0, caster, bulletConfig.IsComeback);
                            }
                        }
                    }
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Tạo đạn bay theo hình tròn
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletFlyByCircle(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, BulletConfig bulletConfig, int bulletVelocity, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Nếu quá ngưỡng thì Set lại
            lifeTime = UnityEngine.Mathf.Min(5f, lifeTime);

            /// Đoạn đường sẽ đi được với tốc độ và thời gian duy trì
            float distance = lifeTime * bulletVelocity;
            /// Bán kính
            float radius = distance / UnityEngine.Mathf.PI / 2;

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                /// Nếu chạy theo đối tượng ra chiêu
                if (bulletConfig.IsFollowTarget)
                {
                    /// Tạo viên đạn tương ứng
                    CreateBulletFlyResult result = KTBulletManager.Instance.CreateBulletFlyByCircle(skill, caster, null, dirVector, radius, bulletVelocity, lifeTime, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);
                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    packetBuilder.Append(result.BulletID, bulletConfig.ID, casterPos, result.ToPos, null, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0, caster, false, radius, true, dirVector);
                }
                /// Nếu không chạy theo đối tượng ra chiêu
                else
                {
                    /// Tạo viên đạn tương ứng
                    CreateBulletFlyResult result = KTBulletManager.Instance.CreateBulletFlyByCircle(skill, caster, casterPos, dirVector, radius, bulletVelocity, lifeTime, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);
                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    packetBuilder.Append(result.BulletID, bulletConfig.ID, casterPos, result.ToPos, null, bulletVelocity, lifeTime, bulletConfig.LoopAnimation, 0, caster, false, radius, false, dirVector);
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Tạo đạn bay theo nửa hình tròn
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletFlyByHalfCircle(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, BulletConfig bulletConfig, int bulletVelocity, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Nhân đôi lên (sau đó mới apply vào thành nửa vòng
            lifeTime *= 2;
            /// Nếu quá ngưỡng thì Set lại
            lifeTime = UnityEngine.Mathf.Min(5f, lifeTime);

            /// Đoạn đường sẽ đi được với tốc độ và thời gian duy trì
            float distance = lifeTime * bulletVelocity;
            /// Bán kính
            float radius = distance / UnityEngine.Mathf.PI / 2;

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                /// Nếu chạy theo đối tượng ra chiêu
                if (bulletConfig.IsFollowTarget)
                {
                    /// Tạo viên đạn tương ứng
                    CreateBulletFlyResult result = KTBulletManager.Instance.CreateBulletFlyByHalfCircle(skill, caster, null, dirVector, radius, bulletVelocity, lifeTime, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);
                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    packetBuilder.Append(result.BulletID, bulletConfig.ID, casterPos, result.ToPos, null, bulletVelocity, lifeTime / 2, bulletConfig.LoopAnimation, 0, caster, false, radius, true, dirVector);
                }
                /// Nếu không chạy theo đối tượng ra chiêu
                else
                {
                    /// Tạo viên đạn tương ứng
                    CreateBulletFlyResult result = KTBulletManager.Instance.CreateBulletFlyByHalfCircle(skill, caster, casterPos, dirVector, radius, bulletVelocity, lifeTime, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);
                    /// Gửi gói tin tạo đạn đến người chơi xung quanh
                    packetBuilder.Append(result.BulletID, bulletConfig.ID, casterPos, result.ToPos, null, bulletVelocity, lifeTime / 2, bulletConfig.LoopAnimation, 0, caster, false, radius, false, dirVector);
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Thực hiện kỹ năng tạo đạn theo hình tròn xung quanh vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        /// <param name="startAngle">Góc so với điểm 0 độ của điểm bắt đầu</param>
        private static void DoSkillBulletByCircleAtPosition(SkillLevelRef skill, PropertyDictionary skillPd, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, GameObject target, BulletConfig bulletConfig, int bulletVelocity, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent, int startAngle)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Khoảng cách bay của đạn
            float distance = bulletVelocity * bulletConfig.LifeTime / 18f;

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Vị trí ban đầu phát chiêu
            int startRadius = skill.Data.Params[0];
            if (startRadius < 20)
            {
                startRadius = 20;
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                /// Chỉ hiển thị ở Client còn GS chỉ tick 1 lần damage duy nhất không
                bool clientOnly = skill.Data.Params[2] == 1;

                /// Loại này sẽ Random góc bắt đầu và từ đó tỏa ra xung quanh
                if (skill.Data.StartPoint == 3)
                {
                    startAngle = KTGlobal.GetRandomNumber(0, 360);
                }
                List<UnityEngine.Vector2> points = KTMath.GetCirclePoints(skill.Data.StartPoint == 4 ? casterPos : pos, dirVector, startAngle, startRadius, bulletCount);

                float delayEach = 0;

                int idx = -1;
                foreach (UnityEngine.Vector2 point in points)
                {
                    idx++;
                    /// Nếu là đạn đứng yên một chỗ
                    if (bulletConfig.MoveKind == 0)
                    {
                        int id = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, bulletExplodeRadius, maxTargetTouch, lifeTime, tickTimeDamage);

                        /// Gửi gói tin tạo đạn đến người chơi xung quanh
                        packetBuilder.Append(id, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, delayEach, caster, false);
                    }
                    /// Nếu là đạn bay theo đường thẳng
                    else if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5 || bulletConfig.MoveKind == 10)
                    {
                        /// Nếu đạn đuổi mục tiêu
                        if (bulletConfig.IsFollowTarget && target != null)
                        {
                            /// Vị trí xuất phát
                            UnityEngine.Vector2 sPos;

                            /// Nếu vị trí xuất phát là từ trong ra ngoài
                            if (skill.Data.StartPoint == 1 || skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6)
                            {
                                sPos = point;
                            }
                            /// Nếu vị trí xuất phát là từ ngoài vào trong
                            else
                            {
                                sPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                            }

                            /// Tạo đạn
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, sPos, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, sPos, sPos, target, bulletVelocity, 0, bulletConfig.LoopAnimation, skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6 ? 0.1f * idx : 0, caster, false);
                        }
                        /// Nếu là loại đạn bay 1 khoảng xong tiếp tục bay đến chỗ mục tiêu
                        else if (skill.Data.StartPoint == 4)
                        {
                            /// Khoảng delta bay thẳng trước
                            float linearFlyTime = skill.Data.Params[4] / 18f;
                            /// Khoảng cách bay được
                            float linearFlyDistance = linearFlyTime * bulletVelocity;

                            /// Vị trí xuất phát
                            UnityEngine.Vector2 sPos = KTMath.FindPointInVectorWithDistance(casterPos, point - casterPos, linearFlyDistance);
                            /// Vị trí đích
                            UnityEngine.Vector2 toPos = pos;

                            ///// Nếu không có mục tiêu
                            //if (target == null || target == caster || !KTLogic.IsOpposite(caster, target))
                            //{
                            //    /// Lấy mục tiêu trước đó
                            //    target = caster.LastTarget;
                            //}

                            ///// Nếu mục tiêu chưa chết và nằm trong phạm vi nhìn
                            //if (target != null && !target.IsDead() && KTGlobal.GetDistanceBetweenGameObjects(caster, target) <= 1000)
                            //{
                            //    /// Lấy vị trí mục tiêu
                            //    toPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
                            //}

                            /// Tìm vị trí không có vật cản
                            toPos = KTGlobal.FindLinearNoObsPoint(gameMap, sPos, toPos, caster.CurrentCopyMapID);

                            /// Nếu trùng vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(sPos, toPos) <= 10)
                            {
                                /// Tìm điểm mới cách 1 khoảng
                                toPos = KTMath.FindPointInVectorWithDistance(toPos, toPos - sPos, 20);
                            }

                            /// Tạo đạn với khoảng Delay để Client biểu diễn
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, sPos, toPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback, linearFlyTime);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, sPos, toPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, -linearFlyTime, caster, bulletConfig.IsComeback);
                        }
                        /// Nếu không phải đạn đuổi mục tiêu
                        else
                        {
                            /// Vị trí xuất phát
                            UnityEngine.Vector2 sPos;
                            /// Vị trí đích
                            UnityEngine.Vector2 toPos;

                            /// Nếu vị trí xuất phát là từ trong ra ngoài
                            if (skill.Data.StartPoint == 1 || skill.Data.StartPoint == 2 || skill.Data.StartPoint == 6 || target == null)
                            {
                                sPos = point;
                                toPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                            }
                            /// Nếu vị trí xuất phát là từ ngoài vào trong
                            else if (skill.Data.StartPoint == 3)
                            {
                                sPos = KTMath.FindPointInVectorWithDistance(point, point - pos, distance);
                                toPos = pos;
                            }
                            /// Nếu vị trí xuất phát là từ phía đối tượng xuất chiêu hướng về phía mục tiêu
                            else
                            {
                                /// Vector chỉ hướng giữa vị trí đối tượng xuất chiêu và vị trí ra chiêu
                                UnityEngine.Vector2 _dirVector = casterPos - pos;

                                /// Dịch chuyển tia từ vị trí ra chiêu về phía đối tượng xuất chiêu
                                sPos = KTMath.FindPointInVectorWithDistance(point, _dirVector, UnityEngine.Vector2.Distance(pos, casterPos));

                                /// Đích đến lúc này sẽ là vị trí đối tượng
                                toPos = pos;
                            }

                            /// Tìm vị trí không có vật cản
                            toPos = KTGlobal.FindLinearNoObsPoint(gameMap, sPos, toPos, caster.CurrentCopyMapID);
                            /// Nếu không trùng với vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(sPos, toPos) > 10)
                            {
                                /// Tạo đạn
                                KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, sPos, toPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                                /// Gửi gói tin tạo đạn đến người chơi xung quanh
                                packetBuilder.Append(result.BulletID, bulletConfig.ID, sPos, result.ToPos == UnityEngine.Vector2.zero ? toPos : result.ToPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, skill.Data.StartPoint == 2 ? (skill.Data.Params[4] > 0 ? skill.Data.Params[4] / 18f : 0.1f) * idx : 0, caster, bulletConfig.IsComeback);
                            }
                        }
                    }
                    delayEach += skill.Data.Params[2] / 18f;
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn kỹ năng tạo đạn theo hình chữ nhật xung quanh vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Tổng số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletByRectangleAroundPosition(SkillLevelRef skill, PropertyDictionary skillPd, GameObject caster, UnityEngine.Vector2 pos, GameObject target, UnityEngine.Vector2? dirVectorProperty, BulletConfig bulletConfig, int bulletVelocity, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Khoảng cách bay của đạn
            float distance = bulletVelocity * bulletConfig.LifeTime / 18f;

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Nếu là đạn đứng yên một chỗ
                if (bulletConfig.MoveKind == 0)
                {
                    /// Chỉ gây sát thương trong vùng ảnh hưởng
                    KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, pos, bulletExplodeRadius * 2 * bulletCount, maxTargetTouch * bulletCount, lifeTime, tickTimeDamage);
                }

                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                List<UnityEngine.Vector2> points = KTMath.GetPointsInsideRectangle(pos, bulletCount, bulletCount, bulletExplodeRadius);

                /// Thứ tự tia đạn
                int index = -1;
                float configDelayTime = skill.Data.Params[0] / 18f;
                float dTime = 0;
                foreach (UnityEngine.Vector2 point in points)
                {
                    /// Tăng thứ tự lên mỗi khi có điểm mới
                    index++;

                    /// Nếu là đạn đứng yên một chỗ
                    if (bulletConfig.MoveKind == 0)
                    {
                        /// Nếu đồng loạt xuất hiện
                        if (skill.Data.StartPoint == 1)
                        {
                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, 0, caster, false);
                        }
                        /// Nếu xuất hiện lần lượt từ ngoài vào trong
                        else if (skill.Data.StartPoint == 2)
                        {
                            float lastDTime = dTime;
                            if (index == 4 || (index + 8 - 4) % 8 == 0)
                            {
                                dTime += configDelayTime;
                            }
                            float randTime = KTGlobal.GetRandomNumber(lastDTime, dTime);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, randTime, caster, false);
                        }
                        /// Nếu xuất hiện ngẫu nhiên
                        else if (skill.Data.StartPoint == 3)
                        {
                            float randTime = KTGlobal.GetRandomNumber(0, configDelayTime);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, randTime, caster, false);
                        }
                    }
                    /// Nếu là đạn bay theo đường thẳng
                    else if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        /// Nếu đạn đuổi mục tiêu
                        if (bulletConfig.IsFollowTarget && target != null)
                        {
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, point, point, target, bulletVelocity, 0, bulletConfig.LoopAnimation, skill.Data.StartPoint == 2 ? 0.1f * index : 0, caster, false);
                        }
                        /// Nếu không phải đạn đuổi mục tiêu
                        else
                        {
                            UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(pos, dirVector, distance);
                            /// Tìm vị trí không có vật cản
                            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, pos, destPos, caster.CurrentCopyMapID);
                            /// Nếu không trùng với vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(pos, destPos) > 10)
                            {
                                KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, pos, destPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                                /// Gửi gói tin tạo đạn đến người chơi xung quanh
                                packetBuilder.Append(result.BulletID, bulletConfig.ID, pos, result.ToPos == UnityEngine.Vector2.zero ? destPos : result.ToPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, skill.Data.StartPoint == 2 ? 0.1f * index : 0, caster, bulletConfig.IsComeback);
                            }
                        }
                    }
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }

        /// <summary>
        /// Thực hiện biểu diễn kỹ năng tạo đạn theo hình thoi xung quanh vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="bulletConfig">Thiết lập đạn</param>
        /// <param name="bulletVelocity">Vận tốc đạn</param>
        /// <param name="bulletCount">Tổng số đạn</param>
        /// <param name="bulletExplodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu tối đa</param>
        /// <param name="bulletTotalRound">Tổng số lượt xuất chiêu</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        private static void DoSkillBulletByDiamondShapeAroundPosition(SkillLevelRef skill, PropertyDictionary skillPd, GameObject caster, UnityEngine.Vector2 pos, GameObject target, UnityEngine.Vector2? dirVectorProperty, BulletConfig bulletConfig, int bulletVelocity, int bulletCount, int bulletExplodeRadius, int maxTargetTouch, int bulletTotalRound, int pieceThroughTargetsPercent)
        {
            /// Thời gian duy trì
            float lifeTime = bulletConfig.LifeTime / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_lifetime_v))
            {
                lifeTime = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_lifetime_v).nValue[0] / 18f;
            }

            /// Thời gian tick gây damage liên tục
            float tickTimeDamage = bulletConfig.DamageInterval / 18f;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_dmginterval))
            {
                tickTimeDamage = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_dmginterval).nValue[0];
            }

            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            /// Vector chỉ hướng bay của đạn
            UnityEngine.Vector2 dirVector;
            /// Nếu có thiết lập hướng ban đầu
            if (dirVectorProperty.HasValue)
            {
                dirVector = dirVectorProperty.Value;
            }
            /// Nếu không có thiết lập hướng ban đầu thì sẽ căn cứ vào hướng hiện tại của đối tượng xuất chiêu
            else
            {
                dirVector = KTMath.DirectionToDirVector(caster.CurrentDir);
            }

            /// Khoảng cách bay của đạn
            float distance = bulletVelocity * bulletConfig.LifeTime / 18f;

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);

            /// <summary>
            /// Khởi tạo đạn bay
            /// </summary>
            void CreateBullets()
            {
                /// Nếu là đạn đứng yên một chỗ
                if (bulletConfig.MoveKind == 0)
                {
                    /// Chỉ gây sát thương trong vùng ảnh hưởng
                    KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, pos, bulletExplodeRadius * 2 * bulletCount, maxTargetTouch * bulletCount, lifeTime, tickTimeDamage);
                }

                /// Tạo đối tượng xây dựng gói tin gửi về Client
                BulletPacketBuilder packetBuilder = new BulletPacketBuilder();

                /// Tạo các điểm trong vùng hình chữ nhật
                List<UnityEngine.Vector2> points = KTMath.GetPointsInsideRectangle(pos, bulletCount, bulletCount, bulletExplodeRadius);
                /// Xoay hình chữ nhật thành hình thoi
                points = KTMath.RotateMatrix(points, 45, pos);

                /// Thứ tự tia đạn
                int index = -1;
                float configDelayTime = skill.Data.Params[0] / 18f;
                float dTime = 0;
                foreach (UnityEngine.Vector2 point in points)
                {
                    /// Tăng thứ tự lên mỗi khi có điểm mới
                    index++;

                    /// Nếu là đạn đứng yên một chỗ
                    if (bulletConfig.MoveKind == 0)
                    {
                        /// Nếu đồng loạt xuất hiện
                        if (skill.Data.StartPoint == 1)
                        {
                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, 0, caster, false);
                        }
                        /// Nếu xuất hiện lần lượt từ ngoài vào trong
                        else if (skill.Data.StartPoint == 2)
                        {
                            float lastDTime = dTime;
                            if (index == 4 || (index + 8 - 4) % 8 == 0)
                            {
                                dTime += configDelayTime;
                            }
                            float randTime = KTGlobal.GetRandomNumber(lastDTime, dTime);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, randTime, caster, false);
                        }
                        /// Nếu xuất hiện ngẫu nhiên
                        else if (skill.Data.StartPoint == 3)
                        {
                            float randTime = KTGlobal.GetRandomNumber(0, configDelayTime);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(-1, bulletConfig.ID, point, point, null, 0, bulletConfig.LifeTime / 18f, bulletConfig.LoopAnimation, randTime, caster, false);
                        }
                    }
                    /// Nếu là đạn bay theo đường thẳng
                    else if (bulletConfig.MoveKind == 1 || bulletConfig.MoveKind == 5)
                    {
                        /// Nếu đạn đuổi mục tiêu
                        if (bulletConfig.IsFollowTarget && target != null)
                        {
                            KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, point, target, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent);

                            /// Gửi gói tin tạo đạn đến người chơi xung quanh
                            packetBuilder.Append(result.BulletID, bulletConfig.ID, point, point, target, bulletVelocity, 0, bulletConfig.LoopAnimation, 0, caster, false);
                        }
                        /// Nếu không phải đạn đuổi mục tiêu
                        else
                        {
                            UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(pos, dirVector, distance);
                            /// Tìm vị trí không có vật cản
                            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, pos, destPos, caster.CurrentCopyMapID);
                            /// Nếu không trùng với vị trí xuất phát
                            if (UnityEngine.Vector2.Distance(pos, destPos) > 10)
                            {
                                KTBulletManager.CreateBulletFlyResult result = KTBulletManager.Instance.CreateBullet(skill, dirVectorProperty, caster, pos, destPos, bulletVelocity, bulletExplodeRadius, maxTargetTouch, pieceThroughTargetsPercent, bulletConfig.IsComeback);

                                /// Gửi gói tin tạo đạn đến người chơi xung quanh
                                packetBuilder.Append(result.BulletID, bulletConfig.ID, pos, result.ToPos == UnityEngine.Vector2.zero ? destPos : result.ToPos, null, bulletVelocity, 0, bulletConfig.LoopAnimation, 0, caster, bulletConfig.IsComeback);
                            }
                        }
                    }
                }

                /// Gửi gói tin về Client
                KT_TCPHandler.SendCreateBullets(caster, packetBuilder.Build());
            }

            /// Tạo viên đạn tương ứng
            CreateBullets();

            /// Nếu số lượt ra chiêu lớn hơn 1 thì chạy Timer
            if (bulletTotalRound > 1)
            {
                /// Thời gian mỗi lần tạo đạn
                float periodActivation = Math.Max(skill.Data.BulletRoundTime / 18f, SkillDataEx.BulletSpawnPeriod);
                /// Thời gian duy trì
                float interval = periodActivation * (bulletTotalRound - 1) + 0.1f;
                /// Tạo luồng thực thi
                KTSkillManager.SetSchedule(periodActivation, interval, () => {
                    /// Tạo viên đạn tương ứng
                    CreateBullets();
                });
            }
        }
    }
}
