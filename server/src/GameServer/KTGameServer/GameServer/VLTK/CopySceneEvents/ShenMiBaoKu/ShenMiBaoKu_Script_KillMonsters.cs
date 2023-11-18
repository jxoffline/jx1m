using GameServer.Logic;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Nhiệm vụ giết quaí
    /// </summary>
    public partial class ShenMiBaoKu_Script_Main
    {
        /// <summary>
        /// Danh sách quái của nhiệm vụ
        /// </summary>
        private readonly Dictionary<string, List<Monster>> monsters = new Dictionary<string, List<Monster>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private void Begin_KillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách quái
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.MonsterInfo monsterInfo in task.Monsters)
            {
                /// Tạo quái
                this.CreateMonster(task, monsterInfo, (monster) =>
                {
                    /// Nếu chưa tồn tại
                    if (!this.monsters.ContainsKey(this.GetTaskKey(task)))
                    {
                        /// Tạo mới
                        this.monsters[this.GetTaskKey(task)] = new List<Monster>();
                    }

                    /// Lưu lại thông tin quái
                    this.monsters[this.GetTaskKey(task)].Add(monster);
                });
            }
        }

        /// <summary>
        /// Theo dõi nhiệm vụ tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private bool Track_KillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.monsters.TryGetValue(this.GetTaskKey(task), out List<Monster> monsters))
            {
                return false;
            }

            /// Nếu con nào đó vẫn còn sống
            if (monsters.Any(x => !x.IsDead()))
            {
                /// Chưa xong
                return false;
            }

            /// Chỉ hoàn thành nhiệm vụ khi đã tiêu diệt quái
            return true;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ tiêu diệt quái
        /// </summary>
        /// <param name="task"></param>
        private void Reset_KillMonsters(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            this.monsters.Remove(this.GetTaskKey(task));
        }
    }
}
