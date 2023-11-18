using GameServer.KiemThe.Entities;
using GameServer.Logic;
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
        #region Tìm kiếm
        /// <summary>
        /// Tìm quái có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Monster Find(int id)
        {
            /// Thông tin quái
            if (KTMonsterManager.monsters.TryGetValue(id, out Monster monster))
            {
                /// Trả về kết quả
                return monster;
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm quái đầu tiên thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Monster Find(Predicate<Monster> predicate)
        {
            /// Danh sách khóa
            List<int> keys = KTMonsterManager.monsters.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTMonsterManager.monsters.TryGetValue(key, out Monster monster))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(monster))
                {
                    /// Trả về kết quả
                    return monster;
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm danh sách quái thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<Monster> FindAll(Predicate<Monster> predicate)
        {
            /// Kết quả
            List<Monster> monsters = new List<Monster>();

            /// Danh sách khóa
            List<int> keys = KTMonsterManager.monsters.Keys.ToList();
            /// Duyệt danh sách
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!KTMonsterManager.monsters.TryGetValue(key, out Monster monster))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(monster))
                {
                    /// Thêm vào danh sách
                    monsters.Add(monster);
                }
            }

            /// Trả về kết quả
            return monsters;
        }
        #endregion

        #region Thêm
        /// <summary>
        /// Thêm quái tương ứng
        /// </summary>
        /// <param name="monster"></param>
        private static void Add(Monster monster)
        {
            /// Nếu đã tồn tại
            if (KTMonsterManager.monsters.TryGetValue(monster.RoleID, out _))
            {
                Console.WriteLine("Duplicate Monster ID: " + (monster.RoleID - (int) ObjectBaseID.Monster));
            }

            /// Thêm vào danh sách tổng
            KTMonsterManager.monsters[monster.RoleID] = monster;
        }
        #endregion

        #region Xóa
        /// <summary>
        /// Xóa quái tương ứng
        /// </summary>
        /// <param name="monster"></param>
        public static void Remove(Monster monster)
        {
            /// Hủy đối tượng
            monster.Dispose();

            /// Xóa khỏi danh sách tổng
            KTMonsterManager.monsters.TryRemove(monster.RoleID, out _);

            /// Trả ID về Pool
            KTMonsterManager.idPool.Return(monster.RoleID - (int) ObjectBaseID.Monster);

            /// Nếu được quản lý bởi khu vực
            if (monster.MonsterZoneID != -1)
            {
                /// Thông tin các khu vực trong bản đồ
                if (KTMonsterManager.zones.TryGetValue(monster.MapCode, out Dictionary<int, MonsterZone> mapZones))
                {
                    /// Thông tin khu vực tương ứng
                    if (mapZones.TryGetValue(monster.MonsterZoneID, out MonsterZone monsterZone))
                    {
                        monsterZone.Remove(monster);
                    }
                }
            }
            /// Nếu không được quản lý bởi khu vực tức là quái di động
            else
            {
                /// Thông tin danh sách quái di động trong bản đồ
                if (KTMonsterManager.dynamicMonsters.TryGetValue(monster.MapCode, out ConcurrentDictionary<int, Monster> monsters))
                {
                    /// Xóa khỏi danh sách
                    monsters.TryRemove(monster.RoleID, out _);
                }

                /// Thông tin danh sách quái di động đang chờ tái sinh trong bản đồ
                if (KTMonsterManager.waitRespawnDynamicMonsters.TryGetValue(monster.MapCode, out ConcurrentDictionary<int, Monster> awaitMonsters))
                {
                    /// Xóa khỏi danh sách
                    awaitMonsters.TryRemove(monster.RoleID, out _);
                }
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(monster.MapCode);
            /// Toác
            if (gameMap == null)
            {
                /// Bỏ qua
                return;
            }

            /// Xóa khỏi MapGrid
            gameMap.Grid.RemoveObject(monster);
        }

        /// <summary>
        /// Xóa quái đầu tiên thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        public static void Remove(Predicate<Monster> predicate)
        {
            /// Thông tin quái
            Monster monster = KTMonsterManager.Find(predicate);
            /// Nếu không tìm thấy
            if (monster == null)
            {
                /// Bỏ qua
                return;
            }
            /// Xóa quái tương ứng
            KTMonsterManager.Remove(monster);
        }

        /// <summary>
        /// Xóa toàn bộ quái thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        public static void RemoveAll(Predicate<Monster> predicate)
        {
            /// Danh sách quái thỏa mãn
            List<Monster> monsters = KTMonsterManager.FindAll(predicate);
            /// Duyệt danh sách
            foreach (Monster monster in monsters)
            {
                /// Xóa quái này
                KTMonsterManager.Remove(monster);
            }
        }
        #endregion
    }
}
