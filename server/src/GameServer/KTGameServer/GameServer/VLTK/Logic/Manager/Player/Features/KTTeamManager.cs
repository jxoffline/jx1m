using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng quản lý nhóm
    /// </summary>
    public static class KTTeamManager
    {
        /// <summary>
        /// Số thành viên tối đa trong nhóm
        /// </summary>
        public const int MaxTeamSize = 6;

        /// <summary>
        /// Danh sách đội nhóm đang có trong hệ thống
        /// </summary>
        private static readonly ConcurrentDictionary<int, KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>>> Teams = new ConcurrentDictionary<int, KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>>>();

        /// <summary>
        /// ID nhóm tự tăng
        /// </summary>
        private static int AutoID = -1;

        /// <summary>
        /// Kiểm tra nhóm tương ứng có tồn tại không
        /// </summary>
        /// <param name="teamID"></param>
        /// <returns></returns>
        public static bool IsTeamExist(int teamID)
        {
            if (KTTeamManager.Teams.TryGetValue(teamID, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Trả về kích thước nhóm
        /// </summary>
        /// <param name="teamID"></param>
        /// <returns></returns>
        public static int GetTeamSize(int teamID)
        {
            if (KTTeamManager.Teams.TryGetValue(teamID, out KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>> pair))
            {
                return pair.Value.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Tạo nhóm
        /// </summary>
        /// <param name="teamID">Nhóm trưởng</param>
        public static void CreateTeam(KPlayer leader)
        {
            KTTeamManager.AutoID = (KTTeamManager.AutoID + 1) % 1000000007;
            int teamID = KTTeamManager.AutoID;
            KTTeamManager.Teams[teamID] = new KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>>(leader, new ConcurrentDictionary<int, KPlayer>());
            /// Thêm nhóm trưởng vào nhóm
            KTTeamManager.AssignTeam(teamID, leader);
            leader.TeamID = teamID;

            /// Hủy thách đấu
            leader.RemoveChallenge(true, false);
        }

        /// <summary>
        /// Giải tán nhóm
        /// </summary>
        /// <param name="player"></param>
        public static void RetainTeam(KPlayer player)
        {
            if (KTTeamManager.Teams.TryGetValue(player.TeamID, out _))
            {
                KTTeamManager.Teams.TryRemove(player.TeamID, out _);

                /// Hủy thách đấu
                player.RemoveChallenge(true, false);
            }
        }

        /// <summary>
        /// Thêm người chơi vào nhóm
        /// </summary>
        /// <param name="teamID">ID nhóm</param>
        /// <param name="player">Người chơi</param>
        public static void AssignTeam(int teamID, KPlayer player)
        {
            /// Nếu người chơi đã có nhóm thì bỏ qua
            if (player.TeamID != -1)
            {
                return;
            }
            /// Nếu nhóm chưa tồn tại thì bỏ qua
            else if (!KTTeamManager.Teams.TryGetValue(teamID, out _))
            {
                return;
            }

            /// Thêm người chơi vào nhóm
            lock (KTTeamManager.Teams[teamID].Value)
			{
                KTTeamManager.Teams[teamID].Value[player.RoleID] = player;
            }
            player.TeamID = teamID;

            /// Thực hiện hàm vào đội
            player.OnJoinTeam();

            /// Hủy thách đấu
            player.RemoveChallenge(true, false);
        }

        /// <summary>
        /// Xóa người chơi khỏi nhóm
        /// </summary>
        /// <param name="player"></param>
        public static void LeaveTeam(KPlayer player)
        {
            /// Nếu người chơi chưa có nhóm thì bỏ qua
            if (player.TeamID == -1)
            {
                return;
            }
            /// Nếu nhóm chưa tồn tại thì bỏ qua
            else if (!KTTeamManager.Teams.TryGetValue(player.TeamID, out _))
            {
                return;
            }

            /// Trưởng nhóm
            KPlayer teamLeader = KTTeamManager.GetTeamLeader(player.TeamID);
            /// Nếu tồn tại
            if (teamLeader != null)
            {
                /// Hủy thách đấu
                teamLeader.RemoveChallenge(true, false);
            }

            /// Xóa người chơi khỏi nhóm
            lock (KTTeamManager.Teams[player.TeamID].Value)
			{
                KTTeamManager.Teams[player.TeamID].Value.TryRemove(player.RoleID, out _);
            }
            player.TeamID = -1;

            /// Thực hiện hàm rời đội
            player.OnLeaveTeam();
        }

        /// <summary>
        /// Bổ nhiệm đội trưởng
        /// </summary>
        /// <param name="player"></param>
        public static void ApproveTeamLeader(KPlayer player)
        {
            /// Nếu người chơi chưa có nhóm thì bỏ qua
            if (player.TeamID == -1)
            {
                return;
            }
            /// Nếu nhóm chưa tồn tại thì bỏ qua
            else if (!KTTeamManager.Teams.TryGetValue(player.TeamID, out KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>> pair))
            {
                return;
            }

            /// Trưởng nhóm cũ
            KPlayer teamLeader = KTTeamManager.GetTeamLeader(player.TeamID);
            /// Nếu tồn tại
            if (teamLeader != null)
            {
                /// Hủy thách đấu
                teamLeader.RemoveChallenge(true, false);
            }

            KTTeamManager.Teams[player.TeamID] = new KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>>(player, KTTeamManager.Teams[player.TeamID].Value);
        }

        /// <summary>
        /// Trả về danh sách thành viên trong nhóm ID tương ứng
        /// </summary>
        /// <param name="teamID">ID nhóm</param>
        /// <returns></returns>
        public static List<KPlayer> GetTeamPlayers(int teamID)
        {
            List<KPlayer> teammates = new List<KPlayer>();
            if (KTTeamManager.Teams.TryGetValue(teamID, out KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>> pair))
            {
                List<int> keys = pair.Value.Keys.ToList();
                foreach (int key in keys)
                {
                    if (pair.Value.TryGetValue(key, out KPlayer player))
                    {
                        teammates.Add(player);
                    }
                }
            }
            return teammates;
        }

        /// <summary>
        /// Trả về ID đội trưởng
        /// </summary>
        /// <param name="teamID"></param>
        /// <returns></returns>
        public static KPlayer GetTeamLeader(int teamID)
        {
            if (KTTeamManager.Teams.TryGetValue(teamID, out KeyValuePair<KPlayer, ConcurrentDictionary<int, KPlayer>> pair))
            {
                return pair.Key;
            }
            return null;
        }
    }
}
