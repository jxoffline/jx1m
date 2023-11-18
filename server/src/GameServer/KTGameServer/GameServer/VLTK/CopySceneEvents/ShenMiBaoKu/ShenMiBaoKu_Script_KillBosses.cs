using GameServer.Logic;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Nhiệm vụ giết Boss
    /// </summary>
    public partial class ShenMiBaoKu_Script_Main
    {
        /// <summary>
        /// Danh sách Boss của nhiệm vụ
        /// </summary>
        private readonly Dictionary<string, List<Monster>> bosses = new Dictionary<string, List<Monster>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private void Begin_KillBosses(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách Boss
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.BossInfo bossInfo in task.Bosses)
            {
                /// Tạo Boss
                this.CreateBoss(task, bossInfo, (boss) =>
                {
                    /// Nếu chưa tồn tại
                    if (!this.bosses.ContainsKey(this.GetTaskKey(task)))
                    {
                        /// Tạo mới
                        this.bosses[this.GetTaskKey(task)] = new List<Monster>();
                    }

                    /// Lưu lại thông tin Boss
                    this.bosses[this.GetTaskKey(task)].Add(boss);
                });
            }
        }

        /// <summary>
        /// Theo dõi nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private bool Track_KillBosses(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.bosses.TryGetValue(this.GetTaskKey(task), out List<Monster> bosses))
            {
                return false;
            }

            /// Nếu con nào đó vẫn còn sống
            if (bosses.Any(x => !x.IsDead()))
            {
                /// Chưa xong
                return false;
            }

            /// Chỉ hoàn thành nhiệm vụ khi đã tiêu diệt Boss
            return true;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ tiêu diệt Boss
        /// </summary>
        /// <param name="task"></param>
        private void Reset_KillBosses(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            this.bosses.Remove(this.GetTaskKey(task));
        }
    }
}
