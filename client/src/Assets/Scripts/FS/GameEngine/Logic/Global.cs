using System;
using System.Collections.Generic;
using System.Xml.Linq;
using FS.Drawing;
using Server.Data;
using UnityEngine;
using FS.GameEngine.Data;
using FS.GameEngine.Network;
using Server.Tools.AStarEx;
using FS.GameEngine.Teleport;
using FS.VLTK.Utilities.UnityComponent;
using HSGameEngine.GameEngine.Network.Tools;

namespace FS.GameEngine.Logic
{
	/// <summary>
	/// Các biến toàn cục hệ thống
	/// </summary>
	public static class Global
    {
        #region VLTK
        /// <summary>
        /// Đối tượng bản đồ hiện tại
        /// </summary>
        public static FS.VLTK.Control.Component.Map CurrentMap { get; set; } = null;

        /// <summary>
        /// Canvas chứa UI trong Game
        /// </summary>
        public static Canvas MainCanvas { get; set; } = null;

        /// <summary>
        /// Prefab Camera soi đối tượng
        /// </summary>
        public static Camera ObjectPreviewCameraPrefab { get; set; } = null;

        /// <summary>
        /// Đối tượng thu âm và phát lại
        /// </summary>
        public static AudioRecorderAndPlayer Recorder { get; set; } = null;

        /// <summary>
        /// Camera dùng trong Radar Map
        /// </summary>
        public static Camera RadarMapCamera { get; set; } = null;

        /// <summary>
        /// Camera chính
        /// </summary>
        public static Camera MainCamera { get; set; } = null;
        #endregion

        #region Tham biến cấu hình
        /// <summary>
        /// Danh sách các tham biến cấu hình hệ thống
        /// </summary>
        public static Dictionary<string, string> RootParams { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Trả về giá trị biến cấu hình hệ thống
        /// </summary>  
        public static string GetRootParam(string name, string defaultValue = "")
        {
            /// Nếu tìm thấy
            if (Global.RootParams.TryGetValue(name, out string value))
            {
                return value;
            }
            /// Không tìm thấy thì trả ra giá trị mặc định
            return defaultValue;
        }

        /// <summary>
        /// Trả về Port đăng nhập
        /// </summary>
        /// <returns></returns>
        public static int GetUserLoginPort()
        {
            string value = Global.GetRootParam("loginport", "4502");
            if (int.TryParse(value, out int port))
            {
                return port;
            }
            return -1;
        }

        /// <summary>
        /// Trả về Port của Server
        /// </summary>
        /// <returns></returns>
        public static int GetGameServerPort()
        {
            string value = Global.GetRootParam("gameport", "4503");
            if (int.TryParse(value, out int port))
            {
                return port;
            }
            return -1;
        }
        #endregion

        #region Quản lý kết nối

        /// <summary>
        /// Danh sách kết nối
        /// </summary>
        public static List<LineData> LineDataList { get; set; } = null;

        /// <summary>
        /// Kết nối hiện tại
        /// </summary>
        public static LineData CurrentListData { get; set; } = null;

        #endregion

        #region Thiết lập
        /// <summary>
        /// Dữ liệu tạm thời
        /// </summary>
        public static GData Data { get; set; } = null;

        /// <summary>
        /// Backup RoleData
        /// </summary>
        private static RoleData RoleDataBackUp = null;

        /// <summary>
        /// Copy RoleData
        /// </summary>
        public static void CopyRoleData(RoleData roleDataMini)
        {
            KTDebug.LogError("CopyRoleData");
            RoleDataBackUp = roleDataMini;
        }

        /// <summary>
        /// Thiết lập RoleData cho Game
        /// </summary>
        public static void SetGameRoleData()
        {
            GameInstance.Game.CurrentSession.roleData = RoleDataBackUp;
            Global.RoleDataBackUp = null;
        }

        /// <summary>
        /// Dữ liệu bản đồ hiện tại
        /// </summary>
        public static GMapData CurrentMapData { get; set; } = null;
        #endregion

        #region Đường dẫn
        /// <summary>
        /// Đánh dấu xem có phải đang Reconnect không
        /// </summary>
		public static bool g_bReconnRoleManager { get; set; } = false;
        
        /// <summary>
        /// Trả về đường dẫn dạng WebURL tương ứng
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="isFolder"></param>
        /// <returns></returns>
        public static string WebPath(string uri, bool isFolder = false)
        {
            return PathUtils.WebPath(uri, isFolder);
        }

        /// <summary>
        /// Trả về tên thiết bị trên WebURL
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceForWebURL()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "ios";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return "androidsub";
            }
            else
            {
#if UNITY_ANDROID
                return "androidsub";
#elif UNITY_IPHONE
                return "ios";
#else
                return "";
#endif
            }
        }
        #endregion

