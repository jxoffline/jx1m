using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.GameEvents.SpecialEvent
{
    /// <summary>
    /// Quản lý sự kiện đặc biệt
    /// </summary>
    public partial class SpecialEventManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Luồng quản lý Võ lâm liên đấu
        /// </summary>
        public static SpecialEventManager Instance { get; private set; }

        /// <summary>
        /// Luồng quản lý Võ lâm liên đấu
        /// </summary>
        private SpecialEventManager()
        {
            this.StartTimer();
        }
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            SpecialEventManager.Instance = new SpecialEventManager();
        }
        #endregion
    }
}
