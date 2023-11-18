using System.Collections.Concurrent;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý xe tiêu
    /// </summary>
    public static partial class KTTraderCarriageManager
    {
        /// <summary>
        /// Danh sách xe tiêu
        /// </summary>
        private static readonly ConcurrentDictionary<int, TraderCarriage> carriages = new ConcurrentDictionary<int, TraderCarriage>();

        /// <summary>
        /// Tìm xe tiêu theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TraderCarriage FindTraderCarriage(int id)
        {
            if (KTTraderCarriageManager.carriages.TryGetValue(id, out TraderCarriage carriage))
            {
                return carriage;
            }
            return null;
        }
    }
}
