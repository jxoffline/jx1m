using GameServer.Logic;
using Server.TCP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Thêm
        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="player"></param>
        public static void Add(KPlayer player)
        {
            /// Thêm vào danh sách tổng
            KTPlayerManager.players[player.RoleID] = player;
            /// Cập nhật ID Socket
            player.ClientSocket.RoleID = player.RoleID;
            /// Thêm vào Timer
            KTPlayerTimerManager.Instance.AddPlayer(player);
            /// Thêm vào bản đồ
            KTPlayerManager.PlayerContainer.Add(player);
        }
        #endregion

        #region Xóa
        /// <summary>
        /// Xóa người chơi khỏi danh sách
        /// </summary>
        /// <param name="player"></param>
        public static void Remove(KPlayer player)
        {
            /// Xóa khỏi danh sách tổng
            KTPlayerManager.players.TryRemove(player.RoleID, out _);
            /// Hủy tham chiếu ID Socket
            player.ClientSocket.RoleID = -1;
            /// Xóa khỏi Timer
            KTPlayerTimerManager.Instance.RemovePlayer(player);
            /// Xóa khỏi bản đồ
            KTPlayerManager.PlayerContainer.Remove(player);
        }
        #endregion

        #region Tìm kiếm
        /// <summary>
        /// Trả về danh sách người chơi
        /// </summary>
        /// <returns></returns>
        public static List<KPlayer> GetAll()
        {
            /// Tạo kết quả
            List<KPlayer> result = new List<KPlayer>();

            /// Danh sách ID
            List<int> roleIDs = KTPlayerManager.players.Keys.ToList();
            /// Duyệt danh sách
            foreach (int roleID in roleIDs)
            {
                /// Nếu không tồn tại
                if (!KTPlayerManager.players.TryGetValue(roleID, out KPlayer player))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thêm vào kết quả
                result.Add(player);
            }

            /// Trả về kết quả
            return result;
        }

        /// <summary>
        /// Tìm người chơi có ID tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public static KPlayer Find(int roleID)
        {
            /// Nếu tồn tại trong danh sách
            if (KTPlayerManager.players.TryGetValue(roleID, out KPlayer player))
            {
                /// Trả về kết quả
                return player;
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm người chơi có tên tương ứng
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public static KPlayer Find(string roleName)
        {
            /// Xóa tiền tố GM ở đầu
            roleName = roleName.Replace("<color=#0486dc>[GM]</color> ", "");

            /// Tìm kiếm
            return KTPlayerManager.Find(x => x.GetRoleData().RoleName == roleName);
        }

        /// <summary>
        /// Tìm người chơi theo Socket tương ứng
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static KPlayer Find(TMSKSocket socket)
        {
            /// Toác
            if (socket == null)
            {
                return null;
            }

            /// Tìm kiếm
            return KTPlayerManager.Find(socket.RoleID);
        }

        /// <summary>
        /// Tìm người chơi đầu tiên trong danh sách thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static KPlayer Find(Predicate<KPlayer> predicate)
        {
            /// Danh sách ID
            List<int> roleIDs = KTPlayerManager.players.Keys.ToList();
            /// Duyệt danh sách
            foreach (int roleID in roleIDs)
            {
                /// Nếu không tồn tại
                if (!KTPlayerManager.players.TryGetValue(roleID, out KPlayer player))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(player))
                {
                    /// Trả về kết quả
                    return player;
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Tìm tất cả người chơi thỏa mãn điều kiện tương ứng
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<KPlayer> FindAll(Predicate<KPlayer> predicate)
        {
            /// Tạo kết quả
            List<KPlayer> result = new List<KPlayer>();

            /// Danh sách ID
            List<int> roleIDs = KTPlayerManager.players.Keys.ToList();
            /// Duyệt danh sách
            foreach (int roleID in roleIDs)
            {
                /// Nếu không tồn tại
                if (!KTPlayerManager.players.TryGetValue(roleID, out KPlayer player))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu thỏa mãn điều kiện
                if (predicate(player))
                {
                    /// Thêm vào kết quả
                    result.Add(player);
                }
            }

            /// Trả về kết quả
            return result;
        }

        /// <summary>
        /// Trả về danh sách người chơi trong bản đồ tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static List<KPlayer> FindAll(int mapCode, int copySceneID = -1)
        {
            return KTPlayerManager.PlayerContainer.GetPlayers(mapCode, copySceneID);
        }
        #endregion

        #region Thống kê
        /// <summary>
        /// Trả về tổng số người chơi
        /// </summary>
        /// <returns></returns>
        public static int GetPlayersCount()
        {
            return KTPlayerManager.players.Count;
        }

        /// <summary>
        /// Trả về tổng số người chơi trong bản đồ tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static int GetPlayersCount(int mapCode, int copySceneID = -1)
        {
            return KTPlayerManager.PlayerContainer.GetTotalPlayers(mapCode, copySceneID);
        }
        #endregion
    }
}
