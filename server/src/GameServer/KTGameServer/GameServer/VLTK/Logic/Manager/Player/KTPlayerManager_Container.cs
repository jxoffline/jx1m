using GameServer.Logic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Quản lý người chơi theo bản đồ
        /// </summary>
        public static class PlayerContainer
        {
            /// <summary>
            /// Danh sách người chơi theo bản đồ
            /// </summary>
            private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, KPlayer>> playersByMaps = new ConcurrentDictionary<int, ConcurrentDictionary<int, KPlayer>>();

            /// <summary>
            /// Khởi tạo với bản đồ tương ứng
            /// </summary>
            /// <param name="mapCode"></param>
            public static void Init(int mapCode)
            {
                PlayerContainer.playersByMaps[mapCode] = new ConcurrentDictionary<int, KPlayer>();
            }

            /// <summary>
            /// Thêm người chơi vào bản đồ tương ứng
            /// </summary>
            /// <param name="player"></param>
            public static void Add(KPlayer player)
            {
                /// Nếu bản đồ tương ứng không tồn tại
                if (!PlayerContainer.playersByMaps.ContainsKey(player.CurrentMapCode))
                {
                    /// Bỏ qua
                    return;
                }

                /// Thêm vào danh sách
                PlayerContainer.playersByMaps[player.CurrentMapCode][player.RoleID] = player;

                /// Thông tin bản đồ
                GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);
                /// Nếu không tồn tại
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Thêm vào bản đồ
                gameMap.Grid.MoveObject(player.PosX, player.PosY, player);
            }

            /// <summary>
            /// Xóa người chơi khỏi bản đồ tương ứng
            /// </summary>
            /// <param name="player"></param>
            public static void Remove(KPlayer player)
            {
                /// Nếu bản đồ tương ứng không tồn tại
                if (!PlayerContainer.playersByMaps.ContainsKey(player.CurrentMapCode))
                {
                    /// Bỏ qua
                    return;
                }

                /// Xóa khỏi danh sách
                PlayerContainer.playersByMaps[player.CurrentMapCode].TryRemove(player.RoleID, out _);

                /// Thông tin bản đồ
                GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);
                /// Nếu không tồn tại
                if (gameMap == null)
                {
                    /// Bỏ qua
                    return;
                }

                /// Xóa khỏi bản đồ
                gameMap.Grid.RemoveObject(player);

                /// Xóa Dic tầm nhìn của client
                player.ClearVisibleObjects(true);
            }

            /// <summary>
            /// Trả về danh sách người chơi trong bản đồ tương ứng
            /// </summary>
            /// <param name="mapCode"></param>
            /// <param name="copySceneID"></param>
            /// <returns></returns>
            public static List<KPlayer> GetPlayers(int mapCode, int copySceneID = -1)
            {
                /// Nếu không tồn tại bản đồ tương ứng
                if (!PlayerContainer.playersByMaps.TryGetValue(mapCode, out ConcurrentDictionary<int, KPlayer> playersDict))
                {
                    return new List<KPlayer>();
                }

                /// Toác
                if (playersDict == null)
                {
                    return new List<KPlayer>();
                }

                /// Kết quả
                List<KPlayer> players = new List<KPlayer>();
                /// Danh sách theo ID
                List<int> playerIDs = playersDict.Keys.ToList();
                /// Duyệt danh sách
                foreach (int playerID in playerIDs)
                {
                    /// Nếu không tồn tại
                    if (!playersDict.TryGetValue(playerID, out KPlayer player))
                    {
                        continue;
                    }

                    /// Nếu không kiểm tra phụ bản hoặc ở trong phụ bản tương ứng
                    if (copySceneID == -1 || copySceneID == player.CurrentCopyMapID)
                    {
                        /// Thêm vào danh sách
                        players.Add(player);
                    }
                }

                /// Trả về kết quả
                return players;
            }

            /// <summary>
            /// Trả về tổng số người chơi trong bản đồ tương ứng
            /// </summary>
            /// <param name="mapCode"></param>
            /// <param name="copySceneID"></param>
            /// <returns></returns>
            public static int GetTotalPlayers(int mapCode, int copySceneID = -1)
            {
                /// Nếu không tồn tại bản đồ tương ứng
                if (!PlayerContainer.playersByMaps.TryGetValue(mapCode, out ConcurrentDictionary<int, KPlayer> playersDict))
                {
                    return 0;
                }

                /// Toác
                if (playersDict == null)
                {
                    return 0;
                }

                /// Tổng số người chơi thỏa mãn
                int count = 0;
                /// Nếu có kiểm tra phụ bản
                if (copySceneID != -1)
                {
                    /// Duyệt danh sách người chơi trong bản đồ
                    List<int> playerIDs = playersDict.Keys.ToList();
                    foreach (int playerID in playerIDs)
                    {
                        /// Nếu không tồn tại
                        if (!playersDict.TryGetValue(playerID, out KPlayer player))
                        {
                            continue;
                        }

                        /// Nếu nằm trong cùng phụ bản
                        if (player.CurrentCopyMapID == copySceneID)
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    count = playersDict.Count;
                }

                /// Trả về kết quả
                return count;
            }
        }
    }
}
