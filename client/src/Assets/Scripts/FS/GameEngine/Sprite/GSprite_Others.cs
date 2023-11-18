using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý đối tượng khác
    /// </summary>
    public partial class GSprite
    {
        /// <summary>
        /// Thông tin hành động của đối tượng
        /// </summary>
        private class OthersAction
        {
            /// <summary>
            /// Sự kiện thực thi hành động
            /// </summary>
            public Action Process { get; set; }

            /// <summary>
            /// Kiểm tra hành động đã được thực thi xong chưa
            /// </summary>
            public Func<bool> IsDone { get; set; }

            /// <summary>
            /// Sự kiện khi hoàn tất
            /// </summary>
            public Action Finish { get; set; }
        }

        /// <summary>
        /// Danh sách hàng đợi các hành động chờ thực thi theo chuỗi của đối tượng theo lệnh của Server
        /// </summary>
        private readonly Queue<OthersAction> othersAction = new Queue<OthersAction>();

        /// <summary>
        /// Hành động hiện tại
        /// </summary>
        private OthersAction currentOthersAction = null;

        /// <summary>
        /// Thêm hành động từ Server gửi về vào hàng đợi
        /// </summary>
        /// <param name="process"></param>
        /// <param name="predicateIsDone"></param>
        /// <param name="finish"></param>
        public void QueueOthersAction(Action process, Func<bool> predicateIsDone, Action finish = null)
        {
            this.othersAction.Enqueue(new OthersAction()
            {
                Process = process,
                IsDone = predicateIsDone,
                Finish = finish,
            });
        }

        /// <summary>
        /// Thực thi hành động của đối tượng khác
        /// </summary>
        private void TickOthersAction()
        {
            /// Nếu động tác trước đó chưa hoàn thành
            if (this.currentOthersAction != null && !this.currentOthersAction.IsDone())
            {
                return;
            }
            /// Nếu hàng đợi rỗng
            else if (this.othersAction.Count <= 0)
            {
                return;
            }

            /// Thực thi sự kiện Finish hành động trước đó
            if (this.currentOthersAction != null)
            {
                this.currentOthersAction.Finish?.Invoke();
            }

            /// Lấy hành động tiếp theo ra khỏi hàng đợi
            this.currentOthersAction = this.othersAction.Dequeue();
            /// Thực thi hành động này
            if (this.currentOthersAction != null)
            {
                this.currentOthersAction.Process?.Invoke();
            }
        }
    }
}
