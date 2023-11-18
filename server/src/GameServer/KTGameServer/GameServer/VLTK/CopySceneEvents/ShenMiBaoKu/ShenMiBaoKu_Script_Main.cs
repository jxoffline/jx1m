using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Script chính điều khiển phụ bản Thần Bí Bảo Khố
    /// </summary>
    public partial class ShenMiBaoKu_Script_Main : CopySceneEvent
    {
        #region Constants
        /// <summary>
        /// Thời gian mỗi lần thông báo thông tin sự kiện tới người chơi
        /// </summary>
        private const int NotifyActivityInfoToPlayersEveryTick = 5000;

        /// <summary>
        /// Thời gian tự trục xuất người chơi ra khỏi khi hoàn thành phụ bản
        /// </summary>
        private const int CompleteAutoKickOutTicks = 1200000;

        /// <summary>
        /// Thời gian chuẩn bị
        /// </summary>
        private const int PrepareTicks = 30000;
        #endregion

        #region Define
        /// <summary>
        /// Thời điểm thông báo cập nhật thông tin sự kiện tới tất cả người chơi lần trước
        /// </summary>
        private long LastNotifyTick;

        /// <summary>
        /// Thứ tự tầng hiện tại
        /// </summary>
        private int currentStageID = -1;

        /// <summary>
        /// Danh sách nhiệm vụ đã hoàn thành
        /// </summary>
        private readonly HashSet<int> completedTasks = new HashSet<int>();

        /// <summary>
        /// Danh sách nhiệm vụ đang làm
        /// </summary>
        private readonly HashSet<int> doingTasks = new HashSet<int>();

        /// <summary>
        /// Thời điểm hoàn thành phụ bản
        /// </summary>
        private long eventCompletedTicks = -1;

        /// <summary>
        /// Danh sách các đối tượng tạo ra ở nhiệm vụ hiện tại
        /// </summary>
        private readonly Dictionary<int, List<object>> currentTaskObjects = new Dictionary<int, List<object>>();

        /// <summary>
        /// Số Boss đã gọi ra bởi Câu Hồn Ngọc
        /// </summary>
        private int totalBossesSummoned = 0;
        #endregion

        #region Core CopySceneEvent
        /// <summary>
        /// Script chính điều khiển phụ bản Thần Bí Bảo Khố
        /// </summary>
        /// <param name="copyScene"></param>
        public ShenMiBaoKu_Script_Main(KTCopyScene copyScene) : base(copyScene)
        {
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
        }

        /// <summary>
        /// Hàm này gọi liên tục trong phụ bản
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu chưa hết thời gian chuẩn bị
            if (this.LifeTimeTicks < ShenMiBaoKu_Script_Main.PrepareTicks)
            {
                /// Nếu đã đến thời gian thông báo thông tin sự kiện
                if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= ShenMiBaoKu_Script_Main.NotifyActivityInfoToPlayersEveryTick)
                {
                    /// Đánh dấu thời gian thông báo thông tin sự kiện
                    this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventDetailsToPlayers(this.CopyScene.Name, ShenMiBaoKu_Script_Main.PrepareTicks - this.LifeTimeTicks, "Chuẩn bị");
                }
                /// Bỏ qua
                return;
            }

            /// Nếu chưa bắt đầu thì bắt đầu
            if (this.currentStageID == -1)
            {
                /// Bắt đầu từ tầng 1
                this.currentStageID = 0;
                /// Bắt đầu
                this.BeginStage();
                /// Đánh dấu thời gian thông báo thông tin sự kiện
                this.LastNotifyTick = 0;
            }

            /// Nếu đã đến thời gian thông báo thông tin sự kiện
            if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= ShenMiBaoKu_Script_Main.NotifyActivityInfoToPlayersEveryTick)
            {
                /// Đánh dấu thời gian thông báo thông tin sự kiện
                this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                /// Nếu đã hoàn thành
                if (this.eventCompletedTicks != -1)
                {
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventDetailsToPlayers(this.CopyScene.Name, ShenMiBaoKu_Script_Main.CompleteAutoKickOutTicks - (KTGlobal.GetCurrentTimeMilis() - this.eventCompletedTicks), "Đã hoàn thành", "Có thể sử dụng <color=yellow>[Câu Hồn Ngọc]</color> để gọi Boss");
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
                if (KTGlobal.GetCurrentTimeMilis() - this.eventCompletedTicks >= ShenMiBaoKu_Script_Main.CompleteAutoKickOutTicks)
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
                if (this.currentStageID >= ShenMiBaoKu.Data.Stages.Count)
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

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
            /// Nếu tồn tại
            if (gameMap != null)
            {
                gameMap.MyNodeGrid.ClearDynamicObsLabels(this.CopyScene.ID);
            }
        }

        /// <summary>
        /// Hàm này gọi đến khi người chơi vào phụ bản
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerEnter(KPlayer player)
        {
            base.OnPlayerEnter(player);
            /// Mở bảng thông báo hoạt động
            this.OpenEventBroadboard(player, (int) GameEvent.ShenMiBaoKu);
            /// Chuyển trạng thái PK hòa bình
            player.PKMode = (int) PKMode.Peace;
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
            this.CloseEventBroadboard(player, (int) GameEvent.ShenMiBaoKu);
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
            if (this.currentStageID < 0 || this.currentStageID >= ShenMiBaoKu.Data.Stages.Count)
            {
                /// Toác
                return new string[0];
            }

            /// Chuỗi thông tin tiến độ phụ bản
            List<string> detailTexts = new List<string>();

            /// Thông tin tầng
            ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task in stageInfo.Tasks)
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
                        /// Tiêu diệt Boss
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillBosses:
                        {
                            /// Danh sách Boss yêu cầu
                            List<string> requireKills = new List<string>();
                            /// Duyệt danh sách Boss
                            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.BossInfo boss in task.Bosses)
                            {
                                /// Thêm Boss vào danh sách
                                requireKills.Add(string.Format("<color=yellow>[{0}]</color>", boss.Name));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Tiêu diệt {0}", string.Join(", ", requireKills));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở cơ quan và tiêu diệt quái
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggersAndKillAllMonsters:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo trigger in task.Triggers)
                            {
                                requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", trigger.Name));
                            }
                                
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
                        /// Tiêu diệt toàn bộ quái
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillMonsters:
                        {
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
                            string desc = string.Format("- Tiêu diệt {0}", string.Join(", ", requireKillMonsters));
                            /// Thêm vào
                            builder.AppendLine(desc);
                            break;
                        }
                        /// Mở cơ quan
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggers:
                        {
                            /// Danh sách cơ quan cần mở
                            List<string> requireOpenTriggers = new List<string>();
                            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo trigger in task.Triggers)
                            {
                                requireOpenTriggers.Add(string.Format("<color=yellow>{0}</color>", trigger.Name));
                            }

                            /// Chuỗi thông tin
                            string desc = string.Format("- Mở {0}", string.Join(", ", requireOpenTriggers));
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
        /// Bắt đầu Stage
        /// </summary>
        private void BeginStage()
        {
            /// Thông tin tầng
            ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];

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
            if (this.currentStageID >= ShenMiBaoKu.Data.Stages.Count)
            {
                /// Đã hoàn thành toàn bộ
                return true;
            }
            /// Thông tin tầng
            ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];
            /// Nếu đã hoàn thành tất cả chuỗi nhiệm vụ của tầng thì hoàn thành tầng hiện tại
            return stageInfo.Tasks.Count <= this.completedTasks.Count;
        }

        /// <summary>
        /// Hoàn thành Stage
        /// </summary>
        private void CompleteStage()
        {
            /// Nếu đã vượt quá tổng số tầng
            if (this.currentStageID >= ShenMiBaoKu.Data.Stages.Count)
            {
                /// Bỏ qua
                return;
            }
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
            this.NotifyAllPlayers(string.Format("Hoàn thành {0}, sau {1} sẽ rời khỏi phụ bản!", "Thần Bí Bảo Khố", KTGlobal.DisplayTimeHourMinuteSecondOnly(ShenMiBaoKu_Script_Main.CompleteAutoKickOutTicks / 1000)));
        }

        /// <summary>
        /// Xử lý các nhiệm vụ của tầng
        /// </summary>
        private void HandleTasks()
        {
            /// Thông tin tầng
            ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task in stageInfo.Tasks)
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
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillMonsters:
                        {
                            isCompleted = this.Track_KillMonsters(task);
                            break;
                        }
                        /// Tiêu diệt Boss
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillBosses:
                        {
                            isCompleted = this.Track_KillBosses(task);
                            break;
                        }
                        /// Mở cơ quan và tiêu diệt quái
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggersAndKillAllMonsters:
                        {
                            isCompleted = this.Track_OpenTriggersAndKillMonsters(task);
                            break;
                        }
                        /// Mở cơ quan
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggers:
                        {
                            isCompleted = this.Track_OpenTriggers(task);
                            break;
                        }
                        /// Không làm gì
                        case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.None:
                        {
                            isCompleted = true;
                            break;
                        }
                    }

                    /// Nếu đã hoàn thành
                    if (isCompleted)
                    {
                        ///// Thông báo hoàn thành nhiệm vụ
                        //this.NotifyAllPlayers(string.Format("Hoàn thành nhiệm vụ: {0}", task.Name));

                        /// Thêm nhiệm vụ vào danh sách đã hoàn thành
                        this.completedTasks.Add(task.ID);

                        /// Xóa toàn bộ các đối tượng nhiệm vụ được tạo ra từ nhiệm vụ trước
                        this.RemoveAllPreviousTaskObjects(task.ID);
                        /// Tạo cổng dịch chuyển và mở các khối Obs động
                        this.CreateFinishTaskTeleportAndOpenDynamicObstacles(task);

                        /// Reset toàn bộ dữ liệu nhiệm vụ
                        this.Reset_KillMonsters(task);
                        this.Reset_KillBosses(task);
                        this.Reset_OpenTriggersAndKillMonsters(task);
                        this.Reset_OpenTriggers(task);
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
            ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];

            /// Duyệt danh sách nhiệm vụ
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task in stageInfo.Tasks)
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
                            /// Tiêu diệt quái
                            case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillMonsters:
                            {
                                this.Begin_KillMonsters(task);
                                break;
                            }
                            /// Tiêu diệt Boss
                            case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.KillBosses:
                            {
                                this.Begin_KillBosses(task);
                                break;
                            }
                            /// Mở cơ quan và tiêu diệt quái
                            case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggersAndKillAllMonsters:
                            {
                                this.Begin_OpenTriggersAndKillMonsters(task);
                                break;
                            }
                            /// Mở cơ quan
                            case ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TaskType.OpenTriggers:
                            {
                                this.Begin_OpenTriggers(task);
                                break;
                            }
                        }

                        /// Tạo cổng dịch chuyển và mở khối Obs khi bắt đầu nhiệm vụ
                        this.CreateOpenTaskTeleportAndOpenDynamicObstacles(task);

                        ///// Thông báo bắt đầu nhiệm vụ
                        //this.NotifyAllPlayers(string.Format("Bắt đầu nhiệm vụ: {0}", task.Name));
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
            KTPlayerManager.ChangeMap(player, this.CopyScene.OutMapCode, this.CopyScene.OutPosX, this.CopyScene.OutPosY);
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
        private string GetTaskKey(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Trả về kết quả
            return string.Format("{0}_{1}", this.currentStageID, task.ID);
        }

        /// <summary>
        /// Tạo cổng dịch chuyển và mở khối Obs động khi hoàn thành nhiệm vụ
        /// </summary>
        /// <param name="task"></param>
        private void CreateFinishTaskTeleportAndOpenDynamicObstacles(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách cổng dịch chuyển
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TeleportInfo teleportInfo in task.Teleports)
            {
                /// Nếu sinh không ra khi kết thúc nhiệm vụ
                if (teleportInfo.SpawnImmediate)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Tạo cổng dịch chuyển
                this.CreateTeleport(task, teleportInfo);
            }

            /// Nếu tồn tại Obs động
            if (task.DynamicObstacles != null)
            {
                /// Nếu không phải mở khi kết thúc nhiệm vụ
                if (task.DynamicObstacles.OpenImmediate)
                {
                    /// Bỏ qua
                    return;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
                /// Nếu tồn tại
                if (gameMap != null)
                {
                    /// Duyệt danh sách
                    foreach (byte layer in task.DynamicObstacles.Layers)
                    {
                        gameMap.MyNodeGrid.OpenDynamicObsLabel(this.CopyScene.ID, layer);
                    }
                }
            }
        }

        /// <summary>
        /// Tạo cổng dịch chuyển và mở khối Obs động khi bắt đầu nhiệm vụ
        /// </summary>
        /// <param name="task"></param>
        private void CreateOpenTaskTeleportAndOpenDynamicObstacles(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task)
        {
            /// Duyệt danh sách cổng dịch chuyển
            foreach (ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TeleportInfo teleportInfo in task.Teleports)
            {
                /// Nếu sinh không ra khi bắt đầu nhiệm vụ
                if (!teleportInfo.SpawnImmediate)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Tạo cổng dịch chuyển
                this.CreateTeleport(task, teleportInfo);
            }

            /// Nếu tồn tại Obs động
            if (task.DynamicObstacles != null)
            {
                /// Nếu không phải mở khi bắt đầu nhiệm vụ
                if (!task.DynamicObstacles.OpenImmediate)
                {
                    /// Bỏ qua
                    return;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.CopyScene.MapCode);
                /// Nếu tồn tại
                if (gameMap != null)
                {
                    /// Duyệt danh sách
                    foreach (byte layer in task.DynamicObstacles.Layers)
                    {
                        gameMap.MyNodeGrid.OpenDynamicObsLabel(this.CopyScene.ID, layer);
                    }
                }
            }
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
        /// Tạo cơ quan
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void CreateTrigger(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task, ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TriggerInfo data, Action<GrowPoint, KPlayer> onOpen)
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
        /// Tạo cổng dịch chuyển tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void CreateTeleport(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task, ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.TeleportInfo data)
        {
            /// Tạo cổng dịch chuyển
            KDynamicArea teleport = KTDynamicAreaManager.Add(this.CopyScene.MapCode, this.CopyScene.ID, data.Name, 10102, data.PosX, data.PosY, -1, 2000, 100, -1, null);
            teleport.OnEnter = (obj) => {
                /// Nếu là người chơi
                if (obj is KPlayer player)
                {
                    /// Nếu là cổng dịch chuyển ra khỏi phụ bản
                    if (data.ToPosX == -1 || data.ToPosY == -1)
                    {
                        /// Đưa về thành
                        this.KickOutPlayer(player);
                    }
                    /// Nếu là cổng dịch chuyển trong phụ bản
                    else
                    {
                        /// Thực hiện chuyển vị trí
                        KTPlayerManager.ChangePos(player, data.ToPosX, data.ToPosY);
                    }
                }
            };
        }

        /// <summary>
        /// Tạo quái tương ứng
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onCreated"></param>
        /// <param name="onDieCallback"></param>
        private void CreateMonster(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task, ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.MonsterInfo data, Action<Monster> onCreated = null, Action<GameObject> onDieCallback = null)
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
        /// <param name="onCreated"></param>
        /// <param name="onDie"></param>
        private void CreateBoss(ShenMiBaoKu.SMBKData.StageInfo.TaskInfo task, ShenMiBaoKu.SMBKData.StageInfo.TaskInfo.BossInfo data, Action<Monster> onCreated = null, Action<GameObject> onDie = null)
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
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra điều kiện dùng Câu Hồn Ngọc để tạo Boss
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public string UseCallBossItem_CheckCondition(KPlayer player)
        {
            /// Nếu chưa hoàn thành
            if (this.currentStageID < ShenMiBaoKu.Data.Stages.Count)
            {
                /// Thông tin tầng
                ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];
                /// Nếu tầng này không cho gọi Boss
                if (!stageInfo.AllowCallBoss)
                {
                    return "Hiện chưa thể sử dụng Câu Hồn Ngọc, hãy hoàn thành các nhiệm vụ đặt ra trước!";
                }
            }

            /// Nếu đã triệu hồi quá số lượng
            if (this.totalBossesSummoned >= ShenMiBaoKu.Data.Config.MaxCallBoss)
            {
                return string.Format("Chỉ có thể triệu hồi tối đa {0} Boss trong Thần Bí Bảo Khố!", ShenMiBaoKu.Data.Config.MaxCallBoss);
            }

            /// Trả về kết quả triệu hồi thành công
            return "OK";
        }

        /// <summary>
        /// Sử dụng Câu Hồn Ngọc tạo Boss
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bossID"></param>
        public void UseCallBossItem(KPlayer player, int bossID)
        {
            /// Nếu chưa hoàn thành
            if (this.currentStageID < ShenMiBaoKu.Data.Stages.Count)
            {
                /// Thông tin tầng
                ShenMiBaoKu.SMBKData.StageInfo stageInfo = ShenMiBaoKu.Data.Stages[this.currentStageID];
                /// Nếu tầng này không cho gọi Boss
                if (!stageInfo.AllowCallBoss)
                {
                    KTPlayerManager.ShowNotification(player, "Hiện chưa thể sử dụng Câu Hồn Ngọc, hãy hoàn thành các nhiệm vụ đặt ra trước!");
                    return;
                }
            }

            /// Nếu đã triệu hồi quá số lượng
            if (this.totalBossesSummoned >= ShenMiBaoKu.Data.Config.MaxCallBoss)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Chỉ có thể triệu hồi tối đa {0} Boss trong Thần Bí Bảo Khố!", ShenMiBaoKu.Data.Config.MaxCallBoss));
                return;
            }

            /// Tăng số lượng Boss đã triệu hồi
            this.totalBossesSummoned++;

            /// Tạo Boss tương ứng
            KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.CopyScene.MapCode,
                CopySceneID = this.CopyScene.ID,
                ResID = bossID,
                PosX = player.PosX,
                PosY = player.PosY,
                MonsterType = MonsterAIType.Boss,
                Tag = "SpecialBoss",
                Camp = 65535,
            });

            /// Thông báo triệu hồi thành công
            KTPlayerManager.ShowNotification(player, "Triệu hồi thành công!");
        }
        #endregion
    }
}
