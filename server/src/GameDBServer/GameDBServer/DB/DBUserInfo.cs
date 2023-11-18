using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameDBServer.DB
{
    /// <summary>
    /// Thông tin tài khoản
    /// </summary>
    public class DBUserInfo
    {
        #region Cơ bản

        /// <summary>
        /// ID tài khoản
        /// </summary>
        public string UserID
        {
            get;
            set;
        }

        private List<int> _ListRoleIDs = new List<int>();

        /// <summary>
        /// Danh sách ID nhân vật
        /// </summary>
        public List<int> ListRoleIDs
        {
            get { return _ListRoleIDs; }
        }

        private List<int> _ListRoleSexes = new List<int>();

        /// <summary>
        /// Danh sách giới tính
        /// </summary>
        public List<int> ListRoleSexes
        {
            get { return _ListRoleSexes; }
        }

        private List<int> _ListRoleOccups = new List<int>();

        /// <summary>
        /// Danh sách môn phái
        /// </summary>
        public List<int> ListRoleOccups
        {
            get { return _ListRoleOccups; }
        }

        private List<string> _ListRoleNames = new List<string>();

        /// <summary>
        /// Danh sách tên
        /// </summary>
        public List<string> ListRoleNames
        {
            get { return _ListRoleNames; }
        }

        private List<int> _ListRoleLevels = new List<int>();

        /// <summary>
        /// Danh sách cấp độ
        /// </summary>
        public List<int> ListRoleLevels
        {
            get { return _ListRoleLevels; }
        }

        private List<int> _ListRoleZoneIDs = new List<int>();

        /// <summary>
        /// Danh sách vị trí đang đứng
        /// </summary>
        public List<string> ListRolePositions
        {
            get { return _ListRoleMapCodes; }
        }

        private List<string> _ListRoleMapCodes = new List<string>();

        /// <summary>
        /// Danh sách máy chủ
        /// </summary>
        public List<int> ListRoleZoneIDs
        {
            get { return _ListRoleZoneIDs; }
        }

        /// <summary>
        /// Đồng
        /// </summary>
        public int Money
        {
            get;
            set;
        }

        /// <summary>
        /// Tổng số đồng đã nạp
        /// </summary>
        public int RealMoney
        {
            get;
            set;
        }

        /// <summary>
        /// Nội dung gì đó khi Logout
        /// </summary>
        public string PushMessageID
        {
            get;
            set;
        }

        #endregion Cơ bản

        #region Mở rộng

        private long _LastReferenceTicks = DateTime.Now.Ticks / 10000;

        /// <summary>
        /// Thời gian sử dụng lần cuối
        /// </summary>
        public long LastReferenceTicks
        {
            get { return _LastReferenceTicks; }
            set { _LastReferenceTicks = value; }
        }

        /// <summary>
        /// Thời gian đăng xuất khỏi GS
        /// </summary>
        public long LogoutServerTicks = 0;

        #endregion Mở rộng

        #region Truy vấn DB

        /// <summary>
        /// Truy vấn dữ liệu thông tin tài khoản
        /// </summary>
        public bool Query(MySQLConnection conn, string userID)
        {
            LogManager.WriteLog(LogTypes.Info, string.Format("Query role info from DB: {0}", userID));

            this.UserID = userID;

            /// Truy vấn thông tin từ bảng t_role
            MySQLSelectCommand cmd = new MySQLSelectCommand(conn, new string[] { "rid", "userid", "rname", "sex", "occupation", "level", "position", "zoneid" },
               new string[] { "t_roles" }, new object[,] { { "userid", "=", userID }, { "isdel", "=", 0 } }, null, new string[,] { { "level", "desc" } }, true, 0, 4, false);
            if (cmd.Table.Rows.Count > 0)
            {
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    ListRoleIDs.Add(Convert.ToInt32(cmd.Table.Rows[i]["rid"].ToString()));
                    ListRoleNames.Add(cmd.Table.Rows[i]["rname"].ToString());
                    ListRoleSexes.Add(Convert.ToInt32(cmd.Table.Rows[i]["sex"].ToString()));
                    ListRoleOccups.Add(Convert.ToInt32(cmd.Table.Rows[i]["occupation"].ToString()));
                    ListRoleLevels.Add(Convert.ToInt32(cmd.Table.Rows[i]["level"].ToString()));
                    ListRolePositions.Add(cmd.Table.Rows[i]["position"].ToString());
                    ListRoleZoneIDs.Add(Convert.ToInt32(cmd.Table.Rows[i]["zoneid"].ToString()));
                }
            }

            this.Money = 0;
            /// Truy vấn thông tin từ bảng t_money
            cmd = new MySQLSelectCommand(conn, new string[] { "money", "realmoney" }, new string[] { "t_money" }, new object[,] { { "userid", "=", userID } }, null, null);
            if (cmd.Table.Rows.Count > 0)
            {
                this.Money = Convert.ToInt32(cmd.Table.Rows[0]["money"].ToString());
                this.RealMoney = Convert.ToInt32(cmd.Table.Rows[0]["realmoney"].ToString());
            }



            return true;
        }

        #endregion Truy vấn DB

        #region Truy vấn thông tin

        /// <summary>
        /// Trả về thông tin tài khoản thu gọn
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="OnlyZoneId"></param>
        /// <returns></returns>
        public UserMiniData GetUserMiniData(string userId, int roleId, int OnlyZoneId)
        {
            UserMiniData userMimiData = new UserMiniData();
            userMimiData.UserId = UserID;
            userMimiData.RealMoney = RealMoney;

            using (MySqlUnity conn = new MySqlUnity())
            {
                MySQLSelectCommand cmd = new MySQLSelectCommand(conn.DbConn, new string[] { "rid", "level", "regtime", "lasttime", "logofftime" }, new string[] { "t_roles" }, new object[,] { { "userid", "=", userId }, { "isdel", "=", 0 }, { "zoneid", "=", OnlyZoneId } }, null, null);
                if (cmd.Table.Rows.Count > 0)
                {
                    for (int i = 0; i < cmd.Table.Rows.Count; i++)
                    {
                        int rid = Convert.ToInt32(cmd.Table.Rows[i]["rid"].ToString());
                        int level = Convert.ToInt32(cmd.Table.Rows[i]["level"].ToString());

                        DateTime.TryParse(cmd.Table.Rows[i]["regtime"].ToString(), out DateTime createTime);
                        DateTime.TryParse(cmd.Table.Rows[i]["lasttime"].ToString(), out DateTime lastTime);
                        DateTime.TryParse(cmd.Table.Rows[i]["logofftime"].ToString(), out DateTime logoffTime);

                        if (rid == roleId)
                        {
                            userMimiData.RoleCreateTime = createTime;
                            userMimiData.RoleLastLoginTime = lastTime;
                            userMimiData.RoleLastLogoutTime = logoffTime;
                        }
                        if (userMimiData.MinCreateRoleTime > createTime)
                        {
                            userMimiData.MinCreateRoleTime = createTime;
                        }
                        if (userMimiData.LastLoginTime < lastTime)
                        {
                            userMimiData.LastLoginTime = lastTime;
                            userMimiData.LastRoleId = rid;
                        }
                        if (userMimiData.LastLogoutTime < logoffTime)
                        {
                            userMimiData.LastLogoutTime = logoffTime;
                        }
                        userMimiData.MaxLevel = level;
                    }
                }
            }

            return userMimiData;
        }

        #endregion Truy vấn thông tin
    }
}