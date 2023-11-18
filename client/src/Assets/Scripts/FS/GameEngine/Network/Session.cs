using System;
using Server.Data;

namespace FS.GameEngine.Network
{
    public class SDKSession
    {
        public static string AccessToken { get; set; }

        public static string TrasnID { get; set; }
    }
    /// <summary>
    /// Quản lý Session của game
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Quản lý Session của game
        /// </summary>
        public Session()
        {
        }

        #region Thông tin nhân vật

        /// <summary>
        /// ID Tài khoản
        /// </summary>
        public string UserID { get; set; } = "";

        /// <summary>
        /// Tên tài khoản
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Token
        /// </summary>
        public string UserToken { get; set; } = "";



     

        /// <summary>
        /// IP đăng nhập lần trước
        /// </summary>
        public string LastLoginIP { get; set; } = "";

        /// <summary>
        /// Thời gian đăng nhập lần trước
        /// </summary>
        public string LastLoginTime { get; set; } = "";

        /// <summary>
        /// Unknow
        /// </summary>
        public string Cm { get; set; }

        /// <summary>
        /// Unknow
        /// </summary>
        public long TimeActive { get; set; }

        /// <summary>
        /// Unknow
        /// </summary>
        public string TokenGS { get; set; }

        /// <summary>
        /// Lớn chưa
        /// </summary>
        public int UserIsAdult { get; set; } = 0;

        /// <summary>
        /// Random token
        /// </summary>
        public int RoleRandToken { get; set; } = -1;

        /// <summary>
        /// ID nhân vật
        /// </summary>
        public int RoleID { get; set; } = -1;

        /// <summary>
        /// Giới tính
        /// </summary>
        public int RoleSex { get; set; } = 0;

        /// <summary>
        /// Tên nhân vật
        /// </summary>
        public string RoleName { get; set; } = "";

        /// <summary>
        /// Đã chơi game chưa
        /// </summary>
        public bool PlayGame { get; set; } = false;

        /// <summary>
        /// Dứ liệu nhân vật
        /// </summary>

        public RoleData roleData { get; set; } = null;
        #endregion Thông tin nhân vật
    }
}

