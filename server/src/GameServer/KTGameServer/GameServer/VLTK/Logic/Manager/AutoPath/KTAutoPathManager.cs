using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities.Algorithms;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Logic.Manager
{
    /// <summary>
    /// Lớp quản lý tự tìm đường
    /// </summary>
    public class KTAutoPathManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Lớp quản lý tự tìm đường
        /// </summary>
        public static KTAutoPathManager Instance { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Thông tin tự tìm đường
        /// </summary>
        private AutoPathXML AutoPaths = null;

        /// <summary>
        /// Đường đi hiện tại đang chạy
        /// NULL nếu không có đường đi
        /// </summary>
        private List<int> currentPaths = null;

        /// <summary>
        /// Đối tượng ứng dụng thuật toán Dijkstra để tìm đường đi trong trường hợp có truyền tống phù
        /// </summary>
        private readonly Dictionary<int, Dijkstra> pathFinderWithTeleportItem = new Dictionary<int, Dijkstra>();

        /// <summary>
        /// Đối tượng ứng dụng thuật toán Dijkstra để tìm đường đi trong trường hợp không có truyền tống phù
        /// </summary>
        private Dijkstra pathFinderWithoutTeleportItem;

        /// <summary>
        /// Đối tượng ứng dụng thuật toán Dijkstra để tìm đường đi trong trường hợp chỉ dùng cổng dịch chuyển
        /// </summary>
        private Dijkstra pathFinderUsingTeleportOnly;

        /// <summary>
        /// Danh sách cạnh không có truyền tống phù được nhóm theo đỉnh bắt đầu
        /// </summary>
        private readonly Dictionary<int, List<AutoPathXML.Node>> commonEdges = new Dictionary<int, List<AutoPathXML.Node>>();

        /// <summary>
        /// Danh sách cạnh chỉ dùng cổng dịch chuyển được nhóm theo đỉnh bắt đầu
        /// </summary>
        private readonly Dictionary<int, List<AutoPathXML.Node>> teleportEdges = new Dictionary<int, List<AutoPathXML.Node>>();

        /// <summary>
        /// Danh sách cạnh có truyền tống phù được nhóm theo đỉnh bắt đầu
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, List<AutoPathXML.Node>>> teleportItemEdges = new Dictionary<int, Dictionary<int, List<AutoPathXML.Node>>>();
        #endregion

        #region Properties
        /// <summary>
        /// Danh sách cạnh không có truyền tống phù được nhóm theo đỉnh bắt đầu
        /// </summary>
        public Dictionary<int, List<AutoPathXML.Node>> Edges
        {
            get
            {
                return this.commonEdges;
            }
        }


        /// <summary>
        /// Danh sách cạnh chỉ dùng cổng dịch chuyển được nhóm theo đỉnh bắt đầu
        /// </summary>
        public Dictionary<int, List<AutoPathXML.Node>> TeleportEdges
        {
            get
            {
                return this.teleportEdges;
            }
        }
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_AutoPath/AutoPath.xml");
            KTAutoPathManager.Instance = new KTAutoPathManager();
            KTAutoPathManager.Instance.AutoPaths = AutoPathXML.Parse(xmlNode);
            KTAutoPathManager.Instance.PrepareData();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Chuẩn bị dữ liệu
        /// </summary>
        public void PrepareData()
        {
            /// Nhóm danh sách cạnh
            List<AutoPathXML.Node> commonGroup = new List<AutoPathXML.Node>();
            commonGroup.AddRange(this.AutoPaths.TransferNPCs);
            commonGroup.AddRange(this.AutoPaths.Teleports);
            this.GroupEdges(commonGroup, this.commonEdges);

            /// Nhóm danh sách cạnh chỉ dùng cổng dịch chuyển
            List<AutoPathXML.Node> teleportOnlyGroup = new List<AutoPathXML.Node>();
            teleportOnlyGroup.AddRange(this.AutoPaths.Teleports);
            this.GroupEdges(teleportOnlyGroup, this.teleportEdges);
            
            /// Nhóm danh sách cạnh theo truyền tống phù
            foreach (KeyValuePair<int, List<AutoPathXML.Node>> pair in this.AutoPaths.TeleportItems)
            {
                if (!this.teleportItemEdges.TryGetValue(pair.Key, out _))
                {
                    this.teleportItemEdges[pair.Key] = new Dictionary<int, List<AutoPathXML.Node>>();
                }
                List<AutoPathXML.Node> list = new List<AutoPathXML.Node>(commonGroup);
                list.AddRange(pair.Value);
                this.GroupEdges(list, this.teleportItemEdges[pair.Key]);
            }

            /// Danh sách cạnh của Dijkstra chỉ sử dụng cổng Teleport
            List<Dijkstra.Edge> teleportEdges = new List<Dijkstra.Edge>();
            /// Danh sách cạnh của Dijkstra
            List<Dijkstra.Edge> commonEdges = new List<Dijkstra.Edge>();

            /// Danh sách NPC dịch chuyển
            foreach (AutoPathXML.Node node in this.AutoPaths.TransferNPCs)
            {
                commonEdges.Add(new Dijkstra.Edge()
                {
                    FromNode = node.FromMapCode,
                    ToNode = node.ToMapCode,
                    Weight = node.Weight,
                });
            }

            /// Danh sách dịch chuyển theo cổng Teleport
            foreach (AutoPathXML.Node node in this.AutoPaths.Teleports)
            {
                commonEdges.Add(new Dijkstra.Edge()
                {
                    FromNode = node.FromMapCode,
                    ToNode = node.ToMapCode,
                    Weight = node.Weight,
                });
                teleportEdges.Add(new Dijkstra.Edge()
                {
                    FromNode = node.FromMapCode,
                    ToNode = node.ToMapCode,
                    Weight = node.Weight,
                });
            }

            /// Danh sách chuyển đổi từ Node sang Dijkstra.Edge, để giữ các cạnh trùng nhau không phải tạo nhiều thực thể gây lãng phí RAM
            Dictionary<AutoPathXML.Node, Dijkstra.Edge> nodesToEdges = new Dictionary<AutoPathXML.Node, Dijkstra.Edge>();

            /// Duyệt danh sách các vật phẩm truyền tống phù
            foreach (KeyValuePair<int, List<AutoPathXML.Node>> pair in this.AutoPaths.TeleportItems)
            {
                List<Dijkstra.Edge> edges = new List<Dijkstra.Edge>();
                foreach (AutoPathXML.Node node in pair.Value)
                {
                    if (!nodesToEdges.TryGetValue(node, out _))
                    {
                        nodesToEdges[node] = new Dijkstra.Edge()
                        {
                            FromNode = node.FromMapCode,
                            ToNode = node.ToMapCode,
                            Weight = node.Weight,
                        };
                    }
                    edges.Add(nodesToEdges[node]);
                }
                /// Thêm thông tin từ NPC và cổng dịch chuyển vào
                edges.AddRange(commonEdges);
                /// Thiết lập giá trị cho danh sách dịch chuyển với truyền tống phù
                this.pathFinderWithTeleportItem[pair.Key] = new Dijkstra(edges);
            }

            /// Thiết lập giá trị cho danh sách dịch chuyển không có truyền tống phù
            this.pathFinderWithoutTeleportItem = new Dijkstra(commonEdges);
            /// Thiết lập giá trị cho danh sách dịch chuyển chỉ thông qua cổng dịch chuyển
            this.pathFinderUsingTeleportOnly = new Dijkstra(teleportEdges);
        }

        /// <summary>
        /// Nhóm tập các danh sách cạnh với đỉnh bắt đầu tương ứng
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="dict"></param>
        private void GroupEdges(List<AutoPathXML.Node> edges, Dictionary<int, List<AutoPathXML.Node>> dict)
        {
            dict.Clear();
            foreach (AutoPathXML.Node edge in edges)
            {
                if (!dict.TryGetValue(edge.FromMapCode, out _))
                {
                    dict[edge.FromMapCode] = new List<AutoPathXML.Node>();
                }
                dict[edge.FromMapCode].Add(edge);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện tìm đường đi giữa 2 bản đồ chỉ định có sử dụng truyền tống phù
        /// <para>Nếu không có truyền tống phù thì sẽ tự gọi hàm không dùng truyền tống phù</para>
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCode"></param>
        /// <returns></returns>
        public List<int> FindPathWithTeleportItem(int itemID, int fromMapCode, int toMapCode)
        {
            if (!this.pathFinderWithTeleportItem.TryGetValue(itemID, out Dijkstra dijkstra))
            {
                return this.FindPathWithoutTeleportItem(fromMapCode, toMapCode);
            }
            this.currentPaths = dijkstra.FindPath(fromMapCode, toMapCode);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Thực hiện tìm đường đi giữa 2 bản đồ chỉ định không sử dụng truyền tống phù
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCode"></param>
        /// <returns></returns>
        public List<int> FindPathWithoutTeleportItem(int fromMapCode, int toMapCode)
        {
            this.currentPaths = this.pathFinderWithoutTeleportItem.FindPath(fromMapCode, toMapCode);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Thực hiện tìm đường đi giữa 2 bản đồ chỉ định chỉ sử dụng cổng dịch chuyển
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCode"></param>
        /// <returns></returns>
        public List<int> FindPathUsingTeleportOnly(int fromMapCode, int toMapCode)
        {
            this.currentPaths = this.pathFinderUsingTeleportOnly.FindPath(fromMapCode, toMapCode);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Thực hiện tìm đường đi giữa bản đồ đích với các bản đồ chỉ định và chọn ra bản đồ gần nhất có sử dụng truyền tống phù
        /// <para>Nếu không có truyền tống phù thì sẽ tự gọi hàm không dùng truyền tống phù</para>
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCodes"></param>
        /// <returns></returns>
        public List<int> FindPathWithTeleportItem(int itemID, int fromMapCode, List<int> toMapCodes)
        {
            if (!this.pathFinderWithTeleportItem.TryGetValue(itemID, out Dijkstra dijkstra))
            {
                return this.FindPathWithoutTeleportItem(fromMapCode, toMapCodes);
            }
            this.currentPaths = dijkstra.FindPath(fromMapCode, toMapCodes);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Thực hiện tìm đường đi giữa bản đồ đích với các bản đồ chỉ định và chọn ra bản đồ gần nhất không sử dụng truyền tống phù
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCodes"></param>
        /// <returns></returns>
        public List<int> FindPathWithoutTeleportItem(int fromMapCode, List<int> toMapCodes)
        {
            this.currentPaths = this.pathFinderWithoutTeleportItem.FindPath(fromMapCode, toMapCodes);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Thực hiện tìm đường đi giữa bản đồ đích với các bản đồ chỉ định và chọn ra bản đồ gần nhất chỉ sử dụng cổng dịch chuyển
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="toMapCodes"></param>
        /// <returns></returns>
        public List<int> FindPathUsingTeleportOnly(int fromMapCode, List<int> toMapCodes)
        {
            this.currentPaths = this.pathFinderUsingTeleportOnly.FindPath(fromMapCode, toMapCodes);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }

        /// <summary>
        /// Kiểm tra trong danh sách tự tìm đường có điểm dịch chuyển ở NPC tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMapCode"></param>
        /// <param name="npcResID"></param>
        /// <returns></returns>
        public bool CanTransferFromNPCToMap(KPlayer player, int toMapCode, out AutoPathXML.Node node)
        {
            int currentMapCode = player.CurrentMapCode;
            int posX = (int) player.CurrentPos.X;
            int posY = (int) player.CurrentPos.Y;
            UnityEngine.Vector2 fromPos = new UnityEngine.Vector2(posX, posY);
            node = this.AutoPaths.TransferNPCs.Where((nodeInfo) => {
                if (nodeInfo.FromMapCode != player.CurrentMapCode || nodeInfo.ToMapCode != toMapCode)
                {
                    return false;
                }
                UnityEngine.Vector2 npcPos = new UnityEngine.Vector2(nodeInfo.PosX, nodeInfo.PosY);
                return UnityEngine.Vector2.Distance(fromPos, npcPos) <= 100;
            }).FirstOrDefault();

            /// Trả về giá trị nếu NODE tồn tại
            return node != null;
        }

        /// <summary>
        /// Kiểm tra trong danh sách tự tìm đường có điểm dịch chuyển theo vật phẩm ID tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <param name="toMapCode"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CanTransferByUsingTeleportItem(KPlayer player, int itemID, int toMapCode, out AutoPathXML.Node node)
        {
            /// Mặc định Node NULL
            node = null;
            /// Nếu vật phẩm không tồn tại trong danh sách
            if (!this.AutoPaths.TeleportItems.TryGetValue(itemID, out List<AutoPathXML.Node> paths))
            {
                return false;
            }

            int currentMapCode = player.CurrentMapCode;
            int posX = (int) player.CurrentPos.X;
            int posY = (int) player.CurrentPos.Y;
            UnityEngine.Vector2 fromPos = new UnityEngine.Vector2(posX, posY);
            node = paths.Where((nodeInfo) => {
                if ((nodeInfo.FromMapCode != 0 && nodeInfo.FromMapCode != player.CurrentMapCode) || nodeInfo.ToMapCode != toMapCode)
                {
                    return false;
                }
                UnityEngine.Vector2 pos = new UnityEngine.Vector2(nodeInfo.PosX, nodeInfo.PosY);

                /// Nếu không có tọa độ
                if (pos.x == -1 && pos.y == -1)
                {
                    return true;
                }
                else
                {
                    return UnityEngine.Vector2.Distance(fromPos, pos) <= 100;
                }
            }).FirstOrDefault();

            /// Trả về giá trị nếu NODE tồn tại
            return node != null;
        }
        #endregion
    }
}