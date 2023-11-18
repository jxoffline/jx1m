using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ hộ tống NPC
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Đối tượng NPC di chuyển
        /// </summary>
        private readonly Dictionary<string, Monster> movingNPCs = new Dictionary<string, Monster>();

        /// <summary>
        /// Bắt đầu nhiệm vụ hộ tống NPC
        /// </summary>
        /// <param name="task"></param>
        private void Begin_TransferNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo NPC di chuyển
            this.CreateMovingNPC(task, task.MovingNPC, (npc) => {
                /// Lưu lại thông tin NPC di chuyển
                this.movingNPCs[this.GetTaskKey(task)] = npc;
            });
        }

        /// <summary>
        /// Theo dõi nhiệm vụ hộ tống NPC
        /// </summary>
        /// <param name="task"></param>
        private bool Track_TransferNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Nếu không tồn tại
            if (!this.movingNPCs.TryGetValue(this.GetTaskKey(task), out Monster movingNPC))
            {
                /// Chưa hoàn thành
                return false;
            }
            /// Chỉ hoàn thành nhiệm vụ khi NPC đã đến đích
            return KTGlobal.GetDistanceBetweenPoints(movingNPC.CurrentPos, new System.Windows.Point(task.MovingNPC.ToPosX, task.MovingNPC.ToPosY)) <= 10;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ hộ tống NPC
        /// </summary>
        /// <param name="task"></param>
        private void Reset_TransferNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.movingNPCs.Remove(this.GetTaskKey(task));
        }
    }
}
