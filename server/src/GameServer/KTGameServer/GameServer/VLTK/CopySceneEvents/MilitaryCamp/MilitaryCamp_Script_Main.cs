using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen
{
    /// <summary>
    /// Script chính điều khiển phụ bản Quân doanh
    /// </summary>
    public partial class MilitaryCamp_Script_Main : CopySceneEvent
    {
        #region Constants
        /// <summary>
        /// Thời gian mỗi lần thông báo thông tin sự kiện tới người chơi
        /// </summary>
        private const int NotifyActivityInfoToPlayersEveryTick = 5000;
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng Mutex dùng khóa Lock
        /// </summary>
        private readonly object Mutex = new object();

        /// <summary>
        /// Thời điểm thông báo cập nhật thông tin sự kiện tới tất cả người chơi lần trước
        /// </summary>
        private long LastNotifyTick;

        /// <summary>
        /// Loại phụ bản
        /// </summary>
        private readonly MilitaryCamp.EventInfo EventData;

        /// <summary>
        /// ID nhóm
        /// </summary>
        private int teamID = -1;

        /// <summary>
        /// Thứ tự tầng hiện tại
        /// </summary>
        private int currentStageID = 0;

        /// <summary>
        /// Danh sách nhiệm vụ đã hoàn thành
        /// </summary>
        private readonly HashSet<int> completedTasks = new HashSet<int>();

        /// <summary>
        /// Danh sách nhiệm vụ đang làm
        /// </summary>
        private readonly HashSet<int> doingTasks = new HashSet<int>();

        /// <summary>
        /// Danh sách NPC cửa
        /// </summary>
        private readonly Dictionary<int, NPC> obstacleNPCs = new Dictionary<int, NPC>();

        /// <summary>
        /// Thời điểm hoàn thành phụ bản
        /// </summary>
        private long eventCompletedTicks = -1;

        /// <summary>
        /// Danh sách các đối tượng tạo ra ở nhiệm vụ hiện tại
        /// </summary>
        private readonly Dictionary<int, List<object>> currentTaskObjects = new Dictionary<int, List<object>>();
        #endregion

        #region Core CopySceneEvent
        /// <summary>
        /// Script chính điều khiển phụ bản Quân doanh
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="data"></param>
        public MilitaryCamp_Script_Main(KTCopyScene copyScene, MilitaryCamp.EventInfo data) : base(copyScene)
        {
            /// Đánh dấu phụ bản
            this.EventData = data;
        }

        /// <summary>
        /// Hàm này gọi khi bắt đầu phụ bản
        /// </summary>
        protected override void OnStart()
        {
            /// Xóa toàn bộ NPC
			this.RemoveAllNPCs();
            /// Xóa toàn bộ quái
            this.RemoveAllMonsters();
            /// Xóa toàn bộ điểm thu thập
            this.RemoveAllGrowPoints();
            /// Xóa toàn bộ cổng dịch chuyển
            this.RemoveAllDynamicAreas();
            /// Cập nhật thời điểm thông báo gần nhất
            this.LastNotifyTick = 0;
            /// Tạo các NPC tĩnh
            this.CreateStaticNPCs();
            /// Bắt đầu từ tầng 1
            this.currentStageID = 0;
            /// Bắt đầu
            this.BeginStage();
        }

        /// <summary>
        /// Hàm này gọi liên tục trong phụ bản
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu đã đến thời gian thông báo thông tin sự kiện
            if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= MilitaryCamp_Script_Main.NotifyActivityInfoToPlayersEveryTick)
            {
                /// Đánh dấu thời gian thông báo thông tin sự kiện
                this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                /// Nếu đã hoàn thành
                if (this.eventCompletedTicks != -1)
                {
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventDetailsToPlayers(this.CopyScene.Name, 30000 - (KTGlobal.GetCurrentTimeMilis() - this.eventCompletedTicks), "Hoàn thành nhiệm vụ!");
                }
                /// Nếu chưa hoàn thành
                else
                {
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventDetailsToPlayers(this.CopyScene.Name, this.CopyScene.DurationTicks - this.LifeTimeTicks, this.GetEventBroadboardDetailText());
                }
            }

            /// Nếu đã hoàn thành phụ bản
            if (this.eventCompletedTicks != -1)
            {
                /// Nếu đã hết thời gian đếm lùi
                if (KTGlobal.GetCurrentTimeMilis() - this.eventCompletedTicks >= 30000)
                {
                    /// Đưa tất cả người chơi ra khỏi bản đồ
                    this.KickOutPlayers();
                    /// Hủy phụ bản
                    this.Dispose();
                    /// Bỏ qua
                    return;
                }
                /// Bỏ qua
                return;
            }

            /// Nếu tầng hiện tại đã hoàn thành
            if (this.IsCurrentStageCompleted())
            {
                /// Thực thi sự kiện hoàn tất Stage
                this.CompleteStage();
                /// Chuyển sang tầng tiếp theo
                this.currentStageID++;
                /// Nếu đã hoàn thành phụ bản
                if (this.currentStageID >= this.EventData.Stages.Count)
                {
                    /// Hoàn thành phụ bản
                    this.CompleteEvent();
                    /// Bỏ qua
                    return;
                }
                /// Nếu chưa hoàn thành phụ bản
                else
                {
                    /// Bắt đầu tầng mới
                    this.BeginStage();
                }
            }
            /// Nếu tâng hiện tại chưa hoàn thành
            else
            {
                /// Theo dõi nhiệm vụ
                this.HandleTasks();
            }
        }

        /// <summary>
        /// Hàm này gọi khi kết thúc phụ bản
        /// </summary>
        protected override void OnClose()
        {
            this.completedTasks.Clear();
            this.doingTasks.Clear();
            this.obstacleNPCs.Clear();

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
            /// Nếu tồn tại
            if (gameMap != null)
            {
                gameMap.MyNodeGrid.ClearDynamicObsLabels(this.CopyScene.ID);
            }
        }

		/// <summary>
		/// Hàm này gọi đến khi người chơi giết quái
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		public override void OnKillObject(KPlayer player, GameObject obj)
		{
			base.OnKillObject(player, obj);
		}

		/// <summary>
		/// Hàm này gọi đến khi người chơi chết
		/// </summary>
		/// <param name="killer"></param>
		/// <param name="player"></param>
		public override void OnPlayerDie(GameObject killer, KPlayer player)
		{
			base.OnPlayerDie(killer, player);
		}

		/// <summary>
		/// Hàm này gọi đến khi người chơi vào phụ bản
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnter(KPlayer player)
		{
			base.OnPlayerEnter(player);
			/// Mở bảng thông báo hoạt động
			this.OpenEventBroadboard(player, (int) GameEvent.MilitaryCamp);
			/// Chuyển trạng thái PK hòa bình
			player.PKMode = (int) PKMode.Peace;
			/// Cập nhật thông tin sự kiện
			this.UpdateEventDetailsToPlayers(this.CopyScene.Name, this.CopyScene.DurationTicks - this.LifeTimeTicks, this.GetEventBroadboardDetailText());
		}

		/// <summary>
		/// Hàm này gọi đến khi người chơi rời phụ bản
		/// </summary>
		/// <param name="player"></param>
		/// <param name="toMap"></param>
		public override void OnPlayerLeave(KPlayer player, GameMap toMap)
		{
			base.OnPlayerLeave(player, toMap);
			/// Chuyển trạng thái PK hòa bình
			player.PKMode = (int) PKMode.Peace;
            /// Đóng bảng thông báo hoạt động
            this.CloseEventBroadboard(player, (int) GameEvent.MilitaryCamp);
		}
        #endregion

        #region Private methods
        /// <summary>
        /// Trả về chuỗi theo dõi tiến độ phụ bản
        /// </summary>
        /// <returns></returns>
        private string[] GetEventBroadboardDetailText()
        {
            /// Nếu quá phạm vi
            if (this.currentStageID < 0 || this.currentStageID >= this.EventData.Stages.Count)
            {
                /// Toác
                return new string[0];
            }

            /// Chuỗi thông tin tiến độ phụ bản
            List<string> detailTexts = new List<string>();

            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (MilitaryCamp.EventInfo.StageInfo.TaskInfo task in stageInfo.Tasks)
            {
                /// Nếu nhiệm vụ này đang được thực hiện
                if (this.doingTasks.Contains(task.ID))
                {
                    /// Tạo mới StringBuilder
                    StringBuilder builder = new StringBuilder();
                    /// Thêm tên nhiệm vụ vào
                    builder.AppendLine(string.Format("<color=orange>{0}</color>", task.Name));
                    /// Loại nhiệm vụ là gì
                    switch (task.Type)
                    {
                        /// Thu thập vật phẩm
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.CollectGrowPoints:
                        {
                            /// Danh sách vật phẩm yêu cầu
                            List<string> requireItemStrings = new List<string>();
                            /// Duyệt danh sách vật phẩm yêu cầu
                            foreach (KeyValuePair<int, int> pair in task.Target.Items)
                            {
                                /// Thông tin vật phẩm
                                ItemData itemData = ItemManager.GetItemTemplate(pair.Key);
                                /// Toác
                                if (itemData == null)
                                {
                                    continue;
                                }

                                /// Thêm vào danh sách
                                requireItemStrings.Add(string.Format("<color=yellow>[{0}]</color> - <color=#48eadc>SL: {1}</color>", itemData.Name, pair.Value));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Thu thập {0}", string.Join(", ", requireItemStrings));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Tiêu diệt Boss
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.KillBoss:
                        {
                            /// Danh sách Boss yêu cầu
                            List<string> requireKills = new List<string>();
                            /// Boss chính
                            requireKills.Add(string.Format("<color=yellow>[{0}]</color>", task.Boss.Name));
                            ///// Nếu có Boss phụ
                            //if (task.ChildBoss != null)
                            //{
                            //    /// Boss phụ
                            //    requireKills.Add(string.Format("<color=yellow>[{0}]</color>", task.ChildBoss.Name));
                            //}

                            /// Chuỗi thông tin
                            string desc = string.Format("- Tiêu diệt {0}", string.Join(", ", requireKills));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Giao vật phẩm cho NPC
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GiveGoodsToNPC:
                        {
                            /// Danh sách vật phẩm yêu cầu
                            List<string> requireItemStrings = new List<string>();
                            /// Duyệt danh sách vật phẩm yêu cầu
                            foreach (KeyValuePair<int, int> pair in task.Target.Items)
                            {
                                /// Thông tin vật phẩm
                                ItemData itemData = ItemManager.GetItemTemplate(pair.Key);
                                /// Toác
                                if (itemData == null)
                                {
                                    continue;
                                }

                                /// Thêm vào danh sách
                                requireItemStrings.Add(string.Format("<color=yellow>[{0}]</color> - <color=#48eadc>SL: {1}</color>", itemData.Name, pair.Value));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Đem {0} đến cho <color=yellow>[{1}]</color>", string.Join(", ", requireItemStrings), task.NPC.Name);
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở cơ quan và tiêu diệt quái
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTriggerAndKillMonsters:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", task.Trigger.Name));
                            /// Danh sách quái cần tiêu diệt
                            List<string> requireKillMonsters = new List<string>();
                            /// Nhóm danh sách quái lại theo ID
                            List<KeyValuePair<string, int>> monsters = task.Monsters.GroupBy(x => x.ID).Select(x => new KeyValuePair<string, int>(x.Select(y => y.Name).FirstOrDefault(), x.Count())).ToList();
                            /// Duyệt danh sách quái
                            foreach (KeyValuePair<string, int> pair in monsters)
                            {
                                requireKillMonsters.Add(string.Format("<color=yellow>[{0}]</color> - <color=#48eadc>SL: {1}</color>", pair.Key, pair.Value));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Mở {0}\n- Tiêu diệt {1}", string.Join(", ", requireOpenTriggers), string.Join(", ", requireKillMonsters));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Hộ tống NPC
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.TransferNPC:
                        {
                            /// Chuỗi thông tin
                            string desc = string.Format("- Hộ tống <color=yellow>[{0}]</color>", task.MovingNPC.Name);
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở toàn bộ cơ quan theo thứ tự
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenOrderedTriggers:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            /// Duyệt danh sách cơ quan
                            foreach (MilitaryCamp.EventInfo.StageInfo.TaskInfo.IndexTriggerInfo.OrderedTriggerInfo triggerData in task.IndexTriggers.Triggers)
                            {
                                requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", triggerData.Name));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Mở {0} theo thứ tự", string.Join(", ", requireOpenTriggers));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở cơ quan
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTrigger:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", task.Trigger.Name));

                            /// Chuỗi thông tin
                            string desc = string.Format("- Mở {0}", string.Join(", ", requireOpenTriggers));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Đoán số
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GuessNumber:
                        {
                            /// Chuỗi thông tin
                            string desc = string.Format("- Đoán số");
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở toàn bộ cơ quan cùng lúc
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAllTriggersSameMoment:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", task.IndexTriggers.Triggers.FirstOrDefault().Name));

                            /// Chuỗi thông tin
                            string desc = string.Format("- Mở {0} cùng lúc", string.Join(", ", requireOpenTriggers));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở cơ quan và dập lửa
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAndProtectAllTriggers:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", task.ProtectTrigger.Triggers.FirstOrDefault().Name));

                            /// Chuỗi thông tin
                            string desc = string.Format("- Khai mở và bảo vệ {0}", string.Join(", ", requireOpenTriggers));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                    }

                    /// Thêm vào danh sách
                    detailTexts.Add(builder.ToString());
                }
            }

            /// Trả về kết quả
            return detailTexts.ToArray();
        }

        /// <summary>
        /// Tạo các NPC tĩnh
        /// </summary>
        private void CreateStaticNPCs()
        {
            /// Duyệt danh sách NPC tĩnh
            foreach (MilitaryCamp.EventInfo.NPCInfo npcInfo in this.EventData.NPCs)
            {
                /// Tạo NPC tương ứng
                this.CreateStaticNPC(npcInfo);
            }
        }

        /// <summary>
        /// Tạo cơ quan thuộc Stage tương ứng
        /// </summary>
        /// <param name="stageInfo"></param>
        private void CreateStageTriggers(MilitaryCamp.EventInfo.StageInfo stageInfo)
        {
            /// Duyệt danh sách cơ quan mở đường
            foreach (MilitaryCamp.EventInfo.StageInfo.TriggerData.KeyTriggerInfo triggerInfo in stageInfo.Triggers.KeyTriggers.Values)
            {
                /// Tạo cơ quan
                GrowPoint trigger = KTGrowPointManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, GrowPointXML.Parse(triggerInfo.ID, triggerInfo.Name, -1, -1, triggerInfo.CollectTick, true), triggerInfo.PosX, triggerInfo.PosY);
                trigger.GrowPointCollectCompleted = (player) => {
                    /// Xóa cơ quan
                    this.RemoveGrowPoint(trigger);
                    /// Thông tin bản đồ tương ứng
                    GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
                    /// Mở Dynamic-Obs này
                    gameMap.MyNodeGrid.OpenDynamicObsLabel(this.CopyScene.ID, triggerInfo.DynamicObsLabel);
                    /// Xóa cổng tương ứng
                    if (this.obstacleNPCs.TryGetValue(triggerInfo.ObstacleTriggerID, out NPC npc))
                    {
                        KTNPCManager.Remove(npc);
                    }
                };
            }

            /// Duyệt danh sách cơ quan cản đường
            foreach (MilitaryCamp.EventInfo.StageInfo.TriggerData.ObstacleTriggerInfo triggerInfo in stageInfo.Triggers.ObstacleTriggers.Values)
            {
                /// Tạo cơ quan
                NPC npc = KTNPCManager.Create(new KTNPCManager.NPCBuilder()
                {
                    MapCode = this.CopyScene.MapCode,
                    CopySceneID = this.CopyScene.ID,
                    ResID = triggerInfo.ID,
                    PosX = triggerInfo.PosX,
                    PosY = triggerInfo.PosY,
                    Name = triggerInfo.Name,
                });
                this.obstacleNPCs[triggerInfo.TriggerID] = npc;
            }

            /// Duyệt danh sách bẫy
            foreach (MilitaryCamp.EventInfo.StageInfo.TriggerData.TrapInfo trapInfo in stageInfo.Triggers.Traps)
            {
                /// Tạo bẫy
                KDynamicArea trap = KTDynamicAreaManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, trapInfo.Name, trapInfo.ID, trapInfo.PosX, trapInfo.PosY, -1, 1000, 100, -1, null);
                trap.OnEnter = (obj) => {
                    if (obj is KPlayer player)
                    {
                        /// Thông báo dính phải bẫy
                        KTPlayerManager.ShowNotification(player, trapInfo.TouchMessage);
                        /// Quay về vị trí xuất phát
                        KTPlayerManager.ChangePos(player, this.CopyScene.EnterPosX, this.CopyScene.EnterPosY);
                    }
                };
            }
        }

        /// <summary>
        /// Tạo quái thuộc Stage tương ứng
        /// </summary>
        /// <param name="stageInfo"></param>
        private void CreateStageMonsters(MilitaryCamp.EventInfo.StageInfo stageInfo)
        {
            /// Duyệt danh sách quái
            foreach (MilitaryCamp.EventInfo.StageInfo.MonsterInfo monsterInfo in stageInfo.Monsters)
            {
                /// Tạo quái
                this.CreateMonster(null, monsterInfo);
            }
        }

        /// <summary>
        /// Tạo cổng dịch chuyển lúc bắt đầu Stage tương ứng
        /// </summary>
        /// <param name="stageInfo"></param>
        private void CreateStageImmediateTeleports(MilitaryCamp.EventInfo.StageInfo stageInfo)
        {
            /// Duyệt danh sách cổng dịch chuyển
            foreach (MilitaryCamp.EventInfo.StageInfo.TeleportInfo teleportInfo in stageInfo.Teleports)
            {
                /// Nếu là cổng dịch chuyển lúc bắt đầu Stage
                if (teleportInfo.SpawnImmediate)
                {
                    /// Tạo cổng
                    this.CreateTeleport(null, teleportInfo);
                }
            }
        }

        /// <summary>
        /// Tạo cổng dịch chuyển lúc hoàn thành Stage tương ứng
        /// </summary>
        /// <param name="stageInfo"></param>
        private void CreateStageDoneTeleports(MilitaryCamp.EventInfo.StageInfo stageInfo)
        {
            /// Duyệt danh sách cổng dịch chuyển
            foreach (MilitaryCamp.EventInfo.StageInfo.TeleportInfo teleportInfo in stageInfo.Teleports)
            {
                /// Nếu là cổng dịch chuyển lúc kết thúc Stage
                if (!teleportInfo.SpawnImmediate)
                {
                    /// Tạo cổng
                    this.CreateTeleport(null, teleportInfo);
                }
            }
        }

        /// <summary>
        /// Bắt đầu Stage
        /// </summary>
        private void BeginStage()
        {
            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];
            /// Tạo cơ quan
            this.CreateStageTriggers(stageInfo);
            /// Tạo quái
            this.CreateStageMonsters(stageInfo);
            /// Tạo cổng dịch chuyển bắt đầu
            this.CreateStageImmediateTeleports(stageInfo);

            /// Thiết lập lại danh sách nhiệm vụ đã hoàn thành
            this.completedTasks.Clear();
            /// Thiết lập lại danh sách nhiệm vụ đang thực hiện
            this.doingTasks.Clear();
        }

        /// <summary>
        /// Đã hoàn thành tầng hiện tại chưa
        /// </summary>
        /// <returns></returns>
        private bool IsCurrentStageCompleted()
        {
            /// Nếu đã vượt quá tổng số tầng
            if (this.currentStageID >= this.EventData.Stages.Count)
            {
                /// Đã hoàn thành toàn bộ
                return true;
            }
            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];
            /// Nếu đã hoàn thành tất cả chuỗi nhiệm vụ của tầng thì hoàn thành tầng hiện tại
            return stageInfo.Tasks.Count <= this.completedTasks.Count;
        }

        /// <summary>
        /// Hoàn thành Stage
        /// </summary>
        private void CompleteStage()
        {
            /// Nếu đã vượt quá tổng số tầng
            if (this.currentStageID >= this.EventData.Stages.Count)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];
            /// Tạo cổng dịch chuyển hoàn thành
            this.CreateStageDoneTeleports(stageInfo);
            /// Xóa toàn bộ nhiệm vụ đã làm
            this.doingTasks.Clear();
            /// Xóa toàn bộ nhiệm vụ đã hoàn thành
            this.completedTasks.Clear();
        }

        /// <summary>
        /// Hoàn thành phụ bản
        /// </summary>
        private void CompleteEvent()
        {
            /// Đánh dấu thời điểm hoàn thành phụ bản
            this.eventCompletedTicks = KTGlobal.GetCurrentTimeMilis();
            /// Thông báo hoàn thành
            this.NotifyAllPlayers(string.Format("Hoàn thành {0}, sau 30s sẽ rời khỏi phụ bản!", this.EventData.Name));
        }

        /// <summary>
        /// Xử lý các nhiệm vụ của tầng
        /// </summary>
        private void HandleTasks()
        {
            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (MilitaryCamp.EventInfo.StageInfo.TaskInfo task in stageInfo.Tasks)
            {
                /// Nếu nhiệm vụ này đã được hoàn thành
                if (this.completedTasks.Contains(task.ID))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu nhiệm vụ này đang được thực hiện
                if (this.doingTasks.Contains(task.ID))
                {
                    /// Đã hoàn thành chưa
                    bool isCompleted = false;

                    /// Loại nhiệm vụ là gì
                    switch (task.Type)
                    {
                        /// Thu thập vật phẩm
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.CollectGrowPoints:
                        {
                            isCompleted = this.Track_CollectGrowPoints(task);
                            break;
                        }
                        /// Tiêu diệt Boss
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.KillBoss:
                        {
                            isCompleted = this.Track_KillBoss(task);
                            break;
                        }
                        /// Giao vật phẩm cho NPC
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GiveGoodsToNPC:
                        {
                            isCompleted = this.Track_GiveGoodsToNPC(task);
                            break;
                        }
                        /// Mở cơ quan và tiêu diệt quái
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTriggerAndKillMonsters:
                        {
                            isCompleted = this.Track_OpenTriggerAndKillMonsters(task);
                            break;
                        }
                        /// Hộ tống NPC
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.TransferNPC:
                        {
                            isCompleted = this.Track_TransferNPC(task);
                            break;
                        }
                        /// Mở toàn bộ cơ quan theo thứ tự
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenOrderedTriggers:
                        {
                            isCompleted = this.Track_OpenOrderedTriggers(task);
                            break;
                        }
                        /// Mở cơ quan
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTrigger:
                        {
                            isCompleted = this.Track_OpenTrigger(task);
                            break;
                        }
                        /// Đoán số
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GuessNumber:
                        {
                            isCompleted = this.Track_GuessNumber(task);
                            break;
                        }
                        /// Mở toàn bộ cơ quan cùng lúc
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAllTriggersSameMoment:
                        {
                            isCompleted = this.Track_OpenAllTriggersSameMoment(task);
                            break;
                        }
                        /// Mở cơ quan và dập lửa
                        case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAndProtectAllTriggers:
                        {
                            isCompleted = this.Track_OpenAndProtectAllTriggers(task);
                            break;
                        }
                    }

                    /// Nếu đã hoàn thành
                    if (isCompleted)
                    {
                        /// Thông báo hoàn thành nhiệm vụ
                        this.NotifyAllPlayers(string.Format("Hoàn thành nhiệm vụ: {0}", task.Name));

                        /// Thêm nhiệm vụ vào danh sách đã hoàn thành
                        this.completedTasks.Add(task.ID);

                        /// Xóa toàn bộ các đối tượng nhiệm vụ được tạo ra từ nhiệm vụ trước
                        this.RemoveAllPreviousTaskObjects(task.ID);

                        /// Reset toàn bộ dữ liệu nhiệm vụ
                        this.Reset_CollectGrowPoints(task);
                        this.Reset_KillBoss(task);
                        this.Reset_GiveGoodsToNPC(task);
                        this.Reset_OpenTriggerAndKillMonsters(task);
                        this.Reset_TransferNPC(task);
                        this.Reset_OpenOrderedTriggers(task);
                        this.Reset_OpenTrigger(task);
                        this.Reset_GuessNumber(task);
                        this.Reset_OpenAllTriggersSameMoment(task);
                        this.Reset_OpenAndProtectAllTriggers(task);
                    }
                }
            }

            /// Giao cho các nhiệm vụ tiếp theo
            this.GiveNextTask();
        }

        /// <summary>
        /// Thêm nhiệm vụ tiếp theo
        /// </summary>
        private void GiveNextTask()
        {
            /// Thông tin tầng
            MilitaryCamp.EventInfo.StageInfo stageInfo = this.EventData.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (MilitaryCamp.EventInfo.StageInfo.TaskInfo task in stageInfo.Tasks)
            {
                /// Nếu nhiệm vụ này đã được hoàn thành
                if (this.completedTasks.Contains(task.ID))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu nhiệm vụ này chưa được thực hiện
                if (!this.doingTasks.Contains(task.ID))
                {
                    /// Biến đánh dấu đã hoàn thành tất cả nhiệm vụ yêu cầu chưa
                    bool isCompletedAllRequireTask = true;
                    /// Duyệt danh sách nhiệm vụ yêu cầu
                    foreach (int requireTaskID in task.RequireTasks)
                    {
                        /// Nếu chưa hoàn thành
                        if (!this.completedTasks.Contains(requireTaskID))
                        {
                            /// Đánh dấu chưa hoàn thành
                            isCompletedAllRequireTask = false;
                            /// Thoát
                            break;
                        }
                    }

                    /// Nếu đã hoàn thành tất cả nhiệm vụ yêu cầu
                    if (isCompletedAllRequireTask)
                    {
                        /// Duyệt danh sách nhiệm vụ yêu cầu trước đó
                        foreach (int requireTaskID in task.RequireTasks)
                        {
                            /// Xóa khỏi danh sách đang làm
                            this.doingTasks.Remove(requireTaskID);
                        }

                        /// Cho nhận nhiệm vụ này
                        this.doingTasks.Add(task.ID);

                        /// Loại nhiệm vụ là gì
                        switch (task.Type)
                        {
                            /// Thu thập vật phẩm
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.CollectGrowPoints:
                            {
                                this.Begin_CollectGrowPoints(task);
                                break;
                            }
                            /// Tiêu diệt Boss
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.KillBoss:
                            {
                                this.Begin_KillBoss(task);
                                break;
                            }
                            /// Giao vật phẩm cho NPC
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GiveGoodsToNPC:
                            {
                                this.Begin_GiveGoodsToNPC(task);
                                break;
                            }
                            /// Mở cơ quan và tiêu diệt quái
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTriggerAndKillMonsters:
                            {
                                this.Begin_OpenTriggerAndKillMonsters(task);
                                break;
                            }
                            /// Hộ tống NPC
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.TransferNPC:
                            {
                                this.Begin_TransferNPC(task);
                                break;
                            }
                            /// Mở toàn bộ cơ quan theo thứ tự
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenOrderedTriggers:
                            {
                                this.Begin_OpenOrderedTriggers(task);
                                break;
                            }
                            /// Mở cơ quan
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenTrigger:
                            {
                                this.Begin_OpenTrigger(task);
                                break;
                            }
                            /// Đoán số
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.GuessNumber:
                            {
                                this.Begin_GuessNumber(task);
                                break;
                            }
                            /// Mở toàn bộ cơ quan cùng lúc
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAllTriggersSameMoment:
                            {
                                this.Begin_OpenAllTriggersSameMoment(task);
                                break;
                            }
                            /// Mở cơ quan và dập lửa
                            case MilitaryCamp.EventInfo.StageInfo.TaskInfo.TaskType.OpenAndProtectAllTriggers:
                            {
                                this.Begin_OpenAndProtectAllTriggers(task);
                                break;
                            }
                        }

                        /// Thông báo bắt đầu nhiệm vụ
                        this.NotifyAllPlayers(string.Format("Bắt đầu nhiệm vụ: {0}", task.Name));
                    }
                }
            }
        }

        /// <summary>
		/// Đưa người chơi trở lại bản đồ báo danh
		/// </summary>
		/// <param name="player"></param>
		private void KickOutPlayer(KPlayer player)
        {
            KTPlayerManager.ChangeMap(player, this.EventData.Config.OutMapID, this.EventData.Config.OutPosX, this.EventData.Config.OutPosY);
        }

        /// <summary>
        /// Đưa tất cả người chơi trở lại bản đồ báo danh
        /// </summary>
        private void KickOutPlayers()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                this.KickOutPlayer(player);
            }
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Trả về chuỗi mã hóa ID nhiệm vụ tầng hiện tại
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private string GetTaskKey(MilitaryCamp.EventInfo.StageInfo.TaskInfo task)
        {
            /// Trả về kết quả
            return string.Format("{0}_{1}", this.currentStageID, task.ID);
        }

        /// <summary>
        /// Xóa toàn bộ các đối tượng nhiệm vụ lần trước
        /// </summary>
        /// <param name="previousTaskID"></param>
        private void RemoveAllPreviousTaskObjects(int previousTaskID)
        {
            /// Toác
            if (!this.currentTaskObjects.ContainsKey(previousTaskID))
            {
                /// Bỏ qua
                return;
            }


            /// Duyệt danh sách đối tượng
            foreach (object obj in this.currentTaskObjects[previousTaskID])
            {
                /// Nếu là GrowPoint
                if (obj is GrowPoint gp)
                {
                    this.RemoveGrowPoint(gp);
                }
                /// Nếu là NPC
                else if (obj is NPC npc)
                {
                    this.RemoveNPC(npc);
                }
                /// Nếu là quái
                else if (obj is Monster monster)
                {
                    this.RemoveMonster(monster);
                }
            }

            /// Làm rỗng danh sách
            this.currentTaskObjects.Remove(previousTaskID);
        }

        /// <summary>
        /// Tạo điểm thu thập tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void CreateGrowPoints(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.GrowPointInfo data)
        {
            /// <summary>
            /// Thực hiện tạo điểm thu thập tại vị trí tương ứng
            /// </summary>
            /// <param name="posX"></param>
            /// <param name="posY"></param>
            void DoCreate(int posX, int posY)
            {
                /// Tạo điểm thu thập
                GrowPoint growPoint = KTGrowPointManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, GrowPointXML.Parse(data.ID, data.Name, data.DisapearAfterBeenCollected ? -1 : data.RespawnTicks, -1, data.CollectTick, true), posX, posY);
                growPoint.ConditionCheck = (player) => {
                    /// Nếu túi đã đầy
                    if (!KTGlobal.IsHaveSpace(1, player))
                    {
                        KTPlayerManager.ShowNotification(player, "Túi đã đầy, không thể thu thập!");
                        return false;
                    }

                    /// OK
                    return true;
                };
                growPoint.GrowPointCollectCompleted = (player) => {
                    /// Nếu có đánh dấu xóa sau khi thu thập
                    if (data.DisapearAfterBeenCollected)
                    {
                        /// Xóa
                        this.RemoveGrowPoint(growPoint);
                    }
                    /// Thêm vật phẩm tương ứng vào người
                    ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, data.ItemID, 1, 0, "MilitaryCamp_EventScript", true, data.BoundAfterBeenCollected ? 1 : 0, false, ItemManager.ConstGoodsEndTime, "", -1);
                };

                /// Nếu có thông tin Task
                if (task != null)
                {
                    /// Nếu chưa tồn tại
                    if (!this.currentTaskObjects.ContainsKey(task.ID))
                    {
                        /// Tạo mới
                        this.currentTaskObjects[task.ID] = new List<object>();
                    }
                    /// Thêm vào danh sách các đối tượng được tạo ra
                    this.currentTaskObjects[task.ID].Add(growPoint);
                }
            }

            /// Nếu tổng số điểm thu thập cần sinh ra lớn hơn số vị trí điểm thu thập được thiết lập
            if (data.Count > data.Positions.Count)
            {
                /// Chọn ngẫu nhiên đè nhau
                for (int i = 1; i <= data.Count; i++)
                {
                    /// Vị trí ngẫu nhiên
                    int randomPos = KTGlobal.GetRandomNumber(0, data.Positions.Count - 1);
                    /// Tạo điểm thu thập tại vị trí tương ứng
                    DoCreate(data.Positions[randomPos].x, data.Positions[randomPos].y);
                }
            }
            /// Nếu tổng số điểm thu thập cần sinh ra bằng số vị trí điểm thu thập được thiết lập
            else if (data.Count == data.Positions.Count)
            {
                /// Duyệt danh sách vị trí
                foreach (UnityEngine.Vector2Int pos in data.Positions)
                {
                    /// Tạo điểm thu thập tại vị trí tương ứng
                    DoCreate(pos.x, pos.y);
                }
            }
            /// Nếu tổng số điểm thu thập cần sinh ra nhỏ hơn số vị trí điểm thu thập được thiết lập
            else
            {
                /// Danh sách vị trí ngẫu nhiên
                List<UnityEngine.Vector2Int> randomPositions = data.Positions.RandomRange(data.Count).ToList();
                /// Duyệt danh sách vị trí
                foreach (UnityEngine.Vector2Int pos in randomPositions)
                {
                    /// Tạo điểm thu thập tại vị trí tương ứng
                    DoCreate(pos.x, pos.y);
                }
            }
        }

        /// <summary>
        /// Tạo cơ quan
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void CreateTrigger(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.TriggerInfo data, Action<GrowPoint, KPlayer> onOpen)
        {
            /// Tạo cơ quan
            GrowPoint trigger = KTGrowPointManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, GrowPointXML.Parse(data.ID, data.Name, -1, -1, data.CollectTick, true), data.PosX, data.PosY);
            trigger.GrowPointCollectCompleted = (player) => {
                /// Thực thi sự kiện khi mở cơ quan
                onOpen?.Invoke(trigger, player);
            };

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(trigger);
            }
        }

        /// <summary>
        /// Tạo cơ quan mở theo thứ tự
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onOpen"></param>
        private void CreateIndexTriggers(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.IndexTriggerInfo data, Action<GrowPoint, KPlayer, MilitaryCamp.EventInfo.StageInfo.TaskInfo.IndexTriggerInfo.OrderedTriggerInfo> onOpen)
        {
            /// <summary>
            /// Thực hiện tạo cơ quan tại vị trí tương ứng
            /// </summary>
            /// <param name="triggerInfo"></param>
            /// <param name="posX"></param>
            /// <param name="posY"></param>
            void DoCreate(MilitaryCamp.EventInfo.StageInfo.TaskInfo.IndexTriggerInfo.OrderedTriggerInfo triggerInfo, int posX, int posY)
            {
                /// Tạo cơ quan
                GrowPoint trigger = KTGrowPointManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, GrowPointXML.Parse(triggerInfo.ID, triggerInfo.Name, -1, -1, triggerInfo.CollectTick, true), posX, posY);
                trigger.GrowPointCollectCompleted = (player) => {
                    /// Thực thi sự kiện khi mở cơ quan
                    onOpen?.Invoke(trigger, player, triggerInfo);
                };
    
                /// Nếu có thông tin Task
                if (task != null)
                {
                    /// Nếu chưa tồn tại
                    if (!this.currentTaskObjects.ContainsKey(task.ID))
                    {
                        /// Tạo mới
                        this.currentTaskObjects[task.ID] = new List<object>();
                    }
                    /// Thêm vào danh sách các đối tượng được tạo ra
                    this.currentTaskObjects[task.ID].Add(trigger);
                }
            }

            /// Nếu tổng số cơ quan cần sinh ra lớn hơn số vị trí cơ quan được thiết lập
            if (data.Triggers.Count > data.Positions.Count)
            {
                /// Chọn ngẫu nhiên đè nhau
                for (int i = 0; i < data.Triggers.Count; i++)
                {
                    /// Vị trí ngẫu nhiên
                    int randomPos = KTGlobal.GetRandomNumber(0, data.Positions.Count - 1);
                    /// Tạo cơ quan tại vị trí tương ứng
                    DoCreate(data.Triggers[i], data.Positions[randomPos].x, data.Positions[randomPos].y);
                }
            }
            /// Nếu tổng số cơ quan cần sinh ra bằng số vị trí cơ quan được thiết lập
            else if (data.Triggers.Count == data.Positions.Count)
            {
                /// Tạo mới đối tượng Random
                Random rand = new Random();
                /// Trộn vị trí
                List<MilitaryCamp.EventInfo.StageInfo.TaskInfo.IndexTriggerInfo.OrderedTriggerInfo> triggers = data.Triggers.OrderBy(x => rand.Next()).ToList();
                /// Duyệt danh sách cơ quan
                for (int i = 0; i < triggers.Count; i++)
                {
                    /// Tạo cơ quan tại vị trí tương ứng
                    DoCreate(triggers[i], data.Positions[i].x, data.Positions[i].y);
                }
            }
            /// Nếu tổng số cơ quan cần sinh ra nhỏ hơn số vị trí cơ quan được thiết lập
            else
            {
                /// Danh sách vị trí ngẫu nhiên
                List<UnityEngine.Vector2Int> randomPositions = data.Positions.RandomRange(data.Triggers.Count).ToList();
                /// Duyệt danh sách cơ quan
                for (int i = 0; i < data.Triggers.Count; i++)
                {
                    /// Tạo cơ quan tại vị trí tương ứng
                    DoCreate(data.Triggers[i], randomPositions[i].x, randomPositions[i].y);
                }
            }
        }

        /// <summary>
        /// Tạo thánh hỏa
        /// </summary>
        /// <param name="data"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="onExplode"></param>
        private void CreateHolyFire(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.ProtectTriggerInfo.HolyFireInfo data, int posX, int posY, Action onExplode)
        {
            /// Tạo thánh hỏa
            GrowPoint holyFire = KTGrowPointManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, GrowPointXML.Parse(data.ID, data.Name, -1, -1, data.CollectTick, true), posX, posY);
            holyFire.GrowPointCollectCompleted = (player) => {
                /// Hủy thánh hỏa
                this.RemoveGrowPoint(holyFire);
            };
            /// Thiết lập thời gian đếm lùi phát nổ
            this.SetTimeout(data.ActivateTicks, () => {
                /// Nếu thánh hỏa không còn tồn tại
                if (!holyFire.Alive)
                {
                    /// Bỏ qua
                    return;
                }
                /// Thực hiện phát nổ
                onExplode?.Invoke();
            });

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(holyFire);
            }
        }

        /// <summary>
        /// Xóa toàn bộ thánh hỏa
        /// <param name="data"></param>
        /// </summary>
        private void RemoveAllHolyFires(MilitaryCamp.EventInfo.StageInfo.TaskInfo.ProtectTriggerInfo.HolyFireInfo data)
        {
            /// Duyệt danh sách điểm thu thập
            foreach (GrowPoint growPoint in this.GetGrowPoints())
            {
                /// Nếu đây là thánh hỏa
                if (growPoint.Data.ResID == data.ID && growPoint.Data.Name == data.Name)
                {
                    /// Hủy thánh hỏa
                    this.RemoveGrowPoint(growPoint);
                }
            }
        }

        /// <summary>
        /// Tạo quái tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void CreateMonster(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.MonsterInfo data, Action<Monster> onCreated = null, Action<GameObject> onDieCallback = null)
        {
            /// Nếu không có dữ liệu
            if (data == null)
            {
                return;
            }

            /// Mức máu
            int hp = data.BaseHP + data.HPIncreaseEachLevel * this.CopyScene.Level;

            /// Tạo quái
            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = data.ID,
                PosX = data.PosX,
                PosY = data.PosY,
                Name = data.Name,
                Title = data.Title,
                MaxHP = hp,
                Level = this.CopyScene.Level,
                MonsterType = data.AIType,
                AIID = data.AIScriptID,
                Tag = "Monster",
                Camp = 65535,
                RespawnTick = data.RespawnTicks,
            });

            /// Nếu có kỹ năng
            if (data.Skills.Count > 0)
            {
                /// Duyệt danh sách kỹ năng
                foreach (SkillLevelRef skill in data.Skills)
                {
                    /// Thêm vào danh sách kỹ năng dùng của quái
                    monster.CustomAISkills.Add(skill);
                }
            }

            /// Nếu có vòng sáng
            if (data.Auras.Count > 0)
            {
                /// Duyệt danh sách vòng sáng
                foreach (SkillLevelRef aura in data.Auras)
                {
                    /// Kích hoạt vòng sáng
                    monster.UseSkill(aura.SkillID, aura.Level, monster);
                }
            }

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(monster);
            }

            /// Thực thi sự kiện tạo quái
            onCreated?.Invoke(monster);

            monster.OnDieCallback = onDieCallback;
        }

        /// <summary>
        /// Tạo Boss tương ứng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onDie"></param>
        private void CreateBoss(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.BossInfo data, Action<Monster> onCreated = null, Action<GameObject> onDie = null)
        {
            /// Nếu không có dữ liệu
            if (data == null)
            {
                return;
            }

            /// Mức máu
            int hp = data.BaseHP + data.HPIncreaseEachLevel * this.CopyScene.Level;
            /// Tạo Boss
            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = data.ID,
                PosX = data.PosX,
                PosY = data.PosY,
                Name = data.Name,
                Title = data.Title,
                MaxHP = hp,
                Level = this.CopyScene.Level,
                MonsterType = data.AIType,
                AIID = data.AIScriptID,
                Tag = "Boss",
                Camp = 65535,
            });

            /// Nếu có kỹ năng
            if (data.Skills.Count > 0)
            {
                /// Duyệt danh sách kỹ năng
                foreach (SkillLevelRef skill in data.Skills)
                {
                    /// Thêm vào danh sách kỹ năng dùng của quái
                    monster.CustomAISkills.Add(skill);
                }
            }

            /// Nếu có vòng sáng
            if (data.Auras.Count > 0)
            {
                /// Duyệt danh sách vòng sáng
                foreach (SkillLevelRef aura in data.Auras)
                {
                    /// Kích hoạt vòng sáng
                    monster.UseSkill(aura.SkillID, aura.Level, monster);
                }
            }

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(monster);
            }

            /// Thực thi sự kiện tạo Boss
            onCreated?.Invoke(monster);

            monster.OnDieCallback = onDie;
        }

        /// <summary>
        /// Tạo cổng dịch chuyển tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void CreateTeleport(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TeleportInfo data)
        {
            /// Tạo cổng dịch chuyển
            KDynamicArea teleport = KTDynamicAreaManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, data.Name, 10102, data.PosX, data.PosY, -1, 2000, 100, -1, null);
            teleport.OnEnter = (obj) => {
                if (obj is KPlayer player)
                {
                    /// Thực hiện chuyển vị trí
					KTPlayerManager.ChangePos(player, data.ToPosX, data.ToPosY);
                }
            };
        }

        /// <summary>
        /// Tạo NPC động tương ứng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void CreateDynamicNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.NPCInfo data, Action<NPC> callback)
        {
            /// Tạo NPC
            NPC npc = KTNPCManager.Create(new KTNPCManager.NPCBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = data.ID,
                PosX = data.PosX,
                PosY = data.PosY,
                Name = data.Name,
                Title = data.Title,
            });
            /// Thực thi sự kiện
            callback?.Invoke(npc);

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(npc);
            }
        }

        /// <summary>
        /// Tạo NPC di chuyển tương ứng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void CreateMovingNPC(MilitaryCamp.EventInfo.StageInfo.TaskInfo task, MilitaryCamp.EventInfo.StageInfo.TaskInfo.MovingNPCInfo data, Action<Monster> callback)
        {
            /// Tạo NPC di động
            Monster npc = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = data.ID,
                PosX = data.PosX,
                PosY = data.PosY,
                Name = data.Name,
                Title = data.Title,
                MaxHP = 100,
                Level = this.CopyScene.Level,
                MonsterType = MonsterAIType.DynamicNPC,
                Tag = "MilitaryCamp",
            });

            /// Miễn dịch toàn bộ
            npc.m_CurrentStatusImmunity = true;
            npc.m_CurrentInvincibility = 1;
            /// Sử dụng thuật toán A* tìm đường
            npc.UseAStarPathFinder = true;
            /// Thực hiện di chuyển đến vị trí tương ứng
            npc.MoveTo(new System.Windows.Point(data.ToPosX, data.ToPosY), true);

            /// Thực thi sự kiện
            callback?.Invoke(npc);

            /// Nếu có thông tin Task
            if (task != null)
            {
                /// Nếu chưa tồn tại
                if (!this.currentTaskObjects.ContainsKey(task.ID))
                {
                    /// Tạo mới
                    this.currentTaskObjects[task.ID] = new List<object>();
                }
                /// Thêm vào danh sách các đối tượng được tạo ra
                this.currentTaskObjects[task.ID].Add(npc);
            }
        }

        /// <summary>
        /// Tạo NPC tĩnh tương ứng điều khiển bởi Script Lua
        /// </summary>
        /// <param name="data"></param>
        private void CreateStaticNPC(MilitaryCamp.EventInfo.NPCInfo data)
        {
            /// Tạo NPC
            KTNPCManager.Create(new KTNPCManager.NPCBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = data.ID,
                PosX = data.PosX,
                PosY = data.PosY,
                Name = data.Name,
                Title = data.Title,
                ScriptID = data.ScriptID,
            });
        }
        #endregion
    }
}
