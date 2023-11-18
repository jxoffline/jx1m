using System;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Biểu diễn Timer
    /// </summary>
    public class KTTimer
    {
        /// <summary>
        /// Tên luồng
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng được kích hoạt
        /// </summary>
        public virtual Action Start { get; set; }

        /// <summary>
        /// Sự kiện kích hoạt liên tục mỗi khoảng thời gian đến khi đối tượng bị hủy
        /// </summary>
        public virtual Action Tick { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng kết thúc thời gian
        /// </summary>
        public virtual Action Finish { get; set; }

        /// <summary>
        /// Sự kiện khi đối tượng bị hủy
        /// </summary>
        public virtual Action Destroy { get; set; }

        /// <summary>
        /// Sự kiện khi có lỗi xảy ra
        /// </summary>
        public virtual Action<Exception> Error { get; set; }

        /// <summary>
        /// Vẫn đang thực thi
        /// </summary>
        public bool Alive { get; set; } = true;

        /// <summary>
        /// Thời gian tồn tại (giây) (-1 nếu vĩnh viễn)
        /// </summary>
        public float Interval { get; set; }

        /// <summary>
        /// Khoảng thời gian kích hoạt liên tục (giây) sự kiện Tick, -1 sẽ vô hiệu
        /// </summary>
        public float PeriodActivation { get; set; } = -1;
    }
}