using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.GameEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.CargoCarriage
{
    /// <summary>
    /// Script sự kiện vận tiêu
    /// </summary>
    public class CargoCarriage_ActivityScript : IActivityScript
    {
        #region Define
        /// <summary>
        /// Hoạt động tương ứng
        /// </summary>
        public KTActivity Activity { get; set; }

        /// <summary>
        /// Danh sách NPC sự kiện
        /// </summary>
        private readonly List<NPC> listNPC = new List<NPC>();
        #endregion

        #region Core
        /// <summary>
        /// Trả về số lượt đã vận tiêu trong ngày của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetTotalRoundsToday(KPlayer player)
        {
            int totalRounds = player.GetValueOfDailyRecore((int) DailyRecord.CargoCarriage_TotalRoundToday);
            if (totalRounds <= 0)
            {
                totalRounds = 0;
            }
            return totalRounds;
        }

        /// <summary>
        /// Thiết lập số lượt đã vận tiêu trong ngày của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetTotalRoundsToday(KPlayer player, int value)
        {
            player.SetValueOfDailyRecore((int) DailyRecord.CargoCarriage_TotalRoundToday, value);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện xe tiêu bị phá hủy
        /// </summary>
        /// <param name="carriage"></param>
        /// <param name="taskData"></param>
        /// <param name="killer"></param>
        /// <param name="mapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void ProcessCarriageBeKilled(TraderCarriage carriage, CargoCarriage.CargoCarriageTaskData taskData, KPlayer killer, int mapID, int posX, int posY)
        {
            /// Toác
            if (taskData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thông tin vật phẩm rơi khi chết
            CargoCarriage.CargoCarriageData.DeadPunishmentData punishData = CargoCarriage.Data.DeathPunishments.Where(x => x.Type == taskData.Type).FirstOrDefault();
            /// Toác
            if (punishData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Danh sách vật phẩm sẽ rơi
            List<KeyValuePair<ItemData, int>> items = punishData.DropItems.Select(x => new KeyValuePair<ItemData, int>(ItemManager.GetItemTemplate(x.ItemID), x.Quantity)).ToList();
            /// Toác
            if (items == null || items.Count <= 0)
            {
                /// Bỏ qua
                return;
            }

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapID);
            /// Toác
            if (gameMap == null)
            {
                /// Bỏ qua
                return;
            }

            /// Chuyển sang tọa độ lưới
            UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2(posX, posY));

            /// Nếu có thằng giết
            if (killer != null)
            {
                /// Tạo vật phẩm rơi ra cho nó
                KTGoodsPackManager.CreateDropToMap(killer, items, (int) gridPos.x, (int) gridPos.y, carriage);
            }
            /// Nếu không có thằng giết
            else
            {
                /// Tạo vật phẩm rơi ra ở Map
                KTGoodsPackManager.CreateDropToMap(mapID, -1, (int) gridPos.x, (int) gridPos.y, items, carriage);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện giao nhiệm vụ vận tiêu cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        public static string GiveTask(KPlayer player, int type)
        {
            /// Nếu chưa nhận thưởng
            if (player.CurrentCompletedCargoCarriageTask != null)
            {
                return string.Format("Ngươi chưa nhận thưởng lần vận tiêu trước. Hãy đến <color=green>{0}</color> tìm <color=yellow>[{0}]</color> để nhận thưởng.", player.CurrentCompletedCargoCarriageTask.DoneNPC.Name);
            }
            /// Nếu chưa bắt đầu sự kiện
            else if (!CargoCarriage.IsStarted)
            {
                return "Sự kiện vận tiêu chưa được mở!";
            }
            /// Nếu đang có nhiệm vụ
            else if (player.CurrentCargoCarriageTask != null)
            {
                return "Ngươi còn đứng đây làm gì nữa, hãy mau mau đi làm nhiệm vụ đi!";
            }
            /// Nếu đã quá số lượt tham gia vận tiêu trong ngày
            else if (CargoCarriage_ActivityScript.GetTotalRoundsToday(player) >= CargoCarriage.Data.Config.MaxRoundsPerDay)
            {
                return string.Format("Ngươi đã tham gia quá <color=yellow>{0} lượt</color> vận tiêu hôm nay. Hãy quay lại hôm sau.", CargoCarriage.Data.Config.MaxRoundsPerDay);
            }

            /// Quãng đường vận tiêu tương ứng
            CargoCarriage.CargoCarriageData.CargoData pathData = CargoCarriage.Data.Paths.Where(x => x.Type == type).FirstOrDefault();
            /// Toác
            if (pathData == null)
            {
                return "Không tìm thấy thông tin vận tiêu tương ứng.";
            }

            /// Nếu cấp độ không đủ
            if (player.m_Level < pathData.RequireLevel)
            {
                return string.Format("Ngươi cần đạt <color=green>cấp {0}</color> mới có thể nhận vận chuyển xe tiêu loại này.", pathData.RequireLevel);
            }

            /// Danh sách quãng đường thỏa mãn
            List<CargoCarriage.CargoCarriageData.CargoData.CargoPath> paths = pathData.Paths.Where(x => x.MinLevel <= player.m_Level).ToList();
            /// Toác
            if (paths.Count <= 0)
            {
                return "Không tìm thấy thông tin vận tiêu tương ứng cấp độ nhân vật hiện tại.";
            }
            /// Chọn 1 quãng đường ngẫu nhiên
            CargoCarriage.CargoCarriageData.CargoData.CargoPath randomPath = paths.RandomRange(1).FirstOrDefault();
            /// Toác
            if (randomPath == null)
            {
                return "Không tìm thấy thông tin vận tiêu tương ứng.";
            }

            /// Chọn NPC bắt đầu ngẫu nhiên
            CargoCarriage.CargoCarriageData.NPCData startNPC = CargoCarriage.Data.NPCs.Where(x => x.MapID == randomPath.FromMapID).RandomRange(1).FirstOrDefault();
            /// Toác
            if (startNPC == null)
            {
                return "Không tìm thấy thông tin NPC giao tiêu tương ứng.";
            }

            /// Chọn NPC đích đến ngẫu nhiên
            CargoCarriage.CargoCarriageData.NPCData endNPC = CargoCarriage.Data.NPCs.Where(x => {
                /// Nếu chung bản đồ
                if (randomPath.FromMapID == randomPath.ToMapID)
                {
                    return x.MapID == randomPath.ToMapID && x != startNPC;
                }
                /// Nếu không chung bản đồ
                else
                {
                    return x.MapID == randomPath.ToMapID;
                }
            }).RandomRange(1).FirstOrDefault();
            /// Toác
            if (startNPC == null)
            {
                return "Không tìm thấy thông tin NPC trả tiêu tương ứng.";
            }

            /// Tạo nhiệm vụ mới
            CargoCarriage.CargoCarriageTaskData taskData = new CargoCarriage.CargoCarriageTaskData()
            {
                Type = type,
                BeginNPC = startNPC,
                DoneNPC = endNPC,
                MovePath = randomPath,
            };

            /// Số lượt đã vận tiêu trong ngày
            int totalRoundsToday = CargoCarriage_ActivityScript.GetTotalRoundsToday(player);
            /// Tăng số lượt đã nhận nhiệm vụ vận tiêu trong ngày
            totalRoundsToday++;
            /// Lưu lại
            CargoCarriage_ActivityScript.SetTotalRoundsToday(player, totalRoundsToday);

            /// Ghi lại thông tin nhiệm vụ cho nó
            player.CurrentCargoCarriageTask = taskData;

            /// Gửi gói tin nhận nhiệm vụ vận tiêu mới
            KT_TCPHandler.SendReceiveNewCargoCarriageTask(player);

            /// OK
            return "OK";
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Xóa toàn bộ NPC sự kiện cũ
        /// </summary>
        private void ClearAllNPCs()
        {
            foreach (NPC npc in this.listNPC)
            {
                KTNPCManager.Remove(npc);
            }
            this.listNPC.Clear();
        }
        #endregion

        #region Core ActivityScript
        /// <summary>
        /// Bắt đầu sự kiện vận tiêu
        /// </summary>
        public void Begin()
        {
            /// Nếu không có thông tin hoạt động
            if (this.Activity == null)
            {
                return;
            }

            /// Thông báo
            KTGlobal.SendSystemEventNotification("Sự kiện Vận Tiêu đã bắt đầu. Hãy đến tìm [Ông chủ tiêu cục] ở các thành thị để đăng ký vận tiêu.");
            /// Đánh dấu sự kiện đã bắt đầu
            CargoCarriage.IsStarted = true;

            /// Xóa NPC cũ
            this.ClearAllNPCs();

            /// Duyệt danh sách NPC
            foreach (CargoCarriage.CargoCarriageData.NPCData npcData in CargoCarriage.Data.NPCs)
            {
                /// Tạo NPC
                NPC npc = KTNPCManager.Create(new KTNPCManager.NPCBuilder()
                {
                    MapCode = npcData.MapID,
                    ResID = npcData.ID,
                    PosX = npcData.PosX,
                    PosY = npcData.PosY,
                    Name = npcData.Name,
                    Title = npcData.Title,
                });
                /// Sự kiện Click
                npc.Click = (player) =>
                {
                    CargoCarriage_TransferNPCScript.Click(player, npc, npcData);
                };
                /// Thêm vào danh sách
                this.listNPC.Add(npc);
            }
        }

        /// <summary>
        /// Kết thúc sự kiện vận tiêu
        /// </summary>
        public void Close()
        {
            /// Thông báo
            KTGlobal.SendSystemEventNotification("Sự kiện Vận Tiêu đã kết thúc. Hãy chuẩn bị tốt cho sự kiện lần tới.");
            /// Bỏ đánh dấu sự kiện đã bắt đầu
            CargoCarriage.IsStarted = false;

            /// Xóa NPC cũ
            this.ClearAllNPCs();
        }
        #endregion
    }
}
