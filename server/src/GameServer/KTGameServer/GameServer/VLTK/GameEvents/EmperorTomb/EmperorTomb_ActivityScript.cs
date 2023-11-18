using GameServer.KiemThe.GameEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.EmperorTomb
{
    /// <summary>
    /// Script hoạt động Tần Lăng
    /// </summary>
    public class EmperorTomb_ActivityScript : IActivityScript
    {
        #region Define
        /// <summary>
        /// Hoạt động tương ứng
        /// </summary>
        public KTActivity Activity { get; set; }

        /// <summary>
        /// Danh sách Script đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, EmperorTomb_Script_Main> scripts = new ConcurrentDictionary<int, EmperorTomb_Script_Main>();

        /// <summary>
        /// Lưu lại thời gian của người chơi ở trong Tần Lăng
        /// </summary>
        private static readonly ConcurrentDictionary<int, int> playerTimes = new ConcurrentDictionary<int, int>();
        #endregion

        #region Core ActivityScript
        /// <summary>
        /// Bắt đầu sự kiện
        /// </summary>
        public void Begin()
        {
            /// Nếu không có thông tin hoạt động
            if (this.Activity == null)
            {
                return;
            }

            /// ID sự kiện
            int activityID = this.Activity.Data.ID;
            /// Nếu là mở Tần Lăng
            if (activityID == 400)
            {
                float durationHourPerDay = EmperorTomb.Config.DurationPerDay / 3600000f;
                KTGlobal.SendSystemEventNotification(string.Format("Thủy Hoàng Địa Cung đã khai mở. Trong thời gian từ 9:00 đến 23:00, anh hùng hào kiệt cấp {0} trở lên có thể thông qua Sứ giả Tần Lăng tại các thành thị để tiến vào. Lưu ý mỗi ngày chỉ được vào tối đa {1} giờ.", EmperorTomb.Config.LimitLevel, Utils.Truncate(durationHourPerDay, 1)));

                /// Làm rỗng Script
                this.scripts.Clear();
                /// Làm rỗng thời gian vào
                EmperorTomb_ActivityScript.playerTimes.Clear();

                /// Duyệt danh sách tầng tương ứng
                foreach (KeyValuePair<int, EmperorTomb.EventDetail.Stage> pair in EmperorTomb.Event.Stages)
                {
                    /// Bản đồ tương ứng
                    GameMap map = KTMapManager.Find(pair.Key);
                    /// Nếu có bản đồ tương ứng
                    if (map != null)
                    {
                        /// Khởi tạo Script tương ứng
                        EmperorTomb_Script_Main script = new EmperorTomb_Script_Main(map, this.Activity);
                        /// Bắt đầu thực thi
                        script.Begin();
                        /// Thêm Script vào danh sách
                        this.scripts[script.Map.MapCode] = script;
                    }
                }
            }
            /// Nếu là thông báo xuất hiện Tần Thủy Hoàng
            else if (activityID == 401)
            {
                KTGlobal.SendSystemEventNotification("Tần Thủy Hoàng đã xuất hiện tại Thủy Hoàng Địa Cung, anh hùng hào kiệt hãy nhanh chân đánh bại!");
            }
            /// Nếu là kết thúc Tần Lăng
            else if (activityID == 402)
            {
                KTGlobal.SendSystemEventNotification("Thủy Hoàng Địa Cung đã đóng lại, hẹn quý bằng hữu lần tới!");
            }
        }

        /// <summary>
        /// Kết thúc sự kiện
        /// </summary>
        public void Close()
        {
            List<int> keys = this.scripts.Keys.ToList();
            /// Duyệt danh sách Script và thực hiện đóng lại
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!this.scripts.TryGetValue(key, out EmperorTomb_Script_Main script))
                {
                    continue;
                }

                /// Hủy Script
                script.Dispose();
            }
            /// Giải phóng bộ nhớ
            this.scripts.Clear();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về thời gian còn lại ở trong Tần Lăng trong ngày của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetTodayTotalSecLeft(KPlayer player)
        {
            /// Nếu không tồn tại trong danh sách
            if (!EmperorTomb_ActivityScript.playerTimes.ContainsKey(player.RoleID))
            {
                /// Trả về thời gian tối đa
                return EmperorTomb.Config.DurationPerDay;
            }
            /// Trả về kết quả
            return EmperorTomb_ActivityScript.playerTimes[player.RoleID];
        }

        /// <summary>
        /// Thiết lập thời gian còn lại ở trong Tần Lăng trong ngày của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="totalSecLeft"></param>
        /// <returns></returns>
        public static void SetTotayTotalSecLeft(KPlayer player, int totalSecLeft)
        {
            EmperorTomb_ActivityScript.playerTimes[player.RoleID] = totalSecLeft;
        }

        /// <summary>
        /// Kiểm tra điều kiện để người chơi có thể vào Tần Lăng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string EnterMap_CheckCondition(KPlayer player)
        {
            /// Nếu Tần Lăng chưa mở
            if (GameMapEventsManager.GetActivityScript(400) == null)
            {
                return "Thủy Hoàng Địa Cung hiện chưa mở, hãy quay lại sau!";
            }
            /// Nếu đã hết thời gian được ở trong Tần Lăng
            else if (EmperorTomb_ActivityScript.GetTodayTotalSecLeft(player) <= 0)
            {
                return "Ngươi đã hết thời gian ở trong Thủy Hoàng Địa Cung trong ngày, để đảm bảo an toàn tính mạng, hãy quay lại vào hôm sau!";
            }
            /// Nếu cấp độ không đủ
            else if (player.m_Level < EmperorTomb.Config.LimitLevel)
            {
                return string.Format("Ngươi cần đạt tối thiểu cấp {0} mới có thể tiến vào!", EmperorTomb.Config.LimitLevel);
            }

            /// Trả về kết quả có thể vào
            return "OK";
        }

        /// <summary>
        /// Dịch chuyển người chơi vào Tần Lăng
        /// </summary>
        /// <param name="player"></param>
        public static void MoveToEmperorTomb(KPlayer player)
        {
            /// Chuyển về bản đồ tương ứng
            KTPlayerManager.ChangeMap(player, EmperorTomb.Event.Relive.MapID, EmperorTomb.Event.Relive.PosX, EmperorTomb.Event.Relive.PosY);
        }

        /// <summary>
        /// Trục xuất người chơi khỏi Tần Lăng
        /// </summary>
        /// <param name="player"></param>
        public static void KickOut(KPlayer player)
        {
            /// Thông tin điểm về thành gần nhất
            KT_TCPHandler.GetPlayerDefaultRelivePos(player, out int mapCode, out int posX, out int posY);
            /// Chuyển về bản đồ tương ứng
            KTPlayerManager.ChangeMap(player, mapCode, posX, posY);
        }

        /// <summary>
        /// Thực hiện Tick liên tục khi người chơi ở trong Tần Lăng
        /// </summary>
        /// <param name="player"></param>
        public static void ProcessPlayerTick(KPlayer player)
        {
            /// Script điều khiển
            EmperorTomb_ActivityScript activityScript = GameMapEventsManager.GetActivityScript<EmperorTomb_ActivityScript>(400);
            /// Nếu Tần Lăng chưa mở
            if (activityScript == null)
            {
                return;
            }

            /// Thời gian còn lại
            int totalSecLeft = EmperorTomb_ActivityScript.GetTodayTotalSecLeft(player);
            /// Nếu đã hết thời gian
            if (totalSecLeft <= 0)
            {
                /// Nếu đang đợi chuyển Map thì thôi
                if (player.WaitingForChangeMap)
                {
                    return;
                }

                /// Trục xuất người chơi ra khỏi bản đồ
                EmperorTomb_ActivityScript.KickOut(player);

                /// Bỏ qua
                return;
            }

            /// Giảm thời gian còn lại
            totalSecLeft -= 500;
            /// Nếu dưới 0
            if (totalSecLeft < 0)
            {
                totalSecLeft = 0;
            }
            /// Lưu lại kết quả
            EmperorTomb_ActivityScript.SetTotayTotalSecLeft(player, totalSecLeft);

            /// Nếu tồn tại Script
            if (activityScript.scripts.TryGetValue(player.CurrentMapCode, out EmperorTomb_Script_Main script))
            {
                /// Thực thi sự kiện Tick người chơi tương ứng
                script.OnPlayerTick(player);
            }

            /// Nếu đã hết thời gian
            if (totalSecLeft == 0)
            {
                /// Nếu đang đợi chuyển Map thì thôi
                if (player.WaitingForChangeMap)
                {
                    return;
                }

                /// Trục xuất người chơi ra khỏi bản đồ
                EmperorTomb_ActivityScript.KickOut(player);
            }
        }
        #endregion
    }
}
