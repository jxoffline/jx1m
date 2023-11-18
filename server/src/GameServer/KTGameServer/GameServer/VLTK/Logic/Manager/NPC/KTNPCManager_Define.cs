using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý NPC
    /// </summary>
    public static partial class KTNPCManager
    {
        #region Map NPC
        /// <summary>
        /// Template NPC trong bản đồ tương ứng
        /// </summary>
        private class MapNPCData
        {
            /// <summary>
            /// ID Res
            /// </summary>
            public int ResID { get; set; }

            /// <summary>
            /// Tên
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Danh hiệu
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Vị trí X
            /// </summary>
            public int PosX { get; set; }

            /// <summary>
            /// Vị trí Y
            /// </summary>
            public int PosY { get; set; }

            /// <summary>
            /// Hướng quay
            /// </summary>
            public KiemThe.Entities.Direction Direction { get; set; }

            /// <summary>
            /// ID Script
            /// </summary>
            public int ScriptID { get; set; }

            /// <summary>
            /// Tên ở bản đồ khu vực
            /// </summary>
            public string MinimapName { get; set; }

            /// <summary>
            /// Có hiện thông tin ở Minimap không
            /// </summary>
            public bool VisibleOnMinimap { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static MapNPCData Parse(XElement xmlNode)
            {
                return new MapNPCData()
                {
                    ResID = int.Parse(xmlNode.Attribute("Code").Value),
                    Name = xmlNode.Attribute("Name").Value,
                    Title = xmlNode.Attribute("Title").Value,
                    PosX = int.Parse(xmlNode.Attribute("X").Value),
                    PosY = int.Parse(xmlNode.Attribute("Y").Value),
                    Direction = (KiemThe.Entities.Direction) int.Parse(xmlNode.Attribute("Dir").Value),
                    ScriptID = int.Parse(xmlNode.Attribute("ScriptID").Value),
                    MinimapName = xmlNode.Attribute("MinimapName").Value,
                    VisibleOnMinimap = int.Parse(xmlNode.Attribute("VisibleOnMinimap").Value) == 1,
                };
            }
        }
        #endregion

        #region Template NPC
        /// <summary>
        /// Thông tin NPC tương ứng
        /// </summary>
        private class NPCData
        {
            /// <summary>
            /// Res ID
            /// </summary>
            public int ResID { get; set; }

            /// <summary>
            /// Tên Res
            /// </summary>
            public string ResName { get; set; }

            /// <summary>
            /// Tên NPC
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Danh hiệu
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static NPCData Parse(XElement xmlNode)
            {
                return new NPCData()
                {
                    ResID = int.Parse(xmlNode.Attribute("ID").Value),
                    ResName = xmlNode.Attribute("ResName").Value,
                    Name = xmlNode.Attribute("Name").Value,
                    Title = xmlNode.Attribute("Title").Value,
                };
            }
        }
        #endregion

        #region Builder
        /// <summary>
        /// Tạo đối tượng NPC
        /// </summary>
        public class NPCBuilder
        {
            /// <summary>
            /// ID bản đồ
            /// </summary>
            public int MapCode { get; set; }

            /// <summary>
            /// ID phụ bản
            /// </summary>
            public int CopySceneID { get; set; } = -1;

            /// <summary>
            /// ID Res
            /// </summary>
            public int ResID { get; set; }

            /// <summary>
            /// Vị trí X
            /// </summary>
            public int PosX { get; set; }

            /// <summary>
            /// Vị trí Y
            /// </summary>
            public int PosY { get; set; }

            /// <summary>
            /// Tên
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Danh hiệu
            /// </summary>
            public string Title { get; set; } = "";

            /// <summary>
            /// Hướng quay
            /// </summary>
            public KiemThe.Entities.Direction Direction { get; set; } = KiemThe.Entities.Direction.DOWN;

            /// <summary>
            /// ID Script
            /// </summary>
            public int ScriptID { get; set; } = -1;

            /// <summary>
            /// Tag
            /// </summary>
            public string Tag { get; set; } = "";

            /// <summary>
            /// Tên ở Minimap
            /// </summary>
            public string MinimapName { get; set; }

            /// <summary>
            /// Hiển thị trên Minimap không
            /// </summary>
            public bool VisibleOnMinimap { get; set; } = true;
        }
        #endregion
    }
}
