using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ mở cơ quan
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Đánh dấu cơ quan đã mở chưa
        /// </summary>
        private readonly Dictionary<string, bool> isTriggerOpened = new Dictionary<string, bool>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenTrigger(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo cơ quan
            this.CreateTrigger(task, task.Trigger, (trigger, player) => {
                /// Xóa cơ quan
                this.RemoveGrowPoint(trigger);
                /// Đánh dấu đã mở cơ quan
                this.isTriggerOpened[this.GetTaskKey(task)] = true;
            });
            /// Đánh dấu chưa mở cơ quan
            this.isTriggerOpened[this.GetTaskKey(task)] = false;
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenTrigger(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.isTriggerOpened.TryGetValue(this.GetTaskKey(task), out bool isTriggerOpened))
            {
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi đã mở cơ quan
            return isTriggerOpened;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenTrigger(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.isTriggerOpened.Remove(this.GetTaskKey(task));
        }
    }
}
