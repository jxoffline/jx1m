using GameDBServer.Logic;
using GameDBServer.Logic.Name;
using GameDBServer.Logic.SystemParameters;
using MySQLDriverCS;

namespace GameDBServer.DB
{
    /// <summary>
    /// Quản lý Database
    /// </summary>
    public class DBManager
    {
        #region Singleton Instance

        private static DBManager instance = new DBManager();

        private DBManager()
        {
        }

        /// <summary>
        /// Quản lý Database
        /// </summary>
        /// <returns></returns>
        public static DBManager getInstance()
        {
            return instance;
        }

        #endregion Singleton Instance

        private DBConnections _DBConns = new DBConnections();

        /// <summary>
        /// Kết nối tới Database
        /// </summary>
        public DBConnections DBConns
        {
            get { return _DBConns; }
        }

        private DBUserMgr _DBUserMgr = new DBUserMgr();

        /// <summary>
        /// Cache thông tin tài khoản
        /// </summary>
        public DBUserMgr dbUserMgr
        {
            get { return _DBUserMgr; }
        }

        private DBRoleMgr _DBRoleMgr = new DBRoleMgr();

        /// <summary>
        /// Cache thông tin nhân vật
        /// </summary>
        public DBRoleMgr DBRoleMgr
        {
            get { return _DBRoleMgr; }
        }

        /// <summary>
        /// Trả về số kết nối tối đa đến DB
        /// </summary>
        public int GetMaxConnsCount()
        {
            return _DBConns.GetDBConnsCount();
        }

        /// <summary>
        /// Tải xuống dữ liệu từ Database
        /// </summary>
        /// <param name="connstr"></param>
        /// <param name="MaxConns"></param>
        /// <param name="codePage"></param>
        public void LoadDatabase(MySQLConnectionString connstr, int MaxConns, int codePage)
        {
            TianMaCharSet.ConvertToCodePage = codePage;

            _DBConns.BuidConnections(connstr, MaxConns);
            MySQLConnection conn = _DBConns.PopDBConnection();

            _DBConns.ChangeMysqlConfig(conn);
            try
            {
                /// Tải thông tin các tham biến hệ thống
                SystemGlobalParametersManager.QuerySystemGlobalParameters(this);

                /// Thiết lập hệ thống từ DB
                GameDBManager.GameConfigMgr.LoadGameConfigFromDB(this);

                /// Danh sách tên nhân vật trong DB
                NameUsedMgr.Instance().LoadFromDatabase(this);

                /// Khởi tạo danh sách Ban
                KTBanManager.Setup(this);
            }
            finally
            {
                _DBConns.PushDBConnection(conn);
            }
        }

        /// <summary>
        /// Tên nhân vật đã tồn tại chưa
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool IsRolenameExist(string strRoleName)
        {
            MySQLConnection conn = _DBConns.PopDBConnection();
            try
            {
                var resultList = DBRoleInfo.QueryRoleIdList_ByRolename_IgnoreDbCmp(conn, strRoleName);
                if (resultList != null && resultList.Count > 0)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                _DBConns.PushDBConnection(conn);
            }
        }

        /// <summary>
        /// Trả về thông tin nhân vật theo tài khoản
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public DBUserInfo GetDBUserInfo(string userID)
        {
            DBUserInfo dbUserInfo = _DBUserMgr.FindDBUserInfo(userID);
            if (null == dbUserInfo)
            {
                dbUserInfo = new DBUserInfo();
                MySQLConnection conn = _DBConns.PopDBConnection();

                try
                {
                    if (!dbUserInfo.Query(conn, userID))
                    {
                        return null;
                    }
                }
                finally
                {
                    _DBConns.PushDBConnection(conn);
                }

                _DBUserMgr.AddDBUserInfo(dbUserInfo);
            }

            return dbUserInfo;
        }

        /// <summary>
        /// Trả về thông tin nhân vật theo tên
        /// </summary>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public DBRoleInfo GetDBRoleInfo(string rolename)
        {
            if (string.IsNullOrEmpty(rolename))
            {
                return null;
            }

            int roleid = -1;

            MySQLConnection conn = _DBConns.PopDBConnection();
            try
            {
                roleid = DBRoleInfo.QueryRoleID_ByRolename(conn, rolename);
            }
            finally
            {
                _DBConns.PushDBConnection(conn);
            }

            return GetDBRoleInfo(roleid);
        }

        /// <summary>
        /// Trả về thông tin nhân vật theo ID
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public DBRoleInfo GetDBRoleInfo(int roleID)
        {
           
            DBRoleInfo dbRoleInfo = _DBRoleMgr.FindDBRoleInfo(roleID);
          
            if (null == dbRoleInfo)
            {
               
                dbRoleInfo = new DBRoleInfo();
                MySQLConnection conn = _DBConns.PopDBConnection();

                try
                {
                   
                    if (!dbRoleInfo.Query(conn, roleID))
                    {
                      
                        return null;
                    }
                }
                finally
                {
                    _DBConns.PushDBConnection(conn);
                }

               
                _DBRoleMgr.AddDBRoleInfo(dbRoleInfo);
            }

            return dbRoleInfo;
        }

        /// <summary>
        /// Trả về thông tin toàn bộ nhân vật trong tài khoản theo ID nhân vật chỉ định
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public DBRoleInfo GetDBAllRoleInfo(int roleID)
        {
            DBRoleInfo dbRoleInfo = _DBRoleMgr.FindDBRoleInfo(roleID);
            if (null == dbRoleInfo)
            {
                dbRoleInfo = new DBRoleInfo();
                MySQLConnection conn = _DBConns.PopDBConnection();

                try
                {
                    if (!dbRoleInfo.Query(conn, roleID, false))
                    {
                        return null;
                    }
                }
                finally
                {
                    _DBConns.PushDBConnection(conn);
                }

                _DBRoleMgr.AddDBRoleInfo(dbRoleInfo);
            }

            return dbRoleInfo;
        }
    }
}