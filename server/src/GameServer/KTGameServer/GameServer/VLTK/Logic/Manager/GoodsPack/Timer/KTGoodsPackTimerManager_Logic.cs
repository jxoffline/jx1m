using GameServer.KiemThe.CopySceneEvents;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng vật phẩm rơi
    /// </summary>
    public partial class KTGoodsPackTimerManager
    {
        /// <summary>
        /// Có cần phải xóa vật phẩm rơi nhay không
        /// </summary>
        /// <param name="goodsPack"></param>
        /// <returns></returns>
        private bool NeedToBeRemoved(KGoodsPack goodsPack)
        {
            /// Nếu không có tham chiếu
            if (goodsPack == null)
            {
                return true;
            }
            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(goodsPack.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (goodsPack.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(goodsPack.CurrentCopyMapID, goodsPack.CurrentMapCode))
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }
    }
}
