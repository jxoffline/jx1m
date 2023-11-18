using GameDBServer.DB;
using GameDBServer.Logic.GuildLogic;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Protocol;
using Server.Tools;
using System;
using System.Text;
using System.Threading;

namespace GameDBServer.Logic.Name
{
    /// <summary>
    /// Mã lỗi trả về khi đổi tên
    /// </summary>
    public enum ChangeNameError
    {
        /// <summary>
        /// Thành công
        /// </summary>
        Success = 0,

        /// <summary>
        /// Tên không hợp lệ
        /// </summary>
        InvalidName = 1,

        /// <summary>
        /// Lỗi DB
        /// </summary>
        DBFailed = 2,

        /// <summary>
        /// Tên đã được sử dụng
        /// </summary>
        NameAlreayUsed = 3,

        /// <summary>
        /// Nhân vật không tồn tại
        /// </summary>
        NotContainRole = 4,
    }

    /// <summary>
    /// Quản lý đổi tên
    /// </summary>
    public class NameManager : SingletonTemplate<NameManager>
    {
        /// <summary>
        /// Quản lý đổi tên
        /// </summary>
        private NameManager()
        { }

        /// <summary>
        /// Thực hiện đổi tên
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcChangeName(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                string uid = fields[0];
                int zoneId = Convert.ToInt32(fields[1]);
                int roleId = Convert.ToInt32(fields[2]);
                string newName = fields[3];

                /// Tên cũ
                string oldName = "";
                /// Nếu thất bại thì trả về mã gì
                string failedMsg = "";

                DBUserInfo userInfo = null;
                DBRoleInfo roleInfo = null;
                ChangeNameError cne = ChangeNameError.Success;

                do
                {
                    userInfo = dbMgr.GetDBUserInfo(uid);
                    if (userInfo == null)
                    {
                        failedMsg = "Can not find account " + uid;
                        cne = ChangeNameError.DBFailed;
                        break;
                    }

                    lock (userInfo)
                    {
                        int i = 0;
                        for (; i < userInfo.ListRoleIDs.Count; ++i)
                        {
                            if (userInfo.ListRoleZoneIDs[i] == zoneId && userInfo.ListRoleIDs[i] == roleId)
                            {
                                break;
                            }
                        }

                        if (i == userInfo.ListRoleIDs.Count)
                        {
                            failedMsg = "Account: " + uid + ", ServerID: " + zoneId.ToString() + " not contains client with RoleID: " + roleId;
                            cne = ChangeNameError.NotContainRole;
                            break;
                        }
                    }

                    /// Tìm DBRoleInfo
                    roleInfo = dbMgr.GetDBRoleInfo(roleId);
                    if (null == roleInfo)
                    {
                        failedMsg = "Not found dbroleinfo, roleid: " + roleId.ToString();
                        cne = ChangeNameError.DBFailed;
                        break;
                    }

                    oldName = roleInfo.RoleName;

                    /// Nếu tên đã tồn tại
                    if (dbMgr.IsRolenameExist(newName) || !this.IsNameCanUseInDb(dbMgr, newName))
                    {
                        failedMsg = "New name: " + newName + " has already existed";
                        cne = ChangeNameError.NameAlreayUsed;
                        break;
                    }

                    /// Thực hiện đổi tên
                    string cmdText = string.Format("UPDATE t_roles SET rname='{0}' WHERE rid={1} AND userid='{2}' AND zoneid={3}", newName, roleId, uid, zoneId);
                    if (!this.ExecNonQuery(dbMgr, cmdText))
                    {
                        failedMsg = "Update new name to t_roles failed";
                        cne = ChangeNameError.DBFailed;
                        break;
                    }

                    lock (userInfo)
                    {
                        for (int i = 0; i < userInfo.ListRoleIDs.Count; ++i)
                        {
                            if (userInfo.ListRoleZoneIDs[i] == zoneId && userInfo.ListRoleIDs[i] == roleId)
                            {
                                userInfo.ListRoleNames[i] = newName;
                                break;
                            }
                        }
                    }

                    lock (roleInfo)
                    {
                        oldName = roleInfo.RoleName;
                        roleInfo.RoleName = newName;
                    }

                    //if (roleInfo.FamilyID > 0)
                    //{
                    //    FamilyManager.getInstance().ChangeFamilyMemberName(roleInfo.RoleID, roleInfo.FamilyID, newName);
                    //}

                    if (roleInfo.GuildID > 0)
                    {
                        GuildManager.getInstance().ChangeGuildMemberName(roleInfo.RoleID, roleInfo.GuildID, newName);
                    }
                    /// TODO update tên cho các chức năng khác ví dụ bang hội

                    cne = ChangeNameError.Success;
                } while (false);

                if (cne == ChangeNameError.Success)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Change name successfully，roleid={0}, old name={1}，new name={2}", roleId, oldName, newName));
                    this.OnChangeNameSuccess(dbMgr, roleId, zoneId, oldName, newName);
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Change name failed，roleid={0}, name={1}, reason={2}", roleId, oldName, failedMsg));
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}", (int)cne, oldName), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Đổi tên thành công
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="roleId"></param>
        /// <param name="zoneId"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        private void OnChangeNameSuccess(DBManager dbMgr, int roleId, int zoneId, string oldName, string newName)
        {
            /// Cache vào GameDB
            DBManager.getInstance().DBRoleMgr.OnChangeName(roleId, zoneId, oldName, newName);
        }

        /// <summary>
        /// Truy vấn cơ sở dữ liệu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool ExecNonQuery(DBManager dbMgr, string sql)
        {
            bool bRet = false;
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", sql), EventLevels.Important);
                MySQLCommand cmd = new MySQLCommand(sql, conn);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    bRet = false;
                    LogManager.WriteLog(LogTypes.Error, string.Format("Failed executing SQL: {0}", sql));
                }

                cmd.Dispose();
                cmd = null;
                bRet = true;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }

            return bRet;
        }

        /// <summary>
        /// Kiểm tra tên có thể sử dụng được không
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsNameCanUseInDb(DBManager dbMgr, string name)
        {
            if (dbMgr == null || string.IsNullOrEmpty(name)) return false;

            MySQLConnection conn = null;
            string prefixName = name + "99999999";
            try
            {
                int key = Thread.CurrentThread.ManagedThreadId;
                string sql = string.Format("REPLACE INTO t_name_check(`id`,`name`) VALUES({0},'{1}');", key, prefixName);
                conn = dbMgr.DBConns.PopDBConnection();

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", sql), EventLevels.Important);
                MySQLCommand cmd = new MySQLCommand(sql, conn);

                cmd.ExecuteNonQuery();
                cmd = new MySQLCommand(string.Format("SELECT name FROM t_name_check WHERE Id = {0};", key), conn);
                var reader = cmd.ExecuteReaderEx();
                if (reader.Read())
                {
                    string nameInDb = reader["name"].ToString();
                    if (!string.IsNullOrEmpty(nameInDb) && nameInDb == prefixName)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }
    }
}