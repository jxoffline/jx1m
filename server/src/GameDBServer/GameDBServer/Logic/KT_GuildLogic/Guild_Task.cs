using GameDBServer.Core;
using GameDBServer.DB;
using MySQLDriverCS;
using Server.Data;
using System;
using System.Security.Cryptography;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        #region GuildTask

        public GuildTask GetGuildTask(int GuildID)
        {
            GuildTask _Task = new GuildTask();

            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = "Select GuildID,TaskID,TaskCountInDay,TaskValue,DayCreate from t_guild_task where GuildID = " + GuildID + "";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    // ID Bang hội
                    _Task.GuildID = Convert.ToInt32(reader["GuildID"].ToString());
                    // ID nhiệm vụ
                    _Task.TaskID = Convert.ToInt32(reader["TaskID"].ToString());
                    // Task Value
                    _Task.TaskValue = Convert.ToInt32(reader["TaskValue"].ToString());
                    // Ngày nhận nhiệm vụ
                    _Task.DayCreate = Convert.ToInt32(reader["DayCreate"].ToString());

                    _Task.TaskCountInDay = Convert.ToInt32(reader["TaskCountInDay"].ToString());
                }
                cmd.Dispose();
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return _Task;
        }

        /// <summary>
        /// Cập nhật value cho nhiệm vụ
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="TaskID"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool UpdateTaskValue(int GuildID, int TaskID, int Value, int CountInDay, bool IsUpdate)
        {
            // Lấy ra ngày hiện tại là ngày nào
            int nDate = TimeUtil.NowDateTime().DayOfYear;

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                // Nếu là update giá trị cho task
                if (IsUpdate)
                {
                    if (_OutGuild.Task.TaskID == TaskID)
                    {
                        _OutGuild.Task.TaskValue = Value;
                        _OutGuild.Task.TaskCountInDay = CountInDay;

                        string Update = "Update t_guild_task set TaskID = " + TaskID + ",TaskValue = " + Value +
                                        ",DayCreate = " + nDate + ",TaskCountInDay = " + CountInDay +
                                        " where GuildID = " + GuildID + "";

                        if (DBWriter.ExecuteSqlScript(Update))
                        {
                            _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _OUT);
                            if(_OUT!=null)
                            {
                                _OUT.Task = _OutGuild.Task;
                              
                            }    
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                } // Nếu là tạo 1 nhiệm vụ mới
                else
                {
                    GuildTask _Task = new GuildTask();
                    _Task.TaskID = TaskID;
                    _Task.TaskValue = Value;
                    _Task.TaskCountInDay = CountInDay;
                    _Task.DayCreate = nDate;
                    _Task.GuildID = GuildID;

                    //Set nhiệm vụ này cho bang này
                    _OutGuild.Task = _Task;

                    string Sql = "INSERT INTO t_guild_task (GuildID, TaskID, TaskValue, DayCreate,TaskCountInDay) VALUES (" + GuildID + "," + TaskID + " ," + Value + ", " + nDate + "," + CountInDay + ") ON DUPLICATE KEY UPDATE TaskID=" + TaskID + ",TaskValue=" + Value + ",DayCreate=" + nDate + ",TaskCountInDay=" + CountInDay + "";
                    if (DBWriter.ExecuteSqlScript(Sql))
                    {
                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _OUT);
                        if (_OUT != null)
                        {
                            _OUT.Task = _Task;
                            
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion GuildTask
    }
}