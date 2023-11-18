using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameDBServer.Logic.KTBan.KTBanModel;

namespace GameDBServer.Logic
{
    /// <summary>
    /// Quản lý danh sách BAN người chơi
    /// </summary>
    public static class KTBanManager
    {
        #region Define
        /// <summary>
        /// Danh sách người chơi bị cấm đăng nhập
        /// </summary>
        private static readonly ConcurrentDictionary<int, BanUser> bannedUserList = new ConcurrentDictionary<int, BanUser>();

        /// <summary>
        /// Danh sách người chơi bị cấm chat
        /// </summary>
        private static readonly ConcurrentDictionary<int, BanUser> bannedChatUserList = new ConcurrentDictionary<int, BanUser>();

        /// <summary>
        /// Danh sách người chơi bị cấm chức năng nào đó
        /// </summary>
        private static readonly ConcurrentDictionary<string, BanUserByType> bannedUserByTypeList = new ConcurrentDictionary<string, BanUserByType>();

        /// <summary>
        /// Đối tượng quản lý DB
        /// </summary>
        private static DBManager dbManager;
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="dbMgr"></param>
        public static void Setup(DBManager dbMgr)
        {
            /// Lưu lại đối tượng quản lý DB
            KTBanManager.dbManager = dbMgr;
            /// Tải xuống danh sách cấm
            KTBanManager.LoadBannedUserFromDB();
            KTBanManager.LoadBannedChatUserFromDB();
            KTBanManager.LoadBannedByTypeUserFromDB();
        }
        #endregion

