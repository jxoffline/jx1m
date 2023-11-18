using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Utilities.Algorithms
{
    /// <summary>
    /// Lớp mô tả thuật toán Dijkstra
    /// </summary>
    public class Dijkstra
    {
        /// <summary>
        /// Đối tượng mô tả 1 cạnh trên đường đi
        /// </summary>
        public class Edge
        {
            /// <summary>
            /// Vị trí bắt đầu
            /// </summary>
            public int FromNode { get; set; }

            /// <summary>
            /// Vị trí đích
            /// </summary>
            public int ToNode { get; set; }

            /// <summary>
            /// Trọng số tương ứng
            /// </summary>
            public int Weight { get; set; }
        }

        /// <summary>
        /// Danh sách cạnh tương ứng
        /// </summary>
        private Dictionary<int, List<Edge>> Edges;

        /// <summary>
        /// Danh sách đỉnh tương ứng
        /// </summary>
        private List<int> NodesIndex;

        #region Constructor
        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        /// <param name="edges"></param>
        public Dijkstra(List<Edge> edges)
        {
            this.PrepareEdgesList(edges);
        }
        #endregion

        #region Core
        /// <summary>
        /// Chuẩn bị danh sách cạch
        /// </summary>
        /// <param name="edges"></param>
        private void PrepareEdgesList(List<Edge> edges)
        {
            /// Làm mới danh sách cạnh
            this.Edges = new Dictionary<int, List<Edge>>();
            this.NodesIndex = new List<int>();

            /// Duyệt danh sách đầu vào
            foreach (Edge edge in edges)
            {
                /// Nếu đỉnh tương ứng chưa được nạp vào danh sách đỉnh
                if (!this.NodesIndex.Contains(edge.FromNode))
                {
                    this.NodesIndex.Add(edge.FromNode);
                }
                if (!this.NodesIndex.Contains(edge.ToNode))
                {
                    this.NodesIndex.Add(edge.ToNode);
                }

                /// Nếu chưa tồn tại vị trí từ đỉnh gốc tương ứng
                if (!this.Edges.TryGetValue(edge.FromNode, out _))
                {
                    this.Edges[edge.FromNode] = new List<Edge>();
                }
                /// Thêm cạnh tương ứng vào danh sách
                this.Edges[edge.FromNode].Add(edge);
            }
        }
        #endregion

        #region Implements
        /// <summary>
        /// Tìm đường đi giữa 2 nút bất kỳ sử dụng thuật toán Dijkstra truyền thống - O(N*M)
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNode"></param>
        /// <returns></returns>
        public List<int> FindPathUsingOriginDijkstra(int fromNode, int toNode)
        {
            /// Kết quả trả về
            List<int> results = new List<int>();

            /// Mảng đánh dấu danh sách các điểm đã đi qua
            HashSet<int> passedNodes = new HashSet<int>();

            /// Ma trận đường đi
            Dictionary<int, int> distances = new Dictionary<int, int>();
            /// Danh sách các đỉnh đã đi qua để đến được vị trí tương ứng
            Dictionary<int, int> traces = new Dictionary<int, int>();

            /// Đổ dữ liệu ban đầu
            foreach (int nodeIndex in this.NodesIndex)
            {
                /// Nếu là nút bắt đầu
                if (nodeIndex == fromNode)
                {
                    distances[nodeIndex] = 0;
                }
                /// Nếu không phải nút bắt đầu
                else
                {
                    distances[nodeIndex] = int.MaxValue;
                }

                traces[nodeIndex] = -1;
            }

            /// Lặp liên tục
            while (true)
            {
                /// Tìm nút có trọng số nhỏ nhất
                int minV = int.MaxValue;
                int thisNodeIndex = -1;
                foreach (KeyValuePair<int, int> pair in distances)
                {
                    if (!passedNodes.Contains(pair.Key) && minV > pair.Value)
                    {
                        minV = pair.Value;
                        thisNodeIndex = pair.Key;
                    }
                }

                /// Nếu không có đường đi
                if (thisNodeIndex == -1)
                {
                    //Console.WriteLine("Weight = " + "INF");
                    break;
                }

                /// Nếu đã đến đích
                if (thisNodeIndex == toNode)
                {
                    //Console.WriteLine("Weight = " + distances[thisNodeIndex]);
                    while (thisNodeIndex != -1)
                    {
                        results.Add(thisNodeIndex);
                        thisNodeIndex = traces[thisNodeIndex];
                    }
                    break;
                }

                /// Đánh dấu đỉnh đã đi qua rồi
                passedNodes.Add(thisNodeIndex);

                /// Nếu tồn tại danh sách đỉnh từ vị trí bắt đầu tương ứng
                if (this.Edges.TryGetValue(thisNodeIndex, out List<Edge> edges))
                {
                    /// Duyệt danh sách cạnh đi từ điểm tương ứng
                    foreach (Edge edge in edges)
                    {
                        /// Nếu khoảng cách từ vị trí xuất phát đến đỉnh đích lớn hơn khoảng cách từ vị trí xuất phát đến đỉnh bắt đầu và đi qua cạnh này đến đỉnh đích
                        /// d[v] > d[u] + c[u,v]
                        if (distances[edge.ToNode] > distances[thisNodeIndex] + edge.Weight)
                        {
                            /// Gắn lại khoảng cách
                            distances[edge.ToNode] = distances[thisNodeIndex] + edge.Weight;
                            /// Cập nhật lại vị trí trước đó
                            traces[edge.ToNode] = edge.FromNode;
                        }
                    }
                }
            }

            /// Đảo ngược danh sách cho đúng với Input
            results.Reverse();
            return results;
        }

        /// <summary>
        /// Tìm đường đi tới tất cả các nút đích tương ứng, trong đó chọn ra 1 nút có quãng đường đến ngắn nhất sử dụng thuật toán Dijkstra truyền thống - O(N*M)
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNodes"></param>
        /// <returns></returns>
        public List<int> FindPathUsingOriginDijkstra(int fromNode, List<int> toNodes)
        {
            /// Kết quả trả về
            List<int> results = new List<int>();

            /// Mảng đánh dấu danh sách các điểm đã đi qua
            HashSet<int> passedNodes = new HashSet<int>();

            /// Ma trận đường đi
            Dictionary<int, int> distances = new Dictionary<int, int>();
            /// Danh sách các đỉnh đã đi qua để đến được vị trí tương ứng
            Dictionary<int, int> traces = new Dictionary<int, int>();

            /// Đổ dữ liệu ban đầu
            foreach (int nodeIndex in this.NodesIndex)
            {
                /// Nếu là nút bắt đầu
                if (nodeIndex == fromNode)
                {
                    distances[nodeIndex] = 0;
                }
                /// Nếu không phải nút bắt đầu
                else
                {
                    distances[nodeIndex] = int.MaxValue;
                }

                traces[nodeIndex] = -1;
            }

            /// Lặp liên tục
            while (true)
            {
                /// Tìm nút có trọng số nhỏ nhất
                int minV = int.MaxValue;
                int thisNodeIndex = -1;
                foreach (KeyValuePair<int, int> pair in distances)
                {
                    if (!passedNodes.Contains(pair.Key) && minV > pair.Value)
                    {
                        minV = pair.Value;
                        thisNodeIndex = pair.Key;
                    }
                }

                /// Nếu không có đường đi
                if (thisNodeIndex == -1)
                {
                    //Console.WriteLine("Weight = " + "INF");
                    break;
                }

                /// Đánh dấu đỉnh đã đi qua rồi
                passedNodes.Add(thisNodeIndex);

                /// Nếu tồn tại danh sách đỉnh từ vị trí bắt đầu tương ứng
                if (this.Edges.TryGetValue(thisNodeIndex, out List<Edge> edges))
                {
                    /// Duyệt danh sách cạnh đi từ điểm tương ứng
                    foreach (Edge edge in edges)
                    {
                        /// Nếu khoảng cách từ vị trí xuất phát đến đỉnh đích lớn hơn khoảng cách từ vị trí xuất phát đến đỉnh bắt đầu và đi qua cạnh này đến đỉnh đích
                        /// d[v] > d[u] + c[u,v]
                        if (distances[edge.ToNode] > distances[thisNodeIndex] + edge.Weight)
                        {
                            /// Gắn lại khoảng cách
                            distances[edge.ToNode] = distances[thisNodeIndex] + edge.Weight;
                            /// Cập nhật lại vị trí trước đó
                            traces[edge.ToNode] = edge.FromNode;
                        }
                    }
                }
            }

            /// Chọn một nút có đường đi tới ngắn nhất
            int minNodeIndex = -1;
            int minWeight = int.MaxValue;
            foreach (int nodeID in toNodes)
            {
                /// Nếu tồn tại đường đi tới nút tương ứng và nhỏ hơn đường đi nhỏ nhất tìm được ở nút khác
                if (distances[nodeID] != int.MaxValue && distances[nodeID] < minWeight)
                {
                    minNodeIndex = nodeID;
                    minWeight = distances[nodeID];
                }
            }

            /// Nếu có kết quả
            if (minNodeIndex != -1)
            {
                //Console.WriteLine("Weight = " + minWeight);
                while (minNodeIndex != -1)
                {
                    results.Add(minNodeIndex);
                    minNodeIndex = traces[minNodeIndex];
                }
            }

            /// Đảo ngược danh sách cho đúng với Input
            results.Reverse();
            return results;
        }

        /// <summary>
        /// Tìm đường đi giữa 2 nút bất kỳ sử dụng thuật toán Dijkstra cải tiến với cấu trúc HEAP O((M + N) * Log2(N))
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNode"></param>
        /// <returns></returns>
        public List<int> FindPath(int fromNode, int toNode)
        {
            /// Kết quả trả về
            List<int> results = new List<int>();

            /// Mảng đánh dấu danh sách các điểm đã đi qua
            HashSet<int> passedNodes = new HashSet<int>();

            /// Danh sách các đỉnh đi từ vị trí xuất phát tới vị trí tương ứng
            BinaryTree<int> tree = new BinaryTree<int>();
            /// Danh sách các nút tương ứng trên cây nhị phân theo thứ tự đỉnh ban đầu
            Dictionary<int, BinaryTree<int>.Node> nodesByIndex = new Dictionary<int, BinaryTree<int>.Node>();
            /// Danh sách các đỉnh đã đi qua để đến được vị trí tương ứng
            Dictionary<int, int> traces = new Dictionary<int, int>();

            /// Đổ dữ liệu ban đầu
            foreach (int nodeIndex in this.NodesIndex)
            {
                /// Nếu là nút bắt đầu
                if (nodeIndex == fromNode)
                {
                    BinaryTree<int>.Node node = new BinaryTree<int>.Node()
                    {
                        Data = nodeIndex,
                        Weight = 0,
                    };
                    tree.Append(node);
                    nodesByIndex[nodeIndex] = node;
                }
                /// Nếu không phải nút bắt đầu
                else
                {
                    BinaryTree<int>.Node node = new BinaryTree<int>.Node()
                    {
                        Data = nodeIndex,
                        Weight = int.MaxValue,
                    };
                    tree.Append(node);
                    nodesByIndex[nodeIndex] = node;
                }

                traces[nodeIndex] = -1;
            }

            /// Lặp liên tục
            while (true)
            {
                /// Nút gốc của cây là nút có trọng số nhỏ nhất
                BinaryTree<int>.Node firstNode = tree.Pop();

                /// Nếu không có đường đi
                if (firstNode == null || firstNode.Weight == int.MaxValue)
                {
                    //Console.WriteLine("Weight = " + "INF");
                    break;
                }

                /// Tên đỉnh tương ứng
                int thisNodeIndex = firstNode.Data;

                /// Nếu đã đến đích
                if (thisNodeIndex == toNode)
                {
                    //Console.WriteLine("Weight = " + firstNode.Weight);
                    while (thisNodeIndex != -1)
                    {
                        results.Add(thisNodeIndex);
                        thisNodeIndex = traces[thisNodeIndex];
                    }
                    break;
                }

                /// Đánh dấu đỉnh đã đi qua rồi
                passedNodes.Add(thisNodeIndex);

                /// Nếu tồn tại danh sách đỉnh từ vị trí bắt đầu tương ứng
                if (this.Edges.TryGetValue(firstNode.Data, out List<Edge> edges))
                {
                    /// Duyệt danh sách cạnh đi từ điểm tương ứng
                    foreach (Edge edge in edges)
                    {
                        /// Nếu khoảng cách từ vị trí xuất phát đến đỉnh đích lớn hơn khoảng cách từ vị trí xuất phát đến đỉnh bắt đầu và đi qua cạnh này đến đỉnh đích
                        /// d[v] > d[u] + c[u,v]
                        if (nodesByIndex[edge.ToNode].Weight > nodesByIndex[thisNodeIndex].Weight + edge.Weight)
                        {
                            /// Gắn lại khoảng cách
                            nodesByIndex[edge.ToNode].Weight = nodesByIndex[thisNodeIndex].Weight + edge.Weight;
                            /// Cập nhật lại vị trí trước đó
                            traces[edge.ToNode] = edge.FromNode;
                            /// Xây lại cây nhị phân
                            tree.Update(nodesByIndex[edge.ToNode]);
                        }
                    }
                }
            }

            /// Đảo ngược danh sách cho đúng với Input
            results.Reverse();
            return results;
        }

        /// <summary>
        /// Tìm đường đi tới tất cả các nút đích tương ứng, trong đó chọn ra 1 nút có quãng đường đến ngắn nhất sử dụng thuật toán Dijkstra cải tiến với cấu trúc HEAP O((M + N) * Log2(N))
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNodes"></param>
        /// <returns></returns>
        public List<int> FindPath(int fromNode, List<int> toNodes)
        {
            /// Kết quả trả về
            List<int> results = new List<int>();

            /// Mảng đánh dấu danh sách các điểm đã đi qua
            HashSet<int> passedNodes = new HashSet<int>();

            /// Danh sách các đỉnh đi từ vị trí xuất phát tới vị trí tương ứng
            BinaryTree<int> tree = new BinaryTree<int>();
            /// Danh sách các nút tương ứng trên cây nhị phân theo thứ tự đỉnh ban đầu
            Dictionary<int, BinaryTree<int>.Node> nodesByIndex = new Dictionary<int, BinaryTree<int>.Node>();
            /// Danh sách các đỉnh đã đi qua để đến được vị trí tương ứng
            Dictionary<int, int> traces = new Dictionary<int, int>();

            /// Đổ dữ liệu ban đầu
            foreach (int nodeIndex in this.NodesIndex)
            {
                /// Nếu là nút bắt đầu
                if (nodeIndex == fromNode)
                {
                    BinaryTree<int>.Node node = new BinaryTree<int>.Node()
                    {
                        Data = nodeIndex,
                        Weight = 0,
                    };
                    tree.Append(node);
                    nodesByIndex[nodeIndex] = node;
                }
                /// Nếu không phải nút bắt đầu
                else
                {
                    BinaryTree<int>.Node node = new BinaryTree<int>.Node()
                    {
                        Data = nodeIndex,
                        Weight = int.MaxValue,
                    };
                    tree.Append(node);
                    nodesByIndex[nodeIndex] = node;
                }

                traces[nodeIndex] = -1;
            }

            /// Lặp liên tục
            while (true)
            {
                /// Nút gốc của cây là nút có trọng số nhỏ nhất
                BinaryTree<int>.Node firstNode = tree.Pop();

                /// Nếu không có đường đi
                if (firstNode == null || firstNode.Weight == int.MaxValue)
                {
                    //Console.WriteLine("Weight = " + "INF");
                    break;
                }

                /// Tên đỉnh tương ứng
                int thisNodeIndex = firstNode.Data;

                /// Đánh dấu đỉnh đã đi qua rồi
                passedNodes.Add(thisNodeIndex);

                /// Nếu tồn tại danh sách đỉnh từ vị trí bắt đầu tương ứng
                if (this.Edges.TryGetValue(firstNode.Data, out List<Edge> edges))
                {
                    /// Duyệt danh sách cạnh đi từ điểm tương ứng
                    foreach (Edge edge in edges)
                    {
                        /// Nếu khoảng cách từ vị trí xuất phát đến đỉnh đích lớn hơn khoảng cách từ vị trí xuất phát đến đỉnh bắt đầu và đi qua cạnh này đến đỉnh đích
                        /// d[v] > d[u] + c[u,v]
                        if (nodesByIndex[edge.ToNode].Weight > nodesByIndex[thisNodeIndex].Weight + edge.Weight)
                        {
                            /// Gắn lại khoảng cách
                            nodesByIndex[edge.ToNode].Weight = nodesByIndex[thisNodeIndex].Weight + edge.Weight;
                            /// Cập nhật lại vị trí trước đó
                            traces[edge.ToNode] = edge.FromNode;
                            /// Xây lại cây nhị phân
                            tree.Update(nodesByIndex[edge.ToNode]);
                        }
                    }
                }
            }

            /// Chọn một nút có đường đi tới ngắn nhất
            int minNodeIndex = -1;
            int minWeight = int.MaxValue;
            foreach (int nodeID in toNodes)
            {
                /// Nếu tồn tại đường đi tới nút tương ứng và nhỏ hơn đường đi nhỏ nhất tìm được ở nút khác
                if (nodesByIndex[nodeID].Weight != int.MaxValue && nodesByIndex[nodeID].Weight < minWeight)
                {
                    minNodeIndex = nodeID;
                    minWeight = nodesByIndex[nodeID].Weight;
                }
            }

            /// Nếu có kết quả
            if (minNodeIndex != -1)
            {
                //Console.WriteLine("Weight = " + minWeight);
                while (minNodeIndex != -1)
                {
                    results.Add(minNodeIndex);
                    minNodeIndex = traces[minNodeIndex];
                }
            }

            /// Đảo ngược danh sách cho đúng với Input
            results.Reverse();
            return results;
        }
        #endregion
    }
}
