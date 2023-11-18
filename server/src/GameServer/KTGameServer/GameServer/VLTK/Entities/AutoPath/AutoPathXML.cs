using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng cấu hình tự tìm đường liên bản đồ
    /// </summary>
    public class AutoPathXML
    {
        /// <summary>
        /// Độ ưu tiên tìm đường (tương đương trọng số của đường đi)
        /// </summary>
        public enum PathFindingPriority
        {
            /// <summary>
            /// Truyền tống phù
            /// </summary>
            TeleportItems = 1,
            /// <summary>
            /// Dịch qua NPC
            /// </summary>
            TransferNPC = 10,
            /// <summary>
            /// Dịch qua công dịch chuyển
            /// </summary>
            Teleport = 100,
        }

        /// <summary>
        /// Đối tượng thông tin dịch chuyển
        /// </summary>
        public class Node
        {
            /// <summary>
            /// Từ ID bản đồ
            /// </summary>
            public int FromMapCode { get; set; }

            /// <summary>
            /// Tới ID bản đồ
            /// </summary>
            public int ToMapCode { get; private set; }

            /// <summary>
            /// Vị trí xuất phát X
            /// </summary>
            public int PosX { get; private set; }

            /// <summary>
            /// Vị trí xuất phát Y
            /// </summary>
            public int PosY { get; private set; }

            /// <summary>
            /// Vị trí đích đến X
            /// </summary>
            public int ToX { get; private set; }

            /// <summary>
            /// Vị trí đích đến Y
            /// </summary>
            public int ToY { get; private set; }

            /// <summary>
            /// Trọng số tương ứng
            /// </summary>
            public int Weight { get; private set; }

            /// <summary>
            /// Chuyển đối tượng từ XML Node
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <param name="weight"></param>
            /// <returns></returns>
            public static Node Parse(XElement xmlNode, int weight)
            {
                return new Node()
                {
                    FromMapCode = int.Parse(xmlNode.Attribute("FromMapCode").Value),
                    ToMapCode = int.Parse(xmlNode.Attribute("ToMapCode").Value),
                    PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                    PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                    ToX = int.Parse(xmlNode.Attribute("ToX").Value),
                    ToY = int.Parse(xmlNode.Attribute("ToY").Value),
                    Weight = weight,
                };
            }

            /// <summary>
            /// Nhân bản đối tượng dưới ID bản đồ xuất phát khác
            /// </summary>
            /// <param name="fromMapCode"></param>
            /// <returns></returns>
            public Node Clone(int fromMapCode)
            {
                return new Node()
                {
                    FromMapCode = fromMapCode,
                    ToMapCode = this.ToMapCode,
                    PosX = this.PosX,
                    PosY = this.PosY,
                    ToX = this.ToX,
                    ToY = this.ToY,
                    Weight = this.Weight,
                };
            }
        }

        /// <summary>
        /// Nhóm bản đồ
        /// </summary>
        public class MapGroup
        {
            /// <summary>
            /// ID nhóm
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Tên nhóm
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Danh sách ID bản đồ thuộc nhóm
            /// </summary>
            public List<int> Maps { get; set; }
        }

        /// <summary>
        /// Danh sách nhóm bản đồ
        /// </summary>
        public Dictionary<int, MapGroup> MapGroups { get; private set; }

        /// <summary>
        /// Danh sách dịch chuyển theo NPC ID
        /// </summary>
        public List<Node> TransferNPCs { get; private set; }

        /// <summary>
        /// Danh sách dịch chuyển theo truyền tống phù
        /// </summary>
        public Dictionary<int, List<Node>> TeleportItems { get; private set; }

        /// <summary>
        /// Danh sách dịch chuyển theo cổng dịch chuyển
        /// </summary>
        public List<Node> Teleports { get; private set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static AutoPathXML Parse(XElement xmlNode)
        {
            AutoPathXML autoPathXML = new AutoPathXML()
            {
                TeleportItems = new Dictionary<int, List<Node>>(),
                Teleports = new List<Node>(),
                TransferNPCs = new List<Node>(),
            };

            #region Danh sách nhóm bản đồ
            autoPathXML.MapGroups = new Dictionary<int, MapGroup>();
            foreach (XElement node in xmlNode.Element("MapGroup").Elements("Group"))
            {
                int groupID = int.Parse(node.Attribute("ID").Value);
                string groupName = node.Attribute("Name").Value;
                string[] mapStrings = node.Attribute("MapList").Value.Split(';');
                List<int> maps = new List<int>();
                foreach (string mapStr in mapStrings)
                {
                    int mapID = int.Parse(mapStr);
                    maps.Add(mapID);
                }
                autoPathXML.MapGroups[groupID] = new MapGroup()
                {
                    ID = groupID,
                    Name = groupName,
                    Maps = maps,
                };
            }
            #endregion

            /// Tạo danh sách Node theo ID nhóm
            List<Node> MakeNodesCorrespondingToMapGroup(Node formNode)
            {
                List<Node> nodes = new List<Node>();
                int groupID = formNode.FromMapCode;

                foreach (int mapCode in autoPathXML.MapGroups[groupID].Maps)
                {
                    nodes.Add(formNode.Clone(mapCode));
                }

                return nodes;
            }

            /// Tạo danh sách Node trên tất cả bản đồ
            List<Node> MakeNodesWholeMaps(Node formNode)
            {
                List<Node> nodes = new List<Node>();

                foreach (GameMap map in KTMapManager.GetAll())
                {
                    nodes.Add(formNode.Clone(map.MapCode));
                }

                return nodes;
            }

            #region Teleport Items
            foreach (XElement node in xmlNode.Element("TeleportItem").Elements("Item"))
            {
                /// Danh sách vật phẩm tương ứng
                string[] itemStrings = node.Attribute("IDList").Value.Split(';');
                List<int> items = new List<int>();
                foreach (string itemStr in itemStrings)
                {
                    int itemID = int.Parse(itemStr);
                    items.Add(itemID);
                }

                /// Danh sách nút chung
                List<Node> nodes = new List<Node>();
                foreach (XElement childNode in node.Elements("Node"))
                {
                    int idx = int.Parse(childNode.Attribute("FromMapCode").Value);
                    Node formNode = Node.Parse(childNode, (int) PathFindingPriority.TeleportItems);
                    /// Nếu đây là nhóm bản đồ
                    if (idx < 0)
                    {
                        nodes.AddRange(MakeNodesCorrespondingToMapGroup(formNode));
                    }
                    /// Nếu là tất cả bản đồ
                    else if (idx == 0)
                    {
                        nodes.AddRange(MakeNodesWholeMaps(formNode));
                    }
                    /// Nếu là bản đồ thường
                    else
                    {
                        nodes.Add(formNode);
                    }
                }

                /// Nhân bản ra danh sách vật phẩm tương ứng
                foreach (int itemID in items)
                {
                    if (!autoPathXML.TeleportItems.TryGetValue(itemID, out _))
                    {
                        autoPathXML.TeleportItems[itemID] = new List<Node>();
                    }
                    autoPathXML.TeleportItems[itemID].AddRange(nodes);
                }
            }
            #endregion

            #region Transfer NPCs
            foreach (XElement node in xmlNode.Element("TransferNPC").Elements("Node"))
            {
                Node formNode = Node.Parse(node, (int) PathFindingPriority.TransferNPC);
                autoPathXML.TransferNPCs.Add(formNode);
            }
            #endregion

            #region Teleport
            foreach (XElement node in xmlNode.Element("Teleport").Elements("Node"))
            {
                Node formNode = Node.Parse(node, (int) PathFindingPriority.Teleport);
                autoPathXML.Teleports.Add(formNode);
            }
            #endregion

            return autoPathXML;
        }
    }
}
