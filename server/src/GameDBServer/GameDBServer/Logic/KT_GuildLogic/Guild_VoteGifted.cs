using GameDBServer.Core;
using GameDBServer.DB;
using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        /// <summary>
        /// Lấy ra số phiếu của thằng bầu
        /// </summary>
        /// <param name="RANK"></param>
        /// <returns></returns>
        public int GetVoteInRank(int RANK)
        {
            if (RANK == (int)GuildRank.Ambassador)
            {
                return 10;
            }
            else if (RANK == (int)GuildRank.Elite)
            {
                return 2;
            }
            else if (RANK == (int)GuildRank.Master)
            {
                return 50;
            }
            else if (RANK == (int)GuildRank.Member)
            {
                return 1;
            }
            else if (RANK == (int)GuildRank.ViceMaster)
            {
                return 25;
            }

            return 1;
        }

        /// <summary>
        /// Lấy ra danh sách thành viên ưu túu
        /// </summary>
        /// <returns></returns>
        public GuildVoteInfo GetVoteInfo(int RoleID, int GuildID, int PageIndex)
        {
            GuildVoteInfo _Guild = new GuildVoteInfo();

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                int END = PageIndex * PageDisplay;

                int START = END - PageDisplay;

                List<GuildMember> TotalMember = _OutGuild.GuildMember.Values.OrderByDescending(x => x.TotalVote).ToList();

                int totalPage = _OutGuild.GuildMember.Count / PageDisplay + 1;
                if (_OutGuild.GuildMember.Count % PageDisplay == 0)
                {
                    totalPage--;
                }
                _Guild.TotalPage = totalPage;
                _Guild.PageIndex = PageIndex;

                if (TotalMember.Count < START)
                {
                    _Guild.GuildMember = TotalMember;
                }

                if (TotalMember.Count > START && TotalMember.Count < END)
                {
                    int RANGER = TotalMember.Count - START;
                    _Guild.GuildMember = TotalMember.GetRange(START, RANGER);
                }
                else
                {
                    _Guild.GuildMember = TotalMember.GetRange(START, PageDisplay);
                }

                int idx = (PageIndex - 1) * PageDisplay + 1;
                foreach (GuildMember meberInfo in _Guild.GuildMember)
                {
                    meberInfo.VoteRank = idx;
                    idx++;
                }

                //Lấy ra thời gian hiện tại
                DayOfWeek Day = DateTime.Now.DayOfWeek;
                int Days = 0;
                if (Day == DayOfWeek.Sunday)
                {
                    Days = 6;
                }
                else
                {
                    Days = Day - DayOfWeek.Monday;
                }

                DateTime WeekStartDate = DateTime.Now.AddDays(-Days);
                DateTime EndWeek = WeekStartDate.AddDays(6);

                WeekStartDate = new DateTime(WeekStartDate.Year, WeekStartDate.Month, WeekStartDate.Day);

                EndWeek = new DateTime(EndWeek.Year, EndWeek.Month, EndWeek.Day, 23, 59, 50);

                _Guild.Start = WeekStartDate.ToString("dd/MM/yyyy");
                _Guild.End = EndWeek.ToString("dd/MM/yyyy");

                _Guild.WEEID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

                _Guild.VoteFor = "";

                var findvotefor = _OutGuild.GuildVotes.Where(x => x.RoleVote == RoleID && x.WeekID == _Guild.WEEID).FirstOrDefault();

                if (findvotefor != null)
                {
                    DBRoleInfo roleInfo = _Database.GetDBRoleInfo(findvotefor.RoleReviceVote);

                    if (roleInfo != null)
                    {
                        _Guild.VoteFor = roleInfo.RoleName;
                    }
                }
            }

            return _Guild;
        }

        /// <summary>
        /// Lớp quản lý vote của người chơi này cho người chơi khác
        /// </summary>
        public string DoVote(int RoleVote, int RoleReviceVote, int GuildID)
        {
            int WEEDID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);
            if (_OutGuild != null)
            {
                var findmember1 = _OutGuild.GuildMember.Values.Where(x => x.RoleID == RoleVote).FirstOrDefault();

                if (findmember1 != null)
                {
                    var findvote = _OutGuild.GuildVotes.Where(x => x.WeekID == WEEDID && x.RoleVote == RoleVote).FirstOrDefault();

                    if (findvote != null)
                    {
                        return "-1:ERROR";
                    }
                    else
                    {
                        var findMember2 = _OutGuild.GuildMember.Values.Where(x => x.RoleID == RoleReviceVote).FirstOrDefault();

                        if (findMember2 != null)
                        {
                            // lấy ra số lượng vote của thằng 1
                            int VOTECOUNT = this.GetVoteInRank(findmember1.Rank);

                            if (this.AddVoteRoleVote(GuildID, RoleVote, _OutGuild.ZoneID, VOTECOUNT, RoleReviceVote))
                            {
                                GuildVote _Vote = new GuildVote();
                                _Vote.ID = RoleVote;
                                _Vote.GuildID = GuildID;
                                _Vote.RoleReviceVote = RoleReviceVote;
                                _Vote.RoleVote = RoleVote;
                                _Vote.VoteCount = VOTECOUNT;
                                _Vote.WeekID = WEEDID;
                                _Vote.ZoneID = _OutGuild.ZoneID;

                                _OutGuild.GuildVotes.Add(_Vote);

                                findMember2.TotalVote = findMember2.TotalVote + VOTECOUNT;

                                return string.Format("1:{0}", findMember2.RoleName);
                            }
                        }
                    }
                }
            }

            return "-100:ERROR";
        }

        public bool AddVoteRoleVote(int GuildID, int RoleID, int ZoneID, int VoteCount, int RoleReviceVote)
        {
            MySQLConnection conn = null;

            try
            {
                int WEEDID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = "Insert into t_voteguild(ZoneID,GuildID,RoleVote,VoteCount,WeekID,RoleReviceVote) VALUES (" + ZoneID + "," + GuildID + ", " + RoleID + "," + VoteCount + "," + WEEDID + "," + RoleReviceVote + ")";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG :" + ex.ToString());

                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }
        }
    }
}