using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core;
using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý khu vực động
    /// </summary>
    public partial class KTDynamicAreaTimerManager
    {
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa khu vực động này ra khỏi Timer không
        /// <param name="dynArea"></param>
        /// </summary>
        /// <returns></returns>
        private bool NeedToBeRemoved(KDynamicArea dynArea)
        {
            /// Nếu không có tham chiếu
            if (dynArea == null)
            {
                return true;
            }

            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(dynArea.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (dynArea.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(dynArea.CurrentCopyMapID, dynArea.CurrentMapCode))
            {
                return true;
            }

            /// Nếu không có người chơi xung quanh
            if (dynArea.VisibleClientsNum <= 0)
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }
    }
}
