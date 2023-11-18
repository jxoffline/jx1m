using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ mở cơ quan và tiêu diệt quái
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Danh sách quái cơ quan hiện đang có
        /// </summary>
        private readonly Dictionary<string, List<GameObject>> triggerMonsters = new Dictionary<string, List<GameObject>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở cơ quan và tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenTriggerAndKillMonsters(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo cơ quan
            this.CreateTrigger(task, task.Trigger, (trigger, player) => {
                /// Xóa cơ quan
                this.RemoveGrowPoint(trigger);
                /// Duyệt danh sách quái
                foreach (MilitaryCamp.EventInfo.StageInfo.MonsterInfo monsterInfo in task.Monsters)
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

        /// <summary>
        /// Theo dõi nhiệm vụ mở cơ quan và tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenTriggerAndKillMonsters(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
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
                //GameObject monster = triggerMonsters.Where(x => !x.IsDead()).FirstOrDefault();
                //Console.WriteLine(monster.CurrentPos);
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
        private void Reset_OpenTriggerAndKillMonsters(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.triggerMonsters.Remove(this.GetTaskKey(task));
        }
    }
}
