using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Logic.FamilyLogic;
using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameDBServer.Logic.GuildLogic
{
    public class GuildManager
    {
        private static GuildManager instance = new GuildManager();

        public static int MaxFamilyCanJoin = 3;

        public DBManager _Database = null;

        public ConcurrentDictionary<int, Guild> TotalFamily = new ConcurrentDictionary<int, Guild>();

        public static GuildManager getInstance()
        {
            return instance;
        }

        public void Setup(DBManager _Db)
        {
            this._Database = _Db;
        }

        public void LoadAllGuild()
        {
            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Select GuildID,GuildName,MoneyBound,MoneyStore,ZoneID,Notify,TotalTerritory,MaxWithDraw,Leader,DateCreate,FamilyMember from t_guild order by GuildID desc");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    Guild _Guild = new Guild();

                    _Guild.GuildID = Convert.ToInt32(reader["GuildID"].ToString());
                    _Guild.GuildName = reader["GuildName"].ToString();
                    _Guild.MoneyBound = Convert.ToInt32(reader["MoneyBound"].ToString());
                    _Guild.MoneyStore = Convert.ToInt32(reader["MoneyStore"].ToString());

                    _Guild.ZoneID = Convert.ToInt32(reader["ZoneID"].ToString());

                    _Guild.Notify = reader["Notify"].ToString();

                    _Guild.TotalTerritory = Convert.ToInt32(reader["TotalTerritory"].ToString());

                    _Guild.MaxWithDraw = Convert.ToInt32(reader["MaxWithDraw"].ToString());

                    _Guild.Leader = Convert.ToInt32(reader["Leader"].ToString());

                    _Guild.DateCreate = DateTime.Parse(reader["DateCreate"].ToString());

                    string FamilyMember = reader["FamilyMember"].ToString();

                    _Guild.Familys = FamilyMember;

                    _Guild.GuildVotes = GetVoteGuild(_Guild.GuildID);

                    _Guild.GuildMember = this.GetGuildMemberFromFamilyStr(FamilyMember);

                    //_Guild.GuildShare = this.GetGuildShare(_Guild.GuildID);
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
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
        /// Tính toán lại cỏ phần
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public List<GuildShare> GetGuildShare(List<GuildMember> _ListMember)
        {
            List<GuildShare> _TotalGuildShare = new List<GuildShare>();

            /// Lấy ra tổng số tiền của bang này
            double MaxMoneyGuild = _ListMember.Sum(x => x.GuildMoney);

            if(_ListMember.Count==0)
            {
                return _TotalGuildShare;
            }    

            List<GuildMember> TmpGuildMember = _ListMember.Where(x => x.GuildMoney > 0).ToList();

            foreach (GuildMember member in _ListMember)
            {
                GuildShare _Share = new GuildShare();

                double Percent = Math.Round(((member.GuildMoney*100) / MaxMoneyGuild),2);

                _Share.FactionID = member.FactionID;
                _Share.FamilyID = member.FamilyID;
                _Share.FamilyName = member.FamilyName;
                _Share.GuildID = member.GuildMoney;
                _Share.Rank = member.Rank;
                _Share.RoleID = member.RoleID;
                _Share.RoleLevel = member.Level;
                _Share.RoleName = member.RoleName;
                _Share.Share = Percent;
                _Share.ZoneID = member.ZoneID;



            }

            return _TotalGuildShare;
        }

        public List<GuildVote> GetVoteGuild(int GuildID)
        {
            int WEEKID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

            List<GuildVote> _TotalVoteGuild = new List<GuildVote>();

            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Select ID,ZoneID,GuildID,RoleVote,VoteCount,WeekID,RoleReviceVote from t_voteguild where WeekID = " + WEEKID + " order by ID desc");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    GuildVote _GuildVote = new GuildVote();

                    _GuildVote.ID = Convert.ToInt32(reader["ID"].ToString());
                    _GuildVote.ZoneID = Convert.ToInt32(reader["ZoneID"].ToString());

                    _GuildVote.GuildID = Convert.ToInt32(reader["GuildID"].ToString());

                    _GuildVote.RoleVote = Convert.ToInt32(reader["RoleVote"].ToString());

                    _GuildVote.VoteCount = Convert.ToInt32(reader["VoteCount"].ToString());

                    _GuildVote.WeekID = Convert.ToInt32(reader["WeekID"].ToString());

                    _GuildVote.RoleReviceVote = Convert.ToInt32(reader["RoleReviceVote"].ToString());

                    _TotalVoteGuild.Add(_GuildVote);
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return _TotalVoteGuild;
        }

        /// <summary>
        /// Lấy ra toàn bộ danh sách gia tộ có trong danh sách
        /// </summary>
        /// <param name="FamilyStr"></param>
        /// <returns></returns>
        public List<GuildMember> GetGuildMemberFromFamilyStr(string FamilyStr)
        {
            List<GuildMember> _TotalMember = new List<GuildMember>();

            string[] Pram = FamilyStr.Split('|');

            foreach (string FamilyIDStr in Pram)
            {
                int FamilyID = Int32.Parse(FamilyIDStr);

                Family _Find = FamilyManager.getInstance().GetFamily(FamilyID);
                if (_Find != null)
                {
                    this.FillMemberToGuild(_Find.FamilyID, _TotalMember);
                }
            }

            return _TotalMember;
        }

        public void FillMemberToGuild(int FamilyID, List<GuildMember> _Input)
        {
            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,level,guildrank,guildmoney,,zoneidroleprestige,familyname,familyid from t_roles where familyid = " + FamilyID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    GuildMember _GuidMember = new GuildMember();

                    _GuidMember.RoleID = Convert.ToInt32(reader["rid"].ToString());

                    _GuidMember.FactionID = Convert.ToInt32(reader["occupation"].ToString());

                    _GuidMember.GuildID = Convert.ToInt32(reader["guildid"].ToString());

                    _GuidMember.FamilyID = Convert.ToInt32(reader["familyid"].ToString());

                    _GuidMember.FamilyName = reader["familyname"].ToString();

                    _GuidMember.GuildMoney = Convert.ToInt32(reader["guildmoney"].ToString());

                    _GuidMember.Level = Convert.ToInt32(reader["level"].ToString());

                    _GuidMember.Prestige = Convert.ToInt32(reader["roleprestige"].ToString());

                    _GuidMember.Rank = Convert.ToInt32(reader["guildrank"].ToString());

                    _GuidMember.RoleName = reader["rname"].ToString();

                    _GuidMember.OnlienStatus = 0;

                    //_GuidMember.ZoneID = 

                    _Input.Add(_GuidMember);
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
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
        /// Create Guild
        /// </summary>
        /// <param name="RoleID1"></param>
        /// <param name="RoleID2"></param>
        /// <param name="RoleID3"></param>
        public int CreateGuild(int RoleID, int FamilyID)
        {
            FamilyMember _Member1 = FamilyManager.getInstance().GetMember(RoleID, FamilyID);

            // Thành viên đầu tiên không có tộc
            if (_Member1 == null)
            {
                return -1;
            }

            // Nếu không phải bang chủ
            if (_Member1.Rank != (int)FamilyRank.Master)
            {
                return -2;
            }

            return 0;
        }

        /// <summary>
        /// Check xem Guild đã tồn tại chưa
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="GuildName"></param>
        /// <returns></returns>
        public static bool IsGuildExist(DBManager dbMgr, string GuildName)
        {
            using (MyDbConnection3 conn = new MyDbConnection3())
            {
                string cmdText = string.Format("SELECT count(*) from t_guild where GuildName='" + GuildName + "'");
                return conn.GetSingleInt(cmdText) > 0;
            }
        }

        /// <summary>
        /// Check xem có guild ID không
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="GuildName"></param>
        /// <returns></returns>
        public static int GetGuildID(DBManager dbMgr, int GuildName)
        {
            int GuildID = -1;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT GuildID from t_guild where GuildName = '" + GuildName + "'");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    GuildID = Global.SafeConvertToInt32(reader["GuildID"].ToString());
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }

            return GuildID;
        }

        /// <summary>
        /// Update rank cho 1 thành viên
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="GuildID"></param>
        /// <param name="Rank"></param>
        public void UpdateRoleRankGuiild(int RoleID, int GuildID, int Rank)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Update t_roles set guildrank = " + Rank + " where guildid = " + GuildID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG :" + ex.ToString());
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
        ///  Update cho 1 tộc join vào BANG
        /// </summary>
        /// <param name="FamilyID"></param>
        /// <param name="GUILDID"></param>
        /// <param name="GUILDNAME"></param>
        /// <param name="GUILDRANK"></param>
        /// <param name="MONEYGUILD"></param>
        public void UpdateRoleJoinGuild(int FamilyID, int GUILDID, string GUILDNAME, int GUILDRANK, int MONEYGUILD)
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
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG :" + ex.ToString());
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