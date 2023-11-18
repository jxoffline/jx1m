using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bản đồ
    /// </summary>
    public static partial class KTMapManager
    {
        /// <summary>
        /// Tải danh sách bản đồ
        /// </summary>
        public static void Load()
        {
            /// Node thông tin bản đồ
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Map/MapConfig.xml");
            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Map"))
            {
                /// Tạo mới
                GameMap gameMap = new GameMap()
                {
                    MapCode = int.Parse(node.Attribute("ID").Value),
                    MapName = node.Attribute("Name").Value,
                    MapConfigDir = node.Attribute("MapCode").Value,
                    MapLevel = int.Parse(node.Attribute("MapLevel").Value),
                    MapType = node.Attribute("MapType").Value,
                    IsKuaFuMap = node.Attribute("IsKuaFuMap") == null ? false : bool.Parse(node.Attribute("IsKuaFuMap").Value),
                };

                /// Thông báo
                Console.WriteLine("Begin load map ID = {0} - {1} {2}", gameMap.MapCode, gameMap.MapName, gameMap.IsKuaFuMap ? "(CrossServer)": "");

                /// Node thông tin kích thước
                XElement obsInfoNode = KTGlobal.ReadXMLData(string.Format("MapConfig/{0}/Obs.xml", gameMap.MapConfigDir));
                /// Kích thước
                gameMap.MapWidth = int.Parse(obsInfoNode.Attribute("MapWidth").Value);
                gameMap.MapHeight = int.Parse(obsInfoNode.Attribute("MapHeight").Value);
                /// Lưới
                gameMap.Grid = new MapGridManager.MapGrid(gameMap.MapCode, gameMap.MapWidth, gameMap.MapHeight, 20, 20);
                /// Thêm vào danh sách
                KTMapManager.maps[gameMap.MapCode] = gameMap;

                /// Đọc dữ liệu các thành phần trong bản đồ
                gameMap.LoadComponents();

                /// Thêm bản đồ vào danh sách quản lý người chơi
                KTPlayerManager.PlayerContainer.Init(gameMap.MapCode);

                /// Tải danh sách quái trong map
                KTMonsterManager.LoadMapMonsters(gameMap.MapCode, gameMap.MapConfigDir);

                /// Tải danh sách NPC trong Map
                KTNPCManager.LoadMapNPCs(gameMap.MapCode, gameMap.MapConfigDir);

                /// Tải danh sách điểm thu thập trong Map
                KTGrowPointManager.LoadMapGrowPoints(gameMap.MapCode, gameMap.MapConfigDir);

                /// Tải danh sách Bot biểu diễn trong Map
                KTDecoBotManager.LoadMapDecoBot(gameMap.MapCode, gameMap.MapConfigDir);
            }
        }
    }
}
