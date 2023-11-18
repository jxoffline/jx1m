using FS.VLTK.Entities.Config;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Bách Bảo Rương
        /// <summary>
        /// Thông tin Bách Bảo Rương
        /// </summary>
        public static KTSeashellCircle SeashellCircleInfo { get; private set; }

        /// <summary>
        /// Đọc dữ liệu từ File Activity/KTSeashellCircle.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadSeashellCircle(XElement xmlNode)
        {
            Loader.SeashellCircleInfo = KTSeashellCircle.Parse(xmlNode);
        }
        #endregion

        #region Hoạt động
        /// <summary>
        /// Danh sách hoạt động
        /// </summary>
        public static Dictionary<int, ActivityXML> Activities { get; } = new Dictionary<int, ActivityXML>();

        /// <summary>
        /// Đọc dữ liệu từ File Activity/ActivityList.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadActivities(XElement xmlNode)
        {
            /// Làm rỗng danh sách
            Loader.Activities.Clear();

            foreach (XElement node in xmlNode.Elements("Activity"))
            {
                ActivityXML activity = ActivityXML.Parse(node);
                Loader.Activities[activity.ID] = activity;
            }
        }
        #endregion

        #region Chúc phúc
        /// <summary>
        /// Danh sách chúc phúc
        /// </summary>
        public static Dictionary<string, PlayerPrayXML> PlayerPrays { get; private set; } = new Dictionary<string, PlayerPrayXML>();

        /// <summary>
        /// Đọc dữ liệu chúc phúc
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadPlayerPray(XElement xmlNode)
		{
            Loader.PlayerPrays.Clear();
            foreach (XElement node in xmlNode.Elements("Pray"))
			{
                PlayerPrayXML playerPray = PlayerPrayXML.Parse(node);
                Loader.PlayerPrays[playerPray.Result] = playerPray;
			}
		}
		#endregion
	}
}
