using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.CargoCarriage
{
    /// <summary>
    /// Script NPC giao tiêu
    /// </summary>
    public static partial class CargoCarriage_TransferNPCScript
    {
        /// <summary>
        /// Sự kiện Click vào NPC giao tiêu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <param name="npcData"></param>
        private static void ClickOnSourceNPC(KPlayer player, NPC npc, CargoCarriage.CargoCarriageData.NPCData npcData)
        {
            /// Nếu đã có xe tiêu
            if (player.CurrentTraderCarriage != null)
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi còn đứng đây làm gì nữa? Hãy mau mau vận tiêu đến địa điểm chỉ định kẻo hết thời gian.");
                return;
            }

            /// Text tương ứng
            string text;

            /// Thông tin quãng đường vận tiêu
            CargoCarriage.CargoCarriageData.CargoData pathData = CargoCarriage.Data.Paths.Where(xx => xx.Type == player.CurrentCargoCarriageTask.Type).FirstOrDefault();
            /// Toác
            if (pathData == null)
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Không tìm thấy dữ liệu vận tiêu. Hãy thử lại.");
                return;
            }

            /// Cấp độ không thỏa mãn
            if (player.m_Level < pathData.RequireLevel)
            {
                CargoCarriage_TransferNPCScript.ShowDialog(player, npc, string.Format("Ngươi cần đạt <color=green>cấp {0}</color> mới có thể nhận vận chuyển xe tiêu loại này.", pathData.RequireLevel));
                return;
            }

            /// Số bạc cần
            int requireMoney = pathData.RequireMoney;
            /// Số bạc khóa cần
            int requireBoundMoney = pathData.RequireBoundMoney;
            /// Số KNB cần
            int requireToken = pathData.RequireToken;
            /// Số KNB khóa cần
            int requireBoundToken = pathData.RequireBoundToken;

            /// Chuỗi thông tin đặt cược
            string requirationText = "";

            /// Nếu có bạc
            if (requireMoney > 0)
            {
                requirationText += string.Format("   - Bạc: <color=yellow>{0}</color>\n", KTGlobal.GetDisplayMoney(requireMoney));
            }
            /// Nếu có bạc khóa
            if (requireMoney > 0)
            {
                requirationText += string.Format("   - Bạc khóa: <color=yellow>{0}</color>\n", KTGlobal.GetDisplayMoney(requireBoundMoney));
            }
            /// Nếu có KNB
            if (requireToken > 0)
            {
                requirationText += string.Format("   - KNB: <color=yellow>{0}</color>\n", KTGlobal.GetDisplayMoney(requireToken));
            }
            /// Nếu có KNB khóa
            if (requireBoundToken > 0)
            {
                requirationText += string.Format("   - KNB khóa: <color=yellow>{0}</color>\n", KTGlobal.GetDisplayMoney(requireBoundToken));
            }

            /// Nếu trong cùng bản đồ
            if (player.CurrentCargoCarriageTask.MovePath.FromMapID == player.CurrentCargoCarriageTask.MovePath.ToMapID)
            {
                text = string.Format("Ngươi đến đúng lúc lắm. Ta có chút hàng hóa muốn nhờ ngươi vận chuyển <color=yellow>[{0}]</color>. Vì giá trị của món hàng này, người phải đặt cược ta mới yên tâm giao cho ngươi. Ngươi cần đặt cược:\n{1}Sau khi vận tiêu thành công, tất cả tiền cược của ngươi sẽ được trả lại. Nếu đồng ý thì đã còn đợi gì nữa? Mau mau bắt đầu thôi.", player.CurrentCargoCarriageTask.DoneNPC.Name, requirationText);
            }
            /// Nếu khác bản đồ
            else
            {
                /// Tên bản đồ
                string mapName = KTMapManager.Find(player.CurrentCargoCarriageTask.MovePath.ToMapID).MapName;
                text = string.Format("Ngươi đến đúng lúc lắm. Ta có chút hàng hóa muốn nhờ ngươi vận chuyển đến cho <color=yellow>[{0}]</color> ở <color=green>{1}</color>. Vì giá trị của món hàng này, người phải đặt cược ta mới yên tâm giao cho ngươi. Ngươi cần đặt cược:\n{2}Sau khi vận tiêu thành công, tất cả tiền cược của ngươi sẽ được trả lại. Nếu đồng ý thì đã còn đợi gì nữa? Mau mau bắt đầu thôi.", player.CurrentCargoCarriageTask.DoneNPC.Name, mapName, requirationText);
            }

            /// Tạo NPCDialog
            KNPCDialog dialog = new KNPCDialog();
            dialog.Owner = player;
            dialog.Selections = new Dictionary<int, string>();
            dialog.Text = text;
            dialog.Selections[-1] = "Ta đã sẵn sàng";
            dialog.Selections[-1000] = "Kết thúc đối thoại";
            /// Sự kiện Select
            dialog.OnSelect = (data) =>
            {
                /// Thực thi sự kiện
                CargoCarriage_TransferNPCScript.HandleSourceNPCSelection(player, npc, npcData, data.SelectID);
            };
            /// Thêm Dialog
            KTNPCDialogManager.AddNPCDialog(dialog);
            /// Hiện Dialog
            dialog.Show(npc, player);
        }

        /// <summary>
        /// Sự kiện Click vào Dialog của NPC giao tiêu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <param name="npcData"></param>
        /// <param name="selectID"></param>
        private static void HandleSourceNPCSelection(KPlayer player, NPC npc, CargoCarriage.CargoCarriageData.NPCData npcData, int selectID)
        {
            /// Kết thúc đối thoại
            if (selectID == -1000)
            {
                /// Đóng NPCDialog
                KT_TCPHandler.CloseDialog(player);
                /// Bỏ qua
                return;
            }

            /// Bắt đầu vận tiêu
            if (selectID == -1)
            {
                /// Đóng NPCDialog
                KT_TCPHandler.CloseDialog(player);

                /// Nếu chưa mở
                if (!CargoCarriage.IsStarted)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Sự kiện vận tiêu vẫn chưa được mở. Hãy quay lại sau.");
                    return;
                }
                /// Nếu không có nhiệm vụ vận tiêu
                else if (player.CurrentCargoCarriageTask == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi chưa nhận nhiệm vụ vận tiêu, không thể nhận tiêu xa.");
                    return;
                }
                /// Nếu không phải NPC nhận tiêu
                else if (player.CurrentCargoCarriageTask.BeginNPC != npcData)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi còn đứng đây làm gì nữa? Hãy mau mau vận tiêu đến địa điểm chỉ định kẻo hết thời gian.");
                    return;
                }
                /// Nếu đã có xe tiêu
                else if (player.CurrentTraderCarriage != null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Ngươi còn đứng đây làm gì nữa? Hãy mau mau vận tiêu đến địa điểm chỉ định kẻo hết thời gian.");
                    return;
                }

                /// Nhiệm vụ tương ứng
                CargoCarriage.CargoCarriageTaskData taskData = player.CurrentCargoCarriageTask;
                /// Loại xe tiêu
                int type = taskData.Type;

                /// Thông tin quãng đường vận tiêu
                CargoCarriage.CargoCarriageData.CargoData pathData = CargoCarriage.Data.Paths.Where(xx => xx.Type == type).FirstOrDefault();
                /// Toác
                if (pathData == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Không tìm thấy dữ liệu vận tiêu. Hãy thử lại.");
                    return;
                }

                /// Cấp độ không thỏa mãn
                if (player.m_Level < pathData.RequireLevel)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, string.Format("Ngươi cần đạt <color=green>cấp {0}</color> mới có thể nhận vận chuyển xe tiêu loại này.", pathData.RequireLevel));
                    return;
                }


                
                /// Số bạc cần
                int requireMoney = pathData.RequireMoney;
                /// Số bạc khóa cần
                int requireBoundMoney = pathData.RequireBoundMoney;
                /// Số KNB cần
                int requireToken = pathData.RequireToken;
                /// Số KNB khóa cần
                int requireBoundToken = pathData.RequireBoundToken;
                /// Nếu không đủ
                if (player.Money < requireMoney)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Số bạc mang theo không đủ.");
                    return;
                }
                else if (player.BoundMoney < requireBoundMoney)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Số bạc khóa mang theo không đủ.");
                    return;
                }
                else if (player.Token < requireToken)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Số đồng mang theo không đủ.");
                    return;
                }
                else if (player.BoundToken < requireBoundToken)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Số đồng khóa mang theo không đủ.");
                    return;
                }

                /// Chọn cho nó 1 cái xe tiêu tương ứng
                CargoCarriage.CargoCarriageData.CarriageData carriageData = CargoCarriage.Data.Carriages.Where(xx => xx.Type == type).RandomRange(1).FirstOrDefault();
                /// Toác
                if (carriageData == null)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Không thể khởi tạo xe tiêu. Hãy thử lại.");
                    return;
                }

                /// Quãng đường di chuyển
                List<KeyValuePair<int, UnityEngine.Vector2Int>> movePaths = KTGlobal.FindPaths(taskData.MovePath.FromMapID, new UnityEngine.Vector2Int(taskData.BeginNPC.PosX, taskData.BeginNPC.PosY), taskData.MovePath.ToMapID, new UnityEngine.Vector2Int(taskData.DoneNPC.PosX, taskData.DoneNPC.PosY));
                /// Nếu không tìm thấy
                if (movePaths == null || movePaths.Count <= 0)
                {
                    CargoCarriage_TransferNPCScript.ShowDialog(player, npc, "Không thể khởi tạo quãng đường vận tiêu. Hãy thử lại.");
                    return;
                }

                int BEFOREVALUE = 0;
                int AFTERVALUE = 0;
                /// Nếu có bạc
                if (requireMoney > 0)
                {
                    BEFOREVALUE = player.Money;
                    /// Thêm bạc cho nó
                    KTPlayerManager.SubMoney(player, requireMoney, "CargoCarriage");

                    AFTERVALUE= player.Money;
                }
                /// Nếu có bạc khóa
                if (requireBoundMoney > 0)
                {
                    /// Thêm bạc khóa cho nó
                    KTPlayerManager.SubBoundMoney(player, requireBoundMoney, "CargoCarriage");
                }
                /// Nếu có KNB
                if (requireToken > 0)
                {
                    BEFOREVALUE = player.Token;
                    /// Thêm KNB cho nó
                    KTPlayerManager.SubToken(player, requireToken, "CargoCarriage");

                    AFTERVALUE = player.Token;

                }
                /// Nếu có bạc khóa
                if (requireBoundToken > 0)
                {
                    /// Thêm bạc khóa cho nó
                    KTPlayerManager.SubBoundToken(player, requireBoundToken, "CargoCarriage");
                }

                LogManager.WriteLog(LogTypes.CargoCarriage, "[" + player.RoleID + "][" + player.RoleName + "]  requireMoney:" + requireMoney + "| requireToken :" + requireToken + "|BEFOREVALUE :" + BEFOREVALUE + "| AFTERVALUE :" + AFTERVALUE);
                /// Tạo xe tiêu
                TraderCarriage carriage = KTTraderCarriageManager.CreateTraderCarriage(player, carriageData.Type, carriageData.ResID, carriageData.Name, carriageData.Vision, carriageData.MoveSpeed, carriageData.MaxHP, pathData.LimitTime, movePaths);
                /// Gửi gói tin thông báo bắt đầu nhiệm vụ vận tiêu
                KT_TCPHandler.SendUpdateCargoCarriageTaskState(player, 2, pathData.LimitTime / 1000);
                /// Sự kiện khi chết
                carriage.OnBeKilled = (killer) =>
                {
                    /// Thực thi hàm khi xe tiêu bị chết
                    CargoCarriage_ActivityScript.ProcessCarriageBeKilled(carriage, taskData, killer, carriage.CurrentMapCode, (int) carriage.CurrentPos.X, (int) carriage.CurrentPos.Y);

                    /// Gửi gói tin thông báo hủy nhiệm vụ vận tiêu
                    KT_TCPHandler.SendUpdateCargoCarriageTaskState(player, 0);
                };
                /// Sự kiện khi hoàn thành
                carriage.Complete = () =>
                {
                    /// Gửi gói tin thông báo hoàn thành nhiệm vụ vận tiêu
                    KT_TCPHandler.SendUpdateCargoCarriageTaskState(player, 1);
                };

                /// Nếu có thông báo lên kênh
                if (pathData.NotifySystem)
                {
                    /// Tên bản đồ
                    string mapName = "Chưa rõ";
                    /// Thông tin bản đồ
                    GameMap gameMap = KTMapManager.Find(taskData.MovePath.FromMapID);
                    /// Nếu tồn tại
                    if (gameMap != null)
                    {
                        mapName = gameMap.MapName;
                    }

                    /// Thông báo lên kênh
                    KTGlobal.SendSystemChat(string.Format("Người chơi <color=#69b9f2>[{0}]</color> đã nhận vận chuyển <color=yellow>{1}</color> tại <color=green>{2}</color>.", player.RoleName, carriage.RoleName, mapName));
                }

                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Bắt đầu vận tiêu!");
            }
        }
    }
}