        #region Tải xuống điểm truyền tống

        /// <summary>
        /// Tải xuống điểm truyền tống
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GTeleport GetTeleport(XElement xmlNode)
        {
            int code = int.Parse(xmlNode.Attribute("Code").Value);
            if (-1 == code)
            {
                return null;
            }

            GTeleport teleport = new GTeleport("teleport");

            teleport.Name = string.Format("Teleport{0}", int.Parse(xmlNode.Attribute("Key").Value));
            teleport.Key = byte.Parse(xmlNode.Attribute("Key").Value);
            teleport.To = int.Parse(xmlNode.Attribute("To").Value);
            teleport.ToX = int.Parse(xmlNode.Attribute("ToX").Value);
            teleport.ToY = int.Parse(xmlNode.Attribute("ToY").Value);
            teleport.PosX = int.Parse(xmlNode.Attribute("X").Value);
            teleport.PosY = int.Parse(xmlNode.Attribute("Y").Value);
            teleport.Radius = int.Parse(xmlNode.Attribute("Radius").Value);
            teleport.Tip = xmlNode.Attribute("Tip").Value;

            return teleport;
        }

        #endregion

        #region Quản lý vật phẩm
        /// <summary>
        /// Hằng số mặc định thời gian sử dụng vật phẩm (loại vĩnh viễn)
        /// </summary>
        public const string ConstGoodsEndTime = "1900-01-01 12:00:00";
        #endregion

        #region Quản lý thời gian

        /// <summary>
        /// Lấy thời gian tính từ năm 1970 đến hiện tại
        /// </summary>
        /// <returns></returns>
        public static int GetMyTimer()
        {
            long ticks = ((DateTime.Now.Ticks - MyDateTime.Before1970Ticks) / 10000);
            return (int) (ticks);
        }

        #endregion

        #region Thư viện dùng cho String
        /// <summary>
        /// Thay thế toàn bộ chuỗi tương ứng
        /// </summary>
        /// <param name="source">源数据</param>
        /// <param name="find">替换对象</param>
        /// <param name="replacement">替换内容</param>
        /// <returns></returns>
        public static string StringReplaceAll(string source, string find, string replacement)
        {
            string str = "";
            if (source != null && replacement != null && find != null)
            {
                str = source.Replace(find, replacement);
            }

            return str;
        }
        #endregion

        #region Thư viện dùng cho chuỗi Byte
        /// <summary>
        /// Trả về giá trị BIT tại vị trí tương ứng
        /// </summary>
        /// <param name="whichOne"></param>
        /// <returns></returns>
        public static int GetBitValue(int whichOne)
        {
            int bitVal = (int) Math.Pow(2, whichOne - 1);
            return bitVal;
        }

        /// <summary>
        /// Trả về giá trị BIT tại vị trí tương ứng
        /// </summary>
        /// <param name="values"></param>
        /// <param name="whichOne"></param>
        /// <returns></returns>
        public static int GetBitValue(List<int> values, int whichOne)
        {
            int index = whichOne / 32;
            int bitIndex = whichOne % 32;
            if (values.Count <= index)
            {
                return 0;
            }
            int value = values[index];
            if ((value & (1 << bitIndex)) != 0)
            {
                return 1;
            }
            return 0;
        }
        #endregion
    }
}
