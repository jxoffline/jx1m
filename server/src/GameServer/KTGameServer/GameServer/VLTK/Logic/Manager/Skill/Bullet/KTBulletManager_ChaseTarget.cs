using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        /// <summary>
        /// Thực hiện đạn bay đuổi theo mục tiêu
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="target">Mục tiêu đuổi theo</param>
        /// <param name="velocity">Vận tốc bay</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        /// <param name="tickDistance">Khoảng cách mỗi lần thực thi hàm Tick</param>
        /// <param name="TickFunction">Hàm thực thi mỗi khoảng</param>
        /// <param name="targetType">Loại mục tiêu</param>
        /// <returns></returns>
        public CreateBulletFlyResult CreateBullet(SkillLevelRef skill, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 fromPos, GameObject target, int velocity, int explodeRadius, int maxTargetTouch, int pieceThroughTargetsPercent, float tickDistance = -1, Action<UnityEngine.Vector2> TickFunction = null)
        {
            if (caster == null || target == null || skill == null)
            {
                return new CreateBulletFlyResult()
                {
                    BulletID = -1,
                    ToPos = UnityEngine.Vector2.zero,
                };
            }

            try
            {
                /// Tính thời gian bay tối đa của đạn
                float lifeTime = KTBulletManager.DefaultBulletLifeTime;

                /// Tính toán lấy giá trị xuyên suốt mục tiêu
                bool pieceThroughTargets = false;
                if (pieceThroughTargetsPercent > 0)
                {
                    int rand = KTGlobal.GetRandomNumber(1, 100);
                    pieceThroughTargets = pieceThroughTargetsPercent >= rand;
                }

                /// Tạo mới viên đạn
                Bullet bullet = new Bullet()
                {
                    Skill = skill,
                    DirVectorProperty = dirVectorProperty,
                    Caster = caster,
                    FromPos = fromPos,
                    ChaseTarget = target,
                    IsTrap = false,
                    MaxLifeTime = lifeTime,
                    Velocity = velocity,
                    MaxTargetTouch = maxTargetTouch,
                    TargetType = skill.Data.TargetType,
                    PirceThroughTargets = pieceThroughTargets,
                    PieceThroughTargetsCount = 0,
                };


                /// Tạo luồng bay
                BulletTimer bulletTimer = new BulletTimer()
                {
                    Bullet = bullet,
                    MaxLifeTime = Math.Min(5000, (int) (lifeTime * 1000)),
                    PeriodTicks = (int) (KTBulletManager.DefaultBulletChaseTargetTick * 1000),
                    LastTickPos = fromPos,
                };
                /*bulletTimer.Start = () => */
                {
                    /// Cập nhật vị trí xuất phát của đạn
                    bulletTimer.CurrentPos = fromPos;
                    /// Thiết lập số mục tiêu đã chạm vào
                    bulletTimer.TotalTouchTarget = 0;

                    /// Thực hiện hàm Tick ngay vị trí xuất phát
                    TickFunction?.Invoke(fromPos);

                    /// Vị trí hiện tại của mục tiêu
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);

                    /// Khoảng cách đến mục tiêu
                    float distanceToTarget = UnityEngine.Vector2.Distance(bulletTimer.CurrentPos, targetPos);

                    /// Khoảng dịch được trong thời gian deltaT
                    float distance = velocity * KTBulletManager.DefaultBulletChaseTargetTick * 1.1f;

                    /// Vector chỉ hướng bay tới chỗ mục tiêu
                    UnityEngine.Vector2 dirVector = targetPos - bulletTimer.CurrentPos;

                    /// Cập nhật hướng bay lần trước
                    bulletTimer.LastDirVector = dirVector;

                    /// Vị trí tiếp theo của đạn
                    UnityEngine.Vector2 nextPos;

                    /// Nếu khoảng dịch deltaT lớn hơn khoảng cách từ vị trí hiện tại tới chỗ mục tiêu thì Set luôn vị trí đích là chỗ mục tiêu
                    if (distance >= distanceToTarget)
                    {
                        nextPos = targetPos;
                    }
                    /// Nếu chưa thể chạm đến mục tiêu trong thời gian này thì dịch lên một đoạn deltaT tương ứng
                    else
                    {
                        nextPos = KTMath.FindPointInVectorWithDistance(bulletTimer.CurrentPos, dirVector, distance);
                    }

                    /// Thực hiện biểu diễn đạn bay
                    this.DoBulletFlyLogic(bulletTimer, bulletTimer.CurrentPos, nextPos, explodeRadius, maxTargetTouch, out UnityEngine.Vector2? stopPos);

                    /// Kiểm tra và thực hiện hàm Tick với mỗi khoảng tương ứng
                    if (tickDistance > 0 && TickFunction != null)
                    {
                        float flyDistance = UnityEngine.Vector2.Distance(bulletTimer.LastTickPos, nextPos);
                        int totalPoints = (int) (flyDistance / tickDistance);
                        UnityEngine.Vector2 tickDirVector = nextPos - bulletTimer.LastTickPos;
                        UnityEngine.Vector2 lastTickPos = bulletTimer.LastTickPos;
                        for (int i = 1; i <= totalPoints; i++)
                        {
                            UnityEngine.Vector2 tickPos = KTMath.FindPointInVectorWithDistance(lastTickPos, tickDirVector, tickDistance * i);
                            float time = UnityEngine.Vector2.Distance(lastTickPos, tickPos) / bullet.Velocity;
                            this.SetTimeout(time, () => {
                                TickFunction?.Invoke(tickPos);
                            }, caster);
                            bulletTimer.LastTickPos = tickPos;
                        }
                    }

                    /// Nếu có giá trị vị trí đích
                    if (stopPos.HasValue)
                    {
                        float distanceToVanish = UnityEngine.Vector2.Distance(bulletTimer.CurrentPos, stopPos.Value);
                        float timeToVanish = distanceToVanish / bullet.Velocity;
                        this.SetTimeout(timeToVanish, () => {
                            UnityEngine.Vector2 vanishPos = bulletTimer.VanishTarget == null ? stopPos.Value : new UnityEngine.Vector2((int) bulletTimer.VanishTarget.CurrentPos.X, (int) bulletTimer.VanishTarget.CurrentPos.Y);
                            /// Thực hiện kỹ năng tan biến
                            KTSkillManager.BulletVanished(skill, caster, vanishPos, bulletTimer.VanishTarget, bulletTimer.LastDirVector);
                        }, caster);
                        /// Nếu vị trí đầu và vị trí đích qúa sát nhau
                        if (UnityEngine.Vector2.Distance(fromPos, stopPos.Value) < 20)
                        {
                            stopPos = KTMath.FindPointInVectorWithDistance(fromPos, dirVector, 20);
                        }
                        return new CreateBulletFlyResult()
                        {
                            BulletID = bullet.ID,
                            ToPos = stopPos.Value,
                        };
                    }

                    /// Cập nhật vị trí mới của đạn
                    bulletTimer.CurrentPos = nextPos;

                    /*
                    /// Nếu khoảng dịch deltaT lớn hơn khoảng cách từ vị trí hiện tại tới chỗ mục tiêu thì hủy luồng
                    if (distance >= distanceToTarget)
                    {
                        /// Hủy đạn vì đã chạm mục tiêu rồi
                        this.timer.KillTimer(bulletTimer);
                    }
                    */
                };
                bulletTimer.Tick = () => {
                    /// Toác đéo gì đó
                    if (caster == null || target == null || skill == null)
                    {
                        /// Hủy đạn vì toác
                        bulletTimer.Stop();
                        return;
                    }

                    /// Nếu đã quá thời gian tồn tại của đạn
                    if (bulletTimer.LifeTime / 1000f >= bullet.MaxLifeTime)
                    {
                        return;
                    }

                    /// Vị trí hiện tại của mục tiêu
                    UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);

                    /// Khoảng cách đến mục tiêu
                    float distanceToTarget = UnityEngine.Vector2.Distance(bulletTimer.CurrentPos, targetPos);

                    /// Khoảng dịch được trong thời gian deltaT
                    float distance = velocity * KTBulletManager.DefaultBulletChaseTargetTick * 1.1f;

                    /// Vector chỉ hướng bay tới chỗ mục tiêu
                    UnityEngine.Vector2 dirVector = targetPos - bulletTimer.CurrentPos;

                    /// Cập nhật hướng bay lần trước
                    bulletTimer.LastDirVector = dirVector;

                    /// Vị trí tiếp theo của đạn
                    UnityEngine.Vector2 nextPos;

                    /// Nếu khoảng dịch deltaT lớn hơn khoảng cách từ vị trí hiện tại tới chỗ mục tiêu thì Set luôn vị trí đích là chỗ mục tiêu
                    if (distance >= distanceToTarget)
                    {
                        nextPos = targetPos;
                    }
                    /// Nếu chưa thể chạm đến mục tiêu trong thời gian này thì dịch lên một đoạn deltaT tương ứng
                    else
                    {
                        nextPos = KTMath.FindPointInVectorWithDistance(bulletTimer.CurrentPos, dirVector, distance);
                    }

                    /// Thực hiện biểu diễn đạn bay
                    this.DoBulletFlyLogic(bulletTimer, bulletTimer.CurrentPos, nextPos, explodeRadius, maxTargetTouch, out _);

                    /// Kiểm tra và thực hiện hàm Tick với mỗi khoảng tương ứng
                    if (tickDistance > 0 && TickFunction != null)
                    {
                        float flyDistance = UnityEngine.Vector2.Distance(bulletTimer.LastTickPos, nextPos);
                        int totalPoints = (int) (flyDistance / tickDistance);
                        UnityEngine.Vector2 tickDirVector = nextPos - bulletTimer.LastTickPos;
                        UnityEngine.Vector2 lastTickPos = bulletTimer.LastTickPos;
                        for (int i = 1; i <= totalPoints; i++)
                        {
                            UnityEngine.Vector2 tickPos = KTMath.FindPointInVectorWithDistance(lastTickPos, tickDirVector, tickDistance * i);
                            float time = UnityEngine.Vector2.Distance(lastTickPos, tickPos) / bullet.Velocity;
                            this.SetTimeout(time, () => {
                                TickFunction?.Invoke(tickPos);
                            }, caster);
                            bulletTimer.LastTickPos = tickPos;
                        }
                    }

                    /// Cập nhật vị trí mới của đạn
                    bulletTimer.CurrentPos = nextPos;

                    /// Nếu khoảng dịch deltaT lớn hơn khoảng cách từ vị trí hiện tại tới chỗ mục tiêu thì hủy luồng
                    if (distance >= distanceToTarget)
                    {
                        /// Hủy đạn vì đã chạm mục tiêu rồi
                        bulletTimer.Stop();
                    }
                };
                bulletTimer.Destroy = () => {
                    /// Toác đéo gì đó
                    if (caster == null || target == null || skill == null)
                    {
                        return;
                    }

                    /// Thực hiện kỹ năng tan biến
                    KTSkillManager.BulletVanished(skill, caster, bulletTimer.CurrentPos, bulletTimer.VanishTarget, bulletTimer.LastDirVector);
                };
                bulletTimer.Error = (ex) => {
                    LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                };
                bulletTimer.Run();

                /// Trả về kết quả là ID đạn
                return new CreateBulletFlyResult()
                {
                    BulletID = bullet.ID,
                    ToPos = UnityEngine.Vector2.zero,
                };
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return new CreateBulletFlyResult()
                {
                    BulletID = -1,
                    ToPos = UnityEngine.Vector2.zero,
                };
            }
        }
    }
}
