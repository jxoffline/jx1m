using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.LuaSystem;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.Manager
{
    /// <summary>
    /// Quản lý Logic điểm thu thập
    /// </summary>
    public static partial class KTGrowPointManager
    {
        #region Core
        /// <summary>
        /// Xóa và thực hiện tái tạo lại điểm thu thập
        /// </summary>
        /// <param name="growPoint"></param>
        public static void RemoveGrowPointAndRespawn(GrowPoint growPoint)
        {
            /// Nếu không tồn tại trong hệ thống
            if (!KTGrowPointManager.GrowPoints.TryGetValue(growPoint.ID, out _))
            {
                return;
            }

            /// Nếu loại đối tượng tái sinh ngay lập tức thì không cần
            if (growPoint.RespawnTime == -1)
            {
                return;
            }
            /// Nếu không tái tạo
            else if (growPoint.RespawnTime == -100)
            {
                KTGrowPointManager.Remove(growPoint);
                return;
            }

            /// Xóa
            growPoint.Alive = false;
            /// Thông báo tới người chơi xung quanh xóa đối tượng
            KTGrowPointManager.NotifyNearClientsToRemoveSelf(growPoint);

            /// Thực hiện đếm lùi tái tạo
            KTGrowPointTimerManager.Instance.Add(growPoint, growPoint.RespawnTime, () =>
            {
                /// Tái sinh
                growPoint.Alive = true;
                /// Thông báo tới người chơi xung quanh đối tượng tái sinh
                KTGrowPointManager.NotifyNearClientsToAddSelf(growPoint);
            });
        }
        #endregion

        /// <summary>
        /// Sự kiện Click vào điểm thu thập
        /// </summary>
        /// <param name="player"></param>
        /// <param name="growPointID"></param>
        public static void GrowPointClick(KPlayer player, int growPointID)
        {
            /// Nếu người chơi đang bị khống chế
            if (!player.IsCanDoLogic())
            {
                KTPlayerManager.ShowNotification(player, "Trong trạng thái bị khống chế, không thể thực hiện thao tác!");
                return;
            }
            /// Đối tượng điểm thu thập
            GrowPoint growPoint = null;
            /// Nếu điểm thu thập không tồn tại
            if (!KTGrowPointManager.GrowPoints.TryGetValue(growPointID, out growPoint))
            {
                KTPlayerManager.ShowNotification(player, "Đối tượng không tồn tại!");
                return;
            }

            /// Nếu đã bị người khác thu thập
            if (!growPoint.Alive)
            {
                KTPlayerManager.ShowNotification(player, "Đối tượng đã bị người khác thu thập!");
                return;
            }

            /// Bản đồ tương ứng
            GameMap map = KTMapManager.Find(player.MapCode);
            if (map == null)
            {
                return;
            }

            /// Thực thi Script Lua tương ứng
            if (growPoint.ScriptID != -1)
            {
                /// Thực thi hàm kiểm tra điều kiện ở Script tương ứng
                KTLuaEnvironment.ExecuteGrowPointScript_OnPreCheckCondition(map, growPoint, player, growPoint.ScriptID, (res) => {
                    /// Nếu kiểm tra điều kiện thất bại
                    if (!res)
                    {
                        return;
                    }

                    /// Thêm thao tác tương ứng cho người chơi
                    player.CurrentProgress = new Entities.Player.KPlayer_Progress()
                    {
                        Hint = "Đang thu thập",
                        InteruptIfTakeDamage = growPoint.Data.InteruptIfTakeDamage,
                        StartTick = KTGlobal.GetCurrentTimeMilis(),
                        DurationTick = growPoint.Data.CollectTick,
                        Cancel = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {
                                KTLuaEnvironment.ExecuteGrowPointScript_OnCancel(map, growPoint, player, growPoint.ScriptID);
                            }
                        },
                        Tick = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {
                                KTLuaEnvironment.ExecuteGrowPointScript_OnActivateEachTick(map, growPoint, player, growPoint.ScriptID);
                            }
                        },
                        Complete = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {
                                KTLuaEnvironment.ExecuteGrowPointScript_OnComplete(map, growPoint, player, growPoint.ScriptID);
                                
                                /// Thực hiện thao tác thu thập hoàn tất sau khoảng tương ứng
                                System.Threading.Tasks.Task func = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayFuntionAsyncTask("GrowPoint_Delay", () => {
                                    /// Thực thi sự kiện thu thập hoàn tất
                                    player.GrowPointCollectCompleted(growPoint);
                                    /// Call sự kiện tùy chỉnh sau khi thu thập xong
                                    growPoint.GrowPointCollectCompleted?.Invoke(player);
                                }), 200);

                                /// Thực hiện xóa đối tượng
                                KTGrowPointManager.RemoveGrowPointAndRespawn(growPoint);


                            }
                            /// Nếu điểm thu thập không còn tồn tại thì thao tác thất bại
                            else
                            {
                                KTLuaEnvironment.ExecuteGrowPointScript_OnFaild(map, growPoint, player, growPoint.ScriptID);
                            }
                        },
                    };
                });
            }
            else
            {
                /// Nếu thỏa mãn điều kiện thu thập
                if (growPoint.ConditionCheck(player))
				{
                    /// Thêm thao tác tương ứng cho người chơi
                    player.CurrentProgress = new Entities.Player.KPlayer_Progress()
                    {
                        Hint = "Đang thu thập",
                        InteruptIfTakeDamage = growPoint.Data.InteruptIfTakeDamage,
                        StartTick = KTGlobal.GetCurrentTimeMilis(),
                        DurationTick = growPoint.Data.CollectTick,
                        Cancel = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {

                            }
                        },
                        Tick = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {

                            }
                        },
                        Complete = () => {
                            /// Nếu điểm thu thập còn tồn tại
                            if (growPoint.Alive)
                            {
                                /// Thực hiện xóa đối tượng
                                KTGrowPointManager.RemoveGrowPointAndRespawn(growPoint);

                                /// Thực thi sự kiện thu thập hoàn tất
                                player.GrowPointCollectCompleted(growPoint);

                                /// Call sự kiện tùy chỉnh sau khi thu thập xong
                                growPoint.GrowPointCollectCompleted?.Invoke(player);
                            }
                        },
                    };
                }
            }
        }
    }
}
