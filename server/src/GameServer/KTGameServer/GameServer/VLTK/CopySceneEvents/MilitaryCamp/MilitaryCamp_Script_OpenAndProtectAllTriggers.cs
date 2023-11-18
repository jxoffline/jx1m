using GameServer.KiemThe.Core;
using GameServer.Logic;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ mở toàn bộ cơ quan và dập lửa
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Thông tin bảo vệ cơ quan
        /// </summary>
        private class ProtectTriggerData
        {
            /// <summary>
            /// Danh sách cơ quan đang bảo vệ
            /// </summary>
            public HashSet<GrowPoint> ProtectingTriggers { get; set; } = new HashSet<GrowPoint>();

            /// <summary>
            /// Thời điểm lần trước tạo thánh hỏa
            /// </summary>
            public long LastCreateHolyFireTicks { get; set; }
        }

        /// <summary>
        /// Danh sách cơ quan đang bảo vệ
        /// </summary>
        private readonly Dictionary<string, ProtectTriggerData> protectTriggers = new Dictionary<string, ProtectTriggerData>();

        /// <summary>
        /// Bắt đầu nhiệm vụ mở toàn bộ cơ quan và dập lửa
        /// </summary>
        /// <param name="task"></param>
        private void Begin_OpenAndProtectAllTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách cơ quan
            foreach (MilitaryCamp.EventInfo.StageInfo.TaskInfo.TriggerInfo triggerInfo in task.ProtectTrigger.Triggers)
            {
                /// Tạo cơ quan
                this.CreateTrigger(task, triggerInfo, (trigger, player) => {
                    /// Thêm vào danh sách cơ quan đã mở
                    this.protectTriggers[this.GetTaskKey(task)].ProtectingTriggers.Add(trigger);
                    /// Thông báo đã mở cơ quan
                    this.NotifyAllPlayers("Một tiếng động lớn, cơ quan đã mở ra. Chú ý thánh hỏa có thể phá hủy cơ quan.");
                });
            }

            /// Tạo danh sách cơ quan đã mở
            this.protectTriggers[this.GetTaskKey(task)] = new ProtectTriggerData()
            {
                LastCreateHolyFireTicks = KTGlobal.GetCurrentTimeMilis(),
            };
            /// Hủy toàn bộ thánh hỏa
            this.RemoveAllHolyFires(task.ProtectTrigger.HolyFire);
        }

        /// <summary>
        /// Theo dõi nhiệm vụ mở toàn bộ cơ quan và dập lửa
        /// </summary>
        /// <param name="task"></param>
        private bool Track_OpenAndProtectAllTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.protectTriggers.TryGetValue(this.GetTaskKey(task), out ProtectTriggerData protectTriggersData))
            {
                return false;
            }

            /// Nếu đã mở toàn bộ cơ quan
            if (protectTriggersData.ProtectingTriggers.Count >= task.ProtectTrigger.Triggers.Count)
            {
                /// Hủy toàn bộ thánh hỏa
                this.RemoveAllHolyFires(task.ProtectTrigger.HolyFire);
                /// Hoàn thành nhiệm vụ
                return true;
            }

            /// Nếu đã đến thời gian sinh thánh hỏa
            if (KTGlobal.GetCurrentTimeMilis() - protectTriggersData.LastCreateHolyFireTicks >= task.ProtectTrigger.HolyFire.SpawnEveryTicks)
            {
                /// Duyệt danh sách cơ quan đã mở
                foreach (GrowPoint trigger in protectTriggersData.ProtectingTriggers)
                {
                    /// Vị trí hiện tại của cơ quan
                    UnityEngine.Vector2 triggerPos = new UnityEngine.Vector2((int) trigger.CurrentPos.X, (int) trigger.CurrentPos.Y);
                    /// Thông tin bản đồ
                    GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
                    /// Lấy vị trí ngẫu nhiên xung quanh
                    UnityEngine.Vector2 randomPos = KTGlobal.GetRandomAroundNoObsPoint(gameMap, triggerPos, KTGlobal.GetRandomNumber(task.ProtectTrigger.HolyFire.RandomRadiusMin, task.ProtectTrigger.HolyFire.RandomRadiusMax), this.CopyScene.ID);

                    /// Tạo thánh hỏa
                    this.CreateHolyFire(task, task.ProtectTrigger.HolyFire, (int) randomPos.x, (int) randomPos.y, () => {
                        /// Thông báo phá hủy cơ quan
                        this.NotifyAllPlayers("Thánh hỏa đã phá hủy cơ quan, phải khai mở lại từ đầu.");
                        /// Xóa danh sách cơ quan đã mở
                        protectTriggersData.ProtectingTriggers.Clear();
                        /// Hủy toàn bộ thánh hỏa
                        this.RemoveAllHolyFires(task.ProtectTrigger.HolyFire);
                        /// Đánh dấu thời gian lần cuối tạo thánh hỏa
                        protectTriggersData.LastCreateHolyFireTicks = KTGlobal.GetCurrentTimeMilis();
                        /// Bỏ qua
                        return;
                    });
                }

                /// Đánh dấu thời gian lần cuối tạo thánh hỏa
                protectTriggersData.LastCreateHolyFireTicks = KTGlobal.GetCurrentTimeMilis();
            }

            /// Chưa hoàn thành nhiệm vụ
            return false;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở toàn bộ cơ quan cùng lúc
        /// </summary>
        /// <param name="task"></param>
        private void Reset_OpenAndProtectAllTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.protectTriggers.Remove(this.GetTaskKey(task));
        }
    }
}
