using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.LuckyCircle
{
    /// <summary>
    /// Quản lý Vòng quay may mắn
    /// </summary>
    public static class KTLuckyCircleManager
    {
        /// <summary>
        /// Phương thức chơi Vòng quay may mắn
        /// </summary>
        public enum LuckyCirclePlayMethod
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Sử dụng đồng
            /// </summary>
            UseToken = 1,
            /// <summary>
            /// Sử dụng đồng khóa
            /// </summary>
            UseBoundToken = 2,
            /// <summary>
            /// Sử dụng vật phẩm
            /// </summary>
            UseItem = 3,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        #region Define
        /// <summary>
        /// Dữ liệu Vòng quay may mắn
        /// </summary>
        private static KTLuckyCircle Data;
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            /// Đối tượng XML tương ứng đọc từ File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Activity/KTLuckyCircle.xml");
            /// Chuyển sang dạng đối tượng
            KTLuckyCircle data = KTLuckyCircle.Parse(xmlNode);
            KTLuckyCircleManager.Data = data;
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
            if (!KTLuckyCircleManager.Data.Config.Activate)
            {
                return false;
            }

            /// Thời điểm hiện tại
            DateTime now = DateTime.Now;
            TimeSpan currentPeriod = new TimeSpan(now.Hour, now.Minute, now.Second);

            /// Nếu không phải ngày trong tuần
            if (!KTLuckyCircleManager.Data.Time.WeekDays.Contains(now.DayOfWeek))
            {
                return false;
            }

            /// Duyệt danh sách các mốc giờ
            foreach (KTLuckyCircle.EventTime.TimeStamp timeStamp in KTLuckyCircleManager.Data.Time.Times)
            {
                /// Thời điểm bắt đầu
                TimeSpan startPeriod = new TimeSpan(timeStamp.Hour, timeStamp.Minute, 0);
                /// Thời điểm kết thúc
                TimeSpan endPeriod = startPeriod.Add(new TimeSpan(0, 0, KTLuckyCircleManager.Data.Time.Duration / 1000));

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
            if (!KTLuckyCircleManager.Data.Config.Activate)
            {
                KTPlayerManager.ShowNotification(player, "Vòng quay may mắn hiện chưa mở, hãy quay lại sau!");
                return;
            }
            /// Nếu cấp độ không đủ
            else if (player.m_Level < KTLuckyCircleManager.Data.Config.LimitLevel)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể mở vòng quay may mắn!", KTLuckyCircleManager.Data.Config.LimitLevel));
                return;
            }

            G2C_LuckyCircle luckyCircle = new G2C_LuckyCircle()
            {
                Items = KTLuckyCircleManager.Data.CellDataToExport,
                LastStopPos = KTLuckyCircleManager.HasAward(player) ? player.LuckyCircle_LastStopPos : -1,
                Fields = new int[] { 0 },
            };
            player.SendPacket<G2C_LuckyCircle>((int) TCPGameServerCmds.CMD_KT_LUCKYCIRCLE, luckyCircle);
        }

        /// <summary>
        /// Kiểm tra người chơi có phần thưởng chưa nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasAward(KPlayer player)
        {
            return player.LuckyCircle_LastStopPos >= 0 && player.LuckyCircle_LastStopPos < KTLuckyCircleManager.Data.Cells.Count;
        }

        /// <summary>
        /// Nhận thưởng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool GetAward(KPlayer player)
        {
            /// Nếu thao tác quá nhanh
            if (KTGlobal.GetCurrentTimeMilis() - player.LuckyCircle_LastTicks < 1000)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác quá nhanh, hãy đợi giây lát và thử lại!");
                return false;
            }
            /// Đánh dấu thời điểm
            player.LuckyCircle_LastTicks = KTGlobal.GetCurrentTimeMilis();

            /// Nếu không có quà
            if (!KTLuckyCircleManager.HasAward(player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Không có phần thưởng có thể nhận.");
                /// Trả về kết quả
                return false;
            }
            /// Thông tin ô tương ứng
            KTLuckyCircle.CellInfo cell = KTLuckyCircleManager.Data.Cells[player.LuckyCircle_LastStopPos];
            /// Số ô trống yêu cầu
            int spaceNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(cell.ItemID, cell.Quantity);
            /// Nếu không đủ ô trống
            if (!KTGlobal.IsHaveSpace(spaceNeed, player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Cần sắp xếp tối thiểu {0} ô trống trong túi để nhận phần thưởng!", spaceNeed));
                /// Trả về kết quả
                return false;
            }

            /// Đánh dấu vị trí dừng
            player.LuckyCircle_LastStopPos = -1;
            /// Thêm vật phẩm tương ứng
            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, cell.ItemID, cell.Quantity, 0, "LuckyCircle", true, player.LuckyCircle_AwardBound ? 1 : 0, false, ItemManager.ConstGoodsEndTime))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                /// Ghi Log
                LogManager.WriteLog(LogTypes.LuckyCircle, string.Format("[ERROR] Player {0} (ID: {1}) get award failed, CellID: {2}.", player.RoleName, player.RoleID, cell.ID));
                /// Trả về kết quả
                return false;
            }
            /// Đánh dấu vật phẩm nhận được là khóa
            player.LuckyCircle_AwardBound = true;

            /// Nếu có thiết lập thông báo khi nhận quà
            if (cell.NotifyAfterTaken)
            {
                GoodsData itemGD = new GoodsData()
                {
                    GoodsID = cell.ItemID,
                };
                KTGlobal.SendSystemEventNotification(string.Format("Trong sự kiện <color=yellow>Vòng quay may mắn</color>, <color=#42a1ff>{0}</color> đã may mắn nhận được <color=#fff642>[{1}]</color>.", player.RoleName, KTGlobal.GetItemName(itemGD)));
            }

            /// Ghi Log
            LogManager.WriteLog(LogTypes.LuckyCircle, string.Format("Player {0} (ID: {1}) get award successfully, CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, cell.ID, cell.ItemID, cell.Quantity));
            /// Trả về kết quả
            return true;
        }

        /// <summary>
        /// Bắt đầu quay Vòng quay may mắn
        /// </summary>
        /// <param name="player"></param>
        /// <param name="method"></param>
        /// <param name="stopPos"></param>
        /// <returns></returns>
        public static bool StartTurn(KPlayer player, LuckyCirclePlayMethod method, out int stopPos)
        {
            /// Đánh dấu vị trí dừng
            stopPos = -1;
            /// Nếu cấp độ không đủ
            if (player.m_Level < KTLuckyCircleManager.Data.Config.LimitLevel)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể bắt đầu lượt quay!", KTLuckyCircleManager.Data.Config.LimitLevel));
                /// Trả về kết quả
                return false;
            }

            /// Nếu thao tác quá nhanh
            if (KTGlobal.GetCurrentTimeMilis() - player.LuckyCircle_LastTicks < 1000)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác quá nhanh, hãy đợi giây lát và thử lại!");
                return false;
            }
            /// Đánh dấu thời điểm
            player.LuckyCircle_LastTicks = KTGlobal.GetCurrentTimeMilis();

            /// Nếu sử dụng đồng
            if (method == LuckyCirclePlayMethod.UseToken)
            {
                /// Nếu số đồng không đủ
                if (player.Token < KTLuckyCircleManager.Data.Requiration.RequireToken)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Cần có {0} đồng mới có thể bắt đầu lượt quay!", KTLuckyCircleManager.Data.Requiration.RequireToken));
                    /// Trả về kết quả
                    return false;
                }
            }
            /// Nếu sử dụng đồng khóa
            else if (method == LuckyCirclePlayMethod.UseBoundToken)
            {
                /// Nếu số đồng không đủ
                if (player.BoundToken < KTLuckyCircleManager.Data.Requiration.RequireBoundToken)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Cần có {0} đồng khóa mới có thể bắt đầu lượt quay!", KTLuckyCircleManager.Data.Requiration.RequireBoundToken));
                    /// Trả về kết quả
                    return false;
                }
            }
            /// Nếu sử dụng vật phẩm
            else if (method == LuckyCirclePlayMethod.UseItem)
            {
                /// Nếu số lượng vật phẩm không đủ
                if (ItemManager.GetItemCountInBag(player, KTLuckyCircleManager.Data.Requiration.RequireItemID) <= 0)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Cần có [{0}] mới có thể bắt đầu lượt quay!", KTGlobal.GetItemName(KTLuckyCircleManager.Data.Requiration.RequireItemID)));
                    /// Trả về kết quả
                    return false;
                }
            }
            else
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Thao tác bị lỗi, hãy thử lại sau!");
                /// Trả về kết quả
                return false;
            }

            /// Nếu có quà chưa nhận
            if (KTLuckyCircleManager.HasAward(player))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Có phần thưởng chưa nhận, hãy ấn Nhận thưởng trước tiên.");
                /// Trả về kết quả
                return false;
            }

            /// Nếu sử dụng đồng
            if (method == LuckyCirclePlayMethod.UseToken)
            {
                /// Thực hiện trừ đồng
                KTGlobal.SubMoney(player, KTLuckyCircleManager.Data.Requiration.RequireToken, MoneyType.Dong, "KTLuckyCircleManager");
                /// Đánh dấu vật phẩm nhận được là không khóa
                player.LuckyCircle_AwardBound = false;
            }
            /// Nếu sử dụng đồng
            else if (method == LuckyCirclePlayMethod.UseBoundToken)
            {
                /// Thực hiện trừ đồng khóa
                KTGlobal.SubMoney(player, KTLuckyCircleManager.Data.Requiration.RequireBoundToken, MoneyType.DongKhoa, "KTLuckyCircleManager");
                /// Đánh dấu vật phẩm nhận được là khóa
                player.LuckyCircle_AwardBound = true;
            }
            /// Nếu sử dụng vật phẩm
            else if (method == LuckyCirclePlayMethod.UseItem)
            {
                /// Thực hiện xóa vật phẩm
                ItemManager.RemoveItemFromBag(player, KTLuckyCircleManager.Data.Requiration.RequireItemID, 1);
                /// Đánh dấu vật phẩm nhận được là khóa
                player.LuckyCircle_AwardBound = true;
            }

            /// Chỉ dùng đồng mới tăng số lượt quay
            if (method == LuckyCirclePlayMethod.UseToken)
            {
                /// Tăng số lượt quay hiện tại
                int totalTurn = player.LuckyCircle_TotalTurn;
                if (totalTurn < 0)
                {
                    totalTurn = 0;
                }
                player.LuckyCircle_TotalTurn = totalTurn + 1;
            }

            /// Xem có CheckPoint nào thỏa mãn không
            List<KTLuckyCircle.CellInfo> checkPointCells = method == LuckyCirclePlayMethod.UseToken ? KTLuckyCircleManager.Data.Cells.Where(x => x.CheckPoints != null && x.CheckPoints.Contains(player.LuckyCircle_TotalTurn)).ToList() : new List<KTLuckyCircle.CellInfo>();
            /// Nếu có CheckPoint thỏa mãn
            if (checkPointCells.Count > 0)
            {
                /// Chọn một CheckPoint ngẫu nhiên
                KTLuckyCircle.CellInfo checkPointCell = checkPointCells[KTGlobal.GetRandomNumber(0, checkPointCells.Count - 1)];
                /// Đánh dấu vị trí dừng
                stopPos = checkPointCell.ID;
                /// Lưu lại
                player.LuckyCircle_LastStopPos = stopPos;
                /// Ghi Log
                LogManager.WriteLog(LogTypes.LuckyCircle, string.Format("Player {0} (ID: {1}) start turn and get checkpoint CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, stopPos, checkPointCell.ItemID, checkPointCell.Quantity));
                /// Trả về kết quả
                return true;
            }
            /// Nếu không có CheckPoint thỏa mãn
            else
            {
                /// Tỷ lệ may mắn tối thiểu
                KTLuckyCircle.CellInfo minCell = KTLuckyCircleManager.Data.Cells.MinBy(x => method == LuckyCirclePlayMethod.UseToken ? x.RateUsingToken : method == LuckyCirclePlayMethod.UseBoundToken ? x.RateUsingBoundToken : x.RateUsingCard);
                KTLuckyCircle.CellInfo maxCell = KTLuckyCircleManager.Data.Cells.MaxBy(x => method == LuckyCirclePlayMethod.UseToken ? x.RateUsingToken : method == LuckyCirclePlayMethod.UseBoundToken ? x.RateUsingBoundToken : x.RateUsingCard);
                int minRate = method == LuckyCirclePlayMethod.UseToken ? minCell.RateUsingToken : method == LuckyCirclePlayMethod.UseBoundToken ? minCell.RateUsingBoundToken : minCell.RateUsingCard;
                int maxRate = method == LuckyCirclePlayMethod.UseToken ? maxCell.RateUsingToken : method == LuckyCirclePlayMethod.UseBoundToken ? maxCell.RateUsingBoundToken : maxCell.RateUsingCard;
                /// Random lại tỷ lệ
                int rate = KTGlobal.GetRandomNumber(minRate, maxRate);
                /// Danh sách các ô thỏa mãn
                List<KTLuckyCircle.CellInfo> cells = KTLuckyCircleManager.Data.Cells.Where(x => x.CheckPoints == null && (method == LuckyCirclePlayMethod.UseToken ? x.RateUsingToken : method == LuckyCirclePlayMethod.UseBoundToken ? x.RateUsingBoundToken : x.RateUsingCard) >= rate).ToList();
                /// Toác
                if (cells.Count <= 0)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Thao tác quay bị lỗi, hãy liên hệ với Admin để báo cáo.");
                    /// Ghi Log
                    LogManager.WriteLog(LogTypes.LuckyCircle, string.Format("[ERROR] Player {0} (ID: {1}) start turn failed, random cells list is empty.", player.RoleName, player.RoleID));
                    /// Trả về kết quả
                    return false;
                }
                /// Chọn một ô ngẫu nhiên
                KTLuckyCircle.CellInfo randomCell = cells[KTGlobal.GetRandomNumber(0, cells.Count - 1)];
                /// Đánh dấu vị trí dừng
                stopPos = randomCell.ID;
                /// Lưu lại
                player.LuckyCircle_LastStopPos = stopPos;
                /// Ghi Log
                LogManager.WriteLog(LogTypes.LuckyCircle, string.Format("Player {0} (ID: {1}) start turn and get CellID: {2} (ItemID: {3}, Quantity: {4}).", player.RoleName, player.RoleID, stopPos, randomCell.ItemID, randomCell.Quantity));
                /// Trả về kết quả
                return true;
            }
        }
        #endregion
    }
}
