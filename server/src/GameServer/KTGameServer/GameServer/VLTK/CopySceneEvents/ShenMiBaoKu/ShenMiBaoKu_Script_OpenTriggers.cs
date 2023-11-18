using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Nhiệm vụ mở cơ quan
    /// </summary>
    public partial class ShenMiBaoKu_Script_Main
    {
        /// <summary>
        /// Danh sách cơ quan đã mở
        /// </summary>
        private readonly Dictionary<string, HashSet<ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo>> openedTriggers = new Dictionary<string, HashSet<ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenTriggers(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách cơ quan
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo triggerInfo in task.Triggers)
            {
                /// Tạo cơ quan
                this.CreateTrigger(task, triggerInfo, (trigger, player) => {
                    /// Xóa cơ quan
                    this.RemoveGrowPoint(trigger);
                    /// Đánh dấu đã mở cơ quan
                    this.openedTriggers[this.GetTaskKey(task)].Add(triggerInfo);
                });
            }
            /// Tạo mới danh sách cơ quan đã mở
            this.openedTriggers[this.GetTaskKey(task)] = new HashSet<ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo>();
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenTriggers(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.openedTriggers.TryGetValue(this.GetTaskKey(task), out HashSet<ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo> openedTriggers))
            {
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi đã mở toàn bộ cơ quan
            return openedTriggers.Count == task.Triggers.Count;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenTriggers(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            this.openedTriggers.Remove(this.GetTaskKey(task));
        }
    }
}
