using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng vật phẩm rơi
    /// </summary>
    public partial class KTStallBotTimerManager
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
        private class StallBotTimer
        {
            /// <summary>
            /// Đối tượng tương ứng
            /// </summary>
            public KStallBot RefObject { get; set; }

            /// <summary>
            /// Thời điểm tạo ra
            /// </summary>
            public long InitTicks { get; set; }

            /// <summary>
            /// Thời gian tồn tại
            /// </summary>
            public long LifeTime { get; set; }

            /// <summary>
            /// Đã hết thời gian tồn tại chưa
            /// </summary>
            public bool IsOver
            {
                get
                {
                    /// Nếu tồn tại vĩnh viễn
                    if (this.LifeTime == -1)
                    {
                        /// Bỏ qua
                        return false;
                    }

                    /// Trả về thời gian tồn tại còn lại
                    return KTGlobal.GetCurrentTimeMilis() - this.InitTicks >= this.LifeTime;
                }
            }
        }
        #endregion
    }
}
