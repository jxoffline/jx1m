using System;
using System.Collections.Generic;

namespace FS.VLTK.Utilities.Algorithms
{
    /// <summary>
    /// Hàng đợi 2 chiều
    /// </summary>
    public class DoubleEndedQueue<T>
    {
        /// <summary>
        /// Định nghĩa nút trong hàng đợi
        /// </summary>
        private class Node
        {
            /// <summary>
            /// Giá trị của nút
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// Nút liền trước
            /// </summary>
            public Node Previous { get; set; }

            /// <summary>
            /// Nút liền sau
            /// </summary>
            public Node NextNode { get; set; }
        }

        /// <summary>
        /// Nút đầu tiên
        /// </summary>
        private Node firstNode = null;

        /// <summary>
        /// Nút cuối cùng
        /// </summary>
        private Node lastNode = null;

        #region Properties
        /// <summary>
        /// Hàng đợi có rỗng không
        /// </summary>
        private bool IsEmpty
        {
            get
            {
                return this.firstNode == null || this.lastNode == null;
            }
        }

        private int _Count = 0;
        /// <summary>
        /// Số phần tử của hàng đợi
        /// </summary>
        public int Count
        {
            get
            {
                return this._Count;
            }
        }
        #endregion

        #region Insert
        /// <summary>
        /// Thêm nút vào đầu hàng đợi
        /// </summary>
        /// <param name="element"></param>
        public void EnqueueFront(T element)
        {
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                /// Tạo nút ở đầu
                this.firstNode = new Node()
                {
                    Previous = null,
                    NextNode = null,
                    Value = element,
                };
                /// Tạo luôn nút ở cuối
                this.lastNode = this.firstNode;
            }
            /// Nếu có nút ở đầu
            else
            {
                /// Lấy nút hiện tại ở đầu
                Node currentFirstNode = this.firstNode;
                /// Tạo nút ở đầu mới
                this.firstNode = new Node()
                {
                    Previous = null,
                    NextNode = currentFirstNode,
                    Value = element,
                };
                /// Đánh dấu nút cũ liền trước nút vừa tạo ra
                currentFirstNode.Previous = this.firstNode;
            }
            /// Tăng kích thước lên 1 đơn vị
            this._Count++;
        }

        /// <summary>
        /// Thêm nút vào cuối hàng đợi
        /// </summary>
        /// <param name="element"></param>
        public void EnqueueRear(T element)
        {
            /// Nếu hàng đợi rỗng
            if (this.lastNode == null)
            {
                /// Tạo nút ở đầu
                this.firstNode = new Node()
                {
                    Previous = null,
                    NextNode = null,
                    Value = element,
                };
                /// Tạo luôn nút ở cuối
                this.lastNode = this.firstNode;
            }
            /// Nếu có nút ở cuối
            else
            {
                /// Lấy nút hiện tại ở cuối
                Node currentLastNode = this.lastNode;
                /// Tạo nút ở cuối mới
                this.lastNode = new Node()
                {
                    NextNode = null,
                    Previous = currentLastNode,
                    Value = element,
                };
                /// Đánh dấu nút liền sau là nút vừa tạo ra
                currentLastNode.NextNode = this.lastNode;
            }
            /// Tăng kích thước lên 1 đơn vị
            this._Count++;
        }
        #endregion

        #region Get
        /// <summary>
        /// Lấy phần tử ở đầu hàng đợi nhưng không xóa
        /// </summary>
        /// <returns></returns>
        public T PeakFront()
        {
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return default;
            }

            /// Trả về giá trị nút ở đầu hàng đợi
            return this.firstNode.Value;
        }

        /// <summary>
        /// Lấy phần tử ở cuối hàng đợi nhưng không xóa
        /// </summary>
        /// <returns></returns>
        public T PeakRear()
        {
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return default;
            }

            /// Trả về giá trị nút ở cuối hàng đợi
            return this.lastNode.Value;
        }
        #endregion

        #region Get and Remove
        /// <summary>
        /// Lấy và xóa phần tử ở đầu hàng đợi
        /// </summary>
        /// <returns></returns>
        public T DequeueFront()
        {
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return default;
            }

            /// Lấy nút ở đầu hàng đợi
            Node node = this.firstNode;
            /// Lấy nút tiếp theo
            Node nextNode = this.firstNode.NextNode;
            /// Nếu không có nút tiếp theo tức hàng đợi đã rỗng
            if (nextNode == null)
            {
                this.firstNode = null;
                this.lastNode = null;
            }
            /// Nếu có nút tiếp theo thì gắn lên đầu
            else
            {
                nextNode.Previous = null;
                this.firstNode = nextNode;
            }
            /// Giảm kích thước hàng đợi đi 1 đơn vị
            this._Count--;

            /// Trả về kết quả
            return node.Value;
        }

        /// <summary>
        /// Lấy và xóa phần tử ở cuối hàng đợi
        /// </summary>
        /// <returns></returns>
        public T DequeueRear()
        {
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return default;
            }

            /// Lấy nút ở cuối hàng đợi
            Node node = this.lastNode;
            /// Lấy nút liền trước
            Node previousNode = this.lastNode.Previous;
            /// Nếu không có nút liền trước tức hàng đợi đã rỗng
            if (previousNode == null)
            {
                this.firstNode = null;
                this.lastNode = null;
            }
            /// Nếu có nút liền trước thì cho xuống cuối
            else
            {
                previousNode.NextNode = null;
                this.lastNode = previousNode;
            }
            /// Giảm kích thước hàng đợi đi 1 đơn vị
            this._Count--;

            /// Trả về kết quả
            return node.Value;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        public void Clear()
        {
            this.firstNode = null;
            this.lastNode = null;
            this._Count = 0;
        }
        #endregion

        #region Linq-Extension
        /// <summary>
        /// Chuyển toàn bộ phần tử trong hàng đợi về dạng List
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            /// Tạo danh sách kết quả
            List<T> list = new List<T>();
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return list;
            }

            /// Duyệt từ nút ở đầu đến khi nào không còn có nút tiếp theo
            Node node = this.firstNode;
            list.Add(node.Value);
            while (node.NextNode != null)
            {
                node = node.NextNode;
                list.Add(node.Value);
            }

            /// Trả về kết quả
            return list;
        }

        /// <summary>
        /// Chuyển toàn bộ phần tử trong hàng đợi về dạng List đảo ngược thứ tự
        /// </summary>
        /// <returns></returns>
        public List<T> ToReverseList()
        {
            /// Tạo danh sách kết quả
            List<T> list = new List<T>();
            /// Nếu hàng đợi rỗng
            if (this.IsEmpty)
            {
                return list;
            }

            /// Duyệt từ nút ở cuối đến khi nào không còn nút liền trước
            Node node = this.lastNode;
            list.Add(node.Value);
            while (node.Previous != null)
            {
                node = node.Previous;
                list.Add(node.Value);
            }

            /// Trả về kết quả
            return list;
        }
        #endregion
    }
}
