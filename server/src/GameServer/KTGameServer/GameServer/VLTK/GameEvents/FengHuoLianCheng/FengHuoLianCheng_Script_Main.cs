using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using Server.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.FengHuoLianCheng
{
    /// <summary>
    /// Script chính thực thi Logic sự kiện Đoán hoa đăng
    /// </summary>
    public class FengHuoLianCheng_Script_Main : GameMapEvent
    {
        #region Define
        /// <summary>
        /// Thời gian cập nhật bảng xếp hạng liên tục
        /// </summary>
        private const int UpdateScoreboardInterval = 10000;

        /// <summary>
        /// Thời gian cập nhật bảng thông tin tích lũy liên tục
        /// </summary>
        private const int UpdateEventBroadboardInterval = 5000;

        /// <summary>
        /// Camp phe công thành
        /// </summary>
        private const int AttackerCampID = 2;

        /// <summary>
        /// Camp phe thủ thành
        /// </summary>
        private const int DefenderCampID = 1;

        /// <summary>
        /// Thứ hạng người chơi
        /// </summary>
        private class PlayerRank
        {
            /// <summary>
            /// ID người chơi
            /// </summary>
            public int RoleID { get; set; }

            /// <summary>
            /// Tổng điểm tích lũy
            /// </summary>
            public int Score { get; set; }

            /// <summary>
            /// Thứ hạng
            /// </summary>
            public int Rank { get; set; }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách người chơi với điểm số tương ứng trong sự kiện
        /// </summary>
        private readonly ConcurrentDictionary<int, int> playerScoreboard = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Bảng xếp hạng
        /// </summary>
        private Dictionary<int, PlayerRank> scoreboard = new Dictionary<int, PlayerRank>();

        /// <summary>
        /// Thời điểm cập nhật bảng xếp hạng lần trước
        /// </summary>
        private long lastUpdateScoreboardTicks = 0;

        /// <summary>
        /// Thời gian cập nhật thông tin bảng sự kiện trước
        /// </summary>
        private long lastUpdateEventBroadboardTicks = 0;

        /// <summary>
        /// Thời điểm bắt đầu lượt tấn công mới lần trước
        /// </summary>
        private long lastStartNewRoundTicks = 0;

        /// <summary>
        /// Lượt tấn công hiện tại
        /// </summary>
        private int roundID = -1;

        /// <summary>
        /// Cấp độ quái và Boss trong sự kiện
        /// </summary>
        private readonly int level;

        /// <summary>
        /// Danh sách nguyên soái
        /// </summary>
        private List<Monster> marshals;

        /// <summary>
        /// Thời điểm Tick bảo vệ nguyên soái lần trước
        /// </summary>
        private long lastProtectMarshalTicks = 0;
        #endregion

        #region Core GameMapEvent
        /// <summary>
        /// Script chính thực thi Logic sự kiện Đoán hoa đăng
        /// </summary>
        /// <param name="map"></param>
        /// <param name="activity"></param>
        public FengHuoLianCheng_Script_Main(GameMap map, KTActivity activity) : base(map, activity)
        {
            /// Cấp độ tổng
            int totalLevel = 0;
            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Thêm vào danh sách tích điểm
                this.playerScoreboard[player.RoleID] = 0;
                /// Tăng cấp độ tổng
                totalLevel += player.m_Level;
                /// Thay đổi Camp
                player.Camp = FengHuoLianCheng_Script_Main.DefenderCampID;

                /// Mở khung sự kiện ở góc trái
                this.OpenEventBroadboard(player, (int) GameEvent.FengHuoLianCheng);

            }
            /// Tính trung bình cấp độ
            this.level = totalLevel / this.GetTotalPlayers();
        }

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        protected override void OnStart()
        {
            /// Cập nhật bảng xếp hạng
            this.UpdateScoreboard();
            /// Cập nhật bảng sự kiện
            this.UpdateEventBroadboard();
            /// Tạo cổng
            this.CreateTeleports();
            /// Tạo NPC
            this.CreateNPCs();
            /// Tạo phe thủ
            this.CreateDefenders();

            /// Đánh dấu thời điểm bắt đầu để nó bỏ qua lượt tạo quái đầu tiên
            this.lastStartNewRoundTicks = KTGlobal.GetCurrentTimeMilis();
            /// Thông báo đợt tấn công sắp bắt đầu
            this.NotifyAllPlayers(string.Format("Đợt tấn công đầu tiên sẽ bắt đầu sau {0} giây nữa!", FengHuoLianCheng.Data.RoundData.RoundPeriod / 1000));
        }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu sự kiện thất bại
            if (this.IsEventFailed())
            {
                /// Thực thi sự kiện
                this.EventFailed();
                /// Bỏ qua
                return;
            }
            /// Cập nhật bảng xếp hạng
            this.UpdateScoreboard();
            /// Cập nhật bảng sự kiện
            this.UpdateEventBroadboard();
            /// Thực thi từng đợt tấn công
            this.HandleRounds();
            /// Bảo vệ nguyên soái
            this.ProtectMarshals();
        }

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected override void OnClose()
        {
            /// Làm rỗng danh sách tích lũy
            this.playerScoreboard.Clear();
            this.scoreboard.Clear();
            /// Làm rỗng danh sách nguyên soái
            this.marshals.Clear();
        }

        /// <summary>
        /// Sự kiện hết thời gian
        /// </summary>
        protected override void OnTimeout()
        {
            /// Gọi phương thức cha
            base.OnTimeout();

            /// Cập nhật bảng xếp hạng
            this.UpdateScoreboard();

            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Nếu không có trong bảng xếp hạng
                if (!this.scoreboard.TryGetValue(player.RoleID, out PlayerRank rankInfo))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Cập nhật thứ hạng
                FengHuoLianCheng_ActivityScript.SetLastEventRank(player, rankInfo.Rank);
            }

            /// Nếu còn trên 1 nguyên soái
            if (this.marshals.Count >= 1)
            {
                /// Sự kiện thành công
                this.EventSuccess();
            }
            /// Nếu không còn nguyên soái
            else
            {
                /// Sự kiện thất bại
                this.EventFailed();
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi giết quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public override void OnKillObject(KPlayer player, GameObject obj)
        {
            /// Gọi phương thức cha
            base.OnKillObject(player, obj);

            /// Nếu không phải Monster
            if (!(obj is Monster monster))
            {
                /// Bỏ qua
                return;
            }
            /// Nếu thằng này không có trong danh sách tích điểm
            else if (!this.playerScoreboard.ContainsKey(player.RoleID))
            {
                /// Bỏ qua
                return;
            }

            /// Đây là loại gì
            switch (monster.Tag)
            {
                /// Quái thường
                case "Monster":
                {
                    /// Số điểm tăng thêm
                    int pointGain = FengHuoLianCheng.Data.EventPointConfig.KillMonsterPoint;
                    /// Tăng điểm tích lũy cho thằng này
                    this.playerScoreboard[player.RoleID] += pointGain;
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Đã tiêu diệt [{0}], tích lũy tăng {1} điểm.", obj.RoleName, pointGain));

                    break;
                }
                /// Boss
                case "Boss":
                {
                    /// Số điểm tăng thêm
                    int pointGain = FengHuoLianCheng.Data.EventPointConfig.KillBossPoint;
                    /// Tăng điểm tích lũy cho thằng này
                    this.playerScoreboard[player.RoleID] += pointGain;
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Đã tiêu diệt [{0}], tích lũy tăng {1} điểm.", obj.RoleName, pointGain));

                    /// Nếu có đội
                    if (player.TeamID != -1 && KTTeamManager.IsTeamExist(player.TeamID))
                    {
                        /// Số điểm tăng thêm
                        int _pointGain = FengHuoLianCheng.Data.EventPointConfig.KillBossTeamPoint;
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, string.Format("Thành viên đội [{0}] đã tiêu diệt [{1}], tích lũy bản thân tăng {2} điểm.", player.RoleName, obj.RoleName, _pointGain));

                        /// Duyệt danh sách các thành viên đội
                        foreach (KPlayer teammate in player.Teammates)
                        {
                            /// Nếu không tồn tại
                            if (!this.playerScoreboard.ContainsKey(teammate.RoleID))
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Tăng điểm tích lũy
                            this.playerScoreboard[teammate.RoleID] += _pointGain;
                        }
                    }

                    /// Số điểm tăng thêm
                    int __pointGain = FengHuoLianCheng.Data.EventPointConfig.KillBossAllPoint;
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("[{0}] đã tiêu diệt [{1}], tích lũy bản thân tăng {2} điểm.", player.RoleName, obj.RoleName, __pointGain));
                    /// Duyệt danh sách người chơi trong bản đồ
                    foreach (KPlayer otherPlayer in this.GetPlayers())
                    {
                        /// Nếu không tồn tại
                        if (!this.playerScoreboard.ContainsKey(otherPlayer.RoleID))
                        {
                            /// Bỏ qua
                            continue;
                        }

                        /// Tăng điểm tích lũy
                        this.playerScoreboard[otherPlayer.RoleID] += __pointGain;
                    }

                    break;
                }
                /// Xe công thành
                case "Siege":
                {
                    /// Số điểm tăng thêm
                    int pointGain = FengHuoLianCheng.Data.EventPointConfig.KillSiegePoint;
                    /// Tăng điểm tích lũy cho thằng này
                    this.playerScoreboard[player.RoleID] += pointGain;
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Đã tiêu diệt [{0}], tích lũy tăng {1} điểm.", obj.RoleName, pointGain));

                    /// Nếu có đội
                    if (player.TeamID != -1 && KTTeamManager.IsTeamExist(player.TeamID))
                    {
                        /// Số điểm tăng thêm
                        int _pointGain = FengHuoLianCheng.Data.EventPointConfig.KillSiegeTeamPoint;
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, string.Format("Thành viên đội [{0}] đã tiêu diệt [{1}], tích lũy bản thân tăng {2} điểm.", player.RoleName, obj.RoleName, _pointGain));

                        /// Duyệt danh sách các thành viên đội
                        foreach (KPlayer teammate in player.Teammates)
                        {
                            /// Nếu không tồn tại
                            if (!this.playerScoreboard.ContainsKey(teammate.RoleID))
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Tăng điểm tích lũy
                            this.playerScoreboard[teammate.RoleID] += _pointGain;
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerEnter(KPlayer player)
        {
            /// Gọi phương thức cha
            base.OnPlayerEnter(player);

            /// Nếu không có trong danh sách điểm tích lũy
            if (!this.playerScoreboard.ContainsKey(player.RoleID))
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Bạn không đăng ký tham dự Phong Hỏa Liên Thành hôm nay!");
                /// Trục xuất
                this.KickOutPlayer(player);
            }

            /// Thay đổi Camp
            player.Camp = FengHuoLianCheng_Script_Main.DefenderCampID;

            /// Mở khung sự kiện ở góc trái
            this.OpenEventBroadboard(player, (int) GameEvent.FengHuoLianCheng);
        }

        /// <summary>
        /// Sự kiện khi người chơi rời khỏi bản đồ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            /// Gọi phương thức cha
            base.OnPlayerLeave(player, toMap);

            /// Đóng khung sự kiện ở góc trái
            this.CloseEventBroadboard(player, (int) GameEvent.FengHuoLianCheng);
        }

        /// <summary>
        /// Sự kiện khi người choi ấn nút về thành khi bị trọng thương
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
		public override bool OnPlayerClickReliveButton(KPlayer player)
        {
            /// Gọi phương thức cha
            base.OnPlayerClickReliveButton(player);

            /// Đưa người chơi về vị trí báo danh
            KTPlayerManager.Relive(player, this.Map.MapCode, FengHuoLianCheng.Data.Map.EnterPosX, FengHuoLianCheng.Data.Map.EnterPosY, 100, 100, 100);
            return true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật bảng xếp hạng
        /// </summary>
        private void UpdateScoreboard()
        {
            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastUpdateScoreboardTicks < FengHuoLianCheng_Script_Main.UpdateScoreboardInterval)
            {
                /// Bỏ qua
                return;
            }
            /// Ghi lại thời gian cập nhật xếp hạng
            this.lastUpdateScoreboardTicks = KTGlobal.GetCurrentTimeMilis();

            /// Hủy xếp hạng cũ
            this.scoreboard.Clear();

            /// Thứ tự
            int idx = 0;
            /// Xếp hạng mới
            this.scoreboard = this.playerScoreboard.OrderByDescending(x => x.Value).Select(x => new PlayerRank()
            {
                RoleID = x.Key,
                Score = x.Value,
                Rank = ++idx,
            }).ToDictionary(tKey => tKey.RoleID, tValue => tValue);
        }

        /// <summary>
        /// Cập nhật thông tin vào bảng thông tin sự kiện
        /// </summary>
        private void UpdateEventBroadboard()
        {
            /// Nếu chưa tạo nguyên soái
            if (this.marshals == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastUpdateEventBroadboardTicks < FengHuoLianCheng_Script_Main.UpdateEventBroadboardInterval)
            {
                /// Bỏ qua
                return;
            }
            /// Ghi lại thời gian cập nhật thông tin
            this.lastUpdateEventBroadboardTicks = KTGlobal.GetCurrentTimeMilis();

            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Nếu không tồn tại
                if (!this.scoreboard.TryGetValue(player.RoleID, out PlayerRank rankInfo))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thời gian còn lại
                long timeLeft = this.DurationTicks - this.LifeTimeTicks;
                /// Chuỗi thông tin
                this.UpdateEventDetails(player, "Phong Hỏa Liên Thành", timeLeft, new string[]
                {
                  
                    "Đợt tấn công thứ :" +  this.roundID,
                    string.Format("Tích lũy: <color=yellow>{0}</color>", rankInfo.Score),
                    string.Format("Xếp hạng: <color=yellow>{0}</color>", rankInfo.Rank),
                    string.Format("Nguyên soái: <color=yellow>{0}</color>", this.marshals.Count),
                });
            }
        }

        /// <summary>
        /// Thực thi các đợt tấn công
        /// </summary>
        private void HandleRounds()
        {
            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastStartNewRoundTicks < FengHuoLianCheng.Data.RoundData.RoundPeriod)
            {
                /// Bỏ qua
                return;
            }
            /// Đánh dấu thời điểm phát động tấn công
            this.lastStartNewRoundTicks = KTGlobal.GetCurrentTimeMilis();

            /// Tăng thứ tự lượt tấn công lên
            this.roundID++;
            /// Nếu đã hết lượt thì mặc định lấy lượt cuối cùng
            if (this.roundID >= FengHuoLianCheng.Data.RoundData.Rounds.Count)
            {
                this.roundID = FengHuoLianCheng.Data.RoundData.Rounds.Count - 1;
            }

            /// Bắt đầu lượt này
            FengHuoLianCheng.FHLCData.RoundInfo.RoundData roundData = FengHuoLianCheng.Data.RoundData.Rounds[this.roundID];
            /// Tạo quái
            this.CreateMonsters(roundData);
            this.CreateBosses(roundData);
            this.CreateSieges(roundData);

            /// Thông báo đã bắt đầu lượt tấn công mới
            this.NotifyAllPlayers(string.Format("Đối phương đã phát động đợt tấn công thứ {0}. Đợt tiếp theo sẽ bắt đầu sau {1} giây.", this.roundID + 1, FengHuoLianCheng.Data.RoundData.RoundPeriod / 1000));
        }

        /// <summary>
        /// Bảo vệ nguyên soái
        /// </summary>
        /// <param name="marshal"></param>
        private void ProtectMarshals()
        {
            /// Nếu chưa tạo nguyên soái
            if (this.marshals == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastProtectMarshalTicks < FengHuoLianCheng.Data.EventPointConfig.ProtectMarshalAwardPeriod)
            {
                /// Bỏ qua
                return;
            }
            /// Đánh dấu thời điểm Tick bảo vệ nguyên soái
            this.lastProtectMarshalTicks = KTGlobal.GetCurrentTimeMilis();

            /// Đánh dấu thằng nào đã nhận điểm bảo vệ nguyên soái rồi
            HashSet<int> markAlreadyGotten = new HashSet<int>();

            /// Duyệt danh sách
            foreach (Monster marshal in this.marshals)
            {
                /// Nếu đã chết
                if (marshal.IsDead())
                {
                    /// Bỏ qua
                    continue;
                }

                /// Danh sách người chơi xung quanh
                List<KPlayer> nearbyPlayers = KTGlobal.GetNearByPlayers(marshal, 600);
                /// Duyệt danh sách người chơi xung quanh
                foreach (KPlayer player in nearbyPlayers)
                {
                    /// Nếu không có trong danh sách
                    if (!this.playerScoreboard.ContainsKey(player.RoleID))
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Nếu đã nhận rồi
                    else if (markAlreadyGotten.Contains(player.RoleID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Ngươi có công bảo vệ nguyên soái, nhận được {0} điểm tích lũy!", FengHuoLianCheng.Data.EventPointConfig.ProtectMarshalAwardPoint));
                    /// Thêm điểm tích lũy
                    this.playerScoreboard[player.RoleID] += FengHuoLianCheng.Data.EventPointConfig.ProtectMarshalAwardPoint;

                    /// Thêm vào danh sách đã nhận điểm rồi
                    markAlreadyGotten.Add(player.RoleID);
                }
            }

            /// Hủy để giải phóng tài nguyên
            markAlreadyGotten.Clear();
        }

        /// <summary>
        /// Sự kiện thành công
        /// </summary>
        private void EventSuccess()
        {
            /// Thông báo
            KTGlobal.SendSystemEventNotification("Sự kiện Phong Hỏa Liên Thành đã kết thúc với thắng lợi thuộc về phe thủ thành. Hẹn gặp quý bằng hữu vào sự kiện lần tiếp theo.");

            /// Trục xuất toàn bộ người chơi ra khỏi bản đồ
            this.KickOutAllPlayers();
        }

        /// <summary>
        /// Sự kiện thất bại
        /// </summary>
        private void EventFailed()
        {
            /// Thông báo sự kiện thất bại
            KTGlobal.SendSystemEventNotification("Trong sự kiện Phong Hỏa Liên Thành ngày hôm nay, phe phòng thủ đã thất bại không thể bảo vệ nguyên soái. Hãy cố gắng vào sự kiện lần tới.");

            /// Trục xuất người chơi
            this.KickOutAllPlayers();
            /// Hủy sự kiện
            KTActivityManager.StopActivity(this.Activity.Data.ID);
            /// Hủy script
            this.Dispose();
        }

        /// <summary>
        /// Có phải sự kiện đã thất bại không
        /// </summary>
        /// <returns></returns>
        private bool IsEventFailed()
        {
            /// Nếu chưa tạo nguyên soái
            if (this.marshals == null)
            {
                /// Chưa bắt đầu
                return false;
            }

            /// Duyệt danh sách
            foreach (Monster marshal in this.marshals)
            {
                /// Nếu vẫn còn sống
                if (!marshal.IsDead())
                {
                    return false;
                }
            }
            /// Không con nào còn sống thì thất bại
            return true;
        }

        /// <summary>
        /// Đưa người chơi ra khỏi bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        private void KickOutPlayer(KPlayer player)
        {
            KTPlayerManager.ChangeMap(player, FengHuoLianCheng.Data.Map.CityMapID, FengHuoLianCheng.Data.Map.CityPosX, FengHuoLianCheng.Data.Map.CityPosY);
        }

        /// <summary>
        /// Đưa toàn bộ người chơi ra khỏi bản đồ hoạt động
        /// </summary>
        private void KickOutAllPlayers()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                this.KickOutPlayer(player);
            }
        }
        #endregion

        #region Support methods
        /// <summary>
        /// Trả về vị trí xung quanh điểm cho trước không nằm trong ô vật cản
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private UnityEngine.Vector2 GetRandomNoObsPos(int posX, int posY, int radius)
        {
            /// Vị trí
            return KTGlobal.GetRandomAroundNoObsPoint(this.Map, new UnityEngine.Vector2(posX, posY), radius, -1);
        }

        /// <summary>
        /// Tạo quái số lượng tương ứng ở các nhóm
        /// </summary>
        /// <param name="roundData"></param>
        private void CreateMonsters(FengHuoLianCheng.FHLCData.RoundInfo.RoundData roundData)
        {
            /// Tổng số quái thêm vào
            int additionMonsters = this.GetTotalPlayers() / FengHuoLianCheng.Data.RoundData.Addition.Monsters;

            /// Duyệt danh sách nhóm
            foreach (FengHuoLianCheng.FHLCData.GroupPositionData groupData in FengHuoLianCheng.Data.GroupPositions)
            {
                /// Duyệt danh sách quái
                foreach (KeyValuePair<int, int> roundMonsterInfo in roundData.Monsters)
                {
                    /// Thời gian delay
                    int delayTime = 0;

                    /// Thông tin quái
                    FengHuoLianCheng.FHLCData.MonsterInfo monsterInfo = FengHuoLianCheng.Data.Attacker.Monsters.Where(x => x.Order == roundMonsterInfo.Key).FirstOrDefault();

                    /// Tổng số quái
                    int monstersCount = roundMonsterInfo.Value;
                    /// Thêm vào tổng số người tham gia
                    monstersCount += additionMonsters;

                    /// Duyệt tổng số sẽ tạo ra
                    for (int i = 1; i <= monstersCount; i++)
                    {
                        /// Chạy timer
                        this.SetTimeout(delayTime, () =>
                        {
                            /// Chọn 1 vị trí ngẫu nhiên trong nhóm
                            FengHuoLianCheng.FHLCData.GroupPositionData.Position position = groupData.Positions.RandomRange(1).FirstOrDefault();

                            /// Ngũ hành
                            KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                            /// Hướng quay
                            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                            /// Vị trí ngẫu nhiên
                            UnityEngine.Vector2 randomPos = this.GetRandomNoObsPos(position.PosX, position.PosY, position.Radius);
                            /// Vị trí
                            int posX = (int) randomPos.x;
                            int posY = (int) randomPos.y;

                            /// Vị trí đích đến ngẫu nhiên
                            UnityEngine.Vector2 randomDestPos = this.GetRandomNoObsPos(groupData.DestPosX, groupData.DestPosY, position.Radius);
                            /// Vị trí đích
                            int destX = (int) randomDestPos.x;
                            int destY = (int) randomDestPos.y;

                            /// Mức máu
                            int hp = monsterInfo.BaseHP + monsterInfo.HPIncreaseEachLevel * this.level;

                            /// Tạo quái thêm mã màu để dễ nhận biết phe địch với phe mình
                            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                            {
                                MapCode = this.Map.MapCode,
                                ResID = monsterInfo.ID,
                                PosX = posX,
                                PosY = posY,
                                Name = "<color=red>" + monsterInfo.Name + "</color>",
                                Title = monsterInfo.Title,
                                Level = this.level,
                                MaxHP = hp,
                                MonsterType = MonsterAIType.Special_Normal,
                                AIID = monsterInfo.AIScriptID,
                                Tag = "Monster",
                                Camp = FengHuoLianCheng_Script_Main.AttackerCampID,
                            });

                            /// Nếu có kỹ năng
                            if (monsterInfo.Skills.Count > 0)
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (SkillLevelRef skill in monsterInfo.Skills)
                                {
                                    /// Thêm vào danh sách kỹ năng dùng của quái
                                    monster.CustomAISkills.Add(skill);
                                }
                            }

                            /// Nếu có vòng sáng
                            if (monsterInfo.Auras.Count > 0)
                            {
                                /// Duyệt danh sách vòng sáng
                                foreach (SkillLevelRef aura in monsterInfo.Auras)
                                {
                                    /// Kích hoạt vòng sáng
                                    monster.UseSkill(aura.SkillID, aura.Level, monster);
                                }
                            }

                            /// Sử dụng thuật toán A* tìm đường
                            monster.UseAStarPathFinder = true;
                            /// Thực hiện di chuyển đến vị trí tương ứng
                            monster.MoveTo(new System.Windows.Point(destX, destY), true);

                            /// Đánh dấu đã đến đích chưa
                            bool destinationReached = false;
                            /// Sự kiện Tick
                            monster.OnTick = () =>
                            {
                                /// Nếu đã đến đích
                                if (destinationReached)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Cập nhật vị trí đứng mới
                                monster.StartPos = monster.CurrentPos;
                                /// Nếu trong 2s chịu sát thương
                                if (KTGlobal.GetCurrentTimeMilis() - monster.LastBeHitTicks < 2000)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Đã đến đích
                                if (KTGlobal.GetDistanceBetweenPoints(new System.Windows.Point(destX, destY), monster.CurrentPos) <= 10)
                                {
                                    /// Đánh dấu đã đến đích
                                    destinationReached = true;
                                    /// Bỏ qua
                                    return;
                                }

                                /// Nếu không có người chơi xung quanh và đang không di chuyển
                                if (!monster.IsChasingTarget && !monster.IsMoving)
                                {
                                    /// Thực hiện di chuyển đến vị trí tương ứng
                                    monster.MoveTo(new System.Windows.Point(destX, destY), true);
                                }
                            };
                        });

                        /// Tăng thời gian Delay
                        delayTime += roundData.DelaySpawn;
                    }
                }
            }
        }

        /// <summary>
        /// Tạo Boss số lượng tương ứng ở các nhóm
        /// </summary>
        /// <param name="count"></param>
        private void CreateBosses(FengHuoLianCheng.FHLCData.RoundInfo.RoundData roundData)
        {
            /// Tổng số boss thêm vào
            int additionBosses = this.GetTotalPlayers() / FengHuoLianCheng.Data.RoundData.Addition.Bosses;

            /// Duyệt danh sách nhóm
            foreach (FengHuoLianCheng.FHLCData.GroupPositionData groupData in FengHuoLianCheng.Data.GroupPositions)
            {
                /// Duyệt danh sách Boss
                foreach (KeyValuePair<int, int> roundBossInfo in roundData.Bosses)
                {
                    /// Thời gian delay
                    int delayTime = 0;

                    /// Thông tin Boss
                    FengHuoLianCheng.FHLCData.BossInfo bossInfo = FengHuoLianCheng.Data.Attacker.Bosses.Where(x => x.Order == roundBossInfo.Key).FirstOrDefault();

                    /// Tổng số Boss
                    int bossesCount = roundBossInfo.Value;
                    /// Thêm vào số lượng người tham gia
                    bossesCount += additionBosses;

                    /// Duyệt tổng số sẽ tạo ra
                    for (int i = 1; i <= bossesCount; i++)
                    {
                        /// Chạy Timer
                        this.SetTimeout(delayTime, () =>
                        {
                            /// Chọn 1 vị trí ngẫu nhiên trong nhóm
                            FengHuoLianCheng.FHLCData.GroupPositionData.Position position = groupData.Positions.RandomRange(1).FirstOrDefault();

                            /// Ngũ hành
                            KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                            /// Hướng quay
                            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                            /// Vị trí ngẫu nhiên
                            UnityEngine.Vector2 randomPos = this.GetRandomNoObsPos(position.PosX, position.PosY, position.Radius);
                            /// Vị trí
                            int posX = (int) randomPos.x;
                            int posY = (int) randomPos.y;

                            /// Vị trí đích đến ngẫu nhiên
                            UnityEngine.Vector2 randomDestPos = this.GetRandomNoObsPos(groupData.DestPosX, groupData.DestPosY, position.Radius);
                            /// Vị trí đích
                            int destX = (int) randomDestPos.x;
                            int destY = (int) randomDestPos.y;

                            /// Mức máu
                            int hp = bossInfo.BaseHP + bossInfo.HPIncreaseEachLevel * this.level;

                            /// Tạo quái thêm mã màu để dễ nhận biết phe địch với phe mình
                            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                            {
                                MapCode = this.Map.MapCode,
                                ResID = bossInfo.ID,
                                PosX = posX,
                                PosY = posY,
                                Name = KTGlobal.CreateStringByColor(bossInfo.Name, ColorType.Pure),
                                Title = bossInfo.Title,
                                MaxHP = hp,
                                MonsterType = MonsterAIType.Special_Boss,
                                Level = this.level,
                                AIID = bossInfo.AIScriptID,
                                Tag = "Boss",
                                Camp = FengHuoLianCheng_Script_Main.AttackerCampID,
                            });

                            /// Nếu có kỹ năng
                            if (bossInfo.Skills.Count > 0)
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (SkillLevelRef skill in bossInfo.Skills)
                                {
                                    /// Thêm vào danh sách kỹ năng dùng của quái
                                    monster.CustomAISkills.Add(skill);
                                }
                            }

                            /// Nếu có vòng sáng
                            if (bossInfo.Auras.Count > 0)
                            {
                                /// Duyệt danh sách vòng sáng
                                foreach (SkillLevelRef aura in bossInfo.Auras)
                                {
                                    /// Kích hoạt vòng sáng
                                    monster.UseSkill(aura.SkillID, aura.Level, monster);
                                }
                            }

                            /// Sử dụng thuật toán A* tìm đường
                            monster.UseAStarPathFinder = true;
                            /// Thực hiện di chuyển đến vị trí tương ứng
                            monster.MoveTo(new System.Windows.Point(destX, destY), true);

                            /// Đánh dấu đã đến đích chưa
                            bool destinationReached = false;
                            /// Sự kiện Tick
                            monster.OnTick = () =>
                            {
                                /// Nếu đã đến đích
                                if (destinationReached)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Cập nhật vị trí đứng mới
                                monster.StartPos = monster.CurrentPos;
                                /// Nếu trong 2s chịu sát thương
                                if (KTGlobal.GetCurrentTimeMilis() - monster.LastBeHitTicks < 2000)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Đã đến đích
                                if (KTGlobal.GetDistanceBetweenPoints(new System.Windows.Point(destX, destY), monster.CurrentPos) <= 10)
                                {
                                    /// Đánh dấu đã đến đích
                                    destinationReached = true;
                                    /// Bỏ qua
                                    return;
                                }

                                /// Nếu không có người chơi xung quanh và đang không di chuyển
                                if (!monster.IsChasingTarget && !monster.IsMoving)
                                {
                                    /// Thực hiện di chuyển đến vị trí tương ứng
                                    monster.MoveTo(new System.Windows.Point(destX, destY), true);
                                }
                            };
                        });

                        /// Tăng thời gian Delay
                        delayTime += roundData.DelaySpawn;
                    }
                }
            }
        }

        /// <summary>
        /// Tạo xe công thành số lượng tương ứng ở các nhóm
        /// </summary>
        /// <param name="roundData"></param>
        private void CreateSieges(FengHuoLianCheng.FHLCData.RoundInfo.RoundData roundData)
        {
            /// Tổng số xe công thành thêm vào
            int additionSieges = this.GetTotalPlayers() / FengHuoLianCheng.Data.RoundData.Addition.Sieges;

            /// Duyệt danh sách nhóm
            foreach (FengHuoLianCheng.FHLCData.GroupPositionData groupData in FengHuoLianCheng.Data.GroupPositions)
            {
                /// Duyệt danh sách quái
                foreach (KeyValuePair<int, int> roundSiegeInfo in roundData.Sieges)
                {
                    /// Thời gian delay
                    int delayTime = 0;

                    /// Thông tin xe công thành
                    FengHuoLianCheng.FHLCData.MonsterInfo siegeInfo = FengHuoLianCheng.Data.Attacker.Sieges.Where(x => x.Order == roundSiegeInfo.Key).FirstOrDefault();

                    /// Tổng số xe công thành
                    int siegesCount = roundSiegeInfo.Value;
                    /// Thêm vào tổng số người
                    siegesCount += additionSieges;

                    /// Duyệt tổng số sẽ tạo ra
                    for (int i = 1; i <= roundSiegeInfo.Value; i++)
                    {
                        /// Chạy Timer
                        this.SetTimeout(delayTime, () =>
                        {
                            /// Chọn 1 vị trí ngẫu nhiên trong nhóm
                            FengHuoLianCheng.FHLCData.GroupPositionData.Position position = groupData.Positions.RandomRange(1).FirstOrDefault();

                            /// Ngũ hành
                            KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                            /// Hướng quay
                            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
                            /// Vị trí
                            int posX = position.PosX + KTGlobal.GetRandomNumber(-position.Radius, position.Radius);
                            int posY = position.PosY + KTGlobal.GetRandomNumber(-position.Radius, position.Radius);

                            /// Vị trí đích đến ngẫu nhiên
                            UnityEngine.Vector2 randomDestPos = this.GetRandomNoObsPos(groupData.DestPosX, groupData.DestPosY, position.Radius);
                            /// Vị trí đích
                            int destX = (int) randomDestPos.x;
                            int destY = (int) randomDestPos.y;

                            /// Mức máu
                            int hp = siegeInfo.BaseHP + siegeInfo.HPIncreaseEachLevel * this.level;

                            /// Tạo quái thêm mã màu để dễ nhận biết phe địch với phe mình
                            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                            {
                                MapCode = this.Map.MapCode,
                                ResID = siegeInfo.ID,
                                PosX = posX,
                                PosY = posY,
                                Name = "<color=red>" + siegeInfo.Name + "</color>",
                                Title = siegeInfo.Title,
                                MaxHP = hp,
                                MonsterType = MonsterAIType.Special_Normal,
                                Level = this.level,
                                AIID = siegeInfo.AIScriptID,
                                Tag = "Siege",
                                Camp = FengHuoLianCheng_Script_Main.AttackerCampID,
                            });

                            /// Nếu có kỹ năng
                            if (siegeInfo.Skills.Count > 0)
                            {
                                /// Duyệt danh sách kỹ năng
                                foreach (SkillLevelRef skill in siegeInfo.Skills)
                                {
                                    /// Thêm vào danh sách kỹ năng dùng của quái
                                    monster.CustomAISkills.Add(skill);
                                }
                            }

                            /// Nếu có vòng sáng
                            if (siegeInfo.Auras.Count > 0)
                            {
                                /// Duyệt danh sách vòng sáng
                                foreach (SkillLevelRef aura in siegeInfo.Auras)
                                {
                                    /// Kích hoạt vòng sáng
                                    monster.UseSkill(aura.SkillID, aura.Level, monster);
                                }
                            }

                            /// Sử dụng thuật toán A* tìm đường
                            monster.UseAStarPathFinder = true;
                            /// Thực hiện di chuyển đến vị trí tương ứng
                            monster.MoveTo(new System.Windows.Point(destX, destY), true);

                            /// Đánh dấu đã đến đích chưa
                            bool destinationReached = false;
                            /// Sự kiện Tick
                            monster.OnTick = () =>
                            {
                                /// Nếu đã đến đích
                                if (destinationReached)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Cập nhật vị trí đứng mới
                                monster.StartPos = monster.CurrentPos;
                                /// Nếu trong 2s chịu sát thương
                                if (KTGlobal.GetCurrentTimeMilis() - monster.LastBeHitTicks < 2000)
                                {
                                    /// Bỏ qua
                                    return;
                                }

                                /// Đã đến đích
                                if (KTGlobal.GetDistanceBetweenPoints(new System.Windows.Point(destX, destY), monster.CurrentPos) <= 10)
                                {
                                    /// Đánh dấu đã đến đích
                                    destinationReached = true;
                                    /// Bỏ qua
                                    return;
                                }

                                /// Nếu không có người chơi xung quanh và đang không di chuyển
                                if (!monster.IsChasingTarget && !monster.IsMoving)
                                {
                                    /// Thực hiện di chuyển đến vị trí tương ứng
                                    monster.MoveTo(new System.Windows.Point(destX, destY), true);
                                }
                            };
                        });

                        /// Tăng thời gian Delay
                        delayTime += roundData.DelaySpawn;
                    }
                }
            }
        }

        /// <summary>
        /// Tạo cổng dịch chuyển
        /// </summary>
        private void CreateTeleports()
        {
            /// Duyệt danh sách cổng dịch chuyển
            foreach (FengHuoLianCheng.FHLCData.TeleportInfo teleportInfo in FengHuoLianCheng.Data.Teleports)
            {
                KDynamicArea teleport = KTDynamicAreaManager.Add(this.Map.MapCode, -1, teleportInfo.Name, 10102, teleportInfo.PosX, teleportInfo.PosY, -1, 2000, 100, -1, null);
                teleport.OnEnter = (obj) => {
                    if (obj is KPlayer player)
                    {
                        /// Thực hiện chuyển vị trí
                        KTPlayerManager.ChangePos(player, teleportInfo.ToPosX, teleportInfo.ToPosY);
                    }
                };
            }
        }

        /// <summary>
        /// Tạo NPC
        /// </summary>
        private void CreateNPCs()
        {
            /// Duyệt danh sách NPC
            foreach (FengHuoLianCheng.FHLCData.NPCInfo npcInfo in FengHuoLianCheng.Data.NPCs)
            {
                /// Tạo NPC
                KTNPCManager.Create(new KTNPCManager.NPCBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = npcInfo.ID,
                    PosX = npcInfo.PosX,
                    PosY = npcInfo.PosY,
                    Name = npcInfo.Name,
                    Title = npcInfo.Title,
                    ScriptID = npcInfo.ScriptID,
                });
            }
        }

        /// <summary>
        /// Tạo phe thủ
        /// </summary>
        private void CreateDefenders()
        {
            /// Duyệt danh sách quái
            foreach (FengHuoLianCheng.FHLCData.MonsterInfo monsterInfo in FengHuoLianCheng.Data.Defender.Monsters)
            {
                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
                /// Vị trí
                int posX = monsterInfo.PosX;
                int posY = monsterInfo.PosY;
                /// Mức máu
                int hp = monsterInfo.BaseHP + monsterInfo.HPIncreaseEachLevel * this.level;

                /// Tạo quái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = monsterInfo.ID,
                    PosX = posX,
                    PosY = posY,
                    Name = monsterInfo.Name,
                    Title = monsterInfo.Title,
                    MaxHP = hp,
                    MonsterType = MonsterAIType.Special_Normal,
                    Level = this.level,
                    AIID = monsterInfo.AIScriptID,
                    Camp = FengHuoLianCheng_Script_Main.DefenderCampID,
                    RespawnTick = monsterInfo.RespawnTicks,
                });
                monster.DynamicRespawnPredicate = () =>
                {
                    /// Nếu hoạt động đã bị hủy thì thôi
                    return !this.isDisposed;
                };

                /// Nếu có kỹ năng
                if (monsterInfo.Skills.Count > 0)
                {
                    /// Duyệt danh sách kỹ năng
                    foreach (SkillLevelRef skill in monsterInfo.Skills)
                    {
                        /// Thêm vào danh sách kỹ năng dùng của quái
                        monster.CustomAISkills.Add(skill);
                    }
                }

                /// Nếu có vòng sáng
                if (monsterInfo.Auras.Count > 0)
                {
                    /// Duyệt danh sách vòng sáng
                    foreach (SkillLevelRef aura in monsterInfo.Auras)
                    {
                        /// Kích hoạt vòng sáng
                        monster.UseSkill(aura.SkillID, aura.Level, monster);
                    }
                }
            }

            /// Duyệt danh sách nguyên soái
            foreach (FengHuoLianCheng.FHLCData.BossInfo marshalInfo in FengHuoLianCheng.Data.Defender.Marshals)
            {
                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
                /// Vị trí
                int posX = marshalInfo.PosX;
                int posY = marshalInfo.PosY;
                /// Mức máu
                int hp = marshalInfo.BaseHP + marshalInfo.HPIncreaseEachLevel * this.level;

                /// Tạo nguyên soái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = marshalInfo.ID,
                    PosX = posX,
                    PosY = posY,
                    Name = marshalInfo.Name,
                    Title = marshalInfo.Title,
                    MaxHP = hp,
                    MonsterType = MonsterAIType.Special_Boss,
                    Level = this.level,
                    AIID = marshalInfo.AIScriptID,
                    Camp = FengHuoLianCheng_Script_Main.DefenderCampID,
                });
                monster.OnDieCallback = (_) =>
                {
                    /// Thông báo
                    this.NotifyAllPlayers(string.Format("[{0}] đã bị đối phương hạ gục!", marshalInfo.Name));
                };

                /// Nếu có kỹ năng
                if (marshalInfo.Skills.Count > 0)
                {
                    /// Duyệt danh sách kỹ năng
                    foreach (SkillLevelRef skill in marshalInfo.Skills)
                    {
                        /// Thêm vào danh sách kỹ năng dùng của quái
                        monster.CustomAISkills.Add(skill);
                    }
                }

                /// Nếu có vòng sáng
                if (marshalInfo.Auras.Count > 0)
                {
                    /// Duyệt danh sách vòng sáng
                    foreach (SkillLevelRef aura in marshalInfo.Auras)
                    {
                        /// Kích hoạt vòng sáng
                        monster.UseSkill(aura.SkillID, aura.Level, monster);
                    }
                }

                /// Nếu danh sách nguyên soái rỗng
                if (this.marshals == null)
                {
                    /// Tạo mới
                    this.marshals = new List<Monster>();
                }

                /// Thêm nguyên soái
                this.marshals.Add(monster);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về xếp hạng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public FHLCScoreboardData GetScoreboard(KPlayer player)
        {
            /// Nếu sự kiện đã bị hủy
            if (this.isDisposed)
            {
                /// Toác
                return null;
            }
            /// Nếu không có trong bảng xếp hạng
            else if (!this.scoreboard.ContainsKey(player.RoleID))
            {
                /// Toác
                return null;
            }

            /// Tạo mới
            FHLCScoreboardData scoreboardData = new FHLCScoreboardData()
            {
                SelfRank = this.scoreboard[player.RoleID].Rank,
                Records = new List<FHLC_Record>(),
            };

            /// Tạo mới danh sách xếp hạng
            List<PlayerRank> ranks;
            /// Nếu bảng xếp hạng dưới 10
            if (this.scoreboard.Count <= 10)
            {
                /// Lấy hết
                ranks = this.scoreboard.Values.ToList();
            }
            /// Nếu bảng xếp hạng trên 10
            else
            {
                /// Lấy 10 thằng đầu tiên
                ranks = this.scoreboard.Values.Where(x => KTPlayerManager.Find(x.RoleID) != null).Take(10).ToList();
            }

            /// Duyệt danh sách xếp hạng
            foreach (PlayerRank rankInfo in ranks)
            {
                /// Người chơi tương ứng
                KPlayer targetPlayer = KTPlayerManager.Find(rankInfo.RoleID);
                /// Nếu không tìm thấy
                if (targetPlayer == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thêm vào danh sách
                scoreboardData.Records.Add(new FHLC_Record()
                {
                    RoleName = targetPlayer.RoleName,
                    Level = targetPlayer.m_Level,
                    FactionID = targetPlayer.m_cPlayerFaction.GetFactionId(),
                    Rank = rankInfo.Rank,
                    Score = rankInfo.Score,
                });
            }

            /// Trả về kết quả
            return scoreboardData;
        }
        #endregion
    }
}
