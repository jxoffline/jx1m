using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.EmperorTomb
{
    /// <summary>
    /// Script chính thực thi sự kiện Tần Lăng
    /// </summary>
    public class EmperorTomb_Script_Main : GameMapEvent
    {
        #region Define
        /// <summary>
        /// Thông tin tầng
        /// </summary>
        private readonly EmperorTomb.EventDetail.Stage stageInfo;

        /// <summary>
        /// Danh sách Boss đã được tạo ra
        /// </summary>
        private readonly HashSet<string> createdBosses;

        /// <summary>
        /// Thời điểm tiếp theo tạo MiniBoss tương ứng
        /// </summary>
        private readonly ConcurrentDictionary<EmperorTomb.EventDetail.Stage.MiniBossInfo, long> nextCreateMiniBosses;

        /// <summary>
        /// Thời điểm Tick lần trước
        /// </summary>
        private long lastTicks = 0;
        #endregion

        #region Core GameMapEvent
        /// <summary>
        /// Script chính thực thi sự kiện Tần Lăng
        /// </summary>
        /// <param name="map"></param>
        /// <param name="activity"></param>
        public EmperorTomb_Script_Main(GameMap map, KTActivity activity) : base(map, activity)
        {
            this.stageInfo = EmperorTomb.Event.Stages[map.MapCode];
            this.createdBosses = new HashSet<string>();
            this.nextCreateMiniBosses = new ConcurrentDictionary<EmperorTomb.EventDetail.Stage.MiniBossInfo, long>();
        }

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        protected override void OnStart()
        {
            /// Xóa toàn bộ quái
            this.RemoveAllMonsters();
            /// Xóa toàn bộ NPC
            this.RemoveAllNPCs();
            /// Xóa toàn bộ cổng dịch chuyển
            this.RemoveAllDynamicAreas();

            /// Tạo quái
            this.CreateMonsters();
            /// Tạo NPC
            this.CreateNPCs();
            /// Tạo cổng dịch chuyển
            this.CreateTeleports();
        }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastTicks < 5000)
            {
                return;
            }
            /// Đánh dấu thời gian
            this.lastTicks = KTGlobal.GetCurrentTimeMilis();
            /// Tạo Boss ở thời gian tương ứng
            this.SpawnBossInTime();
            /// Tạo MiniBoss
            this.SpawnMiniBossInTime();
        }

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected override void OnClose()
        {
            /// Làm rỗng danh sách Boss đã tạo
            this.createdBosses.Clear();

            /// Duyệt danh sách người chơi
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Đưa ra khỏi Tần Lăng
                EmperorTomb_ActivityScript.KickOut(player);
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ sự kiện
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerEnter(KPlayer player)
        {
            base.OnPlayerEnter(player);

            /// Mở bảng thông báo hoạt động
            this.OpenEventBroadboard(player, (int) GameEvent.EmperorTomb);
            /// Thời gian còn lại ở Tần Lăng
            int totalSecLeft = EmperorTomb_ActivityScript.GetTodayTotalSecLeft(player);
            /// Cập nhật thông tin
            this.UpdateEventDetailsToPlayers("Thời gian còn lại", totalSecLeft, "Tiêu diệt tùy tùng", "Tiêu diệt Tần Hủy Hoàng");
        }

        /// <summary>
        /// Sự kiện khi người chơi rời khỏi bản đồ sự kiện
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            base.OnPlayerLeave(player, toMap);

            /// Toác
            if (toMap == null)
            {
                return;
            }

            /// Nếu bản đồ đích không thuộc Tần Lăng
            if (!EmperorTomb.Event.Stages.ContainsKey(toMap.MapCode))
            {
                /// Đóng bảng thông báo hoạt động
                this.CloseEventBroadboard(player, (int) GameEvent.EmperorTomb);
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi ấn nút hồi sinh
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool OnPlayerClickReliveButton(KPlayer player)
        {
            /// Đưa về khu an toàn tầng 1
            KTPlayerManager.Relive(player, EmperorTomb.Event.Relive.MapID, EmperorTomb.Event.Relive.PosX, EmperorTomb.Event.Relive.PosY, 100, 100, 100);
            /// Trả về True
            return true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tạo quái
        /// </summary>
        private void CreateMonsters()
        {
            /// Duyệt danh sách thiết lập quái
            foreach (EmperorTomb.EventDetail.Stage.MonsterInfo monsterInfo in this.stageInfo.Monsters)
            {
                /// Hướng quay
                Entities.Direction dir = Entities.Direction.NONE;
                /// Mức máu
                int hp = monsterInfo.BaseHP + monsterInfo.HPIncreaseEachLevel * EmperorTomb.Config.MonsterLevel;
                /// Ngũ hành
                KE_SERIES_TYPE series = monsterInfo.Series == -1 ? KE_SERIES_TYPE.series_none : (KE_SERIES_TYPE) monsterInfo.Series;

                /// Tạo quái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = monsterInfo.ID,
                    PosX = monsterInfo.PosX,
                    PosY = monsterInfo.PosY,
                    Name = monsterInfo.Name,
                    Title = monsterInfo.Title,
                    MaxHP = hp,
                    Level = EmperorTomb.Config.MonsterLevel,
                    MonsterType = monsterInfo.AIType,
                    AIID = monsterInfo.AIScriptID,
                    Camp = 65535,
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
        }

        /// <summary>
        /// Tạo Boss tương ứng
        /// </summary>
        /// <param name="bossInfo"></param>
        /// <param name="onDie"></param>
        /// <param name="onTick"></param>
        /// <param name="onTakeDamage"></param>
        private void CreateBoss(EmperorTomb.EventDetail.Stage.BossInfo bossInfo, Action<GameObject> onDie, Action<GameObject> onTick)
        {
            /// Hướng quay
            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;
            /// Mức máu
            int hp = bossInfo.BaseHP + bossInfo.HPIncreaseEachLevel * EmperorTomb.Config.MonsterLevel;
            /// Ngũ hành
            KE_SERIES_TYPE series = bossInfo.Series == -1 ? KE_SERIES_TYPE.series_none : (KE_SERIES_TYPE) bossInfo.Series;

            /// Tạo Boss
            Monster boss = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.Map.MapCode,
                ResID = bossInfo.ID,
                PosX = bossInfo.PosX,
                PosY = bossInfo.PosY,
                Name = bossInfo.Name,
                Title = bossInfo.Title,
                MaxHP = hp,
                Level = EmperorTomb.Config.MonsterLevel,
                MonsterType = bossInfo.AIType,
                AIID = bossInfo.AIScriptID,
                Camp = 65535,
                AllowRecordDamage = true,
            });
            boss.OnDieCallback = onDie;

            /// Thông báo
            this.NotifyAllPlayers(string.Format("{0} đã xuất hiện, anh hùng hào kiệt hãy nhanh chân tiêu diệt!", boss.RoleName));
            /// Thực thi sự kiện Tick
            boss.OnTick = () => {
                onTick?.Invoke(boss);
            };

            /// Nếu có kỹ năng
            if (bossInfo.Skills.Count > 0)
            {
                /// Duyệt danh sách kỹ năng
                foreach (SkillLevelRef skill in bossInfo.Skills)
                {
                    /// Thêm vào danh sách kỹ năng dùng của quái
                    boss.CustomAISkills.Add(skill);
                }
            }

            /// Nếu có vòng sáng
            if (bossInfo.Auras.Count > 0)
            {
                /// Duyệt danh sách vòng sáng
                foreach (SkillLevelRef aura in bossInfo.Auras)
                {
                    /// Kích hoạt vòng sáng
                    boss.UseSkill(aura.SkillID, aura.Level, boss);
                }
            }
        }

        /// <summary>
        /// Tạo cổng dịch chuyển tương ứng
        /// </summary>
        private void CreateTeleports()
        {
            /// Duyệt danh sách cổng dịch chuyển
			foreach (EmperorTomb.EventDetail.Stage.TeleportInfo teleportInfo in this.stageInfo.Teleports)
            {
                /// Tạo cổng dịch chuyển
                KDynamicArea teleport = KTDynamicAreaManager.Add(this.Map.MapCode, -1, teleportInfo.Name, 10102, teleportInfo.PosX, teleportInfo.PosY, -1, 2000, 100, -1, null);
                teleport.OnEnter = (obj) => {
                    if (obj is KPlayer player)
                    {
                        this.DoTeleportLogic(teleportInfo, player);
                    }
                };
            }
        }

        /// <summary>
        /// Tạo NPC tương ứng
        /// </summary>
        private void CreateNPCs()
        {
            /// Duyệt danh sách NPC
            foreach (EmperorTomb.EventDetail.Stage.NPCInfo npcInfo in this.stageInfo.NPCs)
            {
                /// Tạo NPC
                KTNPCManager.Create(new KTNPCManager.NPCBuilder()
                {
                    MapCode = this.Map.MapCode,
                    ResID = npcInfo.ID,
                    PosX = npcInfo.PosX,
                    PosY = npcInfo.PosY,
                    ScriptID = npcInfo.ScriptID,
                });
            }
        }

        /// <summary>
        /// Tạo Boss ở thời gian tương ứng
        /// </summary>
        private void SpawnBossInTime()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Giờ
            int hour = now.Hour;
            /// Phút
            int minute = now.Minute;
            /// Ngày
            int day = now.Day;
            /// Tháng
            int month = now.Month;
            /// Năm
            int year = now.Year;
            /// Mã hóa thời gian
            string dateTimeKey = string.Format("{0}_{1}_{2}_{3}_{4}", day, month, year, hour, minute);
            /// Nếu đã tạo Boss ở thời gian này rồi thì thôi
            if (this.createdBosses.Contains(dateTimeKey))
            {
                return;
            }

            /// Danh sách Boss ở thời gian này
            List<EmperorTomb.EventDetail.Stage.BossInfo> bossesList = this.stageInfo.Bosses.Where(x => x.SpawnAt.Hour == hour && x.SpawnAt.Minute == minute).ToList();
            /// Không có
            if (bossesList.Count <= 0)
            {
                return;
            }

            /// Chọn một con Boss ngẫu nhiên
            EmperorTomb.EventDetail.Stage.BossInfo bossInfo = bossesList.RandomRange(1).FirstOrDefault();

            /// Đánh dấu đã tạo Boss ở thời gian này
            this.createdBosses.Add(dateTimeKey);
            /// Tạo Boss tương ứng
            this.CreateBoss(bossInfo, (killer) => {
                /// Nếu không phải người chơi
                if (!(killer is KPlayer killerPlayer))
                {
                    return;
                }

                /// Thực hiện Logic giết Boss
                this.ProcessPlayerKillBossLogic(killerPlayer, bossInfo);
            }, (boss) => {
                /// % máu hiện tại
                int nHPPercent = (int) (boss.m_CurrentLife / (float) boss.m_CurrentLifeMax * 100);
                /// Nếu còn trên 50% máu
                if (nHPPercent >= 50)
                {
                    /// Miễn dịch sát thương ngoại
                    boss.m_ImmuneToPhysicDamage = true;
                    boss.m_ImmuneToMagicDamage = false;
                }
                /// Nếu còn dưới 50% máu
                else
                {
                    /// Miễn dịch sát thương nội
                    boss.m_ImmuneToMagicDamage = true;
                    boss.m_ImmuneToPhysicDamage = false;
                }
            });
        }

        /// <summary>
        /// Tạo MiniBoss ở thời gian tương ứng
        /// </summary>
        private void SpawnMiniBossInTime()
        {
            /// Duyệt danh sách MiniBoss
            foreach (EmperorTomb.EventDetail.Stage.MiniBossInfo miniBossInfo in this.stageInfo.MiniBosses)
            {
                /// Nếu chưa tồn tại trong danh sách
                if (!this.nextCreateMiniBosses.TryGetValue(miniBossInfo, out long nextCreateMiniBossTicks))
                {
                    /// Tạo mới
                    this.nextCreateMiniBosses[miniBossInfo] = KTGlobal.GetCurrentTimeMilis() + KTGlobal.GetRandomNumber(miniBossInfo.MinDuration, miniBossInfo.MaxDuration);
                    /// Bỏ qua
                    continue;
                }

                /// Nếu chưa đánh bại Boss cũ
                if (nextCreateMiniBossTicks == -1)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu chưa đến thời gian
                if (KTGlobal.GetCurrentTimeMilis() < nextCreateMiniBossTicks)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Đánh dấu thời gian tiếp theo tạo MiniBoss
                this.nextCreateMiniBosses[miniBossInfo] = -1;

                /// Chọn Boss ngẫu nhiên
                EmperorTomb.EventDetail.Stage.BossInfo bossInfo = miniBossInfo.Bosses[KTGlobal.GetRandomNumber(0, miniBossInfo.Bosses.Count - 1)];

                /// Toác gì đó
                if (bossInfo == null)
                {
                    /// Đánh dấu thời gian tiếp theo tạo MiniBoss
                    this.nextCreateMiniBosses[miniBossInfo] = KTGlobal.GetCurrentTimeMilis() + KTGlobal.GetRandomNumber(miniBossInfo.MinDuration, miniBossInfo.MaxDuration);
                    /// Bỏ qua
                    continue;
                }

                /// Tạo Boss
                this.CreateBoss(bossInfo, (killer) => {
                    /// Đánh dấu thời gian tiếp theo tạo MiniBoss
                    this.nextCreateMiniBosses[miniBossInfo] = KTGlobal.GetCurrentTimeMilis() + KTGlobal.GetRandomNumber(miniBossInfo.MinDuration, miniBossInfo.MaxDuration);

                    /// Nếu không phải người chơi
                    if (!(killer is KPlayer killerPlayer))
                    {
                        return;
                    }

                    /// Thực hiện Logic giết Boss
                    this.ProcessPlayerKillBossLogic(killerPlayer, bossInfo);
                }, null);
            }
        }

        /// <summary>
        /// Thực hiện Logic cổng dịch chuyển
        /// </summary>
        /// <param name="teleportInfo"></param>
        /// <param name="player"></param>
        private void DoTeleportLogic(EmperorTomb.EventDetail.Stage.TeleportInfo teleportInfo, KPlayer player)
        {
            /// Nếu thông tin bản đồ đến không tồn tại
            if (!EmperorTomb.Event.Stages.TryGetValue(teleportInfo.ToMapID, out EmperorTomb.EventDetail.Stage nextStageInfo))
            {
                return;
            }

            /// Thông tin bản đồ kế tiếp
            GameMap map = KTMapManager.Find(nextStageInfo.MapID);
            /// Toác
            if (map == null)
            {
                KTPlayerManager.ShowNotification(player, "Bản đồ chưa được mở!");
                return;
            }

            /// Dịch chuyển đến tầng tương ứng
            KTPlayerManager.ChangeMap(player, teleportInfo.ToMapID, teleportInfo.ToPosX, teleportInfo.ToPosY);
        }

        /// <summary>
        /// Thực thi Logic người chơi giết Boss
        /// </summary>
        /// <param name="killerPlayer"></param>
        /// <param name="bossInfo"></param>
        private void ProcessPlayerKillBossLogic(KPlayer killerPlayer, EmperorTomb.EventDetail.Stage.BossInfo bossInfo)
        {
            /// Nếu thằng này không có bang và tộc
            if (killerPlayer.GuildID <= 0 && killerPlayer.FamilyID <= 0)
            {
                /// Thông báo
                this.NotifyAllPlayers(string.Format("Người chơi {0} đã đánh bại {1}.", killerPlayer.RoleName, bossInfo.Name));
            }
            else
            {
                /// Thông báo
                this.NotifyAllPlayers(string.Format("Người chơi {0} thuộc bang hội {1} đã đánh bại {2}.", killerPlayer.RoleName, killerPlayer.GuildName, bossInfo.Name));

                /// Danh sách người chơi
                List<KPlayer> players = this.GetPlayers();
                /// Duyệt danh sách người chơi
                foreach (KPlayer player in players)
                {
                    /// Nếu là bản thân
                    if (player == killerPlayer)
                    {
                        continue;
                    }
                    /// Nếu khác bang và tộc
                    else if (player.GuildID != killerPlayer.GuildID && player.FamilyID != killerPlayer.FamilyID)
                    {
                        continue;
                    }
                    /// Nếu khác đội
                    else if (killerPlayer.TeamID == -1 || !KTTeamManager.IsTeamExist(killerPlayer.TeamID) || killerPlayer.TeamID != player.TeamID)
                    {
                        continue;
                    }
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sự kiện Tick người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerTick(KPlayer player)
        {
            /// Nếu chưa chuyển Map xong
            if (player.WaitingForChangeMap)
            {
                return;
            }

            /// Nếu đã đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - player.LastEmperorTombTicks >= 10000)
            {
                /// Mở bảng thông báo hoạt động
                this.OpenEventBroadboard(player, (int) GameEvent.EmperorTomb);
                /// Thời gian còn lại ở Tần Lăng
                int totalSecLeft = EmperorTomb_ActivityScript.GetTodayTotalSecLeft(player);
                /// Cập nhật thông tin
                this.UpdateEventDetailsToPlayers("Thời gian còn lại", totalSecLeft, "Tiêu diệt tùy tùng", "Tiêu diệt Tần Hủy Hoàng");
            }
            /// Đánh dấu thời gian Tick
            player.LastEmperorTombTicks = KTGlobal.GetCurrentTimeMilis();
        }
        #endregion
    }
}
