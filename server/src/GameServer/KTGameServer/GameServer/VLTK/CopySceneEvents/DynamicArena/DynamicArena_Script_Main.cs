using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.DynamicArena
{
    /// <summary>
    /// Script phụ bản lôi đài
    /// </summary>
    public class DynamicArena_Script_Main : CopySceneEvent
    {
        #region Constants
        /// <summary>
        /// Thời điểm cập nhật thông báo sự kiện
        /// </summary>
        private const long NotifyEventBroadboardTicks = 5000;

        /// <summary>
        /// Thời gian chuẩn bị
        /// </summary>
        private const int PrepareTicks = 20000;

        /// <summary>
        /// Thời gian kết thúc
        /// </summary>
        private const int FinishTicks = 10000;

        /// <summary>
        /// Thời gian thực thi kết quả trận đấu
        /// </summary>
        private const int ProcessBattleResultAfterTicks = 5000;

        /// <summary>
        /// Bước phụ bản
        /// </summary>
        private enum EventStep
        {
            /// <summary>
            /// Không làm gì
            /// </summary>
            None,
            /// <summary>
            /// Chuẩn bị
            /// </summary>
            Prepare,
            /// <summary>
            /// Chiến đấu
            /// </summary>
            Fight,
            /// <summary>
            /// Xử lý kết quả trận đấu
            /// </summary>
            ProcessingBattleResult,
            /// <summary>
            /// Kết thúc
            /// </summary>
            Finish,
        }
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện có kết quả thắng cuộc lôi đài
        /// <para>Nếu hòa thì truyền vào tham biến sẽ là NULL</para>
        /// </summary>
        public Action<KPlayer> ProcessBattleResult { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Bước của phụ bản
        /// </summary>
        private EventStep step;

        /// <summary>
        /// Thời điểm lần trước cập nhật thông báo sự kiện
        /// </summary>
        private long lastNotifyEventBroadboardTicks = 0;

        /// <summary>
        /// Số sát thương của các đội
        /// </summary>
        private readonly ConcurrentDictionary<int, long> TotalDamages = new ConcurrentDictionary<int, long>();

        /// <summary>
        /// Danh sách thành viên đội
        /// </summary>
        private readonly Dictionary<int, ConcurrentDictionary<int, KPlayer>> TeamPlayers = new Dictionary<int, ConcurrentDictionary<int, KPlayer>>();

        /// <summary>
        /// Thời điểm phân định thắng thua của trận đấu
        /// </summary>
        private long battleFinishedTicks = 0;

        /// <summary>
        /// Thời điểm bắt đầu xử lý kết quả trận đấu
        /// </summary>
        private long battleProcessingTicks = 0;

        /// <summary>
        /// Người chơi trong nhóm thắng cuộc
        /// </summary>
        private KPlayer winnerPlayer = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Script phụ bản lôi đài
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="firstTeam"></param>
        /// <param name="secondTeam"></param>
        public DynamicArena_Script_Main(KTCopyScene copyScene, List<KPlayer> firstTeam, List<KPlayer> secondTeam) : base(copyScene)
        {
            /// Đánh dấu bước chuẩn bị
            this.step = EventStep.Prepare;
            /// Thêm nhóm tương ứng
            this.TeamPlayers[1] = new ConcurrentDictionary<int, KPlayer>();
            /// Duyệt danh sách người chơi đội 1
            foreach (KPlayer player in firstTeam)
            {
                /// Thêm vào danh sách
                this.TeamPlayers[1][player.RoleID] = player;
            }
            this.TotalDamages[1] = 0;
            this.TeamPlayers[2] = new ConcurrentDictionary<int, KPlayer>();
            /// Duyệt danh sách người chơi đội 2
            foreach (KPlayer player in secondTeam)
            {
                /// Thêm vào danh sách
                this.TeamPlayers[2][player.RoleID] = player;
            }
            this.TotalDamages[2] = 0;
        }
        #endregion

        #region Core CopySceneEvent
        /// <summary>
        /// Hàm này gọi khi bắt đầu phụ bản
        /// </summary>
        protected override void OnStart()
        {
            /// Chuyển sang bước chuẩn bị
            this.step = EventStep.Prepare;
        }

        /// <summary>
        /// Gọi thêm sự kiện khi ngời chơi disconnect
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerDisconnected(KPlayer player)
        {

            LogManager.WriteLog(LogTypes.DynamicArena,"Disconnect call from PlayerLogout : " +  player.RoleName);
           /// Gọi phương thức disconnect
           base.OnPlayerDisconnected(player);

            /// ID nhóm
            int teamID = -1;
            /// Xem thằng này cũ ở nhóm nào rồi xóa
            List<int> keys = this.TeamPlayers.Keys.ToList();

            foreach (int key in keys)
            {
                /// Nếu tồn tại
                if (this.TeamPlayers.TryGetValue(key, out ConcurrentDictionary<int, KPlayer> playerList))
                {
                    /// Nếu không tồn tại thằng này
                    if (!playerList.ContainsKey(player.RoleID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Đánh dấu ID nhóm
                    teamID = key;
                    /// Xóa thằng này khỏi nhóm tương ứng
                    playerList.TryRemove(player.RoleID, out _);

                    /// Thoát lặp
                    break;
                }
            }

            /// Nếu có nhóm
            if (teamID != -1)
            {
                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) left the battle.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
            /// Nếu không có nhóm
            else
            {
                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{2}]: Player {0} (ID: {1}) left the battle, but Team was not presented in the list.", player.RoleName, player.RoleID, this.CopyScene.Name));
            }
        }
        /// <summary>
        /// Hàm này gọi tiên tục trong suốt thời gian tồn tại phụ bản
        /// </summary>
        protected override void OnTick()
        {
            /// Cập nhật thông tin sự kiện
            this.UpdateEventBroadboard(false);

            /// Nếu là bước chuẩn bị
            if (this.step == EventStep.Prepare)
            {
                /// Nếu đã hết thời gian chuẩn bị
                if (this.LifeTimeTicks >= DynamicArena_Script_Main.PrepareTicks)
                {
                    /// Chuyển sang bước chiến đấu
                    this.step = EventStep.Fight;
                    /// Chuyển Camp
                    this.FillPlayersCamp();
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventBroadboard(true);
                }
            }
            /// Nếu là bước chiến đấu
            else if (this.step == EventStep.Fight)
            {
                /// Nếu đã có kết quả
                if (this.IsFinished(out KPlayer winnerTeamPlayer))
                {
                    /// Chuyển sang bước phát thưởng
                    this.step = EventStep.ProcessingBattleResult;
                    /// Đánh dấu thời điểm bắt đầu xử lý kết quả trận đấu
                    this.battleProcessingTicks = KTGlobal.GetCurrentTimeMilis();
                    /// Hủy Camp
                    this.RemovePlayersCamp();
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventBroadboard(true);
                    /// Đánh dấu người chơi trong nhóm thắng cuộc
                    this.winnerPlayer = winnerTeamPlayer;
                }
            }
            /// Nếu là bước xử lý kết quả trận đấu
            else if (this.step == EventStep.ProcessingBattleResult)
            {
                /// Nếu đã đến thời gian
                if (KTGlobal.GetCurrentTimeMilis() - this.battleProcessingTicks >= DynamicArena_Script_Main.ProcessBattleResultAfterTicks)
                {
                    try
                    {
                        /// Thực thi sự kiện có kết quả thắng cuộc
                        this.ProcessBattleResult?.Invoke(this.winnerPlayer);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }

                    /// Chuyển sang bước kết thúc
                    this.step = EventStep.Finish;
                    /// Đánh dấu thời điểm kết thúc
                    this.battleFinishedTicks = KTGlobal.GetCurrentTimeMilis();
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventBroadboard(true);
                }
            }
            /// Nếu là bước kết thúc
            else if (this.step == EventStep.Finish)
            {
                /// Nếu đã hết thời gian đếm lùi
                if (KTGlobal.GetCurrentTimeMilis() - this.battleFinishedTicks >= DynamicArena_Script_Main.FinishTicks)
                {
                    /// Trục xuất toàn bộ người chơi
                    this.KickOutAllPlayers();
                    /// Hủy phụ bản
                    this.Dispose();
                    /// Chuyển qua bước không làm gì
                    this.step = EventStep.None;
                }
            }
        }

        /// <summary>
        /// Hàm này gọi khi phụ bản bị đóng
        /// </summary>
        protected override void OnClose()
        {
        }

        /// <summary>
		/// Sự kiện khi người chơi vào phụ bản
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnter(KPlayer player)
        {
            /// Gọi phương thức cha
            base.OnPlayerEnter(player);

            /// Mở bảng thông báo hoạt động
            this.OpenEventBroadboard(player, (int) GameEvent.DynamicArena);

            /// ID nhóm
            int teamID = -1;
            /// Nếu thuộc nhóm 1
            if (this.TeamPlayers[1].ContainsKey(player.RoleID))
            {
                teamID = 1;
            }
            /// Nếu thuộc nhóm 2
            else if (this.TeamPlayers[2].ContainsKey(player.RoleID))
            {
                teamID = 2;
            }
            /// Nếu không thuộc nhóm nào
            else
            {
                /// Trục xuất
                this.KickOutPlayer(player);
            }

            /// Nếu đang là bước chuẩn bị
            if (this.step == EventStep.Prepare)
            {
                /// Chuyển trạng thái PK hòa bình
                player.PKMode = (int) PKMode.Peace;
                /// Chuyển Camp về -1
                player.Camp = -1;
                /// Cấm chiêu
                player.ForbidUsingSkill = true;
                /// Thiết lập danh hiệu tạm
                player.TempTitle = string.Format("Chiến đội {0}", teamID);
                /// Cập nhật thông tin sự kiện
                this.UpdateEventDetails(player, this.CopyScene.Name, DynamicArena_Script_Main.PrepareTicks - this.LifeTimeTicks, "Chuẩn bị thi đấu");

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) entered the battle.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
            /// Nếu đang là bước chiến đấu
            else if (this.step == EventStep.Fight)
            {
                /// Chuyển trạng thái PK tùy chọn
                player.PKMode = (int) PKMode.Custom;
                /// Chuyển Camp về tương ứng theo nhóm
                player.Camp = teamID;
                /// Hủy cấm chiêu
                player.ForbidUsingSkill = false;
                /// Thiết lập danh hiệu tạm
                player.TempTitle = string.Format("Chiến đội {0}", teamID);
                /// Cập nhật thông tin sự kiện
                this.UpdateEventDetails(player, this.CopyScene.Name, this.DurationTicks - DynamicArena_Script_Main.PrepareTicks - DynamicArena_Script_Main.FinishTicks - this.LifeTimeTicks, new string[]
                {
                    string.Format("Tổng sát thương <color=yellow>chiến đội 1</color>: <color=green>{0}</color>", this.TotalDamages[1]),
                    string.Format("Tổng sát thương <color=yellow>chiến đội 2</color>: <color=green>{0}</color>", this.TotalDamages[2]),
                });

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) entered the battle on fighting phase.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
            /// Nếu đang là bước xử lý kết quả trận đấu
            else if (this.step == EventStep.ProcessingBattleResult)
            {
                /// Chuyển trạng thái PK hòa bình
                player.PKMode = (int) PKMode.Peace;
                /// Chuyển Camp về -1
                player.Camp = -1;
                /// Hủy cấm chiêu
                player.ForbidUsingSkill = false;
                /// Thiết lập danh hiệu tạm
                player.TempTitle = string.Format("Chiến đội {0}", teamID);
                /// Cập nhật thông tin sự kiện
                this.UpdateEventDetails(player, this.CopyScene.Name, DynamicArena_Script_Main.ProcessBattleResultAfterTicks - (KTGlobal.GetCurrentTimeMilis() - this.battleProcessingTicks), "Đang xử lý kết quả trận đấu");

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) entered the battle on processing battle result phase.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
            /// Nếu đang là bước kết thúc
            else if (this.step == EventStep.Finish)
            {
                /// Chuyển trạng thái PK hòa bình
                player.PKMode = (int) PKMode.Peace;
                /// Chuyển Camp về -1
                player.Camp = -1;
                /// Hủy cấm chiêu
                player.ForbidUsingSkill = false;
                /// Thiết lập danh hiệu tạm
                player.TempTitle = string.Format("Chiến đội {0}", teamID);
                /// Cập nhật thông tin sự kiện
                this.UpdateEventDetails(player, this.CopyScene.Name, DynamicArena_Script_Main.FinishTicks - (KTGlobal.GetCurrentTimeMilis() - this.battleFinishedTicks), "Kết thúc chiến đấu");

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) entered the battle on finishing phase.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi rời phụ bản
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            /// Gọi phương thức cha
            base.OnPlayerLeave(player, toMap);

            /// Đóng bảng thông báo hoạt động
            this.CloseEventBroadboard(player, (int) GameEvent.DynamicArena);
            /// Chuyển trạng thái PK hòa bình
            player.PKMode = (int) PKMode.Peace;
            /// Chuyển Camp về -1
            player.Camp = -1;
            /// Hủy cấm chiêu
            player.ForbidUsingSkill = false;
            /// Hủy danh hiệu tạm
            player.TempTitle = "";

            /// ID nhóm
            int teamID = -1;
            /// Xem thằng này cũ ở nhóm nào rồi xóa
            List<int> keys = this.TeamPlayers.Keys.ToList();
            foreach (int key in keys)
            {
                /// Nếu tồn tại
                if (this.TeamPlayers.TryGetValue(key, out ConcurrentDictionary<int, KPlayer> playerList))
                {
                    /// Nếu không tồn tại thằng này
                    if (!playerList.ContainsKey(player.RoleID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Đánh dấu ID nhóm
                    teamID = key;
                    /// Xóa thằng này khỏi nhóm tương ứng
                    playerList.TryRemove(player.RoleID, out _);

                    /// Thoát lặp
                    break;
                }
            }

            /// Nếu có nhóm
            if (teamID != -1)
            {
                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{3}]: TeamID: {0}, Player {1} (ID: {2}) left the battle.", teamID, player.RoleName, player.RoleID, this.CopyScene.Name));
            }
            /// Nếu không có nhóm
            else
            {
                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{2}]: Player {0} (ID: {1}) left the battle, but Team was not presented in the list.", player.RoleName, player.RoleID, this.CopyScene.Name));
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi gây sát thương cho người chơi khác
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="damage"></param>
        public override void OnHitTarget(KPlayer player, GameObject obj, int damage)
        {
            /// Gọi phương thức cha
            base.OnHitTarget(player, obj, damage);

            /// Nếu đối phương không phải người chơi
            if (!(obj is KPlayer))
            {
                return;
            }

            /// Nếu ở chiến đội 1
            if (this.TeamPlayers[1].ContainsKey(player.RoleID))
            {
                /// Cộng dồn sát thương cho nhóm của người chơi gây sát thương
                this.TotalDamages[1] += damage;
            }
            /// Nếu ở chiến đội 2
            else if (this.TeamPlayers[2].ContainsKey(player.RoleID))
            {
                /// Cộng dồn sát thương cho nhóm của người chơi gây sát thương
                this.TotalDamages[2] += damage;
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi bị chết
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        public override void OnPlayerDie(GameObject killer, KPlayer player)
        {
            /// Gọi phương thức cha
            base.OnPlayerDie(killer, player);

            /// Nếu không có bản đồ ra
            if (this.CopyScene.OutMapCode == -1 || this.CopyScene.OutPosX == -1 || this.CopyScene.OutPosY == -1)
            {
                /// Lấy thông tin bản đồ trước đó
                KT_TCPHandler.GetLastMapInfo(player, out int mapCode, out int posX, out int posY);
                /// Cho sống lại ở bản đồ đó
                KTPlayerManager.Relive(player, mapCode, posX, posY, this.CopyScene.ReliveHPPercent, this.CopyScene.ReliveMPPercent, this.CopyScene.ReliveStaminaPercent);
            }
            /// Nếu có bản đồ ra
            else
            {
                /// Cho sống lại ở bản đồ ra
                KTPlayerManager.Relive(player, this.CopyScene.OutMapCode, this.CopyScene.OutPosX, this.CopyScene.OutPosY, this.CopyScene.ReliveHPPercent, this.CopyScene.ReliveMPPercent, this.CopyScene.ReliveStaminaPercent);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Trục xuất toàn bộ người chơi ra khỏi phụ bản
        /// </summary>
        private void KickOutAllPlayers()
        {
            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Trục xuất
                this.KickOutPlayer(player);
            }
        }

        /// <summary>
        /// Trục xuất người chơi khỏi phụ bản
        /// </summary>
        /// <param name="player"></param>
        private void KickOutPlayer(KPlayer player)
        {
            KTPlayerManager.ChangeMap(player, this.CopyScene.OutMapCode, this.CopyScene.OutPosX, this.CopyScene.OutPosY);
        }

        /// <summary>
        /// Trả về ID chiến đội thắng cuộc theo ID nhóm
        /// </summary>
        /// <returns></returns>
        private int GetWinnerTeamByTotalDamages()
        {
            if (this.TotalDamages[1] > this.TotalDamages[2])
            {
                return 1;
            }
            else if (this.TotalDamages[1] < this.TotalDamages[2])
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Trận đấu đã kết thúc chưa
        /// </summary>
        /// <param name="winnerTeamPlayer"></param>
        /// <returns></returns>
        private bool IsFinished(out KPlayer winnerTeamPlayer)
        {
            /// Đánh dấu mặc định kết quả
            winnerTeamPlayer = null;

            /// Nếu 1 trong 2 nhóm đã hết người chơi
            if (this.TeamPlayers[1].Count <= 0 || this.TeamPlayers[2].Count <= 0)
            {
                /// Nếu nhóm 1 còn người chơi
                if (this.TeamPlayers[1].Count > 0)
                {
                    /// Lấy thằng đầu tiên trong nhóm này
                    winnerTeamPlayer = this.TeamPlayers[1].FirstOrDefault().Value;
                }
                /// Nếu nhóm 2 còn người chơi
                else if (this.TeamPlayers[2].Count > 0)
                {
                    /// Lấy thằng đầu tiên trong nhóm này
                    winnerTeamPlayer = this.TeamPlayers[2].FirstOrDefault().Value;
                }
                /// Nếu cả 2 nhóm đều hết người chơi
                else
                {
                    /// ID nhóm thắng cuộc với sát thương gây ra lớn hơn
                    int winnerTeamID = this.GetWinnerTeamByTotalDamages();
                    /// Nếu tồn tại
                    if (winnerTeamID != -1)
                    {
                        /// Lấy thằng đầu tiên trong nhóm này
                        winnerTeamPlayer = this.TeamPlayers[winnerTeamID].FirstOrDefault().Value;
                    }
                }

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{5}]: Battle has finished. Team 1 total damages = {0}, players left = {1} - Team 2 total damages = {2}, players left = {3}. Winner player team = {4}", this.TotalDamages[1], this.TeamPlayers[1].Count, this.TotalDamages[2], this.TeamPlayers[2].Count, winnerTeamPlayer == null ? "None won" : winnerTeamPlayer.RoleName, this.CopyScene.Name));

                /// Trận đấu đã kết thúc
                return true;
            }

            /// Nếu đã hết thời gian chiến đấu
            if (this.LifeTimeTicks >= this.DurationTicks - DynamicArena_Script_Main.PrepareTicks)
            {
                /// ID nhóm thắng cuộc với sát thương gây ra lớn hơn
                int winnerTeamID = this.GetWinnerTeamByTotalDamages();
                /// Nếu tồn tại
                if (winnerTeamID != -1)
                {
                    /// Lấy thằng đầu tiên trong nhóm này
                    winnerTeamPlayer = this.TeamPlayers[winnerTeamID].FirstOrDefault().Value;
                }

                /// Ghi log
                LogManager.WriteLog(LogTypes.DynamicArena, string.Format("[{5}]: Battle has finished. Team 1 total damages = {0}, players left = {1} - Team 2 total damages = {2}, players left = {3}. Winner player team = {4}", this.TotalDamages[1], this.TeamPlayers[1].Count, this.TotalDamages[2], this.TeamPlayers[2].Count, winnerTeamPlayer == null ? "None won" : winnerTeamPlayer.RoleName, this.CopyScene.Name));

                /// Trận đấu đã kết thúc
                return true;
            }

            /// Trận đấu chưa kết thúc
            return false;
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Cập nhật bảng thông báo sự kiện
        /// </summary>
        /// <param name="forceUpdate"></param>
        private void UpdateEventBroadboard(bool forceUpdate)
        {
            /// Nếu không bắt buộc cập nhật
            if (!forceUpdate)
            {
                /// Nếu chưa đến thời gian
                if (KTGlobal.GetCurrentTimeMilis() - this.lastNotifyEventBroadboardTicks < DynamicArena_Script_Main.NotifyEventBroadboardTicks)
                {
                    /// Bỏ qua
                    return;
                }
            }
            /// Cập nhật thời điểm thông báo
            this.lastNotifyEventBroadboardTicks = KTGlobal.GetCurrentTimeMilis();

            /// Nếu đang là bước chuẩn bị
            if (this.step == EventStep.Prepare)
            {
                this.UpdateEventDetailsToPlayers(this.CopyScene.Name, DynamicArena_Script_Main.PrepareTicks - this.LifeTimeTicks, "Chuẩn bị chiến đấu");
            }
            /// Nếu đang là bước chiến đấu
            else if (this.step == EventStep.Fight)
            {
                this.UpdateEventDetailsToPlayers(this.CopyScene.Name, this.DurationTicks - DynamicArena_Script_Main.PrepareTicks - DynamicArena_Script_Main.FinishTicks - this.LifeTimeTicks, new string[]
                {
                    string.Format("Tổng sát thương <color=yellow>chiến đội 1</color>: <color=green>{0}</color>", this.TotalDamages[1]),
                    string.Format("Tổng sát thương <color=yellow>chiến đội 2</color>: <color=green>{0}</color>", this.TotalDamages[2]),
                });
            }
            /// Nếu đang là bước xử lý kết quả trận đấu
            else if (this.step == EventStep.ProcessingBattleResult)
            {
                this.UpdateEventDetailsToPlayers(this.CopyScene.Name, DynamicArena_Script_Main.ProcessBattleResultAfterTicks - (KTGlobal.GetCurrentTimeMilis() - this.battleProcessingTicks), "Đang xử lý kết quả trận đấu");
            }
            /// Nếu đang là bước kết thúc
            else if (this.step == EventStep.Finish)
            {
                this.UpdateEventDetailsToPlayers(this.CopyScene.Name, DynamicArena_Script_Main.FinishTicks - (KTGlobal.GetCurrentTimeMilis() - this.battleFinishedTicks), "Kết thúc chiến đấu");
            }
        }

        /// <summary>
        /// Thiết lập Camp cho toàn bộ người chơi theo nhóm tương ứng
        /// </summary>
        private void FillPlayersCamp()
        {
            /// Danh sách nhóm theo khóa
            List<int> keys = this.TeamPlayers.Keys.ToList();
            /// Duyệt danh sách nhóm
            foreach (int key in keys)
            {
                /// Duyệt danh sách người chơi trong nhóm
                foreach (int playerID in this.TeamPlayers[key].Keys.ToList())
                {
                    /// Nếu không tồn tại
                    if (!this.TeamPlayers[key].TryGetValue(playerID, out KPlayer player))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Chuyển trạng thái PK tùy chọn
                    player.PKMode = (int) PKMode.Custom;
                    /// Chuyển Camp theo ID nhóm
                    player.Camp = key;
                    /// Hủy cấm chiêu
                    player.ForbidUsingSkill = false;
                }
            }
        }

        /// <summary>
        /// Xóa Camp cho toàn bộ người chơi theo nhóm tương ứng
        /// </summary>
        private void RemovePlayersCamp()
        {
            /// Danh sách nhóm theo khóa
            List<int> keys = this.TeamPlayers.Keys.ToList();
            /// Duyệt danh sách nhóm
            foreach (int key in keys)
            {
                /// Duyệt danh sách người chơi trong nhóm
                foreach (int playerID in this.TeamPlayers[key].Keys.ToList())
                {
                    /// Nếu không tồn tại
                    if (!this.TeamPlayers[key].TryGetValue(playerID, out KPlayer player))
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Chuyển trạng thái PK hòa bình
                    player.PKMode = (int) PKMode.Peace;
                    /// Chuyển Camp về -1
                    player.Camp = -1;
                    /// Hủy cấm chiêu
                    player.ForbidUsingSkill = false;
                }
            }
        }
        #endregion
    }
}
