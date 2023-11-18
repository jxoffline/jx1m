using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Generic;

namespace GameServer.KiemThe.GameEvents.CargoCarriage
{
    /// <summary>
    /// Script NPC giao nhận tiêu
    /// </summary>
    public static partial class CargoCarriage_TransferNPCScript
    {
        /// <summary>
        /// Hiện Dialog thông báo tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <param name="text"></param>
        private static void ShowDialog(KPlayer player, NPC npc, string text)
        {
            /// Tạo NPCDialog
            KNPCDialog dialog = new KNPCDialog();
            dialog.Owner = player;
            
            /// Text
            dialog.Text = text;

            /// Selection
            dialog.Selections = new Dictionary<int, string>();
            dialog.Selections[-1000] = "Kết thúc đối thoại";

            /// Click chọn
            dialog.OnSelect = (data) =>
            {
                /// Đóng NPCDialog
                KT_TCPHandler.CloseDialog(player);
            };

            /// Thêm Dialog
            KTNPCDialogManager.AddNPCDialog(dialog);
            /// Hiện Dialog
            dialog.Show(npc, player);
        }

        /// <summary>
        /// Sự kiện Click vào NPC
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        public static void Click(KPlayer player, NPC npc, CargoCarriage.CargoCarriageData.NPCData npcData)
        {
            /// Nếu chưa mở
            if (!CargoCarriage.IsStarted)
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Sự kiện vận tiêu vẫn chưa được mở. Hãy quay lại sau.");
                return;
            }
            /// Nếu không có nhiệm vụ vận tiêu
            else if (player.CurrentCargoCarriageTask == null && player.CurrentCompletedCargoCarriageTask == null)
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Hãy mau mau đi tìm <color=yellow>[Ông chủ tiêu cục]</color> ở <color=green>các thành thị</color> để tham gia vận tiêu.");
                return;
            }

            /// Nếu đây là NPC nhận tiêu
            if (player.CurrentCargoCarriageTask != null && player.CurrentCargoCarriageTask.BeginNPC == npcData)
            {
                CargoCarriage_TransferNPCScript.ClickOnSourceNPC(player, npc, npcData);
            }
            /// Nếu đây là NPC trả tiêu
            else if (player.CurrentCompletedCargoCarriageTask != null && player.CurrentCompletedCargoCarriageTask.DoneNPC == npcData)
            {
                CargoCarriage_TransferNPCScript.ClickOnTargetNPC(player, npc, npcData);
            }
            /// Nếu đây là NPC dọc đường
            else
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi còn đứng đây làm gì nữa? Hãy mau mau vận tiêu đến địa điểm chỉ định kẻo hết thời gian.");
                return;
            }
        }
    }
}
