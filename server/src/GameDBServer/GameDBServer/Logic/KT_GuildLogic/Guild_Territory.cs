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
        public MiniGuildInfo GetMiniGuildInfo(int GuildID)
        {
            MiniGuildInfo _Guild = new MiniGuildInfo();

            try
            {
                TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

                if (_OutGuild != null)
                {
                    _Guild.GuildId = _OutGuild.GuildID;
                    _Guild.GuildName = _OutGuild.GuildName;

                    _Guild.MoneyStore = _OutGuild.MoneyStore;

                    _Guild.TotalPrestige = _OutGuild.GuildMember.Values.Sum(x => x.Prestige);

                    _Guild.TotalMember = _OutGuild.GuildMember.Values.Count;
                }
            }
            catch (Exception ex)
            {
            }

            return _Guild;
        }

        /// <summary>
        /// Lấy ra thông tin lãnh thổ
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public TerritoryInfo GetGuildTerritoryInfo(int GuildID)
        {
            TerritoryInfo _Total = new TerritoryInfo();

            List<Territory> _TotalTerritory = new List<Territory>();

            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Select ID,MapID,MapName,GuildID,Star,Tax,ZoneID,IsMainCity from t_territory where GuildID = " + GuildID + " order by ID desc");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    Territory _Territory = new Territory();

                    _Territory.ID = Convert.ToInt32(reader["ID"].ToString());
                    _Territory.MapID = Convert.ToInt32(reader["MapID"].ToString());

                    _Territory.MapName = DataHelper.Base64Decode(reader["MapName"].ToString());

                    _Territory.GuildID = Convert.ToInt32(reader["GuildID"].ToString());

                    _Territory.Star = Convert.ToInt32(reader["Star"].ToString());

                    _Territory.Tax = Convert.ToInt32(reader["Tax"].ToString());

                    _Territory.ZoneID = Convert.ToInt32(reader["ZoneID"].ToString());

                    _Territory.IsMainCity = Convert.ToInt32(reader["IsMainCity"].ToString());

                    //FILL TÊN BANG VÀO ĐÂY ĐỂ PHỤC VỤ CHO TRANH ĐOẠT LÃNH THỔ
                    if (TotalGuild.TryGetValue(_Territory.GuildID, out Guild _Out))
                    {
                        _Territory.GuildName = _Out.GuildName;
                    }

                    _TotalTerritory.Add(_Territory);
                }

                _Total.Territorys = _TotalTerritory;
                _Total.TerritoryCount = _TotalTerritory.Count;

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

            return _Total;
        }

        public bool RemoveAllMainCity(int GuildId)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                MySQLConnection conn = null;

                try
                {
                    conn = this._Database.DBConns.PopDBConnection();

                    string cmdText = string.Format("Update t_territory set IsMainCity = 0 where GuildID = " + GuildId + "");

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

        /// <summary>
        /// Xóa bõ lãnh thổ
        /// </summary>
        /// <param name="MapID"></param>
        /// <returns></returns>
        public bool RemoveTerritory(int MapID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                string cmdText = string.Format("Delete from t_territory where MapID = " + MapID + "");

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

        public bool IsExitsTerritory(int MapID)
        {
            MySQLConnection conn = null;
            try
            {
                bool isExist = false;

                conn = this._Database.DBConns.PopDBConnection();
                /// Kiểm tra tồn tại chưa
                string cmdText = "Select * from t_territory where MapID = " + MapID;

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                MySQLDataReader reader = cmd.ExecuteReaderEx();
                /// Nếu có dữ liệu nghĩa là đã tồn tại
                isExist = reader.Read();

                cmd.Dispose();
                /// Giải phóng
                cmd = null;

                return isExist;
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
        /// Thêm hoặc update
        /// </summary>
        /// <param name="MapID"></param>
        /// <param name="MapName"></param>
        /// <param name="GuildID"></param>
        /// <param name="Start"></param>
        /// <param name="Tax"></param>
        /// <param name="ZoneID"></param>
        /// <param name="IsMainCity"></param>
        /// <returns></returns>
        public bool InsertOrUpdate(int MapID, string MapName, int GuildID, int Start, int Tax, int ZoneID, int IsMainCity)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();

                bool IsExit = this.IsExitsTerritory(MapID);

                string cmdText = "";

                if (IsExit)
                {
                    cmdText = "Update t_territory set GuildID = " + GuildID + ",Star = " + Start + ",Tax = " + Tax + ",ZoneID = " + ZoneID + ",IsMainCity = " + IsMainCity + " where MapID = " + MapID;
                }
                else
                {
                    cmdText = "Insert into t_territory(MapID,MapName,GuildID,Star,Tax,ZoneID,IsMainCity) VALUES (" + MapID + ",'" + MapName + "'," + GuildID + "," + Start + "," + Tax + "," + ZoneID + "," + IsMainCity + ")";
                }

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

        public bool UpdateTerritoryMainCity(int MapID, int GuildId)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                MySQLConnection conn = null;

                try
                {
                    conn = this._Database.DBConns.PopDBConnection();

                    string cmdText = string.Format("Update t_territory set IsMainCity = 1 where guildid = " + GuildId + " and MapID = " + MapID + "");

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

        public bool UpdateTerritoryTax(int MapID, int GuildId, int Tax)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                MySQLConnection conn = null;

                try
                {
                    conn = this._Database.DBConns.PopDBConnection();

                    string cmdText = string.Format("Update t_territory set Tax = " + Tax + " where guildid = " + GuildId + " and MapID = " + MapID + "");

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

        public string SetMainCity(int GuilID, int MapID)
        {
            TotalGuild.TryGetValue(GuilID, out Guild _OutGuild);

            if (_OutGuild == null)
            {
                return "-1:ERROR";
            }

            var find = _OutGuild.Territorys.Where(x => x.MapID == MapID).FirstOrDefault();
            if (find != null)
            {
                if (_OutGuild.MoneyStore < 3000000)
                {
                    return "-3:ERROR";
                }
                else
                {
                    _OutGuild.MoneyStore = _OutGuild.MoneyStore - 3000000;

                    if (this.UpdateMoneyStore(-3000000, GuilID))
                    {
                        if (this.RemoveAllMainCity(GuilID))
                        {
                            // Set tất cả không có thành chính
                            foreach (Territory _Terorry in _OutGuild.Territorys)
                            {
                                _Terorry.IsMainCity = 0;
                            }
                            // set thành chính cho thành vừa nãy
                            if (this.UpdateTerritoryMainCity(MapID, GuilID))
                            {
                                find.IsMainCity = 1;
                            }
                        }
                        return "100:ERROR";
                    }
                }
            }
            else
            {
                return "-2:ERROR";
            }

            return "-100:ERROR";
        }

        public string SetTaxCity(int GuilID, int MapID, int TaxID)
        {
            TotalGuild.TryGetValue(GuilID, out Guild _OutGuild);

            if (_OutGuild == null)
            {
                return "-1:ERROR";
            }

            var find = _OutGuild.Territorys.Where(x => x.MapID == MapID).FirstOrDefault();
            if (find != null)
            {
                if (this.UpdateTerritoryTax(MapID, GuilID, TaxID))
                {
                    find.Tax = TaxID;
                }

                return "100:ERROR";
            }
            else
            {
                return "-2:ERROR";
            }
        }
    }
}