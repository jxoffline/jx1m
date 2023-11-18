using System;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.Utilities.Algorithms
{
    /// <summary>
    /// Tìm đường trong ma trận
    /// <para>Giải thuật A*:</para> 
    /// <para>Gồm 3 giá trị:</para>
    /// <para>G: Cost tính từ vị trí bắt đầu đến ô hiện tại</para>
    /// <para>H: Cost tính từ ô hiện tại đến vị trí đích</para>
    /// <para>F = G + H</para>
    /// <para>A* sẽ được tối ưu theo thuật toán BFS, nếu gắn các node theo độ ưu tiên hàm F nhỏ nhất có thể, sẽ loại bỏ được các trường hợp thừa không cần thiết của BFS, tiết kiệm thời gian</para>
    /// <para>..</para>
    /// <para>Tại mỗi bước thực hiện BFS, tiến hành tính giá trị hàm F và kiểm tra hàm F có tối ưu không, với mỗi hàm F tìm được tại một ô, đưa vào một hàng đợi ưu tiên sao cho nút ở đuôi hàng đợi bao giờ cũng là nút lớn nhất.</para>
    /// </summary>
    public class AStarWithBFSPathFinder : IDisposable
    {
        #region Define
        /// <summary>
        /// Định nghĩa nút trong ma trận
        /// </summary>
        private class Node
        {
            /// <summary>
            /// Nút cha
            /// </summary>
            public Node Parent { get; set; }

            /// <summary>
            /// Tọa độ
            /// </summary>
            public Vector2 Position { get; set; }

            /// <summary>
            /// Giá trị G
            /// </summary>
            public int G { get; set; }

            /// <summary>
            /// Giá trị H
            /// </summary>
            public int H { get; set; }

            /// <summary>
            /// Giá trị hàm F
            /// </summary>
            public int F
            {
                get
                {
                    return this.G + this.H;
                }
            }

            /// <summary>
            /// Trả về khoảng cách giữa 2 nút
            /// </summary>
            /// <param name="nodeA"></param>
            /// <param name="nodeB"></param>
            /// <returns></returns>
            public static int Distance(Node nodeA, Node nodeB)
            {
                int deltaX = Math.Abs((int)nodeA.Position.x - (int)nodeB.Position.x);
                int deltaY = Math.Abs((int)nodeA.Position.y - (int)nodeB.Position.y);
                return Math.Min(deltaX, deltaY) * 10 + Math.Abs(deltaX - deltaY) * 14;
            }

            /// <summary>
            /// Tạo bản sao nút hiện tại
            /// </summary>
            /// <returns></returns>
            public Node Clone()
            {
                return new Node()
                {
                    Parent = this.Parent,
                    Position = this.Position,
                    G = this.G,
                    H = this.H,
                };
            }

            /// <summary>
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("({0}, {1}) - H = {2}, G = {3}, F = {4}", (int)this.Position.x, (int)this.Position.y, this.H, this.G, this.F);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Chiều rộng lưới
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Chiều cao lưới
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Dánh sách vật cản
        /// </summary>
        private List<Vector2> _Obstruction = new List<Vector2>();
        /// <summary>
        /// Danh sách vật cản
        /// </summary>
        public List<Vector2> Obstructions
        {
            get
            {
                return this._Obstruction;
            }
            set
            {
                this._Obstruction = value;
                this.BuildMatrix();
            }
        }

        /// <summary>
        /// Vị trí tiếp theo có thể đi được tính từ nút hiện tại
        /// </summary>
        public List<Vector2> Direction { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Ma trận đường đi
        /// </summary>
        private HashSet<string> matrix;
        #endregion

        #region Private methods
        /// <summary>
        /// Kiểm tra nút có chứa vật cản không
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsNodeHasObstruction(Vector2 node)
        {
            if (!this.IsNodeInsideMatrix(node))
            {
                return true;
            }
            return this.matrix.Contains(string.Format("{0}_{1}", (int)node.x, (int)node.y));
        }

        /// <summary>
        /// Kiểm tra nút có nằm trong ma trận không
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsNodeInsideMatrix(Vector2 node)
        {
            return !(node.x < 0 || node.x >= this.Width || node.y < 0 || node.y >= this.Height);
        }

        /// <summary>
        /// Xây ma trận dựa vào tập danh sách vật cản đã cho
        /// </summary>
        private void BuildMatrix()
        {
            /// Tạo ma trận đường đi
            this.matrix = new HashSet<string>();

            /// Đánh dấu các vị trí chứa vật cản vào ma trận
            foreach (Vector2 node in this._Obstruction)
            {
                if (this.IsNodeInsideMatrix(node))
                {
                    this.matrix.Add(string.Format("{0}_{1}", (int)node.x, (int)node.y));
                }
            }
        }

        /// <summary>
        /// Có thể trực tiếp di chuyển giữa 2 vị trí này không
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNode"></param>
        /// <returns></returns>
        private bool IsCanMove(Vector2 fromNode, Vector2 toNode)
        {
            if (this.matrix.Contains(string.Format("{0}_{1}", (int)toNode.x, (int)toNode.y)))
            {
                return false;
            }

            Vector2 dirNode = toNode - fromNode;

            if (dirNode == new Vector2(-1, -1))
            {
                Vector2 node1 = fromNode + new Vector2(-1, 0);
                Vector2 node2 = fromNode + new Vector2(0, -1);
                return this.IsNodeInsideMatrix(node1) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node1.x, (int)node1.y)) && this.IsNodeInsideMatrix(node2) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node2.x, (int)node2.y));
            }
            else if (dirNode == new Vector2(-1, 1))
            {
                Vector2 node1 = fromNode + new Vector2(-1, 0);
                Vector2 node2 = fromNode + new Vector2(0, 1);
                return this.IsNodeInsideMatrix(node1) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node1.x, (int)node1.y)) && this.IsNodeInsideMatrix(node2) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node2.x, (int)node2.y));
            }
            else if (dirNode == new Vector2(1, 1))
            {
                Vector2 node1 = fromNode + new Vector2(1, 0);
                Vector2 node2 = fromNode + new Vector2(0, 1);
                return this.IsNodeInsideMatrix(node1) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node1.x, (int)node1.y)) && this.IsNodeInsideMatrix(node2) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node2.x, (int)node2.y));
            }
            else if (dirNode == new Vector2(1, -1))
            {
                Vector2 node1 = fromNode + new Vector2(1, 0);
                Vector2 node2 = fromNode + new Vector2(0, -1);
                return this.IsNodeInsideMatrix(node1) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node1.x, (int)node1.y)) && this.IsNodeInsideMatrix(node2) && !this.matrix.Contains(string.Format("{0}_{1}", (int)node2.x, (int)node2.y));
            }
            else
            {
                return this.IsNodeInsideMatrix(toNode);
            }
        }

        /// <summary>
        /// Trả về vị trí gần nhất không có vật cản
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Vector2? FindNearestNoObstructionNode(Vector2 node)
        {
            /// Nếu nút không có vật cản thì chính là giá trị luôn
            if (!this.IsNodeHasObstruction(node))
            {
                return node;
            }

            /// Danh sách các Node đã đi qua
            HashSet<string> markup = new HashSet<string>();

            int[] dx = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };
            int[] dy = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };

            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                Vector2 currentNode = queue.Dequeue();
                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2 newNode = new Vector2(currentNode.x + dx[i], currentNode.y + dy[i]);
                    if (!markup.Contains(string.Format("{0}_{1}", (int)newNode.x, (int)newNode.y)) && !this.IsNodeHasObstruction(newNode))
                    {
                        return newNode;
                    }
                    else
                    {
                        markup.Add(string.Format("{0}_{1}", (int)newNode.x, (int)newNode.y));
                    }
                    queue.Enqueue(newNode);
                }
            }
            return null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tìm đường đi ngắn nhất giữa 2 ô trong ma trận
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public List<Vector2> FindPathBetweenTwoNodes(Vector2 startNode, Vector2 endNode)
        {
            /// Khởi tạo danh sách kết quả
            List<Vector2> output = new List<Vector2>();

            /// Nếu đang đứng ở vị trí chứa vật cản thì không thể đi được
            if (this.IsNodeHasObstruction(startNode))
            {
                return output;
            }

            /// Tìm nút gần nhất không chứa vật cản
            Vector2? nearestNoObsNodeProperty = this.FindNearestNoObstructionNode(endNode);
            if (!nearestNoObsNodeProperty.HasValue)
            {
                return output;
            }
            endNode = nearestNoObsNodeProperty.Value;

            /// Khởi tạo mảng đánh dấu các nút đã đi qua
            HashSet<string> markup = new HashSet<string>();

            /// Heap được xếp theo thứ tự tăng dần của hàm F
            List<Node> heap = new List<Node>();

            /// Thêm nút vào Heap
            void AddNodeToHeap(Node node)
            {
                heap.Add(node);
                int position = heap.Count - 1;
                while (position > 0 && heap[position].F < heap[position / 2].F)
                {
                    Node tmp = heap[position / 2].Clone();
                    heap[position / 2] = heap[position];
                    heap[position] = tmp;
                }
            }

            Node _endNode = new Node()
            {
                Position = endNode,
                Parent = null,
                G = 0,
                H = 0,
            };
            Node _startNode = new Node()
            {
                Parent = null,
                Position = startNode,
                G = 0,
            };
            _startNode.H = Node.Distance(_startNode, _endNode);

            /// Thêm nút bắt đầu vào Heap
            heap.Add(_startNode);

            /// Đánh dấu nút đầu tiên đã đi qua
            markup.Add(string.Format("{0}_{1}", (int)_startNode.Position.x, (int)_startNode.Position.y));

            /// Biến đánh dấu đã tìm thấy kết quả
            bool isFound = false;

            /// Chừng nào Heap còn chưa rỗng
            while (heap.Count > 0)
            {
                Node node = heap[0];

                foreach (Vector2 directionNode in this.Direction)
                {
                    Node targetNode = new Node()
                    {
                        Position = node.Position + directionNode,
                        Parent = node,
                    };
                    targetNode.G = Node.Distance(targetNode, _startNode);
                    targetNode.H = Node.Distance(targetNode, _endNode);

                    /// Nếu ô ở vị trí tiếp theo nằm trong ma trận, chưa được đi qua và không chứa vật cản thì nạp vào Queue
                    if (this.IsNodeInsideMatrix(targetNode.Position) && !markup.Contains(string.Format("{0}_{1}", (int)targetNode.Position.x, (int)targetNode.Position.y)) && this.IsCanMove(node.Position, targetNode.Position))
                    {
                        /// Nạp vào Heap
                        AddNodeToHeap(targetNode);
                        /// Đánh dấu ô này đã đi qua
                        markup.Add(string.Format("{0}_{1}", (int)targetNode.Position.x, (int)targetNode.Position.y));

                        /// Nếu nút hiện tại trùng với nút đích
                        if (targetNode.Position == endNode)
                        {
                            output.Add(targetNode.Position);
                            Node tmpNode = targetNode;
                            while (tmpNode.Position != startNode)
                            {
                                tmpNode = tmpNode.Parent;
                                output.Add(tmpNode.Position);
                            }
                            output.Reverse();

                            /// Đánh dấu đã tìm thấy kết quả
                            isFound = true;
                            break;
                        }
                    }
                }

                /// Nếu đã tìm thấy kết quả
                if (isFound)
                {
                    break;
                }

                /// Xóa nút đã đi qua khỏi danh sách
                heap.RemoveAt(0);
            }

            return output;
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Dispose()
        {
            this.Obstructions.Clear();
            this.Direction.Clear();
            this.matrix = null;
        }
        #endregion
    }
}
