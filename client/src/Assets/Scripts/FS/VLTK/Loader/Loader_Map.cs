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
        #region MapConfig
        /// <summary>
        /// Danh sách bản đồ trong Game
        /// </summary>
        public static Dictionary<int, Map> Maps { get; private set; } = new Dictionary<int, Map>();

        /// <summary>
        /// Đọc dữ liệu file Map.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadMapConfig(XElement xmlNode)
        {
            Loader.Maps.Clear();

            foreach (XElement node in xmlNode.Elements("Map"))
            {
                Map map = Map.Parse(node);
                Loader.Maps[map.ID] = map;
            }
        }
        #endregion

        #region Bản đồ thế giới
        /// <summary>
        /// Thông tin bản đồ thế giới
        /// </summary>
        public static WorldMapXML WorldMap { get; private set; }

        /// <summary>
        /// Đọc dữ liệu bản đồ thế giới
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadWorldMap(XElement xmlNode)
        {
            Loader.WorldMap = WorldMapXML.Parse(xmlNode);
        }
        #endregion

        #region Bản đồ liên máy chủ
        /// <summary>
        /// Thông tin bản đồ liên máy chủ
        /// </summary>
        public static CrossServerMapXML CrossServerMap { get; private set; }

        /// <summary>
        /// Đọc dữ liệu bản đồ liên máy chủ
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadCrossServerMap(XElement xmlNode)
        {
            Loader.CrossServerMap = CrossServerMapXML.Parse(xmlNode);
        }
        #endregion

        #region Bản đồ lãnh thổ
        /// <summary>
        /// Danh sách bản đồ lãnh thổ
        /// </summary>
        public static Dictionary<int, ColonyMapXML> ColonyMaps { get; private set; } = new Dictionary<int, ColonyMapXML>();

        /// <summary>
        /// Đọc dữ liệu bản đồ lãnh thổ
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadColonyMaps(XElement xmlNode)
		{
            Loader.ColonyMaps.Clear();

            foreach (XElement node in xmlNode.Elements("Node"))
			{
                ColonyMapXML colonyMap = ColonyMapXML.Parse(node);
                Loader.ColonyMaps[colonyMap.MapID] = colonyMap;
			}
		}
		#endregion
	}
}
