using Server.Data;
using System.Collections.Concurrent;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý giao dịch
    /// </summary>
    public static partial class KTTradeManager
    {
        /// <summary>
        /// Danh sách các phiên giao dịch trực tiếp đang thực hiện
        /// </summary>
        private static ConcurrentDictionary<int, ExchangeData> sessions = new ConcurrentDictionary<int, ExchangeData>();
    }
}
