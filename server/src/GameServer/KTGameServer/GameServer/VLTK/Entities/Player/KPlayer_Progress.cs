using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities.Player
{
    /// <summary>
    /// Trạng thái Progress đang làm gì đó của người chơi
    /// </summary>
    public class KPlayer_Progress
    {
        /// <summary>
        /// Thời điểm bắt đầu
        /// </summary>
        public long StartTick { get; set; }

        /// <summary>
        /// Thời gian duy trì
        /// </summary>
        public long DurationTick { get; set; }

        /// <summary>
        /// Sự kiện thực thi liên tục mỗi nửa giây
        /// </summary>
        public Action Tick { get; set; }

        /// <summary>
        /// Sự kiện thực thi khi hoàn tất
        /// </summary>
        public Action Complete { get; set; }

        /// <summary>
        /// Sự kiện thực thi khi hủy bỏ
        /// </summary>
        public Action Cancel { get; set; }

        /// <summary>
        /// Bị hủy bỏ nếu nhận sát thương
        /// </summary>
        public bool InteruptIfTakeDamage { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// Đã hoàn tất chưa
        /// </summary>
        public bool Completed
        {
            get
            {
                return KTGlobal.GetCurrentTimeMilis() - this.StartTick >= this.DurationTick;
            }
        }
    }
}
