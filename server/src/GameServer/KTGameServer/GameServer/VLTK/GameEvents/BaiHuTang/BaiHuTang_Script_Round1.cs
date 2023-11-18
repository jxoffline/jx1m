using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.BaiHuTang
{
    /// <summary>
    /// Script Bạch Hổ Đường tầng 1
    /// </summary>
    public class BaiHuTang_Script_Round1 : GameMapEvent
    {
        #region Constants
        /// <summary>
        /// Thời gian mỗi lần thông báo thông tin sự kiện tới người chơi
        /// </summary>
        private const int NotifyActivityInfoToPlayersEveryTick = 5000;

        /// <summary>
        /// Thời gian mỗi lần cập nhật trạng thái PK của người chơi
        /// </summary>
        private const int ChangePlayersPKModeEveryTick = 5000;
        #endregion

        #region Private fields
        /// <summary>
        /// Thời gian lần trước chuyển trạng thái PK của tất cả người chơi
        /// </summary>
        private long LastChangePKModeTick;

        /// <summary>
        /// Thời điểm thông báo cập nhật thông tin sự kiện tới tất cả người chơi lần trước
        /// </summary>
        private long LastNotifyTick;

        /// <summary>
        /// Bước hiện tại của hoạt động
        /// </summary>
        private int nStep;

        /// <summary>
        /// Boss
        /// </summary>
        private Monster boss;
        #endregion

        #region Properties
        /// <summary>
        /// Cấp độ hoạt động
        /// </summary>
        public int Level { get; set; }
        #endregion

        #region Core GameMapEvent
        /// <summary>
        /// Script Bạch Hổ Đường tầng 1
        /// </summary>
        /// <param name="gameMap"></param>
        /// <param name="activity"></param>
        public BaiHuTang_Script_Round1(GameMap gameMap, KTActivity activity) : base(gameMap, activity)
        {

        }

        /// <summary>
        /// Sự kiện chuẩn bị
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
            /// Cập nhật Step của hoạt động
            this.nStep = 0;
            /// Tạo quái
            this.CreateMonsters();
        }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu đã đến thời gian chuyển PKMode
            if (KTGlobal.GetCurrentTimeMilis() - this.LastChangePKModeTick >= BaiHuTang_Script_Round1.ChangePlayersPKModeEveryTick)
			{
                /// Đánh dấu thời gian chuyển PKMode
                this.LastChangePKModeTick = KTGlobal.GetCurrentTimeMilis();
                /// Chuyển PKMode tương ứng
                this.ChangePlayersPKMode();
			}

            /// Nếu ở Step 0
            if (this.nStep == 0)
            {
                /// Nếu đã đến thời gian ra Boss
                if (this.LifeTimeTicks >= BaiHuTang.Round1.Boss.SpawnAfter)
                {
                    /// Chuyển Step
                    this.nStep = 100;
                    /// Tạo Boss
                    this.CreateBoss();
                    /// Đánh dấu thời gian thông báo thông tin sự kiện
                    this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Cập nhật thông tin sự kiện
                    this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", this.DurationTicks - this.LifeTimeTicks, string.Format("Đánh bại <color=yellow>{0}</color>.", "Thủ Lĩnh Sấm Đường Tặc"));
                }
                else
                {
                    /// Nếu đã đến thời gian thông báo thông tin sự kiện
                    if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= BaiHuTang_Script_Round1.NotifyActivityInfoToPlayersEveryTick)
                    {
                        /// Đánh dấu thời gian thông báo thông tin sự kiện
                        this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                        /// Cập nhật thông tin sự kiện
                        this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", BaiHuTang.Round1.Boss.SpawnAfter - this.LifeTimeTicks, string.Format("Đợi <color=yellow>{0}</color> xuất hiện.", "Thủ Lĩnh Sấm Đường Tặc"));
                    }
                }
            }
            /// Nếu ở Step 1
            else if (this.nStep == 1)
            {
                /// Nếu Boss còn sống
                if (this.IsBossAlive())
                {
                    /// Nếu đã đến thời gian thông báo thông tin sự kiện
                    if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= BaiHuTang_Script_Round1.NotifyActivityInfoToPlayersEveryTick)
                    {
                        /// Đánh dấu thời gian thông báo thông tin sự kiện
                        this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                        /// Cập nhật thông tin sự kiện
                        this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", this.DurationTicks - this.LifeTimeTicks, string.Format("Đánh bại <color=yellow>{0}</color>.", "Thủ Lĩnh Sấm Đường Tặc"));
                    }
                    return;
                }
                /// Nếu chưa mở tầng tiếp theo
                else if (BaiHuTang.CurrentStage != 3)
                {
                    /// Nếu đã đến thời gian thông báo thông tin sự kiện
                    if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= BaiHuTang_Script_Round1.NotifyActivityInfoToPlayersEveryTick)
                    {
                        /// Đánh dấu thời gian thông báo thông tin sự kiện
                        this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                        /// Cập nhật thông tin sự kiện
                        this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", this.DurationTicks - this.LifeTimeTicks, string.Format("Đợi khai mở <color=yellow>[{0}]</color>.", "Bạch Hổ Đường - Tầng 2"));
                    }
                    return;
                }

                /// Thông báo mở tầng
                this.NotifyAllPlayers("[Bạch Hổ Đường - Tầng 2] đã mở, hãy nhanh chân!");
                /// Tạo cổng dịch chuyển lên tầng
                this.CreateTeleport();
                /// Chuyển Step
                this.nStep = 100;

                /// Đánh dấu thời gian thông báo thông tin sự kiện
                this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
                /// Cập nhật thông tin sự kiện
                this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", this.DurationTicks - this.LifeTimeTicks, string.Format("Tiến vào <color=yellow>[{0}]</color>.", "Bạch Hổ Đường - Tầng 2"));
            }
        }

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected override void OnClose()
        {
            /// Đưa toàn bộ người chơi rời khỏi
            this.KickOutAllPlayers();
        }

        /// <summary>
        /// Sự kiện người chơi giết quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public override void OnKillObject(KPlayer player, GameObject obj)
        {
            base.OnKillObject(player, obj);

            /// Nếu là Boss
            if (obj == this.boss)
            {
                this.NotifyAllPlayers(string.Format("{0} đã đánh bại {1}!", player.RoleName, obj.RoleName));

                /// Duyệt danh sách người chơi trong bản đồ
                foreach (KPlayer otherPlayer in this.GetPlayers())
				{
                    /// Nếu đã chết
                    if (otherPlayer.IsDead())
                    {
                        continue;
                    }

                    int prestige;
                    int repute;
                    /// Nếu cùng tộc hoặc bang với người chơi giết Boss
                    if ((otherPlayer.GuildID >= 0 && otherPlayer.GuildID == player.GuildID) || (otherPlayer.FamilyID >= 0 && otherPlayer.FamilyID == player.FamilyID))
					{
                        /// Tăng điểm uy danh theo bang hội giết Boss
                        prestige = BaiHuTang.Round1.Repute.GuildKillBossPrestige;
                        /// Tăng điểm danh vọng theo bang hội giết Boss
                        repute = BaiHuTang.Round1.Repute.GuildKillBossRepute;
					}
                    else
					{
                        /// Tăng điểm uy danh qua ải thường
                        prestige = BaiHuTang.Round1.Repute.OtherPrestige;
                        /// Tăng điểm danh vọng qua ải thường
                        repute = BaiHuTang.Round1.Repute.OtherRepute;
                    }

                    /// Thêm uy danh tương ứng
                    otherPlayer.Prestige += prestige;
                    /// Nếu là Bạch Hổ Đường sơ
                    if (this.Level == 80)
					{
                        repute /= 2;
                    }
                    /// Thêm danh vọng tương ứng
                    KTGlobal.AddRepute(otherPlayer, 501, repute);
                    /// Thông báo nhận danh vọng Bạch Hổ Đường
                    KTPlayerManager.ShowNotification(otherPlayer, string.Format("Nhận {0} điểm Uy danh và {1} điểm danh vọng Bạch Hổ Đường!", prestige, repute));
				}
            }
        }

        /// <summary>
        /// Sự kiện người chơi vào bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerEnter(KPlayer player)
        {
            base.OnPlayerEnter(player);

            /// Mở bảng thông báo hoạt động
            this.OpenEventBroadboard(player, (int) GameEvent.BaiHuTang);
            /// Cập nhật thông tin
            this.UpdateEventDetailsToPlayers("Bạch Hổ Đường - Tầng 1", BaiHuTang.Round1.Boss.SpawnAfter, string.Format("Đợi <color=yellow>{0}</color> xuất hiện.", "Thủ Lĩnh Sấm Đường Tặc"));
        }

        /// <summary>
        /// Sự kiện người chơi rời bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            base.OnPlayerLeave(player, toMap);

            /// Đóng bảng thông báo hoạt động
            this.CloseEventBroadboard(player, (int) GameEvent.BaiHuTang);
            /// Chuyển trạng thái PK
            player.PKMode = (int) PKMode.Peace;
            /// Chuyển Camp về -1
            player.Camp = -1;
        }

        /// <summary>
        /// Sự kiện khi người choi ấn nút về thành khi bị trọng thương
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
		public override bool OnPlayerClickReliveButton(KPlayer player)
		{
            base.OnPlayerClickReliveButton(player);

            /// Đưa người chơi về bản đồ báo danh Bạch Hổ Đường
            KTPlayerManager.Relive(player, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutMapID, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutPosX, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutPosY, 100, 100, 100);
            return true;
		}
		#endregion

		#region Private methods
        /// <summary>
        /// Cập nhật toàn bộ trạng thái PK của người chơi
        /// </summary>
        private void ChangePlayersPKMode()
		{
            foreach (KPlayer player in this.GetPlayers())
			{
                player.PKMode = (int) PKMode.Guild;
			}
		}

		/// <summary>
		/// Tạo quái
		/// </summary>
		private void CreateMonsters()
		{
            /// Duyệt danh sách thiết lập quái
            foreach (BaiHuTang.RoundInfo.MonsterInfo monsterInfo in BaiHuTang.Round1.Monsters)
			{
                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
                /// Mức máu
                int hp = monsterInfo.BaseHP + monsterInfo.HPIncreaseEachLevel * this.Level;

                /// Tạo quái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = monsterInfo.IDByActivity[this.Activity.Data.ID],
                    PosX = monsterInfo.PosX,
                    PosY = monsterInfo.PosY,
                    Name = monsterInfo.Name,
                    Title = monsterInfo.Title,
                    MaxHP = hp,
                    Level = this.Level,
                    MonsterType = (MonsterAIType) monsterInfo.AIType,
                    AIID = monsterInfo.AIScriptID,
                    Camp = 65535,
                    RespawnTick = monsterInfo.RespawnTick,
                });
                monster.DynamicRespawnPredicate = () =>
                {
                    /// Nếu hoạt động đã bị hủy thì thôi
                    return !this.isDisposed;
                };
            }
		}

        /// <summary>
        /// Khởi tạo Boss
        /// </summary>
        private void CreateBoss()
        {
            /// Vị trí xuất hiện
            KeyValuePair<int, int> randomPos = BaiHuTang.Round1.Boss.RandomPos[KTGlobal.GetRandomNumber(0, BaiHuTang.Round1.Boss.RandomPos.Count - 1)];
            /// Ngũ hành
            KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;
            /// Hướng quay
            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
            /// Mức máu
            int hp = BaiHuTang.Round1.Boss.BaseHP + BaiHuTang.Round1.Boss.HPIncreaseEachLevel * this.Level;

            /// Tạo Boss
            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.Map.MapCode,
                ResID = BaiHuTang.Round1.Boss.IDByActivity[this.Activity.Data.ID],
                PosX = randomPos.Key,
                PosY = randomPos.Value,
                Name = BaiHuTang.Round1.Boss.Name,
                Title = BaiHuTang.Round1.Boss.Title,
                MaxHP = hp,
                Level = this.Level,
                MonsterType = (MonsterAIType) BaiHuTang.Round1.Boss.AIType,
                AIID = BaiHuTang.Round1.Boss.AIScriptID,
                Camp = 65535,
            });
            /// Thông báo Boss đã xuất hiện
            this.NotifyAllPlayers(string.Format("{0} đã xuất hiện!", monster.RoleName));
            /// Ghi lại Boss
            this.boss = monster;
            /// Chuyển qua Step 1
            this.nStep = 1;
        }

        /// <summary>
        /// Kiểm tra Boss có còn sống không
        /// </summary>
        /// <returns></returns>
        private bool IsBossAlive()
        {
            return this.boss != null && !this.boss.IsDead();
        }

        /// <summary>
        /// Khởi tạo cổng dịch chuyển
        /// </summary>
        private void CreateTeleport()
        {
            KDynamicArea teleport = KTDynamicAreaManager.Add(this.Map.MapCode, -1, BaiHuTang.Round1.Teleport.Name, BaiHuTang.Round1.Teleport.ResID, BaiHuTang.Round1.Teleport.PosX, BaiHuTang.Round1.Teleport.PosY, -1, 2000, BaiHuTang.Round1.Teleport.Radius, -1, null);
            teleport.OnEnter = (obj) => {
                if (obj is KPlayer)
                {
                    this.TeleportMovePlayerToNextStage(obj as KPlayer);
                }
            };
        }

        /// <summary>
        /// Dịch người chơi đến cửa tiếp theo
        /// </summary>
        /// <param name="player"></param>
        private void TeleportMovePlayerToNextStage(KPlayer player)
        {
            UnityEngine.Vector2 randPos = BaiHuTang.RandomTeleportPositions[KTGlobal.GetRandomNumber(0, BaiHuTang.RandomTeleportPositions.Count - 1)];
            int posX = (int) randPos.x;
            int posY = (int) randPos.y;
            KTPlayerManager.ChangeMap(player, BaiHuTang.Round1.NextMaps[this.Map.MapCode].NextMapID, posX, posY);
        }

        /// <summary>
        /// Đưa người chơi ra khỏi bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        private void KickOutPlayer(KPlayer player)
        {
            KTPlayerManager.ChangeMap(player, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutMapID, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutPosX, BaiHuTang.Round1.OutMaps[this.Map.MapCode].OutPosY);
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
    }
}
