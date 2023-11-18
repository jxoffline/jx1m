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
        /// Tạo bẫy tại vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="dirVectorProperty">Vector chỉ hướng xuất chiêu</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="pos">Vị trí xuất hiện bẫy</param>
        /// <param name="explodeRadius">Phạm vi nổ</param>
        /// <param name="periodTick">Thời gian Tick mỗi lần nếu là loại bẫy không xóa khi chạm mục tiêu</param>
        /// <param name="TickFunction">Hàm thực thi mỗi khi hàm Tick của đạn được gọi</param>
        /// <param name="targetType">Loại mục tiêu</param>
        /// <returns></returns>
        public int CreateTrap(SkillLevelRef skill, UnityEngine.Vector2? dirVectorProperty, GameObject caster, UnityEngine.Vector2 pos, int explodeRadius, float periodTick, int maxTargetTouch, float maxLifeTime, Action<UnityEngine.Vector2> TickFunction = null)
        {
            if (caster == null || skill == null)
            {
                return -1;
            }

            try
            {
                /// Tạo mới bẫy
                Bullet bullet = new Bullet()
                {
                    Skill = skill,
                    DirVectorProperty = dirVectorProperty,
                    Caster = caster,
                    FromPos = pos,
                    ToPos = pos,
                    MaxLifeTime = maxLifeTime,
                    ChaseTarget = null,
                    IsTrap = true,
                    MaxTargetTouch = maxTargetTouch,
                    Velocity = 0,
                    TargetType = skill.Data.TargetType,
                };

                /// Tạo luồng thực thi bẫy
                BulletTimer bulletTimer = new BulletTimer()
                {
                    Bullet = bullet,
                    MaxLifeTime = Math.Min(30000, (int) (maxLifeTime * 1000)),
                    PeriodTicks = (int) ((skill.Data.SkillStyle == "trapnodestroyontouch" ? periodTick : KTBulletManager.DefaultTrapTick) * 1000),
                };
                bulletTimer.Start = () => {
                    if (caster == null || skill == null)
                    {
                        return;
                    }

                    this.DoTrapLogic(bulletTimer, pos, maxTargetTouch, explodeRadius);
                };
                bulletTimer.Tick = () => {
                    if (caster == null || skill == null)
                    {
                        return;
                    }

                    this.DoTrapLogic(bulletTimer, pos, maxTargetTouch, explodeRadius);

                    /// Thực hiện hàm Tick nếu có
                    TickFunction?.Invoke(pos);
                };
                bulletTimer.Destroy = () => {
                    if (caster == null || skill == null)
                    {
                        return;
                    }

                    /// Xóa bẫy khỏi bản đồ
                    KTTrapManager.RemoveTrap(bullet.ID);

                    /// Thực hiện kỹ năng tan biến
                    KTSkillManager.BulletVanished(skill, caster, pos, bulletTimer.VanishTarget, dirVectorProperty);
                };
                bulletTimer.Error = (ex) => {
                    LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                };
                bulletTimer.Run();

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
