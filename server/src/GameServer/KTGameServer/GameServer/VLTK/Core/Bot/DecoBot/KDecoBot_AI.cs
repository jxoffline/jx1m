using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// AI của bot
    /// </summary>
    public partial class KDecoBot
    {
        /// <summary>
        /// Mục tiêu
        /// </summary>
        private Monster target = null;

        /// <summary>
        /// Sự kiện thực thi Tick của AI
        /// </summary>
        private void AITick()
        {
            /// Nếu đang di chuyển thì thôi
            if (KTBotStoryBoardEx.Instance.HasStoryBoard(this))
            {
                /// Bỏ qua
                return;
            }

            /// Nếu vị trí hiện tại quá xa so với vị trí ban đầu
            if (KTGlobal.GetDistanceBetweenPoints(this.CurrentPos, this.InitPos) >= 1000)
            {
                /// Hủy mục tiêu đang đuổi
                this.target = null;
                /// Quay trở về vị trí ban đầu
                KTBotStoryBoardEx.Instance.Add(this, this.CurrentPos, this.InitPos, KE_NPC_DOING.do_run, false, true);
                /// Bỏ qua
                return;
            }

            /// Nếu có mục tiêu
            if (this.target != null && !this.target.IsDead())
            {
                /// Thực hiện tự dùng kỹ năng
                this.ProcessAIUseSkill();
                /// Bỏ qua
                return;
            }

            /// Tìm các mục tiêu xung quanh là cọc gỗ
            List<Monster> targets = KTGlobal.GetNearByEnemies<Monster>(this, 1000, 1);
            /// Nếu không tìm thấy
            if (targets == null || targets.Count <= 0)
            {
                /// Bỏ qua
                return;
            }
            /// Đánh dấu mục tiêu
            this.target = targets[0];
        }

        /// <summary>
        /// Thực hiện AI tự dùng kỹ năng
        /// </summary>
        private void ProcessAIUseSkill()
        {
            /// Nếu không có mục tiêu hoặc mục tiêu đã chết
            if (this.target == null || this.target.IsDead())
            {
                /// Bỏ qua
                return;
            }

            /// Thông tin kỹ năng sẽ dùng
            BotSkill skill = this.skills.Values.Where(x => x.IsReady).OrderByDescending(x => x.Data.Cooldown).FirstOrDefault();
            /// Thông tin kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(skill.Data.SkillID);
            /// Toác
            if (skillData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu chưa đến thời gian dùng kỹ năng
            if (!KTGlobal.FinishedUseSkillAction(this, skillData.IsPhysical ? this.GetCurrentAttackSpeed() : this.GetCurrentCastSpeed()))
            {
                return;
            }

            /// Khoảng cách đến mục tiêu
            float distanceToTarget = KTGlobal.GetDistanceBetweenGameObjects(this, this.target);

            /// Nếu nằm ngoài phạm vi
            if (distanceToTarget > skillData.AttackRadius)
            {
                /// Vị trí bản thân
                UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);
                /// Vị trí đối phương
                UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
                /// Vector hướng di chuyển
                UnityEngine.Vector2 dirVector = targetPos - selfPos;
                /// Vị trí gần nhất cần dịch đến
                UnityEngine.Vector2 toMovePos = KTMath.FindPointInVectorWithDistance(targetPos, -dirVector, skillData.AttackRadius - 20);
                /// Thực hiện tìm đường
                KTBotStoryBoardEx.Instance.Add(this, this.CurrentPos, new System.Windows.Point(toMovePos.x, toMovePos.y), KE_NPC_DOING.do_run, false, true);
            }
            /// Nếu nằm trong phạm vi
            else
            {
                /// Dùng kỹ năng tương ứng
                this.UseSkill(skill, this.target);
            }
        }
    }
}
