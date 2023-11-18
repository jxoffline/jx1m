using GameServer.KiemThe.CopySceneEvents;
using GameServer.Logic;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic cơ bản của bot biểu diễn
    /// </summary>
    public partial class KTDecoBotTimerManager
    {
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa đối tượng này ra khỏi Timer không
        /// <para>- Nếu chết</para>
        /// <para>- Nếu không có người chơi xung quanh</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool NeedToBeRemoved(KDecoBot obj)
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

            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(obj.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (obj.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(obj.CurrentCopyMapID, obj.CurrentMapCode))
            {
                return true;
            }

            /// Nếu không có người chơi xung quanh
            if (obj.VisibleClientsNum <= 0)
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessStart(KDecoBot obj)
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
        private void ProcessTick(KDecoBot obj)
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
    }
}
