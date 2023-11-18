using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ mở toàn bộ cơ quan theo thứ tự
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// ID cơ quan hiện tại đã mở
        /// </summary>
        private readonly Dictionary<string, int> currentOrderedTriggerIDs = new Dictionary<string, int>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở toàn bộ cơ quan theo thứ tự
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenOrderedTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo cơ quan
            this.CreateIndexTriggers(task, task.IndexTriggers, (trigger, player, triggerInfo) => {
                //Console.WriteLine("Player: {0}, TriggerOrder: {1}, Should be: {2}", player.RoleName, (triggerInfo.Order - 1), this.currentOrderedTriggerIDs[this.GetTaskKey(task)]);

                /// Nếu mở sai thứ tự
                if (this.currentOrderedTriggerIDs[this.GetTaskKey(task)] != triggerInfo.Order - 1)
                {
                    /// Hiện thông báo mở sai
                    this.NotifyAllPlayers(string.Format("Thật đáng tiếc, thứ tự cơ quan [{0}] mở không chính xác, tất cả mở lại từ đầu!", player.RoleName));
                    /// Thiết lập lại ID cơ quan đã mở
                    this.currentOrderedTriggerIDs[this.GetTaskKey(task)] = 0;
                }
                /// Nếu mở đúng thứ tự
                else
                {
                    /// Thiết lập ID cơ quan đã mở
                    this.currentOrderedTriggerIDs[this.GetTaskKey(task)] = triggerInfo.Order;
                    /// Thông báo
                    this.NotifyAllPlayers(string.Format("[{0}] đã mở thành công {1}!", player.RoleName, triggerInfo.Name));
                }
            });
            /// Thiết lập lại ID cơ quan đã mở
            this.currentOrderedTriggerIDs[this.GetTaskKey(task)] = 0;
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở toàn bộ cơ quan theo thứ tự
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenOrderedTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.currentOrderedTriggerIDs.TryGetValue(this.GetTaskKey(task), out int currentOrderedTriggerID))
            {
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi đã mở toàn bộ cơ quan
            return currentOrderedTriggerID >= task.IndexTriggers.Triggers.Count;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở toàn bộ cơ quan theo thứ tự
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenOrderedTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.currentOrderedTriggerIDs[this.GetTaskKey(task)] = 0;
        }
    }
}
