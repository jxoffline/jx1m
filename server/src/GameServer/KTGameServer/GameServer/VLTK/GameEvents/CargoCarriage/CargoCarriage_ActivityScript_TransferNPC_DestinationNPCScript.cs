using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.GameEvents.CargoCarriage
{
    /// <summary>
    /// Script NPC nhận tiêu
    /// </summary>
    public static partial class CargoCarriage_TransferNPCScript
    {
        /// <summary>
        /// Sự kiện Click vào NPC trả tiêu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <param name="npcData"></param>
        private static void ClickOnTargetNPC(KPlayer player, NPC npc, CargoCarriage.CargoCarriageData.NPCData npcData)
        {
            /// Tạo NPCDialog
            KNPCDialog dialog = new KNPCDialog();
            dialog.Owner = player;
            dialog.Selections = new Dictionary<int, string>();
            dialog.Text = string.Format("Ngươi đây rồi, ta đợi ngươi mãi. Kiện hàng của <color=yellow>[{0}]</color> xem ra đã làm khổ ngươi rồi. Đây, ta có chút phần thưởng cho ngươi, vui lòng nhận.", player.CurrentCompletedCargoCarriageTask.BeginNPC.Name);
            dialog.Selections[-2] = "Nhận thưởng vận tiêu";
            dialog.Selections[-1000] = "Kết thúc đối thoại";
            /// Sự kiện Select
            dialog.OnSelect = (data) =>
            {
                /// Thực thi sự kiện
                CargoCarriage_TransferNPCScript.HandleDestinationNPCSelection(player, npc, npcData, data.SelectID);
            };
            /// Thêm Dialog
            KTNPCDialogManager.AddNPCDialog(dialog);
            /// Hiện Dialog
            dialog.Show(npc, player);
        }

        /// <summary>
        /// Sự kiện Click vào Dialog của NPC trả tiêu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <param name="npcData"></param>
        /// <param name="selectID"></param>
        private static void HandleDestinationNPCSelection(KPlayer player, NPC npc, CargoCarriage.CargoCarriageData.NPCData npcData, int selectID)
        {
            /// Kết thúc đối thoại
            if (selectID == -1000)
            {
                /// Đóng NPCDialog
                KT_TCPHandler.CloseDialog(player);
                /// Bỏ qua
                return;
            }

            /// Nhận thưởng vận tiêu
            if (selectID == -2)
            {
                /// Nếu không có nhiệm vụ vận tiêu đã hoàn thành
                if (player.CurrentCompletedCargoCarriageTask == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi chưa hoàn thành nhiệm vụ vận tiêu, không thể nhận thưởng.");
                    return;
                }
                /// Nếu không phải NPC trả tiêu
                else if (player.CurrentCompletedCargoCarriageTask.DoneNPC != npcData)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ta không có trách nhiệm phát thưởng ngươi. Hãy tìm đúng người.");
                    return;
                }

                /// Thông tin nhiệm vụ
                CargoCarriage.CargoCarriageTaskData taskData = player.CurrentCompletedCargoCarriageTask;
                /// Phần thưởng tương ứng
                CargoCarriage.CargoCarriageData.AwardData awardData = CargoCarriage.Data.Awards.Where(xx => xx.Type == taskData.Type).FirstOrDefault();
                /// Toác
                if (awardData == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Thông tin phần thưởng bị lỗi.");
                    return;
                }

                /// Tổng số ô trống cần
                int spacesNeed = 0;
                /// Duyệt danh sách vật phẩm thưởng
                foreach (CargoCarriage.CargoCarriageData.EventItem itemInfo in awardData.AwardItems)
                {
                    /// Tăng số ô trống cần lên
                    spacesNeed += KTGlobal.GetTotalSpacesNeedToTakeItem(itemInfo.ItemID, itemInfo.Quantity);
                }

                /// Nếu không đủ ô trống
                if (!KTGlobal.IsHaveSpace(spacesNeed, player))
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi cần sắp xếp lại tối thiểu <color=yellow>{0} ô trống</color> trong túi đồ để nhận thưởng!");
                    return;
                }

                /// Thông tin vận tiêu
                CargoCarriage.CargoCarriageData.CargoData pathData = CargoCarriage.Data.Paths.Where(xx => xx.Type == taskData.Type).FirstOrDefault();
                /// Toác
                if (pathData == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Thông tin phần thưởng bị lỗi.");
                    return;
                }

                /// Hủy nhiệm vụ đã hoàn thành của nó
                player.CurrentCompletedCargoCarriageTask = null;

                /// Tổng số bạc sẽ nhận được, bao gồm quà thưởng và vốn đã cọc
                int totalMoney = awardData.Money + pathData.RequireMoney;
                /// Tổng số bạc khóa sẽ nhận được, bao gồm quà thưởng và vốn đã cọc
                int totalBoundMoney = awardData.BoundMoney + pathData.RequireBoundMoney;
                /// Tổng số KNB sẽ nhận được, bao gồm quà thưởng và vốn đã cọc
                int totalToken = awardData.Token + pathData.RequireToken;
                /// Tổng số KNB khóa sẽ nhận được, bao gồm quà thưởng và vốn đã cọc
                int totalBoundToken = awardData.BoundToken + pathData.RequireBoundToken;

                /// Nếu có bạc
                if (totalMoney > 0)
                {
                    /// Thêm bạc cho nó
                    KTPlayerManager.AddMoney(player, totalMoney, "CargoCarriage");
                }
                /// Nếu có bạc khóa
                if (totalBoundMoney > 0)
                {
                    /// Thêm bạc khóa cho nó
                    KTPlayerManager.AddBoundMoney(player, totalBoundMoney, "CargoCarriage");
                }
                /// Nếu có KNB
                if (totalToken > 0)
                {
                    /// Thêm KNB cho nó
                    KTPlayerManager.AddToken(player, totalToken, "CargoCarriage");
                }
                /// Nếu có bạc khóa
                if (totalBoundToken > 0)
                {
                    /// Thêm bạc khóa cho nó
                    KTPlayerManager.AddBoundToken(player, totalBoundToken, "CargoCarriage");
                }

                /// Duyệt danh sách vật phẩm thưởng
                foreach (CargoCarriage.CargoCarriageData.EventItem itemInfo in awardData.AwardItems)
                {
                    /// Thêm cho nó
                    ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, itemInfo.ItemID, itemInfo.Quantity, 0, "CargoCarriage", true, itemInfo.Bound ? 1 : 0, false, ItemManager.ConstGoodsEndTime, "", -1);
                }

                /// Nếu có bang hội
                if (player.GuildID > 0)
                {
                    /// Tính nhiệm vụ nếu bang có nhiệm vụ vận tiêu
                    GuildManager.getInstance().TaskProsecc(player.GuildID, player, -1, TaskTypes.CarriageTotalCount);
                }
                /// Gửi gói tin thông báo hủy nhiệm vụ vận tiêu
                KT_TCPHandler.SendUpdateCargoCarriageTaskState(player, 0);

                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Nhận thưởng vận tiêu thành công.");
            }
        }
    }
}
