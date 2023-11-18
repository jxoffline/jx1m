using GameServer.Logic;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Nhiệm vụ mở cơ quan và tiêu diệt quái
    /// </summary>
    public partial class ShenMiBaoKu_Script_Main
    {
        /// <summary>
        /// Danh sách quái cơ quan hiện đang có
        /// </summary>
        private readonly Dictionary<string, List<GameObject>> triggerMonsters = new Dictionary<string, List<GameObject>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở cơ quan và tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenTriggersAndKillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách cơ quan
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo triggerInfo in task.Triggers)
            {
                /// Tạo cơ quan
                this.CreateTrigger(task, triggerInfo, (trigger, player) => {
                    /// Xóa cơ quan
                    this.RemoveGrowPoint(trigger);
                    /// Duyệt danh sách quái
                    foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.MonsterInfo monsterInfo in task.Monsters)
                    {
                        /// Tạo quái
                        this.CreateMonster(task, monsterInfo, (monster) => {
                            /// Nếu chưa có danh sách thì tạo mới
                            if (!this.triggerMonsters.ContainsKey(this.GetTaskKey(task)))
                            {
                                this.triggerMonsters[this.GetTaskKey(task)] = new List<GameObject>();
                            }

                            /// Thêm quái cơ quan vào
                            this.triggerMonsters[this.GetTaskKey(task)].Add(monster);
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở cơ quan và tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenTriggersAndKillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Nếu chưa tạo danh sách
            if (!this.triggerMonsters.TryGetValue(this.GetTaskKey(task), out List<GameObject> triggerMonsters))
            {
                /// Bỏ qua
                return false;
            }

            /// Nếu có con nào vẫn còn sống
            if (triggerMonsters.Any(x => !x.IsDead()))
            {
                /// Chưa xong
                return false;
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở cơ quan và tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenTriggersAndKillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            this.triggerMonsters.Remove(this.GetTaskKey(task));
        }
    }
}
