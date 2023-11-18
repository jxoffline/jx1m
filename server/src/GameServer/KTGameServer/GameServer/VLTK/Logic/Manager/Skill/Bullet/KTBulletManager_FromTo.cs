using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý đạn bay
    /// </summary>
    public partial class KTBulletManager
    {
        /// <summary>
        /// Tạo viên đạn bay từ vị trí FromPos đến vị trí ToPos
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="toPos">Vị trí kết thúc</param>
        /// <param name="velocity">Vận tốc</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="maxTargetTouch">Số mục tiêu chạm phải tối đa</param>
        /// <param name="pieceThroughTargetsPercent">Tỷ lệ xuyên suốt mục tiêu</param>
        /// <param name="comeback">Quay trở lại vị trí ban đầu</param>
        /// <param name="tickDistance">Khoảng cách mỗi lần thực thi hàm Tick</param>
        /// <param name="TickFunction">Hàm thực thi mỗi khoảng</param>
        /// <param name="delay">Khoảng delay</param>
        public CreateBulletFlyResult CreateBullet(SkillLevelRef skill, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, int velocity, int explodeRadius, int maxTargetTouch, int pieceThroughTargetsPercent, bool comeback, float tickDistance = -1, Action<UnityEngine.Vector2> TickFunction = null, float delay = 0f)
        {
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
                /// Tính Vector chỉ phương
                UnityEngine.Vector2 dirVector = toPos - fromPos;

                /// Tính quãng đường bay
                float distance = UnityEngine.Vector2.Distance(fromPos, toPos);

                /// Tính thời gian bay tối đa của đạn
                float lifeTime = distance / velocity;

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
                    ToPos = toPos,
                    ChaseTarget = null,
                    IsTrap = false,
                    MaxLifeTime = comeback ? lifeTime * 2 : lifeTime,
                    Velocity = velocity,
                    MaxTargetTouch = maxTargetTouch,
                    TargetType = skill.Data.TargetType,
                    PirceThroughTargets = pieceThroughTargets,
                    PieceThroughTargetsCount = 0,
                };

                /// Biến đánh dấu có phải pha quay trở lại không
                bool isComebackPhrase = false;

                /// Tạo luồng bay
                BulletTimer bulletTimer = new BulletTimer()
                {
                    Bullet = bullet,
                    MaxLifeTime = Math.Min(5000, (int) (lifeTime * 1000)) * (comeback ? 2 : 1),
                    PeriodTicks = (int) (KTBulletManager.DefaultBulletTick * 1000),
                    LastDirVector = dirVector,
                    LastTickPos = fromPos,
                    CurrentPos = fromPos,
                };
                /// Nếu không có Delay
                if (delay <= 0)
                {
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
                        float nextPercent = nextLifeTime / bullet.MaxLifeTime;
                        nextPercent = Math.Min(1f, nextPercent);

                        /// Vị trí tiếp theo của đạn
                        UnityEngine.Vector2 nextPos = fromPos + dirVector * nextPercent;

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
                            this.SetTimeout(timeToVanish, () =>
                            {
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
                bulletTimer.Tick = () => {
                    /// Toác đéo gì đó
                    if (caster == null || skill == null)
                    {
                        /// Hủy đạn vì toác
                        bulletTimer.Stop();
                        return;
                    }

                    /// Phần trăm thời gian trôi qua từ vị trí hiện tại đến vị trí ở Frame tiếp theo
                    float nextLifeTime = bulletTimer.LifeTime + KTBulletManager.DefaultBulletTick * 1000;
                    /// Phần trăm
                    float nextPercent;
                    /// Vị trí tiếp theo của đạn
                    UnityEngine.Vector2 nextPos;
                    /// Nếu đến pha quay ngược lại
                    if (comeback && nextLifeTime > bulletTimer.MaxLifeTime / 2)
                    {
                        /// Nếu là lần đầu tiên quay trở lại
                        if (!isComebackPhrase)
                        {
                            /// Reset số mục tiêu đã chạm
                            bulletTimer.TotalTouchTarget = 0;
                            /// Xóa danh sách mục tiêu đã chạm vào
                            bullet.TouchTargets.Clear();
                        }

                        /// Đánh dấu là pha quay trở lại
                        isComebackPhrase = true;

                        nextPercent = 1f - (nextLifeTime - bulletTimer.LifeTime) / bullet.MaxLifeTime;
                        nextPercent = Math.Min(1f, nextPercent);
                        nextPercent = Math.Max(0, nextPercent);
                    }
                    /// Nếu là pha tiến
                    else
                    {
                        nextPercent = nextLifeTime / bullet.MaxLifeTime;
                        nextPercent = Math.Min(1f, nextPercent);
                        nextPercent = Math.Max(0, nextPercent);
                    }

                    /// Vị trí tiếp theo của đạn
                    nextPos = fromPos + dirVector * nextPercent;

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

                    /// Nếu không quay trở lại vị trí ban đầu
                    if (!comeback)
                    {
                        /// Nếu đã quá thời gian tồn tại tối đa của đạn
                        if (nextPercent >= 1f)
                        {
                            /// Xóa luồng thực thi đạn
                            bulletTimer.Stop();
                            return;
                        }
                    }
                    /// Nếu quay trở lại vị trí ban đầu
                    else
                    {
                        /// Nếu đã quá thời gian tồn tại tối đa của đạn
                        if (isComebackPhrase && nextPercent >= 1f)
                        {
                            /// Xóa luồng thực thi đạn
                            bulletTimer.Stop();
                            return;
                        }
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

                /// Nếu không có delay
                if (delay <= 0)
                {
                    bulletTimer.Run();
                }
                /// Nếu có delay
                else
                {
                    /// Thực thi sau khoảng delay
                    this.SetTimeout(delay, () =>
                    {
                        bulletTimer.Run();
                    }, caster);
                }

                /// Trả về kết quả là ID đạn
                return new CreateBulletFlyResult()
                {
                    BulletID = bullet.ID,
                    ToPos = toPos,
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
