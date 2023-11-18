using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Utilities.Algorithms
{
    /// <summary>
    /// Cấu trúc cây nhị phân
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTree<T>
    {
        /// <summary>
        /// Chế độ sắp xếp các nút trên cây
        /// </summary>
        public enum SortModeIndexes
        {
            /// <summary>
            /// Tăng dần
            /// </summary>
            Ascending,
            /// <summary>
            /// Giảm dần
            /// </summary>
            Descending,
        }

        /// <summary>
        /// Nút trên cây
        /// </summary>
        public class Node
        {
            /// <summary>
            /// Đối tượng tương ứng
            /// </summary>
            public T Data { get; set; }

            /// <summary>
            /// Trọng số
            /// </summary>
            public int Weight { get; set; }

            /// <summary>
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("Data = {0}, Weight = {1}", this.Data.ToString(), this.Weight);
            }
        }

        /// <summary>
        /// Nút gốc của cây
        /// </summary>
        private Node Root
        {
            get
            {
                if (this.IsEmpty())
                {
                    return null;
                }
                return this.Nodes[0];
            }
        }

        /// <summary>
        /// Tổng số nút trên cây
        /// </summary>
        public int Count
        {
            get
            {
                if (this.IsEmpty())
                {
                    return 0;
                }
                return this.Nodes.Count;
            }
        }

        /// <summary>
        /// Danh sách các nút của cây
        /// </summary>
        private readonly List<Node> Nodes = new List<Node>();

        /// <summary>
        /// Danh sách thứ tự nút trên cây
        /// </summary>
        private readonly Dictionary<Node, int> NodeIndexes = new Dictionary<Node, int>();

        /// <summary>
        /// Chế độ sắp xếp các nút trên cây
        /// </summary>
        public SortModeIndexes SortMode { get; private set; }

        #region Constructor
        /// <summary>
        /// Tạo mới cây nhị phân rỗng
        /// </summary>
        public BinaryTree(SortModeIndexes sortMode = SortModeIndexes.Ascending)
        {
            this.SortMode = sortMode;
        }

        /// <summary>
        /// Tạo mới cây nhị phân với danh sách nút tương ứng
        /// </summary>
        /// <param name="nodes"></param>
        public BinaryTree(List<Node> nodes, SortModeIndexes sortMode = SortModeIndexes.Ascending)
        {
            this.SortMode = sortMode;
            this.Build(nodes);
        }
        #endregion

        #region Core
        /// <summary>
        /// Kiểm tra trong 2 nút của cây nút nào có trọng số thỏa mãn điều kiện nằm gần gốc của cây nhất
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private bool IsLower(Node node1, Node node2)
        {
            switch (this.SortMode)
            {
                case SortModeIndexes.Ascending:
                {
                    return node1.Weight <= node2.Weight;
                }
                case SortModeIndexes.Descending:
                {
                    return node1.Weight >= node2.Weight;
                }
                default:
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Trả về vị trí của nút tương ứng trong danh sách
        /// </summary>
        /// <param name="node"></param>
        private int IndexOf(Node node)
        {
            if (this.NodeIndexes.TryGetValue(node, out int index))
            {
                return index;
            }
            return -1;
        }

        /// <summary>
        /// Vun đống từ gốc lên
        /// </summary>
        private void UpHeap()
        {
            /// Nếu cây rỗng
            if (this.IsEmpty())
            {
                return;
            }

            /// Vị trí nút gốc
            int pos = 1;
            /// Lấy nút gốc
            Node node = this.Nodes[pos - 1];
            /// Lặp liên tục chừng nào chưa duyệt hết đến ngọn cây
            while (pos * 2 <= this.Nodes.Count)
            {
                /// Nút cần đổi chỗ
                Node swapNode = node;
                /// Vị trí cần đổi chỗ
                int swapPos = pos;

                /// Vị trí nút con trái
                int leftChildNodePos = pos * 2;
                /// Nút con trái
                Node leftNode = this.Nodes[leftChildNodePos - 1];

                /// Nếu nút cha cần đổi chỗ với nút con trái
                if (this.IsLower(leftNode, swapNode))
                {
                    swapNode = leftNode;
                    swapPos = leftChildNodePos;
                }

                /// Nếu tồn tại nút con phải
                if (pos * 2 + 1 <= this.Nodes.Count)
                {
                    /// Vị trí nút con phải
                    int rightChildNodePos = pos * 2 + 1;
                    /// Nút con phải
                    Node rightNode = this.Nodes[rightChildNodePos - 1];

                    /// Nếu nút cha cần đổi chỗ với nút con phải
                    if (this.IsLower(rightNode, swapNode))
                    {
                        swapNode = rightNode;
                        swapPos = rightChildNodePos;
                    }
                }

                /// Nếu tồn tại nút cần đổi chỗ
                if (swapNode != node)
                {
                    /// Đổi chỗ 2 nút
                    this.Nodes[swapPos - 1] = node;
                    this.NodeIndexes[node] = swapPos - 1;
                    this.Nodes[pos - 1] = swapNode;
                    this.NodeIndexes[swapNode] = pos - 1;
                    /// Cập nhật vị trí nút mới
                    pos = swapPos;
                    node = this.Nodes[pos - 1];
                }
                /// Nếu không phải thì thoát
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Vun đống từ ngọn xuống
        /// </summary>
        private void DownHeap()
        {
            /// Nếu cây rỗng
            if (this.IsEmpty())
            {
                return;
            }

            /// Vị trí nút ở ngọn
            int pos = this.Nodes.Count;
            /// Lấy nút ở ngọn
            Node node = this.Nodes[pos - 1];

            /// Lặp liên tục chừng nào chưa xuống tới gốc
            while (pos / 2 > 0)
            {
                /// Vị trí nút cha
                int parentNodePos = pos / 2;
                /// Nút cha
                Node parentNode = this.Nodes[parentNodePos - 1];

                /// Nếu nút cha cần đổi chỗ với nút hiện tại
                if (this.IsLower(node, parentNode))
                {
                    /// Đổi chỗ 2 nút
                    this.Nodes[parentNodePos - 1] = node;
                    this.NodeIndexes[node] = parentNodePos - 1;
                    this.Nodes[pos - 1] = parentNode;
                    this.NodeIndexes[parentNode] = pos - 1;
                    /// Cập nhật vị trí nút mới
                    pos = parentNodePos;
                    node = this.Nodes[pos - 1];
                }
                /// Nếu không phải thì thoát
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Xây cây nhị phân
        /// </summary>
        /// <param name="nodes"></param>
        private void Build(List<Node> nodes = null)
        {
            /// Nếu danh sách nút rỗng
            if (nodes == null)
            {
                return;
            }
            /// Duyệt danh sách các nút đã thiết lập sẵn
            foreach (Node node in nodes)
            {
                this.Append(node);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra cây có rỗng không
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return this.Nodes == null || this.Nodes.Count <= 0;
        }

        /// <summary>
        /// Thêm nút vào cây
        /// </summary>
        /// <param name="node"></param>
        public void Append(Node node)
        {
            /// Thêm nút hiện tại vào đuôi danh sách
            this.Nodes.Add(node);
            this.NodeIndexes[node] = this.Nodes.Count - 1;

            /// Nếu cây chỉ có dưới 1 phần tử
            if (this.Nodes.Count <= 1)
            {
                return;
            }

            /// Vun đống
            this.DownHeap();
        }

        /// <summary>
        /// Cập nhật giá trị cho nút tương ứng ở cây
        /// </summary>
        /// <param name="node"></param>
        public void Update(Node node)
        {
            /// Vị trí trên cây
            int pos = this.IndexOf(node) + 1;
            /// Nếu không tìm thấy
            if (pos == -1)
            {
                return;
            }

            /// Vị trí nút cha
            int parentNodePos = pos / 2;
            while (parentNodePos > 0 && this.IsLower(this.Nodes[pos - 1], this.Nodes[parentNodePos - 1]))
            {
                Node parentNode = this.Nodes[parentNodePos - 1];
                /// Đổi chỗ 2 nút
                this.Nodes[parentNodePos - 1] = node;
                this.NodeIndexes[node] = parentNodePos - 1;
                this.Nodes[pos - 1] = parentNode;
                this.NodeIndexes[parentNode] = pos - 1;

                pos = parentNodePos;
                node = this.Nodes[pos - 1];
                parentNodePos /= 2;
            }
        }

        /// <summary>
        /// Trả về nút ở vị trí đánh số tương ứng trên cây
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Node NodeAt(int idx)
        {
            /// Nếu nút không tồn tại
            if (this.Nodes.Count <= idx || idx < 0)
            {
                return null;
            }
            return this.Nodes[idx];
        }

        /// <summary>
        /// Lấy nút gốc từ cây nhưng không xóa
        /// </summary>
        /// <param name="node"></param>
        public Node Peak()
        {
            return this.Root;
        }

        /// <summary>
        /// Lấy nút gốc và xóa khỏi cây
        /// </summary>
        /// <returns></returns>
        public Node Pop()
        {
            /// Nếu cây rỗng
            if (this.IsEmpty())
            {
                return null;
            }

            /// Nút ở gốc
            Node result = this.Root;

            /// Nếu cây có trên 1 phần tử
            if (!this.IsEmpty() && this.Nodes.Count > 1)
            {
                /// Lấy nút ở cuối cây
                Node lastNode = this.Nodes[this.Nodes.Count - 1];
                /// Đưa nút ở cuối cây lên gốc
                this.Nodes[0] = lastNode;
                /// Cập nhật vị trí của nút vừa đặt lên gốc
                this.NodeIndexes[lastNode] = 0;
            }

            /// Xóa vị trí tương ứng của phần tử vừa lấy ở gốc
            this.NodeIndexes.Remove(result);
            /// Xóa phần tử ở đuôi của cây
            this.Nodes.RemoveAt(this.Nodes.Count - 1);
            /// Thực hiện vun đống
            this.UpHeap();

            return result;
        }
        #endregion
    }
}
