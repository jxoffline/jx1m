using FS.VLTK.Entities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Danh hiệu đặc biệt
        #region Phi phong
        /// <summary>
        /// Đường dẫn File AssetBundle chứa danh hiệu phi phong
        /// </summary>
        public static string MantleTitlesBundleDir { get; private set; }

        /// <summary>
        /// Tên Atlas chứa danh hiệu phi phong
        /// </summary>
        public static string MantleTitlesAtlasName { get; private set; }

        /// <summary>
        /// Danh sách danh hiệu Phi Phong
        /// </summary>
        public static List<MantleTitleXML> MantleTitles { get; private set; } = new List<MantleTitleXML>();

        /// <summary>
        /// Đọc dữ liệu từ file AnimatedTitle/MantleTitle.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadMantleTitles(XElement xmlNode)
        {
            Loader.MantleTitles.Clear();

            Loader.MantleTitlesBundleDir = xmlNode.Element("Config").Attribute("BundleDir").Value;
            Loader.MantleTitlesAtlasName = xmlNode.Element("Config").Attribute("AtlasName").Value;

            foreach (XElement node in xmlNode.Elements("Title"))
            {
                MantleTitleXML mantleTitle = MantleTitleXML.Parse(node);
                Loader.MantleTitles.Add(mantleTitle);
            }
        }
        #endregion

        #region Quan hàm
        /// <summary>
        /// Đường dẫn File AssetBundle chứa danh hiệu quan hàm
        /// </summary>
        public static string OfficeTitlesBundleDir { get; private set; }

        /// <summary>
        /// Tên Atlas chứa danh hiệu quan hàm
        /// </summary>
        public static string OfficeTitlesAtlasName { get; private set; }

        /// <summary>
        /// Danh sách danh hiệu quan hàm
        /// </summary>
        public static List<OfficeTitleXML> OfficeTitles { get; private set; } = new List<OfficeTitleXML>();

        /// <summary>
        /// Đọc dữ liệu từ file AnimatedTitle/OfficeTitle.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadOfficeTitles(XElement xmlNode)
		{
            Loader.OfficeTitles.Clear();

            Loader.OfficeTitlesBundleDir = xmlNode.Element("Config").Attribute("BundleDir").Value;
            Loader.OfficeTitlesAtlasName = xmlNode.Element("Config").Attribute("AtlasName").Value;

            foreach (XElement node in xmlNode.Elements("Rank"))
			{
                OfficeTitleXML officeTitle = OfficeTitleXML.Parse(node);
                Loader.OfficeTitles.Add(officeTitle);
            }
        }
        #endregion
        #endregion

        #region Hệ thống danh hiệu cá nhân
        /// <summary>
        /// Danh sách danh hiệu cá nhân
        /// </summary>
        public static Dictionary<int, KTitleXML> RoleTitles { get; } = new Dictionary<int, KTitleXML>();

        /// <summary>
        /// Tải dữ liệu Title.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadRoleTitles(XElement xmlNode)
		{
            Loader.RoleTitles.Clear();
            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Title"))
			{
                KTitleXML titleInfo = KTitleXML.Parse(node);
                Loader.RoleTitles[titleInfo.ID] = titleInfo;
			}
		}
        #endregion

        #region Hệ thống danh hiệu đặc biệt
        /// <summary>
        /// Danh sách danh hiệu đặc biệt
        /// </summary>
        public static Dictionary<int, KSpecialTitleXML> SpecialTitles { get; } = new Dictionary<int, KSpecialTitleXML>();

        /// <summary>
        /// Tải dữ liệu SpecialTitle.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadSpecialTitles(XElement xmlNode)
        {
            Loader.SpecialTitles.Clear();
            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Title"))
            {
                KSpecialTitleXML titleInfo = KSpecialTitleXML.Parse(node);
                Loader.SpecialTitles[titleInfo.ID] = titleInfo;
            }
        }
        #endregion
    }
}
