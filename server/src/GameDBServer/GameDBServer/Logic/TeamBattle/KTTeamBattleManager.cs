using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDBServer.Logic.TeamBattle
{
    /// <summary>
    /// Quản lý Võ lâm liên đấu
    /// </summary>
    public class KTTeamBattleManager
    {
        /// <summary>
        /// Truy vấn thông tin Liên đấu
        /// </summary>
        private enum TeamBattleQueryType
        {
            /// <summary>
            /// Truy vấn tổng số chiến đội
            /// </summary>
            GET_TOTAL_TEAMS,

            /// <summary>
            /// Truy vấn thông tin chiến đội bản thân
            /// </summary>
            GET_TEAM_INFO,

            /// <summary>
            /// Đăng ký tham gia
            /// </summary>
            REGISTER,

            /// <summary>
            /// Cập nhật điểm số và tổng số vòng đấu vòng tròn đã tham gia của chiến đội
            /// </summary>
            UPDATE_TEAM_POINT_AND_TOTALBATTLES,

            /// <summary>
            /// Cập nhật danh sách, chọn ra các đội thỏa mãn tham gia
            /// </summary>
            ARRANGE_FINAL_ROUND,

            /// <summary>
            /// Xếp hạng toàn bộ trong tháng
            /// </summary>
            ARRANGE_RANK,

            /// <summary>
            /// Xếp hạng và cập nhật trạng thái phần thưởng cho toàn bộ các chiến đội
            /// </summary>
            ARRANGE_RANK_AND_UPDATE_TEAMS_AWARDS_STATE,

            /// <summary>
            /// Cập nhật trạng thái phần thưởng của chiến đội bản thân
            /// </summary>
            UPDATE_TEAM_AWARDS_STATE,

            /// <summary>
            /// Làm rỗng danh sách nhóm
            /// </summary>
            CLEAR_TEAM_DATA,

            /// <summary>
            /// Trả về danh sách Top chiến đội
            /// </summary>
            GET_TOP_TEAM,
        }

        #region Singleton - Instance

        /// <summary>
        /// Quản lý Võ lâm liên đấu
        /// </summary>
        public static KTTeamBattleManager Instance { get; private set; }

        /// <summary>
        /// Quản lý Võ lâm liên đấu
        /// </summary>
        private KTTeamBattleManager(DBManager dBManager)
        {
            /// Đánh dấu chưa tải xuống dữ liệu hoàn tất
            this.loadFinished = false;
            /// Gắn DBManager
            this.Database = dBManager;
            /// Xóa thông tin chiến đội đã quá hạn
            this.ClearTimeoutTeams();
            /// Tải danh sách chiến đội
            this.LoadTeams();
            /// Cập nhật thứ hạng
            this.MakePlayersRank();
            /// Đánh dấu đã tải xuống dữ liệu hoàn tất
            this.loadFinished = true;
        }

        #endregion Singleton - Instance

        #region Define

        /// <summary>
        /// Số trận tối thiểu cần để xếp hạng nhận thưởng
        /// </summary>
        private const int MinBattlesToArrange = 15;

        /// <summary>
        /// Đánh dấu đã tải xuống dữ liệu hoàn tất chưa
        /// </summary>
        private readonly bool loadFinished = false;

        /// <summary>
        /// Quản lý DB
        /// </summary>
        private readonly DBManager Database;

        /// <summary>
        /// Danh sách chiến đội
        /// </summary>
        private readonly ConcurrentDictionary<int, TeamBattleInfo> Teams = new ConcurrentDictionary<int, TeamBattleInfo>();

        /// <summary>
        /// Danh sách chiến đội đã được xếp hạng từ bảo trì
        /// </summary>
        private readonly List<TeamBattleInfo> TopTeams = new List<TeamBattleInfo>();

        /// <summary>
        /// Khởi tạo Võ lâm liên đấu
        /// </summary>
        public static void Init(DBManager dBManager)
        {
            KTTeamBattleManager.Instance = new KTTeamBattleManager(dBManager);
        }

        #endregion Define

        #region Private methods

        /// <summary>
        /// Xóa các nhóm không nằm trong tháng
        /// </summary>
        private void ClearTimeoutTeams()
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_teambattle_team WHERE MONTH(register_time) <> {0}", DateTime.Now.Month);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Tải danh sách thông tin chiến đội
        /// </summary>
        private void LoadTeams()
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("SELECT * FROM t_teambattle_team");

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    TeamBattleInfo teamBattleInfo = new TeamBattleInfo();
                    teamBattleInfo.ID = Convert.ToInt32(reader["aid"].ToString());
                    teamBattleInfo.Name = reader["name"].ToString();
                    teamBattleInfo.Members = new Dictionary<int, string>();
                    teamBattleInfo.RegisterTime = DateTime.Parse(reader["register_time"].ToString());
                    teamBattleInfo.Point = Convert.ToInt32(reader["point"].ToString());
                    teamBattleInfo.TotalBattles = Convert.ToInt32(reader["total_battles"].ToString());
                    teamBattleInfo.Stage = Convert.ToInt32(reader["stage"].ToString());
                    teamBattleInfo.HasAwards = Convert.ToInt32(reader["has_awards"].ToString()) == 1;
                    if (reader["last_win_time"] != null && !string.IsNullOrEmpty(reader["last_win_time"].ToString()))
                    {
                        teamBattleInfo.LastWinTime = DateTime.Parse(reader["last_win_time"].ToString());
                    }

                    /// Thành viên 1
                    int member1 = Convert.ToInt32(reader["member_1_id"].ToString());
                    /// Thành viên 2
                    int member2 = Convert.ToInt32(reader["member_2_id"].ToString());
                    /// Thành viên 3
                    int member3 = Convert.ToInt32(reader["member_3_id"].ToString());
                    /// Thành viên 4
                    int member4 = Convert.ToInt32(reader["member_4_id"].ToString());

                    /// Thêm thành viên vào nhóm
                    if (member1 != -1)
                    {
                        teamBattleInfo.Members[member1] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member1);
                    }
                    if (member2 != -1)
                    {
                        teamBattleInfo.Members[member2] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member2);
                    }
                    if (member3 != -1)
                    {
                        teamBattleInfo.Members[member3] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member3);
                    }
                    if (member4 != -1)
                    {
                        teamBattleInfo.Members[member4] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member4);
                    }

                    /// Thêm vào danh sách
                    this.Teams[teamBattleInfo.ID] = teamBattleInfo;
                }

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thứ hạng chiến đội người chơi
        /// </summary>
        private void MakePlayersRank()
        {
            /// Thời gian xếp hạng
            DateTime updateTime = DateTime.Now;
            /// Xếp hạng
            List<TeamBattleInfo> teamBattles = this.Teams.Values.Where(x => x.TotalBattles > 0).OrderByDescending(x => x.Stage).ThenByDescending(x => x.Point).ThenBy(x => x.TotalBattles).ThenBy(x => x.LastWinTime).ToList();
            /// Duyệt danh sách và cập nhật hạng tương ứng
            for (int idx = 0; idx < teamBattles.Count; idx++)
            {
                /// Thông tin đội
                TeamBattleInfo teamBattle = teamBattles[idx];
                /// Cập nhật hạng tương ứng
                teamBattle.Rank = idx + 1;
                /// Cập nhật thời điểm xếp hạng
                teamBattle.LastUpdateRankTime = updateTime;
            }

            /// Xóa danh sách cũ
            this.TopTeams.Clear();
            /// Duyệt danh sách làm 1 bản Clone
            foreach (TeamBattleInfo teamBattle in teamBattles)
            {
                /// Thêm vào danh sách đã sắp xếp
                this.TopTeams.Add(teamBattle.Clone());
            }
        }

        /// <summary>
        /// Trả về thông tin chiến đội của người chơi ngay lập tức từ DB
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private TeamBattleInfo GetPlayerTeamInfoImmediateFromDB(int roleID)
        {
            /// Kết quả
            TeamBattleInfo teamBattleInfo = null;

            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("SELECT * FROM t_teambattle_team WHERE member_1_id = {0} OR member_2_id = {0} OR member_3_id = {0} OR member_4_id = {0}", roleID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    teamBattleInfo = new TeamBattleInfo();
                    teamBattleInfo.ID = Convert.ToInt32(reader["aid"].ToString());
                    teamBattleInfo.Name = reader["name"].ToString();
                    teamBattleInfo.Members = new Dictionary<int, string>();
                    teamBattleInfo.RegisterTime = DateTime.Parse(reader["register_time"].ToString());
                    teamBattleInfo.Point = Convert.ToInt32(reader["point"].ToString());
                    teamBattleInfo.TotalBattles = Convert.ToInt32(reader["total_battles"].ToString());
                    teamBattleInfo.Stage = Convert.ToInt32(reader["stage"].ToString());
                    teamBattleInfo.HasAwards = Convert.ToInt32(reader["has_awards"].ToString()) == 1;
                    if (reader["last_win_time"] != null && !string.IsNullOrEmpty(reader["last_win_time"].ToString()))
                    {
                        teamBattleInfo.LastWinTime = DateTime.Parse(reader["last_win_time"].ToString());
                    }

                    /// Thành viên 1
                    int member1 = Convert.ToInt32(reader["member_1_id"].ToString());
                    /// Thành viên 2
                    int member2 = Convert.ToInt32(reader["member_2_id"].ToString());
                    /// Thành viên 3
                    int member3 = Convert.ToInt32(reader["member_3_id"].ToString());
                    /// Thành viên 4
                    int member4 = Convert.ToInt32(reader["member_4_id"].ToString());

                    /// Thêm thành viên vào nhóm
                    if (member1 != -1)
                    {
                        teamBattleInfo.Members[member1] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member1);
                    }
                    if (member2 != -1)
                    {
                        teamBattleInfo.Members[member2] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member2);
                    }
                    if (member3 != -1)
                    {
                        teamBattleInfo.Members[member3] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member3);
                    }
                    if (member4 != -1)
                    {
                        teamBattleInfo.Members[member4] = DBRoleInfo.QueryRoleNameByRoleID(this.Database, member4);
                    }
                }

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }

            return teamBattleInfo;
        }

        /// <summary>
        /// Kiểm tra nhóm đã tồn tại chưa
        /// </summary>
        /// <param name="memberID_1"></param>
        /// <param name="memberID_2"></param>
        /// <param name="memberID_3"></param>
        /// <param name="memberID_4"></param>
        /// <returns></returns>
        private bool IsTeamPlayerExist(int memberID_1, int memberID_2, int memberID_3, int memberID_4)
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return true;
            }

            List<int> keys = this.Teams.Keys.ToList();
            foreach (int key in keys)
            {
                if (!this.Teams.TryGetValue(key, out TeamBattleInfo teamBattleInfo))
                {
                    continue;
                }
                if ((memberID_1 != -1 && teamBattleInfo.Members.Keys.Contains(memberID_1)) || (memberID_2 != -1 && teamBattleInfo.Members.Keys.Contains(memberID_2)) || (memberID_3 != -1 && teamBattleInfo.Members.Keys.Contains(memberID_3)) || (memberID_4 != -1 && teamBattleInfo.Members.Keys.Contains(memberID_4)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra tên nhóm có tồn tại không
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        private bool IsTeamNameExist(string teamName)
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return true;
            }

            List<int> keys = this.Teams.Keys.ToList();
            foreach (int key in keys)
            {
                if (!this.Teams.TryGetValue(key, out TeamBattleInfo teamBattleInfo))
                {
                    continue;
                }
                if (teamBattleInfo.Name == teamName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sắp xếp và tăng bậc để vào vòng chung kết cho Top các chiến đội tương ứng
        /// </summary>
        /// <param name="teamCount"></param>
        private bool ArrangeAndIncreaseStageToTopTeamToTheFinalRound(int teamCount)
        {
            /// Danh sách đội được cập nhật
            Dictionary<int, TeamBattleInfo> teams = this.Teams.Where(x => x.Value.TotalBattles > 0).OrderByDescending(x => x.Value.Point).ThenBy(x => x.Value.TotalBattles).ThenBy(x => x.Value.LastWinTime).Take(teamCount).ToDictionary(tKey => tKey.Key, tValue => tValue.Value);
            /// Danh sách Query để cập nhật vào DB
            List<string> whereClause = new List<string>();
            /// Duyệt danh sách được cập nhật
            foreach (int teamID in teams.Keys)
            {
                /// Cập nhật bậc của chiến đội
                teams[teamID].Stage = 1;
                /// Thêm mệnh đề WHERE tương ứng
                whereClause.Add(string.Format("aid = {0}", teamID));
            }

            /// Lưu vào DB
            {
                MySQLConnection conn = null;

                try
                {
                    conn = this.Database.DBConns.PopDBConnection();
                    string queryString = string.Format("UPDATE t_teambattle_team SET stage = {0} WHERE {1}", 1, string.Join(" OR ", whereClause));

                    MySQLCommand cmd = new MySQLCommand(queryString, conn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cmd = null;

                    return true;
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                    return false;
                }
                finally
                {
                    if (null != conn)
                    {
                        this.Database.DBConns.PushDBConnection(conn);
                    }
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách chiến đội
        /// </summary>
        /// <returns></returns>
        private bool ClearTeamsData()
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_teambattle_team");

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                /// Xóa danh sách được cache
                this.Teams.Clear();
                this.TopTeams.Clear();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        #endregion Private methods

        #region Public methods

        #region Get

        /// <summary>
        /// Trả về tổng số chiến đội đăng ký tham gia
        /// </summary>
        /// <returns></returns>
        public int GetTotalTeamBattles()
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return -1;
            }

            return this.Teams.Count;
        }

        /// <summary>
        /// Trả về danh sách chiến đội tương ứng
        /// </summary>
        /// <returns></returns>
        public List<TeamBattleInfo> GetTeamBattles()
        {
            List<TeamBattleInfo> teamBattles = new List<TeamBattleInfo>();
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return teamBattles;
            }

            List<int> keys = this.Teams.Keys.ToList();
            foreach (int key in keys)
            {
                if (!this.Teams.TryGetValue(key, out TeamBattleInfo teamBattleInfo))
                {
                    continue;
                }
                teamBattles.Add(teamBattleInfo);
            }
            return teamBattles;
        }

        /// <summary>
        /// Trả về thông tin chiến đội của người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public TeamBattleInfo GetPlayerTeamInfo(int roleID)
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return null;
            }

            List<int> keys = this.Teams.Keys.ToList();
            foreach (int key in keys)
            {
                if (!this.Teams.TryGetValue(key, out TeamBattleInfo teamBattleInfo))
                {
                    continue;
                }
                if (teamBattleInfo.Members.Keys.Contains(roleID))
                {
                    return teamBattleInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Trả về danh sách Top các chiến đội được hệ thống sắp xếp trước bảo trì
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TeamBattleInfo> GetTopTeamBattles(int count)
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return new List<TeamBattleInfo>();
            }

            /// Nếu kích thước danh sách dưới số lượng
            if (this.TopTeams.Count < count)
            {
                return this.TopTeams;
            }

            /// Trả về kết quả
            return this.TopTeams.GetRange(0, count);
        }

        #endregion Get

        #region Add

        /// <summary>
        /// Thêm chiến đội tương ứng của người chơi
        /// </summary>
        /// <param name="name"></param>
        /// <param name="memberRoleID_1">-1 nghĩa là rỗng</param>
        /// <param name="memberRoleID_2">-1 nghĩa là rỗng</param>
        /// <param name="memberRoleID_3">-1 nghĩa là rỗng</param>
        /// <param name="memberRoleID_4">-1 nghĩa là rỗng</param>
        /// <returns></returns>
        public TeamBattleInfo AddPlayerTeam(string name, int memberRoleID_1, int memberRoleID_2, int memberRoleID_3 = -1, int memberRoleID_4 = -1)
        {
            /// Nếu chưa đọc dữ liệu xong thì thôi
            if (!this.loadFinished)
            {
                return null;
            }

            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("INSERT INTO t_teambattle_team(name, member_1_id, member_2_id, member_3_id, member_4_id, register_time, point, total_battles, stage, has_awards, last_win_time) VALUES('{0}', {1}, {2}, {3}, {4}, '{5}', {6}, {7}, {8}, {9}, '{10}')", name, memberRoleID_1, memberRoleID_2, memberRoleID_3, memberRoleID_4, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 0, 0, 0, 0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                /// Lấy ra thông tin nhóm vừa mới tạo
                TeamBattleInfo teamBattleInfo = this.GetPlayerTeamInfoImmediateFromDB(memberRoleID_1);

                /// Thêm vào danh sách, ID chưa rõ
                this.Teams[teamBattleInfo.ID] = teamBattleInfo;

                return teamBattleInfo;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                return null;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        #endregion Add

        #region Update

        /// <summary>
        /// Cập nhật điểm tích lũy, tổng số trận thi đấu đã tham gia, tổng số lượt, bậc và thời gian thắng trận lần cuối của chiến đội
        /// </summary>
        /// <param name="teamID"></param>
        /// <param name="point"></param>
        /// <param name="totalBattles"></param>
        /// <param name="stage"></param>
        /// <param name="lastWinTicks"></param>
        /// <returns></returns>
        public bool UpdatePlayerTeamData(int teamID, int point, int totalBattles, int stage, long lastWinTicks)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_teambattle_team SET point = {0}, total_battles = {1}, stage = {2}, last_win_time = '{3}' WHERE aid = {4}", point, totalBattles, stage, new DateTime(lastWinTicks).ToString("yyyy-MM-dd HH:mm:ss"), teamID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                /// Cập nhật thông tin nhóm tương ứng
                if (this.Teams.TryGetValue(teamID, out TeamBattleInfo teamBattleInfo))
                {
                    teamBattleInfo.Point = point;
                    teamBattleInfo.TotalBattles = totalBattles;
                    teamBattleInfo.Stage = stage;
                    teamBattleInfo.LastWinTime = new DateTime(lastWinTicks);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái nhận thưởng của chiến đội
        /// </summary>
        /// <param name="teamID"></param>
        /// <param name="hasAwards"></param>
        /// <returns></returns>
        public bool UpdatePlayerTeamAwardsState(int teamID, bool hasAwards)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_teambattle_team SET has_awards = {0} WHERE aid = {1}", hasAwards ? 1 : 0, teamID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                /// Cập nhật thông tin nhóm tương ứng
                if (this.Teams.TryGetValue(teamID, out TeamBattleInfo teamBattleInfo))
                {
                    teamBattleInfo.HasAwards = hasAwards;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái nhận thưởng cho toàn bộ chiến đội
        /// </summary>
        /// <param name="hasAwards"></param>
        /// <returns></returns>
        public bool UpdatePlayerTeamsAwardsState(bool hasAwards)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_teambattle_team SET has_awards = {0} WHERE total_battles >= {1}", hasAwards ? 1 : 0, KTTeamBattleManager.MinBattlesToArrange);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                /// Cập nhật thông tin nhóm tương ứng
                List<int> keys = this.Teams.Keys.ToList();
                foreach (int teamID in keys)
                {
                    if (this.Teams.TryGetValue(teamID, out TeamBattleInfo teamBattleInfo))
                    {
                        teamBattleInfo.HasAwards = hasAwards;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        #endregion Update

        #endregion Public methods

        #region TCP-Core

        /// <summary>
        /// Xử lý gói tin truy vấn thông tin Võ lâm liên đấu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessTeamBattleCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length < 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Loại yêu cầu
                int type = int.Parse(fields[0]);

                /// Xử lý yêu cầu
                switch (type)
                {
                    /// Trả về tổng số chiến đội đã đăng ký
                    case (int)TeamBattleQueryType.GET_TOTAL_TEAMS:
                        {
                            /// Tổng số chiến đội
                            int totalTeams = this.GetTotalTeamBattles();
                            /// Trả về kết quả
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", totalTeams), nID);

                            break;
                        }
                    /// Trả về thông tin chiến đội
                    case (int)TeamBattleQueryType.GET_TEAM_INFO:
                        {
                            /// ID nhân vật
                            int roleID = int.Parse(fields[1]);
                            /// Thông tin chiến đội tương ứng
                            TeamBattleInfo teamBattleInfo = this.GetPlayerTeamInfo(roleID);
                            /// Nếu không tìm thấy
                            if (teamBattleInfo == null)
                            {
                                teamBattleInfo = new TeamBattleInfo()
                                {
                                    ID = -100,
                                };
                            }
                            /// Chuỗi byte kết quả
                            byte[] resultBytes = DataHelper.ObjectToBytes<TeamBattleInfo>(teamBattleInfo);
                            /// Trả về kết quả
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, resultBytes, 0, resultBytes.Length, nID);

                            break;
                        }
                    /// Đăng ký thi đấu
                    case (int)TeamBattleQueryType.REGISTER:
                        {
                            /// Tên nhóm
                            string name = fields[1];
                            /// ID thành viên 1
                            int memberID_1 = int.Parse(fields[2]);
                            /// ID thành viên 2
                            int memberID_2 = int.Parse(fields[3]);
                            /// ID thành viên 3
                            int memberID_3 = int.Parse(fields[4]);
                            /// ID thành viên 4
                            int memberID_4 = int.Parse(fields[5]);
                            /// Nếu nhóm đã tồn tại
                            if (this.IsTeamPlayerExist(memberID_1, memberID_2, memberID_3, memberID_4))
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                            }
                            /// Nếu tên đã tồn tại
                            else if (this.IsTeamNameExist(name))
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -2), nID);
                            }
                            else
                            {
                                /// Thêm nhóm tương ứng
                                TeamBattleInfo teamBattleInfo = this.AddPlayerTeam(name, memberID_1, memberID_2, memberID_3, memberID_4);
                                /// Toác
                                if (teamBattleInfo == null)
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                                }
                                else
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                                }
                            }

                            break;
                        }
                    /// Cập nhật điểm số và tổng số vòng đấu đã tham gia
                    case (int)TeamBattleQueryType.UPDATE_TEAM_POINT_AND_TOTALBATTLES:
                        {
                            /// ID thành viên
                            int roleID = int.Parse(fields[1]);
                            /// Điểm số
                            int point = int.Parse(fields[2]);
                            /// Tổng số trận
                            int totalBattles = int.Parse(fields[3]);
                            /// Bậc của chiến đội
                            int stage = int.Parse(fields[4]);
                            /// Thời gian thắng trận lần cuối
                            long lastWinTicks = long.Parse(fields[5]);
                            /// Thông tin nhóm tương ứng
                            TeamBattleInfo teamBattleInfo = this.GetPlayerTeamInfo(roleID);
                            /// Nếu không tồn tại
                            if (teamBattleInfo == null)
                            {
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                            }
                            else
                            {
                                /// Cập nhật
                                bool ret = this.UpdatePlayerTeamData(teamBattleInfo.ID, point, totalBattles, stage, lastWinTicks);
                                /// Toác
                                if (!ret)
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                                }
                                else
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                                }
                            }

                            break;
                        }
                    /// Cập nhật danh sách, chọn ra các đội top cao nhất tham gia vòng chung kết
                    case (int)TeamBattleQueryType.ARRANGE_FINAL_ROUND:
                        {
                            /// Số lượng chọn ra
                            int teamCount = int.Parse(fields[1]);
                            /// Thực hiện
                            bool ret = this.ArrangeAndIncreaseStageToTopTeamToTheFinalRound(teamCount);
                            /// Toác
                            if (!ret)
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                            }
                            else
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                            }
                            break;
                        }
                    case (int)TeamBattleQueryType.ARRANGE_RANK:
                        {
                            /// Thực hiện
                            this.MakePlayersRank();
                            /// Trả về kết quả
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                            break;
                        }
                    case (int)TeamBattleQueryType.UPDATE_TEAM_AWARDS_STATE:
                        {
                            /// ID thành viên
                            int roleID = int.Parse(fields[1]);
                            /// Trạng thái
                            int state = int.Parse(fields[2]);
                            /// Thông tin chiến đội
                            TeamBattleInfo teamBattleInfo = this.GetPlayerTeamInfo(roleID);
                            /// Nếu không tồn tại
                            if (teamBattleInfo == null)
                            {
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                            }
                            else
                            {
                                /// Cập nhật
                                bool ret = this.UpdatePlayerTeamAwardsState(teamBattleInfo.ID, state == 1);
                                /// Toác
                                if (!ret)
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                                }
                                else
                                {
                                    /// Trả về kết quả
                                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                                }
                            }
                            break;
                        }
                    case (int)TeamBattleQueryType.ARRANGE_RANK_AND_UPDATE_TEAMS_AWARDS_STATE:
                        {
                            /// Trạng thái
                            int state = int.Parse(fields[1]);
                            /// Xếp hạng
                            this.MakePlayersRank();
                            /// Cập nhật trạng thái nhận thưởng
                            bool ret = this.UpdatePlayerTeamsAwardsState(state == 1);
                            /// Toác
                            if (!ret)
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                            }
                            else
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                            }

                            break;
                        }
                    case (int)TeamBattleQueryType.CLEAR_TEAM_DATA:
                        {
                            /// Thao tác
                            bool ret = this.ClearTeamsData();
                            /// Toác
                            if (!ret)
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                            }
                            else
                            {
                                /// Trả về kết quả
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 1), nID);
                            }

                            break;
                        }
                    case (int)TeamBattleQueryType.GET_TOP_TEAM:
                        {
                            /// Tổng số cần lấy
                            int topCount = int.Parse(fields[1]);
                            /// Chuỗi byte kết quả
                            byte[] resultBytes = DataHelper.ObjectToBytes<List<TeamBattleInfo>>(this.GetTopTeamBattles(topCount));
                            /// Trả về kết quả
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, resultBytes, 0, resultBytes.Length, nID);

                            break;
                        }
                    default:
                        {
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion TCP-Core
    }
}