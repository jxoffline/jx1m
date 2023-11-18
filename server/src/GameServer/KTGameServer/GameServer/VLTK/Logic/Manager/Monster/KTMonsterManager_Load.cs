using GameServer.Logic;
using Server.Tools;
using System;
using System.IO;
using System.Xml.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Tải danh sách Template quái trong hệ thống
        /// </summary>
        public static void LoadMonsterTemplates()
        {
            /// Nội dung File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Monster/Monsters.xml");

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Monster"))
            {
                /// Template tương ứng
                MonsterTemplateData monsterData = MonsterTemplateData.Parse(node);
                /// Thêm vào danh sách
                KTMonsterManager.templateMonsters[monsterData.Code] = monsterData;
            }
        }

        /// <summary>
        /// Tải danh sách quái trong bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="mapName"></param>
        public static void LoadMapMonsters(int mapCode, string mapName)
        {
            /// Tên File tương ứng
            string fileName = string.Format("MapConfig/{0}/Monsters.xml", mapName);
            /// Nội dung File
            XElement xmlNode = KTGlobal.ReadXMLData(fileName);
            /// Toác
            if (xmlNode == null)
            {
                /// Thông báo
                Console.WriteLine(string.Format("Read {0} failed.", fileName));
                LogManager.WriteLog(LogTypes.Error, string.Format("Read {0} failed.", fileName));
                /// Thoát
                return;
            }

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Monsters").Elements("Monster"))
            {
                /// Thông tin khu vực tương ứng
                MapMonsterZoneData zoneData = MapMonsterZoneData.Parse(node);
                /// Tạo khu vực tương ứng
                KTMonsterManager.AddMonsterZone(mapCode, zoneData);
            }
        }

        /// <summary>
        /// Tải danh sách Boss thế giới
        /// </summary>
        public static void LoadWorldBoss()
        {
            /// Nội dung File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Monster/WorldBoss.xml");

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Elements("Group"))
            {
                /// Thông tin
                WorldBossManager.WorldBossData data = WorldBossManager.WorldBossData.Parse(node);
                /// Thêm vào danh sách
                WorldBossManager.Add(data);
            }

            /// Khởi tạo Boss thế giới
            WorldBossManager.Initlaize();
        }
    }
}
