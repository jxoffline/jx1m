using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTPlayerTimerManager
    {
        /// <summary>
        /// Thông tin yêu cầu
        /// </summary>
        private class QueueItem
        {
            /// <summary>
            /// Loại yêu cầu
            /// <para>1: Thêm, 0: Xóa</para>
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// Người chơi tương ứng
            /// </summary>
            public KPlayer Player { get; set; }
        }
    }
}
