using MySQLDriverCS;
using Server.Tools;
using System;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        public int MAXFamilyCanJoin = 5;

        /// <summary>
        ///  Update cho 1 tộc join vào BANG
        /// </summary>
        /// <param name="FamilyID"></param>
        /// <param name="GUILDID"></param>
        /// <param name="GUILDNAME"></param>
        /// <param name="GUILDRANK"></param>
        /// <param name="MONEYGUILD"></param>
        public bool UpdateRoleJoinGuild(int FamilyID, int GUILDID, string GUILDNAME, int GUILDRANK, int MONEYGUILD)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Update t_roles set guildname = '" + GUILDNAME + "',guildid = " + GUILDID + ",guildrank = " + GUILDRANK + ",guildmoney = " + MONEYGUILD + " where familyid = " + FamilyID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());

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

        /// <summary>
        /// Tham gia 1 bang hội nào đó
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="ZoneID"></param>
        /// <param name="FamilyID"></param>
        /// <param name="GuilID"></param>
        /// <returns></returns>
        public string JoinGuild(int RoleID, int FamilyID, int GuilID)
        {
            //FamilyMember _Member1 = FamilyManager.getInstance().GetMember(RoleID, FamilyID);

            //// Thành viên đầu tiên không có tộc
            //if (_Member1 == null)
            //{
            //    return "-1:ERROR";
            //}

            //// Nếu không phải tộc trưởng
            //if (_Member1.Rank != (int)FamilyRank.Master)
            //{
            //    return "-2:ERROR";
            //}

            //// Check xem bang có tồn tại không
            //TotalGuild.TryGetValue(GuilID, out Guild _Guild);

            //if (_Guild == null)
            //{
            //    return "-3:ERROR";
            //}

            //// Check xem người chơi đã có bang nào chưa

            //if (this.RoleExitsGuild(RoleID))
            //{
            //    return "-4:ERROR";
            //}

            //int Total = _Guild.Familys.Split('|').Count();

            //if (Total + 1 > MAXFamilyCanJoin)
            //{
            //    return "-5:ERROR";
            //}

            //if (this.UpdateRoleJoinGuild(FamilyID, _Guild.GuildID, _Guild.GuildName, (int)GuildRank.Member, 0))
            //{
            //    //Update lại thành viên
            //    _Guild.Familys = _Guild.Familys + "|" + FamilyID;

            //    if (this.UpdateFamilyStr(_Guild.GuildID, _Guild.Familys))
            //    {
            //        // Update lại vote
            //        _Guild.GuildVotes = GetVoteGuild(_Guild.GuildID);

            //        // Update lại danh sách thành viên
            //        _Guild.GuildMember = this.GetGuildMemberFromFamilyStr(_Guild.Familys, _Guild.GuildVotes);

            //        foreach (GuildMember _member in _Guild.GuildMember.Values)
            //        {
            //            // SET LẠI CAHCE CHO BỌN MỚI VÀO
            //            if (_member.FamilyID == FamilyID)
            //            {
            //                DBRoleInfo roleInfo = _Database.GetDBRoleInfo(_member.RoleID);
            //                if (roleInfo != null)
            //                {
            //                    lock (roleInfo)
            //                    {
            //                        roleInfo.GuildID = _Guild.GuildID;
            //                        roleInfo.GuildName = _Guild.GuildName;
            //                        roleInfo.GuildRank = (int)GuildRank.Member;
            //                    }
            //                }
            //            }
            //        }
            //        //Update lại cổ tức
            //        _Guild.GuildShare = this.GetGuildShare(_Guild.GuildMember);

            //        // Update cho thằng này làm trưởng lão
            //        this.UpdateDatabaseRoleRankGuiild(RoleID, _Guild.GuildID, (int)GuildRank.Ambassador);

            //        // Update cho thằng này làm trưởng lão
            //        this.UpdateRoleRankObject(RoleID, _Guild.GuildID, (int)GuildRank.Ambassador);

            //        string Msg = "Gia tộc đã tham gia bang hội [" + _Guild.GuildName + "]";

            //        // Gửi thông báo cho cả tộc biết đã tham gia bang hội mới
            //        FamilyManager.getInstance().PushFamilyMsg(Msg, FamilyID, RoleID, "");

            //        return "100:" + _Guild.GuildID + ":" + _Guild.GuildName;
            //    }
            //}

            // Nếu  thành công thì trả về ID bang và tên bang hội để update vào RoleData
            // return "-100:" + _Guild.GuildID + ":" + _Guild.GuildName;

            return "";
        }
    }
}