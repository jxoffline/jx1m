using GameServer.Logic;
using System.Collections.Concurrent;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTPlayerTimerManager
    {
        /// <summary>
        /// Queue chứa danh sách người chơi
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> waitingQueue = new ConcurrentQueue<QueueItem>();

        /// <summary>
        /// Danh sách người chơi
        /// </summary>
        private readonly ConcurrentDictionary<int, KPlayer> players = new ConcurrentDictionary<int, KPlayer>();

        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(KPlayer player)
        {
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 1,
                Player = player,
            });
        }

        /// <summary>
        /// Xóa người chơi khỏi danh sách
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(KPlayer player)
        {
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 0,
                Player = player,
            });
        }
    }
}
