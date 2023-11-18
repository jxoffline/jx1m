using System.Collections.Concurrent;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bot biểu diễn
    /// </summary>
    public static partial class KTDecoBotManager
    {
        /// <summary>
        /// Danh sách bot
        /// </summary>
        private static readonly ConcurrentDictionary<int, KDecoBot> bots = new ConcurrentDictionary<int, KDecoBot>();

        /// <summary>
        /// Tìm bot theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static KDecoBot Find(int id)
        {
            if (KTDecoBotManager.bots.TryGetValue(id, out KDecoBot bot))
            {
                return bot;
            }
            return null;
        }
    }
}
