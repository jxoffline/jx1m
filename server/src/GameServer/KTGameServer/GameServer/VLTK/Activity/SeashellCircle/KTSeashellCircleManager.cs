using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.SeashellCircle
{
    /// <summary>
    /// Quản lý Vòng quay sò (Bách Bảo Rương)
    /// </summary>
    public static class KTSeashellCircleManager
    {
        #region Define

        /// <summary>
        /// Mở vòng quay sò không
        /// </summary>
        public const bool EnableSeashellCircle = true;

        /// <summary>
        /// Vỏ sò vàng
        /// </summary>
        private const int Seashells = 746;

        /// <summary>
        /// Rương xấu xí
        /// </summary>
        private const int Treasure = 745;

        /// <summary>
        /// Tổng số ô
        /// </summary>
        private const int CellsPerRound = 22;

        /// <summary>
        /// Dữ liệu Bách bảo rương
        /// </summary>
        private static KTSeashellCircle Data = null;

        /// <summary>
        /// Tổng số sò đã tích lũy từ lần bảo trì trước tới thời điểm hiện tại
        /// </summary>
        private static int TotalIncomingSells = 0;

        /// <summary>
        /// Tổng số sò quyết định việc sẽ tăng rate
        /// </summary>
        private static int TotalSellsRate = 0;

        /// <summary>
        /// Tổng số sò đã nhả (hoặc quy đổi) từ lần bảo trì trước tới thời điểm hiện tại
        /// </summary>
        private static int TotalOutcomingSells = 0;

        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            /// Đối tượng XML tương ứng đọc từ File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Activity/KTSeashellCircle.xml");
            /// Chuyển sang dạng đối tượng
            KTSeashellCircle data = KTSeashellCircle.Parse(xmlNode);
            KTSeashellCircleManager.Data = data;
        }

        #endregion Define

        #region Core

        /// <summary>
        /// Trả về tổng số vỏ sò mà hệ thống đã giữ của người chơi
        /// </summary>
        /// <returns></returns>
        public static long GetSystemStorageSeashells()
        {
            return KTGlobal.GetSystemGlobalParameterInt64((int)SystemGlobalParameterIndexes.System_Storage_Seashell);
        }

        /// <summary>
        /// Thiết lập tổng số vỏ sò mà hệ thống đã giữ của người chơi
        /// </summary>
        /// <param name="totalSeashells"></param>
        private static void SetSystemStorageSeashells(long totalSeashells)
        {
            KTGlobal.SetSystemGlobalParameter((int)SystemGlobalParameterIndexes.System_Storage_Seashell, totalSeashells);
        }

        /// <summary>
        /// Trả về thông tin ô tại vị trí tương ứng
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static KeyValuePair<KTSeashellCircle.Cell, int> GetCellAtPos(int pos)
        {
            /// Duyệt danh sách ô
            foreach (KTSeashellCircle.Cell cell in KTSeashellCircleManager.Data.Cells)
            {
                /// Nếu nằm trong vị trí 1 sao
                if (cell.Position_1.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 1);
                }
                /// Nếu nằm trong vị trí 2 sao
                if (cell.Position_2.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 2);
                }
                /// Nếu nằm trong vị trí 3 sao
                if (cell.Position_3.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 3);
                }
            }
            /// Không tìm thấy
            return new KeyValuePair<KTSeashellCircle.Cell, int>(null, -1);
        }

        /// <summary>
        /// Trả về loại ô dừng lại lần trước
        /// </summary>
        /// <param name="player"></param>
        private static int GetLastCellType(KPlayer player)
        {
            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;
            /// Vị trí dừng lại trước đó
            int lastStopPos = player.LastSeashellCircleStopPos;

            /// Nếu tầng trước đó không tồn tại
            if (lastStage < 0 || lastStage >= KTSeashellCircleManager.Data.Cells.Count)
            {
                return -1;
            }

            KeyValuePair<KTSeashellCircle.Cell, int> pair = KTSeashellCircleManager.GetCellAtPos(lastStopPos);
            /// Nếu không tìm thấy
            if (pair.Key == null)
            {
                return -1;
            }
            return (int)pair.Key.Type;
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng có rương để nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasTresureToGet(KPlayer player)
        {
            /// Vị trí dừng lại trước đó
            int lastStopPos = player.LastSeashellCircleStopPos;
            /// Trả về kết quả
            return KTSeashellCircleManager.Data.TreasurePosition.Contains(lastStopPos);
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng có quà để nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanGet(KPlayer player)
        {
            /// Nếu có rương thì ok
            if (KTSeashellCircleManager.HasTresureToGet(player))
            {
                return true;
            }

            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;

            /// Nếu tầng trước đó không tồn tại
            if (lastStage < 0 || lastStage >= KTSeashellCircleManager.Data.ExchangeSeashells.Count)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng có thể đổi sò không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanExchange(KPlayer player)
        {
            return KTSeashellCircleManager.CanGet(player) && !KTSeashellCircleManager.HasTresureToGet(player);
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng có thể đặt cược không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanBet(KPlayer player)
        {
            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;
            /// Nếu không có gì để nhận và đổi thì có thể đặt cược
            return (lastStage < 0 || lastStage >= KTSeashellCircleManager.Data.ExchangeSeashells.Count) && (!KTSeashellCircleManager.HasTresureToGet(player) && !KTSeashellCircleManager.CanGet(player) && !KTSeashellCircleManager.CanExchange(player));
        }

        /// <summary>
        /// Kiểm tra người chơi tương ứng có thể thực hiện quay không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool CanStartTurn(KPlayer player)
        {
            /// Nếu có rương thì không được
            return !KTSeashellCircleManager.HasTresureToGet(player);
        }

        #endregion Core

        #region Public methods

        /// <summary>
        /// Khôi phục lại thông tin lần trước
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lastStage"></param>
        /// <param name="lastStopPos"></param>
        /// <param name="lastBet"></param>
        /// <param name="enableGet"></param>
        /// <param name="enableExchange"></param>
        /// <param name="enableBet"></param>
        /// <returns></returns>
        public static void RecoverPreviousData(KPlayer player, out int lastStage, out int lastStopPos, out int lastBet)
        {
            /// Tầng lần trước đó
            lastStage = player.LastSeashellCircleStage;
            /// Vị trí dừng lại trước đó
            lastStopPos = player.LastSeashellCircleStopPos;
            /// Số sò cược lần trước
            lastBet = player.LastSeashellCircleBet;
        }

        /// <summary>
        /// Nhận vỏ sò
        /// </summary>
        /// <param name="player"></param>
        /// <param name="totalSeashells"></param>
        /// <returns></returns>
        public static bool GetSeashell(KPlayer player, out int totalSeashells)
        {
            /// Tổng số sò nhận được
            totalSeashells = 0;
            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;
            /// Số sò cược lần trước
            int lastBet = player.LastSeashellCircleBet;

            /// Nếu số lượng cược không khớp
            if (lastBet != 2 && lastBet != 10 && lastBet != 50)
            {
                KTPlayerManager.ShowNotification(player, "Xin lỗi, bạn không có quà để nhận!");
                return false;
            }

            /// Nếu không có phần quà
            if (KTSeashellCircleManager.CanExchange(player))
            {
                /// Nếu tầng trước đó không tồn tại
                if (lastStage < 0 || lastStage >= KTSeashellCircleManager.Data.ExchangeSeashells.Count)
                {
                    KTPlayerManager.ShowNotification(player, "Bạn không có phần thưởng có thể quy đổi qua vỏ sò!");
                    return false;
                }

                /// Số sò có thể đổi được
                int shellsCount = KTSeashellCircleManager.Data.ExchangeSeashells[lastStage].Value * lastBet / 100;
                /// Số ô trong túi cần
                int totalSpacesNeed = ItemManager.TotalSpaceNeed(KTSeashellCircleManager.Seashells, shellsCount);
                /// Nếu túi đồ không đủ chỗ trống
                if (!KTGlobal.IsHaveSpace(totalSpacesNeed, player))
                {
                    KTPlayerManager.ShowNotification(player, string.Format("Bạn cần sắp xếp {0} ô trống trong túi đồ để nhận vỏ sò!", totalSpacesNeed));
                    return false;
                }

                /// Reset toàn bộ
                player.LastSeashellCircleStage = -1;
                player.LastSeashellCircleStopPos = -1;
                player.LastSeashellCircleBet = -1;
                /// Lưu thông tin vào DB
                player.SaveSeashellCircleParamToDB();

                /// Tạo vật phẩm cho người chơi
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, KTSeashellCircleManager.Seashells, shellsCount, 0, "SeashellCircle", true, 0, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                    LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Tạo vật phẩm ID {0} cho RoleID {1} lỗi SML.", KTSeashellCircleManager.Seashells, player.RoleID));
                    return false;
                }

                /// Thông báo đổi thành công
                KTPlayerManager.ShowNotification(player, string.Format("Đổi thành công, bạn nhận được {0} vỏ sò!", shellsCount));

                /// Cập nhật tổng số sò nhận được
                totalSeashells = shellsCount;

                /// Ghi LOG
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) đổi được {2} vỏ sò.", player.RoleName, player.RoleID, shellsCount));

                /// Tăng tổng số sò đã nhả của Server
                KTSeashellCircleManager.TotalOutcomingSells += shellsCount;
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("Server nhả tổng số sò: {0}", KTSeashellCircleManager.TotalOutcomingSells));
            }

            /// Trả về nhận thành công
            return true;
        }

        /// <summary>
        /// Nhận quà
        /// </summary>
        /// <param name="player"></param>
        /// <param name="awardType"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        /// <returns></returns>
        public static bool GetAward(KPlayer player, out int awardType, out int itemID, out int itemNumber)
        {
            /// Loại quà
            awardType = -1;
            /// Vật phẩm nhận được
            itemID = -1;
            /// Số lượng nhận được
            itemNumber = 0;

            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;
            /// Vị trí dừng lại trước đó
            int lastStopPos = player.LastSeashellCircleStopPos;
            /// Số sò cược lần trước
            int lastBet = player.LastSeashellCircleBet;
            /// Loại ô lần trước
            int lastCellType = KTSeashellCircleManager.GetLastCellType(player);

            /// Nếu số lượng cược không khớp
            if (lastBet != 2 && lastBet != 10 && lastBet != 50)
            {
                KTPlayerManager.ShowNotification(player, "Xin lỗi, bạn không có quà để nhận!");
                return false;
            }

            /// Tỷ lệ tăng thêm của quà thưởng (lần)
            int awardRate;
            if (lastBet == 2)
            {
                awardRate = KTSeashellCircleManager.Data.BetRates.Bet_2;
            }
            else if (lastBet == 10)
            {
                awardRate = KTSeashellCircleManager.Data.BetRates.Bet_10;
            }
            else
            {
                awardRate = KTSeashellCircleManager.Data.BetRates.Bet_50;
            }

            /// Nếu dừng lại ở rương
            if (KTSeashellCircleManager.HasTresureToGet(player))
            {
                /// Nếu túi đồ không đủ ô trống
                if (!KTGlobal.IsHaveSpace(awardRate, player))
                {
                    KTPlayerManager.ShowNotification(player, string.Format("Bạn cần sắp xếp {0} ô trống trong túi đồ để nhận quà!", awardRate));
                    return false;
                }

                /// Thông báo nhận được rương tới tất cả người chơi
                KTGlobal.SendSystemEventNotification(string.Format("Người chơi <color=#38c3ff>[{0}]</color> tại sự kiện <color=green>Bách Bảo Rương</color> đã may mắn quay được <color=orange>{1} cái</color> <color=yellow>[{2}]</color>. Xin chúc mừng!", player.RoleName, awardRate, KTGlobal.GetItemName(KTSeashellCircleManager.Treasure)));

                /// Reset toàn bộ
                player.LastSeashellCircleStage = -1;
                player.LastSeashellCircleStopPos = -1;
                player.LastSeashellCircleBet = -1;
                /// Lưu thông tin vào DB
                player.SaveSeashellCircleParamToDB();

                /// Tạo vật phẩm cho người chơi
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, KTSeashellCircleManager.Treasure, awardRate, 0, "SeashellCircle", true, 0, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                    LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Tạo vật phẩm ID {0} cho RoleID {1} lỗi SML.", KTSeashellCircleManager.Treasure, player.RoleID));
                    return false;
                }

                /// Thông báo đổi thành công
                KTPlayerManager.ShowNotification(player, string.Format("Nhận thành công, bạn nhận được {0} cái [{1}]!", awardRate, KTGlobal.GetItemName(KTSeashellCircleManager.Treasure)));

                /// Cập nhật quà nhận được là vật phẩm
                awardType = 0;
                /// Cập nhật vật phẩm nhận được
                itemID = KTSeashellCircleManager.Treasure;
                /// Cập nhật số lượng nhận được
                itemNumber = awardRate;

                /// Ghi LOG
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) nhận được {2} Rương xấu xí.", player.RoleName, player.RoleID, itemNumber));
            }
            /// Nếu có quà có thể nhận
            else if (KTSeashellCircleManager.CanGet(player))
            {
                /// Nếu tầng trước đó không tồn tại
                if (lastStage < 0 || lastStage >= KTSeashellCircleManager.Data.Cells.Count)
                {
                    KTPlayerManager.ShowNotification(player, "Xin lỗi, bạn không có quà để nhận!");
                    return false;
                }
                /// Nếu ô không tồn tại
                else if (lastCellType < 0 || lastCellType >= KTSeashellCircleManager.Data.Cells.Count)
                {
                    KTPlayerManager.ShowNotification(player, "Xin lỗi, bạn không có quà để nhận!");
                    return false;
                }

                /// Dữ liệu ô ở tầng trước đó
                KTSeashellCircle.Cell cell = KTSeashellCircleManager.Data.Cells[lastCellType];

                /// Giá trị quà ở tầng này
                int value = cell.ValuesByStage[lastStage];

                /// Nếu là Huyền Tinh thì yêu cầu chỗ trống tương ứng
                if (cell.Type == KTSeashellCircle.SeashellCircleCellType.CrystalStone && !KTGlobal.IsHaveSpace(awardRate, player))
                {
                    KTPlayerManager.ShowNotification(player, string.Format("Bạn cần sắp xếp {0} ô trống trong túi đồ để nhận quà!", awardRate));
                    return false;
                }

                /// Reset toàn bộ
                player.LastSeashellCircleStage = -1;
                player.LastSeashellCircleStopPos = -1;
                player.LastSeashellCircleBet = -1;
                /// Lưu thông tin vào DB
                player.SaveSeashellCircleParamToDB();

                /// Xem loại nhận là gì
                switch (lastCellType)
                {
                    /// Huyền Tinh
                    case (int)KTSeashellCircle.SeashellCircleCellType.CrystalStone:
                        {
                            /// Tạo vật phẩm cho người chơi
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, value, awardRate, 0, "SeashellCircle", true, 0, false, ItemManager.ConstGoodsEndTime))
                            {
                                KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Tạo vật phẩm ID {0} cho RoleID {1} lỗi SML.", value, player.RoleID));
                                return false;
                            }

                            /// Thông báo đổi thành công
                            KTPlayerManager.ShowNotification(player, string.Format("Nhận thành công, bạn nhận được {0} cái [{1}]!", awardRate, KTGlobal.GetItemName(value)));

                            /// Cập nhật quà nhận được là vật phẩm
                            awardType = 0;
                            /// Cập nhật vật phẩm nhận được
                            itemID = value;
                            /// Cập nhật số lượng nhận được
                            itemNumber = awardRate;

                            /// Ghi LOG
                            LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) nhận được {2} cái {3}.", player.RoleName, player.RoleID, itemNumber, KTGlobal.GetItemName(value)));
                            break;
                        }
                    /// Tinh hoạt lực
                    case (int)KTSeashellCircle.SeashellCircleCellType.GatherMakePoint:
                        {
                            /// Thêm Tinh Hoạt lực cho người chơi
                            player.ChangeCurMakePoint(value * awardRate);
                            player.ChangeCurGatherPoint(value * awardRate);

                            /// Thông báo đổi thành công
                            KTPlayerManager.ShowNotification(player, string.Format("Nhận thành công!"));

                            /// Cập nhật quà nhận được là tinh hoạt lực
                            awardType = 1;
                            /// Cập nhật số lượng nhận được
                            itemNumber = value * awardRate;

                            /// Ghi LOG
                            LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) nhận được {2} Tinh hoạt lực.", player.RoleName, player.RoleID, itemNumber));
                            break;
                        }
                    /// Bạc
                    case (int)KTSeashellCircle.SeashellCircleCellType.Money:
                        {
                            KTPlayerManager.AddMoney(player, value * awardRate, "SeashellCircle");

                            /// Thông báo đổi thành công
                            KTPlayerManager.ShowNotification(player, string.Format("Nhận thành công!"));

                            /// Cập nhật quà nhận được là bạc
                            awardType = 2;
                            /// Cập nhật số lượng nhận được
                            itemNumber = value * awardRate;

                            /// Ghi LOG
                            LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) nhận được {2} Bạc.", player.RoleName, player.RoleID, itemNumber));
                            break;
                        }
                    /// Đồng khóa
                    case (int)KTSeashellCircle.SeashellCircleCellType.BoundToken:
                        {
                            KTPlayerManager.AddBoundToken(player, value * awardRate, "SeashellCircle");

                            /// Thông báo đổi thành công
                            KTPlayerManager.ShowNotification(player, string.Format("Nhận thành công!"));

                            /// Cập nhật quà nhận được là đồng khóa
                            awardType = 3;
                            /// Cập nhật số lượng nhận được
                            itemNumber = value * awardRate;

                            /// Ghi LOG
                            LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) nhận được {2} Đồng khóa.", player.RoleName, player.RoleID, itemNumber));
                            break;
                        }
                }

                /// Quy ra số sò có thể đổi được để ghi vào LOG thống kê
                int shellsCount = KTSeashellCircleManager.Data.ExchangeSeashells[lastStage].Value * lastBet / 100;
                /// Tăng tổng số sò đã nhả của Server
                KTSeashellCircleManager.TotalOutcomingSells += shellsCount;
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("Server nhả tổng số sò: {0}", KTSeashellCircleManager.TotalOutcomingSells));
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Xin lỗi, bạn không có quà để nhận!");
                return false;
            }

            /// Trả về nhận thành công
            return true;
        }

        /// <summary>
        /// Bắt đầu quay
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="bet">Số sò đã cược</param>
        /// <param name="lastStopPos">Vị trí sẽ dừng lại ở lượt quay này (xem thứ tự ở khung ở Client, đánh số từ trái sang phải, trên xuống dưới)</param>
        /// <param name="durationTick">Thời gian quay</param>
        /// <param name="totalCells">Tổng số ô cần qua</param>
        /// <param name="isFirstTurn">Có phải lần đầu quay không</param>
        public static bool StartTurn(KPlayer player, int bet, out int lastStopPos, out int durationTick, out int totalCells, out bool isFirstTurn)
        {
            /// Thời gian quay
            durationTick = -1;
            /// Tổng số ô cần qua
            totalCells = 0;
            /// Đánh dấu không phải lần đầu
            isFirstTurn = false;

            /// Tầng lần trước đó
            int lastStage = player.LastSeashellCircleStage;
            /// Vị trí dừng lại trước đó
            lastStopPos = player.LastSeashellCircleStopPos;
            /// Số sò cược lần trước
            int lastBet = player.LastSeashellCircleBet;
            /// Loại ô lần trước
            int lastCellType = KTSeashellCircleManager.GetLastCellType(player);

            /// Nếu có rương để nhận
            if (KTSeashellCircleManager.HasTresureToGet(player))
            {
                KTPlayerManager.ShowNotification(player, "Bạn có phần quà cần nhận trước mới có thể quay tiếp!");
                return false;
            }

            /// Nếu không thể quay
            if (!KTSeashellCircleManager.CanStartTurn(player))
            {
                KTPlayerManager.ShowNotification(player, "Bạn hiện không thể thực hiện quay, hãy liên hệ với hỗ trợ để được xử lý!");
                return false;
            }

            /// Đánh dấu có phải lần đầu quay không
            isFirstTurn = lastStage < 0;

            /// Nếu tầng lần trước tồn tại và số sò cược ở lần này không khớp
            if (lastStage >= 0 && lastStage < KTSeashellCircleManager.Data.TurnRateByStages.Count && lastBet != bet)
            {
                KTPlayerManager.ShowNotification(player, "Thao tác bị lỗi. Số sò cược giữa 2 lần liên tiếp không khớp!");
                return false;
            }

            /// Nếu là lần đầu
            if (isFirstTurn)
            {
                /// Nếu số sò có trên người không đủ
                if (ItemManager.GetItemCountInBag(player, KTSeashellCircleManager.Seashells) < bet)
                {
                    KTPlayerManager.ShowNotification(player, "Số sò mang theo không đủ!");
                    return false;
                }
            }

            /// Tầng hiện tại
            int currentStage = lastStage;
            /// Nếu tầng hiện tại vượt quá ngưỡng
            if (currentStage < 0 || currentStage >= KTSeashellCircleManager.Data.TurnRateByStages.Count)
            {
                /// Thiết lập lại
                currentStage = -1;
            }

            /// Có quay vào rương được không
            bool willGetTreasure = false;
            /// Nếu là lượt đầu tiên
            if (isFirstTurn)
            {
                /// Nếu có GM thiết lập
                if (player.GM_SetWillGetTreasureNextTurn && (player.GM_SetWillGetTreasureNextTurnWithBet == -1 || player.GM_SetWillGetTreasureNextTurnWithBet == bet))
                {
                    /// Thiết lập có thể quay vào rương
                    willGetTreasure = true;
                    /// Hủy đánh dấu GM thiết lập
                    player.GM_SetWillGetTreasureNextTurn = false;
                    player.GM_SetWillGetTreasureNextTurnWithBet = -1;
                }
                /// Nếu không có GM thiết lập
                else
                {
                    int BOUNDRATE = 0;
                    int BOUNDFROMLUCKY = 0;
                    // Nếu mà Rate > 10.000 thì tỉ lệ vào rương sẽ tăng thêm 10
                    if (KTSeashellCircleManager.TotalSellsRate > 10000)
                    {
                        BOUNDRATE = KTSeashellCircleManager.TotalSellsRate / 1000;
                    }

                    int LUCKEY = player.m_nCurLucky;

                    // Nếu người chơi có điểm may mắn và điểm may mắn >20
                    if (LUCKEY > 20)
                    {
                        BOUNDFROMLUCKY = LUCKEY / 20;
                        //Cho đoạn này vào để chắc chắn ko vượt quá 5% may mắn
                        if(BOUNDFROMLUCKY>5)
                        {
                            BOUNDFROMLUCKY = 5;
                        }    
                    }
                    /// Tỷ lệ ngẫu nhiên
                    int myRate = KTGlobal.GetRandomNumber(1, 5000);
                    /// Nếu may mắn
                    if (myRate <= (KTSeashellCircleManager.Data.RateToTreasure + BOUNDRATE + BOUNDFROMLUCKY))
                    {
                        // Nếu có thằng quay vào rương thì reset lại rate
                        KTSeashellCircleManager.TotalSellsRate = 0;
                        willGetTreasure = true;
                    }
                }
            }

            /// Vị trí sẽ dừng lại
            int stopPos;
            /// Thời gian quay (Tick)
            durationTick = KTGlobal.GetRandomNumber(KTSeashellCircleManager.Data.MinTimeTick, KTSeashellCircleManager.Data.MaxTimeTick);
            /// Số vòng quay
            int totalRounds = KTGlobal.GetRandomNumber(KTSeashellCircleManager.Data.MinRound, KTSeashellCircleManager.Data.MaxRound);
            /// Đánh dấu quay thành công không
            bool isSuccess;
            /// Tầng kế sau khi quay
            int nextStage = currentStage;

            /// Nếu quay vào rương
            if (willGetTreasure)
            {
                /// Chọn vị trí dừng
                stopPos = KTSeashellCircleManager.Data.TreasurePosition[KTGlobal.GetRandomNumber(0, KTSeashellCircleManager.Data.TreasurePosition.Count - 1)];
                /// Đánh dấu quay thành công
                isSuccess = true;
            }
            /// Nếu không quay vào rương
            else
            {
                /// Thông tin tỷ lệ quay tầng hiện tại
                KTSeashellCircle.TurnRateByStage stageInfo = KTSeashellCircleManager.Data.TurnRateByStages[currentStage + 1];

                /// Danh sách các vị trí 1 sao
                List<int> position_1 = new List<int>();
                /// Danh sách các vị trí 2 sao
                List<int> position_2 = new List<int>();
                /// Danh sách các vị trí 3 sao
                List<int> position_3 = new List<int>();

                /// Có quay vào ô cùng loại không
                bool willGetSameTypeCell = false;
                /// Nếu tầng hiện tại khác 0
                if (currentStage != -1)
                {
                    /// Tỷ lệ hiện tại
                    int nRate = KTGlobal.GetRandomNumber(1, 1000);
                    /// Nếu có thể quay vào ô cùng loại
                    if (nRate <= stageInfo.RateToSameType)
                    {
                        /// Đánh dấu có thể quay vào ô cùng loại
                        willGetSameTypeCell = true;
                    }
                }

                /// Nếu tầng hiện tại là -1
                if (currentStage == -1)
                {
                    /// Duyệt danh sách các ô
                    foreach (KTSeashellCircle.Cell cell in KTSeashellCircleManager.Data.Cells)
                    {
                        /// Thêm các ô 1 sao vào
                        position_1.AddRange(cell.Position_1);
                        /// Thêm các ô 2 sao vào
                        position_2.AddRange(cell.Position_2);
                        /// Thêm các ô 3 sao vào
                        position_3.AddRange(cell.Position_3);
                    }
                    /// Đánh dấu quay thành công
                    isSuccess = true;
                }
                /// Nếu không thể quay vào ô cùng loại
                else if (!willGetSameTypeCell)
                {
                    /// Duyệt danh sách các ô
                    foreach (KTSeashellCircle.Cell cell in KTSeashellCircleManager.Data.Cells)
                    {
                        /// Nếu loại trùng với loại hiện tại thì bỏ qua
                        if ((int)cell.Type == lastCellType)
                        {
                            continue;
                        }
                        /// Thêm các ô 1 sao vào
                        position_1.AddRange(cell.Position_1);
                        /// Thêm các ô 2 sao vào
                        position_2.AddRange(cell.Position_2);
                        /// Thêm các ô 3 sao vào
                        position_3.AddRange(cell.Position_3);
                    }
                    /// Đánh dấu quay thất bại
                    isSuccess = false;
                }
                /// Nếu tầng hiện tại khác 0
                else
                {
                    /// Nếu loại ô không hợp lý
                    if (lastCellType < 0 || lastCellType >= KTSeashellCircleManager.Data.Cells.Count)
                    {
                        KTPlayerManager.ShowNotification(player, "Dữ liệu bị lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                        LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Lỗi bắt đầu quay sò của RoleID {0} SML. 'lastCellType < 0 || lastCellType >= KTSeashellCircleManager.Data.Cells.Count'", player.RoleID));
                        return false;
                    }
                    /// Loại ô tương ứng
                    KTSeashellCircle.Cell cell = KTSeashellCircleManager.Data.Cells[lastCellType];
                    /// Thêm các ô 1 sao vào
                    position_1.AddRange(cell.Position_1);
                    /// Thêm các ô 2 sao vào
                    position_2.AddRange(cell.Position_2);
                    /// Thêm các ô 3 sao vào
                    position_3.AddRange(cell.Position_3);
                    /// Đánh dấu quay thành công
                    isSuccess = true;
                }

                /// Loại sẽ quay vào
                int type = -1;
                /// Tỷ lệ ngẫu nhiên
                int myRate = KTGlobal.GetRandomNumber(1, 1000);
                /// Nếu có thể quay vào ô 3 sao
                if (myRate <= stageInfo.RateToPosition_3)
                {
                    /// Loại sẽ quay vào ô 3 sao
                    type = 3;
                }
                else
                {
                    myRate -= stageInfo.RateToPosition_3;
                    /// Nếu có thể quay vào ô 2 sao
                    if (myRate <= stageInfo.RateToPosition_2)
                    {
                        /// Loại sẽ quay vào ô 2 sao
                        type = 2;
                    }
                    else
                    {
                        myRate -= stageInfo.RateToPosition_2;
                        if (myRate <= stageInfo.RateToPosition_1)
                        {
                            /// Loại sẽ quay vào ô 1 sao
                            type = 1;
                        }
                    }
                }

                /// Kiểm tra loại quay
                switch (type)
                {
                    /// 1 sao
                    case 1:
                        {
                            nextStage++;
                            stopPos = position_1[KTGlobal.GetRandomNumber(0, position_1.Count - 1)];
                            break;
                        }
                    /// 2 sao
                    case 2:
                        {
                            nextStage += 2;
                            stopPos = position_2[KTGlobal.GetRandomNumber(0, position_2.Count - 1)];
                            break;
                        }
                    /// 3 sao
                    case 3:
                        {
                            nextStage += 3;
                            stopPos = position_3[KTGlobal.GetRandomNumber(0, position_3.Count - 1)];
                            break;
                        }
                    /// Toác
                    default:
                        {
                            KTPlayerManager.ShowNotification(player, "Dữ liệu bị lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                            LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Lỗi bắt đầu quay sò của RoleID {0} SML. 'switch (type) => default'", player.RoleID));
                            return false;
                        }
                }
            }

            /// Nếu không có vị trí trước đó
            if (lastStopPos == -1)
            {
                lastStopPos = 0;
            }

            /// Cập nhật tổng số ô cần đi qua
            totalCells = stopPos + totalRounds * KTSeashellCircleManager.CellsPerRound - lastStopPos;

            //PlayerManager.ShowNotification(player, string.Format("LastStopPos = {0}, StopPos = {1}, TotalRound = {2}, Total cells = {3}", lastStopPos, stopPos, totalRounds, totalCells));

            /// Nếu là lần đầu
            if (isFirstTurn)
            {
                /// Xóa số sò tương ứng đã cược
                if (!ItemManager.RemoveItemFromBag(player, KTSeashellCircleManager.Seashells, bet, -1, "Quay Sò"))
                {
                    KTPlayerManager.ShowNotification(player, "Không thể xóa số sò đã cược, xin hãy thử lại!");
                    LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("[ERROR] Lỗi đéo xóa được {0}x sò của RoleID {1}", bet, player.RoleID));
                    return false;
                }
                /// Tăng tổng số sò đã tích lũy của Server
                KTSeashellCircleManager.TotalIncomingSells += bet;
                KTSeashellCircleManager.TotalSellsRate += bet;
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("Server tích lũy tổng số sò: {0}", KTSeashellCircleManager.TotalIncomingSells));
            }

            /// Số sò hiện đang tích lũy
            long currentStorageCount = KTSeashellCircleManager.GetSystemStorageSeashells();
            /// Tăng số sò đã tích lũy
            currentStorageCount += bet;
            /// Ghi lại số sò hiện đang tích lũy
            KTSeashellCircleManager.SetSystemStorageSeashells(currentStorageCount);

            /// Nếu quay thành công
            if (isSuccess)
            {
                /// Nếu quay vào rương
                if (willGetTreasure)
                {
                    /// Cập nhật số cược
                    player.LastSeashellCircleBet = bet;
                    /// Cập nhật tầng dừng lại
                    player.LastSeashellCircleStage = -1;
                    /// Cập nhật vị trí dừng lại
                    player.LastSeashellCircleStopPos = stopPos;
                    /// Lưu thông tin vào DB
                    player.SaveSeashellCircleParamToDB();

                    /// Ghi LOG
                    LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) cược {2} sò, quay vào Rương xấu xí.", player.RoleName, player.RoleID, bet));
                }
                /// Nếu không quay vào rương
                else
                {
                    /// Cập nhật số cược
                    player.LastSeashellCircleBet = bet;
                    /// Cập nhật tầng dừng lại
                    player.LastSeashellCircleStage = nextStage;
                    /// Cập nhật vị trí dừng lại
                    player.LastSeashellCircleStopPos = stopPos;
                    /// Lưu thông tin vào DB
                    player.SaveSeashellCircleParamToDB();

                    /// Ghi LOG
                    LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) tầng {2}, cược {3} sò, quay vào vị trí {4}, tăng lên tầng {5}.", player.RoleName, player.RoleID, currentStage, bet, stopPos, nextStage));
                }
            }
            /// Nếu quay thất bại
            else
            {
                /// Reset toàn bộ
                player.LastSeashellCircleStage = -1;
                player.LastSeashellCircleStopPos = -1;
                player.LastSeashellCircleBet = -1;
                /// Lưu thông tin vào DB
                player.SaveSeashellCircleParamToDB();

                /// Ghi LOG
                LogManager.WriteLog(LogTypes.SeashellCircle, string.Format("{0} (ID: {1}) tầng {2}, cược {3} sò, quay vào vị trí {4}, toạch.", player.RoleName, player.RoleID, currentStage, bet, stopPos));
            }

            /// Trả về kết quả thành công
            return true;
        }

        #endregion Public methods
    }
}