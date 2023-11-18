using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic cơ bản của xe tiêu
    /// </summary>
    public partial class KTTraderCarriageTimerManager
    {
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa đối tượng này ra khỏi Timer không
        /// <para>- Nếu chết</para>
        /// <para>- Nếu không có chủ nhân xung quanh</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool NeedToBeRemoved(TraderCarriage obj)
        {
            /// Nếu không có tham chiếu
            if (obj == null)
            {
                return true;
            }
            /// Nếu đối tượng đã chết
            if (obj.IsDead())
            {
                return true;
            }
            /// Nếu không có chủ nhân
            if (obj.Owner == null)
            {
                return true;
            }
            /// Nếu chủ nhân đã chết
            if (obj.Owner.IsDead())
            {
                return true;
            }
            ///// Nếu chủ nhân không Online
            //if (!obj.Owner.IsOnline())
            //{
            //    return true;
            //}

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessStart(TraderCarriage obj)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NeedToBeRemoved(obj))
            {
                this.Remove(obj);
                return;
            }

            /// Thực thi sự kiện
            this.ExecuteAction(obj.Start, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick
        /// </summary>
        /// <param name="monster"></param>
        private void ProcessTick(TraderCarriage obj)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NeedToBeRemoved(obj))
            {
                this.Remove(obj);
                return;
            }

            /// Thực thi sự kiện
            this.ExecuteAction(obj.Tick, null);
        }

        /// <summary>
        /// Thực hiện hàm Timeout
        /// </summary>
        /// <param name="monster"></param>
        private void ProcessTimeout(TraderCarriage obj)
        {
            /// Thực thi sự kiện
            this.ExecuteAction(obj.OnTimeout, null);
        }
    }
}
