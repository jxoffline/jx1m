using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Nhiệm vụ đoán số
    /// </summary>
    public partial class MilitaryCamp_Script_Main
    {
        /// <summary>
        /// Thông tin đoán số
        /// </summary>
        private class GuessNumberData
        {
            /// <summary>
            /// Giá trị nhỏ nhất của số trong khoảng đã đoán
            /// </summary>
            public int GuessNumberCurrentMinValue { get; set; }

            /// <summary>
            /// Giá trị lớn nhất của số trong khoảng đã đoán
            /// </summary>
            public int GuessNumberCurrentMaxValue { get; set; }

            /// <summary>
            /// Số may mắn trong vòng đoán số hiện tại
            /// </summary>
            public int GuessNumberLuckyNumber { get; set; }

            /// <summary>
            /// Số lượt đã đoán số hiện tại
            /// </summary>
            public int GuessNumberTotalTurns { get; set; }

            /// <summary>
            /// Thành viên đoán số hiện tại
            /// </summary>
            public KPlayer GuessNumberMember { get; set; }

            /// <summary>
            /// Đánh dấu đã hoàn thành đoán số chưa
            /// </summary>
            public bool GuessNumberCompleted { get; set; }
        }

        /// <summary>
        /// Thông tin đoán số
        /// </summary>
        private readonly Dictionary<string, GuessNumberData> guessNumberRoundData = new Dictionary<string, GuessNumberData>();

        /// <summary>
        /// Bắt đầu nhiệm vụ đoán số
        /// </summary>
        /// <param name="task"></param>
        private void Begin_GuessNumber(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// <summary>
            /// Bắt đầu trò chơi
            /// </summary>
            void BeginGame()
            {
                /// Tạo mới
                GuessNumberData data = new GuessNumberData();

                /// Thiết lập lại khoảng Min-Max
                data.GuessNumberCurrentMinValue = task.GuessNumberConfig.MinNumber;
                data.GuessNumberCurrentMaxValue = task.GuessNumberConfig.MaxNumber;
                /// Chọn ngẫu nhiên số may mắn
                data.GuessNumberLuckyNumber = KTGlobal.GetRandomNumber(task.GuessNumberConfig.MinNumber, task.GuessNumberConfig.MaxNumber);
                /// Thiết lập lại số lượt đã đoán
                data.GuessNumberTotalTurns = 0;
                /// Chuyển lượt qua cho thằng khác
                data.GuessNumberMember = this.teamPlayers.Where((teammate) => {
                    /// Nếu thằng này đã Offline
                    if (!teammate.IsOnline())
                    {
                        /// Bỏ qua
                        return false;
                    }
                    /// Nếu nó ở map khác
                    else if (teammate.CurrentMapCode != this.CopyScene.MapCode || teammate.CurrentCopyMapID != this.CopyScene.ID)
                    {
                        /// Bỏ qua
                        return false;
                    }
                    /// Nếu không trong nhóm
                    else if (teammate.TeamID == -1)
                    {
                        /// Bỏ qua
                        return false;
                    }
                    /// Có thể chọn
                    return true;
                }).RandomRange(1).FirstOrDefault();
                /// Thông báo chuyển lượt đoán số
                this.NotifyAllPlayers(string.Format("Lượt đoán đầu tiên thuộc về [{0}].", data.GuessNumberMember.RoleName));

                /// Lưu lại
                this.guessNumberRoundData[this.GetTaskKey(task)] = data;
            }

            /// Tạo NPC
            this.CreateDynamicNPC(task, task.NPC, (npc) => {
                /// Sự kiện Click
                npc.Click = (player) => {
                    /// Hội thoại NPC
                    StringBuilder dialogMsgBuilder = new StringBuilder();
                    /// Tên nhiệm vụ
                    dialogMsgBuilder.AppendLine(string.Format("<color=yellow>{0}:</color>", task.Name));
                    /// Mục tiêu nhiệm vụ
                    dialogMsgBuilder.AppendLine(string.Format("<color=yellow>Đoán chính xác</color> số hệ thống đưa ra trong khoảng <color=green>{0} - {1}</color>, hệ thống ngẫu nhiên hệ thống chọn người chơi bất kỳ trong đội mỗi lượt đoán. Nếu sau <color=yellow>{2} lượt</color> không ai <color=orange>đoán chính xác</color> thì trò chơi <color=yellow>bắt đầu lại</color>.", task.GuessNumberConfig.MinNumber, task.GuessNumberConfig.MaxNumber, task.GuessNumberConfig.MaxTurns));

                    /// Tạo NPC Dialog
                    KNPCDialog dialog = new KNPCDialog();
                    dialog.Owner = player;
                    dialog.Text = dialogMsgBuilder.ToString();
                    dialog.Selections = new Dictionary<int, string>()
                    {
                        { -1, "Ta muốn đoán số" },
                        { -1000, "Kết thúc đối thoại" },
                    };
                    dialog.OnSelect = (x) => {
                        /// Giao vật phẩm
                        if (x.SelectID == -1)
                        {
                            /// Đóng NPCDialog
							KT_TCPHandler.CloseDialog(player);

                            /// Nếu không phải lượt bản thân
                            if (this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember != player)
                            {
                                /// Thằng tương ứng
                                KPlayer targetPlayer = this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember;

                                /// Nếu nó còn Online
                                if (targetPlayer.IsOnline() && targetPlayer.TeamID != -1 && targetPlayer.CurrentMapCode == this.CopyScene.MapCode && targetPlayer.CurrentCopyMapID == this.CopyScene.ID)
                                {
                                    /// Thông báo không phải lượt đoán
                                    KTPlayerManager.ShowNotification(player, string.Format("Hiện đang là lượt đoán số của [{0}].", targetPlayer.RoleName));
                                }
                                /// Nếu không còn trong nhóm
                                else
                                {
                                    KTPlayerManager.ShowNotification(player, string.Format("Hiện đang là lượt đoán số của [{0}], tuy nhiên đã bỏ cuộc. Tự chuyển lượt.", targetPlayer.RoleName));

                                    ///// Tăng số lượt đã đoán lên
                                    //this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberTotalTurns++;
                                    /// Chuyển lượt qua cho thằng khác
                                    this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember = this.teamPlayers.Where((teammate) => {
                                        /// Nếu thằng này đã Offline
                                        if (!teammate.IsOnline())
                                        {
                                            /// Bỏ qua
                                            return false;
                                        }
                                        /// Nếu nó ở map khác
                                        else if (teammate.CurrentMapCode != this.CopyScene.MapCode || teammate.CurrentCopyMapID != this.CopyScene.ID)
                                        {
                                            /// Bỏ qua
                                            return false;
                                        }
                                        /// Nếu không cùng nhóm
                                        else if (teammate.TeamID != player.TeamID)
                                        {
                                            /// Bỏ qua
                                            return false;
                                        }
                                        /// Có thể chọn
                                        return true;
                                    }).RandomRange(1).FirstOrDefault();
                                    /// Thông báo chuyển lượt đoán số
                                    this.NotifyAllPlayers(string.Format("Lượt đoán tiếp theo của [{0}].", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember.RoleName));
                                }

                                /// Bỏ qua
                                return;
                            }
                            /// Nếu là lượt của bản thân
                            else
                            {
                                /// Hiện bảng nhập số
                                KTPlayerManager.ShowInputNumberBox(player, string.Format("Nhập số cần đoán trong khoảng <color=green>{0} - {1}</color>", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMinValue, this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMaxValue), (number) => {
                                    /// Nếu không phải lượt bản thân
                                    if (this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember != player)
                                    {
                                        /// Thông báo không phải lượt đoán
                                        KTPlayerManager.ShowNotification(player, string.Format("Hiện đang là lượt đoán số của [{0}].", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember.RoleName));
                                        /// Bỏ qua
                                        return;
                                    }

                                    /// Tăng số lượt đã đoán
                                    this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberTotalTurns++;
                                    
                                    /// Nếu đây là số cần đoán
                                    if (number == this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberLuckyNumber)
                                    {
                                        /// Thông báo chuyển lượt đoán số
                                        this.NotifyAllPlayers(string.Format("Chúc mừng [{0}] đã đoán chính xác số cần tìm là {1}.", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember.RoleName, this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberLuckyNumber));
                                        /// Đã hoàn thành đoán số
                                        this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCompleted = true;
                                        /// Bỏ qua
                                        return;
                                    }
                                    /// Nếu số cần tìm lớn hơn số đã đoán
                                    else if (this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberLuckyNumber > number)
                                    {
                                        /// Đánh dấu lại khoảng Min
                                        this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMinValue = number;
                                    }
                                    /// Nếu số cần tìm nhỏ hơn số đã đoán
                                    else
                                    {
                                        /// Đánh dấu lại khoảng Max
                                        this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMaxValue = number;
                                    }

                                    /// Thông báo khoảng tương ứng
                                    this.NotifyAllPlayers(string.Format("[{0}] đoán không chính xác. Số cần đoán nằm trong khoảng {1} - {2}.", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember.RoleName, this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMinValue, this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCurrentMaxValue));

                                    /// Nếu đã quá số lượt đoán
                                    if (this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberTotalTurns >= task.GuessNumberConfig.MaxTurns)
                                    {
                                        /// Thông báo quá số lượt đoán
                                        this.NotifyAllPlayers("Đã quá số lượt đoán vẫn chưa tìm ra số cần tìm, trò chơi bắt đầu lại.");
                                        /// Bắt đầu trò chơi
                                        BeginGame();
                                    }
                                    /// Nếu chưa quá số lượt đoán
                                    else
                                    {
                                        /// Chuyển lượt qua cho thằng khác
                                        this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember = this.teamPlayers.Where((teammate) => {
                                            /// Nếu thằng này đã Offline
                                            if (!teammate.IsOnline())
                                            {
                                                /// Bỏ qua
                                                return false;
                                            }
                                            /// Nếu nó ở map khác
                                            else if (teammate.CurrentMapCode != this.CopyScene.MapCode || teammate.CurrentCopyMapID != this.CopyScene.ID)
                                            {
                                                /// Bỏ qua
                                                return false;
                                            }

                                            /// Có thể chọn
                                            return true;
                                        }).RandomRange(1).FirstOrDefault();
                                        /// Thông báo chuyển lượt đoán số
                                        this.NotifyAllPlayers(string.Format("Lượt đoán tiếp theo thuộc về [{0}].", this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberMember.RoleName));
                                    }
                                });
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

            /// Thông báo quá số lượt đoán
            this.NotifyAllPlayers(string.Format("Trò chơi đoán số bắt đầu. Hãy đối thoại với NPC {0} để tham gia.", task.NPC.Name));
            /// Bắt đầu trò chơi
            BeginGame();

            /// Chưa hoàn thành đoán số
            this.guessNumberRoundData[this.GetTaskKey(task)].GuessNumberCompleted = false;
        }

        /// <summary>
        /// Theo dõi nhiệm vụ đoán số
        /// </summary>
        /// <param name="task"></param>
        private bool Track_GuessNumber(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Toác
            if (!this.guessNumberRoundData.TryGetValue(this.GetTaskKey(task), out GuessNumberData data))
            {
                return false;
            }
            /// Kết thúc khi hoàn thành đoán số
            return data.GuessNumberCompleted;
        }

        /// <summary>
        /// Thiết lập lại dữ liệu nhiệm vụ mở cơ quan
        /// </summary>
        /// <param name="task"></param>
        private void Reset_GuessNumber(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            this.guessNumberRoundData.Remove(this.GetTaskKey(task));
        }
    }
}
