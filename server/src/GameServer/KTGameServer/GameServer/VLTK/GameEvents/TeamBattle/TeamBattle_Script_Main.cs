using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.TeamBattle
{
    /// <summary>
    /// Script Võ lâm liên đấu (trận đấu)
    /// </summary>
    public class TeamBattle_Script_Main : CopySceneEvent
    {
        #region Constants
        /// <summary>
        /// Thời gian mỗi lần thông báo thông tin sự kiện tới người chơi
        /// </summary>
        private const int NotifyActivityInfoToPlayersEveryTick = 2000;

        /// <summary>
        /// Thời gian chuẩn bị
        /// </summary>
        private const int PrepareTicks = 60000;
        #endregion

        #region Private fields
        /// <summary>
        /// Bước của phụ bản
        /// </summary>
        private int step = 0;

        /// <summary>
        /// Thời điểm thông báo cập nhật thông tin sự kiện tới tất cả người chơi lần trước
        /// </summary>
        private long LastNotifyTick;

        /// <summary>
        /// Đánh dấu trận chiến đã kết thúc chưa
        /// </summary>
        private bool isBattleFinished = false;

        /// <summary>
        /// Vòng đấu nào
        /// </summary>
        private readonly int Stage = 0;

        /// <summary>
        /// Có tăng bậc của chiến đội cho đội thắng cuộc không
        /// </summary>
        private readonly bool IncreaseStageForWinnerTeam = false;

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
        /// Danh sách tên nhóm
        /// </summary>
        private readonly ConcurrentDictionary<int, string> TeamNames = new ConcurrentDictionary<int, string>();
        #endregion

        #region Constructor
        /// <summary>
        /// Script Võ lâm liên đấu (trận đấu)
        /// </summary>
        /// <param name="copyScene"></param>
        /// <param name="stage"></param>
        public TeamBattle_Script_Main(KTCopyScene copyScene, int stage, bool increaseStageForWinnerTeam) : base(copyScene)
        {
            this.Stage = stage;
            this.IncreaseStageForWinnerTeam = increaseStageForWinnerTeam;
        }
        #endregion

        #region Core CopySceneEvent
        /// <summary>
        /// Hàm  này gọi khi bắt đầu phụ bản
        /// </summary>
        protected override void OnStart()
        {
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi nửa giây chừng nào phụ bản còn tồn tại
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu là bước 0
            if (this.step == 0)
            {
                /// Đang trong thời gian chuẩn bị
                if (this.LifeTimeTicks <= TeamBattle_Script_Main.PrepareTicks)
                {
                    /// Nếu đã đến thời gian thông báo thông tin sự kiện
                    if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= TeamBattle_Script_Main.NotifyActivityInfoToPlayersEveryTick)
                    {
                        /// Đánh dấu thời gian thông báo thông tin sự kiện
                        this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();

                        /// Cập nhật thông tin chiến đội
                        this.TickPlayersTeamInfo();
                        /// Cập nhật thông tin sự kiện
                        this.UpdateEventDetailsToPlayers(this.CopyScene.Name, TeamBattle_Script_Main.PrepareTicks - this.LifeTimeTicks, "Chuẩn bị thi đấu");

                        /// Chuyển về PK hòa bình
                        this.ChangePKModeOfAllPlayersToPeace();
                        /// Cấm dùng kỹ năng
                        this.ForbidAllPlayersUseSkills(true);
                    }
                }
                /// Đã hết thời gian chuẩn bị
                else
                {
                    /// Danh sách nhóm theo Key
                    List<int> keys = this.TeamPlayers.Keys.ToList();
                    /// Duyệt danh sách nhóm
                    foreach (int key in keys)
                    {
                        /// Lấy dữ liệu
                        if (this.TeamPlayers.TryGetValue(key, out ConcurrentDictionary<int, KPlayer> playerList))
                        {
                            /// Chọn thành viên đầu tiên trong nhóm
                            KPlayer player = playerList.FirstOrDefault().Value;
                            /// Thực hiện phát thưởng khi bắt đầu trận
                            TeamBattle_ActivityScript.ProcessEnterBattle(player);
                        }
                    }

                    /// Chuyển qua bước 1
                    this.step = 1;
                    /// Cập nhật thông tin sự kiện
                    if (this.TeamNames.Count == 2)
                    {
                        this.UpdateEventDetailsToPlayers(this.CopyScene.Name, TeamBattle.Battle.Config.Duration - this.LifeTimeTicks, "Tổng sát thương:", string.Format("<color=#ff4242>{0}</color>: <color=green>{1}</color>", this.TeamNames.First().Value, this.TotalDamages.First().Value), string.Format("<color=#42d0ff>{0}</color>: <color=green>{1}</color>", this.TeamNames.Last().Value, this.TotalDamages.Last().Value));
                    }

                    /// Chuyển về PK đặc biệt
                    this.ChangePKModeOfAllPlayerToCustom();
                    /// Cấm dùng kỹ năng
                    this.ForbidAllPlayersUseSkills(false);

                    /// Ghi log
                    if (this.TeamNames.Count == 2)
                    {
                        LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Begin battle between team {0} and team {1}", this.TeamNames.First().Value, this.TeamNames.Last().Value));
                    }
                }
            }
            /// Trong thời gian thi đấu
            else if (this.step == 1)
            {
                /// Nếu đã đến thời gian thông báo thông tin sự kiện
                if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= TeamBattle_Script_Main.NotifyActivityInfoToPlayersEveryTick)
                {
                    /// Đánh dấu thời gian thông báo thông tin sự kiện
                    this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Cập nhật thông tin sự kiện
                    if (this.TeamNames.Count == 2)
                    {
                        this.UpdateEventDetailsToPlayers(this.CopyScene.Name, TeamBattle.Battle.Config.Duration - this.LifeTimeTicks, "Tổng sát thương:", string.Format("<color=#ff4242>{0}</color>: <color=green>{1}</color>", this.TeamNames.First().Value, this.TotalDamages.First().Value), string.Format("<color=#42d0ff>{0}</color>: <color=green>{1}</color>", this.TeamNames.Last().Value, this.TotalDamages.Last().Value));
                    }

                    ///// Chuyển về PK đặc biệt
                    //this.ChangePKModeOfAllPlayerToCustom();
                    ///// Cấm dùng kỹ năng
                    //this.ForbidAllPlayersUseSkills(false);
                }

                /// Nếu đã phân định thắng thua
                if (!this.isBattleFinished && this.IsFinished(out KPlayer winnerTeamPlayer))
                {
                    /// Đánh dấu trận đấu đã kết thúc
                    this.isBattleFinished = true;
                    /// Thực hiện hàm Callback khi có kết quả
                    TeamBattle_ActivityScript.ProcessBattleResult(winnerTeamPlayer, this.Stage, this.IncreaseStageForWinnerTeam);
                    /// Cập nhật thời gian phân định thắng thua
                    this.battleFinishedTicks = KTGlobal.GetCurrentTimeMilis();
                    /// Chuyển qua bước 2
                    this.step = 2;

                    /// Đánh dấu thời gian thông báo thông tin sự kiện
                    this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Cập nhật thông tin sự kiện
                    if (this.TeamNames.Count == 2)
                    {
                        this.UpdateEventDetailsToPlayers(this.CopyScene.Name, TeamBattle.Battle.Config.FinishWaitDuration - KTGlobal.GetCurrentTimeMilis() + this.battleFinishedTicks, "Chúc mừng chiến đội thắng cuộc");
                    }

                    /// Chuyển về PK hòa bình
                    this.ChangePKModeOfAllPlayersToPeace();
                    /// Cấm dùng kỹ năng
                    this.ForbidAllPlayersUseSkills(true);

                    /// Thông báo tới tất cả thành viên
                    KTPlayerManager.ShowNotificationToAllTeammates(winnerTeamPlayer, "Chúc mừng chiến đội đã giành thắng lợi trong trận đấu lần này!");

                    /// Ghi log
                    if (this.TeamNames.Count == 2 && this.TeamNames.ContainsKey(winnerTeamPlayer.TeamID))
                    {
                        LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Finish battle between team {0} and team {1}, winner: {2}", this.TeamNames.First().Value, this.TeamNames.Last().Value, this.TeamNames[winnerTeamPlayer.TeamID]));
                    }
                }
            }
            /// Đã phân định thắng bại
            else if (this.step == 2)
            {
                /// Nếu đã đến thời gian đẩy ra khỏi phụ bản
                if (KTGlobal.GetCurrentTimeMilis() - this.battleFinishedTicks >= TeamBattle.Battle.Config.FinishWaitDuration)
                {
                    /// Đưa tất cả người chơi ra khỏi phụ bản
                    this.KickOutAllPlayers();
                }
            }
        }

        /// <summary>
        /// Hàm này gọi khi phụ bản bị đóng
        /// </summary>
        protected override void OnClose()
        {
            /// Nếu chưa phân định thắng thua
            if (!this.isBattleFinished)
            {
                /// Đánh dấu mặc định kết quả
                KPlayer winnerTeamPlayer;

                /// Danh sách ID các chiến đội
                List<int> teamIDs = this.TotalDamages.Keys.ToList();
                /// Đánh dấu
                Dictionary<int, bool> mark = new Dictionary<int, bool>();
                foreach (int teamID in teamIDs)
                {
                    mark[teamID] = false;
                }


                /// Tổng số nhóm còn người chơi
                int totalTeams = 0;
                /// ID nhóm thắng cuộc
                int winnerTeamID = -1;
                /// Duyệt danh sách đánh dấu
                foreach (KeyValuePair<int, bool> pair in mark)
                {
                    /// Nếu nhóm còn người chơi
                    if (pair.Value)
                    {
                        /// Tăng tổng số nhóm
                        totalTeams++;
                        /// Đánh dấu ID nhóm thắng cuộc
                        winnerTeamID = pair.Key;
                    }
                }

                /// Nếu vẫn chưa có kết quả
                if (totalTeams != 1)
                {
                    /// Sát thương lớn nhất
                    long maxDamages = 0;
                    /// ID nhóm thắng cuộc
                    winnerTeamID = -1;
                    /// Duyệt danh sách gây sát thương
                    foreach (KeyValuePair<int, long> pair in this.TotalDamages)
                    {
                        if (pair.Value > maxDamages)
                        {
                            maxDamages = pair.Value;
                            winnerTeamID = pair.Key;
                        }
                    }
                }

                /// Thành viên đầu tiên của đội thắng cuộc
                KPlayer winnerPlayer = null;
                if (this.TeamPlayers.TryGetValue(winnerTeamID, out ConcurrentDictionary<int, KPlayer> winnerTeam))
                {
                    winnerPlayer = winnerTeam.FirstOrDefault().Value;
                }

                /// Có kết quả
                if (winnerPlayer != null)
                {
                    /// Cập nhật kết quả
                    winnerTeamPlayer = winnerPlayer;

                    /// Thực hiện hàm Callback khi có kết quả
                    TeamBattle_ActivityScript.ProcessBattleResult(winnerTeamPlayer, this.Stage, this.IncreaseStageForWinnerTeam);

                    /// Ghi log
                    LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Finish battle between team {0} and team {1}, winner: {2}", this.TeamNames.First().Value, this.TeamNames.Last().Value, this.TeamNames[winnerTeamPlayer.TeamID]));
                }
            }
        }

        /// <summary>
		/// Sự kiện khi người chơi vào phụ bản
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnter(KPlayer player)
        {
            base.OnPlayerEnter(player);

            /// Mở bảng thông báo hoạt động
            this.OpenEventBroadboard(player, (int) GameEvent.TeamBattle);
            /// Chuyển trạng thái PK hòa bình
            player.PKMode = (int) PKMode.Peace;
            /// Chuyển Camp về -1
            player.Camp = -1;
            /// Cấm chiêu
            player.ForbidUsingSkill = true;
            /// Cập nhật thông tin sự kiện
            this.UpdateEventDetailsToPlayers(this.CopyScene.Name, TeamBattle_Script_Main.PrepareTicks - this.LifeTimeTicks, "Chuẩn bị thi đấu");

            /// Ghi log
            LogManager.WriteLog(LogTypes.TeamBattle, string.Format("TeamID: {0}, Player {1} (ID: {2}) entered the battle.", player.TeamID, player.RoleName, player.RoleID));
        }

        /// <summary>
        /// Sự kiện khi người chơi rời phụ bản
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            base.OnPlayerLeave(player, toMap);

            /// Đóng bảng thông báo hoạt động
            this.CloseEventBroadboard(player, (int) GameEvent.TeamBattle);
            /// Chuyển trạng thái PK hòa bình
            player.PKMode = (int) PKMode.Peace;
            /// Chuyển Camp về -1
            player.Camp = -1;
            /// Hủy cấm chiêu
            player.ForbidUsingSkill = false;
            /// Hủy danh hiệu tạm
            player.TempTitle = "";

            /// Nếu nhóm không tồn tại
            if (!this.TeamPlayers.ContainsKey(player.TeamID))
            {
                /// Xem thằng này cũ ở nhóm nào rồi xóa
                List<int> keys = this.TeamPlayers.Keys.ToList();
                foreach (int key in keys)
                {
                    /// Nếu tồn tại
                    if (this.TeamPlayers.TryGetValue(key, out ConcurrentDictionary<int, KPlayer> playerList))
                    {
                        /// Xóa thằng này khỏi nhóm tương ứng
                        playerList.TryRemove(player.RoleID, out _);
                        /// Thoát lặp
                        break;
                    }
                }

                /// Ghi log
                LogManager.WriteLog(LogTypes.TeamBattle, string.Format("TeamID: {0}, Player {1} (ID: {2}) left the battle, but Team was not presented in the list.", player.TeamID, player.RoleName, player.RoleID));
                return;
            }

            /// Xóa thằng này ra khỏi nhóm
            this.TeamPlayers[player.TeamID].TryRemove(player.RoleID, out _);

            /// Ghi log
            LogManager.WriteLog(LogTypes.TeamBattle, string.Format("TeamID: {0}, Player {1} (ID: {2}) left the battle.", player.TeamID, player.RoleName, player.RoleID));
        }

        /// <summary>
        /// Sự kiện khi người chơi gây sát thương cho người chơi khác
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        /// <param name="damage"></param>
        public override void OnHitTarget(KPlayer player, GameObject obj, int damage)
        {
            base.OnHitTarget(player, obj, damage);

            /// Nếu đối phương không phải người chơi
            if (!(obj is KPlayer))
            {
                return;
            }

            /// Toác gì đó
            if (!this.TotalDamages.ContainsKey(player.TeamID))
            {
                return;
            }

            /// Cộng dồn sát thương cho nhóm của người chơi gây sát thương
            this.TotalDamages[player.TeamID] += damage;
        }

        /// <summary>
        /// Sự kiện khi người chơi bị chết
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="player"></param>
        public override void OnPlayerDie(GameObject killer, KPlayer player)
        {
            base.OnPlayerDie(killer, player);

            /// Cho sống lại ở bản đồ báo danh
            KTPlayerManager.Relive(player, TeamBattle.Config.EnterMapID, TeamBattle.Config.EnterPosX, TeamBattle.Config.EnterPosY, this.CopyScene.ReliveHPPercent, this.CopyScene.ReliveMPPercent, this.CopyScene.ReliveStaminaPercent);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tick thông tin chiến đội
        /// </summary>
        private void TickPlayersTeamInfo()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Mở bảng thông báo hoạt động
                this.OpenEventBroadboard(player, (int) GameEvent.TeamBattle);

                /// Cập nhật thông tin tích lũy chiến đội
                if (!this.TotalDamages.TryGetValue(player.TeamID, out _))
                {
                    this.TotalDamages[player.TeamID] = 0;
                }

                /// Cập nhật danh sách thành viên đội
                if (!this.TeamPlayers.TryGetValue(player.TeamID, out _))
                {
                    this.TeamPlayers[player.TeamID] = new ConcurrentDictionary<int, KPlayer>();
                }
                /// Nếu chưa tồn tại
                if (!this.TeamPlayers[player.TeamID].ContainsKey(player.RoleID))
                {
                    /// Thêm thành viên này vào nhóm
                    this.TeamPlayers[player.TeamID][player.RoleID] = player;
                }

                /// Cập nhật tên nhóm
                if (!this.TeamNames.TryGetValue(player.TeamID, out _))
                {
                    TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player);
                    /// Nếu tìm thấy
                    if (teamInfo != null)
                    {
                        this.TeamNames[player.TeamID] = teamInfo.Name;
                    }
                    else
                    {
                        this.TeamNames[player.TeamID] = "Đội của " + player.RoleName;
                    }
                }

                /// Cập nhật danh hiệu tạm
                player.TempTitle = string.Format("<color=#ffd60a>{0}</color>", this.TeamNames[player.TeamID]);
            }
        }

        /// <summary>
        /// Chuyển trạng thái PK của tất cả người chơi về dạng hòa bình
        /// </summary>
        /// <param name="pkMode"></param>
        private void ChangePKModeOfAllPlayersToPeace()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                player.PKMode = (int) PKMode.Peace;
                player.Camp = -1;
            }
        }

        /// <summary>
        /// Chuyển trạng thái PK của tất cả người chơi về dạng đặc biệt
        /// </summary>
        private void ChangePKModeOfAllPlayerToCustom()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                player.PKMode = (int) PKMode.Custom;
                player.Camp = player.TeamID;
            }
        }

        /// <summary>
        /// Thiết lập trạng thái cấm dùng kỹ năng cho toàn thể người chơi
        /// </summary>
        /// <param name="isForbiden"></param>
        private void ForbidAllPlayersUseSkills(bool isForbiden)
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                player.ForbidUsingSkill = isForbiden;
            }
        }

        /// <summary>
        /// Trận đấu đã kết thúc chưa
        /// </summary>
        /// <param name="winnerTeamPlayer"></param>
        /// <param name="loserTeamPlayer"></param>
        /// <returns></returns>
        private bool IsFinished(out KPlayer winnerTeamPlayer)
        {
            /// Đánh dấu mặc định kết quả
            winnerTeamPlayer = null;

            /// Danh sách ID các chiến đội
            List<int> teamIDs = this.TotalDamages.Keys.ToList();
            /// Đánh dấu
            Dictionary<int, bool> mark = new Dictionary<int, bool>();
            foreach (int teamID in teamIDs)
            {
                mark[teamID] = false;
            }

            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Toác
                if (!mark.ContainsKey(player.TeamID))
                {
                    continue;
                }

                /// Nếu còn sống
                if (!player.IsDead())
                {
                    /// Đánh dấu nhóm này vẫn còn người
                    mark[player.TeamID] = true;
                }
            }

            /// Tổng số nhóm còn người chơi
            int totalTeams = 0;
            /// ID nhóm thắng cuộc
            int winnerTeamID = -1;
            /// Duyệt danh sách đánh dấu
            foreach (KeyValuePair<int, bool> pair in mark)
            {
                /// Nếu nhóm còn người chơi
                if (pair.Value)
                {
                    /// Tăng tổng số nhóm
                    totalTeams++;
                    /// Đánh dấu ID nhóm thắng cuộc
                    winnerTeamID = pair.Key;
                }
            }

            /// Nếu đã phân thắng bại
            if (totalTeams == 1)
            {
                /// Thành viên đầu tiên của đội thắng cuộc
                KPlayer winnerPlayer = null;
                if (this.TeamPlayers.TryGetValue(winnerTeamID, out ConcurrentDictionary<int, KPlayer> winnerTeam))
                {
                    winnerPlayer = winnerTeam.FirstOrDefault().Value;
                }

                /// Toác gì đó
                if (winnerPlayer == null)
                {
                    return false;
                }

                /// Cập nhật kết quả
                winnerTeamPlayer = winnerPlayer;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Đưa tất cả người chơi trở về bản đồ hội trường
        /// </summary>
        private void KickOutAllPlayers()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                KTPlayerManager.ChangeMap(player, TeamBattle.Config.EnterMapID, TeamBattle.Config.EnterPosX, TeamBattle.Config.EnterPosY);
            }
        }
        #endregion
    }
}
