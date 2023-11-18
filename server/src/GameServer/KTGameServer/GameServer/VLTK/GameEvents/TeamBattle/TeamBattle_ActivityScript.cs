using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.TeamBattle
{
    /// <summary>
    /// Quản lý sự kiện Võ lâm liên đấu
    /// </summary>
    public static class TeamBattle_ActivityScript
    {
        #region Define

        /// <summary>
        /// Danh sách người chơi và nhóm đã đăng ký
        /// </summary>
        private static readonly ConcurrentDictionary<int, KPlayer> RegisterPlayers = new ConcurrentDictionary<int, KPlayer>();

        #endregion Define

        #region Timer Logic

        /// <summary>
        /// Thời gian diễn ra sự kiện hiện tại
        /// </summary>
        private static DateTime CurrentEventTime = DateTime.MinValue;

        /// <summary>
        /// Biến đánh dấu đã sắp xếp danh sách nhóm vào chung kết chưa
        /// </summary>
        private static bool ArrangedTeamToFinalRound = false;

        /// <summary>
        /// Biến đánh dấu đã xếp hạng người chơi để nhận thưởng chưa
        /// </summary>
        private static bool ArrangedPlayersRank = false;

        /// <summary>
        /// Thực hiện Logic Timer khi gọi tới
        /// </summary>
        public static void Timer_Tick()
        {
            /// Sự kiện hiện tại
            DateTime? currentEventTimes = TeamBattle_ActivityScript.GetCurrentBattleTime();
            /// Nếu tồn tại
            if (currentEventTimes != null)
            {
                /// Nếu thời gian diễn ra sự kiện hiện tại khác với thời gian diễn ra sự kiện hiện tại đã lưu lại
                if (TeamBattle_ActivityScript.CurrentEventTime != currentEventTimes.Value)
                {
                    /// Đánh dấu lại thời gian diễn ra sự kiện hiện tại
                    TeamBattle_ActivityScript.CurrentEventTime = currentEventTimes.Value;
                    /// Bắt đầu sự kiện
                    TeamBattle_ActivityScript.BeginBattle();
                }
            }

            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Nếu là 0:00 ngày sắp xếp lại danh sách nhóm vào chung kết
            if (now.Day == TeamBattle.Battle.Config.ArrangeToFinalRoundAtDay && now.Hour == 0 && now.Minute == 0 && !TeamBattle_ActivityScript.ArrangedTeamToFinalRound)
            {
                /// Đánh dấu đã sắp xếp rồi
                TeamBattle_ActivityScript.ArrangedTeamToFinalRound = true;
                /// Thực hiện sắp xếp lại danh sách vào chung kết
                bool ret = TeamBattle_ActivityScript.ArrangeAndIncreaseStageToTopTeamToTheFinalRound();
                if (ret)
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, "Arrange and increase stage for top teams to the final round successfully!");
                }
                else
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, "Arrange and increase stage for top teams to the final round failed!");
                }
            }

            /// Nếu là 0:00 ngày đầu tiên bắt đầu nhận thưởng
            if (now.Day == TeamBattle.Award.Config.FromDay && now.Hour == 0 && now.Minute == 0 && !TeamBattle_ActivityScript.ArrangedPlayersRank)
            {
                /// Đánh dấu đã xếp hạng rồi
                TeamBattle_ActivityScript.ArrangedPlayersRank = true;
                /// Thực hiện xếp hạng người chơi
                int ret = TeamBattle_ActivityScript.ArrangePlayersRankAndUpdateAllTeamsAwardState(true);
                if (ret == 1)
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, "Arrange players rank and update all teams award state successfully!");
                }
                else
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Arrange players rank and update all teams award state failed, error code = {0}!", ret));
                }
            }
        }

        #endregion Timer Logic

        #region Notification

        /// <summary>
        /// Thông báo sự kiện
        /// </summary>
        /// <param name="activityID"></param>
        public static void NotifyActivity(int activityID)
        {
            /// Bắt đầu võ lâm liên đấu
            if (activityID >= 200 && activityID <= 202)
            {
                string message = string.Format("Võ lâm liên đấu - {0} đã bắt đầu báo danh từ ngày 12 đến ngày 14 tại Quan Liên Đấu (sơ, cao) ở các thành thị. Quý bằng hữu hãy nhanh chân đăng ký cho mình một chiến đội phù hợp để tham chiến!", TeamBattle_ActivityScript.GetCurrentMonthTeamBattleType());
                KTGlobal.SendSystemEventNotification(message);
            }
            /// Diễn ra Võ lâm liên đấu
            else if (activityID >= 203 && activityID <= 213)
            {
                string message = string.Format("Võ lâm liên đấu - {0} sẽ diễn ra các trận đấu vòng loại trong ngày hôm nay. Các chiến đội hãy chuẩn bị sẵn sàng, thông qua Quan Liên Đấu (sơ, cao) ở các thành thị tiến vào hội trường liên đấu để ghi danh tham chiến!", TeamBattle_ActivityScript.GetCurrentMonthTeamBattleType());
                KTGlobal.SendSystemEventNotification(message);
            }
            /// Chung kết Võ lâm liên đấu
            else if (activityID == 214)
            {
                string message = string.Format("Võ lâm liên đấu - {0} sẽ diễn ra các trận đấu vòng chung kết trong ngày hôm nay. Các chiến đội hãy chuẩn bị sẵn sàng, thông qua Quan Liên Đấu (sơ, cao) ở các thành thị tiến vào hội trường liên đấu để ghi danh tham chiến!", TeamBattle_ActivityScript.GetCurrentMonthTeamBattleType());
                KTGlobal.SendSystemEventNotification(message);
            }
            /// Nhận thưởng Võ lâm liên đấu
            else if (activityID >= 215 && activityID <= 216)
            {
                string message = string.Format("Võ lâm liên đấu - {0} tháng này đã kết thúc. Từ ngày 27 đến hết ngày 28, các chiến đội có thể thông qua Quan Liên Đấu (sơ, cao) ở các thành thị, căn cứ xếp hạng chiến đội nhận các phần quà tương ứng, hãy nhanh chân!", TeamBattle_ActivityScript.GetCurrentMonthTeamBattleType());
                KTGlobal.SendSystemEventNotification(message);
            }
        }

        #endregion Notification

        #region API

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

        /// <summary>
        /// Trả về thông tin liên đấu tháng này
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentMonthTeamBattleType()
        {
            /// Kích thước nhóm
            int teamCapacity = TeamBattle.Register.TeamCapacity;
            /// Chuyển sang chuỗi kết quả tương ứng
            switch (teamCapacity)
            {
                case 1:
                    {
                        return "Đơn đấu";
                    }
                case 2:
                    {
                        return "Song đấu";
                    }
                case 3:
                    {
                        return "Tam đấu";
                    }
                case 4:
                    {
                        return "Tứ đấu";
                    }
            }
            return "Chưa mở";
        }

        /// <summary>
        /// Có phải thời gian đăng ký Võ lâm liên đấu không
        /// </summary>
        /// <returns></returns>
        public static bool IsRegisterTime()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Nếu nằm trong thời gian báo danh
            return nDay >= TeamBattle.Register.FromDay && nDay <= TeamBattle.Register.ToDay;
        }

        /// <summary>
        /// Có phải thời gian diễn ra trận đấu không
        /// </summary>
        /// <returns></returns>
        public static bool IsBattleTime()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;
            /// Nếu nằm trong thời gian diễn ra liên đấu
            return TeamBattle.Battle.EventTimes.Any(x => x.Day == nDay && x.Times.Any(xx => xx.Hour == nHour && xx.Minute <= nMinute && nMinute <= xx.Minute + TeamBattle.Battle.Config.Duration / 60000));
        }

        /// <summary>
        /// Có phải diễn ra trận đấu trong hôm nay không
        /// </summary>
        /// <returns></returns>
        public static bool IsBattleTimeToday()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Trả về kết quả
            return TeamBattle.Battle.EventTimes.Any(x => x.Day == nDay);
        }

        /// <summary>
        /// Trả về thời gian diễn ra trận đấu hiện tại
        /// </summary>
        /// <returns></returns>
        public static DateTime? GetCurrentBattleTime()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;
            /// Sự kiện trong ngày
            TeamBattle.BattleInfo.EventTime eventTime = TeamBattle.Battle.EventTimes.Where(x => x.Day == nDay && x.Times.Any(xx => xx.Hour == nHour && xx.Minute <= nMinute && nMinute <= xx.Minute + TeamBattle.Battle.Config.Duration / 60000)).FirstOrDefault();
            /// Nếu không tìm thấy
            if (eventTime == null)
            {
                return null;
            }
            /// Múi giờ tương ứng
            TeamBattle.BattleInfo.EventTime.TimeStamp timeStamp = eventTime.Times.Where(x => x.Hour == nHour && x.Minute <= nMinute && nMinute <= x.Minute + TeamBattle.Battle.Config.Duration / 60000).FirstOrDefault();
            /// Nếu không tìm thấy
            if (timeStamp == null)
            {
                return null;
            }
            /// Trả về kết quả
            return new DateTime(now.Year, now.Month, now.Day, timeStamp.Hour, timeStamp.Minute, 0);
        }

        /// <summary>
        /// Trả về thông tin trận đấu đang diễn ra ở thời điểm hiện tại
        /// </summary>
        /// <returns></returns>
        public static TeamBattle.BattleInfo.EventTime GetCurrentBattleInfo()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;
            /// Nếu nằm trong thời gian diễn ra liên đấu
            return TeamBattle.Battle.EventTimes.Where(x => x.Day == nDay && x.Times.Any(xx => xx.Hour == nHour && xx.Minute <= nMinute && nMinute <= xx.Minute + TeamBattle.Battle.Config.Duration / 60000)).FirstOrDefault();
        }

        /// <summary>
        /// Trả về thời gian diễn ra liên đấu trận tiếp theo
        /// </summary>
        /// <returns></returns>
        public static TeamBattle.BattleInfo.EventTime GetNextBattleInfo()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;
            /// Ngày gần nhất diễn ra liên đấu
            TeamBattle.BattleInfo.EventTime eventTime = TeamBattle.Battle.EventTimes.OrderBy(x => x.Day).Where(x => x.Day >= nDay && x.Times.Any(xx => xx.Hour > nHour || (xx.Hour == nHour && xx.Minute > nMinute))).FirstOrDefault();
            /// Trả về kết quả
            return eventTime;
        }

        /// <summary>
        /// Trả về thời điểm thi đấu tiếp theo gần nhất
        /// </summary>
        /// <returns></returns>
        public static DateTime? GetNextBattleTime()
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Năm
            int nYear = now.Year;
            /// Tháng
            int nMonth = now.Month;
            /// Ngày trong tháng
            int nDay = now.Day;
            /// Giờ
            int nHour = now.Hour;
            /// Phút
            int nMinute = now.Minute;
            /// Ngày gần nhất diễn ra liên đấu
            TeamBattle.BattleInfo.EventTime eventTime = TeamBattle_ActivityScript.GetNextBattleInfo();
            /// Nếu không có
            if (eventTime == null)
            {
                return null;
            }
            /// Giờ gần nhất
            TeamBattle.BattleInfo.EventTime.TimeStamp nearestHour = eventTime.Times.Where(x => x.Hour > nHour || (x.Hour == nHour && x.Minute > nMinute)).FirstOrDefault();
            /// Nếu không có
            if (nearestHour == null)
            {
                return null;
            }
            /// Trả về kết quả
            return new DateTime(nYear, nMonth, eventTime.Day, nearestHour.Hour, nearestHour.Minute, 0);
        }

        /// <summary>
        /// Trả về bậc của trận đấu kế tiếp
        /// </summary>
        /// <returns></returns>
        public static int GetNextBattleStage()
        {
            /// Ngày gần nhất diễn ra liên đấu
            TeamBattle.BattleInfo.EventTime eventTime = TeamBattle_ActivityScript.GetNextBattleInfo();
            /// Nếu không có
            if (eventTime == null)
            {
                return -1;
            }
            /// Trả về kết quả
            return eventTime.Stage;
        }

        /// <summary>
        /// Trả về tổng số chiến đội
        /// </summary>
        /// <returns></returns>
        public static int GetTotalTeams()
        {
            string strCmd = string.Format("{0}", (int)TeamBattleQueryType.GET_TOTAL_TEAMS);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]);
                }
                catch (Exception) { }
            }
            return 0;
        }

        /// <summary>
        /// Trả về thông tin chiến đội của bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static TeamBattle.TeamBattleInfo GetTeamInfo(KPlayer player)
        {
            string strCmd = string.Format("{0}:{1}", (int)TeamBattleQueryType.GET_TEAM_INFO, player.RoleID);
            byte[] bytesData = UTF8Encoding.UTF8.GetBytes(strCmd);
            TCPProcessCmdResults result = Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, bytesData, bytesData.Length, out bytesData, GameManager.LocalServerId);
            if (TCPProcessCmdResults.RESULT_FAILED != result)
            {
                /// Kết quả trả về
                TeamBattle.TeamBattleInfo teamBattleInfo = DataHelper.BytesToObject<TeamBattle.TeamBattleInfo>(bytesData, 6, bytesData.Length - 6);
                /// Toác
                if (teamBattleInfo == null || teamBattleInfo.ID == -100)
                {
                    return null;
                }
                return teamBattleInfo;
            }
            return null;
        }

        /// <summary>
        /// Cập nhật điểm tích lũy, tổng số trận thi đấu đã tham gia, tổng số lượt, bậc và thời gian thắng trận lần cuối của chiến đội
        /// </summary>
        /// <param name="player"></param>
        /// <param name="point"></param>
        /// <param name="totalBattles"></param>
        /// <param name="lastWinTicks"></param>
        /// <returns></returns>
        public static int UpdatePlayerTeamData(KPlayer player, int point, int totalBattles, int stage, long lastWinTicks)
        {
            string strCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", (int)TeamBattleQueryType.UPDATE_TEAM_POINT_AND_TOTALBATTLES, player.RoleID, point, totalBattles, stage, lastWinTicks);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]);
                }
                catch (Exception) { }
            }
            return -100;
        }

        /// <summary>
        /// Tăng thứ hạng chọn các chiến đội vào vòng chung kết
        /// </summary>
        public static bool ArrangeAndIncreaseStageToTopTeamToTheFinalRound()
        {
            string strCmd = string.Format("{0}:{1}", (int)TeamBattleQueryType.ARRANGE_FINAL_ROUND, TeamBattle.Battle.Config.ToFinalRoundTotalTeams);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]) == 1;
                }
                catch (Exception) { }
            }
            return false;
        }

        /// <summary>
        /// Thực hiện xếp hạng cho chiến đội
        /// </summary>
        /// <returns></returns>
        public static bool ArrangePlayersRank()
        {
            string strCmd = string.Format("{0}", (int)TeamBattleQueryType.ARRANGE_RANK);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]) == 1;
                }
                catch (Exception) { }
            }
            return false;
        }

        /// <summary>
        /// Cập nhật trạng thái nhận thưởng của chiến đội
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hasAwards"></param>
        /// <returns></returns>
        public static int UpdateTeamAwardState(KPlayer player, bool hasAwards)
        {
            string strCmd = string.Format("{0}:{1}:{2}", (int)TeamBattleQueryType.UPDATE_TEAM_AWARDS_STATE, player.RoleID, hasAwards ? 1 : 0);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]);
                }
                catch (Exception) { }
            }
            return -1;
        }

        /// <summary>
        /// Thực hiện xếp hạng cho toàn bộ chiến đội và cập nhật trạng thái nhận thưởng cho toàn bộ chiến đội
        /// </summary>
        /// <param name="hasAwards"></param>
        /// <returns></returns>
        public static int ArrangePlayersRankAndUpdateAllTeamsAwardState(bool hasAwards)
        {
            string strCmd = string.Format("{0}:{1}", (int)TeamBattleQueryType.ARRANGE_RANK_AND_UPDATE_TEAMS_AWARDS_STATE, hasAwards ? 1 : 0);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]);
                }
                catch (Exception) { }
            }
            return -1;
        }

        /// <summary>
        /// Làm rỗng danh sách chiến đội
        /// </summary>
        /// <returns></returns>
        public static int ClearTeamsData()
        {
            string strCmd = string.Format("{0}", (int)TeamBattleQueryType.CLEAR_TEAM_DATA);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields.Length > 0)
            {
                try
                {
                    return int.Parse(fields[0]);
                }
                catch (Exception) { }
            }
            return -1;
        }

        #endregion API

        #region Core Event

        /// <summary>
        /// Chuyển người chơi đến hội trường liên đấu tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void MoveToBattleHall(KPlayer player)
        {
            /// Thực hiện chuyển bản đồ
            KTPlayerManager.ChangeMap(player, TeamBattle.Config.EnterMapID, TeamBattle.Config.EnterPosX, TeamBattle.Config.EnterPosY, true);
        }

        /// <summary>
        /// Tạo chiến đội tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string CreateTeam(KPlayer player, string teamName)
        {
            /// Nếu không có nhóm
            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
            {
                return "Ngươi chưa có nhóm, hãy tạo nhóm rồi tới chỗ ta tiến hành báo danh!";
            }
            /// Nếu không phải trưởng nhóm
            else if (player.TeamLeader != player)
            {
                return "Ngươi không phải trưởng nhóm, chỉ có trưởng nhóm mới có thể tiến hành báo danh!";
            }
            /// Danh sách thành viên trong nhóm
            List<KPlayer> teammates = player.Teammates;
            /// Nếu nhóm không đủ người
            if (teammates.Count != TeamBattle.Register.TeamCapacity)
            {
                return string.Format("Nhóm đăng ký phải gồm có <color=green>{0} thành viên</color>!", TeamBattle.Register.TeamCapacity);
            }

            /// Duyệt danh sách thành viên
            foreach (KPlayer teammate in teammates)
            {
                /// Nếu cấp độ không đủ
                if (teammate.m_Level < TeamBattle.Config.MinLevel)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> cấp độ không đủ <color=green>cấp {1}</color>, không thể báo danh!", teammate.RoleName, TeamBattle.Config.MinLevel);
                }
            }

            /// Nếu các thành viên không ở gần bản thân
            foreach (KPlayer teammate in teammates)
            {
                /// Nếu trùng với bản thân
                if (teammate == player)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu khác bản đồ
                if (teammate.CurrentMapCode != player.CurrentMapCode)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> không ở bản đồ hiện tại, không thể báo danh!", teammate.RoleName);
                }
                /// Khoảng cách
                float distance = KTGlobal.GetDistanceBetweenPlayers(player, teammate);
                /// Nếu ở quá xa
                if (distance > 1000)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> ở quá xa, không thể báo danh!", teammate.RoleName);
                }
            }

            /// Tên có chứa ký tự đặc biệt
            if (!Utils.CheckValidString(teamName))
            {
                return "Tên có chứa ký tự đặc biệt, không thể thao tác!";
            }

            /// Thành viên số 1
            int memberID_1 = teammates[0].RoleID;
            /// Thành viên số 2
            int memberID_2 = teammates.Count > 1 ? teammates[1].RoleID : -1;
            /// Thành viên số 3
            int memberID_3 = teammates.Count > 2 ? teammates[2].RoleID : -1;
            /// Thành viên số 4
            int memberID_4 = teammates.Count > 3 ? teammates[3].RoleID : -1;

            /// Thực hiện báo danh
            string strCmd = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", (int)TeamBattleQueryType.REGISTER, teamName, memberID_1, memberID_2, memberID_3, memberID_4);
            string[] fields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, strCmd, GameManager.LocalServerId);
            if (fields != null && fields.Length > 0)
            {
                try
                {
                    /// Kết quả
                    int result = int.Parse(fields[0]);
                    /// Chiến đội đã tồn tại
                    if (result == -1)
                    {
                        return "Một trong số thành viên nhóm đã thuộc chiến đội khác!";
                    }
                    /// Tên đã tồn tại
                    else if (result == -2)
                    {
                        return "Tên chiến đội đã tồn tại, hãy chọn một tên khác!";
                    }
                    /// Lỗi gì đó
                    else if (result == 0)
                    {
                        return "Báo danh thất bại do có lỗi phát sinh, hãy thử lại sau, và liên hệ với hỗ trợ nếu vẫn không được!";
                    }
                    else if (result == 1)
                    {
                        return "Báo danh thành công!";
                    }
                }
                catch (Exception) { }
            }
            return "Báo danh thất bại, hãy thử lại sau!";
        }

        /// <summary>
        /// Cập nhật thưởng cho nhóm người chơi khi bắt đầu thi đấu
        /// </summary>
        /// <param name="teamPlayer"></param>
        public static void ProcessEnterBattle(KPlayer teamPlayer)
        {
            /// Toác
            if (teamPlayer == null)
            {
                return;
            }

            /// Danh sách thành viên
            List<KPlayer> teammates = teamPlayer.Teammates;
            /// Duyệt danh sách thành viên
            foreach (KPlayer teammate in teammates)
            {
                /// Thêm kinh nghiệm tương ứng
                KTPlayerManager.AddExp(teammate, TeamBattle.Award.EnterRound.Exp);
                /// Thêm bạc khóa tương ứng
                KTPlayerManager.AddBoundMoney(teammate, TeamBattle.Award.EnterRound.BoundMoney, "TeamBattle_EnterRoundAward");
                /// Thêm danh vọng
                KTGlobal.AddRepute(teammate, 501, TeamBattle.Award.EnterRound.Repute);
            }
        }

        /// <summary>
        /// Cập nhật kết quả trận đấu
        /// </summary>
        /// <param name="winnerTeamPlayer"></param>
        /// <param name="stage"></param>
        /// <param name="increaseStageForWinnerTeam"></param>
        public static void ProcessBattleResult(KPlayer winnerTeamPlayer, int stage, bool increaseStageForWinnerTeam)
        {
            /// Toác
            if (winnerTeamPlayer == null)
            {
                return;
            }

            /// Thông tin đội thắng
            TeamBattle.TeamBattleInfo winnerTeamInfo = TeamBattle_ActivityScript.GetTeamInfo(winnerTeamPlayer);
            /// Nếu không tìm thấy
            if (winnerTeamInfo == null)
            {
                return;
            }

            /// Cập nhật số điểm cho đội thắng
            winnerTeamInfo.Point += stage <= 0 ? 1 : stage * 100;
            /// Nếu có tăng bậc cho chiến đội thắng cuộc
            if (increaseStageForWinnerTeam)
            {
                /// Tăng bậc lên
                winnerTeamInfo.Stage++;

                /// Thông báo kênh hệ thống
                KTGlobal.SendSystemEventNotification(string.Format("Chúc mừng chiến đội <color=#33cfff>{0}</color> đã dành chiến thắng trong đợt <color=green>Võ lâm liên đấu</color> vòng chung kết <color=yellow>Bậc {1}</color>.", winnerTeamInfo.Name, stage));
            }

            /// Danh sách thành viên
            List<KPlayer> teammates = winnerTeamPlayer.Teammates;
            /// Duyệt danh sách thành viên
            foreach (KPlayer teammate in teammates)
            {
                /// Thêm kinh nghiệm tương ứng
                KTPlayerManager.AddExp(teammate, TeamBattle.Award.WinRound.Exp);
                /// Thêm bạc khóa tương ứng
                KTPlayerManager.AddBoundMoney(teammate, TeamBattle.Award.WinRound.BoundMoney, "TeamBattle_WinRoundAward");
                /// Thêm danh vọng
                KTGlobal.AddRepute(teammate, 501, TeamBattle.Award.WinRound.Repute);
            }

            /// Cập nhật số điểm
            TeamBattle_ActivityScript.UpdatePlayerTeamData(winnerTeamPlayer, winnerTeamInfo.Point, winnerTeamInfo.TotalBattles, winnerTeamInfo.Stage, DateTime.Now.Ticks);
        }

        /// <summary>
        /// Đăng ký thi đấu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stage"></param>
        public static string RegisterForBattle(KPlayer player)
        {
            /// Nếu chưa có nhóm
            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
            {
                return "Nhóm không tồn tại, không thể báo danh tỷ thí!";
            }
            /// Nếu không phải trưởng nhóm
            else if (player.TeamLeader != player)
            {
                return "Chỉ nhóm trưởng mới có quyền báo danh tỷ thí!";
            }

            /// Thông tin nhóm đã đăng ký trước đó
            TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player);
            /// Nếu không tồn tại
            if (teamInfo == null)
            {
                return "Nhóm của ngươi không đăng ký tham gia Võ lâm liên đấu tháng này!";
            }

            /// Thông tin nhóm hiện tại
            List<KPlayer> teammates = player.Teammates;

            /// Nếu số lượng thành viên không đúng
            if (teammates.Count != teamInfo.Members.Count)
            {
                return "Số thành viên nhóm hiện tại không khớp với số đã báo danh Võ lâm liên đấu tháng này!";
            }

            /// Nếu các thành viên khác nhóm đăng ký
            foreach (int memberID in teamInfo.Members.Keys)
            {
                if (!teammates.Any(x => x.RoleID == memberID))
                {
                    return "Nhóm hiện tại không khớp với nhóm đã báo danh tham gia Võ lâm liên đấu tháng này!";
                }
            }

            /// Nếu đã đăng ký rồi
            foreach (KPlayer teammate in teammates)
            {
                if (TeamBattle_ActivityScript.RegisterPlayers.TryGetValue(teammate.RoleID, out _))
                {
                    return "Có thành viên đã báo danh thi đấu ở chiến đội khác, không thể báo danh thêm!";
                }
            }

            /// Nếu các thành viên không ở gần bản thân
            foreach (KPlayer teammate in teammates)
            {
                /// Nếu trùng với bản thân
                if (teammate == player)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu khác bản đồ
                if (teammate.CurrentMapCode != player.CurrentMapCode)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> không ở bản đồ hiện tại, không thể báo danh!", teammate.RoleName);
                }
                ///// Khoảng cách
                //float distance = KTGlobal.GetDistanceBetweenPlayers(player, teammate);
                ///// Nếu ở quá xa
                //if (distance > 1000)
                //{
                //    return string.Format("Thành viên <color=#47a9ff>{0}</color> ở quá xa, không thể báo danh!", teammate.RoleName);
                //}
            }

            /// Thông tin trận đấu tiếp theo diễn ra trận đấu tiếp theo
            TeamBattle.BattleInfo.EventTime nextBattleTimes = TeamBattle_ActivityScript.GetNextBattleInfo();
            /// Nếu không có trận đấu tiếp theo
            if (nextBattleTimes == null)
            {
                return "Võ lâm liên đấu tháng này đã kết thúc, hãy quay lại sau.";
            }

            /// Nếu nhóm đã tham gia quá số trận trong lượt vòng tròn
            if (nextBattleTimes.Stage == 0 && teamInfo.TotalBattles >= TeamBattle.Battle.Config.MaxCircleRoundBattles)
            {
                return string.Format("Nhóm đã tham gia đủ {0} trận trong lượt đấu vòng tròn, không thể tham gia thêm nữa!", TeamBattle.Battle.Config.MaxCircleRoundBattles);
            }

            /// Nếu tầng của chiến đội không đủ tham dự vòng này
            if (nextBattleTimes.Stage > teamInfo.Stage)
            {
                return "Chiến đội không được xếp hạng để tham dự trận đấu kế tiếp.";
            }

            /// Thêm vào danh sách đăng ký
            TeamBattle_ActivityScript.RegisterPlayers[player.RoleID] = player;
            /// Trả về kết quả
            return "Báo danh thi đấu trận kế tiếp thành công!";
        }

        /// <summary>
        /// Kiểm tra chiến đội bản thân đã đăng ký tỷ thí lượt này chưa
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsRegisteredForBattle(KPlayer player)
        {
            /// Có tồn tại trong danh sách đăng ký không
            bool isRegistered = false;
            /// Danh sách thành viên nhóm
            List<KPlayer> teammates = player.Teammates;
            /// Duyệt danh sách thành viên
            foreach (KPlayer teammate in teammates)
            {
                /// Nếu đã đăng ký
                if (TeamBattle_ActivityScript.RegisterPlayers.ContainsKey(teammate.RoleID))
                {
                    isRegistered = true;
                    break;
                }
            }

            return isRegistered;
        }

        /// <summary>
        /// Kiểm tra chiến đội bản thân có phần thưởng để nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsHavingAwards(KPlayer player, out TeamBattle.AwardInfo.FinalRankAward awardInfo)
        {
            /// Mặc định không có thưởng
            awardInfo = null;

            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày hôm nay
            int nDate = now.Day;
            /// Nếu nằm trong ngày nhận thưởng
            if (TeamBattle.Award.Config.FromDay <= nDate && nDate <= TeamBattle.Award.Config.ToDay)
            {
                /// Thông tin chiến đội bản thân
                TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player);
                /// Toác
                if (teamInfo == null)
                {
                    return false;
                }
                /// Nếu đã nhận rồi
                else if (!teamInfo.HasAwards)
                {
                    return false;
                }

                /// Thông tin phần thưởng tương ứng
                awardInfo = TeamBattle.Award.FinalRanks.Where(x => x.FromRank <= teamInfo.Rank && x.ToRank >= teamInfo.Rank).FirstOrDefault();
                /// Nếu không có
                if (awardInfo == null)
                {
                    return false;
                }
                /// Nếu có thì trả ra kết quả
                else
                {
                    return true;
                }
            }
            /// Nếu chưa đến ngày nhận thưởng
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hủy đăng ký tỷ thí của chiến đội tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void CancelRegister(KPlayer player)
        {
            /// Toác
            if (player == null)
            {
                return;
            }

            /// Nếu không có nhóm
            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
            {
                return;
            }

            /// Nếu đã đăng ký
            if (TeamBattle_ActivityScript.IsRegisteredForBattle(player))
            {
                /// Chuỗi thông báo
                string msg = string.Format("<color=#80bbff>[{0}]</color> đã rời khỏi hội trường, chiến đội tự hủy báo danh tỷ thí!", player.RoleName);

                /// Danh sách thành viên nhóm
                List<KPlayer> teammates = player.Teammates;
                /// Duyệt danh sách thành viên và thông báo hủy
                foreach (KPlayer teammate in teammates)
                {
                    /// Nếu khác bản thân
                    if (teammate != player)
                    {
                        /// Thông báo hủy
                        KTGlobal.SendDefaultChat(teammate, msg);
                        KTPlayerManager.ShowNotification(teammate, msg);
                    }

                    /// Hủy đăng ký
                    TeamBattle_ActivityScript.RegisterPlayers.TryRemove(teammate.RoleID, out _);
                }
            }
        }

        /// <summary>
        /// Trả về tổng số chiến đội đã ghi danh
        /// </summary>
        /// <returns></returns>
        public static int GetTotalRegisteredTeams()
        {
            return TeamBattle_ActivityScript.RegisterPlayers.Count;
        }

        /// <summary>
        /// Bắt đầu lượt trận đấu
        /// </summary>
        public static void BeginBattle()
        {
            /// Thông tin trận hiện tại
            TeamBattle.BattleInfo.EventTime eventTime = TeamBattle_ActivityScript.GetCurrentBattleInfo();
            /// Nếu không tồn tại
            if (eventTime == null)
            {
                return;
            }

            /// Đánh dấu có tăng hạng của chiến đội tương ứng khi thắng cuộc không
            bool increaseStageForWinnerTeam = eventTime.IncreaseStageForWinnerTeam;
            /// Bậc hiện tại
            int stage = eventTime.Stage;

            /// Thông báo
            KTGlobal.SendSystemEventNotification("Võ lâm liên đấu đã bắt đầu lượt đấu mới, anh hùng hào kiệt nếu bỏ lỡ lượt này có thể đăng ký tỷ thí ở lượt kế tiếp!");

            /// Nếu danh sách đăng ký rỗng
            if (TeamBattle_ActivityScript.RegisterPlayers.Count <= 0)
            {
                return;
            }

            /// Đối tượng random
            Random rand = new Random();
            /// Trộn danh sách đăng ký
            List<KPlayer> randomList = TeamBattle_ActivityScript.RegisterPlayers.Where(x => x.Value != null).OrderBy(x => rand.Next()).ToDictionary(tKey => tKey.Key, tValue => tValue.Value).Values.ToList();

            /// Thứ tự
            int idx = 0;
            /// Cho từng cặp một vào
            while (idx + 1 < randomList.Count)
            {
                /// Thành viên đội 1
                KPlayer firstPlayerTeam = randomList[idx];
                /// Thành viên đội 2
                KPlayer secondPlayerTeam = randomList[idx + 1];

                /// Đánh dấu tổng số lượt đã tham gia của nhóm 1
                /// Thông tin nhóm
                TeamBattle.TeamBattleInfo teamInfo_1 = TeamBattle_ActivityScript.GetTeamInfo(firstPlayerTeam);
                /// Tăng số lượt đã tham gia
                teamInfo_1.TotalBattles++;
                /// Cập nhật vào DB
                TeamBattle_ActivityScript.UpdatePlayerTeamData(firstPlayerTeam, teamInfo_1.Point, teamInfo_1.TotalBattles, teamInfo_1.Stage, teamInfo_1.LastWinTime.Ticks);
                /// Đánh dấu tổng số lượt đã tham gia của nhóm 2
                /// Thông tin nhóm
                TeamBattle.TeamBattleInfo teamInfo_2 = TeamBattle_ActivityScript.GetTeamInfo(secondPlayerTeam);
                /// Tăng số lượt đã tham gia
                teamInfo_2.TotalBattles++;
                /// Cập nhật vào DB
                TeamBattle_ActivityScript.UpdatePlayerTeamData(secondPlayerTeam, teamInfo_2.Point, teamInfo_2.TotalBattles, teamInfo_2.Stage, teamInfo_2.LastWinTime.Ticks);

                /// Trộn 2 nhóm vào với nhau
                List<KPlayer> players = new List<KPlayer>();
                players.AddRange(firstPlayerTeam.Teammates);
                players.AddRange(secondPlayerTeam.Teammates);

                /// Bản đồ tương ứng
                int mapID = TeamBattle.Battle.Config.MapID;
                GameMap map = KTMapManager.Find(mapID);
                /// Tạo mới phụ bản
                KTCopyScene copyScene = new KTCopyScene(map, TeamBattle.Battle.Config.Duration)
                {
                    AllowReconnect = false,
                    EnterPosX = TeamBattle.Battle.Config.PosX,
                    EnterPosY = TeamBattle.Battle.Config.PosY,
                    Level = 100,
                    Name = string.Format("Võ lâm liên đấu - Bậc {0}", stage),
                    OutMapCode = TeamBattle.Config.EnterMapID,
                    OutPosX = TeamBattle.Config.EnterPosX,
                    OutPosY = TeamBattle.Config.EnterPosY,
                    ReliveHPPercent = 100,
                    ReliveMPPercent = 100,
                    ReliveStaminaPercent = 100,
                    ReliveMapCode = TeamBattle.Config.EnterMapID,
                    RelivePosX = TeamBattle.Config.EnterPosX,
                    RelivePosY = TeamBattle.Config.EnterPosY,
                };
                /// Bắt đầu phụ bản với Script tương ứng
                TeamBattle_Script_Main script = new TeamBattle_Script_Main(copyScene, stage, increaseStageForWinnerTeam);

                for (int i = 0; i < players.Count; i++)
                {
                    KPlayer player = players[i];

                    /// Thông tin nhóm đã đăng ký trước đó
                    TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player);
                    /// Nếu không tồn tại
                    if (teamInfo == null)
                    {
                        players.Remove(player);
                        KTPlayerManager.ShowMessageBox(player, "Thông báo,", "Bạn không nằm trong chiến đội không thể tham gia");
                    }
                }

                script.Begin(players);

                /// Ghi log
                LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Auto choose team {0} and team {1} to start battle.", teamInfo_1.Name, teamInfo_2.Name));

                /// Tăng id
                idx += 2;
            }

            /// Nếu còn lẻ ra 1 đội
            if (randomList.Count % 2 == 1)
            {
                /// Thành viên đội may mắn
                KPlayer playerTeam = randomList[randomList.Count - 1];
                /// Thông tin nhóm
                TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(playerTeam);
                /// Tăng số lượt đã tham gia
                teamInfo.TotalBattles++;
                /// Cập nhật vào DB
                TeamBattle_ActivityScript.UpdatePlayerTeamData(playerTeam, teamInfo.Point, teamInfo.TotalBattles, teamInfo.Stage, DateTime.Now.Ticks);
                /// Phát thưởng vào trận
                TeamBattle_ActivityScript.ProcessEnterBattle(playerTeam);
                /// Phát thưởng thắng cuộc
                TeamBattle_ActivityScript.ProcessBattleResult(playerTeam, stage, increaseStageForWinnerTeam);

                /// Thông báo
                string msg = "Chúc mừng chiến đội may mắn lẻ khỏi danh sách, tự động thắng trận!";
                foreach (KPlayer teammate in playerTeam.Teammates)
                {
                    KTPlayerManager.ShowNotification(teammate, msg);
                    KTGlobal.SendDefaultChat(teammate, msg);
                }

                /// Ghi log
                LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Auto choose team {0} win without battle.", teamInfo.Name));
            }

            /// Làm rỗng danh sách đăng ký
            TeamBattle_ActivityScript.RegisterPlayers.Clear();
        }

        /// <summary>
        /// Nhận quà thưởng
        /// </summary>
        /// <param name="player"></param>
        public static string GetAwards(KPlayer player)
        {
            /// Thời gian hiện tại
            DateTime now = DateTime.Now;
            /// Ngày hiện tại
            int nDay = now.Day;
            /// Nếu chưa đến ngày
            if (nDay < TeamBattle.Award.Config.FromDay || nDay > TeamBattle.Award.Config.ToDay)
            {
                return "Hiện chưa phải thời gian nhận thưởng, hãy quay lại sau!";
            }
            /// Nếu không có phần thưởng
            if (!TeamBattle_ActivityScript.IsHavingAwards(player, out TeamBattle.AwardInfo.FinalRankAward awardInfo))
            {
                return "Chiến đội không có phần thưởng!";
            }

            /// Thông tin chiến đội
            TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player);
            /// Toác
            if (teamInfo == null)
            {
                return "Chiến đội không tồn tại!";
            }

            ////Code thêm check số trận
            //if(teamInfo.TotalBattles<15)
            //{
            //    return "Tổng số trận đã tham gia phải lớn hơn 15 mới có thể nhận thưởng!";
            //}

            /// Nếu chưa có nhóm
            if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
            {
                return "Nhóm không tồn tại, không thể báo danh tỷ thí!";
            }
            /// Nếu không phải trưởng nhóm
            else if (player.TeamLeader != player)
            {
                return "Chỉ nhóm trưởng mới có quyền báo danh tỷ thí!";
            }

            /// Thông tin nhóm hiện tại
            List<KPlayer> teammates = player.Teammates;

            /// Nếu số lượng thành viên không đúng
            if (teammates.Count != teamInfo.Members.Count)
            {
                return "Số thành viên nhóm hiện tại không khớp với số đã báo danh Võ lâm liên đấu tháng này!";
            }

            /// Nếu các thành viên khác nhóm đăng ký
            foreach (int memberID in teamInfo.Members.Keys)
            {
                if (!teammates.Any(x => x.RoleID == memberID))
                {
                    return "Nhóm hiện tại không khớp với nhóm đã báo danh tham gia Võ lâm liên đấu tháng này!";
                }
            }

            /// Nếu các thành viên không ở gần bản thân
            foreach (KPlayer teammate in teammates)
            {
                /// Nếu trùng với bản thân
                if (teammate == player)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu khác bản đồ
                if (teammate.CurrentMapCode != player.CurrentMapCode)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> không ở bản đồ hiện tại, không thể nhận thưởng.", teammate.RoleName);
                }
                /// Khoảng cách
                float distance = KTGlobal.GetDistanceBetweenPlayers(player, teammate);
                /// Nếu ở quá xa
                if (distance > 1000)
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> ở quá xa, không thể nhận thưởng.", teammate.RoleName);
                }
            }

            /// Duyệt danh sách thành viên, kiểm tra điều kiện
            foreach (KPlayer teammate in teammates)
            {
                /// Tổng số ô trống cần
                int spacesNeed = 0;
                /// Duyệt danh sách phần thưởng
                foreach (TeamBattle.AwardInfo.ItemInfo itemInfo in awardInfo.Items)
                {
                    /// Tăng số ô trống lên với số lượng vật phẩm tương ứng
                    spacesNeed += KTGlobal.GetTotalSpacesNeedToTakeItem(itemInfo.ID, itemInfo.Quantity);
                }

                /// Nếu túi không đủ ô trống
                if (!KTGlobal.IsHaveSpace(spacesNeed, teammate))
                {
                    return string.Format("Thành viên <color=#47a9ff>{0}</color> túi đồ không đủ chỗ trống, cần sắp xếp tối thiểu <color=green>{1} ô trống</color> trong túi để nhận thưởng!", teammate.RoleName, spacesNeed);
                }
            }

            /// Cập nhật nhóm này đã nhận thưởng rồi
            int ret = TeamBattle_ActivityScript.UpdateTeamAwardState(player, false);
            /// Nếu toác
            if (ret != 1)
            {
                LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Player {0} (ID: {1}) has gotten TeamBattle awards of team {2} (ID: {3}), stage = {4}, point = {5}, rank = {6} failed, ERROR_CODE = {7}!", player.RoleName, player.RoleID, teamInfo.Name, teamInfo.ID, teamInfo.Stage, teamInfo.Point, teamInfo.Rank, ret));
                return "Nhận thưởng thất bại, hãy thử lại sau";
            }

            /// Duyệt danh sách thành viên để thêm phần thưởng
            foreach (KPlayer teammate in teammates)
            {
                /// Thêm kinh nghiệm tương ứng
                KTPlayerManager.AddExp(teammate, awardInfo.Exp);
                /// Thêm danh vọng tương ứng
                KTGlobal.AddRepute(teammate, 501, awardInfo.Repute);

                /// Duyệt danh sách phần thưởng
                foreach (TeamBattle.AwardInfo.ItemInfo itemInfo in awardInfo.Items)
                {
                    /// Thêm phần thưởng tương ứng
                    ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, teammate, itemInfo.ID, itemInfo.Quantity, 0, "TeamBattle_FinalAward", true, 0, false, ItemManager.ConstGoodsEndTime);
                }
            }

            /// Ghi Log
            LogManager.WriteLog(LogTypes.TeamBattle, string.Format("Player {0} (ID: {1}) has gotten TeamBattle awards of team {2} (ID: {3}), stage = {4}, point = {5}, rank = {6} successfully!", player.RoleName, player.RoleID, teamInfo.Name, teamInfo.ID, teamInfo.Stage, teamInfo.Point, teamInfo.Rank));

            /// Trả về kết quả
            return string.Format("Trong sự kiện <color=green>Võ lâm liên đấu</color> tháng này, chiến đội <color=yellow><b>{0}</b></color> của ngươi tham gia đến <color=orange>bậc {1}</color>, tổng điểm <color=#1afff4>{2}</color>, xếp hạng <color=green>{3}</color>, nhận phần thưởng thành công!", teamInfo.Name, teamInfo.Stage, teamInfo.Point, teamInfo.Rank);
        }

        /// <summary>
        /// Trả về top chiến đội được xếp hạng gần đây nhất
        /// </summary>
        /// <returns></returns>
        public static List<TeamBattle.TeamBattleInfo> GetTopTeams()
        {
            string strCmd = string.Format("{0}:{1}", (int)TeamBattleQueryType.GET_TOP_TEAM, TeamBattle.Battle.Config.ToFinalRoundTotalTeams);
            byte[] bytesData = UTF8Encoding.UTF8.GetBytes(strCmd);
            TCPProcessCmdResults result = Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_DB_TEAMBATTLE, bytesData, bytesData.Length, out bytesData, GameManager.LocalServerId);
            if (TCPProcessCmdResults.RESULT_FAILED != result)
            {
                /// Kết quả trả về
                List<TeamBattle.TeamBattleInfo> teamBattleInfo = DataHelper.BytesToObject<List<TeamBattle.TeamBattleInfo>>(bytesData, 6, bytesData.Length - 6);
                /// Trả về kết quả
                return teamBattleInfo;
            }
            return new List<TeamBattle.TeamBattleInfo>();
        }

        #endregion Core Event

        #region Events

        /// <summary>
        /// Sự kiện khi người chơi rời bản đồ hội trường
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public static void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            /// Nếu dịch đến bản đồ phụ bản liên đấu thì thôi
            if (toMap != null && toMap.MapCode == TeamBattle.Battle.Config.MapID)
            {
                return;
            }

            /// Hủy thông tin chiến đội tương ứng
            TeamBattle_ActivityScript.CancelRegister(player);
        }

        #endregion Events
    }
}