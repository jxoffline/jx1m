using MySQLDriverCS;
using Server.Tools;
using System;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        #region CMD_KT_GUILD_CREATE

        /// <summary>
        /// CMD_KT_GUILD_CREATE
        /// Trước đó phỉa đảm bảo check ký tự đặc biệt các kiểu rồi
        /// </summary>
        /// <param name="RoleID1"></param>
        /// <param name="RoleID2"></param>
        /// <param name="RoleID3"></param>
        public string CreateGuild(int RoleID, int ZoneID, int FamilyID, string GuildName)
        {

            // Nếu tên bang hội có rồi thì toác
            if (this.IsGuildExist(this._Database, GuildName))
            {
                return "-3:ERROR";
            }


            // Nếu thằng này đã vào bang rồi thì toác
            if (this.RoleExitsGuild(RoleID))
            {
                return "-4:ERROR";
            }





            //if (this.CreateDatabaseGuild(GuildName, RoleID, ZoneID, FamilyID + ""))
            //{
            //    int GuildID = this.GetGuildID(GuildName);

            //    if (this.UpdateRoleJoinGuild(FamilyID, GuildID, GuildName, (int)GuildRank.Member, 0))
            //    {
            //        Guild _Guild = new Guild();
            //        _Guild.DateCreate = DateTime.Now;
            //        _Guild.Familys = FamilyID + "";
            //        _Guild.GuildID = GuildID;

            //        _Guild.GuildVotes = GetVoteGuild(_Guild.GuildID);

            //        _Guild.GuildMember = this.GetGuildMemberFromFamilyStr(FamilyID + "", _Guild.GuildVotes);

            //        _Guild.GuildShare = this.GetGuildShare(_Guild.GuildMember);

            //        _Guild.GuildName = GuildName;

            //        _Guild.LastCacluationShare = TimeUtil.NOW();

            //        _Guild.Leader = RoleID;

            //        _Guild.MaxWithDraw = 50;

            //        _Guild.MoneyBound = 0;

            //        _Guild.MoneyStore = 1000000;

            //        _Guild.Notify = "Thông báo bang hội";

            //        _Guild.Territorys = new List<Territory>();

            //        _Guild.TotalTerritory = 0;

            //        _Guild.ZoneID = ZoneID;

            //        TotalGuild.TryAdd(GuildID, _Guild);

            //        // TIẾN HÀNH CACHE 1 loạt member sau khi đã tạo xong bang
            //        foreach (GuildMember _member in _Guild.GuildMember.Values)
            //        {
            //            DBRoleInfo roleInfo = _Database.GetDBRoleInfo(_member.RoleID);
            //            if (roleInfo != null)
            //            {
            //                lock (roleInfo)
            //                {
            //                    roleInfo.GuildID = GuildID;
            //                    roleInfo.GuildName = GuildName;
            //                    roleInfo.GuildRank = (int)GuildRank.Member;
            //                }
            //            }
            //        }

            //        // TIẾN HÀNH THĂNG CHỨC BANG CHỦ CHO THẰNG TẠO
            //        this.UpdateDatabaseRoleRankGuiild(RoleID, GuildID, (int)GuildRank.Master);

            //        this.UpdateRoleRankObject(RoleID, GuildID, (int)GuildRank.Master);

            //        return "0:" + _Guild.GuildID + ":" + _Guild.GuildName;
            //    }
            //}

            return "-5:ERROR";
        }

        /// <summary>
        /// Thêm mới 1 bang hội
        /// </summary>
        /// <param name="GuildName"></param>
        /// <param name="RoleID"></param>
        /// <param name="ZoneID"></param>
        /// <param name="FamilySTR"></param>
        /// <returns></returns>
        public bool CreateDatabaseGuild(string GuildName, int RoleID, int ZoneID, string FamilySTR)
        {
            MySQLConnection conn = null;

            try
            {
                string Notify = DataHelper.Base64Encode("Thông báo bang hội");

                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = "Insert into t_guild(GuildName,MoneyBound,MoneyStore,ZoneID,Notify,TotalTerritory,MaxWithDraw,Leader,DateCreate,MoneyBuild) VALUES ('" + GuildName + "',0,1000000," + ZoneID + ",'" + Notify + "',0,50," + RoleID + ",now(),0)";

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

        #endregion CMD_KT_GUILD_CREATE
    }
}