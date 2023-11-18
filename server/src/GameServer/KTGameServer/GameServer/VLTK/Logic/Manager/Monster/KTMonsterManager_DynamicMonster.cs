using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Danh sách quái di động (không quản lý bởi khu vực)
        /// </summary>
        private static readonly Dictionary<int, ConcurrentDictionary<int, Monster>> dynamicMonsters = new Dictionary<int, ConcurrentDictionary<int, Monster>>();

        /// <summary>
        /// Danh sách quái di động (không quản lý bởi khu vực) đang chờ tái sinh
        /// </summary>
        private static readonly Dictionary<int, ConcurrentDictionary<int, Monster>> waitRespawnDynamicMonsters = new Dictionary<int, ConcurrentDictionary<int, Monster>>();

        #region Tạo mới

        /// <summary>
        /// Tạo quái di động (không quản lý bởi khu vực) tương ứng
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Monster Create(DynamicMonsterBuilder builder)
        {
            /// Thông tin quái tương ứng
            if (!KTMonsterManager.templateMonsters.TryGetValue(builder.ResID, out MonsterTemplateData templateData))
            {
                /// Không tồn tại thì bỏ qua
                LogManager.WriteLog(LogTypes.Error, string.Format("Create dynamic monster failed. ResID {0} does not exist.", builder.ResID));
                return null;
            }

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(builder.MapCode);
            /// Nếu bản đồ không tồn tại
            if (gameMap == null)
            {
                /// Không tồn tại thì bỏ qua
                LogManager.WriteLog(LogTypes.Error, string.Format("Create dynamic monster failed. MapID {0} does not exist.", builder.MapCode));
                return null;
            }

            /// Nếu danh sách ở bản đồ này chưa tồn tại
            if (!KTMonsterManager.dynamicMonsters.ContainsKey(builder.MapCode))
            {
                /// Tạo mới
                KTMonsterManager.dynamicMonsters[builder.MapCode] = new ConcurrentDictionary<int, Monster>();
            }

            /// Tạo mới quái
            Monster monster = new Monster();
            /// ID tự động
            monster.RoleID = KTMonsterManager.idPool.Take() + (int)ObjectBaseID.Monster;
            /// Bản đồ
            monster.MapCode = builder.MapCode;
            /// Phụ bản
            monster.CurrentCopyMapID = builder.CopySceneID;
            /// Tên
            if (string.IsNullOrEmpty(builder.Name))
            {
                monster.RoleName = templateData.Name;
            }
            else
            {
                monster.RoleName = builder.Name;
            }
            /// Danh hiệu
            if (string.IsNullOrEmpty(builder.Title))
            {
                monster.Title = templateData.Title;
            }
            else
            {
                monster.Title = builder.Title;
            }
            /// Template
            monster.MonsterInfo = templateData;
            /// Sinh lực
            if (builder.MaxHP == -1)
            {
                //Bú cái này vào để nếu set test thì máu là 1
                if (KTGlobal.IsTestModel)
                {
                    monster.ChangeLifeMax(1, 0, 0);
                    monster.m_CurrentLife = 1;
                }
                monster.ChangeLifeMax(templateData.MaxHP, 0, 0);
                monster.m_CurrentLife = templateData.MaxHP;
            }
            else
            {
                //Bú cái này vào để nếu set test thì máu là 1
                if (KTGlobal.IsTestModel)
                {
                    monster.ChangeLifeMax(1, 0, 0);
                    monster.m_CurrentLife = 1;
                }
                monster.ChangeLifeMax(builder.MaxHP, 0, 0);
                monster.m_CurrentLife = builder.MaxHP;
            }
            /// Vị trí
            if (KTGlobal.InObs(builder.MapCode, builder.PosX, builder.PosY, builder.CopySceneID))
            {
                /// Tìm 1 điểm khác xung quanh vì vị trí thiết lập nằm trong điểm Block
                monster.CurrentPos = KTGlobal.GetRandomAroundNoObsPoint(gameMap, new System.Windows.Point(builder.PosX, builder.PosY), builder.CopySceneID);
            }
            else
            {
                /// Lấy vị trí thiết lập
                monster.CurrentPos = new System.Windows.Point(builder.PosX, builder.PosY);
            }
            monster.StartPos = monster.CurrentPos;
            monster.InitializePos = monster.CurrentPos;
            monster.ToPos = monster.CurrentPos;
            /// Tốc chạy
            monster.ChangeRunSpeed(templateData.MoveSpeed, 0, 0);
            /// Loại quái
            monster.MonsterType = builder.MonsterType;
            /// Cấp độ
            if (builder.Level == -1)
            {
                monster.m_Level = templateData.Level;
            }
            else
            {
                monster.m_Level = builder.Level;
            }
            /// Camp
            monster.Camp = builder.Camp;
            /// Tốc đánh
            monster.ChangeAttackSpeed(templateData.AtkSpeed, 0);
            monster.ChangeCastSpeed(templateData.AtkSpeed, 0);
            /// Chính xác
            monster.ChangeAttackRating(MonsterUtilities.GetHitByLevel(templateData.Level), 0, 0);
            /// Né tránh
            monster.ChangeDefend(MonsterUtilities.GetDodgeByLevel(templateData.Level), 0, 0);
            /// Vật công
            monster.SetPhysicsDamage(MonsterUtilities.GetMinDamageByLevel(monster.m_Level), MonsterUtilities.GetMaxDamageByLevel(monster.m_Level));
            /// AI
            if (builder.AIID == -1)
            {
                monster.AIID = templateData.AIID;
            }
            else
            {
                monster.AIID = builder.AIID;
            }
            /// Tầm nhìn mặc định
            monster.SeekRange = 600;
            /// Phục hồi sinh lực mỗi 5s
            monster.m_CurrentLifeReplenish = MonsterUtilities.GetLifeReplenishByLevel(monster.m_Level);
            /// Hướng quay
            if (builder.Direction == Entities.Direction.NONE)
            {
                /// Chọn random
                monster.CurrentDir = (KiemThe.Entities.Direction)KTGlobal.GetRandomNumber(0, 7);
            }
            else
            {
                monster.CurrentDir = builder.Direction;
            }
            /// Tag
            monster.Tag = builder.Tag;

            /// Ngũ hành
            if (builder.Series == KE_SERIES_TYPE.series_none)
            {
                int randomValue = KTGlobal.GetRandomNumber((int)KE_SERIES_TYPE.series_none + 1, (int)KE_SERIES_TYPE.series_num - 1);
                monster.m_Series = (KE_SERIES_TYPE)randomValue;
            }
            else
            {
                monster.m_Series = templateData.Series;
            }

            /// Sát thương ngũ hành - ngoại
            monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_physics, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_physics, monster.m_Series));
            monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_cold, monster.m_Series));
            monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_fire, monster.m_Series));
            monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_light, monster.m_Series));
            monster.SetSeriesDamagePhysics(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_poison, monster.m_Series));
            /// Sát thương ngũ hành - nội
            monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_magic, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_magic, monster.m_Series));
            monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_cold, monster.m_Series));
            monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_fire, monster.m_Series));
            monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_light, monster.m_Series));
            monster.AddSeriesDamageMagics(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesDamageByLevel(monster.m_Level, DAMAGE_TYPE.damage_poison, monster.m_Series));

            /// Kháng
            monster.SetResist(DAMAGE_TYPE.damage_cold, MonsterUtilities.GetSeriesResistanceByLevel(monster.m_Level));
            monster.SetResist(DAMAGE_TYPE.damage_fire, MonsterUtilities.GetSeriesResistanceByLevel(monster.m_Level));
            monster.SetResist(DAMAGE_TYPE.damage_light, MonsterUtilities.GetSeriesResistanceByLevel(monster.m_Level));
            monster.SetResist(DAMAGE_TYPE.damage_poison, MonsterUtilities.GetSeriesResistanceByLevel(monster.m_Level));
            monster.SetResist(DAMAGE_TYPE.damage_physics, MonsterUtilities.GetSeriesResistanceByLevel(monster.m_Level));

            /// Hướng quay
            monster.CurrentDir = (KiemThe.Entities.Direction)KTGlobal.GetRandomNumber(0, 7);

            /// Thời gian tái sinh
            monster.DynamicRespawnTicks = builder.RespawnTick;
            /// Điều kiện tái sinh
            monster.DynamicRespawnPredicate = builder.RespawnPredicate;

            /// Ghi lại lịch sử sát thương
            monster.AllowRecordDamage = builder.AllowRecordDamage;

            /// Thêm vào danh sách
            KTMonsterManager.dynamicMonsters[builder.MapCode][monster.RoleID] = monster;
            /// Thêm vào danh sách quản lý tổng
            KTMonsterManager.Add(monster);

            /// Gọi hàm khởi động
            monster.Awake();

            /// Cập nhật vị trí
            gameMap.Grid.MoveObject((int)monster.CurrentPos.X, (int)monster.CurrentPos.Y, monster);

            /// Trả về kết quả
            return monster;
        }

        #endregion Tạo mới

        #region Tái sinh

        /// <summary>
        /// Thêm quái vào danh sách chờ tái sinh
        /// </summary>
        /// <param name="monster"></param>
        private static void AddDynamicMonsterWaitToRelive(Monster monster)
        {
            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(monster.MapCode);
            /// Nếu bản đồ không tồn tại
            if (gameMap == null)
            {
                /// Không tồn tại thì bỏ qua
                LogManager.WriteLog(LogTypes.Error, string.Format("Append dynamic monster relive failed. MapID {0} does not exist.", monster.MapCode));
                return;
            }

            /// Nếu danh sách ở bản đồ này chưa tồn tại
            if (!KTMonsterManager.waitRespawnDynamicMonsters.ContainsKey(monster.MapCode))
            {
                /// Tạo mới
                KTMonsterManager.waitRespawnDynamicMonsters[monster.MapCode] = new ConcurrentDictionary<int, Monster>();
            }

            /// Thêm vào danh sách chờ tái sinh
            KTMonsterManager.waitRespawnDynamicMonsters[monster.MapCode][monster.RoleID] = monster;
        }

        /// <summary>
        /// Thực hiện tái sinh quái vật
        /// </summary>
        /// <param name="monster"></param>
        private static void DynamicMonsterRelive(Monster monster)
        {
            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(monster.MapCode);
            /// Toác
            if (gameMap == null)
            {
                /// Bỏ qua
                return;
            }

            /// Máu
            monster.m_CurrentLife = monster.m_CurrentLifeMax;

            //Console.WriteLine("DO Monster {0} (ID: {1}) relive with HP = {2}/{3}", monster.RoleName, monster.RoleID, monster.m_CurrentLife, monster.m_CurrentLifeMax);

            /// Thực hiện động tác đứng
            monster.m_eDoing = KE_NPC_DOING.do_stand;

            /// Vị trí
            monster.CurrentPos = monster.InitializePos;
            monster.StartPos = monster.CurrentPos;

            /// Hướng quay
            monster.CurrentDir = (KiemThe.Entities.Direction)KTGlobal.GetRandomNumber(0, 7);

            /// Thực thi sự kiện tái sinh
            monster.OnRelive();

            /// Thông báo quái tái sinh cho người chơi xung quanh
            KTMonsterManager.NotifyMonsterRelive(monster);

            //Console.WriteLine("Notify monster {0} (ID: {1}) relive with HP = {2}/{3}", monster.RoleName, monster.RoleID, monster.m_CurrentLife, monster.m_CurrentLifeMax);
        }

        #endregion Tái sinh

        #region Event

        /// <summary>
        /// Tick quản lý quái di động
        /// </summary>
        private static void DynamicMonster_Tick()
        {
            /// Duyệt danh sách bản đồ có chứa quái tái sinh
            foreach (int mapCode in KTMonsterManager.waitRespawnDynamicMonsters.Keys.ToList())
            {
                /// Danh sách quái theo ID
                List<int> keys = KTMonsterManager.waitRespawnDynamicMonsters[mapCode].Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!KTMonsterManager.waitRespawnDynamicMonsters[mapCode].TryGetValue(key, out Monster monster))
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

                    /// Nếu chưa đến thời điểm tái sinh
                    if (KTGlobal.GetCurrentTimeMilis() - monster.LastDeadTicks < monster.DynamicRespawnTicks)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Xóa khỏi danh sách chờ tái sinh
                    KTMonsterManager.waitRespawnDynamicMonsters[mapCode].TryRemove(monster.RoleID, out _);

                    /// Nếu có phụ bản nhưng phụ bản không tồn tại
                    if (monster.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(monster.CurrentCopyMapID, mapCode))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    try
                    {
                        /// Nếu không thỏa mãn điều kiện tái sinh
                        if (monster.DynamicRespawnPredicate != null && !monster.DynamicRespawnPredicate())
                        {
                            /// Bỏ qua
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                        /// Gặp lỗi cũng bỏ qua luôn
                        continue;
                    }

                    /// Thực hiện tái sinh quái
                    KTMonsterManager.DynamicMonsterRelive(monster);
                }
            }
        }

        #endregion Event
    }
}