using GameServer.KiemThe.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Bot bán hàng
    /// </summary>
    public partial class KStallBot
    {
        /// <summary>
        /// Reset đối tượng
        /// </summary>
        /// <param name="removeFromManager">Xóa khỏi Manager luôn không (mặc định True, nếu gọi từ chính Manager thì False)</param>
        public void Destroy(bool removeFromManager = true)
        {
            /// Nếu xóa khỏi Manager
            if (removeFromManager)
            {
                KTStallBotManager.Remove(this);
            }
        }
    }
}
