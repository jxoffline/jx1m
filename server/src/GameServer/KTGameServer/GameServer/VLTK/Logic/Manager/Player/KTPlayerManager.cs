using GameServer.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Danh sách người chơi theo ID
        /// </summary>
        private static ConcurrentDictionary<int, KPlayer> players = new ConcurrentDictionary<int, KPlayer>();
    }
}
