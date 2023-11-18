using GameServer.Logic;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTMonsterTimerManager
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
        private class MonsterTimer
        {
            /// <summary>
            /// Chủ nhân
            /// </summary>
            public Monster Owner { get; set; }

            /// <summary>
            /// Thời gian thực hiện random dịch chuyển AI
            /// </summary>
            public float AIRandomMoveTick { get; set; }

            /// <summary>
            /// Đã gọi hàm Start chưa
            /// </summary>
            public bool IsStarted { get; set; }

            /// <summary>
            /// Sự kiện Start
            /// </summary>
            public Action Start { get; set; }

            /// <summary>
            /// Sự kiện Tick
            /// </summary>
            public Action Tick { get; set; }

            /// <summary>
            /// Thời điểm lần trước thực hiện hàm Tick
            /// </summary>
            public long LastTick { get; set; }

            /// <summary>
            /// Tick
            /// </summary>
            public int TickTime { get; set; }

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
        }
        #endregion

        #region Constants
        /// <summary>
        /// Thời gian mỗi lần tự tìm vị trí ngẫu nhiên xung quanh để chạy đến
        /// </summary>
        private const int AIRandomMoveTickMin = 20;

        /// <summary>
        /// Thời gian mỗi lần tự tìm vị trí ngẫu nhiên xung quanh để chạy đến
        /// </summary>
        private const int AIRandomMoveTickMax = 60;
        #endregion
    }
}
