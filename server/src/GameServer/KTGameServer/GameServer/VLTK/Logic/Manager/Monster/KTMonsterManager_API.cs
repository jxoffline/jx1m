using GameServer.Logic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Trả về tổng số quái trong bản đồ tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static int GetTotalMonstersAtMap(int mapCode, int copySceneID = -1)
        {
            /// Kết quả
            int totalMonster = 0;

            /// Nếu không có phụ bản
            if (copySceneID == -1)
            {
                /// Nếu tồn tại khu vực
                if (KTMonsterManager.zones.TryGetValue(mapCode, out Dictionary<int, MonsterZone> zones))
                { 
                    /// Danh sách ID khu vực
                    List<int> keys = zones.Keys.ToList();
                    /// Duyệt danh sách
                    foreach (int key in keys)
                    {
                        /// Tăng tổng số
                        totalMonster += zones[key].GetTotalMonsters();
                    }
                }
            }

            /// Nếu tồn tại danh sách quái di động bản đồ tương ứng
            if (KTMonsterManager.dynamicMonsters.TryGetValue(mapCode, out ConcurrentDictionary<int, Monster> monsters))
            {
                /// Danh sách quái theo ID
                List<int> keys = monsters.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!monsters.TryGetValue(key, out Monster monster))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu cùng phụ bản
                    if (monster.CurrentCopyMapID == copySceneID)
                    {
                        /// Tăng số lượng
                        totalMonster++;
                    }
                }
            }

            /// Trả về kết quả
            return totalMonster;
        }

        /// <summary>
        /// Trả về danh sách quái trong bản đồ tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static List<Monster> GetMonstersAtMap(int mapCode, int copySceneID = -1)
        {
            /// Kết quả
            List<Monster> result = new List<Monster>();

            /// Nếu không có phụ bản
            if (copySceneID == -1)
            {
                /// Nếu tồn tại khu vực
                if (KTMonsterManager.zones.TryGetValue(mapCode, out Dictionary<int, MonsterZone> zones))
                {
                    /// Danh sách ID khu vực
                    List<int> keys = zones.Keys.ToList();
                    /// Duyệt danh sách
                    foreach (int key in keys)
                    {
                        /// Thêm vào
                        result.AddRange(zones[key].GetMonsters());
                    }
                }
            }

            /// Nếu tồn tại danh sách quái di động bản đồ tương ứng
            if (KTMonsterManager.dynamicMonsters.TryGetValue(mapCode, out ConcurrentDictionary<int, Monster> monsters))
            {
                /// Danh sách quái theo ID
                List<int> keys = monsters.Keys.ToList();
                /// Duyệt danh sách
                foreach (int key in keys)
                {
                    /// Nếu không tồn tại
                    if (!monsters.TryGetValue(key, out Monster monster))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu cùng phụ bản
                    if (monster.CurrentCopyMapID == copySceneID)
                    {
                        /// Thêm vào danh sách
                        result.Add(monster);
                    }
                }
            }

            /// Trả về kết quả
            return result;
        }
    }
}
