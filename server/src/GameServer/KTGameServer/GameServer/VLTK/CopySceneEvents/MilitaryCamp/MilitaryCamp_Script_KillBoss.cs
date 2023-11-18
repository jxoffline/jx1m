using GameServer.Logic;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ giết Boss
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Danh sách Boss của nhiệm vụ
        /// </summary>
        private readonly Dictionary<string, Monster> bosses = new Dictionary<string, Monster>();

        /// <summary>
        /// Bắt đầu nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private void Begin_KillBoss(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo Boss chính
            this.CreateBoss(task, task.Boss, (boss) =>
            {
                /// Lưu lại thông tin Boss
                this.bosses[this.GetTaskKey(task)] = boss;
            });
            /// Tạo Boss phụ
            this.CreateBoss(task, task.ChildBoss);
        }

        /// <summary>
        /// Theo dõi nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private bool Track_KillBoss(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.bosses.TryGetValue(this.GetTaskKey(task), out Monster boss))
            {
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi đã tiêu diệt Boss
            return boss.IsDead();
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private void Reset_KillBoss(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.bosses.Remove(this.GetTaskKey(task));
        }
    }
}
