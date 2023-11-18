using System.Collections.Concurrent;

namespace GameServer.KiemThe.Utilities.Algorithms
{
    /// <summary>
    /// Quản lý ID tự tăng có thể tái sử dụng được về sau
    /// </summary>
    public class AutoIndexReusablePattern
    {
        /// <summary>
        /// Danh sách ID chưa được sử dụng
        /// </summary>
        private readonly ConcurrentQueue<int> freeIDs = new ConcurrentQueue<int>();

        /// <summary>
        /// Quản lý ID tự tăng có thể tái sử dụng được về sau
        /// </summary>
        /// <param name="capacity"></param>
        public AutoIndexReusablePattern(int capacity)
        {
            /// Sinh ra sẵn
            for (int i = 0; i < capacity; i++)
            {
                /// Thêm vào hàng đợi
                this.freeIDs.Enqueue(i);
            }
        }

        /// <summary>
        /// Trả về ID tự động
        /// </summary>
        /// <returns></returns>
        public int Take()
        {
            /// Nếu hàng đợi rỗng
            if (this.freeIDs.IsEmpty)
            {
                /// Toác
                return 0;
            }

            /// Lấy ra khỏi hàng đợi
            this.freeIDs.TryDequeue(out int id);
            /// Trả về kết quả
            return id;
        }

        /// <summary>
        /// Trả ID về danh sách để tái sử dụng lần sau
        /// </summary>
        /// <param name="id"></param>
        public void Return(int id)
        {
            /// Thêm vào hàng đợi
            this.freeIDs.Enqueue(id);
        }
    }
}
