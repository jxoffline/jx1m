using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        #region Define
        /// <summary>
        /// Khu vực quái cố định trong bản đồ
        /// </summary>
        private class MonsterZone
        {
            #region Properties
            /// <summary>
            /// ID khu vực
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// ID bản đồ
            /// </summary>
            public int MapCode { get; set; }
            
            /// <summary>
            /// Dữ liệu khu vực
            /// </summary>
            public MapMonsterZoneData Data { get; set; }
            #endregion

            #region Private fields
            /// <summary>
            /// Template quái tương ứng
            /// </summary>
            private MonsterTemplateData templateMonster;

            /// <summary>
            /// Danh sách quái trong khu vực
            /// </summary>
            private readonly ConcurrentDictionary<int, Monster> monsters = new ConcurrentDictionary<int, Monster>();

            /// <summary>
            /// Tạm khóa khi có sự kiện quái chết
            /// </summary>
            private readonly object monsterDieLock = new object();

            /// <summary>
            /// Tổng số quái đã bị tiêu diệt
            /// </summary>
            private int totalMonsterBeKilled = 0;

            /// <summary>
            /// Tạm khóa khi có sự thay đổi số lượng tinh anh hoặc thủ lĩnh
            /// </summary>
            private readonly object eliteLock = new object();

            /// <summary>
            /// Tổng số tinh anh hoặc thủ lĩnh đang tồn tại
            /// </summary>
            private int totalEliteAndLeaders = 0;

            /// <summary>
            /// Tạm khóa khi có sự thay đổi số lượng lửa trại hiện có
            /// </summary>
            private readonly object fireCampLock = new object();

            /// <summary>
            /// Tổng số lửa trại hiện có
            /// </summary>
            private int totalFireCamps = 0;
            #endregion

            #region Init
            /// <summary>
            /// Khởi tạo khu vực
            /// </summary>
            public void Initialize()
            {
                /// Thông tin quái
                if (!KTMonsterManager.templateMonsters.TryGetValue(this.Data.Code, out this.templateMonster))
                {
                    /// Không tồn tại thì báo lỗi
                    Console.WriteLine(string.Format("Init monster zone failed. ResID not found. MapCode: {0}, ResID: {1}", this.MapCode, this.Data.Code));
                    LogManager.WriteLog(LogTypes.Error, string.Format("Init monster zone failed. ResID not found. MapCode: {0}, ResID: {1}", this.MapCode, this.Data.Code));
                    /// Bỏ qua
                    return;
                }

                /// Làm rỗng danh sách quái
                this.monsters.Clear();

                /// Duyệt tổng số quái sẽ sinh ra
                for (int i = 1; i <= this.Data.MonsterCount; i++)
                {
                    /// Tạo quái tương ứng
                    this.CreateMonster();
                }
            }
            #endregion

            #region Event
            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public void ProcessTick()
            {
                /// Danh sách ID quái
                List<int> keys = this.monsters.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!this.monsters.TryGetValue(key, out Monster monster))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu còn sống
                    if (!monster.IsDead())
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Chưa cập nhật trạng thái
                    if (monster.LastDeadTicks == 0)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu chưa đến thời điểm tái sinh
                    if (KTGlobal.GetCurrentTimeMilis() - monster.LastDeadTicks < this.Data.RespawnTime * 1000)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thực hiện tái sinh quái vật
                    this.MonsterRelive(monster);
                }
            }

            /// <summary>
            /// Sự kiện khi quái vật chết
            /// </summary>
            /// <param name="monster"></param>
            public void OnMonsterDie(Monster monster)
            {
                /// Nếu không phải quái thường
                if (monster.MonsterType == MonsterAIType.Boss || monster.MonsterType == MonsterAIType.Static || monster.MonsterType == MonsterAIType.Special_Boss || monster.MonsterType == MonsterAIType.Elite)
                {
                    /// Bỏ qua
                    return;
                }

                /// Có thể tạo tinh anh không
                bool canSpawnElite = false;
                /// Có thể tạo thủ lĩnh không
                bool canSpawnLeader = false;

                lock (this.monsterDieLock)
                {
                    /// Tăng tổng số quái đã bị tiêu diệt lên
                    this.totalMonsterBeKilled = (this.totalMonsterBeKilled + 1) % 10000007;
                    /// Nếu lớn hơn thiết lập tạo tinh anh
                    if (this.totalMonsterBeKilled > 0 && this.totalMonsterBeKilled % ServerConfig.Instance.MonsterKilledToSpawnElite == 0)
                    {
                        /// Có thể tạo tinh anh
                        canSpawnElite = true;
                    }

                    /// Nếu lớn hơn thiết lập tạo thủ lĩnh
                    if (this.totalMonsterBeKilled > 0 && this.totalMonsterBeKilled % ServerConfig.Instance.MonsterKilledToSpawnLeader == 0)
                    {
                        /// Có thể tạo thủ lĩnh
                        canSpawnLeader = true;
                    }
                }

                //Console.WriteLine("MapCode: {0}, MonsterZoneID: {1}, TotalMonsterKilled: {2}", this.MapCode, this.ID, this.totalMonsterBeKilled);

                /// Nếu có thể tạo thủ lĩnh
                if (canSpawnLeader)
                {
                    /// Tạo thủ lĩnh
                    this.CreateLeader();
                }

                /// Nếu có thể tạo tinh anh
                if (canSpawnElite)
                {
                    /// Tạo tinh anh
                    this.CreateElite();
                }
            }
            #endregion

            #region API
            /// <summary>
            /// Xóa quái khỏi khu vực tương ứng
            /// </summary>
            /// <param name="monster"></param>
            public void Remove(Monster monster)
            {
                /// Xóa
                this.monsters.TryRemove(monster.RoleID, out _);
            }

            /// <summary>
            /// Trả về tổng số quái còn sống trong khu vực
            /// </summary>
            /// <returns></returns>
            public int GetTotalMonsters()
            {
                /// Kết quả
                int totalMonster = 0;

                /// Danh sách ID quái
                List<int> keys = this.monsters.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (this.monsters.TryGetValue(key, out Monster monster))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu còn sống
                    if (!monster.IsDead())
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tăng số lượng
                    totalMonster++;
                }

                /// Trả về kết quả
                return totalMonster;
            }

            /// <summary>
            /// Trả về danh sách quái còn sống trong khu vực
            /// </summary>
            /// <returns></returns>
            public List<Monster> GetMonsters()
            {
                /// Kết quả
                List<Monster> monsters = new List<Monster>();

                /// Danh sách ID quái
                List<int> keys = this.monsters.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (this.monsters.TryGetValue(key, out Monster monster))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu còn sống
                    if (!monster.IsDead())
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    monsters.Add(monster);
                }

                /// Trả về kết quả
                return monsters;
            }
            #endregion

            #region Private methods
            #region Monster manager
            /// <summary>
            /// Tạo quái trong khu vực tương ứng
            /// </summary>
            private void CreateMonster()
            {
                /// Toác
                if (this.templateMonster == null)
                {
                    /// Bỏ qua
                    return;
                }
                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                /// Toác
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Tạo mới quái
                Monster monster = new Monster();
                /// ID tự động
                monster.RoleID = KTMonsterManager.idPool.Take() + (int) ObjectBaseID.Monster;
                /// Bản đồ
                monster.MapCode = this.MapCode;
                /// Tên
                monster.RoleName = this.templateMonster.Name;
                /// Danh hiệu
                monster.Title = this.templateMonster.Title;
                /// ID khu vực
                monster.MonsterZoneID = this.ID;
                /// Template
                monster.MonsterInfo = this.templateMonster;
                /// Sinh lực
                monster.ChangeLifeMax(this.templateMonster.MaxHP, 0, 0);
                monster.m_CurrentLife = this.templateMonster.MaxHP;
                /// Vị trí
                Point position = KTGlobal.GetRandomAroundNoObsPoint(gameMap, new Point(this.Data.PosX, this.Data.PosY), this.Data.Radius, -1, false);
                monster.CurrentPos = position;
                monster.StartPos = monster.CurrentPos;
                monster.InitializePos = monster.CurrentPos;
                monster.ToPos = monster.CurrentPos;
                /// Tốc chạy
                monster.ChangeRunSpeed(this.templateMonster.MoveSpeed, 0, 0);
                /// Loại quái
                monster.MonsterType = this.templateMonster.MonsterType;
                /// Cấp độ
                monster.m_Level = this.templateMonster.Level;
                /// Camp
                monster.Camp = this.templateMonster.Camp;
                /// Tốc đánh
                monster.ChangeAttackSpeed(this.templateMonster.AtkSpeed, 0);
                monster.ChangeCastSpeed(this.templateMonster.AtkSpeed, 0);
                /// Chính xác
                monster.ChangeAttackRating(MonsterUtilities.GetHitByLevel(this.templateMonster.Level), 0, 0);
                /// Né tránh
                monster.ChangeDefend(MonsterUtilities.GetDodgeByLevel(this.templateMonster.Level), 0, 0);
                /// Vật công
                monster.SetPhysicsDamage(MonsterUtilities.GetMinDamageByLevel(this.templateMonster.Level), MonsterUtilities.GetMaxDamageByLevel(this.templateMonster.Level));
                /// AI
                monster.AIID = this.templateMonster.AIID;
                /// Tầm nhìn mặc định
                monster.SeekRange = 600;
                /// Phục hồi sinh lực mỗi 5s
                monster.m_CurrentLifeReplenish = MonsterUtilities.GetLifeReplenishByLevel(this.templateMonster.Level);

                /// Ngũ hành
                if (this.templateMonster.Series <= KE_SERIES_TYPE.series_none || this.templateMonster.Series >= KE_SERIES_TYPE.series_num)
                {
                    int randomValue = KTGlobal.GetRandomNumber((int) KE_SERIES_TYPE.series_none + 1, (int) KE_SERIES_TYPE.series_num - 1);
                    monster.m_Series = (KE_SERIES_TYPE) randomValue;
                }
                else
                {
                    monster.m_Series = this.templateMonster.Series;
                }

                /// Sát thương ngũ hành - ngoại
                monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_physics, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_physics, monster.m_Series));
                monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_cold, monster.m_Series));
                monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_fire, monster.m_Series));
                monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_light, monster.m_Series));
                monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_poison, monster.m_Series));
                /// Sát thương ngũ hành - nội
                monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_magic, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_magic, monster.m_Series));
                monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_cold, monster.m_Series));
                monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_fire, monster.m_Series));
                monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_light, monster.m_Series));
                monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesDamageByLevel(this.templateMonster.Level, DAMAGE_TYPE.damage_poison, monster.m_Series));

                /// Kháng
                monster.SetResist(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesResistanceByLevel(this.templateMonster.Level));
                monster.SetResist(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesResistanceByLevel(this.templateMonster.Level));
                monster.SetResist(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesResistanceByLevel(this.templateMonster.Level));
                monster.SetResist(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesResistanceByLevel(this.templateMonster.Level));
                monster.SetResist(DAMAGE_TYPE.damage_physics, MonsterUtilities.GetSeriesResistanceByLevel(this.templateMonster.Level));

                /// Hướng quay
                monster.CurrentDir = (KiemThe.Entities.Direction) KTGlobal.GetRandomNumber(0, 7);

                /// Thêm vào danh sách
                this.monsters[monster.RoleID] = monster;

                /// Thêm vào danh sách quản lý tổng
                KTMonsterManager.Add(monster);

                /// Gọi hàm khởi động
                monster.Awake();

                /// Cập nhật vị trí
                gameMap.Grid.MoveObject((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y, monster);
            }

            /// <summary>
            /// Thực hiện tái sinh quái vật
            /// </summary>
            /// <param name="monster"></param>
            private void MonsterRelive(Monster monster)
            {
                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                /// Toác
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Làm mới ngũ hành
                if (monster.MonsterInfo.Series <= KE_SERIES_TYPE.series_none || monster.MonsterInfo.Series >= KE_SERIES_TYPE.series_num)
                {
                    monster.m_Series = (KE_SERIES_TYPE) KTGlobal.GetRandomNumber((int) KE_SERIES_TYPE.series_none + 1, (int) KE_SERIES_TYPE.series_num - 1);
                }
                else
                {
                    monster.m_Series = monster.MonsterInfo.Series;
                }

                /// Máu
                monster.m_CurrentLife = monster.MonsterInfo.MaxHP;
                /// Thực hiện động tác đứng
                monster.m_eDoing = KE_NPC_DOING.do_stand;

                /// Vị trí
                Point position = KTGlobal.GetRandomAroundNoObsPoint(gameMap, new Point(this.Data.PosX, this.Data.PosY), this.Data.Radius, -1, false);
                monster.CurrentPos = position;
                monster.StartPos = monster.CurrentPos;

                /// Hướng quay
                monster.CurrentDir = (KiemThe.Entities.Direction) KTGlobal.GetRandomNumber(0, 7);

                /// Thực thi sự kiện tái sinh
                monster.OnRelive();

                /// Thông báo quái tái sinh cho người chơi xung quanh
                KTMonsterManager.NotifyMonsterRelive(monster);
            }
            #endregion

            #region Firecamp
            /// <summary>
            /// Tạo đống củi với chủ nhân tương ứng
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="posX"></param>
            /// <param name="posY"></param>
            private void CreateFireWood(KPlayer owner, int posX, int posY)
            {
                /// Tăng số lượng đống củi đã sinh ra
                lock (this.fireCampLock)
                {
                    this.totalFireCamps++;
                }

                /// Tạo đống củi
                FireCampManager.CreateFireWood(owner, this.MapCode, -1, posX, posY, () =>
                {
                    /// Giảm số lượng đống củi đã sinh ra
                    lock (this.fireCampLock)
                    {
                        this.totalFireCamps--;
                    }
                });
            }
            #endregion

            #region Elite manager
            /// <summary>
            /// Sự kiện khi tinh anh hay thủ lĩnh bị giết
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="killer"></param>
            private void OnEliteOrLeaderBeKilled(Monster obj, GameObject killer)
            {
                /// Giảm số lượng tinh anh và thủ lĩnh hiện có
                lock (this.eliteLock)
                {
                    this.totalEliteAndLeaders--;
                }

                /// Người chơi giết
                KPlayer killerPlayer = null;
                /// Nếu thằng giết không phải người chơi
                if (!(killer is KPlayer))
                {
                    /// Nếu là pet
                    if (killer is Pet pet)
                    {
                        /// Lấy thông tin chủ nhân tương ứng
                        killerPlayer = pet.Owner;
                    }
                }
                /// Nếu thằng giết là người chơi
                else
                {
                    /// Lưu nó lại
                    killerPlayer = killer as KPlayer;
                }

                /// Tạo đống củi
                this.CreateFireWood(killerPlayer, (int) obj.CurrentPos.X, (int) obj.CurrentPos.Y);
            }

            /// <summary>
            /// Tạo tinh anh
            /// </summary>
            private void CreateElite()
            {
                //Console.WriteLine("MapCode: {0}, MonsterZoneID: {1}, TotalMonsterKilled: {2}, TotalFireCamp: {3}", this.MapCode, this.ID, this.totalMonsterBeKilled, this.totalFireCamps);

                /// Nếu đang có quá số lượng
                if (this.totalFireCamps + this.totalEliteAndLeaders >= ServerConfig.Instance.MaxFireCampAndElitePerZone)
                {
                    //Console.WriteLine("Can not create elite due to maximum numbers of firecamps and elites leaders");
                    /// Bỏ qua
                    return;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                /// Toác
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Tăng số lượng tinh anh và thủ lĩnh hiện có
                lock (this.eliteLock)
                {
                    this.totalEliteAndLeaders++;
                }

                /// Vị trí xuất hiện
                Point spawnPos = KTGlobal.GetRandomAroundNoObsPoint(gameMap, new Point(this.Data.PosX, this.Data.PosY), this.Data.Radius, -1, false);
                /// Sinh lực
                int hp = this.templateMonster.MaxHP * 80;

                /// Tạo tinh anh
                Monster elite = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.MapCode,
                    ResID = this.templateMonster.Code,
                    PosX = (int) spawnPos.X,
                    PosY = (int) spawnPos.Y,
                    Name = this.templateMonster.Name,
                    Title = "Tinh anh",
                    MaxHP = hp,
                    MonsterType = MonsterAIType.Elite,
                    Camp = 65535,
                });
                /// Sự kiện chết
                elite.OnDieCallback = (attacker) =>
                {
                    this.OnEliteOrLeaderBeKilled(elite, attacker);
                };
            }

            /// <summary>
            /// Tạo thủ lĩnh
            /// </summary>
            private void CreateLeader()
            {
                //Console.WriteLine("MapCode: {0}, MonsterZoneID: {1}, TotalMonsterKilled: {2}, TotalFireCamp: {3}", this.MapCode, this.ID, this.totalMonsterBeKilled, this.totalFireCamps);

                /// Nếu đang có quá số lượng
                if (this.totalFireCamps + this.totalEliteAndLeaders >= ServerConfig.Instance.MaxFireCampAndElitePerZone)
                {
                    //Console.WriteLine("Can not create leader due to maximum numbers of firecamps and elites leaders");
                    /// Bỏ qua
                    return;
                }

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(this.MapCode);
                /// Toác
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Tăng số lượng tinh anh và thủ lĩnh hiện có
                lock (this.eliteLock)
                {
                    this.totalEliteAndLeaders++;
                }

                /// Vị trí xuất hiện
                Point spawnPos = KTGlobal.GetRandomAroundNoObsPoint(gameMap, new Point(this.Data.PosX, this.Data.PosY), this.Data.Radius, -1, false);
                /// Sinh lực
                int hp = this.templateMonster.MaxHP * 150;

                /// Tạo tinh anh
                Monster elite = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = this.MapCode,
                    ResID = this.templateMonster.Code,
                    PosX = (int) spawnPos.X,
                    PosY = (int) spawnPos.Y,
                    Name = this.templateMonster.Name,
                    Title = "Thủ lĩnh",
                    MaxHP = hp,
                    MonsterType = MonsterAIType.Elite,
                    Camp = 65535,
                });
                /// Sự kiện chết
                elite.OnDieCallback = (attacker) =>
                {
                    this.OnEliteOrLeaderBeKilled(elite, attacker);
                };
            }
            #endregion
            #endregion
        }

        /// <summary>
        /// Danh sách khu vực quái theo bản đồ
        /// </summary>
        private static readonly Dictionary<int, Dictionary<int, MonsterZone>> zones = new Dictionary<int, Dictionary<int, MonsterZone>>();
        #endregion

        #region Add
        /// <summary>
        /// Thêm khu vực quái tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="data"></param>
        private static void AddMonsterZone(int mapCode, MapMonsterZoneData data)
        {
            /// Nếu chưa tồn tại danh sách khu vực ở bản đồ này
            if (!KTMonsterManager.zones.ContainsKey(mapCode))
            {
                /// Tạo mới
                KTMonsterManager.zones[mapCode] = new Dictionary<int, MonsterZone>();
            }

            /// ID khu vực
            int zoneID = KTMonsterManager.zones[mapCode].Count + 1;

            /// Tạo khu vực
            MonsterZone monsterZone = new MonsterZone()
            {
                ID = zoneID,
                MapCode = mapCode,
                Data = data,
            };
            /// Khởi tạo
            monsterZone.Initialize();
            /// Thêm khu vực tương ứng vào danh sách quản lý
            KTMonsterManager.zones[mapCode][monsterZone.ID] = monsterZone;
        }
        #endregion

        #region Event
        /// <summary>
        /// Tick quản lý khu vực quái
        /// </summary>
        private static void MonsterZone_Tick()
        {
            /// Duyệt danh sách bản đồ
            foreach (int mapCode in KTMonsterManager.zones.Keys.ToList())
            {
                /// Duyệt danh sách ID khu vực
                foreach (int zoneID in KTMonsterManager.zones[mapCode].Keys.ToList())
                {
                    /// Nếu không tồn tại
                    if (!KTMonsterManager.zones[mapCode].TryGetValue(zoneID, out MonsterZone monsterZone))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    try
                    {
                        /// Thực thi sự kiện Tick
                        monsterZone.ProcessTick();
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }
                }
            }
        }
        #endregion
    }
}
