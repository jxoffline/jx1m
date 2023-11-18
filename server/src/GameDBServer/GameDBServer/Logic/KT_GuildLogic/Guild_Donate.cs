using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Linq;

namespace GameDBServer.Logic.GuildLogic
{
    /// <summary>
    /// Quản lý doanate cho bang hội
    /// </summary>
    public partial class GuildManager
    {
        public string DonateGuild(int ROLEDONATE, int GUIILDID, int Money)
        {
            TotalGuild.TryGetValue(GUIILDID, out Guild _GUILD);

            if (_GUILD != null)
            {
                var findmember = _GUILD.GuildMember.Values.Where(x => x.RoleID == ROLEDONATE).FirstOrDefault();

                if (findmember != null)
                {
                    if (_GUILD.MoneyStore + Money > 1000000000)
                    {
                        return "-5:ERROR";
                    }
                    else
                    {
                        if (this.UpdateMoneyStore(Money, GUIILDID))
                        {
                            _GUILD.MoneyStore = _GUILD.MoneyStore + Money;

                            this.PushGuildMsg("Thành viên [" + findmember.RoleName + "] đã cống hiến vào bang hội [" + Money + "] bạc!", GUIILDID, ROLEDONATE, findmember.RoleName);

                            return string.Format("1:{0}", _GUILD.MoneyStore);
                        }
                    }
                }
            }
            else
            {
                return "-2:ERROR";
            }

            return "-100:ERROR";
        }

        public bool UpdateMoneyStore(int MoneyAdd, int GuildId)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                MySQLConnection conn = null;

                try
                {
                    conn = this._Database.DBConns.PopDBConnection();

                    string cmdText = string.Format("Update t_guild set MoneyStore =  MoneyStore + " + MoneyAdd + " where guildid = " + GuildId + "");

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
            }

            return false;
        }
    }
}