using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Generic;
using System.Text;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ giao vật phẩm cho NPC
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Đánh dấu NPC đã nhận đủ vật phẩm yêu cầu chưa
        /// </summary>
        private readonly Dictionary<string, bool> npcTakenEnoughGoods = new Dictionary<string, bool>();

        /// <summary>
        /// Bắt đầu nhiệm vụ giao vật phẩm cho NPC
        /// </summary>
        /// <param name="task"></param>
        private void Begin_GiveGoodsToNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Tạo NPC
            this.CreateDynamicNPC(task, task.NPC, (npc) => {
                /// Sự kiện Click
                npc.Click = (player) => {
                    /// Hội thoại NPC
                    StringBuilder dialogMsgBuilder = new StringBuilder();
                    /// Tên nhiệm vụ
                    dialogMsgBuilder.AppendLine(string.Format("<color=yellow>{0}:</color>", task.Name));
                    /// Danh sách vật phẩm yêu cầu
                    List<string> requireItemsStrings = new List<string>();
                    /// Duyệt danh sách vật phẩm yêu cầu
                    foreach (KeyValuePair<int, int> pair in task.Target.Items)
                    {
                        requireItemsStrings.Add(string.Format("<color=yellow>[{0}]</color> - SL: <color=green>{1}</color>", KTGlobal.GetItemName(pair.Key), pair.Value));
                    }
                    /// Mục tiêu nhiệm vụ
                    dialogMsgBuilder.AppendLine(string.Format("<color=orange>{0}</color> thu thập đủ {1}", task.Target.RequireAllMembers ? "Mỗi thành viên" : "Toàn đội", string.Join(", ", requireItemsStrings)));

                    /// Tạo mới Dialog
                    KNPCDialog dialog = new KNPCDialog();
                    dialog.Owner = player;
                    dialog.Text = dialogMsgBuilder.ToString();
                    dialog.Selections = new Dictionary<int, string>()
                    {
                        { -1, "Giao vật phẩm" },
                        { -1000, "Ta vẫn chưa thu thập đủ" },
                    };
                    dialog.OnSelect = (x) => {
                        /// Giao vật phẩm
                        if (x.SelectID == -1)
                        {
                            /// Đóng NPCDialog
							KT_TCPHandler.CloseDialog(player);

                            /// Nếu không phải đội trưởng
                            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID) || player.TeamLeader == null || player.TeamLeader.RoleID != player.RoleID)
                            {
                                KNPCDialog dialogEx = new KNPCDialog();
                                dialogEx.Owner = player;
                                dialogEx.Text = "Hãy báo đội trưởng đến gặp ta.";
                                KTNPCDialogManager.AddNPCDialog(dialogEx);
                                dialogEx.Show(npc, player);
                                return;
                            }

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
                                    /// Nếu không cùng nhóm
                                    else if (teammate.TeamID != player.TeamID)
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
                                            /// Thông báo thành viên không đủ số lượng yêu cầu
                                            KTPlayerManager.ShowNotification(player, string.Format("[{0}] số lượng vật phẩm yêu cầu chưa đủ.", teammate.RoleName));
                                            /// Thoát
                                            return;
                                        }
                                    }
                                }

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
                                    /// Nếu không cùng nhóm
                                    else if (teammate.TeamID != player.TeamID)
                                    {
                                        /// Bỏ qua
                                        continue;
                                    }

                                    /// Duyệt danh sách vật phẩm yêu cầu
                                    foreach (KeyValuePair<int, int> pair in task.Target.Items)
                                    {
                                        /// Xóa vật phẩm yêu cầu
                                        ItemManager.RemoveItemFromBag(teammate, pair.Key, pair.Value);
                                    }
                                }

                                /// Thông báo giao vật phẩm thành công
                                KTPlayerManager.ShowNotificationToAllTeammates(player, "Giao vật phẩm thành công!");

                                /// Đánh dấu NPC đã nhận đủ số lượng vật phẩm yêu cầu
                                this.npcTakenEnoughGoods[this.GetTaskKey(task)] = true;
                            }
                            /// Nếu nhiệm vụ này chỉ yêu cầu 1 thành viên bất kỳ trong nhóm có đủ số lượng tương ứng
                            else
                            {
                                /// Duyệt danh sách vật phẩm yêu cầu
                                foreach (KeyValuePair<int, int> pair in task.Target.Items)
                                {
                                    /// Nếu không đủ số lượng yêu cầu
                                    if (ItemManager.GetItemCountInBag(player, pair.Key) < pair.Value)
                                    {
                                        /// Thông báo không đủ số lượng yêu cầu
                                        KTPlayerManager.ShowNotification(player, "Số lượng vật phẩm yêu cầu chưa đủ.");
                                        /// Thoát
                                        return;
                                    }
                                }

                                /// Duyệt danh sách vật phẩm yêu cầu
                                foreach (KeyValuePair<int, int> pair in task.Target.Items)
                                {
                                    /// Xóa vật phẩm yêu cầu
                                    ItemManager.RemoveItemFromBag(player, pair.Key, pair.Value);
                                }

                                /// Thông báo giao vật phẩm thành công
                                KTPlayerManager.ShowNotificationToAllTeammates(player, "Giao vật phẩm thành công!");

                                /// Đánh dấu NPC đã nhận đủ số lượng vật phẩm yêu cầu
                                this.npcTakenEnoughGoods[this.GetTaskKey(task)] = true;
                            }
                        }
                        /// Toác
                        else if (x.SelectID == -1000)
                        {
                            /// Đóng NPCDialog
							KT_TCPHandler.CloseDialog(player);
                        }
                    };
                    KTNPCDialogManager.AddNPCDialog(dialog);
                    dialog.Show(npc, player);
                };
			});
            /// Đánh dấu NPC chưa nhận đủ số lượng vật phẩm yêu cầu
            this.npcTakenEnoughGoods[this.GetTaskKey(task)] = false;
        }

        /// <summary>
        /// Theo dõi nhiệm vụ giao vật phẩm cho NPC
        /// </summary>
        /// <param name="task"></param>
        private bool Track_GiveGoodsToNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.npcTakenEnoughGoods.TryGetValue(this.GetTaskKey(task), out bool isNPCTookEnoughGoods))
            {
                return false;
            }
            /// Trả về kết quả
            return isNPCTookEnoughGoods;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ giao vật phẩm cho NPC
        /// </summary>
        /// <param name="task"></param>
        private void Reset_GiveGoodsToNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.npcTakenEnoughGoods.Remove(this.GetTaskKey(task));
        }
    }
}
