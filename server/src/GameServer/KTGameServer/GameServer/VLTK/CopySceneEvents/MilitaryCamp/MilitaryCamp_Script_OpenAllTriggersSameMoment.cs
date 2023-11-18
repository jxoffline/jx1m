using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ mở toàn bộ cơ quan cùng lúc
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Danh sách cơ quan đã mở
        /// </summary>
        private readonly Dictionary<string, HashSet<int>> openedTriggers = new Dictionary<string, HashSet<int>>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở toàn bộ cơ quan cùng lúc
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenAllTriggersSameMoment(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo cơ quan
            this.CreateIndexTriggers(task, task.IndexTriggers, (trigger, player, triggerInfo) => {
                /// Nếu chưa có cơ quan nào được mở
                if (this.openedTriggers[this.GetTaskKey(task)].Count <= 0)
                {
                    /// Đếm lùi 3s sẽ thất bại
                    this.SetTimeout(3000, () => {
                        /// Toác
                        if (!this.openedTriggers.ContainsKey(this.GetTaskKey(task)))
                        {
                            /// Bỏ qua
                            return;
                        }

                        /// Nếu tất cả cơ quan chưa được mở
                        if (this.openedTriggers[this.GetTaskKey(task)].Count < task.IndexTriggers.Triggers.Count)
                        {
                            /// Thông báo thất bại
                            this.NotifyAllPlayers("Khai mở cơ quan thất bại. Cần mở đồng loạt cơ quan cùng lúc!");
                            /// Xóa danh sách cơ quan đã mở
                            this.openedTriggers[this.GetTaskKey(task)].Clear();
                        }
                    });
                }

                /// Thêm vào danh sách cơ quan đã mở
                this.openedTriggers[this.GetTaskKey(task)].Add(trigger.ID);

                /// Thông báo mở thành công
                this.NotifyAllPlayers("Khai mở thành công!");
            });
            /// Tạo mới danh sách cơ quan đã mở
            this.openedTriggers[this.GetTaskKey(task)] = new HashSet<int>();
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở toàn bộ cơ quan cùng lúc
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenAllTriggersSameMoment(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.openedTriggers.TryGetValue(this.GetTaskKey(task), out HashSet<int> openedTriggers))
            {
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi đã mở toàn bộ cơ quan
            return openedTriggers.Count >= task.IndexTriggers.Triggers.Count;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở toàn bộ cơ quan cùng lúc
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenAllTriggersSameMoment(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.openedTriggers.Remove(this.GetTaskKey(task));
        }
    }
}
