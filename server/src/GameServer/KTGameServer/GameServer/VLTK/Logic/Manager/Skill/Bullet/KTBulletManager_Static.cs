using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        /// <summary>
        /// Tạo viên đạn nổ tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí xuất hiện đạn</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu nổ tối đa</param>
        /// <param name="delay">Thời gian delay trước khi tạo</param>
        /// <param name="targetType">Loại mục tiêu</param>
        /// <returns></returns>
        public int CreateBullet(SkillLevelRef skill, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, int explodeRadius, int maxTargetTouch, float delay = 0f)
        {
            if (caster == null || skill == null)
            {
                return -1;
            }

            try
            {
                /// Tạo mới viên đạn
                Bullet bullet = new Bullet()
                {
                    Skill = skill,
                    DirVectorProperty = dirVectorProperty,
                    Caster = caster,
                    FromPos = pos,
                    ToPos = pos,
                    MaxLifeTime = 0,
                    ChaseTarget = null,
                    IsTrap = false,
                    MaxTargetTouch = maxTargetTouch,
                    Velocity = 0,
                    TargetType = skill.Data.TargetType,
                };

                if (delay <= 0)
                {
                    /// Thực hiện biểu diễn đạn nổ tại vị trí
                    this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, true, out GameObject vanishTarget);

                    /// Thực hiện kỹ năng tan biến
                    KTSkillManager.BulletVanished(skill, caster, pos, vanishTarget, dirVectorProperty);
                }
                else
                {
                    this.SetTimeout(delay, () => {
                        if (caster == null || skill == null)
                        {
                            return;
                        }

                        /// Thực hiện biểu diễn đạn nổ tại vị trí
                        this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, true, out GameObject vanishTarget);

                        /// Thực hiện kỹ năng tan biến
                        KTSkillManager.BulletVanished(skill, caster, pos, vanishTarget, dirVectorProperty);
                    }, caster);
                }

                /// Trả về kết quả là ID đạn
                return bullet.ID;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Tạo viên đạn nổ tại vị trí chỉ định nhiều lần đến khi kết thúc thời gian tồn tại
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí xuất hiện đạn</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu nổ tối đa</param>
        /// <param name="lifeTime">Thời gian tồn tại</param>
        /// <param name="tickTime">Thời gian duy trì</param>
        /// <param name="delay">Thời gian delay trước khi tạo</param>
        /// <param name="targetType">Loại mục tiêu</param>
        /// <returns></returns>
        public int CreateBullet(SkillLevelRef skill, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, int explodeRadius, int maxTargetTouch, float lifeTime, float tickTime, float delay = 0f)
        {
            if (caster == null || skill == null)
            {
                return -1;
            }

            try
            {
                /// Tăng thêm một lượng nhỏ đảm bảo vẫn thực thi trong trường hợp lifeTime chia hết cho tickTime
                lifeTime += 0.1f;

                /// Tạo mới viên đạn
                Bullet bullet = new Bullet()
                {
                    Skill = skill,
                    DirVectorProperty = dirVectorProperty,
                    Caster = caster,
                    FromPos = pos,
                    ToPos = pos,
                    MaxLifeTime = lifeTime,
                    ChaseTarget = null,
                    IsTrap = false,
                    MaxTargetTouch = maxTargetTouch,
                    Velocity = 0,
                    TargetType = skill.Data.TargetType,
                };

                /// Nếu có thời gian kích hoạt liên tục
                if (tickTime > 0)
                {
                    /// Tạo luồng bay
                    BulletTimer bulletTimer = new BulletTimer()
                    {
                        Bullet = bullet,
                        MaxLifeTime = Math.Min(20000, (int) (lifeTime * 1000)),
                        PeriodTicks = (int) (tickTime * 1000),
                    };
                    bulletTimer.Start = () => {
                        if (caster == null || skill == null)
                        {
                            /// Hủy đạn vì toác
                            bulletTimer.Stop();
                            return;
                        }

                        /// Reset số mục tiêu đã chạm
                        bullet.TouchTargets.Clear();

                        /// Thực hiện biểu diễn đạn nổ tại vị trí
                        this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, false, out GameObject vanishTarget);
                        bulletTimer.VanishTarget = vanishTarget;
                    };
                    bulletTimer.Tick = () => {
                        if (caster == null || skill == null)
                        {
                            /// Hủy đạn vì toác
                            bulletTimer.Stop();
                            return;
                        }

                        /// Reset số mục tiêu đã chạm
                        bullet.TouchTargets.Clear();

                        /// Thực hiện biểu diễn đạn nổ tại vị trí
                        this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, false, out GameObject vanishTarget);
                        bulletTimer.VanishTarget = vanishTarget;
                    };
                    bulletTimer.Destroy = () => {
                        if (caster == null || skill == null)
                        {
                            /// Hủy đạn vì toác
                            bulletTimer.Stop();
                            return;
                        }

                        /// Thực hiện kỹ năng tan biến
                        KTSkillManager.BulletVanished(skill, caster, pos, bulletTimer.VanishTarget, dirVectorProperty);
                    };
                    bulletTimer.Error = (ex) => {
                        LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                    };

                    if (delay <= 0)
                    {
                        bulletTimer.Run();
                    }
                    else
                    {
                        this.SetTimeout(delay, () => {
                            if (caster == null || skill == null)
                            {
                                return;
                            }

                            bulletTimer.Run();
                        }, caster);
                    }
                }
                /// Nếu không kích hoạt liên tục thì thực hiện hàm Start luôn và không làm gì nữa
                else
                {
                    if (delay <= 0)
                    {
                        /// Thực hiện biểu diễn đạn nổ tại vị trí
                        this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, false, out GameObject vanishTarget);

                        /// Thực hiện kỹ năng tan biến
                        KTSkillManager.BulletVanished(skill, caster, pos, vanishTarget, dirVectorProperty);
                    }
                    else
                    {
                        this.SetTimeout(delay, () => {
                            if (caster == null || skill == null)
                            {
                                return;
                            }

                            /// Thực hiện biểu diễn đạn nổ tại vị trí
                            this.DoStaticBulletLogic(bullet, pos, maxTargetTouch, explodeRadius, false, out GameObject vanishTarget);

                            /// Thực hiện kỹ năng tan biến
                            KTSkillManager.BulletVanished(skill, caster, pos, vanishTarget, dirVectorProperty);
                        }, caster);
                    }
                }

                /// Trả về kết quả là ID đạn
                return bullet.ID;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return -1;
            }
        }
    }
}
