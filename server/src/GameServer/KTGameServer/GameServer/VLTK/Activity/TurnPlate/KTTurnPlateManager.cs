using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.TurnPlate
{
    /// <summary>
    /// Quản lý Vòng quay may mắn - đặc biệt
    /// </summary>
    public static class KTTurnPlateManager
    {
        #region Define
        /// <summary>
        /// Dữ liệu Vòng quay may mắn
        /// </summary>
        private static KTTurnPlate Data;
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            /// Đối tượng XML tương ứng đọc từ File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Activity/KTTurnPlate.xml");
            /// Chuyển sang dạng đối tượng
            KTTurnPlate data = KTTurnPlate.Parse(xmlNode);
            KTTurnPlateManager.Data = data;
        }
        #endregion

        #region Core
        /// <summary>
        /// Sự kiện Vòng quay may mắn có mở không
        /// </summary>
        /// <returns></returns>
        public static bool IsOpened()
        {
            /// Nếu thiết lập không mở
            if (!KTTurnPlateManager.Data.Config.Activate)
            {
                return false;
            }

            /// Thời điểm hiện tại
            DateTime now = DateTime.Now;
            TimeSpan currentPeriod = new TimeSpan(now.Hour, now.Minute, now.Second);

            /// Nếu không phải ngày trong tuần
            if (!KTTurnPlateManager.Data.Time.WeekDays.Contains(now.DayOfWeek))
            {
                return false;
            }

            /// Duyệt danh sách các mốc giờ
            foreach (KTTurnPlate.EventTime.TimeStamp timeStamp in KTTurnPlateManager.Data.Time.Times)
            {
                /// Thời điểm bắt đầu
                TimeSpan startPeriod = new TimeSpan(timeStamp.Hour, timeStamp.Minute, 0);
                /// Thời điểm kết thúc
                TimeSpan endPeriod = startPeriod.Add(new TimeSpan(0, 0, KTTurnPlateManager.Data.Time.Duration / 1000));

                /// Nếu nằm trong thời điểm diễn ra
                if (startPeriod <= currentPeriod && currentPeriod <= endPeriod)
                {
                    return true;
                }
            }

            /// Không tìm thấy
            return false;
        }

        /// <summary>
        /// Mở vòng quay
        /// </summary>
        /// <param name="player"></param>
        public static void OpenCircle(KPlayer player)
        {
            /// Nếu chưa kích hoạt
            if (!KTTurnPlateManager.Data.Config.Activate)
            {
                KTPlayerManager.ShowNotification(player, "Vòng quay may mắn - đặc biệt hiện chưa mở, hãy quay lại sau!");
                return;
            }
            /// Nếu cấp độ không đủ
            else if (player.m_Level < KTTurnPlateManager.Data.Config.LimitLevel)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể mở vòng quay may mắn!", KTTurnPlateManager.Data.Config.LimitLevel));
                return;
            }

            /// Danh sách vật phẩm trong vòng quay
            List<KeyValuePair<int, int>> items = KTTurnPlateManager.Data.CellDataToExport;
            /// Chuỗi dữ liệu gửi đi
            string strcmd = string.Format("{0}:{1}:{2}", 0, player.TurnPlate_LastStopPos, string.Join("|", items.Select(x => string.Format("{0}_{1}", x.Key, x.Value))));
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_TURNPLATE, strcmd);
        }

        /// <summary>
        /// Kiểm tra người chơi có phần thưởng chưa nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasAward(KPlayer player)
        {
            return player.TurnPlate_LastStopPos >= 0 && player.TurnPlate_LastStopPos < KTTurnPlateManager.Data.Cells.Count;
        }

        /// <summary>
        /// Nhận thưởng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool GetAward(KPlayer player)
        {
            /// Nếu thao tác quá nhanh
            if (KTGlobal.GetCurrentTimeMilis() - player.TurnPlate_LastTicks < 1000)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác quá nhanh, hãy đợi giây lát và thử lại!");
                return false;
            }
            /// Đánh dấu thời điểm
            player.TurnPlate_LastTicks = KTGlobal.GetCurrentTimeMilis();

            /// Nếu không có quà
            if (!KTTurnPlateManager.HasAward(player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Không có phần thưởng có thể nhận.");
                /// Trả về kết quả
                return false;
            }
            /// Thông tin ô tương ứng
            KTTurnPlate.CellInfo cell = KTTurnPlateManager.Data.Cells[player.TurnPlate_LastStopPos];
            /// Số ô trống yêu cầu
            int spaceNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(cell.ItemID, cell.Quantity);

            /// Tăng số lượt quay hiện tại
            int totalTurn = player.TurnPlate_TotalTurn;
            if (totalTurn < 0)
            {
                totalTurn = 0;
            }

            /// Check Point vật phẩm cố định tương ứng
            if (KTTurnPlateManager.Data.FixedCheckPointItems.TryGetValue(totalTurn, out KTTurnPlate.FixedCheckPointItemData fixedCheckPointItem))
            {
                spaceNeed += KTGlobal.GetTotalSpacesNeedToTakeItem(fixedCheckPointItem.ItemID, fixedCheckPointItem.Quantity);
            }

            /// Nếu không đủ ô trống
            if (!KTGlobal.IsHaveSpace(spaceNeed, player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Cần sắp xếp tối thiểu {0} ô trống trong túi để nhận phần thưởng!", spaceNeed));
                /// Trả về kết quả
                return false;
            }

            /// Đánh dấu vị trí dừng
            player.TurnPlate_LastStopPos = -1;
            /// Thêm vật phẩm tương ứng
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, cell.ItemID, cell.Quantity, 0, "TurnPlate", true, 0, false, ItemManager.ConstGoodsEndTime))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                /// Ghi Log
                LogManager.WriteLog(LogTypes.TurnPlate, string.Format("[ERROR] Player {0} (ID: {1}) get award failed, CellID: {2}.", player.RoleName, player.RoleID, cell.ID));
                /// Trả về kết quả
                return false;
            }

            /// Check Point vật phẩm cố định tương ứng
            if (fixedCheckPointItem != null)
            {
                /// Thêm vật phẩm tương ứng
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, fixedCheckPointItem.ItemID, fixedCheckPointItem.Quantity, 0, "TurnPlate", true, 1, false, ItemManager.ConstGoodsEndTime))
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.TurnPlate, string.Format("[ERROR] Player {0} (ID: {1}) get award failed, CellID: {2}.", player.RoleName, player.RoleID, cell.ID));
                    /// Trả về kết quả
                    return false;
                }
            }

            /// Nếu có thiết lập thông báo khi nhận quà
            if (cell.NotifyAfterTaken)
            {
                /// Check Point vật phẩm cố định tương ứng
                if (fixedCheckPointItem != null && fixedCheckPointItem.NotifyAfterTaken)
                {
                    GoodsData itemGD = new GoodsData()
                    {
                        GoodsID = cell.ItemID,
                    };
                    GoodsData fixedItemGD = new GoodsData()
                    {
                        GoodsID = fixedCheckPointItem.ItemID,
                    };
                    KTGlobal.SendSystemEventNotification(string.Format("Trong sự kiện <color=yellow>Vòng quay may mắn - đặc biệt</color>, <color=#42a1ff>{0}</color> đã may mắn nhận được <color=#fff642>[{1}]</color> đồng thời nhận được <color=#fff642>[{2}]</color> sau khi đạt <color=green>{3}</color> lượt quay.", player.RoleName, KTGlobal.GetItemName(itemGD), KTGlobal.GetItemName(fixedItemGD), totalTurn));
                }
                /// Không có
                else
                {
                    GoodsData itemGD = new GoodsData()
                    {
                        GoodsID = cell.ItemID,
                    };
                    KTGlobal.SendSystemEventNotification(string.Format("Trong sự kiện <color=yellow>Vòng quay may mắn - đặc biệt</color>, <color=#42a1ff>{0}</color> đã may mắn nhận được <color=#fff642>[{1}]</color>.", player.RoleName, KTGlobal.GetItemName(itemGD)));
                }
            }
            /// Nếu không thông báo khi nhận quà
            else
            {
                /// Check Point vật phẩm cố định tương ứng
                if (fixedCheckPointItem != null && fixedCheckPointItem.NotifyAfterTaken)
                {
                    GoodsData fixedItemGD = new GoodsData()
                    {
                        GoodsID = fixedCheckPointItem.ItemID,
                    };
                    KTGlobal.SendSystemEventNotification(string.Format("Trong sự kiện <color=yellow>Vòng quay may mắn - đặc biệt</color>, <color=#42a1ff>{0}</color> đã nhận được <color=#fff642>[{1}]</color> sau khi đạt <color=green>{2}</color> lượt quay.", player.RoleName, KTGlobal.GetItemName(fixedItemGD), totalTurn));
                }
            }

            /// Check Point vật phẩm cố định tương ứng
            if (fixedCheckPointItem != null)
            {
                /// Ghi Log
                LogManager.WriteLog(LogTypes.TurnPlate, string.Format("Player {0} (ID: {1}) get award successfully, CellID: {2} (ItemID: {3}, Quantity: {4}). Also get fixed awards CheckPoint: {5}, ItemID: {6}, Quantity: {7}", player.RoleName, player.RoleID, cell.ID, cell.ItemID, cell.Quantity, totalTurn, fixedCheckPointItem.ItemID, fixedCheckPointItem.Quantity));
            }
            else
            {
                /// Ghi Log
                LogManager.WriteLog(LogTypes.TurnPlate, string.Format("Player {0} (ID: {1}) get award successfully, CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, cell.ID, cell.ItemID, cell.Quantity));
            }

            /// Trả về kết quả
            return true;
        }

        /// <summary>
        /// Bắt đầu quay Vòng quay may mắn - đặc biệt
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stopPos"></param>
        /// <returns></returns>
        public static bool StartTurn(KPlayer player, out int stopPos)
        {
            /// Đánh dấu vị trí dừng
            stopPos = -1;
            /// Nếu cấp độ không đủ
            if (player.m_Level < KTTurnPlateManager.Data.Config.LimitLevel)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể bắt đầu lượt quay!", KTTurnPlateManager.Data.Config.LimitLevel));
                /// Trả về kết quả
                return false;
            }

            /// Nếu thao tác quá nhanh
            if (KTGlobal.GetCurrentTimeMilis() - player.TurnPlate_LastTicks < 1000)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác quá nhanh, hãy đợi giây lát và thử lại!");
                return false;
            }
            /// Đánh dấu thời điểm
            player.TurnPlate_LastTicks = KTGlobal.GetCurrentTimeMilis();

            /// Nếu số lượng vật phẩm không đủ
            if (ItemManager.GetItemCountInBag(player, KTTurnPlateManager.Data.Requiration.RequireItemID) <= 0)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Cần có [{0}] mới có thể bắt đầu lượt quay!", KTGlobal.GetItemName(KTTurnPlateManager.Data.Requiration.RequireItemID)));
                /// Trả về kết quả
                return false;
            }

            /// Nếu có quà chưa nhận
            if (KTTurnPlateManager.HasAward(player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Có phần thưởng chưa nhận, hãy ấn Nhận thưởng trước tiên.");
                /// Trả về kết quả
                return false;
            }

            /// Thực hiện xóa vật phẩm
            ItemManager.RemoveItemFromBag(player, KTTurnPlateManager.Data.Requiration.RequireItemID, 1);

            /// Tăng số lượt quay hiện tại
            int totalTurn = player.TurnPlate_TotalTurn;
            if (totalTurn < 0)
            {
                totalTurn = 0;
            }
            player.TurnPlate_TotalTurn = totalTurn + 1;

            /// Xem có CheckPoint nào thỏa mãn không
            List<KTTurnPlate.CellInfo> checkPointCells = KTTurnPlateManager.Data.Cells.Where(x => x.CheckPoints != null && x.CheckPoints.Contains(player.TurnPlate_TotalTurn)).ToList();
            /// Nếu có CheckPoint thỏa mãn
            if (checkPointCells.Count > 0)
            {
                /// Chọn một CheckPoint ngẫu nhiên
                KTTurnPlate.CellInfo checkPointCell = checkPointCells[KTGlobal.GetRandomNumber(0, checkPointCells.Count - 1)];
                /// Đánh dấu vị trí dừng
                stopPos = checkPointCell.ID;
                /// Lưu lại
                player.TurnPlate_LastStopPos = stopPos;
                /// Ghi Log
                LogManager.WriteLog(LogTypes.TurnPlate, string.Format("Player {0} (ID: {1}) start turn and get checkpoint CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, stopPos, checkPointCell.ItemID, checkPointCell.Quantity));
                /// Trả về kết quả
                return true;
            }
            /// Nếu không có CheckPoint thỏa mãn
            else
            {
                /// Tỷ lệ may mắn tối thiểu
                KTTurnPlate.CellInfo minCell = KTTurnPlateManager.Data.Cells.MinBy(x => x.Rate);
                KTTurnPlate.CellInfo maxCell = KTTurnPlateManager.Data.Cells.MaxBy(x => x.Rate);
                int minRate = minCell.Rate;
                int maxRate = maxCell.Rate;
                /// Random lại tỷ lệ
                int rate = KTGlobal.GetRandomNumber(minRate, maxRate);
                /// Danh sách các ô thỏa mãn
                List<KTTurnPlate.CellInfo> cells = KTTurnPlateManager.Data.Cells.Where(x => x.CheckPoints == null && x.Rate >= rate).ToList();
                /// Toác
                if (cells.Count <= 0)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Thao tác quay bị lỗi, hãy liên hệ với Admin để báo cáo.");
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.TurnPlate, string.Format("[ERROR] Player {0} (ID: {1}) start turn failed, random cells list is empty.", player.RoleName, player.RoleID));
                    /// Trả về kết quả
                    return false;
                }
                /// Chọn một ô ngẫu nhiên
                KTTurnPlate.CellInfo randomCell = cells[KTGlobal.GetRandomNumber(0, cells.Count - 1)];
                /// Đánh dấu vị trí dừng
                stopPos = randomCell.ID;
                /// Lưu lại
                player.TurnPlate_LastStopPos = stopPos;
                /// Ghi Log
                LogManager.WriteLog(LogTypes.TurnPlate, string.Format("Player {0} (ID: {1}) start turn and get CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, stopPos, randomCell.ItemID, randomCell.Quantity));
                /// Trả về kết quả
                return true;
            }
        }
        #endregion
    }
}
