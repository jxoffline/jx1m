using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ thu thập vật phẩm
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Bắt đầu nhiệm vụ thu thập vật phẩm
        /// </summary>
        /// <param name="task"></param>
        private void Begin_CollectGrowPoints(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo điểm thu thập
            this.CreateGrowPoints(task, task.GrowPoints);
        }

        /// <summary>
        /// Theo dõi nhiệm vụ thu thập vật phẩm
        /// </summary>
        /// <param name="task"></param>
        private bool Track_CollectGrowPoints(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Nếu nhiệm vụ này yêu cầu toàn đội phải có đủ số lượng tương ứng
            if (task.Target.RequireAllMembers)
            {
                /// Duyệt danh sách thành viên nhóm
                foreach (KPlayer teammate in this.teamPlayers)
                {
                    /// Nếu thằng này đã Offline
                    if (!teammate.IsOnline())
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu nó ở map khác
                    else if (teammate.CurrentMapCode != this.CopyScene.MapCode || teammate.CurrentCopyMapID != this.CopyScene.ID)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu không có nhóm
                    else if (teammate.TeamID == -1)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Duyệt danh sách vật phẩm yêu cầu
                    foreach (KeyValuePair<int, int> pair in task.Target.Items)
                    {
                        /// Nếu không đủ số lượng yêu cầu
                        if (ItemManager.GetItemCountInBag(teammate, pair.Key) < pair.Value)
                        {
                            /// Chưa hoàn thành
                            return false;
                        }
                    }
                }

                /// Đã hoàn thành
                return true;
            }
            /// Nếu nhiệm vụ này chỉ yêu cầu 1 thành viên bất kỳ trong nhóm có đủ số lượng tương ứng
            else
            {
                
                /// Duyệt danh sách thành viên nhóm
                foreach (KPlayer teammate in this.teamPlayers)
                {
                    /// Nếu thằng này đã Offline
                    if (!teammate.IsOnline())
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu nó ở map khác
                    else if (teammate.CurrentMapCode != this.CopyScene.MapCode || teammate.CurrentCopyMapID != this.CopyScene.ID)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu không có nhóm
                    else if (teammate.TeamID == -1)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Đánh dấu người chơi có đủ số lượng tất cả không
                    bool enoughMaterials = true;
                    /// Duyệt danh sách vật phẩm yêu cầu
                    foreach (KeyValuePair<int, int> pair in task.Target.Items)
                    {
                        /// Nếu không đủ số lượng yêu cầu
                        if (ItemManager.GetItemCountInBag(teammate, pair.Key) < pair.Value)
                        {
                            /// Đánh dấu không đủ số lượng
                            enoughMaterials = false;
                            /// Thoát
                            break;
                        }
                    }

                    /// Nếu người chơi này đủ số lượng yêu cầu
                    if (enoughMaterials)
                    {
                        /// Hoàn thành nhiệm vụ
                        return true;
                    }
                }

                /// Chưa hoàn thành nhiệm vụ
                return false;
            }
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ thu thập nguyên liệu
        /// </summary>
        /// <param name="task"></param>
        private void Reset_CollectGrowPoints(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
        }
    }
}
