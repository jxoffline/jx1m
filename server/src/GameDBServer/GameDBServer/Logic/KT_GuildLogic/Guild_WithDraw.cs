using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        public bool ChangeMaxWithDraw(int Percent, int GuildId)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                if (this.UpdateWithDraw(Percent, GuildId))
                {
                    _OutGuild.MaxWithDraw = Percent;

                    return true;
                }
            }

            return false;
        }

        public bool UpdateMemberMoney(int Money, int GuildID, int RoleID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);
            if (_OutGuild != null)
            {
                _OutGuild.GuildMember.TryGetValue(RoleID, out GuildMember _OutMember);
                if (_OutMember != null)
                {
                    _OutMember.GuildMoney = _OutMember.GuildMoney + Money;

                    return UpdateGuildMoney(Money, GuildID, RoleID);
                }
            }

            return false;
        }

        public bool UpdateGuildMoney(int Money, int GuildId, int RoleiD)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Update t_roles set guildmoney = guildmoney + " + Money + " where rid = " + RoleiD + " and guildid = " + GuildId + "");

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

        public bool UpdateWithDraw(int Percent, int GuildId)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Update t_guild set MaxWithDraw = " + Percent + " where GuildID = " + GuildId + "");

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