        #region Private methods
        #region Ban User
        /// <summary>
        /// Tải xuống danh sách người chơi bị cấm đăng nhập từ DB
        /// </summary>
        private static void LoadBannedUserFromDB()
        {
            /// Làm rỗng bảng
            KTBanManager.bannedUserList.Clear();

            /// Connection
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT * FROM t_ban_user");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                try
                {
                    MySQLDataReader reader = cmd.ExecuteReaderEx();
                    while (reader.Read())
                    {
                        int roleID = int.Parse(reader["role_id"].ToString());
                        long startTime = long.Parse(reader["start_time"].ToString());
                        long duration = long.Parse(reader["duration"].ToString());
                        string reason = DataHelper.Base64Decode(reader["reason"].ToString());
                        string bannedBy = DataHelper.Base64Decode(reader["banned_by"].ToString());

                        /// Tạo mới
                        BanUser user = new BanUser()
                        {
                            RoleID = roleID,
                            StartTime = startTime,
                            Duration = duration,
                            Reason = reason,
                            BannedBy = bannedBy,
                        };
                        /// Thêm vào danh sách
                        KTBanManager.bannedUserList[roleID] = user;
                    }
                }
                catch (Exception)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Exception while querying banned user list: {0}", cmdText));
                }

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Thêm người chơi vào danh sách cấm đăng nhập
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool AddBannedUser(BanUser user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("INSERT INTO t_ban_user(role_id, start_time, duration, reason, banned_by) VALUES({0}, {1}, {2}, '{3}', '{4}')", user.RoleID, user.StartTime, user.Duration, DataHelper.Base64Encode(user.Reason), DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xóa người chơi khỏi danh sách cấm đăng nhập
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private static bool RemoveBannedUser(int roleID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_ban_user WHERE role_id = {0}", roleID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Cập nhật thông tin người chơi từ danh sách cấm đăng nhập
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool UpdateBannedUser(BanUser user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_ban_user SET start_time = {1}, duration = {2}, reason = '{3}', banned_by = '{4}' WHERE role_id = {0}", user.RoleID, user.StartTime, user.Duration, DataHelper.Base64Encode(user.Reason), DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }
        #endregion

        #region Ban chat
        /// <summary>
        /// Tải xuống danh sách người chơi bị cấm chat từ DB
        /// </summary>
        private static void LoadBannedChatUserFromDB()
        {
            /// Làm rỗng bảng
            KTBanManager.bannedChatUserList.Clear();

            /// Connection
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT * FROM t_ban_chat");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                try
                {
                    MySQLDataReader reader = cmd.ExecuteReaderEx();
                    while (reader.Read())
                    {
                        int roleID = int.Parse(reader["role_id"].ToString());
                        long startTime = long.Parse(reader["start_time"].ToString());
                        long duration = long.Parse(reader["duration"].ToString());
                        string reason = DataHelper.Base64Decode(reader["reason"].ToString());
                        string bannedBy = DataHelper.Base64Decode(reader["banned_by"].ToString());

                        /// Tạo mới
                        BanUser user = new BanUser()
                        {
                            RoleID = roleID,
                            StartTime = startTime,
                            Duration = duration,
                            Reason = reason,
                            BannedBy = bannedBy,
                        };
                        /// Thêm vào danh sách
                        KTBanManager.bannedChatUserList[roleID] = user;
                    }
                }
                catch (Exception)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Exception while querying banned chat user list: {0}", cmdText));
                }

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Thêm người chơi vào danh sách cấm chat
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool AddBannedChatUser(BanUser user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("INSERT INTO t_ban_chat(role_id, start_time, duration, reason, banned_by) VALUES({0}, {1}, {2}, '{3}', '{4}')", user.RoleID, user.StartTime, user.Duration, DataHelper.Base64Encode(user.Reason), DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xóa người chơi khỏi danh sách cấm chat
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private static bool RemoveBannedChatUser(int roleID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_ban_chat WHERE role_id = {0}", roleID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Cập nhật thông tin người chơi từ danh sách cấm chat
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool UpdateBannedChatUser(BanUser user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_ban_chat SET start_time = {1}, duration = {2}, reason = '{3}', banned_by = '{4}' WHERE role_id = {0}", user.RoleID, user.StartTime, user.Duration, DataHelper.Base64Encode(user.Reason), DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }
        #endregion

        #region Ban chức năng
        /// <summary>
        /// Tải xuống danh sách người chơi bị cấm chức năng nào đó từ DB
        /// </summary>
        private static void LoadBannedByTypeUserFromDB()
        {
            /// Làm rỗng bảng
            KTBanManager.bannedUserByTypeList.Clear();

            /// Connection
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT * FROM t_ban");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                try
                {
                    MySQLDataReader reader = cmd.ExecuteReaderEx();
                    while (reader.Read())
                    {
                        int roleID = int.Parse(reader["role_id"].ToString());
                        int banType = int.Parse(reader["ban_type"].ToString());
                        long startTime = long.Parse(reader["start_time"].ToString());
                        long duration = long.Parse(reader["duration"].ToString());
                        string bannedBy = DataHelper.Base64Decode(reader["banned_by"].ToString());

                        /// Tạo mới
                        BanUserByType user = new BanUserByType()
                        {
                            RoleID = roleID,
                            StartTime = startTime,
                            Duration = duration,
                            BannedBy = bannedBy,
                        };
                        /// Thêm vào danh sách
                        KTBanManager.bannedUserByTypeList[string.Format("{0}_{1}", banType, roleID)] = user;
                    }
                }
                catch (Exception)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Exception while querying banned user list: {0}", cmdText));
                }

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Thêm người chơi vào danh sách cấm chức năng gì đó
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool AddBannedUserByType(BanUserByType user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("INSERT INTO t_ban(role_id, ban_type, start_time, duration, banned_by) VALUES({0}, {1}, {2}, {3}, '{4}')", user.RoleID, user.BanType, user.StartTime, user.Duration, DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xóa người chơi khỏi danh sách cấm chức năng gì đó
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private static bool RemoveBannedUserByType(int roleID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_ban WHERE role_id = {0}", roleID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Cập nhật thông tin người chơi từ danh sách cấm chức năng nào đó
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool UpdateBannedUserByType(BanUserByType user)
        {
            MySQLConnection conn = null;

            try
            {
                conn = KTBanManager.dbManager.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_ban SET start_time = {1}, duration = {2}, ban_type = {3}, banned_by = '{4}' WHERE role_id = {0}", user.RoleID, user.StartTime, user.Duration, user.BanType, DataHelper.Base64Encode(user.BannedBy));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    KTBanManager.dbManager.DBConns.PushDBConnection(conn);
                }
            }

            /// OK
            return true;
        }
        #endregion
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra người chơi có bị cấm đăng nhập không
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public static bool IsBannedLogin(int roleID)
        {
            /// Nếu không tồn tại
            if (!KTBanManager.bannedUserList.TryGetValue(roleID, out BanUser banData))
            {
                /// Không cấm
                return false;
            }
            /// Trả về kết quả
            return !banData.IsOver;
        }

        /// <summary>
        /// Trả về thông tin cấm chat của người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public static BanUser GetBanChatUserData(int roleID)
        {
            /// Nếu không tồn tại
            if (!KTBanManager.bannedChatUserList.TryGetValue(roleID, out BanUser banData))
            {
                /// Không có dữ liệu
                return null;
            }
            /// Nếu đã quá thời gian
            else if (banData.IsOver)
            {
                /// Không có dữ liệu
                return null;
            }
            /// Trả về kết quả
            return banData;
        }

        /// <summary>
        /// Trả về danh sách các chức năng bị cấm của người chơi tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public static Dictionary<int, BanUserByType> GetBanList(int roleID)
        {
            /// Tạo mới Dict
            Dictionary<int, BanUserByType> result = KTBanManager.bannedUserByTypeList.Where(x => x.Key.Contains("_" + roleID)).ToDictionary(tKey => int.Parse(tKey.Key.Substring(0, tKey.Key.IndexOf("_"))), tValue => tValue.Value);
            /// Trả về kết quả
            return result;
        }
        #endregion

        #region TCP-Core
        /// <summary>
        /// Xử lý gói tin thao tác cấm người chơi tương ứng đăng nhập hoặc chat
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessBanUser(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            KeyValuePair<BanUser, int> banData;

            try
            {
                banData = DataHelper.BytesToObject<KeyValuePair<BanUser, int>>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds) nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                /// Loại cấm
                int banType = banData.Value;
                /// Dữ liệu người chơi
                BanUser user = banData.Key;

                /// Kết quả thao tác
                bool ret = false;

                /// Nếu là thêm cấm đăng nhập
                if (banType == 0)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedUserList.TryGetValue(user.RoleID, out BanUser banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = Global.GetCurrentTimeMilis();
                        banUser.Duration = user.Duration;
                        banUser.Reason = user.Reason;
                        banUser.BannedBy = user.BannedBy;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedUser(banUser);
                    }
                    /// Nếu chưa tồn tại
                    else
                    {
                        /// Cập nhật thông tin
                        user.StartTime = Global.GetCurrentTimeMilis();

                        /// Lưu vào DB
                        ret = KTBanManager.AddBannedUser(user);
                        /// Lưu vào cache
                        if (ret)
                        {
                            KTBanManager.bannedUserList[user.RoleID] = user;
                        }
                    }
                }
                /// Nếu là thêm cấm chat
                else if (banType == 1)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedChatUserList.TryGetValue(user.RoleID, out BanUser banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = Global.GetCurrentTimeMilis();
                        banUser.Duration = user.Duration;
                        banUser.Reason = user.Reason;
                        banUser.BannedBy = user.BannedBy;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedChatUser(banUser);
                    }
                    /// Nếu chưa tồn tại
                    else
                    {
                        /// Cập nhật thông tin
                        user.StartTime = Global.GetCurrentTimeMilis();

                        /// Lưu vào DB
                        ret = KTBanManager.AddBannedChatUser(user);
                        /// Lưu vào cache
                        if (ret)
                        {
                            KTBanManager.bannedChatUserList[user.RoleID] = user;
                        }
                    }
                }
                /// Nếu là xóa cấm đăng nhập
                else if (banType == 2)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedUserList.TryGetValue(user.RoleID, out BanUser banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = 0;
                        banUser.Duration = 0;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedUser(banUser);
                    }
                }
                /// Nếu là xóa cấm chat
                else if (banType == 3)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedChatUserList.TryGetValue(user.RoleID, out BanUser banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = 0;
                        banUser.Duration = 0;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedChatUser(banUser);
                    }
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", ret ? 1 : 0), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin thao tác cấm người chơi tương ứng chức năng nào đó
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessBanUserByType(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            KeyValuePair<BanUserByType, int> banData;

            try
            {
                banData = DataHelper.BytesToObject<KeyValuePair<BanUserByType, int>>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds) nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                /// Loại
                int type = banData.Value;
                /// Dữ liệu người chơi
                BanUserByType user = banData.Key;

                /// Kết quả thao tác
                bool ret = false;

                /// Nếu là thêm cấm
                if (type == 0)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedUserByTypeList.TryGetValue(string.Format("{0}_{1}", user.BanType, user.RoleID), out BanUserByType banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = Global.GetCurrentTimeMilis();
                        banUser.Duration = user.Duration;
                        banUser.BannedBy = user.BannedBy;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedUserByType(banUser);
                    }
                    /// Nếu chưa tồn tại
                    else
                    {
                        /// Cập nhật thông tin
                        user.StartTime = Global.GetCurrentTimeMilis();

                        /// Lưu vào DB
                        ret = KTBanManager.AddBannedUserByType(user);
                        /// Lưu vào cache
                        if (ret)
                        {
                            KTBanManager.bannedUserByTypeList[string.Format("{0}_{1}", user.BanType, user.RoleID)] = user;
                        }
                    }
                }
                /// Nếu là xóa cấm
                else if (type == 1)
                {
                    /// Nếu đã tồn tại
                    if (KTBanManager.bannedUserByTypeList.TryGetValue(string.Format("{0}_{1}", user.BanType, user.RoleID), out BanUserByType banUser))
                    {
                        /// Cập nhật thông tin
                        banUser.StartTime = 0;
                        banUser.Duration = 0;

                        /// Lưu vào DB
                        ret = KTBanManager.UpdateBannedUserByType(banUser);
                    }
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", ret ? 1 : 0), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int) TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }
        #endregion
    }
}
