using GameServer.KiemThe.Core;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTDynamicAreaTimerManager
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
        private class DynamicAreaTimer
        {
            /// <summary>
            /// Đối tượng tương ứng
            /// </summary>
            public KDynamicArea RefObject { get; set; }

            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public Action Tick { get; set; }

            /// <summary>
            /// Sự kiện hết thời gian
            /// </summary>
            public Action Timeout { get; set; }

            /// <summary>
            /// Thời điểm lần trước thực hiện hàm Tick
            /// </summary>
            public long LastTick { get; set; }

            /// <summary>
            /// Tick
            /// </summary>
            public int TickTime { get; set; }

            /// <summary>
            /// Thời điểm tạo ra
            /// </summary>
            public long InitTicks { get; set; }

            /// <summary>
            /// Thời gian tồn tại
            /// </summary>
            public long LifeTime { get; set; }

            /// <summary>
            /// Đã hoàn thành Tick lần trước chưa
            /// </summary>
            public bool HasCompletedLastTick { get; set; }

            /// <summary>
            /// Đã đến thời điểm Tick chưa
            /// </summary>
            public bool IsTickTime
            {
                get
                {
                    return KTGlobal.GetCurrentTimeMilis() - this.LastTick >= TickTime;
                }
            }

            /// <summary>
            /// Đã hết thời gian chưa
            /// </summary>
            public bool IsOver
            {
                get
                {
                    /// Nếu tồn tại vĩnh viễn
                    if (this.LifeTime == -1)
                    {
                        return false;
                    }
                    /// Trả về kết quả
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.LifeTime;
                }
            }
        }
        #endregion
    }
}
