using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        /// <summary>
        /// Tạo viên đạn bay theo đường tròn chiều thuận từ phía đối tượng xuất chiêu theo hướng tương ứng.
        /// <para></para>
        /// Nếu đạn xuyên suốt mục tiêu thì sẽ tự động thiết lập có thể nổ nhiều lần trên cùng 1 mục tiêu. Có thể tùy biến sau
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="caster"></param>
        /// <param name="startPosProperty"></param>
        /// <param name="dirVector"></param>
        /// <param name="radius"></param>
        /// <param name="velocity"></param>
        /// <param name="lifeTime"></param>
        /// <param name="explodeRadius"></param>
        /// <param name="maxTargetTouch"></param>
        /// <param name="pieceThroughTargetsPercent"></param>
        /// <param name="tickDistance"></param>
        /// <param name="TickFunction"></param>
        /// <returns></returns>
        public CreateBulletFlyResult CreateBulletFlyByCircle(SkillLevelRef skill, GameObject caster, UnityEngine.Vector2? startPosProperty, UnityEngine.Vector2 dirVector, float radius, int velocity, float lifeTime, int explodeRadius, int maxTargetTouch, int pieceThroughTargetsPercent, float tickDistance = -1, Action<UnityEngine.Vector2> TickFunction = null)
        {
            /// Toác
            if (caster == null || skill == null)
            {
                return new CreateBulletFlyResult()
                {
                    BulletID = -1,
                    ToPos = UnityEngine.Vector2.zero,
                };
            }

            try
            {
                /// Đường tròn tương ứng
                KTMath.Circle circle = new KTMath.Circle()
                {
                    A = 0,
                    B = 0,
                    R = radius,
                };

                /// Vị trí xuất phát
                UnityEngine.Vector2 fromPos = startPosProperty != null ? startPosProperty.Value : new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                /// Đoạn đường sẽ đi được
                float distance = velocity * lifeTime;

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
                    DirVectorProperty = null,
                    Caster = caster,
                    FromPos = default,
                    ToPos = default,
                    ChaseTarget = null,
                    IsTrap = false,
                    MaxLifeTime = lifeTime,
                    Velocity = velocity,
                    MaxTargetTouch = maxTargetTouch,
                    TargetType = skill.Data.TargetType,
                    PirceThroughTargets = pieceThroughTargets,
                    PieceThroughTargetsCount = 0,
                    CircleMove = true,
                    MultipleTouchTarget = pieceThroughTargets,
                };

                /// Tạo luồng bay
                BulletTimer bulletTimer = new BulletTimer()
                {
                    Bullet = bullet,
                    MaxLifeTime = Math.Min(5000, (int) (lifeTime * 1000)),
                    PeriodTicks = (int) (KTBulletManager.DefaultBulletTick * 1000),
                    LastTickPos = fromPos,
                    CurrentPos = fromPos,
                };
                /*bulletTimer.Start = () => */
                try
                {
                    /// Cập nhật vị trí xuất phát của đạn
                    bulletTimer.CurrentPos = fromPos;
                    /// Thiết lập số mục tiêu đã chạm vào
                    bulletTimer.TotalTouchTarget = 0;

                    /// Thực hiện hàm Tick ngay vị trí xuất phát
                    TickFunction?.Invoke(fromPos);

                    /// Vị trí tiếp theo của đạn trong khoảng thời gian kế
                    float nextLifeTime = bulletTimer.LifeTime + KTBulletManager.DefaultBulletTick * 1000;
                    /// Phần trăm thời gian đã trôi qua ở giai đoạn tiếp theo
                    float nextPercent = nextLifeTime / bulletTimer.MaxLifeTime;
                    nextPercent = Math.Min(1f, nextPercent);

                    /// Độ dài cung tròn đi được
                    float currentDistance = distance * nextPercent;
                    /// Góc quay tương ứng đi được
                    float angle = 180 * currentDistance / (UnityEngine.Mathf.PI * radius);
                    /// Vector hướng từ tâm đến vị trí hiện tại = Vector hướng từ tâm đến vị trí xuất phát, quay 1 góc tương ứng vừa tìm được
                    UnityEngine.Vector2 centerCurrentVector = KTMath.RotateVector(dirVector, angle);
                    /// Đảo chiều lại
                    centerCurrentVector = -centerCurrentVector;
                    /// Vị trí tiếp theo của đạn theo quỹ đạo đường tròn
                    UnityEngine.Vector2 nextPos = KTMath.FindPointInVectorWithDistance(KTMath.FindPointInVectorWithDistance(circle.Center, dirVector, radius), centerCurrentVector, radius);
                    /// Nếu có vị trí bắt đầu
                    if (startPosProperty != null)
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí xuất phát
                        nextPos += startPosProperty.Value;
                    }
                    /// Nếu không có vị trí bắt đầu
                    else
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí hiện tại của đối tượng ra chiêu
                        nextPos += new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
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

                    //KTSkillManager.UseSkill(caster, null, ((KPlayer) caster).Skills.GetSkillLevelRef(10104), true, nextPos);
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
                bulletTimer.Tick = () => {
                   // Console.WriteLine("TICK " + bulletTimer.PeriodTicks);
                    /// Toác đéo gì đó
                    if (caster == null || skill == null)
                    {
                        /// Hủy đạn vì toác
                        bulletTimer.Stop();
                        return;
                    }

                    /// Phần trăm thời gian trôi qua từ vị trí hiện tại đến vị trí ở Frame tiếp theo
                    float nextLifeTime = bulletTimer.LifeTime + KTBulletManager.DefaultBulletTick * 1000;
                    float nextPercent = nextLifeTime / bulletTimer.MaxLifeTime;
                    nextPercent = Math.Min(1f, nextPercent);

                    /// Độ dài cung tròn đi được
                    float currentDistance = distance * nextPercent;
                    /// Góc quay tương ứng đi được
                    float angle = 180 * currentDistance / (UnityEngine.Mathf.PI * radius);
                    /// Vector hướng từ tâm đến vị trí hiện tại = Vector hướng từ tâm đến vị trí xuất phát, quay 1 góc tương ứng vừa tìm được
                    UnityEngine.Vector2 centerCurrentVector = KTMath.RotateVector(dirVector, angle);
                    /// Đảo chiều lại
                    centerCurrentVector = -centerCurrentVector;
                    /// Vị trí tiếp theo của đạn
                    UnityEngine.Vector2 nextPos = KTMath.FindPointInVectorWithDistance(KTMath.FindPointInVectorWithDistance(circle.Center, dirVector, radius), centerCurrentVector, radius);
                    /// Nếu có vị trí bắt đầu
                    if (startPosProperty != null)
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí xuất phát
                        nextPos += startPosProperty.Value;
                    }
                    /// Nếu không có vị trí bắt đầu
                    else
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí hiện tại của đối tượng ra chiêu
                        nextPos += new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
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

                    //KTSkillManager.UseSkill(caster, null, ((KPlayer) caster).Skills.GetSkillLevelRef(10104), true, nextPos);

                    /// Nếu đã quá thời gian tồn tại tối đa của đạn
                    if (nextPercent >= 1f)
                    {
                        /// Xóa luồng thực thi đạn
                        bulletTimer.Stop();
                        return;
                    }
                };
                bulletTimer.Destroy = () => {
                    /// Toác đéo gì đó
                    if (caster == null || skill == null)
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
                    ToPos = default,
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

        /// <summary>
        /// Tạo viên đạn bay theo nửa đường tròn chiều thuận từ phía đối tượng xuất chiêu theo hướng tương ứng.
        /// <para></para>
        /// Nếu đạn xuyên suốt mục tiêu thì sẽ tự động thiết lập có thể nổ nhiều lần trên cùng 1 mục tiêu. Có thể tùy biến sau
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="caster"></param>
        /// <param name="startPosProperty"></param>
        /// <param name="dirVector"></param>
        /// <param name="radius"></param>
        /// <param name="velocity"></param>
        /// <param name="lifeTime"></param>
        /// <param name="explodeRadius"></param>
        /// <param name="maxTargetTouch"></param>
        /// <param name="pieceThroughTargetsPercent"></param>
        /// <param name="tickDistance"></param>
        /// <param name="TickFunction"></param>
        /// <returns></returns>
        public CreateBulletFlyResult CreateBulletFlyByHalfCircle(SkillLevelRef skill, GameObject caster, UnityEngine.Vector2? startPosProperty, UnityEngine.Vector2 dirVector, float radius, int velocity, float lifeTime, int explodeRadius, int maxTargetTouch, int pieceThroughTargetsPercent, float tickDistance = -1, Action<UnityEngine.Vector2> TickFunction = null)
        {
            /// Toác
            if (caster == null || skill == null)
            {
                return new CreateBulletFlyResult()
                {
                    BulletID = -1,
                    ToPos = UnityEngine.Vector2.zero,
                };
            }

            try
            {
                /// Đường tròn tương ứng
                KTMath.Circle circle = new KTMath.Circle()
                {
                    A = 0,
                    B = 0,
                    R = radius,
                };

                /// Vị trí xuất phát
                UnityEngine.Vector2 fromPos = startPosProperty != null ? startPosProperty.Value : new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
                /// Đoạn đường sẽ đi được
                float distance = velocity * lifeTime;

                /// Thời gian tồn tại tối đa thực
                int maxLifeTime = Math.Min(5000, (int) (lifeTime * 1000));

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
                    DirVectorProperty = null,
                    Caster = caster,
                    FromPos = default,
                    ToPos = default,
                    ChaseTarget = null,
                    IsTrap = false,
                    MaxLifeTime = lifeTime,
                    Velocity = velocity,
                    MaxTargetTouch = maxTargetTouch,
                    TargetType = skill.Data.TargetType,
                    PirceThroughTargets = pieceThroughTargets,
                    PieceThroughTargetsCount = 0,
                    CircleMove = true,
                    MultipleTouchTarget = pieceThroughTargets,
                };

                /// Tạo luồng bay
                BulletTimer bulletTimer = new BulletTimer()
                {
                    Bullet = bullet,
                    /// Chia nửa vì chỉ bay nửa vòng tròn
                    MaxLifeTime = maxLifeTime / 2,
                    PeriodTicks = (int) (KTBulletManager.DefaultBulletTick * 1000),
                    LastTickPos = fromPos,
                    CurrentPos = fromPos,
                };
                /*bulletTimer.Start = () => */
                try
                {
                    /// Cập nhật vị trí xuất phát của đạn
                    bulletTimer.CurrentPos = fromPos;
                    /// Thiết lập số mục tiêu đã chạm vào
                    bulletTimer.TotalTouchTarget = 0;

                    /// Thực hiện hàm Tick ngay vị trí xuất phát
                    TickFunction?.Invoke(fromPos);

                    /// Vị trí tiếp theo của đạn trong khoảng thời gian kế
                    float nextLifeTime = bulletTimer.LifeTime + KTBulletManager.DefaultBulletTick * 1000;
                    /// Phần trăm thời gian đã trôi qua ở giai đoạn tiếp theo
                    float nextPercent = nextLifeTime / maxLifeTime;
                    nextPercent = Math.Min(1f, nextPercent);

                    /// Độ dài cung tròn đi được
                    float currentDistance = distance * nextPercent;
                    /// Góc quay tương ứng đi được
                    float angle = 180 * currentDistance / (UnityEngine.Mathf.PI * radius);
                    /// Vector hướng từ tâm đến vị trí hiện tại = Vector hướng từ tâm đến vị trí xuất phát, quay 1 góc tương ứng vừa tìm được
                    UnityEngine.Vector2 centerCurrentVector = KTMath.RotateVector(dirVector, angle);
                    /// Đảo chiều lại
                    centerCurrentVector = -centerCurrentVector;
                    /// Vị trí tiếp theo của đạn theo quỹ đạo đường tròn
                    UnityEngine.Vector2 nextPos = KTMath.FindPointInVectorWithDistance(KTMath.FindPointInVectorWithDistance(circle.Center, dirVector, radius), centerCurrentVector, radius);
                    /// Nếu có vị trí bắt đầu
                    if (startPosProperty != null)
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí xuất phát
                        nextPos += startPosProperty.Value;
                    }
                    /// Nếu không có vị trí bắt đầu
                    else
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí hiện tại của đối tượng ra chiêu
                        nextPos += new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
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

                    //KTSkillManager.UseSkill(caster, null, ((KPlayer) caster).Skills.GetSkillLevelRef(10104), true, nextPos);
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
                bulletTimer.Tick = () => {
                    //Console.WriteLine("TICK " + bulletTimer.PeriodTicks);
                    /// Toác đéo gì đó
                    if (caster == null || skill == null)
                    {
                        /// Hủy đạn vì toác
                        bulletTimer.Stop();
                        return;
                    }

                    /// Phần trăm thời gian trôi qua từ vị trí hiện tại đến vị trí ở Frame tiếp theo
                    float nextLifeTime = bulletTimer.LifeTime + KTBulletManager.DefaultBulletTick * 1000;
                    float nextPercent = nextLifeTime / maxLifeTime;
                    nextPercent = Math.Min(1f, nextPercent);

                    /// Độ dài cung tròn đi được
                    float currentDistance = distance * nextPercent;
                    /// Góc quay tương ứng đi được
                    float angle = 180 * currentDistance / (UnityEngine.Mathf.PI * radius);
                    /// Vector hướng từ tâm đến vị trí hiện tại = Vector hướng từ tâm đến vị trí xuất phát, quay 1 góc tương ứng vừa tìm được
                    UnityEngine.Vector2 centerCurrentVector = KTMath.RotateVector(dirVector, angle);
                    /// Đảo chiều lại
                    centerCurrentVector = -centerCurrentVector;
                    /// Vị trí tiếp theo của đạn
                    UnityEngine.Vector2 nextPos = KTMath.FindPointInVectorWithDistance(KTMath.FindPointInVectorWithDistance(circle.Center, dirVector, radius), centerCurrentVector, radius);
                    /// Nếu có vị trí bắt đầu
                    if (startPosProperty != null)
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí xuất phát
                        nextPos += startPosProperty.Value;
                    }
                    /// Nếu không có vị trí bắt đầu
                    else
                    {
                        /// Vị trí thực sẽ cộng thêm vị trí hiện tại của đối tượng ra chiêu
                        nextPos += new UnityEngine.Vector2((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y);
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

                    //KTSkillManager.UseSkill(caster, null, ((KPlayer) caster).Skills.GetSkillLevelRef(10104), true, nextPos);

                    /// Nếu đã quá thời gian tồn tại tối đa của đạn
                    if (nextPercent >= 1f)
                    {
                        /// Xóa luồng thực thi đạn
                        bulletTimer.Stop();
                        return;
                    }
                };
                bulletTimer.Destroy = () => {
                    /// Toác đéo gì đó
                    if (caster == null || skill == null)
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
                    ToPos = default,
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
