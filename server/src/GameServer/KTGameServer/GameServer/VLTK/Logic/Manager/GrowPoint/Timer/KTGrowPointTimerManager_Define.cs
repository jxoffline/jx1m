using GameServer.KiemThe.Core;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTGrowPointTimerManager
    {
        #region Define
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
            /// Dữ liệu
            /// </summary>
            public object Data { get; set; }
        }

        /// <summary>
        /// Luồng quản lý
        /// </summary>
        private class GrowPointTimer
        {
            /// <summary>
            /// Đối tượng tương ứng
            /// </summary>
            public GrowPoint RefObject { get; set; }

            /// <summary>
            /// Thời gian Timer được tạo ra
            /// </summary>
            public long InitTicks { get; set; }

            /// <summary>
            /// Thời gian tồn tại
            /// </summary>
            public long LifeTime { get; set; }

            /// <summary>
            /// Sự kiện khi hết thời gian
            /// </summary>
            public Action Timeout { get; set; }

            /// <summary>
            /// Đã hết thời gian chưa
            /// </summary>
            public bool IsOver
            {
                get
                {
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.LifeTime;
                }
            }
        }
        #endregion
    }
}
