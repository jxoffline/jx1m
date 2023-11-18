using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        public string DisbandGuild(int GuildID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            // nếu như bang không tồn tịa thì thôi
            //if (_OutGuild == null)
            //{
            //    return "-1:ERROR";
            //}

            //string[] Pram = _OutGuild.Familys.Split('|');

            //foreach (string FamilyIDStr in Pram)
            //{
            //    int FamilyID = Int32.Parse(FamilyIDStr);

            //    Family _Find = FamilyManager.getInstance().GetFamily(FamilyID);
            //    if (_Find != null)
            //    {
            //        FamilyManager.getInstance().PushFamilyMsg("Bang hội [" + _OutGuild.GuildName + "] đã giải tán,Gia tộc không còn bang hội", FamilyID, 0, "");
            //    }
            //}

            //if (TotalGuild.TryRemove(GuildID, out Guild _out))
            //{
            //    foreach (GuildMember _member in _out.GuildMember.Values)
            //    {
            //        DBRoleInfo roleInfo = _Database.GetDBRoleInfo(_member.RoleID);
            //        if (roleInfo != null)
            //        {
            //            lock (roleInfo)
            //            {
            //                roleInfo.GuildID = 0;
            //                roleInfo.GuildName = "";
            //                roleInfo.GuildRank = (int)GuildRank.Member;
            //            }
            //        }
            //    }
            //    this.RemoveAllRole(GuildID);
            //    this.DeleteAllTerritory(GuildID);
            //    this.DeleteAllVote(GuildID);
            //    this.DeleteGuild(GuildID);

            //    return "100:ERROR";
            //}

            return "-100:ERROR";
        }

        public bool DeleteAllVote(int GuildID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Delete from t_voteguild where GuildID = " + GuildID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }

        public bool DeleteGuild(int GuildID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Delete from t_guild where GuildID = " + GuildID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }

        public bool RemoveAllRole(int GuildID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Update t_roles set guildname = '',guildid = 0,guildrank =0,guildmoney =0 where guildid = " + GuildID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }

        public bool DeleteAllTerritory(int GuildID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Delete from t_territory where GuildID = " + GuildID + "");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }
    }
}