using FS.VLTK.Utilities.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Quản lý Auto tìm đường
    /// </summary>
    public partial class AutoPathManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý Auto tìm đường
        /// </summary>
        public static AutoPathManager Instance { get; private set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng ứng dụng thuật toán Dijkstra để tìm đường đi trong trường hợp có truyền tống phù
        /// </summary>
        private readonly Dictionary<int, Dijkstra> pathFinderWithTeleportItem = new Dictionary<int, Dijkstra>();

        /// <summary>
        /// Đối tượng ứng dụng thuật toán Dijkstra để tìm đường đi trong trường hợp không có truyền tống phù
        /// </summary>
        private Dijkstra pathFinderWithoutTeleportItem;

        /// <summary>
        /// ID truyền tống phù đang sử dụng
        /// </summary>
        private int teleportItemUsing = -1;

        /// <summary>
        /// Danh sách cạnh không có truyền tống phù được nhóm theo đỉnh bắt đầu
        /// </summary>
        private readonly Dictionary<int, List<Entities.Config.AutoPathXML.Node>> commonEdges = new Dictionary<int, List<Entities.Config.AutoPathXML.Node>>();

        /// <summary>
        /// Danh sách cạnh có truyền tống phù được nhóm theo đỉnh bắt đầu
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, List<Entities.Config.AutoPathXML.Node>>> teleportItemEdges = new Dictionary<int, Dictionary<int, List<Entities.Config.AutoPathXML.Node>>>();
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            AutoPathManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.DoLogic());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Nhóm tập các danh sách cạnh với đỉnh bắt đầu tương ứng
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="dict"></param>
        private void GroupEdges(List<Entities.Config.AutoPathXML.Node> edges, Dictionary<int, List<Entities.Config.AutoPathXML.Node>> dict)
        {
            dict.Clear();
            foreach (Entities.Config.AutoPathXML.Node edge in edges)
            {
                if (!dict.TryGetValue(edge.FromMapCode, out _))
                {
                    dict[edge.FromMapCode] = new List<Entities.Config.AutoPathXML.Node>();
                }
                dict[edge.FromMapCode].Add(edge);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Chuẩn bị dữ liệu
        /// </summary>
        public void PrepareData()
        {   
            /// Nhóm danh sách cạnh
            List<Entities.Config.AutoPathXML.Node> commonGroup = new List<Entities.Config.AutoPathXML.Node>();
            commonGroup.AddRange(Loader.Loader.AutoPaths.TransferNPCs);
            commonGroup.AddRange(Loader.Loader.AutoPaths.Teleports);
            this.GroupEdges(commonGroup, this.commonEdges);
            /// Nhóm danh sách cạnh theo truyền tống phù
            foreach (KeyValuePair<int, List<Entities.Config.AutoPathXML.Node>> pair in Loader.Loader.AutoPaths.TeleportItems)
            {
                if (!this.teleportItemEdges.TryGetValue(pair.Key, out _))
                {
                    this.teleportItemEdges[pair.Key] = new Dictionary<int, List<Entities.Config.AutoPathXML.Node>>();
                }
                List<Entities.Config.AutoPathXML.Node> list = new List<Entities.Config.AutoPathXML.Node>(commonGroup);
                list.AddRange(pair.Value);
                this.GroupEdges(list, this.teleportItemEdges[pair.Key]);
            }

            /// Danh sách cạnh của Dijkstra
            List<Dijkstra.Edge> commonEdges = new List<Dijkstra.Edge>();

            /// Danh sách NPC dịch chuyển
            foreach (Entities.Config.AutoPathXML.Node node in Loader.Loader.AutoPaths.TransferNPCs)
            {
                commonEdges.Add(new Dijkstra.Edge()
                {
                    FromNode = node.FromMapCode,
                    ToNode = node.ToMapCode,
                    Weight = node.Weight,
                });
            }

            /// Danh sách dịch chuyển theo cổng Teleport
            foreach (Entities.Config.AutoPathXML.Node node in Loader.Loader.AutoPaths.Teleports)
            {
                commonEdges.Add(new Dijkstra.Edge()
                {
                    FromNode = node.FromMapCode,
                    ToNode = node.ToMapCode,
                    Weight = node.Weight,
                });
            }

            /// Danh sách chuyển đổi từ Node sang Dijkstra.Edge, để giữ các cạnh trùng nhau không phải tạo nhiều thực thể gây lãng phí RAM
            Dictionary<Entities.Config.AutoPathXML.Node, Dijkstra.Edge> nodesToEdges = new Dictionary<Entities.Config.AutoPathXML.Node, Dijkstra.Edge>();

            /// Duyệt danh sách các vật phẩm truyền tống phù
            foreach (KeyValuePair<int, List<Entities.Config.AutoPathXML.Node>> pair in Loader.Loader.AutoPaths.TeleportItems)
            {
                List<Dijkstra.Edge> edges = new List<Dijkstra.Edge>();
                foreach (Entities.Config.AutoPathXML.Node node in pair.Value)
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

            /// Thiết lập giá trj cho danh sách dịch chuyển không có truyền tống phù
            this.pathFinderWithoutTeleportItem = new Dijkstra(commonEdges);
        }

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
                this.teleportItemUsing = -1;
                return this.FindPathWithoutTeleportItem(fromMapCode, toMapCode);
            }
            this.teleportItemUsing = itemID;
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
            this.teleportItemUsing = -1;
            this.currentPaths = this.pathFinderWithoutTeleportItem.FindPath(fromMapCode, toMapCode);
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
                this.teleportItemUsing = -1;
                return this.FindPathWithoutTeleportItem(fromMapCode, toMapCodes);
            }
            this.teleportItemUsing = itemID;
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
            this.teleportItemUsing = -1;
            this.currentPaths = this.pathFinderWithoutTeleportItem.FindPath(fromMapCode, toMapCodes);
            if (this.currentPaths.Count <= 0)
            {
                this.currentPaths = null;
            }
            return this.currentPaths;
        }
        #endregion
    }
}
