using GameServer.KiemThe;
using Server.Tools;
using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý NPC
    /// </summary>
    public static partial class KTNPCManager
    {
        /// <summary>
        /// Tải danh sách Template NPC trong hệ thống
        /// </summary>
        public static void LoadNPCTemplates()
        {
            /// Nội dung File
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_NPC/NPCs.xml");

            /// Duyệt danh sách
            foreach (XElement node in xmlNode.Element("NPCs").Elements("NPC"))
            {
                /// Template tương ứng
                NPCData npcData = NPCData.Parse(node);
                /// Thêm vào danh sách
                KTNPCManager.npcTemplates[npcData.ResID] = npcData;
            }
        }

        /// <summary>
        /// Tải danh sách NPC trong bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="mapName"></param>
        public static void LoadMapNPCs(int mapCode, string mapName)
        {
            /// Tên File tương ứng
            string fileName = string.Format("MapConfig/{0}/NPCs.xml", mapName);
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
            foreach (XElement node in xmlNode.Elements("NPCs").Elements("NPC"))
            {
                /// Thông tin NPC tương ứng
                MapNPCData npcData = MapNPCData.Parse(node);
                /// Thông tin Res
                if (!KTNPCManager.npcTemplates.TryGetValue(npcData.ResID, out NPCData templateData))
                {
                    /// Thông báo
                    Console.WriteLine(string.Format("NPC ResID = {0} not found.", npcData.ResID));
                    LogManager.WriteLog(LogTypes.Error, string.Format("NPC ResID = {0} not found.", npcData.ResID));
                    /// Tiếp tục
                    continue;
                }

                /// Tạo mới NPC
                NPC npc = new NPC()
                {
                    ResID = npcData.ResID,
                    Name = npcData.Name,
                    Title = npcData.Title,
                    MapCode = mapCode,
                    CopyMapID = -1,
                    CurrentPos = new Point(npcData.PosX, npcData.PosY),
                    CurrentDir = npcData.Direction,
                    ScriptID = npcData.ScriptID,
                    MinimapName = npcData.MinimapName,
                    VisibleOnMinimap = npcData.VisibleOnMinimap,
                };

                /// Nếu không có tên
                if (string.IsNullOrEmpty(npc.Name))
                {
                    /// Lấy tên ở Template
                    npc.Name = templateData.Name;
                }
                /// Nếu không có danh hiệu
                if (string.IsNullOrEmpty(npc.Title))
                {
                    /// Lấy tên ở Template
                    npc.Title = templateData.Title;
                }
                /// Nếu không có tên ở Minimap
                if (string.IsNullOrEmpty(npc.MinimapName))
                {
                    /// Lấy tên ở Template
                    npc.MinimapName = templateData.Name;
                }

                /// Thêm vào danh sách
                KTNPCManager.Add(npc);
            }
        }
    }
}
