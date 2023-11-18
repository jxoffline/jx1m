using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.PlayerPray
{
    /// <summary>
    /// Vòng quay chúc phúc
    /// </summary>
    public static class KTPlayerPrayManager
    {
        #region Define
        /// <summary>
        /// Dữ liệu chúc phúc
        /// </summary>
        private static readonly Dictionary<string, PlayerPrayXML> PlayerPrays = new Dictionary<string, PlayerPrayXML>();

        /// <summary>
        /// Cấp độ tối thiểu để được quay chúc phúc
        /// </summary>
        private const int MinLevel = 50;

        /// <summary>
        /// Số lượt quay chúc phúc có được mỗi ngày
        /// </summary>
        private const int TurnAddedEveryday = 1;

        /// <summary>
        /// Tỷ lệ quay vào cùng loại
        /// </summary>
        private static readonly int[] RateToSameType = new int[]
        {
            100, 50, 5, 1, 0
        };

        /// <summary>
        /// Khởi tạo dữ liệu chúc phúc
        /// </summary>
        public static void Init()
        {
            /// Xóa danh sách
            KTPlayerPrayManager.PlayerPrays.Clear();

            /// Đối tượng XML tương ứng đọc từ File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Activity/PlayerPray.xml");

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Pray"))
            {
                PlayerPrayXML playerPray = PlayerPrayXML.Parse(node);
                KTPlayerPrayManager.PlayerPrays[playerPray.Result] = playerPray;
            }
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Trả về số lượt chúc phúc còn lại trong ngày
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetTotalTurnLeft(KPlayer player)
        {
            /// Tổng số lượt còn lại ngày hôm nay
            int totalTurnLeft = player.GetValueOfDailyRecore((int)DailyRecord.PlayerPray);
            /// Nếu sang ngày mới
            if (totalTurnLeft == -1)
            {
                /// Được 1 lượt chúc phúc miễn phí
                totalTurnLeft = KTPlayerPrayManager.TurnAddedEveryday;
            }
            /// Trả về kết quả
            return totalTurnLeft;
        }

        /// <summary>
        /// Thiết lập giá trị số lượt chúc phúc còn lại trong ngày
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        public static void SetTotalTurnLeft(KPlayer player, int value)
        {
            /// Thiết lập số lượt chúc phúc còn lại trong ngày
            player.SetValueOfDailyRecore((int)DailyRecord.PlayerPray, value);
        }

        /// <summary>
        /// Mở khung Chúc phúc
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lastResult"></param>
        /// <param name="enableGetAward"></param>
        /// <param name="enableStartTurn"></param>
        public static bool Open(KPlayer player, out List<string> lastResult, out bool enableGetAward, out bool enableStartTurn)
        {
            /// Dữ liệu mặc định
            lastResult = null;
            enableStartTurn = false;
            enableGetAward = false;

            /// Nếu cấp độ không đủ
            if (player.m_Level < KTPlayerPrayManager.MinLevel)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể Chúc phúc!", KTPlayerPrayManager.MinLevel));
                return false;
            }

            lastResult = player.LastPrayResult;
            enableStartTurn = lastResult == null || lastResult.Count < 5;
            enableGetAward = KTPlayerPrayManager.HasAward(player);

            /// Trả ra kết quả mở khung thành công
            return true;
        }

        /// <summary>
        /// Bắt đầu lượt quay mới
        /// </summary>
        /// <param name="player"></param>
        /// <param name="round"></param>
        /// <param name="stopPos"></param>
        /// <param name="enableGetAward"></param>
        /// <param name="enableStartTurn"></param>
        public static bool StartTurn(KPlayer player, out int round, out int stopPos, out bool enableGetAward, out bool enableStartTurn)
        {
            /// Dữ liệu mặc định
            round = -1;
            stopPos = -1;
            enableGetAward = false;
            enableStartTurn = true;

            /// Nếu cấp độ không đủ
            if (player.m_Level < KTPlayerPrayManager.MinLevel)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Cần đạt cấp {0} trở lên mới có thể Chúc phúc!", KTPlayerPrayManager.MinLevel));
                return false;
            }
            /// Nếu có phần thưởng
            else if (KTPlayerPrayManager.HasAward(player))
            {
                KTPlayerManager.ShowNotification(player, "Hãy nhận phần thưởng trước!");
                return false;
            }
            /// Nếu đã quá số lượt
            else if (player.LastPrayResult != null && player.LastPrayResult.Count >= 5)
            {
                KTPlayerManager.ShowNotification(player, "Đã quá số lượt có thể quay!");
                return false;
            }

            /// Nếu là lượt đầu tiên
            if (player.LastPrayResult == null || player.LastPrayResult.Count <= 0)
            {
                /// Số lượt quay còn lại trong ngày
                int totalTurnLeft = KTPlayerPrayManager.GetTotalTurnLeft(player);

                /// Nếu không có lượt quay
                if (totalTurnLeft <= 0)
                {
                    KTPlayerManager.ShowNotification(player, "Bạn đã hết lượt Chúc phúc trong ngày, hãy quay lại vào ngày mai!");
                    return false;
                }

                /// Trừ 1 lượt chúc phúc trong ngày
                totalTurnLeft--;
                /// Lưu lại số lượt quay
                KTPlayerPrayManager.SetTotalTurnLeft(player, totalTurnLeft);
            }

            /// Nếu chưa tồn tại danh sách thì tạo mới
            if (player.LastPrayResult == null)
            {
                player.LastPrayResult = new List<string>();
            }

            /// Chọn ngẫu nhiên số vòng và vị trí quay vào
            round = KTGlobal.GetRandomNumber(5, 7);

            /// Tỷ lệ quay vào cùng loại
            int rateToSameType = KTPlayerPrayManager.RateToSameType[player.LastPrayResult.Count];
            /// Lần này có may mắn quay vào cùng loại không
            int luckyRate = KTGlobal.GetRandomNumber(1, 50);
            /// Nếu may mắn
            bool canGetSameType = luckyRate <= rateToSameType;

            /// Danh sách các vị trí có thể quay vào
            List<int> canGetPos = new List<int>();
            for (int i = 1; i <= 5; i++)
            {
                /// Nếu trùng với vị trí cũ nhưng không may mắn
                if (player.LastPrayResult.Count > 0 && i.ToString() == player.LastPrayResult.Last() && !canGetSameType)
                {
                    continue;
                }
                /// Thêm vị trí vào danh sách
                canGetPos.Add(i);
            }

            /// Chọn ngẫu nhiên vị trí dừng lại
            stopPos = canGetPos[KTGlobal.GetRandomNumber(0, canGetPos.Count - 1)];
            /// Thêm vào danh sách
            player.LastPrayResult.Add(stopPos.ToString());

            /// Nếu các phần tử có sự khác nhau
            if (player.LastPrayResult.Count > 1 && player.LastPrayResult[0] != stopPos.ToString())
            {
                /// Mở chức năng nhận thưởng
                enableGetAward = true;
                /// Đổ dữ liệu còn lại
                while (player.LastPrayResult.Count < 5)
                {
                    player.LastPrayResult.Add("0");
                }
            }

            /// Nếu đã có kết quả
            if (player.LastPrayResult.Count >= 5)
            {
                /// Đóng chức năng quay
                enableStartTurn = false;
            }
            else
            {
                /// Mở chức năng quay
                enableStartTurn = true;
            }

            /// Lưu vào DB
            player.SavePrayDataToDB();

            /// Trả ra kết quả quay thành công
            return true;
        }

        /// <summary>
        /// Có phần thưởng có thể nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasAward(KPlayer player)
        {
            /// Nếu không có dữ liệu các lần quay
            if (player.LastPrayResult == null || player.LastPrayResult.Count <= 0)
            {
                return false;
            }
            /// Trả về kết quả
            return player.LastPrayResult.Count == 5;
        }

        /// <summary>
        /// Nhận phần thưởng
        /// </summary>
        /// <param name="player"></param>
        public static bool GetAward(KPlayer player)
        {
            /// Nếu không có quà
            if (!KTPlayerPrayManager.HasAward(player))
            {
                KTPlayerManager.ShowNotification(player, "Không có quà thưởng!");
                return false;
            }

            /// Chuỗi thông tin quà
            string resultString = string.Join("_", player.LastPrayResult);

            /// Nếu không tồn tại thông tin quà thưởng tương ứng
            if (!KTPlayerPrayManager.PlayerPrays.TryGetValue(resultString, out PlayerPrayXML playerPrayData))
            {
                KTPlayerManager.ShowNotification(player, "Không có quà thưởng!");
                return false;
            }

            /// Khoảng trống cần trong túi
            int spaceNeed = 0;
            /// Duyệt danh sách vật phẩm thưởng
            foreach (KeyValuePair<int, int> pair in playerPrayData.Items)
            {
                spaceNeed += KTGlobal.GetTotalSpacesNeedToTakeItem(pair.Key, pair.Value);
            }
            /// Nếu túi đồ không đủ chỗ trống
            if (!KTGlobal.IsHaveSpace(spaceNeed, player))
            {
                KTPlayerManager.ShowNotification(player, string.Format("Cần sắp xếp lại tối thiểu {0} khoảng trống trong túi để nhận thưởng!", spaceNeed));
                return false;
            }

            /// Xóa chuỗi thông tin quà
            player.LastPrayResult.Clear();
            /// Lưu vào DB
            player.SavePrayDataToDB();

            /// Duyệt danh sách vật phẩm thưởng
            foreach (KeyValuePair<int, int> pair in playerPrayData.Items)
            {
                /// Thêm vật phẩm tương ứng
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, pair.Key, pair.Value, 0, "PlayerPray", true, 1, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo vật phẩm, hãy liên hệ với hỗ trợ để được trợ giúp!");
                    LogManager.WriteLog(LogTypes.PlayerPray, string.Format("[ERROR] Tạo vật phẩm ID {0} cho RoleID {1} lỗi SML.", pair.Key, player.RoleID));
                }
            }

            /// Duyệt danh sách Buff
            foreach (KeyValuePair<int, int> pair in playerPrayData.Buffs)
            {
                /// Thêm Buff tương ứng
                player.Buffs.AddBuff(pair.Key, pair.Value);
            }


            /// TODO ghi log


            /// Thêm danh vọng tương ứng
            KTGlobal.AddRepute(player, 504, playerPrayData.Repute);

            /// Thông báo nhận thưởng thành công
            KTPlayerManager.ShowNotification(player, string.Format("Nhận thưởng chúc phúc thành công, danh vọng chúc phúc tăng {0} điểm!", playerPrayData.Repute));

            /// Trả về kết quả nhận thành công
            return true;
        }
        #endregion
    }
}
